namespace LionFire.Trading.HistoricalData
{
    public class HistoricalDataQueryParameters
    {
        public string? Exchange;
        public string? ExchangeArea;

        public QueryOptions Options { get; set; }

        public static HistoricalDataQueryParameters Default { get; set; } = new HistoricalDataQueryParameters
        {
            Options = QueryOptions.Default,
        };
    }
    
}
