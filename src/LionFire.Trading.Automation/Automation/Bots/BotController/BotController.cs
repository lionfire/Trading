namespace LionFire.Trading.Automation;

public class BotController : IBotController
{
    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }

    #endregion

    #region Lifecycle

    public BotController(IBotBatchController botBatchController, IBot2 bot)
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
        throw new NotImplementedException();
    }
}


