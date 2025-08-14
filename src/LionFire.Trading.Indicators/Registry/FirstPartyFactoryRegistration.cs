using LionFire.Trading.Indicators.Registry;
using System.Runtime.CompilerServices;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Module initializer that registers FirstParty indicator factories
/// </summary>
internal static class FirstPartyFactoryRegistration
{
    /// <summary>
    /// Registers all FirstParty indicator factories with the registry
    /// This method is automatically called when the assembly is loaded
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        var registry = IndicatorFactoryRegistry.Instance;

        // Register FisherTransform factories for common types
        registry.Register(new FisherTransformFirstPartyFactory<double, double>());
        registry.Register(new FisherTransformFirstPartyFactory<decimal, decimal>());
        registry.Register(new FisherTransformFirstPartyFactory<float, float>());

        // Register Supertrend factories for common types
        registry.Register(new SupertrendFirstPartyFactory<double, double>());
        registry.Register(new SupertrendFirstPartyFactory<decimal, decimal>());
        registry.Register(new SupertrendFirstPartyFactory<float, float>());

        // Add more indicator factory registrations here as needed
    }
}