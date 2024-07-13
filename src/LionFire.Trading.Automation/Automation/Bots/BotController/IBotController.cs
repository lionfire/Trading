namespace LionFire.Trading.Automation;

public interface IBotController : IAccountProvider2
{
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

    IAccount2<double>? Account { get; }

    IEnumerable<IAccount2<double>> Accounts => Account == null ? [] : [Account];

}
