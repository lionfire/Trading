using LionFire.Execution;
using LionFire.Structures;
using LionFire.Threading;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LionFire.Trading.Automation;

public class BacktestTask2 : IStartable
    , IStoppable
    , IPausable
    , IRunnable
//, IProgress<double>  // ENH, find another interface?  Rx?
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Input

    #endregion

    #region Output

    #endregion

    #region Parameters

    public IPBacktestTask2 Parameters { get; }
    public DateChunker DateChunker { get; }

    #region Derived

    public IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong)> Chunks;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestTask2(IServiceProvider serviceProvider, IPBacktestTask2 parameters, DateChunker? dateChunker = null)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
        DateChunker = dateChunker ?? ServiceProvider.GetRequiredService<DateChunker>();

        Chunks = DateChunker.GetBarChunks(Parameters.Start, Parameters.EndExclusive, Parameters.TimeFrame, shortOnly: Parameters.ShortChunks);
        chunks = DateChunker.GetBarChunks(Parameters.Start, Parameters.EndExclusive, Parameters.TimeFrame, Parameters.ShortChunks);
        chunkEnumerator = chunks.GetEnumerator();
        inputs = InitInputs();
    }

    #endregion

    #region State Machine

    //public CancellationToken Terminated => cancelledSource?.Token ?? CancellationToken.None;
    CancellationTokenSource? cancelledSource;

    public Task<bool> RunTask => resetEvent.WaitOneAsync();
    ManualResetEvent resetEvent = new(false);

    IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunks;
    IEnumerator<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunkEnumerator;

    #region (Public)

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancelledSource = new CancellationTokenSource();
        resetEvent.Reset();
        BacktestDate = Parameters.Start;

        if (Parameters.TicksEnabled())
        {
            RunTicks().FireAndForget();
        }
        else
        {
            RunBars().FireAndForget();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        resetEvent.Set();
        return Task.CompletedTask;
    }

    #region Pause and Continue

    // TODO - implement this

    public bool Paused { get; protected set; }

    /// <summary>
    /// For multi-backtest runs, wait for all to finish a certain date range before moving to the next.
    /// </summary>
    public DateTimeOffset PauseAtDate
    {
        get => pauseAtDate;
        set
        {
            pauseAtDate = value;
            if (pauseAtDate > BacktestDate)
            {
                Continue();
            }
            else
            {
                Pause();
            }
        }
    }
    private DateTimeOffset pauseAtDate;

    public Task Pause()
    {
        Paused = true;
        return Task.CompletedTask;
    }

    public Task Continue()
    {
        Paused = false;
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #endregion

    #region Inputs

    List<InputEnumeratorBase> inputs;
    List<AsyncInputEnumerator>? asyncInputs;

    //private void GetInputsChunk((DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //}

    /// <summary>
    /// Gather all inputs including derived ones for the inputs
    /// </summary>
    private List<InputEnumeratorBase> InitInputs()
    {
        var list = new List<InputEnumeratorBase>();

        var ir = ServiceProvider.GetRequiredService<IMarketDataResolver>();

        if (Parameters.Bot.Inputs != null)
        {
            int i = 0;
            foreach (var input in Parameters.Bot.Inputs)
            {
                IHistoricalTimeSeries series = ir.Resolve(input);

                int memory = Parameters.Bot.InputMemories == null ? 0 : Parameters.Bot.InputMemories[i];

                if (memory == 0) { memory = 1; }

                InputEnumeratorBase inputEnumerator;

                if (memory <= 1)
                {
                    inputEnumerator = (InputEnumeratorBase)
                    typeof(SingleValueInputEnumerator<>)
                    .MakeGenericType(series.ValueType)
                    .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType)])!
                    .Invoke(new object[] { series });
                }
                else
                {
                    inputEnumerator = (InputEnumeratorBase)
                        typeof(WindowedInputEnumerator<>)
                        .MakeGenericType(series.ValueType)
                        .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType), typeof(int)])!
                        .Invoke(new object[] { series, memory });
                }

                list.Add(inputEnumerator);

                //if (input is IHistoricalTimeSeries<T> series)
                //{
                //    list.Add(new WindowedInputEnumerator<T>(series));
                //}
                //else if (input is IIndicatorHarness<T> indicator)
                //{
                //    list.Add(new IndicatorInputProcessor<T>(indicator));
                //}
                //else
                //{
                //    throw new NotImplementedException();
                //}
            }
        }
        return list;
    }

    private void InitBot()
    {
        Bot = (IPBot2)ActivatorUtilities.CreateInstance(ServiceProvider, Parameters.Bot.InstanceType, Parameters.Bot);


        InitBotIndicators();


    }

    private void InitBotIndicators()
    {
        //throw new NotImplementedException();

        foreach (var pIndicator in Bot.Indicators)
        {
#error NEXT: resolve via IMarketDataResolver to a IHistoricalSeries?

            var h = new BufferingIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
            {
                IndicatorParameters = new TParameters
                {
                    //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
                    MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                    Period = 14,

                    //Source = 

                },
                TimeFrame = TimeFrame.h1,
                Inputs = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
            });

            var result = await h.GetReverseValues(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        }
    }

    protected IPBot2 Bot { get; private set; }

    #endregion

    #region Outputs

    private void InitOutputs()
    {
        // Outputs need to be calculated in a certain order

        var list = new List<object>();

        outputs = list;
    }
    List<object> outputs = new();

    #endregion

    #region Loop

    DateTimeOffset chunkStart;
    DateTimeOffset chunkEndExclusive;


    private async Task AdvanceInputChunk()
    {
        if (!chunkEnumerator.MoveNext())
        {
            await StopAsync().ConfigureAwait(false);
            return;
        }

        var chunk = chunkEnumerator.Current;
        chunkStart = chunk.range.start;
        chunkEndExclusive = chunk.range.endExclusive;
        long chunkSize = TimeFrame.GetExpectedBarCount(chunkStart, chunkEndExclusive) ?? throw new NotSupportedException(nameof(TimeFrame));

        await Task.WhenAll(inputs.Select(input =>
                 input.PreloadRange(chunkStart, chunkEndExclusive, (uint)chunkSize)
             )).ConfigureAwait(false);

    }

    public TimeFrame TimeFrame => Parameters.TimeFrame;
    private async ValueTask AdvanceInputsByOneBar()
    {
        if (chunkStart == default || BacktestDate >= chunkEndExclusive)
        {
            await AdvanceInputChunk().ConfigureAwait(false);
        }

        var asyncTasks = asyncInputs?.Select(i => i.MoveNextAsync());

        foreach (InputEnumeratorBase input in inputs)
        {
            input.MoveNext();
        }

        if (asyncTasks != null)
        {
            await Task.WhenAll(asyncTasks).ConfigureAwait(false);
        }
    }

    private async Task RunBars()
    {
        TimeSpan timeSpan = Parameters.TimeFrame.TimeSpan;

        while (BacktestDate < Parameters.EndExclusive
            && (false == cancelledSource?.IsCancellationRequested)
            )
        {
            await AdvanceInputsByOneBar().ConfigureAwait(false);
            await NextBar().ConfigureAwait(false);
            BacktestDate += timeSpan;
        }
        await StopAsync().ConfigureAwait(false);
    }

    private async Task RunTicks()
    {
        while (BacktestDate < Parameters.EndExclusive && (false == cancelledSource?.IsCancellationRequested)
            )
        {
            await NextTick();
            throw new NotImplementedException("advance one tick");
            //BacktestDate += IndicatorParameters.TimeFrame.TimeSpan;
        }

    }

    #endregion

    #region State

    public BacktestAccount2? BacktestAccount { get; private set; }
    public DateTimeOffset BacktestDate { get; protected set; } = DateTimeOffset.UtcNow;

    #endregion

    #region Methods

    public async Task NextTick()
    {
    }

    static int NextBarModulus = 0;
    public async Task NextBar()
    {
#if false && TRACE
        if (NextBarModulus++ > 100)
        {
            NextBarModulus = 0;

            if (inputs[0] is InputEnumerator<double> doubleValues)
            {
                Debug.WriteLine($"NextBar: {BacktestDate} Input[0] double: {doubleValues.CurrentValue}");
            }
            else if (inputs[0] is InputEnumerator<decimal> decimalValues)
            {
                //Debug.WriteLine($"NextBar: {BacktestDate} Input[0] decimal: {decimalValues.CurrentValue}");
            }
            else
            {
                Debug.WriteLine($"NextBar: {BacktestDate} Input[0]: {inputs[0].GetType()}");
            }
        }
#endif
    }

    #endregion
}

