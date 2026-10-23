namespace kac.core;

/// <summary>
/// A rule whose message names which part of a document is at fault. <c>docs/maintaining/checks.md</c> states the test.
/// </summary>
public interface IDocumentRule
{
    /// <summary>The id in the type schema's <c>rules:</c> block, which the dispatcher looks the rule up by.</summary>
    RuleId RuleId { get; }

    /// <summary>
    /// Usually not <c>RuleId</c>: <c>y-statement-present</c> emits <c>y-statement</c>. What each id means is
    /// <c>_checks.yaml</c>'s to say.
    /// </summary>
    IReadOnlyList<CheckId> Emits { get; }

    /// <summary>
    /// The config keys the rule reads from its entry in <c>rules:</c>. <c>SchemaChecks</c> reports any other the entry
    /// declares.
    /// </summary>
    IReadOnlyList<string> Reads => [];

    /// <summary>
    /// What is wrong with the config the schema declares for this rule, one sentence each. The rule checks no
    /// document while any is returned, so it is asked before every one.
    /// </summary>
    IEnumerable<string> Misconfigured(RuleSpec spec, Schema schema) => [];

    void Check(RuleContext ctx);
}

/// <summary>
/// Read a threshold from <c>Spec</c>, the rule as the schema declared it, never from a constant of the rule's own.
/// </summary>
public sealed record RuleContext(Doc Doc, TypeSchema Type, RuleSpec Spec, Report Report);
