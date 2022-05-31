using System.Threading.Tasks;
using LionFire.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Indicators;

public class IndicatorProvider
{
    public IServiceProvider ServiceProvider { get; }

    public IndicatorProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private Task<IReadOnlyDictionary<string, IndicatorInfo>> Init()
    {
        var dict = new Dictionary<string, IndicatorInfo>();

        dict.Add(new IndicatorInfo("EMA")
        {
            Name = "EMA",
            LongName = "Exponential Moving Average",
            Tags = new HashSet<string>() { "lagging", "average", "exponentially weighted" },
        });

        dict.Add(new IndicatorInfo("SMA")
        {
            Name = "MA",
            LongName = "Simple Moving Average",
            Tags = new HashSet<string>() { "lagging", "average", "simple average" },
        }); ;

        // TODO: Scan for indicators

        indicators = dict;

        return Task.FromResult(indicators);
    }

    public IReadOnlyDictionary<string, IndicatorInfo> IndicatorInfos => indicators ?? Init().Result;


    private IReadOnlyDictionary<string, IndicatorInfo> indicators;

    public async Task<IReadOnlyDictionary<string, IndicatorInfo>> GetIndicators()
    {
        if (indicators == null) await Init().ConfigureAwait(false);
        return indicators;
    }


    public IIndicator? TryGetIndicator(string key, params object[] parameters)
    {
        if(!IndicatorInfos.TryGetValue(key, out var info)) return null;

        var indicator = (IIndicator)ActivatorUtilities.CreateInstance(ServiceProvider, info.Type, parameters);

        return indicator;
    }
}


