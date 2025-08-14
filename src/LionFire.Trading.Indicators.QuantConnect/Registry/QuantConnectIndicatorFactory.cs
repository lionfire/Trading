using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect.Registry;

/// <summary>
/// Base factory for QuantConnect indicator implementations
/// </summary>
/// <typeparam name="TIndicator">The indicator interface type</typeparam>
/// <typeparam name="TParameters">The parameter type</typeparam>
public abstract class QuantConnectIndicatorFactory<TIndicator, TParameters> : IIndicatorFactory<TIndicator, TParameters>
{
    /// <summary>
    /// Creates an indicator instance with the specified parameters
    /// </summary>
    public abstract TIndicator Create(TParameters parameters);

    /// <summary>
    /// Gets the implementation hint (always QuantConnect)
    /// </summary>
    public ImplementationHint ImplementationHint => ImplementationHint.QuantConnect;

    /// <summary>
    /// Gets a value indicating whether this factory is available
    /// </summary>
    public virtual bool IsAvailable => IsQuantConnectAvailable();

    /// <summary>
    /// Checks if QuantConnect indicators are available in the current context
    /// </summary>
    private static bool IsQuantConnectAvailable()
    {
        try
        {
            // Try to load QuantConnect.Indicators assembly
            var assembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            return assembly != null;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Factory for FisherTransform QuantConnect implementation
/// </summary>
public class FisherTransformQuantConnectFactory<TInput, TOutput> : QuantConnectIndicatorFactory<IFisherTransform<TInput, TOutput>, PFisherTransform<TInput, TOutput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    public override IFisherTransform<TInput, TOutput> Create(PFisherTransform<TInput, TOutput> parameters)
    {
        return QuantConnect_.FisherTransform_QC<TInput, TOutput>.Create(parameters);
    }
}

/// <summary>
/// Factory for Supertrend QuantConnect implementation
/// </summary>
public class SupertrendQuantConnectFactory<TPrice, TOutput> : QuantConnectIndicatorFactory<ISupertrend<TPrice, TOutput>, PSupertrend<TPrice, TOutput>>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    public override ISupertrend<TPrice, TOutput> Create(PSupertrend<TPrice, TOutput> parameters)
    {
        return new QuantConnect_.Supertrend_QC<TPrice, TOutput>(parameters);
    }
}