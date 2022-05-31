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

        public IAccount GetAccount(ExchangeId exchangeId, string accountName)
        {
            if (exchangeId == null || accountName == null) { return null; }

            if (ExchangeAccountProviders.TryGetValue(exchangeId.Id, out var exchangeAccountProvider))
            {
                return GetAccount(exchangeAccountProvider, accountName);
            }
            else
            {
                throw new ArgumentException($"No IExchangeAccountProvider registered for {exchangeId}");
            }
        }

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
