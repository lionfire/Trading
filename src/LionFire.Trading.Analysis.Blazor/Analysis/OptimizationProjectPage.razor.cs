using LionFire.Blazor.Components;
using LionFire.Reactive.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace LionFire.Trading.Analysis;

public partial class OptimizationProjectPage
{
    [CascadingParameter(Name = "WorkspaceServices")]
    public IServiceProvider? WorkspaceServices { get; set; }

    [CascadingParameter(Name = "WorkspaceId")]
    public string? WorkspaceId { get; set; }

    KeyedCollectionView<string, BotEntity, BotVM>? ItemsEditor { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ViewModel!.Reader = WorkspaceServices?.GetService<IObservableReader<string, BotEntity>>();
    }
}

