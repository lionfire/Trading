
using LionFire.Trading.Journal;
using System.Numerics;

namespace LionFire.Trading.Automation;

public interface ISimulationController : IAccountProvider2
{
    DateTimeOffset SimulatedCurrentDate { get; }
    long GetNextTransactionId();
}

public interface ISimulationController<TPrecision> : ISimulationController
where TPrecision : struct, INumber<TPrecision>
{
    ISimulatedAccount2<TPrecision> Account { get; }

    IEnumerable<IAccount2<TPrecision>> Accounts => Account == null ? [] : [Account];

    IBacktestTradeJournal<TPrecision> Journal { get; }

    DateTimeOffset StartTime { get; }

    void OnAccountAborted();
}
