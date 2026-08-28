using System.IO.Compression;
using System.Text;

namespace kac.core;

// What restoring one consumed corpus comes to. Named before anything is written, as `ExportPlan` and
// `PackPlan` are, so a test can ask what a restore would do without a filesystem or a network.
//
// `Files` are named relative to that corpus's own folder under `.imports/`, with the package's payload
// prefix already stripped. `Current` says the folder already holds this version, in which case `Files`
// is empty and nothing was fetched.
public sealed record RestoreStep(
    string Corpus,
    string Shortcode,
    string Version,
    bool Current,
    IReadOnlyList<BundleFile> Files);

// What a whole restore comes to. `Problems` is what stops the run, and a plan carrying one writes
// nothing at all: a corpus half-restored validates against a graph nobody declared.
public sealed record RestorePlan(IReadOnlyList<RestoreStep> Steps, IReadOnlyList<string> Problems);

// What a folder under `.imports/` already holds, read from the export manifest that arrived in it.
//
// The corpus is carried beside the version because a shortcode alone does not say whose records these
// are. An entry repointed at another corpus keeps its shortcode, and a folder judged only by version
// would be left standing with the wrong corpus's records inside it.
public sealed record Imported(string Corpus, string Version);

// The consuming half of publishing. A corpus declares what it consumes, and this fetches each
// dependency at a version the declaration admits and unpacks it where the resolver will look.
//
// **`.imports/` is a restored artefact and is never committed.** It holds another corpus's whole
// content, and a copy in this repository's history would be a second place that content lives and a
// second thing to keep in step. CI restores before it validates, exactly as a build restores packages.
//
// **The range is intent and the lock is what the build used.** A declaration carrying a `resolved:` that
// its range still admits is taken at that version and the registry is never asked, so two restores of an
// unchanged `.corpus.yaml` write the same bytes. A range that has moved past its lock re-resolves, which
// is how changing the declaration takes effect. Whether a newer version has been published is a
// different question, and not one a restore answers.
//
// `docs/cli/restore.md` says what a run writes and what it refuses.
public static class Restore
{
    // Where a restored corpus lands. One folder per shortcode, because a shortcode is what a citation
    // carries and a reader following `eng:pol-VURM` should find it under the name they typed.
    public const string ImportsDir = ".imports";

    // What a restore comes to, given what the descriptor declared.
    //
    // `installed` answers what version each shortcode's folder already holds, or null where it holds
    // nothing readable. It is a function rather than a path so that the whole decision is testable from
    // a set of strings, which is the bargain `Update.Plan` and `New.Plan` already strike.
    public static RestorePlan Plan(
        IReadOnlyList<Consumed> declared, Registry registry, Func<string, Imported?> installed)
    {
        var problems = new List<string>();
        var steps = new List<RestoreStep>();

        if (Declarations(declared, problems) is not { } entries) return new RestorePlan([], problems);

        foreach (var entry in entries)
        {
            var resolved = Version(entry, registry);
            if (resolved.Problem is { } problem)
            {
                problems.Add(problem);
                continue;
            }

            var version = resolved.Value!;

            // Already unpacked, by the same corpus and at the version this resolved to, so nothing is
            // fetched. Both halves are asked, because the checks a fetch is held to are skipped here and
            // a shortcode says nothing about whose records are under it.
            if (installed(entry.Shortcode) == new Imported(entry.Corpus, version))
            {
                steps.Add(new RestoreStep(entry.Corpus, entry.Shortcode, version, true, []));
                continue;
            }

            var package = registry.Package(entry.Source, entry.Corpus, version);
            if (package.Value is not { } bytes)
            {
                problems.Add(package.Problem!);
                continue;
            }

            var unpacked = Unpack(bytes, entry, version);
            if (unpacked.Problem is { } refused) problems.Add(refused);
            else steps.Add(new RestoreStep(entry.Corpus, entry.Shortcode, version, false, unpacked.Value!));
        }

        return new RestorePlan(steps, problems);
    }

