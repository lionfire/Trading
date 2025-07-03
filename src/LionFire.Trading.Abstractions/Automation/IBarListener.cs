namespace LionFire.Trading.Automation;


/// <summary>
/// Something that acts based on market events
/// </summary>
public interface IBarListener : IMarketListenerOrder
{
    /// <summary>
    /// All state has been updated for the next bar and it is ready to be processed by the bot (market participant).
    /// </summary>
    void OnBar();
}

/// <summary>
/// Something that acts based on market events
/// </summary>
public interface ITickListener : IMarketListenerOrder
{
    void OnTick();
}

public interface IMarketParticipant2<TPrecision> : IBarListener
    where TPrecision : struct, INumber<TPrecision>
{
    // FUTURE
    //IDisposable SubscribeToPriceAlert(ExchangeSymbol exchangeSymbol, ...);
}
