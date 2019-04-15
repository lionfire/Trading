using LionFire.ConsoleUtils;
using LionFire.ExtensionMethods;
using LionFire.Trading.Analysis;
using LionFire.Trading.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Portfolios
{
    public static class AnalysisDataUtils
    {
        /// <summary>
        /// Called in EnterOpenTrade
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="longAsset"></param>
        internal static void PopulateEntryVolumes(this ref PortfolioHistoricalTradeVM trade) {
            trade.LongVolumeAtEntry = trade.Trade.Volume;
            trade.ShortVolumeAtEntry = trade.Trade.Volume * trade.Trade.EntryPrice;
        }
    }

    public class PortfolioSimulator
    {
        #region Relationships

        public PortfolioSimulation Sim { get; set; }
        public PortfolioAnalysisOptions Options { get; set; }

        #endregion

        #region Construction and Static Methods

        public static async Task<PortfolioSimulation> Simulate(Portfolio portfolio, PortfolioAnalysisOptions options, CancellationToken? token = null) {
            var sim = new PortfolioSimulation(portfolio, options);
            await new PortfolioSimulator(sim).Simulate(token);
            return sim;
        }

        public PortfolioSimulator(PortfolioSimulation sim) {
            this.Sim = sim;
            if (Sim == null) throw new ArgumentNullException("Sim must be set");
            this.Options = sim.Options;
            if (sim.Options == null) throw new ArgumentNullException("Sim.Options must be set");
        }

        #endregion

        #region Options Cache

        public bool UseEquity => Options.UseEquity;
        public static readonly bool UseBalance = true;

        #endregion

        #region (Private) Simulation State

        #region Open Trades

        internal List<PortfolioHistoricalTradeVM> OpenTrades = new List<PortfolioHistoricalTradeVM>();

        internal void EnterOpenTrade(PortfolioHistoricalTradeVM trade) {
#if DEBUG
            if (Options.Verbosity >= 4) {
                Console.WriteLine(trade.Trade.DumpProperties());
            }
#endif
            //var symbol1 = trade.Component.BacktestResult.Symbol;
            var symbol = trade.Trade.SymbolCode;

            string longAsset, shortAsset;

            if (Options.AssetExposureBars) {
                if (trade.Component.IsMultiSymbol)
                {
                    (longAsset, shortAsset) = AssetNameResolver.ResolvePair(symbol);
                    if (trade.LongAsset == null || trade.ShortAsset == null) throw new AnalysisException($"Could not resolve long and short components of symbol {symbol}");
                }
                else
                {
                    if (trade.Component.LongAsset == null)
                    {
                        throw new AnalysisException($"Could not resolve long and short components of symbol {symbol}");
                    }
                    longAsset = trade.LongAsset = trade.Component.LongAsset;
                    shortAsset = trade.ShortAsset = trade.Component.ShortAsset;
                }

                //Console.WriteLine($"EnterOpenTrade | {symbol} | {trade.LongAsset} | {trade.ShortAsset}");


                var longAssetBar = GetAssetExposureBar(longAsset, openTime);
                var shortAssetBar = GetAssetExposureBar(shortAsset, openTime);

                trade.PopulateEntryVolumes();

                if (Options.JournalLevel >= 1) {
                    Console.WriteLine($"{trade.Trade.EntryTime} OPEN   LONG {longAsset} x {trade.LongVolumeAtEntry}  |  SHORT {shortAsset} x {trade.ShortVolumeAtEntry}");
                }

                longAssetBar.Close += trade.LongVolumeAtEntry;
                shortAssetBar.Close -= trade.ShortVolumeAtEntry;
            }
            OpenTrades.Add(trade);
            if (Options.ComponentExposureBars) {
                trade.Component.OpenTrade(trade.Trade);
            }
        }

        internal void CloseOpenTrade(PortfolioHistoricalTradeVM trade, int openTradesIndex) {
            if (Options.AssetExposureBars) {
                var longAssetBar = GetAssetExposureBar(trade.LongAsset, openTime);
                var shortAssetBar = GetAssetExposureBar(trade.ShortAsset, openTime);

                if (Options.JournalLevel >= 1) {
                    Console.WriteLine($"{trade.Trade.EntryTime} CLOSE  LONG {trade.LongAsset} x {trade.LongVolumeAtEntry}  |  SHORT {trade.ShortAsset} x {trade.ShortVolumeAtEntry}");
                }

                longAssetBar.Close -= trade.LongVolumeAtEntry;
                shortAssetBar.Close += trade.ShortVolumeAtEntry;
            }

            OpenTrades.RemoveAt(openTradesIndex);

            CurrentBalance += trade.Trade.NetProfit / trade.Component.BacktestResult.InitialBalance;

            if (Options.ComponentExposureBars) {
                trade.Component.CloseTrade(trade.Trade);
            }
        }

        #endregion

        DateTime openTime;
        DateTime startDate;
        DateTime end;
        DateTime barEndTime;

        List<PortfolioBacktestBar> equityBars;
        PortfolioBacktestBar equityBar;
        PortfolioBacktestBar lastEquityBar;

        List<PortfolioBacktestBar> balanceBars;
        PortfolioBacktestBar balanceBar;
        PortfolioBacktestBar lastBalanceBar;

        #region Asset Exposure Bars

        /// <summary>
        /// CAVEAT: Exposure is based on the open time of the trade
        /// </summary>
        Dictionary<string, List<PortfolioBacktestBar>> assetExposureBars;
        /// <summary>
        /// This only works if openTime is only ever called sequentially
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="openTime"></param>
        /// <returns></returns>
        public PortfolioBacktestBar GetAssetExposureBar(string symbol, DateTime openTime) {
#if DEBUG
            if (symbol == null) { throw new ArgumentNullException(nameof(symbol)); }
#endif
            List<PortfolioBacktestBar> list = assetExposureBars.GetOrAdd(symbol, s => new List<PortfolioBacktestBar>());
            PortfolioBacktestBar last = null;
            if (list.Count > 0) {
                last = list.Last();
                if (last.OpenTime == openTime) {
                    return last;
                }
#if DEBUG
                if (openTime < last.OpenTime) {
                    throw new InvalidOperationException("Attempt to get older bar via GetAssetExposureBar, which can only return current bar or a new bar");
                }
#endif

            }
            var result = new PortfolioBacktestBar(openTime, last == null ? 0 : last.Close);
            list.Add(result);
            return result;
        }

        #endregion

        #region State: Drawdown

        protected double SimEquityDrawdownFrom;
        protected double SimBalanceDrawdownFrom;

        #endregion

        #region Trade Enumerations

        internal IEnumerable<PortfolioHistoricalTradeVM> AllTrades => Sim.Portfolio.Components
                    .Select(b => b.Trades.OfType<_HistoricalTrade>().Select(t => new PortfolioHistoricalTradeVM { Trade = t, Component = b }))
                    .SelectMany(v => v);
        internal List<PortfolioHistoricalTradeVM> TradesByClosingTime => AllTrades
                    .OrderBy(t => t.Trade.ClosingTime)
                    .ToList();
        internal List<PortfolioHistoricalTradeVM> TradesByEntryTime => AllTrades
                    .OrderBy(t => t.Trade.EntryTime)
                    .ToList();

        #endregion

        protected void Start() {
            if (Options.AssetExposureBars) {
                assetExposureBars = new Dictionary<string, List<PortfolioBacktestBar>>();
            }
            if (!Sim.Portfolio.Components.Any())
            {
                throw new Exception("Portfolio has no components");
            }
            startDate = Sim.Portfolio.Start.Value;
            if (Options.StartTime != default && Options.StartTime > openTime) {
                startDate = Options.StartTime;
            }

            openTime = startDate;

            end = Sim.Portfolio.End.Value;
            if (Options.EndTime != default && Options.EndTime < end) {
                end = Options.EndTime;
            }

            if (UseEquity) {
                equityBars = Sim.EquityBars = new List<PortfolioBacktestBar>();
                SimEquityDrawdownFrom = Options.InitialBalance;
            }
            if (UseBalance) {
                balanceBars = Sim.BalanceBars = new List<PortfolioBacktestBar>();
                SimBalanceDrawdownFrom = Options.InitialBalance;
            }

            if (Options.ComponentExposureBars) {
                foreach (var component in Sim.Portfolio.Components) {
                    component.Start();
                }
            }
        }
        protected void Stop() {
            CloseBar();

            Sim.AssetExposureBars = assetExposureBars;
            Sim.OnStopped();
        }

        //protected async Task<List<PortfolioComponent>> TryLoadTrades(CancellationToken? token = null)
        //{
        //    var fails = new List<PortfolioComponent>();
        //    foreach (var component in Sim.Portfolio.Components)
        //    {
        //        try
        //        {
        //            await component.LoadTrades();
        //        }
        //        catch(Exception ex)
        //        {
        //            Console.WriteLine("Failed to load trades for " + component + " Ex: " + ex.ToString()); // TOLOG
        //            fails.Add(component);
        //        }
        //        if (token.HasValue && token.Value.IsCancellationRequested) return null;
        //    }
        //    return fails;
        //}
        //protected async Task LoadTrades(CancellationToken? token = null)
        //{
        //    List<PortfolioComponent> fails;
        //    if ((fails = await TryLoadTrades(token)).Count > 0)
        //    {
        //        throw new Exception("Failed to load trades for all specified backtests");
        //    }
        //}

        #region CurrentBarEquity

        protected double CurrentBarEquity {
            get { return currentBarEquity; }
            set {
                currentBarEquity = value;
                if (double.IsNaN(Sim.Max) || currentBarEquity > Sim.Max) {
                    Sim.Max = currentBarEquity;
                }
            }
        }
        private double currentBarEquity;

        #endregion

        #region Open and Close Current Bar

        public void OpenBar() {
            bool firstBar;
            if (openTime > startDate) {
                firstBar = false;
                CloseBar();
            } else {
                firstBar = true;
            }
            if (UseEquity) {
                equityBar = new PortfolioBacktestBar(openTime, firstBar ? Sim.Options.InitialBalance : lastEquityBar.Close);
            }
            if (UseBalance) {
                balanceBar = new PortfolioBacktestBar(openTime, firstBar ? Sim.Options.InitialBalance : lastBalanceBar.Close);
            }

            barEndTime = openTime + Options.TimeFrame.TimeSpan;

            if (Options.ComponentExposureBars) {
                foreach (var component in Sim.Portfolio.Components) {
                    component.OpenBar(openTime);
                }
            }
        }

        /// <summary>
        /// Call this at start of OpenBar, or after end of simulation
        /// </summary>
        public void CloseBar() {
            if (UseEquity) {
                lastEquityBar = equityBar;
                equityBars.Add(lastEquityBar);
#if DEBUG
                if (Options.JournalLevel >= 2) {
                    Console.WriteLine("Eq: " + lastEquityBar);
                }
#endif
            }

            if (UseBalance) {
                lastBalanceBar = balanceBar;
                balanceBars.Add(lastBalanceBar);
#if DEBUG
                if (Options.JournalLevel >= 2) {
                    Console.WriteLine("Bal: " + lastBalanceBar);
                }
#endif
            }

            if (Options.ComponentExposureBars) {
                foreach (var component in Sim.Portfolio.Components) {
                    component.OpenBar(openTime);
                }
            }
        }

        #endregion

        #region Current Equity / Balance setters

        private double CurrentEquity {
            get => equityBar.Close;
            set {
                equityBar.Close = value;

                if (equityBar.Close > SimEquityDrawdownFrom) {
                    SimEquityDrawdownFrom = equityBar.Close;
                } else {
                    var dd = SimEquityDrawdownFrom - equityBar.Close;
                    var ddp = dd / SimEquityDrawdownFrom;

                    if (dd > Sim.Stats.MaxEquityDrawdown) {
                        Sim.Stats.MaxEquityDrawdown = dd;
                        //Sim.MaxEquityDrawdownPercent = dd / SimEquityDrawdownFrom;
                        //Console.WriteLine($"{openTime} >>>  Eq dd from {SimEquityDrawdownFrom} to cur {equityBar.Close}.  DD: {dd}, {Sim.MaxEquityDrawdownPercent.ToPercentString(1)}");
                    }
                    if (ddp > Sim.Stats.MaxEquityDrawdownPercent) {
                        Sim.Stats.MaxEquityDrawdownPercent = dd / SimBalanceDrawdownFrom;
                        //Console.WriteLine($"{openTime} >>>%  Bal dd from {SimBalanceDrawdownFrom} to cur {balanceBar.Close}.  DD: {dd}, {Sim.MaxBalanceDrawdownPercent.ToPercentString(1)}");
                    }
                }
            }
        } 

        private double CurrentBalance {
            get => balanceBar.Close;
            set {
                balanceBar.Close = value;

                if (balanceBar.Close > SimBalanceDrawdownFrom) {
                    SimBalanceDrawdownFrom = balanceBar.Close;
                    //Console.WriteLine($">> {openTime} SimBalanceDrawdownFrom up to {SimBalanceDrawdownFrom}");
                } else {
                    var dd = SimBalanceDrawdownFrom - balanceBar.Close;
                    var ddp = dd / SimBalanceDrawdownFrom;

                    if (dd > Sim.Stats.MaxBalanceDrawdown) {
                        Sim.Stats.MaxBalanceDrawdown = dd;
                        //Console.WriteLine($"{openTime} >>>$  Bal dd from {SimBalanceDrawdownFrom} to cur {balanceBar.Close}.  [DD: {dd}], {Sim.MaxBalanceDrawdownPercent.ToPercentString(1)}");
                    }
                    if(ddp > Sim.Stats.MaxBalanceDrawdownPercent) {
                        Sim.Stats.MaxBalanceDrawdownPercent = dd / SimBalanceDrawdownFrom;
                        //Console.WriteLine($"{openTime} >>>%  Bal dd% from {SimBalanceDrawdownFrom} to cur {balanceBar.Close}.  DD: {dd}, [{Sim.MaxBalanceDrawdownPercent.ToPercentString(1)}]");
                    }
                }
            }
        }

        #endregion

        #endregion

        #region (Public) Methods

        public async Task Simulate(CancellationToken? t) {
            if (!this.Sim.Portfolio.Components.Any()) return;

            switch (Options.Mode) {
                case PortfolioEquityCurveMode.Unspecified:
                    throw new ArgumentException("Options.Mode must be specified");
                case PortfolioEquityCurveMode.Precise:
                    await Simulate_Precise(t);
                    break;
                case PortfolioEquityCurveMode.InterpolateEquityFromBalance:
                    await Simulate_EquityInterpolation(t);
                    break;
                case PortfolioEquityCurveMode.BalanceOnly:
                    await Simulate_BalanceOnly(t);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Simulation Main Logic

        public async Task Simulate_Precise(CancellationToken? token) {
            await Task.Run(() => {
                Start();
                throw new NotImplementedException();
                Stop();

            });
        }

        public async Task Simulate_EquityInterpolation(CancellationToken? token) {
            await Task.Run(async () => {
                Start();

                var allTrades = AllTrades;
                //var tradesByClosingTime = TradesByClosingTime;
                var tradesByEntryTime = TradesByEntryTime;

                int indexTradesByEntryTime = 0;
                for (; openTime < end; openTime += Options.TimeFrame.TimeSpan) {
                    if (token.HasValue && token.Value.IsCancellationRequested) return;
                    OpenBar();

                    #region Entries

                    // Fast-forward through trades until we get to the end of this bar
                    for (; indexTradesByEntryTime < tradesByEntryTime.Count && tradesByEntryTime[indexTradesByEntryTime].Trade.EntryTime < barEndTime; indexTradesByEntryTime++) {
                        var trade = tradesByEntryTime[indexTradesByEntryTime];
                        if (trade.Trade.ClosingTime < barEndTime) {
                            continue;
                        }
                        EnterOpenTrade(tradesByEntryTime[indexTradesByEntryTime]);
                        if (token.HasValue && token.Value.IsCancellationRequested) return;
                    }

                    #endregion

                    //var openTradeEquity = 0.0;

                    #region Exits

                    for (int openTradesIndex = OpenTrades.Count - 1; openTradesIndex >= 0; openTradesIndex--) {
                        var trade = OpenTrades[openTradesIndex];
                        if (trade.Trade.ClosingTime < barEndTime) {
                            CloseOpenTrade(trade, openTradesIndex);
                            continue;
                        }
                    }

                    #endregion

                    if (UseEquity) {
                        if (Options.Mode == PortfolioEquityCurveMode.InterpolateEquityFromBalance) // REDUNDANT REFACTOR?
                        {
                            double interpolatedEquityDeltaVsBalance = 0;
                            foreach (var trade in OpenTrades) {
                                var interpolatedNetProfit = trade.InterpolatedNetProfit(barEndTime);
                                interpolatedEquityDeltaVsBalance += (interpolatedNetProfit / trade.Component.BacktestResult.InitialBalance);
                            }
                            CurrentEquity = CurrentBalance + interpolatedEquityDeltaVsBalance;
                        }
                    }

                    // Fast-forward through trades until we get to the end of this bar
                    //    for (; indexTradesByEntryTime < tradesByClosingTime.Count && tradesByClosingTime[indexTradesByEntryTime].Trade.ClosingTime < barEndTime; indexTradesByEntryTime++)
                    //    {
                    //        if (token.HasValue && token.Value.IsCancellationRequested) return;

                    //        var trade = tradesByClosingTime[indexTradesByEntryTime];

                    //        CurrentBalance += trade.Trade.NetProfit;

                    //        //if (trade.Trade.NetProfit > 0) barWinningEquity += trade.Trade.NetProfit;
                    //        //else barLosingEquity -= trade.Trade.NetProfit;
                    //    }
                    //    equityBar.Close = balance;

                    //    //bool broadHighLow = true;
                    //    //if (broadHighLow)
                    //    //{// This is a conservative worst case / best case scenario
                    //    //    equityBar.High = equityBar.Open + barWinningEquity;
                    //    //    equityBar.Low = equityBar.Open - barLosingEquity;
                    //    //}

                    //    Console.WriteLine(" - " + equityBar);
                    //    lastEquityBar = equityBar;
                }
                Stop();

            });
        }

        /// <summary>
        /// This is just a crude approximation of equity curve based on ending net profit of trades.  Plenty of inaccuracies -- don't depend on it.
        /// Might be useful as a pre-screen when crunching large amounts of backtests into a smaller set.
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public async Task Simulate_BalanceOnly(CancellationToken? token) {
            await Task.Run(async () => {
                Start();

                var allTrades = AllTrades;
                var tradesByClosingTime = TradesByClosingTime;

                int tradesByClosingTimeIndex = 0;
                for (; openTime < end; openTime += Options.TimeFrame.TimeSpan) {
                    if (token.HasValue && token.Value.IsCancellationRequested) return;
                    OpenBar();

                    // Fast-forward through trades until we get to the end of this bar
                    for (; tradesByClosingTimeIndex < tradesByClosingTime.Count && tradesByClosingTime[tradesByClosingTimeIndex].Trade.ClosingTime < barEndTime; tradesByClosingTimeIndex++) {
                        if (token.HasValue && token.Value.IsCancellationRequested) return;

                        var trade = tradesByClosingTime[tradesByClosingTimeIndex];
                        CurrentBalance += Sim.Options.InitialBalance * (trade.Trade.NetProfit / trade.Component.BacktestResult.InitialBalance);

                        //if (trade.Trade.NetProfit > 0) barWinningEquity += trade.Trade.NetProfit;
                        //else barLosingEquity -= trade.Trade.NetProfit;
                    }

                    //bool broadHighLow = true;
                    //if (broadHighLow)
                    //{// This is a conservative worst case / best case scenario
                    //    equityBar.High = equityBar.Open + barWinningEquity;
                    //    equityBar.Low = equityBar.Open - barLosingEquity;
                    //}
                    //else
                    //{
                    //    equityBar.High = barHigh;
                    //    equityBar.Low = barLow;
                    //}

                    Console.WriteLine($" - {barEndTime.ToString()} b:{String.Format(Options.NumberFormat, balanceBar.Close).PadRight(Options.DumpColumnWidth, ' ')} maxB: {String.Format(Options.NumberFormat, equityBar.High).PadRight(Options.DumpColumnWidth, ' ')} minB:{String.Format(Options.NumberFormat, equityBar.Low).PadRight(Options.DumpColumnWidth, ' ')}");
                }

                Stop();

            });
        }

        #endregion

    }

}
