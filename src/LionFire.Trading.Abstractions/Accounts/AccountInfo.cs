using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class AccountInfo
    {
        public string BrokerName { get; set;  }
        public string Currency { get; set;  }
        public bool IsLive { get; set;  }
        
        public int AccountNumber { get; set;  }
        public double Leverage { get; set;  }


        public double CommissionPerMillion { get; set;  }
    }
}
