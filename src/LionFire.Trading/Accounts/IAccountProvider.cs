using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IAccountProvider
    {
        /// <param name="accountId">Format: {ExchangeCode}:{AccountName}</param>
        /// <returns>The IAccount, or null if none registered by that Id</returns>
        IAccount GetAccount(string exchangeId, string accountId = "default");

        Dictionary<string, IExchangeAccountProvider> ExchangeAccountProviders { get; }

        IEnumerable<IAccount> Accounts { get; }
    }

    
}
