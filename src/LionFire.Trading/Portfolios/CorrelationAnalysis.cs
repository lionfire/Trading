//using LionFire.ConsoleUtils;
using LionFire.ExtensionMethods.Dumping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Console;

namespace LionFire.Trading.Portfolios
{
    public static class PortfolioCliUtils
    {
        [Conditional("DEBUG")]
        public static void WriteLineVerbosity(this PortfolioSimulation sim, string msg, int verbosityLevel = 1)
        {
            if (sim.Options.Verbosity >= verbosityLevel)
            {
                WriteLine(msg);
            }
        }
    }
    public static class CorrelationAnalysis
    {
        public static Correlation GetCorrelation(this PortfolioSimulation sim, string correlationId)
        {
            var chunks = correlationId.Split('|');

            var c = new Correlation(sim.Portfolio.FindComponent(chunks[0]), sim.Portfolio.FindComponent(chunks[1]));

            foreach (var component in c.Components)
            {
                if (!component.NormalizationMultiplier.HasValue)
                {
                    component.UpdateComponentNormalizationMultiplier(sim);
                }
            }
            return c;
        }

        public class TradeEnterExitEvent : IEquatable<TradeEnterExitEvent>
        {
            public DateTime Time { get; set; }
            public bool Enter { get; set; }

            public PortfolioHistoricalTradeVM Trade { get; set; }

            #region Misc

            public override string ToString() => $"{Time} {(Enter ? "OPEN" : "CLOSE")} - {Trade.Trade.ToString()} ({Trade.Component.ComponentId})";

            public override bool Equals(object obj)
            {
                if (!(obj is TradeEnterExitEvent)) return false;
                return Equals(obj as TradeEnterExitEvent);
            }

            public override int GetHashCode() => Trade.Component.ComponentId.GetHashCode() + Trade.Trade.PositionId.GetHashCode();

            public bool Equals(TradeEnterExitEvent other)
            {
                if (Trade.Component.ComponentId != other.Trade.Component.ComponentId || Trade.Trade.PositionId != other.Trade.Trade.PositionId)
                    return false;

                return true;
            }

            public static bool operator ==(TradeEnterExitEvent x, TradeEnterExitEvent y) => x.Equals(y);
            public static bool operator !=(TradeEnterExitEvent x, TradeEnterExitEvent y) => !(x == y);

