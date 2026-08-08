namespace kac.core;

// The registry. Explicit rather than discovered by reflection, for the same reason `CheckCatalogue.All`
// and `RuleExpr.Functions` are explicit: what runs should be readable in one place, and a rule that
// silently stopped being found would look exactly like a rule that never fires.
public static class DocumentRules
{
    public static readonly IReadOnlyList<IDocumentRule> All =
    [
        new YStatementPresent(),
        new AlternativesHaveVerdicts()
    ];

    public static readonly IReadOnlyDictionary<string, IDocumentRule> ByRuleId =
        All.ToDictionary(r => r.RuleId, StringComparer.Ordinal);
}
