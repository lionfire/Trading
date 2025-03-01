using LionFire.IO;

namespace LionFire.Journaling;

public class JournalService
{
    public IVirtualFilesystem Filesystem { get; }


    public JournalService(IVirtualFilesystem filesystem)
    {
        Filesystem = filesystem;
    }

}

