using LionFire.TypeRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation;

public interface ILiveBotHarness : IBotHarness
{
}



public class LiveBotHarness<TPrecision> : BotHarnessBase, ILiveBotHarness
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    public override BotExecutionMode BotExecutionMode => BotExecutionMode.Live;

    #endregion

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

    #region Lifecycle

    public LiveBotHarness(IBot2 bot)
    {
        this.bot = bot;
    }

    #endregion

    public bool TicksEnabled { get; }

    public DateTimeOffset Start { get; }
    public DateTimeOffset EndExclusive { get; }
    public DateTimeOffset SimulatedCurrentDate { get; protected set; }

}
