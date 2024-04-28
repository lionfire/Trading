
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


// TODO?
//public abstract class BotBase2<TConcrete, TParameters>
//    where TConcrete : IBot2
//{
//}

public abstract class BotBase2<TParameters> 
    where TParameters : PBot2<TParameters>
{
    public TParameters Parameters => parameters;

    public abstract IReadOnlyList<IInputSignal> InputSignals { get; }

    private TParameters parameters;

    private IBotController controller;

    public BotBase2(TParameters parameters, IBotController botController)
    {
        this.parameters = parameters;
        controller = botController;

        if(parameters is IPTimeFrameBot2 ptf)
        {

        }
    }

    public virtual void OnBar(IKline kline) { }

    public static IReadOnlyList<InputSlot> InputSlots()
    {
        throw new NotImplementedException();
    }


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
