using LionFire.Trading.Automation.Journaling.Trades;

namespace LionFire.Trading.Automation;

public class PBotContext<TPrecision>
where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// A unique Id within the Sim
    /// </summary>
    public required long Id { get; init; }

    public required IBot2 Bot { get; init; }


    public required PSimAccount<TPrecision> PSimulatedAccount { get; init; }

    //public required Dictionary<ExchangeSymbol, PSimAccount<TPrecision>>? PBacktestAccounts { get; init; }

    public required BotTradeJournal<TPrecision> BotJournal { get; init; }

    public required IServiceProvider ServiceProvider { get; init; }
}
