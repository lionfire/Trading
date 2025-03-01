using LionFire.Journaling;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationRunNotes
{
    public string? Notes { get; set; }
    public List<JournalEntry>? JournalEntries { get; set; }
}
