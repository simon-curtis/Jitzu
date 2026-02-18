namespace Jitzu.Shell.Infrastructure.Logging;

public static class ConsoleEx
{
    public static void ConfigureOutput()
    {
        if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
            return;

        var stdOut = Console.OpenStandardOutput();
        Console.SetOut(new StreamWriter(stdOut, bufferSize: 65536)
        {
            AutoFlush = false
        });
    }
}
