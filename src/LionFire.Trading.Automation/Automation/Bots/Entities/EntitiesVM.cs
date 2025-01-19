using DynamicData;
using LionFire.Data;
using LionFire.Data.Async.Gets;
using LionFire.Data.Collections;
using LionFire.Mvvm;
using LionFire.Reactive.Reader;
using LionFire.Reactive.Writer;
using LionFire.Structures;
using MorseCode.ITask;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class EntitiesVM<TKey, TValue, TValueVM> : AsyncKeyedCollection<TKey, TValueVM>
    where TKey : notnull
    where TValue : notnull
    where TValueVM : notnull, IKeyed<TKey>, IViewModel<TValue>
{
    public IObservableReader<TKey, TValue> Reader { get; }
    public IObservableWriter<TKey, TValue> Writer { get; }

    public Func<TKey, TValue, TValueVM> Factory { get; set; } = DefaultFactory;
    static Func<TKey, TValue, TValueVM> DefaultFactory = (k, v) => (TValueVM)Activator.CreateInstance(typeof(TValueVM), k, v)!;

    public EntitiesVM(IObservableReader<TKey, TValue> reader, IObservableWriter<TKey, TValue> writer)
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

    #region State

    public IObservableCache<TValueVM, TKey> Items { get; }

    #endregion

    #region AsyncKeyedCollection Implementation 

    public override async ValueTask Add(TValueVM value) => await Writer.Write(GetKey(value), GetEntity(value));
    public override async ValueTask Upsert(TValueVM value) => await Writer.Write(GetKey(value), GetEntity(value));
    public override async ValueTask<bool> Remove(TKey key) => await Writer.Remove(key);

    protected override ITask<IGetResult<IEnumerable<TValueVM>>> GetImpl(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IGetResult<IEnumerable<TValueVM>>)GetResult<IEnumerable<TValueVM>>.Success(Items.Items, TransferResultFlags.Noop | TransferResultFlags.RanSynchronously)).AsITask();
    }

    #endregion

    #region Accessors

    virtual protected TKey GetKey(TValueVM valueVM) => valueVM.Key;

    virtual protected TValue GetEntity(TValueVM valueVM) => valueVM.Value!;
    
    #endregion

}


