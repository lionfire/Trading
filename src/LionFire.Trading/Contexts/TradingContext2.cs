namespace LionFire.Trading;

public abstract class TradingContext2
{
    // ENH: Cache indicators
    //ConcurrentWeakDictionaryCache<string, IIndicator> indicators = new();

    public IServiceProvider ServiceProvider { get; }

    public TradingContext2(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
