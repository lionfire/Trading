using LionFire.Trading.Optimization.Plans;
using LionFire.Trading.Optimization.Plans.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Hosting;

/// <summary>
/// Hosting extensions for optimization plan services.
/// </summary>
public static class OptimizationPlanHostingX
{
    /// <summary>
    /// Adds optimization plan services including repository and validation.
    /// </summary>
    public static IServiceCollection AddOptimizationPlans(
        this IServiceCollection services,
        Action<OptimizationPlanRepositoryOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<OptimizationPlanRepositoryOptions>(_ => { });
        }

        services.AddSingleton<OptimizationPlanValidator>();
        services.AddSingleton<IOptimizationPlanRepository, FileOptimizationPlanRepository>();

        return services;
    }

    /// <summary>
    /// Adds optimization plan template services.
    /// </summary>
    public static IServiceCollection AddOptimizationPlanTemplates(
        this IServiceCollection services,
        Action<TemplateProviderOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<TemplateProviderOptions>(_ => { });
        }

        services.AddSingleton<IOptimizationPlanTemplateProvider, OptimizationPlanTemplateProvider>();

        return services;
    }
}
