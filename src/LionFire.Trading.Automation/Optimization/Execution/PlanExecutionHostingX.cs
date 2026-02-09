using LionFire.Trading.Automation.Optimization.Prioritization;
using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Symbols;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation.Optimization.Execution;

/// <summary>
/// Hosting extensions for plan execution services.
/// </summary>
public static class PlanExecutionHostingX
{
    /// <summary>
    /// Register plan execution services.
    /// </summary>
    public static IServiceCollection AddPlanExecution(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Job queue
        services.AddSingleton<IJobQueueService, JobQueueService>();

        // Job runner
        services.AddSingleton<IJobRunner, LocalJobRunner>();

        // State persistence
        services.Configure<FilePlanExecutionStateOptions>(options =>
        {
            configuration?.GetSection("PlanExecution:State").Bind(options);
        });
        services.AddSingleton<IPlanExecutionStateRepository, FilePlanExecutionStateRepository>();

        // Job prioritization
        services.AddJobPrioritization(configuration);

        // Execution engine
        services.AddSingleton<IPlanExecutionService, PlanExecutionEngine>();

        return services;
    }

    /// <summary>
    /// Register plan execution services with custom options.
    /// </summary>
    public static IServiceCollection AddPlanExecution(
        this IServiceCollection services,
        Action<FilePlanExecutionStateOptions> configureOptions)
    {
        // Job queue
        services.AddSingleton<IJobQueueService, JobQueueService>();

        // Job runner
        services.AddSingleton<IJobRunner, LocalJobRunner>();

        // State persistence
        services.Configure(configureOptions);
        services.AddSingleton<IPlanExecutionStateRepository, FilePlanExecutionStateRepository>();

        // Job prioritization (with default config)
        services.AddJobPrioritization();

        // Execution engine
        services.AddSingleton<IPlanExecutionService, PlanExecutionEngine>();

        return services;
    }
}
