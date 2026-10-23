using System.IO.Compression;
using System.Text;

namespace kac.core;

/// <summary>
/// <c>Files</c> are relative to the corpus's folder under <c>.imports/</c>, payload prefix stripped. <c>Current</c>
/// says the folder already holds this version, and <c>Files</c> is then empty.
/// </summary>
public sealed record RestoreStep(
    string Corpus,
    string Shortcode,
    string Version,
    bool Current,
    IReadOnlyList<BundleFile> Files);

/// <summary>
/// A plan carrying a problem writes nothing: a half-restored corpus validates against a graph nobody declared.
/// </summary>
public sealed record RestorePlan(IReadOnlyList<RestoreStep> Steps, IReadOnlyList<string> Problems);

/// <summary>
/// The corpus is carried beside the version: a repointed entry keeps its shortcode, and a folder judged by version
/// alone would keep the wrong corpus's records.
/// </summary>
public sealed record Imported(string Corpus, string Version);

/// <summary>
/// <c>.imports/</c> is a restored artefact and is never committed. A lock its range still admits is taken without
/// asking the registry, so two restores of an unchanged descriptor write the same bytes. <c>docs/use/cli/restore.md</c>
/// says what a run writes and what it refuses.
/// </summary>
public static class Restore
{
    /// <summary>One folder per shortcode, the name a citation carries.</summary>
    public const string ImportsDir = ".imports";

    /// <summary>
    /// What a package may unpack to. A zip entry declares its uncompressed size and a hostile one is free to understate
    /// it, so both caps are counted against the bytes actually read. A corpus is prose, and the largest export any
    /// corpus here produces is under a megabyte, so these refuse nothing real.
    /// </summary>
    private const long MaxEntryBytes = 16L * 1024 * 1024;
    private const long MaxPackageBytes = 256L * 1024 * 1024;

    /// <summary><c>installed</c> is a function rather than a path, so the decision is testable from strings.</summary>
    public static RestorePlan Plan(
        IReadOnlyList<Consumed> declared, Registry registry, Func<string, Imported?> installed)
    {
        var problems = new List<string>();
        var steps = new List<RestoreStep>();

        if (Declarations(declared, problems) is not { } entries) return new RestorePlan([], problems);

        foreach (var entry in entries)
        {
            var resolved = Version(entry, registry);
            if (resolved.Failed)
            {
                problems.Add(resolved.Problem);
                continue;
            }

            var version = resolved.Value;

            // Both corpus and version are asked: the checks a fetch is held to are skipped here.
            if (installed(entry.Shortcode) == new Imported(entry.Corpus, version))
            {
                steps.Add(new RestoreStep(entry.Corpus, entry.Shortcode, version, true, []));
                continue;
            }

            var package = registry.Package(entry.Source, entry.Corpus, version);
            if (package.Failed)
            {
                problems.Add(package.Problem);
                continue;
            }

            var unpacked = Unpack(package.Value, entry, version);
            if (unpacked.Failed) problems.Add(unpacked.Problem);
            else steps.Add(new RestoreStep(entry.Corpus, entry.Shortcode, version, false, unpacked.Value));
        }

        return new RestorePlan(steps, problems);
    }

    /// <summary>Replaced whole, so a record withdrawn upstream leaves. A current folder is not rewritten.</summary>
    public static IReadOnlyList<string> Write(string corpusRoot, RestorePlan plan)
    {
        var written = new List<string>();

        foreach (var step in plan.Steps.Where(s => !s.Current))
        {
            var root = Folder(corpusRoot, step.Shortcode);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);

            // In the plan's order, which puts the manifest last. See `Unpack`.
            foreach (var file in step.Files)
            {
                var path = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
                Files.OpenFolderFor(path);
                File.WriteAllBytes(path, file.Content);
            }

            written.Add($"{ImportsDir}/{step.Shortcode}");
        }

