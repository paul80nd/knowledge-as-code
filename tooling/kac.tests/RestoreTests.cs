using System.IO.Compression;
using System.Text;
using kac.core;

namespace kac.tests;

public class RestoreTests
{
    private const string Source = "https://feed.example/index.json";
    private const string Shelf = "../engineering/.dist/package";

    [Fact]
    public void A_declaration_with_no_lock_takes_the_highest_version_its_range_admits()
    {
        var step = Assert.Single(Plan([Declared(range: "^0.1.0")], Feed(["0.1.0", "0.1.4", "0.2.0"])).Steps);

        Assert.Equal("0.1.4", step.Version);
        Assert.False(step.Current);
    }

    [Fact]
    public void A_lock_the_range_still_admits_is_taken_without_asking_the_registry()
    {
        var asked = new List<string>();
        var feed = Watched(Feed(["0.1.0", "0.9.0"]), asked);

        Assert.Equal("0.1.0", Assert.Single(Plan([Declared(range: "^0.1.0", resolved: "0.1.0")], feed).Steps)
            .Version);
        Assert.DoesNotContain(asked, url => url.EndsWith("example-engineering/index.json", StringComparison.Ordinal));
    }

    [Fact]
    public void A_lock_the_range_no_longer_admits_is_resolved_again()
        => Assert.Equal("0.2.0",
            Assert.Single(Plan([Declared(range: "^0.2.0", resolved: "0.1.0")], Feed(["0.1.0", "0.2.0"])).Steps)
                .Version);

    [Fact]
    public void A_corpus_already_unpacked_at_the_resolved_version_is_left_alone()
    {
        var step = Assert.Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.1.0")).Steps);

        Assert.True(step.Current);
        Assert.Empty(step.Files);
    }

    [Fact]
    public void A_folder_holding_another_corpus_at_the_right_version_is_fetched_again()
        => Assert.False(Assert
            .Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.1.0", "example-security")).Steps)
            .Current);

    [Fact]
    public void A_corpus_unpacked_at_another_version_is_fetched_again()
        => Assert.False(Assert.Single(Plan([Declared(range: "0.1.0")], Feed(["0.1.0"]), Holding("0.0.9")).Steps)
            .Current);

    [Fact]
    public void A_shortcode_two_declarations_both_claim_is_refused_naming_both()
    {
        var problem = Assert.Single(Plan(
            [Declared(), Declared(corpus: "another-corpus")],
            Feed(["0.1.0"])).Problems);

        Assert.Contains("example-engineering", problem);
        Assert.Contains("another-corpus", problem);
    }

    [Fact]
    public void A_corpus_two_declarations_both_consume_is_refused_naming_both_shortcodes()
    {
        var problem = Assert.Single(Plan(
            [Declared(), Declared(shortcode: "gov")],
            Feed(["0.1.0"])).Problems);

        Assert.Contains("consumed twice, as 'eng' and 'gov'", problem);
    }

    [Fact]
    public void A_package_whose_stamped_shortcode_disagrees_with_the_declaration_is_refused()
    {
        var problem = Assert.Single(
            Plan([Declared(shortcode: "gov")], Feed(["0.1.0"])).Problems);

        Assert.Contains("cited as 'eng:'", problem);
        Assert.Contains("declares it as 'gov:'", problem);
    }

    [Fact]
    public void A_package_calling_itself_another_corpus_is_refused()
        => Assert.Contains("calls itself 'example-engineering'",
            Assert.Single(Plan([Declared(corpus: "example-payments")],
                Feed(["0.1.0"], id: "example-payments", stamped: "example-engineering")).Problems));

    // Zipped by hand rather than sealed, because `pack` refuses to seal an export at a format version it
    // does not itself write.
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

    [Theory]
    [InlineData("corpus/../../escaped.json")]
    [InlineData("corpus//absolute.json")]
    public void An_entry_addressing_a_path_outside_the_import_folder_is_refused(string entry)
        => Assert.Contains("addresses a path outside",
            Assert.Single(Plan([Declared()], Serving(Zip([(entry, "{}")]))).Problems));

    [Fact]
    public void An_entry_that_unpacks_past_the_cap_is_refused()
        => Assert.Contains("unpacks to more than",
            Assert.Single(Plan([Declared()],
                    Serving(Zip([($"{Packer.PayloadDir}/big.jsonl", new string('a', 17 * 1024 * 1024))])))
                .Problems));

    [Fact]
    public void A_package_carrying_no_export_manifest_is_refused()
        => Assert.Contains("no readable corpus/manifest.json",
            Assert.Single(Plan([Declared()], Serving(Zip([("corpus/glossary/terms.jsonl", "{}")]))).Problems));

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

    [Fact]
    public void A_declaration_whose_shortcode_is_not_one_is_refused()
        => Assert.Contains("is too long",
            Assert.Single(Plan([Declared(shortcode: "../../etc")], Feed(["0.1.0"])).Problems));

    [Fact]
    public void A_version_that_is_not_a_range_is_refused()
        => Assert.Contains("neither `1.2.0` nor `^1.2.0`",
            Assert.Single(Plan([Declared(range: ">=1.0.0")], Feed(["0.1.0"])).Problems));

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

