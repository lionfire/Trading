using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class SymbolInfo
    {
        public string Code { get; set;  }
        public int Digits { get; set;  }
        public long LotSize { get; set;  }
        public double PipSize { get; set;  }
        public double PointSize { get; set;  }
        public double Leverage { get; set;  }
        public double TickSize { get; set;  }
        public long VolumeMax { get; set;  }
        public long VolumeMin { get; set;  }
        public long VolumeStep { get; set;  }
    }
}
