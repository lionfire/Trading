using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IValuesResult<out T>
{
    IEnumerable<T>? Values { get; }

    bool IsSuccess { get { return Values != null; } }

    string? FailReason => null;
}

