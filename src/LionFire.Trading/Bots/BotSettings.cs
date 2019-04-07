using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Bots
{
    public class BotSettings
    {
        /// <summary>
        /// in Seconds.  TODO: Remove
        /// </summary>
        public int SettingsCacheTimeout { get; set; }

        #region Logging Levels

        public bool Diag { get; set; }
        public bool Debug { get; set; }

        /// <summary>
        /// Useful for debugging Algos
        /// </summary>
        public bool Log { get; set; }
        public string LogFile { get; set; } = "e:/temp/LionFire.Trading.Algos.cTrader.log";

        #endregion

        #region Email

        public string FromEmail { get; set; }
        public string ToEmail { get; set; }

        #endregion

        #region Backtesting: Uplink to Backtest API

        public string BacktestApi { get; set; }
        public string BacktestTable { get; set; }
        public string BacktestTableKey { get; set; }

        #endregion


        #region Live/Demo bots: uplink to Link / Monitoring

        public string MonitoringApi { get; set; }

        /// <summary>
        /// Enable Link
        /// </summary>
        public bool Link { get; set; }

        /// <summary>
        /// Enable Link during Backtesting
        /// </summary>
        public bool LinkBacktesting { get; set; }
        public string LinkApi { get; set; }

        #endregion
        
        #region Fitness scoring

        public double MinTradesPerMonth { get; set; }
        public double BacktestMinTradesPerMonthExponent { get; set; }

        #endregion

        #region Backtest Filtering

        /// <summary>
        /// 
        /// </summary>
        public double LogBacktestThreshold { get; set; }

        public double LogBacktestDetailThreshold { get; set; }


        #endregion

        #region Algo Settings

        public bool UseTicks { get; set; } = false;
        

        #endregion
    }

}
