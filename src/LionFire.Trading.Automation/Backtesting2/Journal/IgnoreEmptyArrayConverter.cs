using System.Text.Json;
using System.Text.Json.Serialization;

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