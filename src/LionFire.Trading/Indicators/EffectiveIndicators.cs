using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    // For compatibility with cAlgo, and for convenience
    public class EffectiveIndicators
    {
        IndicatorBase owner;
        public EffectiveIndicators(IndicatorBase owner)
        {
            this.owner = owner;
        }

        public IMovingAverageIndicator MovingAverage(MovingAverageType movingAverageType, int periods, BarComponent indicatorBarSource = BarComponent.Close)
        {
            switch (movingAverageType)
            {
                case MovingAverageType.Simple:
                    return new SimpleMovingAverage(new TSimpleMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarSource = indicatorBarSource,
                    });
                //case MovingAverageType.Exponential:
                //    break;
                //case MovingAverageType.Wilder:
                //    break;
                //case MovingAverageType.TimeSeries:
                //    break;
                //case MovingAverageType.Triangular:
                //    break;
                //case MovingAverageType.VIDYA:
                //    break;
                //case MovingAverageType.Weighted:
                //    break;
                //case MovingAverageType.WilderSmoothing:
                //    break;
                default:
                    throw new NotImplementedException("Only SimpleMovingAverage currently supported");
            }

            
        }

        public BollingerBands BollingerBands(BarComponent indicatorBarSource, int periods, double standardDeviation, MovingAverageType movingAverageType)
        {
            
            return new BollingerBands(new TBollingerBands
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
                StandardDev = standardDeviation,
                MovingAverageType = movingAverageType,
                IndicatorBarSource = indicatorBarSource,
            });
        }

        public RelativeStrengthIndex RelativeStrengthIndex(BarComponent indicatorBarSource, int periods)
        {
            return new RelativeStrengthIndex(new TRelativeStrengthIndex
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
                IndicatorBarSource = indicatorBarSource,
            });
        }

        public StochasticOscillator StochasticOscillator(int kPeriods, int kSlowing, int dPeriods, MovingAverageType movingAverageType)
        {
            return new StochasticOscillator(new TStochasticOscillator
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                KPeriods = kPeriods,
                KSlowing = kSlowing,
                DPeriods = dPeriods,
                MovingAverageType = movingAverageType,
            });
        }

        public DonchianChannel DonchianChannel(int periods)
        {
            return new DonchianChannel(new TDonchianChannel
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
            });
        }
    }

    
}
