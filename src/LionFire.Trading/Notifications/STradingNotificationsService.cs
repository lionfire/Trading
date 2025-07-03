using LionFire.DependencyInjection;
using LionFire.Execution;
using LionFire.Trading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Instantiating;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Assets;
using LionFire.Serialization;
using System;
using System.Collections.Specialized;
using LionFire.Structures;
using System.IO;
using LionFire.Persistence;
using System.Threading;
using LionFire.Dependencies;

namespace LionFire.Notifications.Wpf.App
{

    public class STradingNotificationsService : IStartable, ITemplateInstance<TradingNotificationsService>
    {
        public TradingNotificationsService Template { get; set; }

        private List<IFeed_Old> activeFeeds = new List<IFeed_Old>();

        public void AddFeed(IFeed_Old feed)
        {
            foreach (var alertRequest in Template.AlertRequests)
            {
                alertRequest.Attach(feed);
            }
        }

        public void RemoveFeed(IFeed_Old feed)
        {
            foreach (var alertRequest in Template.AlertRequests)
            {
                alertRequest.Detach(feed);
            }
        }

        Dictionary<string, IFeed_Old> feeds = new Dictionary<string, IFeed_Old>();
        Dictionary<string, IAccount_Old> accounts = new Dictionary<string, IAccount_Old>();

        public IFeed_Old DefaultFeed => feeds.Values.FirstOrDefault() ?? accounts.Values.FirstOrDefault();

        //FsObjectCollection<PriceNotifier> alerts; // TODO

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            foreach (var accountName in Template.AccountNames)
            {
                foreach (var feed in DependencyContext.Current.GetServices<IFeed_Old>().Concat(DependencyContext.Current.GetServices<IAccount_Old>()))
                {

                    // FUTURE: Match wildcard
                    if (Template.AccountNames != null && !Template.AccountNames.Contains(feed.Template.Key)) continue;

                    if (!feeds.ContainsKey(feed.Template.Key))
                    {
                        feeds.Add(feed.Template.Key, feed);
                    }
                    if (feed is IAccount_Old acc && !accounts.ContainsKey(feed.Template.Key))
                    {
                        accounts.Add(feed.Template.Key, acc);
                    }

                    if (feed is IStartable startable)
                    {
                        startable.StartAsync();
                    }
                }
            }

            //alerts = new FsObjectCollection<PriceNotifier>() // TODO
            //{

            //    RootPath = Path.Combine(LionFireEnvironment.Directories.GetProgramDataDir("Trading"), "Alerts")
            //};

            //alerts.Handles.Removed += (k, v) => { // TODO
            //    if (v.HasObject) {
            //        v.Object.Detach();
            //    }
            //};


            //foreach (var alert in AssetProviderExtensions.Find<TPriceAlert>())
            throw new NotImplementedException("TODO: FsObjectCollection alerts");
            //foreach (var alert in alerts.Handles) // TODO
            //{
            //    var priceAlert = alert.Value.Object;
            //    priceAlert.Attach(DefaultFeed);
            //    //await (priceAlert as IStartable)?.Start();
            //    priceAlerts.Add(priceAlert);

            //}

            return Task.CompletedTask;
        }

        private void OnObjectReset(IReadHandle<PriceNotifier> handle)
        {

        }

        public Task Stop()
        {
#if OLD
            foreach (var priceAlert in priceAlerts)
            {
                priceAlert.Detach();
            }
#endif
            return Task.CompletedTask;
        }

        List<PriceNotifier> priceAlerts = new List<PriceNotifier>();

    }

    //public interface 

    public class ObservableHandleDictionary<TKey, T>
        where T : IKeyed<TKey>
    {
        
    }
    //public class DictionaryBinder<TKey,TValue>
    //{
    //    public CollectionBinder(INotifyCollectionChanged incc, ICollection<T> currentCollection)
    //    {
    //        this.incc = incc;
    //        incc.CollectionChanged += OnCollectionChanged;
    //    }

    //    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //    {
            
    //    }

    //    public INotifyCollectionChanged incc;
    //    private ICollection<T> 
    //    public Action<T> OnAttach { get; set; }
    //    public Action<T> OnDetach { get; set; }
    //}

    public static class ITemplateInstanceExtensions
    {
        // TODO: Eliminate ITemplate.Template requirement, use reflection instead to populate
    }
}
