using System.IO.Compression;
using System.Text;
using kac.core;

// In-process unit tests for the consuming half of publishing.
//
// There is no fixture corpus behind these and there could not be. A restore is decided by what a
// registry answers, and a golden that reached one would prove somebody else's uptime. So the feed is
// stood up here as a dictionary of URLs, and the packages it serves are built by `Packer` from an export
// written for the case. That is what holds the two halves together: a package these tests refuse is one
// `pack` could really have produced.

namespace kac.tests;

public class RestoreTests
{
    private const string Source = "https://feed.example/index.json";

    [Fact]
    public void A_declaration_with_no_lock_takes_the_highest_version_its_range_admits()
    {
        var step = Assert.Single(Plan([Declared(range: "^0.1.0")], Feed(["0.1.0", "0.1.4", "0.2.0"])).Steps);

        Assert.Equal("0.1.4", step.Version);
        Assert.False(step.Current);
    }

    // The lock is what the last restore actually took, so a restore from an unchanged descriptor asks
    // the registry nothing and takes the same version however much has been published since.
    [Fact]
    public void A_lock_the_range_still_admits_is_taken_without_asking_the_registry()
    {
        var asked = new List<string>();
        var feed = Watched(Feed(["0.1.0", "0.9.0"]), asked);

        Assert.Equal("0.1.0", Assert.Single(Plan([Declared(range: "^0.1.0", resolved: "0.1.0")], feed).Steps)
            .Version);
        Assert.DoesNotContain(asked, url => url.EndsWith("example-engineering/index.json", StringComparison.Ordinal));
    }

    // Which is how changing the declaration takes effect. A range moved past its lock is a corpus asking
    // for something else, and the lock it carries is an answer to the question it used to ask.
    [Fact]
    public void A_lock_the_range_no_longer_admits_is_resolved_again()
        => Assert.Equal("0.2.0",
            Assert.Single(Plan([Declared(range: "^0.2.0", resolved: "0.1.0")], Feed(["0.1.0", "0.2.0"])).Steps)
                .Version);

    // The folder is what a restore produces, so a folder already holding the resolved version is the
    // produced thing and nothing is fetched.
    [Fact]
    public void A_corpus_already_unpacked_at_the_resolved_version_is_left_alone()
    {
        var step = Assert.Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.1.0")).Steps);

        Assert.True(step.Current);
        Assert.Empty(step.Files);
    }

    // The identity checks a fetch is held to are skipped on this path, so a folder judged by version
    // alone would leave one corpus's records standing under another corpus's shortcode.
    [Fact]
    public void A_folder_holding_another_corpus_at_the_right_version_is_fetched_again()
        => Assert.False(Assert
            .Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.1.0", "example-security")).Steps)
            .Current);

    [Fact]
    public void A_corpus_unpacked_at_another_version_is_fetched_again()
        => Assert.False(Assert.Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.0.9")).Steps)
            .Current);

    // One folder per shortcode, so two declarations claiming one would restore over each other and
    // whichever ran last would win in silence.
    [Fact]
    public void A_shortcode_two_declarations_both_claim_is_refused_naming_both()
    {
        var problem = Assert.Single(Plan(
            [Declared(), Declared(corpus: "another-corpus")],
            Feed(["0.1.0"])).Problems);

        Assert.Contains("example-engineering", problem);
        Assert.Contains("another-corpus", problem);
    }

    // The other direction, and the lock is what makes it an error. A version is written back onto the
    // entry naming a corpus, so two entries naming one corpus have one place to record two answers.
    [Fact]
    public void A_corpus_two_declarations_both_consume_is_refused_naming_both_shortcodes()
    {
        var problem = Assert.Single(Plan(
            [Declared(), Declared(shortcode: "gov")],
            Feed(["0.1.0"])).Problems);

        Assert.Contains("consumed twice, as 'eng' and 'gov'", problem);
    }

    // The producer owns the spelling, and every citation in the consuming corpus is written against the
    // one it declared. A package filed under a name that resolves nothing is worse than no package.
    [Fact]
    public void A_package_whose_stamped_shortcode_disagrees_with_the_declaration_is_refused()
    {
        var problem = Assert.Single(
            Plan([Declared(shortcode: "gov")], Feed(["0.1.0"])).Problems);

        Assert.Contains("cited as 'eng:'", problem);
        Assert.Contains("declares it as 'gov:'", problem);
    }

