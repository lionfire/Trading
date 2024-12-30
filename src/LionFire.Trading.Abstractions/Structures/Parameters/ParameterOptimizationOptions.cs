using LionFire.ExtensionMethods.Copying;
using ReactiveUI.SourceGenerators;
using ReactiveUI;
using System.Numerics;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reactive;
using System.Text.Json.Serialization;

namespace LionFire.Trading;

public static class ParameterOptimizationOptions
{
    public static ParameterOptimizationOptions<TValue> Create<TValue>(HierarchicalPropertyInfo info)
        where TValue : struct, INumber<TValue>
    {
        return (ParameterOptimizationOptions<TValue>)Create(info);
    }
    public static IParameterOptimizationOptions Create(HierarchicalPropertyInfo info)
    {
        var valueType = info.ValueType;
        if (valueType.IsEnum) { valueType = Enum.GetUnderlyingType(valueType); }

        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType), info)!;
    }
#if UNUSED
    public static IParameterOptimizationOptions Create(Type valueType, IParameterOptimizationOptions assignFrom)
    {
        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType), assignFrom)!;
    }
#endif
}

public partial class ParameterOptimizationOptions<TValue>
    : ReactiveObject
    , IParameterOptimizationOptions
    where TValue : struct, INumber<TValue>
{

    #region Identity
    public Type ValueType => typeof(TValue);

    [JsonIgnore]
    public string Path { get; }

    [JsonIgnore]
    public HierarchicalPropertyInfo Info { get; }

    private static HierarchicalPropertyInfo StubInfo = new HierarchicalPropertyInfo(typeof(DBNull));
    #endregion

    #region Lifecycle

    public ParameterOptimizationOptions(HierarchicalPropertyInfo info)
    {
        Path = info.Path;
        Info = info;

        this.PropertyChanged += ParameterOptimizationOptions_PropertyChanged;
        //this.WhenAny(x => x, x => x).Subscribe(x =>
        //{
        //    Debug.WriteLine("WhenAny: " + x);
        //    somethingChanged?.OnNext(Unit.Default);
        //});
        //this.WhenAnyValue(x => x.MaxValue).Subscribe(x =>
        //{
        //    Debug.WriteLine($"POO[{info.Key}] - WhenAnyValue MaxValue: " + x);
        //    somethingChanged?.OnNext(Unit.Default);
        //});
    }

    #endregion

    #region Events

    public IObservable<Unit> SomethingChanged => somethingChanged;
    private Subject<Unit> somethingChanged = new();

    #endregion

    #region Event handling

    private void ParameterOptimizationOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Debug.WriteLine("ParameterOptimizationOptions_PropertyChanged: " + e.PropertyName);
    }

    #endregion

    //public ParameterOptimizationOptions(IParameterOptimizationOptions assignFrom) : this()
    //{
    //    if (assignFrom != null)
    //    {
    //        AssignFromExtensions.AssignPropertiesFrom(this, assignFrom);
    //    }
    //}

    [Reactive]
    private bool? _enableOptimization;


    #region Derived

    public TValue? SingleValue
    {
        get => MinValue.HasValue && MinValue == MaxValue ? MinValue : null;
        set
        {
            var oldHasSingleValue = HasSingleValue;
            if (value == SingleValue) return;
            MinValue = MaxValue = value;
            this.RaisePropertyChanged(nameof(SingleValue));
            if (oldHasSingleValue != HasSingleValue)
            {
                this.RaisePropertyChanged(nameof(HasSingleValue));
            }
        }
    }
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : ((MaxValue - MinValue) / TValue.CreateChecked(2.0));
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : (DefaultValue ?? MinValue);
    //((MaxValue - MinValue) / TValue.CreateChecked(2.0)));

    public bool HasSingleValue
    {
        get => SingleValue.HasValue;
        set
        {
            if (value == HasSingleValue) return;
            if (value)
            {
                SingleValue = (EffectiveMaxValue - EffectiveMinValue) / TValue.CreateChecked(2.0);
            }
            else
            {
                EffectiveMaxValue = DefaultMax ?? HardValueMax ?? DefaultMaxForDataType;
                EffectiveMinValue = DefaultMin ?? HardValueMin ?? DefaultMinForDataType;
            }
        }
    }
    public TValue DefaultSingleValue => EffectiveDefaultValue;


    object? IParameterOptimizationOptions.SingleValue => SingleValue;// ?? throw new InvalidOperationException($"{nameof(SingleValue)} not available when {nameof(IsEligibleForOptimization)} is false");

    public bool IsEligibleForOptimization => !SingleValue.HasValue && EnableOptimization != false
        //&& this.Info.ParameterAttribute != null
        ; // TODO: This should be "can have more than one value"

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

    #region TEMP - workaround for ReactiveUI not supporting generic properties

    public object? MaxValueObj => MaxValue;
    public object? StepObj => Step;
    public object? MinValueObj => MinValue;
    #endregion

    #region Min
    public TValue EffectiveValueMin => ValueMin ?? EffectiveHardValueMin;
    public TValue? ValueMin { get; set; }
    public TValue EffectiveHardValueMin
    {
        get => HardValueMin ?? DefaultMinForDataType;
        set
        {
            HardValueMin = value;
        }
    }

    public TValue DefaultMinForDataType => TValue.Zero;

    public bool AllowNegativeValues { get; set; }

    [Reactive]
    private TValue? _hardValueMin;

    //[Reactive]
    public TValue? MinValue
    {
        get => _minValue;
        set
        {
            var changed = _minValue != value;
            if (value > MaxValue || value > EffectiveMaxValue)
            {
                MaxValue = value;
                changed = true;
            }
            if (changed)
            {
                this.RaiseAndSetIfChanged(ref _minValue, value);
                this.RaisePropertyChanged(nameof(EffectiveMinValue));
                //this.RaisePropertyChanged(nameof(MinValueObj));
                //OnSomethingChanged();
            }
        }
    }
    private TValue? _minValue;

    public bool HasMinValue => MinValue.HasValue;
    public TValue? DefaultMin { get; set; }
    public TValue EffectiveMinValue
    {
        get => MinValue ?? DefaultMin ?? HardValueMin ?? (AllowNegativeValues ? DataTypeMinValue : TValue.Zero);
        set => MinValue = value;
    }

    #endregion

    #region Max

    public TValue EffectiveValueMax => ValueMax ?? EffectiveHardValueMax;
    public TValue? ValueMax { get; set; }

    public TValue EffectiveHardValueMax
    {
        get => HardValueMax ?? DefaultMaxForDataType;
        set
        {
            HardValueMax = value;
        }
    }

    [Reactive]
    private TValue? _hardValueMax;

    public TValue? MaxValue
    {
        get => _maxValue;
        set
        {
            var changed = _maxValue != value;
            if (value < MinValue)
            {
                MinValue = value;
                changed = true;
            }
            if (changed)
            {
                this.RaiseAndSetIfChanged(ref _maxValue, value);
                this.RaisePropertyChanged(nameof(EffectiveMaxValue));
                //this.RaisePropertyChanged(nameof(MaxValueObj));
                //OnSomethingChanged();
            }

        }
    }
    //[Reactive]
    private TValue? _maxValue;
    void OnSomethingChanged() => somethingChanged.OnNext(Unit.Default);

    public bool HasMaxValue => MaxValue.HasValue;
    public TValue? DefaultMax { get; set; }
    public TValue EffectiveMaxValue
    {
        get => MaxValue ?? DefaultMax ?? HardValueMax ?? DefaultMaxForDataType;
        set
        {
            if (value < MinValue || value < EffectiveMinValue)
            {
                MinValue = value;
            }
            MaxValue = value;
        }
    }

    #endregion

    #region Range (Min to max)

    public TValue EffectiveRange => (EffectiveMaxValue - EffectiveMinValue) + TValue.One;

    #endregion

    #region Default

    public TValue? DefaultValue { get; set; }
    public TValue EffectiveDefaultValue
    {
        get => DefaultValue ?? (DefaultMax ?? HardValueMax ?? DefaultMaxForDataType) - (DefaultMin ?? HardValueMin ?? DefaultMinForDataType) / TValue.CreateChecked(2.0);
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
            var maxSteps = EffectiveMinStep <= TValue.Zero ? EffectiveRange : (EffectiveRange / EffectiveMinStep);
            while (maxSteps > TValue.CreateChecked(DefaultMinOptimizationSteps))
            {
                maxSteps /= TValue.CreateChecked(2);
            }
            return maxSteps <= TValue.Zero ? TValue.CreateChecked(1) : (EffectiveRange / maxSteps);
        }
    }

    #endregion


    #region Step Distribution


    #region Exponent (for Step)

    public double ExponentSliderStep { get; set; } = 0.1;

    [Reactive]
    private double? _exponent;

    public bool HasExponent => Exponent.HasValue;
    public double EffectiveExponent
    {
        get => Exponent ?? DefaultExponent ?? DefaultDefaultExponent;
        set => Exponent = value;
    }
    public double? DefaultExponent { get; set; }
    public double DefaultDefaultExponent => 1.0;

    public ExponentBasisOrigin ExponentOrigin { get; set; }

    public double? MinExponent { get; set; } = 1.0;
    public double EffectiveMinExponent
    {
        get
        {
            if (MinExponent.HasValue) return MinExponent.Value;
            return 1.0;
        }
    }

    public double? MaxExponent { get; set; }
    public double EffectiveMaxExponent
    => MaxExponent ?? DefaultMaxExponent;

    private double DefaultMaxExponent => 10.0;

    #endregion


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

    public ulong MinCountFromMaxStep => EffectiveRange < TValue.Zero ? 1 :
        (EffectiveMaxStep <= TValue.Zero ? Convert.ToUInt64(Math.Ceiling(Convert.ToSingle(EffectiveRange))) : Convert.ToUInt64(Math.Ceiling(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMaxStep))));
    public ulong? EffectiveMinCount => Math.Max(MinCount ?? 0, MinCountFromMaxStep);

    public ulong MaxCountFromMinStep => EffectiveMinStep == TValue.Zero ? 1 : Convert.ToUInt64(Math.Abs(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMinStep)));
    public ulong EffectiveMaxCount => Math.Min((MaxCount ?? ulong.MaxValue), MaxCountFromMinStep);


    #endregion

    #region Misc

    public IParameterOptimizationOptions Clone() => (IParameterOptimizationOptions)MemberwiseClone();
    public override string ToString() => this.ToXamlProperties();

    #endregion
}

public enum ExponentBasisOrigin
{
    Unspecified,
    Zero = 1 << 0,
    MinValue = 1 << 1,
}