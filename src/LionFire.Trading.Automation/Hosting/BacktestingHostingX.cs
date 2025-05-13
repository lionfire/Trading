using LionFire.Trading.Automation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class BacktestingHostingX
{
    public static ILionFireHostBuilder BacktestingModel(this ILionFireHostBuilder builder)
        => builder.ForIHostApplicationBuilder(b => b.ConfigureServices(services => services
             .Configure<BacktestOptions>(b.Configuration.GetSection(BacktestOptions.ConfigurationLocation))
            )
        );
}
