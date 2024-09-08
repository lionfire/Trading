
using DynamicData;
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
    public required IAccount2 Account { get; set; }

}

public abstract class BotBase2<TParameters, TPrecision>
    : IBot2<TParameters, TPrecision>
    where TParameters : PBot2<TParameters>, IPMarketProcessor
    where TPrecision : struct, INumber<TPrecision>
{
    //public abstract IReadOnlyList<IInputSignal> InputSignals { get; }

    #region Identity

    public string BotId { get; set; } = Guid.NewGuid().ToString();

    #endregion

    #region Relationships

    #region IBotController

    public IBotController<TPrecision>? Controller
    {
        get => controller;
        set
        {
            if (controller != null && controller != value) throw new AlreadySetException();
            controller = value;
        }
    }
    private IBotController<TPrecision>? controller = null!; // OnBar, OnTick are guaranteed to have PBacktests set

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
    IPMarketProcessor IBot2.Parameters { get => parameters; set => parameters = (TParameters)value; }

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

    #region State

    public IObservableCache<IPosition, int> Positions => positions;
    protected SourceCache<IPosition, int> positions = new(p => p.Id); // OPTIMIZE idea: if it has a dedicated SimulatedAccount, use its position list directly instead of this field.

    //public IEnumerable<IPosition> CompatiblePositions => Positions.Items.Where(p => p.SymbolId.Symbol == Symbol);
    //public IEnumerable<IPosition> BotPositions => CompatiblePositions.Where(p => p.SymbolId.Symbol == Symbol && p.Label == );

    #endregion

    #region Methods

    #region Positions

    public async ValueTask CloseAllPositions()
    {
        await Task.WhenAll(Positions.Items.Select(p => Controller.Account.ClosePosition(p).AsTask())).ConfigureAwait(false);
    }

    #endregion

    #endregion

    #region Event Handlers

    public virtual void OnBar() { }
    public virtual async ValueTask Stop()
    {
        await CloseAllPositions();
    }
    public virtual async ValueTask OnBacktestFinished()
    {
        await Stop();

    }

    #endregion


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
