using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Instruments
{
    
    public class PMultiInstrumentIndicator
    {
        public List<InstrumentReference> Instruments { get; set; }
    }

    public class PInstrumentIndicator
    {

        public InstrumentReference Instrument { get; set; }

    }

    //public class PRSIIndicator
}