    // A registry serving the wrong file under an id is unlikely; a `source:` pointing at the wrong feed
    // is not.
    [Fact]
    public void A_package_calling_itself_another_corpus_is_refused()
        => Assert.Contains("calls itself 'example-engineering'",
            Assert.Single(Plan([Declared(corpus: "example-payments")],
                Feed(["0.1.0"], id: "example-payments", stamped: "example-engineering")).Problems));

    // The question `pack` and `bundle` both ask of an export. A package written to a contract this build
    // has never read is refused rather than half-understood.
    //
    // Zipped by hand rather than sealed, because `pack` refuses to seal an export at a format version it
    // does not itself write. The package this refuses came from a later build of the tool.
    [Fact]
    public void A_package_at_another_export_format_version_is_refused()
        => Assert.Contains("Upgrade kac",
            Assert.Single(Plan([Declared()], Serving(Zip([
                ($"{Packer.PayloadDir}/{Exporter.ManifestFile}",
                    ManifestJson("example-engineering", "eng", "0.1.0", Exporter.FormatVersion + 1))
            ]))).Problems));

    [Fact]
    public void A_package_that_is_not_an_archive_is_refused()
        => Assert.Contains("not a readable package",
            Assert.Single(Plan([Declared()], Serving("not a zip"u8.ToArray())).Problems));

    // A package is somebody else's file and a zip may name an entry anywhere it likes.
    [Theory]
    [InlineData("corpus/../../escaped.json")]
    [InlineData("corpus//absolute.json")]
    public void An_entry_addressing_a_path_outside_the_import_folder_is_refused(string entry)
        => Assert.Contains("addresses a path outside",
            Assert.Single(Plan([Declared()], Serving(Zip([(entry, "{}")]))).Problems));

    [Fact]
    public void A_package_carrying_no_export_manifest_is_refused()
        => Assert.Contains("no readable corpus/manifest.json",
            Assert.Single(Plan([Declared()], Serving(Zip([("corpus/glossary/terms.jsonl", "{}")]))).Problems));

    // Each key a restore cannot proceed without, refused by name so the fix is one line in one entry.
    [Theory]
    [InlineData("corpus", "names no corpus")]
    [InlineData("shortcode", "declares no shortcode")]
    [InlineData("version", "states no version")]
    [InlineData("source", "names no source")]
    public void A_declaration_missing_what_a_restore_needs_is_refused_by_name(string key, string said)
    {
        var entry = new Consumed(
            key == "corpus" ? null : "example-engineering",
            key == "shortcode" ? null : "eng",
            key == "version" ? null : "0.1.0",
            null,
            key == "source" ? null : Source);

        Assert.Contains(said, Assert.Single(Plan([entry], Feed(["0.1.0"])).Problems));
    }

    // A shortcode is a folder name as well as a citation, so a value that is not one is refused before
    // it is joined to a path.
    [Fact]
    public void A_declaration_whose_shortcode_is_not_one_is_refused()
        => Assert.Contains("is too long",
            Assert.Single(Plan([Declared(shortcode: "../../etc")], Feed(["0.1.0"])).Problems));

    [Fact]
    public void A_version_that_is_not_a_range_is_refused()
        => Assert.Contains("neither `1.2.0` nor `^1.2.0`",
            Assert.Single(Plan([Declared(range: ">=1.0.0")], Feed(["0.1.0"])).Problems));

    // Named, because the fix is either the range or a release upstream, and which one depends on what is
    // published.
    [Fact]
    public void A_range_no_published_version_satisfies_names_what_the_registry_holds()
        => Assert.Contains("It holds 0.1.0, 0.2.0",
            Assert.Single(Plan([Declared(range: "^1.0.0")], Feed(["0.1.0", "0.2.0"])).Problems));

    // A private feed answers an anonymous read the same way an empty one does, and nothing here can tell
    // them apart, so the message carries both readings.
    [Fact]
    public void A_corpus_the_registry_has_never_held_names_the_token_as_well()
    {
        var problem = Assert.Single(Plan([Declared()], Feed([])).Problems);

        Assert.Contains("holds no version of it at all", problem);
        Assert.Contains(Registry.TokenVariable, problem);
    }

