namespace kac.core;

// A reference as somebody wrote it: an optional scope, the record, and an optional part.
//
// `eng:pol-VURM.TIMEBOX` carries all three. `pol-VURM.TIMEBOX` names a record this corpus holds and a
// clause inside it. `eng:pol-VURM` names an imported record whole.
//
// One reading of the form, so a frontmatter value, a code span and a link label are held to the same
// grammar. `docs/framework/metadata.md` is where the notation is settled.
public readonly record struct Citation(string? Scope, string Record, string? Part)
{
    // The citation as it was written, which is what a message quotes back.
    public override string ToString() =>
        (Scope is null ? "" : Scope + ":") + Record + (Part is null ? "" : "." + Part);

    // The record with its scope and without its part, which is what resolving the first half asks about.
    public string Whole => (Scope is null ? "" : Scope + ":") + Record;

    // The same citation under another scope, or under none. What a message about a misspelling has to
    // suggest writing instead: dropping the part along with the scope would tell an author to replace a
    // clause reference with a whole-record one.
    public Citation In(string? scope) => this with { Scope = scope };

    // What the text says, whatever it says. Nothing here decides whether the scope names an import or
    // the record exists: this reads the form, and the validator answers for the meaning.
    public static Citation Read(string text)
    {
        var (scope, after) = Split(text);
        var dot = after.IndexOf('.', StringComparison.Ordinal);

        return dot < 0
            ? new Citation(scope, after, null)
            : new Citation(scope, after[..dot], after[(dot + 1)..]);
    }

    // The scope a citation carries, and what is left after it.
    //
    // A shortcode is lower-case letters and digits alone, which `CorpusDescriptor.ShortcodeFault` is
    // what holds a producer to. So a colon anywhere else belongs to the text rather than to a scope, and
    // the whole of it comes back unscoped.
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
