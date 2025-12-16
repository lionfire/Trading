using LionFire.IO.Reactive.Filesystem;
using LionFire.Persistence.Filesystem;
using LionFire.Reactive.Persistence;
using LionFire.Referencing;
using LionFire.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

/// <summary>
/// Configures user-level bot services by adding them to the user services collection.
/// Bots are stored at the user level (~/Bots/). 
/// Bot ID format: {OptimizationRunGuid};{BatchId}-{BacktestId}
/// Example: 85c1fd10-dae6-4411-8895-da32c549ccb0;1-0.hjson
/// This is called by WorkspaceLayoutVM.ConfigureUserServices, not as a hosted service.
/// </summary>
public class UserBotsX
{
    public static void AddUserBots(IServiceCollection services, IServiceProvider rootServiceProvider, IReference? userDataReference, ILogger? logger = null)
    {
        try
        {
            if(userDataReference == null)
            {
                logger?.LogWarning("userDataReference not set. User-level bot services will not be configured until it is set.");
                // This is OK - it will be set later by the UI layer
                return;

            }

            logger?.LogInformation("Configuring user-level bot services at: {UserDataDirectory}/Bots", userDataReference.Path);

            // Register bot reader/writer at user level using the standard factory method
            // Bot files: {BotId}.hjson where BotId = {Guid};{BatchId}-{BacktestId}
            try
            {
                services.RegisterObservablesInSubDirForType<BotEntity>(
                    rootServiceProvider,
                    userDataReference,
                    entitySubdir: "Bots",
                    recursive: false,  // Bots are flat files, not in subdirectories
                    autosave: true);

                logger?.LogInformation("User-level bot services registered successfully. IObservableReaderWriter<string, BotEntity> should now be available in UserServices.");
            }
            catch (ObjectDisposedException odex)
            {
                logger?.LogError(odex, "ServiceProvider was disposed while trying to register BotEntity services. This indicates a race condition between disposal and async configuration.");
                throw; 
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to register BotEntity observables in subdirectory");
                throw; 
            }
        }
        catch (ObjectDisposedException odex)
        {
            logger?.LogWarning(odex, "ServiceProvider disposed during user-level bot services configuration - this is expected during component disposal");
            // Don't re-throw - this is expected during disposal
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to configure user-level bot services");
            // Don't re-throw - allow application to continue
        }
    }
}
