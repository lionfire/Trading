using LionFire.IO.Reactive.Filesystem;
using LionFire.Ontology;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using LionFire.Trading.Automation.Portfolios;
using LionFire.UI.Workspaces;
using LionFire.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization.Buffers;

namespace LionFire.Trading.Automation;

public class WorkspaceChildTypeConfigurator<T> : IWorkspaceServiceConfigurator
    where T : notnull
{

    public bool Recursive { get; set; } = false;
    public int RecursionDepth { get; set; } = int.MaxValue;
    public bool AutoSave { get; set; } = true;

    public ValueTask ConfigureWorkspaceServices(IServiceCollection services, UserWorkspacesService userWorkspacesService, string? workspaceId)
    {
        System.Diagnostics.Debug.WriteLine($"[WorkspaceChildTypeConfigurator<{typeof(T).Name}>] ConfigureWorkspaceServices called for workspaceId: {workspaceId}");

        if (userWorkspacesService.UserWorkspaces == null)
        {
            System.Diagnostics.Debug.WriteLine($"[WorkspaceChildTypeConfigurator<{typeof(T).Name}>] UserWorkspaces is null - skipping registration");
            // UserWorkspaces not initialized yet - skip registration
            return ValueTask.CompletedTask;
        }

        var workspaceReference = userWorkspacesService.UserWorkspaces.GetChildSubpath(workspaceId);
        System.Diagnostics.Debug.WriteLine($"[WorkspaceChildTypeConfigurator<{typeof(T).Name}>] workspaceReference: {workspaceReference}");

        if (workspaceReference == null)
        {
            System.Diagnostics.Debug.WriteLine($"[WorkspaceChildTypeConfigurator<{typeof(T).Name}>] workspaceReference is null - skipping registration");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[WorkspaceChildTypeConfigurator<{typeof(T).Name}>] Registering observables for type {typeof(T).Name} at {workspaceReference}");
            services
               .RegisterObservablesInSubDirForType<T>(userWorkspacesService.ServiceProvider, workspaceReference, recursive: Recursive, autosave: AutoSave, recursionDepth: RecursionDepth)
               //.AddSingleton<BotsProvider>()
               ;
        }

        return ValueTask.CompletedTask;
    }
}

#if OLD
public class PortfoliosWorkspaceServiceConfigurator : IWorkspaceServiceConfigurator
{
    public ValueTask ConfigureWorkspaceServices(IServiceCollection services, UserWorkspacesService userWorkspacesService, string? workspaceId)
    {
        var workspaceReference = userWorkspacesService.UserWorkspaces.GetChildSubpath(workspaceId);

        if (workspaceReference != null)
        {
            services
               .RegisterObservablesInSubDirForType<Portfolio>(userWorkspacesService.ServiceProvider, workspaceReference)
               //.AddSingleton<BotsProvider>()
               ;
        }

        return ValueTask.CompletedTask;
    }
}

public class BotsWorkspaceServiceConfigurator : IWorkspaceServiceConfigurator
{
    #region Dependencies

    //public UserWorkspacesService UserWorkspaceService { get; }
    //public IServiceProvider ServiceProvider => UserWorkspaceService.ServiceProvider;

    #endregion

    #region Lifecycle

    //public BotsWorkspaceServiceConfigurator(UserWorkspacesService workspaceService)
    //{
    //    UserWorkspaceService = workspaceService;
    //}

    #endregion

    #region IWorkspaceServiceConfigurator

    public ValueTask ConfigureWorkspaceServices(IServiceCollection services, UserWorkspacesService userWorkspacesService, string? workspaceId)
    {
        var workspaceReference = userWorkspacesService.UserWorkspaces.GetChildSubpath(workspaceId);

        if (workspaceReference != null)
        {
            services
               .RegisterObservablesInSubDirForType<BotEntity>(userWorkspacesService.ServiceProvider, workspaceReference)
               //.AddSingleton<BotsProvider>()
               ;
        }

        //var dir = new LionFire.IO.Reactive.DirectorySelector(WorkspacesDir) { Recursive = true };

        //var method = typeof(WorkspaceTypesConfigurator).GetMethod(nameof(AddFsRWForType))!;
        //foreach (var type in Options.MemberTypes)
        //{
        //    var genericMethod = method.MakeGenericMethod(type);
        //    genericMethod.Invoke(this, [services, dir]);
        //}

        return ValueTask.CompletedTask;
    }

    #endregion
}
#endif
