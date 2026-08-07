---
id: adr-NNNN
tier: decided
status: proposed
decided-on:
owner:
related: [ adr-AAAA, adr-BBBB, adr-CCCC ]
---

# {{Title}}

`ADR: adr-NNNN` `PROPOSED`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`id`** — Matches the filename number. Four digits, never reused.
* **`tier`** — Always `decided` for an ADR.
* **`status`** — `proposed` while under review, then `accepted`, and later `deprecated` or `superseded`. Values are
  lowercase.
* **`decided-on`** — The acceptance date, quoted `"YYYY-MM-DD"`. Leave the key bare until accepted.
* **`owner`** — The named person answerable for the decision, never a team alias.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

> **In the context of** {{use case}}, **facing** {{concern}}, **we decided** {{chosen option}}, **rather than**
> {{alternatives}}, **to achieve** {{quality / benefit}}, **accepting** {{downside}}.

_(One tight Y-statement, keeping all six moves. Make every clause a phrase, not a sentence, and name rejected options in
a few words — the full forces, alternatives, and consequences live in the sections below, so the summary points at them
rather than restating them. Aim for one breath, ~35–45 words.)_

## Context

What is the situation that prompted this decision? What constraints, drivers, or forces are at play? Write this so a
reader joining the project in two years can understand *why* this decision was even on the table.

Keep it factual and free of advocacy — the decision itself comes next.

## Decision

What did we decide? State it clearly and in the active voice ("We will use X for Y."). Keep it to one or two short
paragraphs.

## Alternatives Considered

* **{{Option name}}** — rejected: why.
* **{{Option name}}** — rejected: why.

Include the options that were genuinely weighed, not every theoretical option. One or two sentences per option on why it
lost out. If there were no real alternatives ("we use Azure because we are on Azure"), say so plainly rather than
inventing options.

## Consequences

What changes as a result of this decision? Include both intended outcomes and trade-offs that downstream teams or future
readers need to know about. Cover at least:

* New constraints on services or contributors.
* Operational implications (cost, monitoring, on-call burden).
* Dependencies introduced or removed.
* Anything that becomes harder, not just things that become easier.

## Related

* [adr-NNNN] — {{what it decided, and why it matters here}}.
* [adr-NNNN] — <…>.

_(One line each on **how** the two relate — the annotation is the value, not the link. Cite rather than restate: if
another ADR already made an argument, link it instead of repeating it.)_

## References

* [RFC NNNN](https://www.rfc-editor.org/rfc/rfcNNNN) — {{what it covers}}.

_(External sources only, as inline links — they are unaffected by renames. Omit the section if there are none.)_

[adr-NNNN]: nnnn-kebab-slug.md
[adr-NNNN]: nnnn-kebab-slug.md

_(Link definitions, at the very foot, sorted by label. Internal references use **shortcut reference links** — write
`[adr-0007]` in the prose and define it once here, so a rename is a one-line change. The label is also the display text,
so it must be the canonical id; where you want prose link text instead, use an inline link. Filename slugs are at most
30 characters.)_
