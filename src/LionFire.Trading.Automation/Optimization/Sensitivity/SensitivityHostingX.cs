using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

public static class SensitivityHostingX
{
    /// <summary>
    /// Register core sensitivity analysis services (safe for CLI and Silo).
    /// </summary>
    public static IServiceCollection AddSensitivityAnalysis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SensitivityAnalysisOptions>(
            configuration.GetSection(SensitivityAnalysisOptions.ConfigurationLocation));
        services.Configure<HessianAnalysisOptions>(
            configuration.GetSection("Trading:Optimization:Sensitivity:Hessian"));

        services.AddSingleton<SensitivityAnalyzer>();
        services.AddSingleton<HessianAnalyzer>();
        services.AddSingleton<StepRecommendationPersistence>();
        services.AddSingleton<HessianPersistence>();
        services.AddSingleton<InteractionReportGenerator>();
        services.AddSingleton<PendingAnalysisScanner>();
        services.AddSingleton<BatchSensitivityAnalyzer>();
        services.AddSingleton<MarketTypeProfileAggregator>();
        services.AddSingleton<RecommendationLookupService>();

        return services;
    }

    /// <summary>
    /// Register background hosted services for auto-triggering sensitivity analysis.
    /// Only call from hosts that have PlanExecutionEngine registered (e.g., Silo).
    /// </summary>
    public static IServiceCollection AddSensitivityAnalysisHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SensitivityBackgroundServiceOptions>(
            configuration.GetSection(SensitivityBackgroundServiceOptions.ConfigurationLocation));

        services.AddHostedService<SensitivityAnalysisBackgroundService>();
        services.AddHostedService<PostOptimizationAnalysisTrigger>();

        return services;
    }
}
