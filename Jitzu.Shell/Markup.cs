using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Shell;

public static partial class Markup
{
    public static string FromString(ReadOnlySpan<char> inputString)
    {
        var sb = new StringBuilder();

        while (inputString.IndexOf('[') is var openBracket and > -1)
        {
            if (inputString[openBracket..].IndexOf(']') is not (var closeBracket and > -1))
                break;

            closeBracket += openBracket;

            if (inputString[closeBracket..].IndexOf(@"[\]") is not (var closingTagIndex and > -1))
                throw new Exception("Unbalanced tags");

            closingTagIndex += closeBracket;

            sb.Append(inputString[..openBracket]);

            var codeSet = false;
            var codesSpan = inputString[(openBracket + 1)..closeBracket];
            foreach (var range in codesSpan.Split(';'))
            {
                var ansiCode = GetAnsiCode(codesSpan[range]);
                if (!ansiCode.IsEmpty)
                {
                    sb.Append(ansiCode);
                    codeSet = true;
                }
            }

            sb.Append(inputString[(closeBracket + 1)..closingTagIndex]);

            if (codeSet)
                sb.Append("\e[0m");

            inputString = inputString[(closingTagIndex + 3)..];
        }

        sb.Append(inputString);
        return sb.ToString();
    }

    private static ReadOnlySpan<char> GetAnsiCode(ReadOnlySpan<char> code)
    {
        return code switch
        {
            "fg:black" => "\e[0;30m",
            "fg:red" => "\e[38;5;167m",
            "fg:green" => "\e[38;5;108m",
            "fg:yellow" => "\e[38;5;179m",
            "fg:blue" => "\e[38;5;110m",
            "fg:purple" => "\e[38;5;139m",
            "fg:cyan" => "\e[38;5;109m",
            "fg:white" => "\e[0;37m",
            "fg:DarkSeaGreen4_1" => "\e[38;5;71m",
            "fg:LightBlue_1" => "\e[38;5;117m",
            "fg:magenta" => "\e[38;5;139m",
            "fg:DodgerBlue1" => "\e[38;5;74m",
            "fg:orange" => "\e[38;5;179m",
            "fg:grey" => "\e[38;5;244m",

            "bg:black" => "\e[40m",
            "bg:red" => "\e[41m",
            "bg:green" => "\e[42m",
            "bg:yellow" => "\e[43m",
            "bg:blue" => "\e[44m",
            "bg:purple" => "\e[45m",
            "bg:cyan" => "\e[46m",
            "bg:white" => "\e[47m",
            "bg:DarkSeaGreen4_1" => "\e[38;5;71m",
            "bg:LightBlue_1" => "\e[48;2;97;214;214m",

            "reset" => "\e[0m",
            "bold" => "\e[1m",
            "dim" => "\e[2m",
            "italic" => "\e[3m",
            "underline" => "\e[4m",

            _ => [],
        };
    }

    public static string Remove(ReadOnlySpan<char> input)
    {
        var sb = new StringBuilder();
        foreach (var range in AnsiCodeRegex.EnumerateSplits(input))
            sb.Append(input[range]);

        return sb.ToString();
    }

    [GeneratedRegex(@"\e\[[0-9;]*m")] private static partial Regex AnsiCodeRegex { get; }
}
