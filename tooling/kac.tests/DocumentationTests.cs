// Two tables in `.schema/README.md` restate something the code already holds: the facts an expression
// may call, and the checks the schema-load pass reports. Neither is generated: each carries a column
// of hand-written prose that is the reason to read it at all. So what is held here are the *names* in
// them, and the wording is left alone.
//
// These read the repository rather than a value built in the test. That is what they are for: the fault
// they exist to catch is a page going quietly out of step with the code beside it, and a page nobody
// opens is exactly where that happens.
//
// The reach is the names and no further. A row per *check id* is held, so a check that is renamed,
// retired or introduced is caught; an id that grows a second way to fail is not, because nothing in the
// code distinguishes one arm from another. That belongs to the `schema-declarations` fixture, which
// trips the arms and pins what each one says.

using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

public partial class DocumentationTests
{
    // The page describing the schema, beside the schema it describes at the repository root. One copy
    // serves the template and `example/` alike.
    private static readonly string Readme =
        File.ReadAllText(Path.Combine(Repo.Root, ".schema", "README.md"));

    // A row of the fact table opens with the call it documents: `| \`section_count('Title')\` | int |`.
    [GeneratedRegex(@"^\| `([a-z_]+)\(", RegexOptions.Multiline)]
    private static partial Regex FactRow();

    // A row of the held-to table closes with the check it reports under.
    [GeneratedRegex(@"\| `(schema-[a-z-]+)`\s*\|\s*$", RegexOptions.Multiline)]
    private static partial Regex HeldToRow();

    [Fact]
    public void The_fact_table_documents_every_callable_fact()
    {
        var documented = FactRow().Matches(Readme).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);

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

        var cited = HeldToRow().Matches(Readme).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(declared.Order(StringComparer.Ordinal), cited.Order(StringComparer.Ordinal));
    }
}
