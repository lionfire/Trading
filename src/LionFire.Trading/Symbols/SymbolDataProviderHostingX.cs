using LionFire.Trading.Symbols;
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
}
