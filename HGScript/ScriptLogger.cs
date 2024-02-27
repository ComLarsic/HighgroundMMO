namespace HGScript;

/// <summary>
/// The logger for scripts.
/// </summary>
public static class ScriptLogger
{
    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[Script] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Log an error.
    /// </summary>
    /// <param name="message">The error to log.</param>
    /// <param name="exception">The exception that caused the error.</param>
    public static void LogError(string message, Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Script] {message}\n{exception}");
        Console.ResetColor();
    }

    /// <summary>
    /// Log a warning.
    /// </summary>
    /// <param name="message">The warning to log.</param>
    /// <param name="exception">The exception that caused the warning.</param>
    /// <param name="warning">The warning that was caused.</param>
    public static void LogWarning(string message, Exception exception, string warning)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[Script] {message}\n{exception}\n{warning}");
        Console.ResetColor();
    }
}