            #endregion

        }

        
        public static void CorrelateTimeInMarket(this PortfolioSimulation sim, Correlation c)
        {
            IEnumerable<PortfolioHistoricalTradeVM> AllTrades = c.Components
                    .Select(b => b.Trades.OfType<_HistoricalTrade>().Select(t => new PortfolioHistoricalTradeVM { Trade = t, Component = b }))
                    .SelectMany(v => v);

            var tradeEvents = AllTrades.Select(t => new TradeEnterExitEvent { Enter = true, Time = t.Trade.EntryTime, Trade = t })
                .Concat(AllTrades.Select(t => new TradeEnterExitEvent { Enter = false, Time = t.Trade.ClosingTime, Trade = t }))
                .OrderBy(e => e.Time);

            //var TradesByEntryTime = AllTrades
            //        .OrderBy(t => t.Trade.EntryTime)
            //        .ToList();

            // TODO: Normalized versions of these that are rated versus the max normalized score (which should be 1)
            TimeSpan BothLong = TimeSpan.Zero;
            TimeSpan BothShort = TimeSpan.Zero;
            TimeSpan OppositeDirection = TimeSpan.Zero;
            TimeSpan OffsettingNonZero = TimeSpan.Zero;

            TimeSpan BothFlat = TimeSpan.Zero;
            TimeSpan OneOpen = TimeSpan.Zero;
            // Not sure if this is useful yet.
            double MultipliedByDifference = 0;

            var openTrades = new List<PortfolioHistoricalTradeVM>();
            double netVolume1 = 0;
            double netVolume2 = 0;
            double normalizedNetVolume1 = 0;
            double normalizedNetVolume2 = 0;
            double delta = normalizedNetVolume2 - normalizedNetVolume1;

            DateTime simTime = sim.Portfolio.Start.Value;
            TimeSpan totalTime = TimeSpan.Zero;

            foreach (var e in tradeEvents)
            {
                #region Time Delta

                TimeSpan timeDelta;

                if (sim.Options.StartTime != default && e.Time < sim.Options.StartTime)
                {
                    timeDelta = e.Time - sim.Options.StartTime;
                }
                else if (sim.Options.EndTime != default && e.Time > sim.Options.EndTime)
                {
                    timeDelta = sim.Options.StartTime - e.Time;
                }
                else
                {
                    timeDelta = e.Time - simTime;
                }

                if (timeDelta < TimeSpan.Zero) continue; // If equal to zero, keep going, since there may be multiple events at the same bar opens.

                totalTime += timeDelta;

                #endregion

                #region Collect stats from previous time delta

#if DEBUG
                if (sim.Options.Verbosity >= 5) WriteLine($"1: {normalizedNetVolume1}  2: {normalizedNetVolume2}");
#endif
                // TODO NEXT: for the period of time between simTime and trade entry time (timeDelta), multiply this period of time by the volume delta over that time.
                if (normalizedNetVolume2 > 0 && normalizedNetVolume1 > 0)
                {
                    BothLong += timeDelta;
                }
                else if (normalizedNetVolume2 < 0 && normalizedNetVolume1 < 0)
                {
                    BothShort += timeDelta;
                }
                else if (normalizedNetVolume2 == 0 && normalizedNetVolume1 == 0)
                {
                    BothFlat += timeDelta;
                }

                if (normalizedNetVolume2 < 0 && normalizedNetVolume1 > 0 ||
                  normalizedNetVolume2 < 0 && normalizedNetVolume1 > 0)
                {
                    OppositeDirection += timeDelta; 
                }
                else if (normalizedNetVolume2 != 0 && normalizedNetVolume1 == 0 ||
                normalizedNetVolume2 == 0 && normalizedNetVolume1 != 0)
                {
                    OneOpen += timeDelta;
                }
                // Another idea: non-zero, offsetting
                if (normalizedNetVolume2 != 0 && normalizedNetVolume1 == -normalizedNetVolume2)
                {
                    OffsettingNonZero += timeDelta;
                }

                // These two normalized volumes should be normalized to -1 to +1.
                //MillisecondsMultipliedByDifference += timeDelta.TotalMilliseconds * (normalizedNetVolume2 - normalizedNetVolume1);
                MultipliedByDifference += timeDelta.TotalMilliseconds * Math.Pow(normalizedNetVolume2 - normalizedNetVolume1, 2);

                #endregion

                if (sim.Options.EndTime != default && simTime >= sim.Options.EndTime)
                {
                    break;
                }

                #region Process the trade open/close event

                if (e.Enter)
                {
                    sim.WriteLineVerbosity("OPEN trade:" + e.Trade.Trade.NetVolume, 5);
                    openTrades.Add(e.Trade);
                }
                else
                {
                    sim.WriteLineVerbosity("CLOSE trade:" + e.Trade.Trade.NetVolume, 5);

                    if (!openTrades.Remove(e.Trade))
                    {
                        WriteLine("[correlation] Removing from open trades - NOT FOUND: " + e + $" (entry time: {e.Trade.Trade.EntryTime})");
                    }
                }

                #endregion

                #region Set up state for the next trade:

                netVolume1 = 0;
                netVolume2 = 0;
                normalizedNetVolume1 = 0;
                normalizedNetVolume2 = 0;
                foreach (var t in openTrades)
                {
                    if (t.Component.ComponentId == c.Component1.ComponentId)
                    {
                        var volumeDelta = t.Trade.TradeType == TradeType.Buy ? t.Trade.Volume : -t.Trade.Volume;

                        netVolume1 += t.Trade.TradeType == TradeType.Buy ? t.Trade.Volume : -t.Trade.Volume;
                        normalizedNetVolume1 += PortfolioNormalization.NormalizeVolume(t, sim, CorrelationNormalizationOptions);
                        normalizedNetVolume1 = netVolume1;
                    }
                    else
                    {
#if DEBUG
                        if (t.Component.ComponentId != c.Component2.ComponentId) throw new UnreachableCodeException();
#endif
                        netVolume2 += t.Trade.TradeType == TradeType.Buy ? t.Trade.Volume : -t.Trade.Volume;
                        normalizedNetVolume2 += PortfolioNormalization.NormalizeVolume(t, sim, CorrelationNormalizationOptions);
                        normalizedNetVolume2 = netVolume2;
                    }
                }
                delta = normalizedNetVolume2 - normalizedNetVolume1;

                simTime = e.Time;

                #endregion
            }

            //var PositionSimilarityScore = String.Format(sim.Options.NumberFormat, ((BothLong + BothShort) - Opposite).TotalMilliseconds / totalTime.TotalMilliseconds);
            //var PositioningSimilarityScore = String.Format(sim.Options.NumberFormat, ((BothLong + BothShort + BothFlat) - Opposite).TotalMilliseconds / totalTime.TotalMilliseconds);

            var SameDir = BothLong + BothShort;
            var BothOpen = SameDir + OppositeDirection;
            var BothSamePositioning = BothOpen + BothFlat;

            WriteLine(new
            {
                TotalTime = totalTime,
                

                BothFlat,
                BothLong,
                BothShort,
                OppositeDirection,
                BothOpen,
                SameDir,
                //OffsettingNonZero,

                BothFlatPc = (BothFlat.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                BothLongPc = (BothLong.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                BothShortPc = (BothShort.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                SameDirPc = (SameDir.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                OppositeDirectionPc = (OppositeDirection.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                BothOpenPc = (BothOpen.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                OffsettingNonZeroPc = (OffsettingNonZero.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                //OppositePc = (Opposite.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                SameDirectionSimilarityPc = ((SameDir).TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString()
                    + " / " + (BothOpen.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                OppositeDirectionSimilarityPc = ((OppositeDirection).TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString()
                    + " / " + (BothOpen.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                
                OpenDirectionNetSimilarityPc = ((SameDir - OppositeDirection).TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString()
                    + " / " + (BothOpen.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                // TODO: same as OpenDirectionNetSimilarityPc but use normalized volumes:
                //NetAmplifyingOrHedgingPc = ((SameDir - OppositeDirection).TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString()
                //    + " / " + (BothOpen.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),

                SimilarityWhenSharedDirectionPc = ((BothSamePositioning - OppositeDirection).TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString()
                    + " / " + (BothSamePositioning.TotalMilliseconds / totalTime.TotalMilliseconds).ToPercentString(),
                

                DifferenceScore = Math.Sqrt(MultipliedByDifference / totalTime.TotalMilliseconds),
            }.DumpProperties("Time in market"));

        }
        private static readonly VolumeNormalizationOptions CorrelationNormalizationOptions = new VolumeNormalizationOptions
        {
            Max = 1,
            MaxMode = VolumeNormalizationTargetMode.ToConstant,

            // Let the user specify these: 
            //Curve = PortfolioNormalizationCurveType.Linear,
            //MaxSourceValue = 1,
            //ReductionMode = VolumeNormalizationReductionMode.DivideByMinimumsMultipleOfMinimumAllowedTradeSize | VolumeNormalizationReductionMode.DivideByMinimumAllowedTradeVolume,

        };



        public static void CorrelateExposureBars(this PortfolioSimulation sim, Correlation c)
        {
            double count = c.Component1.LongExposureBars.Count;
            if (c.Component1.LongExposureBars.Count != count)
            {
                throw new Exception($"Mismatched number of bars between {c.Component2.ComponentId} ({count}) and {c.Component2.ComponentId} ({c.Component1.LongExposureBars.Count})");
            }
            double highSum = 0;
            double lowSum = 0;
            //double pow = 2;
            for (int i = 0; i < count; i++)
            {
                //highSum += Math.Pow(Math.Abs(c.Component1.LongExposureBars[i].High - c.Component2.LongExposureBars[i].High), pow);
                //lowSum += Math.Pow(Math.Abs(c.Component1.LongExposureBars[i].Low - c.Component2.LongExposureBars[i].Low), pow);
                highSum += Math.Abs(c.Component1.LongExposureBars[i].High - c.Component2.LongExposureBars[i].High);
                lowSum += Math.Abs(c.Component1.LongExposureBars[i].Low - c.Component2.LongExposureBars[i].Low);
                //weightedSum += Math.Pow(Math.Abs(c.Component1.LongExposureBars[i].Low - c.Component2.LongExposureBars[i].Low), pow);
            }

            //c.HighScore = Math.Sqrt(highSum);
            //c.LowScore = Math.Sqrt(highSum);
            c.HighScore = highSum / count;
            c.LowScore = lowSum / count;
        }
    }

}
