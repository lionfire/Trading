
using LionFire.Trading.Exchanges2;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Exchanges;

public class ExchangeInfos
{
    public ExchangeInfos(IServiceCollection services)
    {
        var keyedSingletons = services
            .Where(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton && descriptor.ServiceKey != null && descriptor.ServiceType == typeof(IExchangeAreaInfo))
            .Select(descriptor => (Type: descriptor.ServiceType, Key: descriptor.ServiceKey))
            .ToList();

        Items = keyedSingletons?.Where(k=>k.Key != null).Select(k => k.Key.ToString()).Cast<string>() ?? [];

    }

    public IEnumerable<string> Items { get; } = [];
    public IEnumerable<string> Areas(string exchange) => ["asdf","xyz"];
}
