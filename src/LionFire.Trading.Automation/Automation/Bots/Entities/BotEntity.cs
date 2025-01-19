using LionFire.Trading.Automation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace LionFire.Trading.Automation;

public partial class BotEntity : ReactiveObject
{
    public PBotHarness? PBotHarness { get; set; }

    [Reactive]
    bool _enabled;

    [Reactive]
    private string? _name;

    [Reactive]
    private string? _comments;

}
