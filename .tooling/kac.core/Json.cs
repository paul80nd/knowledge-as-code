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

// How a link into the published form is built, and the ref every one of them resolves against.
//
// The two templates are the whole rule: a consumer substitutes the `path` and `anchor` a line carries
// and edits nothing else. The commit is already inside each string, so a citation names the version the
// agent read without a ref ever passing through the agent's hands.
//
// The bases the templates were built from are not carried. They are inside the templates, and a manifest
// stating one address twice is a manifest that can state it two ways.
//
// Both templates are null where the corpus publishes nowhere or names a target nothing builds links for,
// which is the same state the per-record files report by carrying no links.
public record ExportPublishing(string Target, string? HumanTemplate, string? RawTemplate, string? Ref);

// One type this export carries, how much of it there is, and where to find it.
//
// Two counts, named apart. `Records` is how many files sit under `Dir`; `Parts` is how many lines sit in
// `PartsFile`, which for a glossary is the size of the vocabulary rather than the number of glossaries.
// One number could be read as either, and the two differ by an order of magnitude.
//
// `PartsFile` is null for a type whose records hold no addressable parts, so a consumer reads the
// per-record files alone rather than seeking a file that was never written; `Parts` is then zero.
public record ExportedType(string Type, int Records, int Parts, string Dir, string? PartsFile);

// One record, carrying what its type's `export:` block declares and nothing else. `Fields` and
// `Sections` are keyed by what the schema named, so a consumer reading a corpus with a type it does not
// know still gets a document it can walk.
//
// Absent is `null` throughout, here and on every line of the flat file. `Exporter.Absent` is where that
// is decided.
public record ExportRecord(
    string Type,
    string Path,
    IReadOnlyDictionary<string, string?> Fields,
    IReadOnlyDictionary<string, string> Sections,
    ExportLinks? Links);

public record ExportLinks(string Human, string Raw);

// One part on one line of the flat file, self-contained by design. `.tooling/features/export.md` sets out what
// that costs and buys; the fields it explains least obviously are these two.
//
// `Status` and `ReviewBy` live on the record as well. They are repeated because a consumer that grepped
// this file has not opened the record, and a hit without them is a definition with nothing saying its
// glossary is still settling.
//
// `SeeAlso` carries the cross-references as ids. A link's target is stripped out of `Definition` and
// `Not`, so the bracket left behind in the prose leads nowhere on its own.
//
// `Path` and `Anchor` are what the manifest's link templates are substituted with, and they are all a
// line carries of an address. Resolving both links on every line would repeat one host, one path prefix
// and one commit for every part in the corpus — most of the file, and a rewrite of every line whenever
// the commit moves, which buries a changed definition in noise.
public record ExportPartLine(
    string Id,
    string Title,
    string? Definition,
    string? Not,
    IReadOnlyList<string>? SeeAlso,
    string Type,
    string Record,
    string Part,
    string? Status,
    string? ReviewBy,
    string Path,
    string Anchor);

// ---------------------------------------------------------------------------
// Bundle documents
//
// What `kac bundle` writes for itself. `plugin.json` is not here: it is the corpus's own file, edited
// as a DOM so that keys this tool has never heard of survive the round trip.
// ---------------------------------------------------------------------------

// What the assembled plugin holds, and why anything is missing from it.
//
// Two corpora running one plugin name may ship different component sets, which is correct and makes
// "does this plugin do X" unanswerable from outside unless the plugin says. This is where it says.
//
// It carries no timestamp and no commit. The export it was built from is inside the plugin already,
// and its manifest states both — a second clock here would be a second answer to one question.
public record BundleRecord(
    int BundleVersion,
    string Plugin,
    string? Version,
    string CorpusRoot,
    BundleExport Export,
    IReadOnlyList<BundleIncluded> Included,
    IReadOnlyList<BundleTrimmed> Trimmed);

// The export this plugin was assembled around, named so that a reader holding the plugin alone can
// tell which corpus and which shape it has, without walking into the copy to find out.
public record BundleExport(int? FormatVersion, string? Corpus, string? ContentVersion, IReadOnlyList<string> Types);

public record BundleIncluded(string Path, IReadOnlyList<string> Requires, string? Note);

// A component and the reason it was left out. The reason is prose because it is read by a person
// asking why the plugin they installed does less than the one their colleague has.
public record BundleTrimmed(string Path, IReadOnlyList<string> Requires, string Reason);

// ---------------------------------------------------------------------------
// The local marketplace
//
// A marketplace is a directory holding `.claude-plugin/marketplace.json`, and it resolves a plugin's
// source against that directory. This is the minimum that installs and validates: what the plugin is,
// who owns it, and where under the marketplace root it sits.
// ---------------------------------------------------------------------------

public record MarketplaceManifest(
    [property: JsonPropertyName("$schema")] string Schema,
    string Name,
    string Description,
    MarketplaceOwner Owner,
    IReadOnlyList<MarketplacePlugin> Plugins);

public record MarketplaceOwner(string Name);

public record MarketplacePlugin(string Name, string Description, string Source);

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ValidateReport))]
[JsonSerializable(typeof(ChecksReport))]
[JsonSerializable(typeof(ExportManifest))]
[JsonSerializable(typeof(ExportRecord))]
[JsonSerializable(typeof(ExportPartLine))]
[JsonSerializable(typeof(BundleRecord))]
[JsonSerializable(typeof(MarketplaceManifest))]
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
