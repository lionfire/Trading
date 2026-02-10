namespace LionFire.Trading.HistoricalData.Retrieval;

/// <summary>
/// Tracks exchange API rate limit usage (e.g., Binance X-MBX-USED-WEIGHT-1M header).
/// Register in DI to receive weight updates from RetrieveHistoricalDataJob.
/// </summary>
public interface IExchangeRateLimitTracker
{
    void ReportWeight(string exchange, string headerName, int usedWeight);
}
