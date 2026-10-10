// `!` turns off a compiler check one site at a time. Use `MemberNotNullWhen` or a read that names what it
// expected instead. Held at nothing rather than at a ceiling.

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

    /// <summary>
    /// Whether the line carries <c>!</c> as the null-forgiving operator rather than as <c>not</c>. The operator follows
    /// the value it vouches for, so what precedes it is a name, a closing bracket or a closing parenthesis. <c>!=</c>
    /// is the other reading of those two characters.
    /// </summary>
    private static bool Forgiving(string line)
    {
        for (var i = 1; i < line.Length; i++)
        {
            if (line[i] != '!' || (i + 1 < line.Length && line[i + 1] == '=')) continue;
            if (char.IsLetterOrDigit(line[i - 1]) || line[i - 1] is '_' or ')' or ']') return true;
        }

        return false;
    }

    /// <summary>
    /// The file with its comments and string literals blanked, so a <c>!</c> inside either is not read as code. Every
    /// literal form ends at a quote. An interpolation hole is code inside a literal and is not seen into, so nothing
    /// here writes one.
    /// </summary>
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

    /// <summary>
    /// Where the literal opening at <c>start</c> ends, one past its closing quote. The three forms differ only in how a
    /// quote inside one is written: a raw literal ends at its fence, a verbatim literal doubles the quote, and an
    /// ordinary one escapes it with a backslash.
    /// </summary>
    internal static int Literal(string source, int start)
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

    /// <summary>
    /// The span replaced by its newlines, so blanking a literal or a comment does not move the lines under it.
    /// </summary>
    private static void Blank(StringBuilder kept, string source, int from, int to)
    {
        for (var i = from; i < to && i < source.Length; i++)
            if (source[i] == '\n')
                kept.Append('\n');
    }
}
