using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace LionFire.Trading.Indicators
{
    public abstract partial class IndicatorBase : MarketParticipant
    {

        
        #region Construction and Init

        public IndicatorBase()
        {
            _effectiveIndicators = new EffectiveIndicators(this);
        }

        #endregion

        public Symbol Symbol { get; set; }

        public TimeFrame TimeFrame {
            get;  set;
        }

        public MarketData MarketData {
            get {
                return Account?.MarketData;
            }
        }

        public EffectiveIndicators EffectiveIndicators {
            get {
                return _effectiveIndicators;
            }
        }
        private EffectiveIndicators _effectiveIndicators;
    }

    public abstract partial class IndicatorBase<TConfig> : IndicatorBase
        where TConfig : ITIndicator, new()
    {

        #region Initialization (LionFire specific)
        

        //private void InitDataSeries()
        //{
        //    foreach (var mi in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        //    {
        //        mi.SetValue(this, new DoubleDataSeries());
        //    }
        //}

        partial void _InitPartial()
        {
            var subs = new List<MarketDataSubscription>();
            if (Symbol == null)
            {
                throw new ArgumentNullException("Symbol");
            }
            if (TimeFrame == null)
            {
                throw new ArgumentNullException("TimeFrame");
            }
            //subs.Add(new MarketDataSubscription(Symbol.Code, TimeFrame.Name));
            //this.DesiredSubscriptions = subs;

            InitializeOutputs();
        }

        partial void OnInitialized_()
        {
            AttachChildren();
        }

        protected virtual void AttachChildren()
        {
            foreach (var pi in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(_pi => typeof(IMarketParticipant).IsAssignableFrom(_pi.PropertyType)))
            {
                var indicator = pi.GetValue(this) as IMarketParticipant;

                indicator.Account = Account;
                //this.Market.Add(indicator);
                //indicator.Init();
            }
        }

        protected ArgumentNullException ConfigMissingException(string paramName) { return new ArgumentNullException("Config missing and a required parameter was not manually set: " + paramName); }

        #endregion

        #region (Public) Methods

        // Nothing here yet
        public abstract void Calculate(int index);
        
        #endregion

        


    }
}
