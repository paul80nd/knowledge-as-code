using kac.core;

namespace kac.tests;

public class TypeSetupTests
{
    [Fact]
    public void A_type_with_a_page_a_folder_and_a_template_is_silent()
        => Assert.Empty(Setup(Holding("adrs.md", "adrs/0001-a.md", "adrs/_template.md")));

    [Fact]
    public void A_type_the_corpus_has_not_built_is_not_reported()
        => Assert.Empty(Setup(Holding("README.md")));

    [Fact]
    public void A_page_with_no_folder_beside_it_is_reported_as_half_a_type()
        => Assert.Contains("a type is set up as both or neither",
            Assert.Single(Setup(Holding("adrs.md"))).Message);

    [Fact]
    public void A_folder_with_no_page_names_the_page_to_add()
        => Assert.Contains("Add adrs.md",
            Assert.Single(Setup(Holding("adrs/0001-a.md", "adrs/_template.md"))).Message);

    [Fact]
    public void A_folder_with_no_template_names_the_template_to_add()
        => Assert.Contains("Add adrs/_template.md",
            Assert.Single(Setup(Holding("adrs.md", "adrs/0001-a.md"))).Message);

    [Fact]
    public void An_untracked_template_counts_as_present()
        => Assert.Empty(Setup(Holding(["adrs.md", "adrs/0001-a.md"], onDisk: "adrs/_template.md")));

    [Fact]
    public void An_untracked_page_does_not_stand_the_type_up()
        => Assert.Contains("Add adrs.md",
            Assert.Single(Setup(Holding(["adrs/0001-a.md", "adrs/_template.md"], onDisk: "adrs.md"))).Message);

    [Fact]
    public void A_type_adopted_and_not_stood_up_is_reported_as_work_outstanding()
        => Assert.Contains("is adopted here and is not stood up",
            Assert.Single(Setup(Holding("README.md"), "adrs")).Message);

    [Fact]
    public void A_type_stood_up_and_not_adopted_is_reported_as_a_contradiction()
        => Assert.Contains("is not in 'types:'",
            Assert.Single(Setup(
                Holding("adrs.md", "adrs/0001-a.md", "adrs/_template.md",
                    "policies.md", "policies/0001-a.md", "policies/_template.md"),
                "policies")).Message);

    [Fact]
    public void A_type_adopted_that_no_schema_covers_names_the_file_that_would_cover_it()
        => Assert.Contains(".schema/widgets.yaml",
            Setup(Holding("README.md"), "adrs", "widgets").Select(f => f.Message).First(m => m.Contains("widgets")));

    /// <summary>Two types, because the last case needs a corpus that has adopted something.</summary>
    private static TypeSchema Type(string key) => new()
    {
        Key = key,
        TypeName = key,
        Folder = key,
        Page = $"{key}.md"
    };

    private static Tree Holding(params string[] held) => Holding(held, null);

    /// <summary>
    /// <c>onDisk</c> is the file that is there and untracked, which is the state that separates the pass's two presence
    /// questions.
    /// </summary>
    private static Tree Holding(string[] held, string? onDisk) => new(
        new HashSet<string>(held, StringComparer.Ordinal),
        rel => rel.EndsWith("_template.md") ? "# Template\n" : "# A\n",
        rel => held.Contains(rel, StringComparer.Ordinal) || rel == onDisk);

    /// <summary>
    /// Every <c>type-setup</c> finding the corpus produces. <c>declared</c> is the corpus's <c>types:</c> block, and
    /// none means the corpus has not declared one.
    /// </summary>
    private static List<Finding> Setup(Tree tree, params string[] declared)
    {
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
            {
                ["adrs"] = Type("adrs"),
                ["policies"] = Type("policies")
            }
        };
        var descriptor = new CorpusDescriptor { Types = declared.Length > 0 ? [.. declared] : null };

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, descriptor), Required.Today)
                .Where(f => f.Check.Value == "type-setup")
        ];
    }
}
