# `bundle-views`

What `kac bundle` generates as a view, asserted through the CLI. [`bundle`](../bundle/README.md) proves how a plugin is
assembled. This fixture proves the two views its plugin manifest declares.

## What the fixture is shaped to reach

**Two standards, one control.** `std-DEPLOY` is a draft with a rule of a MUST and a SHOULD, and `ctl-0002` lists it in
`verifies`. `std-OBS` is active, and no control lists it. So the review view shows a standard with a control and a
standard with none, and both views show two statuses.

**A lookup skill reading standards.** `skills/standards-lookup` survives the trim, so the build index names it as the
way to read a rule's obligations.

**No hook.** The breadcrumb is `bundle`'s concern, and this fixture leaves it out.

## What is asserted where

`expected-files.txt` names each view where its component's path puts it. `expected-content.txt` pins the lines each
view states: a standard's heading, status and controls, and a rule's title, id and obligations. `expected-bundle.txt`
pins what the run printed. A view whose types the export lacks, and a view the tool does not write, are specified in
[`Bundle.feature`](../../../kac.features/Bundle.feature).
