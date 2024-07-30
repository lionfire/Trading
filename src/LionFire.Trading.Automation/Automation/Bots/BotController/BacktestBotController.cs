using System.Numerics;

namespace LionFire.Trading.Automation;



public class BacktestBotController : IBotController
{
    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }

    #region Derived

    public ExchangeSymbol? ExchangeSymbol => (Bot as IPSymbolBarsBot2)?.ExchangeSymbol;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestBotController(IBotBatchController botBatchController, IBot2 bot)
    {
        BotBatchController = botBatchController;
        Bot = bot;
        if (Bot.Parameters is IPSymbolBarsBot2 s)
        {
            account = CreateAccount(s.ExchangeSymbol);
        }
    }

    #endregion

    #region State

    public IAccount2<double>? Account => account;
    private readonly BacktestAccount2<double>? account;

    protected BacktestAccount2<double> CreateAccount(ExchangeId exchange)
        => new BacktestAccount2<double>(this, exchange.Exchange, exchange.ExchangeArea);

    #endregion

    //public ExchangeSymbol? PrimaryExchangeSymbol
    //{
    //}

}


