using System.Reactive.Subjects;

namespace LionFire.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var logger = new CustomLogger(categoryName);
        logger.Observable.Subscribe(subject);
        return logger;
    }
    public IObservable<LogMessage> Observable => subject;
    Subject<LogMessage> subject = new Subject<LogMessage>();

    public void Dispose()
    {
        subject.Dispose();
    }

}

