namespace LionFire.Trading.Automation;

public interface IBotBatchController 
{
    IServiceProvider ServiceProvider { get; }
    BotExecutionMode BotExecutionMode { get; }
}
