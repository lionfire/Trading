using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Templating;
using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading
{
    public class TMarket : IHierarchicalTemplate
    {
        public string BrokerName { get; set; }

        //public List<ITemplate> Participants { get; set; } TODO

        public int? BackFillMinutes { get; set; }

        public List<ITemplate> Children { get; set; }
    }

    public abstract class MarketBase<TTemplate> : IHierarchicalTemplateInstance
        where TTemplate : TMarket
    {
        object ITemplateInstance.Template { get { return Config; } set { Config = (TTemplate)value; } }
        public TTemplate Config { get; set; }

        public MarketDataProvider Data { get; private set; }

        public MarketData MarketData { get; set; }
        

        #region Relationships

        public Server Server {
            get; private set;
        } = new Server();


        #endregion

        public MarketBase()
        {
            Data = new MarketDataProvider((IMarket)this);
            logger = this.GetLogger();
        }

        public abstract MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);


        public virtual void Initialize()
        {
            //InitializeParticipants();
        }
        //protected virtual void InitializeParticipants()
        //{
        //    foreach (var participant in this.participants)
        //    {
        //        participant.Initialize();
        //    }
        //}

        #region Attached MarketParticipants

        void IHierarchicalTemplateInstance.Add(object child) { Add((IMarketParticipant)child); }

        public void Add(IMarketParticipant actor)
        {
            participants.Add(actor);
            actor.Market = (IMarket)this;
        }
        public IReadOnlyList<IMarketParticipant> Participants { get { return participants; } }
        List<IMarketParticipant> participants = new List<IMarketParticipant>();

        #endregion

        #region Misc

        protected ILogger logger;

        #endregion
    }
}
