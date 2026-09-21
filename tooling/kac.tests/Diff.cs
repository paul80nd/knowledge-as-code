using kac.core;

// What a pull request changed, for the guards that compare this branch against the one it targets.
// `VerificationTests` and `ContentVersionTests` both ask git what moved, so the process call and the
// parsing sit here once.

namespace kac.tests;

internal static class Diff
{
    // The branch a pull request targets. CI knows it and a working tree does not, so the job passes it in.
    private const string BaseRef = "KAC_BASE_REF";

    // The commit this branch grew from, or null where git named none and there is nothing to compare. A
    // named base is used alone, so a job that sets `KAC_BASE_REF` and gets no answer fails here instead of
    // falling back to a branch that would compare the wrong thing. That state is a shallow checkout, and a
    // guard reading nothing reports green for a branch it never looked at.
    internal static string? Against()
    {
        var named = Environment.GetEnvironmentVariable(BaseRef);
        string[] candidates = string.IsNullOrEmpty(named) ? ["origin/main", "main"] : [named];

        var before = candidates
            .Select(candidate => Git.Run(Repo.Root, $"merge-base {candidate} HEAD")?.Trim())
            .FirstOrDefault(sha => sha is { Length: >= 40 });

        // An unnamed base that resolves nothing is a tree nobody branched, which is ordinary.
        Assert.False(before is null && !string.IsNullOrEmpty(named),
            $"{BaseRef} names '{named}', and no commit in this tree answers to it, so nothing was "
            + "compared. These guards read the history a shallow checkout drops, so the job setting that "
            + "variable needs 'fetch-depth: 0'.");

        return before;
    }

    // Every file the working tree keeps that git does not track, forward-slashed. `git diff` reports none
    // of them, so a record written on this branch and not yet committed is invisible to `Changed`. CI
    // reads it as an addition, and a guard that missed it locally would be green here and red there.
    internal static IEnumerable<string> Untracked() =>
        (Git.Run(Repo.Root, "ls-files --others --exclude-standard -z") ?? "")
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);

    // Every tracked file the working tree has changed since `before`: git's status letter, the path the
    // file had then, and the path it has now. `filter` is git's `--diff-filter`, so each caller asks for
    // the statuses its own question needs. An edit nobody has committed yet is read the way CI reads a
    // merged one. Only a rename gives two different paths, and a record renamed and rewritten in one pull
    // request is what a guard reading the new path alone would pass over.
    //
    // `-z` separates every field with a NUL, so a path with a quote, a tab or a non-ASCII character in it
    // arrives as git stored it. Without it git escapes such a path and the caller asks for a file whose
    // name it had just mangled.
    internal static IEnumerable<(char Status, string Was, string Now)> Changed(string before, string filter)
    {
        var fields =
            (Git.Run(Repo.Root, $"diff --name-status -M -z --diff-filter={filter} {before} --") ?? "")
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i + 1 < fields.Length;)
        {
            // A rename carries both paths after its status, and every other status carries one.
            var renamed = fields[i].StartsWith('R');
            if (renamed && i + 2 >= fields.Length) yield break;

            var status = fields[i][0];

            yield return renamed
                ? (status, fields[i + 1], fields[i + 2])
                : (status, fields[i + 1], fields[i + 1]);

            i += renamed ? 3 : 2;
        }
    }
}
