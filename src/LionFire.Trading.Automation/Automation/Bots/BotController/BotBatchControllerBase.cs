using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Automation;

/// <summary>
/// Batch processing of multiple bots:
/// - InputEnumerators are enumerated in lock step
/// </summary>
/// <remarks>
/// </remarks>
public abstract class BotBatchControllerBase : IBotBatchController
{
    #region Identity

    public abstract BotExecutionMode BotExecutionMode { get; }

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Parameters

    public IEnumerable<IPBacktestTask2> PBacktests { get; }

    #region Derived

    // Must match across all parameters

    public TimeFrame TimeFrame { get; }
    public DateTimeOffset InputStart { get; }
    public DateTimeOffset Start { get; }
    public DateTimeOffset EndExclusive { get; }

    public bool TicksEnabled { get; }

    #endregion

    #endregion

    #region Lifecycle

    public BotBatchControllerBase(IServiceProvider serviceProvider, IEnumerable<IPBacktestTask2> parameters)
    {
        ServiceProvider = serviceProvider;
        PBacktests = parameters;

        var first = parameters.FirstOrDefault();
        if (first == null) throw new ArgumentException("batch empty");

        TimeFrame = first.TimeFrame;
        Start = first.Start;

        EndExclusive = first.EndExclusive;

        foreach (var p in parameters)
        {
            if (p.TimeFrame != TimeFrame) throw new ArgumentException("TimeFrame mismatch");
            if (p.Start != Start) throw new ArgumentException("Start mismatch");
            if (p.EndExclusive != EndExclusive) throw new ArgumentException("EndExclusive mismatch");
            TicksEnabled |= p.TicksEnabled();
        }
    }

    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    #endregion

    #region State

    #region Enumerators

    public Dictionary<string, InputEnumeratorBase> InputEnumerators { get; } = new();

    #endregion

    #endregion

    #region Init

    public IHistoricalTimeSeries Resolve(Type outputType, object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve(outputType, source);

    #endregion
}

