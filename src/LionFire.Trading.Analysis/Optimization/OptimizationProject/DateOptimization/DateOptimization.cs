using DynamicData;

namespace LionFire.Trading;

public partial class DateOptimization : Buildable
{
    [ReactiveUI.SourceGenerators.Reactive]
    private DateTimeOffset? _startDate;
    [ReactiveUI.SourceGenerators.Reactive]
    private DateTimeOffset? _endDate;

    public SourceList<string>? BotWhitelist { get; set; }
    public SourceList<string>? BotBlacklist { get; set; }
    public SourceList<string>? SymbolWhitelist { get; set; }
    public SourceList<string>? SymbolBlacklist { get; set; }
}
