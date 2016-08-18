using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Sensors
{


    public class PPriceSensor : PValueSensorBase
    {
        public InstrumentReference Instrument { get; set; }

        #region Construction

        public PPriceSensor() { }

        public PPriceSensor(string instrument, ValueCondition greaterOrEqual, decimal price)
        {
            this.Instrument = instrument;
            this.ValueCondition = greaterOrEqual;
            this.Value = price;
        }

        #endregion
    }

    

    public class PriceNotification : TemplateInstanceBase<>
    {
        LionFire.Templating.IInstantiation

    }

    public class Samples
    {
        public List<PPriceSensor> Sample {
            get {

                return new List<PPriceSensor>
                {
                    new PPriceNotification {
                        Instrument = "USDJPY",
                        ValueCondition = ValueCondition.GreaterOrEqual,
                        Price = 102.2001m,
                    },
                    new PPriceSensor("USDJPY", ValueCondition.GreaterOrEqual, 104m),
                };
            }
        }
    }

}
