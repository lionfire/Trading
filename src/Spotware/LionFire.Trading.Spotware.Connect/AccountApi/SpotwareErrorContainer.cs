using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.AccountApi
{
    public class SpotwareErrorContainer
    {
        public SpotwareError error { get; set; }
    }
    public class SpotwareError
    {
        public string errorCode { get; set; }
        public string description { get; set; }
    }
}
