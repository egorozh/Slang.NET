using Microsoft.Extensions.Logging;

namespace Slang.Gpt.Cli;

internal class FileLogger(string logFilePath) : ILogger
{
    private static readonly object Lock = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        string message = formatter(state, exception);

        string logEntry = $"{DateTime.UtcNow:HH:mm:ss.fff} [{logLevel}] - {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception;
        }

        WriteToFile(logEntry);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    private void WriteToFile(string logEntry)
    {
        lock (Lock)
        {
            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}