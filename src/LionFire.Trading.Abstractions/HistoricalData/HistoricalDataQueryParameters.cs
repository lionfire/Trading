namespace LionFire.Trading.HistoricalData
{
    public class HistoricalDataQueryParameters
    {
        public string? Exchange;
        public string? ExchangeArea;

        public HistoricalDataQueryOptions Options { get; set; }

        public static HistoricalDataQueryParameters Default { get; set; } = new HistoricalDataQueryParameters
        {
            Options = HistoricalDataQueryOptions.Default,
        };
    }
    
}
