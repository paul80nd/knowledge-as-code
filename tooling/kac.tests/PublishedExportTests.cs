using kac.core;

namespace kac.tests;

public class PublishedExportTests
{
    private static readonly DateOnly Today = new(2026, 10, 23);

    private const string Record = "notes/note-0001.md";

    private const string Descriptor = """
                                      descriptor-version: 6
                                      corpus: example
                                      content-version: "1.0.0"
                                      types: [notes]
                                      """;

    private const string Type = """
                                type: note
                                label: Note
                                label-plural: Notes
                                folder: notes
                                page: notes.md
                                fields:
                                  status:
                                    required: true
                                    type: string
                                  owner-team:
                                    required: false
                                    type: string
                                sections:
                                  required: [Summary]
                                  optional: [History]
                                export:
                                  version: 1
                                  fields: [id, title, status]
                                  sections:
                                    Summary: full
                                """;

    private static string Note(string summary = "What the note says.", string history = "Written once.",
        string extra = "") =>
        $"""
         ---
         id: note-0001
         type: note
         status: active
         {extra}---

         # A note

         ## Summary

         {summary}

         ## History

         {history}
         """;

    private static Dictionary<string, string> Schema(string type = Type) => new(StringComparer.Ordinal)
    {
        ["_enums.yaml"] = "enums: {}",
        ["_tiers.yaml"] = "tiers: {}",
        ["_checks.yaml"] = "checks: {}",
        ["_universal.yaml"] = "fields: {}",
        ["notes.yaml"] = type
    };

    private static Revision At(string name, IReadOnlyDictionary<string, string> files,
        IReadOnlyDictionary<string, string>? schema = null)
    {
        var all = new Dictionary<string, string>(files, StringComparer.Ordinal)
        {
            [".corpus.yaml"] = files.GetValueOrDefault(".corpus.yaml", Descriptor),
            ["notes.md"] = "# Notes\n"
        };

        return new Revision(name,
            new Tree(new HashSet<string>(all.Keys, StringComparer.Ordinal), rel => all[rel]),
            schema ?? Schema());
    }

    private static Revision With(string note, IReadOnlyDictionary<string, string>? schema = null) =>
        At("revision", new Dictionary<string, string>(StringComparer.Ordinal) { [Record] = note }, schema);

    [Fact]
    public void An_edit_to_a_section_the_type_does_not_export_moves_nothing() =>
        Assert.Empty(PublishedExport.Moved(With(Note()), With(Note(history: "Rewritten since.")), Today));

    [Fact]
    public void A_frontmatter_key_the_export_does_not_name_moves_nothing() =>
        Assert.Empty(PublishedExport.Moved(With(Note()), With(Note(extra: "owner-team: platform\n")), Today));

    [Fact]
    public void An_edit_to_an_exported_section_moves_the_record() =>
        Assert.Equal(["notes/note-0001.json"],
            PublishedExport.Moved(With(Note()), With(Note(summary: "What the note says now.")), Today));

    [Fact]
    public void A_deleted_record_moves_its_file_and_the_manifest_counting_it() =>
        Assert.Equal(["manifest.json", "notes/note-0001.json"],
            PublishedExport.Moved(With(Note()), At("revision", new Dictionary<string, string>()), Today));

    [Fact]
    public void A_schema_change_altering_what_a_record_exports_moves_it()
    {
        var wider = Schema(Type.Replace("Summary: full", "Summary: full\n    History: full"));

        Assert.Contains("notes/note-0001.json",
            PublishedExport.Moved(With(Note()), With(Note(), wider), Today));
    }

    [Fact]
    public void A_schema_change_leaving_every_exported_byte_moves_nothing()
    {
        var reworded = Schema(Type.Replace("label: Note", "label: Short note"));

        Assert.Empty(PublishedExport.Moved(With(Note()), With(Note(), reworded), Today));
    }

    [Fact]
    public void Two_builds_over_one_revision_write_the_same_bytes() =>
        Assert.Empty(PublishedExport.Moved(With(Note()), With(Note()), Today));

    [Fact]
    public void A_revision_the_exporter_refuses_stops_the_comparison_and_names_why()
    {
        var refused = At("the branch", new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [".corpus.yaml"] = Descriptor + "\nexport:\n  exclude: [secret]\n",
            [Record] = Note()
        });

        var thrown = Assert.Throws<InvalidOperationException>(() =>
            PublishedExport.Moved(With(Note()), refused, Today));

        Assert.StartsWith("the branch cannot be exported", thrown.Message);
        Assert.Contains("excludes secret, which an export cannot act on", thrown.Message);
    }
}
