namespace kac.core;

// What looking a citation up came to.
//
// `Missing` is nothing answering to it anywhere. The two spelling answers are a record that does exist,
// written the other way, which is a different mistake and earns a different sentence: a reader told the
// record does not exist goes looking for the record.
public enum Landing
{
    Local,
    Imported,
    Missing,
    NeedsScope,
    NeedsNoScope,
    UnknownScope,

    // The scope names an import this corpus declared and has not restored. `import-restored` reports
    // that once, against the descriptor, so every citation into it stays quiet: a reader whose restore
    // has not run wants one line telling them to run it, not one per reference.
    NotRestored
}

// Where a citation landed, and everything a check asks about the record it found.
//
// The two halves are answered here rather than at each call site. A check reading a part off a `Doc`
// and off an `ImportedRecord` separately would be a second resolution path, free to drift from this
// one in whatever it forgot to ask.
public sealed record Landed(Landing How, Doc? Local, ImportedRecord? Imported, string? Scope)
{
    // The record a message names. A path where this corpus holds the file, and the citation itself where
    // another corpus does: a path into somebody else's repository is a fact about their tree.
    public string Where => Local?.Rel ?? $"{Scope}:{Imported?.Id}";

    // What one part of this record is called, in the word the type's own readers use. Null where the
    // type keeps no parts at all, which is a different fault from naming a part it does not carry.
    //
    // An imported record's type is read from the schema this corpus holds. A consumer that never adopted
    // the producer's type still judges the citation, and falls back to the general word for it.
    public string? Noun(Schema schema)
    {
        if (Local is { } local) return local.Type?.Parts?.Noun;
        if (Imported is not { KeepsParts: true } imported) return null;

        return schema.ByFolder.GetValueOrDefault(imported.Type)?.Parts?.Noun ?? "part";
    }

    // Whether the record carries the part named. Compared ordinally, as a part id is everywhere else.
    public bool Carries(string part) =>
        Local is { } local
            ? local.Parts.Any(p => string.Equals(p.Id, part, StringComparison.Ordinal))
            : Imported!.Parts.Any(p => string.Equals(p, part, StringComparison.Ordinal));

    // The type this record is, as the schema names it. Null where nothing in the schema covers it, which
    // leaves the target to the checks that can answer for it.
    public TypeSchema? Type(Schema schema) =>
        Local is { } local ? local.Type : schema.ByFolder.GetValueOrDefault(Imported!.Type);
}

// Every record a citation can land on, local and imported alike, behind one lookup.
//
// **One path serves both.** Once the graph is assembled an imported id is a record like any other, and
// the checks that walk references never ask which side of a boundary they are on. A corpus is not
// judged more loosely for having imported the record it cites.
//
// What the two sides do not share is how they are spelled. A record this corpus holds is cited bare and
// one it imported is cited with its producer's shortcode, so each spelling is refused on the other's
// records: two spellings of one obligation defeat every search anybody runs for it.
public sealed class Resolver
{
    private readonly Dictionary<string, Doc> _local;
    private readonly Dictionary<string, ImportedRecord> _imported = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _scopes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _shortcodes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _pending;

    public Resolver(Dictionary<string, Doc> local, ImportGraph imports)
    {
        _local = local;
        _pending = [.. imports.NotRestored];

        foreach (var import in imports.Imports)
        {
            _shortcodes.Add(import.Shortcode);

            foreach (var record in import.Records)
            {
                _imported[$"{import.Shortcode}:{record.Id}"] = record;

                // Which import a bare id would have meant, so a citation missing its scope is told the
                // spelling to write. The first import declaring an id wins, in the order `consumes:`
                // lists them, which is the order a reader would resolve them in too.
                _scopes.TryAdd(record.Id, import.Shortcode);
            }
        }
    }

    // Whether anything was imported at all. A corpus standing on its own never meets the spelling rules
    // below, so a check can skip the question entirely.
    public bool Any => _imported.Count > 0;

    // Where this citation's record lands. The part is not looked at here: whether a record carries one
    // is a question for whoever asked, and it reads differently for a citation than for a `ref:` field.
    public Landed Resolve(Citation citation)
    {
        if (citation.Scope is not { } scope)
            return _local.TryGetValue(citation.Record, out var doc)
                ? new Landed(Landing.Local, doc, null, null)
                : _scopes.TryGetValue(citation.Record, out var holder)
                    ? new Landed(Landing.NeedsScope, null, null, holder)
                    : new Landed(Landing.Missing, null, null, null);

        if (_pending.Contains(scope)) return new Landed(Landing.NotRestored, null, null, scope);

        if (!_shortcodes.Contains(scope))
            return new Landed(Landing.UnknownScope, null, null, scope);

        if (_imported.TryGetValue(citation.Whole, out var record))
            return new Landed(Landing.Imported, null, record, scope);

        return _local.ContainsKey(citation.Record)
            ? new Landed(Landing.NeedsNoScope, null, null, scope)
            : new Landed(Landing.Missing, null, null, scope);
    }
}
