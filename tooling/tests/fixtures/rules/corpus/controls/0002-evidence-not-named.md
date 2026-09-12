---
id: ctl-0002
type: control
tier: normative
status: active
owner: human:alex.doe
verifies: [std-ERRORS]
mechanism: review-checklist
frequency: per-pr
applies-to: [all]
---

# Evidence not named

`Control: ctl-0002` `ACTIVE`

## What it checks

Nothing that can be shown. The mechanism says a human works through a checklist every pull request, and no
field says where the record of that lives, so nobody can tell the control from a claim about one.

* `std-ERRORS.a-failure-says-what-happened` says a failure "is reported with the status code that
  describes it". Those are the clause's own words, and `clause-quoted-faithfully` reports nothing here.
* `std-ERRORS.a-failure-says-what-happened` also says a failure "is answered with a `404`". The clause
  says no such thing, and that is the finding.

## How it works

`mechanism` is anything other than `not-enforced`, and `evidence` is absent. `frequency` is filled in, so
the `required-when` on that field stays out of the way. The bullets above add the misquoted clause, so
this document owns two findings.

`applies-to` carries the literal `all` its schema admits beside the service ids. Nothing reports it, and that silence is
the assertion: without `allow-literal` the entry fails `id-format`, and this golden would gain a finding.

## Coverage and gaps

The gap is the evidence, which is the point. `not-enforced` is the honest value for a control nobody can
produce a record for, and the reason this reports rather than fails.
