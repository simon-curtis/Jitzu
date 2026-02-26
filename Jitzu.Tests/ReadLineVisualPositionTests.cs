using Jitzu.Shell.UI;
using Shouldly;

namespace Jitzu.Tests;

public class ReadLineVisualPositionTests
{
    [Test]
    public void EmptyString_ReturnsZeroZero()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("", 80);

        row.ShouldBe(0);
        col.ShouldBe(0);
    }

    [Test]
    public void SingleCharacter_ReturnsZeroOne()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("a", 80);

        row.ShouldBe(0);
        col.ShouldBe(1);
    }

    [Test]
    public void TextShorterThanWidth_StaysOnSameRow()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("hello", 80);

        row.ShouldBe(0);
        col.ShouldBe(5);
    }

    [Test]
    public void TextExactlyBufferWidth_PendingWrapNoRowAdvance()
    {
        // Delayed-wrap model: writing exactly bufferWidth chars leaves the cursor
        // in a pending wrap state — row has NOT advanced yet.
        var text = new string('x', 80);

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(0);
        col.ShouldBe(80);
    }

    [Test]
    public void TextExceedsBufferWidth_WrapsCorrectly()
    {
        var text = new string('x', 90);

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(1);
        col.ShouldBe(10);
    }

    [Test]
    public void TextDoubleBufferWidth_PendingWrapOnSecondRow()
    {
        // 160 chars = 2 full rows. First 80 chars pending, char 81 resolves wrap.
        // After 160 chars: pending again at end of row 1.
        var text = new string('x', 160);

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(1);
        col.ShouldBe(80);
    }

    [Test]
    public void SingleNewline_MovesToNextRow()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("abc\ndef", 80);

        row.ShouldBe(1);
        col.ShouldBe(3);
    }

    [Test]
    public void MultipleNewlines_CountsAllRows()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("a\nb\nc", 80);

        row.ShouldBe(2);
        col.ShouldBe(1);
    }

    [Test]
    public void NewlineAndWrapping_CombineCorrectly()
    {
        // Line 1: 90 chars (wraps once on 80-width terminal → 2 visual rows)
        // Then newline → row 3
        // Line 2: 5 chars
        var text = new string('x', 90) + "\nhello";

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(2);
        col.ShouldBe(5);
    }

    [Test]
    public void TrailingNewline_AddsRow()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("hello\n", 80);

        row.ShouldBe(1);
        col.ShouldBe(0);
    }

    [Test]
    public void ConsecutiveNewlines_AddsBlankRows()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("a\n\n\nb", 80);

        row.ShouldBe(3);
        col.ShouldBe(1);
    }

    [Test]
    public void ExactWidthFollowedByNewline_SingleRowAdvance()
    {
        // Delayed-wrap: 80 chars pending, then \n merges with pending wrap → 1 row advance total.
        var text = new string('x', 80) + "\ny";

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(1);
        col.ShouldBe(1);
    }

    [Test]
    public void ExactWidthFollowedByNonNewline_WrapsAndContinues()
    {
        // 80 chars pending, then 'y' resolves the wrap → row 1, col 1.
        var text = new string('x', 80) + "y";

        var (row, col) = ReadLine.CalculateVisualPosition(text, 80);

        row.ShouldBe(1);
        col.ShouldBe(1);
    }

    [Test]
    public void NarrowTerminal_WrapsFrequently()
    {
        // "abcdef" width 3: abc fills row 0 (pending), d resolves wrap → row 1,
        // def fills row 1 (pending). Result: row 1, col 3 (pending).
        var (row, col) = ReadLine.CalculateVisualPosition("abcdef", 3);

        row.ShouldBe(1);
        col.ShouldBe(3);
    }

    [Test]
    public void WidthOfOne_EachCharWraps()
    {
        // Width 1: a→col=1(pending), b resolves→row=1 col=1(pending), c resolves→row=2 col=1(pending)
        var (row, col) = ReadLine.CalculateVisualPosition("abc", 1);

        row.ShouldBe(2);
        col.ShouldBe(1);
    }

    [Test]
    public void ZeroBufferWidth_ReturnsZeroZero()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("hello", 0);

        row.ShouldBe(0);
        col.ShouldBe(0);
    }

    [Test]
    public void NegativeBufferWidth_ReturnsZeroZero()
    {
        var (row, col) = ReadLine.CalculateVisualPosition("hello", -1);

        row.ShouldBe(0);
        col.ShouldBe(0);
    }

    [Test]
    public void RealisticPrompt_WrappingFirstLine()
    {
        // Simulates: "user@hostname /very/long/path/to/some/deeply/nested/dir (feature-branch)*+  14:30"
        // on a 60-wide terminal where line 1 overflows, then "\n> " on line 2
        var line1 = new string('a', 85); // exceeds 60
        var prompt = line1 + "\n> ";

        var (row, col) = ReadLine.CalculateVisualPosition(prompt, 60);

        // line1: 85 chars / 60 width = 1 wrap (row 1, col 25 at char 85)
        // newline: row 2
        // "> ": col 2
        row.ShouldBe(2);
        col.ShouldBe(2);
    }

    [Test]
    public void RealisticPrompt_NonWrapping()
    {
        // Normal prompt that fits: "user@host dir (branch)  14:30\n> "
        var line1 = new string('a', 50);
        var prompt = line1 + "\n> ";

        var (row, col) = ReadLine.CalculateVisualPosition(prompt, 80);

        row.ShouldBe(1);
        col.ShouldBe(2);
    }

    [Test]
    public void MultiLinePromptWithWrapping_ThreeLines()
    {
        // Line 1: 100 chars (wraps once at 80) → 2 visual rows
        // Line 2: 10 chars → 1 visual row
        // Line 3: 3 chars → 1 visual row
        var prompt = new string('a', 100) + "\n" + new string('b', 10) + "\n" + ">> ";

        var (row, col) = ReadLine.CalculateVisualPosition(prompt, 80);

        // Row 0: chars 1-80 (pending wrap)
        // Row 1: chars 81-100 (wrap resolves for char 81) then \n → row 2
        // Row 2: 10 chars, then \n → row 3
        // Row 3: ">> " → col 3
        row.ShouldBe(3);
        col.ShouldBe(3);
    }

    [Test]
    public void JitzuPrompt_FullWidthLine1ThenNewline()
    {
        // Reproduces the exact Jitzu prompt pattern that caused the regression:
        // line1 is padded to exactly bufferWidth, followed by \n, then "> "
        var line1 = new string('a', 80);
        var prompt = line1 + "\n> ";

        var (row, col) = ReadLine.CalculateVisualPosition(prompt, 80);

        // Delayed-wrap: 80 chars pending, \n merges → 1 row advance, then "> " → col 2
        row.ShouldBe(1);
        col.ShouldBe(2);
    }

    [Test]
    public void JitzuPrompt_FullWidthWithOptionalLine2()
    {
        // line1 (80 chars) + \n + line2 (e.g., "took 3s") + \n + "> "
        var line1 = new string('a', 80);
        var prompt = line1 + "\ntook 3s\n> ";

        var (row, col) = ReadLine.CalculateVisualPosition(prompt, 80);

        // 80 chars pending, \n merges → row 1. "took 3s" → row 1 col 7. \n → row 2. "> " → col 2
        row.ShouldBe(2);
        col.ShouldBe(2);
    }

    [Test]
    public void JitzuPrompt_FullWidthPlusBuffer()
    {
        // Full prompt with user input in the buffer
        var line1 = new string('a', 80);
        var prompt = line1 + "\n> ";
        var fullContent = prompt + "hello world";

        var (row, col) = ReadLine.CalculateVisualPosition(fullContent, 80);

        // 80 chars pending, \n → row 1. "> hello world" → 13 chars → col 13
        row.ShouldBe(1);
        col.ShouldBe(13);
    }
}
