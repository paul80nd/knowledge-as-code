---
name: standards-review
description: Review a change against the standards that travel with this plugin, citing the rule behind every finding.
  Use when someone asks for a review of their work, and before you open a pull request. Pass the fixed point to review
  from, such as a branch, a tag or a commit.
argument-hint: "[fixed point]"
context: fork
agent: general-purpose
background: false
---

# Reviewing a change against the standards

You are the reviewer. You check one change against every rule the estate wrote, and you cite each finding by the
rule's id. Every rule you check is in one file, generated from the export this plugin carries:

```text
${CLAUDE_PLUGIN_ROOT}/views/review.md                             # every rule, its obligations and its controls
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                        # what this export is, and when it was taken
${CLAUDE_PLUGIN_ROOT}/corpus/standards/rules.jsonl                # one line of JSON per rule, with its link
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<record>.json               # one control this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<shortcode>/<record>.json   # one control a corpus this one consumes wrote
```

Use those paths exactly as they appear above. They are already absolute. An installed plugin sits in a cache of its own,
outside the repository you are reviewing.

## Pin the change

The change is everything that differs from a fixed point: the commits since it, and the work not yet committed.

* **The fixed point is `$ARGUMENTS`**: a branch, a tag, a commit, or `main`.
* **With no fixed point given, review from where this branch left the default branch.** Take the merge-base of the
  current commit and the default branch, so the change is the branch's commits plus the uncommitted work. On the
  default branch itself, that leaves the uncommitted work alone.

Read the change with whatever this session has for reading the repository's history. Confirm the fixed point resolves
before you start. Where the change is empty, say so and stop, because an empty change has nothing to review.

## Read the rules whole

Read `${CLAUDE_PLUGIN_ROOT}/views/review.md` from its first line to its last. Where one read stops short of the end,
read on from where it stopped until you reach it. A rule you did not read is a rule you did not check.

The view lists each standard under its id and title, with its status and the controls that check it. Under each
standard it lists the rules, each with the id to cite and its obligations. Each obligation is one bullet, and its
keyword is in bold.

## Decide which rules are yours to check

**Check every obligation yourself, except one an automated control already checks.** A standard's `Controls:` line
lists each control as its id, its title, then its mechanism and its status in brackets.

A control checks an obligation for you only where its status is `active` and its mechanism is `ci` or `runtime-alert`.
For each such control, read its record and its two sections:

* **`sections.What it checks`**, a string of markdown, names the obligations the control checks. Leave those out.
* **`sections.Coverage and gaps`**, a string of markdown, says where the control stops. Check what it leaves.

Every other control leaves the rule with you. `review-checklist` and `manual` mean a person checks the rule while
reviewing, and that person is you. `not-enforced` means nothing checks it. A `planned` or `retired` control checks
nothing today.

**A prefix on a control's id names the corpus that wrote it.** `eng:ctl-0004` is at
`${CLAUDE_PLUGIN_ROOT}/corpus/controls/eng/ctl-0004.json`, and a bare `ctl-0004` is at
`${CLAUDE_PLUGIN_ROOT}/corpus/controls/ctl-0004.json`.

**Say which automated check covers each rule you left out, and say so for each finding where none does.** A reader
deciding whether to merge needs to know what CI will catch and what only this review will.

## Judge each obligation by its keyword

The keyword in bold says what departing from the obligation costs. Compare it whole, because `MUST NOT` opens with
`MUST`.

* **`MUST` and `MUST NOT` state a requirement.** A change that breaks one is a violation. Departing from it takes a
  deviation the rule's owner accepts. Where this plugin ships the `request-deviation` skill, name it as the way to ask.
* **`SHOULD` and `SHOULD NOT` state the usual way.** A change may depart from one by giving its reason in the change,
  and that conforms to the rule. Report the departure as a judgement call, and quote the reason the change gives. Where
  the change gives none, say that it gives none. No deviation is needed.
