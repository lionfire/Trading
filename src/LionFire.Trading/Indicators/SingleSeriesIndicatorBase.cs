using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries
            where TConfig : IIndicatorConfig
    {

        #region Construction

        public SingleSeriesIndicatorBase() { }

        public SingleSeriesIndicatorBase(TConfig config) : base(config)
        {

        }

        #endregion

        partial void _InitPartial();


        protected override void OnInitializing()
        {
            base.OnInitializing();

            _InitPartial();

        }
    }

    //[Obsolete("Use SingleSeriesIndicatorBase")]
    //public abstract partial class SingleSymbolIndicatorBase : IndicatorBase
    //{
        
    //    public IMarketSeries MarketSeries { get; set; }

    //    public string Name {
    //        get {
    //            if (name == null)
    //            {
    //                name = this.GetType().Name.Replace("Indicator", "");
    //            }
    //            return name;
    //        }

    //    }
    //    private string name;
    //}
}
