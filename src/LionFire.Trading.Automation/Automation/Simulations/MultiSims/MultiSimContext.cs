using LionFire.IO;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.HistoricalData;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

/// <summary>
/// Designed to support an Optimization Run, though can also be used for single backtests.
/// FUTURE maybe: More easily support more lightweight scenarios such as single backtest
/// 
/// 1 PMultiSim, 1:
/// - Start
/// - EndExclusive
/// - BotType
///
/// 0 or 1:
/// - Parameters.POptimization  (but currently always 1 since we only use this for optimization)
/// 
/// 1 or more:
/// - SimContext
/// </summary>
public sealed partial class MultiSimContext : ReactiveObject, IValidatable, IDisposable
{
    public ValidationContext ValidateThis(ValidationContext validationContext)
    {
        validationContext
            .PropertyNotNull(nameof(Parameters), Parameters)
            .PropertyNotNull(nameof(ServiceProvider), ServiceProvider)
            .PropertyNotNull(nameof(DateChunker), DateChunker)
            .PropertyNotNull(nameof(BotTypeName), BotTypeName)
            .PropertyNotNull(nameof(FilesystemRetryPipeline), FilesystemRetryPipeline)
            .ValidateOptional(Optimization)
            ;

        if(IsInitialized)
        {
            validationContext
                .PropertyNotNull(nameof(Journal), Journal)
                ;

        }

        return validationContext;
    }

    #region Identity

    public Guid Guid { get; } = Guid.NewGuid();

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    public ResiliencePipeline? FilesystemRetryPipeline { get; }

    public DateChunker DateChunker { get; }

    #endregion

    #region Parameters

    public PMultiSim Parameters { get; }

    #region (Derived)

    // OLD
    //public PBatch DefaultPSimContext => defaultPSimContext ??= new PBatch(Parameters);
    //private PBatch? defaultPSimContext;

    public string BotTypeName { get; }

    public string OutputDirectory => outputDirectory ?? throw new NotInitializedException();
    private string? outputDirectory;

    // OLD
    //            //outputDirectory = GetNumberedRunDirectory(); // BLOCKING I/O
    //            //BatchInfoFileWriter = new(Path.Combine(OutputDirectory, $"BatchInfo.hjson"));

    #endregion

    #endregion

    #region Components

    public OptimizationContext Optimization { get; }

    // TEMP - refactoring
    public OptimizationRunInfo? OptimizationRunInfo => Optimization?.OptimizationRunInfo;

    #endregion

    #region Lifecycle

    public MultiSimContext(IServiceProvider serviceProvider, PMultiSim parameters, DateChunker dateChunker)
    {
        parameters.ValidateOrThrow();

        ServiceProvider = serviceProvider;
        Parameters = parameters;
        DateChunker = dateChunker;

        BotTypeName = ServiceProvider.GetRequiredService<BotTypeRegistry>().GetBotName(Parameters.BotType);

        cancellationTokenSource.Token.Register(() =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetCanceled(cancellationTokenSource.Token);
            }
        });

        if (ServiceProvider.GetService<ResiliencePipelineProvider<string>>()?.TryGetPipeline(FilesystemRetryPolicy.Default, out var p) == true)
        {
            FilesystemRetryPipeline = p;
        }

        Optimization = new(this); // FUTURE: make optional
    }

    public bool IsInitialized { get; private set; }

    public async Task Init()
    {
        this.ValidateOrThrow();
        (outputDirectory, var runId) = await Optimization.BacktestsRepository.GetAndCreateOptimizationRunDirectory(
                    Parameters.ExchangeSymbolTimeFrame!,
                    BotTypeName,
                    Parameters.Start,
                    Parameters.EndExclusive
                    //,Guid.ToString()
                    ).ConfigureAwait(false);

        Journal = ActivatorUtilities.CreateInstance<BacktestsJournal>(ServiceProvider, this, Parameters.PBotType!, /* retainInMemory */ true);

        if (Optimization != null)
        {
            await Optimization.Init();
        }
        
        IsInitialized=true;
        this.ValidateOrThrow();
    }

    public void Dispose()
    {
        cancellationTokenSource?.Dispose();
    }

    #region Cancellation

    public CancellationToken CancellationToken => cancellationTokenSource.Token;
    private CancellationTokenSource cancellationTokenSource = new();
    public bool IsCancellationRequested => cancellationTokenSource.IsCancellationRequested;


    public void Cancel()=> cancellationTokenSource.Cancel();

    public void AddCancellationToken(CancellationToken externalToken)
    {
        if (externalToken == default || !externalToken.CanBeCanceled) { return; }

        // Create a linked token source that responds to both internal and external cancellation
        var oldSource = cancellationTokenSource;
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            oldSource.Token,
            externalToken);

        oldSource.Dispose();

        // Register callback for external cancellation
        externalToken.Register(() =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetCanceled(externalToken);
            }
        });
    }

    #endregion

    #region Completion

    public Task Task => tcs.Task;
    private readonly TaskCompletionSource tcs = new();

    internal void OnFinished() => tcs.TrySetResult();
    internal void OnFaulted(Exception exception) => tcs.TrySetException(exception);

    #endregion

    #endregion

    #region Services

    public BacktestsJournal? Journal { get; private set; } // Set in Init()

    #endregion

    #region Event Handling

    //public Action<IBatchContext>? ConfigureBatch { get; init; }

    public MultiBatchEvents BatchEvents { get; } = new();

    #endregion


}
