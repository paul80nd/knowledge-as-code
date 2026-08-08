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
    public string? RequiredWhen { get; init; } // as written, for the message that quotes it
    public RequiredWhen? RequiredWhenCondition { get; init; } // as parsed, for the check that applies it
    public string Type { get; init; } = "string"; // string|date|enum|id|list|bool|int
    public string? Of { get; init; } // element type when Type == list
    public IReadOnlyList<string>? Values { get; init; } // enum values (resolved)

    // The folders an id in this field may belong to. A list because several fields point at more than
    // one type — a discovery is promoted to a FAQ or a standard — and a scalar `ref:` is the one-entry
    // case rather than a separate shape.
    public IReadOnlyList<string> Refs { get; init; } = [];

    public string? Reciprocal { get; init; } // field on the target that must point back
    public string? Pattern { get; init; }
    public Regex? PatternRegex { get; init; } // Pattern compiled — the message still quotes the source string
    public string? MirrorsSection { get; init; } // section whose ids this field must mirror

    // Words admitted beside the field's declared type — `applies-to: [all]`, `last-rehearsed: never`. A
    // value listed here is taken as written and nothing further is asked of it, which is what lets a date
    // field say "never" and a list of ids say "all" without either widening into a string field and
    // losing the checks that hold every other value it carries.
    public IReadOnlyList<string> AllowLiteral { get; init; } = [];

    // The floor on a list field's length, where the schema sets one. A field whose whole purpose is to be
    // over-filled — a FAQ's search keywords — has nothing else holding it to more than one entry.
    public int? MinItems { get; init; }

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

// The clause table a type's normative section carries — one addressable obligation per row, cited from
// elsewhere as `pol-VURM.TIMEBOX`. Held as its own spec so a type gains clauses by declaring them and a
// type that declares none is simply never checked for any.
//
// The modal orderings are derived here, once per type, because every clause row of every document reads
// them and they never differ between two rows of the same type.
public sealed class ClauseSpec(string idPattern, List<string> binding, List<string> advisory)
{
    public string Section { get; init; } = "Clauses";

    // The table's headers, in order. The first two are read positionally as the id and the clause; any
    // further column is the type's own — `Alignment` on a policy — and is checked for being there and
    // named right, its contents being prose the schema has no view on.
    public IReadOnlyList<string> Columns { get; init; } = ["Id", "Clause"];

    public string IdPattern { get; } = idPattern;
    public Regex? IdPatternRegex { get; } = Schema.CompilePattern(idPattern);

    public IReadOnlyList<string> Binding { get; } = binding;  // written bold — these oblige
    public IReadOnlyList<string> Advisory { get; } = advisory; // written plain — these recommend

    // The order rows must appear in: binding levels before advisory ones, each as the type declares it.
    private readonly List<string> levels = [.. binding, .. advisory];
    public IReadOnlyList<string> Levels => levels;

    // Where a modal sits in that order, or -1 for a modal the type does not declare.
    public int Rank(string modal) => levels.IndexOf(modal);

