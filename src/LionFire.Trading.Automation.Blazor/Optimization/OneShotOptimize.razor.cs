using LionFire.Execution;
using LionFire.Mvvm;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MudBlazor;
using QuantConnect;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using LionFire.Blazor.Components.Terminal;
using System.Reactive.Subjects;
using System.Reactive;
using DynamicData;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OneShotOptimize
{
    [Inject]
    BacktestQueue BacktestQueue { get; set; } = null!;

    [Inject]
    IServiceProvider ServiceProvider { get; set; } = null!;

    bool ShowParameters { get; set; } = true;
    bool ShowResults { get; set; } = false;

    protected override Task OnParametersSetAsync()
    {
        ViewModel!.Changed.Subscribe(_ => InvokeAsync(StateHasChanged));

        ViewModel.WhenAnyValue(x => x.IsRunning).Subscribe(isRunning =>
        {
            if (isRunning)
            {
                ShowParameters = false;
                ShowResults = true;
                InvokeAsync(StateHasChanged);
            }
        });

        return base.OnParametersSetAsync();
    }
}

public class OneShotOptimizeVM : ReactiveObject
{
    public LogVM LinesVM { get; } = new();
    public ILogger ConsoleLog => LinesVM;

    public IServiceProvider ServiceProvider { get; }
    public OneShotOptimizeVM(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;

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
    }

    #region State

    [Reactive]
    public bool IsCompleted { get; set; }
    [Reactive]
    public bool IsRunning { get; set; }
    [Reactive]
    public bool IsAborted { get; set; }
    [Reactive]
    public bool IsAborting { get; set; }

    Task? task;
    //CancellationTokenSource? cts;

    [Reactive]
    public OptimizationTask? OptimizationTask { get; set; }

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; set; }

    #region Input Binding

    public string Exchange { get; set; } = "Binance";

    public Type PBotType { get; set; } = typeof(PAtrBot<double>);
    public string ExchangeArea { get; set; } = "futures";
    public string Symbol { get; set; } = "BTCUSDT";
    public string TimeFrameString { get; set; } = "m1";

    public bool EnableTradeJournals { get; set; } = true;
    public int MaxDetailedJournals { get; set; } = 10;
    public int BatchSize { get; set; } = 128;
    public int? MaxBacktests { get; set; } = 8_192;

    public DateRange DateRange { get; set; } = new(new(2020, 1, 1), new(2020, 2, 1));


    #region Derived

    //public DateTimeOffset Start { get; set; } = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
    //public DateTimeOffset EndExclusive { get; set; } = new DateTimeOffset(2021, 3, 1, 0, 0, 0, TimeSpan.Zero);

    public POptimization POptimization
    {
        get
        {
            return pOptimization ??= new POptimization(PBotType, new ExchangeSymbol(Exchange, ExchangeArea, Symbol))
            {
                MaxBatchSize = BatchSize,
                MaxBacktests = MaxBacktests ?? int.MaxValue,

                EnableParametersAtOrAboveOptimizePriority = -10,

                MaxDetailedJournals = EnableTradeJournals ? MaxDetailedJournals : 0, // TODO: Replace worse ones with better ones

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
            } else
            {
                ConsoleLog.LogInformation("Finished.");
            }
            changed.OnNext(Unit.Default);
            //_ = InvokeAsync(StateHasChanged);
        });

        Task.Run(async () =>
        {
            await OptimizationTask.RunTask;
            LinesVM.Append("Optimization completed", category: GetType().FullName);
        });
    }

    public IObservable<Unit> Changed => changed;
    Subject<Unit> changed = new();

    #endregion
}