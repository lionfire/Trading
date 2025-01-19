using LionFire.IO.Reactive.Hjson;
using LionFire.Reactive.Persistence;
using LionFire.Reactive.Reader;
using LionFire.Reactive.Writer;
using LionFire.Trading.Automation;
using LionFire.Trading.Hosting;
using LionFire.Trading.Link.Blazor.Components.Pages;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class AutomationX
{
    public static IServiceCollection AddBacktesting(this IServiceCollection services)
    {
        return services
            .AddSingleton<BacktestQueue>()
            .AddHostedService(sp => sp.GetRequiredService<BacktestQueue>())
            // Example of named singleton (REVIEW)
            //.AddKeyedSingleton<BacktestBatchQueue>("test", (sp, key) =>
            //     new BacktestBatchQueue(sp.GetOptionsByName<PBacktestBatchQueue>(key as string ?? throw new NotSupportedException("non-string key")))
            //    )
            ;
    }
    public static IServiceCollection AddAutomation(this IServiceCollection services)
    {
        return services
            .AddAutomation_Data()
            ;
    }

    private static IServiceCollection AddAutomation_Data(this IServiceCollection services)
    {
        return services
            .AddEntityDir<BotEntity>()
            ;
    }

    private static string GetAutomationEntityDir<T>(IServiceProvider serviceProvider)
    {
        var rootDir = serviceProvider.GetRequiredService<IOptionsSnapshot<AutomationDataOptions>>().Value.RootDir;
        if (rootDir == null) throw new Exception("No rootDir");

        return Path.Combine(rootDir, typeof(T).Name);
    }
    public static IServiceCollection AddEntityDir<T>(this IServiceCollection services)
        where T : notnull
    {
        // TODO: Vos

        return services
            .AddSingleton<IObservableReader<string, T>>(sp => new HjsonFsDirectoryReaderRx<string, T>(GetAutomationEntityDir<T>(sp)))
            .AddSingleton<IObservableWriter<string, T>>(sp => new HjsonFsDirectoryWriterRx<string, T>(GetAutomationEntityDir<T>(sp)))
            ;
    }
}
