---
id: prc-change-a-skill
type: process
tier: procedural
status: active
applies-to: [ svc-marketplace ]
last-rehearsed: "never"
owner: human:paul.law
tags: [ plugin, skills, versioning ]
---

# Change a skill or a hook

`Process: prc-change-a-skill` `ACTIVE`

Add, change or remove a skill, a hook or a `plugin.json` entry.

## When to use this

You are editing a file under `template/.plugin/`, or one of the skills under `.claude/skills/`. Three trees, and what
a change costs depends on which one. Every corpus here names `plugin.from` in its descriptor, so a skill under
`template/.plugin/` is authored once and shipped in four plugins, and a change to it moves more version stamps than any
other file here. A skill under `.claude/skills/` that `manifest.yaml` sends to a corpus moves `version:` there and the
stamp in every descriptor. One nothing sends moves nothing.

## Prerequisites

* [std-PLUGIN], which states the rules a bundled skill follows. It states no order, so this page supplies one.
* [std-VERS], which states what a move of each version stamp means.
* `kac` run as `dotnet run --project ../../tooling/kac --` from inside a corpus under `examples/`.
* A corpus declaring `consumes:` needs `kac restore` first. `restore` needs its producer packed.

## Steps

1. Decide which tree holds the file. [Skills] names every skill and the tree it lives in.
   * `template/.plugin/` is bundled. It travels inside a plugin and moves `content-version`. Do every step.
   * `.claude/skills/`, where `manifest.yaml` sends it to a corpus, travels into a working tree and moves
     `version:` there. Do steps 3, 7, 11, 12, 14 and 15, and say in your task list why you left each of the others
     out.
   * `.claude/skills/`, where nothing sends it, is this repository's own. No version stamp moves. Do steps 3, 11, 14
     and 15, and say why you left each of the others out.
   * A skill moving between the second and the third is a change to `manifest.yaml`, so step 7 applies to it.
2. Read [std-PLUGIN]. Its rules bind a bundled skill, and its conformance checklist is what a reviewer reads.
3. Load `technical-writing`. No second writing skill applies to a skill, a hook or a `plugin.json`.
4. Write the file under `template/.plugin/`. Copy it nowhere. Every corpus under `examples/` names `plugin.from`, so
   each one reads this tree and receives none of it.
5. If you added or removed a skill directory or a hook directory, declare it under `metadata.components` in
   `template/.plugin/.claude-plugin/plugin.json`. Give it `requires` and a `note`. Add `standalone` where it reads no
   export and supports no other component, and `announce` where it introduces itself at the start of a session.
6. Copy that entry into `.plugin/.claude-plugin/plugin.json` in every corpus under `examples/`. Each corpus owns its own
   manifest, so `kac update` seeds it once and never compares it again.
7. If you added, removed, renamed or moved a file the template sends a corpus, move `version:` in `manifest.yaml`.
   Then move `upstream.template-version` in every corpus's `.corpus.yaml` to the same number. That reaches a file
   under `template/.plugin/`, and a skill under `.claude/skills/` named by an overlay rule.
8. Move `content-version` in each corpus whose bundle ships the change. A lookup skill ships only in the corpora that
   declare the type it reads. A standalone skill ships in all four. [std-VERS] says which component moves.
9. Move the `resolved:` lock of every consumer of a corpus you moved in step 8. Move its `version:` range as well where
   the producer is below 1.0.0 and its minor moved.
10. Prove each lock the way CI will. Delete `.imports/` in `examples/payments` and `examples/dog-fooding`, repack each
    producer, then run `kac restore` in each consumer and read what it reports. `.imports/` is untracked, so a restore
    over a folder you kept proves nothing.
11. Fix the pages your change made wrong. Nothing in CI reads prose for meaning, and the list below is what a session
    adding one skill found wrong by grep after it thought it was finished.
    * A skill added or removed affects the skill list in the root `CLAUDE.md`, the component prose in
      `docs/design/plugin.md`, the sample transcript and component counts in `docs/cli/bundle.md`, and the
      `new: wrote N file(s)` line in both `docs/cli/new.md` and `docs/getting-started.md`.
    * A standalone skill added or removed affects the loop naming them in `.github/workflows/kac.yml` and
      `.azuredevops/kac.yml`, and the prose describing that job in [ctl-0008].
    * A skill a corpus receives affects `template/knowledge-as-code/contributing.md`, the copy of it every corpus
      under `examples/` holds, and the overlay note in `manifest.yaml` that names them.
    * A change to what a skill does affects whichever of those describe it.
    * `docs/skills.md` needs no edit. Its three tables are generated from `plugin.json`, `manifest.yaml` and the
      `.claude/skills/` directory, and `SkillReferenceTests` fails a stale one. Never hand-edit a generated block.
12. Run `kac update --check --from ../../` inside every corpus under `examples/`. It withholds the plugin tree, so it
    proves the rest of the overlay and nothing about step 6.
13. Regenerate each report whose `sources:` names a corpus you moved in step 8. Load `writing-a-report`, which has the
    merge that keeps a person's judgement across a regeneration.
14. Run the test layers, one `kac` invocation at a time. Adding or removing a file under `template/.plugin/` changes
    `expected-tree.txt`, and moving `manifest.yaml` changes `expected-descriptor.yaml`, so run the golden fixtures with
    `GITHUB_ACTIONS=true`.
15. Run [prc-pull-request].

## Verification

`kac update --check` reports no difference in any corpus, and every test layer reports zero errors.

The round trip is the layer that installs the plugin and asks each skill the question it describes. It needs the Claude
Code CLI, and CI runs it over `library`, `engineering` and `payments`.

Close by stating which tree you changed, which corpora moved their `content-version`, and which pages you fixed.

## If it goes wrong

A published `content-version` cannot be replaced. Ship the correction as a new version.

The plugin installed in this repository serves what is on `main`, so a branch editing a skill leaves it behind. Rebuild
it from the working tree to read your change, as this corpus's own `README.md` describes.

## Related

* [std-PLUGIN] states the rules a bundled skill follows. Step 2 is where you read them.
* [std-VERS.which-stamps-move-together-and-which-move-alone] states which stamps move together. Steps 7, 8 and 9 say
  where each falls in the order.
* [std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request] states what step 10 proves, and why a
  restore over a kept `.imports/` proves nothing.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] states the copy step 6 makes. Nothing proves
  that one, because each corpus owns its plugin manifest and no check compares the four.
* [prc-pull-request] merges the change.
* [svc-marketplace] is what a bundled skill is published through.

[Skills]: https://paul80nd.github.io/knowledge-as-code/skills/
[ctl-0008]: ../controls/0008-publish-round-trip.md
[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-PLUGIN]: ../standards/plugin.md
[std-VERS]: ../standards/versioning.md
[std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request]: ../standards/versioning.md#a-producers-move-obliges-every-consumer-in-the-same-pull-request
[std-VERS.which-stamps-move-together-and-which-move-alone]: ../standards/versioning.md#which-stamps-move-together-and-which-move-alone
[svc-marketplace]: ../services/marketplace.md
