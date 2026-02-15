using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Jitzu.Core;
using Jitzu.Shell.Core.Completions;

namespace Jitzu.Shell.UI;

/// <summary>
/// Custom readline implementation with history navigation and basic line editing.
/// </summary>
public class ReadLine(HistoryManager history, ThemeConfig theme, CompletionHandler? completionHandler = null)
{
    private static readonly SearchValues<char> BoundaryValues = SearchValues.Create("\\/ ");

    private static readonly HashSet<string> JitzuKeywords =
    [
        "let", "mut", "if", "else", "while", "for", "fun", "return", "match",
        "type", "break", "continue", "new", "use", "mod", "trait", "impl",
        "pub", "open", "try", "defer", "clear", "union"
    ];

    private List<char> _buffer = [];
    private bool _cancelPressed;
    private string[]? _completions;
    private int _completionWordStart;
    private int _cursorPos;
    private int _historyIndex;
    private int? _selectionStart;
    private string _prompt = "";
    private int _promptRow;
    private int _bufferRow;
    private int _bufferColumn;

    // Ctrl+R reverse search state
    private bool _searchMode;
    private List<char> _searchBuffer = [];
    private int _searchMatchIndex;
    private List<char>? _savedBuffer;
    private int _savedCursorPos;

    // History predictions (data source)
    private const int MaxPredictions = 5;
    private List<string> _predictions = [];

    // Ghost text (inline dimmed suffix from top prediction)
    private string? _ghostText;

    // Unified dropdown (renders either predictions or completions)
    private List<string> _dropdownItems = [];
    private int _dropdownIndex = -1;
    private int _dropdownWindowStart;
    private int _dropdownLinesDrawn;
    private bool _dropdownIsCompletions;

