---
id: std-PROSE
type: standard
tier: normative
status: active
implements: [ eng:pol-KNOW.AGENTS ]
applies-to:
  - all
review-by: "2027-09-02"
owner: human:paul.law
tags: [ documentation, prose, writing ]
---

# Prose reads the same way whoever wrote it

`Standard: std-PROSE` `ACTIVE`

## Summary

Every word this repository publishes follows the Microsoft Writing Style Guide, with the changes below. That covers
records, README files, code comments, schema descriptions, documentation pages, commit messages and pull request
bodies. The writing skills under `.claude/skills/` state the same rules for an agent. Each surface then adds its own
shape on top of them.

## Rules

### The base style

- Prose **MUST** follow the Microsoft Writing Style Guide, except where a clause below changes it.
- A sentence **MUST** state the rule or the fact before the reason.
- A verb **MUST** be literal. A command *lists*, *checks*, *writes*, *reports* or *rejects*. *Hold to*, *carry*,
  *name*, *answer*, *reach* and *seal* **MUST NOT** stand in for one. *Holds* has four plain replacements: *has*,
  *contains*, *keeps* and *checks*. *Carries* has *has*, *contains*, *includes* and *states*.
- A word on that list **MAY** appear where it is itself the plain verb. A heading *names* a topic, a standard *cites*
  a clause, and a service *returns* what a caller asked for.
- A sentence **MUST** state one idea, and an instruction **MUST** give one step.
- A heading **MUST** name its topic, in sentence case.
- Three or more parallel items **MUST** be a list or a table.

_**Covers:** `eng:pol-KNOW.AGENTS`_

### Where this repository differs from Microsoft

- Spelling **MUST** be British.
- An em dash **MUST NOT** appear. A full stop, a comma or a colon replaces it.
- Prose **MUST** wrap at 120 columns, counted in characters. A table row and a link definition **MUST NOT** wrap.
- *We* **MUST** appear only in a commitment in a policy or a standard. Everywhere else the text **MUST** say *you*, or
  name the tool, the framework or the corpus.
- A framework term (corpus, record, type, tier, layer, export, plugin) **MUST** have a gloss on its first use on each
  page.
- A figure of speech **MUST NOT** appear.
- A claim **MUST** give at most one reason.
- A contrast (*rather than*, *instead of*) **MUST** appear only where the reader already holds the wrong idea. A
  contrast inside a clause **MUST** stay, because it names what the clause forbids.
- One thing **MUST** have one name everywhere.

### What prose must not change

- You **MUST** leave alone an identifier, a path, a flag, a command, and the output a command printed.
- You **MUST** leave alone a heading you may not rename, the H1 of a record, and every clause id.
- You **MAY** reword a clause where the obligation is identical: the same subject, the same modal, the same threshold,
  the same list, the same exception and the same cross-reference.
- Rewording a clause **MUST** reword every quotation of it in the same change. `clause-quoted-faithfully` reports a
  quotation that no longer matches.
- You **MUST** leave alone the span between a `BEGIN GENERATED` marker and its `END GENERATED`. To move it, change the
  schema or the frontmatter and run `kac generate`.
- Where this standard and `.schema/` disagree, the schema **MUST** win, and you **MUST** report the contradiction.
- You **MUST** name in your reply every rule you left behind on one of these grounds.

### What stays true

- Prose **MUST** describe what exists today. Agreed but unbuilt work **MUST** go to the issue tracker.
- A schema rule the tool does not implement **MUST** be written as declared and not running.
- A count **MUST** name the command or the test that reports it, unless a decision fixes the set.
- Reasoning that lives elsewhere **MUST** be cited, not restated.
- You **MUST** rewrite the whole block when you add a fact to existing prose.

### Commit messages and pull requests

- A subject line **MUST** say what changed, in the imperative, with no full stop.
- The body **MUST** say why. It is the one place that may describe what used to be true.
- A pull request body **MUST** give the reason and the evidence, and **MUST NOT** retell the diff.

## Examples

```
✅ Good
`validate` reports a missing marker.
Only an error fails the build.
`pack` zips `.dist/export/` into a `.nupkg` under `.dist/package/`.
Two separators extend an id. `.` selects a part. `:` selects a corpus.
To delete the document, click Delete.

❌ Avoid
`validate` holds a corpus to carrying the markers this writes between.
Failing rather than warning is the whole of the trade.
A directory is not something another repository can depend on.
Two separators reach past an id, each with one job.
Click Delete to remove the document.
```

Each line under ❌ Avoid makes the reader read it twice.

- A figurative verb hides the action.
- A generalisation about the design says nothing the reader can act on.
- A sentence saying what a thing is not never says what it is.
- A step written before its purpose is acted on before the purpose is read.

## Conformance checklist

- [ ] Every verb is literal. No "holds", "carries", "names", "answers", "reaches" or "seals" replaces a plain verb.
- [ ] Every section opens on the rule or the fact, not the reason.
- [ ] Every contrast left standing is one a reader needs, and every clause kept the one it had.
- [ ] Every heading names a topic.
- [ ] No em dash survives. Spelling is British.
- [ ] Every line of prose outside a table or a link definition is under 120 characters.
- [ ] No hand edit sits inside a generated block.
- [ ] Every count names what reports it.
- [ ] Every rule left behind is named in the reply.

## Rationale and provenance

Markdown outnumbers code in this repository, so how a sentence reads is a property of the product. The Microsoft
Writing Style Guide is the base because it is public, complete, and known in depth by the models that write here. A
short list of local changes works better than a long local restatement, because a model imitates a known guide more
closely than it follows a rulebook.

The writing skills under `.claude/skills/` state the same rules for an agent and cite no record, so a skill works in a
checkout with no corpus in it. This standard is the record the estate cites.

## Sources and further reading

- **Normative.** [Microsoft Writing Style Guide] is the base style.
- **Normative.** [RFC 8174] gives an RFC 2119 keyword its meaning only in capitals.

## Changelog

- 2026-09-11: replaced the restated rulebook with the Microsoft Writing Style Guide as the base, plus the local changes.
- 2026-09-06: took the wrapping, generated-block, write-what-exists and schema-precedence rules that `CLAUDE.md` had
  been stating a second time.
- 2026-09-02: initial version.

[Microsoft Writing Style Guide]: https://learn.microsoft.com/style-guide/welcome/
[RFC 8174]: https://www.rfc-editor.org/rfc/rfc8174
