using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading.Journal;

public interface ITradeJournal<TPrecision>
      where TPrecision : struct, INumber<TPrecision>
{
    ValueTask Write(JournalEntry<TPrecision> entry);
    ValueTask Close(string context);
    ValueTask CloseAll();
}
