using System.Text.Json;
using kac.core;

namespace kac.tests;

public class ExporterTests
{
    [Fact]
    public void A_type_naming_no_directory_is_read_under_its_own_name()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "types": [{ "type": "glossary", "shapeVersion": 1 }]
              }
              """);

        var type = Assert.Single(manifest.Types);

        Assert.Equal("glossary", type.Dir);
        Assert.Empty(type.Sections);
    }

    [Fact]
    public void An_entry_naming_no_type_is_not_read_as_one()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "types": [{ "shapeVersion": 1 }, { "type": "glossary" }]
              }
              """);

        Assert.Equal(["glossary"], manifest.Types.Select(t => t.Type));
    }

    [Fact]
    public void A_source_naming_no_shortcode_is_dropped()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "sources": [{ "corpus": "Engineering" }, { "shortcode": "eng" }]
              }
              """);

        var source = Assert.Single(manifest.Sources);

        Assert.Equal("eng", source.Shortcode);
        Assert.Equal(Publishing.None, source.Publishing.Target);
    }

    [Fact]
    public void A_manifest_stating_no_publishing_target_publishes_nowhere()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "publishing": { "base": "https://example.invalid/corpus" }
              }
              """);

        Assert.Equal(Publishing.None, manifest.Publishing.Target);
        Assert.Equal("https://example.invalid/corpus", manifest.Publishing.Base);
        Assert.NotNull(manifest.About);
    }

    [Fact]
    public void A_document_that_is_not_JSON_reads_as_no_manifest()
        => Assert.Null(Exporter.ReadManifest("not json at all"));

    private static readonly ExportRun Run =
        new("2026-08-17T00:00:00Z", new DateOnly(2026, 8, 17), "abc123", false);

    /// <summary>A caller naming no sections gets <c>Scope</c> whole, which is what a glossary declares.</summary>
    private static TypeSchema GlossaryType(params (string Section, string Fidelity)[] sections) => new()
    {
        Key = "glossary",
        TypeName = "glossary",
        Folder = "glossary",
        Page = "glossary.md",
        IdPrefix = "gls",
        RequiredSections = ["Scope", "Terms"],
        Parts = new PartSpec(PartSpec.Headings, "", [], [])
            { Section = "Terms", Noun = "term", Level = 3, Asides = ["Also", "Avoid", "Not"] },
        Export = new ExportSpec
        {
            Version = 1,
            Fields = ["id", "title", "narrows", "status", "review-by"],
            Sections = sections.Length > 0 ? sections : [("Scope", ExportSpec.Full)],
            Parts = ExportSpec.Full,
            PartsDeclared = true,
            Line =
            [
                ("id", PartLineSource.PartId),
                ("title", PartLineSource.PartText),
                ("definition", PartLineSource.PartLead),
                ("also", $"{PartLineSource.AsidePrefix}Also"),
                ("avoid", $"{PartLineSource.AsidePrefix}Avoid"),
                ("not", $"{PartLineSource.AsidePrefix}Not"),
                ("seeAlso", PartLineSource.PartSeeAlso),
                ("type", PartLineSource.RecordType),
                ("record", PartLineSource.RecordId),
                ("part", PartLineSource.PartKey),
                ("status", $"{PartLineSource.FrontPrefix}status"),
                ("reviewBy", $"{PartLineSource.FrontPrefix}review-by"),
                ("path", PartLineSource.RecordPath),
                ("anchor", PartLineSource.PartAnchor),
                ("links", PartLineSource.PartLinks)
            ]
        }
    };

    [Fact]
    public void Roots_sort_by_id_and_each_root_s_chain_follows_it()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-zulu", narrows: null, terms: "### Alpha\n\nFrom zulu.\n"),
            Glossary("gls-alpha", narrows: null, terms: "### Alpha\n\nFrom alpha.\n"),
            Glossary("gls-alpha-narrow", narrows: "gls-alpha", terms: "### Alpha\n\nNarrowed.\n")));

        Assert.Equal(
            ["gls-alpha.alpha", "gls-alpha-narrow.alpha", "gls-zulu.alpha"],
            lines.Select(l => l.GetProperty("id").GetString()));
    }

    [Fact]
    public void Inside_a_chain_the_general_entry_comes_before_the_one_refining_it()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-narrow", narrows: "gls-general", terms: "### Title\n\nThe indexed field.\n"),
            Glossary("gls-general", narrows: null, terms: "### Title\n\nThe work itself.\n")));

        Assert.Equal(["gls-general.title", "gls-narrow.title"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    [Fact]
    public void Across_unrelated_roots_the_order_is_stable_and_claims_no_generality()
    {
        static string[] Ids(params string[] glossaries) =>
            [.. TermLines(Corpus(glossaries)).Select(l => l.Text("id"))];

        var estate = Glossary("gls-estate", narrows: null, terms: "### Record\n\nA bibliographic description.\n");
        var framework = Glossary("gls-framework", narrows: null, terms: "### Record\n\nA knowledge document.\n");

        Assert.Equal(Ids(estate, framework), Ids(framework, estate));
        Assert.Equal(2, Ids(estate, framework).Length);

        foreach (var line in TermLines(Corpus(estate, framework)))
            Assert.NotEmpty(line.Text("record"));
    }

    [Fact]
    public void Terms_sort_alphabetically_inside_a_glossary()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", narrows: null, terms: "### Zebra\n\nLast.\n\n### Apple\n\nFirst.\n")));

        Assert.Equal(["Apple", "Zebra"], lines.Select(l => l.GetProperty("title").GetString()));
    }

    [Fact]
    public void A_record_narrowing_something_outside_the_set_is_a_root()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-beta", narrows: "gls-absent", terms: "### Beta\n\nB.\n"),
            Glossary("gls-alpha", narrows: null, terms: "### Alpha\n\nA.\n")));

        Assert.Equal(["gls-alpha.alpha", "gls-beta.beta"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    [Fact]
    public void A_cycle_still_exports_every_record_it_traps()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", narrows: "gls-two", terms: "### One\n\nA.\n"),
            Glossary("gls-two", narrows: "gls-one", terms: "### Two\n\nB.\n")));

        Assert.Equal(["gls-one.one", "gls-two.two"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    [Fact]
    public void A_term_carries_its_definition_and_the_labelled_line_beneath_it_apart()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Query\n\nWhat a reader typed.\n\n**Not:** the request the service received.\n"))));

        Assert.Equal("What a reader typed.", line.GetProperty("definition").GetString());
        Assert.Equal("the request the service received.", line.GetProperty("not").GetString());
    }

    [Fact]
    public void A_term_with_no_labelled_line_carries_a_null_rather_than_an_empty_one()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Facet\n\nA field a reader narrows results by.\n"))));

        Assert.Equal(JsonValueKind.Null, line.GetProperty("not").ValueKind);
    }

    [Fact]
    public void Every_term_is_one_line_and_that_line_is_a_whole_object()
    {
        var file = Single(Plan(Corpus(Glossary("gls-one", null,
            "### Alpha\n\nA.\n\n### Beta\n\nB.\n"))), "glossary/terms.jsonl");

        var lines = file.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        foreach (var line in lines)
            Assert.Equal("gls-one", JsonDocument.Parse(line).RootElement.GetProperty("record").GetString());
    }

    [Fact]
    public void A_definition_wrapped_in_the_source_arrives_as_one_line()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Query\n\nWhat a reader typed, after parsing\nand before matching.\n"))));

        Assert.Equal("What a reader typed, after parsing and before matching.",
            line.GetProperty("definition").GetString());
    }

    [Fact]
    public void A_labelled_line_is_unwrapped_too()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Query\n\nWhat a reader typed.\n\n**Not:** the request the service\nreceived.\n"))));

        Assert.Equal("the request the service received.", line.GetProperty("not").GetString());
    }

    [Fact]
    public void A_paragraph_break_in_a_section_survives_the_unwrapping()
        => Assert.Equal("One paragraph that wraps.\n\nA second paragraph.",
            ExportedScope("One paragraph that\nwraps.\n\nA second\nparagraph."));

    [Fact]
    public void A_list_is_left_exactly_as_written()
        => Assert.Equal("What this admits:\n\n- one thing\n- another thing",
            ExportedScope("What this admits:\n\n- one thing\n- another thing"));

    [Fact]
    public void A_wrapped_line_opening_with_a_code_span_joins_the_paragraph()
        => Assert.Equal("Run the build, not `kac` on your path.",
            ExportedScope("Run the build, not\n`kac` on your path."));

    [Theory]
    [InlineData("Run this:\n```sh\nkac validate\n```")]
    [InlineData("Run this:\n````md\n```sh\nkac validate\n```\n````")]
    [InlineData("Run this:\n~~~sh\nkac validate\n~~~")]
    [InlineData("Run this:\n\n```sh\nkac validate\n```\n\nThen commit.")]
    public void A_fence_is_left_exactly_as_written(string fenced)
        => Assert.Equal(fenced, ExportedScope(fenced));

    [Theory]
    [InlineData("1. Install the linters.\n\n   ```sh\n   brew install yamllint\n   ```\n\n2. Check each version.")]
    [InlineData("- Install the linters.\n\n  ```sh\n  brew install yamllint\n  ```\n\n- Check each version.")]
    [InlineData("1. Install the linters.\n\n   ~~~sh\n   brew install yamllint\n   ~~~")]
    public void A_fence_keeps_the_indent_of_every_line_it_was_written_with(string fenced)
        => Assert.Equal(fenced, ExportedScope(fenced));

    [Fact]
    public void A_wrapped_paragraph_inside_a_list_item_joins_and_keeps_its_indent()
        => Assert.Equal("1. Install the linters.\n\n   Homebrew installs a Python.\n\n   ```sh\n   brew\n   ```",
            ExportedScope("1. Install the linters.\n\n   Homebrew installs\n   a Python.\n\n   ```sh\n   brew\n   ```"));

    [Fact]
    public void A_wrapped_line_opening_with_a_triple_backtick_code_span_joins_the_paragraph()
        => Assert.Equal("Quote a backtick as ``` ` ``` in a sentence.",
            ExportedScope("Quote a backtick as\n``` ` ``` in a sentence."));

    [Theory]
    [InlineData("Run this:\n\n```sh\nkac validate\n\nkac generate\nkac pack\n\nkac bundle\n```")]
    [InlineData("```sh\nkac validate\n\n\nkac generate\nkac pack\n\n\nkac bundle\n```")]
    [InlineData("1. Install.\n\n   ```sh\n   brew update\n\n   brew install\n   yamllint .\n\n   ls\n   ```\n\n2. Go.")]
    [InlineData("~~~yaml\nowner: someone\n\ntier: normative\nstatus: active\n\ntags: []\n~~~")]
    [InlineData("```sh\nkac validate\n\nkac generate\nkac pack\n\nkac bundle\n`````")]
    [InlineData("````md\n```sh\nkac validate\n```\n\nkac generate\nkac pack\n\nkac bundle\n````")]
    [InlineData("```md\n~~~\n\nkac generate\nkac pack\n\n~~~\n```")]
    public void A_fence_holding_a_blank_line_is_left_exactly_as_written(string fenced)
        => Assert.Equal(fenced, ExportedScope(fenced));

    [Fact]
    public void An_unclosed_fence_is_left_as_written_to_the_end_of_the_document()
    {
        const string unclosed = "```sh\nkac validate\n\nkac generate\nkac pack\n\nkac bundle";

        // Markdig runs an unclosed fence to the end of the document, so the headings below it fall inside the section.
        Assert.Equal(unclosed + "\n\n## Terms\n\n### Alpha\n\nA.", ExportedScope(unclosed));
    }

    [Fact]
    public void A_paragraph_after_a_fence_holding_a_blank_line_joins()
        => Assert.Equal("```sh\nkac validate\n\nkac generate\n```\n\nThen commit the change.",
            ExportedScope("```sh\nkac validate\n\nkac generate\n```\n\nThen commit\nthe change."));

    [Fact]
    public void A_full_reference_link_travels_as_an_inline_link_to_its_definition_s_target()
        => Assert.Equal("Read [the guide](../guide.md) first.",
            ExportedScope("Read [the guide][guide] first.\n\n[guide]: ../guide.md"));

    [Theory]
    [InlineData("Read [guide][] first.")]
    [InlineData("Read [guide] first.")]
    public void A_collapsed_or_shortcut_reference_link_travels_as_an_inline_link(string prose)
        => Assert.Equal("Read [guide](../guide.md) first.", ExportedScope($"{prose}\n\n[guide]: ../guide.md"));

    [Fact]
    public void A_reference_matches_its_definition_in_any_case_and_keeps_the_case_it_was_written_in()
        => Assert.Equal("Read [the Guide](../guide.md) and [GUIDE](../guide.md).",
            ExportedScope("Read [the Guide][Guide] and [GUIDE].\n\n[guide]: ../guide.md"));

    [Fact]
    public void A_definition_s_title_travels_with_its_target()
        => Assert.Equal("Read [guide](../guide.md \"The guide\").",
            ExportedScope("Read [guide].\n\n[guide]: ../guide.md \"The guide\""));

    [Fact]
    public void A_target_written_in_angle_brackets_keeps_them()
        => Assert.Equal("Read [guide](<../the guide.md>).",
            ExportedScope("Read [guide].\n\n[guide]: <../the guide.md>"));

    [Fact]
    public void A_reference_with_no_definition_travels_as_written()
        => Assert.Equal("Read [the guide][missing] and [missing].",
            ExportedScope("Read [the guide][missing] and [missing]."));

    [Fact]
    public void A_reference_inside_a_code_span_travels_as_written()
        => Assert.Equal("Write `[the guide][guide]` for a link.",
            ExportedScope("Write `[the guide][guide]` for a link.\n\n[guide]: ../guide.md"));

    [Fact]
    public void An_image_reference_travels_as_an_inline_image()
        => Assert.Equal("See ![the flow](flow.png) and ![flow](flow.png).",
            ExportedScope("See ![the flow][flow] and ![flow].\n\n[flow]: flow.png"));

    [Fact]
    public void An_inline_link_beside_a_reference_link_travels_as_written()
        => Assert.Equal("Read [the guide](../guide.md) and [the notes](notes.md#top).",
            ExportedScope("Read [the guide][guide] and [the notes](notes.md#top).\n\n[guide]: ../guide.md"));

    [Fact]
    public void A_reference_whose_text_carries_emphasis_and_code_keeps_that_text()
        => Assert.Equal("Run [`kac` *now*](../kac.md).",
            ExportedScope("Run [`kac` *now*][kac].\n\n[kac]: ../kac.md"));

    [Fact]
    public void A_reference_with_empty_text_travels_as_an_inline_link_with_empty_text()
        => Assert.Equal("Read [](../guide.md) and ![](flow.png).",
            ExportedScope("Read [][guide] and ![][flow].\n\n[guide]: ../guide.md\n[flow]: flow.png"));

    [Fact]
    public void An_image_reference_inside_a_reference_link_travels_with_both_inline()
        => Assert.Equal("See [![the flow](flow.png)](../guide.md).",
            ExportedScope("See [![the flow][flow]][guide].\n\n[guide]: ../guide.md\n[flow]: flow.png"));

    [Fact]
    public void A_reference_link_wrapped_across_lines_travels_on_one_line()
        => Assert.Equal("Read [the long guide](../guide.md) first.",
            ExportedScope("Read [the long\nguide][guide] first.\n\n[guide]: ../guide.md"));

    [Theory]
    [InlineData("Read [two](gls-two.md).", $"Read [two]({Blob}/glossary/gls-two.md).")]
    [InlineData("Read [two](./gls-two.md).", $"Read [two]({Blob}/glossary/gls-two.md).")]
    [InlineData("Read [two](/glossary/gls-two.md).", $"Read [two]({Blob}/glossary/gls-two.md).")]
    [InlineData("Read [two][].\n\n[two]: gls-two.md", $"Read [two]({Blob}/glossary/gls-two.md).")]
    public void A_relative_link_to_a_record_travels_as_the_address_the_corpus_publishes_it_at(
        string written, string travels)
        => Assert.Equal(travels, ScopeAt(Published, written));

    [Theory]
    [InlineData("Read [beta](gls-two.md#beta).", $"Read [beta]({Blob}/glossary/gls-two.md#beta).")]
    [InlineData("Read [beta][].\n\n[beta]: gls-two.md#beta \"Beta\"",
        $"Read [beta]({Blob}/glossary/gls-two.md#beta \"Beta\").")]
    public void A_relative_link_s_anchor_travels_on_the_address(string written, string travels)
        => Assert.Equal(travels, ScopeAt(Published, written));

    [Fact]
    public void A_relative_link_to_a_page_that_is_no_record_travels_as_its_address()
        => Assert.Equal($"See [the type]({Blob}/glossary.md).", ScopeAt(Published, "See [the type](../glossary.md)."));

    [Theory]
    [InlineData("See [notes](<the notes.md>).")]
    [InlineData("See [notes].\n\n[notes]: <the notes.md>")]
    public void An_address_holding_a_space_travels_in_angle_brackets(string written)
        => Assert.Equal($"See [notes](<{Blob}/glossary/the notes.md>).", ScopeAt(Published, written));

    [Theory]
    [InlineData("See ![the flow](flow.png).")]
    [InlineData("See [the flow](flow.png).")]
    [InlineData("See [one](../services/one.md).")]
    [InlineData("See [up](../../outside.md).")]
    public void A_relative_link_to_no_page_the_corpus_holds_travels_as_written(string written)
        => Assert.Equal(written, ScopeAt(Published, written));

    [Theory]
    [InlineData("See [the spec](https://example.com/spec.md).")]
    [InlineData("See [alpha](#alpha).")]
    [InlineData("Write to [us](mailto:someone@example.com).")]
    public void An_absolute_or_anchor_only_link_travels_as_written(string written)
        => Assert.Equal(written, ScopeAt(Published, written));

    [Theory]
    [InlineData("Read [two](gls-two.md#beta).")]
    [InlineData("Read [two].\n\n[two]: gls-two.md#beta")]
    public void A_corpus_with_no_address_keeps_a_relative_target(string written)
        => Assert.Equal("Read [two](gls-two.md#beta).", ScopeAt(null, written));

    [Fact]
    public void A_relative_link_in_a_term_line_travels_as_the_address()
    {
        var lines = Lines(Exporter.Plan(Linked("### Alpha\n\nSee [beta](gls-two.md#beta).\n\n"
                                               + "**Not:** the other, in [two].\n\n[two]: gls-two.md\n"),
            Published, null, Run));
        var alpha = lines.Single(l => l.Text("id") == "gls-one.alpha");

        Assert.Equal($"See [beta]({Blob}/glossary/gls-two.md#beta).", alpha.Text("definition"));
        Assert.Equal($"the other, in [two]({Blob}/glossary/gls-two.md).", alpha.Text("not"));
    }

    [Fact]
    public void An_inherited_link_travels_as_the_address_its_producer_published()
    {
        var produced = Exporter.Plan(Linked("### Alpha\n\nSee [beta](gls-two.md#beta).\n", scope: "Read [two]."
            + "\n\n[two]: gls-two.md"), Published, null, Run);
        var line = Single(produced, "glossary/terms.jsonl").Content.Split('\n')[0];
        var record = Single(produced, "glossary/gls-one.json").Content;

        var consumed = Holding(Consumed("eng", line), new InheritedRecord("eng", "gls-one.json", record));
        var plan = Exporter.Plan(Corpus(), Elsewhere, null, Run, [consumed]);

        Assert.Equal($"See [beta]({Blob}/glossary/gls-two.md#beta).",
            Lines(plan).Single(l => l.Text("id") == "eng:gls-one.alpha").Text("definition"));
        Assert.Equal($"Read [two]({Blob}/glossary/gls-two.md).",
            JsonDocument.Parse(Single(plan, "glossary/eng/gls-one.json").Content).RootElement
                .GetProperty("sections").GetProperty("Scope").GetString());
    }

    /// <summary>What <c>Published</c> fills its template with, up to the record's path.</summary>
    private const string Blob = "https://github.com/example/corpus/blob/0123456789abcdef0123456789abcdef01234567";

    /// <summary>A second address, so an inherited link built from it would show.</summary>
    private static readonly Publishing? Elsewhere = Publishing.For(
        new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub,
            Base = "https://github.com/example/consumer"
        },
        "fedcba9876543210fedcba9876543210fedcba98");

    /// <summary>
    /// <c>gls-one</c> with <c>terms</c> and <c>scope</c>, beside <c>gls-two</c>, the glossary's type page, an image and a
    /// page whose name has a space.
    /// </summary>
    private static LoadedCorpus Linked(string terms, string scope = "What this admits.")
    {
        var corpus = Corpus(GlossaryType(),
            ("gls-one", Glossary("gls-one", null, terms, scope: scope)),
            ("gls-two", Glossary("gls-two", null, "### Beta\n\nB.\n")));

        var paths = new HashSet<string>(corpus.Docs.Select(d => d.Rel), StringComparer.Ordinal)
            { "glossary.md", "glossary/flow.png", "glossary/the notes.md" };
        corpus.Tree = new Tree(paths, rel => corpus.Docs.FirstOrDefault(d => d.Rel == rel)?.Text ?? "");

        return corpus;
    }

    /// <summary>The <c>Scope</c> section of <c>gls-one</c> in <c>Linked</c>, exported at <c>publishing</c>.</summary>
    private static string? ScopeAt(Publishing? publishing, string scope) =>
        JsonDocument.Parse(Single(Exporter.Plan(Linked("### Alpha\n\nA.\n", scope), publishing, null, Run),
                "glossary/gls-one.json").Content)
            .RootElement.GetProperty("sections").GetProperty("Scope").GetString();

    /// <summary>The <c>Scope</c> section as the export writes it, from <c>scope</c> as a record states it.</summary>
    private static string? ExportedScope(string scope) =>
        Sections(GlossaryType(), scope).GetProperty("Scope").GetString();

    [Theory]
    [InlineData("[the guide][guide]", "[the guide](../guide.md)")]
    [InlineData("[guide][]", "[guide](../guide.md)")]
    [InlineData("[guide]", "[guide](../guide.md)")]
    public void A_reference_link_in_a_term_s_definition_travels_as_an_inline_link(string written, string travels)
        => Assert.Equal($"Read {travels} first.", ExportedTerm($"Read {written} first.").Text("definition"));

    [Theory]
    [InlineData("[the guide][guide]", "[the guide](../guide.md)")]
    [InlineData("[guide][]", "[guide](../guide.md)")]
    [InlineData("[guide]", "[guide](../guide.md)")]
    public void A_reference_link_in_a_term_s_aside_travels_as_an_inline_link(string written, string travels)
        => Assert.Equal($"the other sense, see {travels}.",
            ExportedTerm($"One sense.\n\n**Not:** the other sense, see {written}.").Text("not"));

    [Fact]
    public void A_reference_with_no_definition_in_a_term_travels_as_written()
        => Assert.Equal("Read [the guide][missing] and [missing].",
            ExportedTerm("Read [the guide][missing] and [missing].").Text("definition"));

    [Fact]
    public void A_reference_inside_a_code_span_in_a_term_travels_as_written()
        => Assert.Equal("Write `[the guide][guide]` for a link.",
            ExportedTerm("Write `[the guide][guide]` for a link.").Text("definition"));

    [Fact]
    public void A_term_holding_only_an_aside_takes_no_definition_from_the_link_definitions_below_it()
        => Assert.Equal(JsonValueKind.Null,
            ExportedTerm("**Not:** the other sense.").GetProperty("definition").ValueKind);

    /// <summary>The one term line, from <c>body</c> under its heading and above a definition of <c>guide</c>.</summary>
    private static JsonElement ExportedTerm(string body) =>
        Assert.Single(TermLines(Corpus(Glossary("gls-one", null, $"### Alpha\n\n{body}\n\n[guide]: ../guide.md\n"))));

    [Fact]
    public void A_reference_naming_the_file_and_no_term_carries_nothing_and_is_reported()
    {
        var corpus = Corpus(
            Glossary("gls-one", null, "### Record\n\nOne sense.\n\n**Not:** the other — see [gls-two].\n\n"
                                      + "[gls-two]: gls-two.md\n"),
            Glossary("gls-two", null, "### Record\n\nThe other sense.\n"));

        var referring = TermLines(corpus).Single(l => l.GetProperty("id").GetString() == "gls-one.record");
        Assert.Equal(JsonValueKind.Null, referring.GetProperty("seeAlso").ValueKind);

        // Both glossaries call the term `record`, so the counterpart is guessable here. This asserts the
        // export does not guess it, because the same guess is wrong for a pair that shares no word.
        Assert.Equal(["gls-one.record -> gls-two"], Plan(corpus).Unread);
    }

    [Fact]
    public void A_reference_carrying_an_anchor_resolves_to_the_part_it_names()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", null, "### Alpha\n\nOne sense.\n\n**Not:** see [gls-two].\n\n"
                                      + "[gls-two]: gls-two.md#beta\n"),
            Glossary("gls-two", null, "### Beta\n\nAnother.\n")));

        var referring = lines.Single(l => l.GetProperty("id").GetString() == "gls-one.alpha");
        Assert.Equal(["gls-two.beta"], referring.GetProperty("seeAlso").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void An_anchor_naming_no_part_of_the_target_carries_nothing()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", null, "### Alpha\n\nOne sense.\n\n**Not:** see [gls-two].\n\n"
                                      + "[gls-two]: gls-two.md#gamma\n"),
            Glossary("gls-two", null, "### Beta\n\nUnrelated.\n")));

        var referring = lines.Single(l => l.GetProperty("id").GetString() == "gls-one.alpha");
        Assert.Equal(JsonValueKind.Null, referring.GetProperty("seeAlso").ValueKind);
        Assert.Contains("see [gls-two]", referring.GetProperty("not").GetString());
    }

    [Fact]
    public void A_link_leaving_the_export_is_neither_carried_nor_reported()
    {
        var corpus = Corpus(Glossary("gls-one", null,
            "### Alpha\n\nOwned by [svc-one].\n\n[svc-one]: /services/one.md\n"));

        var line = Assert.Single(TermLines(corpus));
        Assert.Equal(JsonValueKind.Null, line.GetProperty("seeAlso").ValueKind);
        Assert.Empty(Plan(corpus).Unread);
    }

    [Fact]
    public void Every_term_line_carries_the_state_of_the_glossary_it_came_from()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n",
            reviewBy: "2020-01-01", status: "draft"))));

        Assert.Equal("draft", line.GetProperty("status").GetString());
        Assert.Equal("2020-01-01", line.GetProperty("reviewBy").GetString());
    }

    [Fact]
    public void A_field_left_blank_is_null_and_never_an_empty_string()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal(JsonValueKind.Null, record.GetProperty("fields").GetProperty("narrows").ValueKind);
    }

    [Fact]
    public void A_field_the_type_declares_as_a_list_travels_as_an_array()
    {
        var fields = Fields(ListType(), "gls-one", "tags: [ search, discovery ]");

        Assert.Equal(JsonValueKind.Array, fields.GetProperty("tags").ValueKind);
        Assert.Equal(["search", "discovery"], fields.GetProperty("tags").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void A_list_written_as_one_scalar_travels_as_an_array_of_one()
    {
        var fields = Fields(ListType(), "gls-one", "tags: search");

        Assert.Equal(["search"], fields.GetProperty("tags").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void A_list_left_empty_is_null_and_never_an_empty_array()
    {
        var fields = Fields(ListType(), "gls-one", "tags:");

        Assert.Equal(JsonValueKind.Null, fields.GetProperty("tags").ValueKind);
    }

    [Fact]
    public void An_object_entry_carries_the_keys_its_type_declares()
    {
        var fields = Fields(ObjectListType(), "gls-one",
            "aligns-with:\n  - framework: ISO27001\n    clauses: [ A.8.24, A.8.8 ]");

        var entry = Assert.Single(fields.GetProperty("aligns-with").EnumerateArray().ToList());
        Assert.Equal("ISO27001", entry.GetProperty("framework").GetString());
        Assert.Equal(["A.8.24", "A.8.8"], entry.GetProperty("clauses").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void An_object_field_carries_the_keys_its_type_declares()
    {
        var fields = Fields(ObjectType(), "gls-one", "generated:\n  at: 2026-09-08T10:00:00Z\n  by: kac/0.16.0");

        var generated = fields.GetProperty("generated");
        Assert.Equal("2026-09-08T10:00:00Z", generated.GetProperty("at").GetString());
        Assert.Equal("kac/0.16.0", generated.GetProperty("by").GetString());
    }

    [Fact]
    public void An_object_field_left_out_is_null_and_never_an_object_of_nulls()
    {
        var fields = Fields(ObjectType(), "gls-one", "generated:");

        Assert.Equal(JsonValueKind.Null, fields.GetProperty("generated").ValueKind);
    }

    [Fact]
    public void A_type_declaring_no_sections_writes_an_empty_sections_object()
    {
        var text = "---\nid: gls-one\ntier: descriptive\nstatus: draft\nowner: someone\n"
                   + "review-by: \"2030-01-01\"\ntags: [ search ]\n---\n\n"
                   + "# gls-one\n\n## Scope\n\nWhat this admits.\n\n## Terms\n\n### Alpha\n\nA.\n";

        var record = JsonDocument.Parse(
                Single(Plan(Corpus(SectionlessType(), ("gls-one", text))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal(JsonValueKind.Object, record.GetProperty("sections").ValueKind);
        Assert.Empty(record.GetProperty("sections").EnumerateObject());
    }

    [Fact]
    public void A_record_carries_the_fields_and_sections_its_type_declares_and_no_others()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal(
            ["id", "title", "narrows", "status", "review-by"],
            record.GetProperty("fields").EnumerateObject().Select(p => p.Name));
        Assert.Equal(["Scope"], record.GetProperty("sections").EnumerateObject().Select(p => p.Name));
        Assert.Equal("What this admits.", record.GetProperty("sections").GetProperty("Scope").GetString());
    }

    [Fact]
    public void A_corpus_that_publishes_nowhere_writes_records_without_links()
    {
        var plan = Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")));

        var record = JsonDocument.Parse(Single(plan, "glossary/gls-one.json").Content).RootElement;
        Assert.Equal(JsonValueKind.Null, record.GetProperty("links").ValueKind);

        var publishing = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("publishing");
        Assert.Equal("none", publishing.GetProperty("target").GetString());
        Assert.Equal(JsonValueKind.Null, publishing.GetProperty("humanTemplate").ValueKind);
        Assert.Equal(JsonValueKind.Null, publishing.GetProperty("base").ValueKind);
        Assert.Equal(JsonValueKind.Null, publishing.GetProperty("ref").ValueKind);
    }

    [Fact]
    public void The_manifest_carries_where_a_problem_with_the_framework_is_reported()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.FrameworkTarget = Publishing.GitHub;
        corpus.Descriptor.FrameworkBase = "https://github.com/example/framework";

        var framework = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement
            .GetProperty("framework");

        Assert.Equal("github", framework.GetProperty("target").GetString());
        Assert.Equal("https://github.com/example/framework", framework.GetProperty("base").GetString());
    }

    [Fact]
    public void A_corpus_naming_no_framework_tracker_states_the_absence()
    {
        var framework = JsonDocument
            .Parse(Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))),
                Exporter.ManifestFile).Content).RootElement
            .GetProperty("framework");

        Assert.Equal("none", framework.GetProperty("target").GetString());
        Assert.Equal(JsonValueKind.Null, framework.GetProperty("base").ValueKind);
    }

    [Fact]
    public void A_manifest_short_of_a_framework_block_reads_as_naming_no_tracker()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}}
              }
              """);

        Assert.Equal(Publishing.None, manifest.Framework.Target);
        Assert.Null(manifest.Framework.Base);
    }

    [Fact]
    public void The_manifest_carries_where_work_about_this_corpus_is_filed()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.TrackerTarget = Publishing.AzureDevOps;
        corpus.Descriptor.TrackerBase = "https://dev.azure.com/acme/Standards";

        var tracker = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement
            .GetProperty("tracker");

        Assert.Equal("azure-devops", tracker.GetProperty("target").GetString());
        Assert.Equal("https://dev.azure.com/acme/Standards", tracker.GetProperty("base").GetString());
        Assert.Equal("azure-devops:dev.azure.com/acme/standards", tracker.GetProperty("id").GetString());
    }

    [Fact]
    public void A_corpus_stating_no_tracker_files_where_it_publishes()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.PublishingTarget = Publishing.GitHub;
        corpus.Descriptor.Base = "https://github.com/example/corpus";

        var tracker = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement
            .GetProperty("tracker");

        Assert.Equal("https://github.com/example/corpus", tracker.GetProperty("base").GetString());
        Assert.Equal("github:github.com/example/corpus", tracker.GetProperty("id").GetString());
    }

    [Fact]
    public void An_inherited_corpus_carries_its_own_tracker_into_sources()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));

        var tracker = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("sources").EnumerateArray().Single().GetProperty("tracker");

        Assert.Equal("https://example.com/eng", tracker.GetProperty("base").GetString());
        Assert.Equal("github:example.com/eng", tracker.GetProperty("id").GetString());
    }

    [Fact]
    public void A_manifest_short_of_a_tracker_block_files_where_it_publishes()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "publishing": { "target": "github", "base": "https://github.com/example/corpus" },
                "sources": [
                  { "shortcode": "eng",
                    "publishing": { "target": "github", "base": "https://github.com/example/upstream" } }
                ]
              }
              """);

        Assert.Equal("github:github.com/example/corpus", manifest.Tracker.Id);
        Assert.Equal("github:github.com/example/upstream", manifest.Sources.Single().Tracker.Id);
    }

    [Fact]
    public void A_manifest_short_of_a_framework_block_derives_no_tracker_from_where_it_publishes()
    {
        var manifest = Required.Manifest(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "publishing": { "target": "github", "base": "https://github.com/example/corpus" }
              }
              """);

        Assert.Equal(Publishing.None, manifest.Framework.Target);
        Assert.Null(manifest.Framework.Id);
    }

    [Fact]
    public void A_section_carried_at_summary_keeps_its_opening_block_and_drops_the_rest()
    {
        var sections = Sections(GlossaryType(("Scope", ExportSpec.Summary)),
            "What this admits.\n\nAnd the qualification nobody reads first.");

        Assert.Equal("What this admits.", sections.GetProperty("Scope").GetString());
    }

    [Fact]
    public void A_summary_is_the_whole_opening_paragraph_and_never_the_first_wrapped_line()
    {
        var sections = Sections(GlossaryType(("Scope", ExportSpec.Summary)),
            "What this admits, said over\ntwo lines of source.\n\nA second paragraph.");

        Assert.Equal("What this admits, said over two lines of source.",
            sections.GetProperty("Scope").GetString());
    }

    [Fact]
    public void A_section_carried_at_reference_keeps_its_key_and_none_of_its_words()
    {
        var sections = Sections(GlossaryType(("Scope", ExportSpec.Reference)), "What this admits.");

        Assert.Equal(JsonValueKind.Null, sections.GetProperty("Scope").ValueKind);
    }

    [Fact]
    public void A_section_the_record_never_wrote_is_absent_where_a_referenced_one_is_null()
    {
        var type = GlossaryType(("Scope", ExportSpec.Reference), ("Provenance", ExportSpec.Reference));
        var sections = Sections(type, "What this admits.");

        Assert.Equal(["Scope"], sections.EnumerateObject().Select(p => p.Name));
        Assert.Equal(JsonValueKind.Null, sections.GetProperty("Scope").ValueKind);
    }

    [Fact]
    public void The_manifest_states_the_fidelity_each_section_travelled_at()
    {
        var type = GlossaryType(("Scope", ExportSpec.Summary));
        var plan = Plan(Corpus(type, ("gls-one", Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        var declared = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("types")[0].GetProperty("sections");

        Assert.Equal("summary", declared.GetProperty("Scope").GetString());
    }

    [Fact]
    public void A_term_line_carries_the_link_to_its_term()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Published, null, Run);

        Assert.Equal(
            "https://github.com/example/corpus/blob/0123456789abcdef0123456789abcdef01234567/glossary/gls-one.md#alpha",
            Assert.Single(Lines(plan)).GetProperty("links").Text("human"));
    }

    [Fact]
    public void A_wiki_line_links_the_page_rather_than_the_file()
    {
        var wiki = Publishing.For(
            new CorpusDescriptor
            {
                PublishingTarget = Publishing.AzureDevOpsWiki,
                Base = "https://dev.azure.com/acme/Standards/_wiki/wikis/KaC"
            },
            "0123456789abcdef0123456789abcdef01234567");

        var plan = Exporter.Plan(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), wiki, null, Run);

        Assert.Equal(
            "https://dev.azure.com/acme/Standards/_wiki/wikis/KaC?pagePath=%2Fglossary%2Fgls-one&anchor=alpha",
            Assert.Single(Lines(plan)).GetProperty("links").Text("human"));
    }

    [Fact]
    public void A_corpus_that_publishes_nowhere_writes_null_links_on_its_part_lines()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        Assert.Equal(JsonValueKind.Null, line.GetProperty("links").ValueKind);
    }

    [Fact]
    public void Substituting_a_line_into_the_template_gives_the_link_the_record_carries()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Published, null, Run);

        var publishing = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("publishing");
        var links = JsonDocument.Parse(Single(plan, "glossary/gls-one.json").Content).RootElement
            .GetProperty("links");
        var line = Assert.Single(Lines(plan));

        var path = line.Text("path");
        var anchor = line.Text("anchor");

        Assert.Equal(
            $"{links.GetProperty("human").GetString()}#{anchor}",
            publishing.Text("humanTemplate")
                .Replace(Publishing.PathToken, path)
                .Replace(Publishing.AnchorToken, anchor));
    }

    [Fact]
    public void A_corpus_with_no_exporting_type_still_writes_a_manifest()
    {
        var plan = Exporter.Plan(Corpus(), null, null, Run);

        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
        Assert.Empty(plan.Types);

        var manifest = JsonDocument.Parse(plan.Files[0].Content).RootElement;
        Assert.Empty(manifest.GetProperty("types").EnumerateArray());
    }

    [Fact]
    public void The_manifest_counts_records_and_parts_separately()
    {
        var manifest = JsonDocument.Parse(
                Single(Plan(Corpus(
                    Glossary("gls-one", null, "### Alpha\n\nA.\n\n### Beta\n\nB.\n"),
                    Glossary("gls-two", null, "### Gamma\n\nC.\n"))), Exporter.ManifestFile).Content)
            .RootElement;

        var type = Assert.Single(manifest.GetProperty("types").EnumerateArray());
        Assert.Equal(2, type.GetProperty("records").GetInt32());
        Assert.Equal(3, type.GetProperty("parts").GetInt32());
        Assert.Equal("glossary/terms.jsonl", type.GetProperty("partsFile").GetString());
    }

    [Fact]
    public void The_manifest_says_what_this_export_is_and_where_it_came_from()
    {
        var manifest = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), Exporter.ManifestFile).Content)
            .RootElement;

        Assert.Equal(Exporter.FormatVersion, manifest.GetProperty("formatVersion").GetInt32());
        Assert.Equal("test-corpus", manifest.GetProperty("corpus").GetString());
        Assert.Equal("2.1.0", manifest.GetProperty("contentVersion").GetString());
        Assert.Equal("abc123", manifest.GetProperty("commit").GetString());
        Assert.False(manifest.GetProperty("dirty").GetBoolean());
        Assert.Equal(Run.GeneratedAt, manifest.GetProperty("generatedAt").GetString());
    }

    [Fact]
    public void The_manifest_carries_the_shortcode_a_citation_scopes_by()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.Shortcode = "tst";

        var manifest = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement;

        Assert.Equal("test-corpus", manifest.GetProperty("corpus").GetString());
        Assert.Equal("tst", manifest.GetProperty("shortcode").GetString());
    }

    [Fact]
    public void A_corpus_declaring_no_shortcode_states_the_absence()
        => Assert.Equal(JsonValueKind.Null,
            JsonDocument.Parse(
                    Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), Exporter.ManifestFile)
                        .Content)
                .RootElement.GetProperty("shortcode").ValueKind);

    [Fact]
    public void Two_runs_over_one_corpus_differ_only_in_the_generated_timestamp()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        var later = Exporter.Plan(corpus, null, null, Run with { GeneratedAt = "2027-01-01T00:00:00Z" });
        var first = Exporter.Plan(corpus, null, null, Run);

        foreach (var (a, b) in first.Files.Zip(later.Files))
        {
            Assert.Equal(a.Path, b.Path);
            if (a.Path != Exporter.ManifestFile) Assert.Equal(a.Content, b.Content);
        }

        Assert.NotEqual(
            Single(first, Exporter.ManifestFile).Content,
            Single(later, Exporter.ManifestFile).Content);
    }

    [Fact]
    public void An_unsettled_record_travels_by_default_with_its_state_attached()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n", reviewBy: "2020-01-01"))),
                    "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal("draft", record.GetProperty("fields").GetProperty("status").GetString());
        Assert.Equal("2020-01-01", record.GetProperty("fields").GetProperty("review-by").GetString());
    }

    [Theory]
    [InlineData(CorpusDescriptor.ExcludeDraft)]
    [InlineData(CorpusDescriptor.ExcludeOverdue)]
    public void A_corpus_may_withhold_what_has_not_settled_and_the_plan_names_what_it_withheld(string exclude)
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n", reviewBy: "2020-01-01"));
        corpus.Descriptor.ExportExclude.Add(exclude);

        var plan = Exporter.Plan(corpus, null, null, Run);

        Assert.Equal(["gls-one"], plan.Withheld);
        Assert.Empty(plan.Types);
    }

    [Fact]
    public void An_unreadable_review_date_is_not_overdue()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n", reviewBy: "whenever", status: "active"));
        corpus.Descriptor.ExportExclude.Add(CorpusDescriptor.ExcludeOverdue);

        Assert.Empty(Exporter.Plan(corpus, null, null, Run).Withheld);
    }

    [Fact]
    public void Naming_a_type_the_corpus_has_not_adopted_refuses()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), null, "adrs", Run);

        Assert.Empty(plan.Files);
        Assert.Contains("'adrs'", Assert.Single(plan.Refused));
    }

    [Fact]
    public void An_exclusion_the_export_cannot_act_on_refuses()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.ExportExclude.Add("nonsense");

        var plan = Exporter.Plan(corpus, null, null, Run);

        Assert.Empty(plan.Files);
        Assert.Contains("nonsense", Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_corpus_nothing_is_restored_for_refuses()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), null, null, Run,
            missing: ["eng"]);

        Assert.Empty(plan.Files);
        Assert.Contains("eng", Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_corpus_at_another_export_format_refuses()
    {
        var plan = Merged(Corpus(), Consumed("eng", TheirLine) with { FormatVersion = Exporter.FormatVersion + 1 });

        Assert.Empty(plan.Files);
        Assert.Contains($"format {Exporter.FormatVersion + 1}", Assert.Single(plan.Refused));
    }

    [Fact]
    public void Every_refusal_is_listed_at_once()
    {
        var plan = Exporter.Plan(Corpus(), null, "adrs", Run, missing: ["eng"]);

        Assert.Equal(2, plan.Refused.Count);
    }

    private static InheritedCorpus Consumed(string shortcode, params string[] lines) =>
        new(shortcode, Exporter.FormatVersion, $"{shortcode}-corpus", "1.0.0",
            new ExportPublishing("github", $"https://example.com/{shortcode}/{{path}}#{{anchor}}",
                $"https://example.com/{shortcode}", null, "beefbeef"),
            $"https://code.example.com/{shortcode}",
            Tracker.For(Publishing.GitHub, $"https://example.com/{shortcode}", null),
            [],
            [
                new InheritedType("glossary", 1, "glossary", "glossary/terms.jsonl",
                    "record", "part", "id", "seeAlso",
                    new Dictionary<string, string>(StringComparer.Ordinal) { ["Scope"] = ExportSpec.Full },
                    lines,
                    [new InheritedRecord(shortcode, "gls-theirs.json", "{\"type\": \"glossary\"}\n")])
            ]);

    private static ExportPlan Merged(LoadedCorpus corpus, params InheritedCorpus[] consumed) =>
        Exporter.Plan(corpus, null, null, Run, consumed);

    private const string TheirLine =
        """{"id":"gls-theirs.alpha","record":"gls-theirs","seeAlso":["gls-other.beta"],"part":"alpha"}""";

    [Fact]
    public void An_inherited_line_carries_its_producer_s_name_on_every_id_it_holds()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));
        var line = Lines(plan).Single(l => l.Text("id").StartsWith("eng:"));

        Assert.Equal("eng:gls-theirs.alpha", line.GetProperty("id").GetString());
        Assert.Equal("eng:gls-theirs", line.GetProperty("record").GetString());
        Assert.Equal(["eng:gls-other.beta"],
            line.GetProperty("seeAlso").EnumerateArray().Select(v => v.GetString()));
        Assert.Equal("eng", line.GetProperty(Exporter.ShortcodeKey).GetString());
    }

    [Fact]
    public void An_inherited_line_keeps_the_link_its_producer_built()
    {
        const string linked =
            """{"id":"gls-theirs.alpha","record":"gls-theirs","seeAlso":null,"part":"alpha","links":{"human":"https://example.com/eng/glossary/theirs.md#alpha"}}""";

        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Published, null, Run,
            [Consumed("eng", linked)]);
        var line = Lines(plan).Single(l => l.Text("id").StartsWith("eng:"));

        Assert.Equal("https://example.com/eng/glossary/theirs.md#alpha", line.GetProperty("links").Text("human"));
    }

    [Fact]
    public void A_line_this_corpus_wrote_carries_no_name_and_no_shortcode()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));
        var line = Lines(plan).Single(l => l.GetProperty("record").GetString() == "gls-one");

        Assert.Equal("gls-one.alpha", line.GetProperty("id").GetString());
        Assert.False(line.TryGetProperty(Exporter.ShortcodeKey, out _));
    }

    /// <summary>
    /// A line as it reaches a grandchild: <c>eng</c> merged <c>gp</c> and stamped it, so the line arrives naming the
    /// corpus that wrote it rather than the one it came through.
    /// </summary>
    private const string TheirInheritedLine =
        """{"id":"gp:gls-old.alpha","record":"gp:gls-old","seeAlso":null,"part":"alpha","shortcode":"gp"}""";

    [Fact]
    public void A_line_already_naming_its_writer_keeps_that_name()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Carrying(Consumed("eng", TheirInheritedLine), Old));

        var line = Lines(plan).Single(l => l.Text("id").Contains(':'));

        Assert.Equal("gp:gls-old.alpha", line.GetProperty("id").GetString());
        Assert.Equal("gp:gls-old", line.GetProperty("record").GetString());
        Assert.Equal("gp", line.GetProperty(Exporter.ShortcodeKey).GetString());
    }

    [Fact]
    public void A_corpus_its_producer_consumed_reaches_sources_too()
    {
        var eng = Carrying(Consumed("eng", TheirInheritedLine), Old) with
        {
            Sources =
            [
                new ExportSource("gp", "gp-corpus", "0.2.0",
                    new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                        "https://example.com/gp", null, "cafecafe"),
                    "https://code.example.com/gp",
                    Tracker.For(Publishing.GitHub, "https://example.com/gp", null))
            ]
        };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), eng);

        var sources = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("sources").EnumerateArray().ToList();

        Assert.Equal(["eng", "gp"], sources.Select(s => s.GetProperty("shortcode").GetString()));
        Assert.Equal("cafecafe",
            sources[1].GetProperty("publishing").GetProperty("ref").GetString());
    }

    [Fact]
    public void A_producers_repository_base_reaches_sources()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirLine));

        var sources = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("sources").EnumerateArray().ToList();

        Assert.Equal("https://code.example.com/eng",
            Assert.Single(sources).GetProperty("reposBase").GetString());
    }

    [Fact]
    public void One_corpus_arriving_twice_at_two_versions_refuses()
    {
        static ExportSource At(string version) =>
            new("gp", "gp-corpus", version,
                new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                    "https://example.com/gp", null, version),
                "https://code.example.com/gp",
                Tracker.For(Publishing.GitHub, "https://example.com/gp", null));

        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirLine) with { Sources = [At("0.2.0")] },
            Consumed("ops", TheirLine) with { Sources = [At("0.3.0")] });

        Assert.Empty(plan.Files);
        Assert.Contains("0.2.0", Assert.Single(plan.Refused));
        Assert.Contains("0.3.0", plan.Refused[0]);
    }

    [Fact]
    public void Two_accounts_of_one_corpus_differing_only_in_where_its_code_sits_still_merge()
    {
        static ExportSource Hosted(string? reposBase) =>
            new("gp", "gp-corpus", "0.2.0",
                new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                    "https://example.com/gp", null, "cafecafe"),
                reposBase,
                Tracker.For(Publishing.GitHub, "https://example.com/gp", null));

        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirLine) with { Sources = [Hosted(null)] },
            Consumed("ops", TheirLine) with { Sources = [Hosted("https://code.example.com/gp")] });

        Assert.Empty(plan.Refused);
    }

    [Fact]
    public void Two_accounts_of_one_corpus_differing_only_in_where_it_files_still_merge()
    {
        static ExportSource Filing(string tracker) =>
            new("gp", "gp-corpus", "0.2.0",
                new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                    "https://example.com/gp", null, "cafecafe"),
                "https://code.example.com/gp",
                Tracker.For(Publishing.GitHub, tracker, null));

        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirLine) with { Sources = [Filing("https://example.com/gp")] },
            Consumed("ops", TheirLine) with { Sources = [Filing("https://example.com/tickets")] });

        Assert.Empty(plan.Refused);
    }

    [Fact]
    public void An_inherited_record_is_filed_under_its_producer_s_shortcode()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));

        Assert.Contains("glossary/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
        Assert.Contains("glossary/gls-one.json", plan.Files.Select(f => f.Path));
    }

    [Fact]
    public void A_record_its_producer_inherited_keeps_the_folder_of_the_corpus_that_wrote_it()
    {
        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Carrying(Consumed("eng", TheirInheritedLine), Old));

        Assert.Contains("glossary/gp/gls-old.json", plan.Files.Select(f => f.Path));
        Assert.Contains("glossary/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
        Assert.Equal(3, Assert.Single(plan.Types).Records);
    }

    [Fact]
    public void A_record_arriving_by_two_routes_is_written_and_counted_once()
    {
        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Carrying(Consumed("eng", TheirInheritedLine), Old),
            Holding(Consumed("gp"), Old));

        Assert.Single(plan.Files, f => f.Path == "glossary/gp/gls-old.json");
        Assert.Equal(3, Assert.Single(plan.Types).Records);
    }

    [Fact]
    public void A_part_line_arriving_by_two_routes_is_written_and_counted_once()
    {
        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Carrying(Consumed("eng", TheirInheritedLine), Old),
            Holding(Consumed("gp", OldLine), Old));

        Assert.Single(Lines(plan), l => l.Text("id") == "gp:gls-old.alpha");
        Assert.Equal(2, Assert.Single(plan.Types).Parts);
    }

    [Fact]
    public void A_part_line_whose_record_did_not_travel_refuses_and_writes_nothing()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirInheritedLine));

        Assert.Empty(plan.Files);
        Assert.Equal(
            "'glossary' has a part line for gp:gls-old, and the export writes no glossary/gp/gls-old.json. "
            + "Re-export and re-pack the corpus that sent the line, then run kac restore.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void Part_lines_whose_records_all_travelled_do_not_refuse()
    {
        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Carrying(Consumed("eng", TheirLine, TheirInheritedLine), Old));

        Assert.Empty(plan.Refused);
        Assert.Equal(3, Assert.Single(plan.Types).Parts);
    }

    [Fact]
    public void A_line_whose_producer_files_the_type_under_another_folder_does_not_refuse()
    {
        var theirs = Consumed("eng", TheirLine);
        var elsewhere = theirs with { Types = [theirs.Types[0] with { Dir = "lexicon" }] };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), elsewhere);

        Assert.Empty(plan.Refused);
        Assert.Contains("lexicon/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
    }

    [Fact]
    public void A_consumed_type_whose_lines_arrive_with_no_record_files_refuses_and_writes_nothing()
    {
        var plan = Merged(Corpus(), Holding(Consumed("eng", TheirLine)));

        Assert.Empty(plan.Files);
        Assert.Equal(
            "'glossary' arrives from eng with 1 part line and no record files. "
            + "Re-export and re-pack eng, then run kac restore.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_type_its_manifest_counts_records_for_and_no_record_file_arrived_refuses()
    {
        var theirs = Holding(Consumed("eng"));
        theirs = theirs with { Types = [theirs.Types[0] with { StatedRecords = 3 }] };

        var plan = Merged(Corpus(), theirs);

        Assert.Empty(plan.Files);
        Assert.Equal(
            "'glossary' arrives from eng with no record files, and its manifest counts 3 records. "
            + "Re-export and re-pack eng, then run kac restore.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_type_that_lost_its_record_files_refuses_beside_this_corpus_s_own_records()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Holding(Consumed("eng", TheirLine)));

        Assert.Empty(plan.Files);
        Assert.StartsWith("'glossary' arrives from eng with 1 part line", Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_producer_s_copy_with_no_record_files_refuses_where_the_grandparent_s_files_arrive_directly()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Holding(Consumed("eng", TheirInheritedLine)),
            Holding(Consumed("gp", OldLine), Old));

        Assert.Empty(plan.Files);
        Assert.StartsWith("'glossary' arrives from eng with 1 part line and no record files.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_type_with_fewer_record_files_than_its_manifest_counts_refuses()
    {
        var theirs = Consumed("eng", TheirLine);
        theirs = theirs with { Types = [theirs.Types[0] with { StatedRecords = 3 }] };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), theirs);

        Assert.Empty(plan.Files);
        Assert.Equal(
            "'glossary' arrives from eng with 1 of the 3 record files its manifest counts. "
            + "Re-export and re-pack eng, then run kac restore.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_consumed_type_with_no_records_and_no_lines_is_skipped_without_a_word()
    {
        var plan = Merged(Corpus(), Holding(Consumed("eng")));

        Assert.Empty(plan.Refused);
        Assert.Empty(plan.Types);
        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
    }

    [Fact]
    public void A_consumed_type_whose_record_files_all_arrived_is_exported_whole()
    {
        var theirs = Consumed("eng", TheirLine);
        theirs = theirs with { Types = [theirs.Types[0] with { StatedRecords = 1, StatedParts = 1 }] };

        var plan = Merged(Corpus(), theirs);

        Assert.Empty(plan.Refused);
        Assert.Equal(1, Assert.Single(plan.Types).Records);
        Assert.Contains("glossary/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
    }

    /// <summary>Gp's line as gp published it, before eng stamped it into <c>TheirInheritedLine</c>.</summary>
    private const string OldLine = """{"id":"gls-old.alpha","record":"gls-old","seeAlso":null,"part":"alpha"}""";

    /// <summary>Gp's record as eng published it: filed under gp, because gp wrote it.</summary>
    private static readonly InheritedRecord Old = new("gp", "gls-old.json", "{}\n");

    /// <summary>A consumed corpus with these records added to the ones <c>Consumed</c> gives it.</summary>
    private static InheritedCorpus Carrying(InheritedCorpus corpus, params InheritedRecord[] records) =>
        Records(corpus, [.. corpus.Types[0].Records, .. records]);

    /// <summary>A consumed corpus with these records in place of the ones <c>Consumed</c> gives it.</summary>
    private static InheritedCorpus Holding(InheritedCorpus corpus, params InheritedRecord[] records) =>
        Records(corpus, records);

    private static InheritedCorpus Records(InheritedCorpus corpus, IReadOnlyList<InheritedRecord> records) =>
        corpus with { Types = [corpus.Types[0] with { Records = records }] };

    [Fact]
    public void A_type_entry_counts_what_this_corpus_wrote_and_what_it_inherited_together()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n\n### Beta\n\nB.\n")),
            Consumed("eng", TheirLine));

        var type = Assert.Single(plan.Types);
        Assert.Equal(2, type.Records);
        Assert.Equal(3, type.Parts);
    }

    [Fact]
    public void A_source_carries_the_publishing_its_producer_wrote()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));

        var source = Assert.Single(
            JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
                .GetProperty("sources").EnumerateArray());

        Assert.Equal("eng", source.GetProperty("shortcode").GetString());
        Assert.Equal("eng-corpus", source.GetProperty("corpus").GetString());
        Assert.Equal("1.0.0", source.GetProperty("contentVersion").GetString());
        Assert.Equal("beefbeef", source.GetProperty("publishing").GetProperty("ref").GetString());
    }

    [Fact]
    public void A_type_only_a_consumed_corpus_holds_still_travels()
    {
        var plan = Exporter.Plan(Corpus(), null, null, Run, [Consumed("eng", TheirLine)]);

        var type = Assert.Single(plan.Types);
        Assert.Equal("glossary", type.Type);
        Assert.Equal(1, type.Records);
        Assert.Equal("glossary/terms.jsonl", type.PartsFile);
        Assert.Contains("glossary/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
    }

    [Fact]
    public void A_type_this_corpus_did_not_adopt_states_the_in_force_status_its_producer_published()
    {
        var theirs = Consumed("eng", TheirLine);
        var accepted = theirs with { Types = [theirs.Types[0] with { InForce = "accepted" }] };

        var plan = Exporter.Plan(Corpus(PolicyType()), null, null, Run, [accepted]);

        Assert.Equal("accepted", Assert.Single(plan.Types).InForce);
    }

    [Fact]
    public void A_type_exported_at_two_shapes_refuses_and_writes_nothing()
    {
        var theirs = Consumed("eng", TheirLine);
        var mismatched = theirs with { Types = [theirs.Types[0] with { ShapeVersion = 2 }] };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), mismatched);

        Assert.Empty(plan.Files);
        Assert.Contains("shape 1", Assert.Single(plan.Refused));
        Assert.Contains("shape 2", plan.Refused[0]);
    }

    [Fact]
    public void A_type_whose_sections_travelled_differently_refuses()
    {
        var theirs = Consumed("eng", TheirLine);
        var mismatched = theirs with
        {
            Types =
            [
                theirs.Types[0] with
                {
                    Sections = new Dictionary<string, string>(StringComparer.Ordinal)
                        { ["Scope"] = ExportSpec.Summary }
                }
            ]
        };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), mismatched);

        Assert.Empty(plan.Files);
        Assert.Contains("fidelity", Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_type_adopted_here_and_in_force_at_another_status_in_its_producer_refuses()
    {
        var plan = Merged(Corpus(InForceAt("active")), InForce(Consumed("eng", TheirLine), "ratified"));

        Assert.Empty(plan.Files);
        Assert.Equal(
            "'glossary' is in force at 'active' in this corpus and at 'ratified' in eng. A merged file cannot "
            + "state both. Bring both to one in-force status, or drop the type from this corpus.",
            Assert.Single(plan.Refused));
    }

    [Fact]
    public void Two_producers_stating_different_in_force_statuses_refuse()
    {
        var plan = Merged(Corpus(PolicyType()),
            InForce(Consumed("eng", TheirLine), "accepted"), InForce(Consumed("ops", TheirLine), "active"));

        Assert.Empty(plan.Files);
        Assert.Contains("at 'accepted' in eng and at 'active' in ops", Assert.Single(plan.Refused));
    }

    [Fact]
    public void A_producer_stating_no_in_force_status_does_not_refuse()
        => Assert.Empty(Merged(Corpus(InForceAt("active")), Consumed("eng", TheirLine)).Refused);

    /// <summary>The glossary type with a <c>status</c> range in force at the value given.</summary>
    private static TypeSchema InForceAt(string inForce) => FieldType("status",
        new FieldSpec { Name = "status", Type = "enum", Values = ["draft", inForce], InForce = inForce });

    private static InheritedCorpus InForce(InheritedCorpus corpus, string inForce) =>
        corpus with { Types = [corpus.Types[0] with { InForce = inForce }] };

    private static ExportPlan Plan(LoadedCorpus corpus) => Exporter.Plan(corpus, null, null, Run);

    private static readonly Publishing? Published = Publishing.For(
        new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub,
            Base = "https://github.com/example/corpus"
        },
        "0123456789abcdef0123456789abcdef01234567");

    private static ExportFile Single(ExportPlan plan, string path) =>
        Assert.Single(plan.Files, f => f.Path == path);

    private static JsonElement Sections(TypeSchema type, string scope) =>
        JsonDocument.Parse(
                Single(Plan(Corpus(type, ("gls-one", Glossary("gls-one", null, "### Alpha\n\nA.\n", scope: scope)))),
                    "glossary/gls-one.json").Content)
            .RootElement.GetProperty("sections");

    [Fact]
    public void A_clause_line_carries_the_modal_and_the_column_the_type_declares()
    {
        var line = ClauseLines()[0];

        Assert.Equal("pol-DATA.TIMEBOX", line.GetProperty("id").GetString());
        Assert.Equal("MUST be time-boxed.", line.GetProperty("clause").GetString());
        Assert.Equal("MUST", line.GetProperty("level").GetString());
        Assert.Equal("ISO27001 A.5.1", line.GetProperty("alignment").GetString());
        Assert.False(line.TryGetProperty("definition", out _));
    }

    [Fact]
    public void A_clause_that_does_not_bind_still_carries_its_level()
        => Assert.Equal("SHOULD", ClauseLines()[1].GetProperty("level").GetString());

    [Fact]
    public void A_column_the_row_leaves_empty_carries_a_null()
        => Assert.Equal(JsonValueKind.Null, ClauseLines()[1].GetProperty("alignment").ValueKind);

    [Fact]
    public void A_reference_link_in_a_clause_travels_as_its_text_alone()
    {
        var policy = Policy().Replace("**MUST** be time-boxed.", "**MUST** be time-boxed, as [pol-DEVI] asks.");
        var line = Single(Plan(Corpus(PolicyType(), ("pol-DATA", policy))), "policies/clauses.jsonl").Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];

        Assert.Equal("MUST be time-boxed, as pol-DEVI asks.", JsonDocument.Parse(line).RootElement.Text("clause"));
    }

    [Fact]
    public void Clauses_travel_in_the_order_the_table_writes_them()
    {
        var lines = Single(Plan(Corpus(PolicyType(), ("pol-ORDR", Unsorted()))), "policies/clauses.jsonl")
            .Content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement);

        Assert.Equal(["ZONE", "AUDIT"], lines.Select(l => l.GetProperty("part").GetString()));
    }

    [Fact]
    public void A_carried_section_leaves_the_link_reference_definitions_behind()
    {
        var record = Single(Plan(Corpus(PolicyType(), ("pol-DATA", Policy()))), "policies/pol-DATA.json").Content;
        var sections = JsonDocument.Parse(record).RootElement.GetProperty("sections");

        Assert.Equal("A finding may be accepted where [pol-DEVI](devi-deviations.md) records who accepted it.",
            sections.GetProperty("Exceptions").GetString());
    }

    [Fact]
    public void A_clause_anchors_on_the_section_holding_the_table_and_not_on_its_own_id()
    {
        var line = ClauseLines()[0];

        Assert.Equal("TIMEBOX", line.GetProperty("part").GetString());
        Assert.Equal("clauses", line.GetProperty("anchor").GetString());
    }

    [Fact]
    public void A_term_anchors_on_its_own_id()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        Assert.Equal(line.GetProperty("part").GetString(), line.GetProperty("anchor").GetString());
    }

    [Fact]
    public void A_rule_line_carries_the_clauses_its_footnote_gathers()
    {
        var line = RuleLines()[0];

        Assert.Equal("std-TEST.tests-run-on-every-push", line.GetProperty("id").GetString());
        Assert.Equal(["pol-AUTV.BLOCK", "pol-AUTV.MACHINE"],
            line.GetProperty("covers").EnumerateArray().Select(c => c.GetString()));
    }

    [Fact]
    public void A_rule_with_no_footnote_carries_no_clauses()
    {
        var line = RuleLines()[1];

        Assert.Equal("std-TEST.tests-say-what-they-prove", line.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, line.GetProperty("covers").ValueKind);
    }

    [Fact]
    public void The_footnote_is_dropped_from_the_obligations_it_travelled_beside()
    {
        var line = RuleLines()[0];

        Assert.All(line.GetProperty("obligations").EnumerateArray(),
            b => Assert.DoesNotContain("Covers", b.GetProperty("text").GetString()));
    }

    [Fact]
    public void A_rule_line_carries_each_bullet_with_its_level()
    {
        var bullets = RuleLines()[0].GetProperty("obligations").EnumerateArray()
            .Select(b => (b.GetProperty("level").GetString(), b.GetProperty("text").GetString()));

        Assert.Equal(
        [
            ("MUST", "A push **MUST** run the tests."),
            ("MUST NOT", "A push **MUST NOT** skip a failing test, whatever the branch it lands on.")
        ], bullets);
    }

    [Fact]
    public void A_reference_link_in_a_rule_s_bullet_travels_as_an_inline_link()
    {
        var bullet = Assert.Single(RuleLines()[1].GetProperty("obligations").EnumerateArray());

        Assert.Equal("A test name **SHOULD** say what it proves, as [pol-AUTV](autv-automated-verification.md) asks.",
            bullet.Text("text"));
    }

    /// <summary>
    /// The policy type is declared and holds no record. A citation is read against the id prefixes the schema knows, so
    /// a rule citing <c>pol-AUTV</c> gathers nothing where nothing declares <c>pol</c>.
    /// </summary>
    private static List<JsonElement> RuleLines()
    {
        var standards = StandardType();
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                [standards.Key] = standards,
                [PolicyType().Key] = PolicyType()
            }
        };

        var doc = Doc.Parse("standards/std-TEST.md", Standard(), schema);
        Assert.NotNull(doc);

        var corpus = new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = new Tree(new HashSet<string>([doc.Rel], StringComparer.Ordinal),
                rel => rel == doc.Rel ? doc.Text : ""),
            Adopted = [standards],
            Docs = [doc],
            Templates = [],
            SkippedNoFrontmatter = 0
        };

        return
        [
            .. Single(Plan(corpus), "standards/rules.jsonl").Content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => JsonDocument.Parse(l).RootElement)
        ];
    }

    private static string Standard() =>
        """
        ---
        id: std-TEST
        tier: normative
        status: active
        owner: someone
        implements: [ pol-AUTV.BLOCK, pol-AUTV.MACHINE ]
        review-by: "2030-01-01"
        ---

        # Testing

        ## Summary

        What this is for.

        ## Rules

        ### Tests run on every push

        - A push **MUST** run the tests.
        - A push **MUST NOT** skip a failing test, whatever the branch it
          lands on.

        _**Covers:** [pol-AUTV].BLOCK, [pol-AUTV].MACHINE_

        ### Tests say what they prove

        - A test name **SHOULD** say what it proves, as [pol-AUTV] asks.

        [pol-AUTV]: autv-automated-verification.md

        """;

    private static TypeSchema StandardType() => new()
    {
        Key = "standards",
        TypeName = "standard",
        Folder = "standards",
        Page = "standards.md",
        IdPrefix = "std",
        RequiredSections = ["Summary", "Rules"],
        Parts = new PartSpec(PartSpec.Headings, "", ["MUST", "MUST NOT"], ["SHOULD"])
            { Section = "Rules", Noun = "rule", Level = 3 },
        DeclaredFields = [new FieldSpec { Name = "implements", MirrorsCitations = "Covers" }],
        Export = new ExportSpec
        {
            Version = 1,
            Fields = ["id", "title", "implements"],
            Sections = [("Summary", ExportSpec.Full)],
            Parts = ExportSpec.Full,
            PartsDeclared = true,
            Line =
            [
                ("id", PartLineSource.PartId),
                ("title", PartLineSource.PartText),
                ("obligations", PartLineSource.PartBullets),
                ("covers", $"{PartLineSource.CitationPrefix}Covers"),
                ("record", PartLineSource.RecordId),
                ("part", PartLineSource.PartKey)
            ]
        }
    };

    [Fact]
    public void A_reference_two_clauses_cite_is_one_line_naming_both()
    {
        var line = Assert.Single(FrameworkLines(Plan(Aligned())));

        Assert.Equal("ISO 27001:2022", line.GetProperty("framework").GetString());
        Assert.Equal("A.8.13", line.GetProperty("reference").GetString());
        Assert.Equal(["pol-BKUP.COPY", "pol-BKUP.WARM"],
            line.GetProperty("clauses").EnumerateArray().Select(c => c.GetString()));
        Assert.Equal(["pol-BKUP"], line.GetProperty("records").EnumerateArray().Select(r => r.GetString()));
    }

    [Fact]
    public void A_framework_line_carries_the_register_word_and_the_entry_it_sits_at()
    {
        var line = Assert.Single(FrameworkLines(Plan(Aligned())));

        Assert.Equal("Obliged", line.GetProperty("standing").GetString());
        Assert.Equal("frameworks.md", line.GetProperty("path").GetString());
        Assert.Equal("iso-27001", line.GetProperty("anchor").GetString());
    }

    [Fact]
    public void A_framework_line_carries_the_link_to_its_entry()
    {
        var line = Assert.Single(FrameworkLines(Exporter.Plan(Aligned(), Published, null, Run)));

        Assert.Equal(
            "https://github.com/example/corpus/blob/0123456789abcdef0123456789abcdef01234567/frameworks.md#iso-27001",
            line.GetProperty("links").Text("human"));
    }

    [Fact]
    public void The_manifest_names_the_frameworks_file()
        => Assert.Equal("policies/frameworks.jsonl",
            Assert.Single(Plan(Aligned()).Types).FrameworksFile);

    [Fact]
    public void A_type_declaring_no_frameworks_file_writes_none()
    {
        var plan = Plan(Corpus(PolicyType(), ("pol-DATA", Policy())));

        Assert.DoesNotContain(plan.Files, f => f.Path.EndsWith("frameworks.jsonl", StringComparison.Ordinal));
        Assert.Null(Assert.Single(plan.Types).FrameworksFile);
    }

    [Fact]
    public void The_manifest_states_the_status_a_type_treats_as_in_force()
    {
        var plan = Plan(Corpus(PolicyType(inForce: "approved"), ("pol-DATA", Policy())));

        var entry = Assert.Single(JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("types").EnumerateArray());

        Assert.Equal("approved", entry.GetProperty("inForce").GetString());
    }

    [Fact]
    public void A_type_declaring_no_status_states_a_null_in_force()
    {
        var plan = Plan(Corpus(PolicyType(), ("pol-DATA", Policy())));

        var entry = Assert.Single(JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("types").EnumerateArray());

        Assert.Equal(JsonValueKind.Null, entry.GetProperty("inForce").ValueKind);
    }

    /// <summary>
    /// A policy citing a framework by link, beside the register that link reaches. The definition at the foot is what
    /// makes the cell a reference: without one the brackets are literal text and the exporter reads no framework at
    /// all.
    /// </summary>
    private static LoadedCorpus Aligned()
    {
        var corpus = Corpus(PolicyType("frameworks.jsonl"), ("pol-BKUP", AlignedPolicy));

        var files = new HashSet<string>(corpus.Docs.Select(d => d.Rel), StringComparer.Ordinal)
            { "frameworks.md" };

        corpus.Tree = new Tree(files, rel => rel == "frameworks.md"
            ? Register
            : corpus.Docs.FirstOrDefault(d => d.Rel == rel)?.Text ?? "");

        return corpus;
    }

    private const string Register =
        """
        # Frameworks

        ## Obliged

        ### ISO 27001

        Registered against it.
        """;

    private const string AlignedPolicy =
        """
        ---
        id: pol-BKUP
        tier: normative
        status: active
        owner: someone
        review-by: "2030-01-01"
        ---

        # Backups

        ## Purpose

        Why this exists.

        ## Clauses

        | Id     | Clause                          | Alignment               |
        |--------|---------------------------------|-------------------------|
        | `COPY` | **MUST** hold a second copy.    | [ISO 27001:2022].A.8.13 |
        | `WARM` | **MUST** keep one of them warm. | [ISO 27001:2022].A.8.13 |

        [ISO 27001:2022]: ../frameworks.md#iso-27001

        """;

    private static List<JsonElement> FrameworkLines(ExportPlan plan) =>
    [
        .. Single(plan, "policies/frameworks.jsonl").Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement)
    ];

    private static List<JsonElement> ClauseLines() =>
    [
        .. Single(Plan(Corpus(PolicyType(), ("pol-DATA", Policy()))), "policies/clauses.jsonl").Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement)
    ];

    private static string Policy() =>
        """
        ---
        id: pol-DATA
        tier: normative
        status: active
        owner: someone
        review-by: "2030-01-01"
        ---

        # Data

        ## Purpose

        Why this exists.

        ## Clauses

        | Id        | Clause                  | Alignment      |
        |-----------|-------------------------|----------------|
        | `TIMEBOX` | **MUST** be time-boxed. | ISO27001 A.5.1 |
        | `REVIEW`  | SHOULD be reviewed.     |                |

        ## Exceptions

        A finding may be accepted where [pol-DEVI] records who accepted it.

        [pol-DEVI]: devi-deviations.md

        """;

    /// <summary>A policy whose clauses share a level, written in an order alphabetising would not produce.</summary>
    private static string Unsorted() =>
        """
        ---
        id: pol-ORDR
        tier: normative
        status: active
        owner: someone
        review-by: "2030-01-01"
        ---

        # Zones

        ## Purpose

        Why this exists.

        ## Clauses

        | Id      | Clause                                    | Alignment |
        |---------|-------------------------------------------|-----------|
        | `ZONE`  | **MUST** zone a store by what it holds.   |           |
        | `AUDIT` | **MUST** audit each zone once a year.     |           |

        """;

    /// <summary>A policy type declaring no <c>status</c> where <c>inForce</c> is null.</summary>
    private static TypeSchema PolicyType(string frameworks = "", string? inForce = null) => new()
    {
        Fields = inForce is null
            ? []
            : new Dictionary<string, FieldSpec>(StringComparer.Ordinal)
            {
                ["status"] = new()
                {
                    Name = "status", Type = "enum", Values = ["draft", "approved", "retired"], InForce = inForce
                }
            },
        Key = "policies",
        TypeName = "policy",
        Folder = "policies",
        Page = "policies.md",
        IdPrefix = "pol",
        RequiredSections = ["Purpose", "Clauses"],
        Parts = new PartSpec(PartSpec.Table, "", ["MUST", "MUST NOT"], ["SHOULD"])
            { Section = "Clauses", Noun = "clause", Columns = ["Id", "Clause", "Alignment"] },
        Export = new ExportSpec
        {
            Version = 1,
            Fields = ["id", "title"],
            Sections = [("Purpose", ExportSpec.Full), ("Exceptions", ExportSpec.Full)],
            Frameworks = frameworks,
            Parts = ExportSpec.Full,
            PartsDeclared = true,
            Line =
            [
                ("id", PartLineSource.PartId),
                ("clause", PartLineSource.PartText),
                ("level", PartLineSource.PartLevel),
                ("alignment", $"{PartLineSource.ColumnPrefix}Alignment"),
                ("type", PartLineSource.RecordType),
                ("record", PartLineSource.RecordId),
                ("part", PartLineSource.PartKey),
                ("path", PartLineSource.RecordPath),
                ("anchor", PartLineSource.PartAnchor)
            ]
        }
    };

    private static List<JsonElement> TermLines(LoadedCorpus corpus) => Lines(Plan(corpus));

    private static List<JsonElement> Lines(ExportPlan plan) =>
    [
        .. Single(plan, "glossary/terms.jsonl").Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement)
    ];

    private static string Glossary(string id, string? narrows, string terms,
        string reviewBy = "2030-01-01", string status = "draft", string scope = "What this admits.") =>
        $"---\nid: {id}\ntier: descriptive\nstatus: {status}\nowner: someone\n"
        + $"narrows: {narrows}\nreview-by: \"{reviewBy}\"\n---\n\n"
        + $"# {id}\n\n## Scope\n\n{scope}\n\n## Terms\n\n{terms}";

    private static JsonElement Fields(TypeSchema type, string id, string extra)
    {
        var text = $"---\nid: {id}\ntier: descriptive\nstatus: draft\nowner: someone\n"
                   + $"review-by: \"2030-01-01\"\n{extra}\n---\n\n"
                   + $"# {id}\n\n## Scope\n\nWhat this admits.\n\n## Terms\n\n### Alpha\n\nA.\n";

        return JsonDocument.Parse(Single(Plan(Corpus(type, (id, text))), $"glossary/{id}.json").Content)
            .RootElement.GetProperty("fields");
    }

    private static TypeSchema ListType() => FieldType("tags", new FieldSpec
    {
        Name = "tags",
        Type = "list",
        Of = "string"
    });

    private static TypeSchema ObjectListType() => FieldType("aligns-with", new FieldSpec
    {
        Name = "aligns-with",
        Type = "list",
        Of = "object",
        Entry =
        [
            new FieldSpec { Name = "framework", Type = "string" },
            new FieldSpec { Name = "clauses", Type = "list", Of = "string" }
        ]
    });

    private static TypeSchema SectionlessType()
    {
        var type = FieldType("tags", new FieldSpec { Name = "tags", Type = "list", Of = "string" });
        var export = type.DeclaredExport;

        return new TypeSchema
        {
            Key = type.Key,
            TypeName = type.TypeName,
            Folder = type.Folder,
            Page = type.Page,
            IdPrefix = type.IdPrefix,
            RequiredSections = type.RequiredSections,
            Parts = type.Parts,
            Fields = type.Fields,
            Export = new ExportSpec
            {
                Version = export.Version,
                Fields = export.Fields,
                Sections = [],
                Parts = export.Parts,
                PartsDeclared = export.PartsDeclared,
                Line = export.Line
            }
        };
    }

    private static TypeSchema ObjectType() => FieldType("generated", new FieldSpec
    {
        Name = "generated",
        Type = "object",
        Entry =
        [
            new FieldSpec { Name = "at", Type = "timestamp" },
            new FieldSpec { Name = "by", Type = "string" }
        ]
    });

    [Fact]
    public void A_citation_footnote_is_not_exported_as_the_part_s_lead()
    {
        var line = Assert.Single(Lines(Plan(Corpus(Covering(),
            ("gls-one", Glossary("gls-one", null,
                "### Alpha\n\nA, defined.\n\n_**Covers:** [gls-two](gls-two.md).beta_\n"))))));

        Assert.Equal("A, defined.", line.GetProperty("definition").GetString());
    }

    [Fact]
    public void A_part_holding_only_a_citation_footnote_exports_no_lead()
    {
        var line = Assert.Single(Lines(Plan(Corpus(Covering(),
            ("gls-one", Glossary("gls-one", null,
                "### Alpha\n\n_**Covers:** [gls-two](gls-two.md).beta_\n"))))));

        Assert.Equal(JsonValueKind.Null, line.GetProperty("definition").ValueKind);
    }

    /// <summary>
    /// Declared rather than merely present, because the exporter reads the labels off the declared list.
    /// </summary>
    private static TypeSchema Covering()
    {
        var type = GlossaryType();
        return new TypeSchema
        {
            Key = type.Key,
            TypeName = type.TypeName,
            Folder = type.Folder,
            Page = type.Page,
            IdPrefix = type.IdPrefix,
            RequiredSections = type.RequiredSections,
            Parts = type.Parts,
            Export = type.Export,
            DeclaredFields =
                [new FieldSpec { Name = "covers", Refs = ["glossary"], MirrorsCitations = "Covers" }]
        };
    }

    private static TypeSchema FieldType(string name, FieldSpec spec)
    {
        var type = GlossaryType();
        var export = type.DeclaredExport;
        return new TypeSchema
        {
            Key = type.Key,
            TypeName = type.TypeName,
            Folder = type.Folder,
            Page = type.Page,
            IdPrefix = type.IdPrefix,
            RequiredSections = type.RequiredSections,
            Parts = type.Parts,
            Fields = new Dictionary<string, FieldSpec>(StringComparer.Ordinal) { [name] = spec },
            Export = new ExportSpec
            {
                Version = export.Version,
                Fields = [name],
                Sections = export.Sections,
                Parts = export.Parts,
                PartsDeclared = export.PartsDeclared,
                Line = export.Line
            }
        };
    }

    /// <summary>
    /// A loaded corpus holding the glossaries given, in the order given. That is deliberately not the order the export
    /// writes them in, so a passing ordering test is not reading its input back.
    /// </summary>
    private static LoadedCorpus Corpus(params string[] glossaries) =>
        Corpus(GlossaryType(), [.. glossaries.Select(t => (t.Split('\n')[1]["id: ".Length..], t))]);

    private static LoadedCorpus Corpus(TypeSchema type, params (string Id, string Text)[] records)
    {
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [type.Key] = type } };

        var docs = new List<Doc>();
        foreach (var (id, text) in records)
        {
            var doc = Doc.Parse($"{type.Folder}/{id}.md", text, schema);
            Assert.NotNull(doc);
            docs.Add(doc);
        }

        // The tree holds the records, because a cross-reference is a link and a link resolves against
        // what the corpus holds rather than against the disk.
        var tree = new Tree(
            new HashSet<string>(docs.Select(d => d.Rel), StringComparer.Ordinal),
            rel => docs.FirstOrDefault(d => d.Rel == rel)?.Text ?? "");

        return new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = tree,
            Adopted = [type],
            Docs = docs,
            Templates = [],
            SkippedNoFrontmatter = 0
        };
    }
}
