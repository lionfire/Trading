using LionFire.Assets;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Persistence;

namespace LionFire.Trading.Bots
{
    public interface ITBot
    {
        #region Identity

        string Id { get; set; }
        string Symbol { get; set; }
        List<string> Symbols { get; set; }
        string TimeFrame { get; set; }

        #endregion

        #region Max Positions

        // RENAME: MaxPositions
        int MaxOpenPositions { get; set; }
        int MaxLongPositions { get; set; }
        int MaxShortPositions { get; set; }

        #endregion

        #region Position Sizing

        long MinPositionSize { get; set; }

        double PositionPercentOfEquity { get; set; }

        double PositionRiskPercent { get; set; }


        #endregion

        #region Settings

        bool Log { get; }

        #region Backtesting

        double BacktestMinTradesPerMonth { get; set; }

        double LogBacktestThreshold { get; set; }
        #endregion

        #endregion

        #region SL / TP

        double SLinAtr { get; set; }

        double TPinAtr { get; set; }

        double SLinDailyAtr { get; set; }
        double TPinDailyAtr { get; set; }

        #endregion

        #region Filters

        bool UseTradeInPivotDirection { get; set; }
        bool TradeInPivotDirection { get; set; }


        #endregion

    }
    [AssetPath("Algos")]
    public class TBot : ITemplateAsset, ITBot
    {

        public string AssetSubPath { get { return Id; } set { Id = value; } }
        public string Id { get; set; } = IdUtils.GenerateId();

        public string Account { get; set; }

        //[Ignore]
        public string Symbol
        {
            get { return Symbols?.FirstOrDefault(); }
            set
            {
                Symbols = new List<string> { value };
            }
        }

        #region Symbols
        
        public List<string> Symbols
        {
            get { return symbols; }
            set
            {
                // TODO - Fix - why are there duplicates in here?
                if (value != null)
                {
                    value = value.Distinct().ToList();
                }
                symbols = value;
            }
        }
        private List<string> symbols;

        #endregion

        //[Ignore]
        public string TimeFrame
        {
            get { return TimeFrames?.FirstOrDefault(); }
            set { TimeFrames = new List<string> { value }; }
        }
        public List<string> TimeFrames { get; set; }


        public bool Log { get; set; } = false;

        public string LogFile { get; set; } = "e:/temp/Trading.cAlgo.log";

        [Parameter("Log Backtest Fitness Min", DefaultValue = 2.0)]
        public double LogBacktestThreshold { get; set; }

        //[Parameter("Log Backtest", DefaultValue = true)]
        //public bool LogBacktest { get; set; }

        [Parameter("Log Backtest Trades", DefaultValue = false)]
        public bool LogBacktestTrades { get; set; }

        public bool UseTicks { get; set; } = false;

        /// <summary>
        /// In units of quantity, not volume
        /// </summary>
        public long MinPositionSize { get; set; }

        public bool ScalePositionSizeWithEquity { get; set; } = true;

        ///// <summary>
        ///// Uses Equity balance but Symbol pricing -- may use different currencies!!
        ///// </summary>
        //public double PositionRiskPricePercent { get; set; }

        public double PositionRiskPercent { get; set; }

        public double PositionPercentOfEquity { get; set; }

        public bool AllowLong { get; set; } = true;
        public bool AllowShort { get; set; } = true;

        #region Max Positions

        /// <summary>
        /// 0 or MaxValue means no limit
        /// </summary>
        public int MaxOpenPositions { get; set; } = int.MaxValue;
        public int MaxLongPositions { get; set; } = 1;
        public int MaxShortPositions { get; set; } = 1;

        #endregion

        #region Backtesting

        /// <summary>
        /// Trim huge gains
        /// </summary>
        public double BacktestProfitTPMultiplierOnSL { get; set; } = 0.0;

        public double BacktestMinTradesPerMonth { get; set; } = 0;

        #endregion

        #region SL and TP

        public double MaxStopLossPips { get; set; } = 0;


        #region in ATR

        public double SLinAtr { get; set; }

        public double TPinAtr { get; set; }
        #endregion


        #region in Daily ATR

        public double SLinDailyAtr { get; set; }
        public double TPinDailyAtr { get; set; }

        #endregion

        #endregion

        #region Filters

        public bool UseTradeInPivotDirection { get; set; }
        public bool TradeInPivotDirection { get; set; }


        #endregion

        ///// <summary>
        ///// TODO Updates  
        ///// </summary>
        //public double MaxEquityExposurePercent { get; set; } = 0.0;

        //public bool UseTakeProfit {
        //    get {
        //        throw new NotImplementedException();
        //    }

        //    set {
        //        throw new NotImplementedException();
        //    }
        //}

    }
}
