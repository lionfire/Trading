using Binance.Net.Enums;
using LionFire.Trading.Exchanges.Binance;
using McMaster.Extensions.CommandLineUtils;
using Oakton;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class HistoricalDataJobInput : CommonTradingInput
{

    public SymbolBarsRange SymbolBarsRange { get; set; }

    [FlagAlias("fields", true)]
    public string FieldsFlag { get; set; } = "native"; // TODO enum

    //[Option(Description = "Numeric type: { native | decimal | double | float } ")]
    [FlagAlias("numeric-type", true)]
    public string NumericTypeFlag { get; set; } = "native"; // TODO enum


    [Description("If non-default Fields and/or NumericType specified, set this to true to clean up the data that is downsampled to create the requested data")]
    [FlagAlias("clean-up", true)]
    public bool CleanUpRawDataFlag { get; set; } = false;

    
    

    [FlagAlias("limit", true)]
    public int LimitFlag { get; set; } = 1000; // Default 500; max 1000. TODO: learn how this impacts WEIGHT

    [FlagAlias("no-verify", true)]
    public bool NoVerifyFlag { get; set; }

    [FlagAlias("no-update-info", true)]
    [Oakton.IgnoreOnCommandLine]
    public bool NoUpdateInfoFlag { get; set; }

    //[FlagAlias("verbose", 'v')]
    //public bool VerboseFlag { get; set; }

    [FlagAlias("delete-extra-files", true)]
    public bool DeleteExtraFilesFlag { get; set; }

    //[FlagAlias("keep-extra-files", true)]
    //[Oakton.IgnoreOnCommandLine]
    //public bool KeepExtraFiles { get; set; }

    [FlagAlias("quiet", 'q')]
    public bool QuietFlag { get; set; }

    [FlagAlias("compression-level", 'z')]
    public int CompressFlag { get; set; } = 12; // TODO: use 0 thru 12 to control no compression up to max compression

    [Oakton.IgnoreOnCommandLine]
    [FlagAlias("compression-method", true)]
    public string CompressionMethod { get; set; } = "LZ4"; // TODO: Enum

    //public bool Compact { get; set; }

    [FlagAlias("save-empty", true)]
    public bool SaveEmptyFlag { get; set; }

    #region Input: Derived

    [Oakton.IgnoreOnCommandLine]
    public Type? NativeNumericType { get; set; }

    [Oakton.IgnoreOnCommandLine]
    public FieldSet FieldSet { get; set; }

    [Oakton.IgnoreOnCommandLine]
    public TimeFrame TimeFrame => TimeFrame.TryParse(IntervalFlag);

    [Oakton.IgnoreOnCommandLine]
    public KlineInterval? KlineInterval => TimeFrame.Name.ToKlineInterval();

    [Oakton.IgnoreOnCommandLine]
    public DateTime EffectiveTo => TimeFrame.TimeSpanApproximation >= TimeSpan.FromSeconds(2) ? ToFlag - TimeSpan.FromSeconds(1) : ToFlag;

    public DateTime GetEffectiveTo(DateTime to) => TimeFrame.TimeSpanApproximation >= TimeSpan.FromSeconds(2) ? to - TimeSpan.FromSeconds(1) : to - TimeSpan.FromMilliseconds(1);

    #endregion

}
