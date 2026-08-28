using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace kac.core;

// What a pack comes to. Named before the archive is built, as `ExportPlan` and `BundlePlan` are, so a
// test can ask what a package would hold without a filesystem.
//
// `Entries` are named relative to the package root and are the archive's whole contents, envelope
// included. `FileName` is the name a registry stores it under, which is the id and the version joined in
// the shape every NuGet client already expects to find.
//
// `Problems` is what stops the run, and a plan carrying one is never written.
public sealed record PackPlan(
    string Id,
    string Version,
    string Shortcode,
    string FileName,
    IReadOnlyList<BundleFile> Entries,
    IReadOnlyList<string> Problems);

// An export sealed into one versioned file, so a consumer takes a release rather than a clone.
//
// **The envelope is NuGet's and the payload is ours.** The archive is a zip, and the only NuGet-shaped
// thing inside it is the `.nuspec`, which carries the id and the version a registry files the package
// under and nothing else. Everything a consumer acts on — the shortcode, the content version, the types
// and the records — is in `corpus/manifest.json`, which the export already wrote. So a consumer fetches a
// URL and unzips it, with no NuGet client anywhere, and moving to another registry is a change to one
// file rather than a migration.
//
// `docs/cli/pack.md` says what a run writes and what it refuses.
public static class Packer
{
    // Where the export lands inside the package. One directory, so the envelope's own files sit beside
    // the payload rather than among it, and a consumer unpacking the archive strips one known prefix
    // instead of filtering by name.
    public const string PayloadDir = "corpus";

    // The spellings a registry will accept for an id and a version. Both rules are the registry's rather
    // than this tool's, so neither is stated in the schema. A corpus is held to them here, on the run
    // that builds the thing a registry has to file.
    private static readonly Regex IdShape = new(@"^\w+([_.-]\w+)*$", RegexOptions.Compiled);
    private static readonly Regex VersionShape =
        new(@"^\d+\.\d+\.\d+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$", RegexOptions.Compiled);

    // The timestamp every entry carries. Fixed, and at the earliest a zip can express, so two packs of
    // one export are byte-identical. A registry rejects a second upload of a published version, so the
    // question "is what I am about to push the thing I proved" has to be answerable by comparing bytes.
    private static readonly DateTimeOffset Epoch = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // What a pack comes to, given the export tree read off the disk. The corpus is never loaded: a
    // package holds what the export carried, and assembling it from the corpus would let the two
    // disagree about what was published.
    // `repository` is where the corpus's source lives, written into the envelope for a reader who has
    // the package and wants the records it came from. It is supplied rather than derived: the export
    // states where a record is *published*, which is a different address and often a different host.
    // Null leaves the element out.
    public static PackPlan Plan(IReadOnlyList<BundleFile> export, string? repository = null)
    {
        var problems = new List<string>();

        var manifest = JsonRead.Parse(Text(export, Exporter.ManifestFile));
        if (manifest is null)
            return Stop(problems,
                $"the export holds no readable {Exporter.ManifestFile}. Run the export first: kac export");

        // The shape the export declares, held against the shape this build knows how to read, for the
        // reason `Bundler.Plan` asks the same question: a package built from an envelope this tool has
        // never seen would publish a contract nobody agreed to.
        var declaredFormat = JsonRead.Int(manifest["formatVersion"]);
        if (declaredFormat != Exporter.FormatVersion)
            return Stop(problems,
                $"the export declares format version {declaredFormat?.ToString() ?? "none"} and this tool reads "
                + $"version {Exporter.FormatVersion}. Rebuild it: kac export");

        // The three facts a package cannot be built without, each refused by name. A default for any of
        // them would publish a corpus under something nobody chose.
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

        // Refused rather than left null, unlike the export itself. An export with no shortcode is still
        // readable by whoever built it; a package is read by a corpus that has to file it under a name
        // and resolve `eng:pol-VURM` against it, and there is no such name here.
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

    // The plan sealed into the bytes a registry is handed. Nothing here reads a clock or the disk, so
    // one plan always produces one archive.
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

    // Replace the package directory whole, then write the one file it holds. The export and the bundle
    // do the same, and here it is what stops a run leaving the previous version behind for a publish
    // step to pick up alongside the new one.
    public static string Write(string corpusRoot, PackPlan plan)
    {
        var root = Path.Combine(corpusRoot, Dist.Package.Replace('/', Path.DirectorySeparatorChar));
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        Directory.CreateDirectory(root);

        File.WriteAllBytes(Path.Combine(root, plan.FileName), Archive(plan));
        return $"{Dist.Package}/{plan.FileName}";
    }

    // The id and the version, and the two elements a registry refuses a package without. Nothing about
    // the corpus's content is restated here: `corpus/manifest.json` carries it, and a second copy would
    // be a second thing to keep in step.
    //
    // The description is the one place the envelope names the shortcode, because a description is
    // required and a person browsing a feed is owed the word they will cite the package by. What the
    // corpus says about itself opens it, where the corpus said anything: a person browsing a feed is
    // owed the corpus's own account before the tool's.
    //
    // `authors` is required by the format, so a corpus naming nobody is filed under its own id rather
    // than under whoever wrote the template it copied. A licence is not required, and a corpus that
    // chose none asserts none.
    //
    // `repository` is the one element beyond the four, and a registry may act on it: GitHub Packages
    // reads that URL to decide which repository a package belongs to, and a token scoped to a
    // repository refuses a package naming none. `docs/cli/pack.md` says when to pass it.
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

        if (about?.License is { Length: > 0 } licence)
            metadata.Add(new XElement(ns + "license", new XAttribute("type", "expression"), licence));

        if (repository is { Length: > 0 })
            metadata.Add(new XElement(ns + "repository",
                new XAttribute("type", "git"),
                new XAttribute("url", repository)));

        return Xml(new XDocument(new XElement(ns + "package", metadata)));
    }

