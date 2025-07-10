using LionFire.Trading.Feeds.Binance;
using LionFire.Trading.Feeds.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/binance-feed-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Binance Feed Collector");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.File("logs/binance-feed-.txt", rollingInterval: RollingInterval.Day))
        .ConfigureServices((context, services) =>
        {
            // Add trading feeds core services
            services.AddTradingFeeds(context.Configuration);

            // Configure Binance feed collector
            services.Configure<BinanceFeedCollectorOptions>(
                context.Configuration.GetSection("BinanceFeed"));

            // Add Binance feed collector as hosted service
            services.AddHostedService<BinanceFeedCollector>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}