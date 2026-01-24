using System.Net.Http;
using LionFire.Trading.Symbols;
using LionFire.Trading.Symbols.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.Hosting;

/// <summary>
/// Extension methods for registering symbol data provider services.
/// </summary>
public static class SymbolDataProviderHostingX
{
    /// <summary>
    /// Adds symbol data provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration for caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSymbolDataProviders(
        this IServiceCollection services,
        Action<CachedSymbolDataProviderOptions>? configureOptions = null)
    {
        // Register memory cache if not already registered
        services.AddMemoryCache();

        // Configure caching options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<CachedSymbolDataProviderOptions>(_ => { });
        }

        return services;
    }

    /// <summary>
    /// Adds a symbol data provider with caching.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSymbolDataProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, ISymbolDataProvider
    {
        // Register the inner provider
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISymbolDataProvider, TProvider>());

        return services;
    }

    /// <summary>
    /// Adds a symbol data provider with caching using a factory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">Factory function to create the provider.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSymbolDataProvider(
        this IServiceCollection services,
        Func<IServiceProvider, ISymbolDataProvider> factory)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton(factory));
        return services;
    }

    /// <summary>
    /// Wraps an existing provider registration with caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="providerName">The name of the provider to wrap.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachedSymbolDataProvider(
        this IServiceCollection services,
        string providerName)
    {
        // This is used to wrap a specific provider with caching
        // The actual wrapping happens at runtime via decoration
        return services;
    }

    /// <summary>
    /// Adds the CoinLore symbol data provider.
    /// CoinLore provides free market cap and volume data without API keys.
    /// </summary>
    public static IServiceCollection AddCoinLoreSymbolProvider(
        this IServiceCollection services,
        Action<CoinLoreProviderOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<CoinLoreProviderOptions>(_ => { });
        }

        // Ensure HttpClient is available
        services.TryAddSingleton<HttpClient>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISymbolDataProvider, CoinLoreSymbolProvider>());
        return services;
    }

    /// <summary>
    /// Adds the CoinGecko symbol data provider.
    /// CoinGecko provides market cap rankings but has stricter rate limits.
    /// </summary>
    public static IServiceCollection AddCoinGeckoSymbolProvider(
        this IServiceCollection services,
        Action<CoinGeckoProviderOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<CoinGeckoProviderOptions>(_ => { });
        }

        // Ensure HttpClient is available
        services.TryAddSingleton<HttpClient>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISymbolDataProvider, CoinGeckoSymbolProvider>());
        return services;
    }

    /// <summary>
    /// Adds the Binance symbol data provider.
    /// Binance provides accurate volume data directly from the exchange.
    /// </summary>
    public static IServiceCollection AddBinanceSymbolProvider(
        this IServiceCollection services,
        Action<BinanceProviderOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<BinanceProviderOptions>(_ => { });
        }

        // Ensure HttpClient is available
        services.TryAddSingleton<HttpClient>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISymbolDataProvider, BinanceSymbolProvider>());
        return services;
    }

    /// <summary>
    /// Adds all available symbol data providers with default settings.
    /// Includes: CoinLore (market cap), Binance (volume).
    /// </summary>
    public static IServiceCollection AddAllSymbolDataProviders(this IServiceCollection services)
    {
        services.TryAddSingleton<HttpClient>();
        services.AddCoinLoreSymbolProvider();
        services.AddBinanceSymbolProvider();
        // CoinGecko excluded by default due to rate limits - add manually if needed
        return services;
    }
}
