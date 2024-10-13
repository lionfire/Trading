using System;

namespace LionFire.Trading.Portfolios
{
    public static class PortfolioNormalization
    {
        /// <summary>
        /// Gets the multiplier by which all trade volumes from this component are multipied before using in portfolio and correlation analysis.
        /// </summary>
        public static void UpdateComponentNormalizationMultiplier(this PortfolioComponent component, PortfolioSimulation sim)
        {
            double multiplier = 1.0;

            var minTradeVolumeForSymbol = component.SymbolHandle.GetMinTradeVolumeForSymbol();

            if (sim.Options.VolumeNormalization.ReductionMode?.HasFlag(VolumeNormalizationReductionMode.DivideByMinimumsMultipleOfMinimumAllowedTradeSize) == true)
            {
                var min = component.MinAbsoluteVolume;
                if (min > minTradeVolumeForSymbol)
                {
                    multiplier *= minTradeVolumeForSymbol / min;
#if DEBUG
                    if (sim.Options.Verbosity >= 5)
                    {
                        Console.WriteLine($"[VolumeNormalize] component absolute min: {min},  min for symbol {component.BacktestResult.Symbol}: {minTradeVolumeForSymbol}, multiplier: {multiplier}");
                    }
#endif
                }
            }

            if (sim.Options.VolumeNormalization.ReductionMode?.HasFlag(VolumeNormalizationReductionMode.DivideByMinimumAllowedTradeVolume) == true)
            {
                multiplier /= minTradeVolumeForSymbol;
            }

            component.NormalizationMultiplier = multiplier;
        }

        private static double NormalizeVolumeSource(PortfolioSimulation sim, PortfolioComponent component, double unnormalizedVolume)
        {
            double value = unnormalizedVolume * component.NormalizationMultiplier.Value;


#if DEBUG
            if (sim.Options.VolumeNormalization.MaxSourceValue.HasValue && value > sim.Options.VolumeNormalization.MaxSourceValue.Value)
            {
                if (sim.Options.Verbosity >= 6)
                {
                    Console.WriteLine($"Capping normalized volume of {value} to max: {sim.Options.VolumeNormalization.MaxSourceValue.Value}");
                }
            }
#endif
            value = Math.Min(value, sim.Options.VolumeNormalization.MaxSourceValue.Value);

            return value;
        }

        /// <param name="trade"></param>
        /// <param name="sim"></param>
        /// <returns>Returns negative numbers for Sell trades.</returns>
        public static double NormalizeVolume(PortfolioHistoricalTradeVM trade, PortfolioSimulation sim, VolumeNormalizationOptions normalizationOptions = null)
        {
            // Note: Volume is used here instead of NetVolume, to have positive volumes for both buy and sell trades
            var normalizedTradeVolume = NormalizeVolumeSource(sim, trade.Component, trade.Trade.Volume);

#if DEBUG
            if (sim.Options.Verbosity >= 5)
            {
                Console.WriteLine($"[normalize] {trade.Trade.SymbolCode} normalized volume source from ({trade.Trade.TradeType.ToString()}) {trade.Trade.Volume} to {normalizedTradeVolume}");
            }
#endif


            if ((normalizationOptions?.MaxMode ?? sim.Options.VolumeNormalization.MaxMode).Value != VolumeNormalizationTargetMode.None)
            {
                double max = normalizationOptions?.Max ?? sim.Options.VolumeNormalization.Max ?? trade.Component.MaxAbsoluteVolume;

                //switch (mode ?? sim.Options.VolumeNormalization.VolumeNormalizationTargetMax)
                //{
                //    case PortfolioNormalizationTargetMaxMode.None:
                //        //var componentMaxNormalizedVolume = NormalizeVolumeSource(sim, trade.Component, trade.Component.MaxAbsoluteVolume);
                //        //max = componentMaxNormalizedVolume;
                //        //max = 
                //        break;
                //    case PortfolioNormalizationTargetMaxMode.ToConstant:
                //        max = ;
                //        break;
                //    //case PortfolioNormalizationTargetMaxMode.
                //    //case PortfolioNormalizationTargetMaxMode.ScaleToMinTradeVolume:
                //    //    max = 
                //    //    value =
                //    //    if (trade.Trade.TradeType == TradeType.Sell) value *= -1;
                //    //    break;
                //    //case PortfolioNormalizationTargetMaxMode.ToBacktestMaxTradeSize:
                //    //    value = trade.Trade.Volume / trade.Component.MaxAbsoluteVolume;
                //    //    if (trade.Trade.TradeType == TradeType.Sell) value *= -1;
                //    //    break;
                //    ////case PortfolioNormalizationMode.ToBacktestConfiguredMaxTradeSize:
                //    ////    throw new NotImplementedException();
                //    ////    break;
                //    default:
                //        //case PortfolioNormalizationMode.Unspecified:
                //        throw new ArgumentException(nameof(sim.Options.VolumeNormalization.VolumeNormalizationTargetMax));
                //}

                switch (sim.Options.VolumeNormalization.Curve)
                {
                    case PortfolioNormalizationCurveType.Linear:
                        normalizedTradeVolume = trade.Trade.TradeType == TradeType.Buy ? trade.Trade.Volume : -trade.Trade.Volume;
                        break;
                    case PortfolioNormalizationCurveType.Step:
                        normalizedTradeVolume = normalizedTradeVolume >= sim.Options.VolumeNormalization.StepThreshold ? max : 0;
                        break;
                    case PortfolioNormalizationCurveType.EaseIn:
                        {
                            double normalizedTo1 = normalizedTradeVolume / max;
                            normalizedTradeVolume = max * Math.Pow(normalizedTo1, sim.Options.VolumeNormalization.EasingExponent ?? VolumeNormalizationOptions.DefaultEasingExponent);
                            break;
                        }
                    case PortfolioNormalizationCurveType.EaseOut:
                        {
                            double normalizedTo1 = normalizedTradeVolume / max;
                            normalizedTradeVolume = max * Math.Pow(1 - (1 - normalizedTo1), sim.Options.VolumeNormalization.EasingExponent ?? VolumeNormalizationOptions.DefaultEasingExponent);
                            break;
                        }
                    //case PortfolioNormalizationCurveType.Unspecified:
                    default:
                        break;
                }
            }

            if (trade.Trade.TradeType == TradeType.Sell)
            {
                normalizedTradeVolume *= -1;
            }

#if DEBUG
            if (sim.Options.Verbosity >= 0)
            {
                if (trade.Trade.Volume != 1)
                {
                    Console.WriteLine($"[NORMALIZE] {trade.Trade.SymbolCode} normalized volume from ({trade.Trade.TradeType.ToString()}) {trade.Trade.Volume} to {normalizedTradeVolume}   (final)");
                }
            }
#endif

            return normalizedTradeVolume;
        }

        //public class PortfolioBacktest
        //{
        //    public string Id { get; set; }

        //    public double AD { get; set; }
        //    public double MaxEquityDrawdown { get; set; }
        //    public double MaxEquityDrawdownPercent { get; set; }
        //    public double MaxBalanceDrawdown { get; set; }
        //    public double MaxBalanceDrawdownPerunum { get; set; }
        //    public double NetProfit { get; set; }
        //}
    }


}
