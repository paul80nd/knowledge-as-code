using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// The schema model, loaded from .schema/*.yaml. Settled at load and read-only afterwards, which keeps the derived sets
// on TypeSchema in step with the declarations they come from.

public sealed class FieldSpec
{
    public required string Name { get; init; }
    public bool Required { get; init; }
    /// <summary>As written, for the message that quotes it.</summary>
    public string? RequiredWhen { get; init; }
    /// <summary>As parsed, for the check that applies it.</summary>
    public RequiredWhen? RequiredWhenCondition { get; init; }
    /// <summary>One of ValueChecks.FieldTypes.</summary>
    public string Type { get; init; } = "string";
    /// <summary>Each entry, where Type is list: ValueChecks.EntryTypes.</summary>
    public string? Of { get; init; }
    /// <summary>Enum values, resolved.</summary>
    public IReadOnlyList<string>? Values { get; init; }

    /// <summary>
    /// Whether <c>Values</c> is a ranking that a record's dependencies should meet, taken from the enum that declared
    /// it. Only a shared enum declares one, so a field writing its values inline or drawing them from a corpus enum is
    /// never ranked. See <c>EnumSpec</c> and <c>DependencyCriticality</c>.
    /// </summary>
    public bool Ordered { get; init; }

    /// <summary>
    /// Where a value sits in the ranking, <c>0</c> being the most significant. Null where the field declares no
    /// ranking, and where the range excludes the value, which <c>enum</c> reports.
    /// </summary>
    public int? Rank(string value)
    {
        if (!Ordered || Values is null) return null;

        for (var i = 0; i < Values.Count; i++)
            if (string.Equals(Values[i], value, StringComparison.Ordinal))
                return i;

        return null;
    }

    /// <summary>
    /// The one value of <c>values:</c> that says a record is in force: <c>accepted</c> on an ADR, <c>live</c> on a
    /// service, <c>approved</c> on a tool.
    /// </summary>
    public string? InForce { get; init; }

    /// <summary>
    /// The corpus-owned enum this field draws its range from, and null where the schema states the range itself.
    /// <c>Values</c> stays null where the corpus supplied nothing, so this tells that apart from a field with no range.
    /// </summary>
    public string? CorpusEnum { get; init; }

    /// <summary>
    /// The shape of one entry, where a list's entries are objects. The first key declared names the entry:
    /// <c>list-order</c> sorts on it, and a message quotes it.
    /// </summary>
    public IReadOnlyList<FieldSpec>? Entry { get; init; }

    /// <summary>The shared shape this field's object keys come from, and empty where it declares them itself.</summary>
    public string Shape { get; init; } = "";

    public FieldSpec? EntryKey(string name) =>
        Entry?.FirstOrDefault(k => string.Equals(k.Name, name, StringComparison.Ordinal));

    public bool DeclaresId => Type == "id" || (Type == "list" && Of == "id");

    public bool PointsAt(TypeSchema t) => DeclaresId && Refs.Contains(t.Key, StringComparer.Ordinal);

    /// <summary>
    /// The folders an id in this field may belong to. A scalar <c>ref:</c> arrives as a one-entry list.
    /// </summary>
    public IReadOnlyList<string> Refs { get; init; } = [];

    /// <summary>
    /// Whether an id in this field has to reach a part of the record, as a standard's <c>implements:</c> does.
    /// </summary>
    public bool PartRequired { get; init; }

    /// <summary>Field on the target that must point back.</summary>
    public string? Reciprocal { get; init; }
    public string? Pattern { get; init; }
    /// <summary>Pattern compiled, though the message still quotes the source string.</summary>
    public Regex? PatternRegex { get; init; }
    /// <summary>Section whose ids this field must mirror.</summary>
    public string? MirrorsSection { get; init; }

    /// <summary>
    /// The label on the footnote lines whose citations this field must mirror: <c>Covers</c> on a standard's
    /// <c>implements:</c>, read from <c>_**Covers:** [pol-SCRT].EMBED_</c>.
    /// </summary>
    public string? MirrorsCitations { get; init; }

    /// <summary>
    /// Words admitted beside the field's declared type, such as <c>applies-to: [all]</c> and <c>last-rehearsed:
    /// never</c>. A listed value is taken as written and checked no further.
    /// </summary>
    public IReadOnlyList<string> AllowLiteral { get; init; } = [];

    /// <summary>The floor on a list field's length, and null where the schema sets none.</summary>
    public int? MinItems { get; init; }

    /// <summary>
    /// The floor on how many records of the type carry a given value, and null where the schema sets none.
    /// </summary>
    public int? MinRecords { get; init; }

    /// <summary>
    /// Where the field's value comes from, for a field the record never writes, such as <c>sub-path</c> from the
    /// folder. Null for every field an author fills in. <c>Doc.DerivedSources</c> is the vocabulary.
    /// </summary>
    public string? From { get; init; }

    /// <summary>
    /// Why this declaration could not be read, and null where it could. Carried rather than thrown, so one unreadable
    /// field is one finding and the rest of the schema still loads.
    /// </summary>
    public string? Problem { get; init; }

    /// <summary>
    /// <c>Description</c> is what the generated Metadata table renders. <c>Notes</c> is the longer reasoning. A field
    /// with only <c>Notes</c> falls back to them.
    /// </summary>
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public string? TableText => string.IsNullOrEmpty(Description) ? Notes : Description;

    public bool IsLiteral(string? value) =>
        value is not null && AllowLiteral.Contains(value, StringComparer.Ordinal);
}

/// <summary>
/// Where a type keeps the citable parts of a record, and what holds them to shape. A policy keeps them as table rows
/// with the id written beside each. A glossary keeps them as headings, and the id is the heading's anchor. The
/// declarations from <c>Columns</c> down apply to the table source only.
/// </summary>
public sealed class PartSpec(string source, string idPattern, List<string> binding, List<string> advisory)
{
    /// <summary>What one part is called in a message: a clause on a policy, a term in a glossary.</summary>
    public string Noun { get; init; } = "part";

    public string Source { get; } = source;
    public string Section { get; init; } = "";

    /// <summary>
    /// The bold labels a part's optional further blocks open with: <c>Also</c>, <c>Avoid</c> and <c>Not</c> on a
    /// glossary term.
    /// </summary>
    public IReadOnlyList<string> Asides { get; init; } = [];

