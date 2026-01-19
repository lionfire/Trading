using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Trading;
using LionFire.Trading.Automation.Optimization.Analysis;
using LionFire.Trading.Automation.Optimization;

namespace LionFire.Hosting;

public static class TradingAnalysisHostingX
{
    public static ILionFireHostBuilder TradingAnalysis(this ILionFireHostBuilder builder)
        => builder.ForHostBuilder(b => b.ConfigureServices((context, services) => services

        #region Analysis

            .Configure<OptimizationAnalysisOptions>(context.Configuration.GetSection(OptimizationAnalysisOptions.ConfigurationLocation)
                .GetSection(OperatingSystem.IsWindows() ? "Windows" : "Unix"))
            .Configure<IngestOptions>(context.Configuration.GetSection(IngestOptions.ConfigurationLocation)
                .GetSection(OperatingSystem.IsWindows() ? "Windows" : "Unix"))
            .AddSingleton<OptimizationRunAnalysisService>()
            .AddHostedService<OptimizationRunAnalysisService>()

        #endregion

        #region Persistence

            .AddSingleton<IOptimizationRepository, OptimizationRunDiskPersister>()
            .AddSingleton<BotOptimizationStatusRepository>()

        #endregion

        #region ViewModels

            //.AddTransient<>()

        #endregion

        ));
}
