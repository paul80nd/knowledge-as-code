namespace kac.core;

// Explicit, not discovered by reflection: a rule that stopped being found would look like a rule that never fires.
public static class DocumentRules
{
    public static readonly IReadOnlyList<IDocumentRule> All =
    [
        new YStatementPresent(),
        new AlternativesHaveVerdicts(),
        new TermsAreAlphabetical(),
        new NoSelfVerification(),
        new DurationMatchesTheMoments(),
        new ChangelogNewestFirst(),
        new EscalationRequired(),
        new BindsOnlyUnderRules(),
        new DefinitionsDoNotRepeatTheTerm(),
        new EntriesStateNoRequirement()
    ];

    public static readonly IReadOnlyDictionary<RuleId, IDocumentRule> ByRuleId =
        All.ToDictionary(r => r.RuleId);
}
