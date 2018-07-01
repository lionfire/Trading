using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Bots;
using LionFire.Trading.Link.Messages;
using Newtonsoft.Json;

namespace LionFire.Trading
{
    public static class BotLinkConfig
    {

        public static string LinkUrl
        {
            get { return linkUrl; }
            set
            {
                linkUrl = value;
                BotLinkExtensions.InitLinkUrl();
            }
        }
        public static string linkUrl = "http://localhost:5000/api";
#if DEBUG
#else
        //public static string linkUrl = "https://link.firelynx.io/api";
#endif

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
            var response = await HC.PostAsync("/api/bot/" + url,
                new StringContent(Serialize(msg), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }

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

