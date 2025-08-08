using LionFire.Trading.Automation.Journaling.Trades;

namespace LionFire.Trading.Automation;

//public interface IBotContext<TPrecision> : IBotContext
//    where TPrecision : struct, INumber<TPrecision>
//{
//    SimContext<TPrecision> Sim { get; }

//    BotTradeJournal<TPrecision> BotJournal { get; }
//    ISimAccount<TPrecision>? DefaultSimAccount { get; }

//    // REVIEW: make ConcurrentDictionary
//    Dictionary<ExchangeSymbol, ISimAccount<TPrecision>> SimulatedAccounts { get; }
//}

//public interface IBacktestContext<TPrecision> : IBotContext<TPrecision>
//    where TPrecision : struct, INumber<TPrecision>
//{
//    IMultiBacktestHarness BotBatchController { get; }
//}
