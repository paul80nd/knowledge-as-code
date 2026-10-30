---
id: std-A11Y
type: standard
tier: normative
status: active
implements: [ eng:pol-A11Y.CONFORM, eng:pol-A11Y.PUBLISH, eng:pol-A11Y.UPFRONT, eng:pol-A11Y.VENDOR,
              eng:pol-A11Y.WORSE ]
verified-by: [ ctl-accessibility-unchecked, ctl-assessed-pin-tests ]
applies-to:
  - svc-docs-site
  - svc-kac
review-by: "2027-09-07"
owner: human:paul.law
tags: [ accessibility, cli, documentation, wcag ]
---

# The documentation site and `kac` are usable without sight, a mouse or colour

`Standard: std-A11Y` `ACTIVE`

## Summary

This standard covers two surfaces: the documentation site Zensical builds from `docs/`, and the `kac` command line.
[WCAG 2.2 AA] governs the site. On it a Markdown author owns heading order, link text, table structure and image
alternatives, and Zensical's theme owns contrast, focus and keyboard reach. WCAG is written for content a browser
renders, so it does not apply to a terminal. `kac` follows rules stated in the terms a terminal has:
colour repeats what the words already say, and every answer a prompt asks for is also a flag. Both components are
pinned, and each has an assessment in its tool record stating what was checked and what falls short.

## Rules

### STRUCT: A Markdown author owns the structure of a page

- A page **MUST** open on one H1.
- Heading levels **MUST** descend one at a time.
- A heading **MUST** name what sits beneath it.
- Link text **MUST** say where the link goes when the words around it are gone.
- A table **MUST** have a header row.
- A table **MUST** contain data, and never a layout.
- An image **MUST** have alternative text saying what a reader who cannot see it needs.
- A diagram **MUST** have a text equivalent stating the same information.
- That text equivalent **SHOULD** be prose following the diagram.
- A page **MUST NOT** ask a reader to act on a colour, a shape or a position.
- A change to a page **MUST** pass the conformance checklist below before it merges.

_**Covers:** `eng:pol-A11Y.CONFORM`, `eng:pol-A11Y.UPFRONT`_

### THEME: The theme owns contrast, focus and keyboard reach

- `theme:` in `mkdocs.yml` **SHOULD** use the colour schemes Zensical ships, unless the change shows that its colours
  meet [WCAG 2.2 AA].
- `palette:` **SHOULD** offer a light scheme, a dark scheme and the reader's own setting.
- `docs/assets/extra.css` **SHOULD NOT** set a colour, a focus outline or a font size, unless the change shows that
  what it sets meets [WCAG 2.2 AA].
- The site **MUST NOT** include a template override or a script that changes the theme's keyboard behaviour.
- `features:` **MUST NOT** turn on an entry that hides content behind a pointer gesture.

_**Covers:** `eng:pol-A11Y.CONFORM`_

### COLOUR: `kac` carries no meaning in colour alone

- Every message **MUST** state its meaning in words.
- Colour **MUST** repeat what the words already say.
- Every verb **MUST** honour `NO_COLOR` in the environment and `--no-color` on the command line.
- Output **MUST** stay complete when colour is off.
- Output **MUST NOT** separate one column from the next with a box-drawing character or an icon.
- Every answer a prompt asks for **MUST** also be available as a flag.
- A run with no terminal **MUST** exit rather than wait for a keystroke.
- A message a pipeline parses **MUST** go to `--json`, or to the CI platform's annotation or pull request comment, and
  never through a renderer.

_**Covers:** `eng:pol-A11Y.UPFRONT`_

### ASSESS: A pinned component carries an accessibility assessment

- A third-party component rendering either surface **MUST** have an assessment in its tool record.
- The assessment **MUST** name what was checked, and how.
- The assessment **MUST** name what falls short, or say that nothing found does.
- The assessment **MUST** name the version it was made against.
- A pull request moving a pin to a new major or minor version **MUST** include a new assessment.
- A pull request moving a pin below 0.1.0 to any new version **MUST** include a new assessment, because below 0.1.0
  every release may change behaviour.
- We **MUST NOT** adopt a component the assessment finds short without a recorded deviation.

_**Covers:** `eng:pol-A11Y.VENDOR`_

### GAPS: What falls short is written down where a reader finds it

- A shortfall an assessment recorded **MUST** sit in the tool record for the component it was found in.
- A tool record **MUST NOT** claim a conformance level nobody verified.
- A correction **MUST** take the shortfall out of the record in the commit that corrects it.
- This project **MUST** publish an accessibility statement where a law or a contract requires one.
- A published statement **MUST** name every shortfall the tool records list.

_**Covers:** `eng:pol-A11Y.PUBLISH`_

### WORSE: A change that makes either surface worse needs a deviation

- A change knowingly reducing accessibility **MUST NOT** merge without a recorded deviation.
- The deviation **MUST** name the individual accepting the risk.
- The deviation **MUST** state what compensates for the loss.
- The deviation **MUST** state the date somebody looks at it again.
- The deviation **MUST** be a record in this corpus's deviations register.

_**Covers:** `eng:pol-A11Y.WORSE`_

## Examples

