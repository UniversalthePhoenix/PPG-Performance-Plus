namespace PPGPerformancePlus.Services;

public sealed class ConsoleLogger : ILogger
{
    public void Info(string message) => Console.WriteLine($"[PPG+] INFO  {message}");

    public void Warn(string message) => Console.WriteLine($"[PPG+] WARN  {message}");

    public void Error(string message) => Console.WriteLine($"[PPG+] ERROR {message}");
}
