﻿#if UNUSED
using LionFire.Trading.HistoricalData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LionFire.Trading.Historical.Staging;

public class InputStaging<TInput1, TInput2>
{
    public InputStaging(DateChunker chunker,
        IReadOnlyList<IInputSignal> inputs)
    {

    }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    //Channel<(TInput1, TInput2)> OutputChannel { get; } = new Channel<TInput1>();

    //public async Task Run()
    //{

    //}
}

#endif