/// <summary>
/// How to do this?
/// - 
/// </summary>
public class BacktestHarness
{
    List<IIndicatorHarness> indicatorHarnesses;
    List<BacktestBotHarness> backtestBotHarnesses;

    ConcurrentDictionary<Guid, BacktestTask2> tasks = new();
    public bool IsAcceptingNewTasks { get; set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    private bool IsInitialized => Start != default;


    public BacktestHarness(IServiceProvider serviceProvider)
    {
    }

    public bool CanAdd(BacktestTask2 task)
    {
        if (!IsAcceptingNewTasks) return false;

        if (IsInitialized)
        {
            if (Start != task.Parameters.Start || EndExclusive != task.Parameters.EndExclusive) return false;
        }

        return true;
    }

    public int Commonality(BacktestTask2 task)
    {
        int commonality = 0;

        // Something like this:
        //foreach(var input in task.Inputs)
        //{
        //    if (this.InputCounts.Contains(input.Key))
        //    {
        //        commonality += InputCounts[input.Key];
        //    }
        //}

        return commonality;
    }

    public bool Enqueue(BacktestTask2 task)
    {
        if (!CanAdd(task)) return false;

        if (!IsInitialized)
        {
            Start = task.Parameters.Start;
            EndExclusive = task.Parameters.EndExclusive;
        }

        return true;
    }

    public Task Run(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}

// Activating bots
// - Try ctor(TValue)
// - Try ctor(IServiceProvider, TValue)
// - Try GetService<IFactory<TBot, TValue>>().Create(TValue)
// - save successful result to static
public class BacktestTask2<TParameters> : BacktestTask2
    //where TBot : IBot2
    where TParameters : IPBacktestTask2
{
    IBot2 Bot { get; }

    public BacktestTask2(IServiceProvider serviceProvider, TParameters parameters) : base(serviceProvider, parameters)
    {
        if (typeof(TParameters) is IFactory<IBot2> factory)
        {
            Bot = factory.Create();
        }
    }
}
