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
    public string? RequiredWhen { get; init; }                // as written, for the message that quotes it
    public RequiredWhen? RequiredWhenCondition { get; init; } // as parsed, for the check that applies it
    public string Type { get; init; } = "string";             // one of ValueChecks.FieldTypes
    public string? Of { get; init; }                          // each entry, where Type is list: ValueChecks.EntryTypes
    public IReadOnlyList<string>? Values { get; init; }       // enum values, resolved

    // Whether `Values` is a ranking, taken from the enum that declared it. A field writing its values
    // inline is never ranked, because the flag sits on the enum. See EnumSpec.
    public bool Ordered { get; init; }

    // Where a value sits in the ranking, `0` being the most significant. Null where the field declares
    // no ranking, and where the range excludes the value, which `enum` reports.
    public int? Rank(string value)
    {
        if (!Ordered || Values is null) return null;

        for (var i = 0; i < Values.Count; i++)
            if (string.Equals(Values[i], value, StringComparison.Ordinal))
                return i;

        return null;
    }

    // The one value of `values:` that says a record is in force: `accepted` on an ADR, `live` on a service, `approved`
    // on a tool.
    public string? InForce { get; init; }

    // The corpus-owned enum this field draws its range from, and null where the schema states the range itself.
    // `Values` stays null where the corpus supplied nothing, so this tells that apart from a field with no range.
    public string? CorpusEnum { get; init; }

    // The shape of one entry, where a list's entries are objects. The first key declared names the entry: `list-order`
    // sorts on it, and a message quotes it.
    public IReadOnlyList<FieldSpec>? Entry { get; init; }

    // The shared shape this field's object keys come from, and empty where it declares them itself.
    public string Shape { get; init; } = "";

    public FieldSpec? EntryKey(string name) =>
        Entry?.FirstOrDefault(k => string.Equals(k.Name, name, StringComparison.Ordinal));

    public bool DeclaresId => Type == "id" || (Type == "list" && Of == "id");

    public bool PointsAt(TypeSchema t) => DeclaresId && Refs.Contains(t.Key, StringComparer.Ordinal);

    // The folders an id in this field may belong to. A scalar `ref:` arrives as a one-entry list.
    public IReadOnlyList<string> Refs { get; init; } = [];

    // Whether an id in this field has to reach a part of the record, as a standard's `implements:` does.
    public bool PartRequired { get; init; }

    public string? Reciprocal { get; init; } // field on the target that must point back
    public string? Pattern { get; init; }
    public Regex? PatternRegex { get; init; }    // Pattern compiled, though the message still quotes the source string
    public string? MirrorsSection { get; init; } // section whose ids this field must mirror

    // The label on the footnote lines whose citations this field must mirror: `Covers` on a standard's `implements:`,
    // read from `_**Covers:** [pol-SCRT].EMBED_`.
    public string? MirrorsCitations { get; init; }

    // Words admitted beside the field's declared type, such as `applies-to: [all]` and `last-rehearsed: never`. A
    // listed value is taken as written and checked no further.
    public IReadOnlyList<string> AllowLiteral { get; init; } = [];

    // The floor on a list field's length, and null where the schema sets none.
    public int? MinItems { get; init; }

    // The floor on how many records of the type carry a given value, and null where the schema sets none.
    public int? MinRecords { get; init; }

    // Where the field's value comes from, for a field the record never writes, such as `sub-path` from the folder. Null
    // for every field an author fills in. `Doc.DerivedSources` is the vocabulary.
    public string? From { get; init; }

    // Why this declaration could not be read, and null where it could. Carried rather than thrown, so one unreadable
    // field is one finding and the rest of the schema still loads.
    public string? Problem { get; init; }

    // `Description` is what the generated Metadata table renders. `Notes` is the longer reasoning. A field with only
    // `Notes` falls back to them.
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public string? TableText => string.IsNullOrEmpty(Description) ? Notes : Description;

    public bool IsLiteral(string? value) =>
        value is not null && AllowLiteral.Contains(value, StringComparer.Ordinal);
}

