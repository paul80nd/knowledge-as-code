using System.Text;
using System.Text.Json.Nodes;
using kac.core;

namespace kac.tests;

public class BreadcrumbTests
{
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

    [Fact]
    public void It_states_the_day_the_export_was_taken()
        => Assert.Contains("exported 2026-08-18", Render(Manifest("c", "1.0.0", Type("glossary", 1, 3))));

    [Fact]
    public void It_names_a_lookup_that_survived_the_trim_for_a_known_need()
        => Assert.Contains("Ask a lookup skill such as glossary-lookup for a known need.",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)), included: [Glossary]));

    [Fact]
    public void It_warns_against_answering_from_memory_and_names_no_type()
        => Assert.Contains(
            "These records are the organisation's recorded knowledge, so check them before you answer from what you "
            + "already know.",
            Render(Manifest("c", "1.0.0", Type("standards", 1, 3)),
                included: [new PluginComponent("skills/standards-lookup", ["standards"], null, Breadcrumb.Lookup)]));

    [Fact]
    public void It_sends_a_question_with_no_known_place_to_the_router_and_a_known_need_to_a_lookup()
        => Assert.Contains(
            "Ask the ask-the-corpus skill when you do not know which record answers a question, and a lookup skill "
            + "such as glossary-lookup for a known need.",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)), included: [Router, Glossary]));

    [Fact]
    public void A_router_with_no_announced_lookup_is_named_alone()
        => Assert.Contains("Ask the ask-the-corpus skill when you do not know which record answers a question.\n",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)), included: [Router]));

    [Fact]
    public void It_asks_for_a_finding_where_a_change_alters_what_a_record_describes()
        => Assert.Contains(
            "When your change alters something these records describe, say so with the raise-finding skill.",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)), included: [Glossary, WriteBack]));

    [Fact]
    public void A_role_whose_skill_was_trimmed_is_not_mentioned()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)), included: [Glossary]);

        Assert.DoesNotContain("do not know which record", text);
        Assert.DoesNotContain("say so with", text);
    }

    [Fact]
    public void A_skill_that_does_not_announce_is_left_out()
        => Assert.DoesNotContain("policy-lookup",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)),
                included: [Glossary, new PluginComponent("skills/policy-lookup", ["policies"], null)]));

    [Fact]
    public void A_role_the_breadcrumb_does_not_know_is_left_out()
        => Assert.DoesNotContain("odd-one",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)),
                included: [Glossary, new PluginComponent("skills/odd-one", [], null, "herald")]));

    [Fact]
    public void A_plugin_with_no_skill_points_at_nothing()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", 1, 3)));

        Assert.DoesNotContain("Ask ", text);
        Assert.DoesNotContain("recorded knowledge", text);
    }

    [Theory]
    [InlineData("true", Breadcrumb.Lookup)]
    [InlineData("\"router\"", Breadcrumb.Router)]
    [InlineData("\"write-back\"", Breadcrumb.WriteBack)]
    [InlineData("false", null)]
    [InlineData("1", null)]
    public void An_announce_value_is_read_as_a_role(string json, string? role)
        => Assert.Equal(role, Breadcrumb.Role(JsonNode.Parse(json)));

    [Fact]
    public void A_type_with_no_parts_is_counted_in_records()
    {
        var text = Render(Manifest("c", "1.0.0", Type("adrs", records: 7, parts: 0)));

        Assert.Contains("adrs. 7 records", text);
        Assert.DoesNotContain("entries", text);
    }

    [Fact]
    public void A_type_this_tool_has_never_heard_of_is_described_the_same_way()
        => Assert.Contains("shanties. 12 entries across 2 records: Sea and Land",
            Render(Manifest("c", "1.0.0", Type("shanties", 2, 12)),
                Record("shanties/one.json", "Sea"), Record("shanties/two.json", "Land")));

    [Fact]
    public void The_flat_parts_file_is_not_counted_as_a_record()
        => Assert.DoesNotContain("terms",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/terms.jsonl", "{}"), Record("glossary/gls-estate.json", "Example Libraries")));

    [Fact]
    public void A_record_with_no_title_falls_back_to_its_id()
        => Assert.Contains("gls-estate",
            Render(Manifest("c", "1.0.0", Type("glossary", 1, 4)),
                ("glossary/gls-estate.json", """{"fields":{"id":"gls-estate"}}""")));

    [Fact]
    public void A_type_at_the_threshold_names_every_record()
    {
        var text = Render(Manifest("c", "1.0.0", Type("glossary", records: 6, parts: 30)), Contexts(6));

        Assert.Contains("Context1, Context2, Context3, Context4, Context5 and Context6.", text);
        Assert.DoesNotContain("more", text);
    }

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

    [Fact]
    public void A_source_that_sent_parts_and_no_record_file_counts_its_parts_alone()
    {
        var text = Render(
            Manifest("example-payments", "1.0.0", ["gp"], Type("glossary", records: 0, parts: 2)),
            Parts("gp", "gp"));

        Assert.Contains("glossary (from gp). 2 entries.", text);
        Assert.DoesNotContain("0 records", text);
    }

    [Fact]
    public void Each_corpus_line_names_up_to_the_same_few_records()
    {
        var text = Render(
            Manifest("example-payments", "1.0.0", ["eng"], Type("glossary", records: 14, parts: 0)),
            [
                .. Contexts(7),
                .. Enumerable.Range(1, 7).Select(i => Record($"glossary/eng/gls-{i:D3}.json", $"Eng{i}"))
            ]);

        Assert.Contains("glossary. 7 records: Context1, Context2, Context3, Context4, Context5 and 2 more.", text);
        Assert.Contains("glossary (from eng). 7 records: Eng1, Eng2, Eng3, Eng4, Eng5 and 2 more.", text);
    }

    private static readonly PluginComponent Glossary =
        new("skills/glossary-lookup", ["glossary"], null, Breadcrumb.Lookup);

    private static readonly PluginComponent Router = new("skills/ask-the-corpus", [], null, Breadcrumb.Router);

    private static readonly PluginComponent WriteBack =
        new("skills/raise-finding", [], null, Breadcrumb.WriteBack, Standalone: true);

    private static string Render(
        string manifest,
        params (string Path, string Content)[] exportFiles) =>
        Render(manifest, [], exportFiles);

    private static string Render(
        string manifest,
        IReadOnlyList<PluginComponent> included,
        params (string Path, string Content)[] exportFiles) =>
        Breadcrumb.Render(
            Required.Manifest(manifest),
            [.. exportFiles.Select(f => new BundleFile(f.Path, Encoding.UTF8.GetBytes(f.Content)))],
            included);

    private static string Manifest(string corpus, string? contentVersion, params string[] types) =>
        Manifest(corpus, contentVersion, [], types);

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

    /// <summary>
    /// The flat parts file, one line per part. Each argument is the shortcode the line arrived under, or null for a
    /// part this corpus wrote itself.
    /// </summary>
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

    /// <summary>
    /// <c>count</c> records named <c>Context1</c> upward, in the order the export writes them, which decides which of
    /// them a bounded line names.
    /// </summary>
    private static (string Path, string Content)[] Contexts(int count) =>
        [.. Enumerable.Range(1, count).Select(i => Record($"glossary/gls-{i:D3}.json", $"Context{i}"))];

    private static (string, string) Record(string path, string title) =>
        (path, $$$"""{"fields":{"id":"{{{Path.GetFileNameWithoutExtension(path)}}}","title":"{{{title}}}"}}""");
}
