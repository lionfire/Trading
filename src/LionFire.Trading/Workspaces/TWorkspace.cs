using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Assets;
using LionFire.Trading.Workspaces.Screens;
using LionFire.States;
using System.ComponentModel;
using LionFire.Structures;
using System.Collections.ObjectModel;
using LionFire.Trading.Bots;
using LionFire.Validation;
using LionFire.Persistence;
using LionFire.UI.Windowing;
using System.Diagnostics;

namespace LionFire.Trading.Workspaces
{
    [Flags]
    public enum ControlSwitch
    {
        None = 0,
        All = 1 << 0,
        Connect = 1 << 1,
        Download = 1 << 2,
        Live = 1 << 3,
        Demo = 1 << 4,
        Scanners = 1 << 5,
        Paper = 1 << 6,
    }

    public static class TWorkspaceExtensions
    {
        public static ValidationContext Validate(this TWorkspace tw)
        {
            return ((object)tw).Validate()
                .NoDuplicateBots();
        }
        public static ValidationContext NoDuplicateBots(this ValidationContext ctx)
        {
            var tw = (TWorkspace)ctx.Object;

            foreach (var s in tw.Sessions)
            {
                //if(s.LiveBots!=null)s.LiveBots = new ObservableCollection<string>(s.LiveBots.Distinct());
                //if (s.DemoBots != null) s.DemoBots = new ObservableCollection<string>(s.DemoBots.Distinct());
                if (s.Bots != null) s.Bots = new ObservableCollection<IInstantiator>(s.Bots);
                //if (s.PaperBots != null) s.PaperBots = new ObservableCollection<string>(s.PaperBots.Distinct());
            }

            return ctx;
        }
    }


    [AssetPath("Workspaces")]
    public class TWorkspace : ITemplate<Workspace>, INotifyPropertyChanged, IChanged, IValidatable, INotifyOnSaving
    {
        #region Identity

