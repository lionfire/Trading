using DynamicData.Kernel;
using LionFire.Threading;
using LionFire.Trading.Automation;
using Microsoft.Extensions.DependencyInjection;



public class BotRunner : Runner<BotEntity, BotRunner>, IRunner<BotEntity>
{
    #region (static implementation)

    static bool IRunner<BotEntity>.IsEnabled(BotEntity value) => value.Enabled;

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public ILogger<BotRunner> Logger { get; }

    #endregion

    #region Lifecycle

    public BotRunner(IServiceProvider serviceProvider, ILogger<BotRunner> logger) //: base(enabledPredicate: b => b.Enabled)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    #endregion

    #region State

    string? Name;

    bool stopped = false;
    ManualResetEvent stopEvent = new(false);


    // ENH idea: base interface for Runner controller, with
    // - IObservable<bool> Healthy
    // - IObservableList<StatusIndicators> StatusIndicators // Error, Warning, Info, etc.
    // ENH: And if it doesn't implement it directly, ability to use Adapter to bind it
    // ENH: Eliminate BotRunner, and use Runner
    ILiveBotHarness? harness;

    #endregion

    #region Event handling

    protected override ValueTask<bool> Start(BotEntity value, Optional<BotEntity> oldValue)
    {
        Logger.LogInformation("Starting bot: {0}", value);

        try
        {
            harness = LiveBotHarnessFactory.Create(ServiceProvider, value);
        }
        catch (Exception)
        {
            throw; // Let the caller handle this
            //base.OnError(ex);
            //Logger.LogError(ex, "Failed to create bot harness");
            //return ValueTask.FromResult(false);
        }

        stopped = false;
        stopEvent.Reset();
        _ = Task.Run(async () => await Run(value));
        return ValueTask.FromResult(true);
    }

    protected override void OnConfigurationChange(BotEntity value, Optional<BotEntity> oldValue)
    {
        Logger.LogInformation("TODO: Bot configuration change: {0}", value);
    }

    protected override void Stop(Optional<BotEntity> newValue, Optional<BotEntity> oldValue)
    {
        Logger.LogInformation("Stopping bot: {0}", oldValue.HasValue ? oldValue.Value : newValue);
        stopped = true;
        stopEvent.Set();
    }

    #endregion
    async Task Run(BotEntity botEntity)
    {
        ArgumentNullException.ThrowIfNull(botEntity);
        Name = botEntity.Name;

        PeriodicTimer pt = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (!stopped)
        {
            if (!Current.HasValue)
            {
                Logger.LogWarning($"{Name} - configuration disappeared!  Nothing left to do.");
                break;
            }
            else
            {
                Logger.LogDebug($"{Current.Value.Name}: running.");
            }

            await Task.WhenAny(Task.Delay(1000), stopEvent.WaitOneAsync());
        }
    }
}
