using LionFire.Trading.Scanner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class TradingUIHostingX
{
    public static IServiceCollection AddTradingUI(this IServiceCollection services)
    {
        return services
            .AddMvvm()
            .AddAsyncDataMvvm()
            .AddReactivePersistenceMvvm()
            .AddTransient<ScannerVM>()
            ;
    }
}
