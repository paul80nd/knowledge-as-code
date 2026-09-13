---
id: std-A11Y
type: standard
tier: normative
status: active
implements: [ eng:pol-A11Y.CONFORM, eng:pol-A11Y.PUBLISH, eng:pol-A11Y.UPFRONT, eng:pol-A11Y.VENDOR,
              eng:pol-A11Y.WORSE ]
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

This standard covers two surfaces: the documentation site MkDocs builds from `docs/`, and the `kac` command line.
[WCAG 2.2 AA] governs the site. On it a Markdown author owns heading order, link text, table structure and image
alternatives, and Material for MkDocs owns contrast, focus and keyboard reach. WCAG is written for content a browser
renders, so it does not apply to a terminal. `kac` follows rules stated in the terms a terminal has:
colour repeats what the words already say, and every answer a prompt asks for is also a flag. Both components are
pinned, and each has an assessment in its tool record stating what was checked and what falls short.

## Rules

### A Markdown author owns the structure of a page

- A page **MUST** open on one H1.
- Heading levels **MUST** descend one at a time.
- A heading **MUST** name what sits beneath it.
- Link text **MUST** say where the link goes when the words around it are gone.
- A table **MUST** have a header row.
- A table **MUST** contain data, and never a layout.
- An image **MUST** have alternative text saying what a reader who cannot see it needs.
- A diagram **MUST** be followed by prose stating the same information.
- A page **MUST NOT** ask a reader to act on a colour, a shape or a position.
- A change to a page **MUST** pass the conformance checklist below before it merges.

_**Covers:** `eng:pol-A11Y.CONFORM`, `eng:pol-A11Y.UPFRONT`_

### The theme owns contrast, focus and keyboard reach

- `theme:` in `mkdocs.yml` **MUST** use the colour schemes Material for MkDocs ships.
- `palette:` **MUST** offer a light scheme, a dark scheme and the reader's own setting.
- `docs/assets/extra.css` **MUST NOT** set a colour, a focus outline or a font size.
- The site **MUST NOT** include a template override or a script that changes the theme's keyboard behaviour.
- `features:` **MUST NOT** turn on an entry that hides content behind a pointer gesture.

_**Covers:** `eng:pol-A11Y.CONFORM`_

### `kac` carries no meaning in colour alone

- Every message **MUST** state its meaning in words.
- Colour **MUST** repeat what the words already say.
- Every verb **MUST** honour `NO_COLOR` in the environment and `--no-color` on the command line.
- Output **MUST** stay complete when colour is off.
- Output **MUST NOT** separate one column from the next with a box-drawing character or an icon.
- Every answer a prompt asks for **MUST** also be available as a flag.
- A run with no terminal **MUST** exit rather than wait for a keystroke.
- A message a pipeline parses **MUST** go to `--json`, and never through a renderer.

_**Covers:** `eng:pol-A11Y.UPFRONT`_

### A pinned component carries an accessibility assessment

- A third-party component rendering either surface **MUST** have an assessment in its tool record.
- The assessment **MUST** name what was checked, and how.
- The assessment **MUST** name what falls short, or say that nothing found does.
- The assessment **MUST** name the version it was made against.
- A pull request moving a pin to a new major or minor version **MUST** include a new assessment.
- We **MUST NOT** adopt a component the assessment finds short without a recorded deviation.

_**Covers:** `eng:pol-A11Y.VENDOR`_

### What falls short is written down where a reader finds it

- A shortfall an assessment recorded **MUST** sit in the tool record for the component it was found in.
- A tool record **MUST NOT** claim a conformance level nobody verified.
- A correction **MUST** take the shortfall out of the record in the commit that corrects it.
- This project **MUST** publish an accessibility statement where a law or a contract requires one.
- A published statement **MUST** name every shortfall the tool records list.

_**Covers:** `eng:pol-A11Y.PUBLISH`_

### A change that makes either surface worse needs a deviation

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
- [ ] Every image has alternative text, and every diagram has prose beside it stating the same information.
- [ ] No instruction on the page depends on a colour, a shape or a position.
- [ ] `mkdocs.yml` and `docs/assets/extra.css` set no colour, focus outline or font size of their own.
- [ ] A new `kac` message says in words what its colour says.
- [ ] A new `kac` prompt has a flag that supplies its answer.
- [ ] A pin that moved has an assessment against the version it moved to.
- [ ] A shortfall this change introduced has a deviation, an owner and a review date.

## Rationale and provenance

`eng:pol-A11Y` applies to this repository in full, and no standard sat beneath any of its clauses. Its binding clauses
apply to two surfaces: the documentation site and the command line. The two surfaces divide differently, so each takes
its own rules.

**The site divides between the author and the theme.** A Markdown page contributes headings, links, tables, images and
words. Everything a browser needs beyond that comes from Material for MkDocs: the contrast of the two schemes, the
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

**Nothing in CI checks any of this.** The `docs` job runs `mkdocs build`, and `strict: true` reads links inside the
site. No check reads the output for contrast, heading order, link text or keyboard reach. No test reads a `kac`
message for a colour standing alone. Every rule above is a reviewer's, which is what the conformance checklist is for.
A rule nobody checks still binds whoever reads it, and a `verified-by` naming a control that does not exist would
claim a check CI never runs.

**A deviation goes in this corpus's own register.** `eng:pol-DEVI` leaves the location of a deviation to the process,
not to the policy. This corpus adopts the deviations type, so a departure from a rule above is a record in
[deviations](../deviations.md). The tool record for the component keeps the assessment that found the shortfall.

## Sources and further reading

- **Normative.** [WCAG 2.2 AA] sets what a page on the documentation site is measured against. Nobody can apply the
  first rule above without it.
- **Informative.** [Material for MkDocs] documents the `palette:` and `features:` keys the second rule constrains in
  `mkdocs.yml`.

## Changelog

- 2026-09-07: sent a deviation to the register this corpus now holds, rather than to the record for the thing that
  changed.
- 2026-09-07: initial version.

[Material for MkDocs]: https://squidfunk.github.io/mkdocs-material/setup/
[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
