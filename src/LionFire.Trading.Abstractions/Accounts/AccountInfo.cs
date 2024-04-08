using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class AccountInfo
    {
        public string? Exchange { get; set;  }
        public string? ExchangeArea { get; set;  }
        public string? Currency { get; set;  }
        public bool IsLive { get; set;  }
        
        public int AccountNumber { get; set;  }
        public double PreciseLeverage { get; set;  }


        public double CommissionPerMillion { get; set;  }
    }
}