// Where a type keeps the citable parts of a record, and what holds them to shape. A policy keeps them as table rows
// with the id written beside each. A glossary keeps them as headings, and the id is the heading's anchor. The
// declarations from `Columns` down apply to the table source only.
public sealed class PartSpec(string source, string idPattern, List<string> binding, List<string> advisory)
{
    // What one part is called in a message: a clause on a policy, a term in a glossary.
    public string Noun { get; init; } = "part";

    public string Source { get; } = source;
    public string Section { get; init; } = "";

    // The bold labels a part's optional further blocks open with: `Also`, `Avoid` and `Not` on a glossary term.
    public IReadOnlyList<string> Asides { get; init; } = [];

    // The heading level a part is written at, for the heading source.
    public int Level { get; init; } = 3;

    // The table's headers, in order. The first two are read by position as the id and the part.
    public IReadOnlyList<string> Columns { get; init; } = ["Id", "Clause"];

    public string IdPattern { get; } = idPattern;
    public Regex? IdPatternRegex { get; } = Schema.CompilePattern(idPattern);

    public IReadOnlyList<string> Binding { get; } = binding;   // written bold: these oblige
    public IReadOnlyList<string> Advisory { get; } = advisory; // written plain: these recommend

    // The order rows must appear in: binding levels before advisory ones, each as the type declares it.
    private readonly List<string> _levels = [.. binding, .. advisory];
    public IReadOnlyList<string> Levels => _levels;

    // Where a modal sits in that order, or -1 for a modal the type does not declare.
    public int Rank(string modal) => _levels.IndexOf(modal);

    // Longest first, so "MUST NOT" is recognised before the "MUST" that prefixes it.
    public IReadOnlyList<string> ModalsLongestFirst { get; } =
        [.. binding.Concat(advisory).OrderByDescending(m => m.Length)];

    // The modal a part opens with, or null where it opens with none. `clause-order` and the exporter both read it, so
    // the two cannot rank a clause differently.
    public string? Modal(string text) =>
        ModalsLongestFirst.FirstOrDefault(m => text.StartsWith(m, StringComparison.Ordinal));

    // The modal a stretch of text names anywhere in it, or null where it names none. A word the modal only prefixes
    // does not count, so `MUST` inside `MUSTER` reads as nothing.
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

    // The fragment a link into a part carries. A heading's slug is its id and its anchor alike. A table row has no
    // fragment a renderer emits, so a link into a clause lands on the section holding the table.
    public string Anchor(string partId) => Source == Table ? Publishing.Anchor(Section) : partId;

    // The sources an extractor exists for. `schema-dispatch` reads them from here.
    public const string Table = "table";
    public const string Headings = "headings";
    public static readonly IReadOnlyList<string> Sources = [Table, Headings];
}

// What a record of this type contributes to a published export, and how much of each piece travels. A type declaring no
// block contributes nothing. `docs/design/export.md` says what each declaration promises.
public sealed class ExportSpec
{
    // The shape a consumer holds this type's export to, moved by hand. `docs/design/export.md` says what moves it.
    public int Version { get; init; }

    // The frontmatter a record contributes, in the order an export writes it.
    public IReadOnlyList<string> Fields { get; init; } = [];

    // The sections that travel, each with the fidelity it travels at, in declared order.
    public IReadOnlyList<(string Section, string Fidelity)> Sections { get; init; } = [];

    // The fidelity a record's parts travel at, and empty where none do.
    public string Parts { get; init; } = "";

    // Whether the type writes `export.parts:` at all. It tells an absent block from a malformed one, which both leave
    // the fidelity and the line empty.
    public bool PartsDeclared { get; init; }

    // The file a type's framework references travel in, and empty where none do.
    public string Frameworks { get; init; } = "";

