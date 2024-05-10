
using LionFire.Assets;
using LionFire.Trading.HistoricalData.Retrieval;
using QuantConnect.Data.Market;
using System;

namespace LionFire.Trading.Indicators.QuantConnect_;

public class PAverageTrueRange<TValue> : IndicatorParameters<AverageTrueRange>
{
    public int Period { get; set; }
    public QuantConnect.Indicators.MovingAverageType MovingAverageType { get; set; } = QuantConnect.Indicators.MovingAverageType.Wilders;

    //public required InputSignal<TValue> Source { get; set; }

    // TODO: Move this to InputSignal?
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;
}

// TODO: replace decimal with generic type
public class AverageTrueRange : QuantConnectIndicatorWrapper<AverageTrueRange, global::QuantConnect.Indicators.AverageTrueRange, PAverageTrueRange<decimal>, IKline, decimal>, IIndicator2<AverageTrueRange, PAverageTrueRange<decimal>, IKline, decimal>
{
    #region Static

    public static IReadOnlyList<InputSlot> InputSlots()
      => [new InputSlot() {
                    Name = "Source",
                    Type = typeof(IKline<decimal>),
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
                    Type = typeof(decimal),
                }];


    public static List<OutputSlot> Outputs(PAverageTrueRange<decimal> p)
            => [new () {
                     Name = "Average True Range",
                    Type = typeof(decimal),
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
    //                Type = typeof(decimal),
    //            }
    //        },
    //    };
    //}

    #endregion

    public override uint MaxLookback => (uint)Parameters.Period;
    PAverageTrueRange<decimal> Parameters;

    public static AverageTrueRange Create(PAverageTrueRange<decimal> p) => new AverageTrueRange(p);
    public AverageTrueRange(PAverageTrueRange<decimal> parameters)
    {
        Parameters = parameters;
        WrappedIndicator = new global::QuantConnect.Indicators.AverageTrueRange(parameters.Period, parameters.MovingAverageType);
    }

    public override bool IsReady => throw new NotImplementedException();
    public override void OnNext(IReadOnlyList<IKline> inputs)
    {
        // Stub time and period values.  QuantConnect checks the symbol ID and increasing end times.
        DateTime endTime = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan period = new TimeSpan(0, 1, 0);

        foreach (var input in inputs)
        {
            var bar = new TradeBar(time: endTime, symbol: QuantConnect.Symbol.None, open: default /* UNUSED for ATR */, input.HighPrice, input.LowPrice, input.ClosePrice, volume: default, period: period);


            WrappedIndicator.Update(bar);
            endTime += period;
            if (WrappedIndicator.IsReady && subject != null)
            {
                subject.OnNext(new List<decimal> { WrappedIndicator.Current.Price });
            }
        }
    }
    public override void Clear() { base.Clear(); WrappedIndicator.Reset(); }

}
