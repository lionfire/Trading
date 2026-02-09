using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Spectre.Console;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Orleans.Optimization;
using LionFire.Trading.Binance_;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Indicators;

namespace LionFire.Trading.Cli.Commands;

public class WorkerStartOptions
{
    public int MaxConcurrentJobs { get; set; } = 0; // 0 = auto (CPU count)
    public bool Quiet { get; set; } = false;
}

public static class WorkerHandlers
{
    public static void ConfigureCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        cmd.AddOption(new Option<int>("--max-concurrent-jobs",
            () => 0,
            "Maximum concurrent jobs (0 = auto, uses CPU count)"));

        var quietOption = new Option<bool>("--quiet", () => false, "Suppress status output");
        quietOption.AddAlias("-q");
        cmd.AddOption(quietOption);
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Start =>
        (context, builder) =>
        {
            // Configure Orleans client for cluster connectivity
            builder.UseOrleansClient_LF();

            // Register services needed for local job execution
            builder.Services
                .AutomationModel(builder.Configuration)
                .Automation(builder.Configuration)
                .AddHistoricalBars(builder.Configuration)
                .AddSingleton<IndicatorProvider>()
                .AddIndicators()
                .AddSingleton<BinanceClientProvider>()
                .RegisterTypesFromAssemblies<IPBot2>(typeof(PAtrBot<>))
                .RegisterTypesFromAssemblies<IBot2>(typeof(AtrBot<>));

            // Register the queue processor as a hosted background service
            builder.Services.AddOptimizationQueue();

            // Add a startup message service
            builder.Services.AddHostedService<WorkerStartupNotifier>();
        };

    private class WorkerStartupNotifier : BackgroundService
    {
        private readonly ILogger<WorkerStartupNotifier> _logger;

        public WorkerStartupNotifier(ILogger<WorkerStartupNotifier> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started. Polling for optimization jobs. Press Ctrl+C to stop.");
            AnsiConsole.MarkupLine("[green]Worker started.[/] Polling for optimization jobs. Press [yellow]Ctrl+C[/] to stop.");
            return Task.CompletedTask;
        }
    }
}