    // The keys of one part line, in the order the line writes them, each with the source filling it. `PartLineSource`
    // is the vocabulary of sources.
    public IReadOnlyList<(string Key, string Source)> Line { get; init; } = [];

    // How much of a piece travels. `docs/design/export.md` says what each one promises a consumer.
    public const string Full = "full";
    public const string Summary = "summary";
    public const string Reference = "reference";

    // What a section and a part line may travel at. `schema-dispatch` reads both from here.
    public static readonly IReadOnlyList<string> Carried = [Full, Summary, Reference];
    public static readonly IReadOnlyList<string> CarriedByParts = [Full];
}

// Where one key of a part line takes its value from. `schema-dispatch` reads the vocabulary from here. `part.lead`,
// `part.bullets` and `part.aside.<Label>` read a part's body, which only the heading source has, so `SchemaChecks`
// refuses them on a table-sourced type.
public static class PartLineSource
{
    public const string PartId = "part.id";       // `<record-id>.<part-id>`, the whole address
    public const string PartKey = "part.key";     // the part id alone
    public const string PartText = "part.text";   // the heading as written, or the second cell flattened
    public const string PartLead = "part.lead";   // the body's first block
    public const string PartLevel = "part.level"; // the modal the part opens with
    public const string PartBullets = "part.bullets"; // each bullet of the body's first list, with its modal
    public const string PartSeeAlso = "part.see-also";
    public const string PartAnchor = "part.anchor"; // the fragment the part resolves at
    public const string RecordId = "record.id";
    public const string RecordType = "record.type";
    public const string RecordPath = "record.path";

    // What a source naming a field or a column opens with. The remainder is the name, and an empty one
    // is a source naming nothing, which `SchemaChecks` reports.
    public const string FrontPrefix = "front.";
    public const string ColumnPrefix = "column.";

    // A labelled footnote closing a part, named by the label a field of this type reconciles against.
    // The remainder is that label, so `part.citations.Covers` carries what the `Covers` line gathers.
    public const string CitationPrefix = "part.citations.";

    // A bold-labelled block inside a part, named by a label the type's `parts.asides:` declares. The
    // remainder is that label, so `part.aside.Not` carries the `**Not:**` block without its label.
    public const string AsidePrefix = "part.aside.";

    // The sources naming one thing each, so the prefixed families are tested separately.
    public static readonly IReadOnlyList<string> Fixed =
    [
        PartId, PartKey, PartText, PartLead, PartLevel, PartBullets, PartSeeAlso, PartAnchor,
        RecordId, RecordType, RecordPath
    ];

    // The name a prefixed source carries, or null where the source is not of that family. Empty after
    // the prefix returns an empty string, which is a name the caller reports rather than one it resolves.
    public static string? Argument(string source, string prefix) =>
        source.StartsWith(prefix, StringComparison.Ordinal) ? source[prefix.Length..] : null;
}

// A field's `required-when:` condition: a test against one other field of the same document, such as `status ==
// accepted` or `personal-data in [personal, special-category]`. Absence answers false, including for `!=`, because
// `required-field` already reports the missing field.
public sealed record RequiredWhen(string Field, bool Negated, IReadOnlyList<string> Values)
{
    public bool Holds(string? actual) =>
        actual is not null && Values.Contains(actual, StringComparer.Ordinal) != Negated;
}

// One entry of a type's `rules:` block. An absent `severity:` means declared but not enforced, not a default level.
public sealed record RuleSpec
{
    public required RuleId Id { get; init; }

    // The rule in the schema's own words, whitespace folded so it can be rendered into a table cell.
    public string? Description { get; init; }

    public Sev? Severity { get; init; }

    // The condition a document must satisfy, compiled. Absent on a rule implemented as a class in `Rules/`, and on a
    // rule nothing enforces yet.
    public string? Expr { get; init; }
    public Expr? Compiled { get; init; }

    // What the author is told when the expression fails.
    public string? Message { get; init; }

