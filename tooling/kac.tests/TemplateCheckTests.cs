using kac.core;

namespace kac.tests;

public class TemplateCheckTests
{
    [Fact]
    public void A_template_carrying_the_type_s_required_fields_is_silent()
        => Assert.Empty(Fields(Template("id: \"adr-{{slug}}\"\nstatus: draft")));

    [Fact]
    public void A_template_with_no_frontmatter_says_a_copy_would_start_with_none()
        => Assert.Contains("a document copied from it starts with none",
            Assert.Single(Fields("# {{Title}}\n")).Message);

    [Fact]
    public void A_template_missing_a_required_field_names_the_check_a_copy_would_fail()
        => Assert.Contains("would fail required-field",
            Assert.Single(Fields(Template("id: \"adr-{{slug}}\""))).Message);

    [Fact]
    public void A_template_carrying_an_unknown_key_names_the_check_a_copy_would_fail()
        => Assert.Contains("would fail unknown-key",
            Assert.Single(Fields(Template("id: \"adr-{{slug}}\"\nstatus: draft\nowner: someone"))).Message);

    [Fact]
    public void A_template_the_corpus_does_not_yet_hold_is_still_read()
        => Assert.Contains("would fail required-field",
            Assert.Single(Fields(Template("id: \"adr-{{slug}}\""), tracked: false)).Message);

    private static string Template(string frontmatter) =>
        $"---\n{frontmatter}\n---\n\n# {{{{Title}}}}\n";

    private static readonly TypeSchema Adrs = new()
    {
        Key = "adrs",
        TypeName = "adrs",
        Folder = "adrs",
        Page = "adrs.md",
        KnownKeys = new HashSet<string>(["id", "status"], StringComparer.Ordinal),
        DeclaredFields =
        [
            new FieldSpec { Name = "id", Required = true },
            new FieldSpec { Name = "status", Required = true }
        ]
    };

    /// <summary>
    /// <c>tracked</c> says whether the corpus holds the template or it is merely on the disk of whoever wrote it.
    /// </summary>
    private static List<Finding> Fields(string template, bool tracked = true)
    {
        const string rel = "adrs/_template.md";
        var held = tracked ? new[] { "adrs.md", "adrs/0001-a.md", rel } : ["adrs.md", "adrs/0001-a.md"];

        var tree = new Tree(
            new HashSet<string>(held, StringComparer.Ordinal),
            path => path == rel ? template : "# A\n",
            path => held.Contains(path, StringComparer.Ordinal) || path == rel);

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal) { ["adrs"] = Adrs }
        };

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, new CorpusDescriptor()), Required.Today)
                .Where(f => f.Check.Value == "template-fields")
        ];
    }
}
