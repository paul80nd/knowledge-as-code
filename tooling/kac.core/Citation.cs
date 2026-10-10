namespace kac.core;

/// <summary>
/// <c>eng:pol-VURM.TIMEBOX</c> carries a scope, a record and a part. <c>docs/framework/metadata.md</c> settles the
/// notation.
/// </summary>
public readonly record struct Citation(string? Scope, string Record, string? Part)
{
    public override string ToString() =>
        (Scope is null ? "" : Scope + ":") + Record + (Part is null ? "" : "." + Part);

    public string Whole => (Scope is null ? "" : Scope + ":") + Record;

    /// <summary>
    /// Keeps the part, so a suggested spelling never turns a clause reference into a whole-record one.
    /// </summary>
    public Citation In(string? scope) => this with { Scope = scope };

    /// <summary>Reads the form alone. The validator answers whether the scope and the record exist.</summary>
    public static Citation Read(string text)
    {
        var (scope, after) = Split(text);
        var dot = after.IndexOf('.', StringComparison.Ordinal);

        return dot < 0
            ? new Citation(scope, after, null)
            : new Citation(scope, after[..dot], after[(dot + 1)..]);
    }

    /// <summary>
    /// A shortcode is lower-case letters and digits, as <c>CorpusDescriptor.ShortcodeFault</c> requires, so a colon
    /// anywhere else belongs to the text.
    /// </summary>
    public static (string? Scope, string After) Split(string text)
    {
        var colon = text.IndexOf(':', StringComparison.Ordinal);
        if (colon <= 0 || colon == text.Length - 1) return (null, text);

        var scope = text[..colon];
        return scope.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9')
            ? (scope, text[(colon + 1)..])
            : (null, text);
    }
}
