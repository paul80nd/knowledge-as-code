// The `mechanism` engines. `--check` resolves every file against the manifest and compares the shared
// layers against a reference, read-only. `--sync` brings those layers down from one.
//
// Both read the same manifest and the same descriptor, and both ask one predicate what this corpus holds. So
// the check can never report a file missing that a sync would decline to bring.

namespace kac.core;

public static class MechanismCheck
{
    public static int Run(string localRoot, string refRoot, Manifest manifest, CorpusDescriptor descriptor)
    {
        var accepted = descriptor.Accepted.Select(a => a.Path).ToHashSet(StringComparer.Ordinal);
        var localFiles = ListFiles(localRoot);
        var refFiles = ListFiles(refRoot);

        var drift = new List<string>();           // synced, both present, content differs
        var missingLocally = new List<string>();  // synced, in the reference but not here
        var missingUpstream = new List<string>(); // synced, here but not in the reference
        var unclassified = new List<string>();    // matches no manifest rule
        var resolvedDivergence = new List<string>(); // accepted, but now identical again
        var syncedInStep = 0;
        var forkedShared = 0;
        var forkedDiffer = 0;
        var acceptedActive = 0;
        var declinedButHeld = 0;

        foreach (var rel in localFiles.Union(refFiles).OrderBy(r => r, StringComparer.Ordinal))
        {
            var layer = manifest.Resolve(rel);
            if (layer is null) { unclassified.Add(rel); continue; }

            var inLocal = localFiles.Contains(rel);
            var inRef = refFiles.Contains(rel);

            if (Declined(rel, layer, descriptor))
            {
                if (inLocal) declinedButHeld++;
                continue;
            }

            switch (layer)
            {
                case "synced":
                case "verification":
                    var identical = inLocal && inRef && Same(localRoot, refRoot, rel);

                    if (accepted.Contains(rel))
                    {
                        if (identical) resolvedDivergence.Add(rel);
                        else acceptedActive++;
                        break;
                    }

                    if (!inRef) missingUpstream.Add(rel);
                    else if (!inLocal) missingLocally.Add(rel);
                    else if (identical) syncedInStep++;
                    else drift.Add(rel);
                    break;

                case "forked":
                    if (inLocal && inRef)
                    {
                        forkedShared++;
                        if (!Same(localRoot, refRoot, rel)) forkedDiffer++;
                    }
                    break;

                // generated / local / ignored: each corpus owns these; nothing to compare.
            }
        }

        Console.WriteLine($"mechanism: comparing the synced layer against {refRoot}");
        var errors = Section("DRIFT — synced files differ from the reference", drift)
                     + Section("MISSING LOCALLY — synced files in the reference but not here", missingLocally)
                     + Section("MISSING UPSTREAM — synced files here but not in the reference", missingUpstream)
                     + Section("UNCLASSIFIED — files matching no manifest rule", unclassified);

        if (resolvedDivergence.Count > 0)
        {
            Console.WriteLine("RESOLVED — accepted divergences that are now identical again (delete them from .corpus.yaml):");
            foreach (var p in resolvedDivergence) Console.WriteLine($"  {p}");
        }

        Console.WriteLine(
            $"synced: {syncedInStep} in step, {drift.Count} drifted; "
            + $"forked: {forkedShared} shared ({forkedDiffer} differ, informational); "
            + $"accepted divergences: {acceptedActive}.");

        // Held but not asked for: schema files for types this corpus did not adopt, or a fixture tree in
        // a corpus whose role declines the verification layer. Neither is drift, because nothing was
        // compared. Say so anyway — no sync will refresh these files, and the alternative is leaving the
        // reader to find them stale later.
        if (declinedButHeld > 0)
            Console.WriteLine(
                $"declined: {declinedButHeld} file(s) held here that this corpus's descriptor does not ask for. "
                + "They are not synced or compared; delete them, or adopt what they belong to.");

        if (errors > 0)
        {
            Console.Error.WriteLine($"mechanism check failed — {errors} synced-layer problem(s) above.");
            return 1;
        }

        Console.WriteLine("mechanism: synced layer in step.");
        return 0;

        static int Section(string heading, List<string> paths)
        {
            if (paths.Count == 0) return 0;
            Console.Error.WriteLine($"{heading}:");
            foreach (var p in paths) Console.Error.WriteLine($"  {p}");
            return paths.Count;
        }
    }

    // Whether this corpus's descriptor declines the file, so that it is neither missing nor drifted — and, to
    // `mechanism --sync`, not something to bring down. Each answer below is the corpus stating what it
    // took, so check and sync ask this one predicate rather than two that can disagree.
    //
    // A type the corpus did not adopt takes its schema file with it. The schema is otherwise
    // byte-identical, so this is the only place a corpus may hold less of it than upstream does. `types:`
    // turns that absence from a deletion nobody recorded into a decision the corpus can be held to. A
    // corpus that declares no types declines nothing: its folders still describe it, and every schema
    // file it has is one it is expected to have.
    //
    // A corpus whose role is `consumer` declines the verification layer the same way.
    public static bool Declined(string rel, string layer, CorpusDescriptor descriptor) =>
        (layer == "verification" && !descriptor.Verifies)
        || (TypeFile(rel) is { } type && !descriptor.Adopted(type));

    // The type a schema file declares, or null where the path is not one. `.schema/` holds a file per type
    // beside the underscore-prefixed files that belong to no type, and the type's name is the file's —
    // which is the same identity `ref:` and `versus:` use, and the same one `types:` names.
    private static string? TypeFile(string rel)
    {
        if (!rel.StartsWith(".schema/", StringComparison.Ordinal)) return null;
        if (!rel.EndsWith(".yaml", StringComparison.Ordinal)) return null;

        var name = rel[".schema/".Length..^".yaml".Length];
        return name.Length > 0 && name[0] != '_' && !name.Contains('/') ? name : null;
    }

    // Whether two copies of a file say the same thing. LF-normalised, so a working copy checked out with
    // CRLF never reads as drift, and compared on the authored half alone — see Generator.Authored for why
    // a shared page may hold a different table in each corpus and still be in step.
    internal static bool Same(string localRoot, string refRoot, string rel) =>
        Generator.Authored(Files.ReadLf(Path.Combine(localRoot, rel)))
        == Generator.Authored(Files.ReadLf(Path.Combine(refRoot, rel)));

    // Every tracked (and not-ignored) file, relative and forward-slashed. The walk lets the check
    // run in a non-git tree (the test harness assembles one), skipping only .git.
    internal static HashSet<string> ListFiles(string root) =>
        new(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}
