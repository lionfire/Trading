using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using LionFire.Trading.Automation.Blazor.Optimization.Queue;

namespace LionFire.Trading.Automation.Blazor.Optimization.Queue;

public partial class OptimizationQueue
{
    [Inject]
    IServiceProvider ServiceProvider { get; set; } = null!;

    void OnChange() => InvokeAsync(StateHasChanged);

    #region Lifecycle

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        ViewModel ??= ActivatorUtilities.CreateInstance<OptimizationQueueVM>(ServiceProvider);
        
        // Subscribe to ViewModel changes to refresh the UI
        ViewModel.WhenAnyValue(x => x.Jobs).Subscribe(_ => OnChange());
        ViewModel.WhenAnyValue(x => x.QueueStatus).Subscribe(_ => OnChange());
        ViewModel.WhenAnyValue(x => x.IsLoading).Subscribe(_ => OnChange());
        ViewModel.WhenAnyValue(x => x.ErrorMessage).Subscribe(_ => OnChange());
        ViewModel.WhenAnyValue(x => x.StatusFilter).Subscribe(_ => OnChange());
        ViewModel.WhenAnyValue(x => x.AutoRefresh).Subscribe(_ => OnChange());

        return base.OnParametersSetAsync();
    }

    #endregion
}