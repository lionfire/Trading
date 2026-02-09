using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation.Optimization.Prioritization;

/// <summary>
/// Hosting extensions for job prioritization services.
/// </summary>
public static class PrioritizationHostingX
{
    /// <summary>
    /// Register job prioritization services.
    /// </summary>
    public static IServiceCollection AddJobPrioritization(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Promise score calculator
        services.AddSingleton<PromiseScoreCalculator>();

        // Job prioritizer
        services.AddSingleton<IJobPrioritizer, JobPrioritizer>();

        // Configuration
        services.Configure<PrioritizationConfig>(options =>
        {
            configuration?.GetSection("Prioritization").Bind(options);
        });

        return services;
    }

    /// <summary>
    /// Register job prioritization services with custom configuration.
    /// </summary>
    public static IServiceCollection AddJobPrioritization(
        this IServiceCollection services,
        Action<PrioritizationConfig> configure)
    {
        // Promise score calculator
        services.AddSingleton<PromiseScoreCalculator>();

        // Job prioritizer
        services.AddSingleton<IJobPrioritizer, JobPrioritizer>();

        // Configuration
        services.Configure(configure);

        return services;
    }
}
