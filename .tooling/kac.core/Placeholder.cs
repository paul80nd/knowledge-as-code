// ---------------------------------------------------------------------------
// The placeholder mark
// ---------------------------------------------------------------------------

namespace kac.core;

// `{{…}}` is what a template marks as the author's to supply — an id, a value, a filename, a phrase.
// One mark rather than a vocabulary of stand-in words, because the tool has to recognise exactly what
// the templates teach: a corpus that also reads `NNNN`, `XXXX` and `example` as pretend has three more
// ways to write a placeholder than it tells anyone about, and `example` is a slug a real document may
// legitimately want.
//
// The mark is only meaningful in a template. A record carrying one is an unfinished copy rather than a
// document with a placeholder in it, so nothing here is asked of a record.
public static class Placeholder
{
    public const string Mark = "{{";

    public static bool In(string? value) => value?.Contains(Mark, StringComparison.Ordinal) ?? false;
}