        public Guid Guid { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        //string IAsset.AssetSubPath { get { return Name; } }

        #endregion

        #region Lifecycle

        public TWorkspace()
        {
        }

        #endregion

        #region Validation

        public ValidationContext Validate(object kind = null)
        {
            return TWorkspaceExtensions.Validate(this);
        }


        #endregion

        #region WindowSettings

        public WindowSettings WindowSettings
        {
            get
            {
                if (windowSettings == null)
                {
                    windowSettings = new WindowSettings();
                }
                return windowSettings;
            }
            set { windowSettings = value; }
        }
        private WindowSettings windowSettings;

        #endregion

        public bool IsAutoSaveEnabled
        {
            get
            {
                return isAutoSaveEnabled;
            }
            set
            {
                isAutoSaveEnabled = value;
                // FUTURE - use Instantiation
                //this.EnableAutoSave(isAutoSaveEnabled);
                OnPropertyChanged(nameof(IsAutoSaveEnabled));
                IsAutoSaveEnabledChanged?.Invoke();
            }
        }
        private bool isAutoSaveEnabled;
        public event Action IsAutoSaveEnabledChanged;

        public ObservableCollection<TWorkspaceItem> Items { get; set; }

        public ObservableCollection<string> Assemblies { get; set; }

        #region Workspace defaults

        /// <summary>
        /// FUTURE: Syntax: "* -ExcludedBotType" or "IncludedType1 IncludedType2";
        /// </summary>
        public ObservableCollection<string> BotTypes { get; set; }


        /// <summary>
        /// FUTURE: Syntax: "* -ExcludedSymbol" or "IncludedSymbol1 IncludedSymbol2";
        /// </summary>
        public string Symbols { get; set; }

        /// <summary>
        /// If null, all available symbols are included by default
        /// </summary>
        public ObservableCollection<string> SymbolsIncluded { get; set; }
        public ObservableCollection<string> SymbolsExcluded { get; set; }

        #endregion

        public TradeLimits WorkspaceTradeLimits { get; set; }
        public TradeLimits LiveAccountTradeLimits { get; set; }

        public ObservableCollection<string> LiveAccounts { get; set; }

        public ObservableCollection<string> DemoAccounts { get; set; }
        public ObservableCollection<string> ScannerAccounts { get; set; }

        public TradingOptions TradingOptions { get; set; }

        //public ObservableCollection<TSession> Sessions { get; set; }
        public ObservableCollection<TSession> Sessions
        {
            get { return sessions; }
            set
            {
                if (sessions != null) sessions.CollectionChanged -= CollectionChangedToChanged;
                sessions = value;
                if (sessions != null) sessions.CollectionChanged += CollectionChangedToChanged;
            }
        }
        private ObservableCollection<TSession> sessions;

        #region Global Allow switches

        #region AllowAny

        public bool AllowAny
        {
            get { return allowAny; }
            set
            {
                if (allowAny == value) return;
                allowAny = value;
                OnPropertyChanged(nameof(AllowAny));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowAny = true;

        #endregion

        #region AllowConnect

        public bool AllowConnect
        {
            get { return allowConnect; }
            set
            {
                if (allowConnect == value) return;
                allowConnect = value;

                OnPropertyChanged(nameof(AllowConnect));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowConnect = true;
        public event Action ControlSwitchChanged;

        // No INPC on this yet
        public ControlSwitch ControlSwitch
        {
            get
            {
                return
                   (AllowAny ? ControlSwitch.All : ControlSwitch.None)
                    & (AllowConnect ? ControlSwitch.Connect : ControlSwitch.None)
                    & (AllowLive ? ControlSwitch.Live : ControlSwitch.None)
                    & (AllowDemo ? ControlSwitch.Demo : ControlSwitch.None)
                    & (AllowScanners ? ControlSwitch.Scanners : ControlSwitch.None)
                    & (AllowPaper ? ControlSwitch.Paper : ControlSwitch.None)
                    ;
            }
        }

        #endregion

        #region AllowDownload

        public bool AllowDownload
        {
            get { return allowDownload; }
            set
            {
                if (allowDownload == value) return;
                allowDownload = value;
                OnPropertyChanged(nameof(AllowDownload));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowDownload = true;

        #endregion

        #region AllowLive


        public bool AllowLive
        {
            get { return allowLive; }
            set
            {
                if (allowLive == value) return;
                allowLive = value;
                OnPropertyChanged(nameof(AllowLive));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowLive;

        #endregion
        #region AllowDemo


        public bool AllowDemo
        {
            get { return allowDemo; }
            set
            {
                if (allowDemo == value) return;
                allowDemo = value;
                OnPropertyChanged(nameof(AllowDemo));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowDemo = true;

        #endregion

        #region AllowScanners


        public bool AllowScanners
        {
            get { return allowScanners; }
            set
            {
                if (allowScanners == value) return;
                allowScanners = value;
                OnPropertyChanged(nameof(AllowScanners));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowScanners = true;

        #endregion
        #region AllowPaper


        public bool AllowPaper
        {
            get { return allowPaper; }
            set
            {
                if (allowPaper == value) return;
                allowPaper = value;
                OnPropertyChanged(nameof(AllowPaper));
                ControlSwitchChanged?.Invoke();
            }
        }
        private bool allowPaper = true;

        #endregion

        #endregion

        #region AllowSubscribeToTicks

        public bool AllowSubscribeToTicks
        {
            get { return allowSubscribeToTicks; }
            set
            {
                if (allowSubscribeToTicks == value) return;
                allowSubscribeToTicks = value;
                OnPropertyChanged(nameof(AllowSubscribeToTicks));
            }
        }
        private bool allowSubscribeToTicks;

        #endregion

        
        
        #region Misc

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public event Action<object> Changed;

        private void RaiseChanged() => Changed?.Invoke(this);
        private void CollectionChangedToChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaiseChanged();
        }

        public void OnSaving(object persistenceContext = null)
        {
            Saving?.Invoke();
        }
        public event Action Saving;

        #endregion

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
                    //LiveAccounts = new ObservableCollection<string>
                    //{
                    //    "IC Markets.Live.Manual"
                    //},
                    DemoAccounts = new ObservableCollection<string>
                    {
                        "IC Markets.Demo"
                    },
                    ScannerAccounts = new ObservableCollection<string>
                    {
                        "IC Markets.Demo"
                    },


                    Sessions = new ObservableCollection<TSession>
                    {
                        //new TSession(BotMode.Live)
                        //{
                        //    AllowLiveBots = true,
                        //    LiveAccount = "IC Markets.Live.Manual",
                        //    LiveBots = new ObservableCollection<string>
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
                        //    DemoBots = new ObservableCollection<string>
                        //    {
                        //        "a1ayraigo5l0",
                        //    },
                        //},
                        new TSession(BotMode.Scanner)
                        {
                            DemoAccount = "IC Markets.Demo",
                            Bots = new ObservableCollection<IInstantiator>
                            {
                                new PBot {
                                Id = "a1ayraigo5l0",
                                }
                            },
                            //EnabledSymbols = new HashSet<string>  {
                            //    "XAUUSD",
                            //    "EURUSD",
                            //    "XAGUSD",
                            //},
                        },
                        //new TSession(BotMode.Paper)
                        //{
                        //    //PaperAccount = "IC Markets.Demo3",
                        //    PaperBots = new ObservableCollection<string>
                        //    {
                        //        "a1ayraigo5l0",
                        //    },
                        //},
                    },

                    Items = new ObservableCollection<TWorkspaceItem>
                    {
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            ViewModelType = "Log",
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            ViewModelType = "Symbols",
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            ViewModelType = "Backtesting",
                        },
                        new TWorkspaceItem
                        {
                            Session = "Scanner",
                            ViewModelType = "HistoricalData",
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
                            ViewModelType = "Bots",
                            State = new {
                                DisplayName = "Scanners",
                                //Mode = BotMode.Scanner,
                            },
                        },
                    },
                };
            }
        }


        #region SelectedWorkspaceItemId

        public string SelectedWorkspaceItemId
        {
            get { return selectedWorkspaceItemId; }
            set
            {
                if (selectedWorkspaceItemId == value) return;
                selectedWorkspaceItemId = value;
                OnPropertyChanged(nameof(SelectedWorkspaceItemId));
            }
        }
        private string selectedWorkspaceItemId;

        #endregion

        #endregion
        
    }

}
