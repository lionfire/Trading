namespace LionFire.Logging; 

public class LogMessage
{
    public string? Category;
    public string? Message => Formatter?.Invoke(State!, Exception!);
    public LogLevel LogLevel;
    public EventId EventId;
    public object? State;
    public Exception? Exception;
    public Func<object, Exception, string>? Formatter;
}

