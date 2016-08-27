using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class Position 
    {
        public string Comment { get; set;  }
        public double Commissions { get; set;  }
        public double EntryPrice { get; set;  }
        public DateTime EntryTime { get; set;  }
        public double GrossProfit { get; set;  }
        public int Id { get; set;  }
        public string Label { get; set;  }
        public double NetProfit { get; set;  }
        public double Pips { get; set;  }
        public double Quantity { get; set;  }
        public double? StopLoss { get; set;  }
        public double Swap { get; set;  }
        public string SymbolCode { get; set;  }
        public double? TakeProfit { get; set;  }
        public TradeType TradeType { get; set;  }
        public long Volume { get; set;  }
    }
}
