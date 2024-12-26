using LionFire.Logging;
using LionFire.Mvvm;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using QuantConnect;
using ReactiveUI;
using System;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OneShotOptimize
{
    [Inject]
    BacktestQueue BacktestQueue { get; set; } = null!;

    [Inject]
    IServiceProvider ServiceProvider { get; set; } = null!;

    bool ShowParameters { get; set; } = true;
    bool ShowOptimizationStatus { get; set; } = true;
    bool ShowResults { get; set; } = false;
    bool ShowBacktests { get; set; } = false;
    bool ShowLog { get; set; } = true;

    public string GeneralParametersSummary => $"{ViewModel!.PBotType.Name} — {ViewModel!.Symbol} {ViewModel!.TimeFrame} — {ViewModel!.TimeFrame.GetExpectedBarCount(ViewModel!.Common.Start , ViewModel!.Common.EndExclusive)} bars";

    private SortMode _sortMode = SortMode.Multiple;

    void OnChange()
    {
        InvokeAsync(StateHasChanged);
    }
    protected override Task OnParametersSetAsync()
    {
        ViewModel ??= new(ServiceProvider, ServiceProvider.GetRequiredService<CustomLoggerProvider>());
        ViewModel!.DebouncedChanges.Subscribe(_ => OnChange());
        ViewModel!.Changes.Subscribe(_ => OnChange());

        ViewModel.WhenAnyValue(x => x.IsRunning).Subscribe(isRunning =>
        {
            if (isRunning)
            {
                //ShowParameters = false;
                ShowResults = true;
                InvokeAsync(StateHasChanged);
            }
        });
        ViewModel.WhenAnyValue(x=>x.Progress).Subscribe(_ => OnChange());

        return base.OnParametersSetAsync();
    }
}
