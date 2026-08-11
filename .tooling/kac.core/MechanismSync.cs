// The `mechanism --sync` engine: bring the shared layers down from a reference corpus, then record
// what was taken. The write half of the pair whose read half is MechanismCheck.
//
// Sync copies and never deletes. A file this corpus holds that the reference does not is reported and
// left alone: removing it is a decision about the corpus, and a corpus is not something a tool empties
// because an upstream tree happened to be smaller.

namespace kac.core;

public static class MechanismSync
{
    // `manifest` is the *reference's*, because the boundary comes down with the files it describes: a
    // corpus on an older manifest would otherwise resolve the new upstream tree by the old rules and
    // silently skip whatever the rules had not yet heard of.
    public static int Run(string localRoot, string refRoot, Manifest manifest, MechanismLock lockFile,
        string reference, string today)
    {
        var accepted = lockFile.Accepted.ToDictionary(a => a.Path, a => a.Reason, StringComparer.Ordinal);
        var localFiles = MechanismCheck.ListFiles(localRoot);
        var refFiles = MechanismCheck.ListFiles(refRoot);
        var declinedTypes = DeclinedTypePaths(refRoot, lockFile);

        var updated = new List<string>();      // shared, copied down because the authored halves differed
        var seeded = new List<string>();       // forked, copied because this corpus had none
        var skipped = new List<string>();      // accepted divergences, left as they are
        var heldHere = new List<string>();     // shared, here but not in the reference
        var unclassified = new List<string>(); // in the reference, matching no manifest rule
        var inStep = 0;
        var declined = 0;

        foreach (var rel in refFiles.Union(localFiles).OrderBy(r => r, StringComparer.Ordinal))
        {
            var layer = manifest.Resolve(rel);
            if (layer is null)
            {
                // Only the reference's own tree is sync's to complain about. A local file the manifest
                // cannot place is a real problem, and `--check` is the voice that says so.
                if (refFiles.Contains(rel)) unclassified.Add(rel);
                continue;
            }

            if (layer is not ("synced" or "verification" or "forked")) continue;

            if (MechanismCheck.Declined(rel, layer, lockFile) || declinedTypes.Declines(rel))
            {
                declined++;
                continue;
            }

            if (!refFiles.Contains(rel))
            {
                if (layer is not "forked") heldHere.Add(rel);
                continue;
            }

            if (accepted.TryGetValue(rel, out var reason))
            {
                skipped.Add(reason is null ? rel : $"{rel}  ({reason})");
                continue;
            }

            // Forked is seeded once and never reconciled, so the only question it answers is whether this
            // corpus has a copy at all. Shared files are compared on their authored half — a page whose
            // generated block is built from local types is in step while that half matches, and copying it
            // would replace the block with the reference's until the regeneration below undid it.
            if (localFiles.Contains(rel))
            {
                if (layer is "forked") continue;
                if (MechanismCheck.Same(localRoot, refRoot, rel)) { inStep++; continue; }
            }
            else if (layer is "forked")
            {
                Copy(refRoot, localRoot, rel);
                seeded.Add(rel);
                continue;
            }

            Copy(refRoot, localRoot, rel);
            updated.Add(rel);
        }

        Console.WriteLine($"mechanism: syncing the shared layers from {reference}");
        Section("UPDATED — brought down from the reference", updated);
        Section("SEEDED — the corpus's own from here on, copied because it had none", seeded);
        Section("SKIPPED — accepted divergences, left as they are", skipped);
        Section("HELD HERE, NOT UPSTREAM — shared files the reference does not have (sync never deletes)", heldHere);

        MechanismLock.Stamp(localRoot, manifest.Version, reference, today);
        Console.WriteLine(
            $"synced: {updated.Count} updated, {inStep} already in step; seeded {seeded.Count}; "
            + $"skipped {skipped.Count}; declined {declined}. "
            + $"Recorded in .mechanism.lock as mechanism version {manifest.Version}, taken {today}.");

        if (unclassified.Count > 0)
        {
            Console.Error.WriteLine("UNCLASSIFIED — files in the reference matching no manifest rule, so not copied:");
            foreach (var p in unclassified) Console.Error.WriteLine($"  {p}");
            Console.Error.WriteLine("mechanism sync: the reference's manifest does not resolve its own tree — fix it there.");
            return 1;
        }

        // Every synced page may carry a generated block built from this corpus's own types, so the copies
        // above are only right once they have been rebuilt against what this corpus holds. Regenerating
        // here is what makes `index --check` sync's postcondition rather than the reader's next surprise.
        return Regenerate(localRoot);

        static void Section(string heading, List<string> paths)
        {
            if (paths.Count == 0) return;
            Console.WriteLine($"{heading}:");
            foreach (var p in paths) Console.WriteLine($"  {p}");
        }
    }

    private static int Regenerate(string localRoot)
    {
        try
        {
            if (Commands.Index(localRoot, check: false) == 0) return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"mechanism sync: regeneration failed — {ex.Message}");
        }

        Console.Error.WriteLine(
            "mechanism sync: the files are in place but the generated blocks were not rebuilt. "
            + "Run ./kac validate to see what the corpus is missing, then ./kac index.");
        return 1;
    }

    private static void Copy(string fromRoot, string toRoot, string rel)
    {
        var target = Path.Combine(toRoot, rel);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(Path.Combine(fromRoot, rel), target, overwrite: true);
    }

    // The pages and folders of types this corpus did not adopt.
    //
    // `Declined` names the schema file, which is all the check needs: a type's root page and template are
    // `forked`, and the check never asks a corpus to hold a forked file. Sync does seed one that is
    // absent, so without this it would stand up every type the reference has and leave the corpus holding
    // pages for types it had declined — with `validate` then reporting each as stood up and not adopted.
    private static DeclinedPaths DeclinedTypePaths(string refRoot, MechanismLock lockFile)
    {
        if (lockFile.Types is null) return new DeclinedPaths([], []);

        var files = new HashSet<string>(StringComparer.Ordinal);
        var folders = new List<string>();
        foreach (var (name, type) in Schema.Load(refRoot).ByFolder)
        {
            if (lockFile.Adopted(name)) continue;
            if (!string.IsNullOrEmpty(type.Page)) files.Add(type.Page);
            folders.Add((string.IsNullOrEmpty(type.Folder) ? name : type.Folder) + "/");
        }

        return new DeclinedPaths(files, folders);
    }

    private record DeclinedPaths(HashSet<string> Files, List<string> Folders)
    {
        public bool Declines(string rel) =>
            Files.Contains(rel) || Folders.Any(f => rel.StartsWith(f, StringComparison.Ordinal));
    }
}
