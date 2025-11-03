using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using LionFire.Trading.Automation.Orleans.Optimization;

namespace LionFire.Hosting;

/// <summary>
/// Hosting extensions for optimization queue services
/// </summary>
public static class OptimizationQueueHostingExtensions
{
    /// <summary>
    /// Add optimization queue processing services to the host
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddOptimizationQueue(this IServiceCollection services)
        => services.AddHostedService<OptimizationQueueProcessor>();

    /// <summary>
    /// Add optimization queue processing services to the host builder
    /// </summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <returns>Host builder for chaining</returns>
    public static IHostBuilder AddOptimizationQueue(this IHostBuilder hostBuilder) 
        => hostBuilder.ConfigureServices((context, services) 
            => services.AddOptimizationQueue());


    public static ISiloBuilder AddOptimizationQueueGrains(this ISiloBuilder silo)
    {
        throw new NotImplementedException();
    }
}