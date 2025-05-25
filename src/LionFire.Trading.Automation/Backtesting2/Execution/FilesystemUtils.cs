using LionFire.ExtensionMethods;

namespace LionFire.Trading.Automation;

public static class FilesystemUtils
{
    public static (string dir, string id) GetUniqueDirectory(string baseDir, string prefix, string suffix, int zeroPadding = 0)

    {
        for (int i = 0; ; i++)
        {
            var name = $"{prefix}{i.ToString("D" + zeroPadding)}{suffix}";
            var dir = Path.Combine(baseDir, name);
            if (!Directory.Exists(dir) && (!Directory.Exists(baseDir) || !Directory.GetFiles(baseDir, name + ".*").Any()))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch { } // EMPTYCATCH
                return (dir, name);
            }
        }
    }
}
