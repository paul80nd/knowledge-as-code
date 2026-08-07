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
    public string? RequiredWhen { get; init; } // e.g. "status == accepted"
    public string Type { get; init; } = "string"; // string|date|enum|id|list|bool|int
    public string? Of { get; init; } // element type when Type == list
    public IReadOnlyList<string>? Values { get; init; } // enum values (resolved)
    public string? Ref { get; init; } // folder the id must belong to
    public string? Reciprocal { get; init; } // field on the target that must point back
    public string? Pattern { get; init; }
    public Regex? PatternRegex { get; init; } // Pattern compiled — the message still quotes the source string
    public string? MirrorsSection { get; init; } // section whose ids this field must mirror

    // Two audiences, deliberately separate. `Description` is what a reader needs at a glance and is what
    // the generated Metadata table renders; `Notes` is the longer why-it-exists, which belongs in the
    // schema where there is room for it. A field with only Notes falls back to them.
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public string? TableText => string.IsNullOrEmpty(Description) ? Notes : Description;
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
    public string? IndexSort { get; init; }
    public ClauseSpec? Clauses { get; init; }
    public IReadOnlyList<Dictionary<string, object>> Rules { get; init; } = [];

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
    public bool HasRule(string id) =>
        Rules.Any(r => r.TryGetValue("id", out var rid) && string.Equals(rid.ToString(), id, StringComparison.Ordinal));

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

        var rules = new List<Dictionary<string, object>>();
        if (Yaml.Get(root, "rules") is YamlSequenceNode ruleNodes)
            rules.AddRange(ruleNodes.Children.Select(r => Yaml.Map(r).ToDictionary(x => x.Item1, object (x) => x.Item2)));

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
            IndexSort = index is null ? null : Yaml.Str(Yaml.Get(index, "sort")),

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

        // `values: $enums.status` draws the vocabulary from _enums.yaml; a sequence declares it inline.
        // A name no enum answers to leaves the field without values, which reads downstream as a field
        // whose range the schema does not constrain.
        IReadOnlyList<string>? values = Yaml.Get(node, "values") switch
        {
            YamlScalarNode { Value: { } v } when v.StartsWith("$enums.", StringComparison.Ordinal) =>
                enums.GetValueOrDefault(v["$enums.".Length..]),
            YamlSequenceNode seq => Yaml.StrList(seq),
            _ => null
        };

        return new FieldSpec
        {
            Name = name,
            Required = Yaml.Bool(Yaml.Get(node, "required")),
            RequiredWhen = Yaml.Str(Yaml.Get(node, "required-when")),
            Type = Yaml.Str(Yaml.Get(node, "type")) ?? "string",
            Of = Yaml.Str(Yaml.Get(node, "of")),
            Values = values,
            Ref = Yaml.Str(Yaml.Get(node, "ref")),
            Reciprocal = Yaml.Str(Yaml.Get(node, "reciprocal")),
            Pattern = pattern,
            PatternRegex = CompilePattern(pattern),
            MirrorsSection = Yaml.Str(Yaml.Get(node, "mirrors-section")),
            Description = Collapse(Yaml.Str(Yaml.Get(node, "description"))),
            Notes = Collapse(Yaml.Str(Yaml.Get(node, "notes")))
        };
    }

    // The field a name resolves to for a given type: its own declaration where it has one, otherwise the
    // universal field it inherits. One definition, so the list derived at load and the lookup a check
    // makes cannot disagree about which field a name means.
    private static FieldSpec? Effective(IReadOnlyDictionary<string, FieldSpec> typeFields,
        IReadOnlyDictionary<string, FieldSpec> universal, string name) =>
        typeFields.TryGetValue(name, out var tf) ? tf : universal.GetValueOrDefault(name);

    public FieldSpec? EffectiveField(TypeSchema t, string name) => Effective(t.Fields, Universal, name);

    // A pattern the schema declares, held as a Regex so the expression is parsed once at load rather
    // than looked up in the framework's cache on every value it is applied to. Interpreted rather than
    // RegexOptions.Compiled: a compiled pattern generates IL on first use, which a corpus matching it a
    // handful of times never earns back, and the two are level once volumes are high enough to matter.
    internal static Regex? CompilePattern(string? pattern) =>
        string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.CultureInvariant);

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
