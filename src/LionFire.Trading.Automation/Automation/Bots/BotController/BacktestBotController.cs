
using CryptoExchange.Net.CommonObjects;
using LionFire.Trading.Journal;

namespace LionFire.Trading.Automation;

public class BacktestBotController<TPrecision> : IBotController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }
    public PBacktestAccount<TPrecision> PBacktestAccount { get; }
    public ITradeJournal<TPrecision> Journal { get; }

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => (Bot.Parameters as IPBarsBot2)?.ExchangeSymbolTimeFrame;
    public DateTimeOffset SimulatedCurrentDate => BotBatchController.SimulatedCurrentDate;

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

    public IAccount2<TPrecision> Account => account;
    private readonly BacktestAccount2<TPrecision> account;

    public long GetNextTransactionId() => account.GetNextTransactionId();


    protected BacktestAccount2<TPrecision> CreateAccount(ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame)
        => new BacktestAccount2<TPrecision>(PBacktestAccount, this, ExchangeSymbolTimeFrame.Exchange, ExchangeSymbolTimeFrame.ExchangeArea, ExchangeSymbolTimeFrame.Symbol);


    #endregion

    public async ValueTask OnStarting()
    {
        await Journal.Write(new JournalEntry<TPrecision>(account)
        {
            EntryType = JournalEntryType.JournalOpen,
            Time = BotBatchController.Start,
        });
    }
}


