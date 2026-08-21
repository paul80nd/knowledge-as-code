using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace kac.core;
// ---------------------------------------------------------------------------
// Schema model — loaded from .schema/*.yaml
//
// Everything here is settled at load and read-only afterwards. That is what makes the derived sets on
// TypeSchema safe to hold: they are computed from the declarations beside them, and nothing can change
// a declaration out from under a set that was derived from it.
// ---------------------------------------------------------------------------

public sealed class FieldSpec
{
    public required string Name { get; init; }
    public bool Required { get; init; }
    public string? RequiredWhen { get; init; }                // as written, for the message that quotes it
    public RequiredWhen? RequiredWhenCondition { get; init; } // as parsed, for the check that applies it
    public string Type { get; init; } = "string";             // string|date|enum|id|list
    public string? Of { get; init; }                          // element type when Type == list
    public IReadOnlyList<string>? Values { get; init; }       // enum values (resolved)

    // The folders an id in this field may belong to. A list because several fields point at more than
    // one type — a discovery is promoted to a FAQ or a standard — and a scalar `ref:` is the one-entry
    // case rather than a separate shape.
    public IReadOnlyList<string> Refs { get; init; } = [];

    public string? Reciprocal { get; init; } // field on the target that must point back
    public string? Pattern { get; init; }
    public Regex? PatternRegex { get; init; }    // Pattern compiled — the message still quotes the source string
    public string? MirrorsSection { get; init; } // section whose ids this field must mirror

    // Words admitted beside the field's declared type — `applies-to: [all]`, `last-rehearsed: never`. A
    // value listed here is taken as written and nothing further is asked of it, which is what lets a date
    // field say "never" and a list of ids say "all" without either widening into a string field and
    // losing the checks that hold every other value it carries.
    public IReadOnlyList<string> AllowLiteral { get; init; } = [];

    // The floor on a list field's length, where the schema sets one. A field whose whole purpose is to be
    // over-filled — a FAQ's search keywords — has nothing else holding it to more than one entry.
    public int? MinItems { get; init; }

    // The floor on how many records of the type carry a given value, where the schema sets one. It is
    // what a field says when its values are meant to group documents rather than describe one: a value
    // below the floor divides nothing, and belongs in a field that is free to be unique.
    public int? MinRecords { get; init; }

    // Why this declaration could not be read, where it could not be. Carried on the spec rather than
    // thrown at load so that one unreadable field is one finding naming the file and the key, and the
    // rest of the schema still loads: a corpus is told what is wrong with it, not handed a stack trace.
    public string? Problem { get; init; }

    // Two audiences, deliberately separate. `Description` is what a reader needs at a glance and is what
    // the generated Metadata table renders; `Notes` is the longer why-it-exists, which belongs in the
    // schema where there is room for it. A field with only Notes falls back to them.
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public string? TableText => string.IsNullOrEmpty(Description) ? Notes : Description;

    // Whether a value is one of the literals this field admits. Asked of a scalar field's value and of
    // each entry of a list, because `allow-literal` is about the word rather than about the shape that
    // carries it.
    public bool IsLiteral(string? value) =>
        value is not null && AllowLiteral.Contains(value, StringComparer.Ordinal);
}

// Where a type keeps the parts of a record that something else may cite, and what holds them to shape.
//
// A part is an identifiable child of a record: `pol-VURM.TIMEBOX` names the policy and then the
// obligation inside it, `gls-knowledge-as-code.corpus` the glossary and then the term. Which children
// count is the type's own business, so the type names the source and one extractor reads it. A policy
// keeps its parts as the rows of a table, where each row's id is written beside it; a glossary keeps
// them as the headings its terms are written as, where the id comes from the heading and is the anchor a
// link would use. Held as its own spec, so a type gains parts by declaring them. A type that declares
// none offers none, and a citation into it is reported as reaching nothing.
//
// Everything from `Columns` down is the table source's. A heading has no columns, no authored id to hold
// to a pattern and no modal, so a type sourcing headings declares none of them.
//
// The modal orderings are derived here, once per type, because every row of every document reads them
// and they never differ between two rows of the same type.
public sealed class PartSpec(string source, string idPattern, List<string> binding, List<string> advisory)
{
    // What one part is called, so a message about a missing one uses the word the type's own readers do:
    // a clause on a policy, a term in a glossary. "part" is the general word, and a type that names none
    // falls back to it.
    public string Noun { get; init; } = "part";

