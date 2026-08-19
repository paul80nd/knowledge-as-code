// Two tables in `.schema/README.md` restate something the code already holds: the facts an expression
// may call, and the checks the schema-load pass reports. Neither is generated — each carries a column
// of hand-written prose that is the reason to read it at all — so what is held here are the *names* in
// them, and the wording is left alone.
//
// These read the repository rather than a value built in the test, which no other test here does. That
// is what they are for: the fault they exist to catch is a page going quietly out of step with the code
// beside it, and a page nobody opens is exactly where that happens.
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
    // The template, where the schema and the page describing it are authored. Every corpus receives a
    // copy of both, and `TemplateTests` holds those copies to matching.
    private static readonly string Template = Path.Combine(RepoRoot(), "template");
    private static readonly string Readme = File.ReadAllText(Path.Combine(Template, ".schema", "README.md"));

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
        var declared = Schema.Load(Template).Checks
            .Select(c => c.Id.Value)
            .Where(id => id.StartsWith("schema-", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        var cited = HeldToRow().Matches(Readme).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(declared.Order(StringComparer.Ordinal), cited.Order(StringComparer.Ordinal));
    }

    // The repository, found by the solution it holds. A corpus is what `kac` walks up for; what these tests
    // want is the tree carrying the engine and the schema page together, and one folder answers to that.
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "tooling", "kac.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException(
            "no 'tooling/kac.slnx' above the test assembly — these tests read the repository they ship in.");
    }
}
