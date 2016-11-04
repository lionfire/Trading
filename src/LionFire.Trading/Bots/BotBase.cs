using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Structures;
using LionFire.Execution;
#if cAlgo
using cAlgo.API;
#endif
using LionFire.Templating;

namespace LionFire.Trading.Bots
{
    // TODO: Rename BotBase to SingleSeriesBotBase  and make a new BotBase that is more generic

    public partial class BotBase<_TBot> : IBot, IInitializable
        where _TBot : TBot, new()
    {
        public string Version { get; set; } = "0.0.0";

        #region Configuration

        object ITemplateInstance.Template { get { return Template; } set { this.Template = (_TBot)value; } }

        TBot IBot.Template { get { return Template; } set { Template = (_TBot)value; } }

        public _TBot Template { get; set; } = new _TBot();

        public LosingTradeLimiterConfig LosingTradeLimiterConfig { get; set; } = new LosingTradeLimiterConfig();

        #endregion

        public Task<bool> Initialize()
        {
            logger = this.GetLogger(this.ToString().Replace(' ', '.'), Template.Log);
            return Task.FromResult(true);
        }

#if cAlgo
        protected virtual void OnStarting() // This is the main initialization point for cAlgo
#else
        protected override void OnStarting()
#endif
        {
#if cAlgo
            Initialize().Wait();
#endif

            logger.LogInformation($"------- START {this} -------");
        }
        partial void OnStarting_();

        protected virtual void OnStopping()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }

        protected virtual void OnNewBar()
        {
        }

        #region Derived

        public bool CanOpenLong
        {
            get
            {
                var count = Positions.Where(p => p.TradeType == TradeType.Buy).Count();
                return count < Template.MaxLongPositions;
            }
        }
        public bool CanOpenShort
        {
            get
            {
                var count = Positions.Where(p => p.TradeType == TradeType.Sell).Count();
                return count < Template.MaxShortPositions;
            }
        }
        public bool CanOpen
        {
            get
            {
                var count = Positions.Count;
                return Template.MaxOpenPositions == 0 || count < Template.MaxOpenPositions;
            }
        }

        public bool CanOpenType(TradeType tradeType)
        {
            if (!CanOpen) return false;
            switch (tradeType)
            {
                case TradeType.Buy:
                    return CanOpenLong;
                case TradeType.Sell:
                    return CanOpenShort;
                default:
                    return false;
            }
        }

        #endregion

        #region Misc

        public virtual string Label
        {
            get
            {
                return label ?? this.GetType().Name;
            }
            set
            {
                label = value;
            }
        }
        private string label = null;

        //public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

#if cAlgo
        public Microsoft.Extensions.Logging.ILogger Logger
        {
            get { return logger; }
        }
        protected Microsoft.Extensions.Logging.ILogger logger { get; set; }
#endif

        #endregion
    }


}
