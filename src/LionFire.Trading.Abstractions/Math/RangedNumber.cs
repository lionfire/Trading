using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class RangedNumber
    {
        public RangedNumber(double number, Unit unit, double startNumber = 0.0)
        {
            this.StartNumber = startNumber;
            this.Number = number;
            this.Unit = unit;
        }
        public Unit Unit { get; set; }
        public double Number { get; set; }
        public double StartNumber { get; set; }
    }

}
