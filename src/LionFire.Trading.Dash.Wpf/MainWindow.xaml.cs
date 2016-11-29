#define Live
using LionFire.Applications.Hosting;
using LionFire.Assets;
using LionFire.Execution;
using LionFire.Extensions.Logging;
using LionFire.Templating;
using LionFire.Trading.Applications;
using LionFire.Trading.Bots;
using LionFire.Trading.Dash.Wpf;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Spotware.Connect;
using LionFire.Trading.Supervising;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LionFire.Trading.Dash.Wpf
{
    public class BotVM
    {
        public IBot Bot { get; set; }

        public string Type { get { return Bot.GetType().Name; } }
        public string Id { get { return Bot.Template.Id; } }

        public SupervisorBotState State { get; set; }
    }

    public class WorkspaceVM : INotifyPropertyChanged
    {
        public Workspace Workspace { get; set; }

        public ObservableCollection<BotVM> Bots { get; set; }


        #region Symbols

        public ObservableCollection<SymbolVM> Symbols
        {
            get { return symbols; }
            set
            {
                if (symbols == value) return;
                symbols = value;
                OnPropertyChanged(nameof(Symbols));
            }
        }
        private ObservableCollection<SymbolVM> symbols;

        #endregion


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    public class SymbolVM : INotifyPropertyChanged
    {
        public Symbol Symbol { get; private set; }
        public IAccount Account { get; private set; }

        public SymbolVM(Symbol symbol, IAccount account)
        {
            this.Symbol = symbol;
            this.Name = symbol.Code;
            this.Account = account;
        }
        public string Name { get; set; }


        #region Subscribed

        public bool Subscribed
        {
            get { return subscribed; }
            set { subscribed = value;
                if (subscribed)
                {
                }
                else
                {

                }
            }
        }
        private bool subscribed;

        #endregion



        #region Bid

        public double Bid
        {
            get { return bid; }
            set
            {
                if (bid == value) return;
                bid = value;
                OnPropertyChanged(nameof(Bid));
            }
        }
        private double bid;

        #endregion

        #region Ask

        public double Ask
        {
            get { return ask; }
            set
            {
                if (ask == value) return;
                ask = value;
                OnPropertyChanged(nameof(Ask));
            }
        }
        private double ask;

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IAppHost app;


        public WorkspaceVM VM { get; set; } = new WorkspaceVM();

        

        public MainWindow()
        {
            InitializeComponent();
            InitWorkspace();
        }

        private void InitWorkspace()
        {
            //try
            //{
            LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

            //var tWorkspace = new TWorkspace()
            //{                
            //};
            //tWorkspace.Assemblies = new List<string> { "LionFire.Trading.Proprietary" };

            StatusText = "Initializing";

            app = new AppHost()

            #region Bootstrap
                                .AddJsonAssetProvider(@"c:\Trading")
                                .Bootstrap()
            #endregion

            #region Logging
                                                    .ConfigureServices(sc => sc.AddLogging())
                                                    .AddInit(a => a
                                                            .ServiceProvider.GetService<ILoggerFactory>()
                                                            .AddNLog()
                                                        //.AddConsole()
                                                        )
            #endregion

                                //.AddSpotwareConnectClient("LionFire.Trading.Sandbox")
                                .AddSpotwareConnectClient("LionFire.Trading.App")
#if Live
                                                    .AddTrading(TradingOptions.Auto, AccountMode.Live)
                                                    .Add<TCTraderAccount,CTraderAccount>("IC Markets.Live.Manual", a => a.IsCommandLineEnabled = false )
#else
                                                    .AddTrading(TradingOptions.Auto, AccountMode.Demo)
                                                    .Add<TCTraderAccount>("IC Markets.Demo3")
#endif
                                ;

            StatusText = "Starting app";

            app.Run();

            StatusText = "Creating workspace";
            VM.Workspace = TWorkspace.Default.Create();
            StatusText = "Starting workspace";
            VM.Workspace.Start();
            
            var account = app.Components.OfType<CTraderAccount>().FirstOrDefault();
            if (account != null)
            {
                account.StatusTextChanged += OnAccountStatusTextChanged;
                StatusText = account.StatusText;
            }
            else
            {
                StatusText = "No account configured";
            }

            if (account != null)
            {
                account.StatusTextChanged += OnAccountStatusTextChanged;
                StatusText = account.StatusText;

                var symbols = new ObservableCollection<SymbolVM>();

                foreach (var s in account.SymbolsAvailable)
                {
                    var symbol = account.GetSymbol(s);
                    var vm = new SymbolVM(symbol, account);
                    symbols.Add(vm);
                }
                VM.Symbols = symbols;
            }
            OnAccountStatusTextChanged();


        }

        void OnAccountStatusTextChanged()
        {
            var account = app.Components.OfType<CTraderAccount>().FirstOrDefault();
            if (account != null)
            {
                StatusText = account.StatusText;
            }
            else
            {
                StatusText = "No account configured";
            }
        }

        #region StatusText

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }
        private string statusText;

        #endregion


        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion

    }
}