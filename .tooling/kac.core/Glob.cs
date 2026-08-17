using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

// Minimal glob → regex for manifest patterns: `**` spans path segments, `*` stays within one, and a
// leading `**/` also matches at the root. Enough for the manifest's vocabulary, not a full gitignore.
namespace kac.core;

public static class Glob
{
    // One cache for the process, so it is written to by whoever asks first and read by everyone after.
    // Concurrent because the callers are: a spec suite runs its scenarios side by side, and each loads a
    // corpus of its own. A `Dictionary` corrupts itself under that, and it does so in whichever scenario
    // happened to be running rather than in the one that would explain why.
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
