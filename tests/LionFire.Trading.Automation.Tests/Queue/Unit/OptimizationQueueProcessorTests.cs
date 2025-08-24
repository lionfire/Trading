using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using LionFire.Trading.Automation.Orleans.Optimization;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.Automation;
using System.Text.Json;

namespace LionFire.Trading.Automation.Tests.Queue.Unit;

/// <summary>
/// Unit tests for OptimizationQueueProcessor
/// </summary>
public class OptimizationQueueProcessorTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IOptimizationQueueGrain> _mockQueueGrain;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<OptimizationQueueProcessor>> _mockLogger;
    private readonly OptimizationQueueProcessor _processor;

    public OptimizationQueueProcessorTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockQueueGrain = new Mock<IOptimizationQueueGrain>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<OptimizationQueueProcessor>>();

        _mockGrainFactory
            .Setup(x => x.GetGrain<IOptimizationQueueGrain>("global", null))
            .Returns(_mockQueueGrain.Object);

        _processor = new OptimizationQueueProcessor(
            _mockGrainFactory.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldStartProcessingLoop()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // Quick cancellation for test

        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((OptimizationQueueItem?)null); // No jobs available

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        
        // Wait for background processing to start
        await Task.Delay(50);
        
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockQueueGrain.Verify(
            x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessJobAsync_WithValidJob_ShouldCompleteSuccessfully()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            ParametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT" }),
            Status = OptimizationJobStatus.Running
        };

        var mockOptimizationTask = new Mock<OptimizationTask>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        mockScope
            .Setup(x => x.ServiceProvider.GetService(typeof(OptimizationTask)))
            .Returns(mockOptimizationTask.Object);

        mockOptimizationTask
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OptimizationResult { Success = true, ResultPath = "/results/test.json" });

        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(job);

        _mockQueueGrain
            .Setup(x => x.CompleteJobAsync(job.JobId, "/results/test.json"))
            .Returns(Task.CompletedTask);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Allow processing
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockQueueGrain.Verify(
            x => x.CompleteJobAsync(job.JobId, "/results/test.json"),
            Times.Once);
    }

    [Fact]
    public async Task ProcessJobAsync_WithFailedOptimization_ShouldFailJob()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            ParametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT" }),
            Status = OptimizationJobStatus.Running
        };

        var mockOptimizationTask = new Mock<OptimizationTask>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        mockScope
            .Setup(x => x.ServiceProvider.GetService(typeof(OptimizationTask)))
            .Returns(mockOptimizationTask.Object);

        mockOptimizationTask
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Optimization failed"));

        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(job);

        _mockQueueGrain
            .Setup(x => x.FailJobAsync(job.JobId, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Allow processing
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockQueueGrain.Verify(
            x => x.FailJobAsync(job.JobId, It.Is<string>(msg => msg.Contains("Optimization failed"))),
            Times.Once);
    }

    [Fact]
    public async Task ProcessJobAsync_WithCancellation_ShouldCancelJob()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            ParametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT" }),
            Status = OptimizationJobStatus.Running
        };

        var mockOptimizationTask = new Mock<OptimizationTask>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        mockScope
            .Setup(x => x.ServiceProvider.GetService(typeof(OptimizationTask)))
            .Returns(mockOptimizationTask.Object);

        mockOptimizationTask
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(job);

        _mockQueueGrain
            .Setup(x => x.CancelJobAsync(job.JobId))
            .ReturnsAsync(true);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Allow processing
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockQueueGrain.Verify(
            x => x.CancelJobAsync(job.JobId),
            Times.Once);
    }

    [Fact]
    public async Task ProcessingLoop_WithNoJobs_ShouldWaitAndRetry()
    {
        // Arrange
        var dequeueCallCount = 0;
        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((OptimizationQueueItem?)null)
            .Callback(() => dequeueCallCount++);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(300));

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(250); // Allow several polling cycles
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        dequeueCallCount.Should().BeGreaterThan(1); // Should have polled multiple times
    }

    [Fact]
    public async Task Heartbeat_ShouldBeSentPeriodically()
    {
        // Arrange
        var job = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            ParametersJson = JsonSerializer.Serialize(new { Symbol = "BTCUSDT" }),
            Status = OptimizationJobStatus.Running
        };

        var mockOptimizationTask = new Mock<OptimizationTask>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        mockScope
            .Setup(x => x.ServiceProvider.GetService(typeof(OptimizationTask)))
            .Returns(mockOptimizationTask.Object);

        // Setup long-running task to test heartbeat
        var cancellationTokenSource = new CancellationTokenSource();
        mockOptimizationTask
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                await Task.Delay(1000, ct); // Long delay to allow heartbeat
                return new OptimizationResult { Success = true, ResultPath = "/results/test.json" };
            });

        _mockQueueGrain
            .Setup(x => x.DequeueJobAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(job);

        _mockQueueGrain
            .Setup(x => x.HeartbeatAsync(job.JobId, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(600));

        // Act
        await _processor.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(500); // Allow heartbeat to be sent
        await _processor.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockQueueGrain.Verify(
            x => x.HeartbeatAsync(job.JobId, It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void GetSiloId_ShouldReturnConsistentId()
    {
        // Act
        var processor1 = new OptimizationQueueProcessor(
            _mockGrainFactory.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var processor2 = new OptimizationQueueProcessor(
            _mockGrainFactory.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        // Assert
        // Each processor should have a unique silo ID
        var siloId1 = GetPrivateSiloId(processor1);
        var siloId2 = GetPrivateSiloId(processor2);

        siloId1.Should().NotBeNullOrEmpty();
        siloId2.Should().NotBeNullOrEmpty();
        siloId1.Should().NotBe(siloId2);
    }

    private static string GetPrivateSiloId(OptimizationQueueProcessor processor)
    {
        var field = typeof(OptimizationQueueProcessor)
            .GetField("_siloId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)field!.GetValue(processor)!;
    }
}

/// <summary>
/// Fake OptimizationResult for testing
/// </summary>
public class OptimizationResult
{
    public bool Success { get; set; }
    public string? ResultPath { get; set; }
}