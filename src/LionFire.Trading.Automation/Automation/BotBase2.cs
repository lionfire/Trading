namespace LionFire.Trading.Automation;

public interface IBotController
{

}

public class LiveBotController
{

}
public class BacktestBotController
{

}

public class BotBase2<TParameters> : IBot2
    where TParameters : PBot2
{
    public TParameters Parameters => parameters;
    private TParameters parameters;

    private IBotController controller;

    public BotBase2(TParameters parameters, IBotController botController)
    {
        this.parameters = parameters;
        controller = botController;

        if(parameters is PTimeFrameBot2 ptf)
        {

        }
    }

    public virtual void OnBar(IKline kline) { }


    #region Controlling the bot

    // TODO NEXT

    // - Live bot: catch up
    //   - Open/Close positions: result code "deferred"
    //   - default policies:
    //     - close: after full catch-up
    //     - open: defer
    //       - ENH: if < x% on the way to average profit, and expected profit per trade - x > 0 (including commissions)
    //   - ENH: Show deferred actions to user who just (re)started the bot and let them have discretion
    // - Live bot: OnTick
    // - Backtest bot: OnTick
    // - Backtest bot: OnBar
    
    // Lower priority, since bots will typically run live with ticks and we will be able to confirm bars (unless there's lag)
    // - Live bot: tentative bar becomes available
    // - Live bot: confirmed bar becomes available
    // - Live bot: revision bar becomes available

    #endregion


}
