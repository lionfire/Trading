using LionFire.Reactive.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Automation;

public class ObservableEntitiesProvider<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public ObservableEntitiesProvider(IServiceProvider serviceProvider)
    {
        ReaderWriter = serviceProvider.GetService<IObservableReaderWriter<TKey, TValue>>();
        Reader ??= serviceProvider.GetRequiredService<IObservableReader<TKey, TValue>>();
    }

    public IObservableReaderWriter<TKey, TValue>? ReaderWriter { get; }

    public IObservableReader<TKey, TValue>? Reader { get; }
}

