using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

// TODO: Reconcile with IHistoricalDataResult
public interface IValuesResult<out T>
{
    IReadOnlyList<T>? Values { get; }

    bool IsSuccess => Values != null;

    string? FailReason => null;
}

