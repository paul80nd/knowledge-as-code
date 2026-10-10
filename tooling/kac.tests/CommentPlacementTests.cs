using System.Text.RegularExpressions;
using System.Xml;

namespace kac.tests;

/// <summary>
/// Microsoft's C# conventions: a member's comment is an XML doc comment, and a comment sits on a line of its own.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public partial class CommentPlacementTests
{
    /// <summary>A member declaration. A local function cannot carry an accessibility modifier, so none matches.</summary>
    [GeneratedRegex(@"^\s*(public|internal|private|protected|file)\b")]
    private static partial Regex Member();

    [Fact]
    public void A_comment_on_a_member_is_a_doc_comment()
    {
        var found = new List<string>();

        foreach (var file in CommentCitationTests.Sources())
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!IsLineComment(lines[i])) continue;

                var start = i;
                while (i < lines.Length && IsLineComment(lines[i])) i++;

                var next = i;
                while (next < lines.Length && IsAttribute(lines[next])) next++;

                if (!FollowsAttribute(lines, start) && next < lines.Length && Member().IsMatch(lines[next]))
                    found.Add($"{Relative(file)}:{start + 1}");
            }
        }

        Assert.True(found.Count == 0,
            "a member's comment is written with // where a caller reads ///:\n  " + string.Join("\n  ", found));
    }

    [Fact]
    public void No_comment_trails_code_except_a_test_case_label()
    {
        var found = new List<string>();

        foreach (var file in CommentCitationTests.Sources())
        {
            var source = File.ReadAllText(file);
            var lines = source.Split('\n');
            var line = 0;
            var lineStart = 0;

            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (c == '\n')
                {
                    line++;
                    lineStart = i + 1;
                }
                else if (c is '"' or '\'')
                {
                    var end = NullForgivingTests.Literal(source, i);
                    line += source.AsSpan(i, end - i).Count('\n');
                    lineStart = Math.Max(lineStart, source.LastIndexOf('\n', end - 1) + 1);
                    i = end - 1;
                }
                else if (c == '/' && i + 1 < source.Length && source[i + 1] is '/' or '*')
                {
                    var trailing = source.AsSpan(lineStart, i - lineStart).Trim().Length > 0;
                    if (source[i + 1] == '/' && trailing && !InTestCase(lines, line))
                        found.Add($"{Relative(file)}:{line + 1}");

                    var close = source[i + 1] == '/'
                        ? source.IndexOf('\n', i)
                        : source.IndexOf("*/", i, StringComparison.Ordinal);
                    if (close < 0) break;
                    line += source.AsSpan(i, close - i).Count('\n');
                    i = close - 1;
                }
            }
        }

        Assert.True(found.Count == 0,
            "a comment trails code. Put it on the line above:\n  " + string.Join("\n  ", found));
    }

    [Fact]
    public void Every_doc_comment_is_well_formed_xml()
    {
        var found = new List<string>();

        foreach (var file in CommentCitationTests.Sources())
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!IsDocComment(lines[i])) continue;

                var start = i;
                var body = new List<string>();
                while (i < lines.Length && IsDocComment(lines[i])) body.Add(lines[i++].TrimStart()[3..]);

                try
                {
                    new XmlDocument().LoadXml("<doc>" + string.Join('\n', body) + "</doc>");
                }
                catch (XmlException e)
                {
                    found.Add($"{Relative(file)}:{start + 1} {e.Message}");
                }
            }
        }

        Assert.True(found.Count == 0,
            "a doc comment is not well-formed XML. Escape < and &, and close every tag:\n  "
            + string.Join("\n  ", found));
    }

    private static bool IsLineComment(string line)
    {
        var t = line.TrimStart();
        return t.StartsWith("//", StringComparison.Ordinal) && !t.StartsWith("///", StringComparison.Ordinal);
    }

    private static bool IsDocComment(string line) => line.TrimStart().StartsWith("///", StringComparison.Ordinal);

    private static bool IsAttribute(string line) => line.TrimStart().StartsWith('[');

    /// <summary>
    /// Whether the line above the comment closes an attribute, so the comment labels the next one in a list.
    /// </summary>
    private static bool FollowsAttribute(string[] lines, int at)
    {
        for (var k = at - 1; k >= 0; k--)
            if (lines[k].Trim() is { Length: > 0 } above)
                return above.StartsWith('[') && above.EndsWith(']');

        return false;
    }

    /// <summary>
    /// Whether the line is part of an <c>[InlineData]</c> row, whose trailing label annotates one case of a table.
    /// </summary>
    private static bool InTestCase(string[] lines, int at)
    {
        var k = at;
        while (k > 0 && lines[k].TrimStart() is var t && (t.StartsWith('+') || t.StartsWith('"'))) k--;

        return lines[k].TrimStart().StartsWith("[InlineData(", StringComparison.Ordinal);
    }

    private static string Relative(string file) => Path.GetRelativePath(Repo.Root, file).Replace('\\', '/');
}
