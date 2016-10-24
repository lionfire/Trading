using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class AccountBase
    {
        public PositionStats PositionStats { get; protected set; }


        #region Logger

        public ILogger  Logger {
            get { return logger; }
        }
        protected ILogger logger;

        #endregion


    }

    public class LiveAccount : AccountBase, IAccount
    {

        IPositions IAccount.Positions { get { return Positions; } }
        public Positions Positions { get; set; } = new Positions();

        IPendingOrders IAccount.PendingOrders { get { return PendingOrders; } }
        public PendingOrders PendingOrders { get; set; } = new PendingOrders();

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

        public TradeResult ClosePosition(Position position)
        {
            throw new NotImplementedException();
        }

        public TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit)
        {
            throw new NotImplementedException();
        }

        public GetFitnessArgs GetFitnessArgs()
        {
            throw new NotImplementedException();
        }
    }
}
