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

namespace LionFire.Trading.Indicators
{

    public abstract partial class IndicatorBase<TConfig> : IIndicator
        where TConfig : IIndicatorConfig, new()
    {
        #region Construction and Init

        public IndicatorBase() { }

        public IndicatorBase(TConfig config)
        {
            this.Config = config;
        }

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
            catch (Exception)
            {
                throw new Exception("InitLog threw");
            }
            try
            {
                _InitPartial();
            }
            catch (Exception)
            {
                throw new Exception("_InitPartial threw");
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
                l = this.GetLogger(this.ToString().Replace(' ', '.'), Config == null ? false : Config.Log);

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




        #region Configuration

        #region Config

        public TConfig Config {
            get { return config; }
            set { config = value; OnConfigChanged(); }
        }
        private TConfig config = new TConfig();

        protected virtual void OnConfigChanged()
        {
        }

        #endregion


        IIndicatorConfig IIndicator.Config { get { return this.Config; } set { this.Config = (TConfig)value; } }

        #endregion



        #region Misc

        public virtual string ToStringDescription()
        {
            return this.ToString();
        }

        protected Microsoft.Extensions.Logging.ILogger l;

        #endregion


    }
}
