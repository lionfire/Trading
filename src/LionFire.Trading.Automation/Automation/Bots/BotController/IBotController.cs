
namespace LionFire.Trading.Automation;

public interface IBotController<TPrecision> : IAccountProvider2
    where TPrecision : struct, INumber<TPrecision>
{
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

    IAccount2<TPrecision> Account { get; }

    IEnumerable<IAccount2<TPrecision>> Accounts => Account == null ? [] : [Account];

}
