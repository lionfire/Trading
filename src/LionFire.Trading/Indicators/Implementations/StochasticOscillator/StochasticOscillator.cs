using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TStochasticOscillator : TSingleSeriesIndicator
    {
        public override int Periods
        {
            get
            {
                return KPeriods + KSlowing + DPeriods; // REVIEW
            }
            set
            {
            }
        }
        public int KPeriods { get; set; } = 9;
        public int KSlowing { get; set; } = 3;
        public int DPeriods { get; set; } = 9;
        public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;
    }


    public class StochasticOscillator : SingleSeriesIndicatorBase<TStochasticOscillator>
    {

        #region Outputs

        public DataSeries PercentD { get; private set; } = new DataSeries();
        public DataSeries PercentK { get; private set; } = new DataSeries();

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return PercentD;
                yield return PercentK;
            }
        }

        #endregion

        #region Relationships

        public override IEnumerable<IAccountParticipant> Children
        {
            get
            {
                yield return kDonc;
            }
        }
        DonchianChannel kDonc;

        #endregion

        #region Construction

        public StochasticOscillator(TStochasticOscillator config) : base(config)
        {
        }

        protected override void OnInitializing()
        {
            kDonc = EffectiveIndicators.DonchianChannel(Template.KPeriods);
            base.OnInitializing();
        }

        #endregion


        public override Task CalculateIndex(int index)
        {

            /*
             * 
             * % K = (Current Close - Lowest Low)/ (Highest High - Lowest Low) *100
             * % D = 3 day SMA of % K
             * 
             * Lowest Low = lowest low for the look-back period
             * Highest High = highest high for the look-back period
             * % K is multiplied by 100 to move the decimal point two places
             * 
             */

            var k = ((DataSeries.LastValue - kDonc.Bottom.LastValue) / (kDonc.Top.LastValue - kDonc.Bottom.LastValue)) * 100;
            for (int i = Template.KSlowing - 1; i > 0; i--)
            {
                k += PercentK.Last(i);
            }
            k /= Template.KSlowing;
            PercentK[index] = k;

            // TODO: create averager indicator for D%

            var d = 0.0;
            for (int i = Template.DPeriods; i > 0; i--)
            {
                d += PercentK.Last(i);
            }
            d /= Template.DPeriods;
            PercentD[index] = d;
            return Task.CompletedTask;
        }
    }
}
