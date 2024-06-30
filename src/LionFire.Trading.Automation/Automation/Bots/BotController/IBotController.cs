namespace LionFire.Trading.Automation;

public interface IBotController : IAccountProvider2
{
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

    SimulatedAccount2<double>? PrimaryAccount { get; }

    IEnumerable<SimulatedAccount2<double>> Accounts => PrimaryAccount == null ? [] : [PrimaryAccount];

}
