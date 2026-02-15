using System.Runtime.CompilerServices;
using System.Text;

namespace Jitzu.Core.Formatting;

public class TreeWriter
{
    private string _indent = "";
    private readonly StringBuilder _sb = new();
    private void Indent() => _indent += "  ";
    private void Outdent() => _indent = _indent[2..];

    public void WriteKeyValue(string name, string value, string? typeName = null)
    {
        _sb.Append(_indent);
        if (typeName is not null)
            _sb.Append($"\e[34m{name}\e[0m \e[90m{typeName}\e[0m: ");
        else
            _sb.Append($"\e[34m{name}\e[0m: ");
        _sb.AppendLine(value);
    }

    public void WriteKeyValue(string name, int value, string? typeName = null)
    {
        _sb.Append(_indent);
        if (typeName is not null)
            _sb.Append($"\e[34m{name}\e[0m \e[90m{typeName}\e[0m: ");
        else
            _sb.Append($"\e[34m{name}\e[0m: ");
        _sb.AppendLine(value.ToString());
    }

    public void WriteNotImplemented(string name, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int? calledLinerNumber = null)
    {
        _sb.Append(_indent);
        _sb.Append($"\e[34m{name}\e[0m: ");
        _sb.AppendLine($"\e[90m[Not Implemented]\e[0m in {callerFileName}:{calledLinerNumber}");
    }

    public void StartObject(string name, string? typeName = null)
    {
        _sb.Append(_indent);
        if (typeName is not null)
            _sb.Append($"\e[34m{name}\e[0m \e[90m{typeName}\e[0m: ");
        else
            _sb.Append($"\e[34m{name}\e[0m: ");
        _sb.AppendLine();
        Indent();
    }

    public void EndObject()
    {
        Outdent();
    }

    public override string ToString() => _sb.ToString();
}
