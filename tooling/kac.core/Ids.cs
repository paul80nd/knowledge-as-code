namespace kac.core;

// A check id: what a finding reports under, and what `_checks.yaml` declares an entry for.
// It converts to neither `RuleId` nor string. The two are one suffix apart (`y-statement-present` is a rule,
// `y-statement` the check it reports), so only the compiler can tell them apart.
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
