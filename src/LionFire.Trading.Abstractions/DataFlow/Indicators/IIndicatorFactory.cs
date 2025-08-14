using System.Numerics;

namespace LionFire.Trading.DataFlow.Indicators;

/// <summary>
/// Factory interface for creating indicator instances
/// </summary>
/// <typeparam name="TIndicator">The indicator interface type</typeparam>
/// <typeparam name="TParameters">The parameter type for the indicator</typeparam>
public interface IIndicatorFactory<out TIndicator, in TParameters>
{
    /// <summary>
    /// Creates an indicator instance with the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters for the indicator</param>
    /// <returns>An indicator instance</returns>
    TIndicator Create(TParameters parameters);

    /// <summary>
    /// Gets the implementation hint this factory provides
    /// </summary>
    ImplementationHint ImplementationHint { get; }

    /// <summary>
    /// Gets a value indicating whether this factory is available in the current context
    /// </summary>
    bool IsAvailable { get; }
}

/// <summary>
/// Registry for indicator factories that resolves circular dependencies
/// </summary>
public interface IIndicatorFactoryRegistry
{
    /// <summary>
    /// Registers a factory for a specific indicator type and implementation hint
    /// </summary>
    /// <typeparam name="TIndicator">The indicator interface type</typeparam>
    /// <typeparam name="TParameters">The parameter type</typeparam>
    /// <param name="factory">The factory instance</param>
    void Register<TIndicator, TParameters>(IIndicatorFactory<TIndicator, TParameters> factory);

    /// <summary>
    /// Gets a factory for the specified implementation hint
    /// </summary>
    /// <typeparam name="TIndicator">The indicator interface type</typeparam>
    /// <typeparam name="TParameters">The parameter type</typeparam>
    /// <param name="hint">The preferred implementation hint</param>
    /// <returns>A factory instance or null if not found</returns>
    IIndicatorFactory<TIndicator, TParameters>? GetFactory<TIndicator, TParameters>(ImplementationHint hint);

    /// <summary>
    /// Gets the best available factory for the specified indicator type
    /// </summary>
    /// <typeparam name="TIndicator">The indicator interface type</typeparam>
    /// <typeparam name="TParameters">The parameter type</typeparam>
    /// <param name="preferredHint">The preferred implementation hint</param>
    /// <returns>A factory instance or null if none available</returns>
    IIndicatorFactory<TIndicator, TParameters>? GetBestFactory<TIndicator, TParameters>(ImplementationHint preferredHint = ImplementationHint.Auto);
}