using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class PositionCloseDetails
    {
        public double EntryPrice { get; set; }
        public double Profit { get; set; }
        public double Swap { get; set; }
        public double Commission { get; set; }
        public long Balance { get; set; }
        //public int BalanceVersion { get; set; }
        public string Comment { get; set; }
        public double StopLossPrice { get; set; }
        public double TakeProfitPrice { get; set; }

        public double QuoteToDepositConversionRate { get; set; }
        public double ClosedVolume { get; set; }
        public double ProfitInPips { get; set; }
        public double Roi { get; set; }
        public double EquityBasedRoi { get; set; }
        public double Equity { get; set; }
    }
}
