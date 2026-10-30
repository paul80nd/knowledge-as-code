namespace kac.core;

/// <summary>
/// A rule comparing one record with its state at the revision <c>validate --since</c> names. It runs only with that
/// flag. <c>docs/maintaining/checks.md</c> says which interface a new rule wants.
/// </summary>
public interface IHistoryRule
{
    RuleId RuleId { get; }

    /// <summary>
    /// Usually not <c>RuleId</c>, as <c>IDocumentRule.Emits</c> explains. What each id means is <c>_checks.yaml</c>'s
    /// to say.
    /// </summary>
    IReadOnlyList<CheckId> Emits { get; }

    /// <summary>Asked only of a record whose text differs between the two revisions.</summary>
    void Check(HistoryRuleContext ctx);
}

/// <summary>
/// One record at both revisions, and reported against where it is now. <c>Validator.CheckHistory</c> says how the two
/// are paired.
/// </summary>
/// <param name="Before">The record at <paramref name="Since"/>, read with the schema of the working tree.</param>
/// <param name="Since">The revision as the caller spelled it, for a message to name.</param>
/// <param name="InForce">The type's <c>in-force:</c> value, read through the universal layer. Null where it names none.</param>
/// <param name="Renamed">
/// Each record's id at <paramref name="Since"/> to its id now, for every record whose id changed.
/// </param>
public sealed record HistoryRuleContext(
    Doc Before, Doc After, TypeSchema Type, RuleSpec Spec, string Since, string? InForce,
    IReadOnlyDictionary<string, string> Renamed, Report Report);
