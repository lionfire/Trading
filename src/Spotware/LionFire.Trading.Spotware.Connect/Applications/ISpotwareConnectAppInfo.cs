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

         string ApiHost { get; set; }

        string TradeApiHost { get; set; }
        int? TradeApiPort { get; set; }
    }

    [AssetPath("Apis/SpotwareConnect")]
    public class SpotwareConnectAppInfo : ISpotwareConnectAppInfo
    {
        public bool IsSandbox { get; set; }
        public string ClientPublicId { get; set; }
        public string ClientSecret { get; set; }

        public string ApiHost { get; set; }
        public string TradeApiHost { get; set; }
        public int? TradeApiPort { get; set; }

        public static int DefaultTradeApiPort = 5032;
        public static string DefaultTradeApiHost = "tradeapi.spotware.com";

        public static string DefaultApiHost = "api.spotware.com";
    }

   
}
