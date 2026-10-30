namespace kac.core;

/// <summary>A count and its noun as the tool prints them: <c>1 file</c>, <c>2 files</c>, <c>0 files</c>.</summary>
public static class Counted
{
    /// <summary>
    /// Adds <c>s</c> to <paramref name="one"/> for any count but one. Pass <paramref name="many"/> for a noun that
    /// does not take <c>s</c>, such as <c>entries</c>.
    /// </summary>
    public static string Of(int n, string one, string? many = null) => $"{n} {(n == 1 ? one : many ?? one + "s")}";
}
