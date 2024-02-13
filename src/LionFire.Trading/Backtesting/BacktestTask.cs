using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using LionFire.Trading.Backtesting;
using LionFire.Applications;
using LionFire.Applications.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Instantiating;
using LionFire.Trading.Bots;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using LionFire.Trading;
using System.Reflection;

namespace LionFire.Applications.Trading;


public class BacktestTask : AppTask, IMarketTask
{
    #region Identity

    public DateTime BacktestDate { get; set; } = DateTime.Now;

    #endregion

    #region Relationships


    IAccount IMarketTask.Account { get { return BacktestAccount; } }
    public BacktestAccount BacktestAccount { get; private set; }

    #endregion

    #region Parameters

    public TBacktestAccount Config
    {
        get { return BacktestAccount?.Template; }
        set
        {
            throw new NotImplementedException("Instantiate with ctor that has parameters");
            //BacktestAccount =  value.Create<BacktestAccount>();
        }
    }

    #endregion

    #region Construction

    public BacktestTask(TBacktestAccount config = null)
    {
        if (config != null)
        {
            BacktestAccount = config.Create<BacktestAccount>();
            //this.Config = config;
        }
    }

    #endregion

    #region Init

    bool isInitialized = false;

    public override async Task<bool> Initialize()
    {
        if (isInitialized) return true;
        isInitialized = true;
        logger = this.GetLogger();

        if (await BacktestAccount.Initialize().ConfigureAwait(false) == false) { return false; }

        return await base.Initialize().ConfigureAwait(false);
    }

    #endregion

    #region Run

    Stopwatch runStopwatch;
    protected override async  Task Run()
    {
        logger.LogInformation($"Starting backtest from {Config.StartDate} to {Config.EndDate}");

        runStopwatch = System.Diagnostics.Stopwatch.StartNew();
        await BacktestAccount.Run();
        runStopwatch.Stop();
        OnFinished();
        //return Task.CompletedTask;
    }

    protected virtual void OnFinished()
    {
        foreach (var bot in BacktestAccount.Participants.OfType<IBot>())
        {
            OnBotFinished(bot);
        }
        var saveTasks = new List<Task<string>>();
        if (Config.SaveBacktestBotConfigs)
        {
            foreach (var bot in BacktestAccount.Participants.OfType<IBot>())
            {
                saveTasks.Add(BotConfigRepository.SaveConfig(bot));
            }
        }
        Task.WaitAll(saveTasks.ToArray());

    }

    protected virtual void OnBotFinished(IBot bot)
    {
        var account = bot.Account as BacktestAccount;
        var fitnessArgs = account?.GetFitnessArgs();

        double fitness;
        var customFitness = bot as IHasCustomFitness;
        if (customFitness != null)
        {
            fitness = customFitness.GetFitness(fitnessArgs);
        }
        else
        {
            fitness = double.NaN;
        }

        var RoiVsDd = ((account.NetProfitPercent * 100.0) / (Config.TimeSpan.TotalDays / 365.0)) / (account.MaxEquityDrawdownPercent * 100.0);

        logger.LogInformation($"[aroi/dd: {RoiVsDd.ToString("N2")}] [pft/yr: {((account.NetProfitPercent * 100.0) / (Config.TimeSpan.TotalDays / 365.0)).ToString("N1")}%] [dd%: {(account.MaxEquityDrawdownPercent * 100.0).ToString("N2")}] [{account.history.Count} trades] [pft: {(account.NetProfitPercent * 100.0).ToString("N1")}%] [Eq: {account.Equity.ToCurrencyString()}]  [bal: {account.Balance.ToCurrencyString()}] [Eq range: {account.MinEquity.ToCurrencyString()}-{account.MaxEquity.ToCurrencyString()}]");
        logger.LogInformation($"Backtest [time: {TimeSpan.FromMilliseconds(runStopwatch.ElapsedMilliseconds)}] [{Config.TimeSpan.TotalDays.ToString("N1")} days] [{(1000 * Config.TimeSpan.TotalDays / runStopwatch.ElapsedMilliseconds).ToString("N1") } days/sec] [{(Config.TotalBars / runStopwatch.ElapsedMilliseconds).ToString("N1")}k bars/sec]");

        //double MinProfitPerYear = 8;
        //double MaxDrawdownPercent = 40.0;
        //int MinTrades = 40;
    }

    #endregion

    #region Misc

    ILogger logger;

    #endregion
}
