using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HGSerializationUtils;

/// <summary>
/// A JSON converter for Vector2.
/// </summary>
public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        float x = 0;
        float y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector2(x, y);

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
            var propertyName = reader.GetString();
            reader.Read();
            if (propertyName == "X")
                x = reader.GetSingle();
            else if (propertyName == "Y")
                y = reader.GetSingle();
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", float.IsFinite(value.X) ? value.X : 0);
        writer.WriteNumber("Y", float.IsFinite(value.Y) ? value.Y : 0);
        writer.WriteEndObject();
    }
}