using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using LionFire.ExtensionMethods.Dumping;
using LionFire.Inspection.Nodes;
using LionFire.Parsing.String;
using LionFire.Threading;
using MemoryPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace LionFire.Trading.Journal;

public class JournalStatsCollector
{
    public readonly RollingAverage AverageMinutesPerWinningTrade = new();

    public JournalStats JournalStats
    {
        get
        {
            s.AverageMinutesPerWinningTrade = AverageMinutesPerWinningTrade.CurrentAverage;
            return s;
        }
    }
    private JournalStats s = new();

    internal void OnClose<TPrecision>(JournalEntry<TPrecision> entry) where TPrecision : struct, INumber<TPrecision>
    {
        if (entry.RealizedGrossProfitDelta > TPrecision.Zero)
        {
            if (entry.Position == null) { throw new ArgumentNullException(nameof(entry.Position)); }
            var positionDuration = entry.Time - entry.Position.EntryTime; /// REVIEW - move this somewhere?
            AverageMinutesPerWinningTrade.AddValue(positionDuration.TotalMinutes);

            s.WinningTrades++;
        }
        else if (entry.RealizedGrossProfitDelta == TPrecision.Zero)
        {
            s.BreakevenTrades++;
        }
        else if (entry.RealizedGrossProfitDelta <= TPrecision.Zero)
        {
            s.LosingTrades++;
        }
        else { s.UnknownTrades++; }
    }
}

public class JournalStats
{
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public int BreakevenTrades { get; set; }
    public int UnknownTrades { get; set; }
    public double AverageMinutesPerWinningTrade { get; set; }
}


public class RollingAverage
{
    private double sum = 0;
    private long count = 0;

    public void AddValue(double value)
    {
        sum += value;
        count++;
    }

    public double CurrentAverage => count > 0 ? (sum / count) : double.NaN;
}

