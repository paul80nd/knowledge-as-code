# Contributing

How knowledge is added, reviewed, and promoted. This applies equally to people and to AI sessions — the rules are the
same regardless of who is holding the keyboard.

Four pages carry what a contribution needs, and a type's `_template.md` sends you here rather than repeating them.
[Taxonomy](taxonomy.md) says where a document goes. [Metadata](metadata.md) covers the frontmatter.
[Style](style.md) holds the rules for the words, which are the same in every document, comment and commit message.
[Authoring](authoring.md) holds what a document's tier adds on top.

## The shape of a contribution

1. Work out where it goes — [taxonomy](taxonomy.md) has a decision table.
2. Copy the type's `_template.md`. It marks the parts you supply as `{{placeholder}}` and fences its own guidance
   between `DELETE FROM HERE` and `DELETE TO HERE` comments; a finished document has neither left in it.
3. Allocate an ID in the style that type uses — the next unused number, a four-character mnemonic for the concept, or a
   slug. Check the folder's index for what is already taken; see [metadata](metadata.md#ids).
4. Fill in the frontmatter — see [metadata](metadata.md).
5. Write the content, to [style](style.md) and [authoring](authoring.md). Follow the template's section structure, which
   exists so documents of a type are comparable. The tier rules are why a runbook step and an ADR paragraph are held to
   different constraints.
6. Open a PR against the corpus repository. Review expectations follow the tier, below.

Generated content — indexes, digests, reports — is not edited by hand. If an index looks wrong, the frontmatter it was
built from is wrong.

### Writing a template

`{{…}}` marks everything the author supplies, and nothing else does — not `NNNN`, not `XXXX`, not a slug called
`example`. One mark, so that a template teaches exactly what the tool recognises. The casing carries what a sentence
would otherwise have to: `pol-{{MNEM}}` in `{{mnem}}-kebab-slug.md` says a mnemonic is upper-case in the id and
lower-case in the filename. `{{a}}` and `{{b}}` stand for *another* document, and `{{a}}.md` is its whole filename.

CI checks templates — `template-fields`, and everything a copy inherits — so the mark has to survive YAML, and in two
places it does not:

* **A placeholder cannot sit in a flow sequence.** `related: [ adr-{{a}} ]` is a parse error, because a plain scalar in
  flow context may not contain a brace. Write the list as a block sequence.
* **A placeholder opening a value has to be quoted.** `review-by: {{date}}` is read as a flow mapping rather than as
  text, so the field arrives holding nothing. Write `review-by: "{{date}}"`. A placeholder that follows something —
  `svc-{{slug}}` — needs no quotes.

### Links

One rule applies only to the documents under `knowledge-as-code/`, and to `knowledge-as-code.md` above them: they **name
a type and never link to one**. Every corpus running the framework holds the same copy, so a link that resolves here
says nothing about where it is read — and a link into a type's folder is worse still, since the records it points at are
the first thing a corpus deletes. Where a link is genuinely wanted, it belongs in a generated block, which is written
from the types the corpus actually stood up. `framework-names-types` enforces this.

**References to another document by its id use shortcut reference links** — the label is the id and doubles as the
display text:

    New headers are governed by [adr-0013].

    [adr-0013]: 0013-http-custom-header-naming.md

**References with prose link text use inline links**, since the display text differs from the target:

    The rule lives in the [value-formats standard](/standards/public-api/value-formats.md).

Definitions go at the very foot of the document, after all prose sections, sorted by label. Where a
`## Related` section exists it uses the same shortcut labels, so a path appears exactly once per document and a rename
is a one-line change.

**The label is the id exactly as that document carries it** — `adr-0013`, `pol-DEVI`, `svc-billing-api`. The prefix is
always lower-case; what follows takes the type's own form, so a mnemonic stays upper-case and a slug stays lower-case.
The label is its own display text, so a label that is not the id shows the reader an id that does not exist.

