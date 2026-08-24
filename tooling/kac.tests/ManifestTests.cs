// Unit tests for the two files a corpus is sorted by: the manifest's first-rule-wins layering and where
// `to:` lands what it matched, and the descriptor's own keys, read from `.corpus.yaml` written into a
// temp directory. The engines that act on what they return are in MechanismTests.

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

    // -- where a file lands --

    // A rule naming no destination leaves the path alone, which is every rule of the portability
    // manifest and most of the template's.
    [Fact]
    public void A_rule_with_no_destination_lands_a_file_where_it_was_read()
    {
        var m = new Manifest { Rules = [new ManifestRule(["knowledge-as-code/**"], Manifest.Overlay)] };

        Assert.Equal(
            new Placement(Manifest.Overlay, "knowledge-as-code/taxonomy.md"),
            m.Place("knowledge-as-code/taxonomy.md"));
    }

    // `to:` replaces the pattern's directory prefix, so a whole folder relocates and the shape inside it
    // survives. This is what lets a template be authored in a subdirectory of the repository serving it.
    // A single-file pattern is rewritten by its folder too, so several of them relocate under one `to:`.
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

    // A rule's destination patterns are its own with the same rewrite applied, which is how a check
    // reading the corpus side knows what a corpus may hold there.
    [Theory]
    [InlineData("template/knowledge-as-code/**", "knowledge-as-code/", "knowledge-as-code/**")]
    [InlineData("template/*.md", "", "*.md")]
    [InlineData(".schema/**", null, ".schema/**")]
    public void A_rules_destinations_are_its_patterns_rewritten(string pattern, string? to, string destination)
    {
        var rule = new ManifestRule([pattern], Manifest.Overlay, to);

        Assert.Equal([destination], Manifest.Destinations(rule));
    }

    // A pattern opening on `**/` matches at any depth, so it names no one folder to rewrite. The
    // destination is then `to:` itself, and a rule wanting a tail carried has to name the folder it
    // starts from.
    [Fact]
    public void A_rule_matching_at_any_depth_lands_everything_it_matches_on_one_path()
    {
        var m = new Manifest { Rules = [new ManifestRule(["**/notes.md"], Manifest.Seed, "notes.md")] };

        Assert.Equal("notes.md", m.Place("deeply/nested/notes.md")?.Path);
    }

    // A tombstone is a layer like any other to the reader. What deletes the file is `update`, and what
    // matters here is that the layer survives being read.
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

    // -- the types a corpus has adopted --

    // The two answers are different states, not the same one written two ways. A descriptor that says
    // nothing leaves adoption to the filesystem, so nothing it holds can be surplus to what it declared.
    [Fact]
    public void A_descriptor_declaring_no_types_adopts_everything()
    {
        var descriptor = new CorpusDescriptor();

        Assert.Null(descriptor.Types);
        Assert.True(descriptor.Adopted("adrs"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", "synced", descriptor));
    }

    [Fact]
    public void A_descriptor_declaring_types_declines_the_schema_files_of_the_rest()
    {
        var descriptor = new CorpusDescriptor { Types = ["adrs"] };

        Assert.True(descriptor.Adopted("adrs"));
        Assert.False(descriptor.Adopted("runbooks"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", "synced", descriptor));
        Assert.True(MechanismCheck.Declined(".schema/runbooks.yaml", "synced", descriptor));
    }

    // Everything else under `.schema/` belongs to no type and is shared whatever a corpus adopted, so a
    // corpus is never let off holding it.
    [Theory]
    [InlineData(".schema/_universal.yaml")]
    [InlineData(".schema/_tiers.yaml")]
    [InlineData(".schema/README.md")]
    [InlineData("tooling/kac/Program.cs")]
    [InlineData("runbooks.md")]
    public void Nothing_but_a_type_file_is_ever_declined(string path)
        => Assert.False(MechanismCheck.Declined(path, "synced", new CorpusDescriptor { Types = ["adrs"] }));

    [Fact]
    public void Types_are_read_from_the_descriptor()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"),
            "role: consumer\ntypes:\n  - adrs\n  - policies\n");

        Assert.Equal(["adrs", "policies"], CorpusDescriptor.Load(dir).Types);
    }

    // -- the layer a corpus's role declines --

    // The same decision as declining a type, taken about the tests rather than about the schema. A
    // consumer runs a tool proven upstream, so a fixture it does not hold is neither missing nor drifted.
    [Theory]
    [InlineData("consumer", true)]
    [InlineData("source", false)]
    [InlineData("", false)] // a descriptor that has said nothing is held to everything, as with types
    public void A_consumer_declines_the_verification_layer(string role, bool declined)
    {
        var descriptor = new CorpusDescriptor { Role = role };

        Assert.Equal(!declined, descriptor.Verifies);
        Assert.Equal(declined, MechanismCheck.Declined("tooling/kac.tests/GlobTests.cs", "verification", descriptor));
        Assert.False(MechanismCheck.Declined("tooling/kac/Program.cs", "synced", descriptor));
    }

    // -- the three versions --

    // Each key answers a different question, and the descriptor is read for all three at once.
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

    // A descriptor saying nothing about a version is not one saying zero. The check names the silence.
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

    // The default is the cautious one, so a corpus that never chose is never handed three dozen rewritten
    // seed files by an update it did not ask for.
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

    // A key nested in a block is found where it lives, and one that was dropped rather than renamed says
    // so instead of naming a replacement that does not exist.
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

    [Fact]
    public void A_descriptor_on_a_dropped_key_is_told_to_delete_it()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "upstream:\n  synced-from: ../src\n");

        var message = CorpusDescriptor.RenamedKeyInUse(dir);

        Assert.NotNull(message);
        Assert.Contains("`upstream.synced-from:`", message);
        Assert.Contains("Delete it.", message);
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

    // A corpus with no descriptor at all has no key to rename, and the mechanism command has its own
    // answer for what is missing.
    [Fact]
    public void A_corpus_with_no_descriptor_has_nothing_to_rename()
        => Assert.Null(CorpusDescriptor.RenamedKeyInUse(Directory.CreateTempSubdirectory().FullName));

    // -- what a sync records --

    [Fact]
    public void Stamping_rewrites_the_upstream_block_and_leaves_the_commentary()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path,
            "descriptor-version: 0\ncontent-version: \"2.1.0\"\nrole: consumer\n\nupstream:\n  url:               ../src\n"
            + "  template-version:  1\n  commit:            5fa039b0\n  taken-on:          \"2026-01-01\"\n\n"
            + "# Why a divergence is worth accepting.\nskip: []\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        var after = File.ReadAllText(path);
        Assert.Contains($"descriptor-version: {CorpusDescriptor.Format}\n", after);
        Assert.Contains("  template-version:  3\n", after);
        Assert.Contains("  taken-on:          \"2026-08-11\"\n", after);
        Assert.Contains("  url:               ../src\n", after);   // untouched: the sync does not own it
        Assert.Contains("  commit:            5fa039b0\n", after); // untouched: a sync resolves no commit
        Assert.Contains("content-version: \"2.1.0\"\n", after);    // untouched: only the corpus knows this one
        Assert.Contains("# Why a divergence is worth accepting.", after);
    }

    // The tool owns the file's format, so a descriptor that never stated one is given it, below whatever
    // header comment opens the file and above the first key.
    [Fact]
    public void Stamping_a_descriptor_stating_no_format_writes_one_above_its_first_key()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path, "# What this corpus is.\ncorpus: sample\nrole: consumer\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        Assert.Equal(CorpusDescriptor.Format, CorpusDescriptor.Load(dir).DescriptorVersion);
        Assert.Contains($"# What this corpus is.\ndescriptor-version: {CorpusDescriptor.Format}\ncorpus: sample\n",
            File.ReadAllText(path));
    }

    // A descriptor that has never been synced has no block to rewrite, so the first sync opens one.
    [Fact]
    public void Stamping_a_descriptor_with_no_upstream_block_writes_one()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "role: consumer\n");

        CorpusDescriptor.Stamp(dir, 3, "2026-08-11");

        var reloaded = CorpusDescriptor.Load(dir);
        Assert.Equal("consumer", reloaded.Role);
        Assert.Contains("upstream:", File.ReadAllText(Path.Combine(dir, ".corpus.yaml")));
    }
}
