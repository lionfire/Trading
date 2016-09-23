//#define TRACE_EQUITY
//#define TRACE_BALANCE
using LionFire.Extensions.Logging;
using LionFire.Trading.Bots;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{

    public class BacktestAccount : MarketParticipant, IAccount
    {
        #region Relationships

        public BacktestMarket BacktestMarket {
            get {
                return Market as BacktestMarket;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Market.Ticked += Market_Ticked;
        }

        private void Market_Ticked()
        {
            //logger.LogInformation("BacktestAccount - " + Market.Server.Time.ToDefaultString());
            UpdatePositions();
        }

        #endregion

        #region Construction

        public BacktestAccount()
        {
            logger = this.GetLogger();
        }
        public BacktestAccount(string brokerName) : this()
        {
            this.BrokerName = brokerName;
        }

        #endregion

        #region State
        
        #region Equity

        public double Equity {
            get { return equity; }
            set {
#if TRACE_EQUITY
                if (equity == value) return;
                var lastValue = equity;
#endif
                lastEquity = equity;
                equity = value;
                MaxEquity = double.IsNaN(MaxEquity) ? equity : Math.Max(MaxEquity, equity);
                MinEquity = double.IsNaN(MinEquity) ? equity : Math.Min(MinEquity, equity);
                if (double.IsNaN(lastEquity)) lastEquity = equity;
#if TRACE_EQUITY
                logger.LogInformation(Market.Server.Time.ToDefaultString() + " Eq: " + equity.ToCurrencyString());
#endif
            }
        }
        private double equity;

        #endregion

        #region Balance

        public double StartingBalance { get; set; }
        public double NetProfitPercent {
            get {
                return (Equity - StartingBalance) / StartingBalance;
            }
        }

        public double Balance {
            get { return balance; }
            set {
#if TRACE_BALANCE
                if (balance == value) return;
                var lastValue = balance;
#endif

                if (double.IsNaN(balance))
                {
                    if (!double.IsNaN(value))
                    {
                        StartingBalance = value;
                    }
                }
                lastBalance = balance;
                balance = value;
                MaxBalance = double.IsNaN(MaxBalance) ? balance : Math.Max(MaxBalance, balance);
                MinBalance = double.IsNaN(MinBalance) ? balance : Math.Min(MinBalance, balance);
                if (double.IsNaN(lastBalance)) lastBalance = balance;
#if TRACE_BALANCE
                logger.LogInformation(Market.Server.Time.ToDefaultString() + " Bal: " + balance.ToCurrencyString());
#endif
            }
        }
        private double balance = double.NaN;

        #endregion


        public double MarginUsed { get; set; }

        IPositions IAccount.Positions { get { return this.Positions; } }
        public Positions Positions { get; private set; } = new Positions();

        public double StopOutLevel { get { return BacktestMarket.Config.StopOutLevel; } }

        #endregion

        #region Account info

        #region BrokerName

        public string BrokerName {
            get { return brokerName; }
            set {
                brokerName = value;
                this.AccountInfo = BrokerInfoUtils.GetAccountInfo(BrokerName);
            }
        }
        private string brokerName;

        #endregion

        public string Currency {
            get { return EffecitveAccountInfo.Currency; }
        }

        public bool IsDemo {
            get { return true; }
        }

        protected AccountInfo EffecitveAccountInfo { get { return AccountInfo ?? DefaultAccountInfo; } }
        public AccountInfo AccountInfo { get; set; }

        #endregion

        #region (Static) Defaults

        public static AccountInfo DefaultAccountInfo {
            get {
                if (defaultAccountInfo == null)
                {
                    defaultAccountInfo = new AccountInfo()
                    {
                        CommissionPerMillion = 0.0,
                        BrokerName = "(default)",
                        AccountNumber = 0,
                        IsLive = false,
                        Currency = "USD",
                        Leverage = 100.0,
                    };
                }
                return defaultAccountInfo;
            }
        }
        private static AccountInfo defaultAccountInfo;

        #endregion

        #region Derived

        public double MarginLevel { get { return Equity / MarginUsed; } }
        public double MarginLevelPercent { get { return 100.0 * Equity / MarginUsed; } }

        public double FreeMargin { get { return Equity - MarginUsed; } }

        #endregion

        #region Initialization

        protected override void OnStarting()
        {
            base.OnStarting();

            Equity = Balance = (this.Market as BacktestMarket).Config.StartingBalance;
        }

        #endregion

        public int positionCounter = 1;

        #region (Public) Methods

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null)
        {
            var slippage = 0;
            var entryPrice = tradeType == TradeType.Buy ? symbol.Bid + slippage : symbol.Ask - slippage;

            // TODO: Commission per trade on stocks!!

            var p = new Position()
            {
                Comment = comment,
                Id = positionCounter++,
                EntryTime = Market.SimulationTime,
                EntryPrice = entryPrice,
                Commissions = volume * EffecitveAccountInfo.CommissionPerMillion / 1000000.0,
                Label = "Backtest (LFT)",
                SymbolCode = symbol.Code, // REVIEW - eliminate this
                Symbol = symbol,
                TradeType = tradeType,
                Volume = volume,
            };
            Positions.Add(p);

            return new TradeResult
            {
                Error = ErrorCode.TechnicalError
            };

        }

        public _History history { get; private set; } = new _History();
        public bool SaveHistory {
            get { return history != null; }
            set {
                if (value)
                {
                    if (history == null)
                    {
                        history = new _History();
                    }
                }
                else
                {
                    history = null;
                }
            }
        }


        public TradeResult ClosePosition(Position position)
        {
            if (!this.Positions.Contains(position))
            {
                return new TradeResult
                {
                    IsSuccessful = false,
                    Message = "Position does not exist",
                };
            }
            this.Balance += position.NetProfit;
            Positions.Remove(position);

            if (SaveHistory)
            {
                history.Add(new _HistoricalTrade(this, position));
            }

            return new TradeResult
            {
                Message = $"Closed position (#{position.Id}) {position.Volume} {position.SymbolCode} for profit of {position.NetProfit}",
            };
        }

        public TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region (Private) Methods

        public double MaxEquity { get; set; }
        public double MinEquity { get; set; } = double.NaN;
        public double MaxEquityDrawdown { get; set; }
        public double MaxEquityDrawdownPercent { get; set; }
        public double lastEquity { get; set; } = double.NaN;

        public double MaxBalance { get; set; }
        public double MinBalance { get; set; } = double.NaN;
        public double MaxBalanceDrawdown { get; set; }
        public double MaxBalanceDrawdownPercent { get; set; }
        public double lastBalance { get; set; } = double.NaN;

        public void UpdatePositions()
        {

            var marginUsed = 0.0;
            var netProfit = 0.0;
            foreach (var position in Positions)
            {
                var exitPrice = position.TradeType == TradeType.Buy ? position.Symbol.Bid : position.Symbol.Ask;
                //position.GrossProfit =
                    var grossProfit = ((exitPrice - position.EntryPrice) / position.Symbol.TickSize) * position.Symbol.TickValue;
                
                if (position.TradeType == TradeType.Sell)
                {
                    grossProfit *= -1;
                }
                position.GrossProfit = grossProfit;
                netProfit += position.NetProfit;
                marginUsed += exitPrice*position.Volume / position.Symbol.PreciseLeverage; // TODO: Convert to account currency!!!
            }

            this.Equity = Balance + netProfit;
           
            if (!double.IsNaN(Equity) && lastEquity != Equity)
            {
                MaxEquityDrawdown = Math.Max(MaxEquityDrawdown, MaxEquity - Equity);
                MaxEquityDrawdownPercent = Math.Max(MaxEquityDrawdownPercent, (MaxEquity - Equity) / MaxEquity);
            }
            if (!double.IsNaN(Balance) && lastBalance != Balance)
            {
                MaxBalanceDrawdown = Math.Max(MaxBalanceDrawdown, MaxBalance - Balance);
                MaxBalanceDrawdownPercent = Math.Max(MaxBalanceDrawdownPercent, (MaxBalance - Balance) / MaxBalance);
            }

            this.MarginUsed = marginUsed;

            if (this.MarginLevel < this.StopOutLevel)
            {
                logger.LogWarning($"TODO - STOP OUT at {StopOutLevel * 100.0}% (current margin level: {MarginLevelPercent}%)");
            }
            //logger.LogInformation($"{Market.Server.Time.ToDefaultString()} Eq: {Equity}  Bal: {Balance}");
        }

        public GetFitnessArgs GetFitnessArgs()
        {

            var result = new _GetFitnessArgs()
            {
                AverageTrade = history.Select(p=>p.NetProfit).Average(),
                Equity = Equity,
                History = this.history,
                LosingTrades = history.Where(p => p.NetProfit < 0).Count(),
                MaxBalanceDrawdown = this.MaxBalanceDrawdown,
                MaxBalanceDrawdownPercentages = this.MaxBalanceDrawdownPercent,
                MaxEquityDrawdown = this.MaxEquityDrawdown,
                MaxEquityDrawdownPercentages = this.MaxEquityDrawdownPercent,
                NetProfit = Equity - StartingBalance,
                ProfitFactor = history.Select(p=>p.NetProfit).Where(np=>np > 0.0).Sum() / history.Select(p => p.NetProfit).Where(np => np < 0.0).Sum(), // TOVERIFY
                SortinoRatio = double.NaN, // TODO
                SharpeRatio = double.NaN, // FUTURE
                TotalTrades = history.Count,
                WinningTrades = history.Where(p => p.NetProfit >= 0).Count(),
            };
            return result;
        }

        #endregion

        #region Event Handling

        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            base.OnBar(symbolCode, timeFrame, bar);

            UpdatePositions();
        }

        public override void OnTick(SymbolBar bar)
        {
            base.OnTick(bar);

            UpdatePositions();
        }




        #endregion

        #region Misc

        private static ILogger logger;

        #endregion
    }
}
