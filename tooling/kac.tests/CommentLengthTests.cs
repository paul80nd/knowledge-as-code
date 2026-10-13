namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class CommentLengthTests
{
    /// <summary>
    /// Past this, a comment is arguing. writing-in-the-tool sends the argument to the commit message.
    /// </summary>
    private const int MaxLines = 3;

    [Fact]
    public void Every_comment_is_one_paragraph_of_at_most_three_lines()
    {
        var overlong = new List<string>();

        foreach (var file in CommentCitationTests.Sources())
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!IsComment(lines[i])) continue;

                var start = i;
                var breaks = false;
                while (i < lines.Length && IsComment(lines[i]))
                    breaks |= lines[i++].Trim() == "//";

                if (i - start > MaxLines || breaks)
                    overlong.Add($"{Path.GetRelativePath(Repo.Root, file).Replace('\\', '/')}:{start + 1} "
                              + $"runs {i - start} line(s){(breaks ? " in more than one paragraph" : "")}");
            }
        }

        Assert.True(overlong.Count == 0,
            $"a comment runs past one paragraph of {MaxLines} lines. State the constraint, or cite the page "
            + "holding the reasoning:\n  " + string.Join("\n  ", overlong));
    }

    /// <summary>
    /// <c>///</c> is read by a caller and carries its own XML structure, so only a maintainer's <c>//</c> is held here.
    /// </summary>
    private static bool IsComment(string line)
    {
        var t = line.TrimStart();
        return t.StartsWith("//", StringComparison.Ordinal) && !t.StartsWith("///", StringComparison.Ordinal);
    }
}