    /// <summary>The heading level a part is written at, for the heading source.</summary>
    public int Level { get; init; } = 3;

    /// <summary>The table's headers, in order. The first two are read by position as the id and the part.</summary>
    public IReadOnlyList<string> Columns { get; init; } = ["Id", "Clause"];

    public string IdPattern { get; } = idPattern;
    public Regex? IdPatternRegex { get; } = Schema.CompilePattern(idPattern);

    /// <summary>Written bold: these oblige.</summary>
    public IReadOnlyList<string> Binding { get; } = binding;
    /// <summary>Written plain: these recommend.</summary>
    public IReadOnlyList<string> Advisory { get; } = advisory;

    /// <summary>
    /// The order rows must appear in: binding levels before advisory ones, each as the type declares it.
    /// </summary>
    private readonly List<string> _levels = [.. binding, .. advisory];
    public IReadOnlyList<string> Levels => _levels;

    /// <summary>Where a modal sits in that order, or -1 for a modal the type does not declare.</summary>
    public int Rank(string modal) => _levels.IndexOf(modal);

    /// <summary>Longest first, so "MUST NOT" is recognised before the "MUST" that prefixes it.</summary>
    public IReadOnlyList<string> ModalsLongestFirst { get; } =
        [.. binding.Concat(advisory).OrderByDescending(m => m.Length)];

    /// <summary>
    /// The modal a part opens with, or null where it opens with none. <c>clause-order</c> and the exporter both read
    /// it, so the two cannot rank a clause differently.
    /// </summary>
    public string? Modal(string text) =>
        ModalsLongestFirst.FirstOrDefault(m => text.StartsWith(m, StringComparison.Ordinal));

    /// <summary>
    /// The modal a stretch of text names anywhere in it, or null where it names none. A word the modal only prefixes
    /// does not count, so <c>MUST</c> inside <c>MUSTER</c> reads as nothing.
    /// </summary>
    public string? ModalNamed(string text) => ModalsLongestFirst.FirstOrDefault(m => Names(text, m));

    private static bool Names(string text, string modal)
    {
        for (var i = text.IndexOf(modal, StringComparison.Ordinal);
             i >= 0;
             i = text.IndexOf(modal, i + 1, StringComparison.Ordinal))
        {
            var opens = i == 0 || !char.IsLetter(text[i - 1]);
            var closes = i + modal.Length == text.Length || !char.IsLetter(text[i + modal.Length]);
            if (opens && closes) return true;
        }

        return false;
    }

    /// <summary>
    /// The fragment a link into a part carries. A heading's slug is its id and its anchor alike. A table row has no
    /// fragment a renderer emits, so a link into a clause lands on the section holding the table.
    /// </summary>
    public string Anchor(string partId) => Source == Table ? Publishing.Anchor(Section) : partId;

    /// <summary>The sources an extractor exists for. <c>schema-dispatch</c> reads them from here.</summary>
    public const string Table = "table";
    public const string Headings = "headings";
    public static readonly IReadOnlyList<string> Sources = [Table, Headings];
}

/// <summary>
/// What a record of this type contributes to a published export, and how much of each piece travels. A type declaring
/// no block contributes nothing. <c>docs/reference/export.md</c> says what each declaration promises.
/// </summary>
public sealed class ExportSpec
{
    /// <summary>
    /// The shape a consumer holds this type's export to, moved by hand. <c>docs/reference/export.md</c> says what moves
    /// it.
    /// </summary>
    public int Version { get; init; }

    /// <summary>The frontmatter a record contributes, in the order an export writes it.</summary>
    public IReadOnlyList<string> Fields { get; init; } = [];

    /// <summary>The sections that travel, each with the fidelity it travels at, in declared order.</summary>
    public IReadOnlyList<(string Section, string Fidelity)> Sections { get; init; } = [];

    /// <summary>The fidelity a record's parts travel at, and empty where none do.</summary>
    public string Parts { get; init; } = "";

    /// <summary>
    /// Whether the type writes <c>export.parts:</c> at all. It tells an absent block from a malformed one, which both
    /// leave the fidelity and the line empty.
    /// </summary>
    public bool PartsDeclared { get; init; }

    /// <summary>The file a type's framework references travel in, and empty where none do.</summary>
    public string Frameworks { get; init; } = "";

    /// <summary>
    /// The keys of one part line, in the order the line writes them, each with the source filling it.
    /// <c>PartLineSource</c> is the vocabulary of sources.
    /// </summary>
    public IReadOnlyList<(string Key, string Source)> Line { get; init; } = [];

    /// <summary>
    /// How much of a piece travels. <c>docs/reference/export.md</c> says what each one promises a consumer.
    /// </summary>
    public const string Full = "full";
    public const string Summary = "summary";
    public const string Reference = "reference";

    /// <summary>What a section and a part line may travel at. <c>schema-dispatch</c> reads both from here.</summary>
    public static readonly IReadOnlyList<string> Carried = [Full, Summary, Reference];
    public static readonly IReadOnlyList<string> CarriedByParts = [Full];
}

/// <summary>
/// Where one key of a part line takes its value from. <c>schema-dispatch</c> reads the vocabulary from here.
/// <c>part.lead</c>, <c>part.bullets</c> and <c>part.aside.&lt;Label&gt;</c> read a part's body, which only the heading
/// source has, so <c>SchemaChecks</c> refuses them on a table-sourced type.
/// </summary>
public static class PartLineSource
{
    /// <summary><c>&lt;record-id&gt;.&lt;part-id&gt;</c>, the whole address.</summary>
    public const string PartId = "part.id";
    /// <summary>The part id alone.</summary>
    public const string PartKey = "part.key";
    /// <summary>The heading as written, or the second cell flattened.</summary>
    public const string PartText = "part.text";
    /// <summary>The body's first block.</summary>
    public const string PartLead = "part.lead";
    /// <summary>The modal the part opens with.</summary>
    public const string PartLevel = "part.level";
    /// <summary>Each bullet of the body's first list, with its modal.</summary>
    public const string PartBullets = "part.bullets";
    public const string PartSeeAlso = "part.see-also";
    /// <summary>The fragment the part resolves at.</summary>
    public const string PartAnchor = "part.anchor";
    /// <summary>The part's URL, built for the corpus's publishing target. Null where the corpus has no address.</summary>
    public const string PartLinks = "part.links";
    public const string RecordId = "record.id";
    public const string RecordType = "record.type";
    public const string RecordPath = "record.path";

