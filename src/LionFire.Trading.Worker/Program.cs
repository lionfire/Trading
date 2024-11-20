using LionFire.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using FireLynx.Blazor;
using LionFire.Persistence.Filesystem;
using LionFire.Vos;
using System;
using Microsoft.Extensions.Configuration;
using LionFire.Mvvm;
using LionFire.Types;
using LionFire.Trading.Binance_;
using LionFire.Trading.Indicators;
using LionFire.Blazor.Components.Terminal;
using System.Reactive.Subjects;
using LionFire.Trading.Automation.Blazor.Optimization;
using Serilog;

var wab = WebApplication.CreateBuilder(args);
var clp = new CustomLoggerProvider();

Host.CreateApplicationBuilder(args)
    .LionFire(9850, lf => lf
        //.FireLynxApp()
        .If(Convert.ToBoolean(lf.Configuration["Orleans:Enable"]), lf => lf.Silo())
        .WebHost<TradingWorkerStartup>(w => w.Http().BlazorServer())
        .ConfigureServices(services => services
            .AddHistoricalBars(lf.Configuration)
            .AddSingleton<BinanceClientProvider>()
            .AddSingleton<IndicatorProvider>()
            .AddIndicators()
            .Backtesting(lf.Configuration)
            .AddSingleton(clp)
            .AddLogging(b =>b.AddProvider(clp))
            //    .VosMount("/output".ToVobReference(), @"f:\st\Investing-Output\.local".ToFileReference())
            .AddUIComponents()
            .AddMudBlazorComponents()
            .AddMvvm()
            .AddTradingUI()
            //.AddTransient<OneShotOptimizeVM>()
            .AddSingleton<OneShotOptimizeVM>()
            .AddAutomation()
        )
    )
    //.I(i => i.Configuration.AddUserSecrets<FireLynxAppStartup>(optional: true, reloadOnChange: true)) // TODO SECURITY - disable in Production
    .Build()
    .Run();


public class CustomLoggerProvider : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var logger = new CustomLogger(categoryName);
        logger.Observable.Subscribe(subject);
    }
    public IObservable<LogMessage> Observable => subject;
    Subject<LogMessage> subject = new Subject<LogMessage>();

    public void Dispose()
    {
        // Implement disposal logic if necessary
    }

}

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
            LogLevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatter = (o, ex) => formatter((TState)o, ex)
        });
    }

    public IObservable<LogMessage> Observable => subject;
    Subject<LogMessage> subject = new Subject<LogMessage>();

}
public class LogMessage
{
    public LogLevel LogLevel;
    public EventId EventId;
    public object? State;
    public Exception? Exception;
    public Func<object, Exception, string>? Formatter;
}

