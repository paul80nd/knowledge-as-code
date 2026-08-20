# Contributing

How knowledge is added, reviewed, and promoted. This applies equally to people and to AI sessions — the rules are the
same regardless of who is holding the keyboard.

Four pages carry what a contribution needs, and a type's `_template.md` sends you here rather than repeating them.
[Taxonomy](taxonomy.md) says where a document goes. [Metadata](metadata.md) covers the frontmatter.
[Style](style.md) holds the rules for the words, which are the same in every document, comment and commit message.
[Authoring](authoring.md) holds what a document's tier adds on top. This page holds the rest: the link and template
conventions CI enforces, the review model, and what outranks what when two rules disagree.

## What outranks what

Four sources of rules, in this order.

1. **The schema and the validator.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report. These are
   executable, so they are the authority on anything mechanical: required sections, clause modals, id and filename
   formats, link forms, and which text rules a type actually declares.
2. **The type's own pages.** `<type>.md` for what the type holds, `<type>/_template.md` for the sections it must have.
3. **This page**, for the conventions below.
4. **[Style](style.md) and [authoring](authoring.md)**, for the prose.

**Where a prose rule contradicts the schema, that is a finding to report and never an instruction to act on.** Say which
of the two is wrong and leave both alone. Read literally, one wrong bullet in a rulebook can strip a keyword from every
normative document in the corpus while claiming the rulebook's authority.

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

One rule applies only to the documents under `knowledge-as-code/` and to `knowledge-as-code.md` above them: they **name
a type and never link to one**. Every corpus holds the same copy of those files, and a corpus that never adopted
standards has no `/standards` page to open. A link into a type's folder is worse again: the records it points at are the
first thing a corpus deletes.

Where a link is genuinely wanted, put it in a generated block. Those are written from the types the corpus adopted, so
they can only name pages that exist. `framework-names-types` holds you to this.

**References to another document by its id use shortcut reference links** — the label is the id and doubles as the
display text:

    New headers are governed by [adr-0013].

    [adr-0013]: 0013-http-custom-header-naming.md

**A reference to a part of a document names the part** — a clause of a policy, a term of a glossary — as
`<id>.<part>`, which is the form a citation already uses. The label carries it and the target lands on it:

    A title in the catalogue is not the indexed field — see [gls-search.title].

    [gls-search.title]: search.md#title

Linking to the file instead lands a reader at the top of the document, to find the part for themselves. It also loses
the reference: a tool reading the corpus carries what the link states, and a link naming no part states none.

**References with prose link text use inline links**, since the display text differs from the target:

    The rule lives in the [value-formats standard](/standards/public-api/value-formats.md).

Definitions go at the very foot of the document, after all prose sections, sorted by label. Where a
`## Related` section exists it uses the same shortcut labels, so a path appears exactly once per document and a rename
is a one-line change.

**The label is the id exactly as that document carries it** — `adr-0013`, `pol-DEVI`, `svc-billing-api`. The prefix is
always lower-case; what follows takes the type's own form, so a mnemonic stays upper-case and a slug stays lower-case. A
part id is the record's own id, a dot, and the part as its type writes one: `pol-DEVI.TIMEBOX`, `gls-search.title`. The
label is its own display text, so a label that is not the id shows the reader an id that does not exist.

CI enforces this with `label-canonical`. It matters because reference and definition are matched case-insensitively:
`[ADR-0013]` resolves perfectly happily, so nothing else would ever catch it. Reconciliation against the `related:`
frontmatter is likewise case-insensitive.

CI also fails on an undefined label or an unused definition, and ignores fenced and indented code blocks.

## How do I contribute…?

Type-specific steps live with the type. Each type's page says what that type holds, what it is not, and what it asks of
you when you add one. If you are not sure which type you need, [taxonomy](taxonomy.md) has the decision table.

The review model below applies to every type.

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

If you are an agent contributing to this corpus:

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
* Anything containing secrets, connection strings, tokens or customer data. This corpus is broadly readable.
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

**An agent proposing knowledge has its own identity** — a service account that can open pull requests and cannot merge
them. A human accepts what it proposes, and the branch policy is what makes that true rather than a convention anyone
has to remember.
