
using LionFire.Structures;
using LionFire.Trading.HistoricalData;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using Spectre.Console;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators;

public interface IIndicator2
{


}

public interface IIndicator2<TParameters, TBar, TResult>
{

    #region Parameters

    int StartIndex { get; set; }

    TParameters Parameters { get; set; }

    #region Derived

    int? WarmUpPeriod { get; }

    #endregion

    #endregion

    #region Input

    void OnBars(TBar[] bars);

    #endregion

    #region Output

    IReadOnlyList<TResult> Output { get; }

    void Calculate(int index);
    void CalculateAll();


    #endregion

}

public class AlgoContext
{
    public bool Backtesting { get; set; }
    public TimeSpan TimeFrameInterval { get; internal set; }
}

public abstract class Indicator2Base<TParameters, TBar, TResult> : IIndicator2
//where TParameters : IIndicatorParameters
{
    #region State

    protected List<TBar> input = new List<TBar>();
    protected List<TResult> output = new List<TResult>();

    #endregion

    public AlgoContext Context { get; set; }

    #region Parameters

    public TParameters Parameters
    {
        get => parameters;
        set
        {
            if (Context?.Backtesting != true) { parameters = value; }
            OnParameters(value);
        }
    }
    TParameters parameters;
    protected virtual void OnParameters(TParameters parameters) { }

    #region Derived

    /// <summary>
    /// Delay between start of input bars and start of output.
    /// </summary>
    protected abstract int? WarmUpPeriod { get; }

    #endregion
    #endregion

    #region Input

    public virtual void OnInput(TBar[] bars)
    {
        //input.AddRange(bars);
    }

    #endregion

    #region Output

    public IReadOnlyList<TResult> Output => output;

    protected int GetOutputIndex(int index) => index + (WarmUpPeriod ?? throw new ArgumentException(nameof(WarmUpPeriod)));

    protected abstract bool IsValueCalculated(TResult result);

    protected bool IsCalculated(int index)
    {
        var outputIndex = GetOutputIndex(index);
        return outputIndex <= output.Count && IsValueCalculated(output[index]);
    }

    #endregion


    protected virtual IEnumerable<OutputInfo> OutputInfos => DefaultOutputInfos;
    static List<OutputInfo> DefaultOutputInfos = new List<OutputInfo>
    {
        new OutputInfo("Default"),
    };


}
public class OutputInfo : IKeyed
{
    public OutputInfo() { }
    public OutputInfo(string name, string description = null)
    {
        Key = name.Replace(" ", "");
        Name = name;
        Description = description;
    }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class QuantConnectIndicator<TParameters> : Indicator2Base<TParameters, OhlcvItem, IndicatorDataPoint>
{

    protected override bool IsValueCalculated(IndicatorDataPoint result)
    {
        //return result.Value != default;
        throw new NotImplementedException();
    }

    public DateTime StartTime { get; set; }
    public TimeSpan Interval { get; set; }

    protected override int? WarmUpPeriod => throw new NotImplementedException();

    protected virtual void OnParameters(TParameters parameters) { }
}


public class IndicatorParameters
{
    public bool Backtesting { get; set; }
}

public class PeriodParameter : IndicatorParameters
{
    public int Period { get; set; }
}
public class MoneyFlowIndexAdapter : QuantConnectIndicator<PeriodParameter>
{
    #region State

    MoneyFlowIndex native;

    #endregion

    #region Parameters

    #region Event Handling

    protected override void OnParameters(PeriodParameter parameters)
    {
        native = new MoneyFlowIndex(parameters.Period);
    }

    #endregion

    #region Derived

    protected override int? WarmUpPeriod => native?.WarmUpPeriod;

    #endregion

    #endregion

    #region Input

    public DateTime? InputTimeCursor;
    public int InputIndex;

    public override void OnInput(OhlcvItem[] tradeBars)
    {
        if (!InputTimeCursor.HasValue && (StartTime == default || Interval == default)) throw new ArgumentException($"{nameof(StartTime)} and {nameof(Interval)} must be set");

        if (!InputTimeCursor.HasValue) { InputTimeCursor = StartTime; }

        foreach (var bar in tradeBars)
        {
            native.Update(
                new TradeBar
                {
                    Time = InputTimeCursor.Value,
                    Open = bar.Open,
                    High = bar.High,
                    Low = bar.Low,
                    Close = bar.Close,
                    Volume = bar.Volume,
                }
                );
            if (InputIndex > WarmUpPeriod)
            {
                Recorder.OnNext((InputTimeCursor.Value, native.Current.Value));
                output.Add(native.Current);
            }
        }

        InputTimeCursor += Context.TimeFrameInterval;
        //base.OnInput(tradeBars); ;
    }

    #endregion

    #region Output

    IndicatorRecorder<decimal> Recorder = new();

    protected override IEnumerable<OutputInfo> OutputInfos => outputInfos;
    static List<OutputInfo> outputInfos = new List<OutputInfo>
    {
        new OutputInfo("Positive Money Flow"),
        new OutputInfo("Negative Money Flow"),
    };

    #endregion

    //protected override bool IsValueCalculated((IndicatorDataPoint, IndicatorDataPoint) result)
    //{
    //    ;
    //}
}

public class IndicatorRecorder<TOutput> : IObservable<TOutput>
{
    //Subject<TOutput>
    public void OnNext((DateTime, TOutput) next)
    {

    }

    public IDisposable Subscribe(IObserver<TOutput> observer)
    {
        throw new NotImplementedException();
    }
}