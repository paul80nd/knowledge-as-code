// The `mechanism --check` engine: resolve every file against the manifest and compare the synced
// layer against the reference. Read-only — it classifies and reports, and never writes.
public static class MechanismCheck
{
    public static int Run(string localRoot, string refRoot, Manifest manifest, MechanismLock lockFile)
    {
        var accepted = lockFile.Accepted.Select(a => a.Path).ToHashSet(StringComparer.Ordinal);
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

        foreach (var rel in localFiles.Union(refFiles).OrderBy(r => r, StringComparer.Ordinal))
        {
            var layer = manifest.Resolve(rel);
            if (layer is null) { unclassified.Add(rel); continue; }

            var inLocal = localFiles.Contains(rel);
            var inRef = refFiles.Contains(rel);

            switch (layer)
            {
                case "synced":
                    var identical = inLocal && inRef
                        && Files.ReadLf(Path.Combine(localRoot, rel)) == Files.ReadLf(Path.Combine(refRoot, rel));

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
                        if (Files.ReadLf(Path.Combine(localRoot, rel)) != Files.ReadLf(Path.Combine(refRoot, rel)))
                            forkedDiffer++;
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
            Console.WriteLine("RESOLVED — accepted divergences that are now identical again (delete them from mechanism.lock):");
            foreach (var p in resolvedDivergence) Console.WriteLine($"  {p}");
        }

        Console.WriteLine(
            $"synced: {syncedInStep} in step, {drift.Count} drifted; "
            + $"forked: {forkedShared} shared ({forkedDiffer} differ, informational); "
            + $"accepted divergences: {acceptedActive}.");

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

    // Every tracked (and not-ignored) file, relative and forward-slashed. The walk lets the check
    // run in a non-git tree (the test harness assembles one), skipping only .git.
    private static HashSet<string> ListFiles(string root) =>
        new(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}
