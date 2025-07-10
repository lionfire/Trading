using LionFire.Trading.Feeds.MEXC;
using LionFire.Trading.Feeds.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/mexc-feed-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MEXC Feed Collector (Stub)");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.File("logs/mexc-feed-.txt", rollingInterval: RollingInterval.Day))
        .ConfigureServices((context, services) =>
        {
            // Add trading feeds core services
            services.AddTradingFeeds(context.Configuration);

            // Configure MEXC feed collector
            services.Configure<MexcFeedCollectorOptions>(
                context.Configuration.GetSection("MexcFeed"));

            // Add MEXC feed collector as hosted service
            services.AddHostedService<MexcFeedCollector>();
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