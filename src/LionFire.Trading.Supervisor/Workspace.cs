using LionFire.Assets;
using LionFire.Execution;
using LionFire.Templating;
using LionFire.Trading.Bots;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Spotware.Connect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Supervising
{

    public class TypeResolverTemp
    {
        public static Type GetTemplateType(string type)
        {
            if (type == "LionTrender") { return typeof(TLionTrender); }
            return null;
        }
        public static Type GetType(string type)
        {
            if (type == "LionTrender") { return typeof(LionTrender); }
            return null;
        }

        public static IAccount CreateAccount(string name)
        {
            var tAccount = name.Load<TCTraderAccount>();
            if (tAccount == null) return null;
            var account = tAccount.Create();
            return account;
        }
    }

    public class Workspace : ITemplateInstance<TWorkspace>
    {
        #region Relationships

        public TWorkspace Template { get; set; }
        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (TWorkspace)value; } }

        #endregion

        #region State

        public ObservableCollection<IAccount> LiveAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> DemoAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> Accounts { get; private set; } = new ObservableCollection<IAccount>();

        public ObservableCollection<WorkspaceBot> Bots { get; private set; } = new ObservableCollection<WorkspaceBot>();
        public ObservableCollection<PriceAlert> PriceAlerts { get; private set; } = new ObservableCollection<PriceAlert>();

        #region Derived

        public IAccount DefaultLiveAccount
        {
            get { return Accounts.Where(a => !a.IsDemo).FirstOrDefault(); }
        }
        public IAccount DefaultDemoAccount
        {
            get { return Accounts.Where(a => a.IsDemo).FirstOrDefault(); }
        }
        public IAccount DefaultScannerAccount
        {
            get { return DefaultLiveAccount ?? DefaultDemoAccount; }
        }

        #endregion

        private void ResetState()
        {
            LiveAccounts.Clear();
            DemoAccounts.Clear();
            Accounts.Clear();
            Bots.Clear();
            PriceAlerts.Clear();
        }

        #endregion

        #region Construction and Initialization

        public Workspace()
        {
        }


        public Task Start()
        {
            ResetState();

            if ((Template.TradingOptions.AccountModes & AccountMode.Live) == AccountMode.Live)
            {
                foreach (var accountId in Template.LiveAccounts)
                {
                    var account = TypeResolverTemp.CreateAccount(accountId);
                    LiveAccounts.Add(account);
                    Accounts.Add(account);
                }
            }

            if ((Template.TradingOptions.AccountModes & AccountMode.Demo) == AccountMode.Demo)
            {
                foreach (var accountId in Template.DemoAccounts)
                {
                    var account = TypeResolverTemp.CreateAccount(accountId);
                    DemoAccounts.Add(account);
                    Accounts.Add(account);
                }
            }

            foreach (var account in Accounts.OfType<CTraderAccount>())
            {
                account.IsCommandLineEnabled = false; // TEMP
            }
            

            foreach (var botId in Template.Scanners)
            {
                var botType = "LionTrender";
                
                var tBot = new TWorkspaceBot(botType, botId);
                var bot = tBot.Create();

                bot.Bot = bot.Template.Id.Load<TLionTrender>().Create();

                bot.Bot.Mode = BotMode.Scanner;

                Bots.Add(bot);

                if (DefaultScannerAccount as IAccount != null)
                {
                    bot.Bot.Account = DefaultScannerAccount;
                }
            }

            foreach (var account in Accounts.OfType<IStartable>())
            {
                account.Start().Wait();
            }

            foreach (var bot in Bots)
            {
                bot.Bot.Start().Wait();
            }

            return Task.CompletedTask;
        }

        #endregion


        public List<Type> BotTypes { get; set; } = new List<Type>();



    }

}
