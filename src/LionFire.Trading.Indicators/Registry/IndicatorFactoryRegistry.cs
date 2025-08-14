using LionFire.Trading.DataFlow.Indicators;
using System.Collections.Concurrent;

namespace LionFire.Trading.Indicators.Registry;

/// <summary>
/// Thread-safe registry for indicator factories that resolves circular dependencies
/// </summary>
public sealed class IndicatorFactoryRegistry : IIndicatorFactoryRegistry
{
    private static readonly Lazy<IndicatorFactoryRegistry> _instance = new(() => new IndicatorFactoryRegistry());

    /// <summary>
    /// Gets the singleton instance of the registry
    /// </summary>
    public static IndicatorFactoryRegistry Instance => _instance.Value;

    // Use a dictionary with composite key to store factories
    private readonly ConcurrentDictionary<FactoryKey, object> _factories = new();

    private IndicatorFactoryRegistry() { }

    /// <summary>
    /// Registers a factory for a specific indicator type and implementation hint
    /// </summary>
    public void Register<TIndicator, TParameters>(IIndicatorFactory<TIndicator, TParameters> factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        var key = new FactoryKey(typeof(TIndicator), typeof(TParameters), factory.ImplementationHint);
        _factories.AddOrUpdate(key, factory, (_, _) => factory);
    }

    /// <summary>
    /// Gets a factory for the specified implementation hint
    /// </summary>
    public IIndicatorFactory<TIndicator, TParameters>? GetFactory<TIndicator, TParameters>(ImplementationHint hint)
    {
        var key = new FactoryKey(typeof(TIndicator), typeof(TParameters), hint);
        
        if (_factories.TryGetValue(key, out var factory))
        {
            if (factory is IIndicatorFactory<TIndicator, TParameters> typedFactory && typedFactory.IsAvailable)
            {
                return typedFactory;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the best available factory for the specified indicator type
    /// </summary>
    public IIndicatorFactory<TIndicator, TParameters>? GetBestFactory<TIndicator, TParameters>(ImplementationHint preferredHint = ImplementationHint.Auto)
    {
        // First try the preferred hint
        if (preferredHint != ImplementationHint.Auto)
        {
            var preferredFactory = GetFactory<TIndicator, TParameters>(preferredHint);
            if (preferredFactory != null)
                return preferredFactory;
        }

        // Fallback priority order: Optimized -> QuantConnect -> FirstParty
        var fallbackOrder = new[]
        {
            ImplementationHint.Optimized,
            ImplementationHint.QuantConnect,
            ImplementationHint.FirstParty
        };

        foreach (var hint in fallbackOrder)
        {
            if (hint == preferredHint) continue; // Already tried

            var factory = GetFactory<TIndicator, TParameters>(hint);
            if (factory != null)
                return factory;
        }

        return null;
    }

    /// <summary>
    /// Gets all registered factories for debugging/inspection
    /// </summary>
    public IEnumerable<(Type IndicatorType, Type ParameterType, ImplementationHint Hint, bool IsAvailable)> GetRegisteredFactories()
    {
        return _factories.Select(kvp => 
        {
            var key = kvp.Key;
            var factory = kvp.Value;
            
            // Use reflection to get IsAvailable since we don't know the generic types at runtime
            var isAvailableProperty = factory.GetType().GetProperty(nameof(IIndicatorFactory<object, object>.IsAvailable));
            var isAvailable = (bool)(isAvailableProperty?.GetValue(factory) ?? false);
            
            return (key.IndicatorType, key.ParameterType, key.Hint, isAvailable);
        });
    }

    /// <summary>
    /// Composite key for factory lookup
    /// </summary>
    private sealed record FactoryKey(Type IndicatorType, Type ParameterType, ImplementationHint Hint);
}