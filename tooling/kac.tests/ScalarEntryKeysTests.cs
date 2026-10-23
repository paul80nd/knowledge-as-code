// Reads both ends from the source: the `if (...) continue;` guards ahead of a `FrontList(` read in the same method,
// and the `yield return` lines of `SchemaChecks.ScalarEntryKeys`. A guard is seen on one line, and on a FieldSpec
// named `spec` or reached through `Fields[...]`.

using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class ScalarEntryKeysTests
{
    private static readonly string Core = Path.Combine(Repo.Root, "tooling", "kac.core");

    [Fact]
    public void Schema_shape_refuses_every_key_that_guards_a_scalar_list_read()
    {
        var keys = YamlKeys();
        var guarding = GuardingProperties(keys).Select(p => keys[p]).ToHashSet(StringComparer.Ordinal);
        var listed = Listed(keys);

        var missing = guarding.Except(listed).Order(StringComparer.Ordinal).ToList();
        var unread = listed.Except(guarding).Order(StringComparer.Ordinal).ToList();

        Assert.True(missing.Count == 0 && unread.Count == 0,
            "SchemaChecks.ScalarEntryKeys is out of step with the reads through Doc.FrontList. "
            + $"Guards a read and is not listed: [{string.Join(", ", missing)}]. "
            + $"Listed and guards no read: [{string.Join(", ", unread)}].");
    }

    /// <summary>Each <c>FieldSpec</c> property that <c>Schema.ParseField</c> fills from a YAML key, and that key.</summary>
    private static Dictionary<string, string> YamlKeys()
    {
        var source = File.ReadAllText(Path.Combine(Core, "Schema.cs"));
        var start = source.IndexOf("return new FieldSpec", StringComparison.Ordinal);
        Assert.True(start >= 0, "Schema.cs no longer builds a FieldSpec with 'return new FieldSpec'.");

        var block = source[start..source.IndexOf("};", start, StringComparison.Ordinal)];
        var keys = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Match m in Assignment().Matches(block))
        {
            var rhs = m.Groups["rhs"].Value;
            var key = Get().Match(rhs) is { Success: true } direct
                ? direct.Groups["key"].Value
                : Local(source, Identifier().Match(rhs).Value);

            if (key is not null) keys[m.Groups["prop"].Value] = key;
        }

        return keys;
    }

    /// <summary>The key a local read with <c>node.Get</c>, or null where it was not read from one.</summary>
    private static string? Local(string source, string local)
    {
        if (local.Length == 0) return null;

        var m = Regex.Match(source, $@"\b{local}\s*=\s*[^;]*node\.Get\(""(?<key>[\w-]+)""\)");
        return m.Success ? m.Groups["key"].Value : null;
    }

    private static HashSet<string> GuardingProperties(Dictionary<string, string> keys)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in Directory.EnumerateFiles(Core, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")) continue;

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var code = lines[i].TrimStart();
                if (code.StartsWith("//", StringComparison.Ordinal) || !code.Contains("FrontList(")
                                                                     || code.Contains("List<string> FrontList(")) continue;

                for (var j = i; j >= 0 && !MemberStart().IsMatch(lines[j]); j--)
                    if (Guard().Match(lines[j]) is { Success: true } guard)
                        foreach (Match p in SpecProperty().Matches(guard.Groups["condition"].Value))
                            if (keys.ContainsKey(p.Groups["prop"].Value)) found.Add(p.Groups["prop"].Value);
            }
        }

        return found;
    }

    /// <summary>The keys <c>ScalarEntryKeys</c> yields, each checked against the property its line tests.</summary>
    private static HashSet<string> Listed(Dictionary<string, string> keys)
    {
        var source = File.ReadAllText(Path.Combine(Core, "Checks", "SchemaChecks.cs"));
        var start = source.IndexOf("IEnumerable<string> ScalarEntryKeys(", StringComparison.Ordinal);
        Assert.True(start >= 0, "SchemaChecks.cs no longer declares ScalarEntryKeys.");

        var body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];
        var listed = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match m in Yield().Matches(body))
        {
            var prop = Property().Match(m.Groups["condition"].Value).Groups["prop"].Value;
            var key = m.Groups["key"].Value;

            Assert.True(keys.GetValueOrDefault(prop) == key,
                $"ScalarEntryKeys yields '{key}' on a test of '{prop}', which Schema.cs reads from "
                + $"'{keys.GetValueOrDefault(prop) ?? "no key"}'.");
            listed.Add(key);
        }

        return listed;
    }

    [GeneratedRegex(@"^\s+(?<prop>[A-Z]\w*) = (?<rhs>.+?),?\s*$", RegexOptions.Multiline)]
    private static partial Regex Assignment();

    [GeneratedRegex(@"node\.Get\(""(?<key>[\w-]+)""\)")]
    private static partial Regex Get();

    [GeneratedRegex(@"^[a-z]\w*")]
    private static partial Regex Identifier();

    [GeneratedRegex(@"^ {4}[A-Za-z].*\(")]
    private static partial Regex MemberStart();

    [GeneratedRegex(@"\bif \((?<condition>.*)\) continue;")]
    private static partial Regex Guard();

    [GeneratedRegex(@"\.(?<prop>[A-Z]\w*)\b")]
    private static partial Regex Property();

    [GeneratedRegex(@"(?:\bspec|\bFields\[\w+\])\.(?<prop>[A-Z]\w*)\b")]
    private static partial Regex SpecProperty();

    [GeneratedRegex(@"if \((?<condition>.*)\) yield return ""(?<key>[\w-]+)"";")]
    private static partial Regex Yield();
}
