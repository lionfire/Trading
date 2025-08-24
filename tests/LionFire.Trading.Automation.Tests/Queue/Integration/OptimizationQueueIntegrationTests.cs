using FluentAssertions;
using LionFire.Trading.Automation.Tests.Queue.Infrastructure;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using System.Text.Json;

namespace LionFire.Trading.Automation.Tests.Queue.Integration;

/// <summary>
/// Integration tests for the complete optimization queue workflow
/// </summary>
public class OptimizationQueueIntegrationTests : IAsyncLifetime
{
    private OrleansTestCluster _cluster = null!;

    public async Task InitializeAsync()
    {
        _cluster = new OrleansTestCluster();
        await _cluster.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _cluster.StopAsync();
        _cluster.Dispose();
    }

    [Fact]
    public async Task CompleteWorkflow_SubmitProcessComplete_ShouldWorkEndToEnd()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("integration-test");
        var parameters = new
        {
            Symbol = "BTCUSDT",
            Interval = "1h",
            Start = DateTimeOffset.UtcNow.AddDays(-30),
            End = DateTimeOffset.UtcNow
        };
        var parametersJson = JsonSerializer.Serialize(parameters);

        // Act 1: Submit job
        var submittedJob = await queueGrain.EnqueueJobAsync(parametersJson, 1, "IntegrationTest");

        // Assert 1: Job submitted correctly
        submittedJob.Should().NotBeNull();
        submittedJob.Status.Should().Be(OptimizationJobStatus.Queued);
        submittedJob.Priority.Should().Be(1);

        // Act 2: Dequeue job (simulating processor)
        var dequeuedJob = await queueGrain.DequeueJobAsync("test-silo-1", 1);

        // Assert 2: Job dequeued and status updated
        dequeuedJob.Should().NotBeNull();
        dequeuedJob!.JobId.Should().Be(submittedJob.JobId);
        dequeuedJob.Status.Should().Be(OptimizationJobStatus.Running);
        dequeuedJob.AssignedSiloId.Should().Be("test-silo-1");
        dequeuedJob.StartedTime.Should().NotBeNull();

        // Act 3: Update progress (simulating optimization progress)
        var progress = new LionFire.Trading.Optimization.OptimizationProgress
        {
            Completed = 500,
            Queued = 1000,
            Start = DateTimeOffset.UtcNow
        };
        await queueGrain.UpdateJobProgressAsync(submittedJob.JobId, progress);

        // Assert 3: Progress updated
        var jobWithProgress = await queueGrain.GetJobAsync(submittedJob.JobId);
        jobWithProgress!.Progress.Should().NotBeNull();
        jobWithProgress.Progress!.PerUn.Should().Be(0.5);

        // Act 4: Send heartbeat
        await queueGrain.HeartbeatAsync(submittedJob.JobId, "test-silo-1");

        // Assert 4: Heartbeat received
        var jobAfterHeartbeat = await queueGrain.GetJobAsync(submittedJob.JobId);
        jobAfterHeartbeat!.LastUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        // Act 5: Complete job
        var resultPath = "/results/optimization-results.json";
        await queueGrain.CompleteJobAsync(submittedJob.JobId, resultPath);

        // Assert 5: Job completed
        var completedJob = await queueGrain.GetJobAsync(submittedJob.JobId);
        completedJob!.Status.Should().Be(OptimizationJobStatus.Completed);
        completedJob.ResultPath.Should().Be(resultPath);
        completedJob.CompletedTime.Should().NotBeNull();
        completedJob.Duration.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipleJobs_WithPriorities_ShouldProcessInCorrectOrder()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("priority-test");
        
        // Submit jobs with different priorities
        var lowPriorityJob = await queueGrain.EnqueueJobAsync("{\"test\": \"low\"}", 10, "Test");
        var highPriorityJob = await queueGrain.EnqueueJobAsync("{\"test\": \"high\"}", 1, "Test");
        var mediumPriorityJob = await queueGrain.EnqueueJobAsync("{\"test\": \"medium\"}", 5, "Test");

