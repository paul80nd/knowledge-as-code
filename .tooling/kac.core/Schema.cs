using YamlDotNet.RepresentationModel;

namespace kac.core;
// ---------------------------------------------------------------------------
// Schema model — loaded from .schema/*.yaml
// ---------------------------------------------------------------------------

public class FieldSpec
{
    public required string Name;
    public bool Required;
    public string? RequiredWhen; // e.g. "status == accepted"
    public string Type = "string"; // string|date|enum|id|list|bool|int
    public string? Of; // element type when Type == list
    public List<string>? Values; // enum values (resolved)
    public string? Ref; // folder the id must belong to
    public string? Reciprocal; // field on the target that must point back
    public string? Pattern;
    public string? MirrorsSection; // section whose ids this field must mirror

    // Two audiences, deliberately separate. `Description` is what a reader needs at a glance and is
    // what the generated Metadata table renders; `Notes` is the longer why-it-exists, which belongs
    // in the schema where there is room for it. A field with only Notes falls back to them, so the
    // two can be adopted a schema at a time rather than all at once.
    public string? Description;
    public string? Notes;

    public string? TableText => string.IsNullOrEmpty(Description) ? Notes : Description;
}

public class TypeSchema
{
    public string TypeName = "", Label = "", Folder = "", Page = "", Tier = "", Lifecycle = "";
    public string IdPrefix = "", IdStyle = "";
    public int IdWidth;
    public string? FilenamePattern;
    public int SlugMax = 30;
    public string? H1Pattern;
    public bool IdAsCode;
    public List<string> FieldOrder = [];
    public Dictionary<string, FieldSpec> Fields = [];
    public List<string> RequiredSections = [];
    public List<string> OptionalSections = [];
    public List<string> IndexColumns = [];
    public string? IndexSort;
    public readonly List<Dictionary<string, object>> Rules = [];

    // How a single document of this type is named in generated prose — "Policy", "ADR", "NFR". The
    // schema declares it rather than the generator deriving it, because there is no rule that turns
    // `policy` into "Policy" and `adr` into "ADR" without knowing which names are initialisms. The
    // fallbacks exist so a type schema that predates the field still renders something sensible.
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
}

public class Schema
{
    public List<string> UniversalOrder = [];
    public Dictionary<string, FieldSpec> Universal = [];
    public List<string> Reserved = [];
    public readonly Dictionary<string, List<string>> Enums = [];
    public readonly Dictionary<string, TypeSchema> ByFolder = [];

    public static Schema Load(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, ".schema");
        var s = new Schema();

        var enumsRoot = Yaml.LoadFile(Path.Combine(dir, "_enums.yaml"));
        foreach (var (name, node) in Yaml.Map(Yaml.Get(enumsRoot, "enums")))
            s.Enums[name] = Yaml.StrList(Yaml.Get(node, "values"));

        var uni = Yaml.LoadFile(Path.Combine(dir, "_universal.yaml"));
        foreach (var (name, node) in Yaml.Map(Yaml.Get(uni, "fields")))
        {
            s.UniversalOrder.Add(name);
            s.Universal[name] = ParseField(name, node, s);
        }

        s.Reserved = Yaml.StrList(Yaml.Get(uni, "reserved"));

        foreach (var file in Directory.GetFiles(dir, "*.yaml").OrderBy(f => f))
        {
            var baseName = Path.GetFileNameWithoutExtension(file);
            if (baseName.StartsWith('_')) continue; // _universal, _enums
            s.ByFolder[baseName] = ParseType(Yaml.LoadFile(file), s);
        }

