using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace kac.core;

/// <summary>
/// What a pack comes to, named before the archive is built, as <c>ExportPlan</c> and <c>BundlePlan</c> are.
/// <c>Entries</c> are the archive's whole contents, named relative to the package root. A plan carrying <c>Problems</c>
/// is never written.
/// </summary>
public sealed record PackPlan(
    string Id,
    string Version,
    string Shortcode,
    string FileName,
    IReadOnlyList<BundleFile> Entries,
    IReadOnlyList<string> Problems);

/// <summary>
/// An export sealed into one versioned file. The envelope is NuGet's and the payload is ours: the <c>.nuspec</c>
/// carries only the id and the version, and everything a consumer acts on is in <c>corpus/manifest.json</c>.
/// <c>docs/use/cli/pack.md</c> says what a run writes and what it refuses.
/// </summary>
public static class Packer
{
    /// <summary>Where the export lands inside the package, so a consumer strips one known prefix.</summary>
    public const string PayloadDir = "corpus";

    private static readonly Regex IdShape = new(@"^\w+([_.-]\w+)*$", RegexOptions.Compiled);

    private static readonly Regex VersionShape =
        new(@"^\d+\.\d+\.\d+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$", RegexOptions.Compiled);

    private static readonly DateTimeOffset Epoch = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// What a pack comes to, given the export tree read off the disk. The corpus is never loaded, so the package and
    /// the export cannot disagree. <c>repository</c> is where the corpus's source lives, supplied by the caller, and
    /// null leaves the element out.
    /// </summary>
    public static PackPlan Plan(IReadOnlyList<BundleFile> export, string? repository = null)
    {
        var problems = new List<string>();

        var manifest = JsonRead.Parse(Text(export, Exporter.ManifestFile));
        if (manifest is null)
            return Stop(problems,
                $"the export holds no readable {Exporter.ManifestFile}. Run the export first: kac export");

        // Held against the shape this build can read, for the reason `Bundler.Plan` asks the same question.
        var declaredFormat = JsonRead.Int(manifest["formatVersion"]);
        if (declaredFormat != Exporter.FormatVersion)
            return Stop(problems,
                $"the export declares format version {declaredFormat?.ToString() ?? "none"} and this tool reads "
                + $"version {Exporter.FormatVersion}. Rebuild it: kac export");

        // The three facts a package cannot be built without, each refused by name.
        var id = JsonRead.Str(manifest["corpus"]);
        if (id is null)
            return Stop(problems,
                "the export names no corpus, and that name is what the package is published under. "
                + "Write `corpus:` in .corpus.yaml and export again.");

        if (!IdShape.IsMatch(id) || id.Length > 100)
            return Stop(problems,
                $"'{id}' cannot be a package id. A registry takes letters, digits and underscores, joined "
                + "by a dot, a dash or an underscore, up to 100 characters. Rename the corpus in "
                + ".corpus.yaml and export again.");

        var version = JsonRead.Str(manifest["contentVersion"]);
        if (version is null)
            return Stop(problems,
                "the export states no content version, and that is what the package is versioned by. "
                + "Write `content-version:` in .corpus.yaml and export again.");

        if (!VersionShape.IsMatch(version))
            return Stop(problems,
                $"'{version}' is not a version a registry can order. Write `content-version:` in "
                + ".corpus.yaml as major.minor.patch, optionally with a prerelease suffix, and export again.");

        // Refused, unlike in the export, because a consumer has to file the package under a name and resolve
        // `eng:pol-VURM` against it.
        var shortcode = JsonRead.Str(manifest["shortcode"]);
        if (shortcode is null)
            return Stop(problems,
                "the export declares no shortcode, and a consumer cites what it imports by one. "
                + "Write `shortcode:` in .corpus.yaml and export again.");

        var fileName = $"{id}.{version}.nupkg";
        var payload = export
            .Select(f => f with { Path = $"{PayloadDir}/{f.Path}" })
            .OrderBy(f => f.Path, StringComparer.Ordinal)
            .ToList();

        var nuspecPath = $"{id}.nuspec";
        List<BundleFile> entries =
        [
            Utf8("[Content_Types].xml", ContentTypes(payload.Select(f => f.Path))),
            Utf8("_rels/.rels", Rels(nuspecPath)),
            Utf8(nuspecPath, Nuspec(id, version, shortcode, repository, About(manifest))),
            .. payload
        ];

        return new PackPlan(id, version, shortcode, fileName, entries, problems);
    }

