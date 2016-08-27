using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots.Features
{
    public class StopLossTrailerConfig
    {
        public RangedNumber Input { get; set; }
        public RangedNumber StopLossLocation { get; set; }

        public Func<double, double> Function { get; set; }
    }
}
