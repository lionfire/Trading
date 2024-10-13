
using CryptoExchange.Net.CommonObjects;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation;


public class BacktestBotController<TPrecision> : IBotController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    public long Id { get; internal set; }

    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }
    public PBacktestAccount<TPrecision> PBacktestAccount { get; }
    public ITradeJournal<TPrecision> Journal { get; }

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => (Bot.Parameters as IPBarsBot2)?.ExchangeSymbolTimeFrame;
    public DateTimeOffset SimulatedCurrentDate => BotBatchController.SimulatedCurrentDate;
    public DateTimeOffset StartTime => BotBatchController.Start;

    #endregion

    #endregion

    #region Lifecycle

    public static async ValueTask<BacktestBotController<TPrecision>> Create(IBotBatchController botBatchController, IBot2 bot, PBacktestAccount<TPrecision> pBacktestAccount, ITradeJournal<TPrecision> journal)
    {
        var c = new BacktestBotController<TPrecision>(botBatchController, bot, pBacktestAccount, journal);
        await c.OnStarting();
        return c;
    }

    private BacktestBotController(IBotBatchController botBatchController, IBot2 bot, PBacktestAccount<TPrecision> pBacktestAccount, ITradeJournal<TPrecision> journal)
    {
        BotBatchController = botBatchController;
        Bot = bot;
        PBacktestAccount = pBacktestAccount;
        Journal = journal;
        //Journal.ExchangeSymbol = (bot.Parameters as IPSymbolBot2)?.ExchangeSymbol;
        var e = ExchangeSymbolTimeFrame;
        if (e != null)
        {
            account = CreateAccount(e);

        }
        else
        {
            throw new NotImplementedException("TODO: How do we know which Exchange the account is on?");
        }
    }

    #endregion

    #region State

    public ISimulatedAccount2<TPrecision> Account => account;


    private readonly BacktestAccount2<TPrecision> account;

    public long GetNextTransactionId() => account.GetNextTransactionId();


    protected BacktestAccount2<TPrecision> CreateAccount(ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame)
        => new BacktestAccount2<TPrecision>(PBacktestAccount, this, ExchangeSymbolTimeFrame.Exchange, ExchangeSymbolTimeFrame.ExchangeArea, ExchangeSymbolTimeFrame.Symbol);


    #endregion

    #region Methods

    public async ValueTask OnStarting()
    {
        Journal.Write(new JournalEntry<TPrecision>(account)
        {
            EntryType = JournalEntryType.Start,
            Time = BotBatchController.Start,
        });
    }

    static int x = 0;

    public static double GetFitness(BacktestResult backtestResult)
    {
        return backtestResult.AD;
    }

    public async ValueTask OnFinished()
    {
        await Bot.OnBacktestFinished();

        var result = new BacktestResult
        {
            Aborted = Account.IsAborted,
            MaxBalanceDrawdown = Convert.ToDouble(Account.CurrentBalanceDrawdown),
            MaxBalanceDrawdownPercentages = Convert.ToDouble(Account.MaxBalanceDrawdownPerunum),
            MaxEquityDrawdown = Convert.ToDouble(Account.CurrentEquityDrawdown),
            MaxEquityDrawdownPercentages = Convert.ToDouble(Account.MaxEquityDrawdownPercent),
            AD = Account.AnnualizedBalanceReturnOnInvestmentVsDrawdownPercent,

            //Fitness = ,
            //WinningTrades = Account.WinningTrades,
            Id = "id-" + x++
        };
        result.Fitness = GetFitness(result);

        Journal.FileName = $"{(result.Aborted ? "ABORTED" : "")} {result.Fitness:0.000}f {(result.AD == result.Fitness ? "" : result.AD.ToString("0.0"))}ad  id={result.Id} {(result.MaxBalanceDrawdownPercentages * 100.0).ToString("0.0")}bddp";
        await Journal.CloseAll();
    }

    public void OnAccountAborted()
    {
        Journal.Write(new JournalEntry<TPrecision>(Account)
        {
            EntryType = JournalEntryType.Abort,
            Time = BotBatchController.SimulatedCurrentDate,
        });
    }

    #endregion
}


