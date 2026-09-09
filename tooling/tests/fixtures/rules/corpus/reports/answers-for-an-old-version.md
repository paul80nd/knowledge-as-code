---
id: rpt-answers-for-an-old-version
type: report
tier: descriptive
status: active
owner: human:alex.doe
generated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }
sources:
  - { resource: fixture-corpus, version: "0.1.0" }
verified:
  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }
---

# A report answering for a version the corpus has left behind

`Report: rpt-answers-for-an-old-version` `ACTIVE`

`sources` names the corpus at 0.1.0 and the descriptor says 0.2.0, so the corpus has moved since anybody read this.
The numbers below still read as numbers, which is why `report-stale` says so rather than leaving a reader to notice.

A warning rather than an error. The report may well still hold, and whoever owns it either confirms that and raises
the version by hand or runs it again.
