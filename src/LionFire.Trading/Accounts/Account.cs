using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    

    public class LiveAccount : IAccount
    {

        IPositions IAccount.Positions { get { return Positions; } }
        public Positions Positions { get; set; } = new Positions();
        public double Equity { get; set; }
        public string Currency { get; set; }

        public double Balance {
            get {
                throw new NotImplementedException();
            }
        }

        public double StopOutLevel {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsDemo {
            get {
                throw new NotImplementedException();
            }
        }

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null)
        {
            throw new NotImplementedException();
        }
    }
}
