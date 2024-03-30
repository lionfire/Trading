using Oakton;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class CommonTradingInput : NetCoreInput
{
    [FlagAlias("exchange", 'e')]
    public string ExchangeFlag { get; set; } = "Binance";


    [FlagAlias("area", 'a')]
    public string ExchangeAreaFlag { get; set; } = "futures";

    [FlagAlias("symbol", 's')]
    public string Symbol { get; set; } = "BTCUSDT";

    [FlagAlias("from", 'f')]
    public DateTime FromFlag { get => fromFlag > ToFlag ? ToFlag : fromFlag; set => fromFlag = value; }
    private DateTime fromFlag = DateTime.UtcNow - TimeSpan.FromHours(24);

    [FlagAlias("to", 't')]
    public DateTime ToFlag { get; set; } = DateTime.UtcNow + TimeSpan.FromHours(25);

    [FlagAlias("timeframe", 'i')]
    public string IntervalFlag { get; set; } = "h1";
}
