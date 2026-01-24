namespace LionFire.Trading.Symbols;

/// <summary>
/// Represents the state of a symbol within a collection.
/// </summary>
public enum SymbolState
{
    /// <summary>
    /// Symbol is actively included in the collection and used for operations.
    /// </summary>
    Active,

    /// <summary>
    /// Symbol qualifies for inclusion but awaits user approval.
    /// New symbols from provider queries start in this state.
    /// </summary>
    Pending,

    /// <summary>
    /// Symbol has been explicitly excluded by the user.
    /// Will not be auto-added even if it qualifies again.
    /// </summary>
    Excluded,

    /// <summary>
    /// Symbol was previously active but is no longer available on the exchange.
    /// Retained for historical reference.
    /// </summary>
    Delisted,

    /// <summary>
    /// Symbol is excluded and hidden from the UI.
    /// Used to declutter the interface for permanently unwanted symbols.
    /// </summary>
    Hidden
}
