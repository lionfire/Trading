using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Automation.Orleans.Optimization;

namespace LionFire.Trading.Automation.Tests.Queue.Infrastructure;

/// <summary>
/// Orleans test cluster configuration for optimization queue tests
/// </summary>
public class OrleansTestCluster : IDisposable
{
    private TestCluster? _cluster;
    private bool _disposed;

    /// <summary>
    /// Get the Orleans grain factory for testing
    /// </summary>
    public IGrainFactory GrainFactory => _cluster?.GrainFactory ?? throw new InvalidOperationException("Cluster not started");

    /// <summary>
    /// Start the test cluster
    /// </summary>
    public async Task StartAsync()
    {
        var builder = new TestClusterBuilder();
        
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        builder.AddClientBuilderConfigurator<TestClientConfigurator>();

        _cluster = builder.Build();
        await _cluster.DeployAsync();
    }

    /// <summary>
    /// Stop the test cluster
    /// </summary>
    public async Task StopAsync()
    {
        if (_cluster != null)
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
            _cluster = null;
        }
    }

    /// <summary>
    /// Get the optimization queue grain for testing
    /// </summary>
    public IOptimizationQueueGrain GetQueueGrain(string grainId = "test-queue")
    {
        return GrainFactory.GetGrain<IOptimizationQueueGrain>(grainId);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}

/// <summary>
/// Silo configurator for Orleans test cluster
/// </summary>
public class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder
            .UseInMemoryReminderService()
            .AddMemoryGrainStorageAsDefault()
            .AddMemoryGrainStorage("optimization-queue")
            .ConfigureApplicationParts(parts => parts
                .AddApplicationPart(typeof(OptimizationQueueGrain).Assembly)
                .WithReferences())
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
    }
}

/// <summary>
/// Client configurator for Orleans test cluster
/// </summary>
public class TestClientConfigurator : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
    {
        clientBuilder
            .ConfigureApplicationParts(parts => parts
                .AddApplicationPart(typeof(IOptimizationQueueGrain).Assembly)
                .WithReferences())
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
    }
}