    /// <summary>
    /// What a source naming a field or a column opens with. The remainder is the name, and an empty one is a source
    /// naming nothing, which <c>SchemaChecks</c> reports.
    /// </summary>
    public const string FrontPrefix = "front.";
    public const string ColumnPrefix = "column.";

    /// <summary>
    /// A labelled footnote closing a part, named by the label a field of this type reconciles against. The remainder is
    /// that label, so <c>part.citations.Covers</c> carries what the <c>Covers</c> line gathers.
    /// </summary>
    public const string CitationPrefix = "part.citations.";

    /// <summary>
    /// A bold-labelled block inside a part, named by a label the type's <c>parts.asides:</c> declares. The remainder is
    /// that label, so <c>part.aside.Not</c> carries the <c>**Not:**</c> block without its label.
    /// </summary>
    public const string AsidePrefix = "part.aside.";

    /// <summary>The sources naming one thing each, so the prefixed families are tested separately.</summary>
    public static readonly IReadOnlyList<string> Fixed =
    [
        PartId, PartKey, PartText, PartLead, PartLevel, PartBullets, PartSeeAlso, PartAnchor, PartLinks,
        RecordId, RecordType, RecordPath
    ];

    /// <summary>
    /// The name a prefixed source carries, or null where the source is not of that family. Empty after the prefix
    /// returns an empty string, which is a name the caller reports rather than one it resolves.
    /// </summary>
    public static string? Argument(string source, string prefix) =>
        source.StartsWith(prefix, StringComparison.Ordinal) ? source[prefix.Length..] : null;
}

/// <summary>
/// A field's <c>required-when:</c> condition: a test against one other field of the same document, such as <c>status ==
/// accepted</c> or <c>personal-data in [personal, special-category]</c>. Absence answers false, including for
/// <c>!=</c>, because <c>required-field</c> already reports the missing field.
/// </summary>
public sealed record RequiredWhen(string Field, bool Negated, IReadOnlyList<string> Values)
{
    public bool Holds(string? actual) =>
        actual is not null && Values.Contains(actual, StringComparer.Ordinal) != Negated;
}

/// <summary>
/// One entry of a type's <c>rules:</c> block. An absent <c>severity:</c> means declared but not enforced, not a default
/// level.
/// </summary>
public sealed record RuleSpec
{
    public required RuleId Id { get; init; }

    /// <summary>
    /// The rule in the schema's own words, whitespace folded so it can be rendered into a table cell.
    /// </summary>
    public string? Description { get; init; }

    public Sev? Severity { get; init; }

    /// <summary>
    /// The condition a document must satisfy, compiled. Absent on a rule implemented as a class in <c>Rules/</c>, and
    /// on a rule nothing enforces yet.
    /// </summary>
    public string? Expr { get; init; }
    public Expr? Compiled { get; init; }

    /// <summary>What the author is told when the expression fails.</summary>
    public string? Message { get; init; }

    /// <summary>
    /// Compiling an expression is refused without a severity and a message, so a compiled rule always has both.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Compiled))]
    [MemberNotNullWhen(true, nameof(Message))]
    public bool IsExpression => Compiled is not null;

    /// <summary>
    /// The config keys the entry declares, from those some rule class <c>Reads</c>, each with its value as written. A
    /// rule class reads its own keys from here. <c>SchemaChecks</c> reports a key the rule's own class does not read.
    /// </summary>
    public IReadOnlyDictionary<string, YamlNode> Config { get; init; } = new Dictionary<string, YamlNode>();

    /// <summary>The line the entry starts on in its <c>.schema/</c> file, and null for a rule built in code.</summary>
    public int? Line { get; init; }

    /// <summary>
    /// Why this rule could not be read, and null where it could. <c>SchemaChecks</c> reports it against the file.
    /// </summary>
    public string? Problem { get; init; }
}

public sealed class TypeSchema
{
    /// <summary>
    /// The base name of the type's schema file: the folder its records live in, and what <c>ref:</c> names.
    /// </summary>
    public string Key { get; init; } = "";

