using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace kac.core;

// Every JSON document kac emits is a record serialized through the source generator below. The project
// sets `IsAotCompatible`, so the trim analyzers fail the build on a reflection-based serializer call.

public record ValidateReport(ValidateSummary Summary, IReadOnlyList<ValidateFinding> Findings);

public record ValidateSummary(int Validated, int Templates, int Skipped, int Errors, int Warnings, int Infos);

public record ValidateFinding(string File, int? Line, string Severity, string Check, string Message);

public record ChecksReport(IReadOnlyList<CheckInfo> Checks);

public record CheckInfo(string Check, string Severity, string Summary);

// A consumer reads these instead of cloning the corpus, so their shape is a contract once published.
// `docs/design/export.md` says what moves `FormatVersion` and each `ShapeVersion`.

// Everything that varies between two runs over one commit is confined here, so the rest of the output
// is byte-identical run to run. `Commit` and `Dirty` are null where git could not answer. `Shortcode`,
// `ReposBase` and every `About` field are null where the corpus stated none.
public record ExportManifest(
    int FormatVersion,
    string? Corpus,
    string? Shortcode,
    string? ContentVersion,
    int? MechanismVersion,
    string? Commit,
    bool? Dirty,
    string GeneratedAt,
    ExportAbout About,
    ExportPublishing Publishing,
    string? ReposBase,
    ExportTracker Tracker,
    ExportTracker Framework,
    IReadOnlyList<ExportSource> Sources,
    IReadOnlyList<ExportedType> Types);

// `Area` is null on every target but Azure DevOps. `Base` and `Id` are null together, where the block
// addresses no backlog. `Id` leaves `Area` out: one project is one backlog however many areas divide it.
// `Tracker.cs` states what each one is written from.
public record ExportTracker(string Target, string? Base, string? Area, string? Id);

// Each source carries its producer's own publishing, tracker and repos base: a record of `eng` is read
// at eng's commit, in eng's repository, and filed on eng's backlog.
public record ExportSource(
    string Shortcode,
    string? Corpus,
    string? ContentVersion,
    ExportPublishing Publishing,
    string? ReposBase,
    ExportTracker Tracker);

// Every field is null where the corpus has not said. Nothing is invented from a neighbour's value.
public record ExportAbout(
    string? DisplayName,
    string? Description,
    ExportAuthor? Author,
    string? License);

public record ExportAuthor(string? Name, string? Url);

// `HumanTemplate`, `Base` and `Ref` are null together, where the corpus publishes nowhere or names a
// target nothing builds links for. `PathPrefix` is null where the corpus is its repository.
// `docs/design/export.md` sets out how a client reads a record's source from these.
public record ExportPublishing(
    string Target,
    string? HumanTemplate,
    string? Base,
    string? PathPrefix,
    string? Ref);

// `PartsFile`, `RecordKey`, `PartKey` and `IdKey` are null where the type keeps no parts, and `Parts`
// is then zero. `SeeAlsoKey` is also null where the type declares no such key. `FrameworksFile` is null
// for every type that declares none. `docs/design/export.md` sets out what moves `ShapeVersion`.
public record ExportedType(
    string Type,
    int ShapeVersion,
    int Records,
    int Parts,
    string Dir,
    string? PartsFile,
    string? RecordKey,
    string? PartKey,
    string? IdKey,
    string? SeeAlsoKey,
    IReadOnlyDictionary<string, string> Sections,
    string? FrameworksFile = null);

// `Trust` is null where the type's export declares no `verified` field. A section carried at
// `reference` is null, and a section the record never wrote is absent from the map.
// `Exporter.Absent` decides absence and `Exporter.Value` the shape of a field.
public record ExportRecord(
    string Type,
    string Path,
    string? Trust,
    IReadOnlyDictionary<string, JsonNode?> Fields,
    IReadOnlyDictionary<string, string?> Sections,
    ExportLinks? Links);

public record ExportLinks(string Human);

// The type declares a part line's keys in its own `export.parts.line:` block. `docs/design/export.md`
// says why a line carries a `path` and an `anchor` rather than two resolved links.

// `plugin.json` is not here: it is edited as a DOM, so keys this tool has never heard of survive.

// No timestamp and no commit: the copied export's manifest states both. `docs/cli/bundle.md` says why
// the component set is stated rather than inferred.
public record BundleRecord(
    int BundleVersion,
    string Plugin,
    string? Version,
    string CorpusRoot,
    BundleExport Export,
    IReadOnlyList<BundleIncluded> Included,
    IReadOnlyList<BundleTrimmed> Trimmed);

public record BundleExport(int? FormatVersion, string? Corpus, string? ContentVersion, IReadOnlyList<string> Types);

public record BundleIncluded(string Path, IReadOnlyList<string> Requires, string? Note);

public record BundleTrimmed(string Path, IReadOnlyList<string> Requires, string Reason);

// `Dist.Root` says why the marketplace is the root and not a sibling.

public record MarketplaceManifest(
    [property: JsonPropertyName("$schema")]
    string Schema,
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
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(BundleRecord))]
[JsonSerializable(typeof(MarketplaceManifest))]
public partial class KacJson : JsonSerializerContext
{
    private static KacJson? s_relaxed;
    private static KacJson? s_line;

    // CLI output with relaxed escaping, so a quote or an em dash reaches the reader as itself. Lazy, so it does not
    // touch `Default` during static construction, whose order across the partial is unspecified.
    public static KacJson Relaxed => s_relaxed ??= new KacJson(Escaped(indented: true));

    // One object per line, so a grep hands back a complete one.
    public static KacJson Line => s_line ??= new KacJson(Escaped(indented: false));

    // `Options` is the generator's own name on this partial, so this one is named for what it does.
    private static JsonSerializerOptions Escaped(bool indented) => new(Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = indented
    };
}
