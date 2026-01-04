namespace LionFire.Trading.Automation;

/// <summary>
/// Result from the Add Bot dialog.
/// </summary>
public class AddBotDialogResult
{
    public required string BotTypeName { get; init; }
    public required string Symbol { get; init; }
    public required string Exchange { get; init; }
    public required string ExchangeArea { get; init; }
    public required TimeFrame TimeFrame { get; init; }
    public string? Name { get; init; }
    public string? Comments { get; init; }
    public bool Enabled { get; init; }
    public bool Live { get; init; }

    /// <summary>
    /// Generates a unique key for the bot.
    /// </summary>
    public string GenerateKey()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{BotTypeName}_{Symbol}_{TimeFrame.ToShortString()}_{suffix}";
    }

    /// <summary>
    /// Creates a BotEntity from this result.
    /// </summary>
    public BotEntity ToBotEntity()
    {
        return new BotEntity
        {
            BotTypeName = BotTypeName,
            Symbol = Symbol,
            Exchange = Exchange,
            ExchangeArea = ExchangeArea,
            TimeFrame = TimeFrame,
            Name = Name ?? $"{BotTypeName} {Symbol} {TimeFrame.ToShortString()}",
            Comments = Comments,
            Enabled = Enabled,
            Live = Live
        };
    }
}
