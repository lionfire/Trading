using System.Numerics;

namespace LionFire.Trading.Automation;

/// <summary>
/// Represents the state of a <see cref="LiveContext{TPrecision}"/>.
/// </summary>
public enum LiveContextState
{
    /// <summary>
    /// Context has been created but not started.
    /// </summary>
    Created,

    /// <summary>
    /// Context is starting up (initializing resources).
    /// </summary>
    Starting,

    /// <summary>
    /// Context is running and processing live data.
    /// </summary>
    Running,

    /// <summary>
    /// Context is shutting down (releasing resources).
    /// </summary>
    Stopping,

    /// <summary>
    /// Context has stopped and released all resources.
    /// </summary>
    Stopped
}

/// <summary>
/// Provides a real-time market context for live trading operations.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="SimContext{TPrecision}"/> which uses simulated time during backtesting,
/// LiveContext uses actual system time (<see cref="DateTimeOffset.UtcNow"/>).
/// </para>
///
/// <para>
/// This context is designed for use with real-time bots running in Orleans grains or
/// similar hosting environments. It integrates with price monitoring services for
/// SL/TP trigger detection.
/// </para>
///
/// <para>
/// The context supports graceful shutdown through its <see cref="CancellationToken"/>
/// and implements <see cref="IAsyncDisposable"/> for proper resource cleanup.
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations (e.g., double, decimal).</typeparam>
public class LiveContext<TPrecision> : IMarketContext<TPrecision>, IAsyncDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    /// <summary>
    /// Gets the unique identifier for this context instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    #endregion

    #region Dependencies

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region IMarketContext Implementation

    /// <inheritdoc />
    /// <remarks>
    /// For LiveContext, this always returns the current UTC time.
    /// </remarks>
    public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    /// <remarks>
    /// For LiveContext, this always returns <c>true</c>.
    /// </remarks>
    public bool IsLive => true;

    /// <inheritdoc />
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    #endregion

    #region Lifecycle

    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Gets the current state of the context.
    /// </summary>
    public LiveContextState State { get; private set; } = LiveContextState.Created;

    /// <summary>
    /// Initializes a new instance of <see cref="LiveContext{TPrecision}"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public LiveContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LiveContext{TPrecision}"/> with an external cancellation token.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="externalCancellation">An external cancellation token to link with.</param>
    public LiveContext(IServiceProvider serviceProvider, CancellationToken externalCancellation)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);
    }

    /// <summary>
    /// Starts the live context and any associated services.
    /// </summary>
    /// <returns>A task that completes when the context has started.</returns>
    public ValueTask StartAsync()
    {
        if (State != LiveContextState.Created)
        {
            throw new InvalidOperationException($"Cannot start context in state {State}");
        }

        State = LiveContextState.Starting;

        // TODO: Initialize price monitor if available
        // var priceMonitor = ServiceProvider.GetService<ILivePriceMonitor>();
        // if (priceMonitor != null) { ... }

        State = LiveContextState.Running;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Stops the live context and releases associated resources.
    /// </summary>
    /// <returns>A task that completes when the context has stopped.</returns>
    public async ValueTask StopAsync()
    {
        if (State == LiveContextState.Stopped || State == LiveContextState.Stopping)
        {
            return;
        }

        State = LiveContextState.Stopping;

        // Signal cancellation to all listeners
        await _cancellationTokenSource.CancelAsync();

        State = LiveContextState.Stopped;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (State != LiveContextState.Stopped)
        {
            await StopAsync();
        }

        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}
