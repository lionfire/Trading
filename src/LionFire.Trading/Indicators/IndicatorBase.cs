using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Instantiating;
using LionFire.Execution;
using LionFire.Reactive.Subjects;
using System.Diagnostics;
using LionFire.Reactive;

namespace LionFire.Trading.Indicators
{

    public abstract partial class IndicatorBase<TIndicator> : IIndicator, IExecutable
        where TIndicator : ITIndicator, new()
    {
        #region Relationships

        //#if cAlgo
        public virtual IEnumerable<IndicatorDataSeries> Outputs
        //#else
        //            public virtual IEnumerable<IDoubleDataSeries> Outputs
        //#endif
        {
            get
            {
                yield break;
            }
        }

        public void SetBlank(int index)
        {
#if !cAlgo
            foreach (var o in Outputs)
            {
                o.SetBlank(index);
            }
#else
            foreach (var o in Outputs.OfType<IndicatorDataSeries>())
            {
                o[index] = double.NaN;
            }
#endif
        }

        protected int CalculatedCount
        {
            get
            {
                var output = Outputs.FirstOrDefault();
                return output == null ? 0 : output.Count;
            }
        }
        public int LastIndex
        {
            get
            {
#if cAlgo
                foreach (var o in Outputs)
                {
                    return o.Count - 1;
                }
                return int.MinValue;
#else
                foreach (var o in Outputs)
                {
                    return o.LastIndex;
                }
                return int.MinValue;
#endif
            }
        }

        #region Derived

        protected virtual MarketSeries series
        {
            get
            {
#if cAlgo
                return Bot == null ? this.MarketSeries : Bot.MarketSeries;
#else
                //return this.MarketSeries;
                return null;
#endif
            }
            // add set to make it faster?
        }

        #endregion

        #endregion

        #region Configuration

        #region Template

        public TIndicator Template
        {
            get { return template; }
            set { template = value; OnConfigChanged(); }
        }
        private TIndicator template = new TIndicator();

        protected virtual void OnConfigChanged()
        {
        }

        #endregion

        ITIndicator IIndicator.Template { get { return this.Template; } set { this.Template = (TIndicator)value; } }
        ITemplate ITemplateInstance.Template { get { return Template; } set { this.Template = (TIndicator)value; } }

        #endregion


        #region Construction and Init

        public IndicatorBase()
        {
#if cAlgo
            LionFireEnvironment.ProgramName = "Trading";
#endif
        }

        public IndicatorBase(TIndicator config) : this()
        {
            this.Template = config;
        }

        // Not in cAlgo: called by OnStarting
        public void Init()
        {
            OnInitializing();
            OnInitialized();
        }


        

        protected virtual void OnInitializing()
        {
            State = ExecutionState.Initializing;
            try
            {
                try
                {
                    InitLog();
                }
                catch (Exception e)
                {
                    throw new Exception("InitLog threw", e);
                }
                try
                {
                    _InitPartial();
                }
                catch (Exception ex)
                {
                    throw new Exception("_InitPartial threw", ex);
                }

                ValidateConfiguration();

#if cAlgo
            //if (Bot == null)
            //{
            //    throw new Exception("TEMP - Bot == null in OnInitializing()");
            //}
            if (Bot != null && Bot.Indicators == null)
            {
                throw new Exception("Bot != null && Bot.Indicators == null in OnInitializing()");
            }
#endif
                if (EffectiveIndicators == null)
                {
                    throw new Exception("EffectiveIndicators == null");
                }
                if (!Outputs.Any())
                {
                    throw new Exception("!Outputs.Any().  Override Outputs.");
                }

                foreach (var child in Children.OfType<IIndicator>())
                {
#if !cAlgo
                    if (child.Account == null)
                    {
                    // REvIEW - shouldn't be needed here?
                        child.Account = this.Account;
                    }
                    child.Start();
#endif
                }
                State = ExecutionState.Ready;
            }
            catch (Exception)
            {
                State = ExecutionState.Uninitialized;
                throw;
            }
        }

        
        protected virtual void ValidateConfiguration()
        {
        }


        protected virtual void OnInitialized()
        {
            OnInitialized_();

        }
        partial void OnInitialized_();

        partial void _InitPartial();

        protected void InitializeOutputs()
        {
            var type = this.GetType();
#if cAlgo
            if (type.GetTypeInfo().GetCustomAttribute<IndicatorAttribute>() != null)
            {
                type = type.BaseType;
            }
#endif
            foreach (var mi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(_ => _.PropertyType == typeof(IndicatorDataSeries)))
            {
                try
                {
                    if (mi.GetValue(this) == null)
                    {
                        mi.SetValue(this, new CustomIndicatorDataSeries());
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to initialize IndicatorDataSeries with CustomIndicatorDataSeries", ex);
                }
            }
        }

        //        public
        //#if !cAlgo
        //            async
        //#endif
        //            Task Start()
        //        {
        //#if !cAlgo
        //            await base.Start();
        //            await EnsureDataAvailable(Account.ExtrapolatedServerTime);
        //#else
        //            return Task.CompletedTask;
        //#endif
        //        }

        protected
#if cAlgo
         virtual
#else
         async override
#endif
        Task OnStarting()
        {
#if !cAlgo
            Init();
            await base.OnStarting().ConfigureAwait(false);
#else
            return Task.CompletedTask;
#endif

            //l = this.GetLogger(this.ToString().Replace(' ', '.'), Config.Log);
            //if (l != null)
            //{
            //    l.LogInformation($"------- START {this} -------");
            //}
            //OnInitializing();
        }

        public virtual void InitLog()
        {
            try
            {
                l = this.GetLogger(this.ToString().Replace(' ', '.'), Template == null ? false : Template.Log);

                l.LogInformation($"....... START {this.ToStringDescription()} .......");
            }
            catch (TypeInitializationException tie)
            {
                throw tie.InnerException.InnerException;
            }
        }

#endregion

        //protected SortedList<KeyValuePair<DateTime, TimeSpan>, int> indexOffsets = new SortedList<KeyValuePair<DateTime, TimeSpan>, int>();

#if cAlgo

        public override void Calculate(int index)
        {
            CalculateIndex(index).Wait(); // REVIEW Wait
        }

#endif

        public abstract Task CalculateIndex(int index);
        


        public  async Task CalculateToTime(DateTime date)
        {

#if cAlgo
            var series = Bot == null ? MarketSeries : Bot.MarketSeries;
            if (MarketSeries == null && Bot == null)
            {
                throw new ArgumentNullException("MarketSeries == null && Bot == null");
            }
#else
            //var series = MarketSeries;
#endif
#if NULLCHECKS
            if (series == null)
            {
                throw new ArgumentNullException("MarketSeries");
            }
#endif

            //l.Debug("Calculating until " + date);

#if cAlgo
            var startIndex = 0;
#else
            var startIndex = series.FindIndex(date)-2;
            if (startIndex == -1) { startIndex = 0;
                Debug.WriteLine("TODO FIXME: IndicatorBase.CalculateToTime did not find seriesIndex for date: " + date);
            }
#endif
            startIndex = Math.Max(LastIndex, startIndex);

            for (int index = startIndex; series.OpenTime[index] < date; index++)
            {
#if cAlgo
                if (index >= series.OpenTime.Count) break;
#else
                if (index >= series.LastIndex)
                {
                    if ((date-series.OpenTime[index] ) > TimeSpan.FromMinutes(1))
                    {
                        Debug.WriteLine($"Indicator stopping due to lack of data: (series.OpenTime[index] - date) is {(series.OpenTime[index] - date)} for date {date} and last series time: {series.OpenTime[index]}");
                    }
                    break;
                }
#endif
                var openTime = series.OpenTime[index];
                //l.Warn($"series.OpenTime[index] {openTime} open: {series.Open[index]}");
                if (double.IsNaN(series.Open[index])) continue;
                await CalculateIndex(index).ConfigureAwait(false);
            }
            //l.Info("Calculated until " + date + " " + OpenLongPoints.LastValue);
        }

#region Misc

        public virtual string ToStringDescription()
        {
            return this.ToString();
        }

        protected Microsoft.Extensions.Logging.ILogger l;

#endregion


    }
}
