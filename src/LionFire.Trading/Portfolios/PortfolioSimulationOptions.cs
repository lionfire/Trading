using System;
using System.Collections.Generic;

namespace LionFire.Trading.Portfolios
{
    public class PortfolioAnalysisOptions
    {
        public PortfolioAnalysisOptions() { }

        #region Core Parameters

        public TimeFrame TimeFrame { get; set; }
        public double InitialBalance { get; set; } = 1;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        #endregion

        #region Precision

        public PortfolioEquityCurveMode Mode { get; set; }

        #endregion

        #region Analysis features

        /// <summary>
        /// TODO: Set this to false by default if the analysis doesn't need it
        ///  - !ComponentExposureBars
        /// </summary>
        public bool AssetExposureBars { get; set; } = true;

        public bool Correlate { get; set; }

        public bool ComponentExposureBars => Correlate;

        #endregion

        #region Volume Normalization

        public VolumeNormalizationOptions VolumeNormalization { get; set; }

        #endregion

        #region Display Parameters

        public string NumberFormat => InitialBalance <= 1 ? "{0:0.000}" : (InitialBalance < 100 ? "{0:0.00}" : "{0:0.0}");

        /// <summary>
        /// Journal verbosity level
        ///  0: off
        ///  1: OPEN and CLOSE
        ///  2: Equity at end of each bar
        /// </summary>
        public int JournalLevel { get; set; }

        /// <summary>
        /// Verbosity level
        ///  4: Trade properties
        /// </summary>
        public int Verbosity { get; set; }

        #region Text output

        // Consider moving this section
        public int DumpColumnWidth { get; set; } = 10;

        #endregion

        #endregion

        #region Derived

        public bool UseEquity => Mode != PortfolioEquityCurveMode.BalanceOnly;

        public bool NeedsTrades => Mode != PortfolioEquityCurveMode.BalanceOnly;

        #endregion
    }
}
