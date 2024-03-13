using LionFire.ExtensionMethods.Dumping;

namespace LionFire.Trading.HistoricalData;

public sealed class HistoricalDataMemoryCacheKey : IEquatable<HistoricalDataMemoryCacheKey?>
{
    public Type? Type;
    public string? Exchange;
    public string? ExchangeArea;
    public string? Symbol;
    public DateTimeOffset Start;
    public DateTimeOffset EndExclusive;

    #region Misc

    #region (auto-generated)

    public override bool Equals(object? obj)
    {
        return Equals(obj as HistoricalDataMemoryCacheKey);
    }

    public bool Equals(HistoricalDataMemoryCacheKey? other)
    {
        return other is not null &&
               EqualityComparer<Type?>.Default.Equals(Type, other.Type) &&
               Exchange == other.Exchange &&
               ExchangeArea == other.ExchangeArea &&
               Symbol == other.Symbol &&
               Start == other.Start &&
               EndExclusive == other.EndExclusive;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Exchange, ExchangeArea, Symbol, Start, EndExclusive);
    }

    public static bool operator ==(HistoricalDataMemoryCacheKey? left, HistoricalDataMemoryCacheKey? right)
    {
        return EqualityComparer<HistoricalDataMemoryCacheKey>.Default.Equals(left, right);
    }

    public static bool operator !=(HistoricalDataMemoryCacheKey? left, HistoricalDataMemoryCacheKey? right)
    {
        return !(left == right);
    }

    #endregion

    public override string ToString() => this.Dump().ToString();
    
    #endregion
}

