using System.Runtime.InteropServices;
using Jitzu.Core.Language;
using Jitzu.Core.Runtime;

namespace Jitzu.Core.Logging;

public static class ByteCodeWriter
{
    public static void WriteToFile(string outputPath, UserFunction function)
    {
        using var writer = File.CreateText(outputPath);
        AppendChunk(writer, function);
    }

    private static void AppendChunk(TextWriter writer, UserFunction function)
    {
        writer.WriteLine($";;;;;; {function}");
        WriteByteCode(function.Chunk, writer);
        foreach (var nestedFunc in function.Chunk.Constants.OfType<UserFunction>())
        {
            // Stop recursion
            if (nestedFunc == function)
                continue;

            writer.WriteLine();
            AppendChunk(writer, nestedFunc);
        }
    }

    private static void WriteByteCode(Chunk chunk, TextWriter writer)
    {
        var baseRegister = -1;

        ReadOnlySpan<byte> code = CollectionsMarshal.AsSpan(chunk.Code);
        var lastSource = chunk.DebugSpans.Values.First();

        var ip = 0;
        while (ip < code.Length)
        {
            if (chunk.DebugSpans.TryGetValue(ip, out var span) && !lastSource.IsOnSameLine(span))
            {
                lastSource = span;
                writer.WriteLine($";;;;;; {lastSource}");
            }

            var opIp = ip;
            var op = (OpCode)code[ip++];
            var opName = op.ToStringFast();

            writer.Write($"{opIp,-6:0000} {opName,-13} ");

            switch (op)
            {
                case OpCode.None:
                    writer.WriteLine("Noop");
                    break;

                case OpCode.Dup:
                {
                    var source = baseRegister;
                    var register = ++baseRegister;
                    writer.WriteLine($"R{register} ← R{source}");
                    break;
                }

                case OpCode.LoadConst:
                {
                    var constIndex = ReadInt(code, ref ip);
                    var register = ++baseRegister;
                    writer.WriteLine($"R{register} ← constants[{constIndex}] ({chunk.Constants[constIndex]})");
                    break;
                }

                case OpCode.SetLocal:
                {
                    var constIndex = ReadInt(code, ref ip);
                    // R3 → locals[5] (store 0xABCD to global slot 5)
                    var register = baseRegister--;
                    writer.WriteLine($"locals[{constIndex}] ← R{register}");
                    break;
                }

                case OpCode.GetLocal:
                {
                    var constIndex = ReadInt(code, ref ip);
                    var register = ++baseRegister;
                    writer.WriteLine($"R{register} ← locals[{constIndex}]");
                    break;
                }

                case OpCode.SetGlobal:
                {
                    var constIndex = ReadInt(code, ref ip);
                    // R3 → globals[5] (store 0xABCD to global slot 5)
                    var register = baseRegister--;
                    writer.WriteLine($"globals[{constIndex}] ← R{register}");
                    break;
                }

                case OpCode.GetGlobal:
                {
                    var constIndex = ReadInt(code, ref ip);
                    var register = ++baseRegister;
                    writer.WriteLine($"R{register} ← globals[{constIndex}]");
                    break;
                }

                case OpCode.Construct:
                {
                    var constIndex = ReadInt(code, ref ip);
                    var targetType = chunk.Constants[constIndex];
                    writer.WriteLine($"R[{++baseRegister}] ← constant[{constIndex}] (new {targetType})");
                    break;
                }

                case OpCode.GetField:
                {
                    var constIndex = ReadInt(code, ref ip);
                    var constant = chunk.Constants[constIndex];
                    writer.WriteLine($"R{baseRegister} ← R{baseRegister}[{constant}]");
                    break;
                }

                case OpCode.SetField:
                {
                    var name = chunk.Constants[ReadInt(code, ref ip)];
                    var value = baseRegister--;
                    var target = baseRegister;
                    writer.WriteLine($"R{baseRegister} ← R{target}[{name}] ← R{value}");
                    break;
                }

                case OpCode.Call:
                {
                    // → 0x0080, cleanup=3 (call add_three_numbers)
                    var callRegister = baseRegister;
                    var argCount = ReadInt(code, ref ip);
                    baseRegister -= argCount;
                    writer.WriteLine($"R{baseRegister} ← R{callRegister}(argc: {argCount})");
                    break;
                }

                case OpCode.Return:
                {
                    var returnRegister = baseRegister--;
                    writer.WriteLine($"return R{returnRegister}");
                    break;
                }

                case OpCode.Pop:
                {
                    var register = baseRegister--;
                    writer.WriteLine($"discard R{register}");
                    break;
                }

                case OpCode.Jump:
                {
                    var jumpIp = ReadInt(code, ref ip);
                    writer.WriteLine($"→ {jumpIp:0000}");
                    break;
                }

                case OpCode.JumpIfFalse:
                {
                    var jumpIp = ReadInt(code, ref ip);
                    var register = baseRegister--;
                    writer.WriteLine($"R{register} == false → {jumpIp:0000}");
                    break;
                }

                case OpCode.Loop:
                {
                    var offset = ReadInt(code, ref ip);
                    writer.WriteLine($"jump {offset:0000}");
                    break;
                }

                case OpCode.Inc:
                    PrintLine(writer, lastSource, opIp, OpCode.Inc);
                    break;

                case OpCode.Dec:
                    PrintLine(writer, lastSource, opIp, OpCode.Dec);
                    break;

                case OpCode.Mul:
                    PrintLine(writer, lastSource, opIp, OpCode.Mul);
                    break;

                case OpCode.Div:
                    PrintLine(writer, lastSource, opIp, OpCode.Div);
                    break;

                case OpCode.Mod:
                    PrintLine(writer, lastSource, opIp, OpCode.Mod);
                    break;

                case OpCode.Lt:
                    PrintLine(writer, lastSource, opIp, OpCode.Lt);
                    break;

                case OpCode.Lte:
                    PrintLine(writer, lastSource, opIp, OpCode.Lte);
                    break;

                case OpCode.Gt:
                    PrintLine(writer, lastSource, opIp, OpCode.Gt);
                    break;

                case OpCode.Gte:
                    PrintLine(writer, lastSource, opIp, OpCode.Gte);
                    break;

                case OpCode.Add:
                {
                    var sourceTwo = baseRegister--;
                    var sourceOne = baseRegister;
                    writer.WriteLine($"R{sourceOne} ← R{sourceOne} + R{sourceTwo}");
                    break;
                }

                case OpCode.Sub:
                {
                    var sourceTwo = baseRegister--;
                    var sourceOne = baseRegister;
                    writer.WriteLine($"R{sourceOne} ← R{sourceOne} - R{sourceTwo}");
                    break;
                }

                case OpCode.Compare:
                {
                    var sourceTwo = baseRegister--;
                    var sourceOne = baseRegister;
                    writer.WriteLine($"R{sourceOne} ← R{sourceOne} ? R{sourceTwo}");
                    break;
                }

                case OpCode.Eq:
                {
                    var sourceTwo = baseRegister--;
                    var sourceOne = baseRegister;

                    writer.WriteLine($"R{sourceOne} ← R{sourceOne} == R{sourceTwo}");
                    break;
                }

                case OpCode.IndexGet:
                {
                    // R0 ← R0[R1], 0 (load element 2 from 32-bit array)
                    var indexRegister = baseRegister--;
                    var subjectRegister = baseRegister;
                    var targetRegister = subjectRegister;
                    writer.WriteLine($"R{targetRegister} ← R{subjectRegister}[R{indexRegister}]");
                    break;
                }

                case OpCode.IndexSet:
                    PrintLine(writer, lastSource, opIp, OpCode.IndexSet);
                    break;

                case OpCode.NewArray:
                    PrintLine(writer, lastSource, opIp, OpCode.NewArray);
                    break;

                case OpCode.NewString:
                {
                    var length = ReadInt(code, ref ip);
                    PrintLine(writer, lastSource, opIp, OpCode.NewString, length);
                    break;
                }

                case OpCode.NewInt:
                {
                    var value = ReadInt(code, ref ip);
                    PrintLine(writer, lastSource, opIp, OpCode.NewInt, value);
                    break;
                }

                case OpCode.NewDouble:
                {
                    var value = ReadInt(code, ref ip);
                    PrintLine(writer, lastSource, opIp, OpCode.NewDouble, value);
                    break;
                }

                case OpCode.Swap:
                    PrintLine(writer, lastSource, opIp, OpCode.Swap);
                    break;

                case OpCode.TryUnwrap:
                case OpCode.UnwrapUnion:
                    // R0 ← R0
                    writer.WriteLine($"R{baseRegister} ← R{baseRegister}");
                    break;

                case var other:
                    throw new ArgumentOutOfRangeException(other.ToString());
            }
        }
    }

    private static int ReadInt(ReadOnlySpan<byte> code, ref int ip)
    {
        var i = BitConverter.ToInt32(code[ip..]);
        ip += 4;
        return i;
    }

    private static void PrintLine(TextWriter writer, SourceSpan source, int ip, OpCode op, object? value = null) =>
        writer.WriteLine(
            $"0x{ip,-6:X4} {op.ToString(),-13} {value,-13} ; {source.FilePath}:{source.Start.Line}:{source.Start.Column}");
}