public sealed class TradeJournal<TPrecision> : ITradeJournal<TPrecision>, IDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    #region Dependencies

    private readonly ILogger<TradeJournal<TPrecision>> logger;

    #endregion

    #region Options

    public TradeJournalOptions Options => options;
    private readonly TradeJournalOptions options;

    private string? pathWithoutExtension { get; set; }
    private void UpdatePath() => pathWithoutExtension = Path.Combine(Options.JournalDir ?? throw new ArgumentNullException(nameof(Options.JournalDir)), context);
    public string? FileName { get; set; }
    public ExchangeSymbol? ExchangeSymbol
    {
        get => exchangeSymbol;
        set
        {
            exchangeSymbol = value;
            //if (!Directory.Exists(JournalDirectory)) { Directory.CreateDirectory(JournalDirectory); } // BLOCKING IO
            UpdatePath();
        }
    }

    public string Context
    {
        get => context;
        set
        {
            context = value;
            UpdatePath();
        }
    }
    private string context = "Journal-" + Guid.NewGuid();

    #region Derived

    public string? JournalDirectory => Options?.JournalDir;
    //{
    //    get
    //    {
    //        var path = Options.JournalDir;
    //        if (Options.ExchangeSubDir)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol?.Exchange ?? "UnknownExchange");
    //        }
    //        if (Options.ExchangeAreaSubDir && ExchangeSymbol?.ExchangeArea != null)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol.ExchangeArea);
    //        }
    //        if (Options.SymbolSubDir)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol?.Symbol ?? "UnknownSymbol");
    //        }
    //        return path;
    //    }
    //}

    #endregion

    #endregion

    #region Lifecycle

    public TradeJournal(IServiceProvider serviceProvider, ILogger<TradeJournal<TPrecision>> logger, TradeJournalOptions options, ExchangeSymbol? exchangeSymbol = null)
    {
        this.logger = logger;
        this.options = options;
        if (options.JournalDir == null)
        {
            // REVIEW - find a better way to do this
            options.JournalDir = serviceProvider.GetService<IOptionsMonitor<TradeJournalOptions>>().CurrentValue.JournalDir ?? throw new ArgumentException("No JournalDir");
        }
        ExchangeSymbol = exchangeSymbol;
        UpdatePath();
    }

    //~TradeJournal() => Dispose();

    public void Dispose()
    {
        if (entries == null) return;
        DisposeAsync().AsTask().Wait();
    }
    public async ValueTask DisposeAsync()
    {
        await CloseAll();
        entries = null!;
    }

    //private string GetPath(string filename) => Path.Combine(Options.JournalDir ?? throw new ArgumentNullException(nameof(Options.JournalDir)), filename);

    private void OnClosed(string path)
    {
        if (FileName != null)
        {
            int i = 0;
            string newPath;

            string getPath() => Path.Combine(JournalDirectory ?? throw new ArgumentNullException(nameof(JournalDirectory)), FileName) + (i++ == 0 ? "" : $" ({i:000})") + Path.GetExtension(path);

            if (Options.ReplaceOutput && File.Exists(newPath = getPath()))
            {
                File.Delete(newPath);
            }
            else
            {
                do
                {
                    newPath = getPath();
                } while (File.Exists(newPath));
            }

            if (DiscardDetails || !KeepAborted && IsAborted)
            {
                DeleteWithRetry(path);
            }
            else { File.Move(path, newPath); }
        }
    }
    private static readonly bool KeepAborted = false;

    public bool IsAborted { get; set; }
    public bool DiscardDetails { get; set; }
    public async void DeleteWithRetry(string path)
    {
        for (int retries = 0; retries < 10; retries++)
        {
            try
            {
                File.Delete(path);
                break;
            }
            catch { await Task.Delay(500); }
        }
    }

    public async ValueTask CloseAll()
    {
        if (!DiscardDetails)
        {
            await _Write(forceWriteToDisk: true);
        }

        {
            var copy = csvWriters?.ToArray();
            if (copy != null)
            {
                csvWriters = null!;
                foreach (var kvp in copy)
                {
                    await kvp.Value.FlushAsync();
                    try
                    {
                    }
                    catch { }
                    kvp.Value.Dispose();
                    try
                    {
                    }
                    catch { }
                }
            }
        }
        {
            var copy = fileStreams?.ToArray();
            if (copy != null)
            {
                fileStreams = null!;
                foreach (var kvp in copy.Where(c => c.Key.Item2 != JournalFormat.CSV)) { kvp.Value.Dispose(); }
                foreach (var c in copy) OnClosed(c.Value.Name);
            }
        }
    }

    #endregion

    #region State

    public JournalStatsCollector JournalStatsCollector => journalStatsCollector;
    private JournalStatsCollector journalStatsCollector = new();
    public JournalStats JournalStats => journalStatsCollector.JournalStats;


    ConcurrentDictionary<(string, JournalFormat), FileStream> fileStreams = new();
    ConcurrentDictionary<string, CsvWriter> csvWriters = new();

    #endregion

    #region ITradeJournal

    AsyncLock writeLockBinary = new();
    AsyncLock writeLock = new();
    private ExchangeSymbol? exchangeSymbol;

    ConcurrentQueue<JournalEntry<TPrecision>> entries = new(); // TODO: Replace with channel
    public bool IsDisposed => entries == null;


    Dictionary<int, IPosition<TPrecision>> openPositions = new();

    void UpdateStats(JournalEntry<TPrecision> entry)
    {
        switch (entry.EntryType)
        {
            case JournalEntryType.Unspecified:
                break;
            case JournalEntryType.Open:
                //openPositions.Add(entry.)
                break;
            case JournalEntryType.Close:

                journalStatsCollector.OnClose(entry);
                break;
            case JournalEntryType.Modify:
                break;
            case JournalEntryType.CreateOrder:
                break;
            case JournalEntryType.ModifyOrder:
                break;
            case JournalEntryType.CancelOrder:
                break;
            case JournalEntryType.SwapFee:
                break;
            case JournalEntryType.InterestFee:
                break;
            case JournalEntryType.Abort:
                break;
            case JournalEntryType.Start:
                break;
            case JournalEntryType.End:
                break;
            default:
                break;
        }

    }

    public void Write(JournalEntry<TPrecision> entry)
    {
        UpdateStats(entry);
        if (!Options.EffectiveEnabled) return;
        entries.Enqueue(entry);
        if (entries.Count > Options.BufferEntries) { _Write(forceWriteToDisk: true).FireAndForget(); }
    }

    private async Task _Write(bool forceWriteToDisk = false)
    {
        if (!Options.EffectiveEnabled || DiscardDetails) return;
        if (!forceWriteToDisk && Options.PreferInMemory) return;

        if (entries == null) { return; } // Disposed
        while (entries.TryDequeue(out var entry))
        {
            if (Options.LogLevel != LogLevel.None)
            {
                logger.Log(Options.LogLevel, $"Journal: {entry.Dump()}");
            }

            if ((Options.JournalFormat & JournalFormat.Binary) == JournalFormat.Binary)
            {
                var bin = MemoryPackSerializer.Serialize(entry);
                var stream = GetStream(JournalFormat.Binary);

                await stream.WriteAsync(bin)
                    //.ConfigureAwait(false)
                    ;
            }

            if ((Options.JournalFormat & JournalFormat.CSV) == JournalFormat.CSV)
            {
                var writer = GetCsvWriter();
                writeLock.Wait(() =>
                {
                    //{
                    //await writer.WriteRecordsAsync([entry]);
                    //writer.WriteRecord(entry);
                    writer.WriteRecords([entry]);
                    //}
                    //await writer.FlushAsync();
                });
            }

            if ((Options.JournalFormat & JournalFormat.Text) == JournalFormat.Text)
            {
                var stream = GetStream(JournalFormat.Text);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(entry.ToXamlProperties() + Environment.NewLine)).ConfigureAwait(false);
            }
        }
    }

    public async ValueTask Close(string context)
    {
        if (fileStreams.TryRemove((context, JournalFormat.Binary), out var fs)) { fs.Dispose(); }
        if (fileStreams.TryRemove((context, JournalFormat.Text), out fs)) { fs.Dispose(); }
        if (csvWriters.TryRemove(context, out var csv))
        {
            await csv.FlushAsync();
            await csv.DisposeAsync();
        }
    }

    #endregion

    #region (Private) IO

    private readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        //HasHeaderRecord = true,
        //NewLine = "\n",
        NewLine = "\r\n",
    };

    CsvWriter GetCsvWriter()
    {
        return csvWriters.GetOrAdd(Context, key =>
        {
            //var ext = Options.CsvSeparator switch
            //{
            //    ',' => ".csv",
            //    ';' => ".csv",
            //    '\t' => ".tsv",
            //    _ => ".csv",
            //};

            //var fs = new FileStream(Path.Combine(Options.JournalDir, key + ext), FileMode.Append, FileAccess.Write, FileShare.Read);
            var writer = new StreamWriter(GetStream(JournalFormat.CSV));
            var csv = new CsvWriter(writer, CsvConfiguration);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.Start,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return csv;
        });
    }

    FileStream GetStream(JournalFormat journalFormat)
    {
        return fileStreams.GetOrAdd((Context, journalFormat), key =>
        {
            var ext = journalFormat switch
            {
                JournalFormat.Binary => ".dat",
                JournalFormat.Text => ".txt",
                JournalFormat.CSV => Options.CsvSeparator switch
                {
                    '\t' => ".tsv",
                    _ => ".csv",
                },
                _ => throw new NotSupportedException($"Specify a single {nameof(journalFormat)}"),
            };

            if (!Directory.Exists(Path.GetDirectoryName(pathWithoutExtension)))
            {
                Debug.WriteLine($"WARNING - dir missing: {JournalDirectory}");
                Debug.WriteLine(Environment.StackTrace);

                //return null;
            }

            var fs = new FileStream(pathWithoutExtension + ext, FileMode.Append, FileAccess.Write, FileShare.Read);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.Start,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return fs;
        });
    }

    #endregion



}
