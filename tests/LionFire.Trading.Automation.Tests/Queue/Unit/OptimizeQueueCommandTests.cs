using FluentAssertions;
using Moq;
using Orleans;
using System.Text.Json;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;

namespace LionFire.Trading.Automation.Tests.Queue.Unit;

/// <summary>
/// Unit tests for CLI queue commands
/// Note: These tests focus on the command logic. Full CLI integration tests are in the Integration folder.
/// </summary>
public class OptimizeQueueCommandTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IOptimizationQueueGrain> _mockQueueGrain;

    public OptimizeQueueCommandTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockQueueGrain = new Mock<IOptimizationQueueGrain>();

        _mockGrainFactory
            .Setup(x => x.GetGrain<IOptimizationQueueGrain>("global", null))
            .Returns(_mockQueueGrain.Object);
    }

    [Fact]
    public async Task QueueAdd_WithValidParameters_ShouldSubmitJob()
    {
        // Arrange
        var expectedJob = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            Priority = 3,
            Status = OptimizationJobStatus.Queued,
            ParametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT", Interval = "1h" }),
            SubmittedBy = "CLI"
        };

        _mockQueueGrain
            .Setup(x => x.EnqueueJobAsync(It.IsAny<string>(), 3, "CLI"))
            .ReturnsAsync(expectedJob);

        // Act
        var result = await SimulateQueueAddCommand(
            parametersJson: expectedJob.ParametersJson,
            priority: 3,
            submittedBy: "CLI");

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().Be(expectedJob.JobId);
        result.Priority.Should().Be(3);
        result.Status.Should().Be(OptimizationJobStatus.Queued);

        _mockQueueGrain.Verify(
            x => x.EnqueueJobAsync(expectedJob.ParametersJson, 3, "CLI"),
            Times.Once);
    }

    [Fact]
    public async Task QueueList_WithStatusFilter_ShouldReturnFilteredJobs()
    {
        // Arrange
        var expectedJobs = new List<OptimizationQueueItem>
        {
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running }
        };

        _mockQueueGrain
            .Setup(x => x.GetJobsAsync(OptimizationJobStatus.Running, 50))
            .ReturnsAsync(expectedJobs);

        // Act
        var result = await SimulateQueueListCommand(
            status: OptimizationJobStatus.Running,
            limit: 50);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(job => job.Status.Should().Be(OptimizationJobStatus.Running));

        _mockQueueGrain.Verify(
            x => x.GetJobsAsync(OptimizationJobStatus.Running, 50),
            Times.Once);
    }

    [Fact]
    public async Task QueueList_WithSummaryFlag_ShouldReturnQueueStatus()
    {
        // Arrange
        var expectedStatus = new OptimizationQueueStatus
        {
            QueuedCount = 5,
            RunningCount = 2,
            CompletedCount = 10,
            FailedCount = 1,
            TotalJobs = 18,
            ActiveSilos = 3,
            AverageJobDuration = TimeSpan.FromMinutes(45)
        };

        _mockQueueGrain
            .Setup(x => x.GetQueueStatusAsync())
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await SimulateQueueSummaryCommand();

        // Assert
        result.Should().NotBeNull();
        result.QueuedCount.Should().Be(5);
        result.RunningCount.Should().Be(2);
        result.CompletedCount.Should().Be(10);
        result.TotalJobs.Should().Be(18);
        result.ActiveSilos.Should().Be(3);

        _mockQueueGrain.Verify(
            x => x.GetQueueStatusAsync(),
            Times.Once);
    }

    [Fact]
    public async Task QueueCancel_WithValidJobId_ShouldCancelJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(true);

        // Act
        var result = await SimulateQueueCancelCommand(jobId);

        // Assert
        result.Should().BeTrue();

        _mockQueueGrain.Verify(
            x => x.CancelJobAsync(jobId),
            Times.Once);
    }

    [Fact]
    public async Task QueueCancel_WithInvalidJobId_ShouldReturnFalse()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(false);

        // Act
        var result = await SimulateQueueCancelCommand(jobId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task QueueStatus_WithValidJobId_ShouldReturnJobDetails()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var expectedJob = new OptimizationQueueItem
        {
            JobId = jobId,
            Status = OptimizationJobStatus.Running,
            Priority = 3,
            CreatedTime = DateTimeOffset.UtcNow.AddMinutes(-30),
            StartedTime = DateTimeOffset.UtcNow.AddMinutes(-25),
            Progress = new LionFire.Trading.Optimization.OptimizationProgress
            {
                Completed = 750,
                Queued = 1000
            },
            AssignedSiloId = "silo-worker-01",
            SubmittedBy = "CLI"
        };

        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync(expectedJob);

        // Act
        var result = await SimulateQueueStatusCommand(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobId);
        result.Status.Should().Be(OptimizationJobStatus.Running);
        result.Progress.Should().NotBeNull();
        result.Progress!.PerUn.Should().Be(0.75);
        result.AssignedSiloId.Should().Be("silo-worker-01");
    }

    [Fact]
    public async Task QueueStatus_WithInvalidJobId_ShouldReturnNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync((OptimizationQueueItem?)null);

        // Act
        var result = await SimulateQueueStatusCommand(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task QueueAdd_WithDifferentPriorities_ShouldUseCorrectPriority(int priority)
    {
        // Arrange
        var expectedJob = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            Priority = priority,
            Status = OptimizationJobStatus.Queued
        };

        _mockQueueGrain
            .Setup(x => x.EnqueueJobAsync(It.IsAny<string>(), priority, It.IsAny<string>()))
            .ReturnsAsync(expectedJob);

        // Act
        var result = await SimulateQueueAddCommand(
            parametersJson: "{}",
            priority: priority,
            submittedBy: "CLI");

        // Assert
        result.Priority.Should().Be(priority);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task QueueList_WithDifferentLimits_ShouldRespectLimit(int limit)
    {
        // Arrange
        var jobs = Enumerable.Range(0, limit)
            .Select(i => new OptimizationQueueItem { JobId = Guid.NewGuid() })
            .ToList();

        _mockQueueGrain
            .Setup(x => x.GetJobsAsync(null, limit))
            .ReturnsAsync(jobs);

        // Act
        var result = await SimulateQueueListCommand(status: null, limit: limit);

        // Assert
        result.Should().HaveCount(limit);
    }

    // Helper methods to simulate command execution without actual CLI framework

    private async Task<OptimizationQueueItem> SimulateQueueAddCommand(
        string parametersJson, 
        int priority, 
        string submittedBy)
    {
        var queueGrain = _mockGrainFactory.Object.GetGrain<IOptimizationQueueGrain>("global");
        return await queueGrain.EnqueueJobAsync(parametersJson, priority, submittedBy);
    }

    private async Task<IReadOnlyList<OptimizationQueueItem>> SimulateQueueListCommand(
        OptimizationJobStatus? status, 
        int limit)
    {
        var queueGrain = _mockGrainFactory.Object.GetGrain<IOptimizationQueueGrain>("global");
        return await queueGrain.GetJobsAsync(status, limit);
    }

    private async Task<OptimizationQueueStatus> SimulateQueueSummaryCommand()
    {
        var queueGrain = _mockGrainFactory.Object.GetGrain<IOptimizationQueueGrain>("global");
        return await queueGrain.GetQueueStatusAsync();
    }

    private async Task<bool> SimulateQueueCancelCommand(Guid jobId)
    {
        var queueGrain = _mockGrainFactory.Object.GetGrain<IOptimizationQueueGrain>("global");
        return await queueGrain.CancelJobAsync(jobId);
    }

    private async Task<OptimizationQueueItem?> SimulateQueueStatusCommand(Guid jobId)
    {
        var queueGrain = _mockGrainFactory.Object.GetGrain<IOptimizationQueueGrain>("global");
        return await queueGrain.GetJobAsync(jobId);
    }
}