using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Structures;

namespace LionFire.Trading.Spotware.Connect.Applications
{
    public class SpotwareConnectMarketProvider : IKeyedRO<string>
    {
        #region Constants

        public const string Scheme = "ctrader";

        string IKeyedRO<string>.Key { get { return Scheme; } }

        #endregion

        public IAccount GetMarket(string configName)
        {
            var split = configName.Split(':');
            if (split.Length < 1) throw new ArgumentException("Format: urischeme:<...>");
            if (split[0].ToLowerInvariant() != "ctrader") throw new ArgumentException("Only accepts urls with scheme 'ctrader'");

#if NET462
            var market = new CTraderAccount();
            //market.ConfigName = configName;
            return market;
#else
            throw new NotImplementedException("Only implemented for .NET Framework");
#endif
        }
    }

}
