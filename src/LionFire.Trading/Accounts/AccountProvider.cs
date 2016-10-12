using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Applications
{
    public class AccountProvider : IAccountProvider
    {
        public IAccount GetAccount(string configName)
        {
            var split = configName.Split(':');
            if (split.Length < 1) throw new ArgumentException("Format: urischeme:<...>");

            return null;
        }
    }
}
