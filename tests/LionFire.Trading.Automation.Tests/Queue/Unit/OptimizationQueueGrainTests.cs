using FluentAssertions;
using LionFire.Trading.Automation.Tests.Queue.Infrastructure;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using System.Text.Json;

namespace LionFire.Trading.Automation.Tests.Queue.Unit;

/// <summary>
/// Unit tests for OptimizationQueueGrain operations
/// </summary>
public class OptimizationQueueGrainTests : IAsyncLifetime
{
    private OrleansTestCluster _cluster = null!;
    private IOptimizationQueueGrain _queueGrain = null!;

    public async Task InitializeAsync()
    {
        _cluster = new OrleansTestCluster();
        await _cluster.StartAsync();
        _queueGrain = _cluster.GetQueueGrain("test-queue");
    }

    public async Task DisposeAsync()
    {
        await _cluster.StopAsync();
        _cluster.Dispose();
    }

    [Fact]
    public async Task EnqueueJobAsync_ShouldCreateJobWithCorrectProperties()
    {
        // Arrange
        var parametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT", Interval = "1h" });
        var priority = 3;
        var submittedBy = "TestUser";

        // Act
        var job = await _queueGrain.EnqueueJobAsync(parametersJson, priority, submittedBy);

        // Assert
        job.Should().NotBeNull();
        job.JobId.Should().NotBeEmpty();
        job.Priority.Should().Be(priority);
        job.Status.Should().Be(OptimizationJobStatus.Queued);
        job.ParametersJson.Should().Be(parametersJson);
        job.SubmittedBy.Should().Be(submittedBy);
        job.CreatedTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        job.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task EnqueueJobAsync_WithHigherPriority_ShouldOrderCorrectly()
    {
        // Arrange
        var lowPriorityJob = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        var highPriorityJob = await _queueGrain.EnqueueJobAsync("{}", 1, "user2");

        // Act
        var nextJob = await _queueGrain.DequeueJobAsync("test-silo", 1);

        // Assert
        nextJob.Should().NotBeNull();
        nextJob!.JobId.Should().Be(highPriorityJob.JobId);
    }

    [Fact]
    public async Task GetQueueStatusAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.EnqueueJobAsync("{}", 3, "user2");
        
        var job = await _queueGrain.DequeueJobAsync("test-silo", 1);
        await _queueGrain.CompleteJobAsync(job!.JobId, "/results/test.json");

        // Act
        var status = await _queueGrain.GetQueueStatusAsync();

        // Assert
        status.Should().NotBeNull();
        status.QueuedCount.Should().Be(1);
        status.RunningCount.Should().Be(0);
        status.CompletedCount.Should().Be(1);
        status.TotalJobs.Should().Be(2);
    }

    [Fact]
    public async Task DequeueJobAsync_WhenQueueEmpty_ShouldReturnNull()
    {
        // Act
        var job = await _queueGrain.DequeueJobAsync("test-silo", 1);

        // Assert
        job.Should().BeNull();
    }

    [Fact]
    public async Task DequeueJobAsync_WithMaxConcurrentJobs_ShouldLimitJobs()
    {
        // Arrange
        await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.EnqueueJobAsync("{}", 5, "user2");
        await _queueGrain.EnqueueJobAsync("{}", 5, "user3");

        // Act
        var job1 = await _queueGrain.DequeueJobAsync("silo1", 2);
        var job2 = await _queueGrain.DequeueJobAsync("silo1", 2);
        var job3 = await _queueGrain.DequeueJobAsync("silo1", 2);

        // Assert
        job1.Should().NotBeNull();
        job2.Should().NotBeNull();
        job3.Should().BeNull(); // Should be limited by maxConcurrentJobs
    }

    [Fact]
    public async Task CancelJobAsync_WhenJobIsQueued_ShouldReturnTrue()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");

        // Act
        var cancelled = await _queueGrain.CancelJobAsync(job.JobId);

        // Assert
        cancelled.Should().BeTrue();
        
