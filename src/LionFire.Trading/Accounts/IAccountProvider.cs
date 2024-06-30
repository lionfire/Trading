using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading;

// TODO: Rename to IAccountProviderService
// REFACTOR: Also see IAccountProvider2 which is simpler, and reconcile: one simple one that can be used in a Bot environment, and a more deluxe interface for a UI / live bot environment.

public interface IAccountProvider
{
    /// <param name="accountId">Format: {ExchangeCode}:{AccountName}</param>
    /// <returns>The IAccount, or null if none registered by that Id</returns>
    IAccount GetAccount(ExchangeId exchangeId, string accountId = "default");

    Dictionary<string, IExchangeAccountProvider> ExchangeAccountProviders { get; }

    IEnumerable<IAccount> Accounts { get; }
}
