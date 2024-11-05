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
                ViewModel!.Start = new DateTimeOffset(range.Start.Value, TimeSpan.Zero);
                ViewModel!.EndExclusive = new DateTimeOffset(range.End.Value, TimeSpan.Zero);
            });
        }
        return base.OnParametersSetAsync();
    }

    public DateRange DateRange { get; set; } = new(new(2021, 1, 1), new(2021, 3, 1));


}