    // What the corpus said about itself, read back off the export it published. Null where the export
    // predates the block, which leaves every element built the way it was before.
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

    // The OPC relationship naming the manifest inside the package. A zip is not a package until
    // something points at the part that describes it, and this is that pointer.
    //
    // The relationship id has to be unique within the document and to open on a letter. It is derived
    // from the part it points at rather than generated, because a fresh one on every run would be the
    // only thing keeping two packs of one export from matching.
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

    // A stable id for the one relationship, as sixteen hex characters behind an `R`. The hash is a
    // spelling of the part's name and never a claim about its content.
    private static string RelationshipId(string nuspecPath) =>
        "R" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nuspecPath)))[..16];

    // What each part of the package is, which OPC asks for by extension. Every extension the payload
    // carries is declared, so a type naming its flat file something new travels without this being
    // touched. A part with no extension takes an override of its own, because a default has nothing to
    // key on.
    //
    // OPC compares an extension without regard to case, so two files spelled `.json` and `.JSON` are one
    // extension there and would be two declarations of it here. Both are lower-cased on the way in, and
    // the package is invalid without that.
    private static string ContentTypes(IEnumerable<string> payload)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/content-types";
        const string octet = "application/octet";

        // Read once per path, because both loops below turn on whether it came back empty. A name
        // closing on a bare dot has an extension of `.` and no characters after it, so it is a part
        // with no extension and takes the override rather than a default nothing could key on.
        var paths = payload
            .Select(p => (Path: p, Extension: Path.GetExtension(p).TrimStart('.').ToLowerInvariant()))
            .ToList();

        // `rels` is declared below with the content type OPC gives it, so it never reaches the loop
        // whatever a type happens to name one of its files.
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

    // How every document here is written out. The settings are the point: a line ending stated rather
    // than taken from the platform, so a package built on Windows and one built on Linux are the same
    // bytes, and no byte-order mark, which a reader of a `.nuspec` does not expect.
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

    // A `StringWriter` that admits to being UTF-8. The base class reports UTF-16, and `XmlWriter` writes
    // whatever encoding it is told into the declaration, so without this every document here would open
    // by naming an encoding it is not in.
    private sealed class StringWriterWith(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding { get; } = encoding;
    }
}
