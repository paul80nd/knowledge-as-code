using System.Text;
using System.Text.Json.Nodes;
using kac.core;

// What a session is told at the moment it starts, which is the only moment the plugin gets before the
// agent has already decided what a word means.
//
// These read the rendered text rather than a structure, because the text is the artefact: nothing
// parses a breadcrumb, and a line that reads wrongly is wrong however well-formed it was.

namespace kac.tests;

public class BreadcrumbTests
{
    // The count tells a reader at a glance that the export is present and non-empty, which is the
    // failure a silently absent bundle otherwise looks exactly like.
    [Fact]
    public void It_states_the_corpus_its_version_the_count_and_the_records()
    {
        var text = Render(Manifest("example-libraries", "2.3.4", Type("glossary", records: 2, parts: 41)),
            Record("glossary/gls-estate.json", "Example Libraries"),
            Record("glossary/gls-search.json", "Search"));

        Assert.Contains("example-libraries 2.3.4", text);
        Assert.Contains("41 entries across 2 records", text);
        Assert.Contains("Example Libraries and Search", text);
    }

    // An export reads the same however long ago it was written, and a session is entitled to know it is
    // holding last year's vocabulary.
    [Fact]
    public void It_states_the_day_the_export_was_taken()
        => Assert.Contains("exported 2026-08-18", Render(Manifest("c", "1.0.0", Type("glossary", 1, 3))));

