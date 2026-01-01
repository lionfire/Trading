using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Journal;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation;

public class MarketParticipantContext<TPrecision>
{
    // REVIEW - offload to initializer class, and let it be GC'ed after init?
    public List<PInputToMappingToValuesWindowProperty>? InputMappings { get; set; } = [];

}

public sealed class BotContext<TPrecision> : MarketParticipantContext<TPrecision>, IBotContext, IBotContext2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    /// <summary>
    /// Gets the numeric ID for this context (from Parameters).
    /// </summary>
    public long Id => Parameters.Id;

    /// <inheritdoc />
    string IBotContext2<TPrecision>.Id => Id.ToString();

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider => Parameters.ServiceProvider;
    
    #endregion

    #region Relationships

    public SimContext<TPrecision> Sim { get; }

    /// <inheritdoc />
    IMarketContext<TPrecision> IBotContext2<TPrecision>.MarketContext => Sim;

    public IBot2 Bot { get; }

    public BotTradeJournal<TPrecision> BotJournal => Parameters.BotJournal;

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => (Bot.Parameters as IPBarsBot2)?.ExchangeSymbolTimeFrame;

    #endregion

    #endregion

    #region Parameters

    public PBotContext<TPrecision> Parameters { get; }

    
    public TimeFrame TimeFrame { get; set; }

    #endregion

    #region Lifecycle

    internal BotContext(SimContext<TPrecision> sim, PBotContext<TPrecision> parameters)
    {
        SimAccountFactory = () => DefaultSimAccount ?? throw new Exception("DefaultSimAccount is null. Set either SimAccountFactory or DefaultSimAccount");

        Sim = sim;
        Parameters = parameters;

        // Copied to this
        Bot = parameters.Bot;

        if (Bot.Parameters is IPBarsBot2 pbb && pbb.ExchangeSymbolTimeFrame.TimeFrame != null)
        {
            TimeFrame = pbb.ExchangeSymbolTimeFrame.TimeFrame;
        }
        else
        {
            throw new NotImplementedException("TODO: DefaultTimeFrame");
        }

        var e = ExchangeSymbolTimeFrame;
        if (e != null)
        {
            //GetSimulatedAccount(e);
            // Pass the bot's exchange area to SimAccount as a fallback if the account has UnknownExchange
            var exchangeAreaFallback = new ExchangeArea(e.Exchange, e.Area);
            defaultSimAccount = new SimAccount<TPrecision>(this, parameters.PSimulatedAccount, exchangeAreaFallback);
        }
        else
        {
            throw new NotImplementedException("TODO: How do we know which Exchange the account is on?");
        }

        //JobJournal.ExchangeSymbol = (bot.PMultiSim as IPSymbolBot2)?.ExchangeSymbol;
    }

    #endregion

    #region State

    public DateTimeOffset SimulatedCurrentDate { get; }

    public bool IsKeepingUpWithReality => false;

    #region Accounts

    public Dictionary<ExchangeSymbol, ISimAccount<TPrecision>>? SimulatedAccounts { get; private set; }
    object simulatedAccountsLock = new();

    public IAccount2<TPrecision>? Account { get; set; }
    public Func<ISimAccount<TPrecision>> SimAccountFactory { get; set; }
    public ISimAccount<TPrecision>? DefaultSimAccount => defaultSimAccount;
    private ISimAccount<TPrecision>? defaultSimAccount;

    /// <inheritdoc />
    IAccount2<TPrecision> IBotContext2<TPrecision>.DefaultAccount => DefaultSimAccount ?? throw new InvalidOperationException("DefaultSimAccount is not initialized");
    //public long GetNextTransactionId() => defaultSimAccount.GetNextTransactionId();

    #endregion

    #endregion

    #region Methods

    public static double GetFitness(BacktestResult backtestResult)
    {
        return backtestResult.AD;
    }

    #endregion

    #region Event Handling

    public ValueTask OnStarting()
    {
        // TODO: Log the current clock time somewhere, not in the Trade JobJournal
        // TODO: Log the time for any quick backtesting that needs to be done to get the bot state caught up.

        BotJournal.Write(new JournalEntry<TPrecision>(DefaultSimAccount)
        {
            EntryType = JournalEntryType.Start,
            Time = Sim.SimulatedCurrentDate,
        });
        return ValueTask.CompletedTask;
    }

    static int x = 0;

    public bool IsAborted => DefaultSimAccount?.IsAborted == true 
        || SimulatedAccounts?.Any(kvp => kvp.Value.IsAborted) == true;

    public async ValueTask OnFinished()
    {
        await Bot.OnBacktestFinished();

        var h = DefaultSimAccount?.PrimaryHolding!;

        var result = new BacktestResult
        {
            Aborted = IsAborted,
            MaxBalanceDrawdown = Convert.ToDouble(h.CurrentBalanceDrawdown),
            MaxBalanceDrawdownPerunum = Convert.ToDouble(h.MaxBalanceDrawdownPerunum),
            MaxEquityDrawdown = Convert.ToDouble(h.CurrentEquityDrawdown),
            MaxEquityDrawdownPercentages = Convert.ToDouble(h.MaxEquityDrawdownPerunum),
            AD = h.AnnualizedBalanceRoiVsDrawdownPercent,

            //Fitness = ,
            //WinningTrades = Account.WinningTrades,
            Id = "id-" + x++
        };
        result.Fitness = GetFitness(result);

        BotJournal.FileName = $"{(result.Aborted ? "ABORTED" : "")} {result.Fitness:0.000}f {(result.AD == result.Fitness ? "" : result.AD.ToString("0.0"))}ad  id={result.Id} {(result.MaxBalanceDrawdownPerunum * 100.0).ToString("0.0")}bddp";

        if (double.IsNaN(result.Fitness) || result.Fitness < BotJournal.Options.DiscardDetailsWhenFitnessBelow
            || !BotJournal.Options.EffectiveEnabled) { BotJournal.DiscardDetails = true; }

        if (!BotJournal.DiscardDetails)
        {
            if (result.Aborted) { BotJournal.IsAborted = true; }
            else
            {
            }
        }

        await BotJournal.Finish(result.Fitness);
        await BotJournal.DisposeAsync();
    }

    #endregion

}


//public class BotContext<TPrecision> : IBotContext<TPrecision>
//    where TPrecision : struct, INumber<TPrecision>
//{
//    public required IAccount2 Account { get; set; }
//}


