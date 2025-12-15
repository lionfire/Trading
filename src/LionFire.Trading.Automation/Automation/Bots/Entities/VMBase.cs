using LionFire.Reactive.Persistence;
using LionFire.Structures;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace LionFire.Mvvm;

public abstract partial class VMBase<TKey, TValue> : ReactiveObject, IKeyed<TKey>
    where TKey : notnull
    where TValue : class
{
    public abstract TKey Key { get; }

    public abstract TValue? Value { get; set; }

    /// <summary>
    /// Error message if the value failed to load (e.g., file not found, corrupt data).
    /// Null if no error occurred.
    /// </summary>
    [Reactive]
    private string? _loadError;

    /// <summary>
    /// True if there was an error loading the value.
    /// </summary>
    public bool HasLoadError => LoadError != null;

    //public IServiceProvider ServiceProvider { get; }

    //IObservableReader<TKey, TValue>? observableReader;

    //public VMBase(IServiceProvider serviceProvider)
    //{
    //    ServiceProvider = serviceProvider;
    //    observableReader = serviceProvider.GetService<IObservableReader<TKey, TValue>>();
    //}
}

