using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading.Journal;

// TODO: Eliminate this interface. Use the concrete class instead.  Revisit if I have a non-backtest Trade Journal.
public interface IBacktestTradeJournal<TPrecision> : IAsyncDisposable
      where TPrecision : struct, INumber<TPrecision>
{
    void Write(JournalEntry<TPrecision> entry);
    ValueTask Finish(double fitness);

    string? FileName { get; set; }
    ExchangeSymbol? ExchangeSymbol { get; set; }
    bool IsAborted { get; set; }
    bool DiscardDetails { get; set; }

    TradeJournalOptions Options { get; }
    JournalStats JournalStats { get; }

    IEnumerable<JournalEntry<TPrecision>> MemoryEntries { get; }
}
