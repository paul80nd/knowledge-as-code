using kac.core;

// Every reference one type makes to another, held to naming a type this framework declares.
//
// `SchemaChecks` cannot ask this. A `ref:` or a `versus:` naming a folder no schema covers is a type the
// corpus turned down, and a corpus keeps no list of what it turned down, so the two readings arrive there
// looking the same. Here they do not: `.schema/` at this root is where every type is authored and every
// one of them is present, so a name nothing covers is misspelled.
//
// That is why this is a repository test. It answers for the schema this repository ships and never for
// the tool's logic, and a corpus that received a subset of `.schema/` carries none of it.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class SchemaReferenceTests
{
    private static readonly Schema Authored = Schema.Load(Repo.Root);

    // Named as the finding would name them, so a failure says which file and which declaration to open.
    private static IEnumerable<string> Dangling(Func<TypeSchema, IEnumerable<string>> targets) =>
        from type in Authored.ByFolder
        from target in targets(type.Value)
        where !Authored.ByFolder.ContainsKey(target)
        select $".schema/{type.Key}.yaml: {target}";

    [Fact]
    public void Every_ref_names_a_type_this_schema_declares()
        => Assert.Empty(Dangling(t => t.Fields.Values.SelectMany(field => field.Refs)));

    [Fact]
    public void Every_versus_names_a_type_this_schema_declares()
        => Assert.Empty(Dangling(t => t.Versus.Select(v => v.Other)));

    // The universal fields, which belong to no type and are checked under `_universal.yaml` in their own
    // right. One of them declaring a `ref:` reaches every record in every corpus.
    [Fact]
    public void Every_ref_a_universal_field_declares_names_a_type_this_schema_declares()
        => Assert.Empty(
            from field in Authored.Universal
            from target in field.Value.Refs
            where !Authored.ByFolder.ContainsKey(target)
            select $".schema/_universal.yaml: {field.Key} -> {target}");
}
