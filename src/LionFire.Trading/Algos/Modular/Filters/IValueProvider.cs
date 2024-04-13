#if OLD // TOTRIAGE
//#nullable enable

namespace LionFire.Trading.Algos.Modular.Filters;

public interface IValueProvider<T>
{
    BacktestRunner? Context { get; set; }

    /// <summary>
    /// Same as this[0]
    /// </summary>
    T CurrentValue => this[0];

    T this[int barsAgo] { get; }
}


#endif