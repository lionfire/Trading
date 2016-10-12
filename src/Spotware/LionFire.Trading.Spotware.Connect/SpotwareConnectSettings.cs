using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect
{
    public class SpotwareConnectSettings
    {
        public string ClientPublicId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiHost { get; set; }
        public int? ApiPort { get; set; }

        public string AccessToken { get; set; }
        public long AccountId { get; set; }


    }
}