    public string Source { get; } = source;
    public string Section { get; init; } = "";

    // The label a part's optional second block opens with, written bold — `Not` on a glossary term.
    // Empty where a type's parts carry no such block, and the whole of what makes one legible to
    // anything but a reader. `.schema/README.md` says why it is declared here rather than in the code
    // that reads it.
    public string Aside { get; init; } = "";

    // The heading level a part is written at, for the heading source. H3 throughout the corpus: an H1 is
    // the document and an H2 is a section, so the first level below them is the first that can be a part.
    public int Level { get; init; } = 3;

    // The table's headers, in order. The first two are read positionally as the id and the part; any
    // further column is the type's own — `Alignment` on a policy — and is checked for being there and
    // named right, its contents being prose the schema has no view on.
    public IReadOnlyList<string> Columns { get; init; } = ["Id", "Clause"];

    public string IdPattern { get; } = idPattern;
    public Regex? IdPatternRegex { get; } = Schema.CompilePattern(idPattern);

    public IReadOnlyList<string> Binding { get; } = binding;   // written bold — these oblige
    public IReadOnlyList<string> Advisory { get; } = advisory; // written plain — these recommend

    // The order rows must appear in: binding levels before advisory ones, each as the type declares it.
    private readonly List<string> levels = [.. binding, .. advisory];
    public IReadOnlyList<string> Levels => levels;

    // Where a modal sits in that order, or -1 for a modal the type does not declare.
    public int Rank(string modal) => levels.IndexOf(modal);

    // Longest first, so "MUST NOT" is recognised before the "MUST" that prefixes it.
    public IReadOnlyList<string> ModalsLongestFirst { get; } =
        [.. binding.Concat(advisory).OrderByDescending(m => m.Length)];

    // The sources an extractor exists for. Read by `schema-dispatch` from here, so a type naming a source
    // nothing reads is reported instead of quietly offering no parts.
    public const string Table = "table";
    public const string Headings = "headings";
    public static readonly IReadOnlyList<string> Sources = [Table, Headings];
}

// What a record of this type contributes to a published export, and how much of each piece travels.
//
// An export is a dependency surface: once a consumer corpus reads one, its shape is a contract. So what
// travels is declared by the type, beside the sections and fields it selects from, rather than decided
// by an exporter reading each type in turn. A type declaring no block contributes nothing.
//
// Selection is by key, so the two structural declarations cannot disagree: a section named here is one
// the type's `sections:` block declares, and parts are the ones its `parts:` block already locates.
//
// Fidelity is written against every entry and defaulted nowhere. A section carried whole and a section
// reduced to a line are different promises to whoever reads the export. That difference is the whole of
// what this block says.
public sealed class ExportSpec
{
    // The frontmatter a record contributes, in the order an export writes it. Each names a field the
    // type declares or inherits, so an export carries what a record actually holds.
    public IReadOnlyList<string> Fields { get; init; } = [];

    // The sections that travel, each with the fidelity it travels at, in declared order.
    public IReadOnlyList<(string Section, string Fidelity)> Sections { get; init; } = [];

    // The fidelity a record's parts travel at, and empty where none do — a glossary's terms carry the
    // definition and the line naming what the term is confused with, which is the content that changes
    // what a reader does.
    public string Parts { get; init; } = "";

    // Carried whole: the record's own words reach the consumer unchanged.
    public const string Full = "full";

    // Reduced to a line, and reduced to a breadcrumb back to the record it came from. Both are named
    // ahead of the exporter that writes them, so the model holds still when a type reaches for one.
    public const string Summary = "summary";
    public const string Reference = "reference";

    // The fidelities an export writes. Read by `schema-dispatch` from here, so a type declaring one of
    // the others is told so rather than exporting silence under it.
    public static readonly IReadOnlyList<string> Carried = [Full];
}

// A field's `required-when:` — the condition under which an otherwise optional field must be filled
// in, as a test against one other field of the same document. A deliberately closed vocabulary:
// `status == accepted`, `mechanism != not-enforced`, `classification in [personal, special-category]`.
//
// It is not the `expr:` language, and the difference is the point. A rule asks a question about a whole
// document and may need to combine several; this asks one thing about one field, and stays legible in a
// schema a reader is skimming for what a type requires.
//
// Absence answers false either way — including for `!=`. Where the field a condition names is missing,
// `required-field` is already reporting that; requiring a second field on top of it would report one
// omission as two.
public sealed record RequiredWhen(string Field, bool Negated, IReadOnlyList<string> Values)
{
    public bool Holds(string? actual) =>
        actual is not null && Values.Contains(actual, StringComparer.Ordinal) != Negated;
}

