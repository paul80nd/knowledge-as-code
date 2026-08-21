using System.Text.Json;
using System.Text.Json.Nodes;

namespace kac.core;

// `Json.cs` holds the documents kac emits, which are records and are serialized. This holds the other
// direction: the documents kac reads back — a corpus's own `plugin.json`, and an export's manifest and
// record files, which may have been written by a different build of this tool.
//
// Every reader here survives a value of the wrong shape rather than throwing over one. Nothing
// validates these files before they are read, and a run that fell over on a mistyped key would report
// a stack trace where the caller has a sentence ready about what was missing and what to do next.
public static class JsonRead
{
    // A document parsed, or null where it is absent or not a JSON object. A leading byte-order mark is
    // stripped: an editor on Windows may add one, and `JsonNode.Parse` refuses a document starting with
    // it.
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

    // A value read as a string, or null where it is absent, blank or some other type. Blank counts as
    // absent: a key present and empty is a key nobody filled in, and treating it as a value would put
    // an empty name where the caller has a fallback ready.
    public static string? Str(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var s) && s.Length > 0 ? s : null;

    public static int? Int(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var i) ? i : null;
}
