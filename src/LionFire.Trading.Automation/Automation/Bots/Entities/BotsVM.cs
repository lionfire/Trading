using DynamicData;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Automation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class EntitiesVMSlim<TKey, TValue, TValueVM> : ReactiveObject
    where TKey : notnull
    where TValue : notnull
    where TValueVM : notnull
{
    #region Parameters

    public Func<TKey, TValue, TValueVM> Factory { get; set; } = DefaultFactory;
    static Func<TKey, TValue, TValueVM> DefaultFactory = (k, v) => (TValueVM)Activator.CreateInstance(typeof(TValueVM), k, v)!;

    #endregion

    #region Lifecycle

    public EntitiesVMSlim()
    {
    }

    #endregion

    #region Reader

    /// <summary>
    /// Set via Reader
    /// </summary>
    public IObservableReaderWriter<TKey, TValue>? ReaderWriter { get; private set; }
    public IObservableReader<TKey, TValue>? Reader
    {
        get => reader;
        set
        {
            if (reader == value) return;
            readerDisposables?.Dispose();
            reader = value;
            readerDisposables = new();

            if (reader != null)
            {
                ReaderWriter = reader as IObservableReaderWriter<TKey, TValue>;

                Items = reader.ObservableCache.Connect()
                    .Transform((e, key) => Factory(key, e))
                    .AsObservableCache()
                    .DisposeWith(readerDisposables);
                //Items = Observable.Create<IChangeSet<TValueVM, TKey>>(observer =>
                //{
                //    var cache = new SourceCache<TValueVM, TKey>(x => x.Key);
                //    var subscription = Reader.Items.Connect().Transform(x => (TValueVM)Activator.CreateInstance(typeof(TValueVM), x)).Subscribe(cache.AddOrUpdate);
                //    return cache.Connect().Subscribe(observer.OnNext);
                //});
            }
        }
    }
    private IObservableReader<TKey, TValue>? reader;

    CompositeDisposable readerDisposables = new();

    #endregion

    /// <summary>
    /// Set via Reader
    /// </summary>
    public IObservableCache<TValueVM, TKey>? Items { get; private set; }
}

public class BotsVM : EntitiesVMSlim<string, BotEntity, BotVM>
{

    public BotsVM()
    {
    }
}


