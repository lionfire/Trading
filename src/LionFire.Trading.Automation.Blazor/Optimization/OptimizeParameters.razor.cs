using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ReactiveUI;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class OptimizeParameters(ILogger<OptimizeParameters> Logger)
{

    ILogger Logger2 = Logger;

    //[CascadingParameter(Name = "OneShotOptimizeVM")]
    //public OneShotOptimizeVM OneShotOptimizeVM { get; set; }

    protected override Task OnParametersSetAsync()
    {
        this.WhenAnyValue(x => x.ViewModel.DateRange).Subscribe(range =>
        {
            ViewModel!.POptimization.CommonBacktestParameters.Start = !range.Start.HasValue ? DateTimeOffset.MinValue : new DateTimeOffset(range.Start.Value, TimeSpan.Zero);
            ViewModel!.POptimization.CommonBacktestParameters.EndExclusive = !range.End.HasValue ? DateTimeOffset.MaxValue : new DateTimeOffset(range.End.Value, TimeSpan.Zero);
        });
        return base.OnParametersSetAsync();
    }



}

