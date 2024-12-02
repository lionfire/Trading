using DynamicData;
using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Exchanges2;

public interface IExchangeAreaInfo : IHostedService
{

    public IObservableCache<MarketInfo, string> Markets { get; }
    public ValueTask<IEnumerable<MarketInfo>> GetMarkets();

}

[GenerateSerializer]
public class MarketInfo
{
    [Id(0)]
    public string Symbol { get; set; } = null!;
    [Id(1)]
    public string BaseAsset { get; set; } = null!;
    [Id(2)]
    public string QuoteAsset { get; set; } = null!;
}