```
✅ Good
## The order the commands run in

A square box is a command you type. A rounded one is what you do or what you get.

(the Mermaid diagram)

**The loop is the part you live in.** Write a record, run [`validate`](validate.md), run
[`generate`](generate.md), write the next one.

| Command    | What it does                           |
|------------|----------------------------------------|
| `validate` | Check every record against the schema. |
| `generate` | Rebuild what the corpus derives.       |

❌ Avoid
## The order the commands run in

The diagram below shows it.

(the Mermaid diagram)

The green boxes are the ones you run. See [here](validate.md) for more.

| `validate` | `generate` |
|------------|------------|
| Check every record against the schema. | Rebuild what the corpus derives. |
```

The avoided page states the route only in the picture, so a reader who gets no picture gets no route. "The green
boxes" names a colour the diagram does not use, and a reader who cannot see it has no other cue. "See here" tells a
reader listing the links on the page nothing about where any of them goes. The avoided table puts both commands in the
header row, so a screen reader announces "validate" as the column heading over the sentence describing it.

## Conformance checklist

- [ ] The page opens on one H1, and every heading below it descends one level at a time.
- [ ] Every link says where it goes without the sentence around it.
- [ ] Every table has a header row and contains data.
- [ ] Every image has alternative text, and every diagram has a text equivalent stating the same information.
- [ ] No instruction on the page depends on a colour, a shape or a position.
- [ ] A new `kac` message says in words what its colour says.
- [ ] A new `kac` prompt has a flag that supplies its answer.
- [ ] A pin that moved has an assessment against the version it moved to.
- [ ] A shortfall this change introduced has a deviation, an owner and a review date.

## Rationale and provenance

`eng:pol-A11Y` applies to this repository in full, and no standard sat beneath any of its clauses. Its binding clauses
apply to two surfaces: the documentation site and the command line. The two surfaces divide differently, so each takes
its own rules.

**The site divides between the author and the theme.** A Markdown page contributes headings, links, tables, images and
words. Everything a browser needs beyond that comes from Zensical's theme: the contrast of the two schemes, the
focus outline, the skip link and the keyboard order of the navigation. An author cannot fix what the theme gets wrong,
and the theme cannot fix a heading that jumps from H2 to H4. One set of rules over both would leave each reviewer
guessing which half applied to them.

**The command line is in scope, and WCAG is not the measure.** WCAG 2.2 AA is written for content a user agent renders,
and its success criteria address markup, contrast ratios and pointer targets. A terminal has none of those, so judging
`kac` against it would produce a verdict about nothing. `kac` still needs rules of its own. A screen reader reads a
terminal, and a message whose severity is shown only by the colour red arrives at that reader with no severity at all.
So the rules above are stated in the terms a terminal has. `Commands.Severity` already pairs every colour with a word,
`Out.NoColor` strips colour for `NO_COLOR` and `--no-color` alike, and `NewSettings` and `UpdateSettings` give a flag
for every prompt their verbs draw. This standard writes down what the code already does, so the next verb keeps doing
it.

**Nothing in CI checks any of this, except the pins.** [ctl-assessed-pin-tests] fails a pull request where an assessed
component's pin leaves the range its tool record approves, or where the assessment names no version at that pin. The
`docs` job runs `zensical build`, and `strict: true` checks links inside the site. No check reads the output for
contrast, heading order, link text or keyboard reach. No test reads a `kac` message for a colour standing alone. Every
other rule above is a reviewer's, which is what the conformance checklist is for. [ctl-accessibility-unchecked] records
what goes unchecked.

**A deviation goes in this corpus's own register.** `eng:pol-DEVI` leaves the location of a deviation to the process,
not to the policy. This corpus adopts the deviations type, so a departure from a rule above is a record in
[deviations](../deviations.md). The tool record for the component keeps the assessment that found the shortfall.

## Sources and further reading

- **Normative.** [WCAG 2.2 AA] sets what a page on the documentation site is measured against. Nobody can apply the
  first rule above without it.
- **Informative.** [Zensical's setup pages] document the `palette:` and `features:` keys the second rule constrains in
  `mkdocs.yml`.

## Changelog

- 2026-10-29: a pin below 0.1.0 needs a new assessment at every version it moves to, because every release there may
  change behaviour.
- 2026-10-25: a message a pipeline parses may also go to the CI platform's annotation or pull request comment, which
  `kac validate` writes under CI for the platform to read.
- 2026-10-25: Zensical builds the site, so the theme whose colour schemes a change should keep is Zensical's.
- 2026-10-19: a check now fails a pull request that moves an assessed pin without a new assessment, so the rationale
  names the pins as the one part of this standard CI checks.
- 2026-10-18: a diagram has a text equivalent, which should be the prose after it. The theme's colour schemes and an
  `extra.css` setting no colour become `SHOULD`, and a check against WCAG 2.2 AA stands in for either. The three-way
  `palette:` becomes `SHOULD`.
- 2026-09-07: sent a deviation to the register this corpus now holds, rather than to the record for the thing that
  changed.
- 2026-09-07: initial version.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[Zensical's setup pages]: https://zensical.org/docs/setup/colors/
[ctl-accessibility-unchecked]: ../controls/accessibility-unchecked.md
[ctl-assessed-pin-tests]: ../controls/assessed-pin-tests.md
