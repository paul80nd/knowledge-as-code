namespace kac.core;

public sealed class BindsOnlyUnderRules : IDocumentRule
{
    public RuleId RuleId => new("binds-only-under-rules");

    private static readonly CheckId Reports = new("modal-outside-rules");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        if (ctx.Type.Parts is not { Binding.Count: > 0 } spec) return;

        foreach (var (modal, text, line) in Strays(ctx.Doc, spec))
            ctx.Report.Warn(Reports,
                $"this bullet names '{modal}' outside `{spec.Section}`: \"{Md.Snippet(text)}\". No citation "
                + $"reaches a {spec.Noun} written here. Move the bullet under a `{spec.Section}` heading, or write "
                + "the keyword in backticks where the bullet only names one.",
                line);
    }

    /// <summary>
    /// Top-level lists only. A nested list is one bullet's workings, and a list inside a block quote quotes somebody
    /// else's rule.
    /// </summary>
    private static IEnumerable<(string Modal, string Text, int Line)> Strays(Doc d, PartSpec spec)
    {
        foreach (var (list, section) in Md.Lists(d.Ast))
        {
            if (string.Equals(section, spec.Section, StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var (text, plain, bold, line) in Md.Bullets(list))
            {
                // The plain reading first, as `part-modal` asks it. A bold run is asked what modal it names, not held
                // equal to one, so a bullet bolding a whole sentence is still caught.
                var modal = spec.ModalNamed(plain)
                            ?? bold.Select(spec.ModalNamed).FirstOrDefault(m => m is not null);

                if (modal is not null) yield return (modal, text, line);
            }
        }
    }
}
