namespace LionFire.Trading.Automation;

public interface IBotHarness
{
    BotExecutionMode BotExecutionMode { get; }
    DateTimeOffset Start { get; }
    DateTimeOffset SimulatedCurrentDate { get; }
    DateTimeOffset EndExclusive { get; }

    bool TicksEnabled { get; }
    //bool BarsEnabled { get; } // ENH - for ticks only

}

public interface IBotBatchController : IBotHarness
{
    IServiceProvider ServiceProvider { get; }

}
