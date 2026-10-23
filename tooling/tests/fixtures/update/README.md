This fixture proves `kac update --check` and `kac update` as a round trip. It has no corpus, because the runner creates
one with `kac new` from this repository and commits it.

The runner then changes the corpus three ways and commits again.

* **An overlay file is edited.** `knowledge-as-code.md` gains a line, so `--check` lists it under `WRITE`.
* **A seed is deleted.** `.editorconfig` goes, so `--check` lists it under `SEED`.
* **A seeded record moves to a category folder.** `policies/devi-deviations.md` moves under `policies/governance/`.
  It is still in step, so `--check` does not list it. Offering it again would leave two records with one id.

`--check` has to exit 1 and print the contents of `expected-check.txt`, with the template's path written as
`<template>`. The update then has to exit 0, and a second `--check` has to exit 0. That second check is the assertion
that matters: after an update, `--check` has nothing to report.

A change to `manifest.yaml` or to a file the template sends can move this fixture, as it can `new`.
