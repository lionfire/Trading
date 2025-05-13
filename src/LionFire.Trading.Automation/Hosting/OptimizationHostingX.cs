using LionFire.Trading.Automation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class OptimizationHostingX
{
    public static IServiceCollection Optimization(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<OptimizationOptions>(configuration.GetSection(OptimizationOptions.ConfigurationLocation))
            ;
    }
}
