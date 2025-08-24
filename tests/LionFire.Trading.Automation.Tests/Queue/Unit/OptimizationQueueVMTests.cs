using FluentAssertions;
using Moq;
using Orleans;
using Microsoft.Extensions.Logging;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.Automation.Blazor.Optimization.Queue;

namespace LionFire.Trading.Automation.Tests.Queue.Unit;

/// <summary>
/// Unit tests for OptimizationQueueVM (Blazor view model)
/// </summary>
public class OptimizationQueueVMTests : IDisposable
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IOptimizationQueueGrain> _mockQueueGrain;
    private readonly Mock<ILogger<OptimizationQueueVM>> _mockLogger;
    private readonly OptimizationQueueVM _viewModel;

    public OptimizationQueueVMTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockQueueGrain = new Mock<IOptimizationQueueGrain>();
        _mockLogger = new Mock<ILogger<OptimizationQueueVM>>();

        _mockGrainFactory
            .Setup(x => x.GetGrain<IOptimizationQueueGrain>("global", null))
            .Returns(_mockQueueGrain.Object);

        _viewModel = new OptimizationQueueVM(
            _mockGrainFactory.Object,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        _viewModel?.Dispose();
    }

    [Fact]
    public async Task RefreshAsync_ShouldLoadJobsAndStatus()
    {
        // Arrange
        var expectedJobs = new List<OptimizationQueueItem>
        {
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running, Priority = 1 },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Queued, Priority = 5 }
        };

        var expectedStatus = new OptimizationQueueStatus
        {
            QueuedCount = 1,
            RunningCount = 1,
            CompletedCount = 10,
            FailedCount = 2,
            TotalJobs = 14
        };

        _mockQueueGrain
            .Setup(x => x.GetJobsAsync(null, 100))
            .ReturnsAsync(expectedJobs);

        _mockQueueGrain
            .Setup(x => x.GetQueueStatusAsync())
            .ReturnsAsync(expectedStatus);

        // Act
        await _viewModel.RefreshAsync();

        // Assert
        _viewModel.Jobs.Should().HaveCount(2);
        _viewModel.Jobs.Should().BeEquivalentTo(expectedJobs);
        
        _viewModel.QueueStatus.Should().NotBeNull();
        _viewModel.QueueStatus!.QueuedCount.Should().Be(1);
        _viewModel.QueueStatus.RunningCount.Should().Be(1);
        _viewModel.QueueStatus.TotalJobs.Should().Be(14);

        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_WithError_ShouldSetErrorState()
    {
        // Arrange
        _mockQueueGrain
            .Setup(x => x.GetJobsAsync(It.IsAny<OptimizationJobStatus?>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Orleans connection failed"));

        // Act
        await _viewModel.RefreshAsync();

        // Assert
        _viewModel.Jobs.Should().BeEmpty();
        _viewModel.QueueStatus.Should().BeNull();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.ErrorMessage.Should().Contain("Orleans connection failed");
    }

    [Fact]
    public async Task CancelJobAsync_WithValidJob_ShouldCancelAndRefresh()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(true);

        // Setup refresh after cancel
        _mockQueueGrain
            .Setup(x => x.GetJobsAsync(null, 100))
            .ReturnsAsync(new List<OptimizationQueueItem>());

        _mockQueueGrain
            .Setup(x => x.GetQueueStatusAsync())
            .ReturnsAsync(new OptimizationQueueStatus());

        // Act
        await _viewModel.CancelJobAsync(jobId);

        // Assert
        _mockQueueGrain.Verify(x => x.CancelJobAsync(jobId), Times.Once);
        _mockQueueGrain.Verify(x => x.GetJobsAsync(null, 100), Times.Once);
    }

    [Fact]
    public async Task CancelJobAsync_WithFailedCancellation_ShouldSetErrorMessage()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(false);

        // Act
        await _viewModel.CancelJobAsync(jobId);

        // Assert
        _viewModel.ErrorMessage.Should().Contain("Failed to cancel job");
    }

    [Fact]
    public void FilterJobs_WithStatusFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var allJobs = new List<OptimizationQueueItem>
        {
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Queued },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Completed },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running }
        };

        _viewModel.Jobs.Clear();
        foreach (var job in allJobs)
        {
            _viewModel.Jobs.Add(job);
        }

        // Act
        _viewModel.StatusFilter = OptimizationJobStatus.Running;

        // Assert
        _viewModel.FilteredJobs.Should().HaveCount(2);
        _viewModel.FilteredJobs.Should().AllSatisfy(job => 
            job.Status.Should().Be(OptimizationJobStatus.Running));
    }

    [Fact]
    public void FilterJobs_WithoutFilter_ShouldShowAllJobs()
    {
        // Arrange
        var allJobs = new List<OptimizationQueueItem>
        {
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Running },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Queued },
            new() { JobId = Guid.NewGuid(), Status = OptimizationJobStatus.Completed }
        };

        _viewModel.Jobs.Clear();
        foreach (var job in allJobs)
        {
            _viewModel.Jobs.Add(job);
        }

        // Act
        _viewModel.StatusFilter = null;

        // Assert
        _viewModel.FilteredJobs.Should().HaveCount(3);
        _viewModel.FilteredJobs.Should().BeEquivalentTo(allJobs);
    }

    [Fact]
    public void ToggleAutoRefresh_ShouldStartAndStopTimer()
    {
        // Arrange
        _viewModel.IsAutoRefreshEnabled.Should().BeFalse();

        // Act: Enable auto-refresh
        _viewModel.ToggleAutoRefresh();

        // Assert
        _viewModel.IsAutoRefreshEnabled.Should().BeTrue();

        // Act: Disable auto-refresh
        _viewModel.ToggleAutoRefresh();

        // Assert
        _viewModel.IsAutoRefreshEnabled.Should().BeFalse();
    }

    [Fact]
    public void GetStatusColor_ShouldReturnCorrectColors()
    {
        // Act & Assert
        _viewModel.GetStatusColor(OptimizationJobStatus.Queued).Should().Be("warning");
        _viewModel.GetStatusColor(OptimizationJobStatus.Running).Should().Be("info");
        _viewModel.GetStatusColor(OptimizationJobStatus.Completed).Should().Be("success");
        _viewModel.GetStatusColor(OptimizationJobStatus.Failed).Should().Be("error");
        _viewModel.GetStatusColor(OptimizationJobStatus.Cancelled).Should().Be("default");
    }

    [Fact]
    public void GetStatusIcon_ShouldReturnCorrectIcons()
    {
        // Act & Assert
        _viewModel.GetStatusIcon(OptimizationJobStatus.Queued).Should().Contain("queue");
        _viewModel.GetStatusIcon(OptimizationJobStatus.Running).Should().Contain("play");
        _viewModel.GetStatusIcon(OptimizationJobStatus.Completed).Should().Contain("check");
        _viewModel.GetStatusIcon(OptimizationJobStatus.Failed).Should().Contain("error");
        _viewModel.GetStatusIcon(OptimizationJobStatus.Cancelled).Should().Contain("cancel");
    }

    [Fact]
    public void FormatDuration_ShouldFormatCorrectly()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            StartedTime = DateTimeOffset.UtcNow.AddMinutes(-30),
            CompletedTime = DateTimeOffset.UtcNow
        };

        // Act
        var formatted = _viewModel.FormatDuration(job);

        // Assert
        formatted.Should().Contain("30");
        formatted.Should().Contain("minutes");
    }

    [Fact]
    public void FormatDuration_WithNullTimes_ShouldReturnDash()
    {
        // Arrange
        var job = new OptimizationQueueItem();

        // Act
        var formatted = _viewModel.FormatDuration(job);

        // Assert
        formatted.Should().Be("-");
    }

    [Fact]
    public void FormatProgress_WithRunningJob_ShouldShowPercentage()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            Status = OptimizationJobStatus.Running,
            Progress = new LionFire.Trading.Optimization.OptimizationProgress
            {
                Completed = 750,
                Queued = 1000
            }
        };

        // Act
        var formatted = _viewModel.FormatProgress(job);

        // Assert
        formatted.Should().Contain("75");
        formatted.Should().Contain("%");
    }

    [Fact]
    public void FormatProgress_WithCompletedJob_ShouldShow100Percent()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            Status = OptimizationJobStatus.Completed
        };

        // Act
        var formatted = _viewModel.FormatProgress(job);

        // Assert
        formatted.Should().Be("100%");
    }

    [Fact]
    public void FormatProgress_WithQueuedJob_ShouldReturnDash()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            Status = OptimizationJobStatus.Queued
        };

        // Act
        var formatted = _viewModel.FormatProgress(job);

        // Assert
        formatted.Should().Be("-");
    }

    [Theory]
    [InlineData(OptimizationJobStatus.Queued, true)]
    [InlineData(OptimizationJobStatus.Running, true)]
    [InlineData(OptimizationJobStatus.Completed, false)]
    [InlineData(OptimizationJobStatus.Failed, false)]
    [InlineData(OptimizationJobStatus.Cancelled, false)]
    public void CanCancelJob_ShouldReturnCorrectValue(OptimizationJobStatus status, bool canCancel)
    {
        // Arrange
        var job = new OptimizationQueueItem { Status = status };

        // Act
        var result = _viewModel.CanCancelJob(job);

        // Assert
        result.Should().Be(canCancel);
    }

    [Fact]
    public void SortJobs_ShouldSortByPriorityThenCreatedTime()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var jobs = new List<OptimizationQueueItem>
        {
            new() { Priority = 5, CreatedTime = baseTime.AddMinutes(-10) },
            new() { Priority = 1, CreatedTime = baseTime.AddMinutes(-5) },  // Should be first
            new() { Priority = 5, CreatedTime = baseTime.AddMinutes(-20) }, // Should be second
            new() { Priority = 10, CreatedTime = baseTime.AddMinutes(-1) }  // Should be last
        };

        _viewModel.Jobs.Clear();
        foreach (var job in jobs)
        {
            _viewModel.Jobs.Add(job);
        }

        // Act
        var sortedJobs = _viewModel.FilteredJobs.ToList();

        // Assert
        sortedJobs[0].Priority.Should().Be(1);
        sortedJobs[1].Priority.Should().Be(5);
        sortedJobs[1].CreatedTime.Should().Be(baseTime.AddMinutes(-20)); // Earlier time first
        sortedJobs[2].Priority.Should().Be(5);
        sortedJobs[2].CreatedTime.Should().Be(baseTime.AddMinutes(-10));
        sortedJobs[3].Priority.Should().Be(10);
    }
}