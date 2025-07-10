using LionFire.Trading.Feeds.Storage;
using LionFire.Trading.Feeds.Tracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Feeds.DependencyInjection;

public static class FeedsServiceCollectionExtensions
{
    public static IServiceCollection AddTradingFeeds(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Core services
        services.AddSingleton<ICvdTracker, CvdTracker>();
        
        // Storage
        services.Configure<FasterLogOptions>(
            configuration.GetSection("TradingFeeds:Storage"));
        services.AddSingleton<ITimeSeriesStorage, FasterLogTimeSeriesStorage>();
        
        return services;
    }
}