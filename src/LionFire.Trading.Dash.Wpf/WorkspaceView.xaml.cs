//#define Live
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
using LionFire.Trading.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using LionFire.Parsing.String;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.DataGrid;
using Newtonsoft.Json;
using LionFire.Trading.Backtesting;
using Newtonsoft.Json.Linq;


namespace LionFire.Trading.Dash.Wpf
{
#if UNUSED
    
    public class AppVM : INotifyPropertyChanged
    {
#region NumberOfLiveBots

        public int NumberOfLiveBots
        {
            get { return numberOfLiveBots; }
            set
            {
                if (numberOfLiveBots == value) return;
                numberOfLiveBots = value;
                OnPropertyChanged(nameof(NumberOfLiveBots));
            }
        }
        private int numberOfLiveBots;

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
#endif

    public partial class WorkspaceView : Window, INotifyPropertyChanged
    {
        IAppHost app;

        
        public WorkspaceVM VM { get; set; } = new WorkspaceVM();

        List<Task> tasks = new List<Task>();
        public WorkspaceView()
        {
            TypeResolverTemp.Register();
            InitializeComponent();
            InitApp();
            tasks.Add(InitWorkspace());
            Workspace1Tabs.SelectionChanged += Workspace1Tabs_SelectionChanged;
            ResultsFilterBox.TextChanged += ResultsFilterBox_TextChanged;
            ResultsADFilterSlider.ValueChanged += ResultsADFilterSlider_ValueChanged;
            ResultsFilterBox.KeyDown += ResultsFilterBox_KeyDown;

            foreach (var b in SymbolFilterButtons.Children.OfType<ToggleButton>())
            {
                b.Checked += B_Checked;
                b.Unchecked += B_Checked;
            }
        }


        private void ResultsFilterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RefreshBacktestResults(true);
            }
        }