    public string TypeName { get; init; } = "";
    public string Label { get; init; } = "";
    public string LabelPlural { get; init; } = "";
    public string Folder { get; init; } = "";
    public string Page { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Lifecycle { get; init; } = "";

    /// <summary>
    /// What this type is, in one line. Rendered into the taxonomy's decision table and the corpus index.
    /// </summary>
    public string Summary { get; init; } = "";
    public string GoesHere { get; init; } = "";

    /// <summary>
    /// The paragraph beneath the one-liner, as the framework defines the type. Examples belong on the type's own page.
    /// </summary>
    public string Detail { get; init; } = "";

    /// <summary>
    /// The close calls, each against one other type, declared once on the type a reader most likely reached for. That
    /// type titles the heading.
    /// </summary>
    public IReadOnlyList<(string Other, string Text)> Versus { get; init; } = [];

    /// <summary>Where the type's name came from, and where it parts company with whatever lent it.</summary>
    public LineageSpec? Lineage { get; init; }

    /// <summary>
    /// Where this type's name already means something else to a reader from another framework, and empty for most
    /// types. A blank line separates paragraphs.
    /// </summary>
    public string Collision { get; init; } = "";
    public string IdPrefix { get; init; } = "";
    public string IdStyle { get; init; } = "";
    public WidthSpec IdWidth { get; init; } = new(4, 4);
    public string? FilenamePattern { get; init; }
    /// <summary>FilenamePattern compiled, though the message quotes the source.</summary>
    public Regex? FilenameRegex { get; init; }

    /// <summary>
    /// Whether the filename opens with the id's discriminator, as <c>0007-decide-x.md</c> does. Where false, nothing
    /// reads the head of a filename as an id.
    /// </summary>
    public bool FilenameCarriesId { get; init; } = true;
    public int SlugMax { get; init; } = 30;
    public IReadOnlyList<string> FieldOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> RequiredSections { get; init; } = [];
    public IReadOnlyList<string> OptionalSections { get; init; } = [];
    public IReadOnlyList<string> IndexColumns { get; init; } = [];

    /// <summary>
    /// The columns the index sorts on, in order of precedence. A scalar <c>sort:</c> arrives as a one-entry list.
    /// </summary>
    public IReadOnlyList<string> IndexSort { get; init; } = [];

    /// <summary><c>ascending</c> (the default) or <c>descending</c>, applied to the whole sort.</summary>
    public string IndexOrder { get; init; } = "";
    public PartSpec? Parts { get; init; }
    public ExportSpec? Export { get; init; }

    /// <summary>
    /// The two above, for a caller that has already established the type declares them. Each throws where it does not.
    /// </summary>
    public ExportSpec DeclaredExport => Export
                                        ?? throw new InvalidOperationException(
                                            $"type '{TypeName}' declares no export: to read.");

    public PartSpec DeclaredParts => Parts
                                     ?? throw new InvalidOperationException(
                                         $"type '{TypeName}' declares no parts: to read.");

    public IReadOnlyList<RuleSpec> Rules { get; init; } = [];

    // Derived at load from the declarations above by the Derive* helpers. A hand-built TypeSchema leaves them empty,
    // which the generator's tests rely on.

    /// <summary>Every frontmatter key a document of this type may carry.</summary>
    public IReadOnlySet<string> KnownKeys { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Every pair of keys whose relative order the schema fixes.</summary>
    public IReadOnlyList<(string Before, string After)> KeyOrderEdges { get; init; } = [];

    /// <summary>
    /// Every field a document of this type is judged against, in the order the schema declares them, with each type
    /// override already resolved against the universal field it refines.
    /// </summary>
    public IReadOnlyList<FieldSpec> DeclaredFields { get; init; } = [];

    /// <summary>How one document of this type is named in generated prose: "Policy", "ADR", "NFR".</summary>
    public string DisplayName =>
        !string.IsNullOrEmpty(Label) ? Label
        : !string.IsNullOrEmpty(TypeName) ? char.ToUpperInvariant(TypeName[0]) + TypeName[1..]
        : IdPrefix.ToUpperInvariant();

    /// <summary>
    /// How the collection is named: "ADRs", "Policies". Nothing derives a plural, so <c>label-plural:</c> is required
    /// where <c>label:</c> is not.
    /// </summary>
    public string PluralName => !string.IsNullOrEmpty(LabelPlural) ? LabelPlural : DisplayName;

    public bool HasRule(RuleId id) => Rules.Any(r => r.Id == id);

    public bool AnyField(Func<FieldSpec, bool> predicate) => Fields.Values.Any(predicate);

    /// <summary>
    /// The same question of the keys inside an object list's entries, to any depth. Apart from <c>AnyField</c>, because
    /// <c>reciprocal</c>, <c>mirrors-section</c> and <c>ref</c> read top-level fields only.
    /// </summary>
    public bool AnyEntryKey(Func<FieldSpec, bool> predicate) =>
        Fields.Values.Any(f => Nested(f).Any(predicate));

    private static IEnumerable<FieldSpec> Nested(FieldSpec field) =>
        (field.Entry ?? []).SelectMany(key => new[] { key }.Concat(Nested(key)));

    /// <summary>The universal fields, the type's own, and the reserved keys the publishing platform adds.</summary>
    public static IReadOnlySet<string> DeriveKnownKeys(
        IEnumerable<string> universalOrder, IEnumerable<string> fieldOrder, IEnumerable<string> reserved) =>
        universalOrder.Concat(fieldOrder).Concat(reserved).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Every pair within each declared chain is a constraint, not just neighbours, so an absent key does not drop the
    /// constraint between the keys either side of it.
    /// </summary>
    public static IReadOnlyList<(string Before, string After)> DeriveKeyOrderEdges(
        IReadOnlyList<string> universalOrder, IReadOnlyList<string> fieldOrder)
    {
        // A set while building, so a pair both chains declare is reported once.
        var edges = new HashSet<(string, string)>();
        foreach (var chain in new[] { universalOrder, fieldOrder })
            for (var i = 0; i < chain.Count; i++)
            for (var j = i + 1; j < chain.Count; j++)
                edges.Add((chain[i], chain[j]));
        return [.. edges];
    }
}

/// <summary>
/// A key a schema file carries that the loader never asks for. Held rather than thrown, so one is one finding.
/// </summary>
public sealed record UnreadKey(string File, string Where, string Key);

/// <summary>
/// A mapping the loader is reading, which remembers what it was asked for. The keys a level admits are the keys the
/// loader reads, and <c>notes:</c> at every level.
/// </summary>
internal sealed class Level(YamlNode? node, string where)
{
    private readonly HashSet<string> _asked = new(StringComparer.Ordinal);

    public string Where { get; } = where;

    /// <summary>
    /// Whether the block is there at all. A type declares its parts, and its export, by carrying the block.
    /// </summary>
    public bool Present => node is YamlMappingNode;

    public YamlNode? Get(string key)
    {
        _asked.Add(key);
        return Yaml.Get(node, key);
    }

    public IEnumerable<string> Unread() => Yaml.Map(node)
        .Select(entry => entry.Item1)
        .Where(key => key != Schema.Commentary && !_asked.Contains(key));
}

/// <summary>
/// The mappings read from one schema file, in the order they were opened, so unread keys report in reading order.
/// </summary>
internal sealed class KeyReader(string file)
{
    private readonly List<Level> _levels = [];

    public Level At(YamlNode? node, string where)
    {
        var level = new Level(node, where);
        _levels.Add(level);
        return level;
    }

    public IEnumerable<UnreadKey> Unread() =>
        _levels.SelectMany(l => l.Unread().Select(key => new UnreadKey(file, l.Where, key)));
}

/// <summary>One shared enum: its values, and whether they rank. The first value is the most significant.</summary>
public sealed record EnumSpec(IReadOnlyList<string> Values, bool Ordered);

public sealed record TierSpec(string Name, string Label, string Behaviour, string Note);

/// <summary>
/// A type's prior art, what the framework took, and where it parts company; a type with no useful ancestor says so in
/// <c>PriorArt</c> and leaves the others empty. Links are written inline, because the block this renders into cannot
/// see the definitions at the foot of its page.
/// </summary>
public sealed record LineageSpec(string PriorArt, string Alignment, string Divergence);

/// <summary>
/// How long a <c>numbered</c> or <c>mnemonic</c> discriminator may be. A scalar <c>width:</c> sets both ends. A
/// <c>min:</c>/<c>max:</c> mapping opens a span, which <c>SchemaChecks</c> refuses on a type whose filename carries the
/// id.
/// </summary>
public sealed record WidthSpec(int Min, int Max)
{
    public bool Admits(int length) => length >= Min && length <= Max;

    public override string ToString() => Min == Max ? $"{Min}" : $"{Min} to {Max}";
}

public sealed partial class Schema
{
    /// <summary>The one key admitted at every level and required at none. See Level.</summary>
    public const string Commentary = "notes";

    /// <summary>
    /// The tiers as <c>_tiers.yaml</c> declares them, in the order every generated list of types is grouped by.
    /// <c>SchemaChecks</c> holds them against the universal <c>tier</c> field.
    /// </summary>
    public IReadOnlyList<TierSpec> Tiers { get; init; } = [];

    /// <summary>
    /// Every check the validator can emit, as <c>_checks.yaml</c> declares it. <c>SchemaChecks</c> holds the catalogue
    /// against the code.
    /// </summary>
    public IReadOnlyList<CheckDef> Checks { get; init; } = [];

    public IReadOnlyList<string> UniversalOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Universal { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> Reserved { get; init; } = [];

    public IReadOnlyDictionary<string, EnumSpec> Enums { get; init; } =
        new Dictionary<string, EnumSpec>();

    public IReadOnlyDictionary<string, TypeSchema> ByFolder { get; init; } = new Dictionary<string, TypeSchema>();

    /// <summary>
    /// The prefix every type's ids open with. Empty prefixes are dropped, because one would admit every code span
    /// carrying a dot.
    /// </summary>
    public IReadOnlySet<string> IdPrefixes => field ??=
        ByFolder.Values.Select(t => t.IdPrefix).Where(p => p.Length > 0).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Every key these files carry that the loader never asks for, in the order a reader would meet them.
    /// </summary>
    public IReadOnlyList<UnreadKey> UnreadKeys { get; private init; } = [];

    /// <summary>The shared field declarations, reserved keys and enums every type is read against.</summary>
    private sealed record UniversalLayer(
        IReadOnlyList<string> Order,
        IReadOnlyDictionary<string, FieldSpec> Fields,
        IReadOnlyList<string> Reserved,
        IReadOnlyDictionary<string, EnumSpec> Enums,
        IReadOnlyDictionary<string, IReadOnlyList<FieldSpec>> Shapes);

    /// <summary>
    /// The nearest folder at or above the corpus root holding a <c>.schema/</c>, and null where none does.
    /// </summary>
    public static string? FindRoot(string corpusRoot)
    {
        var dir = new DirectoryInfo(corpusRoot);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".schema")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }

    /// <summary>
    /// The schema at or above <c>root</c>, falling back to <c>root</c> itself so a missing schema fails on the file it
    /// cannot open. <c>corpusEnums</c> is what the corpus supplied for fields drawing their range from <c>$corpus.</c>.
    /// </summary>
    public static Schema LoadNearest(string root,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? corpusEnums = null) =>
        Load(FindRoot(root) ?? root, corpusEnums);

    public static Schema Load(string root,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? corpusEnums = null)
    {
        var dir = Path.Combine(root, ".schema");
        // Read as the file carries it, where the corpus side reads through `Files.ReadLf`. The schema
        // needs no normalising of its own, because YAML normalises a line break to `\n` on input. So a
        // `.schema/` checked out with CRLF endings reaches every value as LF.
        return Load(Directory.GetFiles(dir, "*.yaml")
            .ToDictionary(f => Path.GetFileName(f), File.ReadAllText, StringComparer.Ordinal), corpusEnums);
    }

    /// <summary>
    /// The schema's files by name, already read. Walked in ordinal order, because a map has none. A missing name reads
    /// as an empty document, so a missing shared block is reported as findings.
    /// </summary>
    public static Schema Load(IReadOnlyDictionary<string, string> files,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? corpusEnums = null)
    {
        corpusEnums ??= new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var unread = new List<UnreadKey>();
        YamlNode Read(string name) => Yaml.Load(files.GetValueOrDefault(name, ""));

        var enumKeys = new KeyReader(".schema/_enums.yaml");
        var enumsRoot = enumKeys.At(Read("_enums.yaml"), TheFile);
        var enums = new Dictionary<string, EnumSpec>();
        foreach (var (name, node) in Yaml.Map(enumsRoot.Get("enums")))
        {
            var declared = enumKeys.At(node, $"enum '{name}'");
            enums[name] = new EnumSpec(Yaml.StrList(declared.Get("values")), Yaml.Bool(declared.Get("ordered")));
        }
        unread.AddRange(enumKeys.Unread());

        // After the enums, because a shape's keys are fields and one of them may draw its values from
        // `$enums.`. Before the universal fields, because either of those may name a shape.
        var shapeKeys = new KeyReader(".schema/_shapes.yaml");
        var shapesRoot = shapeKeys.At(Read("_shapes.yaml"), TheFile);
        var shapes = new Dictionary<string, IReadOnlyList<FieldSpec>>(StringComparer.Ordinal);
        foreach (var (name, node) in Yaml.Map(shapesRoot.Get("shapes")))
        {
            var shape = shapeKeys.At(node, $"shape '{name}'");
            var keys = new List<FieldSpec>();
            foreach (var (key, decl) in Yaml.Map(shape.Get("entry")))
                keys.Add(ParseField(shapeKeys, shapeKeys.At(decl, $"shape '{name}' key '{key}'"), key,
                    enums, shapes, corpusEnums));
            shapes[name] = keys;
        }

        unread.AddRange(shapeKeys.Unread());

        var tierKeys = new KeyReader(".schema/_tiers.yaml");
        var tiersRoot = tierKeys.At(Read("_tiers.yaml"), TheFile);
        var tiers = new List<TierSpec>();
        foreach (var (name, node) in Yaml.Map(tiersRoot.Get("tiers")))
        {
            var tier = tierKeys.At(node, $"tier '{name}'");
            tiers.Add(new TierSpec(name, Yaml.Str(tier.Get("label")) ?? "",
                Yaml.Str(tier.Get("behaviour"))?.Trim() ?? "", Yaml.Str(tier.Get("note"))?.Trim() ?? ""));
        }

        unread.AddRange(tierKeys.Unread());

        var checkKeys = new KeyReader(".schema/_checks.yaml");
        var checksRoot = checkKeys.At(Read("_checks.yaml"), TheFile);
        var checks = new List<CheckDef>();
        foreach (var (id, node) in Yaml.Map(checksRoot.Get("checks")))
        {
            var check = checkKeys.At(node, $"check '{id}'");
            checks.Add(new CheckDef(new CheckId(id),
                // Error unless the entry says otherwise.
                Yaml.Str(check.Get("severity")) switch
                {
                    "warning" => Sev.Warning,
                    "info" => Sev.Info,
                    _ => Sev.Error
                },
                Yaml.Str(check.Get("description"))?.Trim() ?? "",
                Yaml.Str(check.Get("notes"))?.Trim() ?? "",
                Yaml.Bool(check.Get("on-type-page"), true)));
        }

        unread.AddRange(checkKeys.Unread());

        var universalKeys = new KeyReader(".schema/_universal.yaml");
        var uni = universalKeys.At(Read("_universal.yaml"), TheFile);
        var universalOrder = new List<string>();
        var universal = new Dictionary<string, FieldSpec>();
        foreach (var (name, node) in Yaml.Map(uni.Get("fields")))
        {
            universalOrder.Add(name);
            universal[name] = ParseField(universalKeys, universalKeys.At(node, $"field '{name}'"), name,
                enums, shapes, corpusEnums);
        }

        var layer = new UniversalLayer(universalOrder, universal, Yaml.StrList(uni.Get("reserved")), enums,
            shapes);
        unread.AddRange(universalKeys.Unread());

        var byFolder = new Dictionary<string, TypeSchema>();
        foreach (var name in files.Keys.Where(n => n.EndsWith(".yaml", StringComparison.Ordinal))
                     .OrderBy(n => n, StringComparer.Ordinal))
        {
            var baseName = Path.GetFileNameWithoutExtension(name);
            // The shared blocks, which declare no type.
            if (baseName.StartsWith('_')) continue;

            var keys = new KeyReader($".schema/{baseName}.yaml");
            byFolder[baseName] = ParseType(keys.At(Read(name), TheFile), keys, layer, baseName, corpusEnums);
            unread.AddRange(keys.Unread());
        }

        return new Schema
        {
            Tiers = tiers,
            Checks = checks,
            UniversalOrder = layer.Order,
            Universal = layer.Fields,
            Reserved = layer.Reserved,
            Enums = layer.Enums,
            ByFolder = byFolder,
            UnreadKeys = unread
        };
    }

    /// <summary>How the top level names itself in a finding. The file is already named beside the message.</summary>
    private const string TheFile = "the file";

    private static TypeSchema ParseType(Level root, KeyReader keys, UniversalLayer layer, string key,
        IReadOnlyDictionary<string, IReadOnlyList<string>> corpusEnums)
    {
        var id = keys.At(root.Get("id"), "the 'id' block");
        var fn = keys.At(root.Get("filename"), "the 'filename' block");
        var sections = keys.At(root.Get("sections"), "the 'sections' block");
        var index = keys.At(root.Get("index"), "the 'index' block");

        var fieldOrder = new List<string>();
        var fields = new Dictionary<string, FieldSpec>();
        foreach (var (name, node) in Yaml.Map(root.Get("fields")))
        {
            fieldOrder.Add(name);
            fields[name] = ParseField(keys, keys.At(node, $"field '{name}'"), name, layer.Enums, layer.Shapes,
                corpusEnums);
        }

        var rules = root.Get("rules") is YamlSequenceNode ruleNodes
            ? ruleNodes.Children.Select(node => ParseRule(node, keys)).ToList()
            : [];

        var filenamePattern = Yaml.Str(fn.Get("pattern"));
        var parts = keys.At(root.Get("parts"), "the 'parts' block");

        // The names under `export.sections:` and `export.parts.line:` are headings and the type's own keys, not schema
        // keys, so their mappings are read directly and never opened as a level.
        var export = keys.At(root.Get("export"), "the 'export' block");
        var exportPartsNode = export.Get("parts");
        var exportParts = keys.At(exportPartsNode, "the 'export.parts' block");

        // All three are read whether or not the block is there, so a lineage missing only `prior-art:` reports as a
        // shape problem.
        var lineage = keys.At(root.Get("lineage"), "the 'lineage' block");
        var priorArt = Yaml.Str(lineage.Get("prior-art"))?.Trim() ?? "";
        var alignment = Yaml.Str(lineage.Get("alignment"))?.Trim() ?? "";
        var divergence = Yaml.Str(lineage.Get("divergence"))?.Trim() ?? "";

        return new TypeSchema
        {
            Key = key,
            TypeName = Yaml.Str(root.Get("type")) ?? "",
            Label = Yaml.Str(root.Get("label")) ?? "",
            LabelPlural = Yaml.Str(root.Get("label-plural")) ?? "",
            Folder = Yaml.Str(root.Get("folder")) ?? "",
            Page = Yaml.Str(root.Get("page")) ?? "",
            Tier = Yaml.Str(root.Get("tier")) ?? "",
            Lifecycle = Yaml.Str(root.Get("lifecycle")) ?? "",
            Summary = Yaml.Str(root.Get("summary")) ?? "",
            GoesHere = Yaml.Str(root.Get("goes-here")) ?? "",
            Detail = Yaml.Str(root.Get("detail"))?.Trim() ?? "",
            Versus = [.. Yaml.Map(root.Get("versus")).Select(e => (e.Item1, Yaml.Str(e.Item2)?.Trim() ?? ""))],
            Lineage = priorArt.Length > 0 ? new LineageSpec(priorArt, alignment, divergence) : null,
            Collision = Yaml.Str(root.Get("collision"))?.Trim() ?? "",

            IdPrefix = Yaml.Str(id.Get("prefix")) ?? "",
            IdStyle = Yaml.Str(id.Get("style")) ?? "",
            IdWidth = ParseWidth(id, keys),

            FilenamePattern = filenamePattern,
            FilenameRegex = CompilePattern(filenamePattern),
            FilenameCarriesId = Yaml.Bool(fn.Get("carries-id"), true),
            SlugMax = Yaml.Int(fn.Get("slug-max"), 30),

            FieldOrder = fieldOrder,
            Fields = fields,

            RequiredSections = Yaml.StrList(sections.Get("required")),
            OptionalSections = Yaml.StrList(sections.Get("optional")),

            Parts = parts.Present
                ? new PartSpec(
                    Yaml.Str(parts.Get("source")) ?? "",
                    Yaml.Str(parts.Get("id-pattern")) ?? "",
                    Yaml.StrList(parts.Get("binding")),
                    Yaml.StrList(parts.Get("advisory")))
                {
                    Noun = Yaml.Str(parts.Get("noun")) ?? "part",
                    Section = Yaml.Str(parts.Get("section")) ?? "",
                    Asides = Yaml.StrList(parts.Get("asides")),
                    Level = Yaml.Int(parts.Get("level"), 3),
                    Columns = Yaml.StrList(parts.Get("columns")) is { Count: > 0 } cols
                        ? cols
                        : ["Id", "Clause"]
                }
                : null,

            Export = export.Present
                ? new ExportSpec
                {
                    Version = Yaml.Int(export.Get("version"), 0),
                    Fields = Yaml.StrList(export.Get("fields")),
                    Sections =
                    [
                        .. Yaml.Map(export.Get("sections"))
                            .Select(e => (e.Item1, Yaml.Str(e.Item2)?.Trim() ?? ""))
                    ],
                    Frameworks = Yaml.Str(export.Get("frameworks")) ?? "",
                    Parts = Yaml.Str(exportParts.Get("fidelity")) ?? "",
                    PartsDeclared = exportPartsNode is not null,
                    Line =
                    [
                        .. Yaml.Map(exportParts.Get("line"))
                            .Select(e => (e.Item1, Yaml.Str(e.Item2)?.Trim() ?? ""))
                    ]
                }
                : null,

            IndexColumns = Yaml.StrList(index.Get("columns")),
            IndexSort = StrOrList(index.Get("sort")),
            IndexOrder = Yaml.Str(index.Get("order")) ?? "",

            Rules = rules,

            KnownKeys = TypeSchema.DeriveKnownKeys(layer.Order, fieldOrder, layer.Reserved),
            KeyOrderEdges = TypeSchema.DeriveKeyOrderEdges(layer.Order, fieldOrder),
            DeclaredFields = DeriveDeclaredFields(layer, fieldOrder, fields)
        };
    }

    /// <summary>
    /// <c>width: 4</c> and a <c>min:</c>/<c>max:</c> block are one declaration. A block short of one end reads as an
    /// exact width at the end it wrote.
    /// </summary>
    private static WidthSpec ParseWidth(Level id, KeyReader keys)
    {
        var node = id.Get("width");
        if (node is not YamlMappingNode)
        {
            var exact = Yaml.Int(node, 4);
            return new WidthSpec(exact, exact);
        }

        var range = keys.At(node, "the 'id.width' block");
        var min = Yaml.Int(range.Get("min"), 4);
        return new WidthSpec(min, Yaml.Int(range.Get("max"), min));
    }

    /// <summary>
    /// Universal fields first, then the type's own, with a name in both resolved to the type's refinement.
    /// </summary>
    private static IReadOnlyList<FieldSpec> DeriveDeclaredFields(UniversalLayer layer,
        IReadOnlyList<string> fieldOrder, IReadOnlyDictionary<string, FieldSpec> fields) =>
    [
        .. layer.Order.Concat(fieldOrder).Distinct()
            .Select(n => Effective(fields, layer.Fields, n))
            .OfType<FieldSpec>()
    ];

    private static FieldSpec ParseField(KeyReader keys, Level node, string name,
        IReadOnlyDictionary<string, EnumSpec> enums,
        IReadOnlyDictionary<string, IReadOnlyList<FieldSpec>> shapes,
        IReadOnlyDictionary<string, IReadOnlyList<string>> corpusEnums)
    {
        var pattern = Yaml.Str(node.Get("pattern"));
        string? problem = null;

        // `values: $enums.status` draws the range from _enums.yaml, and a sequence declares it inline. `values:
        // $corpus.platform` draws it from the corpus, where an undeclared name is no schema defect:
        // `Validator.CheckCorpusEnums` reports it.
        IReadOnlyList<string>? values = null;
        string? corpusEnum = null;
        var ordered = false;
        switch (node.Get("values"))
        {
            case YamlScalarNode { Value: { } v } when v.StartsWith("$corpus.", StringComparison.Ordinal):
                corpusEnum = v["$corpus.".Length..];
                // An empty list resolves to no range, not to an empty one that would refuse every value.
                values = corpusEnums.TryGetValue(corpusEnum, out var stated) && stated.Count > 0
                    ? stated
                    : null;
                break;
            case YamlScalarNode { Value: { } v } when v.StartsWith("$enums.", StringComparison.Ordinal):
                var enumName = v["$enums.".Length..];
                if (enums.GetValueOrDefault(enumName) is { } shared)
                {
                    values = shared.Values;
                    ordered = shared.Ordered;
                }
                else
                {
                    problem = $"field '{name}' draws its values from '{v}', and _enums.yaml declares no "
                              + $"'{enumName}'.";
                }

                break;
            case YamlSequenceNode seq:
                values = Yaml.StrList(seq);
                break;
        }

        var refs = StrOrList(node.Get("ref"));

        // Parsed here so an unreadable condition is reported against the field. See ParseRequiredWhen.
        var requiredWhen = Yaml.Str(node.Get("required-when"));
        RequiredWhen? condition = null;
        try
        {
            condition = ParseRequiredWhen(name, requiredWhen);
        }
        catch (RuleExprException ex)
        {
            problem ??= ex.Message;
        }

        List<FieldSpec>? entry = null;
        foreach (var (key, decl) in Yaml.Map(node.Get("entry")))
        {
            entry ??= [];
            entry.Add(ParseField(keys, keys.At(decl, $"field '{name}' entry key '{key}'"), key, enums, shapes,
                corpusEnums));
        }

        var shape = Yaml.Str(node.Get("shape"));
        if (shape is { Length: > 0 })
        {
            if (entry is not null)
                problem ??= $"field '{name}' takes its shape from '{shape}' and declares an 'entry:' "
                            + "block as well. A shape is taken whole, so drop one of the two.";
            else if (shapes.GetValueOrDefault(shape) is { } declared) entry = [.. declared];
            else
                problem ??= $"field '{name}' takes its shape from '{shape}', and _shapes.yaml declares "
                            + "no such shape.";
        }

        return new FieldSpec
        {
            Name = name,
            Required = Yaml.Bool(node.Get("required")),
            RequiredWhen = requiredWhen,
            RequiredWhenCondition = condition,
            Shape = shape ?? "",
            Type = Yaml.Str(node.Get("type")) ?? "string",
            Of = Yaml.Str(node.Get("of")),
            Entry = entry,
            Values = values,
            Ordered = ordered,
            InForce = Yaml.Str(node.Get("in-force")),
            CorpusEnum = corpusEnum,
            Refs = refs,
            PartRequired = Yaml.Bool(node.Get("part-required")),
            Reciprocal = Yaml.Str(node.Get("reciprocal")),
            Pattern = pattern,
            PatternRegex = CompilePattern(pattern),
            MirrorsSection = Yaml.Str(node.Get("mirrors-section")),
            MirrorsCitations = Yaml.Str(node.Get("mirrors-citations")),
            From = Yaml.Str(node.Get("from")),
            AllowLiteral = Yaml.StrList(node.Get("allow-literal")),
            MinItems = Yaml.NullableInt(node.Get("min-items")),
            MinRecords = Yaml.NullableInt(node.Get("min-records")),
            Description = Collapse(Yaml.Str(node.Get("description"))),
            Notes = Collapse(Yaml.Str(node.Get("notes"))),
            Problem = problem
        };
    }

    /// <summary>
    /// An unrecognised severity reads as absent, leaving the rule declared but not enforced. A rule with an
    /// <c>expr:</c> must compile and carry a severity and a message. A failure is recorded on the rule for
    /// <c>SchemaChecks</c>, and the rule loads inert.
    /// </summary>
    private static RuleSpec ParseRule(YamlNode node, KeyReader keys)
    {
        var rule = keys.At(node, $"rule '{Yaml.Str(Yaml.Get(node, "id")) ?? ""}'");
        var id = Yaml.Str(rule.Get("id")) ?? "";
        var source = Yaml.Str(rule.Get("expr"));
        var severity = Yaml.Str(rule.Get("severity")) switch
        {
            "error" => Sev.Error,
            "warning" => Sev.Warning,
            _ => (Sev?)null
        };
        var message = Collapse(Yaml.Str(rule.Get("message")));

        Expr? compiled = null;
        string? problem = null;
        if (source is not null)
        {
            if (severity is null)
                problem = $"rule '{id}' has an expr but no severity. Say whether it fails "
                          + "a build or advises an author.";
            else if (string.IsNullOrEmpty(message))
                problem = $"rule '{id}' has an expr but no message: an author has to be "
                          + "told what to do about it.";
            else
                try
                {
                    compiled = RuleExpr.Compile(source);
                }
                catch (RuleExprException ex)
                {
                    problem = $"rule '{id}': {ex.Message}";
                }
        }

        var config = new Dictionary<string, YamlNode>(StringComparer.Ordinal);
        foreach (var key in CheckCatalogue.ReadByRules())
            if (rule.Get(key) is { } value)
                config[key] = value;

        return new RuleSpec
        {
            Id = new RuleId(id),
            Description = Collapse(Yaml.Str(rule.Get("description"))),
            Severity = severity,
            Expr = source,
            Compiled = compiled,
            Message = message,
            Problem = problem,
            Config = config,
            Line = Yaml.LineOf(node, 0)
        };
    }

    /// <summary>
    /// A condition outside the vocabulary throws, and ParseField catches it against the field. Read as absent, it would
    /// leave the field silently never required.
    /// </summary>
    public static RequiredWhen? ParseRequiredWhen(string field, string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition)) return null;

        var inList = InListRegex().Match(condition);
        if (inList.Success)
            return new RequiredWhen(inList.Groups[1].Value,
                false,
                [.. inList.Groups[2].Value.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0)]);

