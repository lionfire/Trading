#if false
using Binance.Net.Enums;
using LionFire.Trading.Exchanges.Binance;
using McMaster.Extensions.CommandLineUtils;

namespace LionFire.Trading.HistoricalData.Retrieval;


public class Old_HistoricalDataRetrieveJob
{
    #region Construction

    //public HistoricalDataRetrieveJob(BinanceClientProvider binanceClientProvider, KlineArrayFileProvider klineArrayFileProvider, ILogger<HistoricalDataRetrieveJob> logger)
    //{
    //    BinanceClientProvider = binanceClientProvider;
    //    KlineArrayFileProvider = klineArrayFileProvider;
    //    Logger = logger;
    //    BinanceClient = BinanceClientProvider.GetPublicClient();
    //}

    #endregion

    #region Parameters

    private DataInput DataInput { get; set; } = new();

    [Option(ShortName = "e")]
    public string Exchange { get; set; } = "Binance";

    [Option(ShortName = "a")]
    public string ExchangeArea { get; } = "futures";

    [Option(ShortName = "s")]
    public string Symbol { get; } = "BTCUSDT";


    [Option(Template = "--fields", Description = "Which fields to save: { native | ohlcv | ohlc } ")]
    public string Fields { get; } = "native";

    [Option(Description = "Numeric type: { native | decimal | double | float } ")]
    public string NumericType { get; } = "native";

    [Option(Template = "--cleanup", Description = "If non-default Fields and/or NumericType specified, set this to true to clean up the data that is downsampled to create the requested data")]
    public bool CleanUpRawData { get; } = false;

    [Option(ShortName = "i")]
    public string Interval { get; } //= "m1"; // TODO

    [Option(ShortName = "f")]
    public DateTime From { get; } //= DateTime.UtcNow - TimeSpan.FromHours(24);

    [Option(ShortName = "t")]
    public DateTime To { get; } //= DateTime.UtcNow + TimeSpan.FromHours(25);

    public int Limit { get; set; } = 1000; // Default 500; max 1000. TODO: learn how this impacts WEIGHT

    [Option("--no-verify")]
    public bool NoVerify { get; }

    [Option(ShortName = "v")]
    public bool Verbose { get; }

    [Option(ShortName = "q")]
    public bool Quiet { get; }

    // Use 0 thru 12 to control no compression up to max compression
    [Option(ShortName = "z")]
    public int Compress { get; } = 12;

    [Option(ShortName = "c")]
    public bool Compact { get; }

    #region Derived

    public Type NativeNumericType { get; set; }

    public FieldSet FieldSet { get; set; }

    public TimeFrame TimeFrame => TimeFrame.TryParse(Interval);

    public KlineInterval? KlineInterval => TimeFrame.Name.ToKlineInterval();

    public DateTime EffectiveTo => TimeFrame.TimeSpanApproximation >= TimeSpan.FromSeconds(2) ? To - TimeSpan.FromSeconds(1) : To;

    public DateTime GetEffectiveTo(DateTime to) => TimeFrame.TimeSpanApproximation >= TimeSpan.FromSeconds(2) ? to - TimeSpan.FromSeconds(1) : to - TimeSpan.FromMilliseconds(1);


    #endregion

    #endregion

}

#endif