    /// <summary>
    /// The plan sealed into bytes. Nothing here reads a clock or the disk, so one plan always produces one archive.
    /// </summary>
    public static byte[] Archive(PackPlan plan)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var file in plan.Entries)
            {
                var entry = zip.CreateEntry(file.Path, CompressionLevel.Optimal);
                entry.LastWriteTime = Epoch;
                using var stream = entry.Open();
                stream.Write(file.Content);
            }

        return buffer.ToArray();
    }

    /// <summary>
    /// Replace the package directory whole, so a publish step never picks up the previous version beside the new one.
    /// </summary>
    public static string Write(string corpusRoot, PackPlan plan)
    {
        var root = Path.Combine(corpusRoot, Dist.Package.Replace('/', Path.DirectorySeparatorChar));
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        Directory.CreateDirectory(root);

        File.WriteAllBytes(Path.Combine(root, plan.FileName), Archive(plan));
        return $"{Dist.Package}/{plan.FileName}";
    }

    /// <summary>
    /// The id, the version, and the elements a registry refuses a package without; <c>corpus/manifest.json</c> carries
    /// the content. <c>authors</c> is required, so a corpus naming nobody is filed under its own id.
    /// </summary>
    private static string Nuspec(
        string id, string version, string shortcode, string? repository, ExportAbout? about)
    {
        XNamespace ns = "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd";

        var cited = $"Cited as '{shortcode}:'.";
        var description = about?.Description is { Length: > 0 } said
            ? $"{said} {cited}"
            : $"The {id} knowledge corpus, exported as data. {cited}";

        var metadata = new XElement(ns + "metadata",
            new XElement(ns + "id", id),
            new XElement(ns + "version", version),
            new XElement(ns + "authors", about?.Author?.Name is { Length: > 0 } who ? who : id),
            new XElement(ns + "description", description));

        // A licence is not required, and a corpus that chose none asserts none.
        if (about?.License is { Length: > 0 } licence)
            metadata.Add(new XElement(ns + "license", new XAttribute("type", "expression"), licence));

        // GitHub Packages reads this URL to decide which repository owns the package. `docs/use/cli/pack.md` says when.
        if (repository is { Length: > 0 })
            metadata.Add(new XElement(ns + "repository",
                new XAttribute("type", "git"),
                new XAttribute("url", repository)));

        return Xml(new XDocument(new XElement(ns + "package", metadata)));
    }

    /// <summary>
    /// What the corpus said about itself, read back off the export. Null where the export has no <c>about</c> block.
    /// </summary>
    private static ExportAbout? About(System.Text.Json.Nodes.JsonObject manifest)
    {
        if (JsonRead.Object(manifest["about"]) is not { } about) return null;

        var author = JsonRead.Object(about["author"]);

        return new ExportAbout(
            JsonRead.Str(about["displayName"]),
            JsonRead.Str(about["description"]),
            author is null ? null : new ExportAuthor(JsonRead.Str(author["name"]), JsonRead.Str(author["url"])),
            JsonRead.Str(about["license"]));
    }

    /// <summary>
    /// The OPC relationship naming the manifest. Its id must be unique in the document and open on a letter, and is
    /// derived from the part rather than generated, so two packs of one export match.
    /// </summary>
    private static string Rels(string nuspecPath)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";

        return Xml(new XDocument(
            new XElement(ns + "Relationships",
                new XElement(ns + "Relationship",
                    new XAttribute("Type", "http://schemas.microsoft.com/packaging/2010/07/manifest"),
                    new XAttribute("Target", "/" + nuspecPath),
                    new XAttribute("Id", RelationshipId(nuspecPath))))));
    }

    /// <summary>A stable id for the one relationship, as sixteen hex characters behind an <c>R</c>.</summary>
    private static string RelationshipId(string nuspecPath) =>
        "R" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nuspecPath)))[..16];

    private static string ContentTypes(IEnumerable<string> payload)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/content-types";
        const string octet = "application/octet";

        // A name closing on a bare dot has an extension of `.`, so it is a part with no extension and takes the
        // override.
        var paths = payload
            .Select(p => (Path: p, Extension: Path.GetExtension(p).TrimStart('.').ToLowerInvariant()))
            .ToList();

        // `rels` is declared below with the content type OPC gives it, so it never reaches this loop.
        var extensions = paths
            .Select(p => p.Extension)
            .Where(e => e.Length > 0)
            .Append("nuspec")
            .Where(e => !e.Equals("rels", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(e => e, StringComparer.Ordinal);

        var root = new XElement(ns + "Types",
            new XElement(ns + "Default",
                new XAttribute("Extension", "rels"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")));

        foreach (var extension in extensions)
            root.Add(new XElement(ns + "Default",
                new XAttribute("Extension", extension),
                new XAttribute("ContentType", octet)));

        foreach (var path in paths.Where(p => p.Extension.Length == 0))
            root.Add(new XElement(ns + "Override",
                new XAttribute("PartName", "/" + path.Path),
                new XAttribute("ContentType", octet)));

        return Xml(new XDocument(root));
    }

    /// <summary>
    /// A line ending stated, so a package built on Windows and one built on Linux are the same bytes, and no byte-order
    /// mark, which a <c>.nuspec</c> reader does not expect.
    /// </summary>
    private static string Xml(XDocument document)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            Encoding = new UTF8Encoding(false)
        };

        using var text = new StringWriterWith(Encoding.UTF8);
        using (var writer = XmlWriter.Create(text, settings)) document.Save(writer);
        return text + "\n";
    }

    private static PackPlan Stop(List<string> problems, string problem)
    {
        problems.Add(problem);
        return new PackPlan("", "", "", "", [], problems);
    }

    private static BundleFile Utf8(string path, string content) =>
        new(path, new UTF8Encoding(false).GetBytes(content));

    private static string? Text(IReadOnlyList<BundleFile> files, string path) =>
        files.FirstOrDefault(f => f.Path == path) is { } file
            ? new UTF8Encoding(false).GetString(file.Content).TrimStart('\uFEFF')
            : null;

    private sealed class StringWriterWith(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding { get; } = encoding;
    }
}