        foreach (var (op, negated) in new[] { ("!=", true), ("==", false) })
        {
            var parts = condition.Split(op, 2);
            if (parts.Length != 2) continue;
            return new RequiredWhen(parts[0].Trim(), negated, [parts[1].Trim()]);
        }

        throw new RuleExprException(
            $"field '{field}' has a required-when the schema cannot read: '{condition}'. "
            + "Write it as 'other == value', 'other != value', or 'other in [a, b]'.");
    }

    /// <summary>The field a name resolves to for a type: its own declaration, otherwise the universal one.</summary>
    private static FieldSpec? Effective(IReadOnlyDictionary<string, FieldSpec> typeFields,
        IReadOnlyDictionary<string, FieldSpec> universal, string name) =>
        typeFields.TryGetValue(name, out var tf) ? tf : universal.GetValueOrDefault(name);

    public FieldSpec? EffectiveField(TypeSchema t, string name) => Effective(t.Fields, Universal, name);

    /// <summary>For a caller holding a name it knows the type declares. Throws otherwise.</summary>
    public FieldSpec DeclaredField(TypeSchema t, string name) => EffectiveField(t, name)
                                                                 ?? throw new InvalidOperationException(
                                                                     $"type '{t.TypeName}' declares no field '{name}'.");

    /// <summary>
    /// A key written as one value or a list. <c>Yaml.Str</c> returns null for a sequence, so reading such a key with it
    /// drops every list-form declaration.
    /// </summary>
    private static IReadOnlyList<string> StrOrList(YamlNode? node) => node is YamlSequenceNode
        ? Yaml.StrList(node)
        : Yaml.Str(node) is { } single
            ? [single]
            : [];

    internal static Regex? CompilePattern(string? pattern) =>
        string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.CultureInvariant);

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [GeneratedRegex(@"^\s*(\S+)\s+in\s*\[(.*)\]\s*$")]
    private static partial Regex InListRegex();
}
