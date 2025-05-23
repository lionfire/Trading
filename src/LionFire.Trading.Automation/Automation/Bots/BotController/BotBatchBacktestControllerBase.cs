﻿using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.ExtensionMethods.Cloning;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Automation;

public abstract class BotHarnessBase
{
    #region Identity

    public abstract BotExecutionMode BotExecutionMode { get; }

    #endregion

}

/// <summary>
/// Batch processing of multiple bots:
/// - InputEnumerators are enumerated in lock step
/// </summary>
/// <remarks>
/// REVIEW: This base class should maybe be combined with BacktestBatchTask2 
/// </remarks>
public abstract class BotBatchBacktestControllerBase : BotHarnessBase, IBotBatchController
{
    #region Identity

    public MultiBacktestContext Context { get; }

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Parameters

    public IEnumerable<PBacktestTask2> PBacktests { get; }
    public BacktestExecutionOptions ExecutionOptions { get; }
    public PBacktestBatchTask2 BatchParameters { get; }

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

    public BotBatchBacktestControllerBase(IServiceProvider serviceProvider, IEnumerable<PBacktestTask2> parameters, MultiBacktestContext context)
    {
        ServiceProvider = serviceProvider;
        PBacktests = parameters;
        Context = context;

        var first = parameters.FirstOrDefault();
        if (first == null) throw new ArgumentException("batch empty");

        TimeFrame = first.TimeFrame ?? (first.PBot as IPTimeFrameBot2)?.TimeFrame ?? throw new ArgumentNullException($"Neither first {nameof(parameters)} nor first {nameof(first.PBot)} has a {nameof(first.TimeFrame)}");
        Start = first.Start;
        EndExclusive = first.EndExclusive;

        foreach (var p in PBacktests)
        {
            if (p.EffectiveTimeFrame != TimeFrame) throw new ArgumentException("TimeFrame mismatch");
            if (p.Start != Start) throw new ArgumentException("Start mismatch");
            if (p.EndExclusive != EndExclusive) throw new ArgumentException("EndExclusive mismatch");

            TicksEnabled |= p.TicksEnabled();
        }
    }

    protected virtual ValueTask Init()
    {
        foreach (var p in PBacktests)
        {
            ValidateParameter(p);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// (No need to call this base method)
    /// </summary>
    /// <param name="p"></param>
    protected virtual void ValidateParameter(PBacktestTask2 p) { }

    public abstract Task StartAsync(CancellationToken cancellationToken = default);


    protected virtual void OnFinishing()
    {
    }

    protected virtual ValueTask OnFinished()
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region State

    public DateTimeOffset SimulatedCurrentDate { get; protected set; }

    public BacktestBatchJournal Journal { get; protected set; }

    #region Enumerators

    public Dictionary<string, InputEnumeratorBase> InputEnumerators { get; } = new();

    #endregion

    #endregion

    #region Init

    public IHistoricalTimeSeries Resolve(Type outputType, object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve(outputType, source);

    #endregion
}

