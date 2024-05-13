namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

public abstract class IndicatorParameters<TIndicator> : IIndicatorParameters, IParametersFor<TIndicator>
{
    public Type IndicatorType => typeof(TIndicator);
    public abstract Type InputType { get; }
    public abstract Type OutputType { get; }

    public int Memory { get; set; } = 1;

}

public abstract class IndicatorParameters<TIndicator, TOutput> : IndicatorParameters<TIndicator>
{
    /// <summary>
    /// Matches TOutput
    /// </summary>
    public override Type InputType => typeof(TOutput);
    public override Type OutputType => typeof(TOutput);
}
public abstract class IndicatorParameters<TIndicator, TInput, TOutput> : IndicatorParameters<TIndicator>
{
    /// <summary>
    /// Matches TOutput
    /// </summary>
    public override Type InputType => typeof(TInput);
    public override Type OutputType => typeof(TOutput);
}

public interface IParametersFor<T> { }


