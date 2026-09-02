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
    // A manifest short of a key the exporter always writes. Deserialising fills the gap with null
    // whatever the record declares, so the read settles each one before a caller can walk into it.
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

    // A source is named by its shortcode everywhere else in the export, so one naming none names nobody.
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

    // A borrowed address resolves somewhere wrong rather than nowhere, so nothing of the reader's own
    // is filled in for a producer that stated none.
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

    // A glossary as this corpus writes one: `narrows:` orders it, `Scope` travels whole, and the terms
    // under `Terms` are its parts.
    // A caller naming no sections gets `Scope` whole, which is what a glossary declares.
    private static TypeSchema GlossaryType(params (string Section, string Fidelity)[] sections) => new()
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
            Sections = sections.Length > 0 ? sections : [("Scope", ExportSpec.Full)],
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

    // A grep meeting a redefined term meets the general one first, whichever root it belongs under.
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

    // Nothing in the corpus ranks two roots, so this pins the order and deliberately not the generality
    // a reader might take from it.
    [Fact]
    public void Across_unrelated_roots_the_order_is_stable_and_claims_no_generality()
    {
        static string[] Ids(params string[] glossaries) =>
            [.. TermLines(Corpus(glossaries)).Select(l => l.Text("id"))];

        var estate = Glossary("gls-estate", narrows: null, terms: "### Record\n\nA bibliographic description.\n");
        var framework = Glossary("gls-framework", narrows: null, terms: "### Record\n\nA knowledge document.\n");

        // Neither declares `narrows`, so nothing in the corpus ranks them. The same two ids come back
        // whichever order they are loaded in, and that is the whole of the claim.
        Assert.Equal(Ids(estate, framework), Ids(framework, estate));
        Assert.Equal(2, Ids(estate, framework).Length);

        // What a reader needs to tell "refines" from "unrelated" is on the record, and every
        // line reaches it by naming the record it came from.
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

    // Dropping such a record, or leaving it unordered at the end, would hide what the corpus contains.
    [Fact]
    public void A_record_narrowing_something_outside_the_set_is_a_root()
    {
        var lines = TermLines(Corpus(
            Glossary("gls-beta", narrows: "gls-absent", terms: "### Beta\n\nB.\n"),
            Glossary("gls-alpha", narrows: null, terms: "### Alpha\n\nA.\n")));

        Assert.Equal(["gls-alpha.alpha", "gls-beta.beta"], lines.Select(l => l.GetProperty("id").GetString()));
    }

    // Reporting the cycle is the validator's. An export holding the records back would be a second,
    // smaller account of what the corpus holds.
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

    // The wrap is a fact about the file. Carried into the export it defeats what the flat file is for,
    // because a grep for a phrase straddling the break matches nothing.
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

    // Joining a list destroys it, where leaving a paragraph wrapped merely reads as written. The doubt
    // resolves towards leaving the block alone.
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

    // A link's target is stripped out of the prose, so an agent reading `see [gls-two]` is handed a
    // bracket it cannot follow. The ids travel beside the words, and resolve to the part.
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

    // The broken link is `fragment-resolves`'s to report. The export's part is to carry no id rather
    // than a plausible one.
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

    // Only a link reaching another record of the same export can name a part of it.
    [Fact]
    public void A_link_leaving_the_export_is_neither_carried_nor_reported()
    {
        var corpus = Corpus(Glossary("gls-one", null,
            "### Alpha\n\nOwned by [svc-one].\n\n[svc-one]: /services/one.md\n"));

        var line = Assert.Single(TermLines(corpus));
        Assert.Equal(JsonValueKind.Null, line.GetProperty("seeAlso").ValueKind);
        Assert.Empty(Plan(corpus).Unread);
    }

    // A consumer that grepped the flat file has not opened the record, so a hit carrying no state is a
    // definition with nothing saying its glossary is still settling.
    [Fact]
    public void Every_term_line_carries_the_state_of_the_glossary_it_came_from()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n",
            reviewBy: "2020-01-01", status: "draft"))));

        Assert.Equal("draft", line.GetProperty("status").GetString());
        Assert.Equal("2020-01-01", line.GetProperty("reviewBy").GetString());
    }

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

    [Fact]
    public void A_field_the_type_declares_as_a_list_travels_as_an_array()
    {
        var fields = Fields(ListType(), "gls-one", "tags: [ search, discovery ]");

        Assert.Equal(JsonValueKind.Array, fields.GetProperty("tags").ValueKind);
        Assert.Equal(["search", "discovery"], fields.GetProperty("tags").EnumerateArray().Select(e => e.GetString()));
    }

    // A shape `list` refuses, read here anyway so the export never disagrees with `Doc.FrontList` about
    // what a field holds.
    [Fact]
    public void A_list_written_as_one_scalar_travels_as_an_array_of_one()
    {
        var fields = Fields(ListType(), "gls-one", "tags: search");

        Assert.Equal(["search"], fields.GetProperty("tags").EnumerateArray().Select(e => e.GetString()));
    }

    // `[]` beside `null` would give a consumer two spellings of the same absence to branch on.
    [Fact]
    public void A_list_left_empty_is_null_and_never_an_empty_array()
    {
        var fields = Fields(ListType(), "gls-one", "tags:");

        Assert.Equal(JsonValueKind.Null, fields.GetProperty("tags").ValueKind);
    }

    // The reading `Doc.FrontEntries` cannot give: it takes the key naming an entry, which is all a table
    // cell has room for, so a roll-up read that way would carry the framework and drop the references.
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

    // Links built on an empty base would resolve to nothing. The manifest states the same absence.
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

    // A consumer reading one absence as the other would report a record as silent on something its type
    // never asked it to send.
    [Fact]
    public void A_section_the_record_never_wrote_is_absent_where_a_referenced_one_is_null()
    {
        var type = GlossaryType(("Scope", ExportSpec.Reference), ("Provenance", ExportSpec.Reference));
        var sections = Sections(type, "What this admits.");

        Assert.Equal(["Scope"], sections.EnumerateObject().Select(p => p.Name));
        Assert.Equal(JsonValueKind.Null, sections.GetProperty("Scope").ValueKind);
    }

    // Without this a summary reaches a consumer looking exactly like a whole section.
    [Fact]
    public void The_manifest_states_the_fidelity_each_section_travelled_at()
    {
        var type = GlossaryType(("Scope", ExportSpec.Summary));
        var plan = Plan(Corpus(type, ("gls-one", Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        var declared = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("types")[0].GetProperty("sections");

        Assert.Equal("summary", declared.GetProperty("Scope").GetString());
    }

    // The host, the path prefix and the commit sit in the manifest's templates, said once for the whole
    // export rather than sixty times over thirty terms.
    [Fact]
    public void A_term_line_carries_the_two_substitutions_and_no_resolved_link()
    {
        var line = Assert.Single(TermLines(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))));

        Assert.Equal("glossary/gls-one.md", line.GetProperty("path").GetString());
        Assert.Equal("alpha", line.GetProperty("anchor").GetString());
        Assert.False(line.TryGetProperty("links", out _));
    }

    // A consumer that builds a link and one that reads a record's own are never handed two different
    // strings.
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

    // A consumer reads an answer rather than meeting an empty directory.
    [Fact]
    public void A_corpus_with_no_exporting_type_still_writes_a_manifest()
    {
        var plan = Exporter.Plan(Corpus(), null, null, Run);

        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
        Assert.Empty(plan.Types);

        var manifest = JsonDocument.Parse(plan.Files[0].Content).RootElement;
        Assert.Empty(manifest.GetProperty("types").EnumerateArray());
    }

    // One number would be read as either, and for a glossary they differ by an order of magnitude. The
    // parts count sizes the vocabulary and is not derivable without reading the flat file.
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

    // `corpus` tells one export from another. `shortcode` is what a citation writes before the colon, so
    // a consumer resolving `eng:pol-VURM` knows which of the exports it holds answers it.
    [Fact]
    public void The_manifest_carries_the_shortcode_a_citation_scopes_by()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"));
        corpus.Descriptor.Shortcode = "tst";

        var manifest = JsonDocument.Parse(Single(Plan(corpus), Exporter.ManifestFile).Content).RootElement;

        Assert.Equal("test-corpus", manifest.GetProperty("corpus").GetString());
        Assert.Equal("tst", manifest.GetProperty("shortcode").GetString());
    }

    // The absence is stated rather than the key dropped, so a consumer reads one shape either way.
    [Fact]
    public void A_corpus_declaring_no_shortcode_states_the_absence()
        => Assert.Equal(JsonValueKind.Null,
            JsonDocument.Parse(
                    Single(Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n"))), Exporter.ManifestFile)
                        .Content)
                .RootElement.GetProperty("shortcode").ValueKind);

    // A timestamp is the only moving part, which is what lets a consumer diff two exports to see whether
    // the corpus moved.
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

    // The state travels with the record, which is what lets a consumer decide for itself.
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

    // The malformed date is the validator's to report. Withholding the record over it would answer one
    // problem with a quieter one.
    [Fact]
    public void An_unreadable_review_date_is_not_overdue()
    {
        var corpus = Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n", reviewBy: "whenever", status: "active"));
        corpus.Descriptor.ExportExclude.Add(CorpusDescriptor.ExcludeOverdue);

        Assert.Empty(Exporter.Plan(corpus, null, null, Run).Withheld);
    }

    [Fact]
    public void Naming_a_type_narrows_what_is_written_and_leaves_the_rest_out()
    {
        var plan = Exporter.Plan(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), null, "adrs", Run);

        Assert.Equal([Exporter.ManifestFile], plan.Files.Select(f => f.Path));
    }

    // A consumed corpus as `.imports/` states it. Every key a merge stamps is named here rather than
    // assumed, because a producer names its own keys and this is where they arrive from.
    private static InheritedCorpus Consumed(string shortcode, params string[] lines) =>
        new(shortcode, Exporter.FormatVersion, $"{shortcode}-corpus", "1.0.0",
            new ExportPublishing("github", $"https://example.com/{shortcode}/{{path}}#{{anchor}}",
                $"https://example.com/{shortcode}", null, "beefbeef"),
            [],
            [
                new InheritedType("glossary", 1, "glossary", "glossary/terms.jsonl",
                    "record", "part", "id", "seeAlso",
                    new Dictionary<string, string>(StringComparer.Ordinal) { ["Scope"] = ExportSpec.Full },
                    lines,
                    [new InheritedRecord("gls-theirs.json", "{\"type\": \"glossary\"}\n")])
            ]);

    private static ExportPlan Merged(LoadedCorpus corpus, params InheritedCorpus[] consumed) =>
        Exporter.Plan(corpus, null, null, Run, consumed);

    private const string TheirLine =
        """{"id":"gls-theirs.alpha","record":"gls-theirs","seeAlso":["gls-other.beta"],"part":"alpha"}""";

    // All three keys that hold an id, and the line itself. A `seeAlso` left bare would point at whatever
    // this corpus happens to call the same thing, which resolves and resolves wrongly.
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

    // The rule a citation already follows, on a line: bare names this corpus. So a consumer tells its own
    // records from the ones it inherited without being told which corpus it is.
    [Fact]
    public void A_line_this_corpus_wrote_carries_no_name_and_no_shortcode()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));
        var line = Lines(plan).Single(l => l.GetProperty("record").GetString() == "gls-one");

        Assert.Equal("gls-one.alpha", line.GetProperty("id").GetString());
        Assert.False(line.TryGetProperty(Exporter.ShortcodeKey, out _));
    }

    // A line as it reaches a grandchild: `eng` merged `gp` and stamped it, so the line arrives naming the
    // corpus that wrote it rather than the one it came through.
    private const string TheirInheritedLine =
        """{"id":"gp:gls-old.alpha","record":"gp:gls-old","seeAlso":null,"part":"alpha","shortcode":"gp"}""";

    // What makes a grandparent arrive labelled once rather than twice. Restamping would file the record
    // under the corpus this one fetched it through, and send every link for it to the wrong repository.
    [Fact]
    public void A_line_already_naming_its_writer_keeps_that_name()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirInheritedLine));

        var line = Lines(plan).Single(l => l.Text("id").Contains(':'));

        Assert.Equal("gp:gls-old.alpha", line.GetProperty("id").GetString());
        Assert.Equal("gp:gls-old", line.GetProperty("record").GetString());
        Assert.Equal("gp", line.GetProperty(Exporter.ShortcodeKey).GetString());
    }

    // The other half of the same promise. A line naming `gp` resolves only where `gp` is in `sources`,
    // and this corpus never heard of `gp`: it reads the account `eng` published.
    [Fact]
    public void A_corpus_its_producer_consumed_reaches_sources_too()
    {
        var eng = Consumed("eng", TheirInheritedLine) with
        {
            Sources =
            [
                new ExportSource("gp", "gp-corpus", "0.2.0",
                    new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                        "https://example.com/gp", null, "cafecafe"))
            ]
        };

        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), eng);

        var sources = JsonDocument.Parse(Single(plan, Exporter.ManifestFile).Content).RootElement
            .GetProperty("sources").EnumerateArray().ToList();

        Assert.Equal(["eng", "gp"], sources.Select(s => s.GetProperty("shortcode").GetString()));
        Assert.Equal("cafecafe",
            sources[1].GetProperty("publishing").GetProperty("ref").GetString());
    }

    // Two corpora consumed here can each have consumed a third, at two versions. Whichever account won,
    // a line naming that third would resolve to a commit half its records were never read at.
    [Fact]
    public void One_corpus_arriving_twice_at_two_versions_refuses()
    {
        static ExportSource At(string version) =>
            new("gp", "gp-corpus", version,
                new ExportPublishing("github", "https://example.com/gp/{path}#{anchor}",
                    "https://example.com/gp", null, version));

        var plan = Merged(
            Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")),
            Consumed("eng", TheirLine) with { Sources = [At("0.2.0")] },
            Consumed("ops", TheirLine) with { Sources = [At("0.3.0")] });

        Assert.Empty(plan.Files);
        Assert.Contains("0.2.0", Assert.Single(plan.Refused));
        Assert.Contains("0.3.0", plan.Refused[0]);
    }

    // Two corpora can name one record, so an inherited file is filed under the corpus that wrote it. The
    // parts file merges because a line says whose it is; a filename cannot.
    [Fact]
    public void An_inherited_record_is_filed_under_its_producer_s_shortcode()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n")), Consumed("eng", TheirLine));

        Assert.Contains("glossary/eng/gls-theirs.json", plan.Files.Select(f => f.Path));
        Assert.Contains("glossary/gls-one.json", plan.Files.Select(f => f.Path));
    }

    // Two counts in one entry, because a consumer asks how much of a type there is and not how much of it
    // each corpus wrote. Which corpus wrote a record is a fact about the line.
    [Fact]
    public void A_type_entry_counts_what_this_corpus_wrote_and_what_it_inherited_together()
    {
        var plan = Merged(Corpus(Glossary("gls-one", null, "### Alpha\n\nA.\n\n### Beta\n\nB.\n")),
            Consumed("eng", TheirLine));

        var type = Assert.Single(plan.Types);
        Assert.Equal(2, type.Records);
        Assert.Equal(3, type.Parts);
    }

    // The address is the one thing a merge cannot merge. A record of `eng` is read at eng's commit under
    // eng's prefix, and this corpus's own template gets both wrong.
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

    // A rule of this corpus cites a clause of a policy it never adopted, and that citation resolves only
    // where the policy travelled too. So an inherited type arrives whole rather than being filtered to
    // what this corpus happens to hold.
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

    // A merged file whose halves are shaped differently reads as one file and answers two ways, and
    // nothing in the output says so. So the run stops where somebody can act on it.
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

    // The same fault one level down. A skill told a section always travels would be right for half the
    // records and wrong for the rest.
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

    private static ExportPlan Plan(LoadedCorpus corpus) => Exporter.Plan(corpus, null, null, Run);

    // A corpus with an address, for the tests that read one. The base and the ref are `PublishingTests`'
    // business; what matters here is that an export handed one writes a template rather than a link.
    private static readonly Publishing? Published = Publishing.For(
        new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub,
            Base = "https://github.com/example/corpus"
        },
        "0123456789abcdef0123456789abcdef01234567");

    private static ExportFile Single(ExportPlan plan, string path) =>
        Assert.Single(plan.Files, f => f.Path == path);

    // The `sections` object of the one record a type wrote, for the tests that ask what reached it.
    private static JsonElement Sections(TypeSchema type, string scope) =>
        JsonDocument.Parse(
                Single(Plan(Corpus(type, ("gls-one", Glossary("gls-one", null, "### Alpha\n\nA.\n", scope: scope)))),
                    "glossary/gls-one.json").Content)
            .RootElement.GetProperty("sections");

    // What a type declaring its own line buys: two types export parts through one exporter, and neither
    // one's vocabulary reaches the other's file.
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

    // The modal is read from the words rather than from the emphasis around them.
    [Fact]
    public void A_clause_that_does_not_bind_still_carries_its_level()
        => Assert.Equal("SHOULD", ClauseLines()[1].GetProperty("level").GetString());

    [Fact]
    public void A_column_the_row_leaves_empty_carries_a_null()
        => Assert.Equal(JsonValueKind.Null, ClauseLines()[1].GetProperty("alignment").ValueKind);

    // Sorting these would put `AUDIT` first, which is a different policy from the one the page shows.
    [Fact]
    public void Clauses_travel_in_the_order_the_table_writes_them()
    {
        var lines = Single(Plan(Corpus(PolicyType(), ("pol-ORDR", Unsorted()))), "policies/clauses.jsonl")
            .Content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonDocument.Parse(l).RootElement);

        Assert.Equal(["ZONE", "AUDIT"], lines.Select(l => l.GetProperty("part").GetString()));
    }

    // The definitions sit at the foot of the document, which is inside whichever section is written
    // last. They render as nothing, so a section carried as prose must not hand a consumer a run of
    // paths that no reader of the page sees.
    [Fact]
    public void A_carried_section_leaves_the_link_reference_definitions_behind()
    {
        var record = Single(Plan(Corpus(PolicyType(), ("pol-DATA", Policy()))), "policies/pol-DATA.json").Content;
        var sections = JsonDocument.Parse(record).RootElement.GetProperty("sections");

        Assert.Equal("A finding may be accepted where [pol-DEVI] records who accepted it.",
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

        ## Exceptions

        A finding may be accepted where [pol-DEVI] records who accepted it.

        [pol-DEVI]: devi-deviations-are-recorded.md

        """;

    // A policy whose clauses share a level, written in an order alphabetising would not produce.
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
            Sections = [("Purpose", ExportSpec.Full), ("Exceptions", ExportSpec.Full)],
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

    // The `fields` object of one record, over a type and one line of extra frontmatter. The line sits
    // last, because `key-order` is the corpus's business and nothing here reads it.
    private static JsonElement Fields(TypeSchema type, string id, string extra)
    {
        var text = $"---\nid: {id}\ntier: descriptive\nstatus: draft\nowner: someone\n"
                   + $"review-by: \"2030-01-01\"\n{extra}\n---\n\n"
                   + $"# {id}\n\n## Scope\n\nWhat this admits.\n\n## Terms\n\n### Alpha\n\nA.\n";

        return JsonDocument.Parse(Single(Plan(Corpus(type, (id, text))), $"glossary/{id}.json").Content)
            .RootElement.GetProperty("fields");
    }

    // A glossary declaring `tags` as a list of strings and exporting it.
    private static TypeSchema ListType() => FieldType("tags", new FieldSpec
    {
        Name = "tags",
        Type = "list",
        Of = "string"
    });

    // A glossary declaring the shape a policy's `aligns-with` has: a list whose entries are objects, one
    // key naming the entry and one holding a list of its own.
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

    // A footnote reconciling a field against what the part cites is a fact about coverage rather than a
    // piece of the part. Left in, a part carrying nothing else travels with the footnote standing where
    // its words belong, and the consumer reads a coverage line as the definition.
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

    // The glossary type with a field reconciling its citations against a `Covers` line. Declared rather
    // than merely present, because that is the list the exporter reads the labels off.
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