        return s;
    }

    private static TypeSchema ParseType(YamlNode root, Schema s)
    {
        var t = new TypeSchema
        {
            TypeName = Yaml.Str(Yaml.Get(root, "type")) ?? "",
            Label = Yaml.Str(Yaml.Get(root, "label")) ?? "",
            Folder = Yaml.Str(Yaml.Get(root, "folder")) ?? "",
            Page = Yaml.Str(Yaml.Get(root, "page")) ?? "",
            Tier = Yaml.Str(Yaml.Get(root, "tier")) ?? "",
            Lifecycle = Yaml.Str(Yaml.Get(root, "lifecycle")) ?? ""
        };

        var id = Yaml.Get(root, "id");
        if (id is not null)
        {
            t.IdPrefix = Yaml.Str(Yaml.Get(id, "prefix")) ?? "";
            t.IdStyle = Yaml.Str(Yaml.Get(id, "style")) ?? "";
            t.IdWidth = Yaml.Int(Yaml.Get(id, "width"), 4);
        }

        var fn = Yaml.Get(root, "filename");
        if (fn is not null)
        {
            t.FilenamePattern = Yaml.Str(Yaml.Get(fn, "pattern"));
            t.SlugMax = Yaml.Int(Yaml.Get(fn, "slug-max"), 30);
        }

        var title = Yaml.Get(root, "title");
        if (title is not null)
        {
            t.H1Pattern = Yaml.Str(Yaml.Get(title, "h1-pattern"));
            t.IdAsCode = Yaml.Bool(Yaml.Get(title, "id-as-code"));
        }

        foreach (var (name, node) in Yaml.Map(Yaml.Get(root, "fields")))
        {
            t.FieldOrder.Add(name);
            t.Fields[name] = ParseField(name, node, s);
        }

        var sections = Yaml.Get(root, "sections");
        if (sections is not null)
        {
            t.RequiredSections = Yaml.StrList(Yaml.Get(sections, "required"));
            t.OptionalSections = Yaml.StrList(Yaml.Get(sections, "optional"));
        }

        var index = Yaml.Get(root, "index");
        if (index is not null)
        {
            t.IndexColumns = Yaml.StrList(Yaml.Get(index, "columns"));
            t.IndexSort = Yaml.Str(Yaml.Get(index, "sort"));
        }

        if (Yaml.Get(root, "rules") is YamlSequenceNode rules)
            foreach (var r in rules.Children)
                t.Rules.Add(Yaml.Map(r).ToDictionary(x => x.Item1, object (x) => x.Item2));

        return t;
    }

    private static FieldSpec ParseField(string name, YamlNode node, Schema s)
    {
        var f = new FieldSpec
        {
            Name = name,
            Required = Yaml.Bool(Yaml.Get(node, "required")),
            RequiredWhen = Yaml.Str(Yaml.Get(node, "required-when")),
            Type = Yaml.Str(Yaml.Get(node, "type")) ?? "string",
            Of = Yaml.Str(Yaml.Get(node, "of")),
            Ref = Yaml.Str(Yaml.Get(node, "ref")),
            Reciprocal = Yaml.Str(Yaml.Get(node, "reciprocal")),
            Pattern = Yaml.Str(Yaml.Get(node, "pattern")),
            MirrorsSection = Yaml.Str(Yaml.Get(node, "mirrors-section")),
            Description = Collapse(Yaml.Str(Yaml.Get(node, "description"))),
            Notes = Collapse(Yaml.Str(Yaml.Get(node, "notes")))
        };

        var values = Yaml.Get(node, "values");
        switch (values)
        {
            case YamlScalarNode sc when (sc.Value?.StartsWith("$enums.") == true):
                s.Enums.TryGetValue(sc.Value["$enums.".Length..], out f.Values);
                break;
            case YamlSequenceNode:
                f.Values = Yaml.StrList(values);
                break;
        }
        return f;
    }

    // Merge the universal status enum (per-type override) into the type's status field.
    public FieldSpec? EffectiveField(TypeSchema t, string name) =>
        t.Fields.TryGetValue(name, out var tf) ? tf : Universal.GetValueOrDefault(name);

    public IEnumerable<string> KnownKeys(TypeSchema t) =>
        UniversalOrder.Concat(t.FieldOrder).Concat(Reserved).Distinct();

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
