using System.Numerics;

namespace LionFire.Trading.Automation;

/// <summary>
/// Represents the market context in which trading operations occur.
/// Provides access to current time and distinguishes between live and simulated trading.
/// </summary>
/// <remarks>
/// This interface is implemented by:
/// <list type="bullet">
///   <item><description><see cref="SimContext{TPrecision}"/> - For backtesting with simulated time</description></item>
///   <item><description>LiveContext - For real-time trading with actual time</description></item>
/// </list>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations (e.g., double, decimal).</typeparam>
public interface IMarketContext<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets the current time in the market context.
    /// </summary>
    /// <remarks>
    /// For simulation contexts, this returns the simulated current date/time.
    /// For live contexts, this returns <see cref="DateTimeOffset.UtcNow"/>.
    /// </remarks>
    DateTimeOffset CurrentTime { get; }

    /// <summary>
    /// Gets a value indicating whether this context represents live trading.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is a live trading context with real-time data;
    /// <c>false</c> if this is a simulation/backtest context.
    /// </value>
    bool IsLive { get; }

    /// <summary>
    /// Gets a cancellation token that is signaled when the context is shutting down.
    /// </summary>
    /// <remarks>
    /// Trading bots should observe this token to gracefully exit when the context is stopped.
    /// </remarks>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies within this context.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}
