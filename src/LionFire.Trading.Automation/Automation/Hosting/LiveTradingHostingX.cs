using LionFire.Trading.Automation.Accounts;
using LionFire.Trading.Automation.FillSimulation;
using LionFire.Trading.Automation.PriceMonitoring;
using LionFire.Trading.PriceMonitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.Hosting;

/// <summary>
/// Hosting extensions for live trading services.
/// </summary>
public static class LiveTradingHostingX
{
    /// <summary>
    /// Adds all live trading services including price monitoring, pending order management,
    /// and fill simulation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="mode">The fill simulation mode to use (default: Simple).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLiveTrading(
        this IServiceCollection services,
        FillSimulationMode mode = FillSimulationMode.Simple)
    {
        return services
            .AddPendingOrderManager()
            .AddFillSimulation(mode);
    }

    /// <summary>
    /// Adds the pending order manager for tracking SL/TP orders.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// The <see cref="PendingOrderManager{TPrecision}"/> requires an <see cref="ILivePriceMonitor"/>
    /// to be registered. If no price monitor is registered, pending orders will not trigger.
    /// </remarks>
    public static IServiceCollection AddPendingOrderManager(this IServiceCollection services)
    {
        // Register the pending order manager as a singleton per precision type
        // It manages orders globally and subscribes to price feeds
        services.TryAddSingleton<IPendingOrderManager<decimal>, PendingOrderManager<decimal>>();
        services.TryAddSingleton<IPendingOrderManager<double>, PendingOrderManager<double>>();

        return services;
    }

    /// <summary>
    /// Adds fill simulation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="mode">The fill simulation mode to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFillSimulation(
        this IServiceCollection services,
        FillSimulationMode mode = FillSimulationMode.Simple)
    {
        switch (mode)
        {
            case FillSimulationMode.Simple:
                services.TryAddSingleton<IFillSimulator<decimal>, SimpleFillSimulator<decimal>>();
                services.TryAddSingleton<IFillSimulator<double>, SimpleFillSimulator<double>>();
                break;

            case FillSimulationMode.Realistic:
                services.TryAddSingleton<IFillSimulator<decimal>, RealisticFillSimulator<decimal>>();
                services.TryAddSingleton<IFillSimulator<double>, RealisticFillSimulator<double>>();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown fill simulation mode");
        }

        return services;
    }

    /// <summary>
    /// Adds fill simulation with configuration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The realistic fill options to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFillSimulation(
        this IServiceCollection services,
        RealisticFillOptions options)
    {
        services.AddSingleton<IFillSimulator<decimal>>(sp =>
            new RealisticFillSimulator<decimal>(options));
        services.AddSingleton<IFillSimulator<double>>(sp =>
            new RealisticFillSimulator<double>(options));

        return services;
    }
}
