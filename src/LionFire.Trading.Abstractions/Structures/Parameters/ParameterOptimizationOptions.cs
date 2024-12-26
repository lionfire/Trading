using LionFire.ExtensionMethods.Copying;
using ReactiveUI.SourceGenerators;
using ReactiveUI;
using System.Numerics;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reactive;

namespace LionFire.Trading;

public static class ParameterOptimizationOptions
{
    public static IParameterOptimizationOptions Create(Type valueType)
    {
        if (valueType.IsEnum) { valueType = Enum.GetUnderlyingType(valueType); }

        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType))!;
    }
    public static IParameterOptimizationOptions Create(Type valueType, IParameterOptimizationOptions assignFrom)
    {
        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType), assignFrom)!;
    }
}

public partial class ParameterOptimizationOptions<TValue>
    : ReactiveObject
    , IParameterOptimizationOptions
    where TValue : struct, INumber<TValue>
{
    public IObservable<Unit> SomethingChanged => somethingChanged;
    private Subject<Unit> somethingChanged = new();

    public Type ValueType => typeof(TValue);
    public ParameterOptimizationOptions() {
        this.PropertyChanged += ParameterOptimizationOptions_PropertyChanged;
        this.WhenAny(x=>x, x => x).Subscribe(x =>
        {
            Debug.WriteLine("WhenAny: " + x);
            somethingChanged?.OnNext(Unit.Default);
        });
        this.WhenAnyValue(x => x.MaxValue).Subscribe(x =>
        {
            Debug.WriteLine("WhenAnyValue MaxValue: " + x);
            somethingChanged?.OnNext(Unit.Default);
        });
    }

    private void ParameterOptimizationOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Debug.WriteLine("ParameterOptimizationOptions_PropertyChanged: " + e.PropertyName);
    }

    //public ParameterOptimizationOptions(IParameterOptimizationOptions assignFrom) : this()
    //{
    //    if (assignFrom != null)
    //    {
    //        AssignFromExtensions.AssignPropertiesFrom(this, assignFrom);
    //    }
    //}

    #region Derived

    public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : null;
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : ((MaxValue - MinValue) / TValue.CreateChecked(2.0));
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : (DefaultValue ?? MinValue);
    //((MaxValue - MinValue) / TValue.CreateChecked(2.0)));

    object IParameterOptimizationOptions.SingleValue => SingleValue ?? throw new InvalidOperationException($"{nameof(SingleValue)} not available when {nameof(IsEligibleForOptimization)} is false");

    public bool IsEligibleForOptimization => !SingleValue.HasValue && EnableOptimization != false; // TODO: This should be "can have more than one value"

    [Reactive]
    private bool? _enableOptimization;

    public TValue? Range => MaxValue - MinValue;

    //public Func<(TValue min, TValue max, int count), Func<int, TValue>> GetFunc
    //{
    //    // Use DistributionParameter as the log base
    //    if (DistributionParameter.HasValue)
    //    {
    //        return (min, max, count) => (int i) => min + (max - min) * Math.Pow(DistributionParameter.Value, i / count);
    //    }
    //}

    //return (min, max, count) => (int i) => min + (max - min) * i / count;
    #endregion

    public int? OptimizeOrder { get; set; }

    public object? MaxValueObj => MaxValue;
    public object? StepObj => Step;
    public object? MinValueObj => MinValue;

    #region Min

    public bool AllowNegativeValues { get; set; }

    public TValue? HardMinValue { get; set; }

    //[Reactive]
    public TValue? MinValue
    {
        get => _minValue;
        set
        {
            var changed = _minValue != value;
            this.RaiseAndSetIfChanged(ref _minValue, value);
            if (changed)
            {
                this.RaisePropertyChanged(nameof(EffectiveMinValue));
                this.RaisePropertyChanged(nameof(MinValueObj));
            }
        }
    }
    private TValue? _minValue;

    public bool HasMinValue => MinValue.HasValue;
    public TValue? DefaultMin { get; set; }
    public TValue EffectiveMinValue
    {
        get => MinValue ?? DefaultMin ?? HardMinValue ?? (AllowNegativeValues ? DataTypeMinValue : TValue.Zero);
        set => MinValue = value;
    }

    #endregion

    #region Max

    public TValue? HardMaxValue { get; set; }
    [Reactive]
    private TValue? _maxValue;

    public bool HasMaxValue => MaxValue.HasValue;
    public TValue? DefaultMax { get; set; }
    public TValue EffectiveMaxValue
    {
        get => MaxValue ?? DefaultMax ?? HardMaxValue ?? DefaultMaxForDataType;
        set => MaxValue = value;
    }

    #endregion

    #region Range (Min to max)

    public TValue EffectiveRange => (EffectiveMaxValue - EffectiveMinValue) + TValue.One;

    #endregion

    #region Default

    public TValue? DefaultValue { get; set; }
    public TValue EffectiveDefaultValue
    {
        get => DefaultValue ?? EffectiveMinValue; // TODO - what should the fallback default value be?
        set => DefaultValue = value;
    }
    public bool HasDefaultValue => DefaultValue.HasValue;
    object? IParameterOptimizationOptions.DefaultValue => DefaultValue;

    #endregion

    #region Step

    public TValue? MinStep { get; set; }
    public TValue EffectiveMinStep
    {
        get
        {
            if (MinStep.HasValue) return MinStep.Value;
            var range = EffectiveRange;

            if (range < TValue.CreateChecked(10)) { return TValue.CreateChecked(0.1); }

            return TValue.One;
        }
    }

    /// <summary>
    /// When optimizing, don't skip over ranges larger than this amount in order for the optimization to be considered complete.
    /// </summary>
    public TValue? MaxStep { get; set; }
    public TValue EffectiveMaxStep
    => MaxStep ?? DefaultMaxStep;
    [Reactive]
    private TValue? _step;

    public bool HasStep => Step.HasValue;
    public TValue EffectiveStep
    {
        get => Step ?? EffectiveMaxStep;
        set => Step = value;
    }

    const int DefaultMinOptimizationSteps = 16;
    private TValue DefaultMaxStep
    {
        get
        {
            var maxSteps = EffectiveMinStep <= TValue.Zero ? EffectiveRange : EffectiveRange / EffectiveMinStep;
            while (maxSteps > TValue.CreateChecked(DefaultMinOptimizationSteps))
            {
                maxSteps /= TValue.CreateChecked(2);
            }
            return maxSteps <= TValue.Zero ? TValue.AdditiveIdentity : EffectiveRange / maxSteps;
        }
    }

    #endregion


    #region Distribution function

    public double? DistributionParameter { get; set; }

    #endregion

    #region Limits

    public ulong? MinCount { get; set; }
    public ulong? MaxCount { get; set; }

    #endregion

    public double? FitnessOfInterest { get; set; }


    #region Derived

    public static TValue DefaultMaxForDataType
    {
        get
        {
            if (typeof(TValue) == typeof(bool)) return (TValue)(object)true;
            return TValue.CreateChecked(100);
        }
    }

    public static TValue DataTypeMaxValue => (TValue)typeof(TValue).GetField(nameof(int.MaxValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;
    public static TValue DataTypeMinValue => (TValue)typeof(TValue).GetField(nameof(int.MinValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;

    public ulong MinCountFromMaxStep => Convert.ToUInt64(Math.Ceiling(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMaxStep)));
    public ulong? EffectiveMinCount => Math.Max(MinCount ?? 0, MinCountFromMaxStep);

    public ulong MaxCountFromMinStep => (EffectiveMinStep <= TValue.Zero ? Convert.ToUInt64(Convert.ToSingle(EffectiveRange)) : Convert.ToUInt64(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMinStep)));
    public ulong EffectiveMaxCount => Math.Min((MaxCount ?? ulong.MaxValue), MaxCountFromMinStep);

    #endregion

    #region Misc

    public IParameterOptimizationOptions Clone() => (IParameterOptimizationOptions)MemberwiseClone();
    public override string ToString() => this.ToXamlProperties();

    #endregion
}
