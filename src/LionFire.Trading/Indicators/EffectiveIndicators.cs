using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    // For compatibility with cAlgo, and for convenience
    public partial class EffectiveIndicators
    {
        IndicatorBase owner;
        public EffectiveIndicators(IndicatorBase owner)
        {
            this.owner = owner;
        }

        public IMovingAverageIndicator MovingAverage(MovingAverageType movingAverageType, int periods, BarComponent indicatorBarComponent = BarComponent.Close, DataSeries indicatorBarSource = null)
        {
            switch (movingAverageType)
            {
                case MovingAverageType.Simple:
                    return new SimpleMovingAverage(new TSimpleMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.Exponential:
                    return new ExponentialMovingAverage(new TExponentialMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.TimeSeries:
                    return new TimeSeriesMovingAverage(new TTimeSeriesMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.Triangular:
                    return new TriangleMovingAverage(new TTriangleMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.VIDYA:
                    return new VidyaMovingAverage(new TVidyaMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.Weighted:
                    return new WeightedMovingAverage(new TWeightedMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                case MovingAverageType.WilderSmoothing:
                    return new WilderSmoothingMovingAverage(new TWilderSmoothingMovingAverage
                    {
                        TimeFrame = owner.TimeFrame,
                        Symbol = owner.Symbol.Code,
                        Periods = periods,
                        IndicatorBarComponent = indicatorBarComponent,
                        IndicatorBarSource = indicatorBarSource,
                    });
                default:
                    throw new NotImplementedException("Only SimpleMovingAverage currently supported");
            }            
        }

        //public BollingerBands BollingerBands(DataSeries series, int periods, double standardDeviation, MovingAverageType movingAverageType)  MOVED
        //{
            
        //    return new BollingerBands(new TBollingerBands
        //    {
        //        TimeFrame = owner.TimeFrame,
        //        Symbol = owner.Symbol.Code,
        //        Periods = periods,
        //        StandardDev = standardDeviation,
        //        MovingAverageType = movingAverageType,
        //        IndicatorBarSource = series,
        //    });
        //}

        public RelativeStrengthIndex RelativeStrengthIndex(DataSeries series, int periods)
        {
            return new RelativeStrengthIndex(new TRelativeStrengthIndex
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
                IndicatorBarSource = series,
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
