namespace LionFire.Trading.Journal;

public enum JournalFormat
{
    Unspecified = 0,
    Binary = 1 << 0,
    CSV = 1 << 1,
    Text = 1 << 2,
    Json = 1 << 3,
    Hjson = 1 << 4,
}
