using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class SchemaReferenceTests
{
    private static readonly Schema Authored = Schema.Load(Repo.Root);

    private static IEnumerable<string> Dangling(Func<TypeSchema, IEnumerable<string>> targets) =>
        from type in Authored.ByFolder
        from target in targets(type.Value)
        where !Authored.ByFolder.ContainsKey(target)
        select $".schema/{type.Key}.yaml: {target}";

    [Fact]
    public void This_schema_declares_nothing_the_tool_cannot_act_on()
    {
        var found = new List<Finding>();
        SchemaChecks.Check(Authored, found);

        Assert.Empty(found);
    }

    [Fact]
    public void Every_ref_names_a_type_this_schema_declares()
        => Assert.Empty(Dangling(t => t.Fields.Values.SelectMany(field => field.Refs)));

    [Fact]
    public void Every_versus_names_a_type_this_schema_declares()
        => Assert.Empty(Dangling(t => t.Versus.Select(v => v.Other)));

    [Fact]
    public void Every_ref_a_universal_field_declares_names_a_type_this_schema_declares()
        => Assert.Empty(
            from field in Authored.Universal
            from target in field.Value.Refs
            where !Authored.ByFolder.ContainsKey(target)
            select $".schema/_universal.yaml: {field.Key} -> {target}");
}