* **`MAY` permits.** It gives you nothing to report.

## Cite each finding

**Cite the rule's id, as the view's `Cite as` line gives it.** A rule is addressed by its heading, so the obligation
has no id of its own. Quote the obligation the change breaks, and cite the rule it sits under.

**Link the rule from `rules.jsonl`.** Find the line whose `id` matches the one you cite, and quote its `links.human`.
Each line is one rule, with these keys:

| Field                | Type                     | What it holds                                                                |
|----------------------|--------------------------|------------------------------------------------------------------------------|
| `id`                 | string                   | `<standard-id>.<rule-key>`, the address to cite                              |
| `title`              | string                   | the heading the obligations sit under                                        |
| `obligations`        | list of objects, or null | one entry per bullet, each with a `level` and a `text`                       |
| `covers`             | list of strings, or null | the policy clauses this rule discharges                                      |
| `seeAlso`            | list of strings, or null | the rules of other standards this one points at                              |
| `type`               | string                   | `standards`, on every line of this file                                      |
| `record`             | string                   | the standard the rule belongs to                                             |
| `part`               | string                   | the rule's key inside that standard                                          |
| `shortcode`          | string, or absent        | the corpus that published the rule. A rule written here carries no such key  |
| `status`, `reviewBy` | string                   | how far the standard has settled, and the date it was meant to be read again |
| `links`              | object or null           | `human`, the link to the rule. `null` where nothing can address it           |
| `path`, `anchor`     | string                   | the record's file in its corpus, and the fragment the rule sits at           |

**A key with no value is `null`, and the key is still there.** `shortcode` is one exception, and the row above says
so. `links` is the other: a line another corpus exported before the key existed has none. Where `links` is `null` or
absent, say the rule has no link.

**A prefix on a rule's id names the corpus that wrote it.** `eng:std-TEST.every-test-is-hermetic` was published by the
corpus whose shortcode is `eng`. Look the shortcode up in `sources` in `manifest.json`, and name that corpus in words.
An inherited rule binds the change as fully as one written here.

## Say when a standard is unsettled

A standard is in force at status `active`. The view gives each standard's status under its heading, and a rule's line
gives the same `status` beside its `reviewBy`. Report a finding under an unsettled standard all the same, under the
heading its keyword gives, and say which of these you saw:

* **`status: draft`**: the standard was a proposal when the export was taken, so the finding is not in force.
* **`status: deprecated`**: the standard is on its way out, so the finding is not in force.
* **`status: superseded`**: another standard replaced it. Say so, and say the finding is not in force.
* **`reviewBy` earlier than today**: the standard passed the date it was meant to be read again.

**Quote `generatedAt` from `manifest.json` beside any status or review date you report.** The export is a copy taken
on that day, and reads the same however long ago it was.

## Report

Report under two headings, each finding on its own line or short paragraph:

* **Violations**: each broken `MUST` or `MUST NOT`, with the rule's id, the obligation quoted, the file and the lines
  that break it, and what checks the rule, or that nothing automated does.
* **Judgement calls**: each departure from a `SHOULD` or `SHOULD NOT`, with the rule's id, the obligation quoted, and
  the reason the change gives or the fact that it gives none.

A finding under a standard whose status is not `active` stays under the heading its keyword gives, marked not in
force, with that status and `generatedAt`.

Close with one line naming the corpus and every entry in `sources` whose rules you checked, and the rules you left to
an automated control. Keep the report under 500 words, so whoever decides whether to merge can read it at once.

**A review that finds nothing says only that no rule written here was broken.** The estate may not have written a rule
for what the change does. Say that, and leave the rest of the judgement to whoever approves the change.

## Credit

The shape of this review is the Standards review in [`code-review`][cr], by Matt Pocock, released under the MIT
licence, © 2026 Matt Pocock.

[cr]: https://github.com/mattpocock/skills/tree/main/skills/engineering/code-review
