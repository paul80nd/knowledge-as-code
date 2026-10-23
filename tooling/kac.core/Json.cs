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
// `docs/reference/export.md` says what moves `FormatVersion` and each `ShapeVersion`.

/// <summary>
/// Everything that varies between two runs over one commit is confined here, so the rest of the output is
/// byte-identical run to run. <c>Commit</c> and <c>Dirty</c> are null where git could not answer. <c>Shortcode</c>,
/// <c>ReposBase</c> and every <c>About</c> field are null where the corpus stated none.
/// </summary>
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

/// <summary>
/// <c>Area</c> is null on every target but Azure DevOps. <c>Base</c> and <c>Id</c> are null together, where the block
/// addresses no backlog. <c>Id</c> leaves <c>Area</c> out: one project is one backlog however many areas divide it.
/// <c>Tracker.cs</c> states what each one is written from.
/// </summary>
public record ExportTracker(string Target, string? Base, string? Area, string? Id);

/// <summary>
/// Each source carries its producer's own publishing, tracker and repos base: a record of <c>eng</c> is read at eng's
/// commit, in eng's repository, and filed on eng's backlog.
/// </summary>
public record ExportSource(
    string Shortcode,
    string? Corpus,
    string? ContentVersion,
    ExportPublishing Publishing,
    string? ReposBase,
    ExportTracker Tracker);

/// <summary>Every field is null where the corpus has not said. Nothing is invented from a neighbour's value.</summary>
public record ExportAbout(
    string? DisplayName,
    string? Description,
    ExportAuthor? Author,
    string? License);

public record ExportAuthor(string? Name, string? Url);

/// <summary>
/// <c>HumanTemplate</c>, <c>Base</c> and <c>Ref</c> are null together, where the corpus publishes nowhere or names a
/// target nothing builds links for. <c>PathPrefix</c> is null where the corpus is its repository.
/// <c>docs/reference/export.md</c> sets out how a client reads a record's source from these.
/// </summary>
public record ExportPublishing(
    string Target,
    string? HumanTemplate,
    string? Base,
    string? PathPrefix,
    string? Ref);

/// <summary>
/// <c>PartsFile</c>, <c>RecordKey</c>, <c>PartKey</c> and <c>IdKey</c> are null where the type keeps no parts, and
/// <c>Parts</c> is then zero. <c>SeeAlsoKey</c> is also null where the type declares no such key. <c>FrameworksFile</c>
/// is null for every type that declares none. <c>InForce</c> is null for a type with no <c>status</c> range, and for a
/// type a producer exported before the key existed. <c>docs/reference/export.md</c> sets out what moves
/// <c>ShapeVersion</c>.
/// </summary>
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
    string? FrameworksFile = null,
    string? InForce = null);

/// <summary>
/// <c>Trust</c> is null where the type's export declares no <c>verified</c> field. A section carried at
/// <c>reference</c> is null, and a section the record never wrote is absent from the map. <c>Exporter.Absent</c>
/// decides absence and <c>Exporter.Value</c> the shape of a field.
/// </summary>
public record ExportRecord(
    string Type,
    string Path,
    string? Trust,
    IReadOnlyDictionary<string, JsonNode?> Fields,
    IReadOnlyDictionary<string, string?> Sections,
    ExportLinks? Links);

public record ExportLinks(string Human);

// The type declares a part line's keys in its own `export.parts.line:` block. `docs/reference/export.md`
// says why a line carries a `path` and an `anchor` rather than two resolved links.

// `plugin.json` is not here: it is edited as a DOM, so keys this tool has never heard of survive.

/// <summary>
/// No timestamp and no commit: the copied export's manifest states both. <c>docs/use/cli/bundle.md</c> says why the
/// component set is stated rather than inferred.
/// </summary>
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

    /// <summary>
    /// CLI output with relaxed escaping, so a quote or an em dash reaches the reader as itself. Lazy, so it does not
    /// touch <c>Default</c> during static construction, whose order across the partial is unspecified.
    /// </summary>
    public static KacJson Relaxed => s_relaxed ??= new KacJson(Escaped(indented: true));

    /// <summary>One object per line, so a grep hands back a complete one.</summary>
    public static KacJson Line => s_line ??= new KacJson(Escaped(indented: false));

    /// <summary>
    /// <c>Options</c> is the generator's own name on this partial, so this one is named for what it does.
    /// </summary>
    private static JsonSerializerOptions Escaped(bool indented) => new(Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = indented
    };
}
