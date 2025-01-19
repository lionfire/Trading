using DynamicData;
using LionFire.Reactive.Reader;
using LionFire.Reactive.Writer;
using LionFire.Trading.Automation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class EntitiesVMSlim<TKey, TValue, TValueVM> : ReactiveObject
    where TKey : notnull
    where TValue : notnull
    where TValueVM : notnull
{
    public IObservableReader<TKey, TValue> Reader { get; }
    public IObservableWriter<TKey, TValue> Writer { get; }

    public Func<TKey, TValue, TValueVM> Factory { get; set; } = DefaultFactory;
    static Func<TKey, TValue, TValueVM> DefaultFactory = (k, v) => (TValueVM)Activator.CreateInstance(typeof(TValueVM), k, v)!;

    public EntitiesVMSlim(IObservableReader<TKey, TValue> reader, IObservableWriter<TKey, TValue> writer)
    {
        this.Reader = reader;
        this.Writer = writer;

        Items = Reader.Items.Connect()
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

    public BotsVM(IObservableReader<string, BotEntity> reader, IObservableWriter<string, BotEntity> writer) : base(reader, writer)
    {
    }
}


