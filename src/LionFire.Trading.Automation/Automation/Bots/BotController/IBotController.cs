namespace LionFire.Trading.Automation;

public interface IBotController : IAccountProvider2
{
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

    IAccount2 Account { get; }

    IEnumerable<IAccount2> Accounts => Account == null ? [] : [Account];

}
