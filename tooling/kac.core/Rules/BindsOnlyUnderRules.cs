using Markdig.Syntax;

namespace kac.core;

// Only the parts section carries obligations. An obligation written anywhere else is addressed by
// nothing, so a control citing it, an export carrying it and a reader following the citation all reach
// past it. The author reads the rule on the page and never learns it landed where no citation arrives.
//
// Bullets only, and never a paragraph. A rule has a shape the template teaches: a bullet, subject
// first, with the modal in bold capitals. That shape is the evidence the author meant a rule, and a
// sentence discussing a keyword is not one. `part-modal` demands the same shape inside the section.
//
// A modal counts written bold, as the corpus writes one, and written plain in capitals, as BCP 14
// does: the two are the same intention, and `part-modal` reads both for the same reason. Both the
// binding modals and the advisory ones, because a `SHOULD` outside the section is as unreachable as a
// `MUST`. So the message says the bullet names a modal, and never that it binds: half the levels a
// type declares recommend rather than oblige, and `clause-modal` holds a table's rows to that reading.
//
// The message quotes the bullet, which is why this stays in C#. One fixed string could say that a
// modal sits outside the section and never say which line carries it. It offers backticks as the
// second fix, because the bullet that trips this most often names a keyword rather than using one: a
// changelog entry recording a modal that moved, a checklist item restating the rule it ticks.
//
// Reported as a warning, per bullet. A bullet outside the section is inferred to be a misplaced rule,
// and an `Examples` section bulleting the rule it demonstrates makes the inference wrong. A warning
// asks the author to look; an error would claim more than the rule knows.
public sealed class BindsOnlyUnderRules : IDocumentRule
{
    public RuleId RuleId => new("binds-only-under-rules");

    private static readonly CheckId Reports = new("modal-outside-rules");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        // The section is read from the type rather than written here, so a second type adopting this
        // rule is a line in its schema. A type declaring no modals declares no way for a bullet to
        // bind, and a glossary sourcing headings is asked none of this.
        if (ctx.Type.Parts is not { Binding.Count: > 0 } spec) return;

        foreach (var (modal, text, line) in Strays(ctx.Doc, spec))
            ctx.Report.Warn(Reports,
                $"this bullet names '{modal}' outside `{spec.Section}`: \"{Md.Snippet(text)}\". No citation "
                + $"reaches a {spec.Noun} written here. Move the bullet under a `{spec.Section}` heading, or write "
                + "the keyword in backticks where the bullet only names one.",
                line);
    }

    // The bullets of every list outside the parts section, each with the modal it names. Top-level
    // lists only, which draws two boundaries. A nested list is one bullet's workings, the boundary
    // `Md.Bullets` already keeps under a part heading. A list inside a block quote is somebody else's
    // words, and quoting a rule is naming one.
    private static IEnumerable<(string Modal, string Text, int Line)> Strays(Doc d, PartSpec spec)
    {
        var inSection = false;

        foreach (var block in d.Ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), spec.Section, StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
                continue;
            }

            if (inSection || block is not ListBlock list) continue;

            foreach (var (text, plain, bold, line) in Md.Bullets(list))
            {
                // The plain reading is asked first for the reason `part-modal` asks it first: a bullet
                // with an unbolded keyword names the modal there, and the bold runs beside it do not.
                //
                // A bold run is asked what modal it names, rather than held equal to one. `part-modal`
                // can hold it equal because a bullet bolding a whole sentence is reported there for
                // having no modal at all. Here that bullet is the misplaced rule this looks for, and
                // an equality test would pass over it.
                var modal = spec.ModalNamed(plain)
                            ?? bold.Select(spec.ModalNamed).FirstOrDefault(m => m is not null);

                if (modal is not null) yield return (modal, text, line);
            }
        }
    }
}
