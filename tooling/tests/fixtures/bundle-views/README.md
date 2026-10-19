# `bundle-views`

What `kac bundle` generates as a view, asserted through the CLI. [`bundle`](../bundle/README.md) proves how a plugin is
assembled. This fixture proves the two views its plugin manifest declares.

## What the fixture is shaped to reach

**Two standards, one control.** `std-DEPLOY` is a draft with a rule of a MUST and a SHOULD, and `ctl-0002` lists it in
`verifies`. `std-OBS` is active, and no control lists it. So the review view shows a standard with a control and a
standard with none, and both views show two statuses.

**A lookup skill reading standards.** `skills/standards-lookup` survives the trim, so the build index names it as the
way to read a rule's obligations.

**A hook beside the build index.** The `hooks` component needs only `standards`, so it survives, and `bundle` renders
`hooks/build-index.json` from the build index. The breadcrumb is [`bundle`](../bundle/README.md)'s to prove.

## What is asserted where

`expected-files.txt` names each view where its component's path puts it, and the hook output. `expected-content.txt`
pins the lines each view states: each view's opening, a standard's heading, status and controls, a rule's title,
and a rule's id and obligations in the review view. It pins the hook output's event and its first line. `expected-bundle.txt`
pins what the run printed. A view whose types the export lacks, and a view the tool does not write, are specified in
[`Bundle.feature`](../../../kac.features/Bundle.feature).
