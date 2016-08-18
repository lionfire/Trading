using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Sensors
{
    public abstract class PValueSensorBase
    {

        public ValueCondition ValueCondition { get; set; }

        public decimal Value { get; set; }
    }
}
