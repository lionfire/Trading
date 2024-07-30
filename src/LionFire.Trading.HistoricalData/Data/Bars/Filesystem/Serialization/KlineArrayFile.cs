using LionFire.Trading.HistoricalData.Persistence;
using System;
using System.IO;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineArrayFile : IDisposable
{


    public FileStream FileStream { get; protected set; }
    public KlineArrayInfo Info { get; }
    //public SymbolBarsRange BarsRangeReference => Info.BarsRangeReference;


    public string DownloadingPath { get; set; }
    public string PartialPath { get; set; }
    public string CompletePath { get; set; }
    public bool IsComplete { get; set; }
    public bool ShouldSave { get; set; } = true;

    public static string CompletePathFromDownloadingPath(string downloadingPath) => downloadingPath.Replace(KlineArrayFileConstants.DownloadingFileExtension, "");

    public KlineArrayFile(string path, SymbolBarsRange reference, FileStream fileStream)
    {
        KlineArrayInfo info = new KlineArrayInfo { SymbolBarsRange = reference };
        DownloadingPath = path ?? throw new ArgumentNullException(nameof(path));
        var chunks = path.Split(KlineArrayFileConstants.DownloadingFileExtension, StringSplitOptions.None);
        if (chunks.Length > 2) { throw new ArgumentException($"Not supported: more than one '{KlineArrayFileConstants.DownloadingFileExtension}' in the path"); }
        CompletePath = CompletePathFromDownloadingPath(path);
        PartialPath = path.Replace(KlineArrayFileConstants.DownloadingFileExtension, KlineArrayFileConstants.PartialFileExtension);

        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

        //int buffer = 256 * 1024;
        //bool useAsync = false;
        //FileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, buffer, useAsync);
        //FileStream = File.Open(path, FileMode.Create);
        FileStream = fileStream;
        Info = info;
    }
    public static async ValueTask<KlineArrayFile?> Create(string path, SymbolBarsRange reference, bool truncateIfExists = false, CancellationToken cancellationToken = default)
    {
        FileStream? fileStream = await Task.Run(() =>
        {
            if (File.Exists(path)) return null;
            tryAgain:
            try
            {
                return File.Open(path, truncateIfExists ? FileMode.Create : FileMode.CreateNew);
            }
            catch (IOException ioex)
            {
                if (ioex.Message.EndsWith("it is being used by another process."))
                {
                    if (truncateIfExists && !cancellationToken.IsCancellationRequested) { goto tryAgain; }
                    else { return null; }

                }
                else { throw; }
            }
        });
        if (fileStream == null) return null;
        return new KlineArrayFile(path, reference, fileStream);
    }

    public void MoveDownloadingToComplete()
    {
        try
        {
            DeleteExistingPartialFile();
            DeleteExistingCompleteFile();
            File.Move(DownloadingPath, CompletePath);
        }
        finally
        {
            KlineArrayFileProvider.OnDownloadFinished(DownloadingPath);
        }
    }
    public void MoveDownloadingToPartial()
    {
        DeleteExistingPartialFile();
        DeleteExistingCompleteFile();
        File.Move(DownloadingPath, PartialPath);
    }

    public void DeleteExistingFiles()
    {
        DeleteExistingPartialFile();
        DeleteExistingCompleteFile();
        DeleteExistingIncompleteFile();
    }
    public void DeleteExistingCompleteFile()
    {
        if (File.Exists(CompletePath)) { File.Delete(CompletePath); }
    }
    public void DeleteExistingDownloadingFile()
    {
        if (File.Exists(DownloadingPath)) { File.Delete(DownloadingPath); }
    }
    public void DeleteExistingPartialFile()
    {
        if (File.Exists(PartialPath)) { File.Delete(PartialPath); }
    }
    public void DeleteExistingIncompleteFile()
    {
        if (File.Exists(DownloadingPath)) { File.Delete(DownloadingPath); }
    }

    public void Dispose()
    {
        FileStream?.Dispose();
        if (!ShouldSave)
        {
            DeleteExistingDownloadingFile();
        }
        else
        {
            if (IsComplete)
            {
                MoveDownloadingToComplete();
            }
            else
            {
                MoveDownloadingToPartial();
            }
        }
    }
}
