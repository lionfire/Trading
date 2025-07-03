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

using System.Runtime.InteropServices;

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

    public string GeneralParametersSummary => $"{ViewModel!.PBotType.Name} — {ViewModel!.Symbol} {ViewModel!.TimeFrame} — {ViewModel!.TimeFrame.GetExpectedBarCount(ViewModel!.PMultiSim.Start, ViewModel!.PMultiSim.EndExclusive)} bars";


    void OnChange()
    {
        InvokeAsync(StateHasChanged);
    }

    #region Lifecycle

    protected override Task OnInitializedAsync()
    {
      
        return base.OnInitializedAsync();
    }

    #endregion

    protected override Task OnParametersSetAsync()
    {
        ViewModel ??= ActivatorUtilities.CreateInstance<OneShotOptimizeVM>(ServiceProvider);
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
        ViewModel.WhenAnyValue(x => x.Progress).Subscribe(_ => OnChange());

        return base.OnParametersSetAsync();
    }

    // DUPLICATE of OptimizationStatus.razor.cs
    MudBlazor.Color ComprehensivenessColor => ViewModel!.POptimization.LevelsOfDetail.ComprehensiveScanPerUn switch
    {
        >= 1 => Color.Success,
        <= 0.1 => Color.Error,
        _ => Color.Warning,
    };

    int _activeTabIndex { get; set; }

}

