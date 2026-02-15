namespace Jitzu.Core.Common.Logging;

public static class ExceptionPrinter
{
    public static void Print(JitzuException ex)
    {
        var (file, length, start, end) = ex.Location;
        if (!File.Exists(file))
            return;

        var lines = File.ReadAllLines(file);

        var startErrorLine = start.Line - 1;
        var endErrorLine = end.Line - 1;

        var startLine = Math.Max(startErrorLine - 1, 0);
        var endLine = Math.Min(endErrorLine + 1, lines.Length);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    ╭───[ ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(ex.Message);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" ]");

        for (var i = startLine; i < endLine; i++)
        {
            var line = lines[i];
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{(i + 1):000} │ ");
            Console.ResetColor();

            if (i == startErrorLine)
            {
                Console.Write(line[..(start.Column - 1)]);

                Console.ForegroundColor = ConsoleColor.Red;
                if (i == endErrorLine)
                {
                    Console.Write(line[(start.Column - 1)..(end.Column - 1)]);
                    Console.ResetColor();
                    Console.WriteLine(line[(end.Column - 1)..]);
                }
                else
                {
                    Console.WriteLine(line[(start.Column - 1)..]);
                }

                continue;
            }

            if (i > startErrorLine && i < endErrorLine)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(line);
                continue;
            }

            if (i == endErrorLine)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var endIndex = Math.Min(end.Column, line.Length);
                
                Console.Write(line[..endIndex]);
                Console.ResetColor();

                Console.WriteLine(line[endIndex..]);
                continue;
            }

            Console.WriteLine(line);
        }

        Console.WriteLine();
    }
}