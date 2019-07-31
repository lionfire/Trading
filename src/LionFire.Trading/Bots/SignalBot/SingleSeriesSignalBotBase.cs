#if DEBUG
#define NULLCHECKS
#define TRACE_RISK
#define TRACE_CLOSE
#define TRACE_OPEN
#define TRACE_EVALUATE
#endif
#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using LionFire.Trading.Indicators;

using LionFire.Extensions.Logging;
using LionFire.Trading;
using System.IO;
using Newtonsoft.Json;
using LionFire.Trading.Backtesting;
using LionFire.Instantiating;
using System.Diagnostics;

namespace LionFire.Trading.Bots
{
    public interface ISingleSeriesSignalBot : IHasSingleSeries, IBot, ISignalBot
    {
#if !cAlgo
        //IEnumerable<Position> ReversePositions { get; }
        DateTime LastOpenTime {get;}
        DateTime LastCloseTime { get; }
        TradeKind? LastTradeType {get;}
#endif
    }

    public partial class SingleSeriesSignalBotBase<IndicatorType, TSingleSeriesSignalBotBase, TIndicator> : SignalBotBase<IndicatorType, TSingleSeriesSignalBotBase, TIndicator>, ISingleSeriesSignalBot, IHasCustomFitness
  where IndicatorType : class, ISignalIndicator, new()
  where TSingleSeriesSignalBotBase : TSingleSeriesSignalBot<TIndicator>, new()
      where TIndicator : class, ITIndicator, new()
    {

        #region Config

        public int MinStopLossTimesSpread = 5; // TEMP TODO
        public bool UseTakeProfit = false; // TEMP

        #endregion

        #region Construction

        public SingleSeriesSignalBotBase()
        {
            SignalBotBase_();
        }

        public void CreateIndicator()
        {
            if (Indicator != null)
            {
                Debug.WriteLine($"warn - {this.GetType().Name}.CreateIndicator: Indicator is already created. Replacing");
            }
            Indicator = (ISignalIndicator)Template.Indicator.Create(typeof(IndicatorType));
            //Indicator = new IndicatorType();
            //var iti = Indicator as ITemplateInstance<ITIndicator>;
            //iti.Template = Template.Indicator;
        }

        public SingleSeriesSignalBotBase(string symbol, string timeFrame) : this()
        {
            this.Template = new TSingleSeriesSignalBotBase()
            {
                Symbol = symbol,
                TimeFrame = timeFrame,
                Indicator = new TIndicator
                {
                },
            };
        }

        partial void SignalBotBase_();

        #endregion

        #region Computations by Derived Class

        public virtual double StopLossInPips { get { return 0; } }
        public virtual double TakeProfitInPips { get { return 0; } }

        #endregion

        #region State

        public int barCount = 0;

        #endregion

        public IEnumerable<Position> MyPositions
        {
            get {
                return base.Positions.Where(p => p.Label == this.Label);
            }
        }


        #region Evaluate

        //private int counter = 0;

        private object botLock = new object();
        public override void Evaluate() // TOASYNC
        {
#if !cAlgo
            if(State == ExecutionStateEx.Faulted) return;
#endif

            try
            {
#if cAlgo
                DateTime time = Server.Time;
#else
                DateTime time = Account.ExtrapolatedServerTime;
#endif
                if (time == default(DateTime))
                {
                    return;
                }

                if (!StartDate.HasValue) StartDate = ExtrapolatedServerTime;
                EndDate = ExtrapolatedServerTime;

#if !cAlgo
                if (State != ExecutionStateEx.Started)
                {
                    throw new InvalidExecutionStateException(ExecutionStateEx.Started, State);
                }
#endif
#if NULLCHECKS
                if (Indicator == null)
                {
                    throw new ArgumentNullException("Indicator (Evaluate)");
                }
                //if (Market.IsBacktesting && Server == null)
                //{
                //    throw new ArgumentNullException("!IsBacktesting && Server");
                //}
#endif

                try
                {
                    Indicator.CalculateToTime(time);
                }
                catch (Exception ex)
                {
                    throw new Exception("Indicator.CalculateToTime threw " + ex + " stack: " + ex.StackTrace, ex);
                }

#if TRACE_EVALUATE
                var traceThreshold = 0.0;

                Debug.WriteLineIf(double.IsNaN(Indicator.OpenLongPoints.LastValue), "NaN OpenLongPoints: " + this.ToString());

                if (Indicator.OpenLongPoints.LastValue > traceThreshold
                    || Indicator.CloseLongPoints.LastValue > traceThreshold
                || Indicator.OpenShortPoints.LastValue > traceThreshold
                    || Indicator.CloseShortPoints.LastValue > traceThreshold
                                    )
                {
                    logger.LogDebug($"[{this.ToString()} evaluate #{Indicator.OpenLongPoints.Count}] Open long: " + Indicator.OpenLongPoints.LastValue.ToString("N2") + " Close long: " + Indicator.CloseLongPoints.LastValue.ToString("N2") +
                         " Open short: " + Indicator.OpenShortPoints.LastValue.ToString("N2") + " Close short: " + Indicator.CloseShortPoints.LastValue.ToString("N2")
                        );
                }
#endif

#if false
            if (Indicator == null)
            {
                var msg = "No indicator for SignalBot.";
                l.Error(msg);
                throw new Exception(msg);
            }
            if (Indicator.OpenLongPoints == null)
            {
                var msg = "No Indicator.OpenLongPoints.";
                l.Error(msg);
                throw new Exception(msg);
            }
#endif

                if (double.IsNaN(Indicator.OpenLongPoints.LastValue))
                {
                    Debug.WriteLine($"{this} Indicator.OpenLongPoints.LastValue is NaN");
                }

                if (Template.AllowLong
                    && Indicator.OpenLongPoints.LastValue >=
                    1.0
                    && Indicator.CloseLongPoints.LastValue <
                    0.9
                    && CanOpenLong && CanOpen)
                {
                    _Open(TradeType.Buy, Indicator.LongStopLoss);
                }

                if (Template.AllowShort
                && -Indicator.OpenShortPoints.LastValue >=
                    1.0
                    && -Indicator.CloseShortPoints.LastValue <
                    0.9
                    && CanOpenShort && CanOpen)
                {

                    _Open(TradeType.Sell, Indicator.ShortStopLoss);
                }

                List<Position> toClose = null;
                if (Indicator.CloseLongPoints.LastValue >= 1.0)
                {
                    foreach (var position in MyPositions.Where(p => p.TradeType == TradeType.Buy))
                    {

#if TRACE_CLOSE
                        string plus = position.NetProfit > 0 ? "+" : "";
                        logger.LogInformation($"{Server.Time.ToDefaultString()} [CLOSE LONG {position.Quantity} x {Symbol.Code} @ {Indicator.Symbol.Ask}] {plus}{position.NetProfit}");
#endif
                        if (toClose == null) toClose = new List<Position>();
                        toClose.Add(position);
                    }
                }
                if (-Indicator.CloseShortPoints.LastValue >= 1.0)
                {
                    foreach (var position in MyPositions.Where(p => p.TradeType == TradeType.Sell))
                    {

#if TRACE_CLOSE
                        string plus = position.NetProfit > 0 ? "+" : "";
                        logger.LogInformation($"{Server.Time.ToDefaultString()} [CLOSE SHORT {position.Quantity} x {Symbol.Code} @ {Indicator.Symbol.Bid}] {plus}{position.NetProfit}");
#endif
                        if (toClose == null) toClose = new List<Position>();
                        toClose.Add(position);
                    }
                }
                if (toClose != null)
                {
                    foreach (var c in toClose)
                    {
                        var result = ClosePosition(c);
#if TRACE_CLOSE
                        logger.LogTrace(result.ToString());
#endif
                    }
                }
                OnEvaluated();
            }
            catch (InvalidExecutionStateException iex)
            {
                Debug.WriteLine($"{this} InvalidExecutionStateException. {iex.ToString()}");
            }
            catch (Exception ex)
            {
                LastException = ex;
                Debug.WriteLine($"{this} faulted with exception: {ex.ToString()}");
#if cAlgo
                Stop();
#else
                SetState(ExecutionStateEx.Faulted);
#endif
            }

        }
        public Exception LastException { get; set; }


        #endregion

        public Dictionary<Position, BotPosition> BotPositionDict = new Dictionary<Position, BotPosition>();

        #region Position Management


        private void _Open(TradeType tradeType, IndicatorDataSeries indicatorSL)
        {
            if (!CanOpenType(tradeType)) return;

            var price = tradeType == TradeType.Buy ? Symbol.Ask : Symbol.Bid;
            var stopLoss = indicatorSL.LastValue;
            var spread = Symbol.Ask - Symbol.Bid;
            var stopLossDistance = Math.Abs(price - stopLoss);
            stopLossDistance = Math.Max(spread * MinStopLossTimesSpread, stopLossDistance);
            var stopLossDistancePips = stopLossDistance / Symbol.PipSize;
            var risk = stopLossDistance * Symbol.VolumeInUnitsStep;

            var TakeProfitInPips = 0.0;
            var absStopLossDistance = Math.Abs(stopLossDistance);
            var volumeInUnits = GetPositionVolume(absStopLossDistance == 0 ?(double?) null : absStopLossDistance, tradeType);

#if TRACE_RISK
            logger.LogTrace($"Risk calc: Symbol.Ask {Symbol.Ask}, stopLoss {stopLoss} stopLossDist {stopLossDistance.ToString("N3")} SL-Pips: {stopLossDistancePips}");
#endif

#if TRACE_OPEN
            LogOpen(tradeType, volumeInUnits, risk, stopLoss, stopLossDistance);
#endif

            if (IsBacktesting && Template.BacktestProfitTPMultiplierOnSL > 0)
            {
                UseTakeProfit = true;
                TakeProfitInPips = stopLossDistancePips * Template.BacktestProfitTPMultiplierOnSL;
            }

            OpenPosition(tradeType, stopLossDistancePips, UseTakeProfit ? TakeProfitInPips : double.NaN, volumeInUnits);
        }

        private void OpenPosition(TradeType tradeType, double stopLossInPips, double takeProfitInPips, double volumeInUnits)
        {
             
            if (volumeInUnits == 0) return;

            if (Template.MaxStopLossPips > 0)
            {
#if cAlgo
                // TODO: only print if not backtesting
                if(!IsBacktesting) Print($"[sl] MaxStopLossPips {Template.MaxStopLossPips}.  Reducing from {stopLossInPips}.");
#endif
                stopLossInPips = double.IsNaN(stopLossInPips) ? Template.MaxStopLossPips : Math.Min(Template.MaxStopLossPips, stopLossInPips);
#if cAlgo
                if (!IsBacktesting) Print($" -- confirm:  {stopLossInPips}.");
#endif
            }

            //if (tradeType == TradeType.Sell) { stopLossInPips = -stopLossInPips; }

            TradeResult result = null;
            if (Modes.HasFlag(BotMode.Live) || Modes.HasFlag(BotMode.Demo) || IsBacktesting)
            {
                result = ExecuteMarketOrder(tradeType, Symbol, volumeInUnits, Label, stopLossInPips, takeProfitInPips);
            }

            if (!IsBacktesting)
            {
                var accountPosition = result?.Position;

                if (accountPosition != null || Modes.HasFlag(BotMode.Paper) || Modes.HasFlag(BotMode.Scanner))
                {
                    var botPosition = new BotPosition(accountPosition, this, this.Symbol, tradeType, stopLossInPips, takeProfitInPips, volumeInUnits);
                    if (accountPosition != null)
                    {
                        BotPositionDict.Add(accountPosition, botPosition);
                    }
                    OnNewPosition(botPosition);
                }
            }
        }

        protected void OnNewPosition(BotPosition p)
        {
            //var trailers = new List<IOnBar>();

            /*
p.onBars.Add(new StopLossTrailer(p) 
{
    //EndValue = 0.85,
    EndValue = 0.5,
    ValueUnit = Unit.Profit,
    Key = new RangedNumber(15, Unit.Bars),
    Function = DoubleFunctions.Linear
});*/

            //var closePointsTSL = new StopLossTrailerConfig
            //{
            //    Input = new RangedNumber(1, Unit.ClosePoints, 0.2),
            //    StopLossLocation = new RangedNumber(0.85, Unit.NearChannel, 0.5),
            //    Function = DoubleFunctions.Linear
            //};

            //p.onBars.Add(new StopLossTrailer(p, closePointsTSL));
        }

        #endregion

        #region Backtesting

        #region Fitness



        //#if cAlgo
        //        protected
        //#else
        //        public
        //#endif
        //            override double GetFitness(GetFitnessArgs args)
        //        {
        //            var initialBalance = args.History.Count == 0 ? args.Equity : args.History[0].Balance - args.History[0].NetProfit;

        //            var invDrawDown = 1 - (Math.Min(100, args.MaxEquityDrawdownPercentages) / 100);
        //            //var drawDownPenalty = 0.5;
        //            var drawDownPenalty = 0.8;
        //            invDrawDown = Math.Pow(invDrawDown, drawDownPenalty * Math.E);

        //            var tradeCount = args.History.Count;
        //            var tradeCountBonus = 2;
        //            var tradeCountMultiplier = Math.Log(tradeCount, 5 / tradeCountBonus);

        //            var fitness = (args.NetProfit / initialBalance) * invDrawDown * tradeCountMultiplier;




        //            return fitness;
        //        }

        #endregion






        #endregion

        #region Misc

        public string TradeString(TradeType tradeType)
        {
            return tradeType == TradeType.Buy ? "LONG" : "SHORT";
        }

        private void LogOpen(TradeType tradeType, double volumeInUnits, double risk, double stopLoss, double stopLossDistance)
        {
            if (RuntimeSettings.Log != true) return;
            string stopLossDistanceAccount = "";
            var purchaseCurrency = Symbol.Code.Substring(0, 3);
            if (purchaseCurrency != Account.Currency)
            {
                stopLossDistanceAccount = " / " + ConvertToCurrency(stopLossDistance, purchaseCurrency, Account.Currency) + " " + Account.Currency;
            }

            var openPoints = tradeType == TradeType.Buy ? Indicator.OpenLongPoints.LastValue : Indicator.OpenShortPoints.LastValue;
            var price = tradeType == TradeType.Buy ? Indicator.Symbol.Ask : Indicator.Symbol.Bid;

#if cAlgo
            var dateStr = this.MarketSeries.OpenTime.LastValue.ToDefaultString();
            logger.LogInformation($"{dateStr} [{TradeString(tradeType)} {volumeInUnits} {Symbol.Code} @ {price}] SL: {stopLoss} (dist: {stopLossDistance.ToString("N3")}{stopLossDistanceAccount}) risk: {risk.ToString("N2")}");
#else
            var dateStr = this.Account.Server.Time.ToDefaultString();
            logger.LogInformation($"{dateStr} [{TradeString(tradeType)} {volumeInUnits} {Symbol.Code} @ {price}] SL: {stopLoss} (dist: {stopLossDistance.ToString("N3")}{stopLossDistanceAccount}) risk: {risk.ToString("N2")}");
#endif
        }

        #endregion

        #region TOREVIEW

        #region Event Handlers

        //#if cAlgo
        //        protected override void OnTick()
        //        {
        //            base.OnTick();
        //            Symbol_Tick(new SymbolTick()); // REFACTOR to base
        //        }
        //#endif





        #endregion

        #endregion

    }

}