    [Fact]
    public void A_registry_that_cannot_be_read_is_reported_rather_than_read_as_empty()
        => Assert.Contains("could not read the registry",
            Assert.Single(Plan([Declared()], _ => new Fetched(null, 500, "500 Server Error")).Problems));

    // A refusal anywhere stops the whole run. A corpus with two of its three imports on disk validates
    // against a graph nobody declared.
    [Fact]
    public void Nothing_is_planned_where_any_declaration_is_refused()
        => Assert.Empty(Plan([Declared(), Declared(corpus: "example-payments", shortcode: "pay", range: "nope")],
            Feed(["0.1.0"])).Steps);

    // The envelope is a registry's business. What a consumer reads is the export, at the paths the
    // export wrote it at.
    [Fact]
    public void The_payload_prefix_is_stripped_and_the_envelope_dropped()
    {
        var step = Assert.Single(Plan([Declared()], Feed(["0.1.0"], ["glossary/terms.jsonl"])).Steps);

        Assert.Equal(["glossary/terms.jsonl", "manifest.json"], step.Files.Select(f => f.Path));
    }

    [Fact]
    public void A_restored_file_carries_the_bytes_the_export_wrote()
    {
        var step = Assert.Single(Plan([Declared()], Feed(["0.1.0"], ["glossary/terms.jsonl"])).Steps);

        Assert.Equal("{\"term\":\"resilience\"}\n",
            Encoding.UTF8.GetString(step.Files.Single(f => f.Path == "glossary/terms.jsonl").Content));
    }

    // A record withdrawn upstream has to leave. The folder is replaced whole rather than written over,
    // so what is on disk afterwards is what the package holds and nothing else.
    [Fact]
    public void A_restore_replaces_the_import_folder_whole()
    {
        var root = Temp();
        var stale = Path.Combine(root, Restore.ImportsDir, "eng", "glossary", "withdrawn.json");
        Directory.CreateDirectory(Path.GetDirectoryName(stale)!);
        File.WriteAllText(stale, "{}");

        Restore.Write(root, Plan([Declared()], Feed(["0.1.0"], ["glossary/terms.jsonl"])));

        Assert.False(File.Exists(stale));
        Assert.True(File.Exists(Path.Combine(root, Restore.ImportsDir, "eng", "glossary", "terms.jsonl")));
    }

    // Same lock in, same bytes out. A consumer's validation has to be reproducible, and a restore that
    // wrote something different from one run to the next would be the one thing making it not.
    [Fact]
    public void Two_restores_of_one_lock_write_the_same_bytes()
    {
        var root = Temp();
        var declared = new[] { Declared(range: "^0.1.0", resolved: "0.1.0") };
        var feed = Feed(["0.1.0", "0.2.0"], ["glossary/terms.jsonl"]);

        Restore.Write(root, Plan(declared, feed));
        var first = Read(root);

        Restore.Write(root, Plan(declared, feed));

        Assert.Equal(first, Read(root));
    }

    // What is on disk is what a later run asks about, and what it reports is what actually arrived
    // rather than what something recorded having asked for.
    [Fact]
    public void What_a_folder_holds_is_read_from_the_manifest_that_arrived()
    {
        var root = Temp();
        Restore.Write(root, Plan([Declared()], Feed(["0.1.0"])));

        Assert.Equal(new Imported("example-engineering", "0.1.0"), Restore.Installed(root, "eng"));
        Assert.Null(Restore.Installed(root, "pay"));
    }

    // The manifest is what `Installed` reads a folder's identity from, so a write that stopped halfway
    // must not have left one. Written last, a folder with a manifest is a folder with its records.
    [Fact]
    public void The_manifest_is_the_last_file_written()
    {
        var step = Assert.Single(Plan([Declared()],
            Feed(["0.1.0"], ["glossary/terms.jsonl", "policies/clauses.jsonl"])).Steps);

        Assert.Equal(Exporter.ManifestFile, step.Files[^1].Path);
    }

