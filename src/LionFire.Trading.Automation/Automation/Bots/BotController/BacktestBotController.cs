
using CryptoExchange.Net.CommonObjects;
using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LionFire.Trading.Automation;


public sealed class BacktestBotController<TPrecision> : IBotController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    public long Id { get; internal set; }

    #region Relationships

    public IBotBatchController BotBatchController => botBatchController;
    private BotBatchControllerBase botBatchController;
    public IBot2 Bot { get; }
    public PBacktestAccount<TPrecision> PBacktestAccount { get; }
    public BacktestTradeJournal<TPrecision> Journal { get; }
    IBacktestTradeJournal<TPrecision> ISimulationController<TPrecision>.Journal => Journal;

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => (Bot.Parameters as IPBarsBot2)?.ExchangeSymbolTimeFrame;
    public DateTimeOffset SimulatedCurrentDate => BotBatchController.SimulatedCurrentDate;
    public DateTimeOffset StartTime => BotBatchController.Start;

    #endregion

    #endregion

    #region Lifecycle

    public static async ValueTask<BacktestBotController<TPrecision>> Create(IBotBatchController botBatchController, IBot2 bot, PBacktestAccount<TPrecision> pBacktestAccount, BacktestTradeJournal<TPrecision> journal)
    {
        var c = new BacktestBotController<TPrecision>(botBatchController, bot, pBacktestAccount, journal);
        await c.OnStarting();
        return c;
    }

    private BacktestBotController(IBotBatchController botBatchController, IBot2 bot, PBacktestAccount<TPrecision> pBacktestAccount, BacktestTradeJournal<TPrecision> journal)
    {
        this.botBatchController = (BotBatchControllerBase)botBatchController;
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
            MaxBalanceDrawdownPerunum = Convert.ToDouble(Account.MaxBalanceDrawdownPerunum),
            MaxEquityDrawdown = Convert.ToDouble(Account.CurrentEquityDrawdown),
            MaxEquityDrawdownPercentages = Convert.ToDouble(Account.MaxEquityDrawdownPerunum),
            AD = Account.AnnualizedBalanceReturnOnInvestmentVsDrawdownPercent,

            //Fitness = ,
            //WinningTrades = Account.WinningTrades,
            Id = "id-" + x++
        };
        result.Fitness = GetFitness(result);

        Journal.FileName = $"{(result.Aborted ? "ABORTED" : "")} {result.Fitness:0.000}f {(result.AD == result.Fitness ? "" : result.AD.ToString("0.0"))}ad  id={result.Id} {(result.MaxBalanceDrawdownPerunum * 100.0).ToString("0.0")}bddp";

        if (double.IsNaN(result.Fitness) || result.Fitness < Journal.Options.DiscardDetailsWhenFitnessBelow
            || !Context.ShouldLogTradeDetails) { Journal.DiscardDetails = true; }

        if (!Journal.DiscardDetails)
        {
            if (result.Aborted) { Journal.IsAborted = true; }
            else
            {
            }
        }

        await Journal.Finish(result.Fitness);
        await Journal.DisposeAsync();
    }

    MultiBacktestContext Context => botBatchController.Context;

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


