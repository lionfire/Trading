using LionFire.Trading.HistoricalData.Persistence;
using System;

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

    public KlineArrayFile(string path, SymbolBarsRange reference)
    {
        KlineArrayInfo info = new KlineArrayInfo { SymbolBarsRange = reference };
        DownloadingPath = path ?? throw new ArgumentNullException(nameof(path));
        var chunks = path.Split(KlineArrayFileConstants.DownloadingFileExtension, StringSplitOptions.None);
        if (chunks.Length > 2) { throw new ArgumentException($"Not supported: more than one '{KlineArrayFileConstants.DownloadingFileExtension}' in the path"); }
        CompletePath = path.Replace(KlineArrayFileConstants.DownloadingFileExtension, "");
        PartialPath = path.Replace(KlineArrayFileConstants.DownloadingFileExtension, KlineArrayFileConstants.PartialFileExtension);

        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

        //int buffer = 256 * 1024;
        //bool useAsync = false;
        //FileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, buffer, useAsync);
        FileStream = File.Open(path, FileMode.Create);
        Info = info;
    }

    public void MoveDownloadingToComplete()
    {
        DeleteExistingPartialFile();
        DeleteExistingCompleteFile();
        File.Move(DownloadingPath, CompletePath);
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
