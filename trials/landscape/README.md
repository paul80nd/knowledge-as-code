# The landscape review

This review compares `kac` with the offerings that solve a similar problem. For each one it says where `kac` is the
same, where it differs, and what `kac` lacks. Whoever plans the trial reads it, in
[#1164](https://github.com/paul80nd/knowledge-as-code/issues/1164), and so does whoever reviews the use cases against
it, in [#1161](https://github.com/paul80nd/knowledge-as-code/issues/1161).

[`docs/compared.md`](../../docs/compared.md) is the public comparison, for somebody choosing whether to adopt. This
review is for the people building `kac`, so it also lists what `kac` lacks.

## Take a reading

1. Split the offerings into families. A run covers each family in its own pass. The 2026-10-27 run used four:
   developer portals and catalogues, decision records and compliance as code, agent context tools, and knowledge
   formats with their validators.
2. Read each offering from its primary sources: its own documentation, specification and source repository. Record
   the URL and the day you read it beside every claim. Mark anything you concluded and did not read as "(inferred)".
3. Write each family's notes to `runs/<date>/<family>.md`.
4. Check every claim the review relies on against its source before the review states it.
5. Diff the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) specification
   against the version the last run read. `kac` conforms as an OKF producer, so each change is a finding.
6. Write the review to `runs/<date>.md`. It states the findings, and links each family's notes for the evidence.
7. File each finding that asks for work, and list the issue beside the finding.

## Reading the result

A run has no pass mark. A later run compares with this one: an offering that gained what `kac` lacked, or a lead
`kac` lost, is the change to look for.

## Runs

| Run                              | Families |
|----------------------------------|---------:|
| [2026-10-27](runs/2026-10-27.md) |        4 |