    [Fact]
    public void Nothing_is_planned_where_any_declaration_is_refused()
        => Assert.Empty(Plan([Declared(), Declared(corpus: "example-payments", shortcode: "pay", range: "nope")],
            Feed(["0.1.0"])).Steps);

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

    [Fact]
    public void A_restore_replaces_the_import_folder_whole()
    {
        var root = Temp();
        var stale = Path.Combine(root, Restore.ImportsDir, "eng", "glossary", "withdrawn.json");
        Files.OpenFolderFor(stale);
        File.WriteAllText(stale, "{}");

        Restore.Write(root, Plan([Declared()], Feed(["0.1.0"], ["glossary/terms.jsonl"])));

        Assert.False(File.Exists(stale));
        Assert.True(File.Exists(Path.Combine(root, Restore.ImportsDir, "eng", "glossary", "terms.jsonl")));
    }

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

    [Fact]
    public void What_a_folder_holds_is_read_from_the_manifest_that_arrived()
    {
        var root = Temp();
        Restore.Write(root, Plan([Declared()], Feed(["0.1.0"])));

        Assert.Equal(new Imported("example-engineering", "0.1.0"), Restore.Installed(root, "eng"));
        Assert.Null(Restore.Installed(root, "pay"));
    }

    [Fact]
    public void The_manifest_is_the_last_file_written()
    {
        var step = Assert.Single(Plan([Declared()],
            Feed(["0.1.0"], ["glossary/terms.jsonl", "policies/clauses.jsonl"])).Steps);

        Assert.Equal(Exporter.ManifestFile, step.Files[^1].Path);
    }

    [Fact]
    public void A_folder_source_resolves_against_the_packages_the_folder_holds()
    {
        var asked = new List<string>();

        var step = Assert.Single(Plan(
            [Declared(range: "^0.1.0", source: Shelf)],
            Watched(Feed(["0.9.0"]), asked),
            folder: Packages(["0.1.0", "0.1.4", "0.2.0"])).Steps);

        Assert.Equal("0.1.4", step.Version);
        Assert.Empty(asked);
    }

    [Fact]
    public void A_package_file_named_in_another_case_is_still_found()
        => Assert.Equal("0.1.0", Assert.Single(Plan(
            [Declared(range: "0.1.0", source: Shelf)],
            Feed([]),
            folder: Packages(["0.1.0"], named: v => $"Example-Engineering.{v}.NUPKG")).Steps).Version);

    [Fact]
    public void A_longer_package_id_in_the_same_folder_is_not_read_as_a_version()
    {
        var problem = Assert.Single(Plan(
            [Declared(source: Shelf)],
            Feed([]),
            folder: Packages(["0.1.0"], named: _ => "example-engineering.extras.0.1.0.nupkg")).Problems);

        Assert.Contains("kac pack", problem, StringComparison.Ordinal);
        Assert.DoesNotContain("extras", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void A_source_naming_no_folder_is_refused_naming_both_forms_a_source_takes()
    {
        var problem = Assert.Single(Plan(
            [Declared(source: "../nowhere")], Feed([]), folder: Packages(["0.1.0"])).Problems);

        Assert.Contains("there is no folder at ../nowhere", problem, StringComparison.Ordinal);
        Assert.Contains("service index", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void A_folder_holding_no_package_names_the_command_that_fills_it()
    {
        var problem = Assert.Single(Plan(
            [Declared(source: Shelf)],
            Feed([]),
            folder: Packages(["0.1.0"], id: "example-security")).Problems);

        Assert.Contains("kac pack", problem, StringComparison.Ordinal);
        Assert.DoesNotContain(Registry.TokenVariable, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void A_package_a_folder_served_is_held_to_the_shortcode_it_stamped()
    {
        var problem = Assert.Single(Plan(
            [Declared(source: Shelf)],
            Feed([]),
            folder: Packages(["0.1.0"], shortcode: "gov")).Problems);

        Assert.Contains("cited as 'gov:' by its own manifest", problem, StringComparison.Ordinal);
    }

    private static RestorePlan Plan(
        IReadOnlyList<Consumed> declared, Func<string, Fetched> feed,
        Func<string, Imported?>? installed = null, FolderFeed? folder = null) =>
        Restore.Plan(declared, new Registry(feed, folder ?? Empty), installed ?? (_ => null));

    private static readonly FolderFeed Empty = new(_ => null, _ => null);

    private static Func<string, Imported?> Holding(string version, string corpus = "example-engineering") =>
        _ => new Imported(corpus, version);

    private static Consumed Declared(
        string corpus = "example-engineering",
        string shortcode = "eng",
        string range = "0.1.0",
        string? resolved = null,
        string source = Source) =>
        new(corpus, shortcode, range, resolved, source);

    // A folder holding one package per version, under the names `kac pack` writes. Anything but `Shelf`
    // is a path with no folder on it, which is the case a source pointing at nothing takes.
    private static FolderFeed Packages(
        string[] versions,
        string id = "example-engineering",
        string shortcode = "eng",
        Func<string, string>? named = null)
    {
        var held = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        foreach (var version in versions)
            held[(named ?? (v => $"{id}.{v}.nupkg"))(version)] = Package(id, shortcode, version, []);

        return new FolderFeed(
            source => source == Shelf ? [.. held.Keys] : null,
            path => held.TryGetValue(Path.GetFileName(path), out var body) ? body : null);
    }

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
