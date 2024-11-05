using LionFire.Execution;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MudBlazor;
using QuantConnect;
using ReactiveUI;
using System;

namespace LionFire.Trading.Worker.Components.Pages.Optimization;

public partial class OneShotOptimize
{
    [Inject]
    BacktestQueue BacktestQueue { get; set; }

    [Inject]
    IServiceProvider ServiceProvider { get; set; }

    bool ShowParameters { get; set; } = true;


   
    void OnOptimize()
    {
        var optimizationTask = new OptimizationTask(ServiceProvider, ViewModel.POptimization);

        cts = new();
        var task = optimizationTask.Run(cts.Token);

        IsRunning = true;
        IsAborted = false;

        Task.Run(async () =>
        {
            await task;
            IsRunning = false;
            IsCompleted = true;
            if (IsAborting)
            {
                IsAborted = true;
                IsAborting = false;
            }
            _ = InvokeAsync(StateHasChanged);
        });
    }

    void OnCancel()
    {
        IsAborting = true;
        cts.Cancel();
    }
    Task? task;
    CancellationTokenSource cts;

    OptimizationTask? OptimizationTask { get; set; }

    bool IsCompleted { get; set; }
    bool IsRunning { get; set; }
    bool IsAborted { get; set; }
    bool IsAborting { get; set; }

   
}

public class OneShotOptimizeVM : ReactiveObject
{
    public OneShotOptimizeVM()
    {
    }


    #region State

    #region Input Binding

    public string Exchange { get; set; } = "Binance";

    public Type PBotType { get; set; } = typeof(PAtrBot<double>);
    public string ExchangeArea { get; set; } = "futures";
    public string Symbol { get; set; } = "BTCUSDT";
    public string TimeFrameString { get; set; } = "m1";



    #region Derived

    public DateTimeOffset Start { get; set; } = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset EndExclusive { get; set; } = new DateTimeOffset(2021, 3, 1, 0, 0, 0, TimeSpan.Zero);

    public POptimization POptimization
    {
        get
        {
            return new POptimization(PBotType, new ExchangeSymbol(Exchange, ExchangeArea, Symbol))
            {
                CommonBacktestParameters = new()
                {
                    PBotType = PBotType,
                    Start = Start,
                    EndExclusive = EndExclusive,
                    Features = BotHarnessFeatures.Bars,
                    TimeFrame = TimeFrame,
                    ExchangeSymbol = ExchangeSymbolTimeFrame,
                }
            };
        }
    }

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


}