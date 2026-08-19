# Knowledge as Code

This corpus is not a document store. It is a structured, validated, machine-readable body of knowledge that happens to
render as a wiki: maintained in git, reviewed through pull requests, and readable *and writable* by AI coding sessions
as well as people.

This page is the way in. The detail lives beneath it:

| If you want to…                              | Go to                                             |
|----------------------------------------------|---------------------------------------------------|
| Understand what kinds of knowledge live here | [Taxonomy](knowledge-as-code/taxonomy.md)         |
| Know why the framework is shaped this way    | [Principles](knowledge-as-code/principles.md)     |
| Add or change something                      | [Contributing](knowledge-as-code/contributing.md) |
| Write a clear sentence, anywhere at all      | [Style](knowledge-as-code/style.md)               |
| Know how a document should be written        | [Authoring](knowledge-as-code/authoring.md)       |
| Know what metadata a document needs          | [Metadata](knowledge-as-code/metadata.md)         |
| Understand what CI checks and builds         | [Automation](knowledge-as-code/automation.md)     |
| See where the taxonomy's names came from     | [Lineage](knowledge-as-code/lineage.md)           |

Working this way is itself a decision, and a corpus that holds ADRs should record it as one.

## The problem

Engineering knowledge is spread thin. Architectural reasoning lives in one place; functional detail lives in work items;
operational know-how lives in people's heads or in a chat message from eight months ago; the answer to "why does the
build fail when I do X" is rediscovered roughly every quarter by whoever hits it next.

AI coding sessions sharpen this in both directions. A session that does not know the standards will confidently produce
work that violates them. A session that *discovers* something useful has nowhere to put it, so the discovery dies when
the session ends. Knowledge neither arrives where it is needed nor accumulates from where it is found.

## The commitment

Knowledge is treated as code:

* **Plain Markdown in git.** Diffable, reviewable, versioned, blamable. No proprietary format, no export step.
* **Structured metadata.** Every document carries frontmatter, so it can be validated, indexed and cross-referenced by
  machine rather than by memory.
* **Reviewed through pull requests**, at a rigour proportionate to what the document is — see [tiers](#tiers).
* **Published as a wiki**, so it stays readable by anyone without a terminal or IDE.
* **Readable and writable by agents.** Sessions consult it before working and contribute back what they learn.

The last point changes the character of the thing. A wiki only humans write grows at the rate humans remember to write;
a wiki that sessions can also write to grows at the rate work happens — which is why the review model matters more here
than it otherwise would.

## The normative hierarchy

Four layers, each citing the one above it. This is the conventional policy hierarchy, used deliberately rather than
inventing a vocabulary — [lineage](knowledge-as-code/lineage.md) records where it comes from and where it diverges.

| Layer             | Answers                         | Changes         | Example                                                                      |
|-------------------|---------------------------------|-----------------|------------------------------------------------------------------------------|
| Policy            | What do we commit to, and why?  | Rarely          | "Secrets are never stored in source control."                                |
| Standard          | What must I do as a result?     | With practice   | "Services **MUST** read secrets from a managed vault via workload identity." |
| Control           | How do we know it's being done? | With tooling    | "CI runs secret scanning on every PR; failures block merge."                 |
| Process / Runbook | How do I actually do it?        | With the estate | "Rotating a secret in the vault."                                            |

ADRs sit alongside rather than inside this hierarchy. An ADR records a *decision* and is immutable; a standard records
the *resulting practice* and is maintained in place. The ADR owns the "why", the standard owns the "what", and the
standard cites the ADR rather than restating it.

Where a policy corresponds to an external framework it says so, clause by clause. That is **alignment, not
certification**: it means the framework covers the same ground, not that anyone is audited against it. What this corpus
actually stands obliged by is recorded in [`frameworks.md`](/frameworks.md) and nowhere else.

## Tiers

**What a document is about** (its type) and **how it behaves** (its tier) are different things, and it is behaviour that
determines the rules.

| Tier            | Behaviour                                                   | Review                               |
|-----------------|-------------------------------------------------------------|--------------------------------------|
| **Decided**     | Immutable once accepted. Superseded, never rewritten.       | PR, two reviewers                    |
| **Normative**   | Living. Owned. Edited in place with a changelog.            | PR, owner approves                   |
| **Descriptive** | Living. Must mirror reality. Verifiable against the estate. | PR, but drift detection catches more |
| **Procedural**  | Living. Must be rehearsed to stay true.                     | PR + evidence of last rehearsal      |
| **Observed**    | Perishable. Unreviewed until promoted. Expires by default.  | None until promotion                 |

A new kind of knowledge does not need new machinery — it needs a tier. Every validation rule, review expectation,
[language rule](knowledge-as-code/authoring.md#by-tier) and generated report keys off the tier rather than the type. Why
that is the load-bearing idea is set out in [principles](knowledge-as-code/principles.md#behaviour-before-subject).

Observed is the row that surprises people: the tier carrying the least authority is the one the corpus most depends on,
because capture that is not free does not
happen. [Cheap capture, deliberate promotion](knowledge-as-code/principles.md#cheap-capture-deliberate-promotion) is
where that argument lives.

**Lifecycle** (`immutable` / `living` / `perishable`) follows from tier and is not stated separately. Two fields that
can contradict each other is a defect waiting to happen.

## What this is not

* **Not a replacement for the work tracker.** Work items own delivery. This corpus owns durable knowledge. Where they
  overlap, it links rather than copies.
* **Not a document dump.** Every document has a type, a tier, an owner and a reason to exist. Content that fits no type
  is a prompt to discuss the taxonomy, not to create a `misc/` folder.
* **Not certified compliance.** See the note on alignment above.
* **Not automatically true.** Documents carry a status and, in the Observed tier, a confidence level. Read them.

---

**A note on scope.** The documents under `knowledge-as-code/` describe the system; they are not themselves part of the
taxonomy and carry no taxonomy frontmatter. The constitution is not one of the laws. This also keeps the mechanism —
schema, validators, generators, skills — cleanly separable from the corpus's content.
