using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ReactiveUI;
using System.Reactive.Disposables;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OptimizationStatus(ILogger<OptimizeParameters> Logger)
{
    ILogger Logger2 = Logger;

    CompositeDisposable disposables = new();

    protected override Task OnParametersSetAsync()
    {
        //this.ViewModel!.POptimization2.ParametersChanged.Subscribe(_ =>
        //{
        //    ViewModel.Sim.POptimization.OnLevelsOfDetailChanged();
        //    StateHasChanged();
        //})
        //    .DisposeWith(disposables);
        this.ViewModel!.POptimization.WhenAnyValue(x=>x.LevelsOfDetail).Subscribe(_ =>
        {
            InvokeAsync(StateHasChanged);
        })
            .DisposeWith(disposables);
        return base.OnParametersSetAsync();
    }

    public ValueTask DisposeAsync()
    {
        disposables?.Dispose();
        return ValueTask.CompletedTask;
    }

    //protected override Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {

    //    }
    //    return base.OnAfterRenderAsync(firstRender);
    //}

    MudBlazor.Color ComprehensivenessColor => ViewModel!.POptimization.LevelsOfDetail.ComprehensiveScanPerUn switch
    {
        >= 1 => Color.Success,
        <= 0.1 => Color.Error,
        _ => Color.Warning,
    };

    private void OnLevelClicked(double testCount)
    {
        ViewModel!.POptimization.MaxBacktests = (long)testCount;
    }

    private static string FormatTestCount(double count) => count switch
    {
        >= 1_000_000_000_000 => $"{count / 1_000_000_000_000:N1}T",
        >= 1_000_000_000 => $"{count / 1_000_000_000:N1}B",
        >= 1_000_000 => $"{count / 1_000_000:N1}M",
        >= 1_000 => $"{count / 1_000:N1}K",
        _ => count.ToString("N0"),
    };
}