        return written;
    }

    /// <summary>The unpacked export's own manifest answers, because it is what actually arrived.</summary>
    public static Imported? Installed(string corpusRoot, string shortcode)
    {
        var path = Path.Combine(Folder(corpusRoot, shortcode), Exporter.ManifestFile);
        if (!File.Exists(path)) return null;

        var manifest = JsonRead.Parse(File.ReadAllText(path));
        return JsonRead.Str(manifest?["corpus"]) is { } corpus
               && JsonRead.Str(manifest?["contentVersion"]) is { } version
            ? new Imported(corpus, version)
            : null;
    }

    private static string Folder(string corpusRoot, string shortcode) =>
        Path.Combine(corpusRoot, ImportsDir, shortcode);

    /// <summary>
    /// All are judged before any is fetched, so a refusal never leaves <c>.imports/</c> half-written.
    /// </summary>
    private static List<Declaration>? Declarations(IReadOnlyList<Consumed> declared, List<string> problems)
    {
        var entries = new List<Declaration>();

        for (var i = 0; i < declared.Count; i++)
        {
            var entry = declared[i];

            var at = entry.Corpus is { } named ? $"'{named}'" : $"entry {i + 1} of `consumes:`";

            if (entry.Corpus is not { } corpus)
            {
                problems.Add($"{at} names no corpus. That is the package id the producer publishes under.");
                continue;
            }

            if (entry.Shortcode is not { } shortcode)
            {
                problems.Add($"{at} declares no shortcode. It is what this corpus files the import under "
                             + "and what a citation into it carries, so a restore cannot pick one.");
                continue;
            }

            if (CorpusDescriptor.ShortcodeFault(shortcode) is { } fault)
            {
                problems.Add($"{at} declares shortcode '{shortcode}', which {fault}. It is the producer's "
                             + "own spelling, so copy it from their .corpus.yaml.");
                continue;
            }

            if (entry.Version is not { } range)
            {
                problems.Add($"{at} states no version. Write the range this corpus means, as `1.2.0` for "
                             + "one version or `^1.2.0` for the newest that cannot have changed a meaning.");
                continue;
            }

            if (!VersionRange.Legible(range))
            {
                problems.Add($"{at} states version '{range}', which is neither `1.2.0` nor `^1.2.0`. "
                             + "Those are the two forms a range may take.");
                continue;
            }

            if (entry.Source is not { } source)
            {
                problems.Add($"{at} names no source. It is where the package is fetched from: a "
                             + "registry's service index, or a folder holding what a producer built.");
                continue;
            }

            entries.Add(new Declaration(corpus, shortcode, range, entry.Resolved, source));
        }

        foreach (var clash in Repeated(entries, e => e.Shortcode))
            problems.Add($"'{clash.Key}' is claimed by {string.Join(" and ", clash.Select(e => e.Corpus))}. "
                         + $"A shortcode names one corpus, and both would restore into {ImportsDir}/"
                         + $"{clash.Key}/.");

        // An error rather than a waste: the lock is written onto the entry naming a corpus, so two entries
        // naming one corpus have one place to record two answers.
        foreach (var clash in Repeated(entries, e => e.Corpus))
            problems.Add($"'{clash.Key}' is consumed twice, as "
                         + $"{string.Join(" and ", clash.Select(e => $"'{e.Shortcode}'"))}. A corpus is "
                         + "consumed once, under the shortcode its producer declared.");

        return problems.Count == 0 ? entries : null;
    }

    private static IEnumerable<IGrouping<string, Declaration>> Repeated(
        List<Declaration> entries, Func<Declaration, string> by) =>
        entries.GroupBy(by, StringComparer.Ordinal).Where(g => g.Count() > 1);

    private static Registry.Answer<string> Version(Declaration entry, Registry registry)
    {
        if (entry.Resolved is { } locked && VersionRange.Admits(entry.Version, locked))
            return new Registry.Answer<string>(locked, null);

        var published = registry.Versions(entry.Source, entry.Corpus);
        if (published.Value is not { } versions) return new Registry.Answer<string>(null, published.Problem);

        if (VersionRange.Best(entry.Version, versions) is not { } best)
            return new Registry.Answer<string>(null,
                $"'{entry.Corpus}' has no version matching '{entry.Version}' at {entry.Source}. "
                + (versions.Count == 0
                    ? Registry.Absent(entry.Source)
                    : $"It holds {string.Join(", ", versions)}."));

        return new Registry.Answer<string>(best, null);
    }

    /// <summary>
    /// The envelope is dropped: everything under the payload prefix <c>kac pack</c> wrote is the corpus.
    /// </summary>
    private static Registry.Answer<IReadOnlyList<BundleFile>> Unpack(
        byte[] bytes, Declaration entry, string version)
    {
        var prefix = Packer.PayloadDir + "/";
        var files = new List<BundleFile>();
        var unpacked = 0L;

        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);

            foreach (var zipped in archive.Entries)
            {
                if (!zipped.FullName.StartsWith(prefix, StringComparison.Ordinal)) continue;
                if (zipped.FullName.EndsWith('/')) continue;

                var path = zipped.FullName[prefix.Length..];

                // Stops a zip entry addressing a path outside the folder it is unpacked into.
                if (!Contained(path))
                    return Refused(entry, $"holds an entry named '{zipped.FullName}', which addresses a "
                                          + $"path outside {ImportsDir}/{entry.Shortcode}/.");

                using var stream = zipped.Open();
                using var buffer = new MemoryStream();
                if (!CopyCapped(stream, buffer, MaxEntryBytes))
                    return Refused(entry, $"holds an entry named '{zipped.FullName}' that unpacks to more "
                                          + $"than {MaxEntryBytes / (1024 * 1024)}MB.");

                unpacked += buffer.Length;
                if (unpacked > MaxPackageBytes)
                    return Refused(entry, $"unpacks to more than {MaxPackageBytes / (1024 * 1024)}MB.");

                files.Add(new BundleFile(path, buffer.ToArray()));
            }
        }
        catch (InvalidDataException)
        {
            return Refused(entry, "is not a readable package.");
        }

        var manifest = JsonRead.Parse(Text(files, Exporter.ManifestFile));
        if (manifest is null)
            return Refused(entry, $"carries no readable {Packer.PayloadDir}/{Exporter.ManifestFile}, so "
                                  + "there is no export inside it.");

        // A package written to a contract this tool has never seen is refused rather than half-read.
        var format = JsonRead.Int(manifest["formatVersion"]);
        if (format != Exporter.FormatVersion)
            return Refused(entry, $"is at export format version {format?.ToString() ?? "none"} and this "
                                  + $"tool reads version {Exporter.FormatVersion}. Upgrade kac, or consume "
                                  + "a version of that corpus this one can read.");

        var stamped = JsonRead.Str(manifest["shortcode"]);
        if (stamped != entry.Shortcode)
            return Refused(entry, $"is cited as '{stamped ?? "nothing"}:' by its own manifest, and this "
                                  + $"corpus declares it as '{entry.Shortcode}:'. The producer owns the "
                                  + "spelling, so change the declaration to match it.");

        var named = JsonRead.Str(manifest["corpus"]);
        if (named != entry.Corpus)
            return Refused(entry, $"calls itself '{named ?? "nothing"}', and was fetched as "
                                  + $"'{entry.Corpus}'. Check `source:` names the feed that corpus "
                                  + "publishes to.");

        var carried = JsonRead.Str(manifest["contentVersion"]);
        if (carried != version)
            return Refused(entry, $"was fetched at {version} and says it is {carried ?? "no version"}. "
                                  + "The registry is serving something other than what it listed.");

        // The manifest goes last: `Installed` reads its presence as the folder's identity, so a write that
        // stopped halfway must not leave one behind.
        return new Registry.Answer<IReadOnlyList<BundleFile>>(
        [
            .. files
                .OrderBy(f => f.Path.Equals(Exporter.ManifestFile, StringComparison.Ordinal))
                .ThenBy(f => f.Path, StringComparer.Ordinal)
        ], null);
    }

    /// <summary>
    /// Read off the string rather than the disk, so the answer is the same where the folder does not exist.
    /// </summary>
    private static bool Contained(string path) =>
        path.Length > 0
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Split('/', '\\').Any(s => s is ".." or "");

    /// <summary>
    /// Copies until the cap is passed, and answers whether it stayed inside it. <c>CopyTo</c> cannot replace it,
    /// because it reads an entry whole before the cap is asked.
    /// </summary>
    private static bool CopyCapped(Stream from, Stream to, long cap)
    {
        var chunk = new byte[81920];
        var total = 0L;

        int read;
        while ((read = from.Read(chunk, 0, chunk.Length)) > 0)
        {
            total += read;
            if (total > cap) return false;
            to.Write(chunk, 0, read);
        }

        return true;
    }

    private static Registry.Answer<IReadOnlyList<BundleFile>> Refused(Declaration entry, string fault) =>
        new(null, $"the package for '{entry.Corpus}' {fault}");

    private static string? Text(List<BundleFile> files, string path) =>
        files.FirstOrDefault(f => f.Path == path) is { } file
            ? new UTF8Encoding(false).GetString(file.Content).TrimStart('﻿')
            : null;

    private sealed record Declaration(
        string Corpus,
        string Shortcode,
        string Version,
        string? Resolved,
        string Source);
}
