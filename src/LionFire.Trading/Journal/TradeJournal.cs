using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using LionFire.ExtensionMethods.Dumping;
using LionFire.Inspection.Nodes;
using MemoryPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace LionFire.Trading.Journal;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class AliasAttribute : Attribute
{
    public AliasAttribute(string alias) => Alias = alias;

    public string Alias { get; }
}

public class ConvertToStringClass2
{
    #region Dependencies

    public PropertyInfo PropertyInfo { get; }
    public ConvertToStringClass2? ParentConverter { get; }

    #endregion

    #region Lifecycle

    public ConvertToStringClass2(PropertyInfo propertyInfo, ConvertToStringClass2? parentConverter = null)
    {
        PropertyInfo = propertyInfo;
        ParentConverter = parentConverter;
    }

    #endregion

    public object? GetValue<T>(T root)
    {
        object? instance = ParentConverter == null
            ? root
            : ParentConverter.GetValue(root);
        return instance == null
            ? null
            : PropertyInfo.GetValue(instance);
    }

    public string? ConvertToString<T>(ConvertToStringArgs<T> args)
    {
        return GetValue(args.Value)?.ToString() ?? "";
    }
}

public class HierarchicalPropertyInfo : IKeyable<string>
{
    #region Relationships

    public HierarchicalPropertyInfo? Parent { get; set; }

    #endregion

    #region Identity

    public string Key { get; set; }

    #region Derived

    public string Path { get; }
    public string Name => LastPropertyInfo.Name;
    public string? Alias { get; set; }
    public string EffectiveName => Alias ?? Name;

    #endregion

    #endregion

    #region Lifecycle

    public HierarchicalPropertyInfo(HierarchicalPropertyInfo? parent, IReadOnlyList<PropertyInfo> propertyInfos)
    {
        Parent = parent;
        Path = string.Join(".", propertyInfos.Select(pi => pi.Name));
        Key = Path;
        PropertyInfos = propertyInfos;
        Alias = LastPropertyInfo.GetCustomAttribute<AliasAttribute>()?.Alias;
    }
    #endregion

    #region Children

    public IReadOnlyList<PropertyInfo> PropertyInfos { get; }

    #region Derived

    public PropertyInfo LastPropertyInfo => PropertyInfos.Last();

    #endregion

    #endregion

    #region Bonus

    public ConvertToStringClass2 ConvertToString
    {
        get
        {
            if (convertToString == null)
            {
                convertToString = new ConvertToStringClass2(LastPropertyInfo, Parent?.ConvertToString);
            }
            return convertToString;
        }
    }
    private ConvertToStringClass2? convertToString;

    #endregion
}

public class TradeJournal<TPrecision> : ITradeJournal<TPrecision>, IDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity


    #endregion

    #region Dependencies

    private readonly ILogger<TradeJournal<TPrecision>> logger;

    #endregion

    #region Options

    public TradeJournalOptions Options => options;
    private readonly TradeJournalOptions options;

    private string? pathWithoutExtension { get; set; }
    private void UpdatePath() => pathWithoutExtension = Path.Combine(Options.JournalDir ?? throw new ArgumentNullException(), context);
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

    public string JournalDirectory => Options.JournalDir;
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

    public TradeJournal(ILogger<TradeJournal<TPrecision>> logger, TradeJournalOptions options, ExchangeSymbol? exchangeSymbol = null)
    {
        this.logger = logger;
        this.options = options;
        ExchangeSymbol = exchangeSymbol;
        UpdatePath();
    }

    ~TradeJournal() => Dispose();

    public void Dispose()
    {
        CloseAll().AsTask().Wait();
    }
    private string GetPath(string filename) => Path.Combine(Options.JournalDir, filename);

    private void OnClosed(string path)
    {
        if (FileName != null)
        {
            int i = 0;
            string newPath;

            string getPath() => Path.Combine(JournalDirectory, FileName) + (i++ == 0 ? "" : $" ({i:000})") + Path.GetExtension(path);

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

            File.Move(path, newPath);
        }
    }

    public async ValueTask CloseAll()
    {
        await _Write();
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
                foreach (var c in copy) OnClosed(c.Value.Name);
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
    private ExchangeSymbol? exchangeSymbol;

    ConcurrentQueue<JournalEntry<TPrecision>> entries = new(); // TODO: Replace with channel

    public void Write(JournalEntry<TPrecision> entry)
    {
        entries.Enqueue(entry);
        if (entries.Count > 100) { _Write(); }
    }

    private async ValueTask _Write()
    {
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
