using Caliburn.Micro;
using LionFire.Extensions.AssignFrom;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LionFire.Structures;
using LionFire.Trading.Workspaces.Screens;

namespace LionFire.Trading.Dash.Wpf
{

    public class HistoricalDataViewModel : WorkspaceScreen, IHasStateType
    {

        Type IHasStateType.StateType { get { return typeof(HistoricalDataScreenState); } }

        #region Parameters

        #region From

        public DateTime From
        {
            get { return from; }
            set
            {
                if (from == value) return;
                from = value;
                NotifyOfPropertyChange(() => From);

            }
        }
        private DateTime from = DateTime.UtcNow - TimeSpan.FromDays(31);

        #endregion

        #region To

        public DateTime EffectiveTo
        {
            get { return CacheOnly ? To : DateTime.UtcNow; }
        }

        public DateTime To
        {
            get { return to; }
            set
            {
                if (to == value) return;
                to = value;
                NotifyOfPropertyChange(() => To);
                NotifyOfPropertyChange(() => EffectiveTo);
            }
        }
        private DateTime to = DateTime.UtcNow;

        #endregion

        public string SelectedTimeFrame
        {
            get { return selectedTimeFrame; }
            set
            {
                if (selectedTimeFrame == value) return;
                selectedTimeFrame = value;
                UpdateSelectedMarketSeries();
                NotifyOfPropertyChange(() => SelectedTimeFrame);
                NotifyOfPropertyChange(() => T1);
                NotifyOfPropertyChange(() => M1);
                NotifyOfPropertyChange(() => H1);
                NotifyOfPropertyChange(() => CalendarMode);
                
            }
        }
        private string selectedTimeFrame = "h1";

        #region CacheOnly

        public bool CacheOnly
        {
            get { return cacheOnly; }
            set
            {
                if (cacheOnly == value) return;
                cacheOnly = value;
                if (cacheOnly)
                {

                }
                NotifyOfPropertyChange(() => CacheOnly);
                NotifyOfPropertyChange(() => EffectiveTo);
                NotifyOfPropertyChange(() => CanChangeToDate);
            }
        }
        private bool cacheOnly = false;

        #endregion

        #endregion


        public bool CanChangeToDate
        {
            get { return CacheOnly; }
        }

        public HistoricalDataViewModel()
        {
            this.DisplayName = "Historical Data";
            DataItems.Add(new HistoricalDataItemViewModel
            {
                Count = 1000,
                IsAvailable = true,
                IsPartial = false,
                To = DateTime.Now,
                From = DateTime.UtcNow,
            });
            this.PropertyChanged += HistoricalDataViewModel_PropertyChanged;
        }

        private void HistoricalDataViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Property changed: " + e.PropertyName);
        }

        #region Lookup Data


        public IEnumerable<string> SymbolsAvailable
        {
            get
            {
                return "(All)".Yield().Concat(this.Session.Session.Account.SymbolsAvailable);
            }
        }

        public IEnumerable<string> TimeFramesAvailable
        {
            get
            {
                yield return "t1";
                yield return "m1";
                yield return "h1";
            }
        }

        #endregion

        #region State

        #region SelectedSymbol

        public string SelectedSymbolCode
        {
            get { return selectedSymbolCode; }
            set
            {
                if (selectedSymbolCode == value) return;
                selectedSymbolCode = value;
                NotifyOfPropertyChange(() => SelectedSymbolCode);
                try
                {
                    if (selectedSymbolCode == AllSymbolCode)
                    {
                        SelectedSymbol = null;
                    }
                    else
                    {
                        SelectedSymbol = this.Account.GetSymbol(selectedSymbolCode);
                    }
                }
                catch { SelectedSymbol = null; }
            }
        }
        private string selectedSymbolCode;

        public const string AllSymbolCode = "(All)";
        #endregion

        #region Derived


        #region SelectedSymbol

        public Symbol SelectedSymbol
        {
            get { return symbol; }
            set
            {
                if (symbol == value) return;
                symbol = value;

                UpdateSelectedMarketSeries();
                NotifyOfPropertyChange(() => SelectedSymbol);
            }
        }
        private Symbol symbol;

        #endregion


        #region SelectedMarketSeries

