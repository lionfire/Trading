using LionFire.Reactive.Persistence;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive.Linq;

namespace LionFire.Mvvm;

public partial class ObservableReaderWriterVM<TKey, TValue, TValueVM> : ObservableReaderVM<TKey, TValue, TValueVM>
    where TKey : notnull
    where TValue : notnull
{
    public IObservableWriter<TKey, TValue> Writer { get; }

    public ObservableReaderWriterVM(IObservableReader<TKey, TValue> reader, IObservableWriter<TKey, TValue> writer) : base(reader)
    {
        Writer = writer;
    }

    //public ValueTask Write()
    //{
    //    return Writer.Write(Id, Value);
    //}
}

public partial class ObservableReaderWriterItemVM<TKey, TValue, TValueVM> : ObservableReaderItemVM<TKey, TValue, TValueVM>
    where TKey : notnull
    where TValue : notnull
{
    public IObservableWriter<TKey, TValue> Writer { get; }

    public ObservableReaderWriterItemVM(IObservableReader<TKey, TValue> reader, IObservableWriter<TKey, TValue> writer) : base(reader)
    {
        Writer = writer;


    }
    IDisposable? rnpcSubscription = null;

    public ValueTask Write()
    {
        return Writer.Write(Id, Value);
    }
}


