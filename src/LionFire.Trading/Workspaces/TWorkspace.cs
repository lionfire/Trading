using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    public class TWorkspace : ITemplate<Workspace>
    {
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
                    TradingOptions = new TradingOptions()
                    {
                        AccountModes = AccountMode.Demo,
                        AutoConfig = true,
                    },
                    LiveAccounts = new List<string>
                    {
                        "IC Markets.Live.Manual"
                    },
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
                        new TSession("Default Scanners")
                        {
                            ScanAccount = "IC Markets.Demo3",
                            Scanners = new List<string>
                            {
                                "a1ayraigo5l0",
                            },
                        },
                        new TSession("Experimental Bots")
                        {
                            ScanAccount = "IC Markets.Demo3",
                        }
                    },
                };
            }
        }

        #endregion

    }
}
