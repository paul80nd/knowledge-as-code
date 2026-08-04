using System.Text;
using System.Text.RegularExpressions;

// Minimal glob → regex for manifest patterns: `**` spans path segments, `*` stays within one, and a
// leading `**/` also matches at the root. Enough for the manifest's vocabulary, not a full gitignore.
public static class Glob
{
    private static readonly Dictionary<string, Regex> Cache = [];

    public static bool IsMatch(string path, string pattern) =>
        (Cache.TryGetValue(pattern, out var re) ? re : Cache[pattern] = Compile(pattern)).IsMatch(path);

    private static Regex Compile(string glob)
    {
        var sb = new StringBuilder("^");
        var i = 0;
        if (glob.StartsWith("**/")) { sb.Append("(?:.*/)?"); i = 3; }
        for (; i < glob.Length; i++)
        {
            var c = glob[i];
            if (c == '*')
            {
                if (i + 1 < glob.Length && glob[i + 1] == '*') { sb.Append(".*"); i++; }
                else sb.Append("[^/]*");
            }
            else if (c == '/') sb.Append('/');
            else if ("\\.+?()[]{}|^$".IndexOf(c) >= 0) sb.Append('\\').Append(c);
            else sb.Append(c);
        }
        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.CultureInvariant);
    }
}