// One entry of a type's `rules:` block. A rule states an intention from the moment it is written, and
// most of them are still only that — prose the schema carries and no check answers to yet. It gains a
// `severity:` when something can act on it, which is why an absent severity means "declared, not
// enforced" rather than a default level.
public sealed record RuleSpec
{
    public required RuleId Id { get; init; }

    // The rule in the schema's own words, whitespace folded so it can be rendered into a table cell.
    public string? Description { get; init; }

    public Sev? Severity { get; init; }

    // The condition a document must satisfy, as the schema wrote it, and compiled. A rule carrying one
    // needs no C# at all: the dispatcher evaluates it and reports `Message` where it comes out false.
    // Absent on the rules that need a real algorithm — git history, graph walks, anything spanning
    // documents — which stay in C# as a class in `Rules/`, and on the rules nothing answers to yet.
    public string? Expr { get; init; }
    public Expr? Compiled { get; init; }

    // What the author is told when the expression fails. Distinct from Description, which says what the
    // rule means to someone reading the type page: one is a diagnosis, the other a definition.
    public string? Message { get; init; }

    // The ceiling `y-statement-present` holds a Y-statement to. It sits in the schema for the reason
    // every threshold does — a number chosen rather than measured is one a corpus tunes, and tuning it
    // should not be a release — and on the rule rather than on a field because no field carries it.
    public int? MaxWords { get; init; }

    // Why this rule could not be read, where it could not be. See FieldSpec.Problem — same reasoning,
    // and SchemaChecks turns both into findings against the file that declares them.
    public string? Problem { get; init; }
}

public sealed class TypeSchema
{
    // How the schema names this type: the base name of its file, which is the folder its records live
    // in. It is what `ref:` and `versus:` name, and what a finding calls the type, so it is carried here
    // rather than being the dictionary key alone — a TypeSchema handed to a renderer would otherwise
    // have lost the one name the schema knows it by.
    public string Key { get; init; } = "";

