using CsvHelper.Configuration;
using CsvHelper;
using System.IO;
using System.Threading.Channels;
using System.Globalization;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;

#if UNUSED
public class IgnoreEmptyArrayConverter : JsonConverter<List<object>>
{
    public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<List<object>>(ref reader, options) ?? new();
    }

    public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
    {
        if (value != null && value.Count > 0)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item != null)
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
            }
            writer.WriteEndArray();
        }
    }
}

public class IgnoreEmptyArrayConverter2 : JsonConverter<object[]>
{
    public override object[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<object[]>(ref reader, options) ?? Array.Empty<object>();
    }

    public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
    {
        if (value != null && value.Length > 0)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item != null)
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
            }
            writer.WriteEndArray();
        }
    }
}
#endif

public class IgnoreEmptyArrayConverter<T> : JsonConverter<T[]>
{
    public override T[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T[]>(ref reader, options) ?? Array.Empty<T>();
    }

    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
    {
        if (value != null && value.Length > 0)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item != null)
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
            }
            writer.WriteEndArray();
        }
    }
}

public class BacktestBatchJournal : IAsyncDisposable
{

    #region Parameters

    public string JournalFilename { get; set; } = "backtests";
    public Type PBotType { get; }

    private readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        //HasHeaderRecord = true,
        //NewLine = "\n",
        NewLine = "\r\n",
        //ShouldQuote = args => false,
    };

    #region Derived

    //private ParameterMetadata ParameterMetadata { get; }

    #endregion

    #endregion

    #region Lifecycle

    public BacktestBatchJournal(string dir, Type pBotType)
    {
        var path = Path.Combine(dir, JournalFilename + ".csv");
        var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        var writer = new StreamWriter(fs);
        csv = new CsvWriter(writer, CsvConfiguration);

        consumeTask = Consume();
        PBotType = pBotType;
        //ParameterMetadata = ParameterMetadata.Get(pBotType);

        //var mapType = typeof(ParametersMapper<>).MakeGenericType(pBotType);
        csv.Context.RegisterClassMap(new ParametersMapper(pBotType));
        //typeof(CsvContext).GetMethod(nameof(CsvContext.RegisterClassMap), new Type[] { })!.MakeGenericMethod(mapType).Invoke(csv.Context, null);
    }

    public class ParametersMapper : DynamicSplitMap<BacktestBatchJournalEntry>
    {
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            Converters = { 
                       //new System.Text.Json.Serialization.JsonStringEnumConverter()
                       new IgnoreEmptyArrayConverter<IKeyed<string>>(),
                   }
        };
        public ParametersMapper(Type pType) : base(pType, nameof(BacktestBatchJournalEntry.Parameters))
        {
            //var parameterMetadata = ParameterMetadata.Get(pType);

            Map(m => m.Id);
            Map(m => m.Fitness);
            Map(m => m.AD);

            //Map(m => m.Parameters).Convert(row =>
            //{
            //    //var entry = row.Value;
            //    //.GetRecord<BacktestBatchJournalEntry>();
            //    //foreach(var pi in ParameterMetadata)
            //    //{
            //    //    object value = pi.GetValue(entry);
            //    //}
            //    var json = System.Text.Json.JsonSerializer.Serialize(row.Value.Parameters, options);
            //    //var json =  JsonConvert.SerializeObject(row.Value.Parameters);
            //    var hjson = JsonValue.Parse(json).ToString(Stringify.Hjson);
            //    return json;
            //    //return string.Join(", ", row.Value.Parameters!); // parameterMetadata.Items.Select(kvp => kvp.GetValue(row.Value)));
            //});

            //var bpi = BotParametersInfo.Get(pType);


            //foreach (var kvp in bpi.Parameters)
            //{
            //    //Map(_ => kvp.Key).Convert((ConvertFromString<string>) (row =>
            //    //{
            //    // TODO
            //    //}));
            //    //Map(kvp.Value.PropertyInfo.DeclaringType!, kvp.Value.PropertyInfo)

            //    Map(e => kvp.Key).Convert((ConvertToString<BacktestBatchJournalEntry>)(row =>
            //    {
            //        return PropertyFlattener.GetValueFromPath(row.Value.Parameters, kvp.Key)
            //            ?.ToString() // TODO: Serialize using Json or Hjson?
            //            ;
            //    }));
            //}
        }
    }

    Task consumeTask;
    CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    public ValueTask DisposeAsync()
    {
        channel.Writer.Complete();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region State

    CsvWriter csv;
    Channel<BacktestBatchJournalEntry> channel = Channel.CreateUnbounded<BacktestBatchJournalEntry>();

    #endregion

    #region (Public) Methods

    public bool TryWrite(BacktestBatchJournalEntry e)
    {
        return channel.Writer.TryWrite(e);
    }
    public async ValueTask WriteAsync(BacktestBatchJournalEntry e)
    {
        await channel.Writer.WriteAsync(e);
    }

    public async Task Consume()
    {
        await foreach (var item in channel.Reader.ReadAllAsync().WithCancellation(CancellationTokenSource.Token))
        {
            await csv.WriteRecordsAsync([item]);
        }
        await csv.DisposeAsync();

    }

    #endregion
}

//public class ParameterMetadata
//{
//    #region (static)

//    private static ConcurrentDictionary<Type, ParameterMetadata> cache = new();

//    public static ParameterMetadata Get(Type type)
//    {
//        return cache.GetOrAdd(type, k => new ParameterMetadata(type));
//    }
//    #endregion

//    public List<Key> Items { get; } = new();

//    public ParameterMetadata(Type type)
//    {

//        foreach (var pi in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
//        {
//            //if (pi.GetCustomAttributes<SignalAttribute>().Any()) continue;

//            //if (!pi.PropertyType.IsPrimitive) continue;

//            Items.Add(pi);
//        }
//    }
//}