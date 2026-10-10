namespace kac.core;

// Explicit, as `DocumentRules.All` is: a rule that stopped being found would look like a rule that never fires.
public static class CorpusRules
{
    public static readonly IReadOnlyList<ICorpusRule> All =
    [
        new NoDependencyCycles(),
        new AlertAfterNeedsAnAlert(),
        new AlignmentRollup(),
        new ClauseQuotedFaithfully(),
        new DependencyCriticality(),
        new FeatureFileRepo(),
        new MirrorsRepoLinks(),
        new ReportStale()
    ];

    public static readonly IReadOnlyDictionary<RuleId, ICorpusRule> ByRuleId =
        All.ToDictionary(r => r.RuleId);
}
