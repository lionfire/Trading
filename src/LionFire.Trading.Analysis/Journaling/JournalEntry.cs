using LionFire.Structures;

namespace LionFire.Journaling;

public class JournalEntry
{
    public DateTimeOffset CreationTime { get; set; }
    public DateTimeOffset LastModifiedTime { get; set; }

    public int Id { get; set; }
    public int RevisionNumber { get; set; }
    public string? ParentHash { get; set; }
    public string? Hash { get; set; }

    public string? Body { get; set; }

    public bool IsDeleted { get; set; }

    public FlagCollection? Flags { get; set; }
}


public class Journal
{
    public object Reference { get; set; }

    public List<JournalEntry> Entries { get; set; }

    public Dictionary<int, JournalEntry> Revisions { get; set; }
}

public class JournalPersister
{
    public JournalPersister()
    {

    }

}

