namespace kac.core;

/// <summary>
/// A check id: what a finding reports under, and what <c>_checks.yaml</c> declares an entry for. It converts to neither
/// <c>RuleId</c> nor string. The two are one suffix apart (<c>y-statement-present</c> is a rule, <c>y-statement</c> the
/// check it reports), so only the compiler can tell them apart.
/// </summary>
public readonly record struct CheckId(string Value) : IComparable<CheckId>
{
    public int CompareTo(CheckId other) => string.CompareOrdinal(Value, other.Value);

    public override string ToString() => Value;
}

/// <summary>
/// A rule id: what a type's <c>rules:</c> block declares, and what the dispatcher finds a rule class by. A rule reports
/// under one or more <c>CheckId</c>s, which are usually not this. See CheckId.
/// </summary>
public readonly record struct RuleId(string Value)
{
    public override string ToString() => Value;
}
