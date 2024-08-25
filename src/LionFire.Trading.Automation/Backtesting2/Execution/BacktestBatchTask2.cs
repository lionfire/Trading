//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.Structures;
using LionFire.Threading;
using LionFire.Trading.Automation.Bots;
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

/// <summary>
/// A backtest task that supports batching of backtests.
/// 
/// Must be homogeneous:
/// - TimeFrame for bars
/// - date range
/// 
/// Can be heterogeneous:
/// - Ticks
/// 
/// Reused:
/// - inputs (such as indicators, even if they have different lookback requirements)
/// </summary>
public class BacktestBatchTask2
    : BotBatchControllerBase
    , IBacktestBatch
{
    #region Identity

    public override BotExecutionMode BotExecutionMode => BotExecutionMode.Backtest;

    #endregion

    #region Parameters


    public BacktestExecutionOptions? ExecutionOptions { get; }
    public DateChunker DateChunker { get; }

    #region Derived

    public IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong)> Chunks;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestBatchTask2(IServiceProvider serviceProvider, IEnumerable<IPBacktestTask2> parameters, BacktestExecutionOptions? executionOptions = null, DateChunker? dateChunker = null) : base(serviceProvider, parameters)
    {
        try
        {
            ExecutionOptions = executionOptions ?? ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestExecutionOptions>>().CurrentValue;
            DateChunker = dateChunker ?? ServiceProvider.GetRequiredService<DateChunker>();

            Chunks = DateChunker.GetBarChunks(Start, EndExclusive, TimeFrame, shortOnly: ExecutionOptions.ShortChunks);
            chunks = DateChunker.GetBarChunks(Start, EndExclusive, TimeFrame, ExecutionOptions.ShortChunks);
            chunkEnumerator = chunks.GetEnumerator();

            foreach (var p in PBacktests)
            {
                CreateBot(p);
            }

            InitInputs();
        }
        catch (Exception ex)
        {
            runTaskCompletionSource.SetException(ex);
            throw;
        }
    }

    public PBacktestAccount<double> PBacktestAccountPrototype { get; set; } = PBacktestAccount<double>.Default;

    ConcurrentDictionary<string, PBacktestAccount<double>> SymbolPBacktestAccounts = new();

    private void CreateBot(IPBacktestTask2 p)
    {
        var bot = (IBot2)(Activator.CreateInstance(p.Bot.MaterializedType)
            ?? throw new Exception("Failed to create bot: " + p.Bot.MaterializedType));
        bot.Parameters = p.Bot;

        var pAccount = PBacktestAccountPrototype;

        if (bot is IBarsBot<double> barsBot)
        {
            pAccount = SymbolPBacktestAccounts.GetOrAdd(barsBot.Parameters.ExchangeSymbolTimeFrame.Symbol, _ =>
            {
                var copy = (PBacktestAccount<double>)PBacktestAccountPrototype.Clone();
                copy.Bars = new HLCReference<double>(barsBot.Parameters.ExchangeSymbolTimeFrame);
                return copy;
            });
        }

        var controller = new BacktestBotController<double>(this, bot, pAccount);
        bot.Controller = controller;
        bot.Init();
        backtests.Add(new BacktestState(p, bot, controller));
    }

    #endregion

    #region State Machine

    readonly CancellationTokenSource cancelledSource = new();

    public Task RunTask => runTaskCompletionSource == null ? Task.CompletedTask : runTaskCompletionSource.Task;

    IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunks;
    IEnumerator<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunkEnumerator;

    #region (Public)

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        BacktestDate = Start;
        if (backtests.Count == 0) throw new InvalidOperationException("No backtests");
        Run();

        return Task.CompletedTask;
    }

    public async Task Cancel(CancellationToken cancellationToken = default)
    {
        cancelledSource.Cancel();
        await runTaskCompletionSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    //public async Task StopAsync(CancellationToken cancellationToken = default)
    //{
    //    await tcs.Task.ConfigureAwait(false);
    //    return Task.CompletedTask;
    //}

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

    //List<InputEnumeratorBase> inputs;
    //List<AsyncInputEnumerator>? asyncInputs;
    //List<InputEnumeratorBase> indicatorInputs;

    IEnumerable<InputEnumeratorBase> AllInputEnumerators
    {
        get
        {
            if (AllInputEnumerators2 != null)
            {
                foreach (var input in AllInputEnumerators2)
                {
                    yield return input;
                }
            }
            //if (indicatorInputs != null)
            //{
            //    foreach (var input in indicatorInputs)
            //    {
            //        yield return input;
            //    }
            //}
        }
    }
    //private void GetInputsChunk((DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //}

    private class InputItem
    {
        public int Index { get; set; }
        public int Lookback { get; set; }
        public required IPInput PInput { get; init; }
        public InputEnumeratorBase? Enumerator { get; set; }
    }

    List<InputEnumeratorBase> AllInputEnumerators2 { get; set; } = default!; // Set by ctor


    public interface IInputSlotFulfiller
    {
        bool TryFulfill(InputSlot inputSlot);
    }

#if BacktestAccountSlottedParameters
    static InputInjectionInfo BacktestAccountSymbolInjectionInfo = new InputInjectionInfo(typeof(PBacktestAccount<double>).GetProperty(nameof(PBacktestAccount<double>.Bars))!, typeof(BacktestAccount2<double>).GetProperty(nameof(BacktestAccount2<double>.Bars))!);
#endif

    /// <summary>
    /// Gather all inputs including derived ones for the inputs
    /// </summary>
    private void InitInputs()
    {
        var ir = ServiceProvider.GetRequiredService<IMarketDataResolver>();

        Dictionary<string, InputItem> inputEnumerators = new();

        #region InputItem creation, and determine max lookback for each input

        foreach (var backtest in backtests)
        {
            var pBacktest = backtest.PBacktest;

            int inputEnumeratorIndex = -1;
            foreach (var inputInjectionInfo in (backtest.BotInfo.InputInjectionInfos ?? Enumerable.Empty<InputInjectionInfo>()))
            {
                inputEnumeratorIndex++;
                int lookback = pBacktest.Bot.InputLookbacks == null ? 0 : pBacktest.Bot.InputLookbacks[inputEnumeratorIndex];
                //IPInput pHydratedInput;

                //{
                //    IPInput pInput = (IPInput)inputInjectionInfo.Parameter.GetValue(backtest.PBacktest.Bot, null)!;

                //    while (pInput is IPInputThatSupportsUnboundInputs unboundInput)
                //    {
                //        pInput = new PBoundInput(unboundInput, backtest.PBacktest.Bot);
                //    }

                //    pHydratedInput = pInput;
                //}

                //var key = pHydratedInput.Key;

                //backtest.InstanceInputInfos.Add(new InstanceInputInfo(pHydratedInput, inputInjectionInfo));

                ////if (firstBarsInput == null && pHydratedInput.ValueType.IsAssignableTo(typeof(IKlineMarker)))
                ////{
                ////    firstBarsInput = pHydratedInput;
                ////}

                //if (inputEnumerators.TryGetValue(key, out InputItem? value))
                //{
                //    value.Lookback = Math.Max(value.Lookback, lookback);
                //}
                //else
                //{
                //    inputEnumerators.Add(key, new InputItem
                //    {
                //        PInput = pHydratedInput,
                //        //PInput = (IPInput)typeInputInfo.Parameter.GetValue(backtest.PBacktest.Bot, null)!,
                //        Lookback = lookback,
                //        Index = inputEnumeratorIndex,
                //    });
                //}
                NewMethod(inputEnumerators, backtest, inputEnumeratorIndex, inputInjectionInfo, lookback, backtest.PBacktest.Bot);
            }

            // BacktestAccount
#if BacktestAccountSlottedParameters
            if (backtest.Controller.Account is BacktestAccount2<double> bta)
            {
                if ()
                {
                    NewMethod(inputEnumerators, backtest, inputEnumeratorIndex, BacktestAccountSymbolInjectionInfo, 1, bta.Parameters);
                }
            }
#endif

            static void NewMethod(Dictionary<string, InputItem> inputEnumerators, BacktestState backtest, int inputEnumeratorIndex, InputInjectionInfo inputInjectionInfo, int lookback, IPTimeFrameMarketProcessor instanceObject)
            {
                IPInput pHydratedInput;

                {
                    IPInput pInput = (IPInput)inputInjectionInfo.Parameter.GetValue(instanceObject, null)!;

                    while (pInput is IPInputThatSupportsUnboundInputs unboundInput)
                    {
                        pInput = new PBoundInput(unboundInput, instanceObject);
                    }

                    pHydratedInput = pInput;
                }

                var key = pHydratedInput.Key;

                backtest.InstanceInputInfos.Add(new InstanceInputInfo(pHydratedInput, inputInjectionInfo));

                //if (firstBarsInput == null && pHydratedInput.ValueType.IsAssignableTo(typeof(IKlineMarker)))
                //{
                //    firstBarsInput = pHydratedInput;
                //}

                if (inputEnumerators.TryGetValue(key, out InputItem? value))
                {
                    value.Lookback = Math.Max(value.Lookback, lookback);
                }
                else
                {
                    inputEnumerators.Add(key, new InputItem
                    {
                        PInput = pHydratedInput,
                        //PInput = (IPInput)typeInputInfo.Parameter.GetValue(backtest.PBacktest.Bot, null)!,
                        Lookback = lookback,
                        Index = inputEnumeratorIndex,
                    });
                }
            }
        }

        #endregion

        //foreach (var backtest in backtests)
        //{
        //    if (backtest.Bot is ISymbolBarsBot<double> symbolBot)
        //    {
        //        symbolBot.Bars = ir.Resolve(symbolBot.Parameters.ExchangeSymbol);
        //    }

        //    //if (backtest.Controller.Account is BacktestAccount2<double> backtestAccount)
        //    //{
        //    //    var barsReference = backtestAccount.Parameters.Bars;

        //    //    if (barsReference != null) { throw new NotImplementedException(); }
        //    //    else
        //    //    {

        //    //        backtestAccount.Bars = ir.Resolve(backtestAccount.Parameters.Bars);
        //    //    }
        //    //}
        //}

        #region Create enumerator for each input

        foreach (var (key, value) in inputEnumerators)
        {
            IHistoricalTimeSeries series = ir.Resolve(value.PInput);

            if (value.Lookback == 0)
            {
                inputEnumerators[key].Enumerator = (InputEnumeratorBase)typeof(SingleValueInputEnumerator<,>)
                   .MakeGenericType(series.ValueType, series.PrecisionType)
                   .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType)])!
                   .Invoke([series]);
            }
            else if (value.Lookback < 0) throw new ArgumentOutOfRangeException(nameof(value.Lookback));
            else
            {
                inputEnumerators[key].Enumerator = (InputEnumeratorBase)typeof(ChunkingInputEnumerator<,>)
                    .MakeGenericType(series.ValueType, series.PrecisionType)
                    .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType), typeof(int)])!
                    .Invoke([series, value.Lookback]);
            }
        }

        #endregion

        #region Provide the Input Enumerator's window to each bot (and accounts)

        foreach (var backtest in backtests)
        {
#if FUTURE // Maybe
            foreach (var a in backtest.Controller.Accounts)
            // OPTIMIZE - this is currently superfluous, when there's just one slot for Bars
            {
                var pAccount = a.Parameters;
                if (pAccount is IPMayHaveUnboundInputSlots slotted)
                {
                    var values = PBoundSlots.ResolveSlotValues(slotted, backtest.Bot.Parameters);
                    int index = -1;
                    foreach (var slotInfo in SlotsInfo.GetSlotsInfo(a.Parameters.GetType()).Slots)
                    {
                        index++;
                        if (slotInfo.ParameterProperty!.GetValue(slotted) == null) 
                        {
                            slotInfo.ParameterProperty!.SetValue(slotted, values[index]);
                        }
                    }
                }
            }
#endif

            var pBacktest = backtest.PBacktest;

            HydrateIII(inputEnumerators, backtest);
            foreach (var a in backtest.Controller.Accounts.OfType<IHasInstanceInputInfos>()) { HydrateIII(inputEnumerators, a); }
            backtest.InitFinished();
        }

        #endregion

        AllInputEnumerators2 = inputEnumerators.Values.Select(e => e.Enumerator!).ToList();

        static void HydrateIII(Dictionary<string, InputItem> inputEnumerators, IHasInstanceInputInfos backtest)
        {
            foreach (var iii in backtest.InstanceInputInfos)
            {
                var inputEnumerator = inputEnumerators[iii.PInput.Key];
                iii.TypeInputInfo.Values.SetValue(backtest.Instance, inputEnumerator.Enumerator!.Values);
            }
        }
    }

    

    IEnumerable<IBot2> bots => backtests.Select(s => s.Bot);
    List<BacktestState> backtests = new();

    private record struct BacktestState(IPBacktestTask2 PBacktest, IBot2 Bot, IBotController<double> Controller) : IHasInstanceInputInfos
    {
        internal List<InstanceInputInfo> InstanceInputInfos => instanceInputInfos;
        internal List<InstanceInputInfo> instanceInputInfos = new();

        #region Derived

        IAccount2? Account => Controller.Account;

        #endregion

        internal void InitFinished()
        {
            instanceInputInfos = null;
        }

        public BotInfo BotInfo { get; init; } = BotInfos.Get(PBacktest.Bot.GetType(), Bot.GetType());

        #region IHasInstanceInputInfos

        List<InstanceInputInfo> IHasInstanceInputInfos.InstanceInputInfos => InstanceInputInfos;
        object IHasInstanceInputInfos.Instance => Bot;
        
        #endregion
    }

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


    #region (Private) Run

    private void Run()
    {
        Task.Run(async () =>
        {
            try
            {
                await (TicksEnabled ? RunTicks() : RunBars()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                runTaskCompletionSource.SetException(ex);
            }
        }, cancelledSource.Token).FireAndForget();

        #region (local)

        async Task RunBars()
        {
            TimeSpan timeSpan = TimeFrame.TimeSpan;

            #region Load lookback data (inputs only, no bot)

            #endregion

            int counter = 0;
            var sw = Stopwatch.StartNew();
            while (BacktestDate < EndExclusive
                && (false == cancelledSource?.IsCancellationRequested)
                )
            {
                counter++;
                await AdvanceInputsByOneBar().ConfigureAwait(false);
                //await NextBar().ConfigureAwait(false);
#if truex
                Parallel.ForEach(backtests, b =>
                {
                    var account = b.Controller.Account;
                    if (account != null) account.OnBar();
                    else
                    {
                        foreach (var account2 in b.Controller.Accounts) { account2.OnBar(); }
                    }
                    b.Bot.OnBar();
                });
#elif truex
                await Task.WhenAll(backtests.Select(b => Task.Run(() =>
                {
                    //Task.Yield();
                    var account = b.Controller.Account;
                    if (account != null) account.OnBar();
                    else
                    {
                        foreach (var account2 in b.Controller.Accounts) { account2.OnBar(); }
                    }
                    b.Bot.OnBar();
                })));
#elif true
                foreach (var b in backtests)
                {
                    var account = b.Controller.Account;
                    if (account != null) account.OnBar();
                    else
                    {
                        foreach (var account2 in b.Controller.Accounts) { account2.OnBar(); }
                    }
                    b.Bot.OnBar();
                }
#else
#endif
                BacktestDate += timeSpan;
            }
            Debug.WriteLine($"{counter} bars for {backtests.Count} backtests in {sw.Elapsed.TotalSeconds}s ({(backtests.Count * counter / sw.Elapsed.TotalSeconds).ToString("N0")}/s)");

            await Task.WhenAll(backtests.Select(b => b.Bot.OnBacktestFinished().AsTask())).ConfigureAwait(false);  // Close all positions, make BacktestResult ready

            OnFinished();
            runTaskCompletionSource.SetResult();
            //await StopAsync().ConfigureAwait(false);
        }

        async Task RunTicks()
        {
            while (BacktestDate < EndExclusive && (false == cancelledSource?.IsCancellationRequested)
                )
            {
                await NextTick();
                throw new NotImplementedException("advance one tick");
                //BacktestDate += IndicatorTimeFrame.TimeSpan;
            }
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// Runs after all backtests are finished
    /// </summary>
    protected void OnFinished()
    {
    }

    #region Inputs

    DateTimeOffset chunkStart;
    DateTimeOffset chunkEndExclusive;

    private void AdvanceInputChunk()
    {
        if (!chunkEnumerator.MoveNext())
        {
            throw new Exception("AdvanceInputChunk: unexpected end of data");
            //await StopAsync().ConfigureAwait(false);
            //return;
        }

        var chunk = chunkEnumerator.Current;
        chunkStart = chunk.range.start;
        chunkEndExclusive = chunk.range.endExclusive;
        if (EndExclusive < chunkEndExclusive) { chunkEndExclusive = EndExclusive; }
        //long chunkSize = TimeFrame.GetExpectedBarCount(chunkStart, chunkEndExclusive) ?? throw new NotSupportedException(nameof(TimeFrame));
    }

    private Task PreloadInputChunk() => Task.WhenAll(
            AllInputEnumerators
                .Select(input => input.PreloadRange(chunkStart, chunkEndExclusive))
                .Where(t => !t.IsCompletedSuccessfully)
                .Select(t => t.AsTask())
        );

    private async ValueTask LoadInputLookback()
    {
        List<Task>? tasks = null;
        foreach (InputEnumeratorBase input in AllInputEnumerators)
        {
            if (input.LookbackRequired > 0)
            {
                tasks ??= new();
                tasks.Add(input.PreloadRange(TimeFrame.AddBars(Start, -input.LookbackRequired), BacktestDate).AsTask());
            }
        }
        if (tasks != null) { await Task.WhenAll(tasks).ConfigureAwait(false); }
    }

    private async ValueTask AdvanceInputsByOneBar()
    {
        bool first = chunkStart == default;

        if (chunkStart == default || BacktestDate >= chunkEndExclusive)
        {
            AdvanceInputChunk();

            if (first)
            {
                await LoadInputLookback().ConfigureAwait(false);
                chunkStart = Start;
            }

            await PreloadInputChunk().ConfigureAwait(false);
        }

        //var asyncTasks = asyncInputs?.Select(i => i.MoveNextAsync());

        foreach (InputEnumeratorBase input in AllInputEnumerators)
        {
            input.MoveNext();
        }

        //if (asyncTasks != null)
        //{
        //    await Task.WhenAll(asyncTasks).ConfigureAwait(false);
        //}
    }

    #endregion

    #region State

    readonly TaskCompletionSource runTaskCompletionSource = new();
    public DateTimeOffset BacktestDate { get; protected set; } = DateTimeOffset.UtcNow;

    #endregion

    #region Methods

    public Task NextTick()
    {
        throw new NotImplementedException();
    }

    static int NextBarModulus = 0;
    public async ValueTask NextBar()
    {
#if true && TRACE
        if (NextBarModulus++ > 50000)
        {
            NextBarModulus = 0;

            //if (inputs[0] is InputEnumerator<double> doubleValues)
            //{
            //    Debug.WriteLine($"NextBar: {BacktestDate} Input[0] double: {doubleValues.CurrentValue}");
            //}
            //else if (inputs[0] is InputEnumerator<decimal> decimalValues)
            //{
            //    //Debug.WriteLine($"NextBar: {BacktestDate} Input[0] decimal: {decimalValues.CurrentValue}");
            //}
            //else
            {
                Debug.WriteLine($"NextBar: {BacktestDate} Input[0]: {AllInputEnumerators2[0].GetType()}");
            }
        }
#endif
        //await Task.Delay(0); // TEMP
    }

    #endregion
}

/// <summary>
/// How to do this?
/// - 
/// </summary>
public class BacktestHarness
{
    //List<IIndicatorHarness> indicatorHarnesses;
    //List<BacktestBotHarness> backtestBotHarnesses;

    ConcurrentDictionary<Guid, BacktestBatchTask2> tasks = new();
    public bool IsAcceptingNewTasks { get; set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    private bool IsInitialized => Start != default;


    public BacktestHarness(IServiceProvider serviceProvider)
    {
    }

    public bool CanAdd(BacktestBatchTask2 task)
    {
        if (!IsAcceptingNewTasks) return false;

        if (IsInitialized)
        {
            if (Start != task.Start || EndExclusive != task.EndExclusive) return false;
        }

        return true;
    }

    public int Commonality(BacktestBatchTask2 task)
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

    public bool Enqueue(BacktestBatchTask2 task)
    {
        if (!CanAdd(task)) return false;

        if (!IsInitialized)
        {
            Start = task.Start;
            EndExclusive = task.EndExclusive;
        }

        return true;
    }

    public Task Run(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}

//// Activating bots
//// - Try ctor(TValue)
//// - Try ctor(IServiceProvider, TValue)
//// - Try GetService<IFactory<TBot, TValue>>().Create(TValue)
//// - save successful result to static
//public class BacktestTask2<TParameters> : BacktestTask2
//    //where TBot : IBot2
//    where TParameters : IPBacktestTask2
//{

//    public BacktestTask2(IServiceProvider serviceProvider, TParameters parameters) : base(serviceProvider, parameters)
//    {
//        if (typeof(TParameters) is IFactory<IBot2> factory)
//        {
//            Bot = factory.Create();
//        }
//    }
//}
