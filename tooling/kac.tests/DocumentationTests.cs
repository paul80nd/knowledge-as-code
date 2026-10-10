// A check renamed, retired or introduced is caught. An id growing a second way to fail is not, because
// nothing in the code tells one arm from another. The `schema-declarations` fixture pins those.

using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class DocumentationTests
{
    private static string Page(string name) =>
        File.ReadAllText(Path.Combine(Repo.Root, "docs", "design", name));

    // A row of the fact table opens with the call it documents: `| \`section_count('Title')\` | int |`.
    [GeneratedRegex(@"^\| `([a-z_]+)\(", RegexOptions.Multiline)]
    private static partial Regex FactRow();

    // A row of the held-to table closes with the check it reports under.
    [GeneratedRegex(@"\| `(schema-[a-z-]+)`\s*\|\s*$", RegexOptions.Multiline)]
    private static partial Regex HeldToRow();

    [Fact]
    public void The_fact_table_documents_every_callable_fact()
    {
        var documented = FactRow().Matches(Page("expressions.md"))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            RuleExpr.FunctionNames.Order(StringComparer.Ordinal),
            documented.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void The_held_to_table_names_every_check_the_schema_pass_reports()
    {
        var declared = Schema.Load(Repo.Root).Checks
            .Select(c => c.Id.Value)
            .Where(id => id.StartsWith("schema-", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        var cited = HeldToRow().Matches(Page("held-to.md"))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(declared.Order(StringComparer.Ordinal), cited.Order(StringComparer.Ordinal));
    }
}
