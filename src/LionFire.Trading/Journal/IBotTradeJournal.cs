using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading.Journal;

/// <summary>
/// Used both for backtesting, and live trading (to record actions taken, and potentially any expected results.)
/// One per bot.
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
public interface IBotTradeJournal<TPrecision> : IAsyncDisposable
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
