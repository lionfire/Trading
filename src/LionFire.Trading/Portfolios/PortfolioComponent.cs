using LionFire.Trading.Backtesting;
using LionFire.Trading.Bots;
using LionFire.Trading.Instruments;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LionFire.Trading.Portfolios
{
    public class PortfolioComponent : PortfolioComponentSimState // TODO: Don't inherit from SimState, rather get the Sim to make a SimState associated with each component
    {
        #region Construction

        public PortfolioComponent() { ComponentId = IdUtils.GenerateId(4);  }
        public PortfolioComponent(string backtestResultId) :this() { BacktestResultId = backtestResultId; }

        #endregion

        #region Identity

        public string PortfolioId { get; set; }
        public string ComponentId { get; set; }       

        #endregion

        #region Foreign Key Relationships

        //public Portfolio Portfolio { get; set; }
        //public string PortfolioId { get; set; }

        #endregion

        #region Parameters

        /// <summary>
        /// Multiply the percentage gains and losses by this in order to give this Backtest a different weight within a portfolio
        /// </summary>
        public double Weight { get; set; } = 1.0;

        #endregion

        #region Data

        public string BacktestResultId { get; set; }
        public BacktestResult BacktestResult { get; set; }

        [NotMapped]
        public _HistoricalTrade[] Trades { get; set; }

        #endregion

        #region (Derived)

        public SymbolHandle SymbolHandle {
            get {
                if(symbolHandle == null && BacktestResult?.Symbol != null)
                {
                    symbolHandle = new SymbolHandle(BacktestResult.Symbol);
                }
                return symbolHandle;
            }
        } private SymbolHandle symbolHandle;

        public string Symbol => BacktestResult.Symbol;
        public string LongAsset => SymbolHandle.LongAsset;
        public string ShortAsset => SymbolHandle.ShortAsset;

        /// <summary>
        /// True if this component trades multiple symbols.
        /// (Not implemented yet.)
        /// </summary>
        public bool IsMultiSymbol { get; } = false;

        public double MaxAbsoluteVolume {
            get {
                if (!maxAbsoluteVolume.HasValue) {
                    var max = 0.0;
                    foreach (var t in this.Trades) {
                        max = Math.Max(max, t.Volume);
                    }
                    maxAbsoluteVolume = max;
                }
                return maxAbsoluteVolume ?? double.NaN;
            }
        }
        double? maxAbsoluteVolume;

        public double MinAbsoluteVolume {
            get {
                if (!minAbsoluteVolume.HasValue) {
                    var min = double.MaxValue;
                    foreach (var t in this.Trades) {
                        min = Math.Min(min, t.Volume);
                    }
                    minAbsoluteVolume = min;
                }
                return minAbsoluteVolume ?? double.NaN;
            }
        }
        double? minAbsoluteVolume;
               
        #endregion

        #region Misc

        public override string ToString() => ComponentId;

        public string ToLongString() => $"{ComponentId}: {BacktestResult?.Symbol} {BacktestResult?.TimeFrame} {BacktestResult?.BotType} {BacktestResultId}";

        #endregion
    }
}
