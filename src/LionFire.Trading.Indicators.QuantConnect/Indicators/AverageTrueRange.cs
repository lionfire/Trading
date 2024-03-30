using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LionFire.Trading;
using QuantConnect.Data.Market;

namespace LionFire.Trading.Indicators.QuantConnect_;

public abstract class QuantConnectIndicatorWrapper<TConcrete, TQuantConnectIndicator, TParameters, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, TParameters, TInput, TOutput>
      where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator<TConcrete, TParameters, TInput, TOutput>
{
    #region State

    public TQuantConnectIndicator WrappedIndicator { get; protected set; }

    #endregion

}

public class PAverageTrueRange
{
    public int Period { get; set; }
    public QuantConnect.Indicators.MovingAverageType MovingAverageType { get; set; } = QuantConnect.Indicators.MovingAverageType.Wilders;
}

public class AverageTrueRange : QuantConnectIndicatorWrapper<AverageTrueRange, global::QuantConnect.Indicators.AverageTrueRange, PAverageTrueRange, IKline, decimal>, IIndicator<AverageTrueRange, PAverageTrueRange, IKline, decimal>
{
    #region Static

    public static IndicatorCharacteristics Characteristics(PAverageTrueRange parameter)
    {
        return new IndicatorCharacteristics
        {
            Inputs = new List<IndicatorInputCharacteristics>
            {
                new IndicatorInputCharacteristics
                {
                    Name = "Source",
                    Type = typeof(IKline),
                }
            },
            Outputs = new List<IndicatorOutputCharacteristics>
            {
                new IndicatorOutputCharacteristics
                {
                    Name = "Average True Range",
                    Type = typeof(decimal),
                }
            },
        };
    }

    #endregion

    public override uint Lookback => (uint)Parameters.Period;
    PAverageTrueRange  Parameters;

    public static AverageTrueRange Create(PAverageTrueRange p) => new AverageTrueRange(p);
    public AverageTrueRange(PAverageTrueRange parameters)
    {
        Parameters = parameters;
        WrappedIndicator = new global::QuantConnect.Indicators.AverageTrueRange(parameters.Period, parameters.MovingAverageType);
    }

    public override void OnNext(IReadOnlyList<IKline> inputs)
    {
        foreach (var input in inputs)
        {
            var bar = new TradeBar(default, default, open: default /* UNUSED for ATR */, input.HighPrice, input.LowPrice, input.ClosePrice, default);

            WrappedIndicator.Update(bar);
            if (WrappedIndicator.IsReady && subject != null)
            {
                subject.OnNext(new List<decimal> { WrappedIndicator.Current.Price });
            }
        }
    }
    public override void Clear() { base.Clear(); WrappedIndicator.Reset(); }

}
