namespace kac.core;

/// <summary>
/// Explicit, as <c>DocumentRules.All</c> is: a rule that stopped being found would look like a rule that never fires.
/// </summary>
public static class CorpusRules
{
    public static readonly IReadOnlyList<ICorpusRule> All =
    [
        new NoDependencyCycles(),
        new AlertAfterNeedsAnAlert(),
        new AlignmentRollup(),
        new ClauseQuotedFaithfully(),
        new DecisionsCiteNoDescriptiveRecord(),
        new DependencyCriticality(),
        new FeatureFileRepo(),
        new MirrorsRepoLinks(),
        new RedefinitionsAreLinked(),
        new ReportStale()
    ];

    public static readonly IReadOnlyDictionary<RuleId, ICorpusRule> ByRuleId =
        All.ToDictionary(r => r.RuleId);
}
