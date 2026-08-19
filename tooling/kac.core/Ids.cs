// ---------------------------------------------------------------------------
// The two ids
// ---------------------------------------------------------------------------

namespace kac.core;

// A check id: what a finding reports under, and what `_checks.yaml` declares an entry for.
//
// Its own type because the corpus has two kinds of id in play and they are one suffix apart:
// `y-statement-present` is the rule a type declares, `y-statement` is what that rule reports under.
// Held as strings the two swap silently, and the swap reads as correct in every place it can happen —
// a registry lookup, a catalogue entry, a table row. Neither converts to the other or to a string, so
// the compiler asks the question a reader would have to.
//
// There is no conversion from string either: an id enters the program where one is declared — the
// schema, or a rule class naming what it emits — rather than wherever a literal is convenient.
//
// Ordered, because findings and catalogue entries are listed for people to read and an id is what they
// are listed by. Ordinal, like every other comparison over these files.
public readonly record struct CheckId(string Value) : IComparable<CheckId>
{
    public int CompareTo(CheckId other) => string.CompareOrdinal(Value, other.Value);

    public override string ToString() => Value;
}

// A rule id: what a type's `rules:` block declares, and what the dispatcher finds a rule class by.
// A rule reports under one or more `CheckId`s, which are usually not this. See CheckId.
public readonly record struct RuleId(string Value)
{
    public override string ToString() => Value;
}
