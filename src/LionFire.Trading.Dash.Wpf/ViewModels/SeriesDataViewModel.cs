using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace LionFire.Trading.Dash.Wpf
{
    public class SeriesDataViewModel : WorkspaceScreen
    {
        //public static SeriesDataViewModel Instance
        //{
        //    get
        //    {
        //        return instance;
        //    }
        //}
        //static SeriesDataViewModel instance = new SeriesDataViewModel()
        //{
        //    SymbolCode = "EURUSD",
        //    TimeFrame = "h1"
        //};

        #region MarketSeriesBase

            [Browsable(false)]
        public MarketSeriesBase MarketSeriesBase
        {
            get { return marketSeriesBase; }
            set
            {
                if (marketSeriesBase == value) return;
                marketSeriesBase = value;
                NotifyOfPropertyChange(() => MarketSeriesBase);
                NotifyOfPropertyChange(() => MarketSeries);
                NotifyOfPropertyChange(() => MarketTickSeries);
            }
        }
        private MarketSeriesBase marketSeriesBase;

        #endregion

        public MarketSeries MarketSeries => MarketSeriesBase as MarketSeries;
        public MarketTickSeries MarketTickSeries => MarketSeriesBase as MarketTickSeries;

        [Browsable(false)]
        public IEnumerable<string> SymbolsAvailable => SessionViewModel.Session.SymbolsAvailable;
        [Browsable(false)]
        public IEnumerable<string> TimeFramesAvailable => SessionViewModel.Session.TimeFramesAvailable;
        

        #region SymbolCode

        public string SymbolCode
        {
            get { return symbolCode; }
            set
            {
                if (symbolCode == value) return;
                symbolCode = value;
                MarketSeriesBase = SessionViewModel.Session.Account.GetMarketSeries(SymbolCode, timeFrameName);
                NotifyOfPropertyChange(() => SymbolCode);
                RaiseSeriesChanged();
            }
        }
        private string symbolCode = "XAUUSD";

        #endregion

        [Browsable(false)]
        public bool IsTickMode => TimeFrame == "t1";

        [Browsable(false)]
        public bool IsBarMode => TimeFrame != "t1";

        #region TimeFrame

        public string TimeFrame
        {
            get { return timeFrameName; }
            set
            {
                if (timeFrameName == value) return;
                timeFrameName = value;
                MarketSeriesBase = SessionViewModel.Session.Account.GetMarketSeries(SymbolCode, timeFrameName);
                NotifyOfPropertyChange(() => TimeFrame);
                RaiseSeriesChanged();
            }
        }
        private string timeFrameName = "m1";

        private void RaiseSeriesChanged()
        {
            NotifyOfPropertyChange(() => IsTickMode);
            NotifyOfPropertyChange(() => IsBarMode);
            NotifyOfPropertyChange(() => DataStartDate);
            NotifyOfPropertyChange(() => DataEndDate);
            NotifyOfPropertyChange(() => Count);
            NotifyOfPropertyChange(() => MarketSeriesBase);
            NotifyOfPropertyChange(() => MarketSeries);
            NotifyOfPropertyChange(() => MarketTickSeries);
            NotifyOfPropertyChange(() => PropertiesObject);
            
        }
        #endregion

        public object PropertiesObject => this;

        public DateTime? DataStartDate => MarketSeriesBase?.DataStartDate;
        public DateTime? DataEndDate => MarketSeriesBase?.DataEndDate;
        public int? Count => MarketSeriesBase?.Count;

        [Browsable(false)]
        public IEnumerable<Tick> Ticks
        {
            get {
                return null;
                // TODO: Wrapper from ticks to the 3 bidirectional lists?? or just convert the arrays to a single bidir list?
                //return MarketTickSeries?.Ticks;
            }
        }
        [Browsable(false)]
        public IEnumerable<TimedBar> Bars
        {
            get
            {
                return null;
                //return MarketSeries?.Bars;
            }
        }

    }
}
