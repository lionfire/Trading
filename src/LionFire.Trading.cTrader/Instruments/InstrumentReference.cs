using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class InstrumentReference
    {
        public string Instrument { get; set; }

        public static implicit operator InstrumentReference(string instrumentName)
        {
            return new InstrumentReference { Instrument = instrumentName };
        }
    }

}
