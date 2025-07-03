//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using CryptoExchange.Net.Objects.Options;
using Hjson;
using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.Hosting;
using LionFire.IO;
using LionFire.Structures;
using LionFire.Threading;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.Journal;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Polly.Registry;
using Polly;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using LionFire.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using LionFire.Trading.Automation.Journaling.Trades;
using System.Threading.Tasks;
using Serilog.Core;

namespace LionFire.Trading.Automation;

/// <summary>
/// A backtest task that executes a batch of backtests.
/// 
/// Must be homogeneous:
/// - DefaultTimeFrame for bars
/// - date range
/// - Bot Type
/// - primary DefaultSymbol
/// 
/// Can be heterogeneous:
/// - Ticks
/// 
/// Reused:
/// - inputs (such as indicators, even if they have different lookback requirements)
/// </summary>
public sealed class BatchHarness<TPrecision>
    : BatchHarnessBase
    , IBacktestBatch
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    public int BatchId { get; } = batchCounter++;
    private static int batchCounter = 0;

    #endregion

    #region Contained by

    public override MultiSimContext MultiSimContext => Context.MultiSimContext;

    #endregion

    #region Dependencies

    #region (Derived)

    public BacktestsJournal Journal => MultiSimContext.Journal;

    #endregion

    #endregion

    #region Parameters

    public PSimAccount<TPrecision> PBacktestAccountPrototype { get; set; }
        = PSimAccount<TPrecision>.DefaultForBacktesting;

    ConcurrentDictionary<string, PSimAccount<TPrecision>> PBacktestAccountsPerSymbol = new();

    public BacktestOptions BacktestOptions { get; }
    public TradeJournalOptions TradeJournalOptions => Context.MultiSimContext.Optimization.POptimization.TradeJournalOptions;

    public DateChunker DateChunker => MultiSimContext.DateChunker;

    public ExchangeSymbol? ExchangeSymbol => Context.ExchangeSymbolTimeFrame;

    #region Derived

    public IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong)> Chunks;

    #region Must match across all parameters

    public Type PBotType => MultiSimContext.Parameters.PBotType ?? typeof(DBNull);
    public Type BotType => MultiSimContext.Parameters.BotType ?? typeof(DBNull);

    #endregion

    #region Directory

    public string BatchDirectory { get; }

    #endregion

    public bool TicksEnabled => MultiSimContext.Parameters.Features.Ticks();

    #endregion

    #endregion

    #region Lifecycle

    public BatchHarness(MultiSimContext multiSimContext, PBatch pBatch) : base(multiSimContext.ServiceProvider, pBatch)
    {
        try
        {
            Context = ActivatorUtilities.CreateInstance<BatchContext<TPrecision>>(
                multiSimContext.ServiceProvider,
                multiSimContext);

            BacktestOptions = ServiceProvider.GetRequiredService<IOptionsSnapshot<BacktestOptions>>().Value;

            Chunks = DateChunker.GetBarChunks(Start, EndExclusive, TimeFrame, shortOnly: ExecutionOptions?.ShortChunks ?? false);
            chunks = DateChunker.GetBarChunks(Start, EndExclusive, TimeFrame, ExecutionOptions?.ShortChunks ?? false);
            chunkEnumerator = chunks.GetEnumerator();

            if (Journal != null)
            {
                BatchDirectory = Journal.BatchDirectory;
                DisposeJournal = false;
            }
            else
            {
                throw new NotImplementedException("Journal must be set");
            }
        }
        catch (Exception ex)
        {
            runTaskCompletionSource.SetException(ex);
            throw;
        }
    }

    public async ValueTask Init()
    {
        try
        {
            long validationTime = 0;
            foreach (var p in PBacktests)
            {
                var sw = Stopwatch.StartNew();
                ValidateParameter(p);
                validationTime += sw.ElapsedMilliseconds;
                await CreateBot(p);
            }
            Log.Get<BatchHarness<TPrecision>>().LogInformation("Validation time: {validationTime}ms", validationTime);

            InitInputs();
        }
        catch (Exception ex)
        {
            runTaskCompletionSource.SetException(ex);
            throw;
        }
    }

    private async ValueTask CreateBot(PBotWrapper p)
    {
        IBot2 bot = InstantiateBot<TPrecision>(p.PBot);

        var pAccount = PBacktestAccountPrototype;

        if (bot is IBarsBot<TPrecision> barsBot
            && barsBot.Parameters.ExchangeSymbolTimeFrame.Symbol != null)
        {
            pAccount = PBacktestAccountsPerSymbol.GetOrAdd(barsBot.Parameters.ExchangeSymbolTimeFrame.Symbol, _ =>
                {
                    var copy = (PSimAccount<TPrecision>)PBacktestAccountPrototype.Clone();
                    copy.Bars = new HLCReference<TPrecision>(barsBot.Parameters.ExchangeSymbolTimeFrame);

                    copy.DefaultHolding.AssetProtection = PAssetProtection<TPrecision>.Default;

                    return copy;
                });
        }

        ExchangeSymbol exchangeSymbol = (bot.Parameters as IPSymbolBot2)?.ExchangeSymbol!;


        //var context = await BotBatchBacktestContext<TPrecision>.Create(this, bot, pAccount, tradeJournal);
        var context = new BotContext<TPrecision>(SimContext, new PBotContext<TPrecision>
        {
            Id = Context.GetNextBotId(),
            Bot = bot,
            PSimulatedAccount = pAccount,
            BotJournal = ActivatorUtilities.CreateInstance<BotTradeJournal<TPrecision>>(ServiceProvider, (bot.Parameters as IPSymbolBot2)?.ExchangeSymbol!, TradeJournalOptions, Context),
            ServiceProvider = ServiceProvider,
        });
        await context.OnStarting();

        bot.Context = context;
        bot.Init();

        backtests.Add(new BacktestState(p, bot, context));
    }

    protected override void ValidateParameter(PBotWrapper p)
    {
        if (p.PBot?.GetType() != PBotType) throw new ArgumentException("Bot type mismatch");
    }

    #endregion


    #region State

    public BatchContext<TPrecision> Context { get; }

    #region Convenience

    public SimContext<TPrecision> SimContext => Context;
    public override ISimContext ISimContext => Context;
    protected override ISimContext GetSimContext() => SimContext;

    private DateTimeOffset SimulatedCurrentDate => Context.SimulatedCurrentDate;

    #endregion

    #endregion

    #region State Machine

    readonly CancellationTokenSource CancellationTokenSource = new();

    public Task RunTask => runTaskCompletionSource == null ? Task.CompletedTask : runTaskCompletionSource.Task;

    IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunks;
    IEnumerator<((DateTimeOffset start, DateTimeOffset endExclusive) range, bool isLong)> chunkEnumerator;

    #region (Public)

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _ = Run();
        return Task.CompletedTask;
    }

    public async Task Cancel(CancellationToken cancellationToken = default)
    {
        CancellationTokenSource.Cancel();
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
            if (pauseAtDate > SimulatedCurrentDate)
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

    List<InputEnumeratorBase> AllInputEnumerators2 { get; set; } = default!; // Set by ctor

    public interface IInputSlotFulfiller
    {
        bool TryFulfill(InputSlot inputSlot);
    }

    // OLD  - can't remember why I thought this was a good idea
    //#if BacktestAccountSlottedParameters
    //    static InputInjectionInfo BacktestAccountSymbolInjectionInfo = new InputInjectionInfo(typeof(PBacktestAccount<TPrecision>).GetProperty(nameof(PBacktestAccount<TPrecision>.Bars))!, typeof(BacktestAccount2<TPrecision>).GetProperty(nameof(BacktestAccount2<TPrecision>.Bars))!);
    //#endif

    private void InitInputsForPBot(IPTimeFrameBot2? pBot, IBot2 bot, BotInfo botInfo, Dictionary<string, IndexedInput> inputEnumerators)
    {
        int inputEnumeratorIndex = -1;
        foreach (var inputInjectionInfo in (botInfo.InputParameterToValueMapping ?? Enumerable.Empty<InputParameterToValueMapping>()))
        {
            ArgumentNullException.ThrowIfNull(pBot);
            inputEnumeratorIndex++;

            int lookback = pBot.InputLookbacks == null
                ? 0
                : pBot.InputLookbacks[inputEnumeratorIndex];

            if (pBot is IHasInputMappings him)
            {
                InputMappingTools.InitInputsForBacktest(inputEnumerators, him, inputEnumeratorIndex, inputInjectionInfo, lookback, pBot);
            }
            else
            {
                // TEMP
                throw new UnreachableCodeException(); // REVIEW
            }
        }
    }


    /// <summary>
    /// Gather all inputs including derived ones for the inputs
    /// </summary>
    private void InitInputs()
    {
        var ir = ServiceProvider.GetRequiredService<IMarketDataResolver>();

        Dictionary<string, IndexedInput> inputEnumerators = [];

        #region InputItem creation, and determine max lookback for each input

        foreach (var backtest in backtests)
        {
            var pBacktest = backtest.PBacktest;

            InitInputsForPBot(pBacktest.PBot, backtest.Bot, backtest.BotInfo, inputEnumerators);

            // BacktestAccount
#if BacktestAccountSlottedParameters
            if (backtest.Controller.Account is BacktestAccount2<TPrecision> bta)
            {
                    NewMethod(inputEnumerators, backtest, inputEnumeratorIndex, BacktestAccountSymbolInjectionInfo, 1, bta.Parameters);
            }
#endif

        }
        #endregion

        //foreach (var backtest in backtests)
        //{
        //    if (backtest.Bot is ISymbolBarsBot<TPrecision> symbolBot)
        //    {
        //        symbolBot.Bars = ir.Resolve(symbolBot.PMultiSim.ExchangeSymbol);
        //    }

        //    //if (backtest.MultiSimContext.Account is BacktestAccount2<TPrecision> backtestAccount)
        //    //{
        //    //    var barsReference = backtestAccount.PMultiSim.Bars;

        //    //    if (barsReference != null) { throw new NotImplementedException(); }
        //    //    else
        //    //    {

        //    //        backtestAccount.Bars = ir.Resolve(backtestAccount.PMultiSim.Bars);
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

            InputMappingTools.HydrateInputMappings(inputEnumerators, backtest);

            foreach (var a in backtest.BotContext.SimulatedAccounts.Values.OfType<IHasInputMappings>()) { InputMappingTools.HydrateInputMappings(inputEnumerators, a); }

            backtest.InitFinished();
        }

        #endregion

        AllInputEnumerators2 = inputEnumerators.Values.Select(e => e.Enumerator!).ToList();

    }
    IEnumerable<IBot2> bots => backtests.Select(s => s.Bot);
    List<BacktestState> backtests = [];

    private record struct BacktestState(PBotWrapper PBacktest, IBot2 Bot, IBotContext<TPrecision> BotContext) : IHasInputMappings
    {

        #region Derived

        IAccount2? Account => BotContext.DefaultSimAccount;

        #endregion

        internal void InitFinished()
        {
            inputMappings = null!; // BREAKNULLABILITY
        }

        public BotInfo BotInfo { get; init; } = BotInfos.Get(PBacktest.PBot!.GetType(), Bot.GetType());

        #region IHasInputMappings

        internal readonly List<InputMapping> InputMappings => inputMappings;
        internal List<InputMapping> inputMappings = [];
        List<InputMapping> IHasInputMappings.InputMappings => InputMappings;
        IBarListener IHasInputMappings.Instance => Bot;

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

    #region (Public) Run

    BacktestBatchProgress? Progress;

    public Task Run()
    {
        Context.Start();
        if (backtests.Count == 0) throw new InvalidOperationException("No backtests");

        return Task.Run(async () =>
        {
            try
            {
                Progress = new() { BatchId = BatchId };
                MultiSimContext.BatchEvents.BatchStarting(Progress);
                Progress.Total = backtests.Count;

                await (TicksEnabled ? RunTicks() : RunBars()).ConfigureAwait(false);
                if (Context.IsCancelled)
                {
                    Debug.WriteLine(this.GetType().Name + " canceled");
                }
                else
                {
                    foreach (var b in backtests)
                    {
                        if (b.PBacktest.OnFinished != null)
                        {
                            b.PBacktest.OnFinished();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                runTaskCompletionSource.SetException(ex);
            }
            finally
            {
                MultiSimContext.BatchEvents.BatchFinished(Progress);
            }
        });

        #region (local)

        async Task RunBars()
        {
            TimeSpan timeSpan = TimeFrame.TimeSpan;

            #region Load lookback data (inputs only, no bot)

            #endregion

            int counter = 0;
            var sw = Stopwatch.StartNew();

            if (EndExclusive != Start) // MOVE: This should be checked in validation
            {
                throw new ArgumentException("Zero duration");
            }

            var totalTimeSpan = EndExclusive - Start;
            while (Context.SimulatedCurrentDate < EndExclusive
                && (!CancellationTokenSource.IsCancellationRequested)
                && (!Context.IsCancelled)
                )
            {
                counter++;
                if (counter % 1_000 == 0)
                {
                    Progress.PerUn = (Context.SimulatedCurrentDate - Start) / totalTimeSpan;
                }
                await AdvanceInputsByOneBar().ConfigureAwait(false);

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
                    var account = b.BotContext.DefaultSimAccount;
                    if (account != null)
                    {
                        if (account.IsAborted) continue;
                        account.OnBar();
                    }
                    else
                    {
                        foreach (var account2 in b.BotContext.SimulatedAccounts.Select(kvp => kvp.Value).Where(a => !a.IsAborted))
                        {
                            account2.OnBar();
                        }
                    }
                    b.Bot.OnBar();
                }
#else
#endif
                this.Context.SimulatedCurrentDate += timeSpan;
            }
            Debug.WriteLine($"{counter} bars for {backtests.Count} backtests in {sw.Elapsed.TotalSeconds}s ({(backtests.Count * counter / sw.Elapsed.TotalSeconds).ToString("N0")}/s)");

            await Task.WhenAll(backtests.Select(b => b.BotContext.OnFinished().AsTask())).ConfigureAwait(false);  // Close all positions, make BacktestResult ready

            foreach (var b in backtests)
            {
                var h = b.BotContext.DefaultSimAccount?.PrimaryHolding;

                var entry = new BacktestBatchJournalEntry
                {
                    BatchId = BatchId,
                    Id = b.BotContext.Id,
                    AD = h?.AnnualizedBalanceRoiVsDrawdownPercent ?? double.NaN, // REVIEW - wired up correctly?
                    AMWT = b.BotContext.BotJournal.JournalStats.AverageMinutesPerWinningTrade,
                    Wins = b.BotContext.BotJournal.JournalStats.WinningTrades,
                    Losses = b.BotContext.BotJournal.JournalStats.LosingTrades,
                    Breakevens = b.BotContext.BotJournal.JournalStats.BreakevenTrades,
                    UnknownTrades = b.BotContext.BotJournal.JournalStats.UnknownTrades, // TEMP - shouldn't happen. Write warnings to log instead
                    MaxBalanceDrawdown = Convert.ToDouble(h.MaxBalanceDrawdown),
                    MaxBalanceDrawdownPerunum = Convert.ToDouble(h.MaxBalanceDrawdownPerunum),
                    MaxEquityDrawdown = Convert.ToDouble(h.MaxEquityDrawdown),
                    MaxEquityDrawdownPerunum = Convert.ToDouble(h.MaxEquityDrawdownPerunum),

                    IsAborted = b.BotContext.BotJournal.IsAborted,
                    JournalEntries = b.BotContext.BotJournal.MemoryEntries

                    // TODO NEXT: PMultiSim
                };

                entry.Parameters = b.Bot.Parameters;

                //entry.PMultiSim = new ();
                //foreach(var p in ParameterMetadata.Get(PBotType).Items)
                //{
                //    entry.PMultiSim.Add(p.GetValue(b.PBacktest.Bot));
                //}

                //foreach (var p in b.PBacktest.Bot.PMultiSim)
                //{
                //    entry.PMultiSim.Add(p.ToString());
                //}

                while (!Journal.TryWrite(entry)) { await Task.Delay(10); }
            }

            OnFinishing();
            await OnFinished();

            runTaskCompletionSource.SetResult();
            //await StopAsync().ConfigureAwait(false);
        }

        async Task RunTicks()
        {
            while (Context.SimulatedCurrentDate < EndExclusive
                && !Context.IsCancelled
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

    protected override void OnFinishing()
    {
        base.OnFinishing();
    }

    protected override async ValueTask OnFinished()
    {
        MultiSimContext.BatchEvents.OnCompleted(backtests.Count);
        await base.OnFinished();

        //    await SaveBatchInfo(); // OLD

        if (DisposeJournal)
        {
            await Journal.DisposeAsync();
        }
    }

    public bool DisposeJournal { get; set; } = true; // OLD - never true anymore after ctor


#if OLD
    private async ValueTask SaveBatchInfo()
    {
        var r = GetOptimizationRunInfo<OptimizationRunInfo>();

        var json = JsonConvert.SerializeObject(r, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            
        });

        var hjsonValue = Hjson.JsonValue.Parse(json);
        var hjson = hjsonValue.ToString(hjsonOptions);

        await Task.Yield(); // Hjson is synchronous
        if (FilesystemRetryPipeline != null)
        {
            await FilesystemRetryPipeline.ExecuteAsync(_ =>
            {
                go();
                return ValueTask.CompletedTask;
            });
        }
        else { go(); }
        void go()
        {
            Context.BatchInfoFileWriter.SaveIfDifferent(hjson);
        }
    }
#endif

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
        //long chunkSize = DefaultTimeFrame.GetExpectedBarCount(chunkStart, chunkEndExclusive) ?? throw new NotSupportedException(nameof(DefaultTimeFrame));
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
                tasks.Add(input.PreloadRange(TimeFrame.AddBars(Start, -input.LookbackRequired), SimulatedCurrentDate).AsTask());
            }
        }
        if (tasks != null) { await Task.WhenAll(tasks).ConfigureAwait(false); }
    }

    private async ValueTask AdvanceInputsByOneBar()
    {
        bool first = chunkStart == default;

        if (chunkStart == default || SimulatedCurrentDate >= chunkEndExclusive)
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

            //if (inputs[0] is InputEnumerator<TPrecision> TPrecisionValues)
            //{
            //    Debug.WriteLine($"NextBar: {BacktestDate} Input[0] TPrecision: {TPrecisionValues.CurrentValue}");
            //}
            //else if (inputs[0] is InputEnumerator<decimal> decimalValues)
            //{
            //    //Debug.WriteLine($"NextBar: {BacktestDate} Input[0] decimal: {decimalValues.CurrentValue}");
            //}
            //else
            {
                Debug.WriteLine($"NextBar: {SimContext.SimulatedCurrentDate} Input[0]: {AllInputEnumerators2[0].GetType()}");
            }
        }
#endif
        //await Task.Delay(0); // TEMP
    }

    #endregion
}

#if UNUSED
/// <summary>
/// How to do this?
/// - 
/// </summary>
public class BacktestHarness_OLD
{
    //List<IIndicatorHarness> indicatorHarnesses;
    //List<BacktestBotHarness> backtestBotHarnesses;

    ConcurrentDictionary<Guid, BacktestBatchTask2> tasks = new();
    public bool IsAcceptingNewTasks { get; set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    private bool IsInitialized => Start != default;


    public BacktestHarness_OLD(IServiceProvider serviceProvider)
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

#endif

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


