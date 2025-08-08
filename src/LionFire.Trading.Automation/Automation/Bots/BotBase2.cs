using DynamicData;

namespace LionFire.Trading.Automation;

public interface IBotContext
{
    long Id { get; }
    IBot2 Bot { get; }

    ValueTask OnFinished();
}


public abstract class BotBase2<TParameters, TPrecision>
    : MarketParticipantBase<TPrecision>
    , IBot2<TParameters, TPrecision>
    where TParameters : PBot2<TParameters>, IPMarketProcessor
    where TPrecision : struct, INumber<TPrecision>
{
    //public abstract IReadOnlyList<IInputSignal> InputSignals { get; }
    public float ListenOrder => ListenerOrders.Bot;


    #region Identity

    public string BotId { get; set; } = Guid.NewGuid().ToString();

    #endregion

    #region Relationships

    #region BotContext

    public BotContext<TPrecision>? Context
    {
        get => context;
        set
        {
            if (context != null && context != value) throw new AlreadySetException();
            context = value;
        }
    }
    private BotContext<TPrecision>? context = null!; // OnBar, OnTick are guaranteed to have PBacktests set
    IBotContext IBot2.Context { get => Context ?? throw new InvalidOperationException(); set => Context = (BotContext<TPrecision>?)value; }

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
    IPBot2 IBot2.Parameters { get => parameters; set => parameters = (TParameters)value; }
    IPMarketProcessor IMarketListener.Parameters => Parameters;

    protected virtual void OnParametersSet()
    {
    }
    #endregion

    #region Lifecycle

    //public BotBase2(TParameters parameters, IBotContext? botController = null)
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

    public IObservableCache<IPosition<TPrecision>, int> Positions => positions;


    protected SourceCache<IPosition<TPrecision>, int> positions = new(p => p.Id); // OPTIMIZE idea: if it has a dedicated DefaultAccount, use its position list directly instead of this field.

    //public IEnumerable<IPosition> CompatiblePositions => Positions.Items.Where(p => p.SymbolId.DefaultSymbol == DefaultSymbol);
    //public IEnumerable<IPosition> BotPositions => CompatiblePositions.Where(p => p.SymbolId.DefaultSymbol == DefaultSymbol && p.Label == );

    #endregion

    #region Methods

    #region Positions

    public async ValueTask CloseAllPositions()
    {
        foreach (var p in positions.KeyValues.Values)
        {
            p.Close();
        }
        //await Task.WhenAll(Positions.Items.Select(p => Context.DefaultSimAccount?.ClosePosition(p).AsTask())).ConfigureAwait(false);
    }

    #endregion

    #endregion


    #region Event Handlers

    public virtual void OnBar() { }
    public virtual async ValueTask Stop() // OPTIMIZE: Sync version
    {
        if (Parameters.ClosePositionsOnStop)
        {
            await CloseAllPositions();
        }
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
