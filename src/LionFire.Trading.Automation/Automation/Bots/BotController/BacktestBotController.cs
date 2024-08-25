
namespace LionFire.Trading.Automation;

public class BacktestBotController<TPrecision> : IBotController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Relationships

    public IBotBatchController BotBatchController { get; }
    public IBot2 Bot { get; }
    public PBacktestAccount<double> PBacktestAccount { get; }

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => (Bot.Parameters as IPBarsBot2)?.ExchangeSymbolTimeFrame;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestBotController(IBotBatchController botBatchController, IBot2 bot, PBacktestAccount<TPrecision> pBacktestAccount)
    {
        BotBatchController = botBatchController;
        Bot = bot;
        PBacktestAccount = pBacktestAccount;
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


    protected BacktestAccount2<TPrecision> CreateAccount(ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame)
        => new BacktestAccount2<TPrecision>(PBacktestAccount, this, ExchangeSymbolTimeFrame.Exchange, ExchangeSymbolTimeFrame.ExchangeArea, ExchangeSymbolTimeFrame.Symbol);

    #endregion

}


