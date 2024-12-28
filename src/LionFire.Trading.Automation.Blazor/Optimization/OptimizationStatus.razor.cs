using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ReactiveUI;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OptimizationStatus(ILogger<OptimizeParameters> Logger)
{
    ILogger Logger2 = Logger;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.ViewModel!.POptimization2.ParametersChanged.Subscribe(_ => StateHasChanged());
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    MudBlazor.Color ComprehensivenessColor => ViewModel!.POptimization2.LevelsOfDetail.ComprehensiveScanPerUn switch
    {
        >= 1 => Color.Success,
        <= 0.1 => Color.Error,
        _ => Color.Warning,
    };

}
