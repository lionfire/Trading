//using LionFire.Trading.Indicators.InputSignals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class QuantConnectIndicatorsHostingX
{
    public static IServiceCollection AddQuantConnectIndicators(this IServiceCollection services)
    {
        //services
            //.AddSingleton<IMarketDataResolver, MarketDataResolver>()
            //;
        return services;
    }   
}
