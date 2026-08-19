// The `mechanism --sync` engine: bring the shared layers down from a reference corpus, then record
// what was taken. MechanismCheck reads the same manifest and the same descriptor; this half writes.
//
// Sync copies and never deletes. Where this corpus holds a file the reference does not, sync names it
// and leaves it alone. Deleting knowledge because an upstream tree was smaller is not a tool's call.
//
// Deciding and doing are two steps. `Plan` says what the sync comes to and touches nothing; `Apply`
// carries it out. So what a sync comes to can be asked and answered without a corpus on disk.

namespace kac.core;

// What a sync comes to. Every list is a set of paths the reader should be told about; `Updated` and
// `Seeded` are also, and only, the files to copy — named once, so no second list can disagree with
// this one about what a sync touches.
public sealed record SyncPlan(
    IReadOnlyList<string> Updated,      // shared, to be copied down because the authored halves differ
    IReadOnlyList<string> Seeded,       // forked, to be copied because this corpus has none
    IReadOnlyList<string> Skipped,      // accepted divergences, left as they are — path and reason
    IReadOnlyList<string> HeldHere,     // shared, here but not in the reference
    IReadOnlyList<string> Unclassified, // in the reference, matching no manifest rule
    int InStep,
    int Declined)
{
    public IEnumerable<string> Copies => Updated.Concat(Seeded);

    // A reference whose own manifest cannot place its own tree. Sync copies what it could resolve and
    // then stops: the rest is a defect upstream, and guessing at it is how a corpus takes a file nobody
    // meant to share.
    public bool ReferenceIsUnsound => Unclassified.Count > 0;
}

public static class MechanismSync
{
    // What syncing from this reference would come to, decided from listings rather than a filesystem.
    //
    // `manifest` is the *reference's*, because the boundary comes down with the files it describes: a
    // corpus on an older manifest would otherwise resolve the new upstream tree by the old rules and
    // silently skip whatever the rules had not yet heard of.
    //
    // `declinedTypes` is the reference's pages and folders for types this corpus did not adopt, which
    // the caller resolves from the reference's schema — see `DeclinedTypePaths`. `same` answers whether
    // two copies of a path say the same thing.
    public static SyncPlan Plan(IReadOnlySet<string> localFiles, IReadOnlySet<string> refFiles,
        Manifest manifest, CorpusDescriptor descriptor, DeclinedPaths declinedTypes, Func<string, bool> same)
    {
        var accepted = descriptor.Accepted.ToDictionary(a => a.Path, a => a.Reason, StringComparer.Ordinal);

        var updated = new List<string>();
        var seeded = new List<string>();
        var skipped = new List<string>();
        var heldHere = new List<string>();
        var unclassified = new List<string>();
        var inStep = 0;
        var declined = 0;

        foreach (var rel in refFiles.Union(localFiles).OrderBy(r => r, StringComparer.Ordinal))
        {
            var layer = manifest.Resolve(rel);
            if (layer is null)
            {
                // Sync complains only about the reference's own tree. A local file the manifest cannot
                // place is a real problem, and `--check` is the voice that says so.
                if (refFiles.Contains(rel)) unclassified.Add(rel);
                continue;
            }

            if (layer is not ("synced" or "verification" or "forked")) continue;

            if (MechanismCheck.Declined(rel, layer, descriptor) || declinedTypes.Declines(rel))
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

            // A forked file is seeded once and never reconciled, so the only question here is whether this
            // corpus has a copy. A shared file is compared on its authored half instead. A page whose
            // generated block is built from local types is in step while that half matches, and copying it
            // would swap the block for the reference's until the regeneration afterwards undid the swap.
            if (localFiles.Contains(rel))
            {
                if (layer is "forked") continue;
                if (same(rel))
                {
                    inStep++;
                    continue;
                }
            }
            else if (layer is "forked")
            {
                seeded.Add(rel);
                continue;
            }

            updated.Add(rel);
        }

        return new SyncPlan(updated, seeded, skipped, heldHere, unclassified, inStep, declined);
    }

    // Carry the plan out: copy what it names, then record what was taken. Copying and stamping are one
    // step because a corpus holding the reference's files and not saying so has taken a sync it cannot
    // be held to.
    public static void Apply(SyncPlan plan, string localRoot, string refRoot, Manifest manifest,
        string reference, string today)
    {
        foreach (var rel in plan.Copies) Copy(refRoot, localRoot, rel);
        CorpusDescriptor.Stamp(localRoot, manifest.Version, reference, today);
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
    // `forked`, and the check never asks a corpus to hold a forked file. Sync does seed an absent one.
    // Without this it would stand up every type the reference has, and `validate` would then report each
    // of them as stood up and not adopted.
    public static DeclinedPaths DeclinedTypePaths(string refRoot, CorpusDescriptor descriptor)
    {
        if (descriptor.Types is null) return new DeclinedPaths([], []);

        var files = new HashSet<string>(StringComparer.Ordinal);
        var folders = new List<string>();
        foreach (var (name, type) in Schema.Load(refRoot).ByFolder)
        {
            if (descriptor.Adopted(name)) continue;
            if (!string.IsNullOrEmpty(type.Page)) files.Add(type.Page);
            folders.Add((string.IsNullOrEmpty(type.Folder) ? name : type.Folder) + "/");
        }

        return new DeclinedPaths(files, folders);
    }
}

public record DeclinedPaths(HashSet<string> Files, List<string> Folders)
{
    public bool Declines(string rel) =>
        Files.Contains(rel) || Folders.Any(f => rel.StartsWith(f, StringComparison.Ordinal));
}
