// The `mechanism` engines. `--check` resolves every file against the manifest and compares the shared
// layers against a reference, read-only. `--sync` brings those layers down from one.
//
// Both read the same manifest and the same descriptor, and both ask one predicate what this corpus
// holds. So the check can never report a file missing that a sync would decline to bring.
//
// Each engine answers in a value and prints nothing, so a caller other than a person can act on what
// it says. Commands.ReportMechanism is the half that writes it out.

namespace kac.core;

// Where every file stands against the reference. The lists are faults, in the order a reader should
// act on them; the counts are the corpus in step, which is worth a line and not a list.
public sealed record MechanismReport(
    IReadOnlyList<string> Drift,              // synced, both present, content differs
    IReadOnlyList<string> MissingLocally,     // synced, in the reference but not here
    IReadOnlyList<string> MissingUpstream,    // synced, here but not in the reference
    IReadOnlyList<string> Unclassified,       // matches no manifest rule
    IReadOnlyList<string> ResolvedDivergence, // accepted, but now identical again
    int SyncedInStep,
    int ForkedShared,
    int ForkedDiffer,
    int AcceptedActive,
    int DeclinedButHeld)
{
    // What makes the check fail. Resolved divergences and the counts are reported and are not faults:
    // one is a tidy-up the corpus can make when it likes, and the rest are the corpus being right.
    public int Problems => Drift.Count + MissingLocally.Count + MissingUpstream.Count + Unclassified.Count;
}

public static class MechanismCheck
{
    // Where this corpus stands against a reference, decided from listings rather than from a filesystem.
    //
    // `same` answers whether two copies of a path say the same thing; the caller supplies it, so the
    // whole classification is decidable from in-memory sets and every arm of it can be asserted without
    // two trees on disk. `Same` below is what a command passes.
    public static MechanismReport Classify(IReadOnlySet<string> localFiles, IReadOnlySet<string> refFiles,
        Manifest manifest, CorpusDescriptor descriptor, Func<string, bool> same)
    {
        var accepted = descriptor.Accepted.Select(a => a.Path).ToHashSet(StringComparer.Ordinal);

        var drift = new List<string>();
        var missingLocally = new List<string>();
        var missingUpstream = new List<string>();
        var unclassified = new List<string>();
        var resolvedDivergence = new List<string>();
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
                    var identical = inLocal && inRef && same(rel);

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
                        if (!same(rel)) forkedDiffer++;
                    }

                    break;

                // generated / local / ignored: each corpus owns these; nothing to compare.
            }
        }

        return new MechanismReport(drift, missingLocally, missingUpstream, unclassified, resolvedDivergence,
            syncedInStep, forkedShared, forkedDiffer, acceptedActive, declinedButHeld);
    }

    // Whether this corpus's descriptor declines the file, so that it is neither missing nor drifted —
    // and, to `mechanism --sync`, not something to bring down. Each answer below is the corpus stating
    // what it took, so check and sync ask this one predicate rather than two that can disagree.
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
