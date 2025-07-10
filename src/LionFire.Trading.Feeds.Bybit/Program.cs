using LionFire.Trading.Feeds.Bybit;
using LionFire.Trading.Feeds.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/bybit-feed-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Bybit Feed Collector");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.File("logs/bybit-feed-.txt", rollingInterval: RollingInterval.Day))
        .ConfigureServices((context, services) =>
        {
            // Add trading feeds core services
            services.AddTradingFeeds(context.Configuration);

            // Configure Bybit feed collector
            services.Configure<BybitFeedCollectorOptions>(
                context.Configuration.GetSection("BybitFeed"));

            // Add Bybit feed collector as hosted service
            services.AddHostedService<BybitFeedCollector>();
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