    public string Read(string prompt)
    {
        _buffer = [];
        _cancelPressed = false;
        _completions = null;

        _completionWordStart = 0;
        _historyIndex = history.Count;
        _selectionStart = null;
        _searchMode = false;
        _searchBuffer = [];
        _savedBuffer = null;
        _prompt = prompt;
        _predictions = [];
        _ghostText = null;
        _dropdownItems = [];
        _dropdownIndex = -1;
        _dropdownWindowStart = 0;
        _dropdownLinesDrawn = 0;
        _dropdownIsCompletions = false;

        _cursorPos = 0;
        _promptRow = Console.CursorTop;
        _bufferRow = _promptRow + prompt.Count('\n');
        _bufferColumn = Markup.Remove(prompt.Split('\n').Last()).Length;

        RedrawLine();

        List<char>? tempBuffer = null;
        ClearCompletions();

        Console.CancelKeyPress += OnCancel;
        try
        {
            while (true)
            {
                if (_cancelPressed)
                {
                    _cancelPressed = false;

                    if (_selectionStart is not null)
                    {
                        var view = CollectionsMarshal.AsSpan(_buffer);
                        var start = Math.Min(_cursorPos, _selectionStart.Value);
                        var end = Math.Max(_cursorPos, _selectionStart.Value);
                        var selection = view[start..end];
                        CopyToClipboard(selection.ToString());
                        _selectionStart = null;
                        RedrawLine();
                        continue;
                    }

                    if (_buffer.Count > 0)
                    {
                        _cursorPos = _buffer.Count;
                        Console.SetCursorPosition(_bufferColumn + _cursorPos + 1, _bufferRow);
                        Console.WriteLine(Markup.FromString("[fg:red]^c[\\]"));
                    }
                    else
                    {
                        Console.WriteLine();
                    }

                    return "";
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(8); // ~120fps
                    continue;
                }

                var key = Console.ReadKey(intercept: true);

                // Ctrl+R: enter or cycle search mode
                if (key.Key == ConsoleKey.R && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    if (!_searchMode)
                        EnterSearchMode();
                    else
                        CycleSearchBackward();
                    continue;
                }

                // Route all other keys through search handler when in search mode
                if (_searchMode)
                {
                    if (HandleSearchKey(key))
                    {
                        Console.WriteLine();
                        return new string(_buffer.ToArray());
                    }
                    continue;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        if (_dropdownIndex >= 0 && _dropdownIndex < _dropdownItems.Count)
                        {
                            if (_dropdownIsCompletions)
                                ApplyCompletionAtIndex(_dropdownIndex);
                            else
                                AcceptPrediction();
                        }
                        DismissDropdown();
                        RedrawLine();
                        Console.WriteLine();
                        return new string(_buffer.ToArray());

                    case ConsoleKey.Backspace:
                        ClearCompletions();
                        if (DeleteSelection())
                        {
                            UpdatePredictions();
                            RedrawLine();
                        }
                        else if (_cursorPos > 0)
                        {
                            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                            {
                                var view = CollectionsMarshal.AsSpan(_buffer);
                                var endOfPreviousWord = JumpToLastBoundary(view, _cursorPos);
                                var output = $"{view[..endOfPreviousWord]}{view[_cursorPos..]}";
                                _buffer = output.ToCharArray().ToList();
                                _cursorPos = endOfPreviousWord;
                            }
                            else
                            {
                                _buffer.RemoveAt(_cursorPos - 1);
                                _cursorPos--;
                            }

                            UpdatePredictions();
                            RedrawLine();
                        }

                        break;

                    case ConsoleKey.Delete:
                        if (!_dropdownIsCompletions && _dropdownIndex >= 0 && _dropdownIndex < _dropdownItems.Count)
                        {
                            var entry = _dropdownItems[_dropdownIndex];
                            _ = history.RemoveAsync(entry);
                            _dropdownItems.RemoveAt(_dropdownIndex);
                            _predictions.Remove(entry);

                            if (_dropdownItems.Count == 0)
                                DismissDropdown();
                            else if (_dropdownIndex >= _dropdownItems.Count)
                                _dropdownIndex = _dropdownItems.Count - 1;

                            RedrawLine();
                            break;
                        }

                        ClearCompletions();
                        if (_cursorPos < _buffer.Count)
                        {
                            _buffer.RemoveAt(_cursorPos);
                            UpdatePredictions();
                            RedrawLine();
                        }

                        break;

                    case ConsoleKey.LeftArrow:
                        if (_cursorPos > 0)
                        {
                            _selectionStart = key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                                ? _selectionStart ?? _cursorPos
                                : null;

                            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                            {
                                var view = CollectionsMarshal.AsSpan(_buffer);
                                _cursorPos = JumpBack(view, _cursorPos);
                            }
                            else
                            {
                                _cursorPos--;
                            }

                            RedrawLine();
                        }

                        break;

                    case ConsoleKey.RightArrow:
                        if (_ghostText != null && _cursorPos == _buffer.Count)
                        {
                            _buffer.AddRange(_ghostText);
                            _cursorPos = _buffer.Count;
                            _ghostText = null;
                            UpdatePredictions();
                            RedrawLine();
                        }
                        else if (_cursorPos < _buffer.Count)
                        {
                            _selectionStart = key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                                ? _selectionStart ?? _cursorPos
                                : null;

                            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                            {
                                var view = CollectionsMarshal.AsSpan(_buffer);
                                _cursorPos = JumpForward(view, _cursorPos, _buffer.Count);
                            }
                            else
                            {
                                _cursorPos++;
                            }

                            RedrawLine();
                        }

                        break;

                    case ConsoleKey.UpArrow:
                    {
                        var navigateHistory = false;
                        if (_dropdownItems.Count > 0)
                        {
                            if (_dropdownIsCompletions)
                            {
                                if (_dropdownIndex > 0)
                                    _dropdownIndex--;
                                RedrawLine();
                                break;
                            }

                            ClearCompletions();

                            // History predictions dropdown
                            if (_dropdownIndex > 0)
                            {
                                _dropdownIndex--;
                                UpdateDropdownGhostText();
                            }
                            else if (_dropdownIndex == 0)
                            {
                                _dropdownIndex = -1;
                                _ghostText = null;
                            }
                            else
                            {
                                DismissDropdown();
                                navigateHistory = true;
                            }

                            if (!navigateHistory)
                            {
                                RedrawLine();
                                break;
                            }
                        }

                        if (history.Count > 0 && _historyIndex > 0)
                        {
                            tempBuffer ??= _buffer;

                            _historyIndex--;
                            _buffer.Clear();
                            _buffer.AddRange(history[_historyIndex]);
                            _cursorPos = _buffer.Count;
                            RedrawLine();
                        }

                        break;
                    }

                    case ConsoleKey.DownArrow:
                        if (_dropdownItems.Count > 0)
                        {
                            if (_dropdownIsCompletions)
                            {
                                if (_dropdownIndex < _dropdownItems.Count - 1)
                                    _dropdownIndex++;
                            }
                            else
                            {
                                ClearCompletions();
                                if (_dropdownIndex < _dropdownItems.Count - 1)
                                {
                                    _dropdownIndex++;
                                    UpdateDropdownGhostText();
                                }
                                else if (_dropdownIndex == _dropdownItems.Count - 1)
                                {
                                    _dropdownIndex = -1;
                                    _ghostText = null;
                                }
                            }

                            RedrawLine();
                        }
                        else if (history.Count > 0 && _historyIndex < history.Count)
                        {
                            _historyIndex++;

                            _buffer.Clear();
                            if (_historyIndex < history.Count)
                            {
                                _buffer.AddRange(history[_historyIndex]);
                            }
                            else if (tempBuffer != null)
                            {
                                _buffer = tempBuffer;
                                tempBuffer = null;
                            }

                            _cursorPos = _buffer.Count;
                            RedrawLine();
                        }

                        break;

                    case ConsoleKey.Escape:
                        if (_dropdownItems.Count > 0 || _ghostText != null)
                        {
                            _ghostText = null;
                            DismissDropdown();
                            RedrawLine();
                        }
                        break;

                    case ConsoleKey.Tab when completionHandler != null:
                        HandleTabCompletion(key);
                        break;

                    case ConsoleKey.Home:
                    case ConsoleKey.A when key.Modifiers.HasFlag(ConsoleModifiers.Control) && OperatingSystem.IsLinux():
                    {
                        _selectionStart = key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                            ? _selectionStart ?? _cursorPos
                            : null;
                        _cursorPos = 0;
                        RedrawLine();
                        break;
                    }

                    case ConsoleKey.End:
                    {
                        _selectionStart = key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                            ? _selectionStart ?? _cursorPos
                            : null;
                        _cursorPos = _buffer.Count;
                        RedrawLine();
                        break;
                    }

                    // The terminal paste-case
                    case var _ when Console.KeyAvailable:
                        List<char> incomingBuffer = [key.KeyChar];
                        while (Console.KeyAvailable)
                            incomingBuffer.Add(Console.ReadKey(true).KeyChar);

                        ClearCompletions();
                        DeleteSelection();
                        _buffer.InsertRange(_cursorPos, incomingBuffer);
                        _cursorPos += incomingBuffer.Count;
                        UpdatePredictions();
                        RedrawLine();
                        break;

                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            ClearCompletions();
                            DeleteSelection();
                            _buffer.Insert(_cursorPos, key.KeyChar);
                            _cursorPos++;
                            UpdatePredictions();
                            RedrawLine();
                        }

                        break;
                }
            }
        }
        finally
        {
            ClearDropdownLines();
            Console.CancelKeyPress -= OnCancel;
        }
    }

    private void OnCancel(object? _, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        _cancelPressed = true;
    }

    private static int JumpToLastBoundary(Span<char> view, int cursorPos)
    {
        // Sdad    asd adasd
        //           ^
        //         ^
        //        ^

        view = view[..cursorPos];

        var lastNonBoundary = view.LastIndexOfAnyExcept(BoundaryValues);
        if (lastNonBoundary is -1)
            return 0;

        view = view[..lastNonBoundary];
        return view.LastIndexOfAny(BoundaryValues) + 1;
    }

    private static int JumpBack(ReadOnlySpan<char> view, int cursorPos)
    {
        // Sdad    asd adasd
        //          ^
        //     ^
        // ^

        view = view[..cursorPos];
        var boundaryEnd = view.LastIndexOfAnyExcept(BoundaryValues);
        if (boundaryEnd is -1)
            return 0;

        view = view[..boundaryEnd];
        return view.LastIndexOfAny(BoundaryValues) + 1;
    }

    private static int JumpForward(ReadOnlySpan<char> view, int cursorPos, int maxIndex)
    {
        // Sdad asd    adasd
        //       ^
        //         ^
        //             ^

        var firstBoundary = view[cursorPos..].IndexOfAny(BoundaryValues);
        if (firstBoundary == -1)
            return maxIndex;

        cursorPos = firstBoundary + cursorPos;
        var firstNonBoundary = view[cursorPos..].IndexOfAnyExcept(BoundaryValues);
        if (firstNonBoundary is -1)
            return maxIndex;

        return firstNonBoundary + cursorPos;
    }

    private void ClearCompletions()
    {
        _completions = null;

        if (_dropdownIsCompletions)
        {
            ClearDropdownLines();
            _dropdownItems = [];
            _dropdownIndex = -1;
            _dropdownWindowStart = 0;
            _dropdownLinesDrawn = 0;
            _dropdownIsCompletions = false;
        }
    }

    private void HandleTabCompletion(ConsoleKeyInfo key)
    {
        // Tab 2: Accept current completion and close dropdown
        if (_dropdownIsCompletions && _dropdownItems.Count > 0)
        {
            ApplyCompletionAtIndex(_dropdownIndex);
            ClearCompletions();
            DismissDropdown();
            RedrawLine();
            return;
        }

        // Tab 1: Open completion suggestions dropdown
        // Dismiss any history predictions/ghost text
        DismissDropdown();

        if (_completions == null || _completions.Length == 0)
        {
            var currentLine = CollectionsMarshal.AsSpan(_buffer);
            _completions = completionHandler!(currentLine[.._cursorPos].ToString());

            _completionWordStart = FindCompletionWordStart();
        }

        if (_completions.Length > 0)
        {
            if (_completions.Length == 1)
            {
                // Single completion — apply immediately, no dropdown
                InsertCompletion(_completions[0]);
                ClearCompletions();
            }
            else
            {
                // Multiple completions — show dropdown, highlight first item
                _dropdownItems = [.._completions];
                _dropdownIndex = 0;
                _dropdownIsCompletions = true;
            }

            RedrawLine();
        }
    }

    private void RedrawLine()
    {
        if (_searchMode)
        {
            RedrawSearchLine();
            return;
        }

        var sb = new ArrayBufferWriter<char>(_prompt.Length + _buffer.Count);
        // write position if in debug
#if DEBUG
        sb.Write($"BH: {Console.BufferHeight} CT: {Console.CursorTop} PR: {_promptRow} ");
#endif
        sb.Write(_prompt);

        var bufferView = CollectionsMarshal.AsSpan(_buffer);
        if (_selectionStart != null)
        {
            var (start, end) = GetRangeFromCursor(_cursorPos, _selectionStart.Value);
            sb.Write(HighlightBuffer(bufferView, start, end));
        }
        else
        {
            sb.Write(HighlightBuffer(bufferView));
        }

        // for (var i = lineDeficit; i > 0; i--)
        // {
        //     _promptRow--;
        //     Console.WriteLine();
        // }

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, _promptRow);
        Console.Write(sb.WrittenSpan);

        var newlineCount = sb.WrittenSpan.Count('\n');

        if (_promptRow == Console.CursorTop)
        {
            _promptRow -= newlineCount;
            _bufferRow -= newlineCount;
        }

        // Render ghost text (inline dimmed suffix from top prediction or selected dropdown item)
        var ghostLen = 0;
        if (_ghostText != null && _selectionStart == null && !_dropdownIsCompletions)
        {
            Console.Write($"{theme["prediction.text"]}{_ghostText}{ThemeConfig.Reset}");
            ghostLen = _ghostText.Length;
        }

        if (Console.BufferWidth - Console.CursorLeft is var remainder and > 0)
        {
            Span<char> blankSpace = stackalloc char[remainder];
            blankSpace.Fill(' ');
            Console.Write(blankSpace.ToString());
        }

        DrawDropdown();

        var targetRow = _bufferRow;
        if (targetRow >= Console.BufferHeight)
            targetRow = Console.BufferHeight - 1;

        Console.SetCursorPosition(_bufferColumn + _cursorPos, targetRow);
        Console.CursorVisible = true;
    }

    private void RedrawSearchLine()
    {
        var query = new string(_searchBuffer.ToArray());
        var matched = new string(_buffer.ToArray());
        var searchPrompt = Markup.FromString($"[dim](reverse-i-search)[\\]'{query}': {matched}");

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, _bufferRow);
        Console.Write(searchPrompt);

        if (Console.BufferWidth - Console.CursorLeft is var remainder and > 0)
        {
            Span<char> blankSpace = stackalloc char[remainder];
            blankSpace.Fill(' ');
            Console.Write(blankSpace.ToString());
        }

        // Position cursor after the rendered text
        var cursorCol = $"(reverse-i-search)'{query}': {matched}".Length;
        if (cursorCol >= Console.BufferWidth)
            cursorCol = Console.BufferWidth - 1;

        Console.SetCursorPosition(cursorCol, _bufferRow);
        Console.CursorVisible = true;
    }

    private static (int Start, int End) GetRangeFromCursor(int first, int second) => first > second
        ? (second, first)
        : (first, second);

    private static void CopyToClipboard(string text)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        var osc52 = $"\e]52;c;{base64}\a";
        Console.Out.Write(osc52);
        Console.Out.Flush();
    }

    private void EnterSearchMode()
    {
        _searchMode = true;
        _searchBuffer = [];
        _searchMatchIndex = history.Count - 1;
        _savedBuffer = [.._buffer];
        _savedCursorPos = _cursorPos;
        RedrawLine();
    }

    private void ExitSearchMode(bool accept)
    {
        _searchMode = false;
        if (!accept && _savedBuffer != null)
        {
            _buffer = _savedBuffer;
            _cursorPos = _savedCursorPos;
        }
        _savedBuffer = null;
        RedrawLine();
    }

    private void CycleSearchBackward()
    {
        if (_searchMatchIndex <= 0)
            return;

        var query = new string(_searchBuffer.ToArray());
        var match = history.SearchBackward(query, _searchMatchIndex - 1);
        if (match >= 0)
        {
            _searchMatchIndex = match;
            _buffer = history.GetEntry(match).ToList();
            _cursorPos = _buffer.Count;
        }
        RedrawLine();
    }

    private void PerformSearch()
    {
        var query = new string(_searchBuffer.ToArray());
        if (string.IsNullOrEmpty(query))
        {
            if (_savedBuffer != null)
            {
                _buffer = [.._savedBuffer];
                _cursorPos = _savedCursorPos;
            }
            RedrawLine();
            return;
        }

        var match = history.SearchBackward(query, _searchMatchIndex);
        if (match >= 0)
        {
            _searchMatchIndex = match;
            _buffer = history.GetEntry(match).ToList();
            _cursorPos = _buffer.Count;
        }
        RedrawLine();
    }

    /// <summary>
    /// Handles a key press while in search mode. Returns true if the line should be submitted.
    /// </summary>
    private bool HandleSearchKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                // Accept match and execute
                _searchMode = false;
                _savedBuffer = null;
                return true;

            case ConsoleKey.Escape:
                // Cancel search, restore original buffer
                ExitSearchMode(accept: false);
                return false;

            case ConsoleKey.Backspace:
                if (_searchBuffer.Count > 0)
                {
                    _searchBuffer.RemoveAt(_searchBuffer.Count - 1);
                    // Reset search from end so we find closest match
                    _searchMatchIndex = history.Count - 1;
                    PerformSearch();
                }
                return false;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    _searchBuffer.Add(key.KeyChar);
                    PerformSearch();
                }
                else
                {
                    // Any other control key: accept match, exit search, let user edit
                    ExitSearchMode(accept: true);
                }
                return false;
        }
    }

    private void UpdatePredictions()
    {
        // Don't update history predictions while showing completion dropdown
        if (_dropdownIsCompletions)
            return;

        var text = new string(_buffer.ToArray());
        _predictions = history.GetPredictions(text, MaxPredictions);
        _dropdownIndex = -1;

        // Set ghost text from top prediction
        if (_predictions.Count > 0 && text.Length > 0)
        {
            var topPrediction = _predictions[0];
            _ghostText = topPrediction.Length > text.Length ? topPrediction[text.Length..] : null;
        }
        else
        {
            _ghostText = null;
        }

        // Populate dropdown with predictions
        _dropdownItems = _predictions;
        _dropdownIsCompletions = false;
    }

    /// <summary>
    /// Updates ghost text to show the suffix of the currently selected dropdown item.
    /// </summary>
    private void UpdateDropdownGhostText()
    {
        if (_dropdownIndex < 0 || _dropdownIndex >= _dropdownItems.Count)
        {
            _ghostText = null;
            return;
        }

        if (_dropdownIsCompletions)
        {
            _ghostText = null;
            return;
        }

        var currentText = new string(_buffer.ToArray());
        var selectedItem = _dropdownItems[_dropdownIndex];

        // Extract suffix (what comes after current input)
        if (selectedItem.Length > currentText.Length)
            _ghostText = selectedItem[currentText.Length..];
        else
            _ghostText = null;
    }

    private void DismissDropdown()
    {
        ClearDropdownLines();
        _predictions = [];
        _ghostText = null;
        _dropdownItems = [];
        _dropdownIndex = -1;
        _dropdownWindowStart = 0;
        _dropdownLinesDrawn = 0;
        _dropdownIsCompletions = false;
    }

    /// <summary>
    /// Deletes the currently selected text and positions the cursor at the selection start.
    /// Returns true if there was a selection to delete.
    /// </summary>
    private bool DeleteSelection()
    {
        if (_selectionStart is null) return false;

        var start = Math.Min(_cursorPos, _selectionStart.Value);
        var end = Math.Max(_cursorPos, _selectionStart.Value);
        _buffer.RemoveRange(start, end - start);
        _cursorPos = start;
        _selectionStart = null;
        return true;
    }

    private void AcceptPrediction()
    {
        if (_dropdownIndex < 0 || _dropdownIndex >= _predictions.Count)
            return;

        var selected = _predictions[_dropdownIndex];
        _buffer = selected.ToList();
        _cursorPos = _buffer.Count;
        DismissDropdown();
    }


    private void ApplyCompletionAtIndex(int index)
    {
        if (_completions == null || index < 0 || index >= _completions.Length)
            return;

        InsertCompletion(_completions[index]);
    }

    /// <summary>
    /// Inserts a completion value at _completionWordStart, handling quoting for paths with spaces.
    /// Replaces the range [_completionWordStart.._cursorPos], including any surrounding quotes.
    /// </summary>
    private void InsertCompletion(string completion)
    {
        var replaceStart = _completionWordStart;
        var replaceEnd = _cursorPos;

        // If existing text starts with a quote, include it in the replacement range
        if (replaceStart < _buffer.Count && _buffer[replaceStart] == '"')
        {
            // Also include closing quote if cursor is right before/after one
            if (replaceEnd < _buffer.Count && _buffer[replaceEnd] == '"')
                replaceEnd++;
            else if (replaceEnd > 0 && _buffer[replaceEnd - 1] == '"')
            {
                // Cursor is right after closing quote — already included
            }
        }

        // Add quotes if the path contains spaces
        var needsQuotes = completion.Contains(' ');
        var replacement = needsQuotes ? $"\"{completion}\"" : completion;

        _buffer.RemoveRange(replaceStart, replaceEnd - replaceStart);
        _buffer.InsertRange(replaceStart, replacement);
        _cursorPos = replaceStart + replacement.Length - (needsQuotes ? 1 : 0);
    }

    /// <summary>
    /// Finds the start of the current word for completion, including any opening quote.
    /// </summary>
    private int FindCompletionWordStart()
    {
        var pos = _cursorPos;

        // If cursor is right after a closing quote, find the matching opening quote
        if (pos > 0 && _buffer[pos - 1] == '"')
        {
            pos--; // skip closing quote
            while (pos > 0 && _buffer[pos - 1] != '"')
                pos--;
            if (pos > 0) pos--; // include the opening quote
            return pos;
        }

        // Check if we're inside a quoted string by counting quotes before cursor
        var quoteCount = 0;
        for (var i = 0; i < pos; i++)
        {
            if (_buffer[i] == '"')
                quoteCount++;
        }

        if (quoteCount % 2 == 1)
        {
            // Inside a quoted string — scan back to the opening quote
            while (pos > 0 && _buffer[pos - 1] != '"')
                pos--;
            if (pos > 0) pos--; // include the opening quote
            return pos;
        }

        // Scan back through non-whitespace, stopping at whitespace or finding an opening quote
        while (pos > 0)
        {
            var ch = _buffer[pos - 1];
            if (ch == '"')
            {
                pos--; // include the opening quote
                break;
            }
            if (char.IsWhiteSpace(ch))
                break;
            pos--;
        }

        return pos;
    }

    private const int MaxDropdownVisible = 10;

    private void DrawDropdown()
    {
        ClearDropdownLines();

        var total = _dropdownItems.Count;
        if (total == 0)
        {
            _dropdownLinesDrawn = 0;
            return;
        }

        // Ensure enough room below the input line for the dropdown.
        // If the input line is near the bottom of the buffer, emit newlines
        // to scroll the viewport up, then adjust our tracked row positions.
        var neededRows = Math.Min(total, MaxDropdownVisible) + 1; // +1 for status line
        var availableRows = Console.BufferHeight - _bufferRow - 1;
        if (availableRows < neededRows)
        {
            var scroll = neededRows - availableRows;
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            for (var s = 0; s < scroll; s++)
                Console.WriteLine();
            _bufferRow -= scroll;
            _promptRow -= scroll;
            if (_promptRow < 0) _promptRow = 0;
            if (_bufferRow < 0) _bufferRow = 0;
            availableRows = Console.BufferHeight - _bufferRow - 1;
        }

        // Reserve 1 row for the status line
        var maxVisible = Math.Min(MaxDropdownVisible, availableRows - 1);
        if (maxVisible <= 0)
        {
            _dropdownLinesDrawn = 0;
            return;
        }

        var visibleCount = Math.Min(total, maxVisible);

        // Only scroll the window when the selection goes out of view
        if (_dropdownIndex >= 0)
        {
            if (_dropdownIndex < _dropdownWindowStart)
                _dropdownWindowStart = _dropdownIndex;
            else if (_dropdownIndex >= _dropdownWindowStart + visibleCount)
                _dropdownWindowStart = _dropdownIndex - visibleCount + 1;
        }
        if (_dropdownWindowStart + visibleCount > total)
            _dropdownWindowStart = total - visibleCount;
        if (_dropdownWindowStart < 0)
            _dropdownWindowStart = 0;

        var width = Console.BufferWidth;
        var dimColor = theme["prediction.text"];
        var selBg = theme["prediction.selected.bg"];
        var selFg = theme["prediction.selected.fg"];
        var gutterColor = theme["dropdown.gutter"];
        var statusColor = theme["dropdown.status"];

        // Choose gutter character based on mode: history predictions use "░ ", tab completions use "⏵ "
        var gutterChar = _dropdownIsCompletions ? "⏵ " : "░ ";

        for (var i = 0; i < visibleCount; i++)
        {
            var itemIndex = _dropdownWindowStart + i;
            var row = _bufferRow + 1 + i;
            Console.SetCursorPosition(0, row);

            var isSelected = itemIndex == _dropdownIndex;
            var text = _dropdownItems[itemIndex];

            var maxTextWidth = width - 2; // "X " gutter (2 chars)
            if (text.Length > maxTextWidth)
                text = text[..maxTextWidth];

            var padding = maxTextWidth - text.Length;
            var pad = padding > 0 ? new string(' ', padding) : "";

            if (isSelected)
                Console.Write($"{selBg}{selFg}{gutterChar}{text}{pad}{ThemeConfig.Reset}");
            else
                Console.Write($"{dimColor}{gutterChar}{text}{pad}{ThemeConfig.Reset}");
        }

        // Status line
        var statusRow = _bufferRow + 1 + visibleCount;
        if (statusRow < Console.BufferHeight)
        {
            Console.SetCursorPosition(0, statusRow);
            var pos = _dropdownIndex >= 0 ? _dropdownIndex + 1 : 0;
            var modeLabel = _dropdownIsCompletions ? " paths" : " history";
            var status = $"({pos}/{total}){modeLabel}";
            Console.Write($"  {statusColor}{status}{ThemeConfig.Reset}");
            var statusLen = 2 + status.Length;
            var statusPad = width - statusLen;
            if (statusPad > 0)
                Console.Write(new string(' ', statusPad));

            _dropdownLinesDrawn = visibleCount + 1;
        }
        else
        {
            _dropdownLinesDrawn = visibleCount;
        }
    }

    private void ClearDropdownLines()
    {
        if (_dropdownLinesDrawn <= 0)
            return;

        var width = Console.BufferWidth;
        var blankStr = new string(' ', width);

        for (var i = 0; i < _dropdownLinesDrawn; i++)
        {
            var row = _bufferRow + 1 + i;
            if (row >= Console.BufferHeight)
                break;
            Console.SetCursorPosition(0, row);
            Console.Write(blankStr);
        }

        // Reposition cursor back to the input line so output doesn't leave blank gaps
        var restoreRow = _bufferRow < Console.BufferHeight ? _bufferRow : Console.BufferHeight - 1;
        Console.SetCursorPosition(_bufferColumn + _buffer.Count, restoreRow);

        _dropdownLinesDrawn = 0;
    }

    private string HighlightBuffer(ReadOnlySpan<char> buffer, int selStart = -1, int selEnd = -1)
    {
        if (buffer.IsEmpty) return "";

        var sb = new StringBuilder(buffer.Length + 64);
        var pos = 0;
        var commandPos = true;
        var hasSelection = selStart >= 0 && selEnd >= 0;
        var selBg = theme["selection.bg"];
        var selFg = theme["selection.fg"];

        while (pos < buffer.Length)
        {
            var ch = buffer[pos];

            // Whitespace
            if (ch is ' ' or '\t')
            {
                AppendWithSelection(sb, ch, pos, hasSelection, selStart, selEnd, selBg, selFg, null);
                pos++;
                continue;
            }

            // String literals
            if (ch is '"' or '\'' or '`')
            {
                var start = pos;
                var quote = ch;
                pos++;
                while (pos < buffer.Length && buffer[pos] != quote)
                {
                    if (buffer[pos] == '\\' && pos + 1 < buffer.Length) pos++;
                    pos++;
                }
                if (pos < buffer.Length) pos++;
                AppendSpanWithSelection(sb, buffer[start..pos], start, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.string"]);
                commandPos = false;
                continue;
            }

            // Chain operators: && and ||
            if (ch == '&' && pos + 1 < buffer.Length && buffer[pos + 1] == '&')
            {
                AppendSpanWithSelection(sb, "&&", pos, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                pos += 2;
                commandPos = true;
                continue;
            }

            if (ch == '|' && pos + 1 < buffer.Length && buffer[pos + 1] == '|')
            {
                AppendSpanWithSelection(sb, "||", pos, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                pos += 2;
                commandPos = true;
                continue;
            }

            // Pipe
            if (ch == '|')
            {
                AppendSpanWithSelection(sb, "|", pos, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                pos++;
                commandPos = true;
                continue;
            }

            // Semicolon — command separator
            if (ch == ';')
            {
                AppendWithSelection(sb, ch, pos, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                pos++;
                commandPos = true;
                continue;
            }

            // Command substitution $( ... )
            if (ch == '$' && pos + 1 < buffer.Length && buffer[pos + 1] == '(')
            {
                AppendSpanWithSelection(sb, "$(", pos, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                pos += 2;
                commandPos = true;
                continue;
            }

            // Redirection operators
            if (ch is '>' or '<')
            {
                var opStart = pos;
                pos++;
                if (ch == '>' && pos < buffer.Length && buffer[pos] == '>') pos++;
                AppendSpanWithSelection(sb, buffer[opStart..pos], opStart, hasSelection, selStart, selEnd, selBg, selFg, theme["syntax.pipe"]);
                continue;
            }

            // Structural single characters
            if (ch is '(' or ')' or '{' or '}' or '[' or ']')
            {
                AppendWithSelection(sb, ch, pos, hasSelection, selStart, selEnd, selBg, selFg, null);
                pos++;
                continue;
            }

            // Unhandled operator chars (lone & etc.) — just emit and advance
            if (ch is '&' or '$')
            {
                AppendWithSelection(sb, ch, pos, hasSelection, selStart, selEnd, selBg, selFg, null);
                pos++;
                continue;
            }

            // Word: consume until delimiter
            var wordStart = pos;
            while (pos < buffer.Length
                   && buffer[pos] is not (' ' or '\t' or '"' or '\'' or '`'
                       or '|' or '(' or ')' or '{' or '}' or '[' or ']' or ';'
                       or '&' or '>' or '<')
                   && !(buffer[pos] == '$' && pos + 1 < buffer.Length && buffer[pos + 1] == '('))
                pos++;

            var word = buffer[wordStart..pos];
            var wordStr = word.ToString();

            string? color = null;
            if (JitzuKeywords.Contains(wordStr))
                color = theme["syntax.keyword"];
            else if (wordStr is "true" or "false")
                color = theme["syntax.boolean"];
            else if (commandPos)
                color = theme["syntax.command"];
            else if (word.Length > 1 && word[0] == '-')
                color = theme["syntax.flag"];

            AppendSpanWithSelection(sb, word, wordStart, hasSelection, selStart, selEnd, selBg, selFg, color);
            commandPos = false;
        }

        return sb.ToString();
    }

    private static void AppendWithSelection(StringBuilder sb, char ch, int pos,
        bool hasSelection, int selStart, int selEnd, string selBg, string selFg, string? fgColor)
    {
        var inSel = hasSelection && pos >= selStart && pos < selEnd;
        if (inSel || fgColor != null)
        {
            if (inSel) sb.Append(selBg);
            sb.Append(fgColor ?? (inSel ? selFg : ""));
            sb.Append(ch);
            sb.Append(ThemeConfig.Reset);
        }
        else
        {
            sb.Append(ch);
        }
    }

    private static void AppendSpanWithSelection(StringBuilder sb, ReadOnlySpan<char> text, int startPos,
        bool hasSelection, int selStart, int selEnd, string selBg, string selFg, string? fgColor)
    {
        if (!hasSelection || startPos >= selEnd || startPos + text.Length <= selStart)
        {
            // Entirely outside selection
            if (fgColor != null)
            {
                sb.Append(fgColor);
                sb.Append(text);
                sb.Append(ThemeConfig.Reset);
            }
            else
            {
                sb.Append(text);
            }
            return;
        }

        // May overlap selection — render char by char
        for (var i = 0; i < text.Length; i++)
        {
            var charPos = startPos + i;
            var inSel = charPos >= selStart && charPos < selEnd;

            if (inSel) sb.Append(selBg);
            sb.Append(fgColor ?? (inSel ? selFg : ""));
            sb.Append(text[i]);
            if (inSel || fgColor != null) sb.Append(ThemeConfig.Reset);
        }
    }
}