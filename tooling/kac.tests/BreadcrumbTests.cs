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
    // The four things the breadcrumb exists to state, in one line and one more. The count is the load
    // bearing one: it is what tells a reader at a glance that the export is present and non-empty,
    // which is the failure a silently absent bundle otherwise looks exactly like.
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

    // The date the copy was taken, because an export reads the same however long ago it was written and
    // a session is entitled to know it is holding last year's vocabulary.
    [Fact]
    public void It_states_the_day_the_export_was_taken()
        => Assert.Contains("exported 2026-08-18", Render(Manifest("c", "1.0.0", Type("glossary", 1, 3))));

    // Where to take the question the breadcrumb has just created. The skill is named from the components
    // that survived the trim, so a plugin cannot advertise a skill it did not ship.
    [Fact]
    public void It_names_the_skills_that_survived_the_trim()
        => Assert.Contains("Ask the glossary-lookup skill",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)),
                included: [new PluginComponent("skills/glossary-lookup", ["glossary"], null)]));

    // A plugin carrying no skill points at nothing rather than at a name written here. The breadcrumb's
    // job is to create the question, and a plugin with nothing to answer it should not pretend.
    [Fact]
    public void A_plugin_with_no_skill_points_at_nothing()
        => Assert.DoesNotContain("Ask the", Render(Manifest("c", "1.0.0", Type("glossary", 1, 3))));

    // A type whose records hold no addressable parts is counted in records alone. "0 entries" would
    // read as an empty export, which is the one thing the count is there to rule out.
    [Fact]
    public void A_type_with_no_parts_is_counted_in_records()
    {
        var text = Render(Manifest("c", "1.0.0", Type("adrs", records: 7, parts: 0)));

        Assert.Contains("adrs — 7 records", text);
        Assert.DoesNotContain("entries", text);
    }

    // Nothing here names a record type. A corpus adopting a type this tool has never heard of gets a
    // breadcrumb about it without a line changing in the renderer.
    [Fact]
    public void A_type_this_tool_has_never_heard_of_is_described_the_same_way()
        => Assert.Contains("shanties — 12 entries across 2 records: Sea and Land",
            Render(Manifest("c", "1.0.0", Type("shanties", 2, 12)),
                Record("shanties/one.json", "Sea"), Record("shanties/two.json", "Land")));

    // The parts file sits in the type's own directory and is not a record. Counting it would put a
    // filename among the record names, where every other entry is something a person recognises.
    [Fact]
    public void The_flat_parts_file_is_not_counted_as_a_record()
        => Assert.DoesNotContain("terms",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/terms.jsonl", "{}"), Record("glossary/gls-estate.json", "Example Libraries")));

    // A record the export wrote without a title still gets a name. A gap where one record should be
    // would read as a corpus covering one fewer context than it does.
    [Fact]
    public void A_record_with_no_title_falls_back_to_its_id()
        => Assert.Contains("gls-estate",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/gls-estate.json", """{"fields":{"id":"gls-estate"}}""")));

    // What a session pays for the line is fixed by the renderer, not by the corpus. Six names is the
    // threshold; six are all named, because a handful of titles is what says which contexts a type
    // covers.
    [Fact]
    public void A_type_at_the_threshold_names_every_record()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", records: 6, parts: 30)), Contexts(6));

        Assert.Contains("Context1, Context2, Context3, Context4, Context5 and Context6.", text);
        Assert.DoesNotContain("more", text);
    }

    // Past it the line names what it can and counts the rest, so it is one line for a corpus of any size.
    // The remainder is stated rather than dropped: a list cut short silently reads as the whole of what
    // the type covers.
    [Fact]
    public void A_type_past_the_threshold_names_the_first_few_and_counts_the_rest()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", records: 200, parts: 4000)), Contexts(200));

        Assert.Contains("Context1, Context2, Context3, Context4, Context5 and 195 more.", text);
        Assert.DoesNotContain("Context6", text);
    }

    // One record over the threshold is still bounded by it: the line names five and counts the two it did
    // not, rather than growing to seven.
    [Fact]
    public void One_record_over_the_threshold_is_still_bounded_by_it()
        => Assert.Contains("Context1, Context2, Context3, Context4, Context5 and 2 more.",
            Render(Manifest("c", "1.0.0", Type("glossary", records: 7, parts: 35)), Contexts(7)));

    // -- helpers --

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
        $$"""
          {
            "formatVersion": {{Exporter.FormatVersion}},
            "corpus": "{{corpus}}",
            "contentVersion": {{(contentVersion is null ? "null" : $"\"{contentVersion}\"")}},
            "generatedAt": "2026-08-18T18:48:53Z",
            "types": [{{string.Join(",", types)}}]
          }
          """;

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
