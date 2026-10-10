namespace kac.core;

/// <summary>
/// A rule whose question no single document answers. <c>../../CLAUDE.md</c> says which interface a new rule wants.
/// </summary>
public interface ICorpusRule
{
    RuleId RuleId { get; }

    /// <summary>
    /// Usually not <c>RuleId</c>, as <c>IDocumentRule.Emits</c> explains. What each id means is <c>_checks.yaml</c>'s
    /// to say.
    /// </summary>
    IReadOnlyList<CheckId> Emits { get; }

    void Check(CorpusRuleContext ctx);
}

public sealed record CorpusRuleContext(
    IReadOnlyList<Doc> Docs,
    // Resolve an id through `ById`, so it resolves exactly as `ref-resolves` did.
    IReadOnlyDictionary<string, Doc> ById,
    Tree Tree,
    TypeSchema Type,
    RuleSpec Spec,
    // The content version of this corpus under its own name, and of each consumed corpus under the name its descriptor
    // gives.
    IReadOnlyDictionary<string, string> Versions,
    // Null where the descriptor states no `repos-base`.
    string? ReposBase,
    Action<Doc, CheckId, string, int?> Err,
    Action<Doc, CheckId, string, int?> Warn)
{
    public IEnumerable<Doc> Records => Docs.Where(d => d.Type == Type);
}
