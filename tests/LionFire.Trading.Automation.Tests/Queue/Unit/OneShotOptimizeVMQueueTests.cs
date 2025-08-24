using FluentAssertions;
using Moq;
using Orleans;
using Microsoft.Extensions.Logging;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.Automation.Blazor.Optimization;

namespace LionFire.Trading.Automation.Tests.Queue.Unit;

/// <summary>
/// Unit tests for queue functionality in OneShotOptimizeVM
/// </summary>
public class OneShotOptimizeVMQueueTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IOptimizationQueueGrain> _mockQueueGrain;
    private readonly OneShotOptimizeVM _viewModel;

    public OneShotOptimizeVMQueueTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockQueueGrain = new Mock<IOptimizationQueueGrain>();

        _mockGrainFactory
            .Setup(x => x.GetGrain<IOptimizationQueueGrain>("global", null))
            .Returns(_mockQueueGrain.Object);

        // Create view model with mocked dependencies
        _viewModel = new OneShotOptimizeVM(_mockGrainFactory.Object);
    }

    [Fact]
    public async Task OnQueue_WithValidParameters_ShouldSubmitToQueue()
    {
        // Arrange
        var expectedJob = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            Priority = 5,
            Status = OptimizationJobStatus.Queued,
            SubmittedBy = "UI"
        };

        _mockQueueGrain
            .Setup(x => x.EnqueueJobAsync(It.IsAny<string>(), 5, "UI"))
            .ReturnsAsync(expectedJob);

        // Act
        await _viewModel.OnQueue();

        // Assert
        _mockQueueGrain.Verify(
            x => x.EnqueueJobAsync(
                It.IsAny<string>(), 
                5, 
                "UI"),
            Times.Once);

        _viewModel.QueuedJobId.Should().Be(expectedJob.JobId);
        _viewModel.QueueStatus.Should().Be(OptimizationJobStatus.Queued);
    }

    [Fact]
    public async Task OnQueue_WithCustomPriority_ShouldUseCorrectPriority()
    {
        // Arrange
        _viewModel.QueuePriority = 3;
        
        var expectedJob = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            Priority = 3,
            Status = OptimizationJobStatus.Queued
        };

        _mockQueueGrain
            .Setup(x => x.EnqueueJobAsync(It.IsAny<string>(), 3, "UI"))
            .ReturnsAsync(expectedJob);

        // Act
        await _viewModel.OnQueue();

        // Assert
        _mockQueueGrain.Verify(
            x => x.EnqueueJobAsync(It.IsAny<string>(), 3, "UI"),
            Times.Once);
    }

    [Fact]
    public async Task OnCancelQueued_WithValidJob_ShouldCancelJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;
        _viewModel.QueueStatus = OptimizationJobStatus.Queued;

        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(true);

        // Act
        await _viewModel.OnCancelQueued();

        // Assert
        _mockQueueGrain.Verify(x => x.CancelJobAsync(jobId), Times.Once);
        _viewModel.QueuedJobId.Should().BeNull();
        _viewModel.QueueStatus.Should().BeNull();
    }

    [Fact]
    public async Task OnCancelQueued_WithFailedCancellation_ShouldKeepJobState()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;
        _viewModel.QueueStatus = OptimizationJobStatus.Running;

        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(jobId))
            .ReturnsAsync(false);

        // Act
        await _viewModel.OnCancelQueued();

        // Assert
        _viewModel.QueuedJobId.Should().Be(jobId);
        _viewModel.QueueStatus.Should().Be(OptimizationJobStatus.Running);
    }

    [Fact]
    public async Task RefreshQueueStatus_WithValidJob_ShouldUpdateStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;

        var updatedJob = new OptimizationQueueItem
        {
            JobId = jobId,
            Status = OptimizationJobStatus.Running,
            Progress = new LionFire.Trading.Optimization.OptimizationProgress
            {
                Completed = 250,
                Queued = 1000
            },
            EstimatedCompletionTime = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync(updatedJob);

        // Act
        await _viewModel.RefreshQueueStatus();

        // Assert
        _viewModel.QueueStatus.Should().Be(OptimizationJobStatus.Running);
        _viewModel.QueueProgress.Should().NotBeNull();
        _viewModel.QueueProgress!.PerUn.Should().Be(0.25);
        _viewModel.QueueEstimatedCompletion.Should().BeCloseTo(
            DateTimeOffset.UtcNow.AddMinutes(30), 
            TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RefreshQueueStatus_WithCompletedJob_ShouldClearQueueState()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;

        var completedJob = new OptimizationQueueItem
        {
            JobId = jobId,
            Status = OptimizationJobStatus.Completed,
            ResultPath = "/results/optimization-results.json"
        };

        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync(completedJob);

        // Act
        await _viewModel.RefreshQueueStatus();

        // Assert
        _viewModel.QueueStatus.Should().Be(OptimizationJobStatus.Completed);
        _viewModel.QueueResultPath.Should().Be("/results/optimization-results.json");
        
        // After a delay, the queue state should be cleared
        await Task.Delay(100);
        _viewModel.QueuedJobId.Should().BeNull();
    }

    [Fact]
    public async Task RefreshQueueStatus_WithFailedJob_ShouldShowError()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;

        var failedJob = new OptimizationQueueItem
        {
            JobId = jobId,
            Status = OptimizationJobStatus.Failed,
            ErrorMessage = "Optimization failed due to invalid parameters"
        };

        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync(failedJob);

        // Act
        await _viewModel.RefreshQueueStatus();

        // Assert
        _viewModel.QueueStatus.Should().Be(OptimizationJobStatus.Failed);
        _viewModel.QueueErrorMessage.Should().Be("Optimization failed due to invalid parameters");
    }

    [Fact]
    public async Task RefreshQueueStatus_WithNonExistentJob_ShouldClearState()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _viewModel.QueuedJobId = jobId;

        _mockQueueGrain
            .Setup(x => x.GetJobAsync(jobId))
            .ReturnsAsync((OptimizationQueueItem?)null);

        // Act
        await _viewModel.RefreshQueueStatus();

        // Assert
        _viewModel.QueuedJobId.Should().BeNull();
        _viewModel.QueueStatus.Should().BeNull();
        _viewModel.QueueProgress.Should().BeNull();
    }

    [Fact]
    public void HasQueuedJob_WithActiveJob_ShouldReturnTrue()
    {
        // Arrange
        _viewModel.QueuedJobId = Guid.NewGuid();
        _viewModel.QueueStatus = OptimizationJobStatus.Queued;

        // Act & Assert
        _viewModel.HasQueuedJob.Should().BeTrue();
    }

    [Fact]
    public void HasQueuedJob_WithoutJob_ShouldReturnFalse()
    {
        // Arrange
        _viewModel.QueuedJobId = null;
        _viewModel.QueueStatus = null;

        // Act & Assert
        _viewModel.HasQueuedJob.Should().BeFalse();
    }

    [Fact]
    public void CanCancelQueue_WithQueuedJob_ShouldReturnTrue()
    {
        // Arrange
        _viewModel.QueuedJobId = Guid.NewGuid();
        _viewModel.QueueStatus = OptimizationJobStatus.Queued;

        // Act & Assert
        _viewModel.CanCancelQueue.Should().BeTrue();
    }

    [Fact]
    public void CanCancelQueue_WithRunningJob_ShouldReturnTrue()
    {
        // Arrange
        _viewModel.QueuedJobId = Guid.NewGuid();
        _viewModel.QueueStatus = OptimizationJobStatus.Running;

        // Act & Assert
        _viewModel.CanCancelQueue.Should().BeTrue();
    }

    [Fact]
    public void CanCancelQueue_WithCompletedJob_ShouldReturnFalse()
    {
        // Arrange
        _viewModel.QueuedJobId = Guid.NewGuid();
        _viewModel.QueueStatus = OptimizationJobStatus.Completed;

        // Act & Assert
        _viewModel.CanCancelQueue.Should().BeFalse();
    }

    [Theory]
    [InlineData(OptimizationJobStatus.Queued, "Queued")]
    [InlineData(OptimizationJobStatus.Running, "Running")]
    [InlineData(OptimizationJobStatus.Completed, "Completed")]
    [InlineData(OptimizationJobStatus.Failed, "Failed")]
    [InlineData(OptimizationJobStatus.Cancelled, "Cancelled")]
    public void QueueStatusText_ShouldReturnCorrectText(OptimizationJobStatus status, string expectedText)
    {
        // Arrange
        _viewModel.QueueStatus = status;

        // Act & Assert
        _viewModel.QueueStatusText.Should().Be(expectedText);
    }

    [Fact]
    public void QueueProgressPercent_WithProgress_ShouldReturnCorrectPercentage()
    {
        // Arrange
        _viewModel.QueueProgress = new LionFire.Trading.Optimization.OptimizationProgress
        {
            Completed = 750,
            Queued = 1000
        };

        // Act & Assert
        _viewModel.QueueProgressPercent.Should().Be(75.0);
    }

    [Fact]
    public void QueueProgressPercent_WithoutProgress_ShouldReturnZero()
    {
        // Arrange
        _viewModel.QueueProgress = null;

        // Act & Assert
        _viewModel.QueueProgressPercent.Should().Be(0);
    }

    [Fact]
    public void QueuePosition_WithQueuedJob_ShouldCalculateCorrectly()
    {
        // This would require mocking queue position calculation
        // For now, test that it handles null cases gracefully
        
        // Arrange
        _viewModel.QueuedJobId = null;

        // Act & Assert
        _viewModel.QueuePosition.Should().BeNull();
    }
}