Valid records that must produce no finding. This is the guard against a false positive.

A check that starts firing on a sound record fails here first. The golden is empty, and the summary counts seven
records and four templates, so a record dropped from discovery fails here too.

* **A flat corpus.** An ADR and a policy sit in the folder their type names.
* **A nested corpus.** A second policy sits one folder down, under `policies/security/`. Two standards with different
  ids share the filename `testing.md`, one a folder down and one two folders down, so nothing may key a record on its
  filename alone.
* **Both forms of a departure.** One deviation departs from the policy's clause, `pol-VURM.CLEAN`, so that citation
  has to resolve. The other departs from `none` and gives the work to a `role:` under `assigned-to`, the form for
  technical debt no clause states.

Several feature files read this corpus as the sound baseline for the command they specify, among them
`Clean.feature`, `Discovery.feature` and `Checks.feature`.