    public string TypeName { get; init; } = "";
    public string Label { get; init; } = "";
    public string LabelPlural { get; init; } = "";
    public string Folder { get; init; } = "";
    public string Page { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Lifecycle { get; init; } = "";

    // What this type is, in one line, and what a reader has in hand when it is the answer. Both are
    // rendered rather than read by a check: the taxonomy's decision table and the corpus's own index are
    // generated from them, so a type says what it is once, here, instead of once in every page that
    // introduces it.
    public string Summary { get; init; } = "";
    public string GoesHere { get; init; } = "";

    // The paragraph beneath the one-liner: what the type carries beyond its first sentence, and the edge
    // a reader is most likely to walk over. It is the type as the framework defines it, so it stays free
    // of anything local — the examples and the estate belong on the type's own page, which is the corpus's
    // to write.
    public string Detail { get; init; } = "";

    // The calls that are genuinely close, each against one other type. A pair is declared once, by the
    // type the reader is most likely to have reached for — which is also the type the heading is titled
    // from, so "ADR vs Standard" lives on `adrs`. Declared order is kept for the messages; the page they
    // render on sorts them for itself.
    public IReadOnlyList<(string Other, string Text)> Versus { get; init; } = [];

    // Where the type's name came from, and where it parts company with whatever lent it. The framework's
    // own intellectual debt rather than the corpus's, so it is identical wherever this schema is taken —
    // which is the difference between it and a corpus's standing against a framework, recorded once in
    // `frameworks.md` and nowhere near here.
    public LineageSpec? Lineage { get; init; }

    // Where this type's name already means something else to a reader arriving from another framework, and
    // what they will get wrong because of it. Empty for most types: a word only collides where somebody
    // else got there first, and inventing a collision to fill the key would waste the warning.
    //
    // Paragraphs are separated by a blank line in the schema and rendered as paragraphs.
    public string Collision { get; init; } = "";
    public string IdPrefix { get; init; } = "";
    public string IdStyle { get; init; } = "";
    public int IdWidth { get; init; }
    public string? FilenamePattern { get; init; }
    public Regex? FilenameRegex { get; init; } // FilenamePattern compiled — the message quotes the source
    public int SlugMax { get; init; } = 30;
    public IReadOnlyList<string> FieldOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> RequiredSections { get; init; } = [];
    public IReadOnlyList<string> OptionalSections { get; init; } = [];
    public IReadOnlyList<string> IndexColumns { get; init; } = [];

    // The columns the index sorts on, in order of precedence. A list because a type whose rows group
    // naturally reads better sorted twice — a runbook by severity then id — and a scalar `sort:` is the
    // one-entry case rather than a separate shape.
    public IReadOnlyList<string> IndexSort { get; init; } = [];

    // `ascending` (the default) or `descending`, applied to the sort as a whole. A postmortem index is
    // the case for it: the incident someone is looking for is almost always the most recent.
    public string IndexOrder { get; init; } = "";
    public PartSpec? Parts { get; init; }
    public ExportSpec? Export { get; init; }
    public IReadOnlyList<RuleSpec> Rules { get; init; } = [];

    // -- derived at load from the declarations above; see the Derive* helpers --
    //
    // Each is a per-type constant that every document of the type asks for. Left underived, a hand-built
    // TypeSchema is simply a type with no fields to know about, which is what the generator's tests want.

    // Every frontmatter key a document of this type may carry.
    public IReadOnlySet<string> KnownKeys { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    // Every pair of keys whose relative order the schema fixes.
    public IReadOnlyList<(string Before, string After)> KeyOrderEdges { get; init; } = [];

    // Every field a document of this type is judged against, in the order the schema declares them, with
    // each type override already resolved against the universal field it refines.
    public IReadOnlyList<FieldSpec> DeclaredFields { get; init; } = [];

    // How a single document of this type is named in generated prose — "Policy", "ADR", "NFR". Declared
    // by the schema rather than derived, because no rule turns `policy` into "Policy" and `adr` into
    // "ADR" without knowing which names are initialisms. The fallbacks keep a schema that omits `label`
    // rendering something sensible.
    public string DisplayName =>
        !string.IsNullOrEmpty(Label) ? Label
        : !string.IsNullOrEmpty(TypeName) ? char.ToUpperInvariant(TypeName[0]) + TypeName[1..]
        : IdPrefix.ToUpperInvariant();

    // How the *collection* is named — "ADRs", "Policies", "NFRs", "Data". Where a generated line points at
    // the type's page rather than at one of its records, this is the name that belongs on the link.
    //
    // Declared with no fallback worth the name, which is why `label-plural:` is required where `label:` is
    // not: nothing turns `nfr` into "NFRs" and `glossary` into "Glossary", and appending an `s` produces a
    // word for some types and a mistake for the rest. The fallback exists only so that a hand-built
    // TypeSchema renders something.
    public string PluralName => !string.IsNullOrEmpty(LabelPlural) ? LabelPlural : DisplayName;

    // Whether this type declares a given rule. The reader-facing checks table uses it to show a
    // rule's row only on the pages whose schema actually carries the rule.
    public bool HasRule(RuleId id) => Rules.Any(r => r.Id == id);

    // Whether any field on this type declares the given FieldSpec property — the same question for
    // schema-driven core checks (reciprocal, mirrors-section) that only fire when a field opts in.
    public bool AnyField(Func<FieldSpec, bool> predicate) => Fields.Values.Any(predicate);

    // The universal fields, the type's own, and the reserved keys the publishing platform adds.
    // Deduplicated, since a type refining `status` declares it in both chains. Order carries no meaning
    // here — the only question asked of it is whether a key is in it.
    public static IReadOnlySet<string> DeriveKnownKeys(
        IEnumerable<string> universalOrder, IEnumerable<string> fieldOrder, IEnumerable<string> reserved) =>
        universalOrder.Concat(fieldOrder).Concat(reserved).ToHashSet(StringComparer.Ordinal);

    // Key order is declared across two files that share the `status` key, so rather than invent one
    // arbitrary total order, every pair within each declared chain becomes a constraint and genuinely
    // unconstrained pairs stay free. Taken pairwise across the whole chain rather than between
    // neighbours, so an absent intermediate key does not drop the constraint between its neighbours.
    public static IReadOnlyList<(string Before, string After)> DeriveKeyOrderEdges(
        IReadOnlyList<string> universalOrder, IReadOnlyList<string> fieldOrder)
    {
        // A set while building, because a type that re-declares two universal keys in the order the
        // universal chain already holds them contributes that pair twice, and a document out of order on
        // it would be told about one fault twice. A list once built, because checking a document walks
        // every constraint and never asks whether a given one is present.
        var edges = new HashSet<(string, string)>();
        foreach (var chain in new[] { universalOrder, fieldOrder })
            for (var i = 0; i < chain.Count; i++)
            for (var j = i + 1; j < chain.Count; j++)
                edges.Add((chain[i], chain[j]));
        return [.. edges];
    }
}

// A key a schema file carries that the loader never asks for. Held rather than thrown, as every schema
// defect is, so that one such key is one finding naming the file and the key and the rest of the schema
// still loads.
public sealed record UnreadKey(string File, string Where, string Key);

// A mapping the loader is reading, which remembers what it was asked for. The vocabulary a level admits
// is therefore the set of keys the loader actually reads: adding a key to the loader adds it to the
// vocabulary in the same edit, where a list of permitted keys written out beside the check would be a
// list of what is spelled correctly rather than of what runs.
//
// `notes:` is the exception, admitted at every level and parsed only on a field. It is the schema's one
// word for commentary, and these files earn it — they are read by someone who cannot ask what a key was
// for. Naming commentary is what lets the rest of the key space be closed around it.
internal sealed class Level(YamlNode? node, string where)
{
    private readonly HashSet<string> asked = new(StringComparer.Ordinal);

    public string Where { get; } = where;

    // Whether the block is there at all, for the two callers that treat an absent block and an empty one
    // differently: a type declares its parts, and its export, by carrying the block. One carrying neither
    // has neither.
    public bool Present => node is YamlMappingNode;

    public YamlNode? Get(string key)
    {
        asked.Add(key);
        return Yaml.Get(node, key);
    }

    public IEnumerable<string> Unread() => Yaml.Map(node)
        .Select(entry => entry.Item1)
        .Where(key => key != Schema.Commentary && !asked.Contains(key));
}

// The mappings read from one schema file. Levels are harvested in the order they were opened, so a file
// with several unread keys reports them in the order someone reading it would meet them.
internal sealed class KeyReader(string file)
{
    private readonly List<Level> levels = [];

    public Level At(YamlNode? node, string where)
    {
        var level = new Level(node, where);
        levels.Add(level);
        return level;
    }

    public IEnumerable<UnreadKey> Unread() =>
        levels.SelectMany(l => l.Unread().Select(key => new UnreadKey(file, l.Where, key)));
}

// One tier: the word a document carries in its frontmatter, what it is called on a page, how a document
// of that tier behaves, and — where the tier has one — the thing worth saying about it before the types
// beneath it are listed.
public sealed record TierSpec(string Name, string Label, string Behaviour, string Note);

// A type's prior art, and the two things worth saying about it: what the framework took, and where it
// deliberately parts company. Links are written inline rather than as reference labels, because the block
// this renders into cannot see the definitions at the foot of the page it lands on — and a label whose
// definition is deleted renders as literal brackets rather than as a failure.
//
// A type with no useful ancestor says so in `PriorArt` and leaves the other two empty. Claiming an
// ancestor a type does not have would be worse than admitting none, so the absence is a real answer and
// renders as one.
public sealed record LineageSpec(string PriorArt, string Alignment, string Divergence);

public sealed partial class Schema
{
    // The one key admitted at every level and required at none. See Level.
    public const string Commentary = "notes";

    // The tiers as `_tiers.yaml` declares them, in declared order — which is the order every generated
    // list of types is grouped by. The vocabulary itself belongs to the universal `tier` field; these are
    // the same values with the prose a frontmatter enum has no room for, and SchemaChecks holds the two
    // against each other.
    public IReadOnlyList<TierSpec> Tiers { get; init; } = [];

    // Every check the validator can emit, as `_checks.yaml` declares it. The schema is where a check is
    // defined: its severity, the words a reader meets, the reasoning, and the concern it belongs to.
    // The code decides what runs, and `SchemaChecks` holds the two against each other.
    public IReadOnlyList<CheckDef> Checks { get; init; } = [];

    public IReadOnlyList<string> UniversalOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Universal { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> Reserved { get; init; } = [];

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Enums { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();

    public IReadOnlyDictionary<string, TypeSchema> ByFolder { get; init; } = new Dictionary<string, TypeSchema>();

    // The prefix every type's ids open with, which separates a citation from a filename of the same
    // shape. Derived from the types themselves, so a corpus adopting one gains its prefix here and a
    // corpus declining one stops reading citations into it. Empty prefixes are dropped: a type declaring
    // none would otherwise admit every code span carrying a dot.
    public IReadOnlySet<string> IdPrefixes => idPrefixes ??=
        ByFolder.Values.Select(t => t.IdPrefix).Where(p => p.Length > 0).ToHashSet(StringComparer.Ordinal);

    private IReadOnlySet<string>? idPrefixes;

    // Every key these files carry that the loader never asked for, in the order it would be met reading
    // them. A key nothing reads is a declaration in a file documented as the contract the tool enforces,
    // which is the same promise a rule id nothing dispatches makes.
    public IReadOnlyList<UnreadKey> UnreadKeys { get; private init; } = [];

    // The parts of the schema every type is read against: the shared field declarations, the reserved
    // keys, and the enum vocabularies a field can draw on. Carried as one argument into type parsing
    // because a type's derived sets span its own declarations and these together.
    private sealed record UniversalLayer(
        IReadOnlyList<string> Order,
        IReadOnlyDictionary<string, FieldSpec> Fields,
        IReadOnlyList<string> Reserved,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Enums);

    // Takes whatever holds a `.schema/`: a corpus, or the template a corpus is made from. The schema is
    // the same document in both, and the callers that read it from the template are the ones proving it.
    public static Schema Load(string root)
    {
        var dir = Path.Combine(root, ".schema");
        var unread = new List<UnreadKey>();

        var enumKeys = new KeyReader(".schema/_enums.yaml");
        var enumsRoot = enumKeys.At(Yaml.LoadFile(Path.Combine(dir, "_enums.yaml")), TheFile);
        var enums = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var (name, node) in Yaml.Map(enumsRoot.Get("enums")))
            enums[name] = Yaml.StrList(enumKeys.At(node, $"enum '{name}'").Get("values"));
        unread.AddRange(enumKeys.Unread());

        var tierKeys = new KeyReader(".schema/_tiers.yaml");
        var tiersRoot = tierKeys.At(Yaml.LoadFile(Path.Combine(dir, "_tiers.yaml")), TheFile);
        var tiers = new List<TierSpec>();
        foreach (var (name, node) in Yaml.Map(tiersRoot.Get("tiers")))
        {
            var tier = tierKeys.At(node, $"tier '{name}'");
            tiers.Add(new TierSpec(name, Yaml.Str(tier.Get("label")) ?? "",
                Yaml.Str(tier.Get("behaviour"))?.Trim() ?? "", Yaml.Str(tier.Get("note"))?.Trim() ?? ""));
        }

        unread.AddRange(tierKeys.Unread());

        var checkKeys = new KeyReader(".schema/_checks.yaml");
        var checksRoot = checkKeys.At(Yaml.LoadFile(Path.Combine(dir, "_checks.yaml")), TheFile);
        var checks = new List<CheckDef>();
        foreach (var (id, node) in Yaml.Map(checksRoot.Get("checks")))
        {
            var check = checkKeys.At(node, $"check '{id}'");
            checks.Add(new CheckDef(new CheckId(id),
                Yaml.Str(check.Get("severity")) == "warning" ? Sev.Warning : Sev.Error,
                Yaml.Str(check.Get("description"))?.Trim() ?? "",
                Yaml.Str(check.Get("notes"))?.Trim() ?? "",
                // Most checks belong on a type page, so the key is written only where one does not.
                Yaml.Str(check.Get("on-type-page")) is not "false"));
        }

        unread.AddRange(checkKeys.Unread());

        var universalKeys = new KeyReader(".schema/_universal.yaml");
        var uni = universalKeys.At(Yaml.LoadFile(Path.Combine(dir, "_universal.yaml")), TheFile);
        var universalOrder = new List<string>();
        var universal = new Dictionary<string, FieldSpec>();
        foreach (var (name, node) in Yaml.Map(uni.Get("fields")))
        {
            universalOrder.Add(name);
            universal[name] = ParseField(universalKeys.At(node, $"field '{name}'"), name, enums);
        }

        var layer = new UniversalLayer(universalOrder, universal, Yaml.StrList(uni.Get("reserved")), enums);
        unread.AddRange(universalKeys.Unread());

        var byFolder = new Dictionary<string, TypeSchema>();
        foreach (var file in Directory.GetFiles(dir, "*.yaml").OrderBy(f => f))
        {
            var baseName = Path.GetFileNameWithoutExtension(file);
            if (baseName.StartsWith('_')) continue; // the shared blocks, which declare no type

            var keys = new KeyReader($".schema/{baseName}.yaml");
            byFolder[baseName] = ParseType(keys.At(Yaml.LoadFile(file), TheFile), keys, layer, baseName);
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

    // How a level names itself in a finding. The file is already named beside the message, so the top of
    // one says only that it is the top.
    private const string TheFile = "the file";

    private static TypeSchema ParseType(Level root, KeyReader keys, UniversalLayer layer, string key)
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
            fields[name] = ParseField(keys.At(node, $"field '{name}'"), name, layer.Enums);
        }

        var rules = root.Get("rules") is YamlSequenceNode ruleNodes
            ? ruleNodes.Children.Select(node => ParseRule(node, keys)).ToList()
            : [];

        var filenamePattern = Yaml.Str(fn.Get("pattern"));
        var parts = keys.At(root.Get("parts"), "the 'parts' block");

        // The section names under `export.sections:` are headings rather than schema keys, so the mapping
        // holding them is read directly and never opened as a level: a heading is not a key the loader
        // could have been expected to ask for, and reporting one as unread would make selecting by key
        // impossible.
        var export = keys.At(root.Get("export"), "the 'export' block");

        // All three read whether or not the block is there, so that a lineage missing only its `prior-art:`
        // is reported as the shape problem it is rather than as two keys the loader never asked for.
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
            IdWidth = Yaml.Int(id.Get("width"), 4),

            FilenamePattern = filenamePattern,
            FilenameRegex = CompilePattern(filenamePattern),
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
                    Aside = Yaml.Str(parts.Get("aside")) ?? "",
                    Level = Yaml.Int(parts.Get("level"), 3),
                    Columns = Yaml.StrList(parts.Get("columns")) is { Count: > 0 } cols
                        ? cols
                        : ["Id", "Clause"]
                }
                : null,

            Export = export.Present
                ? new ExportSpec
                {
                    Fields = Yaml.StrList(export.Get("fields")),
                    Sections =
                    [
                        .. Yaml.Map(export.Get("sections"))
                            .Select(e => (e.Item1, Yaml.Str(e.Item2)?.Trim() ?? ""))
                    ],
                    Parts = Yaml.Str(export.Get("parts")) ?? ""
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

    // Every field a document of the type is judged against, in declared order — the universal fields
    // first, then the type's own, with a name appearing in both resolved to the type's refinement of it.
    // A name that resolves to no declaration is dropped rather than carried as a hole.
    private static IReadOnlyList<FieldSpec> DeriveDeclaredFields(UniversalLayer layer,
        IReadOnlyList<string> fieldOrder, IReadOnlyDictionary<string, FieldSpec> fields) =>
    [
        .. layer.Order.Concat(fieldOrder).Distinct()
            .Select(n => Effective(fields, layer.Fields, n))
            .OfType<FieldSpec>()
    ];

    private static FieldSpec ParseField(Level node, string name,
        IReadOnlyDictionary<string, IReadOnlyList<string>> enums)
    {
        var pattern = Yaml.Str(node.Get("pattern"));
        string? problem = null;

        // `values: $enums.status` draws the vocabulary from _enums.yaml; a sequence declares it inline.
        // A name no enum answers to is a defect in the schema rather than a field with no range: the
        // declaration says the values are written down somewhere, and they are not.
        IReadOnlyList<string>? values = null;
        switch (node.Get("values"))
        {
            case YamlScalarNode { Value: { } v } when v.StartsWith("$enums.", StringComparison.Ordinal):
                var enumName = v["$enums.".Length..];
                values = enums.GetValueOrDefault(enumName);
                if (values is null)
                    problem = $"field '{name}' draws its values from '{v}', and _enums.yaml declares no "
                              + $"'{enumName}'.";
                break;
            case YamlSequenceNode seq:
                values = Yaml.StrList(seq);
                break;
        }

        // A scalar `ref:` is one folder, a sequence is several; both arrive as a list so that neither
        // form can be read as the absence of a ref.
        var refs = StrOrList(node.Get("ref"));

        // The condition is parsed here so that an unreadable one is reported against the field that
        // carries it. It is a defect in the schema either way: a condition nothing can read is one that
        // never holds, leaving the field quietly unrequired while the schema goes on claiming it is.
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

        return new FieldSpec
        {
            Name = name,
            Required = Yaml.Bool(node.Get("required")),
            RequiredWhen = requiredWhen,
            RequiredWhenCondition = condition,
            Type = Yaml.Str(node.Get("type")) ?? "string",
            Of = Yaml.Str(node.Get("of")),
            Values = values,
            Refs = refs,
            Reciprocal = Yaml.Str(node.Get("reciprocal")),
            Pattern = pattern,
            PatternRegex = CompilePattern(pattern),
            MirrorsSection = Yaml.Str(node.Get("mirrors-section")),
            AllowLiteral = Yaml.StrList(node.Get("allow-literal")),
            MinItems = Yaml.NullableInt(node.Get("min-items")),
            MinRecords = Yaml.NullableInt(node.Get("min-records")),
            Description = Collapse(Yaml.Str(node.Get("description"))),
            Notes = Collapse(Yaml.Str(node.Get("notes"))),
            Problem = problem
        };
    }

    // A severity the tool does not recognise reads as absent, which leaves the rule declared but not
    // enforced — the same state as a rule that names no severity at all, and the safe one: a check that
    // does not run is visible in `kac checks`, where a check that ran at a level nobody meant is not.
    //
    // A rule carrying an `expr:` is held to more, because it is a rule that claims to be finished. It
    // must compile, and it must say at what level it fires and what to tell the author, or it would load
    // as a check that can never report anything. Each of those is a defect in the schema rather than in
    // any document, and is recorded on the rule for SchemaChecks to report against the file that
    // declares it — the rule itself loading inert, so that one broken rule is one finding.
    private static RuleSpec ParseRule(YamlNode node, KeyReader keys)
    {
        // Read once outside the recorder to name the level, then everything through it.
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
                problem = $"rule '{id}' has an expr but no severity — say whether it fails "
                          + "a build or advises an author.";
            else if (string.IsNullOrEmpty(message))
                problem = $"rule '{id}' has an expr but no message — an author has to be "
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
            MaxWords = rule.Get("max-words") is YamlScalarNode { Value: { } mw } && int.TryParse(mw, out var words)
                ? words
                : null
        };
    }

    // A condition the vocabulary does not cover throws, and ParseField catches it against the field that
    // wrote it. Reading it as absent would be worse than useless: an unreadable condition is one that
    // never holds, so the field it guards is quietly never required while the schema claims it is.
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

    // The field a name resolves to for a given type: its own declaration where it has one, otherwise the
    // universal field it inherits. One definition, so the list derived at load and the lookup a check
    // makes cannot disagree about which field a name means.
    private static FieldSpec? Effective(IReadOnlyDictionary<string, FieldSpec> typeFields,
        IReadOnlyDictionary<string, FieldSpec> universal, string name) =>
        typeFields.TryGetValue(name, out var tf) ? tf : universal.GetValueOrDefault(name);

    public FieldSpec? EffectiveField(TypeSchema t, string name) => Effective(t.Fields, Universal, name);

    // A key the schema may write either way — one folder or several, one sort column or several. Both
    // arrive as a list, so that neither form can be read as the absence of the key: `Yaml.Str` answers
    // null for a sequence, so a scalar accessor over a key that may be written as one silently drops
    // every declaration that uses the list form.
    private static IReadOnlyList<string> StrOrList(YamlNode? node) => node is YamlSequenceNode
        ? Yaml.StrList(node)
        : Yaml.Str(node) is { } single
            ? [single]
            : [];

    // A pattern the schema declares, held as a Regex so the expression is parsed once at load rather
    // than looked up in the framework's cache on every value it is applied to. Interpreted rather than
    // RegexOptions.Compiled: a compiled pattern generates IL on first use, which a corpus matching it a
    // handful of times never earns back, and the two are level once volumes are high enough to matter.
    internal static Regex? CompilePattern(string? pattern) =>
        string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.CultureInvariant);

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [GeneratedRegex(@"^\s*(\S+)\s+in\s*\[(.*)\]\s*$")]
    private static partial Regex InListRegex();
}
