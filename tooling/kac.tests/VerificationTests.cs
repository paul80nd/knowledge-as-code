using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class VerificationTests
{
    [Fact]
    public void A_record_whose_body_changed_carries_a_new_verification()
    {
        var before = Diff.Against();
        if (before is null) return;

        var folders = Schema.Load(Repo.Root).ByFolder
            .Where(pair => pair.Value.Fields.ContainsKey("verified"))
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(folders.Count > 0,
            "no type declares a 'verified' field, so this guard is checking nothing. Either the field was "
            + "renamed, or the types declaring it have gone.");

        var stale = new List<string>();
        var unread = new List<string>();

        // A modification or a rename, because a file added at this revision has nothing at `before` to
        // compare and a file deleted has nothing now.
        foreach (var (was, now) in Diff.Changed(before, "MR")
                     .Where(file => RecordPath.IsRecord(file.Now, folders)))
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
}