    // Longest first, so "MUST NOT" is recognised before the "MUST" that prefixes it.
    public IReadOnlyList<string> ModalsLongestFirst { get; } =
        [.. binding.Concat(advisory).OrderByDescending(m => m.Length)];
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
    public required string Id { get; init; }

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
    public string TypeName { get; init; } = "";
    public string Label { get; init; } = "";
    public string Folder { get; init; } = "";
    public string Page { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Lifecycle { get; init; } = "";
    public string Shape { get; init; } = CollectionShape;
    public string IdPrefix { get; init; } = "";
    public string IdStyle { get; init; } = "";
    public string IdValue { get; init; } = "";
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
    public ClauseSpec? Clauses { get; init; }
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

    // The two shapes a type can take. Most are a folder of records; the glossary is one document read
    // end to end. Declared rather than inferred: an absent `folder:` and a deliberate `folder: null`
    // are the same string once parsed, so inferring the shape from the folder cannot tell a
    // single-document type from a collection whose folder key was lost.
    public const string CollectionShape = "collection";
    public const string SingleDocumentShape = "single-document";

    public bool IsSingleDocument => Shape == SingleDocumentShape;

    // How a single document of this type is named in generated prose — "Policy", "ADR", "NFR". Declared
    // by the schema rather than derived, because no rule turns `policy` into "Policy" and `adr` into
    // "ADR" without knowing which names are initialisms. The fallbacks keep a schema that omits `label`
    // rendering something sensible.
    public string DisplayName =>
        !string.IsNullOrEmpty(Label) ? Label
        : !string.IsNullOrEmpty(TypeName) ? char.ToUpperInvariant(TypeName[0]) + TypeName[1..]
        : IdPrefix.ToUpperInvariant();

    // Whether this type declares a given rule. The reader-facing checks table uses it to show a
    // rule's row only on the pages whose schema actually carries the rule.
    public bool HasRule(string id) => Rules.Any(r => string.Equals(r.Id, id, StringComparison.Ordinal));

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

public sealed class Schema
{
    public IReadOnlyList<string> UniversalOrder { get; init; } = [];
    public IReadOnlyDictionary<string, FieldSpec> Universal { get; init; } = new Dictionary<string, FieldSpec>();
    public IReadOnlyList<string> Reserved { get; init; } = [];
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Enums { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();
    public IReadOnlyDictionary<string, TypeSchema> ByFolder { get; init; } = new Dictionary<string, TypeSchema>();

    // The parts of the schema every type is read against: the shared field declarations, the reserved
    // keys, and the enum vocabularies a field can draw on. Carried as one argument into type parsing
    // because a type's derived sets span its own declarations and these together.
    private sealed record UniversalLayer(
        IReadOnlyList<string> Order,
        IReadOnlyDictionary<string, FieldSpec> Fields,
        IReadOnlyList<string> Reserved,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Enums);

    public static Schema Load(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, ".schema");

        var enumsRoot = Yaml.LoadFile(Path.Combine(dir, "_enums.yaml"));
        var enums = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var (name, node) in Yaml.Map(Yaml.Get(enumsRoot, "enums")))
            enums[name] = Yaml.StrList(Yaml.Get(node, "values"));

        var uni = Yaml.LoadFile(Path.Combine(dir, "_universal.yaml"));
        var universalOrder = new List<string>();
        var universal = new Dictionary<string, FieldSpec>();
        foreach (var (name, node) in Yaml.Map(Yaml.Get(uni, "fields")))
        {
            universalOrder.Add(name);
            universal[name] = ParseField(name, node, enums);
        }

        var layer = new UniversalLayer(universalOrder, universal, Yaml.StrList(Yaml.Get(uni, "reserved")), enums);

        var byFolder = new Dictionary<string, TypeSchema>();
        foreach (var file in Directory.GetFiles(dir, "*.yaml").OrderBy(f => f))
        {
            var baseName = Path.GetFileNameWithoutExtension(file);
            if (baseName.StartsWith('_')) continue; // _universal, _enums
            byFolder[baseName] = ParseType(Yaml.LoadFile(file), layer);
        }

        return new Schema
        {
            UniversalOrder = layer.Order,
            Universal = layer.Fields,
            Reserved = layer.Reserved,
            Enums = layer.Enums,
            ByFolder = byFolder
        };
    }

    private static TypeSchema ParseType(YamlNode root, UniversalLayer layer)
    {
        var id = Yaml.Get(root, "id");
        var fn = Yaml.Get(root, "filename");
        var sections = Yaml.Get(root, "sections");
        var index = Yaml.Get(root, "index");

        var fieldOrder = new List<string>();
        var fields = new Dictionary<string, FieldSpec>();
        foreach (var (name, node) in Yaml.Map(Yaml.Get(root, "fields")))
        {
            fieldOrder.Add(name);
            fields[name] = ParseField(name, node, layer.Enums);
        }

        var rules = Yaml.Get(root, "rules") is YamlSequenceNode ruleNodes
            ? ruleNodes.Children.Select(ParseRule).ToList()
            : [];

        var filenamePattern = fn is null ? null : Yaml.Str(Yaml.Get(fn, "pattern"));

        return new TypeSchema
        {
            TypeName = Yaml.Str(Yaml.Get(root, "type")) ?? "",
            Label = Yaml.Str(Yaml.Get(root, "label")) ?? "",
            Folder = Yaml.Str(Yaml.Get(root, "folder")) ?? "",
            Page = Yaml.Str(Yaml.Get(root, "page")) ?? "",
            Tier = Yaml.Str(Yaml.Get(root, "tier")) ?? "",
            Lifecycle = Yaml.Str(Yaml.Get(root, "lifecycle")) ?? "",
            Shape = Yaml.Str(Yaml.Get(root, "shape")) ?? TypeSchema.CollectionShape,

            IdPrefix = id is null ? "" : Yaml.Str(Yaml.Get(id, "prefix")) ?? "",
            IdStyle = id is null ? "" : Yaml.Str(Yaml.Get(id, "style")) ?? "",
            IdWidth = id is null ? 0 : Yaml.Int(Yaml.Get(id, "width"), 4),
            IdValue = id is null ? "" : Yaml.Str(Yaml.Get(id, "value")) ?? "",

            FilenamePattern = filenamePattern,
            FilenameRegex = CompilePattern(filenamePattern),
            SlugMax = fn is null ? 30 : Yaml.Int(Yaml.Get(fn, "slug-max"), 30),

            FieldOrder = fieldOrder,
            Fields = fields,

            RequiredSections = sections is null ? [] : Yaml.StrList(Yaml.Get(sections, "required")),
            OptionalSections = sections is null ? [] : Yaml.StrList(Yaml.Get(sections, "optional")),

            Clauses = Yaml.Get(root, "clauses") is { } clauses
                ? new ClauseSpec(
                    Yaml.Str(Yaml.Get(clauses, "id-pattern")) ?? "",
                    Yaml.StrList(Yaml.Get(clauses, "binding")),
                    Yaml.StrList(Yaml.Get(clauses, "advisory")))
                {
                    Section = Yaml.Str(Yaml.Get(clauses, "section")) ?? "Clauses",
                    Columns = Yaml.StrList(Yaml.Get(clauses, "columns")) is { Count: > 0 } cols
                        ? cols
                        : ["Id", "Clause"]
                }
                : null,

            IndexColumns = index is null ? [] : Yaml.StrList(Yaml.Get(index, "columns")),
            IndexSort = index is null ? [] : StrOrList(Yaml.Get(index, "sort")),
            IndexOrder = index is null ? "" : Yaml.Str(Yaml.Get(index, "order")) ?? "",

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

    private static FieldSpec ParseField(string name, YamlNode node,
        IReadOnlyDictionary<string, IReadOnlyList<string>> enums)
    {
        var pattern = Yaml.Str(Yaml.Get(node, "pattern"));
        string? problem = null;

        // `values: $enums.status` draws the vocabulary from _enums.yaml; a sequence declares it inline.
        // A name no enum answers to is a defect in the schema rather than a field with no range: the
        // declaration says the values are written down somewhere, and they are not.
        IReadOnlyList<string>? values = null;
        switch (Yaml.Get(node, "values"))
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
        var refs = StrOrList(Yaml.Get(node, "ref"));

        // The condition is parsed here so that an unreadable one is reported against the field that
        // carries it. It is a defect in the schema either way: a condition nothing can read is one that
        // never holds, leaving the field quietly unrequired while the schema goes on claiming it is.
        var requiredWhen = Yaml.Str(Yaml.Get(node, "required-when"));
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
            Required = Yaml.Bool(Yaml.Get(node, "required")),
            RequiredWhen = requiredWhen,
            RequiredWhenCondition = condition,
            Type = Yaml.Str(Yaml.Get(node, "type")) ?? "string",
            Of = Yaml.Str(Yaml.Get(node, "of")),
            Values = values,
            Refs = refs,
            Reciprocal = Yaml.Str(Yaml.Get(node, "reciprocal")),
            Pattern = pattern,
            PatternRegex = CompilePattern(pattern),
            MirrorsSection = Yaml.Str(Yaml.Get(node, "mirrors-section")),
            AllowLiteral = Yaml.StrList(Yaml.Get(node, "allow-literal")),
            MinItems = Yaml.NullableInt(Yaml.Get(node, "min-items")),
            Description = Collapse(Yaml.Str(Yaml.Get(node, "description"))),
            Notes = Collapse(Yaml.Str(Yaml.Get(node, "notes"))),
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
    private static RuleSpec ParseRule(YamlNode node)
    {
        var id = Yaml.Str(Yaml.Get(node, "id")) ?? "";
        var source = Yaml.Str(Yaml.Get(node, "expr"));
        var severity = Yaml.Str(Yaml.Get(node, "severity")) switch
        {
            "error" => Sev.Error,
            "warning" => Sev.Warning,
            _ => (Sev?)null
        };
        var message = Collapse(Yaml.Str(Yaml.Get(node, "message")));

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
            Id = id,
            Description = Collapse(Yaml.Str(Yaml.Get(node, "description"))),
            Severity = severity,
            Expr = source,
            Compiled = compiled,
            Message = message,
            Problem = problem,
            MaxWords = Yaml.Get(node, "max-words") is YamlScalarNode { Value: { } mw } && int.TryParse(mw, out var words)
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

        var inList = System.Text.RegularExpressions.Regex.Match(
            condition, @"^\s*(\S+)\s+in\s*\[(.*)\]\s*$");
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
        : Yaml.Str(node) is { } single ? [single] : [];

    // A pattern the schema declares, held as a Regex so the expression is parsed once at load rather
    // than looked up in the framework's cache on every value it is applied to. Interpreted rather than
    // RegexOptions.Compiled: a compiled pattern generates IL on first use, which a corpus matching it a
    // handful of times never earns back, and the two are level once volumes are high enough to matter.
    internal static Regex? CompilePattern(string? pattern) =>
        string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.CultureInvariant);

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
