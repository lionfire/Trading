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
using LionFire.Execution;
using LionFire.Avalon;

namespace LionFire.Trading
{


    public class BotViewModel : System.ComponentModel.INotifyPropertyChanged, IViewModel
    {

        #region Relationships

        public BotViewModel Self { get { return this; } }
        public BotViewModel Self2 { get { return this; } }

        #region Bot
        
        public IBot Bot
        {
            get { return bot; }
            set
            {
                bot = value;
                bot.BotPositionChanged += Bot_PositionChanged;

                var signalBot = SignalBot;
                if (signalBot != null)
                {
                    signalBot.Evaluated += OnEvaluated;
                }

            }
        }
        object IViewModel.Model { get { return Bot; } set { Bot = (IBot)value; } }
        private void OnEvaluated()
        {
            OnPropertyChanged("Bot");
            UpdateSignalText();
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
        public string TimeFrame { get { return TBot?.TimeFrame; } }

        #endregion

        #region State

        // TODO: Get IBot.ExecutionState
        public SupervisorBotState State { get; set; }
        //<xcdg:Column FieldName = "State" />


        public bool CanLive => false;
        public bool CanDemo => false;
        public bool CanScanner => true;
        public bool CanPaper => false;

        #region IsScanEnabled

        public bool IsScanEnabled
        {
            get { return isScanEnabled; }
            set
            {
                if (isScanEnabled == value) return;
                isScanEnabled = value;
                if (isScanEnabled)
                {
                    if (!Bot.Mode.HasFlag(BotMode.Scanner))
                    {
                        if (Bot.IsStarted())
                        {
                            Bot.Stop();
                        }

                        Bot.Mode |= BotMode.Scanner;
                    }
                    if (!Bot.IsStarted())
                    {
                        Bot.Start();
                    }
                    OnPropertyChanged(nameof(IsScanEnabled));
                }
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

        public BotViewModel()
        {
            backtestResults.CollectionChanged += BacktestResults_CollectionChanged;
        }
        public BotViewModel(IBot bot) : this()
        {
            this.Bot = bot;

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
            var dir = System.IO.Path.Combine(LionFireEnvironment.AppProgramDataDir, @"Results");
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


        #region LongSignalText

        public string LongSignalText
        {
            get { return longSignalText; }
            set
            {
                if (longSignalText == value) return;
                longSignalText = value;
                OnPropertyChanged(nameof(LongSignalText));
            }
        }
        private string longSignalText;

        #endregion


        #region ShortSignalText

        public string ShortSignalText
        {
            get { return shortSignalText; }
            set
            {
                if (shortSignalText == value) return;
                shortSignalText = value;
                OnPropertyChanged(nameof(ShortSignalText));
            }
        }
        private string shortSignalText;

        #endregion

        public void Update()
        {
            OnPropertyChanged(nameof(Indicator));

        }

        public void UpdateNetPositionSize()
        {
            var vol = Bot.BotPositions.GetNetVolume();
            if (vol > 0)
            {
                NetPositionSizeText = "LONG " + Math.Abs(vol);
            }
            else if (vol < 0)
            {
                NetPositionSizeText = "SHORT " + Math.Abs(vol);
            }
            else
            {
                NetPositionSizeText = "";
            }
        }

        #region NetPositionSizeText

        public string NetPositionSizeText
        {
            get { return netPositionSizeText; }
            set
            {
                if (netPositionSizeText == value) return;
                netPositionSizeText = value;
                OnPropertyChanged(nameof(NetPositionSizeText));
            }
        }
        private string netPositionSizeText;

        #endregion

        public void UpdateSignalText()
        {
            if (CloseLong < -Threshold)
            {
                //LongSignalText = "";
                LongSignalText = "CLOSE Long";
                hasLong = false;
            }
            else if (OpenLong > Threshold && CloseLong > -Threshold)
            {
                LongSignalText = "L@" + this.SignalBot.Indicator.Symbol.Bid;
                LongSignalText = "LONG";
                hasLong = true;
            }
            else if (hasLong)
            {
                LongSignalText = "Hold LONG";
            }
            else
            {
                LongSignalText = "";
            }

            if (OpenShort < Threshold-1 && CloseShort < -1+Threshold)
            {
                ShortSignalText = "SHORT";
                hasShort = true;
            }
            else if (CloseShort > -1+Threshold)
            {
                ShortSignalText = "CLOSE Short";
                hasShort = false;
            }
            else if (hasShort)
            {
                ShortSignalText = "HOLD Short";
            }
            else
            {
                ShortSignalText = "";
            }
        }
        private bool hasLong;
        private bool hasShort;
        public double Threshold = 0.9;


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
