using LionFire.Execution;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using MudBlazor;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
using ReactiveUI.SourceGenerators;
using ReactiveAttribute = ReactiveUI.Fody.Helpers.ReactiveAttribute;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OneShotOptimizeVM : ReactiveObject
{
    public LogVM LinesVM { get; } = new();
    public ILogger ConsoleLog => LinesVM;

    public IServiceProvider ServiceProvider { get; }
    public CustomLoggerProvider CustomLoggerProvider { get; }

    public OneShotOptimizeVM(IServiceProvider serviceProvider, LionFire.Logging.CustomLoggerProvider customLoggerProvider)
    {
        ServiceProvider = serviceProvider;
        CustomLoggerProvider = customLoggerProvider;

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
            Debug.WriteLine("OMBJ");
        });
        this.WhenAnyValue(x => x.OptimizationTask!).Subscribe(oc =>
        {
            Debug.WriteLine("OT");
        });
        debouncedChanges = changesToDebounce.Throttle(TimeSpan.FromMilliseconds(500));
        disposables.Add(this.WhenAnyValue(x => x.IsRunning).Subscribe(v => OnIsRunningValue(v)));

    }
    CompositeDisposable disposables = new();

    [Reactive]
    public OptimizationProgress Progress { get; set; } = OptimizationProgress.NoProgress;

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
    public bool IsCompleted { get; set; }
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
    public bool IsAborted { get; set; }
    [Reactive]
    public bool IsAborting { get; set; }

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
    public OptimizationTask? OptimizationTask { get; set; }

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; set; }

    #region Input Binding

    public string Exchange { get; set; } = "Binance";

    public Type PBotType { get; set; } = typeof(PAtrBot<double>);
    public string ExchangeArea { get; set; } = "futures";
    public string Symbol { get; set; } = "BTCUSDT";
    public string TimeFrameString { get; set; } = "m1";

    public int BatchSize { get; set; } = 128;
    public int MaxBacktests { get; set; } = 8_192;

    public DateRange DateRange { get; set; } = new(new(2020, 1, 1), new(2020, 2, 1));

    public TradeJournalOptions TradeJournalOptions { get; set; } = new(); // ENH: get defaults for this and other parameters somewhere/somehow, based on the a template for the user (perhaps the user has many templates)

    #region Derived

    // REVIEW TODO FIXME: once pOptimization gets created, all options are ignored
    public POptimization POptimization
    {
        get
        {
            return pOptimization ??= new POptimization(PBotType, new ExchangeSymbol(Exchange, ExchangeArea, Symbol))
            {
                MaxBatchSize = BatchSize,
                MaxBacktests = MaxBacktests,

                EnableParametersAtOrAboveOptimizePriority = -10,

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
    POptimization? pOptimization;

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
        OptimizationTask = new OptimizationTask(ServiceProvider, POptimization);

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