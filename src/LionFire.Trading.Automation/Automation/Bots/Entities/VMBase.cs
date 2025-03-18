using LionFire.Reactive.Persistence;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace LionFire.Mvvm;

public abstract class VMBase<TKey, TValue> : ReactiveObject
    where TKey : notnull
    where TValue : notnull
{
    public abstract TKey Key { get; }

    public abstract TValue Value { get; }

    //public IServiceProvider ServiceProvider { get; }

    //IObservableReader<TKey, TValue>? observableReader;

    //public VMBase(IServiceProvider serviceProvider)
    //{
    //    ServiceProvider = serviceProvider;
    //    observableReader = serviceProvider.GetService<IObservableReader<TKey, TValue>>();
    //}
}

