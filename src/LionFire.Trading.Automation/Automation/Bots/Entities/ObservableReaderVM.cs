using LionFire.Reactive.Reader;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive.Linq;

namespace LionFire.Mvvm;

public partial class ObservableReaderVM<TKey, TValue, TValueVM> : ReactiveObject
    where TKey : notnull
    where TValue : notnull
{
    #region Parameters

    [Reactive]
    private TKey? _id;

    #endregion

    #region Lifecycle

    public ObservableReaderVM(IObservableReader<TKey, TValue> assets)
    {
        this.WhenAnyValue(x => x.Id).Subscribe(id =>
        {
            Value = default;
            IsLoading = true;
            if (id != null)
            {
                assets.Listen(id).Subscribe(v => Value = v);
            }
        });
    }

    #endregion

    #region State

    [Reactive]
    private TValue? _value;

    [Reactive]
    private bool _isLoading;

    //TValue? IReadWrapper<TValue>.Value => throw new NotImplementedException();

    #endregion

}

