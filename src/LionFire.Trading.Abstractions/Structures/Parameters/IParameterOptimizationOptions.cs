using ReactiveUI;
using System.Reactive;
using System.Text.Json.Serialization;

namespace LionFire.Trading;

public interface IParameterOptimizationOptions : IReactiveNotifyPropertyChanged<IReactiveObject>, IReactiveObject
{
    #region Identity

    string Path { get; }

    [JsonIgnore]
    HierarchicalPropertyInfo Info { get; }

    #endregion

    [JsonIgnore]
    Type ValueType { get; }
    int? OptimizeOrder { get; set; }
    double? Exponent { get; }
    //object? MinStep { get; }
    //object? MaxStep { get; }
    object? MaxValueObj { get; }
    object? StepObj { get; }
    object? MinValueObj { get; }
    bool HasStep { get; }
    //int? MinProbes { get; }
    //int? MaxProbes { get; }

    double? FitnessOfInterest { get; set; }

    #region Derived

    ulong? EffectiveMinCount { get; }
    ulong EffectiveMaxCount { get; }

    //int StepsPossible { get; }

    bool IsEligibleForOptimization { get; }
    bool? EnableOptimization { get; set; }
    object? SingleValue { get; }
    object? DefaultValue { get; }

    #endregion

    IParameterOptimizationOptions Clone();

    /// <summary>
    /// Apply numeric overrides from plan-level parameter configuration.
    /// Only non-null values are applied.
    /// </summary>
    void ApplyNumericOverrides(double? minValue, double? maxValue, double? step, double? exponent);

    [JsonIgnore]
    IObservable<Unit> SomethingChanged { get; }
}
