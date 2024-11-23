using System.Reactive.Subjects;

namespace LionFire.Logging;
public class CustomLogger : Microsoft.Extensions.Logging.ILogger 
{
    private readonly string _categoryName;

    public CustomLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);
        //Console.WriteLine($"CUSTOM - [{_categoryName}] {logLevel}: {message}"); // Example: Write to console
        subject.OnNext(new LogMessage
        {
            Category = _categoryName,
            LogLevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatter = (o, ex) => formatter((TState)o, ex)
        });
    }

    public IObservable<LogMessage> Observable => subject;
    private Subject<LogMessage> subject = new Subject<LogMessage>();

}

