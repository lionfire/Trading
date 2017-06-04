using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Triggers
{
    public class TPriceChangeAmountTrigger : TSingleMarketTriggerBase<TPriceChangeAmountTrigger>, ITemplate
    {

        public decimal ChangeAmount { get; set; }

        public TimeSpan TimeSpan { get; set; }
        public TimeSpanAnchor TimeSpanAnchor { get; set; }
        public DateTime ReferenceDate { get; set; }

        /// <summary>
        /// Used as day start reference point when TimeSpanAnchor is set to Day.
        /// </summary>
        public DateTime? DayStart { get; set; }
    }
}
