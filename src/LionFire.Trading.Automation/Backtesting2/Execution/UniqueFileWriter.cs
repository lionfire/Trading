namespace LionFire.Trading.Automation;

public class UniqueFileWriter
{
    // Based on: https://stackoverflow.com/questions/9545619/a-fast-hash-function-for-string-in-c-sharp
    static UInt64 CalculateHash(string read)
    {
        UInt64 hashedValue = 3074457345618258791ul;
        for (int i = 0; i < read.Length; i++)
        {
            hashedValue += read[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    UInt64 lastHash = 0;
    int counter = -1;

    private readonly object _lock = new();

    public string TemplatedPath { get; }

    public UniqueFileWriter(string templatedPath)
    {
        this.TemplatedPath = templatedPath;
    }
    public Task SaveIfDifferent(string contents)
    {
        lock (_lock)
        {
            counter++; // Always increment counter (ENH: make configurable)
            var newHash = CalculateHash(contents);
            if (newHash == lastHash) return Task.CompletedTask;
            lastHash = newHash;
        }

        var path = TemplatedPath.Replace("{0}", counter.ToString());
        if (path == TemplatedPath)
        {
            var dir = Path.GetDirectoryName(TemplatedPath) ?? throw new ArgumentException($"{nameof(TemplatedPath)} is unknown directory");
            if (counter == 0)
            {
                path = Path.Combine(dir, Path.GetFileNameWithoutExtension(path) + Path.GetExtension(path));
            }
            else
            {
                path = Path.Combine(dir, Path.GetFileNameWithoutExtension(path) + " (" + (counter + 1) + ")" + Path.GetExtension(path));
            }
        }

        if (System.IO.File.Exists(path))
        {
            var existing = System.IO.File.ReadAllText(path);
            if (existing == contents) return Task.CompletedTask;
            else throw new AlreadySetException();
        }
        return System.IO.File.WriteAllTextAsync(path, contents);
    }
}
