// The manifest and the descriptor as values. The engines acting on them are in NewTests and UpdateTests.

using kac.core;

namespace kac.tests;

public class ManifestTests
{
    private static Manifest Sample() => new()
    {
        Rules =
        [
            new ManifestRule(["knowledge-as-code/**"], "synced"),
            new ManifestRule(["**/*.md"], "forked"),
            new ManifestRule(["**"], "local") // catch-all
        ]
    };

    [Theory]
    [InlineData("knowledge-as-code/taxonomy.md", "synced")] // first rule wins
    [InlineData("adrs/0001-x.md", "forked")]                // falls through to the .md rule
    [InlineData("scripts/build.txt", "local")]              // only the catch-all matches
    public void Resolve_returns_the_first_matching_rules_layer(string path, string expected)
        => Assert.Equal(expected, Sample().Resolve(path));

    [Fact]
    public void Resolve_is_null_when_no_rule_matches()
    {
        var m = new Manifest { Rules = [new ManifestRule(["knowledge-as-code/**"], "synced")] };
        Assert.Null(m.Resolve("adrs/0001-x.md"));
    }

    // Every rule of the portability manifest names no destination, and most of the template's.
    [Fact]
    public void A_rule_with_no_destination_lands_a_file_where_it_was_read()
    {
        var m = new Manifest { Rules = [new ManifestRule(["knowledge-as-code/**"], Manifest.Overlay)] };

        Assert.Equal(
            new Placement(Manifest.Overlay, "knowledge-as-code/taxonomy.md"),
            m.Place("knowledge-as-code/taxonomy.md"));
    }

    // Rewriting the prefix lets a template be authored in a subdirectory of the repository serving it. A
    // single-file pattern is rewritten by its folder too, so several relocate under one `to:`.
    [Theory]
    [InlineData("template/knowledge-as-code/**", "knowledge-as-code/", "template/knowledge-as-code/taxonomy.md",
        "knowledge-as-code/taxonomy.md")]
    [InlineData("template/**", "", "template/adrs/_template.md", "adrs/_template.md")]
    [InlineData("template/CLAUDE.md", "", "template/CLAUDE.md", "CLAUDE.md")]
    [InlineData("template/.gitignore", "", "template/.gitignore", ".gitignore")]
    [InlineData("template/**/_template.md", "", "template/adrs/_template.md", "adrs/_template.md")]
    [InlineData("template/glossary/knowledge-as-code.md", "glossary/", "template/glossary/knowledge-as-code.md",
        "glossary/knowledge-as-code.md")]
    public void To_replaces_the_patterns_directory_prefix(string pattern, string to, string read, string lands)
    {
        var m = new Manifest { Rules = [new ManifestRule([pattern], Manifest.Seed, to)] };

        Assert.Equal(new Placement(Manifest.Seed, lands), m.Place(read));
    }

    // The rewrite is how a check reading the corpus side knows what a corpus may hold there.
    [Theory]
    [InlineData("template/knowledge-as-code/**", "knowledge-as-code/", "knowledge-as-code/**")]
    [InlineData("template/*.md", "", "*.md")]
    [InlineData(".schema/**", null, ".schema/**")]
    public void A_rules_destinations_are_its_patterns_rewritten(string pattern, string? to, string destination)
    {
        var rule = new ManifestRule([pattern], Manifest.Overlay, to);

        Assert.Equal([destination], Manifest.Destinations(rule));
    }

    // A pattern opening on `**/` matches at any depth, so it names no one folder to rewrite. A rule
    // wanting a tail carried has to name the folder it starts from.
    [Fact]
    public void A_rule_matching_at_any_depth_lands_everything_it_matches_on_one_path()
    {
        var m = new Manifest { Rules = [new ManifestRule(["**/notes.md"], Manifest.Seed, "notes.md")] };

        Assert.Equal("notes.md", m.Place("deeply/nested/notes.md")?.Path);
    }

    // A tombstone is a layer like any other to the reader. What deletes the file is `update`.
    [Fact]
    public void A_tombstone_resolves_to_the_removed_layer()
    {
        var m = new Manifest
        {
            Rules = [new ManifestRule(["template/knowledge-as-code/style.md"], Manifest.Removed, "knowledge-as-code/")]
        };

        Assert.Equal(
            new Placement(Manifest.Removed, "knowledge-as-code/style.md"),
            m.Place("template/knowledge-as-code/style.md"));
    }

    // A descriptor that says nothing leaves adoption to the filesystem, so nothing it holds can be
    // surplus to what it declared.
    [Fact]
    public void A_descriptor_declaring_no_types_adopts_everything()
    {
        var descriptor = new CorpusDescriptor();

        Assert.Null(descriptor.Types);
        Assert.True(descriptor.Adopted("adrs"));
    }

