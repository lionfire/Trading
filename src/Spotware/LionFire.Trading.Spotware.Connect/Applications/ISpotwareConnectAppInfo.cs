using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect
{
    public interface ISpotwareConnectAppInfo
    {
        string ClientPublicId { get; }
        string ClientSecret { get; }
    }
    public class SpotwareConnectAppInfo : ISpotwareConnectAppInfo
    {
        public string ClientPublicId { get; set; }
        public string ClientSecret { get; set; }
    }

   
}
