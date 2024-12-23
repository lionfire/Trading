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

    MudBlazor.Color ComprehensivenessColor => ViewModel!.POptimization2.LevelsOfDetail.ComprehensiveScanPerUn switch
    {
        >= 1 => Color.Success,
        <= 0.1 => Color.Error,
        _ => Color.Warning,
    };

}
