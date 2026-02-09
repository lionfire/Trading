using LionFire.Trading.Optimization.Matrix;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Hosting extensions for plan matrix services.
/// </summary>
public static class MatrixHostingX
{
    /// <summary>
    /// Register plan matrix state services.
    /// </summary>
    public static IServiceCollection AddPlanMatrix(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.Configure<FilePlanMatrixStateOptions>(options =>
        {
            configuration?.GetSection("PlanMatrix:State").Bind(options);
        });
        services.AddSingleton<IPlanMatrixStateRepository, FilePlanMatrixStateRepository>();
        services.AddSingleton<IPlanMatrixService, PlanMatrixService>();
        services.AddSingleton<IMatrixResultsProvider, MatrixResultsProvider>();

        return services;
    }
}
