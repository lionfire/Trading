using LionFire.Execution;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using MudBlazor;
using ReactiveUI;
//using ReactiveUI.Fody.Helpers;
//using ReactiveAttribute = ReactiveUI.Fody.Helpers.ReactiveAttribute;
using ReactiveUI.SourceGenerators;
using LionFire.Blazor.Components.Terminal;
using System.Reactive.Subjects;
using System.Reactive;
using DynamicData;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using LionFire.Logging;
using LionFire.Trading.Journal;
using System.Reactive.Linq;
using LionFire.Mvvm;
using System.Reactive.Disposables;
using QuantConnect.Api;
using LionFire.Blazor.Components;
using LionFire.ReactiveUI_;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OneShotOptimizeVM : DisposableBaseViewModel
{
    public LogVM LinesVM { get; } = new();
    public ILogger ConsoleLog => LinesVM;

    public IServiceProvider ServiceProvider { get; }
    public CustomLoggerProvider CustomLoggerProvider { get; }

    void ResetParameters()
    {
        var p = POptimization2;

        var c = Context.CommonBacktestParameters;
        c.ExchangeSymbol = new("Binance", "futures", "BTCUSDT");
        c.PBotType = BotTypes.FirstOrDefault();
        c.TimeFrame = TimeFrame.m1;
        c.Start = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        c.EndExclusive = new(2024, 1, 8, 0, 0, 0, TimeSpan.Zero);
    }

    public OneShotOptimizeVM(IServiceProvider serviceProvider, LionFire.Logging.CustomLoggerProvider customLoggerProvider)
    {
        ServiceProvider = serviceProvider;
        CustomLoggerProvider = customLoggerProvider;

        ResetParameters();

        customLoggerProvider.Observable.Subscribe(logEntry =>
        {
            LinesVM.Append(logEntry.Message ?? "", logEntry.LogLevel, logEntry.Category ?? "");
        });

        this.WhenAnyValue(x => x.OptimizationTask!.OptimizationMultiBatchJournal!.ObservableCache).Subscribe(oc =>
        {
            Backtests = oc;
        });
        this.WhenAnyValue(x => x.OptimizationTask!.OptimizationMultiBatchJournal!).Subscribe(oc =>
        {
            Debug.WriteLine("WhenAnyValue OMBJ");
        });
        this.WhenAnyValue(x => x.OptimizationTask!).Subscribe(oc =>
        {
            Debug.WriteLine("WhenAnyValue OptimizationTask");
        });
        debouncedChanges = changesToDebounce.Throttle(TimeSpan.FromMilliseconds(500));
        disposables.Add(this.WhenAnyValue(x => x.IsRunning).Subscribe(v => OnIsRunningValue(v)));

        Context.POptimization.ParametersChanged.Subscribe(_ => OnParametersChanged()).DisposeWith(disposables);

        this.WhenAnyValue(x => x.Context.POptimization.Parameters).Subscribe(_ => OnParametersChanged()).DisposeWith(disposables);
    }
    CompositeDisposable disposables = new();

    [Reactive]
    private OptimizationProgress _progress = OptimizationProgress.NoProgress;

    void OnParametersChanged()
    {
        Debug.WriteLine($"Context.POptimization.Parameters: {Context.POptimization.Parameters.Count}");
        this.RaisePropertyChanged(nameof(MinParameterPriority));
        this.RaisePropertyChanged(nameof(MaxParameterPriority));
    }

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
                    Progress = OptimizationTask.OptimizationStrategy.Progress;
                    changes.OnNext(Unit.Default);
                    await timer.WaitForNextTickAsync();

                };
            });
        }
    }
    private bool isKnownToBeRunning;

    #region State

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

    //[Reactive]
    //public int TestsQueued { get; set; }

    Task? task;
    //CancellationTokenSource? cts;

    public IObservable<Unit> DebouncedChanges => debouncedChanges;
    private IObservable<Unit> debouncedChanges;
    Subject<Unit> changesToDebounce = new();

    public IObservable<Unit> Changes => changes;
    Subject<Unit> changes = new();

    [Reactive]
    private OptimizationTask? _optimizationTask;

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

    public int MinParameterPriority => Context.POptimization.Parameters.Count == 0 ? 0 : Context.POptimization.Parameters.KeyValues.Values.Min(v => v.Info.ParameterAttribute?.OptimizePriorityInt ?? 0);
    public int MaxParameterPriority => Context.POptimization.Parameters.Count == 0 ? 0 : Context.POptimization.Parameters.KeyValues.Values.Max(v => v.Info.ParameterAttribute?.OptimizePriorityInt ?? 0);

    public long MaxBacktests { get => POptimization2.MaxBacktests; set => POptimization2.MaxBacktests = value; }
    public double MaxBacktestsExponential { get => Math.Log2(MaxBacktests); set => MaxBacktests = (long)Math.Pow(2.0, value); }


    public DateRange DateRange { get; set; } = new(new(2020, 1, 1), new(2020, 3, 1));

    public TradeJournalOptions TradeJournalOptions => POptimization2.TradeJournalOptions;
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

    public PMultiBacktestContext Context
    {
        get => context;
        set => RaiseAndSetNestedViewModelIfChanged(ref context, value);
    }
    private PMultiBacktestContext context = new();

    public PBacktestBatchTask2 Common => Context.CommonBacktestParameters;

    public POptimization POptimization2 => Context.POptimization;

    public TimeFrame TimeFrame => TimeFrame.Parse(TimeFrameString);

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame => new(Exchange, ExchangeArea, Symbol, TimeFrame);

    #endregion

    #endregion

    public List<string> Exchanges { get; set; } = ["Binance", "MEXC", "Phemex", "Bybit"];
    public List<string> TimeFrames { get; set; } = ["m1", "h1"];

    #endregion

    #region Data Sources

    public List<string> Symbols = ["BTCUSDT", "ETHUSDT"];
    public List<Type> BotTypes = [
        typeof(PAtrBot<double>),
        typeof(PDualAtrBot<double>),
        ];

    #endregion

    #region Methods

    public void Clear()
    {
        Progress = OptimizationProgress.NoProgress;
        OptimizationTask = null;

        Context = new();
        ResetParameters();
    }

    public void Cancel()
    {
        OptimizationTask?.Cancel();

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

    public void OnOptimize()
    {
        ConsoleLog.LogInformation("OnOptimize");

        Progress = OptimizationProgress.NoProgress;

        OptimizationTask = new OptimizationTask(ServiceProvider, Context);

        var task = OptimizationTask.Run();

        IsRunning = true;
        IsAborted = false;
        changes.OnNext(Unit.Default); // TODO - avoid this

        Task.Run(async () =>
        {
            ConsoleLog.LogInformation("Waiting for optimization to complete...");
            await task;
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
            changes.OnNext(Unit.Default);
            //_ = InvokeAsync(StateHasChanged);
        });

        Task.Run(async () =>
        {
            await OptimizationTask.RunTask;
            LinesVM.Append("Optimization completed", category: GetType().FullName);
        });
    }



    #endregion
}