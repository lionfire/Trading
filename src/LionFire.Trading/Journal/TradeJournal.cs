using CsvHelper;
using CsvHelper.Configuration;
using LionFire.ExtensionMethods.Dumping;
using MemoryPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace LionFire.Trading.Journal;

public class TradeJournal<TPrecision> : ITradeJournal<TPrecision>, IDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    private readonly string DefaultContext = "Process-" + Guid.NewGuid();

    #endregion

    #region Dependencies

    private readonly ILogger<TradeJournal<TPrecision>> logger;

    #endregion

    #region Options

    public TradeJournalOptions Options => options.CurrentValue;
    private readonly IOptionsMonitor<TradeJournalOptions> options;

    #endregion

    #region Lifecycle

    public TradeJournal(ILogger<TradeJournal<TPrecision>> logger, IOptionsMonitor<TradeJournalOptions> options)
    {
        this.logger = logger;
        this.options = options;

        if (!Directory.Exists(Options.JournalDir))
        {
            Directory.CreateDirectory(Options.JournalDir);
        }
    }

    ~TradeJournal() => Dispose();

    public void Dispose()
    {
        CloseAll().AsTask().Wait();
    }

    public async ValueTask CloseAll()
    {
        {
            var copy = csvWriters.ToArray();
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
            var copy = fileStreams.ToArray();
            if (copy != null)
            {
                fileStreams = null!;
                foreach (var kvp in copy.Where(c => c.Key.Item2 != JournalFormat.CSV)) { kvp.Value.Dispose(); }
            }
        }

    }

    #endregion

    #region State

    ConcurrentDictionary<(string, JournalFormat), FileStream> fileStreams = new();
    ConcurrentDictionary<string, CsvWriter> csvWriters = new();

    #endregion

    #region ITradeJournal

    AsyncLock writeLockBinary = new();
    AsyncLock writeLock = new();
    public async ValueTask Write(JournalEntry<TPrecision> entry)
    {
        if (Options.LogLevel != LogLevel.None) { }
        logger.Log(Options.LogLevel, $"Journal: {entry.Dump()}");

        if (Options.JournalFormat.HasFlag(JournalFormat.Binary))
        {
            var bin = MemoryPackSerializer.Serialize(entry);
            var stream = GetStream(entry.Context, JournalFormat.Binary);

            await stream.WriteAsync(bin)
                //.ConfigureAwait(false)
                ;
        }

        if (Options.JournalFormat.HasFlag(JournalFormat.CSV))
        {
            var writer = GetCsvWriter(entry.Context);
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

        if (Options.JournalFormat.HasFlag(JournalFormat.Text))
        {
            var stream = GetStream(entry.Context, JournalFormat.Text);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(entry.ToXamlProperties() + Environment.NewLine)).ConfigureAwait(false);
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

    CsvWriter GetCsvWriter(string? context)
    {
        context ??= DefaultContext;

        return csvWriters.GetOrAdd(context, key =>
        {
            //var ext = Options.CsvSeparator switch
            //{
            //    ',' => ".csv",
            //    ';' => ".csv",
            //    '\t' => ".tsv",
            //    _ => ".csv",
            //};

            //var fs = new FileStream(Path.Combine(Options.JournalDir, key + ext), FileMode.Append, FileAccess.Write, FileShare.Read);
            var writer = new StreamWriter(GetStream(context, JournalFormat.CSV));
            var csv = new CsvWriter(writer, CsvConfiguration);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.JournalOpen,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return csv;
        });
    }

    FileStream GetStream(string? context, JournalFormat journalFormat)
    {
        context ??= DefaultContext;

        return fileStreams.GetOrAdd((context, journalFormat), key =>
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

            var fs = new FileStream(Path.Combine(Options.JournalDir, key.Item1 + ext), FileMode.Append, FileAccess.Write, FileShare.Read);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.JournalOpen,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return fs;
        });
    }

    #endregion

}
