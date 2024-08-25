using LionFire.Execution;
using LionFire.Threading;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Indicators.Harnesses;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static LionFire.Trading.Automation.BacktestBatchTask2;

namespace LionFire.Trading.Automation;

public static class InputEnumeratorFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="series"></param>
    /// <param name="lookback"></param>
    /// <param name="inputEnumerator"></param>
    /// <returns>True if the inputEnumerator has been changed from null to an object, or from one object to another</returns>
    public static bool CreateOrGrow(Type type, Func<IHistoricalTimeSeries> series, int lookback, ref InputEnumeratorBase? inputEnumerator)
    {
        if (inputEnumerator != null)
        {
            if (lookback > 0)
            {
                if (inputEnumerator is IChunkingInputEnumerator chunking)
                {
                    chunking.GrowLookback(lookback);
                    return false;
                }
                //else if (inputEnumerator is IWindowedInputEnumerator windowed)
                //{
                //    windowed.GrowCapacityIfNeeded(lookback);
                //    return false;
                //}
                else
                {
                    inputEnumerator = null; // DESTRUCTIVE - only intended to be invoked at init time.
                    // Fall through to recreate.
                }
            }
            // else if lookback == 1, we're ok since existing inputEnumerator must already have at least 1.
        }
        throw new NotImplementedException("TPrecision");
        //inputEnumerator = lookback == 1
        //    ? (InputEnumeratorBase)Activator.CreateInstance(typeof(SingleValueInputEnumerator<,>).MakeGenericType(type, ______FIXME_______ ), series())!
        //    : (InputEnumeratorBase)Activator.CreateInstance(typeof(ChunkingInputEnumerator<,>).MakeGenericType(type, ______FIXME_______), series(), lookback)!;
        return true;
    }
}