CI enforces this with `label-canonical`. It matters because reference and definition are matched case-insensitively:
`[ADR-0013]` resolves perfectly happily, so nothing else would ever catch it. Reconciliation against the `related:`
frontmatter is likewise case-insensitive.

CI also fails on an undefined label or an unused definition, and ignores fenced and indented code blocks.

## How do I contribute…?

Type-specific steps live with the type: each type's own page says what it holds, what it is not, and what adding one
asks of you. [Taxonomy](taxonomy.md) has the decision table if you are not sure which you need, and the index at the
repository root lists what this corpus actually holds.

The review model below applies to all of them.

## Review by tier

The review bar follows what a document *is*, not who wrote it.

| Tier            | Review required                    | Merge criteria                                                                   |
|-----------------|------------------------------------|----------------------------------------------------------------------------------|
| **Decided**     | Two reviewers                      | Alternatives genuinely weighed; consequences stated including the unwelcome ones |
| **Normative**   | The document's owner               | Rules are testable; RFC 2119 keywords used correctly; changelog updated          |
| **Descriptive** | One reviewer                       | Cross-references resolve; content matches the estate as it actually is           |
| **Procedural**  | One reviewer who has done the task | Steps are followable by someone who hasn't; rollback stated                      |
| **Observed**    | None                               | Merges on CI passing. Authority comes at promotion, not capture.                 |

Two consequences worth being explicit about:

**Observed content is unreviewed by design.** A discovery is cheap because nobody gates it. That is the point — a
capture step with a review attached is a capture step that doesn't happen. The tier's low authority is what makes the
low bar safe.

**Decided content is immutable after merge.** Not "discouraged from changing" — immutable. Corrections are limited to
typos and status transitions. To change a decision, write a new one that supersedes it.

## For AI sessions

If you are an agent contributing to this wiki:

* **Capture as discoveries, not FAQs.** You cannot confirm your own observations. `source: session`,
  `confidence: unverified`.
* **Never edit a Decided-tier document.** Propose a superseding one.
* **Never hand-edit generated regions** — anything between `<!-- BEGIN GENERATED -->` and `<!-- END GENERATED -->`.
  Change the source and let CI rebuild.
* **Cite, don't restate.** If the reasoning lives in an ADR, link it. Duplication is how this corpus rots.
* **Use the glossary's terms exactly.** Where the glossary distinguishes two near-synonyms, the distinction is
  load-bearing; treating them as interchangeable produces work that is plausible and subtly wrong.
* **When unsure of placement, ask.** A well-written document in the wrong folder is a cost, not a contribution.

## What not to add

* Content duplicated from an ADO work item — link instead.
* Anything containing secrets, connection strings, tokens or customer data. This wiki is broadly readable.
* Raw session logs. Distilled discoveries only.
* Speculative documentation for work not yet started — that's what the backlog is for.
* A document that fits no type. Raise the gap instead.

## Branches and review

Trunk-based. Short-lived branches, PR into `main`, wiki publishes from `main`.

**Branch policy on `main`**

* Minimum one reviewer.
* Build validation required — schema, links and generated-content freshness must pass.
* Path-scoped automatically-included reviewers raise the bar on Decided and Normative content (`adrs/*`, `standards/*`,
  `policies/*`) without raising it everywhere. This is how the tier model is enforced in practice: ADO's
  minimum-reviewer count is branch-level, but required reviewers can be scoped to paths.
* `discoveries/*` has no path rule and merges on a green build.

**CI does not commit.** If generated content is stale the build fails and tells you which command to run locally. A
pipeline that pushes fixes into the PR branch produces bot commits, re-triggers itself, and makes "who changed this"
unanswerable — a bad trade for a repository whose value is a trustworthy history.

**[The dreamer](automation.md#scheduled-tasks) has its own identity** — a service account that can open pull requests
and cannot merge them. That is the technical form of "proposes, never commits".
