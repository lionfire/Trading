using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading
{
    public partial class PositionEx
    {
        public PositionDouble Position{get;set;}

        public double EntryPrice => Position.EntryPrice;

        public double? StopLoss { get; set; }
        public double? TakeProfit { get; set; }
        public TradeType TradeType { get; set; }
        public Symbol Symbol { get; set; }

        public IAccount Account => Position.Account;
    }
}

