using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.Grains.Optimization;
using Orleans;
using Orleans.Runtime;

namespace LionFire.Trading.Automation.Orleans.Optimization;

/// <summary>
/// Background service that processes optimization jobs from the queue
/// </summary>
public class OptimizationQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OptimizationQueueProcessor> _logger;
    private readonly IGrainFactory _grainFactory;
    private readonly string _siloId;
    private readonly int _maxConcurrentJobs;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _heartbeatInterval;
    
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _runningJobs = new();

    public OptimizationQueueProcessor(
        IServiceProvider serviceProvider,
        ILogger<OptimizationQueueProcessor> logger,
        IGrainFactory grainFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _grainFactory = grainFactory;
        _siloId = Environment.MachineName + "-" + Environment.ProcessId; // Simple silo ID
        _maxConcurrentJobs = Environment.ProcessorCount; // TEMP: One job per CPU core - we probably want 1 max per machine for normal use to leverage CPU cache, or at most, one job per actual core (don't count threads)
        _pollInterval = TimeSpan.FromSeconds(5);
        _heartbeatInterval = TimeSpan.FromMinutes(2);
        
        _concurrencySemaphore = new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs);
        
        _logger.LogInformation("OptimizationQueueProcessor initialized with SiloId={SiloId}, MaxConcurrentJobs={MaxConcurrentJobs}", 
            _siloId, _maxConcurrentJobs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3_000);

        _logger.LogInformation("OptimizationQueueProcessor starting - waiting for Orleans cluster to be ready");
        
        // Wait for Orleans cluster to be ready
        await WaitForOrleansClusterReady(stoppingToken);
        
        _logger.LogInformation("Orleans cluster is ready - starting queue processing");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in queue processing loop");
            }
            
            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        
        _logger.LogInformation("OptimizationQueueProcessor stopping");
        
        // Cancel all running jobs
        foreach (var (jobId, cts) in _runningJobs)
        {
            _logger.LogInformation("Cancelling running job {JobId}", jobId);
            cts.Cancel();
        }
        
        // Wait for running jobs to complete (with timeout)
        var timeout = TimeSpan.FromMinutes(2);
        var waitStart = DateTime.UtcNow;
        while (_runningJobs.Any() && DateTime.UtcNow - waitStart < timeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private async Task WaitForOrleansClusterReady(CancellationToken stoppingToken)
    {
        // Option 1: Test with application grain (original approach)
        await WaitForOrleansClusterReadyViaApplicationGrain(stoppingToken);
        
        // Option 2: Additional verification using Orleans Management Grain
        await WaitForOrleansClusterReadyViaManagementGrain(stoppingToken);
    }

    private async Task WaitForOrleansClusterReadyViaApplicationGrain(CancellationToken stoppingToken)
    {
        const int maxRetries = 60; // 5 minutes with 5-second intervals
        var retryCount = 0;
        
        _logger.LogDebug("Testing Orleans cluster readiness via application grain...");
        
        while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
        {
            try
            {
                // Try to get a simple grain to test cluster connectivity
                var testGrain = _grainFactory.GetGrain<IOptimizationQueueGrain>("health-check");
                
                // Use a simple operation with timeout to test readiness
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
                
                // This will throw if Orleans isn't ready
                await testGrain.GetQueueStatusAsync();
                
                _logger.LogInformation("Orleans cluster connection verified successfully via application grain");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                retryCount++;
                _logger.LogDebug("Orleans cluster not ready yet (attempt {RetryCount}/{MaxRetries}): {Message}", 
                    retryCount, maxRetries, ex.Message);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Orleans cluster failed to become ready after {MaxRetries} attempts", maxRetries);
                    throw new InvalidOperationException("Orleans cluster is not ready", ex);
                }
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task WaitForOrleansClusterReadyViaManagementGrain(CancellationToken stoppingToken)
    {
        const int maxRetries = 12; // 1 minute with 5-second intervals
        var retryCount = 0;
        
        _logger.LogDebug("Testing Orleans cluster readiness via management grain...");
        
        while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
        {
            try
            {
                // Use Orleans built-in management grain to check cluster status
                var managementGrain = _grainFactory.GetGrain<IManagementGrain>(0);
                
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
                
                // Get active silos - this will throw if cluster isn't ready
                var hosts = await managementGrain.GetHosts(true);
                
                if (hosts?.Values?.Any(h => h == SiloStatus.Active) == true)
                {
                    var activeSilos = hosts.Values.Count(h => h == SiloStatus.Active);
                    _logger.LogInformation("Orleans cluster verified via management grain: {ActiveSilos} active silo(s) found", activeSilos);
                    return;
                }
                else
                {
                    _logger.LogDebug("No active silos found in cluster yet (attempt {RetryCount}/{MaxRetries})", retryCount + 1, maxRetries);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                retryCount++;
                _logger.LogDebug("Orleans management grain not accessible yet (attempt {RetryCount}/{MaxRetries}): {Message}", 
                    retryCount, maxRetries, ex.Message);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogWarning("Could not verify Orleans cluster status via management grain after {MaxRetries} attempts", maxRetries);
                    // Don't fail here - the application grain test already passed
                    return;
                }
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        // Check if we have capacity for more jobs
        if (_concurrencySemaphore.CurrentCount == 0)
        {
            return;
        }

        var queueGrain = _grainFactory.GetGrain<IOptimizationQueueGrain>("global");
        
        try
        {
            // TODO: 
            var job = await queueGrain.DequeueJobAsync(_siloId, _maxConcurrentJobs);
            if (job == null)
            {
                return; // No jobs available
            }

            _logger.LogInformation("Dequeued job {JobId} for processing", job.JobId);
            
            // Start processing job in background
            _ = Task.Run(async () =>
            {
                var jobCts = new CancellationTokenSource();
                var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, jobCts.Token);
                
                _runningJobs[job.JobId] = jobCts;
                
                try
                {
                    await ProcessJobAsync(job, queueGrain, combinedCts.Token);
                }
                finally
                {
                    _runningJobs.TryRemove(job.JobId, out _);
                    _concurrencySemaphore.Release();
                    jobCts.Dispose();
                    combinedCts.Dispose();
                }
            }, stoppingToken);
            
            await _concurrencySemaphore.WaitAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dequeuing job from queue");
        }
    }

    private async Task ProcessJobAsync(OptimizationQueueItem job, IOptimizationQueueGrain queueGrain, CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        _logger.LogInformation("Starting execution of job {JobId}", jobId);
        
        try
        {
            // Deserialize parameters
            var parameters = JsonSerializer.Deserialize<PMultiSim>(job.ParametersJson);
            if (parameters == null)
            {
                throw new InvalidOperationException("Failed to deserialize job parameters");
            }

            // Create optimization task
            using var scope = _serviceProvider.CreateScope();
            var optimizationTask = new OptimizationTask(scope.ServiceProvider, parameters);
            
            // Set up progress reporting
            var progressReportingTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested && 
                       !optimizationTask.MultiSimContext.Task.IsCompleted)
                {
                    try
                    {
                        // Report progress to queue
                        var progress = optimizationTask.OptimizationStrategy.Progress;
                        await queueGrain.UpdateJobProgressAsync(jobId, progress);
                        
                        // Send heartbeat
                        await queueGrain.HeartbeatAsync(jobId, _siloId);
                        
                        await Task.Delay(_heartbeatInterval, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reporting progress for job {JobId}", jobId);
                    }
                }
            }, cancellationToken);

            // Start the optimization
            await optimizationTask.StartAsync();
            
            // Wait for completion or cancellation
            while (!cancellationToken.IsCancellationRequested && !optimizationTask.MultiSimContext.Task.IsCompleted)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            
            // Stop progress reporting
            progressReportingTask.GetAwaiter().GetResult(); // Wait for progress task to complete
            
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Job {JobId} was cancelled", jobId);
                await queueGrain.CancelJobAsync(jobId);
                return;
            }

            // Check if optimization completed successfully
            if (optimizationTask.MultiSimContext.Task.IsCompletedSuccessfully)
            {
                var resultPath = optimizationTask.MultiSimContext.Journal?.BatchDirectory;
                await queueGrain.CompleteJobAsync(jobId, resultPath);
                
                _logger.LogInformation("Job {JobId} completed successfully. Results: {ResultPath}", 
                    jobId, resultPath);
            }
            else
            {
                var errorMessage = "Optimization task did not complete successfully";
                await queueGrain.FailJobAsync(jobId, errorMessage);
                
                _logger.LogWarning("Job {JobId} failed: {ErrorMessage}", jobId, errorMessage);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job {JobId} was cancelled", jobId);
            await queueGrain.CancelJobAsync(jobId);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Job execution failed: {ex.Message}";
            _logger.LogError(ex, "Job {JobId} failed with exception", jobId);
            
            try
            {
                await queueGrain.FailJobAsync(jobId, errorMessage);
            }
            catch (Exception reportEx)
            {
                _logger.LogError(reportEx, "Failed to report job failure for {JobId}", jobId);
            }
        }
    }

    public override void Dispose()
    {
        _concurrencySemaphore?.Dispose();
        foreach (var cts in _runningJobs.Values)
        {
            cts?.Dispose();
        }
        base.Dispose();
    }
}