    // Replace each restored corpus's folder whole, as the export and the bundle replace theirs. A record
    // withdrawn upstream has to leave, and a folder written over in place would keep it.
    //
    // A step that found the folder current is not rewritten. It already holds these bytes, and deleting
    // and restoring them would make a no-op run look like work in every file watcher watching.
    public static IReadOnlyList<string> Write(string corpusRoot, RestorePlan plan)
    {
        var written = new List<string>();

        foreach (var step in plan.Steps.Where(s => !s.Current))
        {
            var root = Folder(corpusRoot, step.Shortcode);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);

            // Written in the order the plan named them, which puts the manifest last.
            foreach (var file in step.Files)
            {
                var path = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, file.Content);
            }

            written.Add($"{ImportsDir}/{step.Shortcode}");
        }

        return written;
    }

    // What a shortcode's folder already holds, or null where it holds nothing this tool wrote. The
    // unpacked export's own manifest is what answers, because it is what actually arrived rather than
    // what something recorded having asked for.
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

    // Every declaration read and held to what a restore needs of it, or null where one of them was not.
    //
    // All of them are judged before any of them is fetched. A run that fetched two corpora and then
    // refused the third would leave `.imports/` describing a graph the descriptor does not.
    private static List<Declaration>? Declarations(IReadOnlyList<Consumed> declared, List<string> problems)
    {
        var entries = new List<Declaration>();

        for (var i = 0; i < declared.Count; i++)
        {
            var entry = declared[i];

            // Named by its corpus where it has one, because that is what a reader scans the block for.
            // An entry that has not even said which corpus it consumes is named by its position, which
            // is all there is to name it by.
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

        // One folder per shortcode, so two declarations claiming one would each be restoring over the
        // other and whichever ran last would win in silence. Both are named, because the fix is to
        // change one of them and only their owner knows which.
        foreach (var clash in Repeated(entries, e => e.Shortcode))
            problems.Add($"'{clash.Key}' is claimed by {string.Join(" and ", clash.Select(e => e.Corpus))}. "
                         + $"A shortcode names one corpus, and both would restore into {ImportsDir}/"
                         + $"{clash.Key}/.");

        // The other direction, and the lock is what makes it an error rather than a waste. A version is
        // written back onto the entry naming a corpus, so two entries naming one corpus have two ranges
        // and one place to record what either came to.
        foreach (var clash in Repeated(entries, e => e.Corpus))
            problems.Add($"'{clash.Key}' is consumed twice, as "
                         + $"{string.Join(" and ", clash.Select(e => $"'{e.Shortcode}'"))}. A corpus is "
                         + "consumed once, under the shortcode its producer declared.");

        return problems.Count == 0 ? entries : null;
    }

    // The declarations sharing a value that has to be theirs alone, grouped by it. Empty where every
    // entry differs, which is every descriptor anybody meant to write.
    private static IEnumerable<IGrouping<string, Declaration>> Repeated(
        List<Declaration> entries, Func<Declaration, string> by) =>
        entries.GroupBy(by, StringComparer.Ordinal).Where(g => g.Count() > 1);

    // Which version this declaration resolves to, and the registry asked only where the answer is not
    // already written down.
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

    // The package opened, held against what the declaration said it would be, and reduced to the payload
    // a consumer reads.
    //
    // The envelope is dropped here. `[Content_Types].xml`, the relationships and the `.nuspec` are a
    // registry's business, and the corpus this restores is everything under the payload prefix `kac pack`
    // wrote it beneath.
    private static Registry.Answer<IReadOnlyList<BundleFile>> Unpack(
        byte[] bytes, Declaration entry, string version)
    {
        var prefix = Packer.PayloadDir + "/";
        var files = new List<BundleFile>();

        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);

            foreach (var zipped in archive.Entries)
            {
                if (!zipped.FullName.StartsWith(prefix, StringComparison.Ordinal)) continue;
                if (zipped.FullName.EndsWith('/')) continue;

                var path = zipped.FullName[prefix.Length..];

                // A package is somebody else's file, and a zip may name an entry anywhere it likes. This
                // is what stops one addressing a path outside the folder it is being unpacked into.
                if (!Contained(path))
                    return Refused(entry, $"holds an entry named '{zipped.FullName}', which addresses a "
                                          + $"path outside {ImportsDir}/{entry.Shortcode}/.");

                using var stream = zipped.Open();
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
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

        // The shape the export declares, held against the shape this build reads, as `pack` and `bundle`
        // both ask it. A package written to a contract this tool has never seen is refused rather than
        // half-read.
        var format = JsonRead.Int(manifest["formatVersion"]);
        if (format != Exporter.FormatVersion)
            return Refused(entry, $"is at export format version {format?.ToString() ?? "none"} and this "
                                  + $"tool reads version {Exporter.FormatVersion}. Upgrade kac, or consume "
                                  + "a version of that corpus this one can read.");

        // The declaration's own claim about what it is importing, held against what the producer stamped.
        // The consumer's spelling is what every citation in this corpus is written against, so one that
        // disagrees would file a corpus under a name that resolves nothing.
        var stamped = JsonRead.Str(manifest["shortcode"]);
        if (stamped != entry.Shortcode)
            return Refused(entry, $"is cited as '{stamped ?? "nothing"}:' by its own manifest, and this "
                                  + $"corpus declares it as '{entry.Shortcode}:'. The producer owns the "
                                  + "spelling, so change the declaration to match it.");

        // The same question about the package's other half of its identity. A registry serving the wrong
        // file under an id is unlikely; a `source:` pointing at the wrong feed is not.
        var named = JsonRead.Str(manifest["corpus"]);
        if (named != entry.Corpus)
            return Refused(entry, $"calls itself '{named ?? "nothing"}', and was fetched as "
                                  + $"'{entry.Corpus}'. Check `source:` names the feed that corpus "
                                  + "publishes to.");

        var carried = JsonRead.Str(manifest["contentVersion"]);
        if (carried != version)
            return Refused(entry, $"was fetched at {version} and says it is {carried ?? "no version"}. "
                                  + "The registry is serving something other than what it listed.");

        // Ordered so the manifest is last, because its presence is what `Installed` reads the folder's
        // identity from. A write that stopped halfway would otherwise leave a folder answering for
        // records it does not hold, and every later run would call it current and never repair it. The
        // order is the plan's rather than the writer's, so what a run does is decided in one place.
        return new Registry.Answer<IReadOnlyList<BundleFile>>(
        [
            .. files
                .OrderBy(f => f.Path.Equals(Exporter.ManifestFile, StringComparison.Ordinal))
                .ThenBy(f => f.Path, StringComparer.Ordinal)
        ], null);
    }

    // Whether a path stays inside the folder it is unpacked into: no root, no drive, and no segment
    // walking back up. Read off the string rather than resolved against the disk, because the answer has
    // to be the same on a machine where the folder does not exist yet.
    private static bool Contained(string path) =>
        path.Length > 0
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Split('/', '\\').Any(s => s is ".." or "");

    private static Registry.Answer<IReadOnlyList<BundleFile>> Refused(Declaration entry, string fault) =>
        new(null, $"the package for '{entry.Corpus}' {fault}");

    private static string? Text(List<BundleFile> files, string path) =>
        files.FirstOrDefault(f => f.Path == path) is { } file
            ? new UTF8Encoding(false).GetString(file.Content).TrimStart('﻿')
            : null;

    // One entry of `consumes:` with everything a restore needs of it present. `Consumed` is what the
    // file said and every field of it is nullable; this is what survived being read, so nothing below
    // re-asks whether a corpus stated its source.
    private sealed record Declaration(
        string Corpus,
        string Shortcode,
        string Version,
        string? Resolved,
        string Source);
}