        var retrievedJob = await _queueGrain.GetJobAsync(job.JobId);
        retrievedJob!.Status.Should().Be(OptimizationJobStatus.Cancelled);
    }

    [Fact]
    public async Task CancelJobAsync_WhenJobIsRunning_ShouldReturnTrue()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);

        // Act
        var cancelled = await _queueGrain.CancelJobAsync(job.JobId);

        // Assert
        cancelled.Should().BeTrue();
        
        var retrievedJob = await _queueGrain.GetJobAsync(job.JobId);
        retrievedJob!.Status.Should().Be(OptimizationJobStatus.Cancelled);
    }

    [Fact]
    public async Task CancelJobAsync_WhenJobNotFound_ShouldReturnFalse()
    {
        // Act
        var cancelled = await _queueGrain.CancelJobAsync(Guid.NewGuid());

        // Assert
        cancelled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateJobProgressAsync_ShouldUpdateProgress()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        
        var progress = new LionFire.Trading.Optimization.OptimizationProgress
        {
            Completed = 50,
            Queued = 100
        };

        // Act
        await _queueGrain.UpdateJobProgressAsync(job.JobId, progress);

        // Assert
        var retrievedJob = await _queueGrain.GetJobAsync(job.JobId);
        retrievedJob!.Progress.Should().NotBeNull();
        retrievedJob.Progress!.Completed.Should().Be(50);
        retrievedJob.Progress.Queued.Should().Be(100);
        retrievedJob.Progress.PerUn.Should().Be(0.5);
    }

    [Fact]
    public async Task CompleteJobAsync_ShouldSetStatusAndResultPath()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        var resultPath = "/results/optimization-123.json";

        // Act
        await _queueGrain.CompleteJobAsync(job.JobId, resultPath);

        // Assert
        var completedJob = await _queueGrain.GetJobAsync(job.JobId);
        completedJob!.Status.Should().Be(OptimizationJobStatus.Completed);
        completedJob.ResultPath.Should().Be(resultPath);
        completedJob.CompletedTime.Should().NotBeNull();
        completedJob.CompletedTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task FailJobAsync_ShouldSetStatusAndErrorMessage()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        var errorMessage = "Optimization failed due to invalid parameters";

        // Act
        await _queueGrain.FailJobAsync(job.JobId, errorMessage);

        // Assert
        var failedJob = await _queueGrain.GetJobAsync(job.JobId);
        failedJob!.Status.Should().Be(OptimizationJobStatus.Failed);
        failedJob.ErrorMessage.Should().Be(errorMessage);
        failedJob.CompletedTime.Should().NotBeNull();
    }

    [Fact]
    public async Task FailJobAsync_WithRetryAttempts_ShouldRetryBeforeFailure()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);

        // Act & Assert - First failure should retry
        await _queueGrain.FailJobAsync(job.JobId, "First failure");
        var retriedJob = await _queueGrain.GetJobAsync(job.JobId);
        retriedJob!.Status.Should().Be(OptimizationJobStatus.Queued);
        retriedJob.RetryCount.Should().Be(1);

        // Continue until max retries exceeded
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        await _queueGrain.FailJobAsync(job.JobId, "Second failure");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        await _queueGrain.FailJobAsync(job.JobId, "Third failure");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        await _queueGrain.FailJobAsync(job.JobId, "Final failure");

        var finalJob = await _queueGrain.GetJobAsync(job.JobId);
        finalJob!.Status.Should().Be(OptimizationJobStatus.Failed);
        finalJob.RetryCount.Should().Be(3); // MaxRetries reached
    }

    [Fact]
    public async Task GetJobsAsync_WithStatusFilter_ShouldReturnFilteredJobs()
    {
        // Arrange
        await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.EnqueueJobAsync("{}", 5, "user2");
        
        var job = await _queueGrain.DequeueJobAsync("test-silo", 1);
        await _queueGrain.CompleteJobAsync(job!.JobId, "/results/test.json");

        // Act
        var queuedJobs = await _queueGrain.GetJobsAsync(OptimizationJobStatus.Queued, 10);
        var completedJobs = await _queueGrain.GetJobsAsync(OptimizationJobStatus.Completed, 10);

        // Assert
        queuedJobs.Should().HaveCount(1);
        queuedJobs.First().Status.Should().Be(OptimizationJobStatus.Queued);
        
        completedJobs.Should().HaveCount(1);
        completedJobs.First().Status.Should().Be(OptimizationJobStatus.Completed);
    }

    [Fact]
    public async Task GetJobsAsync_WithLimit_ShouldRespectLimit()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _queueGrain.EnqueueJobAsync("{}", 5, $"user{i}");
        }

        // Act
        var jobs = await _queueGrain.GetJobsAsync(null, 3);

        // Assert
        jobs.Should().HaveCount(3);
    }

    [Fact]
    public async Task HeartbeatAsync_ShouldUpdateLastUpdatedTime()
    {
        // Arrange
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        await _queueGrain.DequeueJobAsync("test-silo", 1);
        
        var initialTime = (await _queueGrain.GetJobAsync(job.JobId))!.LastUpdated;
        await Task.Delay(100); // Small delay to ensure time difference

        // Act
        await _queueGrain.HeartbeatAsync(job.JobId, "test-silo");

        // Assert
        var updatedJob = await _queueGrain.GetJobAsync(job.JobId);
        updatedJob!.LastUpdated.Should().BeAfter(initialTime);
    }

    [Fact]
    public async Task CleanupAsync_ShouldRemoveOldCompletedJobs()
    {
        // Arrange
        var job1 = await _queueGrain.EnqueueJobAsync("{}", 5, "user1");
        var job2 = await _queueGrain.EnqueueJobAsync("{}", 5, "user2");
        
        // Complete jobs
        await _queueGrain.DequeueJobAsync("test-silo", 2);
        await _queueGrain.CompleteJobAsync(job1.JobId, "/results/test1.json");
        await _queueGrain.CompleteJobAsync(job2.JobId, "/results/test2.json");

        // Act
        var cleanedCount = await _queueGrain.CleanupAsync(0, 60); // Remove jobs older than 0 days

        // Assert
        cleanedCount.Should().Be(2);
        var remainingJobs = await _queueGrain.GetJobsAsync(null, 10);
        remainingJobs.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public async Task EnqueueJobAsync_WithInvalidParameters_ShouldUseDefaults(string? submittedBy, string expectedSubmittedBy)
    {
        // Act
        var job = await _queueGrain.EnqueueJobAsync("{}", 5, submittedBy);

        // Assert
        job.SubmittedBy.Should().Be(expectedSubmittedBy);
    }
}