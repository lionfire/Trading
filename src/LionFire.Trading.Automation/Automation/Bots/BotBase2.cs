
using LionFire.Ontology;

namespace LionFire.Trading.Automation;

// TODO?
//public abstract class BotBase2<TConcrete, TParameters>
//    where TConcrete : IBot2
//{
//}

public interface IBotContext
{

}

public class BotContext
{
    public IAccount2 Account { get; set; }

}

public abstract class BotBase2<TParameters>
    : IBot2<TParameters>
    where TParameters : PBot2<TParameters>
{
    //public abstract IReadOnlyList<IInputSignal> InputSignals { get; }

    #region Dependencies

    #endregion

    #region Relationships

    #region IBotController

    public IBotController? Controller
    {
        get => controller;
        set
        {
            if (controller != null && controller != value) throw new AlreadySetException();
            controller = value;
        }
    }
    private IBotController? controller = null!; // OnBar, OnTick are guaranteed to have PBacktests set

    #endregion

    #endregion

    #region TParameters

    public TParameters Parameters
    {
        get => parameters;
        set
        {
            parameters = value;
            OnParametersSet();
        }
    }
    private TParameters parameters = null!; // OnBar, OnTick are guaranteed to have PBacktests set
    object IBot2.Parameters { get => parameters; set => parameters = (TParameters)value; }

    protected virtual void OnParametersSet()
    {
    }
    #endregion

    #region Lifecycle

    //public BotBase2(TParameters parameters, IBotController? botController = null)
    //{
    //    this.parameters = parameters;
    //    controller = botController;

    //    if (parameters is IPTimeFrameBot2 ptf)
    //    {

    //    }
    //}

    public virtual void Init() { }

    #endregion

    public virtual void OnBar() { }

    //public static IReadOnlyList<InputSlot> InputSlots()
    //{
    //    throw new NotImplementedException();
    //}


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
