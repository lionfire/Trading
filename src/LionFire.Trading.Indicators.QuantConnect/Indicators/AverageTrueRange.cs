using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;
using MAT = QuantConnect.Indicators.MovingAverageType;

namespace LionFire.Trading.Indicators.QuantConnect_;


// Input: IKline aspects: High, Low, Close
// TODO: Use HLC<TOutput> instead of IKline as the TInput
public class PAverageTrueRange<TPrice, TOutput> : IndicatorParameters<AverageTrueRange<TPrice, TOutput>, HLC<TPrice>, TOutput>

{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ATR({Period})";

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
      => [new InputSlot() {
                    Name = "Source",
                    ValueType = typeof(HLC<TPrice>),
                    Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
                    DefaultSource = 0,
                }];

    [JsonIgnore]
    public override IReadOnlyList<InputSlot> InputSlots => GetInputSlots();

    #endregion

    #region Parameters

    [TradingParameter(HardValueMin = 2,
        DefaultMin = 3, 
        DefaultMax = 100,
        ValueMax = 300, 
        HardValueMax = 65_536, // Arbitrary, can increase
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent  = 5.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 100,
        // TODO: Default to Fibonacci, with base of 0 (2 3 5 8 13...)
        OptimizerHints = OptimizationDistributionKind.Period, 
        SearchLogarithmExponent = 2.0)]
    public int Period { get; set; }

    [TradingParameter(OptimizePriority = -3, OptimizerHints = OptimizationDistributionKind.Category, DefaultValue = MAT.Wilders,
        HardValueMin = 0, // TEMP - shouldn't need for Enums.
        HardValueMax = 10,
        DefaultSearchSpaces = [
            (MAT[])[ // TODO: Find a nicer way to initialize these
                MAT.Simple,
                MAT.Exponential,
                MAT.Wilders,
                MAT.Kama,
                MAT.Alma,
                //MAT.Triangular,
                MAT.DoubleExponential,
                MAT.TripleExponential,
                MAT.Hull,
                MAT.LinearWeightedMovingAverage,
                MAT.T3
            ],
            (MAT[])[
                MAT.Simple,
                MAT.Exponential,
                MAT.Wilders,
                MAT.Kama,
                MAT.Hull,
                MAT.T3
            ],
            (MAT[])[
                MAT.Simple,
                MAT.Exponential,
                MAT.Wilders,
            ],
            (MAT[])[
                MAT.Exponential,
            ]
        ])]
    public MAT MovingAverageType { get; set; } = MAT.Wilders;

    #endregion

    #region Inputs

    public HLCReference<TOutput>? Bars { get; set; }
    // TODO: Consolidate Bars' HLCReference<TOutput> into SlotSource?
    [JournalIgnore]
    public SlotSource BarsSource { get; set; }  // Optional: will fall back to first input if not set or this property doesn't exist. 

    //public IReadOnlyList<SlotSource> ATRSources { get; set; }  // Hypothetical, for other situations // OLD - not sure what this was for

    #endregion

    // TODO: Move this to InputSignal?
    // TODO: Is there a better way to standardize this by interface or convention?
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;
}

// TODO: Use HLC<TOutput> instead of IKline as the TInput
public class AverageTrueRange<TPrice, TOutput> : QuantConnectIndicatorWrapper<AverageTrueRange<TPrice, TOutput>, global::QuantConnect.Indicators.AverageTrueRange, PAverageTrueRange<TPrice, TOutput>, HLC<TPrice>, TOutput>, IIndicator2<AverageTrueRange<TPrice, TOutput>, PAverageTrueRange<TPrice, TOutput>, HLC<TPrice>, TOutput>
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


    public static List<OutputSlot> Outputs(PAverageTrueRange<TPrice, TOutput> p)
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

    public readonly PAverageTrueRange<TPrice, TOutput> Parameters;

    #region Derived

    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    public static AverageTrueRange<TPrice, TOutput> Create(PAverageTrueRange<TPrice, TOutput> p) => new AverageTrueRange<TPrice, TOutput>(p);
    public AverageTrueRange(PAverageTrueRange<TPrice, TOutput> parameters) : base(new global::QuantConnect.Indicators.AverageTrueRange("ATR", parameters.Period, parameters.MovingAverageType))
    {
        Parameters = parameters;
    }

    #endregion



    #region State

    public override bool IsReady => throw new NotImplementedException();

    #endregion


#if truex
    #region Alternate style (bot style)

    #region Inputs (REVIEW - if this was like a bot)

    public IReadOnlyValuesWindow<IKline> Bars1 { get; set; }
    public IReadOnlyValuesWindow<IKline> Bars2 { get; set; }

    #endregion

    TOutput[]? output;
    int outputIndex = 0;
    int outputSkip = 0;

    #region Event Handling

    public override void OnBar()
    {
        var x = Bars2[0] - Bars1[0];
    }

    ProcessorMode Mode => ProcessorMode.Bar;

    #endregion
    
    #endregion
#else 
    ProcessorMode Mode => ProcessorMode.BatchInput;
#endif
    public enum ProcessorMode
    {
        Unspecified = 0,
        BatchInput = 1 << 0,
        Bar = 1 << 1,
        Tick = 1 << 2,
    }

    #region Event Handling

    #region State

    // Stub time and period values.  QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;
    TradeBar tradeBar = new TradeBar(time: DefaultEndTime,
                symbol: global::QuantConnect.Symbol.None,
                open: default /* UNUSED for ATR */,
                high: default,
                low: default,
                close: default,
                volume: default,
                period: period);

    #endregion


    // Process a Batch of Inputs
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            //tradeBar.EndTime = endTime;
            //tradeBar.High = Convert.ToDecimal(input.High);
            //tradeBar.Low = Convert.ToDecimal(input.Low);
            //tradeBar.Close= Convert.ToDecimal(input.Close);
            //WrappedIndicator.Update(tradeBar); // Can't reuse tradeBars since WrappedIndicator stores as is

            WrappedIndicator.Update(new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: default /* UNUSED for ATR */,
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
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
        else if (outputBuffer != null)
        {
            if (outputIndex >= outputBuffer.Length)
            {
                throw new InvalidOperationException(
                    $"Output buffer overflow: attempted to write at index {outputIndex} but buffer length is {outputBuffer.Length}. " +
                    "This indicates a mismatch between expected and actual bar counts.");
            }
            outputBuffer[outputIndex++] = value;
        }
    }

    #endregion

    #region Methods

    public override void Clear() { base.Clear(); WrappedIndicator.Reset(); }

    #endregion

}

