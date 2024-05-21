using LionFire.Trading.Indicators.QuantConnect_;
using QuantConnect.Securities;
using LionFire.Trading.ValueWindows;
using LionFire.Trading.Indicators.Harnesses;
using System.Collections.Concurrent;
using QuantConnect.Indicators;

namespace LionFire.Trading.Automation.Bots;

public static class BotParametersTypeInfo
{
    //public static IEnumerable<object> GetPIndicators(IPBot2 pBot)
    //{
    //}

    //ConcurrentDictionary<Type, IEnumerable<>>
}

public class PAtrBot<TValue> : PSymbolBarsBot2<PAtrBot<TValue>>
{
    #region Static

    //public static IReadOnlyList<InputSlot> InputSlots() => GetInputSlots();
      
    //public static IReadOnlyList<InputSlot> InputSlots()
    //  => [new InputSlot() {
    //                Name = "ATR",
    //                Type = typeof(AverageTrueRange),
    //            }];

    #endregion

    public PAverageTrueRange<TValue>? ATR { get; set; }

    public PUnidirectionalBot? Unidirectional { get; set; }
    public PPointsBot? Points { get; set; }
    //public static PPointsBot StandardDefaults { get; set; } = new PPointsBot
    //{
    //    //OpenThreshold
    //};

    public PAtrBot() { }
    public PAtrBot(uint period)
    {
        ATR = new PAverageTrueRange<TValue>
        {
            Period = (int)period,
            Memory = 2,
        };
        InputLookbacks = [(int)period];
    }
    public override Type InstanceType => typeof(AtrBot<TValue>);

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(ATR, nameof(ATR));
        ArgumentNullException.ThrowIfNull(Points, nameof(Points));
        ArgumentNullException.ThrowIfNull(Unidirectional, nameof(Unidirectional));
    }
}

//#if TODO
[Bot(Direction = BotDirection.Unidirectional)]
public class AtrBot<TValue> : StandardBot2<PAtrBot<TValue>>
{
    #region Inputs

    public IReadOnlyList<IInputSignal> Inputs
    {
        get
        {
            // TInputSlots with specifics on lookbacks
            return [
                //new InputSignal<decimal>() {
                //    Name = "ATR",
                //    Type = typeof(AverageTrueRange),
                //    Lookback = IndicatorParameters.ATR.Period,
                //    Phase = 0,
                //    Source = IndicatorParameters.Input,
                //}
                ];
        }
    }

    public override IReadOnlyList<IInputSignal> InputSignals { get; } = new List<IInputSignal>(); // TODO

    #endregion

    //private AverageTrueRange ATR { get; init; }
    public IReadOnlyValuesWindow<double> ATR { get; set; }

    public OutputComponentOptions OutputExecutionOptions { get; } = new OutputComponentOptions
    {
        Memory = 2,
    };

    public AtrBot()
    {
        // TODO: Live Indicator harness if live
        //var eATR = new BufferingIndicatorHarness<AverageTrueRange<TValue>, PAverageTrueRange<TValue>, IKline, TValue>(serviceProvider, new IndicatorHarnessOptions<PAverageTrueRange<TValue>>(parameters.ATR!)
        //{
        //    Inputs = parameters.Inputs == null ? [] : [parameters.Inputs],
        //    TimeFrame = parameters.TimeFrame,
        //});

        //eATR.GetWindow(OutputExecutionOptions.Memory);

        //ATR = eATR.Memory;
    }

    #region State

    public int OpenScore { get; set; } = 0;
    public int CloseScore { get; set; } = 0;

    #endregion

    public override void OnBar(IKline kline)
    {
        if (ATR[0] > ATR[1]) OpenScore++;
        if (ATR[0] < ATR[1]) CloseScore++;

        if (OpenScore >= Parameters.Points!.OpenThreshold) { Open(); }
        if (CloseScore >= Parameters.Points.CloseThreshold) { Close(); }

        //long s;
    }

}
//#endif