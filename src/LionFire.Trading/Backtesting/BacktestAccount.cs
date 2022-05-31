//#define TRACE_EQUITY
//#define TRACE_BALANCE
using LionFire.Assets;
using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.ExtensionMethods.Copying;
using LionFire.Extensions.Logging;
using LionFire.Instantiating;
using LionFire.Trading;
using LionFire.Trading.Accounts;
using LionFire.Trading.Bots;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{

    public class BacktestAccount : SimulatedAccountBase<TBacktestAccount, BacktestAccount>
    {
        #region Construction

        public BacktestAccount(ILogger logger, ILoggerFactory loggerFactory) : base(logger, loggerFactory)
        {
        }
        public BacktestAccount() : base(null, null) { }

        #endregion


        #region State



        #region Equity

        public override double Equity
        {
            get { return equity; }
            protected set
            {
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
        public override decimal EquityDecimal
        {
            get => (decimal)Equity;
            protected set => Equity = (double) value;
        }

        #endregion

        #region Balance

        public double StartingBalance { get; set; }
        public double NetProfitPercent => (Equity - StartingBalance) / StartingBalance;

        public override double Balance
        {
            get { return balance; }
            protected set
            {
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

        public override decimal BalanceDecimal
        {
            get => (decimal)Balance;
            protected set => Balance = (double)value;
        }

        #endregion

        #endregion

        public override DateTime BacktestEndDate { get { return Template.EndDate; } }

        #region Account Info

        protected TAccount EffecitveAccountInfo
        {
            get
            {
                return Template
                    //??  DefaultAccountInfo
                    ;
            }
        }

        #endregion

        #region (Static) Defaults

        //public static TBacktestAccount DefaultAccountInfo
        //{
        //    get
        //    {
        //        if (defaultAccountInfo == null)
        //        {
        //            defaultAccountInfo = new TBacktestAccount()
        //            {
        //                CommissionPerMillion = 0.0,
        //                Exchange = "(default)",
        //                AccountId = "(default)",
        //                IsLive = false,
        //                Currency = "USD",
        //                Leverage = 100.0,
        //            };
        //        }
        //        return defaultAccountInfo;
        //    }
        //}
        //private static TBacktestAccount defaultAccountInfo;

        #endregion

        #region Derived


        public double MarginLevel { get { return Equity / MarginUsed; } }
        public double MarginLevelPercent { get { return 100.0 * Equity / MarginUsed; } }

        public double FreeMargin { get { return Equity - MarginUsed; } }

        #endregion

        #region Informational Properties

        public override bool IsBacktesting { get { return true; } }

        #endregion

        #region Initialization

        bool isInitialized = false;

        public async override Task<bool> Initialize()
        {
            if (isInitialized) return true;


            if (Template.SimulateAccount != null)
            {
                throw new NotImplementedException();
#if TOPORT
                TAccount simulatedAccount = await Template.SimulateAccount.Load<TAccount>();
                Template.AssignPropertiesFrom(simulatedAccount);
#endif
            }

            this.TimeFrame = Template.TimeFrame;
            this.StartDate = Template.StartDate;
            this.EndDate = Template.EndDate;

            if (await base.Initialize().ConfigureAwait(false) == false) { return false; }

            Equity = Balance = Template.StartingBalance;

            isInitialized = true;

            return true;
        }

        #endregion

        public int positionCounter = 1;

        #region (Public) Trading Methods

        public override TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, double volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null)
        {
            var slippage = 0;
            var entryPrice = tradeType == TradeType.Buy ? symbol.Bid + slippage : symbol.Ask - slippage;

            // TODO: Commission per trade on stocks!!

            var p = new PositionDouble()
            {
                Comment = comment,
                Id = positionCounter++,
                EntryTime = ServerTime,
                EntryPrice = entryPrice,
                Commissions = volume * EffecitveAccountInfo.CommissionPerMillion / 1000000.0,
                Label = "Backtest (LFT)",
                SymbolCode = symbol.Code, // REVIEW - eliminate this
                Symbol = symbol,
                TradeType = tradeType,
                Volume = volume,
            };
            positions.Add(p);

            return new TradeResult
            {
                Error = ErrorCode.TechnicalError
            };

        }

        public override TradeResult ClosePosition(PositionDouble position)
        {
            if (!this.Positions.Contains(position))
            {
                return new TradeResult
                {
                    IsSuccessful = false,
                    Message = "Position does not exist",
                };
            }
#if SanityChecks
            if (double.IsNaN(position.NetProfit))
            {
                throw new Exception("ClosePosition: position.NetProfit is null");
            }
#endif
            this.Balance += position.NetProfit;
            positions.Remove(position);

            if (SaveHistory)
            {
                history.Add(new _HistoricalTrade(this, position));
            }

            return new TradeResult
            {
                Message = $"Closed position (#{position.Id}) {position.Volume} {position.SymbolCode} for profit of {position.NetProfit}",
            };
        }

        public override TradeResult ModifyPosition(PositionDouble position, double? stopLoss, double? takeProfit)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Statistics

        public _History history { get; private set; } = new _History();
        public bool SaveHistory
        {
            get { return history != null; }
            set
            {
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

        #endregion

        #region Backtesting

        public override GetFitnessArgs GetFitnessArgs()
        {

            var result = new _GetFitnessArgs()
            {
                AverageTrade = history.Any() ? history.Select(p => p.NetProfit).Average() : double.NaN,
                Equity = Equity,
                History = this.history,
                LosingTrades = history.Where(p => p.NetProfit < 0).Count(),
                MaxBalanceDrawdown = this.MaxBalanceDrawdown,
                MaxBalanceDrawdownPercentages = this.MaxBalanceDrawdownPercent,
                MaxEquityDrawdown = this.MaxEquityDrawdown,
                MaxEquityDrawdownPercentages = this.MaxEquityDrawdownPercent,
                NetProfit = Equity - StartingBalance,
                ProfitFactor = history.Select(p => p.NetProfit).Where(np => np > 0.0).Sum() / history.Select(p => p.NetProfit).Where(np => np < 0.0).Sum(), // TOVERIFY
                SortinoRatio = double.NaN, // TODO
                SharpeRatio = double.NaN, // FUTURE
                TotalTrades = history.Count,
                WinningTrades = history.Where(p => p.NetProfit >= 0).Count(),
            };
            return result;
        }

        #endregion

        #region (Private) Methods


        public void UpdatePositions()
        {
            var marginUsed = 0.0;
            var netProfit = 0.0;

            foreach (var position in Positions)
            {
                netProfit += position.NetProfit;
                marginUsed += position.CurrentExitPrice * position.Volume / position.Symbol.PreciseLeverage; // TODO: Convert to account currency!!!
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
                Logger.LogWarning($"TODO - STOP OUT at {StopOutLevel * 100.0}% (current margin level: {MarginLevelPercent}%)");
            }
            //logger.LogInformation($"{Market.Server.Time.ToDefaultString()} Eq: {Equity}  Bal: {Balance}");
        }

        #endregion

        #region Event Handling

        protected override void RaiseTicked()
        {
            base.RaiseTicked();
            //logger.LogInformation("BacktestAccount - " + Market.Server.Time.ToDefaultString());
            UpdatePositions();
        }

        #endregion

        #region Market Series

        public override MarketSeries CreateMarketSeries(string code, TimeFrame timeFrame)
        {
            return new MarketSeries(this, code, timeFrame);
        }

        #endregion

        public override Task<IPositions> RefreshPositions(CancellationToken cancellationToken = default) => Task.FromResult(Positions2);

        public override IEnumerable<string> SymbolsAvailable
        {
            get
            {
                return BrokerInfoUtils.GetSymbolsAvailable(Template?.Exchange);
            }
        }

        #region Event Handling

        protected override void OnLastExecutedTimeChanged()
        {
#if SanityChecks
            if (LastExecutedTime != default(DateTime) && LastExecutedTime != Template.StartDate && double.IsNaN(Account.Balance))
            {
                throw new InvalidOperationException("Backtest in progress while Account Balance is NaN");
            }
#endif
            base.OnLastExecutedTimeChanged(); // Does nothing at the moment
            this.UpdatePositions();
        }

        #endregion

    }
}
