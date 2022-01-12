using System.Collections.Generic;

namespace LionFire.Trading
{
    public interface IExchangeAccountProvider
    {
        string Key { get; }
        IAccount GetAccount(string accountId = "default");

        IReadOnlyDictionary<string,IAccount> Accounts { get; }
        IEnumerable<string> AccountIdsAvailable { get; }
    }
}
