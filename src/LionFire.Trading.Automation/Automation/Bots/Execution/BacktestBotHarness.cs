namespace LionFire.Trading.Automation;

public interface ILiveBotHarness : IBotHarness
{

}

public static class LiveBotHarnessFactory
{
    public static ILiveBotHarness Create(IServiceProvider serviceProvider, BotEntity botEntity)
    {
        //Type type = Type.GetType(value.PBotTypeName);

        throw new NotImplementedException();
    }

}
public class LiveBotHarness<TPrecision> : BotHarnessBase, ILiveBotHarness
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    public override BotExecutionMode BotExecutionMode => BotExecutionMode.Live;

    #endregion

    public bool TicksEnabled { get; }

    public DateTimeOffset Start { get; }
    public DateTimeOffset EndExclusive { get; }
    public DateTimeOffset SimulatedCurrentDate { get; protected set; }

}
