using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace LionFire.Trading.Indicators
{
    public abstract partial class IndicatorBase<TConfig> : MarketParticipant
    {



        protected override void OnStarting()
        {
            base.OnStarting();
            Init();
        }

        public Symbol Symbol { get; set; }

        // Nothing here yet
        public abstract void Calculate(int index);

        private void InitDataSeries()
        {
            foreach (var mi in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                mi.SetValue(this, new DoubleDataSeries());
            }
        }

    }
}