        // Act: Dequeue jobs
        var firstJob = await queueGrain.DequeueJobAsync("silo-1", 3);
        var secondJob = await queueGrain.DequeueJobAsync("silo-1", 3);
        var thirdJob = await queueGrain.DequeueJobAsync("silo-1", 3);

        // Assert: Jobs processed in priority order
        firstJob!.JobId.Should().Be(highPriorityJob.JobId);
        secondJob!.JobId.Should().Be(mediumPriorityJob.JobId);
        thirdJob!.JobId.Should().Be(lowPriorityJob.JobId);
    }

    [Fact]
    public async Task FailureAndRetry_ShouldRetryUpToMaxAttempts()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("retry-test");
        var job = await queueGrain.EnqueueJobAsync("{}", 5, "RetryTest");

        // Act & Assert: Retry cycle
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            // Dequeue and fail
            var dequeuedJob = await queueGrain.DequeueJobAsync("retry-silo", 1);
            dequeuedJob.Should().NotBeNull();
            
            await queueGrain.FailJobAsync(job.JobId, $"Attempt {attempt} failed");
            
            var jobState = await queueGrain.GetJobAsync(job.JobId);
            jobState!.RetryCount.Should().Be(attempt);
            
            if (attempt < 3)
            {
                // Should be requeued for retry
                jobState.Status.Should().Be(OptimizationJobStatus.Queued);
            }
            else
            {
                // Max retries reached, should be failed
                jobState.Status.Should().Be(OptimizationJobStatus.Failed);
                jobState.ErrorMessage.Should().Contain("Attempt 3 failed");
            }
        }

        // Final attempt should not be dequeueable
        var finalDequeue = await queueGrain.DequeueJobAsync("retry-silo", 1);
        finalDequeue.Should().BeNull();
    }

    [Fact]
    public async Task ConcurrentSilos_ShouldDistributeJobsCorrectly()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("concurrent-test");
        
        // Submit multiple jobs
        var jobs = new List<OptimizationQueueItem>();
        for (int i = 0; i < 5; i++)
        {
            var job = await queueGrain.EnqueueJobAsync($"{{\"job\": {i}}}", 5, "ConcurrentTest");
            jobs.Add(job);
        }

        // Act: Multiple silos dequeue jobs concurrently
        var silo1Jobs = new List<OptimizationQueueItem?>();
        var silo2Jobs = new List<OptimizationQueueItem?>();

        for (int i = 0; i < 3; i++)
        {
            var job1 = await queueGrain.DequeueJobAsync("silo-1", 2);
            var job2 = await queueGrain.DequeueJobAsync("silo-2", 2);
            
            silo1Jobs.Add(job1);
            silo2Jobs.Add(job2);
        }

        // Assert: Jobs distributed among silos
        var totalAssignedJobs = silo1Jobs.Count(j => j != null) + silo2Jobs.Count(j => j != null);
        totalAssignedJobs.Should().Be(5); // All jobs should be assigned

        // Check silo assignments
        foreach (var job in silo1Jobs.Where(j => j != null))
        {
            job!.AssignedSiloId.Should().Be("silo-1");
        }
        
        foreach (var job in silo2Jobs.Where(j => j != null))
        {
            job!.AssignedSiloId.Should().Be("silo-2");
        }
    }

    [Fact]
    public async Task QueueStatus_ShouldReflectCurrentState()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("status-test");

        // Submit jobs in different states
        var queuedJob = await queueGrain.EnqueueJobAsync("{}", 5, "StatusTest");
        var runningJob = await queueGrain.EnqueueJobAsync("{}", 5, "StatusTest");
        var completedJob = await queueGrain.EnqueueJobAsync("{}", 5, "StatusTest");
        var failedJob = await queueGrain.EnqueueJobAsync("{}", 5, "StatusTest");

        // Process jobs to different states
        await queueGrain.DequeueJobAsync("status-silo", 3);
        await queueGrain.DequeueJobAsync("status-silo", 3);
        await queueGrain.DequeueJobAsync("status-silo", 3);

        await queueGrain.CompleteJobAsync(completedJob.JobId, "/results/completed.json");
        await queueGrain.FailJobAsync(failedJob.JobId, "Test failure");
        // runningJob remains running, queuedJob remains queued

        // Act
        var status = await queueGrain.GetQueueStatusAsync();

        // Assert
        status.Should().NotBeNull();
        status.QueuedCount.Should().Be(1);
        status.RunningCount.Should().Be(1);
        status.CompletedCount.Should().Be(1);
        status.FailedCount.Should().Be(1);
        status.TotalJobs.Should().Be(4);
        status.ActiveSilos.Should().Be(1);
    }

    [Fact]
    public async Task JobCancellation_ShouldWorkAtDifferentStages()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("cancel-test");

        // Test cancellation of queued job
        var queuedJob = await queueGrain.EnqueueJobAsync("{}", 5, "CancelTest");
        var cancelledQueued = await queueGrain.CancelJobAsync(queuedJob.JobId);
        cancelledQueued.Should().BeTrue();

        var cancelledQueuedState = await queueGrain.GetJobAsync(queuedJob.JobId);
        cancelledQueuedState!.Status.Should().Be(OptimizationJobStatus.Cancelled);

        // Test cancellation of running job
        var runningJob = await queueGrain.EnqueueJobAsync("{}", 5, "CancelTest");
        await queueGrain.DequeueJobAsync("cancel-silo", 1);
        
        var cancelledRunning = await queueGrain.CancelJobAsync(runningJob.JobId);
        cancelledRunning.Should().BeTrue();

        var cancelledRunningState = await queueGrain.GetJobAsync(runningJob.JobId);
        cancelledRunningState!.Status.Should().Be(OptimizationJobStatus.Cancelled);

        // Test cancellation of completed job (should fail)
        var completedJob = await queueGrain.EnqueueJobAsync("{}", 5, "CancelTest");
        await queueGrain.DequeueJobAsync("cancel-silo", 1);
        await queueGrain.CompleteJobAsync(completedJob.JobId, "/results/test.json");
        
        var cancelledCompleted = await queueGrain.CancelJobAsync(completedJob.JobId);
        cancelledCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupOldJobs_ShouldRemoveCompletedJobsCorrectly()
    {
        // Arrange
        var queueGrain = _cluster.GetQueueGrain("cleanup-test");

        // Create and complete several jobs
        var jobs = new List<OptimizationQueueItem>();
        for (int i = 0; i < 5; i++)
        {
            var job = await queueGrain.EnqueueJobAsync($"{{\"cleanup\": {i}}}", 5, "CleanupTest");
            jobs.Add(job);
            
            await queueGrain.DequeueJobAsync("cleanup-silo", 5);
            await queueGrain.CompleteJobAsync(job.JobId, $"/results/cleanup-{i}.json");
        }

        // Add one queued job that should not be cleaned up
        var queuedJob = await queueGrain.EnqueueJobAsync("{\"keep\": true}", 5, "CleanupTest");

        // Act: Cleanup completed jobs
        var cleanedCount = await queueGrain.CleanupAsync(retentionDays: 0, timeoutMinutes: 60);

        // Assert
        cleanedCount.Should().Be(5); // 5 completed jobs should be cleaned

        var remainingJobs = await queueGrain.GetJobsAsync(null, 10);
        remainingJobs.Should().HaveCount(1);
        remainingJobs.First().JobId.Should().Be(queuedJob.JobId);
        remainingJobs.First().Status.Should().Be(OptimizationJobStatus.Queued);
    }
}