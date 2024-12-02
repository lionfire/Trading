using LionFire.Trading.Exchanges;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class TradingDataHostingX
{
    public static IServiceCollection AddTradingData(this IServiceCollection services)
    {
        return services
                .AddSingleton<IServiceCollection>(services)
                .AddSingleton<ExchangeInfos>()
            ;
    }
}
