using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation;

public class MultiBacktestContext 
{
    #region Dependencies
    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Configuration

    public BacktestOptions BacktestOptions => ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestOptions>>().CurrentValue;

    #endregion

    #region Parameters

    public PMultiBacktestContext Parameters { get; }
    public POptimization POptimization => Parameters.POptimization;

    public BacktestExecutionOptions ExecutionOptions
    {
        get => executionOptions ??= executionOptions
            ?? ServiceProvider.GetService<IOptionsMonitor<BacktestExecutionOptions>>()?.CurrentValue
            ?? new BacktestExecutionOptions();
        set => executionOptions = value;
    }
    private BacktestExecutionOptions? executionOptions;

    #region Derived

    public ExchangeSymbol ExchangeSymbol => Parameters.ExchangeSymbol;
    public Type BotType => Parameters.BotType;

    public CancellationToken CancellationToken { get; set; }

    #endregion

    #endregion

    #region Lifecycle

    public static MultiBacktestContext Create(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        return ActivatorUtilities.CreateInstance<MultiBacktestContext>(serviceProvider, parameters);
    }

    public MultiBacktestContext(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
        if (Parameters.POptimization == null) { throw new ArgumentNullException(nameof(Parameters.POptimization)); }
    }

    #endregion

    #region State

    public BestJournalsTracker BestJournalsTracker => bestJournalsTracker ??= new(this);
    private BestJournalsTracker? bestJournalsTracker;
    public MultiBacktestEvents Events { get; } = new();

    #endregion

    #region LogDirectory

    // BLOCKING I/O, first time used
    public string LogDirectory
    {
        get
        {
            if (logDirectory == null)
            {
                logDirectory = GetBatchDirectory(); // BLOCKING I/O
                BatchInfoFileWriter = new(Path.Combine(LogDirectory, $"BatchInfo.hjson"));
            }
            return logDirectory;
        }
    }
    private string? logDirectory;

    private string GetBatchDirectory() // BLOCKING I/O
    {
        var path = BacktestOptions.Dir;

        string botTypeName = BotType.Name;

        if (BotType.IsGenericType)
        {
            int i = botTypeName.IndexOf('`');
            if (i >= 0) { botTypeName = botTypeName[..i]; }
        }

        if (ExecutionOptions.BotSubDir) { path = System.IO.Path.Combine(path, botTypeName); }
        if (ExecutionOptions.ExchangeSubDir) { path = System.IO.Path.Combine(path, ExchangeSymbol?.Exchange ?? "UnknownExchange"); }
        if (ExecutionOptions.ExchangeAreaSubDir && ExchangeSymbol?.ExchangeArea != null) { path = System.IO.Path.Combine(path, ExchangeSymbol.ExchangeArea); }
        if (ExecutionOptions.SymbolSubDir) { path = System.IO.Path.Combine(path, ExchangeSymbol?.Symbol ?? "UnknownSymbol"); }

        path = FilesystemUtils.GetUniqueDirectory(path, "", "", 4); // BLOCKING I/O

        return path;
    }

    #endregion

    #region State

    //public int TradeJournalCount { get; set; }

    #region Derived

    //public bool ShouldLogTradeDetails => (POptimization == null || TradeJournalCount < POptimization?.MaxDetailedJournals);
    public bool ShouldLogTradeDetails => POptimization.TradeJournalOptions.EffectiveEnabled;

    #endregion

    #endregion

    public UniqueFileWriter BatchInfoFileWriter { get; private set; }
}

