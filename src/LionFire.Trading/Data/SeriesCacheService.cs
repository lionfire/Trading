using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using LionFire.Execution;
using System.Threading.Tasks;
using LionFire.Instantiating;
using System.ComponentModel;
using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;
using LionFire.Structures;
using LionFire.Validation;
using LionFire.Execution.Executables;
using System.Threading;
using LionFire.DependencyInjection;
using LionFire.Dependencies;
using LionFire.Applications;

namespace LionFire.Trading.Data
{

    public interface IHasHealth
    {
        /// <summary>
        /// true means fully healthy, false means there is at least one issue, and null means health check (and remedies) are in progress.
        /// </summary>
        bool? IsHealthy { get; }
    }

    public interface IHealthMonitor
    {
        bool IsMonitoringHealth { get; set; }
    }

    public class HealthCheckSettings
    {
        public TimeSpan? TimeBetweenThoroughChecks { get; set; }
    }

    public interface IHealthChecker
    {
        Task CheckHealth();
    }
    public interface IHealthPoller
    {
        int CheckHealthMillisecondsInterval { get; set; }
    }

    public interface ISelfHealing
    {
        // TODO: Return IsHealthy
        Task<bool> Heal(); // TODO: Return a Job that can have in-depth progress info
    }


    public class SeriesCacheService : ITemplate<SSeriesCacheService>, INotifyPropertyChanged
        , IHealthPoller
    {

        public string Key => $"{Account.Template.Key}/{Symbol}/{TimeFrame.Name}";

        #region Parameters

        [SetOnce]
        public string Symbol { get; set; }

        //[SetOnce]
        //public string Broker { get; set; }

        [SetOnce]
        public IFeed Account { get; set; }

        //[SetOnce]
        //public string AccountType { get; set; }


        [SetOnce]
        [SerializeIgnore]
        public TimeFrame TimeFrame { get; set; }
        public string TimeFrameCode { get { return TimeFrame?.ToString(); } set { TimeFrame = TimeFrame.TryParse(value); } }

        #endregion

        #region Settings

        #region OldestDataFirst

        /// <summary>
        /// Default is false: download newest data first and work backwards.
        /// Set this to true to start downloading oldest data first instead.
        /// </summary>
        public bool OldestDataFirst
        {
            get { return oldestDataFirst; }
            set
            {
                if (oldestDataFirst == value) return;
                oldestDataFirst = value;
                OnPropertyChanged(nameof(OldestDataFirst));
            }
        }
        private bool oldestDataFirst;

        #endregion

        #region Auto

        public bool? Auto
        {
            get { return auto; }
            set
            {
                if (auto == value) return;
                auto = value;
                OnPropertyChanged(nameof(Auto));
            }
        }
        private bool? auto;

        #endregion

        #endregion

        #region State

        #region LastValidationDate

        public DateTime? LastValidationDate
        {
            get { return lastValidationDate; }
            set
            {
                if (lastValidationDate == value) return;
                lastValidationDate = value;
                OnPropertyChanged(nameof(LastValidationDate));
            }
        }
        private DateTime? lastValidationDate;

        #endregion


        #region IsHealthy

        public bool? IsHealthy
        {
            get { return isHealthy; }
            set
            {
                if (isHealthy == value) return;
                isHealthy = value;
                OnPropertyChanged(nameof(IsHealthy));
            }
        }

        public int CheckHealthMillisecondsInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private bool? isHealthy;

        #endregion

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }


    [HasDependencies]
    public class SSeriesCacheService : InitializableExecutableBase, ITemplateInstance<SeriesCacheService>, IStartable, IStoppable, ISelfHealing, IHasHealth, IHealthChecker, IKeyed<string>, IHasRunTask
    {

        #region Identity

        public string Key => Template.Key;
        [SetOnce]
        public SeriesCacheService Template { get; set; }

        #endregion


        [Dependency]
        public TradingOptions TradingOptions { get; set; }

        public MarketSeriesBase MarketSeries => Template.Account.GetMarketSeries(Template.Symbol, Template.TimeFrame); // TOMICROOPTIMIZE - cache

        #region Convenience

        public IHistoricalDataProvider HistoricalDataProvider => Template.Account.HistoricalDataProvider;

        #endregion
        #region Lifecycle

        private ILogger logger;

        public SSeriesCacheService()
        {
            logger = this.GetLogger();
        }

        //public Task<ValidationContext> Initialize()
        //{
        //    ValidationContext validationContext = null;
        //    if (!this.IsInitailized())
        //    {
        //        Task.FromResult(this.TryResolveDependencies(ref validationContext));
        //        if (validationContext.IsValid()) State = ExecutionStateEx.Ready;
        //    }
        //    return Task.FromResult(validationContext);
        //}


        //// TODO TOARCH - Is there an elegant way to call Initialize from the base class, and have derived classes create ValidationContext only if needed?
        //protected virtual void Initialize(ref ValidationContext validationContext)
        //{
        //    base.OnInitializing(ref validationContext);
        //    this.TryResolveDependencies(ref validationContext));
        //}

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            (await Initialize()).EnsureValid();

            State = ExecutionStateEx.Starting;
            // TODO FIXME: Enable autosave on Template.  It would be cool to do it from ITemplate Create mechanism.

