using LionFire.Trading.Indicators.QuantConnect_;
using QuantConnect.Securities;
using LionFire.Trading.ValueWindows;
using LionFire.Trading.Indicators.Harnesses;

namespace LionFire.Trading.Automation.Bots;

public class PStandardBot : PBot2<PStandardBot>
{

    #region Common for points systems

    [Parameter(DefaultValue = 1, MinValue = 1, MaxValue = int.MaxValue, DefaultMax = 30)]
    public int OpenThreshold { get; set; } = 3;

    [Parameter(DefaultValue = 1, MinValue = 1, MaxValue = int.MaxValue, DefaultMax = 30)]
    public int CloseThreshold { get; set; } = 2;

    #endregion

    #region Standard

    [Parameter("Reverse open and close operations", DefaultValue = false)]
    public bool ReverseOpenClose { get; set; }
    [Parameter("Reverse open and close operations", DefaultValue = false)]
    public bool Long { get; set; }

    /// <summary>
    /// If 1.0f, open all
    /// </summary>
    public float IncrementalOpenAmount { get; set; } = 1.0f;

    /// <summary>
    /// If 1.0f, close all
    /// </summary>
    public float IncrementalCloseAmount { get; set; } = 1.0f;

    #endregion
}

public class PAtrBot : PSymbolBarsBot2
{
    public required PAverageTrueRange ATR { get; init; }

    public required PStandardBot Standard { get; set; }
    public static PStandardBot StandardDefaults { get; set; } = new PStandardBot
    {
        //OpenThreshold
    };
}

[Bot(Direction = BotDirection.Unidirectional)]
public class AtrBot : StandardBot2<PAtrBot>
{
    #region Static

    public static IReadOnlyList<InputSlot> TInputs()
      => [new InputSlot() {
                    Name = "ATR",
                    Type = typeof(AverageTrueRange),
                }];

    #endregion

    #region Inputs

    public IReadOnlyList<Input> Inputs
    {
        get
        {
            // TInputs with specifics on lookbacks
            return [new Input() {
                    Name = "ATR",
                    Type = typeof(AverageTrueRange),
                    Lookback = Parameters.ATR.Period,
                    Phase = 0,
                    Source = Parameters.Input,
                }];
        }
    }

    #endregion

    //private AverageTrueRange ATR { get; init; }
    TimeFrameValuesWindow<double> ATR { get; init; }

    public OutputComponentOptions OutputExecutionOptions { get; } = new
    {
        Memory = 2,
    };

    public AtrBot(IServiceProvider serviceProvider, PAtrBot parameters) : base(parameters)
    {
        // TODO: Live Indicator harness if live
        var eATR = new HistoricalIndicatorHarness<AverageTrueRange, PAverageTrueRange, IKline, double>(serviceProvider, new IndicatorHarnessOptions<PAverageTrueRange>
        {

            Parameters = parameters.ATR,
            InputReferences = [parameters.Input],
            TimeFrame = parameters.TimeFrame,
        });

        eATR.GetWindow(OutputExecutionOptions.Memory)

        ATR = eATR.Memory;
    }


    #region State

    public int OpenScore { get; set; } = 0;
    public int CloseScore { get; set; } = 0;

    #endregion

    public override void OnBar(IKline kline)
    {
        if (ATR[0] > ATR[1]) OpenScore++;
        if (ATR[0] < ATR[1]) CloseScore++;

        if (OpenScore >= Parameters.Standard.OpenThreshold) { Open(); }
        if (CloseScore >= Parameters.Standard.CloseThreshold) { Close(); }

        long s;
    }

}
