using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;

namespace LionFire.Trading.Indicators.QuantConnect_;

#error NEXT: 
// Consider unifying Indicators with Bots somehow, by having a common resolution mechanism, while still keeping the possibility of Indicators not having to own their own sources.
// Maybe Indicators are exactly like bots, and a Bot harness (BotsController) executes them as well


#error NEXT: Maybe?
public interface ISlotsInfo
{
    static IReadOnlyList<InputSlot> InputSlots { get; }
}

// Input: IKline aspects: High, Low, Close
// TODO: Use HLC<TOutput> instead of IKline as the TInput
public class PAverageTrueRange<TOutput> : IndicatorParameters<AverageTrueRange<TOutput>, IKline, TOutput>
{
    #region Identity

    public override string Key => $"ATR({Period})";

    #endregion

    #region Type Info

    //public override IReadOnlyList<InputSlot> InputSlots => [
    //    //InputSlot.BarMultiAspect<TOutput>( DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close)
    //    ];
    public static IReadOnlyList<InputSlot> InputSlots()
      => [new InputSlot() {
                    Name = "Source",
                    ValueType = typeof(IKline<TOutput>),
                    Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
                    DefaultSource = 0,
                }];

    #endregion

    #region Parameters

    public int Period { get; set; }
    public QuantConnect.Indicators.MovingAverageType MovingAverageType { get; set; } = QuantConnect.Indicators.MovingAverageType.Wilders;

    #endregion

    #region Inputs

    public HLCReference<TOutput>? Bars { get; set; }

    #endregion

    // TODO: Move this to InputSignal?
    // TODO: Is there a better way to standardize this by interface or convention?
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;
}

// TODO: Use HLC<TOutput> instead of IKline as the TInput
public class AverageTrueRange<TOutput> : QuantConnectIndicatorWrapper<AverageTrueRange<TOutput>, global::QuantConnect.Indicators.AverageTrueRange, PAverageTrueRange<TOutput>, IKline, TOutput>, IIndicator2<AverageTrueRange<TOutput>, PAverageTrueRange<TOutput>, IKline, TOutput>
{
    #region Static

    
    //public static List<InputSlot> InputSlots()
    //    => [new () {
    //                Name = "Source",
    //                Type = typeof(IKline),
    //            }];
    public static IReadOnlyList<OutputSlot> Outputs()
            => [new () {
                     Name = "Average True Range",
                    ValueType = typeof(TOutput),
                }];


    public static List<OutputSlot> Outputs(PAverageTrueRange<TOutput> p)
            => [new () {
                     Name = "Average True Range",
                    ValueType = typeof(TOutput),
                }];
    //public static IOComponent Characteristics(PAverageTrueRange parameter)
    //{
    //    return new IOComponent
    //    {
    //        InputSignals = new List<InputSlot>
    //        {
    //            new InputSlot
    //            {
    //                Name = "Source",
    //                Type = typeof(IKline),
    //            }
    //        },
    //        Outputs = new List<OutputSlot>
    //        {
    //            new OutputSlot
    //            {
    //                Name = "Average True Range",
    //                Type = typeof(TOutput),
    //            }
    //        },
    //    };
    //}

    #endregion

    #region Parameters

    public readonly PAverageTrueRange<TOutput> Parameters;

    #region Derived

    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    public static AverageTrueRange<TOutput> Create(PAverageTrueRange<TOutput> p) => new AverageTrueRange<TOutput>(p);
    public AverageTrueRange(PAverageTrueRange<TOutput> parameters) : base(new global::QuantConnect.Indicators.AverageTrueRange(parameters.Period, parameters.MovingAverageType))
    {
        Parameters = parameters;
    }

    #endregion

    //#region Inputs (REVIEW - if this was like a bot)

    //public IReadOnlyValuesWindow<IKline> Bars { get; set; }
    
    //#endregion

    #region State

    public override bool IsReady => throw new NotImplementedException();

    #endregion

    #region Event Handling


    // Process a Batch of Inputs
    public override void OnNext(IReadOnlyList<IKline> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {

        DateTime endTime = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        // Stub time and period values.  QuantConnect checks the symbol ID and increasing end times.
        TimeSpan period = new TimeSpan(0, 1, 0);

        foreach (var input in inputs)
        {
            WrappedIndicator.Update(new TradeBar(
                time: endTime, 
                symbol: QuantConnect.Symbol.None, 
                open: default /* UNUSED for ATR */, 
                input.HighPrice, 
                input.LowPrice, 
                input.ClosePrice, 
                volume: default, 
                period: period));

            endTime += period;

            if (WrappedIndicator.IsReady && subject != null)
            {
                subject.OnNext(new List<TOutput> { ConvertToOutput(WrappedIndicator.Current.Price) });
            }
            OnNext_PopulateOutput(ConvertToOutput(WrappedIndicator.Current.Price), output, ref outputIndex, ref outputSkip);
        }
    }
    private static void OnNext_PopulateOutput(TOutput value, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) { outputSkip--; }
        else if (outputBuffer != null) outputBuffer[outputIndex++] = value;
    }

    #endregion

    #region Methods

    public override void Clear() { base.Clear(); WrappedIndicator.Reset(); }

    #endregion

}

