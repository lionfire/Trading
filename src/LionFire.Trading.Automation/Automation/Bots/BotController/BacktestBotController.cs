namespace LionFire.Trading.Automation;

public class BacktestBotController : IBotController
{
    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }
    public SimulatedAccount2<double>? PrimaryAccount { get; } = null!;  // TODO

    #endregion

    #region Lifecycle

    public BacktestBotController(IBotBatchController botBatchController, IBot2 bot)
    {
        BotBatchController = botBatchController;
        Bot = bot;
    }

    #endregion

    //public ExchangeSymbol? PrimaryExchangeSymbol
    //{
    //}

    public IAccount2 GetAccount(ExchangeId exchange)
    {
        var account = new BacktestFuturesAccount2<double>(this, exchange.Exchange, exchange.ExchangeArea);
        return account;
    }
}


