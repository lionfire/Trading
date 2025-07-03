namespace LionFire.Trading.Automation;

#if UNUSED

// UNUSED: New approach: all bots operate in the same way (like they're being backtested, even if fed data in real time), and have an optional reference to a LiveBotServices collection of services for doing something in real time with that bot's state (such as making real money market executions.)

public enum BotExecutionMode
{
    Unspecified = 0,
    Backtest = 1,
    Live = 2,
}

#endif