using System.Text.Json;
using kac.core;

// In-process unit tests for the export.
//
// The golden fixture holds one corpus and pins what the CLI writes for it, file for file. It cannot
// hold a second, and several rules need one: ordering is about several roots with a chain beneath one
// of them, exclusion is about a corpus that declares it, and reproducibility is about running twice
// over one tree. That is what these build.

namespace kac.tests;

public class ExporterTests
{
    private static readonly ExportRun Run =
        new("2026-08-17T00:00:00Z", new DateOnly(2026, 8, 17), "abc123", false);

    // A glossary as this corpus writes one: `narrows:` orders it, `Scope` travels whole, and the terms
    // under `Terms` are its parts.
    private static TypeSchema GlossaryType() => new()
    {
        Key = "glossary",
        TypeName = "glossary",
        Folder = "glossary",
        Page = "glossary.md",
        IdPrefix = "gls",
        RequiredSections = ["Scope", "Terms"],
        Parts = new PartSpec(PartSpec.Headings, "", [], [])
            { Section = "Terms", Noun = "term", Level = 3, Aside = "Not" },
        Export = new ExportSpec
        {
            Version = 1,
            Fields = ["id", "title", "narrows", "status", "review-by"],
            Sections = [("Scope", ExportSpec.Full)],
            Parts = ExportSpec.Full,
            PartsDeclared = true,
            Line =
            [
                ("id", PartLineSource.PartId),
                ("title", PartLineSource.PartText),
                ("definition", PartLineSource.PartLead),
                ("not", PartLineSource.PartAside),
                ("seeAlso", PartLineSource.PartSeeAlso),
                ("type", PartLineSource.RecordType),
                ("record", PartLineSource.RecordId),
                ("part", PartLineSource.PartKey),
                ("status", $"{PartLineSource.FrontPrefix}status"),
                ("reviewBy", $"{PartLineSource.FrontPrefix}review-by"),
                ("path", PartLineSource.RecordPath),
                ("anchor", PartLineSource.PartAnchor)
            ]
        }
    };

    // -- ordering: roots by id, each root's chain beneath it, and what that does and does not mean --

    // `narrows` orders a chain and nothing orders one chain against another, so the roots sort by id and
    // each root's chain follows it. A grep meeting a redefined term therefore meets the general one
    // first, whichever root it belongs under.
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

