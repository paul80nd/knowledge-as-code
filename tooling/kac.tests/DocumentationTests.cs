// Two tables on the documentation site restate what the code holds: the facts an expression may call, and
// the checks the schema-load pass reports. Each carries hand-written prose that is the reason to read it,
// so what is held here are the names in them and the wording is left alone.
//
// A check renamed, retired or introduced is caught. An id growing a second way to fail is not, because
// nothing in the code tells one arm from another. The `schema-declarations` fixture pins those.

using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

public partial class DocumentationTests
{
    // The two schema pages of the site's design reference. They document the schema without travelling
    // with it, so a corpus that took a copy reads them at the published URL.
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

    // Both directions, because each is a different way for the page to lie. An id the catalogue does
    // not declare is a row describing a check that cannot fire; a `schema-` check with no row is a way
    // to fail the schema load that the page does not admit to.
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
