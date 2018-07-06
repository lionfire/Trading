using System.Collections.Generic;

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

        double MinPositionSize { get; set; }

        double PositionPercentOfEquity { get; set; }

        double PositionRiskPercent { get; set; }


        #endregion

        #region Runtime Settings

        bool Log { get; }

        bool Link { get; set; }


        #endregion

        #region Settings


        #region Backtesting

        double BacktestMinTradesPerMonth { get; set; }
        double BacktestMinTradesPerMonthExponent { get; set; }

        double LogBacktestThreshold { get; set; }
        bool LogBacktestTrades { get; set; }
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

        //BotTags Tags { get; set; }

    }

    public class BotTags
    {
        public List<BotTag> Tags { get; set; }
    }


    /// <summary>
    /// Prefixes:
    ///  always
    ///  !never
    ///  ?sometimes
    ///  +optional
    ///  *multi
    ///  Examples:
    ///  
    /// Unidirectional 
    /// Bidirectional
    /// 
    /// Donchian
    /// Fuzzy
    /// 
    /// MeanReversion
    /// TrendFollowing
    /// Cypher
    /// PriceAction
    /// Candlestick
    /// 
    /// </summary>
    public class BotTag
    {
        public string Name { get; set; }

        /// <summary>
        /// Prefix: +
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Always { get; set; }
    }

}
