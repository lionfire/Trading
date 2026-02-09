using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using LionFire.Trading.Automation.Orleans.Optimization;
using LionFire.Trading.Automation.Optimization.Execution;
using LionFire.Trading.Optimization.Execution;

namespace LionFire.Hosting;

/// <summary>
/// Hosting extensions for optimization queue services
/// </summary>
public static class OptimizationQueueHostingExtensions
{
    /// <summary>
    /// Add optimization queue processing services to the host.
    /// This registers the BackgroundService that polls the grain queue and executes jobs locally.
    /// </summary>
    public static IServiceCollection AddOptimizationQueue(this IServiceCollection services)
        => services.AddHostedService<OptimizationQueueProcessor>();

    /// <summary>
    /// Add optimization queue processing services to the host builder.
    /// </summary>
    public static IHostBuilder AddOptimizationQueue(this IHostBuilder hostBuilder)
        => hostBuilder.ConfigureServices((context, services)
            => services.AddOptimizationQueue());

    /// <summary>
    /// Add optimization queue grains to the silo.
    /// Orleans auto-discovers grains in referenced assemblies, so this is mainly
    /// for explicit configuration if needed.
    /// </summary>
    public static ISiloBuilder AddOptimizationQueueGrains(this ISiloBuilder silo)
    {
        // Orleans auto-discovers grains from referenced assemblies.
        // The OptimizationQueueGrain is in this assembly and will be found automatically.
        return silo;
    }

    /// <summary>
    /// Register plan execution services in distributed mode.
    /// Uses the Orleans grain queue for job distribution instead of local execution.
    /// Call this instead of AddPlanExecution() when distributed mode is enabled.
    /// </summary>
    public static IServiceCollection AddDistributedPlanExecution(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Register all base plan execution services (queue, state, prioritization, engine)
        services.AddPlanExecution(configuration);

        // Override IJobRunner with distributed version that submits to grain queue
        services.Replace(ServiceDescriptor.Singleton<IJobRunner, DistributedJobRunner>());

        return services;
    }
}
