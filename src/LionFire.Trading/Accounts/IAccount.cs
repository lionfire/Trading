using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public interface IAccount
    {
        double Equity { get;  }
        double Balance { get;  }
        string Currency { get; }

        double StopOutLevel { get; }

        bool IsDemo { get;  }

        TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = null, double? takeProfitPips = null, double? marketRangePips = null, string comment = null);

        
        IPositions Positions { get; }

        IPendingOrders PendingOrders { get; }

        TradeResult ClosePosition(Position position);
        TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);

        GetFitnessArgs GetFitnessArgs();

        PositionStats PositionStats { get; }
    }


}
