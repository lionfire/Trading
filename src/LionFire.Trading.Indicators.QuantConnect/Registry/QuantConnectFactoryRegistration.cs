using LionFire.Trading.Indicators.Registry;
using LionFire.Trading.Indicators.QuantConnect.Registry;
using System.Runtime.CompilerServices;

namespace LionFire.Trading.Indicators.QuantConnect;

/// <summary>
/// Module initializer that registers QuantConnect indicator factories
/// </summary>
internal static class QuantConnectFactoryRegistration
{
    /// <summary>
    /// Registers all QuantConnect indicator factories with the registry
    /// This method is automatically called when the assembly is loaded
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        var registry = IndicatorFactoryRegistry.Instance;

        // Register FisherTransform factories for common types
        registry.Register(new FisherTransformQuantConnectFactory<double, double>());
        registry.Register(new FisherTransformQuantConnectFactory<decimal, decimal>());
        registry.Register(new FisherTransformQuantConnectFactory<float, float>());

        // Register Supertrend factories for common types
        registry.Register(new SupertrendQuantConnectFactory<double, double>());
        registry.Register(new SupertrendQuantConnectFactory<decimal, decimal>());
        registry.Register(new SupertrendQuantConnectFactory<float, float>());

        // Add more QuantConnect indicator factory registrations here as needed
    }
}