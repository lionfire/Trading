using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ReactiveUI;

namespace LionFire.Trading.Worker.Components.Pages.Optimization;

public partial class OptimizeParameters
{

    //[CascadingParameter(Name = "OneShotOptimizeVM")]
    //public OneShotOptimizeVM OneShotOptimizeVM { get; set; }

    public OptimizeParameters()
    {
    }

    protected override Task OnParametersSetAsync()
    {
        if (ViewModel != null)
        {
            this.WhenAnyValue(x => x.DateRange).Subscribe(range =>
            {
                ViewModel!.POptimization.CommonBacktestParameters.Start = !range.Start.HasValue ? DateTimeOffset.MinValue : new DateTimeOffset(range.Start.Value, TimeSpan.Zero);
                ViewModel!.POptimization.CommonBacktestParameters.EndExclusive = !range.End.HasValue ? DateTimeOffset.MaxValue : new DateTimeOffset(range.End.Value, TimeSpan.Zero);
            });
        }
        return base.OnParametersSetAsync();
    }

    public DateRange DateRange { get; set; } = new(new(2020, 1, 1), new(2020, 2, 1));


}