    [Fact]
    public void A_descriptor_declaring_types_declines_the_schema_files_of_the_rest()
    {
        var descriptor = new CorpusDescriptor { Types = ["adrs"] };

        Assert.True(descriptor.Adopted("adrs"));
        Assert.False(descriptor.Adopted("runbooks"));
        Assert.False(Declines(".schema/adrs.yaml"));
        Assert.True(Declines(".schema/runbooks.yaml"));
    }

    // Everything else under `.schema/` belongs to no type and is shared whatever a corpus adopted.
    [Theory]
    [InlineData(".schema/_universal.yaml")]
    [InlineData(".schema/_tiers.yaml")]
    [InlineData(".schema/README.md")]
    [InlineData("README.md")]
    public void Nothing_but_a_type_file_is_ever_declined(string path) => Assert.False(Declines(path));

    // The one predicate deciding what a declined type takes with it, read against the real schema so a
    // type folder renamed there reaches this test.
    private static bool Declines(string path) => New.DeclinesTypes(Schema.Load(Repo.Root), ["adrs"])(path);

    [Fact]
    public void Types_are_read_from_the_descriptor()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "types:\n  - adrs\n  - policies\n");

        Assert.Equal(["adrs", "policies"], CorpusDescriptor.Load(dir).Types);
    }

    // The two names answer two questions: what the corpus calls itself, and what a citation from another
    // corpus writes before the colon.
    [Fact]
    public void A_descriptor_reads_both_of_the_names_a_corpus_has()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "corpus: sample\nshortcode: smp\n");

        var descriptor = CorpusDescriptor.Load(dir);

        Assert.Equal("sample", descriptor.Name);
        Assert.Equal("smp", descriptor.Shortcode);
    }

    // The key `kac new` writes is bare, and it has to reach the validator as absent: an empty string
    // there would be reported as a shortcode that is too short.
    [Theory]
    [InlineData("corpus: sample\n")]
    [InlineData("corpus: sample\nshortcode:\n")]
    public void A_descriptor_declaring_no_shortcode_reads_as_silent(string yaml)
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), yaml);

        Assert.Null(CorpusDescriptor.Load(dir).Shortcode);
    }

    [Fact]
    public void The_three_versions_are_read_from_the_descriptor()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"),
            "descriptor-version: 1\ncorpus: sample\ncontent-version: \"2.1.0\"\n"
            + "upstream:\n  template-version: 3\n  ref: main\n  commit: 5fa039b0\n  taken-on: \"2026-08-20\"\n");

        var descriptor = CorpusDescriptor.Load(dir);

        Assert.Equal(1, descriptor.DescriptorVersion);
        Assert.Equal("2.1.0", descriptor.ContentVersion);
        Assert.Equal(3, descriptor.TemplateVersion);
        Assert.Equal("main", descriptor.UpstreamRef);
        Assert.Equal("5fa039b0", descriptor.UpstreamCommit);
        Assert.Equal("2026-08-20", descriptor.TakenOn);
    }

    // A descriptor saying nothing about a version is not one saying zero.
    [Fact]
    public void A_descriptor_stating_no_versions_reads_as_silent()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "corpus: sample\n");

        var descriptor = CorpusDescriptor.Load(dir);

        Assert.Null(descriptor.DescriptorVersion);
        Assert.Null(descriptor.ContentVersion);
        Assert.Null(descriptor.TemplateVersion);
    }

    // A corpus that never chose is never handed three dozen rewritten seed files by an update it did not
    // ask for.
    [Fact]
    public void A_descriptor_naming_no_update_policy_is_cautious()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "corpus: sample\n");

        Assert.Equal(CorpusDescriptor.Cautious, CorpusDescriptor.Load(dir).UpdatePolicy);
    }

    [Fact]
    public void A_descriptor_reads_the_files_it_skips()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"),
            "update-policy: full\nskip:\n  - path: .plugin/hooks/breadcrumb\n    reason: Patched for our proxy.\n");

        var descriptor = CorpusDescriptor.Load(dir);

        Assert.Equal(CorpusDescriptor.Full, descriptor.UpdatePolicy);
        Assert.Equal([new SkippedFile(".plugin/hooks/breadcrumb", "Patched for our proxy.")], descriptor.Skipped);
    }

    // The migration is the author's to make, so the message has to carry everything the edit needs.
    [Fact]
    public void A_descriptor_on_the_old_version_key_is_told_what_to_rename_it_to()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "version: 1\ncorpus: sample\n");

        var message = CorpusDescriptor.RenamedKeyInUse(dir);

        Assert.NotNull(message);
        Assert.Contains("`version:`", message);
        Assert.Contains("`descriptor-version:`", message);
        Assert.Contains(Path.Combine(dir, ".corpus.yaml"), message);
    }

    // A key nested in a block is found where it lives.
    [Theory]
    [InlineData("upstream:\n  mechanism-version: 3\n", "`upstream.mechanism-version:`", "`template-version:`")]
    [InlineData("upstream:\n  synced-on: \"2026-01-01\"\n", "`upstream.synced-on:`", "`taken-on:`")]
    [InlineData("accepted-divergences: []\n", "`accepted-divergences:`", "`skip:`")]
    public void A_descriptor_on_a_renamed_key_is_told_what_it_became(string content, string names, string becomes)
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), content);

        var message = CorpusDescriptor.RenamedKeyInUse(dir);

        Assert.NotNull(message);
        Assert.Contains(names, message);
        Assert.Contains(becomes, message);
    }

    // Naming what took over makes the fix one edit rather than a search.
    [Theory]
    [InlineData("upstream:\n  synced-from: ../src\n", "`upstream.synced-from:`", "upstream.url")]
    [InlineData("role: consumer\n", "`role:`", "prove the tool")]
    public void A_descriptor_on_a_dropped_key_is_told_to_delete_it(string yaml, string names, string because)
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), yaml);

        var message = CorpusDescriptor.RenamedKeyInUse(dir);

        Assert.NotNull(message);
        Assert.Contains(names, message);
        Assert.Contains("delete it", message);
        Assert.Contains(because, message);
    }

    [Theory]
    [InlineData("descriptor-version: 1\ncorpus: sample\n")] // renamed already
    [InlineData("corpus: sample\n")]                        // never held the key
    public void A_descriptor_using_no_renamed_key_passes(string content)
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), content);

        Assert.Null(CorpusDescriptor.RenamedKeyInUse(dir));
    }

    // `update` has its own answer for a missing descriptor.
    [Fact]
    public void A_corpus_with_no_descriptor_has_nothing_to_rename()
        => Assert.Null(CorpusDescriptor.RenamedKeyInUse(Directory.CreateTempSubdirectory().FullName));

    [Fact]
    public void Stamping_rewrites_the_upstream_block_and_leaves_the_commentary()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path,
            "descriptor-version: 0\ncontent-version: \"2.1.0\"\n\nupstream:\n  url:               ../src\n"
            + "  template-version:  1\n  commit:            5fa039b0\n  taken-on:          \"2026-01-01\"\n\n"
            + "# Why owning a file is worth declaring.\nskip: []\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11", "9c4e1d2a");

        var after = File.ReadAllText(path);
        Assert.Contains($"descriptor-version: {CorpusDescriptor.Format}\n", after);
        Assert.Contains("  template-version:  3\n", after);
        Assert.Contains("  taken-on:          \"2026-08-11\"\n", after);
        Assert.Contains("  commit:            9c4e1d2a\n", after);
        Assert.Contains("  url:               ../src\n", after); // untouched: the update does not own it
        Assert.Contains("content-version: \"2.1.0\"\n", after);  // untouched: only the corpus knows this one
        Assert.Contains("# Why owning a file is worth declaring.", after);
    }

    // A template read from a folder resolves no commit, and a key filled with one nobody resolved is
    // worse than a key left as it stands.
    [Fact]
    public void Stamping_without_a_commit_leaves_the_one_already_recorded()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, "upstream:\n  commit:            5fa039b0\n  template-version:  1\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        Assert.Contains("  commit:            5fa039b0\n", File.ReadAllText(path));
    }

    // The tool owns the file's format.
    [Fact]
    public void Stamping_a_descriptor_stating_no_format_writes_one_above_its_first_key()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, "# What this corpus is.\ncorpus: sample\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        Assert.Equal(CorpusDescriptor.Format, CorpusDescriptor.Load(dir).DescriptorVersion);
        Assert.Contains($"# What this corpus is.\ndescriptor-version: {CorpusDescriptor.Format}\ncorpus: sample\n",
            File.ReadAllText(path));
    }

    // The first update is the corpus recording where it takes from for the first time.
    [Fact]
    public void Stamping_a_descriptor_with_no_upstream_block_writes_one()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "corpus: sample\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        var reloaded = CorpusDescriptor.Load(dir);
        Assert.Equal("sample", reloaded.Name);
        Assert.Equal(3, reloaded.TemplateVersion);
        Assert.Contains("upstream:", File.ReadAllText(Path.Combine(dir, ".corpus.yaml")));
    }

    // What a corpus consumes, read as the file states it. Every field is what was written, because an
    // entry short of anything is refused by name where the message can say which key is missing.
    [Fact]
    public void The_consumes_block_is_read_entry_by_entry()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), Consumes);

        Assert.Equal(
            [
                new Consumed("example-engineering", "eng", "^0.1.0", "0.1.0", "https://feed.example/index.json"),
                new Consumed("example-security", "sec", "1.0.0", null, "https://feed.example/index.json")
            ],
            CorpusDescriptor.Load(dir).Consumes);
    }

    [Fact]
    public void A_corpus_consuming_nothing_reads_as_consuming_nothing()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "corpus: sample\n");

        Assert.Empty(CorpusDescriptor.Load(dir).Consumes);
    }

    // The lock lands on the entry it belongs to, whether or not that entry already carried one, and the
    // range and the reason somebody wrote beside it are both still there afterwards.
    [Fact]
    public void Resolving_writes_the_version_onto_each_entry_and_leaves_the_commentary()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, Consumes);

        CorpusDescriptor.SetResolved(dir, new Dictionary<string, string>
        {
            ["example-engineering"] = "0.1.4",
            ["example-security"] = "1.0.0"
        });

        var after = File.ReadAllText(path);
        Assert.Contains("# Pinned while the vocabulary settles.", after);
        Assert.Contains("    version: ^0.1.0\n    resolved: \"0.1.4\"\n", after);
        Assert.Contains("    version: 1.0.0\n    source: https://feed.example/index.json\n"
                        + "    resolved: \"1.0.0\"\n", after);

        Assert.Equal(["0.1.4", "1.0.0"], CorpusDescriptor.Load(dir).Consumes.Select(c => c.Resolved));
    }

    // An entry the run did not resolve is left as it stands, so a refusal on one dependency never
    // rewrites the lock of another.
    [Fact]
    public void An_entry_the_restore_did_not_resolve_keeps_the_lock_it_had()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, Consumes);

        CorpusDescriptor.SetResolved(dir,
            new Dictionary<string, string> { ["example-security"] = "1.0.0" });

        Assert.Equal(["0.1.0", "1.0.0"], CorpusDescriptor.Load(dir).Consumes.Select(c => c.Resolved));
    }

    // The two passes over one file have to agree on what an entry says. A YAML comment after a value is
    // not part of it, and a writer reading it as one silently writes no lock at all.
    [Fact]
    public void A_comment_after_a_value_is_not_part_of_it()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path,
            "consumes:\n  - corpus: example-engineering  # our policies live here\n"
            + "    shortcode: eng\n    version: ^0.1.0\n");

        var written = CorpusDescriptor.SetResolved(dir,
            new Dictionary<string, string> { ["example-engineering"] = "0.1.4" });

        Assert.Equal(["example-engineering"], written);
        Assert.Equal("0.1.4", Assert.Single(CorpusDescriptor.Load(dir).Consumes).Resolved);
    }

    // A dash standing alone opens an entry whose keys are on the lines below it, which the loader reads
    // and this pass has to read the same way.
    [Fact]
    public void A_dash_on_a_line_of_its_own_opens_an_entry()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path,
            "consumes:\n  -\n    corpus: example-engineering\n    shortcode: eng\n    version: ^0.1.0\n");

        CorpusDescriptor.SetResolved(dir,
            new Dictionary<string, string> { ["example-engineering"] = "0.1.4" });

        Assert.Equal("0.1.4", Assert.Single(CorpusDescriptor.Load(dir).Consumes).Resolved);
    }

    // A shape this pass cannot place is reported by its absence from the return, so a caller says what
    // landed rather than saying everything did.
    [Fact]
    public void An_entry_this_pass_cannot_place_is_left_out_of_what_it_reports()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"),
            "consumes: [{corpus: example-engineering, shortcode: eng, version: ^0.1.0}]\n");

        Assert.Empty(CorpusDescriptor.SetResolved(dir,
            new Dictionary<string, string> { ["example-engineering"] = "0.1.4" }));
    }

    // A run that resolved every entry to the lock it already carried has changed nothing, and a file
    // nothing edited should not report as edited.
    [Fact]
    public void Resolving_to_the_lock_already_written_leaves_the_file_alone()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, Consumes);
        var written = File.GetLastWriteTimeUtc(path);

        CorpusDescriptor.SetResolved(dir,
            new Dictionary<string, string> { ["example-engineering"] = "0.1.0" });

        Assert.Equal(Consumes, File.ReadAllText(path));
        Assert.Equal(written, File.GetLastWriteTimeUtc(path));
    }

    // A `consumes:` block with two entries, one already locked and one not, and a comment inside it that
    // a rewrite has to leave standing.
    private const string Consumes =
        """
        corpus: sample

        consumes:
          # Pinned while the vocabulary settles.
          - corpus: example-engineering
            shortcode: eng
            version: ^0.1.0
            resolved: "0.1.0"
            source: https://feed.example/index.json

          - corpus: example-security
            shortcode: sec
            version: 1.0.0
            source: https://feed.example/index.json

        skip: []

        """;
}
