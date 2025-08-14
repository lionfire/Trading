using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Indicators.Parameters;

namespace LionFire.Trading.Indicators.Registry;

/// <summary>
/// Base factory for first-party indicator implementations
/// </summary>
/// <typeparam name="TIndicator">The indicator interface type</typeparam>
/// <typeparam name="TParameters">The parameter type</typeparam>
public abstract class FirstPartyIndicatorFactory<TIndicator, TParameters> : IIndicatorFactory<TIndicator, TParameters>
{
    /// <summary>
    /// Creates an indicator instance with the specified parameters
    /// </summary>
    public abstract TIndicator Create(TParameters parameters);

    /// <summary>
    /// Gets the implementation hint (always FirstParty)
    /// </summary>
    public ImplementationHint ImplementationHint => ImplementationHint.FirstParty;

    /// <summary>
    /// Gets a value indicating whether this factory is available (always true for FirstParty)
    /// </summary>
    public virtual bool IsAvailable => true;
}

/// <summary>
/// Factory for FisherTransform FirstParty implementation
/// </summary>
public class FisherTransformFirstPartyFactory<TInput, TOutput> : FirstPartyIndicatorFactory<IFisherTransform<TInput, TOutput>, PFisherTransform<TInput, TOutput>>
    where TInput : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    public override IFisherTransform<TInput, TOutput> Create(PFisherTransform<TInput, TOutput> parameters)
    {
        return Native.FisherTransform_FP<TInput, TOutput>.Create(parameters);
    }
}

/// <summary>
/// Factory for Supertrend FirstParty implementation
/// </summary>
public class SupertrendFirstPartyFactory<TPrice, TOutput> : FirstPartyIndicatorFactory<ISupertrend<TPrice, TOutput>, PSupertrend<TPrice, TOutput>>
    where TPrice : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    public override ISupertrend<TPrice, TOutput> Create(PSupertrend<TPrice, TOutput> parameters)
    {
        return new Native.Supertrend_FP<TPrice, TOutput>(parameters);
    }
}