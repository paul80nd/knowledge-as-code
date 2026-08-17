using kac.core;

// In-process unit tests for `framework-names-types`, the one rule the framework's own documentation is
// held to.
//
// Those documents are shared byte-for-byte by every corpus running the framework, so they have to read
// correctly in a corpus that adopted three types and in one that adopted seventeen. A link to a type page
// cannot: it either resolves or is a dead end, depending on a decision the page cannot see. The defect is
// therefore invisible in the corpus that writes it, where every type page exists — which is also why no
// fixture can show that the right files are being read. This is where that is asserted.

namespace kac.tests;

public class FrameworkDocTests
{
    [Fact]
    public void Naming_a_type_without_linking_to_it_is_silent()
        => Assert.Empty(Framework(("knowledge-as-code.md", "# The framework\n\nAn ADR records a decision.\n")));

    [Fact]
    public void A_link_to_a_type_page_says_to_name_the_type_instead()
    {
        var finding = Assert.Single(Framework(
            ("knowledge-as-code.md", "# The framework\n\nSee the [ADRs](/adrs).\n")));

        Assert.Equal("framework-names-types", finding.Check.Value);
        Assert.Contains("Name the type instead", finding.Message);
    }

    // The worse of the two, and worth its own words: every corpus is told to delete the records it
    // inherits, so a link to one dies even where the type was adopted.
    [Fact]
    public void A_link_to_a_record_inside_a_type_is_reported_as_the_worse_fault()
        => Assert.Contains("the first thing a corpus deletes",
            Assert.Single(Framework(
                ("knowledge-as-code.md", "# The framework\n\nSee [one](/adrs/0001-a.md).\n"))).Message);

    // A generated block is written against this corpus rather than against the framework, so its links are
    // this corpus's and are right by construction.
    [Fact]
    public void A_link_inside_a_generated_block_is_exempt()
        => Assert.Empty(Framework(("knowledge-as-code.md",
            "# The framework\n\n<!-- BEGIN GENERATED: types-index -->\n[ADRs](/adrs)\n"
            + "<!-- END GENERATED: types-index -->\n")));

    // -- which files are read --

    // The glob is what finds these, so a document inside the framework's folder is read as the root one is.
    [Fact]
    public void A_document_inside_the_framework_s_folder_is_read()
        => Assert.Equal("knowledge-as-code/style.md",
            Assert.Single(Framework(
                ("knowledge-as-code/style.md", "# Style\n\nSee the [ADRs](/adrs).\n"))).File);

    // The framework's own glossary is a record, filed under a type and validated like any other. It is read
    // here as well because it is shared byte-for-byte, and the link pass is not run over it twice.
    [Fact]
    public void The_shared_glossary_is_read_although_it_is_also_a_record()
        => Assert.Equal("glossary/knowledge-as-code.md",
            Assert.Single(Framework(
                ("glossary/knowledge-as-code.md",
                    "---\nid: gls-knowledge-as-code\n---\n\n# Words\n\nSee the [ADRs](/adrs).\n"))).File);

    // A document the corpus does not hold is in nobody else's clone, so it is not one of the framework's.
    [Fact]
    public void A_document_the_corpus_does_not_hold_is_not_read()
        => Assert.Empty(Framework(
            ("knowledge-as-code/draft.md", "# Draft\n\nSee the [ADRs](/adrs).\n"), tracked: false));

    // -- the corpus these are asked against --

    // Two types, so that a link can name one: `adrs` is what the documents above link to, and `glossary` is
    // where the framework's own shared glossary is filed.
    private static Schema Schema() => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
        {
            ["adrs"] = new() { Key = "adrs", TypeName = "adrs", Folder = "adrs", Page = "adrs.md" },
            ["glossary"] = new()
                { Key = "glossary", TypeName = "glossary", Folder = "glossary", Page = "glossary.md" }
        }
    };

    // The corpus a framework document sits in: both types stood up, so every link in the cases above
    // resolves and `framework-names-types` is the only thing left to report.
    private static List<Finding> Framework((string Path, string Text) doc, bool tracked = true)
    {
        string[] corpus =
        [
            "adrs.md", "adrs/0001-a.md", "adrs/_template.md",
            "glossary.md", "glossary/_template.md"
        ];
        var held = tracked ? [.. corpus, doc.Path] : corpus;

        var tree = new Tree(
            new HashSet<string>(held, StringComparer.Ordinal),
            path => path == doc.Path ? doc.Text : "# A\n",
            path => held.Contains(path, StringComparer.Ordinal) || path == doc.Path);

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, Schema(), new CorpusDescriptor()))
                .Where(f => f.Check.Value == "framework-names-types")
        ];
    }
}
