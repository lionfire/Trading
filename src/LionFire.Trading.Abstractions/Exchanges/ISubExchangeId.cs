namespace LionFire.Trading.Exchanges;

public interface ISubExchangeId
{
    /// <summary>
    /// Examples:
    ///  - Binance
    ///  - OANDA
    ///  - IC Markets
    /// </summary>
    string Exchange { get; }

    /// <summary>
    /// Examples:
    ///  - Futures
    ///  - Spot
    ///  - Margin
    ///  - Options
    /// </summary>
    string ExchangeAreaKind { get; }

    /// <summary>
    /// Examples:
    ///  - USD(S)-M
    ///  - COIN-M
    /// </summary>
    string ExchangeArea { get; }

}