    private static RestorePlan Plan(
        IReadOnlyList<Consumed> declared, Func<string, Fetched> feed,
        Func<string, Imported?>? installed = null) =>
        Restore.Plan(declared, new Registry(feed), installed ?? (_ => null));

    // A folder under `.imports/` holding one corpus at one version, whatever shortcode is asked about.
    private static Func<string, Imported?> Holding(string version, string corpus = "example-engineering") =>
        _ => new Imported(corpus, version);

    private static Consumed Declared(
        string corpus = "example-engineering",
        string shortcode = "eng",
        string range = "0.1.0",
        string? resolved = null) =>
        new(corpus, shortcode, range, resolved, Source);

    // A feed answering the three requests a restore makes: the service index, the version listing, and
    // the package itself at every version listed.
    private static Func<string, Fetched> Feed(
        string[] versions,
        string[]? files = null,
        string id = "example-engineering",
        string shortcode = "eng",
        string? stamped = null)
    {
        const string flat = "https://feed.example/download/";

        var served = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [Source] = Utf8($$"""
                              {"resources":[{"@id":"{{flat}}","@type":"PackageBaseAddress/3.0.0"}]}
                              """),
            [$"{flat}{id}/index.json"] = Utf8($$"""{"versions":[{{Quoted(versions)}}]}""")
        };

        foreach (var version in versions)
            served[$"{flat}{id}/{version}/{id}.{version}.nupkg"] =
                Package(stamped ?? id, shortcode, version, files ?? []);

        return url => served.TryGetValue(url, out var body)
            ? new Fetched(body, 200, null)
            : new Fetched(null, 404, "404 Not Found");
    }

    // A feed serving one package at every address a restore could ask a package for, for the cases about
    // an archive rather than about resolution.
    private static Func<string, Fetched> Serving(byte[] package)
    {
        var feed = Feed(["0.1.0"]);

        return url => url.EndsWith(".nupkg", StringComparison.Ordinal)
            ? new Fetched(package, 200, null)
            : feed(url);
    }

    private static Func<string, Fetched> Watched(Func<string, Fetched> feed, List<string> asked) =>
        url =>
        {
            asked.Add(url);
            return feed(url);
        };

    // A package as `kac pack` would have sealed it, so what these tests refuse is what a producer could
    // really have published.
    private static byte[] Package(string id, string shortcode, string version, string[] files)
    {
        var export = new List<BundleFile>
        {
            new(Exporter.ManifestFile, Utf8(ManifestJson(id, shortcode, version, Exporter.FormatVersion)))
        };

        export.AddRange(files.Select(f => new BundleFile(f, Utf8("{\"term\":\"resilience\"}\n"))));
        return Packer.Archive(Packer.Plan(export));
    }

    // An export manifest carrying the four keys a restore reads from it.
    private static string ManifestJson(string id, string shortcode, string version, int format) =>
        $$"""
          {
            "formatVersion": {{format}},
            "corpus": "{{id}}",
            "shortcode": "{{shortcode}}",
            "contentVersion": "{{version}}",
            "types": []
          }
          """;

    // An archive built entry by entry, for the shapes `pack` would never produce and a restore still has
    // to survive.
    private static byte[] Zip((string Path, string Content)[] entries)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (path, content) in entries)
            {
                using var stream = zip.CreateEntry(path).Open();
                stream.Write(Utf8(content));
            }

        return buffer.ToArray();
    }

    // Every restored file and its bytes, as one comparable value.
    private static IReadOnlyList<(string, string)> Read(string root)
    {
        var imports = Path.Combine(root, Restore.ImportsDir);

        return
        [
            .. Directory.EnumerateFiles(imports, "*", SearchOption.AllDirectories)
                .Select(f => (Path.GetRelativePath(imports, f).Replace('\\', '/'), File.ReadAllText(f)))
                .OrderBy(f => f.Item1, StringComparer.Ordinal)
        ];
    }

    private static string Temp()
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-restore-" + Guid.NewGuid().ToString("n")[..12]);
        Directory.CreateDirectory(root);
        return root;
    }

    private static byte[] Utf8(string text) => new UTF8Encoding(false).GetBytes(text);

    private static string Quoted(string[] versions) =>
        string.Join(",", versions.Select(v => $"\"{v}\""));
}
