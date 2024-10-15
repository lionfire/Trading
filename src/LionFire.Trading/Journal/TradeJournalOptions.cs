﻿using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Journal;

public class TradeJournalOptions
{
    #region Directory

    public string? JournalDir { get; set; }

    public bool ReplaceOutput { get; set; } = true;

    #endregion

    public JournalFormat JournalFormat { get; set; } =
        //JournalFormat.Binary |
        JournalFormat.CSV;
    public char CsvSeparator { get; set; } = ',';


    #region Batch

    public JournalFormat BatchInfoFormat { get; set; } = JournalFormat.Hjson;

    #endregion

    public LogLevel LogLevel { get; set; } = LogLevel.None;

    #region Keeping Details

    public double DiscardDetailsWhenFitnessBelow { get; set; } = 1.0;
    public bool DiscardDetailsWhenAborted { get; set; } // FUTURE = true;
    public int KeepDetailsForTopNResultsIncludingAborted { get; set; } = 10;
    public int KeepDetailsForTopNResults { get; set; } = 100;

    //public long MaxDiskSpaceForDetails { get; set; } = 20 * 1024 * 1024; // ENH

    #endregion

    public TradeJournalOptions Clone() => (TradeJournalOptions)this.MemberwiseClone();
}