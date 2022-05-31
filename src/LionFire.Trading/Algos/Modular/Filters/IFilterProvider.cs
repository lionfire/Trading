#nullable enable

using LionFire;
using LionFire.Trading.Backtesting2;

namespace LionFire.Trading.Algos.Modular.Filters
{
    public interface IFilterProvider
    {
    }
    public interface IValueProvider<T>
    {
        BacktestRunner? Context { get; set; }

        /// <summary>
        /// Same as this[0]
        /// </summary>
        T CurrentValue => this[0];

        T this[int barsAgo] { get; }
    }

    

}
