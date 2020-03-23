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

namespace LionFire.Notifications.Wpf.App
{
    public class STradingNotificationsService : IStartable, ITemplateInstance<TradingNotificationsService>
    {
        public TradingNotificationsService Template { get; set; }

        private List<IFeed> activeFeeds = new List<IFeed>();

        public void AddFeed(IFeed feed)
        {
            foreach (var alertRequest in Template.AlertRequests)
            {
                alertRequest.Attach(feed);
            }
        }

        public void RemoveFeed(IFeed feed)
        {
            foreach (var alertRequest in Template.AlertRequests)
            {
                alertRequest.Detach(feed);
            }
        }

        Dictionary<string, IFeed> feeds = new Dictionary<string, IFeed>();
        Dictionary<string, IAccount> accounts = new Dictionary<string, IAccount>();

        public IFeed DefaultFeed => feeds.Values.FirstOrDefault() ?? accounts.Values.FirstOrDefault();

        //FsObjectCollection<PriceNotifier> alerts; // TODO

        public Task Start()
        {
            foreach (var accountName in Template.AccountNames)
            {
                foreach (var feed in DependencyContext.Current.GetServices<IFeed>().Concat(DependencyContext.Current.GetServices<IAccount>()))
                {

                    // FUTURE: Match wildcard
                    if (Template.AccountNames != null && !Template.AccountNames.Contains(feed.Template.Key)) continue;

                    if (!feeds.ContainsKey(feed.Template.Key))
                    {
                        feeds.Add(feed.Template.Key, feed);
                    }
                    if (feed is IAccount acc && !accounts.ContainsKey(feed.Template.Key))
                    {
                        accounts.Add(feed.Template.Key, acc);
                    }

                    if (feed is IStartable startable)
                    {
                        startable.Start();
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
            foreach (var priceAlert in priceAlerts)
            {
                priceAlert.Detach();
            }
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
