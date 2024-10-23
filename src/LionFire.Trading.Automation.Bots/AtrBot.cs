using LionFire.Trading.Indicators.QuantConnect_;
using QuantConnect.Securities;
using LionFire.Trading.ValueWindows;
using LionFire.Trading.Indicators.Harnesses;
using System.Collections.Concurrent;
using QuantConnect.Indicators;
using System.Diagnostics;
using System.Reflection;
using MathNet.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Bots;


public class PAtrBot<TValue> : PStandardBot2<PAtrBot<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{

    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof(AtrBot<TValue>);
    public static Type StaticMaterializedType => typeof(AtrBot<TValue>);

    //public static IReadOnlyList<InputSlot> InputSlots() => GetInputSlots();

    //public static IReadOnlyList<InputSlot> InputSlots()
    //  => [new InputSlot() {
    //                Name = "ATR",
    //                Type = typeof(AverageTrueRange),
    //            }];

    #endregion

    [Signal(0)]
    public PAverageTrueRange<double, TValue>? ATR { get; set; }

    /// <summary>
    /// Convention: match parameter name
    /// Can be get only, derived on other parameters.
    /// - if false, the ATR ITimeValueSeries{TValue} won't receive any values.
    /// </summary>
    //public bool UseAtr { get; set; } = true; 

    public PUnidirectionalBot? Unidirectional { get; set; }
    public PPointsBot? Points { get; set; }
    //public static PPointsBot StandardDefaults { get; set; } = new PPointsBot
    //{
    //    //OpenThreshold
    //};

    [JsonIgnore]
    const int Lookback = 1;

    #region Lifecycle

    public PAtrBot() { }
    public PAtrBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, uint period, QuantConnect.Indicators.MovingAverageType movingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders) : base(exchangeSymbolTimeFrame)
    {
        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = (int)period,
            Lookback = Lookback,
            MovingAverageType = movingAverageType,
            //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        };

        FinalizeInit();
    }

    public override void FinalizeInit()
    {
        // REVIEW - does InputLookbacks make sense?
        // ENH - automate setting this somehow
        //InputLookbacks = [(int)period + Lookback];
        InputLookbacks = [0, Lookback];

        ATR!.Lookback = Lookback;

        base.FinalizeInit();
    }

    #endregion

    #region Validaiton

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(ATR, nameof(ATR));
        ArgumentNullException.ThrowIfNull(Points, nameof(Points));
        ArgumentNullException.ThrowIfNull(Unidirectional, nameof(Unidirectional));
    }

    #endregion
}

[Bot(Direction = BotDirection.Unidirectional)]
public class AtrBot<TValue> : StandardBot2<PAtrBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    #region Inputs

    public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;

    //public IReadOnlyValuesWindow<HLC<double>> Bars { get; set; } = null!;

    #region OLD

    //public IReadOnlyList<IInputSignal> Inputs
    //{
    //    get
    //    {
    //        // TInputSlots with specifics on lookbacks
    //        return [
    //            //new InputSignal<decimal>() {
    //            //    Name = "ATR",
    //            //    Type = typeof(AverageTrueRange),
    //            //    Lookback = IndicatorParameters.ATR.Period,
    //            //    Phase = 0,
    //            //    Source = IndicatorParameters.Input,
    //            //}
    //            ];
    //    }
    //}
    //public override IReadOnlyList<IInputSignal> InputSignals { get; } = new List<IInputSignal>(); // TODO

    #endregion

    #endregion

    //public OutputComponentOptions OutputExecutionOptions { get; } = new OutputComponentOptions
    //{
    //    Memory = 2,
    //};

    //public AtrBot()
    //{
    //    // TODO: Live Indicator harness if live
    //    //var eATR = new BufferingIndicatorHarness<AverageTrueRange<TValue>, PAverageTrueRange<double. TValue>, IKline, TValue>(serviceProvider, new IndicatorHarnessOptions<PAverageTrueRange<TValue>>(parameters.ATR!)
    //    //{
    //    //    Inputs = parameters.Inputs == null ? [] : [parameters.Inputs],
    //    //    TimeFrame = parameters.TimeFrame,
    //    //});

    //    //eATR.GetWindow(OutputExecutionOptions.Memory);

    //    //ATR = eATR.Memory;
    //}

    #region State

    public float OpenScore { get; set; } = 0;
    public float CloseScore { get; set; } = 0;

    #endregion

    #region Event Handling

    int barIndex = 0;

    public override void OnBar()
    {
        //if (barIndex++ % 50000 == 0)
        //{
        //    if (ATR.Size > 0) {
        //        Debug.WriteLine($"#{barIndex} {this.GetType().Name}.OnBar ATR: {ATR[0]}, bars available: {ATR.Size}"); }
        //    else { Debug.WriteLine($"#{barIndex} {this.GetType().Name}.OnBar Bar: N/A"); }

        //    //if (Bars.Size > 0)
        //    //{
        //    //    Debug.WriteLine($"#{barIndex} {this.GetType().Name}.OnBar Bar: {Bars[0]}, bars available: {Bars.Size}");
        //    //}
        //    //else
        //    //{
        //    //    Debug.WriteLine($"#{barIndex} {this.GetType().Name}.OnBar Bar: N/A");
        //    //}
        //}

        float factor = 0.8f;
        if (ATR[0] > ATR[1]) OpenScore++;
        else OpenScore *= factor;

        if (ATR[0] < ATR[1]) CloseScore++;
        else CloseScore *= factor;

        //Thread.SpinWait(150);
        //Thread.SpinWait(50);
        if (OpenScore >= Parameters.Points!.OpenThreshold) { TryOpen(); }
        if (CloseScore >= Parameters.Points.CloseThreshold) { TryClose(); }

        var sl = Direction switch
        {
            LongAndShort.Long => Bars[0].Low - ATR[0],
            LongAndShort.Short => Bars[0].High + ATR[0],
            _ => throw new NotImplementedException(),
        };

        var tp = Direction switch
        {
            LongAndShort.Long => Bars[0].High + ATR[0],
            LongAndShort.Short => Bars[0].Low - ATR[0],
            _ => throw new NotImplementedException(),
        };


        Account.SetStopLosses(Symbol, Direction, sl, StopLossFlags.TightenOnly);
        Account.SetTakeProfits(Symbol, Direction, tp, StopLossFlags.Unspecified);
    }

    #endregion

}


