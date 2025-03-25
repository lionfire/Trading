namespace LionFire.Trading.Automation;

public class BotsProvider : ObservableEntitiesProvider<string, BotEntity>
{
    public BotsProvider(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}

