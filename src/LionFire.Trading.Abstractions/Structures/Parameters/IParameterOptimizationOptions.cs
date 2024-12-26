using ReactiveUI;
using System.Reactive;

namespace LionFire.Trading;

public interface IParameterOptimizationOptions : IReactiveNotifyPropertyChanged<IReactiveObject>, IReactiveObject
{
    Type ValueType { get; }
    int? OptimizeOrder { get; set; }
    double? DistributionParameter { get; }
    //object? MinStep { get; }
    //object? MaxStep { get; }
    object? MaxValueObj { get; }
    object? StepObj { get; }
    object? MinValueObj { get; }
    //int? MinProbes { get; }
    //int? MaxProbes { get; }

    double? FitnessOfInterest { get; set; }

    #region Derived

    ulong? EffectiveMinCount { get; }
    ulong EffectiveMaxCount { get; }

    //int StepsPossible { get; }

    bool IsEligibleForOptimization { get; }
    bool? EnableOptimization { get; set; }
    object SingleValue { get; }
    object? DefaultValue { get; }

    #endregion

    IParameterOptimizationOptions Clone();

    IObservable<Unit> SomethingChanged { get; }
}
