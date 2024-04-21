using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Data;

public interface IObservableSeries<out T>
{
    ValueTask<IAsyncDisposable> Subscribe(IObserver<T> observer, SeriesSubscriptionOptions? options = null);
}

