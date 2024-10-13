using LionFire.Structures;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

//public class EmptyArrayConverter<T> : JsonConverter<T[]> // MOVE
//{
//    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        return Array.Empty<T>();
//    }

//    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
//    {
//        // Not applicable in this case, as we're ignoring empty arrays
//        writer.
//    }
//}

//public class IgnoreEmptyArrayConverter : JsonConverter<List<object>>
//{
//    public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        return JsonSerializer.Deserialize<List<object>>(ref reader, options);
//    }

//    public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
//    {
//        if (value != null && value.Count > 0)
//        {
//            writer.WriteStartArray();
//            foreach (var item in value)
//            {
//                if (item != null)
//                {
//                    JsonSerializer.Serialize(writer, item, options);
//                }
//            }
//            writer.WriteEndArray();
//        }
//    }
//}


// TODO: Also have indicators derive from this?
// TODO: Subclass for unbound inputs?
public abstract class PMarketProcessor : IPMarketProcessor
{
    //public IPInput[]? Inputs { get;  }
    //[JsonConverter(typeof(EmptyArrayConverter<IKeyed<string>>))]
    [JsonIgnore]
    public virtual IKeyed<string>[] DerivedInputs => [];

    [JsonIgnore]
    public int[]? InputLookbacks { get; set; }

    [JsonIgnore]
    public abstract Type MaterializedType { get; }
    //public abstract Type MaterializedType { get; }


}
