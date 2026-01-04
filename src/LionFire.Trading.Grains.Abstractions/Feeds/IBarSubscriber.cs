namespace LionFire.Trading.Feeds;

/// <summary>
/// Interface for grains that can receive bar (candlestick) data directly from a scraper.
/// This enables demand-driven bar delivery without broadcast channels.
/// </summary>
public interface IBarSubscriber : IGrainWithStringKey
{
    /// <summary>
    /// Receives bars directly from a scraper grain.
    /// </summary>
    /// <param name="bars">The bar envelopes containing kline data and status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnBars(IEnumerable<BarEnvelope> bars);
}