        private void UpdateSelectedMarketSeries()
        {
            if (symbol == null || String.IsNullOrWhiteSpace(SelectedTimeFrame))
            {
                SelectedMarketSeries = null;
            }
            else
            {
                SelectedMarketSeries = symbol.GetMarketSeriesBase(SelectedTimeFrame);
            }
        }

        public MarketSeriesBase SelectedMarketSeries
        {
            get { return selectedMarketSeries; }
            set
            {
                if (selectedMarketSeries == value) return;
                selectedMarketSeries = value;
                NotifyOfPropertyChange(() => SelectedMarketSeries);
            }
        }
        private MarketSeriesBase selectedMarketSeries;

        #endregion



        #region SymbolItemCount

        public int SymbolItemCount
        {
            get { return symbolItemCount; }
            set
            {
                if (symbolItemCount == value) return;
                symbolItemCount = value;
                NotifyOfPropertyChange(() => SymbolItemCount);
            }
        }
        private int symbolItemCount;

        #endregion



        #endregion

        #endregion

        #region Selected Item

        #region SelectedTimeFrame


        #region CalendarMode

        public CalendarMode CalendarMode
        {
            get
            {
                CalendarMode mode;
                switch (selectedTimeFrame)
                {
                    default:
                    case "t1":
                        mode = CalendarMode.Month;
                        break;
                    case "m1":
                        mode = CalendarMode.Year;
                        break;
                    case "h1":
                        mode = CalendarMode.Decade;
                        break;
                }
                return mode;
            }
        }

        #endregion

        #region Derived

        #region T1

        public bool T1
        {
            get { return SelectedTimeFrame == "t1"; }
            set
            {
                if (value) { SelectedTimeFrame = "t1"; }
            }
        }


        #endregion

        #region M1

        public bool M1
        {
            get { return SelectedTimeFrame == "m1"; }
            set
            {
                if (value) { SelectedTimeFrame = "m1"; }
            }
        }

        #endregion

        #region H1

        public bool H1
        {
            get { return SelectedTimeFrame == "h1"; }
            set
            {
                if (value) { SelectedTimeFrame = "h1"; }
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion

        public BindableCollection<string> MissingData { get; private set; } = new BindableCollection<string>();

        public bool CanLoad()
        {
            return true;
        }
        public MarketSeriesBase MarketSeries => this.Session.Session.Account.GetSymbol(SelectedSymbolCode).GetMarketSeriesBase(this.SelectedTimeFrame);
        //public BindableCollection<MarketSeriesBase> MarketSerieses => (MarketSeriesBase)this.Session.Session.Account.GetSymbol(SelectedSymbol).GetMarketSeries(this.SelectedTimeFrame);
        public IAccount Account => Session?.Session?.Account;

        public void Load()
        {
            Debug.WriteLine("Load clicked");

            if (/*MarketSerieses != null && MarketSerieses.Count > 0 ||*/ SelectedSymbolCode == "(All)")
            {
                if (SelectedSymbolCode == "(All)")
                {
                    Parallel.ForEach(Session.Session.Account.SymbolsAvailable, symbolCode =>
                    {
                        var series = (MarketSeriesBase)this.Session.Session.Account.GetSymbol(symbolCode).GetMarketSeriesBase(this.SelectedTimeFrame);
                        //Account.HistoricalDataProvider.GetData(MarketSeries, From, To, cacheOnly: this.CacheOnly);
                        _Load(series);
                    });
                }
            }
            else
            {
                _Load(MarketSeries);
                //Account.HistoricalDataProvider.GetData(MarketSeries, From, To, cacheOnly: this.CacheOnly);
            }
        }
        private void _Load(MarketSeriesBase series)
        {
            Account.HistoricalDataProvider.GetData(series, From, To, cacheOnly: this.CacheOnly);
        }

        #region DataItems

        public BindableCollection<HistoricalDataItemViewModel> DataItems
        {
            get { return dataItems; }
            set
            {
                if (dataItems == value) return;
                dataItems = value;
                NotifyOfPropertyChange(() => DataItems);
            }
        }
        private BindableCollection<HistoricalDataItemViewModel> dataItems = new BindableCollection<HistoricalDataItemViewModel>();

        #endregion

    }

}
