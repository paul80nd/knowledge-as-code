namespace kac.core;

/// <summary>
/// A rule whose question no single document answers. <c>docs/maintaining/checks.md</c> says which interface a new
/// rule wants.
/// </summary>
public interface ICorpusRule
{
    RuleId RuleId { get; }

    /// <summary>
    /// Usually not <c>RuleId</c>, as <c>IDocumentRule.Emits</c> explains. What each id means is <c>_checks.yaml</c>'s
    /// to say.
    /// </summary>
    IReadOnlyList<CheckId> Emits { get; }

    /// <summary>As <c>IDocumentRule.Reads</c>.</summary>
    IReadOnlyList<string> Reads => [];

    /// <summary>
    /// What is wrong with the config the schema declares for this rule, one sentence each. Given the corpus, because a
    /// value may name something only the corpus defines. <c>Check</c> is not called while any is returned.
    /// </summary>
    IEnumerable<string> Misconfigured(CorpusRuleContext ctx) => [];

    void Check(CorpusRuleContext ctx);
}

/// <summary>What a corpus rule reads, and the two ways it reports a finding.</summary>
/// <param name="ById">Resolve an id through this, so it resolves exactly as <c>ref-resolves</c> does.</param>
/// <param name="ReposBase">Null where the descriptor states no <c>repos-base</c>.</param>
public sealed record CorpusRuleContext(
    IReadOnlyList<Doc> Docs,
    IReadOnlyDictionary<string, Doc> ById,
    Tree Tree,
    TypeSchema Type,
    RuleSpec Spec,
    // The content version of this corpus under its own name, and of each consumed corpus under the name its descriptor
    // gives.
    IReadOnlyDictionary<string, string> Versions,
    string? ReposBase,
    Action<Doc, CheckId, string, int?> Err,
    Action<Doc, CheckId, string, int?> Warn)
{
    public IEnumerable<Doc> Records => Docs.Where(d => d.Type == Type);
}
