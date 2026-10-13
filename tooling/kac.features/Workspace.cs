using System.Text.Json.Nodes;
using kac.core;

/// <summary>
/// One scenario's corpus. Reqnroll builds one per scenario, hands the same instance to every binding class that asks for
/// it, and disposes it when the scenario ends.
/// </summary>
public sealed class Workspace : IDisposable
{
    private string? _root;
    private SortedDictionary<string, byte[]> _before = new(StringComparer.Ordinal);

    public string Root => _root ?? throw new InvalidOperationException(
        "assemble a corpus first (missing 'Given the <name> fixture corpus')");

    /// <summary>-1 until a command has run.</summary>
    public int Exit { get; set; } = -1;

    public void Assemble(string fixtureName) => _root = Harness.Assemble(fixtureName);

    /// <summary>Takes a snapshot first, so a step can say what the command wrote.</summary>
    public void Run(Func<string, int> command)
    {
        _before = Snapshot();
        Exit = command(Root);
    }

    public string PathOf(string relative) => Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));

    public string Read(string relative) => File.ReadAllText(PathOf(relative));

    public void Write(string relative, string content)
    {
        var full = PathOf(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full) ?? Root);
        File.WriteAllText(full, content);
    }

    public JsonObject ReadJson(string relative) =>
        JsonRead.Parse(Read(relative)) ?? throw new InvalidOperationException($"{relative} is not a JSON object");

    public void WriteJson(string relative, JsonObject json) => Write(relative, json.ToJsonString());

    /// <summary>Every path added, removed or rewritten by the last command, with <c>/</c> as the separator.</summary>
    public List<string> Changed()
    {
        var after = Snapshot();
        return
        [
            .. _before.Keys.Union(after.Keys, StringComparer.Ordinal)
                .Where(p => !_before.TryGetValue(p, out var a) || !after.TryGetValue(p, out var b)
                                                                  || !a.AsSpan().SequenceEqual(b))
                .Order(StringComparer.Ordinal)
        ];
    }

    public SortedDictionary<string, byte[]> Snapshot(string relative = "")
    {
        var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
        var from = PathOf(relative);
        if (!Directory.Exists(from)) return files;

        foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
            files[Path.GetRelativePath(from, file).Replace('\\', '/')] = File.ReadAllBytes(file);
        return files;
    }

    public void Dispose()
    {
        if (_root is not null) Harness.Delete(_root);
    }
}
