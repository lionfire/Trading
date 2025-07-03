
using CryptoExchange.Net.CommonObjects;
using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LionFire.Trading.Automation;
#if FUTURE // Maybe. Try to avoid if possible
public sealed class LiveBotContext<TPrecision> : BotContextBase<TPrecision>
   where TPrecision : struct, INumber<TPrecision>
{

    #region Lifecycle

    public LiveBotContext(SimContext<TPrecision> simContext, PBotContext<TPrecision> pContext) : base(simContext, pContext)
    {
    }

    #endregion

    #region State

    public override DateTimeOffset SimulatedCurrentDate => simulatedCurrentDate;
    DateTimeOffset simulatedCurrentDate;

    public override DateTimeOffset SimulationStartTime => simulationStartTime;
    DateTimeOffset simulationStartTime;

    #endregion
}

// UNUSED / UNNECESSARY
public sealed class BotBatchBacktestContext<TPrecision> : BotContextBase<TPrecision>, IBacktestContext<TPrecision>
   where TPrecision : struct, INumber<TPrecision>
{

    #region Relationships

    //public IMultiBacktestHarness BotBatchController => botBatchController;
    //private MultiBacktestHarnessBase botBatchController;

    #endregion

    #region Lifecycle

    #region Factory methods

    //public static async ValueTask<BotBatchBacktestContext<TPrecision>> Create(PBotContext<TPrecision> pContext, IBatchBotHarness botBatchController)
    //{
    //    var c = new BotBatchBacktestContext<TPrecision>(pContext, botBatchController);
    //    await c.OnStarting();
    //    return c;
    //}

    #endregion

    public BotBatchBacktestContext(SimContext<TPrecision> simContext, PBotContext<TPrecision> pContext, IMultiBacktestHarness botBatchController) : base(simContext, pContext)
    {
        this.botBatchController = (MultiBacktestHarnessBase)botBatchController;


    }

    #endregion

    #region State

    public override DateTimeOffset SimulatedCurrentDate => Sim.SimulatedCurrentDate;
    public override DateTimeOffset SimulationStartTime => Sim.Start;

    #endregion

    #region Event Handling

    public override ValueTask OnStarting()
    {
        Journal.Write(new JournalEntry<TPrecision>(simulatedAccount)
        {
            EntryType = JournalEntryType.Start,
            Time = Sim.Start,
        });
        return ValueTask.CompletedTask;
    }

    #endregion
}



#endif

