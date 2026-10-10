namespace kac.core;

// What looking a citation up came to. `Missing` is nothing answering to it anywhere. The two spelling answers are a
// record that does exist, written the other way.
public enum Landing
{
    Local,
    Imported,
    Missing,
    NeedsScope,
    NeedsNoScope,
    UnknownScope,

    // The scope names a corpus that is not on disk. `import-restored` reports that once against the descriptor, so
    // every citation into it stays quiet.
    NotRestored
}

// Where a citation landed, and everything a check asks about the record it found, for a local `Doc` and an
// `ImportedRecord` alike.
public sealed record Landed(Landing How, Doc? Local, ImportedRecord? Imported, string? Scope)
{
    // The record a message names: a path where this corpus holds the file, and the citation itself where another
    // corpus does.
    public string Where => Local?.Rel ?? $"{Scope}:{Imported?.Id}";

    // What one part of this record is called, in the word the type's own readers use. Null where the type keeps no
    // parts at all.
    public string? Noun(Schema schema)
    {
        if (Local is { } local) return local.Type?.Parts?.Noun;
        if (Imported is not { KeepsParts: true } imported) return null;

        return schema.ByFolder.GetValueOrDefault(imported.Type)?.Parts?.Noun ?? "part";
    }

    // The imported record, for a caller that has already established the citation landed on one. Throws otherwise.
    private ImportedRecord Found => Imported
                                    ?? throw new InvalidOperationException(
                                        $"a citation that landed {How} holds no imported record.");

    // Compared ordinally, as a part id is everywhere else.
    public bool Carries(string part) =>
        Local is { } local
            ? local.Parts.Any(p => string.Equals(p.Id, part, StringComparison.Ordinal))
            : Found.Parts.Any(p => string.Equals(p, part, StringComparison.Ordinal));

    // The type this record is, as the schema names it, and null where nothing in the schema covers it.
    public TypeSchema? Type(Schema schema) =>
        Local is { } local ? local.Type : schema.ByFolder.GetValueOrDefault(Found.Type);
}

// Every record a citation can land on, local and imported alike, behind one lookup. A local record is cited bare and
// an imported one with its producer's shortcode, and each spelling is refused on the other's records.
public sealed class Resolver
{
    // Case-insensitive throughout, as `byId` and `id-unique` are, so a mis-cased import still resolves.
    private readonly Dictionary<string, Doc> _local;
    private readonly Dictionary<string, ImportedRecord> _imported = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _scopes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _shortcodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _pending;

    public Resolver(Dictionary<string, Doc> local, ImportGraph imports)
    {
        _local = local;
        _pending = new HashSet<string>(imports.NotRestored, StringComparer.OrdinalIgnoreCase);

        foreach (var import in imports.Imports)
        {
            _shortcodes.Add(import.Shortcode);

            // The scope is the record's own, not the import's, which is how a citation into a grandparent resolves.
            foreach (var record in import.Records)
            {
                _shortcodes.Add(record.Scope);
                _imported[$"{record.Scope}:{record.Id}"] = record;

                // Which corpus a bare id would have meant, so a citation missing its scope is told the spelling to
                // write. The first record declaring an id wins.
                _scopes.TryAdd(record.Id, record.Scope);
            }
        }
    }

    // Whether anything was imported at all. A corpus standing on its own skips the spelling rules.
    public bool Any => _imported.Count > 0;

    // Where this citation's record lands. Whether the record carries the part is the caller's question.
    public Landed Resolve(Citation citation)
    {
        if (citation.Scope is not { } scope)
            return _local.TryGetValue(citation.Record, out var doc)
                ? new Landed(Landing.Local, doc, null, null)
                : _scopes.TryGetValue(citation.Record, out var holder)
                    ? new Landed(Landing.NeedsScope, null, null, holder)
                    : new Landed(Landing.Missing, null, null, null);

        if (_pending.Contains(scope)) return new Landed(Landing.NotRestored, null, null, scope);

        // A shortcode nothing here knows, while a declared import is missing, may be a corpus inside that export.
        // `import-restored` already names the fix.
        if (!_shortcodes.Contains(scope))
            return _pending.Count > 0
                ? new Landed(Landing.NotRestored, null, null, scope)
                : new Landed(Landing.UnknownScope, null, null, scope);

        if (_imported.TryGetValue(citation.Whole, out var record))
            return new Landed(Landing.Imported, null, record, scope);

        return _local.ContainsKey(citation.Record)
            ? new Landed(Landing.NeedsNoScope, null, null, scope)
            : new Landed(Landing.Missing, null, null, scope);
    }
}