    // Compiling an expression is refused without a severity and a message, so a compiled rule always has both.
    [MemberNotNullWhen(true, nameof(Compiled))]
    [MemberNotNullWhen(true, nameof(Message))]
    public bool IsExpression => Compiled is not null;

    // The framework standings `alignment-rollup` holds a policy's roll-up to, as the corpus's own register writes them.
    public IReadOnlyList<string> Postures { get; init; } = [];

    // The ceiling `y-statement-present` holds a Y-statement to.
    public int? MaxWords { get; init; }

    // Why this rule could not be read, and null where it could. `SchemaChecks` reports it against the file.
    public string? Problem { get; init; }
}

public sealed class TypeSchema
{
    // The base name of the type's schema file: the folder its records live in, and what `ref:` names.
    public string Key { get; init; } = "";

    public string TypeName { get; init; } = "";
    public string Label { get; init; } = "";
    public string LabelPlural { get; init; } = "";
    public string Folder { get; init; } = "";
    public string Page { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Lifecycle { get; init; } = "";

    // What this type is, in one line. Rendered into the taxonomy's decision table and the corpus index.
    public string Summary { get; init; } = "";
    public string GoesHere { get; init; } = "";

    // The paragraph beneath the one-liner, as the framework defines the type. Examples belong on the type's own page.
    public string Detail { get; init; } = "";

    // The close calls, each against one other type, declared once on the type a reader most likely reached for. That
    // type titles the heading.
    public IReadOnlyList<(string Other, string Text)> Versus { get; init; } = [];

    // Where the type's name came from, and where it parts company with whatever lent it.
    public LineageSpec? Lineage { get; init; }

    // Where this type's name already means something else to a reader from another framework, and empty for most types.
    // A blank line separates paragraphs.
    public string Collision { get; init; } = "";
    public string IdPrefix { get; init; } = "";
    public string IdStyle { get; init; } = "";
    public WidthSpec IdWidth { get; init; } = new(4, 4);
    public string? FilenamePattern { get; init; }
    public Regex? FilenameRegex { get; init; } // FilenamePattern compiled, though the message quotes the source

    // Whether the filename opens with the id's discriminator, as `0007-decide-x.md` does. Where false, nothing reads
    // the head of a filename as an id.
    public bool FilenameCarriesId { get; init; } = true;
    public int SlugMax { get; init; } = 30;
    public IReadOnlyList<string> FieldOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> RequiredSections { get; init; } = [];
    public IReadOnlyList<string> OptionalSections { get; init; } = [];
    public IReadOnlyList<string> IndexColumns { get; init; } = [];

    // The columns the index sorts on, in order of precedence. A scalar `sort:` arrives as a one-entry list.
    public IReadOnlyList<string> IndexSort { get; init; } = [];

    // `ascending` (the default) or `descending`, applied to the whole sort.
    public string IndexOrder { get; init; } = "";
    public PartSpec? Parts { get; init; }
    public ExportSpec? Export { get; init; }

    // The two above, for a caller that has already established the type declares them. Each throws where it does not.
    public ExportSpec DeclaredExport => Export
                                        ?? throw new InvalidOperationException(
                                            $"type '{TypeName}' declares no export: to read.");

    public PartSpec DeclaredParts => Parts
                                     ?? throw new InvalidOperationException(
                                         $"type '{TypeName}' declares no export.parts: to read.");

    public IReadOnlyList<RuleSpec> Rules { get; init; } = [];

    // Derived at load from the declarations above by the Derive* helpers. A hand-built TypeSchema leaves them empty,
    // which the generator's tests rely on.

    // Every frontmatter key a document of this type may carry.
    public IReadOnlySet<string> KnownKeys { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    // Every pair of keys whose relative order the schema fixes.
    public IReadOnlyList<(string Before, string After)> KeyOrderEdges { get; init; } = [];

    // Every field a document of this type is judged against, in the order the schema declares them, with
    // each type override already resolved against the universal field it refines.
    public IReadOnlyList<FieldSpec> DeclaredFields { get; init; } = [];

    // How one document of this type is named in generated prose: "Policy", "ADR", "NFR".
    public string DisplayName =>
        !string.IsNullOrEmpty(Label) ? Label
        : !string.IsNullOrEmpty(TypeName) ? char.ToUpperInvariant(TypeName[0]) + TypeName[1..]
        : IdPrefix.ToUpperInvariant();

    // How the collection is named: "ADRs", "Policies". Nothing derives a plural, so `label-plural:` is required where
    // `label:` is not.
    public string PluralName => !string.IsNullOrEmpty(LabelPlural) ? LabelPlural : DisplayName;

    public bool HasRule(RuleId id) => Rules.Any(r => r.Id == id);

    public bool AnyField(Func<FieldSpec, bool> predicate) => Fields.Values.Any(predicate);

    // The same question of the keys inside an object list's entries, to any depth. Apart from `AnyField`, because
    // `reciprocal`, `mirrors-section` and `ref` read top-level fields only.
    public bool AnyEntryKey(Func<FieldSpec, bool> predicate) =>
        Fields.Values.Any(f => Nested(f).Any(predicate));

    private static IEnumerable<FieldSpec> Nested(FieldSpec field) =>
        (field.Entry ?? []).SelectMany(key => new[] { key }.Concat(Nested(key)));

    // The universal fields, the type's own, and the reserved keys the publishing platform adds.
    public static IReadOnlySet<string> DeriveKnownKeys(
        IEnumerable<string> universalOrder, IEnumerable<string> fieldOrder, IEnumerable<string> reserved) =>
        universalOrder.Concat(fieldOrder).Concat(reserved).ToHashSet(StringComparer.Ordinal);

    // Every pair within each declared chain is a constraint, not just neighbours, so an absent key does not drop the
    // constraint between the keys either side of it.
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

// A key a schema file carries that the loader never asks for. Held rather than thrown, so one is one finding.
public sealed record UnreadKey(string File, string Where, string Key);

// A mapping the loader is reading, which remembers what it was asked for. The keys a level admits are the keys the
// loader reads, and `notes:` at every level.
internal sealed class Level(YamlNode? node, string where)
{
    private readonly HashSet<string> _asked = new(StringComparer.Ordinal);

    public string Where { get; } = where;

    // Whether the block is there at all. A type declares its parts, and its export, by carrying the block.
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

// The mappings read from one schema file, in the order they were opened, so unread keys report in reading order.
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

// One shared enum: its values, and whether they rank. The first value is the most significant.
public sealed record EnumSpec(IReadOnlyList<string> Values, bool Ordered);

public sealed record TierSpec(string Name, string Label, string Behaviour, string Note);

// A type's prior art, what the framework took, and where it parts company; a type with no useful ancestor says so in
// `PriorArt` and leaves the others empty. Links are written inline, because the block this renders into cannot see
// the definitions at the foot of its page.
public sealed record LineageSpec(string PriorArt, string Alignment, string Divergence);

// How long a `numbered` or `mnemonic` discriminator may be. A scalar `width:` sets both ends. A `min:`/`max:` mapping
// opens a span, which `SchemaChecks` refuses on a type whose filename carries the id.
public sealed record WidthSpec(int Min, int Max)
{
    public bool Admits(int length) => length >= Min && length <= Max;

    public override string ToString() => Min == Max ? $"{Min}" : $"{Min} to {Max}";
}

public sealed partial class Schema
{
    // The one key admitted at every level and required at none. See Level.
    public const string Commentary = "notes";

    // The tiers as `_tiers.yaml` declares them, in the order every generated list of types is grouped by.
    // `SchemaChecks` holds them against the universal `tier` field.
    public IReadOnlyList<TierSpec> Tiers { get; init; } = [];

    // Every check the validator can emit, as `_checks.yaml` declares it. `SchemaChecks` holds the catalogue against the
    // code.
    public IReadOnlyList<CheckDef> Checks { get; init; } = [];

    public IReadOnlyList<string> UniversalOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Universal { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> Reserved { get; init; } = [];

    public IReadOnlyDictionary<string, EnumSpec> Enums { get; init; } =
        new Dictionary<string, EnumSpec>();

    public IReadOnlyDictionary<string, TypeSchema> ByFolder { get; init; } = new Dictionary<string, TypeSchema>();

    // The prefix every type's ids open with. Empty prefixes are dropped, because one would admit every code span
    // carrying a dot.
    public IReadOnlySet<string> IdPrefixes => field ??=
        ByFolder.Values.Select(t => t.IdPrefix).Where(p => p.Length > 0).ToHashSet(StringComparer.Ordinal);

    // Every key these files carry that the loader never asks for, in the order a reader would meet them.
    public IReadOnlyList<UnreadKey> UnreadKeys { get; private init; } = [];

    // The shared field declarations, reserved keys and enums every type is read against.
    private sealed record UniversalLayer(
        IReadOnlyList<string> Order,
        IReadOnlyDictionary<string, FieldSpec> Fields,
        IReadOnlyList<string> Reserved,
        IReadOnlyDictionary<string, EnumSpec> Enums,
        IReadOnlyDictionary<string, IReadOnlyList<FieldSpec>> Shapes);

    // The nearest folder at or above the corpus root holding a `.schema/`, and null where none does.
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

    // The schema at or above `root`, falling back to `root` itself so a missing schema fails on the file it cannot
    // open. `corpusEnums` is what the corpus supplied for fields drawing their range from `$corpus.`.
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

    // The schema's files by name, already read. Walked in ordinal order, because a map has none. A missing name reads
    // as an empty document, so a missing shared block is reported as findings.
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
            if (baseName.StartsWith('_')) continue; // the shared blocks, which declare no type

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

    // How the top level names itself in a finding. The file is already named beside the message.
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

    // `width: 4` and a `min:`/`max:` block are one declaration. A block short of one end reads as an exact width at the
    // end it wrote.
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

    // Universal fields first, then the type's own, with a name in both resolved to the type's refinement.
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

    // An unrecognised severity reads as absent, leaving the rule declared but not enforced. A rule with an `expr:` must
    // compile and carry a severity and a message. A failure is recorded on the rule for `SchemaChecks`, and the rule
    // loads inert.
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

        return new RuleSpec
        {
            Id = new RuleId(id),
            Description = Collapse(Yaml.Str(rule.Get("description"))),
            Severity = severity,
            Expr = source,
            Compiled = compiled,
            Message = message,
            Problem = problem,
            Postures = Yaml.StrList(rule.Get("postures")),
            MaxWords = rule.Get("max-words") is YamlScalarNode { Value: { } mw } && int.TryParse(mw, out var words)
                ? words
                : null
        };
    }

    // A condition outside the vocabulary throws, and ParseField catches it against the field. Read as absent, it would
    // leave the field silently never required.
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

    // The field a name resolves to for a type: its own declaration, otherwise the universal one.
    private static FieldSpec? Effective(IReadOnlyDictionary<string, FieldSpec> typeFields,
        IReadOnlyDictionary<string, FieldSpec> universal, string name) =>
        typeFields.TryGetValue(name, out var tf) ? tf : universal.GetValueOrDefault(name);

    public FieldSpec? EffectiveField(TypeSchema t, string name) => Effective(t.Fields, Universal, name);

    // For a caller holding a name it knows the type declares. Throws otherwise.
    public FieldSpec DeclaredField(TypeSchema t, string name) => EffectiveField(t, name)
                                                                 ?? throw new InvalidOperationException(
                                                                     $"type '{t.TypeName}' declares no field '{name}'.");

    // A key written as one value or a list. `Yaml.Str` returns null for a sequence, so reading such a key with it drops
    // every list-form declaration.
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
