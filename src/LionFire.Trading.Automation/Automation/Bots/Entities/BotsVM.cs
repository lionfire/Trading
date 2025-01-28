using DynamicData;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Automation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class EntitiesVMSlim<TKey, TValue, TValueVM> : ReactiveObject
    where TKey : notnull
    where TValue : notnull
    where TValueVM : notnull
{
    public IObservableReaderWriter<TKey, TValue> ReaderWriter { get; }
    public IObservableReader<TKey, TValue> Reader { get; }

    public Func<TKey, TValue, TValueVM> Factory { get; set; } = DefaultFactory;
    static Func<TKey, TValue, TValueVM> DefaultFactory = (k, v) => (TValueVM)Activator.CreateInstance(typeof(TValueVM), k, v)!;

    public EntitiesVMSlim(IObservableReader<TKey, TValue> reader)
    {
        this.Reader = reader;
        this.ReaderWriter = reader as IObservableReaderWriter<TKey, TValue>;

        Items = Reader.ObservableCache.Connect()
            .Transform((e, key) => Factory(key, e))
            .AsObservableCache();

        //Items = Observable.Create<IChangeSet<TValueVM, TKey>>(observer =>
        //{
        //    var cache = new SourceCache<TValueVM, TKey>(x => x.Key);
        //    var subscription = Reader.Items.Connect().Transform(x => (TValueVM)Activator.CreateInstance(typeof(TValueVM), x)).Subscribe(cache.AddOrUpdate);
        //    return cache.Connect().Subscribe(observer.OnNext);
        //});

    }

    public IObservableCache<TValueVM, TKey> Items { get; }
}

public class BotsVM : EntitiesVMSlim<string, BotEntity, BotVM>
{

    public BotsVM(IObservableReaderWriter<string, BotEntity> reader) : base(reader )
    {
    }
}


