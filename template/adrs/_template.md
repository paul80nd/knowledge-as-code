---
id: adr-{{nnnn}}
type: adr
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

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what an ADR adds to it. Add an
optional field from [the type page](../adrs.md#metadata) where you have a value for it.

**The form.** Lean Nygard style, opened by a Y-statement summary and with an explicit Alternatives Considered section.
[adr-0001](0001-knowledge-as-code.md) is the worked example.

**Frontmatter**

* **`id`**: matches the filename number. Four digits, never reused. Save the file as `{{nnnn}}-kebab-case-title.md`,
  taking the next unused number. The ADRs index shows the highest one in use.
* **`tier`**: always `decided` for an ADR.
* **`status`**: `proposed` while under review, then `accepted` or `rejected`, and later `deprecated` or `superseded`.
  Values are lowercase. A proposal the team turns down becomes `rejected` and keeps its number.
* **`decided-on`**: the day the decision was settled, quoted `"YYYY-MM-DD"`. Leave the key bare until then, and set it
  in the change that moves the status to `accepted` or `rejected`.
* **`supersedes`** and **`superseded-by`**: set the old ADR's `status: superseded` and `superseded-by`, and the new
  one's `supersedes`. A supersession recorded on one side only fails the build.
* **`owner`**: who is answerable for the decision, as `human:alex.doe` or `role:head-of-engineering`. Never a team
  alias.

**Keywords.** An ADR that establishes a default or a policy may use [RFC
2119](https://datatracker.ietf.org/doc/html/rfc2119) keywords. An ADR that records a decision uses plain declarative
prose.

**Once it is decided.** Change an accepted or rejected decision only by writing a new ADR that supersedes it. Edit it in
place only to correct what it misstates about the decision as agreed, to fix its prose, a typo or a broken link, or to
move its status. Never update it to follow what changed afterwards. The commit message says what you changed.

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
paragraphs. Where the proposal was rejected, say what you turned down, and why.

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
