using LionFire.Trading;
using LionFire.Trading.HistoricalData.Retrieval;
using System.ComponentModel;

namespace LionFire.Trading.HistoricalData;

public class KlineArrayInfo
{

    public string Exchange { get; set; }
    public string ExchangeArea { get; set; }
    public string Symbol { get; set; }
    public string TimeFrame { get; set; }

    public DateTime Start { get; set; }
    public DateTime EndExclusive { get; set; }

    /// <summary>
    /// If squashed from multiple files, uses oldest time
    /// </summary>
    public DateTime RetrieveTime { get; set; }

    public bool IncludesOpenTime { get; set; }
    public FieldSet FieldSet { get; set; }
    public string NumericType { get; set; }

    public string Compression { get; set; }

    #region Derived

    [DefaultValue(-1)]
    public long Bars { get; set; }

    public DateTime FirstOpenTime { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Close { get; set; }
    public DateTime LastOpenTime { get; set; }


    public List<(DateTime, DateTime)> Gaps { get; set; }

    public bool MissingBarsIncluded { get; set; }

    #region Derived

    // Derived, but send it to serialized file to be nicer to whoever is reading it
    public bool IsComplete { get; set; } // => noGaps && !info.MissingBarsOnlyAtStart && !info.MissingBarsOnlyAtEnd;

    #endregion

    public bool MissingBarsOnlyAtEnd { get; set; }
    public bool MissingBarsOnlyAtStart { get; set; }

    #endregion

}

