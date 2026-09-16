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

        foreach (var rel in Modified(before).Where(rel => IsRecord(rel, folders)))
        {
            if (Git.Run(Repo.Root, $"show {before}:{rel}") is not { } was) continue;

            if (Verification.Unverified(was.Replace("\r\n", "\n"), Files.ReadLf(Path.Combine(Repo.Root, rel))))
                stale.Add(rel);
        }

        Assert.True(stale.Count == 0,
            "the body of a verified record changed and its 'verified' list did not:\n  "
            + string.Join("\n  ", stale)
            + "\nAdd an entry naming whoever read the new text, or leave the body alone.");
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
            .FirstOrDefault(sha => sha is { Length: 40 });
    }

    // Every tracked file the working tree has changed since `before`, so an edit nobody has committed yet
    // is read the way CI reads a merged one. A rename is left out: git reports it as one path, and
    // `git show` of the old one answers for a record this tree no longer has.
    private static IEnumerable<string> Modified(string before) =>
        (Git.Run(Repo.Root, $"diff --name-only --diff-filter=M {before} --") ?? "")
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(rel => rel.Replace('\\', '/'));

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
