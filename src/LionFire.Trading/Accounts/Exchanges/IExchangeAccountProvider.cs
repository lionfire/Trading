using System.Collections.Generic;

namespace LionFire.Trading;

public interface IExchangeAccountProvider
{
    string Key { get; }
    IAccount_Old GetAccount(string accountId = "default");

    IReadOnlyDictionary<string,IAccount_Old> Accounts { get; }
    IEnumerable<string> AccountIdsAvailable { get; }
}
