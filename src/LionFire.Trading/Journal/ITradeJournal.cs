using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading.Journal;

public interface ITradeJournal<TPrecision> : IAsyncDisposable
      where TPrecision : struct, INumber<TPrecision>
{
    void Write(JournalEntry<TPrecision> entry);
    ValueTask Close(string context);
    ValueTask CloseAll();

    string FileName { get; set; }
    ExchangeSymbol? ExchangeSymbol { get; set; }
    bool IsAborted { get; set; }
    bool DiscardDetails { get; set; }

    TradeJournalOptions Options { get; }

}
