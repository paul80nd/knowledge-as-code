using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace kac.core;

public static class Yaml
{
    public static YamlNode LoadFile(string path) => Load(File.ReadAllText(path));

    // The same parse over text the caller already holds, so a document can arrive from somewhere other
    // than a file. `Schema.Load` reads a schema handed over as strings through this.
    public static YamlNode Load(string text)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(text));
        return stream.Documents.Count > 0 ? stream.Documents[0].RootNode : new YamlMappingNode();
    }

    public static YamlNode? Get(YamlNode? node, string key)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                if (kv.Key is YamlScalarNode k && k.Value == key)
                    return kv.Value;
        return null;
    }

    public static IEnumerable<(string, YamlNode)> Map(YamlNode? node)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                yield return (((YamlScalarNode)kv.Key).Value ?? "", kv.Value);
    }

    // A plain `null` or `~` reads as absent, not as the four-character string "null". That is the same
    // rule the validator applies to a document's frontmatter, applied to the schema that declares it. The
    // schema writes `folder: null` and `prefix: null` where a type has neither, and without this a
    // folderless type is one whose folder is named "null": every emptiness test downstream silently
    // passes. Plain style only, so a value quoted "null" is still the string someone meant.
    public static string? Str(YamlNode? node)
    {
        if (node is not YamlScalarNode sc) return null;
        return sc is { Style: ScalarStyle.Plain, Value: "" or "~" or "null" or "Null" or "NULL" } ? null : sc.Value;
    }

    public static bool Bool(YamlNode? node)
        => Bool(node, false);

    // The same reading for a key whose absence means yes. Both spellings of each answer are taken, because
    // YAML admits `no` and `off` as well as `false`, and a schema writing one of those to switch something
    // off would otherwise switch it on. Anything else falls back, which is what an absent key does.
    public static bool Bool(YamlNode? node, bool fallback)
        => (node as YamlScalarNode)?.Value?.ToLowerInvariant() switch
        {
            "true" or "yes" or "on" => true,
            "false" or "no" or "off" => false,
            _ => fallback
        };

    public static int Int(YamlNode? node, int fallback)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : fallback;

    // The same reading, where the schema's silence and a number it declares have to stay
    // distinguishable. A floor of zero is not the absence of one.
    public static int? NullableInt(YamlNode? node)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : null;

    public static List<string> StrList(YamlNode? node)
    {
        var result = new List<string>();
        if (node is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (item is YamlScalarNode s)
                    result.Add(s.Value ?? "");
        return result;
    }

    // A scalar's value exactly as written, with none of the readings `Str` applies. The schema is read
    // through `Str`, where a plain `null` is the absence of a declaration; a document's frontmatter is
    // read through this, where the characters an author typed are the thing being judged and a check
    // reporting `'status' value '' is not one of…` has to be able to quote back what is there.
    public static string? Raw(YamlNode node) => (node as YamlScalarNode)?.Value;

    // Where a node sits in the file that carries it. A parser reads a frontmatter block on its own, so
    // the lines it reports start at 1 within the block; `frontStart` is the line the block opens on in
    // the document, which turns one into the other.
    //
    // A node the parser gave no position falls back to the block's own line, which is the nearest true
    // answer. The finding lands on the frontmatter and never on a line picked at random.
    public static int? LineOf(YamlNode node, int frontStart)
        => node.Start.Line > 0 ? (int)node.Start.Line + frontStart - 1 : frontStart;
}
