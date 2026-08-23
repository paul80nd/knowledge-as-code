---
id: adr-{{nnnn}}
tier: decided
status: proposed
decided-on:
owner:
related:
  - adr-{{a}}
  - adr-{{b}}
---

# {{Title}}

`ADR: adr-{{nnnn}}` `PROPOSED`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what an ADR adds to that.

**Frontmatter**

* **`id`**: matches the filename number. Four digits, never reused.
* **`tier`**: always `decided` for an ADR.
* **`status`**: `proposed` while under review, then `accepted`, and later `deprecated` or `superseded`. Values are
  lowercase.
* **`decided-on`**: the acceptance date, quoted `"YYYY-MM-DD"`. Leave the key bare until accepted.
* **`owner`**: the named person answerable for the decision, never a team alias.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

> **In the context of** {{use case}}, **facing** {{concern}}, **we decided** {{chosen option}}, **rather than**
> {{alternatives}}, **to achieve** {{quality / benefit}}, **accepting** {{downside}}.

_(One tight Y-statement, keeping all six moves. Make every clause a phrase, not a sentence, and name rejected options in
a few words. The full forces, alternatives, and consequences live in the sections below, so the summary points at them
rather than restating them. Aim for one breath, ~35–45 words.)_

## Context

What is the situation that prompted this decision? What constraints, drivers, or forces are at play? Write this so a
reader joining the project in two years can understand *why* this decision was even on the table.

Keep it factual and free of advocacy: the decision itself comes next.

## Decision

What did we decide? State it clearly and in the active voice ("We will use X for Y."). Keep it to one or two short
paragraphs.

## Alternatives Considered

* **{{Option name}}** was rejected because {{reason}}.
* **{{Option name}}** was rejected because {{reason}}.

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

* [adr-{{a}}] decided {{what}}, and that matters here because {{why}}.
* [adr-{{b}}] relates because {{how}}.

_(One line each on **how** the two relate. The annotation is the value, not the link. Cite rather than restate: if
another ADR already made an argument, link it instead of repeating it.)_

## References

* [RFC {{number}}](https://www.rfc-editor.org/rfc/rfc{{number}}) covers {{what}}.

_(External sources only, as inline links. A rename does not break them. Omit the section if there are none.)_

[adr-{{a}}]: {{a}}.md
[adr-{{b}}]: {{b}}.md
