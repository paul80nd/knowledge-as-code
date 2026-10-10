using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace kac.core;

/// <summary>
/// Minimal glob → regex for manifest patterns: <c>**</c> spans path segments, <c>*</c> stays within one, and a leading
/// <c>**/</c> also matches at the root. Enough for the manifest's vocabulary, not a full gitignore.
/// </summary>
public static class Glob
{
    /// <summary>
    /// Concurrent, because the spec suite runs scenarios side by side and a <c>Dictionary</c> corrupts itself under
    /// that.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Regex> Cache = new(StringComparer.Ordinal);

    public static bool IsMatch(string path, string pattern) =>
        Cache.GetOrAdd(pattern, Compile).IsMatch(path);

    private static Regex Compile(string glob)
    {
        var sb = new StringBuilder("^");
        var i = 0;
        if (glob.StartsWith("**/"))
        {
            sb.Append("(?:.*/)?");
            i = 3;
        }

        for (; i < glob.Length; i++)
        {
            var c = glob[i];
            switch (c)
            {
                case '*' when i + 1 < glob.Length && glob[i + 1] == '*':
                    sb.Append(".*");
                    i++;
                    break;
                case '*':
                    sb.Append("[^/]*");
                    break;
                case '/':
                    sb.Append('/');
                    break;
                default:
                {
                    if ("\\.+?()[]{}|^$".Contains(c)) sb.Append('\\').Append(c);
                    else sb.Append(c);
                    break;
                }
            }
        }

        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.CultureInvariant);
    }
}
