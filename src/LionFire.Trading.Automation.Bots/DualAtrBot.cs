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

public class PDualAtrBot<TValue> : PStandardBot2<PDualAtrBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{

    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof(DualAtrBot<TValue>);

    //public static IReadOnlyList<InputSlot> InputSlots() => GetInputSlots();

    //public static IReadOnlyList<InputSlot> InputSlots()
    //  => [new InputSlot() {
    //                Name = "SlowATR",
    //                Type = typeof(AverageTrueRange),
    //            }];

    #endregion


    [PSignal]
    public PAverageTrueRange<double, TValue>? SlowATR { get; set; }

    [PSignal]
    public PAverageTrueRange<double, TValue>? FastATR { get; set; }

    /// <summary>
    /// Convention: match parameter name
    /// Can be get only, derived on other parameters.
    /// - if false, the SlowATR ITimeValueSeries{TValue} won't receive any values.
    /// </summary>
    //public bool UseDualAtr { get; set; } = true; 

    public PUnidirectionalBot? Unidirectional { get; set; }
    public PPointsBot? Points { get; set; }
    //public static PPointsBot StandardDefaults { get; set; } = new PPointsBot
    //{
    //    //OpenThreshold
    //};

    [JsonIgnore]
    const int Lookback = 1;

    //public PDualAtrBot() { }
    public PDualAtrBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, uint slowPeriod, uint fastPeriod, QuantConnect.Indicators.MovingAverageType movingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders) : base(exchangeSymbolTimeFrame)
    {
        SlowATR = new PAverageTrueRange<double, TValue>
        {
            Period = (int)slowPeriod,
            Lookback = Lookback,
            MovingAverageType = movingAverageType,
            //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        };

        FastATR = new PAverageTrueRange<double, TValue>
        {
            Period = (int)fastPeriod,
            Lookback = Lookback,
            MovingAverageType = movingAverageType,
            //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        };

        // REVIEW - does InputLookbacks make sense?
        // ENH - automate setting this somehow
        //InputLookbacks = [(int)slowPeriod + Lookback];
        InputLookbacks = [0, Lookback];

        Init();
    }


    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(SlowATR, nameof(SlowATR));
        ArgumentNullException.ThrowIfNull(FastATR, nameof(FastATR));
        ArgumentNullException.ThrowIfNull(Points, nameof(Points));
        ArgumentNullException.ThrowIfNull(Unidirectional, nameof(Unidirectional));
    }
}

[Bot(Direction = BotDirection.Unidirectional)]
public class DualAtrBot<TValue> : StandardBot2<PDualAtrBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    #region Inputs

    [Signal(0)]
    public IReadOnlyValuesWindow<TValue> SlowATR { get; set; } = null!;
    [Signal(1)]
    public IReadOnlyValuesWindow<TValue> FastATR { get; set; } = null!;

    //public IReadOnlyValuesWindow<HLC<double>> Bars { get; set; } = null!;

    #region OLD

    //public IReadOnlyList<IInputSignal> Inputs
    //{
    //    get
    //    {
    //        // TInputSlots with specifics on lookbacks
    //        return [
    //            //new InputSignal<decimal>() {
    //            //    Name = "SlowATR",
    //            //    Type = typeof(AverageTrueRange),
    //            //    Lookback = IndicatorParameters.SlowATR.Period,
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

    //public DualAtrBot()
    //{
    //    // TODO: Live Indicator harness if live
    //    //var eATR = new BufferingIndicatorHarness<AverageTrueRange<TValue>, PAverageTrueRange<double. TValue>, IKline, TValue>(serviceProvider, new IndicatorHarnessOptions<PAverageTrueRange<TValue>>(parameters.SlowATR!)
    //    //{
    //    //    Inputs = parameters.Inputs == null ? [] : [parameters.Inputs],
    //    //    DefaultTimeFrame = parameters.DefaultTimeFrame,
    //    //});

    //    //eATR.GetWindow(OutputExecutionOptions.Memory);

    //    //SlowATR = eATR.Memory;
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
        //    if (SlowATR.Size > 0) {
        //        Debug.WriteLine($"#{barIndex} {this.GetType().Name}.OnBar SlowATR: {SlowATR[0]}, bars available: {SlowATR.Size}"); }
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
        if (FastATR[0] > FastATR[1] && FastATR[0] > SlowATR[0]) OpenScore++;
        else OpenScore *= factor;

        if (FastATR[0] < FastATR[1] || FastATR[0] <= SlowATR[0]) CloseScore++;
        else CloseScore *= factor;

        //Thread.SpinWait(150);
        //Thread.SpinWait(50);
        if (OpenScore >= Parameters.Points!.OpenThreshold) { TryOpen(); }
        if (CloseScore >= Parameters.Points.CloseThreshold) { TryClose(); }

        var sl = Direction switch
        {
            LongAndShort.Long => Bars[0].Low - FastATR[0],
            LongAndShort.Short => Bars[0].High + FastATR[0],
            _ => throw new NotImplementedException(),
        };

        var tp = Direction switch
        {
            LongAndShort.Long => Bars[0].High + FastATR[0],
            LongAndShort.Short => Bars[0].Low - FastATR[0],
            _ => throw new NotImplementedException(),
        };


        Account.SetStopLosses(Symbol, Direction, sl, StopLossFlags.TightenOnly);
        Account.SetTakeProfits(Symbol, Direction, tp, StopLossFlags.Unspecified);
    }

    #endregion

}


