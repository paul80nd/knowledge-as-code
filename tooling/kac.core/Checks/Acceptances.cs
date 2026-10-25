using YamlDotNet.RepresentationModel;

namespace kac.core;

/// <summary>
/// The <c>accepts:</c> list a record keeps in its frontmatter: each entry names a check and the reason the record keeps
/// that check's warning. See <c>docs/use/cli/validate.md</c>.
/// </summary>
public static class Acceptances
{
    public const string Field = "accepts";

    public static readonly CheckId Stale = new("acceptance-stale");
    public static readonly CheckId Refused = new("acceptance-check");

    /// <summary>
    /// Rewrites <paramref name="findings"/> in place: each entry turns the warnings its check reported on its record
    /// into info. An entry missing its check or its reason is skipped, because <c>entry-key</c> or
    /// <c>field-pattern</c> has reported it.
    /// </summary>
    public static void Apply(IReadOnlyList<Doc> docs, IReadOnlyList<CheckDef> catalogue, List<Finding> findings)
    {
        var severities = catalogue.ToLookup(c => c.Id, c => c.Severity);

        foreach (var doc in docs)
        {
            if (doc.FrontNode(Field) is not YamlSequenceNode entries) continue;
            var report = new Report(doc.Rel, findings);

            foreach (var entry in entries.Children)
            {
                if (Yaml.Get(entry, "check") is not YamlScalarNode { Value: { Length: > 0 } named }) continue;
                if (Yaml.Get(entry, "reason") is not YamlScalarNode { Value: { } reason }
                    || string.IsNullOrWhiteSpace(reason)) continue;
                var check = new CheckId(named);
                var line = Yaml.LineOf(entry, doc.FrontStartLine);

                if (Refusal(check, severities) is { } refusal)
                {
                    report.Err(Refused, refusal, line);
                    continue;
                }

                // Matched against what is still a warning, so a second entry for one check is stale.
                var matched = 0;
                for (var i = 0; i < findings.Count; i++)
                {
                    var f = findings[i];
                    if (f.File != doc.Rel || f.Severity != Sev.Warning || f.Check != check) continue;
                    findings[i] = f with { Severity = Sev.Info, Message = $"accepted: {reason}" };
                    matched++;
                }

                if (matched == 0)
                    report.Warn(Stale,
                        $"no '{named}' warning is left on this record for this entry to accept. "
                        + $"Remove the entry from '{Field}'.", line);
            }
        }
    }

    /// <summary>
    /// Null for a check the catalogue declares as a warning and nowhere as an error. <c>acceptance-stale</c> is
    /// refused: an entry accepting it would keep every stale entry on the record quiet.
    /// </summary>
    private static string? Refusal(CheckId check, ILookup<CheckId, Sev> severities)
    {
        if (check == Stale)
            return $"'{Stale.Value}' cannot be accepted. Remove the entry it reports, and then this one.";
        if (!severities.Contains(check))
            return $"'{check.Value}' is not a check `kac validate` reports. `kac checks` lists every check id.";
        if (severities[check].Contains(Sev.Error))
            return $"'{check.Value}' reports errors, and '{Field}' takes a warning only. "
                   + "Fix what it reports, and remove the entry.";
        if (!severities[check].Contains(Sev.Warning))
            return $"'{check.Value}' reports info, and '{Field}' takes a warning only. Remove the entry.";
        return null;
    }
}
