What `kac report coverage --out` writes, compared with what the same report prints. No golden is committed.

The corpus has one policy with two clauses. A standard discharges `WRITTEN` and nothing discharges `OPEN`, so the
report has a covered row and an uncovered one. `.corpus.yaml` states `corpus` and `content-version`, because the
report's `sources:` cites both.

The runner runs the report three times and checks these.

* The file `--out` writes is what the report printed, with the `generated:` block taken out of both, since two runs
  disagree on the moment.
* The file ends on one newline, as every record does, and it contains the row for `WRITTEN` against `std-COVER`.
* A second `--out` to the same path exits 1, says the path already exists, and leaves the file unchanged.

`ReportsTests` pins the report's text. This fixture adds only that `--out` changes where the report goes and nothing
in it. `Report.feature` reads this corpus for what `report` promises.
