using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Templating;
using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using LionFire.Trading;

namespace LionFire.Trading
{
    public abstract class TMarket : IHierarchicalTemplate
    //, ITemplate<LiveMarket>
    {
        public string ConfigName { get; set; }
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
            MarketData = new MarketData() { Market = (IMarket)this }; // ASSERT: this must implement IMarket
            logger = this.GetLogger();
        }

        #region Symbols

        protected ConcurrentDictionary<string, Symbol> symbols = new ConcurrentDictionary<string, Symbol>();

        public Symbol GetSymbol(string symbolCode)
        {
            return symbols.GetOrAdd(symbolCode, code => CreateSymbol(code));

        }
        protected abstract Symbol CreateSymbol(string symbolCode);

        #endregion

        #region Series

        protected ConcurrentDictionary<KeyValuePair<string, string>, MarketSeries> marketSeries = new ConcurrentDictionary<KeyValuePair<string, string>, MarketSeries>();
        public abstract MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);

        #endregion



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

        #region MarketSeries

        public IMarketSeries GetMarketSeries(string symbol, TimeFrame tf)
        {
            return GetSymbol(symbol).GetMarketSeries(tf);
        }

        #endregion

        #region Symbol Subscriptions

        /// <summary>
        /// Subscribe to a symbol at a particular timeframe.  Failure to subscribe will result in a NotSubscribedException
        /// </summary>
        /// <param name="symbolCode"></param>
        /// <param name="timeFrame"></param>
        /// <returns></returns>
        public IDisposable Subscribe(string symbolCode, TimeFrame timeFrame)
        {
            var key = symbolCode + ";" + timeFrame.Name;
            var newValue = subscriptions.AddOrUpdate(key, 1, (_, val) => val + 1);
            OnSubscriptionChanged(key, newValue);
            return new SubscriptionDecrementer(symbolCode, this);
        }

        protected virtual void Subscribe(string symbolCode, string timeFrame)
        {
        }
        protected virtual void Unsubscribe(string symbolCode, string timeFrame)
        {
        }

        #region Private

        ConcurrentDictionary<string, int> subscriptions = new ConcurrentDictionary<string, int>();
        private object subscriptionsLock = new object();

        private void OnSubscriptionChanged(string key, int newValue)
        {
            var split = key.Split(';');
            if (newValue == 1)
            {
                Subscribe(split[0], split[1]);
            }
            else if (newValue == 0)
            {
                Unsubscribe(split[0], split[1]);
            }
        }

        private class SubscriptionDecrementer : IDisposable
        {
            MarketBase<TTemplate> market;
            private ConcurrentDictionary<string, int> Dict { get { return market.subscriptions; } }
            private string Key;
            public SubscriptionDecrementer(string key, MarketBase<TTemplate> market)
            {
                this.Key = key;
                this.market = market;
            }
            public void Dispose()
            {
                var newValue = Dict.AddOrUpdate(Key, 0, (key, val) => Math.Max(0, val - 1));
                this.market.OnSubscriptionChanged(Key, newValue);
            }
        }

        #endregion

        #endregion

        #region Attached MarketParticipants

        void IHierarchicalTemplateInstance.Add(object child)
        { Add((IMarketParticipant)child); }

        public void Add(IMarketParticipant actor)
        {
            participants.Add(actor);
            actor.Market = (IMarket)this;
        }
        public IReadOnlyList<IMarketParticipant> Participants { get { return participants; } }
        List<IMarketParticipant> participants = new List<IMarketParticipant>();

        #endregion

        #region Events

        public event Action Ticked;
        protected void RaiseTicked() { Ticked?.Invoke(); }
        #endregion

        #region Misc

        public ILogger Logger { get { return logger; } }
        protected ILogger logger;

        #endregion
    }



}
