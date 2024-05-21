namespace LionFire.Trading.Automation;

public interface IBotController
{
    IServiceProvider ServiceProvider { get; }
    BotExecutionMode BotExecutionMode { get; }
}

