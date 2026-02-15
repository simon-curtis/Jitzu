using System.Reflection;
using Jitzu.Core.Logging;
using Jitzu.Core.Types;

namespace Jitzu.Core.Runtime;

public class ForeignFunction(MethodInfo methodInfo) : IShellFunction
{
    public MethodInfo MethodInfo { get; } = methodInfo;

    public ForeignFunction(Delegate @delegate) : this(@delegate.Method)
    {
    }

    public object? Invoke(Value[] args) => InvokeMethodInfo(MethodInfo, args);

    public static object? InvokeMethodInfo(MethodInfo methodInfo, Span<Value> args)
    {
        Span<ParameterInfo> parameters = methodInfo.GetParameters();
        var cursor = 0;

        try
        {
            object? instance = null;
            if (!methodInfo.IsStatic)
            {
                instance = args[0].AsObject();
                cursor++;
            }

            Span<object?> arguments = new object?[parameters.Length];

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if (parameter.HasDefaultValue)
                    arguments[index] = parameter.DefaultValue;
            }

            for (; cursor < args.Length; cursor++)
            {
                var parameter = parameters[cursor];

                if (parameter.IsOptional)
                    continue;

                if (cursor == parameters.Length - 1 && parameter.ParameterType.IsArray)
                {
                    var elementType = parameter.ParameterType.GetElementType()!;
                    var length = args.Length - cursor;

                    var array = Array.CreateInstance(elementType, length);
                    for (var j = 0; j < length; j++)
                        array.SetValue(args[cursor + j].AsObject(), j);

                    arguments[cursor] = array;
                    break;
                }

                arguments[cursor] = args[cursor].AsObject();
            }

            return methodInfo.Invoke(instance, arguments.ToArray());
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap inner exceptions from reflection calls
            return new Err<string>(
                $"Error running method: {methodInfo.Name}: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return new Err<string>($"Error running method: {methodInfo.Name}: {ex.Message}");
        }
    }

    public override string ToString() => ValueFormatter.FormatMethod(MethodInfo);
}