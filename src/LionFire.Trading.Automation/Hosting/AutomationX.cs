using LionFire.Trading.Automation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class AutomationX
{
    public static IServiceCollection AddAutomation(this IServiceCollection services)
    {
        return services
            .AddSingleton<BacktestQueue>()

            // Example of named singleton (REVIEW)
            //.AddKeyedSingleton<BacktestBatchQueue>("test", (sp, key) =>
            //     new BacktestBatchQueue(sp.GetOptionsByName<PBacktestBatchQueue>(key as string ?? throw new NotSupportedException("non-string key")))
            //    )
            ;
    }
}