        private void TimeDelayUpdateResultsFilter(bool forceUpdate = false)
        {
            if (ResultsFilterBox_TextChanged_cts != null)
            {
                ResultsFilterBox_TextChanged_cts.Cancel();
            }

            ResultsFilterBox_TextChanged_cts = new CancellationTokenSource();


            Task.Factory.StartNew(() =>
            {
                refreshResults = DateTime.Now + TimeSpan.FromMilliseconds(400);
                ////while (refreshResults<DateTime.Now) {
                Thread.Sleep(400);
                //}
            }, ResultsFilterBox_TextChanged_cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled || ResultsFilterBox_TextChanged_cts.Token.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() => RefreshBacktestResults(forceUpdate));
            }
            );
        }

        private void B_Checked(object sender, RoutedEventArgs e)
        {
            TimeDelayUpdateResultsFilter(true);
        }

        private void ResultsADFilterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeDelayUpdateResultsFilter(true);
        }
        CancellationTokenSource ResultsFilterBox_TextChanged_cts;
        DateTime refreshResults;
        string lastResultsFilterText;
        private void ResultsFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeDelayUpdateResultsFilter();

        }

        private void Workspace1Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var header = (string)(Workspace1Tabs.SelectedItem as TabItem)?.Header;

            if (header == "Results")
            {
                if (ResultsGrid.ItemsSource == null)
                {
                    RefreshBacktestResults();
                }
            }
        }

        private void ResultToScanner_Click(object sender, RoutedEventArgs e)
        {
            AddScanner(ResultsGrid.SelectedItem as BacktestResultHandle);
            //Debug.WriteLine(e.OriginalSource);
            //Debug.WriteLine(e.Source);
            //Debug.WriteLine((e.Source as FrameworkElement).DataContext);
            //Debug.WriteLine(sender.GetType().FullName);
        }
        private void ResultToBot_Click(object sender, RoutedEventArgs e)
        {
            AddLiveBot(ResultsGrid.SelectedItem as BacktestResultHandle);
            //Debug.WriteLine((e.Source as FrameworkElement).DataContext);
            //Debug.WriteLine(sender.GetType().FullName);
        }

        public Type ResolveType(string typeName)
        {

            var type = Type.GetType(typeName);
            if (type != null) return type;
            typeName = typeName.Replace("LionFire.Trading.cTrader", "LionFire.Trading.Proprietary");
            type = Type.GetType(typeName);
            return type;
        }
        public void AddDemoBot(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Demo | BotMode.Scanner | BotMode.Paper, VM.Workspace.DefaultDemoAccount);
        }

        public void AddLiveBot(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Live | BotMode.Scanner | BotMode.Paper, VM.Workspace.DefaultLiveAccount);
        }
        public void AddScanner(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Scanner | BotMode.Paper, VM.Workspace.DefaultScannerAccount);
        }
        public void _AddBot(BacktestResultHandle handle, BotMode mode, IAccount account = null)
        {
            var sessionVM = VM.Sessions.First();

            if (account == null) { account = this.VM.Workspace.DefaultDemoAccount; }

            var backtestResult = handle.Object;

            var templateType = ResolveType(backtestResult.BotConfigType);
            //var templateType = Type.GetType(result.BotConfigType);
            if (templateType == null)
            {
                MessageBox.Show($"Failed to resolve type: {backtestResult.BotConfigType}");
                return;
            }

            var config = (ITemplate)((JObject)backtestResult.Config).ToObject(templateType);
            var bot = (IBot)config.Create();
            bot.Mode = BotMode.Scanner | BotMode.Paper;
            var botVM = new BotVM() { Bot = bot };
            botVM.State = SupervisorBotState.Scanner;

            if (bot.Mode.HasFlag(BotMode.Live))
            {
                throw new Exception("Not ready for live yet");
            }
            bot.Account = DefaultCTraderAccount;
            botVM.AddBacktestResult(backtestResult);
            

            //result.BotType
            if (mode.HasFlag(BotMode.Scanner))
            {
                VM.Scanners.Add(botVM);
            }
            else if (mode.HasFlag(BotMode.Demo))
            {
                VM.LiveBots.Add(botVM);
            }
            else if ( mode.HasFlag(BotMode.Demo))
            {
                VM.DemoBots.Add(botVM);
            }

            Task.Factory.StartNew(()=> bot.Start());
        }

        private void RefreshBacktestResults(bool forceRefresh = false)
        {
            if (!forceRefresh && lastResultsFilterText == ResultsFilterBox.Text) return;
            lastResultsFilterText = ResultsFilterBox.Text;
            var results = LoadResults(ResultsADFilterSlider.Value);

            DataGridCollectionView collectionView = new DataGridCollectionView(results);
            collectionView.SortDescriptions.Add(new SortDescription("AD", ListSortDirection.Descending));
            ResultsGrid.ItemsSource = collectionView;


        }

        private List<BacktestResultHandle> LoadResults(double minAD = double.NaN)
        {
            List<BacktestResultHandle> results = new List<BacktestResultHandle>();

            bool gotAD = false;

            var dir = @"c:\Trading\Results\"; // HARDPATH
            foreach (var path in Directory.GetFiles(dir))
            {
                var str = System.IO.Path.GetFileNameWithoutExtension(path);

                var filters = ResultsFilterBox.Text.Split(' ');
                bool failedFilter = false;
                foreach (var filter in filters)
                {
                    if (filter.StartsWith(">")
                        || filter.StartsWith("<")
                        || filter.StartsWith("=")
                        ) continue;

                    if (!str.Contains(filter))
                    {
                        failedFilter = true;
                        break;
                    }
                }
                if (failedFilter) continue;

                var handle = new BacktestResultHandle();
                handle.Path = path;
                handle.AssignFromString(str);


                foreach (var filter in filters)
                {
                    string unit = null;
                    object val;
                    object curVal;
                    PropertyInfo pi;
                    if (filter.StartsWith(">="))
                    {
                        var filterString = filter.Substring(2);
                        filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                        if (pi == null) break;
                        curVal = pi.GetValue(handle);

                    }
                    else if (filter.StartsWith("<="))
                    {
                        var filterString = filter.Substring(2);
                        filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                        if (pi == null) break;
                        curVal = pi.GetValue(handle);

                    }
                    else if (filter.StartsWith(">"))
                    {
                        var filterString = filter.Substring(1);
                        filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                        if (pi == null) break;
                        curVal = pi.GetValue(handle);
                        switch (pi.PropertyType.Name)
                        {
                            case "Double":
                                if (!((double)curVal > (double)val)) failedFilter = true;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else if (filter.StartsWith("<"))
                    {
                        var filterString = filter.Substring(1);
                        filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                        if (pi == null) break;
                        curVal = pi.GetValue(handle);
                        switch (pi.PropertyType.Name)
                        {
                            case "Double":
                                if (!((double)curVal < (double)val)) failedFilter = true;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else if (filter.StartsWith("="))
                    {
                        var filterString = filter.Substring(1);
                        filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                        if (pi == null) break;
                        curVal = pi.GetValue(handle);
                        switch (pi.PropertyType.Name)
                        {
                            //case "Double":
                            //    if (!((double)curVal == (double)val)) failedFilter = true;
                            //    break;
                            default:
                                if (curVal != val) failedFilter = true;
                                break;
                        }
                    }
                    if (unit == "ad")
                    {
                        gotAD = true;
                    }
                }
                if (!gotAD && !double.IsNaN(minAD))
                {
                    if (handle.AD < minAD) failedFilter = true;
                }

                foreach (var b in SymbolFilterButtons.Children.OfType<ToggleButton>())
                {
                    if (b.IsChecked == true)
                    {
                        var filter = b.Content as string;
                        if (filter.Length == 3)
                        {
                            if (!handle.Symbol.Contains(filter)) { failedFilter = true; break; }
                        }
                    }
                }
                if (failedFilter) continue;
                results.Add(handle);
            }

            return results;
        }




#region IsConnectedDesired

        public bool IsConnectedDesired
        {
            get { return isConnectedDesired; }
            set
            {
                if (isConnectedDesired == value) return;
                isConnectedDesired = value;
                OnPropertyChanged(nameof(IsConnectedDesired));
            }
        }
        private bool isConnectedDesired;

#endregion

        private void InitApp()
        {
            LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

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
                                ;

            StatusText = "Starting app";
            app.Run();
            StatusText = "App started";
        }

        private async Task InitWorkspace()
        {
            StatusText = "Creating workspace";

            VM.Workspace = TWorkspace.Default.Create();

            StatusText = "Starting workspace";

            await VM.Workspace.Start();

            StatusText = "Started workspace";

            var account = DefaultCTraderAccount;

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

                await Task.Factory.StartNew(() =>
                {
                    var symbols = new ObservableCollection<SymbolVM>();

                    foreach (var s in account.SymbolsAvailable)
                    {
                        var symbol = account.GetSymbol(s);
                        var vm = new SymbolVM(symbol, account);
                        symbols.Add(vm);
                    }
                    VM.Symbols = symbols;


                    foreach (var scannerId in VM.Workspace.Scanners)
                    {
                        var dir = @"C:\Trading\Results";
                        foreach (var file in Directory.GetFiles(dir, $"*id={scannerId}*.json"))
                        {
                            var json = File.ReadAllText(file);
                            var br = JsonConvert.DeserializeObject<BacktestResult>(json);
                            AddScanner(br);
                        }
                    }

                });
            }
            OnAccountStatusTextChanged();

        }

        protected IAccount DefaultCTraderAccount
        {
            get { return VM.Workspace.Accounts.OfType<CTraderAccount>().FirstOrDefault(); }
        }

        void OnAccountStatusTextChanged()
        {
            var account = DefaultCTraderAccount;
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