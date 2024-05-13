
using LionFire.Assets;
using LionFire.Trading.HistoricalData.Retrieval;
using QuantConnect.Data.Market;
using System;

namespace LionFire.Trading.Indicators.QuantConnect_;

public class PAverageTrueRange<TOutput> : IndicatorParameters<AverageTrueRange<TOutput>, IKline, TOutput>
{
    public int Period { get; set; }
    public QuantConnect.Indicators.MovingAverageType MovingAverageType { get; set; } = QuantConnect.Indicators.MovingAverageType.Wilders;

    //public required InputSignal<TOutput> Source { get; set; }

    // TODO: Move this to InputSignal?
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;
}

public class AverageTrueRange<TOutput> : QuantConnectIndicatorWrapper<AverageTrueRange<TOutput>, global::QuantConnect.Indicators.AverageTrueRange, PAverageTrueRange<TOutput>, IKline, TOutput>, IIndicator2<AverageTrueRange<TOutput>, PAverageTrueRange<TOutput>, IKline, TOutput>
{
    #region Static

    public static IReadOnlyList<InputSlot> InputSlots()
      => [new InputSlot() {
                    Name = "Source",
                    Type = typeof(IKline<TOutput>),
                //Lookback = Period, // Doesn't belong here
                    Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
                }];
    //public static List<InputSlot> InputSlots()
    //    => [new () {
    //                Name = "Source",
    //                Type = typeof(IKline),
    //            }];
    public static IReadOnlyList<OutputSlot> Outputs()
            => [new () {
                     Name = "Average True Range",
                    Type = typeof(TOutput),
                }];


    public static List<OutputSlot> Outputs(PAverageTrueRange<TOutput> p)
            => [new () {
                     Name = "Average True Range",
                    Type = typeof(TOutput),
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

    public override int MaxLookback => Parameters.Period;
    PAverageTrueRange<TOutput> Parameters;

    public static AverageTrueRange<TOutput> Create(PAverageTrueRange<TOutput> p) => new AverageTrueRange<TOutput>(p);
    public AverageTrueRange(PAverageTrueRange<TOutput> parameters) : base(new global::QuantConnect.Indicators.AverageTrueRange(parameters.Period, parameters.MovingAverageType))
    {
        Parameters = parameters;
    }

    public override bool IsReady => throw new NotImplementedException();
    public override void OnNext(IReadOnlyList<IKline> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {

        DateTime endTime = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        // Stub time and period values.  QuantConnect checks the symbol ID and increasing end times.
        TimeSpan period = new TimeSpan(0, 1, 0);

        foreach (var input in inputs)
        {
            var bar = new TradeBar(time: endTime, symbol: QuantConnect.Symbol.None, open: default /* UNUSED for ATR */, input.HighPrice, input.LowPrice, input.ClosePrice, volume: default, period: period);

            WrappedIndicator.Update(bar);
            endTime += period;
            if (WrappedIndicator.IsReady && subject != null)
            {
                subject.OnNext(new List<TOutput> { ConvertToOutput(WrappedIndicator.Current.Price) });
            }
            OnNext_PopulateOutput(ConvertToOutput(WrappedIndicator.Current.Price), output, ref outputIndex, ref outputSkip);
        }
    }

    public override void Clear() { base.Clear(); WrappedIndicator.Reset(); }

    public static void OnNext_PopulateOutput(TOutput value, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) { outputSkip--; }
        else if (outputBuffer != null) outputBuffer[outputIndex++] = value;
    }

}

