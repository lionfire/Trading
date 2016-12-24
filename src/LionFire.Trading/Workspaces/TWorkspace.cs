using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Assets;
using LionFire.Trading.Workspaces.Screens;

namespace LionFire.Trading.Workspaces
{
    [AssetPath("Workspaces")]
    public class TWorkspace : ITemplate<Workspace>
    {
        #region Identity

        public Guid Guid { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        #endregion

        public List<TWorkspaceItem> Items { get; set; }

        public List<string> Assemblies { get; set; }

        #region Workspace defaults

        /// <summary>
        /// FUTURE: Syntax: "* -ExcludedBotType" or "IncludedType1 IncludedType2";
        /// </summary>
        public List<string> BotTypes { get; set; }


        /// <summary>
        /// FUTURE: Syntax: "* -ExcludedSymbol" or "IncludedSymbol1 IncludedSymbol2";
        /// </summary>
        public string Symbols { get; set; }

        /// <summary>
        /// If null, all available symbols are included by default
        /// </summary>
        public List<string> SymbolsIncluded { get; set; }
        public List<string> SymbolsExcluded { get; set; }

        #endregion

        public TradeLimits WorkspaceTradeLimits { get; set; }
        public TradeLimits LiveAccountTradeLimits { get; set; }

        public List<string> LiveAccounts { get; internal set; }

        public List<string> DemoAccounts { get; internal set; }
        public List<string> ScannerAccounts { get; internal set; }

        public TradingOptions TradingOptions { get; internal set; }

        public List<TSession> Sessions { get; set; }

        #region Static Default

        public static TWorkspace Default
        {
            get
            {
                return new TWorkspace()
                {
                    Name = "Default",
                    TradingOptions = new TradingOptions()
                    {
                        AccountModes = AccountMode.Demo,
                        AutoConfig = true,
                    },
                    //LiveAccounts = new List<string>
                    //{
                    //    "IC Markets.Live.Manual"
                    //},
                    DemoAccounts = new List<string>
                    {
                        "IC Markets.Demo3"
                    },
                    ScannerAccounts = new List<string>
                    {
                        "IC Markets.Demo3"
                    },


                    Sessions = new List<TSession>
                    {
                        //new TSession(BotMode.Live)
                        //{
                        //    AllowLiveBots = true,
                        //    LiveAccount = "IC Markets.Live.Manual",
                        //    LiveBots = new List<string>
                        //    {
                        //        "a1ayraigo5l0",
                        //    },
                        //},
                        //new TSession(BotMode.Demo | BotMode.Scanner, "Experimental Bots")
                        //{
                        //    DemoAccount = "IC Markets.Demo3",
                        //},
                        //new TSession(BotMode.Demo)
                        //{
                        //    DemoAccount = "IC Markets.Demo3",
                        //    DemoBots = new List<string>
                        //    {
                        //        "a1ayraigo5l0",
                        //    },
                        //},
                        new TSession(BotMode.Scanner)
                        {
                            DemoAccount = "IC Markets.Demo3",
                            Scanners = new List<string>
                            {
                                "a1ayraigo5l0",
                            },
                            EnabledSymbols = new HashSet<string>  {
                                "XAUUSD",
                                "EURUSD",
                                "XAGUSD",
                            },
                        },
                        //new TSession(BotMode.Paper)
                        //{
                        //    //PaperAccount = "IC Markets.Demo3",
                        //    PaperBots = new List<string>
                        //    {
                        //        "a1ayraigo5l0",
                        //    },
                        //},
                    },

                    Items = new List<TWorkspaceItem>
                    {
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            View = "Symbols",
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            View = "Backtesting",
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            View = "HistoricalData",
                            IsSelected = true,
                            State = new HistoricalDataScreenState
                            {
                                SelectedSymbolCode = "XAUUSD",
                                CacheMode = false,
                                SelectedTimeFrame = "h1",
                                //From = new DateTime(2010, 1, 1),
                                From = DateTime.UtcNow - TimeSpan.FromDays(2),
                            },
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            View = "Bots",
                            State = new {
                                DisplayName = "Scanners",
                            },
                        },
                    },
                };
            }
        }

        #endregion
    }

    public class TWorkspaceItem : ITemplate<WorkspaceItem>
    {
        #region Construction

        public TWorkspaceItem() { }
        public TWorkspaceItem(string type, string session, object state = null)
        {
            this.View = type;
            this.Session = session;
            this.State = state;
        }

        #endregion

        public string Session { get; set; }
        public string View { get; set; }
        public object State { get; set; }

        public bool IsSelected { get; set; }

    }

    public class WorkspaceItem : TemplateInstanceBase<TWorkspaceItem>
    {

    }

}
