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
            .AddSingleton<BacktestBatcher>()

            // Example of named singleton (REVIEW)
            //.AddKeyedSingleton<BacktestBatcher>("test", (sp, key) =>
            //     new BacktestBatcher(sp.GetOptionsByName<PBacktestBatcher>(key as string ?? throw new NotSupportedException("non-string key")))
            //    )
            ;
    }
}