    // The skill line is where a session takes the question the breadcrumb has just created. A plugin
    // cannot advertise a skill it did not ship.
    [Fact]
    public void It_names_the_skills_that_survived_the_trim()
        => Assert.Contains("Ask the glossary-lookup skill",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)),
                included: [new PluginComponent("skills/glossary-lookup", ["glossary"], null, Announce: true)]));

    // Three skills announce and each asks a different question, so the sentence names none of them. A
    // corpus shipping the standards skill and no glossary would otherwise be warned about words alone.
    [Fact]
    public void The_skill_line_warns_against_answering_from_memory_and_names_no_type()
        => Assert.Contains("Ask the standards-lookup skill before you answer from what you already know.",
            Render(Manifest("c", "1.0.0", Type("standards", 1, 3)),
                included: [new PluginComponent("skills/standards-lookup", ["standards"], null, Announce: true)]));

    // A skill somebody asks for by name already has its question, so it is not worth a line every
    // session pays for. `announce` on the manifest entry is what separates the two.
    [Fact]
    public void A_skill_that_does_not_announce_is_left_out()
        => Assert.DoesNotContain("policy-lookup",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)),
                included:
                [
                    new PluginComponent("skills/glossary-lookup", ["glossary"], null, Announce: true),
                    new PluginComponent("skills/policy-lookup", ["policies"], null)
                ]));

    // No skill name is written here to fall back on.
    [Fact]
    public void A_plugin_with_no_skill_points_at_nothing()
        => Assert.DoesNotContain("Ask the", Render(Manifest("c", "1.0.0", Type("glossary", 1, 3))));

    // "0 entries" would read as an empty export, which is the one thing the count is there to rule out.
    [Fact]
    public void A_type_with_no_parts_is_counted_in_records()
    {
        var text = Render(Manifest("c", "1.0.0", Type("adrs", records: 7, parts: 0)));

        Assert.Contains("adrs. 7 records", text);
        Assert.DoesNotContain("entries", text);
    }

    // Nothing in the renderer names a record type.
    [Fact]
    public void A_type_this_tool_has_never_heard_of_is_described_the_same_way()
        => Assert.Contains("shanties. 12 entries across 2 records: Sea and Land",
            Render(Manifest("c", "1.0.0", Type("shanties", 2, 12)),
                Record("shanties/one.json", "Sea"), Record("shanties/two.json", "Land")));

    // The parts file sits in the type's own directory. Counting it would put a filename among the record
    // names, where every other entry is something a person recognises.
    [Fact]
    public void The_flat_parts_file_is_not_counted_as_a_record()
        => Assert.DoesNotContain("terms",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/terms.jsonl", "{}"), Record("glossary/gls-estate.json", "Example Libraries")));

    // A gap where a record should be would read as a corpus covering one fewer context than it does.
    [Fact]
    public void A_record_with_no_title_falls_back_to_its_id()
        => Assert.Contains("gls-estate",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/gls-estate.json", """{"fields":{"id":"gls-estate"}}""")));

    // What a session pays for the line is fixed by the renderer, not by the corpus. A handful of titles
    // is what says which contexts a type covers.
    [Fact]
    public void A_type_at_the_threshold_names_every_record()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", records: 6, parts: 30)), Contexts(6));

        Assert.Contains("Context1, Context2, Context3, Context4, Context5 and Context6.", text);
        Assert.DoesNotContain("more", text);
    }

    // One line covers a corpus of any size.
    [Fact]
    public void A_type_past_the_threshold_names_the_first_few_and_counts_the_rest()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", records: 200, parts: 4000)), Contexts(200));

        Assert.Contains("Context1, Context2, Context3, Context4, Context5 and 195 more.", text);
        Assert.DoesNotContain("Context6", text);
    }

    [Fact]
    public void One_record_over_the_threshold_is_still_bounded_by_it()
        => Assert.Contains("Context1, Context2, Context3, Context4, Context5 and 2 more.",
            Render(Manifest("c", "1.0.0", Type("glossary", records: 7, parts: 35)), Contexts(7)));

    // A merged type counts several corpora's records together, and the number is the load-bearing part
    // of the line. Read under the installing corpus's own name it sends a reader looking for records
    // nobody there wrote.
    [Fact]
    public void A_type_merged_from_a_source_is_counted_under_each_corpus_that_wrote_it()
    {
        var text = Render(
            Manifest("example-payments", "1.0.0", ["eng"], Type("glossary", records: 2, parts: 3)),
            Record("glossary/gls-local.json", "Payments"),
            Record("glossary/eng/gls-shared.json", "Engineering"),
            Parts(null, "eng", "eng"));

        Assert.Contains("glossary. 1 entry across 1 record: Payments.", text);
        Assert.Contains("glossary (from eng). 2 entries across 1 record: Engineering.", text);
    }

    // Every record of the type came from somewhere else, and a line for the installing corpus would
    // report it holding a glossary of its own.
    [Fact]
    public void A_type_the_installing_corpus_wrote_none_of_gets_no_line_of_its_own()
    {
        var text = Render(
            Manifest("example-payments", "1.0.0", ["eng"], Type("glossary", records: 1, parts: 2)),
            Record("glossary/eng/gls-shared.json", "Engineering"),
            Parts("eng", "eng"));

        Assert.Contains("glossary (from eng). 2 entries across 1 record: Engineering.", text);
        Assert.DoesNotContain("\nglossary. ", text);
    }

    // The bound on how many records a line names is what a session pays for the text, and it is per
    // line rather than per type.
    [Fact]
    public void Each_corpus_line_names_up_to_the_same_few_records()
    {
        var text = Render(
            Manifest("example-payments", "1.0.0", ["eng"], Type("glossary", records: 14, parts: 0)),
            [.. Contexts(7), .. Enumerable.Range(1, 7).Select(i => Record($"glossary/eng/gls-{i:D3}.json", $"Eng{i}"))]);

        Assert.Contains("glossary. 7 records: Context1, Context2, Context3, Context4, Context5 and 2 more.", text);
        Assert.Contains("glossary (from eng). 7 records: Eng1, Eng2, Eng3, Eng4, Eng5 and 2 more.", text);
    }

    private static string Render(
        string manifest,
        params (string Path, string Content)[] exportFiles) =>
        Render(manifest, [], exportFiles);

    private static string Render(
        string manifest,
        IReadOnlyList<PluginComponent> included,
        params (string Path, string Content)[] exportFiles) =>
        Breadcrumb.Render(
            (JsonObject)JsonNode.Parse(manifest)!,
            [.. exportFiles.Select(f => new BundleFile(f.Path, Encoding.UTF8.GetBytes(f.Content)))],
            included);

    private static string Manifest(string corpus, string? contentVersion, params string[] types) =>
        Manifest(corpus, contentVersion, [], types);

    // `sources` names the corpora this one consumes, which is what the exporter merged into the types
    // below and what the renderer reads to say whose records are whose.
    private static string Manifest(string corpus, string? contentVersion, string[] sources, params string[] types) =>
        $$"""
          {
            "formatVersion": {{Exporter.FormatVersion}},
            "corpus": "{{corpus}}",
            "contentVersion": {{(contentVersion is null ? "null" : $"\"{contentVersion}\"")}},
            "generatedAt": "2026-08-18T18:48:53Z",
            "sources": [{{string.Join(",", sources.Select(c => $"{{\"shortcode\":\"{c}\"}}"))}}],
            "types": [{{string.Join(",", types)}}]
          }
          """;

    // The flat parts file, one line per part. Each argument is the shortcode the line arrived under,
    // or null for a part this corpus wrote itself, which is the only place the split is written down.
    private static (string, string) Parts(params string?[] sources) =>
        ("glossary/terms.jsonl", string.Join("\n", sources.Select((c, i) =>
            c is null
                ? $$"""{"id":"gls-local.t{{i}}"}"""
                : $$"""{"id":"{{c}}:gls-shared.t{{i}}","shortcode":"{{c}}"}""")));

    private static string Type(string type, int records, int parts) =>
        $$"""
          {"type":"{{type}}","records":{{records}},"parts":{{parts}},
           "dir":"{{type}}","partsFile":"{{type}}/terms.jsonl"}
          """;

    // `count` records of one type, named `Context1` upward and ordered as the export orders them, which
    // is what decides which of them a bounded line names.
    private static (string Path, string Content)[] Contexts(int count) =>
        [.. Enumerable.Range(1, count).Select(i => Record($"glossary/gls-{i:D3}.json", $"Context{i}"))];

    private static (string, string) Record(string path, string title) =>
        (path, $$$"""{"fields":{"id":"{{{Path.GetFileNameWithoutExtension(path)}}}","title":"{{{title}}}"}}""");
}
