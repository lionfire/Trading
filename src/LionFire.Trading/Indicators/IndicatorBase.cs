using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if cAlgo
using cAlgo.API;
#endif
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Templating;

namespace LionFire.Trading.Indicators
{

    public abstract partial class IndicatorBase<TIndicator> : IIndicator
        where TIndicator : ITIndicator, new()
    {
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


        //#region Identity

        //ITemplate ITemplateInstance Tempalte{get{}}

        //#endregion


        #region Construction and Init

        public IndicatorBase() { }

        public IndicatorBase(TIndicator config)
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

        public
#if !cAlgo
            async 
#endif
            Task Start()
        {
#if !cAlgo
            await EnsureDataAvailable(Account.ExtrapolatedServerTime);
#else
            return Task.CompletedTask;
#endif
        }

        protected
#if cAlgo
         virtual
#else
         override
#endif
        void OnStarting()
        {
#if !cAlgo
            Init();
            base.OnStarting();
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

        //public virtual void Calculate(int index)
        //{
        //}

        public virtual void CalculateToTime(DateTime openTime)
        {
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
