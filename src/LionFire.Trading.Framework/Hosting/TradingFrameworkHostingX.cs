using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class TradingFrameworkHostingX
{
    public static IServiceCollection AddTradingFramework(this IServiceCollection services)
    {
        return services
            //.AddTrading() // TODO?
            .AddTradingData()
            ;
    }
}
