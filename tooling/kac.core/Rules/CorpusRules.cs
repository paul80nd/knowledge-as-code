namespace kac.core;

// The registry, explicit for the reason `DocumentRules.All` is: what runs should be readable in one
// place, and a rule that silently stopped being found would look exactly like a rule that never fires.
public static class CorpusRules
{
    public static readonly IReadOnlyList<ICorpusRule> All =
    [
        new NoDependencyCycles()
    ];

    public static readonly IReadOnlyDictionary<RuleId, ICorpusRule> ByRuleId =
        All.ToDictionary(r => r.RuleId);
}
