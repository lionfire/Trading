using LionFire.Applications.Hosting;
using LionFire.Trading.cTrader.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class cTraderRedisApplicationHostExtensions
    {
        public static void AddBrokerAccount(this IAppHost app)
        {
            var brokerAccount = new CAlgoRedisBot();
            brokerAccount.Config = new TCAlgoRedisBot
            {
                AccountMode = AccountMode.Demo,
                AccountId = "3235730",
                BrokerName = "IC Markets",
            };
            app.Add(brokerAccount);
        }
    }
}
