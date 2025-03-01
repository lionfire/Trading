using ReactiveUI;

namespace LionFire.Trading;

public partial class Buildable : ReactiveObject
{
    [ReactiveUI.SourceGenerators.Reactive]
    private string key = null!;

    [ReactiveUI.SourceGenerators.Reactive]
    private bool _isUpToDate;

    [ReactiveUI.SourceGenerators.Reactive]
    private bool _isBuilding;
}
