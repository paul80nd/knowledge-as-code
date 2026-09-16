using kac.core;

// A `fix` and a `report` each list who checked the record and when. Rewrite the prose under one of those
// entries and it vouches for text its author never read. `validate` cannot see that, because a rule reads
// one document and has no earlier version of it to compare against. This reads the diff instead.
//
// The reach is the body, and no further. A frontmatter key moved on its own leaves the prose somebody
// verified exactly as they read it, so nothing is asked of the list.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class VerificationTests
{
    [Fact]
    public void A_record_whose_body_changed_carries_a_new_verification()
    {
        var named = Environment.GetEnvironmentVariable("KAC_BASE_REF");
        var before = Base(named);

        // A named base git cannot reach is a shallow checkout, and this guard reads nothing and passes on
        // one. An unnamed base that resolves nothing is a tree nobody branched, which is ordinary.
        Assert.False(before is null && !string.IsNullOrEmpty(named),
            $"KAC_BASE_REF names '{named}', and no commit in this tree answers to it, so nothing was "
            + "compared. This guard reads the history a shallow checkout drops, so the job setting that "
            + "variable needs 'fetch-depth: 0'.");
        if (before is null) return;

        var folders = Schema.Load(Repo.Root).ByFolder
            .Where(pair => pair.Value.Fields.ContainsKey("verified"))
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.Ordinal);

        // Every type declaring the field, so a rename of `fixes` or a third type taking a `verified` list
        // is covered with nothing to remember. Reading none of them is this guard having gone quiet, and
        // green is what it would report from then on.
        Assert.True(folders.Count > 0,
            "no type declares a 'verified' field, so this guard is checking nothing. Either the field was "
            + "renamed, or the types declaring it have gone.");

        var stale = new List<string>();
        var unread = new List<string>();

        foreach (var (was, now) in Changed(before).Where(pair => IsRecord(pair.Now, folders)))
        {
            // git answers nothing for a path it cannot spell as one argument, which is a path with a
            // space in it. Reported rather than skipped: a record this guard silently passed over reads
            // exactly like one it approved.
            if (Git.Run(Repo.Root, $"show {before}:{was}") is not { } text)
            {
                unread.Add(was);
                continue;
            }

            if (Verification.Unverified(text, Files.ReadLf(Path.Combine(Repo.Root, now)))) stale.Add(now);
        }

        Assert.True(unread.Count == 0,
            $"git could not read these at {before}, so nothing was compared:\n  "
            + string.Join("\n  ", unread));

        Assert.True(stale.Count == 0,
            "the body of these records changed and nobody answered for it:\n  "
            + string.Join("\n  ", stale)
            + "\nName yourself in 'generated' where you wrote the new text, or add a 'verified' entry "
            + "naming whoever read it.");
    }

    // The commit this branch grew from, or null where git could not name one. `named` comes from
    // `KAC_BASE_REF` and is the pull request's own target branch, which CI knows and a working tree does
    // not. It is used alone, so a job that sets it and gets no answer fails instead of falling back to a
    // branch that would compare the wrong thing.
    private static string? Base(string? named)
    {
        string[] candidates = string.IsNullOrEmpty(named) ? ["origin/main", "main"] : [named];

        return candidates
            .Select(candidate => Git.Run(Repo.Root, $"merge-base {candidate} HEAD")?.Trim())
            .FirstOrDefault(sha => sha is { Length: >= 40 });
    }

    // Every tracked file the working tree has changed since `before`, as the path it had then and the
    // path it has now. An edit nobody has committed yet is read the way CI reads a merged one. A rename
    // is a change like any other, and the two paths differ only for one: a record renamed and rewritten
    // in one pull request is what a guard reading the new path alone would pass over.
    //
    // `-z` separates every field with a NUL, so a path with a quote, a tab or a non-ASCII character in it
    // arrives as git stored it. Without it git escapes such a path and this would ask for a file whose
    // name it had just mangled.
    private static IEnumerable<(string Was, string Now)> Changed(string before)
    {
        var fields = (Git.Run(Repo.Root, $"diff --name-status -M -z --diff-filter=MR {before} --") ?? "")
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i + 1 < fields.Length;)
        {
            // A rename carries both paths after its status, and a modification carries one.
            var renamed = fields[i].StartsWith('R');
            if (renamed && i + 2 >= fields.Length) yield break;

            yield return renamed ? (fields[i + 1], fields[i + 2]) : (fields[i + 1], fields[i + 1]);
            i += renamed ? 3 : 2;
        }
    }

    // A record of a type declaring the field, in any of the trees here. The folder is looked for anywhere
    // above the file, because a corpus may file its records in subfolders and a type reads them all.
    // `_index.md` is generated and `_template.md` is a shape rather than a record, and neither carries a
    // verification.
    private static bool IsRecord(string rel, IReadOnlySet<string> folders)
    {
        var parts = rel.Split('/');

        return rel.EndsWith(".md", StringComparison.Ordinal)
               && !parts[^1].StartsWith('_')
               && parts[..^1].Any(folders.Contains);
    }
}
