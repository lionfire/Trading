using LionFire.Trading.Indicators.QuantConnect_;
using QuantConnect.Securities;
using LionFire.Trading.ValueWindows;
using LionFire.Trading.Indicators.Harnesses;
using System.Collections.Concurrent;
using QuantConnect.Indicators;
using System.Diagnostics;
using System.Reflection;

namespace LionFire.Trading.Automation.Bots;

public class PAtrBot<TValue> : PStandardBot2<PAtrBot<TValue>, TValue>
{

    #region Static

    public override Type MaterializedType => typeof(AtrBot<TValue>);

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

    const int Lookback = 1;
    //public PAtrBot() { }
    public PAtrBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, uint period, QuantConnect.Indicators.MovingAverageType movingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders) : base(exchangeSymbolTimeFrame)
    {
        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = (int)period,
            Lookback = Lookback,
            MovingAverageType = movingAverageType,
            //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        };

        // REVIEW - does InputLookbacks make sense?
        // ENH - automate setting this somehow
        //InputLookbacks = [(int)period + Lookback];
        InputLookbacks = [0, Lookback];

        InitFromDefault();
    }
    

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(ATR, nameof(ATR));
        ArgumentNullException.ThrowIfNull(Points, nameof(Points));
        ArgumentNullException.ThrowIfNull(Unidirectional, nameof(Unidirectional));
    }
}

[Bot(Direction = BotDirection.Unidirectional)]
public class AtrBot<TValue> : StandardBot2<PAtrBot<TValue>, TValue>
{
    #region Inputs

    public IReadOnlyValuesWindow<double> ATR { get; set; } = null!;

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

    public int OpenScore { get; set; } = 0;
    public int CloseScore { get; set; } = 0;

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

        if (ATR[0] > ATR[1]) OpenScore++;
        if (ATR[0] < ATR[1]) CloseScore++;

        //Thread.SpinWait(150);
        //Thread.SpinWait(50);
        //if (OpenScore >= Parameters.Points!.OpenThreshold) { TryOpen(); }
        //if (CloseScore >= Parameters.Points.CloseThreshold) { TryClose(); }

        //long s;
    }

    #endregion

}
