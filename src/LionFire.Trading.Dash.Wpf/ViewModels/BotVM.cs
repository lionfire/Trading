using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Bots;
using LionFire.Trading.Indicators;
using LionFire.Trading.Workspaces;
using LionFire.Trading.Backtesting;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace LionFire.Trading
{


    public class BotVM : System.ComponentModel.INotifyPropertyChanged
    {

        #region Relationships

        public BotVM Self { get { return this; } }


        //public IBot Bot { get; set; }

        #region Bot

        public IBot Bot
        {
            get { return bot; }
            set { bot = value;
                bot.BotPositionChanged += Bot_PositionChanged;

                var signalBot = SignalBot;
                if (signalBot != null)
                {
                    signalBot.Evaluated += OnEvaluated;
                }

            }
        }
        private void OnEvaluated()
        {
            OnPropertyChanged("Bot");
        }

        private void Bot_PositionChanged(PositionEvent obj)
        {
            var pos = obj.Position;
            UpdatePositionVM();
            
        }
        protected virtual void UpdatePositionVM()
        {
            foreach (var pos in bot.BotPositions)
            {
            }
        }

        private IBot bot;

        #endregion


        public ISignalBot SignalBot { get { return Bot as ISignalBot; } }
        public TBot TBot { get { return Bot?.Template; } }

        #endregion

        #region Identity

        public string Type { get { return Bot.GetType().Name; } }
        public string Id { get { return Bot.Template.Id; } }

        #endregion

        #region Parameters

        public string Symbol { get { return TBot?.Symbol; } }

        #endregion

        #region State

        // TODO: Get IBot.ExecutionState
        public SupervisorBotState State { get; set; }
        //<xcdg:Column FieldName = "State" />


        #region IsScanEnabled

        public bool IsScanEnabled
        {
            get { return isScanEnabled; }
            set
            {
                if (isScanEnabled == value) return;
                isScanEnabled = value;
                OnPropertyChanged(nameof(IsScanEnabled));
            }
        }
        private bool isScanEnabled;

        #endregion


        #region IsLiveEnabled

        public bool IsLiveEnabled
        {
            get { return isLiveEnabled; }
            set
            {
                if (isLiveEnabled == value) return;
                isLiveEnabled = value;
                OnPropertyChanged(nameof(IsLiveEnabled));
            }
        }
        private bool isLiveEnabled;

        #endregion


        #region IsDemoEnabled

        public bool IsDemoEnabled
        {
            get { return isDemoEnabled; }
            set
            {
                if (isDemoEnabled == value) return;
                isDemoEnabled = value;
                OnPropertyChanged(nameof(IsDemoEnabled));
            }
        }
        private bool isDemoEnabled;

        #endregion

        #endregion

        #region Construction

        public BotVM()
        {
            backtestResults.CollectionChanged += BacktestResults_CollectionChanged;
        }

        #endregion

        #region BacktestResults

        public ObservableCollection<BacktestResult> BacktestResults
        {
            get
            {
                if (backtestResults.Count == 0)
                {
                    LoadBacktestResults(); // TOOPTIMIZE
                }
                return backtestResults;
            }
        }
        private ObservableCollection<BacktestResult> backtestResults = new ObservableCollection<BacktestResult>();

        private void BacktestResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnBacktestResultsChanged();
        }
        private void OnBacktestResultsChanged()
        {
            best = null;
        }

        private void LoadBacktestResults()
        {
             var dir = @"C:\Trading\Results"; // HARDPATH
            if (backtestResults == null) { backtestResults = new ObservableCollection<BacktestResult>(); }
            backtestResults.Clear();
            foreach (var file in Directory.GetFiles(dir, $"*id={Bot.Template.Id}*.json"))
            {
                var json = File.ReadAllText(file);
                var br = JsonConvert.DeserializeObject<BacktestResult>(json);
                backtestResults.Add(br);
            }
        }

        #region Derived


        public BacktestResult BestBacktestResult
        {
            get
            {
                if (best == null && BacktestResults.Any())
                {
                    foreach (var br in BacktestResults)
                    {
                        if (best == null || best.AD < br.AD)
                        {
                            best = br;
                        }
                    }
                }
                return best;
            }
        }
        BacktestResult best = null;

        public void AddBacktestResult(BacktestResult result)
        {
            if (backtestResults.Count > 0)
            {
                if (backtestResults.Where(br => br.BacktestDate == result.BacktestDate).Any()) return; // Already added
                throw new NotImplementedException("Not implemented when backtestresults already contains results");
            }
            backtestResults.Add(result);
            OnBacktestResultsChanged();
            var best = BestBacktestResult;
        }

        public double AD
        {
            get
            {
                var best = BestBacktestResult;
                if (best == null) return double.NaN;
                return best.AD;
            }
        }

        public double TPM
        {
            get
            {
                var best = BestBacktestResult;
                if (best == null) return double.NaN;
                return best.TradesPerMonth;
            }
        }

        public double Days
        {
            get
            {
                var best = BestBacktestResult;
                if (best == null) return double.NaN;
                return best.Days;
            }
        }

        #endregion

        #endregion


        public ISignalIndicator Indicator { get { return SignalBot?.Indicator; } }


        public double OpenLong { get { return Indicator.OpenLongPoints.LastValue; } }
        public double CloseLong { get { return Indicator.CloseLongPoints.LastValue; } }

        #region LongPosition

        public Position LongPosition
        {
            get { return longPosition; }
            set
            {
                if (longPosition == value) return;
                longPosition = value;

                OnPropertyChanged(nameof(LongPosition));
            }
        }
        private Position longPosition;

        #endregion

        public double OpenShort { get { return Indicator.OpenShortPoints.LastValue; } }
        public double CloseShort { get { return Indicator.CloseShortPoints.LastValue; } }

        #region ShortPosition

        public Position ShortPosition
        {
            get { return shortPosition; }
            set
            {
                if (shortPosition == value) return;
                shortPosition = value;
                OnPropertyChanged(nameof(ShortPosition));
            }
        }
        private Position shortPosition;

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
