using LionFire.Execution;
using LionFire.Structures;
using LionFire.Threading;
using LionFire.Trading.Indicators.Harnesses;
using System.Collections.Concurrent;

namespace LionFire.Trading.Automation;

public class BacktestTask2 : IStartable
    , IStoppable
    , IPausable
//, IProgress<double>  // ENH, find another interface?  Rx?
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Input

    #endregion

    #region Output

    #endregion

    #region Parameters

    public IPBacktestTask2 Parameters { get; }

    #endregion

    #region Lifecycle

    public BacktestTask2(IServiceProvider serviceProvider, IPBacktestTask2 parameters)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
    }

    #endregion

    #region State Machine

    CancellationTokenSource cts;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cts = new CancellationTokenSource();
        BacktestDate = Parameters.Start;

        if (Parameters.TicksEnabled)
        {
            RunTicks().FireAndForget();
        }
        else
        {
            RunBars().FireAndForget();
        }
        return Task.CompletedTask;
    }

    public bool Paused { get; protected set; }

    public Task Pause()
    {
        Paused = true;
        return Task.CompletedTask;
    }

    public Task Continue()
    {
        Paused = false;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }

    #endregion

    #region Loop

    private async Task RunBars()
    {
        while (BacktestDate < Parameters.EndExclusive && !cts.IsCancellationRequested)
        {
            await NextBar();
        }
    }

    private Task RunTicks()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region State

    public BacktestAccount2? BacktestAccount { get; private set; }
    public DateTimeOffset BacktestDate { get; protected set; } = DateTimeOffset.UtcNow;

    #endregion

    #region Methods

    public async Task NextBar()
    {
    }

    #endregion
}

/// <summary>
/// How to do this?
/// - 
/// </summary>
public class BacktestHarness 
{
    List<IIndicatorHarness> indicatorHarnesses;
    List<BacktestBotHarness> backtestBotHarnesses;

    ConcurrentDictionary<Guid, BacktestTask2> tasks = new();
    public bool IsAcceptingNewTasks { get; set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    private bool IsInitialized => Start != default;


    public BacktestHarness(IServiceProvider serviceProvider)
    {
    }

    public bool CanAdd(BacktestTask2 task)
    {
        if (!IsAcceptingNewTasks) return false;

        if (IsInitialized)
        {
            if (Start != task.Parameters.Start || EndExclusive != task.Parameters.EndExclusive) return false;
        }

        return true;
    }

    public int Commonality(BacktestTask2 task)
    {
        int commonality = 0;

        // Something like this:
        //foreach(var input in task.Inputs)
        //{
        //    if (this.InputCounts.Contains(input.Key))
        //    {
        //        commonality += InputCounts[input.Key];
        //    }
        //}

        return commonality;
    }

    public bool Enqueue(BacktestTask2 task)
    {
        if (!CanAdd(task)) return false;

        if (!IsInitialized)
        {
            Start = task.Parameters.Start;
            EndExclusive = task.Parameters.EndExclusive;
        }

        return true;
    }

    public Task Run(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}

// Activating bots
// - Try ctor(TValue)
// - Try ctor(IServiceProvider, TValue)
// - Try GetService<IFactory<TBot, TValue>>().Create(TValue)
// - save successful result to static
public class BacktestTask2<TParameters> : BacktestTask2
    //where TBot : IBot2
    where TParameters : IPBacktestTask2
{
    IBot2 Bot { get; }

    public BacktestTask2(IServiceProvider serviceProvider, TParameters parameters):base(serviceProvider, parameters)
    {
        if (typeof(TParameters) is IFactory<IBot2> factory)
        {
            Bot = factory.Create();
        }
    }
}
