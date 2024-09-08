using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Journal;

public class TradeJournalOptions
{
    public JournalFormat JournalFormat { get; set; } =
        JournalFormat.Binary |
        JournalFormat.CSV;
    public char CsvSeparator { get; set; } = ',';

    public LogLevel LogLevel { get; set; } = LogLevel.None;

    //public string JournalDir { get; set; } = @"f:\TJ"; // TEMP HARDCODE HARDPATH
    public string JournalDir { get; set; } = @"z:\Trading\Journal"; // TEMP HARDCODE HARDPATH
}
