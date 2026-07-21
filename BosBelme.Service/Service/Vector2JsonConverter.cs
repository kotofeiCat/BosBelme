using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace One_Shot_Bounce.Engine;

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        float x = 0;
        float y = 0;

        if (root.TryGetProperty("x", out var xProp))
        {
            x = xProp.GetSingle();
        }
        else if (root.TryGetProperty("X", out var xPropUpper))
        {
            x = xPropUpper.GetSingle();
        }

        if (root.TryGetProperty("y", out var yProp))
        {
            y = yProp.GetSingle();
        }
        else if (root.TryGetProperty("Y", out var yPropUpper))
        {
            y = yPropUpper.GetSingle();
        }

        return new Vector2(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }
}