    // Generality holds inside a chain, and this is the case the ordering was built for: `gls-narrow`
    // narrows `gls-general`, both define `Title`, and a grep meets the general one first.
    [Fact]
    public void Inside_a_chain_the_general_entry_comes_before_the_one_refining_it()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-narrow", narrows: "gls-general", terms: "### Title\n\nThe indexed field.\n"),
            Glossary("gls-general", narrows: null, terms: "### Title\n\nThe work itself.\n")));

        Assert.Equal(["gls-general.title", "gls-narrow.title"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    // Across unrelated roots the order is stable and means nothing else. This asserts only that the
    // order does not move, and deliberately not that either entry is the general one.
    [Fact]
    public void Across_unrelated_roots_the_order_is_stable_and_claims_no_generality()
    {
        string[] Ids(params string[] glossaries) =>
            [.. TermLines(Corpus(glossaries)).Select(l => l.GetProperty("id").GetString()!)];

        var estate = Glossary("gls-estate", narrows: null, terms: "### Record\n\nA bibliographic description.\n");
        var framework = Glossary("gls-framework", narrows: null, terms: "### Record\n\nA knowledge document.\n");

        // Neither declares `narrows`, so nothing in the corpus ranks them. The same two ids come back
        // whichever order they are loaded in, and that is the whole of the claim.
        Assert.Equal(Ids(estate, framework), Ids(framework, estate));
        Assert.Equal(2, Ids(estate, framework).Length);

        // What a reader needs to tell "refines" from "unrelated" is on the record, and every
        // line reaches it by naming the record it came from.
        foreach (var line in TermLines(Corpus(estate, framework)))
            Assert.NotEmpty(line.GetProperty("record").GetString()!);
    }

    [Fact]
    public void Terms_sort_alphabetically_inside_a_glossary()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", narrows: null, terms: "### Zebra\n\nLast.\n\n### Apple\n\nFirst.\n")));

        Assert.Equal(["Apple", "Zebra"], lines.Select(l => l.GetProperty("title").GetString()));
    }

    // A record naming a glossary the corpus does not hold has nothing here to sit beneath, so it is a
    // root. Dropping it, or leaving it unordered at the end, would hide a record the corpus contains.
    [Fact]
    public void A_record_narrowing_something_outside_the_set_is_a_root()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-beta", narrows: "gls-absent", terms: "### Beta\n\nB.\n"),
            Glossary("gls-alpha", narrows: null, terms: "### Alpha\n\nA.\n")));

        Assert.Equal(["gls-alpha.alpha", "gls-beta.beta"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    // A cycle leaves its members unreachable from any root. Reporting it is the validator's; an export
    // that quietly held the records back would be a second, smaller account of what the corpus holds.
    [Fact]
    public void A_cycle_still_exports_every_record_it_traps()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-one", narrows: "gls-two", terms: "### One\n\nA.\n"),
            Glossary("gls-two", narrows: "gls-one", terms: "### Two\n\nB.\n")));

        Assert.Equal(["gls-one.one", "gls-two.two"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    // -- a term, split as the type writes one --

    [Fact]
    public void A_term_carries_its_definition_and_the_labelled_line_beneath_it_apart()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Query\n\nWhat a reader typed.\n\n**Not:** the request the service received.\n"))));

        Assert.Equal("What a reader typed.", line.GetProperty("definition").GetString());
        Assert.Equal("the request the service received.", line.GetProperty("not").GetString());
    }

    // Most terms carry no such line: the label marks a confusion worth heading off, and most words
    // have none. So its absence is a null rather than an empty string standing in for one.
    [Fact]
    public void A_term_with_no_labelled_line_carries_a_null_rather_than_an_empty_one()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null,
            "### Facet\n\nA field a reader narrows results by.\n"))));

        Assert.Equal(JsonValueKind.Null, line.GetProperty("not").ValueKind);
    }

    // A line has to be parseable on its own, because a grep hands back a line and nothing around it.
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

    // -- the wrap column, taken back out --

    // The corpus wraps at 120 columns. Those breaks are a fact about the file, and carried into the
    // export they defeat the one thing the flat file is for: a grep for a phrase straddling the wrap
    // matches nothing.
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

    // A blank line is the author's, and says something a wrap does not.
    [Fact]
    public void A_paragraph_break_in_a_section_survives_the_unwrapping()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n",
                    scope: "One paragraph that\nwraps.\n\nA second\nparagraph."))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal("One paragraph that wraps.\n\nA second paragraph.",
            record.GetProperty("sections").GetProperty("Scope").GetString());
    }

    // Joining a list into one line destroys it, where leaving a paragraph wrapped merely reads as it was
    // written. The doubt is resolved towards leaving the block alone.
    [Fact]
    public void A_list_is_left_exactly_as_written()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n",
                    scope: "What this admits:\n\n- one thing\n- another thing"))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal("What this admits:\n\n- one thing\n- another thing",
            record.GetProperty("sections").GetProperty("Scope").GetString());
    }

    // -- cross-references --

    // A link's target is stripped out of the prose, so an agent reading `see [gls-two]` in the text
    // alone is handed a bracket it cannot follow. The ids are carried beside the words.
    //
    // They resolve to the **part**, and only where the link names one.
    [Fact]
    public void A_reference_naming_the_file_and_no_term_carries_nothing_and_is_reported()
    {
        var corpus = Corpus(
            Glossary("gls-one", null, "### Record\n\nOne sense.\n\n**Not:** the other — see [gls-two].\n\n"
                                      + "[gls-two]: gls-two.md\n"),
            Glossary("gls-two", null, "### Record\n\nThe other sense.\n"));

        var referring = TermLines(corpus).Single(l => l.GetProperty("id").GetString() == "gls-one.record");
        Assert.Equal(JsonValueKind.Null, referring.GetProperty("seeAlso").ValueKind);

        // Both glossaries happen to call the term `record`, so the counterpart is guessable here.
        // Guessing it is what this asserts the export does not do. The same guess is silently wrong for
        // a pair that does not share a word, and the run reports the link instead.
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

    // An anchor naming a heading the target does not hold carries nothing. The link is broken, which is
    // `fragment-resolves`'s to report; the export's part is to carry no id rather than a plausible one.
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

    // A link to something outside the export (a service, an ADR) is not a cross-reference and is not
    // reported as one. Only a link reaching another record of the same export can name a part of it.
    [Fact]
    public void A_link_leaving_the_export_is_neither_carried_nor_reported()
    {
        var corpus = Corpus(Glossary("gls-one", null,
            "### Alpha\n\nOwned by [svc-one].\n\n[svc-one]: /services/one.md\n"));

        var line = Assert.Single(TermLines(corpus));
        Assert.Equal(JsonValueKind.Null, line.GetProperty("seeAlso").ValueKind);
        Assert.Empty(Plan(corpus).Unread);
    }

    // -- the trust marker --

    // `terms.jsonl` is what gets grepped, and a consumer that grepped it has not opened the record. A
    // hit carrying no state is a definition with nothing saying its glossary is still settling.
    [Fact]
    public void Every_term_line_carries_the_state_of_the_glossary_it_came_from()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n",
            reviewBy: "2020-01-01", status: "draft"))));

        Assert.Equal("draft", line.GetProperty("status").GetString());
        Assert.Equal("2020-01-01", line.GetProperty("reviewBy").GetString());
    }

    // -- one spelling of absent --

    // `""` here beside `null` there would make the spelling of absent a property of which file was
    // opened.
    [Fact]
    public void A_field_left_blank_is_null_and_never_an_empty_string()
    {
        var record = JsonDocument.Parse(
                Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), "glossary/gls-one.json").Content)
            .RootElement;

        Assert.Equal(JsonValueKind.Null, record.GetProperty("fields").GetProperty("narrows").ValueKind);
    }

    // -- the record --

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

    // A corpus the tool cannot address writes its records without links rather than with links built on
    // an empty base. The absence is the honest answer and the manifest says the same thing.
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
        Assert.Equal(JsonValueKind.Null, publishing.GetProperty("rawTemplate").ValueKind);
    }

    // -- the address a line carries --

    // A line names what varies and nothing else. The host, the path prefix and the commit are in the
    // manifest's templates, said once for the whole export rather than sixty times over thirty terms.
    [Fact]
    public void A_term_line_carries_the_two_substitutions_and_no_resolved_link()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        Assert.Equal("glossary/gls-one.md", line.GetProperty("path").GetString());
        Assert.Equal("alpha", line.GetProperty("anchor").GetString());
        Assert.False(line.TryGetProperty("links", out _));
    }

    // The two carriers agree. Substituting a line's `path` and `anchor` into the manifest's templates
    // produces the addresses the record file resolves for itself, so a consumer that builds a link and
    // one that reads a record's own are never handed two different strings.
    [Fact]
    public void Substituting_a_line_into_the_templates_gives_the_links_the_record_carries()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Published, null, Run);

        var publishing = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("publishing");
        var links = JsonDocument.Parse(Single(plan, "glossary/gls-one.json").Content).RootElement
            .GetProperty("links");
        var line = Assert.Single(Lines(plan));

        var path = line.GetProperty("path").GetString()!;
        var anchor = line.GetProperty("anchor").GetString()!;

        Assert.Equal(
            links.GetProperty("raw").GetString(),
            publishing.GetProperty("rawTemplate").GetString()!.Replace(Publishing.PathToken, path));
        Assert.Equal(
            $"{links.GetProperty("human").GetString()}#{anchor}",
            publishing.GetProperty("humanTemplate").GetString()!
                .Replace(Publishing.PathToken, path)
                .Replace(Publishing.AnchorToken, anchor));
    }

    // -- the manifest --

    // "Nothing" is a valid statement of what a corpus has. A corpus that adopted no exporting type still
    // produces a manifest, so a consumer reads an answer rather than meeting an empty directory.
    [Fact]
    public void A_corpus_with_no_exporting_type_still_writes_a_manifest()
    {
        var plan = Exporter.Plan(Corpus(), null, null, Run);

        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
        Assert.Empty(plan.Types);

        var manifest = JsonDocument.Parse(plan.Files[0].Content).RootElement;
        Assert.Empty(manifest.GetProperty("types").EnumerateArray());
    }

    // Two counts, named apart. One number would be read as either, and for a glossary they differ by an
    // order of magnitude: three files, thirty terms. The parts count is what sizes the vocabulary, and
    // it is not derivable without reading the flat file.
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

    // Two names for one corpus, and the manifest carries both. `corpus` tells one export from another,
    // and `shortcode` is what a citation writes before the colon, so a consumer resolving `eng:pol-VURM`
    // knows which of the exports it holds answers it.
    [Fact]
    public void The_manifest_carries_the_shortcode_a_citation_scopes_by()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.Shortcode = "tst";

        var manifest = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement;

        Assert.Equal("test-corpus", manifest.GetProperty("corpus").GetString());
        Assert.Equal("tst", manifest.GetProperty("shortcode").GetString());
    }

    // A corpus nothing cites has declared none, and the manifest states the absence rather than dropping
    // the key. A consumer then reads one shape either way.
    [Fact]
    public void A_corpus_declaring_no_shortcode_states_the_absence()
        => Assert.Equal(JsonValueKind.Null,
            JsonDocument.Parse(
                    Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), Exporter.ManifestFile)
                        .Content)
                .RootElement.GetProperty("shortcode").ValueKind);

    // Two runs over one commit differ in the timestamp and nowhere else, which is what makes an export
    // something a consumer can diff to see whether the corpus moved.
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

    // -- what a corpus may leave behind --

    // The default, and the one that matters: a draft glossary and one long past its review date both
    // travel, carrying the state that lets a consumer decide.
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

    // An absent or unreadable date is never overdue. It is the field's own fault and the validator's to
    // report, and withholding a record over it would answer one problem with a quieter one.
    [Fact]
    public void An_unreadable_review_date_is_not_overdue()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n", reviewBy: "whenever", status: "active"));
        corpus.Descriptor.ExportExclude.Add(CorpusDescriptor.ExcludeOverdue);

        Assert.Empty(Exporter.Plan(corpus, null, null, Run).Withheld);
    }

    // -- narrowing what is written --

    [Fact]
    public void Naming_a_type_narrows_what_is_written_and_leaves_the_rest_out()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), null, "adrs", Run);

        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
    }

    // -- helpers --

    private static ExportPlan Plan(LoadedCorpus corpus) => Exporter.Plan(corpus, null, null, Run);

    // A corpus with an address, for the tests that read one. The bases and the ref are `PublishingTests`'
    // business; what matters here is that an export handed one writes templates rather than links.
    private static readonly Publishing? Published = Publishing.For(
        new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub,
            HumanBase = "https://github.com/example/corpus/blob",
            RawBase = "https://raw.githubusercontent.com/example/corpus"
        },
        "0123456789abcdef0123456789abcdef01234567");

    private static ExportFile Single(ExportPlan plan, string path) =>
        Assert.Single(plan.Files, f => f.Path == path);

    // -- a type whose parts are rows, and whose line says so --

    // A clause carries its modal and the type's own third column, and carries none of a glossary's
    // words. This is what a type declaring its own line buys: two types export parts through one
    // exporter, and neither one's vocabulary reaches the other's file.
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

    // An advisory clause is plain rather than bold, so the modal is read from the words rather than
    // from the emphasis around them.
    [Fact]
    public void A_clause_that_does_not_bind_still_carries_its_level()
        => Assert.Equal("SHOULD", ClauseLines()[1].GetProperty("level").GetString());

    // An empty cell is the absence of a value, spelled as every other absence in an export is.
    [Fact]
    public void A_column_the_row_leaves_empty_carries_a_null()
        => Assert.Equal(JsonValueKind.Null, ClauseLines()[1].GetProperty("alignment").ValueKind);

    private static List<JsonElement> ClauseLines() =>
    [
        .. Single(Plan(Corpus(PolicyType(), ("pol-DATA", Policy()))), "policies/clauses.jsonl").Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement)
    ];

    // A policy as this corpus writes one: clauses are rows under `Clauses`, the binding ones bold.
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

        """;

    // The type behind those clauses. Its line names a modal and a column, and neither has a home in a
    // glossary's line.
    private static TypeSchema PolicyType() => new()
    {
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
            Sections = [("Purpose", ExportSpec.Full)],
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

    // A loaded corpus holding the glossaries given, in the order given. That is deliberately not the
    // order the export writes them in, so a passing ordering test is not reading its input back.
    private static LoadedCorpus Corpus(params string[] glossaries) =>
        Corpus(GlossaryType(), [.. glossaries.Select(t => (t.Split('\n')[1]["id: ".Length..], t))]);

    // The same, for a type the caller supplies. A record is named by its id, as every record here is.
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
