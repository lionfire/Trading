using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading
{

    public class AccountProvider : IAccountProvider
    {
        public Dictionary<string, IExchangeAccountProvider> ExchangeAccountProviders { get; }

        public AccountProvider(IEnumerable<IExchangeAccountProvider> exchangeAccountProviders)
        {
            ExchangeAccountProviders = exchangeAccountProviders.ToDictionary(p => p.Key);
        }

        public IAccount GetAccount(string exchangeId, string accountName) 
            => (exchangeId == null || accountName == null) ? null 
            : (ExchangeAccountProviders.TryGetValue(exchangeId, out var exchangeAccountProvider)
                ? GetAccount(exchangeAccountProvider, accountName)
                : throw new ArgumentException($"No IExchangeAccountProvider registered for {exchangeId}"));

        private IAccount GetAccount(IExchangeAccountProvider exchangeAccountProvider, string accountName) 
            => exchangeAccountProvider.GetAccount(accountName);

        public IEnumerable<IAccount> Accounts => ExchangeAccountProviders.Values.SelectMany(eap => eap.Accounts.Values);

        //public IAccount GetAccount(string configName)
        //{
        //    var split = configName.Split(':');
        //    if (split.Length < 1) throw new ArgumentException("Format: urischeme:<...>");

        //    return null;
        //}
    }
}
