using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using LionFire.Validation;

namespace LionFire.Trading.Triggers
{
   
    public class TPriceTrigger : TSingleMarketTriggerBase, ITemplate<PriceTrigger>
    {

        #region Construction

        public TPriceTrigger() { }
        public TPriceTrigger(string symbol, ComparisonOperator comparisonOperator, decimal price)
        {
            this.Symbol = symbol;
            this.ComparisonOperator = comparisonOperator;
            this.Price = price;
        }

        #endregion

        public ComparisonOperator ComparisonOperator { get; set; }


        public decimal Price { get; set; }

        [DefaultValue(PriceQuoteType.Bid)]
        public PriceQuoteType PriceQuoteType { get; set; }
    }
    

    
}
}
