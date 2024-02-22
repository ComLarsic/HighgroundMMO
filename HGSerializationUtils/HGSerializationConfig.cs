using System.Text.Json;

namespace HGSerializationUtils;

/// <summary>
/// A collection of utilities for serialization.
/// </summary>
public static class HGSerializationConfig
{
    /// <summary>
    /// The default JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        Converters = { new Vector2Converter() }
    };
}
