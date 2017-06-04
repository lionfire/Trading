using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using LionFire.Validation;
using LionFire.Triggers;

namespace LionFire.Trading.Triggers
{
    //public enum OptionFlags
    //{
    //    None = 0,
    //    Rearm = 1 << 0,
    //}

    
    public class TPriceTrigger : TSingleMarketTriggerBase<PriceTrigger>, ITemplate<PriceTrigger>
    {

        #region Construction

        public TPriceTrigger() { }
        public TPriceTrigger(string symbol, ComparisonOperator comparisonOperator, decimal price, TriggerOptions options = null)
        {
            this.Symbol = symbol;
            this.ComparisonOperator = comparisonOperator;
            this.Price = price;
            this.Options = options;
        }

        #endregion

        public ComparisonOperator ComparisonOperator { get; set; }


        public decimal Price { get; set; }

        [DefaultValue(PriceQuoteType.Bid)]
        public PriceQuoteType PriceQuoteType { get; set; }
    }
    

    
}

