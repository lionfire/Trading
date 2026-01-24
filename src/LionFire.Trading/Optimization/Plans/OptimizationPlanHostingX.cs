using System.IO;
using LionFire.Applications;
using LionFire.Trading.Optimization.Plans;
using LionFire.Trading.Optimization.Plans.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LionFire.Hosting;

/// <summary>
/// Hosting extensions for optimization plan services.
/// </summary>
public static class OptimizationPlanHostingX
{
    /// <summary>
    /// Adds optimization plan services including repository and validation.
    /// When no options are configured, defaults to storing plans in {AppProgramDataDir}/Plans.
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

        // Post-configure to set default PlansDirectory from AppDirectories if not already set
        services.AddSingleton<IPostConfigureOptions<OptimizationPlanRepositoryOptions>>(sp =>
        {
            var appDirs = sp.GetService<AppDirectories>();
            return new PostConfigureOptions<OptimizationPlanRepositoryOptions>(
                Options.DefaultName,
                options =>
                {
                    if (string.IsNullOrWhiteSpace(options.PlansDirectory) && appDirs?.AppProgramDataDir != null)
                    {
                        options.PlansDirectory = Path.Combine(appDirs.AppProgramDataDir, "Plans");
                    }
                });
        });

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
