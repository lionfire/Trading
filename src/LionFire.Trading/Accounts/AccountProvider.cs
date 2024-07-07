using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading;

public class AccountProvider : IAccountProvider
{
    public Dictionary<string, IExchangeAccountProvider> ExchangeAccountProviders { get; }

    public AccountProvider(IEnumerable<IExchangeAccountProvider> exchangeAccountProviders, ILogger<AccountProvider> logger)
    {
        ExchangeAccountProviders = exchangeAccountProviders.ToDictionary(p => p.Key);
        Logger = logger;
    }

    public IAccount? GetAccount(ExchangeId exchangeId, string accountName)
    {
        if (!exchangeId.HasValue || accountName == null) { return null; }

        if (ExchangeAccountProviders.TryGetValue(exchangeId.Id, out var exchangeAccountProvider))
        {
            return GetAccount(exchangeAccountProvider, accountName);
        }
      
        Logger.LogWarning($"No IExchangeAccountProvider registered for {exchangeId}");
        return null;
    }

    private IAccount GetAccount(IExchangeAccountProvider exchangeAccountProvider, string accountName)
        => exchangeAccountProvider.GetAccount(accountName);

    public IEnumerable<IAccount> Accounts => ExchangeAccountProviders.Values.SelectMany(eap => eap.Accounts.Values);

    public ILogger<AccountProvider> Logger { get; }

    //public IAccount GetAccount(string configName)
    //{
    //    var split = configName.Split(':');
    //    if (split.Length < 1) throw new ArgumentException("Format: urischeme:<...>");

    //    return null;
    //}
}
