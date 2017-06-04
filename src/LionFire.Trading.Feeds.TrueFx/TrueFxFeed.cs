using LionFire.Instantiating;
using LionFire.Trading.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.TrueFx
{

    public class TTrueFxFeed : TFeed, ITemplate<TrueFxFeed>
    {
    }

    public class TrueFxFeed : FeedBase<TTrueFxFeed>
    {
        public override bool IsSimulation => false;

        #region IsAuthenticated

        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
            set
            {
                if (isAuthenticated == value) return;
                isAuthenticated = value;
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }
        private bool isAuthenticated;

        #endregion

        #region SymbolsAvailable

        public IEnumerable<string> AuthenticatedSymbolsAvailable { get; protected set; } = new List<string>
        {
            "EURUSD"
            , "USDJPY"
            , "GBPUSD"
            , "EURGBP"
            , "USDCHF"
            , "EURJPY"
            , "EURCHF"
            , "USDCAD"
            , "AUDUSD"
            , "GBPJPY"
             , "CADCHF"
            , "CADJPY"
            , "CHFJPY"
            , "EURAUD"
            , "EURCHF"
            , "AUDCAD"
            , "AUDCHF"
            , "AUDJPY"
            , "AUDNZD"
            , "EURCAD"
            , "EURNOK"
            , "EURNZD"
            , "GBPCAD"
            , "GBPCHF"
            , "NZDJPY"
            , "NZDUSD"
            , "USDNOK"
            , "USDSEK"

        };

        public IEnumerable<string> UnauthenticatedSymbolsAvailable { get; protected set; } = new List<string>
        {
              "EURUSD"
            , "USDJPY"
            , "GBPUSD"
            , "EURGBP"
            , "USDCHF"
            , "EURJPY"
            , "EURCHF"
            , "USDCAD"
            , "AUDUSD"
            , "GBPJPY"
        };

        public override IEnumerable<string> SymbolsAvailable => IsAuthenticated ? AuthenticatedSymbolsAvailable : UnauthenticatedSymbolsAvailable;

        #endregion


        #region Time

        public override DateTime ExtrapolatedServerTime => throw new NotImplementedException();

        public override DateTime ServerTime { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        #endregion


        public override MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame)
        {
            return new MarketSeries(this, symbol, timeFrame);
        }

        protected override Symbol CreateSymbol(string symbolCode)
        {
            return new SymbolImpl(symbolCode, this);
        }
    }
}
