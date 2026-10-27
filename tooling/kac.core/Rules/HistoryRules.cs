namespace kac.core;

/// <summary>
/// Explicit, as <c>DocumentRules.All</c> is: a rule that stopped being found would look like a rule that never fires.
/// </summary>
public static class HistoryRules
{
    /// <summary>
    /// Reported by <c>Validator.CheckHistory</c> for every type, because it reads only the type's <c>in-force:</c>
    /// value. No schema declares it as a rule.
    /// </summary>
    public static readonly CheckId RetireNotDelete = new("retire-not-delete");

    public static readonly IReadOnlyList<IHistoryRule> All =
    [
        new ImmutableAfter(new RuleId("immutable-after-accepted"), ["accepted", "rejected"], "ADR",
            id => $"A decided ADR changes only by a new ADR that supersedes it. Restore the text, and write the "
                  + $"new decision in an ADR naming {id} in `supersedes`."),
        new ImmutableAfter(new RuleId("immutable-after-published"), ["published"], "postmortem",
            id => "A published account changes only by a typo fixed, a link corrected, a reflow or an `accepts` "
                  + "entry. Restore the text, and write what was learnt since in a new postmortem that links to "
                  + $"{id}."),
        new PartIdKept()
    ];

    public static readonly IReadOnlyDictionary<RuleId, IHistoryRule> ByRuleId =
        All.ToDictionary(r => r.RuleId);

    /// <summary>
    /// The history checks a run without <c>--since</c> skips for this corpus: each history rule an adopted type
    /// declares, then <c>retire-not-delete</c> where an adopted type names an <c>in-force:</c> value.
    /// </summary>
    public static IReadOnlyList<string> Skipped(Schema schema, IEnumerable<TypeSchema> adopted)
    {
        var types = adopted.ToList();
        List<string> skipped =
        [
            .. types.SelectMany(t => t.Rules).Select(r => r.Id).Where(ByRuleId.ContainsKey).Select(r => r.Value)
                .Distinct(StringComparer.Ordinal)
        ];

        if (types.Any(t => schema.EffectiveField(t, "status")?.InForce is { Length: > 0 }))
            skipped.Add(RetireNotDelete.Value);

        return skipped;
    }
}
