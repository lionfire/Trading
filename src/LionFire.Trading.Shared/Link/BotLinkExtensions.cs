using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Bots;
using LionFire.Trading.Link.Messages;
using Newtonsoft.Json;

namespace LionFire.Trading
{
    public enum ReleaseMode
    {
        None = 0,
        Prod,
        Beta,
        Alpha,
        Local,
    }

    public static class BotLinkConfig
    {

        public static string LinkUrl
        {
            get {
                if(defaultUplinkUrls.ContainsKey(Environment.MachineName))
                {
                    return defaultUplinkUrls[Environment.MachineName];
                }
                switch (ReleaseMode)
                {
                    case ReleaseMode.Prod:
                        return ProdLinkUrl;
                    case ReleaseMode.Beta:
                        return BetaLinkUrl;
                    case ReleaseMode.Alpha:
                        return AlphaLinkUrl;
                    case ReleaseMode.Local:
                        return LocalLinkUrl;
                    default:
                        break;
                }
                return AlphaLinkUrl;
            }
            set
            {
                specificLinkUrl = value;
                BotLinkExtensions.Reset();
            }
        }
        private static string specificLinkUrl = null;

        public const string AlphaLinkUrl = "https://alpha-link.firelynx.io/api";
        public const string BetaLinkUrl = "https://beta-link.firelynx.io/api";
        public const string ProdLinkUrl = "https://link.firelynx.io/api";
        public const string LocalLinkUrl = "http://localhost:9230/api";

#if DEBUG
#else
        //public static string linkUrl = "https://link.firelynx.io/api";
#endif

        static Dictionary<string, string> defaultUplinkUrls = new Dictionary<string, string>();

        static BotLinkConfig()
        {
            defaultUplinkUrls.Add("AHATEM", LocalLinkUrl);
        }

        public static ReleaseMode ReleaseMode { get; set; } = ReleaseMode.Alpha;

    }



    public static class BotLinkExtensions
    {
        public static HttpClient HC
        {
            get
            {
                if (_hc == null)
                {
                    InitLinkUrl();
                }
                return _hc;
            }
        }
        private static HttpClient _hc;
        public static void Reset()
        {
            _hc = null;
        }

        internal static void InitLinkUrl()
        {
            _hc = new HttpClient
            {
                BaseAddress = new Uri(BotLinkConfig.LinkUrl),
            };
        }

        public static async Task Send(this IBot bot, MBotInfo msg) => await _Send(bot, msg, "info");
        public static async Task Send(this IBot bot, MStatus msg) => await _Send(bot, msg, "status");

        private static async Task _Send(this IBot bot, object msg, string url)
        {
            try
            {
                var response = await HC.PostAsync("/api/bot/" + url,
                    new StringContent(Serialize(msg), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
            }
            catch(Exception ex)
            {
                LinkSendException?.Invoke(ex);
            }
        }
        public static event Action<Exception> LinkSendException;

        private static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private static string Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.None, JsonSerializerSettings);
            return json;
        }
    }
}

