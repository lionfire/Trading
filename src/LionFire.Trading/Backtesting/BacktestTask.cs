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
using LionFire.Templating;
using LionFire.Trading.Bots;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using LionFire.Trading;

namespace LionFire.Applications.Trading
{
    
    public class BacktestTask : AppTask, IMarketTask
    {
        #region Identity

        public DateTime BacktestDate { get; set; } = DateTime.Now;

        #endregion

        #region Relationships

        IMarket IMarketTask.Market { get { return this.Market; } }
        public BacktestMarket Market { get; private set; }

        #endregion

        #region Parameters

        public TBacktestMarket Config {
            get { return Market?.Config; }
            set {
                Market = LionFire.Templating.ITemplateExtensions.Create<BacktestMarket>(value);
            }
        }

        #endregion

        #region Construction

        public BacktestTask(TBacktestMarket config = null)
        {
            if (config != null)
            {
                this.Config = config;
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

            Market.Initialize();

            return await base.Initialize();
        }

        #endregion

        #region Run

        Stopwatch runStopwatch;
        protected override void Run()
        {
            logger.LogInformation($"Starting backtest from {Config.StartDate} to {Config.EndDate}");

            runStopwatch = System.Diagnostics.Stopwatch.StartNew();
            Market.Run();
            runStopwatch.Stop();
            OnFinished();
        }

        protected virtual void OnFinished()
        {
            foreach (var bot in Market.Participants.OfType<IBot>())
            {
                OnBotFinished(bot);
            }
            var saveTasks = new List<Task<string>>();
            if (Config.SaveBacktestBotConfigs)
            {
                foreach (var bot in Market.Participants.OfType<IBot>())
                {
                    saveTasks.Add(BotConfigRepository.SaveConfig(bot));
                }
            }
            Task.WaitAll(saveTasks.ToArray());

        }

        protected virtual void OnBotFinished(IBot bot)
        {
            var fitnessArgs = bot.Account.GetFitnessArgs();
            var account = bot.Account as BacktestAccount;

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

    public class BotConfigRepository
    {
        public static string ConfigDir { get { return @"E:\Trading\Configs\"; } }
        public static async Task<string> SaveConfig(IBot bot)
        {
            var dir = ConfigDir;
            dir = Path.Combine(dir, bot.GetType().Name);
            var version = bot.Version.GetMinorCompatibilityVersion();
            dir = Path.Combine(dir, version);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var config = bot.Config;
            if (config.Id == null)
            {
                config.Id = IdUtils.GenerateId();
            }

            var filename = config.Id + ".json";

            var path = Path.Combine(dir, filename);

            var json = JsonConvert.SerializeObject(bot.Config);

            using (var sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                await sw.WriteAsync(json);
            }

            return config.Id;
        }

    }


}
