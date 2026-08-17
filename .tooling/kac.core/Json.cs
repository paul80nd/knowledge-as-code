using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace kac.core;
// ---------------------------------------------------------------------------
// JSON output models
//
// Every JSON document kac emits is a record serialized through the source generator
// (KacJson) — reflection-based serialization is disabled under AOT. To add a new document
// (e.g. `digest` or `drift` output): declare its record shape here and add one
// `[JsonSerializable(typeof(...))]` line to KacJson; no other plumbing is needed.
//
// Property names are PascalCase and emitted camelCase; `int?` line is written as `null`.
// ---------------------------------------------------------------------------

public record ValidateReport(ValidateSummary Summary, IReadOnlyList<ValidateFinding> Findings);

public record ValidateSummary(int Validated, int Templates, int Skipped, int Errors, int Warnings);

public record ValidateFinding(string File, int? Line, string Severity, string Check, string Message);

public record ChecksReport(IReadOnlyList<CheckInfo> Checks);

public record CheckInfo(string Check, string Severity, string Summary);

// ---------------------------------------------------------------------------
// Export documents
//
// A consumer reads these instead of cloning the corpus, so their shape is a contract from the moment
// one is published. `ExportManifest.FormatVersion` is what says which contract is in hand.
// ---------------------------------------------------------------------------

// What this export is, where it came from, and what is in it. Everything that varies between two runs
// over one commit is confined here, so the rest of the output is byte-identical run to run.
//
// `Commit` and `Dirty` together say whether the export can be rebuilt from its own account of itself: a
// commit alone would read as reproducible over a tree that was not. Both are null where git could not
// answer, which is honest about a corpus unpacked from an archive rather than cloned.
public record ExportManifest(
    int FormatVersion,
    string? Corpus,
    string? ContentVersion,
    int? MechanismVersion,
    string? Commit,
    bool? Dirty,
    string GeneratedAt,
    ExportPublishing Publishing,
    IReadOnlyList<ExportedType> Types);

// Where the published form is served from, and the ref every link in this export resolves against. The
// bases are null where the corpus publishes nowhere or names a target nothing builds links for, which
// is the same state the records report by carrying no links.
public record ExportPublishing(string Target, string? HumanBase, string? RawBase, string? Ref);

// One type this export carries, and where to find it. `Parts` is null for a type whose records have no
// parts to flatten, so a consumer reads the per-record files alone rather than seeking a file that was
// never written.
public record ExportedType(string Type, int Count, string Dir, string? Parts);

// One record, carrying what its type's `export:` block declares and nothing else. `Fields` and
// `Sections` are keyed by what the schema named, so a consumer reading a corpus with a type it does not
// know still gets a document it can walk.
public record ExportRecord(
    string Type,
    string Path,
    IReadOnlyDictionary<string, string?> Fields,
    IReadOnlyDictionary<string, string> Sections,
    ExportLinks? Links);

public record ExportLinks(string Human, string Raw);

// One part on one line of the flat file. Self-contained by design: it repeats the record it belongs to
// and the links back to it, because a grep hands back a line and nothing around it.
public record ExportPartLine(
    string Id,
    string Title,
    string Definition,
    string? Not,
    string Type,
    string Record,
    string Part,
    ExportLinks? Links);

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ValidateReport))]
[JsonSerializable(typeof(ChecksReport))]
[JsonSerializable(typeof(ExportManifest))]
[JsonSerializable(typeof(ExportRecord))]
[JsonSerializable(typeof(ExportPartLine))]
public partial class KacJson : JsonSerializerContext
{
    private static KacJson? _relaxed;
    private static KacJson? _line;

    // Shared context for CLI output: the source-generated metadata from Default, plus relaxed escaping,
    // so a quote or an em dash in a finding reaches the reader as itself rather than as a numeric
    // escape. Lazily initialised so it does not touch the generator's Default during static
    // construction, whose order across the partial is unspecified.
    public static KacJson Relaxed => _relaxed ??= new KacJson(Escaped(indented: true));

    // The same, not indented, for a document whose unit is the line. JSONL exists so that a grep hands
    // back a complete object, which an indented one spread over several lines never does.
    public static KacJson Line => _line ??= new KacJson(Escaped(indented: false));

    // `Options` is the generator's own name on this partial, so this one is named for what it does.
    private static JsonSerializerOptions Escaped(bool indented) => new(Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = indented
    };
}
