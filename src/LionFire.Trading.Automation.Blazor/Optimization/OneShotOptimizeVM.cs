using DynamicData;
using LionFire.Blazor.Components;
using LionFire.Blazor.Components.Terminal;
using LionFire.Execution;
using LionFire.Logging;
using LionFire.Mvvm;
using LionFire.Reactive.Persistence;
using LionFire.ReactiveUI_;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Optimization;
using LionFire.Trading.Automation.Portfolios;
using LionFire.Trading.Journal;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using QuantConnect.Api;
using ReactiveUI;
//using ReactiveUI.Fody.Helpers;
//using ReactiveAttribute = ReactiveUI.Fody.Helpers.ReactiveAttribute;
using ReactiveUI.SourceGenerators;
using ScottPlot.Hatches;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OneShotOptimizeVM : DisposableBaseViewModel
{
    public LogVM LinesVM { get; } = new();
    public ILogger ConsoleLog => LinesVM;

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    //public CustomLoggerProvider CustomLoggerProvider { get; }
    public BotTypeRegistry BotTypeRegistry { get; }
    public IGrainFactory GrainFactory { get; }

    #endregion

    #region Ambient

    public Portfolio2? Portfolio { get; set; }

    #endregion


    #region Lifecycle

    public OneShotOptimizeVM(IServiceProvider serviceProvider
        //, LionFire.Logging.CustomLoggerProvider customLoggerProvider
        , BotTypeRegistry botTypeRegistry
        , IGrainFactory grainFactory)
    {
        ServiceProvider = serviceProvider;
        //CustomLoggerProvider = customLoggerProvider;
        BotTypeRegistry = botTypeRegistry;
        GrainFactory = grainFactory;

        #region Event Handlers

        //customLoggerProvider.Observable.Subscribe(logEntry =>
        //{
        //    LinesVM.Append(logEntry.Message ?? "", logEntry.LogLevel, logEntry.Category ?? "");
        //});

        this.WhenAnyValue(x => x.MultiSimContext!.Journal!.ObservableCache).Subscribe(oc =>
        {
            Backtests = oc;
        });
        this.WhenAnyValue(x => x.MultiSimContext!.Journal!).Subscribe(oc =>
        {
            Debug.WriteLine("WhenAnyValue OMBJ");
        });
        this.WhenAnyValue(x => x.MultiSimContext!).Subscribe(oc =>
        {
            Debug.WriteLine("WhenAnyValue OptimizationTask");
        });
        debouncedChanges = changesToDebounce.Throttle(TimeSpan.FromMilliseconds(500));
        disposables.Add(this.WhenAnyValue(x => x.IsRunning).Subscribe(v => OnIsRunningValue(v)));

        //Sim.POptimization.ParametersChanged.Subscribe(_ => OnParametersChanged()).DisposeWith(disposables);

        this.WhenAnyValue(x => x.POptimization.Parameters).Subscribe(_ => OnParametersChanged()).DisposeWith(disposables);

        #endregion

        Reset();
    }
    CompositeDisposable disposables = new();

    [Reactive]
    private OptimizationProgress _progress = OptimizationProgress.NoProgress;

    void OnParametersChanged()
    {
        Debug.WriteLine($"Sim.POptimization.PMultiSim: {POptimization.Parameters.Count}");
        this.RaisePropertyChanged(nameof(MinParameterPriority));
        this.RaisePropertyChanged(nameof(MaxParameterPriority));
    }

    #endregion


    #region State

    [Reactive]
    private OptimizationTask? _optimizationTask;

    #region (Derived)

    public OptimizationRunInfo OptimizationRunInfo => OptimizationTask?.MultiSimContext?.OptimizationRunInfo;

    public MultiSimContext? MultiSimContext => _optimizationTask?.MultiSimContext;

    #endregion

    [Reactive]
    private bool _isCompleted;

    //[Reactive]
    //public bool IsRunning { get; set; }

    //public bool IsRunning
    //{
    //    get => _isRunning;
    //    set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    //}
    [ReactiveUI.SourceGenerators.ReactiveAttribute]
    private bool _isRunning;

    [Reactive]
    private bool _isAborted;

    [Reactive]
    private bool _isAborting;

    [Reactive]
    private bool _isFaulted;

    //[Reactive]
    //public int TestsQueued { get; set; }
    
    #region Queue-related Properties
    
    [Reactive]
    private OptimizationQueueItem? _queuedJob;
    
    [Reactive]
    private OptimizationQueueStatus? _queueStatus;
    
    /// <summary>
    /// True if this user has a job queued or running in the global queue
    /// </summary>
    public bool HasQueuedJob => QueuedJob != null && 
        (QueuedJob.Status == OptimizationJobStatus.Queued || QueuedJob.Status == OptimizationJobStatus.Running);
    
    /// <summary>
    /// Queue position if job is queued (1-based)
    /// </summary>
    public int? QueuePosition => QueueStatus?.QueuedCount;
    
    /// <summary>
    /// Estimated time until this job starts
    /// </summary>
    public TimeSpan? EstimatedStartTime => QueueStatus?.EstimatedNextJobDelay;
    
    #endregion

    Task? task;
    //CancellationTokenSource? cts;

    public IObservable<Unit> DebouncedChanges => debouncedChanges;
    private IObservable<Unit> debouncedChanges;
    Subject<Unit> changesToDebounce = new();

    public IObservable<Unit> Changes => changes;
    Subject<Unit> changes = new();


    // TODO
    //public B NestedViewModel
    //{
    //    get { return _nestedViewModel; }
    //    set { RaiseAndSetNestedViewModelIfChanged(ref _nestedViewModel, value); }
    //}
    //private B _nestedViewModel;

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; set; }

    #region Input Binding

    [Reactive]
    private string _exchange = "Binance";

    [Reactive]
    private Type _PBotType = typeof(PAtrBot<double>);

    [Reactive]
    private string _exchangeArea = "futures";

    [Reactive]
    private string _symbol = "BTCUSDT";

    [Reactive]
    private string _timeFrameString = "h1";

    public int BatchSize { get; set; } = 1024;//= 128;
    public double BatchSizeExponential { get => Math.Log2(BatchSize); set => BatchSize = (int)Math.Pow(2.0, value); }

    public int MinParameterPriority => POptimization.Parameters.Count == 0 ? 0 : POptimization.Parameters.KeyValues.Values.Min(v => v.Info.ParameterAttribute?.OptimizePriorityInt ?? 0);
    public int MaxParameterPriority => POptimization.Parameters.Count == 0 ? 0 : POptimization.Parameters.KeyValues.Values.Max(v => v.Info.ParameterAttribute?.OptimizePriorityInt ?? 0);

    public long MaxBacktests { get => POptimization.MaxBacktests; set => POptimization.MaxBacktests = value; }
    public double MaxBacktestsExponential { get => Math.Log2(MaxBacktests); set => MaxBacktests = (long)Math.Pow(2.0, value); }


    public DateRange DateRange { get; set; } = new(new(2020, 1, 1), new(2020, 3, 1));

    public TradeJournalOptions TradeJournalOptions => POptimization.TradeJournalOptions;
    // ENH: get defaults for this and other parameters somewhere/somehow, based on the a template for the user (perhaps the user has many templates)

    #region Derived

