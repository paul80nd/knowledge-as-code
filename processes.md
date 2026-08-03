# Processes

How to perform the planned tasks that keep the platform running.

**[→ Index](processes/INDEX.md)**

## What is a process?

A procedure you follow **deliberately**, because you decided to: onboarding a new developer, cutting a release,
provisioning an environment, rotating a secret. Numbered steps, prerequisites, a verification step and a way back.

## Why we use them

Procedural knowledge is the kind most reliably held in one person's head, and the kind most expensive to lose. Writing
it down is also the only way to find out that a step everyone "just knows" stopped working three months ago.

Processes carry `last-rehearsed` for exactly that reason. A procedure nobody has walked through is a hypothesis.

## Scope

The distinction that matters: **are you doing this because you planned to, or because something is broken?**

* Planned — a process.
* Broken — a [runbook](/runbooks).

Different audiences, different tone, different consequences when they go stale. A process that is slightly out of date
is annoying; a runbook that is slightly out of date is dangerous.

A process is also not:

* **A rule** — "deployments happen in dependency order" is a [standard](/standards); the release process cites it.
* **A reference list** — system requirements and port tables are [service](/services) catalogue content, not steps. A
  document with no steps in it is not a process.
* **An explanation** — how the pipeline works is an [explanation](/explanations); how to use it is a process.

## Metadata

<!-- BEGIN GENERATED: schema-processes -->

| Field                 | Req | Type | Notes                                      |
|-----------------------|-----|------|--------------------------------------------|
| `status`              | ●   | enum | `active` · `draft` · `retired`             |
| `applies-to`          |     | list | Service ids                                |
| `last-rehearsed`      | ●   | date | Quoted. `"never"` is permitted and honest. |
| `rehearsal-frequency` |     | enum | `per-release` · `quarterly` · `annual`     |
| `requires-access`     |     | list | Systems or roles needed                    |

<!-- END GENERATED: schema-processes -->

## Adding a process

1. Copy [`template.md`](processes/template.md) to `<slug>.md`. Processes use slug ids — `prc-releasing`.
2. Write the steps imperatively and in order. Assume the reader has not done it before.
3. Include prerequisites, a verification step ("you know it worked when…") and a rollback.
4. Set `last-rehearsed`. `"never"` is permitted and preferable to a guess.
5. Name what access is needed in `requires-access`, and who to ask. "Obtain the file from the repository owner" is not
   useful to someone who doesn't know who that is.

**Conventions**

* **No hedging.** "Typically the order would be…" is not followable. If the order varies, say what it depends on.
* **Verification is not optional.** A process that ends at the last action leaves the reader guessing whether it worked.
* **Rehearse before you trust.** Update `last-rehearsed` when someone actually follows it end to end — not when someone
  edits the document.

## What CI checks

<!-- BEGIN GENERATED: checks-processes -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-processes -->
