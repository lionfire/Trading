using LionFire.Events;

namespace LionFire.Trading.Automation;

public interface IBacktestBatch
    : IStartable
    //, IStoppable
    , IPausable
    , IRunnable
    , IAsyncCancellable
//, IProgress<double>  // ENH, find another interface?  Rx?
{
}


