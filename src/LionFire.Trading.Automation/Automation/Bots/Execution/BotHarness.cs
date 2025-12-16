using LionFire.Trading.Automation.Bots;
using LionFire.TypeRegistration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LionFire.Trading.Automation;

public interface IRealtimeBotHarness : IBotHarness, IHostedService
{
}


/// <summary>
/// Single bot
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
public sealed class BotHarness<TPrecision> : BotHarnessBase
    , IRealtimeBotHarness
    where TPrecision : struct, INumber<TPrecision>
{

    #region Relationships

    public IBot2 Bot
    {
        get => bot;
        set
        {
            if (bot != null) throw new AlreadySetException();
            bot = value;
        }
    }
    private IBot2 bot;

    #endregion

    private readonly IGrainFactory grainFactory;

    #region Lifecycle

    public BotHarness(IBot2 bot, IGrainFactory grainFactory, SimContext<TPrecision> simContext) 
    {
        this.bot = bot;
        this.grainFactory = grainFactory;
        this.SimContext = simContext;

    }
    public BotInfo BotInfo => BotInfos.Get(Bot.Parameters.GetType(), Bot.GetType());

    private void InitInputs()
    {
        // TODO: Implement live input initialization logic here.
        // This should be similar to MultiBacktestHarness.InitInputs, but adapted for live data sources.
        // For example, subscribe to live market data feeds and wire them to the bot's input properties.
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitInputs(); // Initialize live inputs before starting
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region State

    public SimContext<TPrecision> SimContext { get; }
    ISimContext IBotHarness.ISimContext => SimContext;
    public override ISimContext ISimContext => SimContext;

    #endregion

    public bool TicksEnabled { get; }

    public DateTimeOffset Start { get; }
    public DateTimeOffset EndExclusive { get; }
    public DateTimeOffset SimulatedCurrentDate { get; protected set; }

}
