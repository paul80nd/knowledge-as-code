using System.Text.Json;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// The JSON kac reads back: a corpus's own <c>plugin.json</c>, and an export's manifest and record files, which a
/// different build of this tool may have written. <c>Json.cs</c> holds the documents kac writes. Every reader here
/// returns null or a default for a value of the wrong shape, so the caller can say what was missing.
/// </summary>
public static class JsonRead
{
    public static JsonObject? Parse(string? text)
    {
        if (text is null) return null;

        try
        {
            return JsonNode.Parse(text.TrimStart('﻿')) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static JsonObject? Object(JsonNode? node) => node as JsonObject;

    /// <summary>A value read as a string, or null where it is absent, empty or some other type.</summary>
    public static string? Str(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var s) && s.Length > 0 ? s : null;

    public static int? Int(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var i) ? i : null;

    /// <summary>A value read as a boolean, and false where it is absent or holds anything else.</summary>
    public static bool Bool(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<bool>(out var b) && b;
}
