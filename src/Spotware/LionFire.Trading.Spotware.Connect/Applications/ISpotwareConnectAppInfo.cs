using LionFire.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect
{
    public interface ISpotwareConnectAppInfo
    {
        bool IsSandbox
        {
            get;
        }
        string ClientPublicId { get; }
        string ClientSecret { get; }
    }

    [AssetPath("Apis/SpotwareConnect")]
    public class SpotwareConnectAppInfo : ISpotwareConnectAppInfo
    {
        public bool IsSandbox { get; set; }
        public string ClientPublicId { get; set; }
        public string ClientSecret { get; set; }
    }

   
}
