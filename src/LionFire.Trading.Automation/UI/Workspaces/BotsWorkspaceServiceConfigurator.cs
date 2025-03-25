using LionFire.IO.Reactive.Filesystem;
using LionFire.Ontology;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using LionFire.UI.Workspaces;
using LionFire.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization.Buffers;

namespace LionFire.Trading.Automation;

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

    public ValueTask ConfigureWorkspaceServices(IServiceCollection services, UserWorkspacesService userWorkspacesService, string workspaceId)
    {
        var workspaceReference = userWorkspacesService.UserWorkspaces.GetChildSubpath(workspaceId);

        if (workspaceReference != null)
        {
            services
               .RegisterObservablesInSubDirForType<BotEntity>(userWorkspacesService.ServiceProvider, workspaceReference)
               .AddSingleton<BotsProvider>()
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

