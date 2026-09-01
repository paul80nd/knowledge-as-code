// `!` turns off a compiler check one site at a time, and says nothing where it was wrong. A repository
// with `Nullable` enabled throughout has two better answers, both used here: a type that carries its own
// invariant through `MemberNotNullWhen`, and a read that names what it expected. `Required.cs` holds the
// second for tests, and `kac.core` holds it wherever a caller has already established a value.
//
// Held at nothing rather than at a ceiling. A budget above zero is a number nobody can argue about, and
// the first site under it is the one that teaches the next reader that `!` is available.

using System.Text;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class NullForgivingTests
{
    [Fact]
    public void No_source_file_silences_a_nullable_warning()
    {
        var found = new List<string>();

        foreach (var file in Sources())
        {
            var line = 0;
            foreach (var text in Code(File.ReadAllText(file)).Split('\n'))
            {
                line++;
                if (Forgiving(text)) found.Add($"{Path.GetRelativePath(Repo.Root, file)}:{line}");
            }
        }

        Assert.True(found.Count == 0,
            $"The null-forgiving operator appears at {found.Count} site(s). Carry the fact on the type, "
            + $"or read it through something that names what it expected:\n  {string.Join("\n  ", found)}");
    }

    private static IEnumerable<string> Sources() =>
        Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    // Whether the line carries `!` as the null-forgiving operator rather than as `not`. The operator
    // follows the value it vouches for, so what precedes it is a name, a closing bracket or a closing
    // parenthesis. `!=` is the other reading of those two characters.
    private static bool Forgiving(string line)
    {
        for (var i = 1; i < line.Length; i++)
        {
            if (line[i] != '!' || (i + 1 < line.Length && line[i + 1] == '=')) continue;
            if (char.IsLetterOrDigit(line[i - 1]) || line[i - 1] is '_' or ')' or ']') return true;
        }

        return false;
    }

    // The file with its comments and its string literals blanked, so a `!` inside either is not read as
    // code. Every literal form C# offers is a quote to skip to: raw, verbatim and ordinary all end at a
    // quote, and only the escape rules between them differ. The one it cannot see into is an
    // interpolation hole, whose contents are code inside a literal, and nothing here writes one.
    private static string Code(string source)
    {
        var kept = new StringBuilder(source.Length);
        var i = 0;

        while (i < source.Length)
        {
            var c = source[i];

            if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n') i++;
                continue;
            }

            if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                var close = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                Blank(kept, source, i, close < 0 ? source.Length : close + 2);
                i = close < 0 ? source.Length : close + 2;
                continue;
            }

            if (c is '"' or '\'')
            {
                var end = Literal(source, i);
                Blank(kept, source, i, end);
                i = end;
                continue;
            }

            kept.Append(c);
            i++;
        }

        return kept.ToString();
    }

    // Where the literal opening at `start` ends, one past its closing quote. The three forms differ only
    // in how a quote inside one is written: a raw literal ends at its fence, a verbatim literal doubles
    // the quote, and an ordinary one escapes it with a backslash.
    private static int Literal(string source, int start)
    {
        var quote = source[start];

        if (quote == '"' && source.AsSpan(start).StartsWith("\"\"\""))
        {
            var close = source.IndexOf("\"\"\"", start + 3, StringComparison.Ordinal);
            return close < 0 ? source.Length : close + 3;
        }

        var verbatim = start > 0 && source[start - 1] == '@';

        for (var i = start + 1; i < source.Length; i++)
        {
            if (verbatim)
            {
                if (source[i] != quote) continue;
                if (i + 1 < source.Length && source[i + 1] == quote) i++;
                else return i + 1;
            }
            else if (source[i] == '\\') i++;
            else if (source[i] == quote) return i + 1;
        }

        return source.Length;
    }

    // The span replaced by its newlines, so blanking a literal or a comment does not move the lines under
    // it.
    private static void Blank(StringBuilder kept, string source, int from, int to)
    {
        for (var i = from; i < to && i < source.Length; i++)
            if (source[i] == '\n') kept.Append('\n');
    }
}
