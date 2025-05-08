using LionFire.IO.Reactive.Hjson;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Portfolios;
using LionFire.Trading.Hosting;
using LionFire.UI.Workspaces;
using LionFire.Workspaces;
using LionFire.Workspaces.Services;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Hosting;

public static class AutomationHostingX
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

    //public static IServiceCollection AddAutomation(this IServiceCollection services)
    //{
    //    return services
    //        .AddAutomationRuntime()
    //        .AddAutomationModel()
    //        ;
    //}

    public static IServiceCollection AddAutomationUI(this IServiceCollection services)
    {
        return services
            .AddTransient<BotsVM>()
            .AddTransient<BotVM>()
            ;
    }

    public static IServiceCollection AddWorkspaceChildType<T>(this IServiceCollection services)
        where T : notnull
    {
        return services
                .TryAddEnumerableSingleton<IWorkspaceServiceConfigurator, WorkspaceChildTypeConfigurator<T>>()
            ;
    }
    public static IServiceCollection AddAutomationModel(this IServiceCollection services)
    {
        return services
            .AddWorkspaceChildType<Portfolio>()
            .AddWorkspaceChildType<BotEntity>()

            // OLD
            //.TryAddEnumerableSingleton<IWorkspaceServiceConfigurator, BotsWorkspaceServiceConfigurator>()
            //.TryAddEnumerableSingleton<IWorkspaceServiceConfigurator, PortfoliosWorkspaceServiceConfigurator>()

            .AddWorkspaceDocumentService<string, BotEntity>()
            .AddWorkspaceDocumentService<string, Portfolio>()
            ;
    }
    
    public static IServiceCollection AddAutomationRuntime(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IWorkspaceDocumentRunner<string, BotEntity>, WorkspaceDocumentRunner<string, BotEntity, BotRunner>>());
        return services
            //.AddHostedSingleton<AutomationRuntime>()
            ;
    }
    



    //private static string GetAutomationEntityDir<T>(IServiceProvider serviceProvider)
    //{
    //    var rootDir = serviceProvider.GetRequiredService<IOptionsSnapshot<AutomationDataOptions>>().Value.RootDir;
    //    if (rootDir == null) throw new Exception("No rootDir");

    //    return Path.Combine(rootDir, typeof(T).Name);
    //}

    //public static IServiceCollection AddEntityDir<T>(this IServiceCollection services)
    //    where T : notnull
    //{
    //    // TODO: Vos

    //    return services
    //        .AddSingleton<IObservableReader<string, T>>(sp => ActivatorUtilities.CreateInstance<HjsonFsDirectoryReaderRx<string, T>>(sp, GetAutomationEntityDir<T>(sp)))
    //        .AddSingleton<IObservableWriter<string, T>>(sp => ActivatorUtilities.CreateInstance<HjsonFsDirectoryWriterRx<string, T>>(sp, GetAutomationEntityDir<T>(sp)))
    //        ;
    //}
}