#if OLD
    public POptimization POptimization
    {
        get
        {
            return  new POptimization(PBotType, new ExchangeSymbol(Exchange, ExchangeArea, Symbol))
            {
                MaxBatchSize = BatchSize,
                MaxBacktests = MaxBacktests,

                MinParameterPriority = MinParameterPriority,

                TradeJournalOptions = TradeJournalOptions,

                CommonBacktestParameters = new()
                {
                    PBotType = PBotType,
                    //Start = Start,
                    //EndExclusive = EndExclusive,
                    Features = BotHarnessFeatures.Bars,
                    TimeFrame = TimeFrame,
                    ExchangeSymbol = ExchangeSymbolTimeFrame,
                    //StartingBalance = 10000,

                }
                //GranularityStepMultiplier = 4,
                //ParameterOptimizationOptions = new Dictionary<string, IParameterOptimizationOptions>
                //{
                //    ["Period"] = new ParameterOptimizationOptions<int>
                //    {
                //        MaxProbes = 20,
                //        MinProbes = 20,
                //        MinValue = 2,
                //        MaxValue = 40,
                //        OptimizationStep = 3
                //    },
                //    //["OpenThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 2 },
                //    //["CloseThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 3 },
                //},
            };
        }
    }
#endif

    //public PBatch Context
    //{
    //    get => context;
    //    set => RaiseAndSetNestedViewModelIfChanged(ref context, value);
    //}
    //private PBatch context = new();

    public PMultiSim PMultiSim
    {
        get => pMultiSim;
        set => RaiseAndSetNestedViewModelIfChanged(ref pMultiSim, value);
    }
    private PMultiSim pMultiSim;

    public POptimization? POptimization => PMultiSim?.POptimization!;

    public TimeFrame TimeFrame => TimeFrame.Parse(TimeFrameString);

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame => new(Exchange, ExchangeArea, Symbol, TimeFrame);

    #endregion

    #endregion

    public List<string> Exchanges { get; set; } = ["Binance", "MEXC", "Phemex", "Bybit"];
    public List<string> TimeFrames { get; set; } = ["m1", "h1"];

    #endregion

    #region Data Sources

    public List<string> Symbols = ["BTCUSDT", "ETHUSDT"];

    /// <summary>
    /// Bot types available for optimization. Derived from BotTypeRegistry.
    /// Open generic types are closed with double as the type argument.
    /// </summary>
    public List<Type> BotTypes => botTypes ??= GetBotTypesFromRegistry();
    private List<Type>? botTypes;

    private List<Type> GetBotTypesFromRegistry()
    {
        var result = new List<Type>();

        foreach (var type in BotTypeRegistry.PBotRegistry.Names.Values)
        {
            try
            {
                Type closedType;
                if (type.IsGenericTypeDefinition)
                {
                    // Close open generic with double (standard for trading bots)
                    var typeArgs = type.GetGenericArguments();
                    var closedArgs = typeArgs.Select(_ => typeof(double)).ToArray();
                    closedType = type.MakeGenericType(closedArgs);
                }
                else
                {
                    closedType = type;
                }
                result.Add(closedType);
            }
            catch (Exception ex)
            {
                // Skip types that can't be closed with double
                System.Diagnostics.Debug.WriteLine($"Could not close generic type {type.Name}: {ex.Message}");
            }
        }

        return result.OrderBy(t => t.Name).ToList();
    }

    #endregion

    #region Event Handling

    private void OnIsRunningValue(bool val)
    {
        if (isKnownToBeRunning == IsRunning) return;
        isKnownToBeRunning = IsRunning;

        if (isKnownToBeRunning)
        {
            _ = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.25));
                while (isKnownToBeRunning && !IsAborted && OptimizationTask != null)
                {
                    if (OptimizationTask.OptimizationStrategy?.Progress != null)
                    {
                        Progress = OptimizationTask.OptimizationStrategy?.Progress;
                    }
                    changes.OnNext(Unit.Default);
                    await timer.WaitForNextTickAsync();
                }
            });
        }
    }
    private bool isKnownToBeRunning;

    public void OnToggle(BacktestBatchJournalEntry entry)
    {
        Debug.WriteLine("Toggle BacktestBatchJournalEntry: " + entry.Id);
        if (Portfolio == null) return;

        OptimizationRunReference? orr = OptimizationTask?.MultiSimContext?.OptimizationRunInfo;

        Portfolio.Toggle(orr, entry);
    }
    public async void OnExportToBot(BacktestBatchJournalEntry item, IObservableReaderWriter<string, BotEntity>? bots)
    {
        var bot = new BotEntity
        {
            //Name = $"{OptimizationRunInfo.OptimizationExecutionDate.ToShortDateString()}",
            Name = $"{OptimizationRunInfo.Guid};{item.BatchId}-{item.Id}",
            Comments = $"Exported from Optimization Run {OptimizationRunInfo.Guid}",
            ExchangeSymbolTimeFrame = OptimizationRunInfo.ExchangeSymbolTimeFrame,
            BotTypeName = OptimizationRunInfo.BotTypeName,

            // TODO: Pick one 
            //ParametersDictionary = item.PMultiSim?.ToParametersDictionary(),
            Parameters = item.Parameters,
        };

        try
        {
            await bots.Write(bot.Name, bot);
        }
        catch { }
    }

    public async Task OnOptimize()
    {
        ConsoleLog.LogInformation("OnOptimize");

        Progress = OptimizationProgress.NoProgress;

        OptimizationTask = new OptimizationTask(ServiceProvider, PMultiSim);

        Task startTask;

        try
        {
            startTask = OptimizationTask.StartAsync();
            //if (task.IsFaulted) { throw task.Exception; }
        }
        catch (Exception ex)
        {
            ConsoleLog.LogError(ex, "Exception starting OptimizationTask");
            IsAborted = true;
            throw;
        }

        IsRunning = true;
        IsAborted = false;
        changes.OnNext(Unit.Default); // TODO - avoid this

        _ = Task.Run(async () =>
        {
            try
            {
                ConsoleLog.LogInformation("Waiting for optimization to complete...");
                await startTask;
                await OptimizationTask.RunTask!;
                ConsoleLog.LogInformation("Waiting for optimization to complete...done.");
                IsRunning = false;
                IsCompleted = true;
                if (IsAborting)
                {
                    ConsoleLog.LogInformation("Aborted.");
                    IsAborted = true;
                    IsAborting = false;
                }
                else
                {
                    ConsoleLog.LogInformation("Finished.");
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.LogError(ex, "Optimization failed: {Message}\n{StackTrace}", ex.Message, ex.ToString());
                IsRunning = false;
                IsFaulted = true;
            }
            changes.OnNext(Unit.Default);
            //_ = InvokeAsync(StateHasChanged);
        });

        _ = Task.Run(async () =>
        {
            try
            {
                await startTask;
                await OptimizationTask.RunTask!;
                LinesVM.Append("Optimization completed", category: GetType().FullName);
            }
            catch (Exception ex)
            {
                LinesVM.Append($"Optimization FAILED: {ex}", category: GetType().FullName);
            }
        });
    }
    
    /// <summary>
    /// Queue an optimization job instead of running it immediately
    /// </summary>
    public async Task OnQueue(int priority = 5)
    {
        try
        {
            ConsoleLog.LogInformation("Queuing optimization job with priority {Priority}", priority);
            
            // Serialize the current parameters with custom options for Orleans grain compatibility
            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.Preserve,
                Converters = { new JsonStringEnumConverter() }
            };
            var parametersJson = JsonSerializer.Serialize(PMultiSim, serializerOptions);
            
            // Get the queue grain and enqueue the job
            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");
            var queuedJob = await queueGrain.EnqueueJobAsync(parametersJson, priority, "UI-User");
            
            QueuedJob = queuedJob;
            
            ConsoleLog.LogInformation("Job {JobId} queued successfully at position {Position}", 
                queuedJob.JobId, await GetQueuePositionAsync(queuedJob.JobId));
                
            // Start monitoring queue status
            _ = Task.Run(MonitorQueueStatus);
        }
        catch (Exception ex)
        {
            ConsoleLog.LogError(ex, "Failed to queue optimization job");
            throw;
        }
    }
    
    /// <summary>
    /// Cancel the currently queued job
    /// </summary>
    public async Task OnCancelQueued()
    {
        if (QueuedJob == null) return;
        
        try
        {
            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");
            var cancelled = await queueGrain.CancelJobAsync(QueuedJob.JobId);
            
            if (cancelled)
            {
                ConsoleLog.LogInformation("Job {JobId} cancelled successfully", QueuedJob.JobId);
                QueuedJob = null;
            }
            else
            {
                ConsoleLog.LogWarning("Failed to cancel job {JobId}", QueuedJob.JobId);
            }
        }
        catch (Exception ex)
        {
            ConsoleLog.LogError(ex, "Error cancelling job {JobId}", QueuedJob?.JobId);
        }
    }
    
    /// <summary>
    /// Refresh queue status information
    /// </summary>
    public async Task RefreshQueueStatus()
    {
        try
        {
            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");
            QueueStatus = await queueGrain.GetQueueStatusAsync();
            
            // Update current job status if we have one
            if (QueuedJob != null)
            {
                var updatedJob = await queueGrain.GetJobAsync(QueuedJob.JobId);
                if (updatedJob != null)
                {
                    QueuedJob = updatedJob;
                    
                    // If job completed, clear it after a delay
                    if (updatedJob.Status == OptimizationJobStatus.Completed ||
                        updatedJob.Status == OptimizationJobStatus.Failed ||
                        updatedJob.Status == OptimizationJobStatus.Cancelled)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            QueuedJob = null;
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleLog.LogWarning(ex, "Failed to refresh queue status");
        }
    }
    
    /// <summary>
    /// Get the position of a specific job in the queue
    /// </summary>
    private async Task<int> GetQueuePositionAsync(Guid jobId)
    {
        try
        {
            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");
            var jobs = await queueGrain.GetJobsAsync(OptimizationJobStatus.Queued, 1000);
            
            var position = jobs.Select((job, index) => new { job, index })
                              .FirstOrDefault(x => x.job.JobId == jobId)?.index + 1;
            
            return position ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Monitor queue status in background
    /// </summary>
    private async Task MonitorQueueStatus()
    {
        while (HasQueuedJob)
        {
            await RefreshQueueStatus();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    #endregion

    #region Methods

    public void DoGC()
    {
        GC.Collect();
    }
    public void Reset()
    {
        Progress = OptimizationProgress.NoProgress;
        OptimizationTask = null;
        //PMultiSim.Dispose();
        //Context.Dispose();
        GC.Collect();
        //Context = new();
        PMultiSim = new();
        PMultiSim.POptimization = null;
        PMultiSim.POptimization = new(PMultiSim);
        ResetParameters();

    }

    public void Cancel()
    {
        MultiSimContext?.Cancel();

        //if (cts == null)
        //{
        //    ConsoleLog.LogWarning("Cancel requested, but no task is running.");
        //    return;
        //}
        ConsoleLog.LogInformation("Cancel requested.  Aborting...");
        IsAborting = true;
        //cts?.Cancel();
        //cts = null;
    }



    #endregion

    #region (Private) Methods

    private void ResetParameters()
    {
        var p = POptimization;

        var c = PMultiSim;
        c.ExchangeSymbolTimeFrame = new("Binance", "futures", "BTCUSDT", TimeFrame.m1);
        c.PBotType = BotTypes.FirstOrDefault();

        var now = DateTimeOffset.UtcNow;
        var startYear = now.Year;
        var startMonth = now.Month - 6;
        if (startMonth <= 0)
        {
            startYear--;
            startMonth += 12;
        }

        c.Start = new(startYear, startMonth, 1, 0, 0, 0, TimeSpan.Zero);
        c.EndExclusive = new(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

    }


    #endregion

}