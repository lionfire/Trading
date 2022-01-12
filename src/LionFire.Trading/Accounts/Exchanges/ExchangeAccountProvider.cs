using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LionFire.Trading
{
    public abstract class ExchangeAccountProvider<AccountType> : IExchangeAccountProvider
        where AccountType : class, IAccount
    {
        public abstract string Key { get; }

        protected IConfiguration Configuration { get; }
        protected IServiceProvider ServiceProvider { get; }

        public IConfiguration GetAccountsSection() => Configuration.GetSection(Key);

        public IConfiguration GetAccountSection(string accountId = "default") => GetAccountsSection().GetSection(accountId);

        public ExchangeAccountProvider(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public virtual IAccount GetAccount(string accountId = "default")
        {
            lock (_accountsLock)
            {
                if (Accounts.ContainsKey(accountId)) { return Accounts[accountId]; }
                else
                {
                    var account = ActivatorUtilities.CreateInstance<AccountType>(ServiceProvider, accountId, GetAccountSection(accountId));
                    accounts.Add(accountId, account);
                    return account;
                }
            }
        }

        public virtual IEnumerable<string> AccountIdsAvailable
        {
            get
            {
                var exchangeSection = Configuration.GetSection(Key);
                if (exchangeSection != null)
                {
                    foreach (var section in exchangeSection.GetChildren())
                    {
                        yield return section.Key;
                    }
                }
                yield break;
            }
        }

        public IReadOnlyDictionary<string, IAccount> Accounts => accounts;
        private Dictionary<string, IAccount> accounts = new();
        private object _accountsLock = new();
    }
}
