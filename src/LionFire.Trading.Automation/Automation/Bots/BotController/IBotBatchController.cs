﻿namespace LionFire.Trading.Automation;

public interface IBotBatchController
{
    IServiceProvider ServiceProvider { get; }
    BotExecutionMode BotExecutionMode { get; }
    DateTimeOffset Start { get; }
    DateTimeOffset SimulatedCurrentDate { get; }
    DateTimeOffset EndExclusive { get; }

    bool TicksEnabled { get; }

}