            if (!Directory.Exists(TimeFrameDirectory))
            {
                logger.LogInformation($" + Creating {TimeFrameDirectory}");
                Directory.CreateDirectory(TimeFrameDirectory);
            }

            if (Template.TimeFrame == TimeFrame.m1)
            {
                var dirs = Directory.GetDirectories(TimeFrameDirectory).Select(d => Path.GetFileName(d)).OrderBy(s => s);
                if (dirs.Count() >= 1)
                {
                    var count = Convert.ToInt32(dirs.Last()) - Convert.ToInt32(dirs.First());
                    string countIndicator = count > 0 ? Enumerable.Repeat("|", count).Aggregate((x, y) => x + y) : "";
                    logger.LogInformation($" - {Template.Symbol} {Template.TimeFrame.Name} cache: {dirs.First()}-{dirs.Last()} {countIndicator}");
                }
            }
            else if (Template.TimeFrame == TimeFrame.h1)
            {
                var files = Directory.GetFiles(TimeFrameDirectory).Select(s => Path.GetFileName(s.Substring(0, s.IndexOf('.')))).Distinct().OrderBy(s => s);
                if (files.Count() > 0)
                {
                    var count = Convert.ToInt32(files.Last()) - Convert.ToInt32(files.First());
                    string countIndicator = count > 0 ? Enumerable.Repeat("|", count).Aggregate((x, y) => x + y) : "";
                    logger.LogInformation($" - {Template.Symbol} {Template.TimeFrame.Name} cache: {files.First()}-{files.Last()} {countIndicator}");
                }
            }

            cts = new CancellationTokenSource();
            RunTask = Task.Run(async () =>
            {
                var forceReretrieveEmptyData = TradingOptions.ForceReretrieveEmptyData;

                var results = await HistoricalDataProvider.GetData(MarketSeries, TradingOptions.EffectiveHistoricalDataStart, TradingOptions.EffectiveHistoricalDataEnd, true, forceReretrieveEmptyData: forceReretrieveEmptyData, cancellationToken: cts.Token);

                foreach (var result in results)
                {
                    logger.LogTrace($"{MarketSeries}: {result.Count} data points retrieved");
                }
                State = ExecutionStateEx.Stopped;
            });

            SetState(ExecutionStateEx.Starting, ExecutionStateEx.Started);
        }

        protected Task OnFinished()
        {
            return StopAsync();
        }

        public void Cancel()
        {
            var token = cts;
            token?.Cancel();
        }
        private CancellationTokenSource cts;

        public Task RunTask { get; private set; }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            SetState(ExecutionStateEx.Started, ExecutionStateEx.Stopping);
            SetState(ExecutionStateEx.Stopping, ExecutionStateEx.Stopped);
            return Task.CompletedTask;
        }

        //#error NEXT: Figure out service vs job architecture.  How to run this, vs how to monitor?  Should monitor be a generic thing?
        //public abstract class MonitorBase : ExecutableExBase, IStartable, IStoppable
        //{
        //    /// <summary>
        //    /// Return Validation items containing info about what needs to be done.  Empty or null if nothing to be done.
        //    /// </summary>
        //    /// <returns></returns>
        //    public abstract Task<ValidationContext> CheckStatus();
        //    public abstract Task Start();
        //    public abstract Task Stop();

        //    #region LastChecked

        //    public DateTime? LastChecked
        //    {
        //        get { return lastChecked; }
        //        set
        //        {
        //            if (lastChecked == value) return;
        //            lastChecked = value;
        //            OnPropertyChanged(nameof(LastChecked));
        //        }
        //    }
        //    private DateTime? lastChecked;

        //    #endregion


        //    public int PollMillisecondsInterval { get; set; }

        //}

        #endregion


        //@"C:\ProgramData\LionFire\Trading\Data\IC Markets\AUDCAD\m1\2017\1"; 
        public string RootDir { get; set; } = Path.Combine(DependencyContext.Current.GetService<AppDirectories>().AppProgramDataDir, "Data");

        public IFeed Account => Template.Account;

        public string TimeFrameDirectory
        {
            get
            {
                return Path.Combine(RootDir, Account.Template.BrokerName, Account.Template.AccountType ?? "", Template.Symbol, Template.TimeFrame.Name);
            }
        }

        public bool? IsHealthy => throw new NotImplementedException();

        /// <summary>
        /// Find end date: start from today (UTC) and crawl backwards to find last downloaded data.
        /// Find the start date
        /// Based on config, start downloading most recent data first
        /// </summary>
        /// <returns></returns>
        public Task CheckHealth()
        {
            //Directory.GetDirectories(Directory)
            throw new NotImplementedException();
        }

        /// <summary>
        /// Crawl through all data to find anomalies:
        ///  - long gaps
        ///  - strange spikes 
        ///    - TODO: Spike detection and cleanup algos.
        ///      - one-off blips
        ///      - daily or weekly spread spikes
        ///      - keep a log of changed data and reasoning, potential undo
        ///  
        /// </summary>
        /// <returns></returns>
        public Task Validate()
        {

            Template.LastValidationDate = DateTime.UtcNow;
            throw new NotImplementedException();
        }

        public Task<bool> Heal()
        {
            throw new NotImplementedException();
        }
    }
}
