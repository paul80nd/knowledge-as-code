---
id: rpt-framework-coverage
type: report
tier: descriptive
status: active
owner: human:alex.doe
generated: { at: 2026-09-11T12:02:45Z, by: kac/0.25.0 }
sources:
  - { resource: example-engineering, version: "0.16.1" }
verified:
  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }
  - { at: 2026-09-09T18:29:19Z, by: coverage-sweep/1.0.0 }
  - { at: 2026-09-11T08:20:00Z, by: coverage-sweep/1.0.0 }
  - { at: 2026-09-11T09:05:00Z, by: coverage-sweep/1.0.0 }
  - { at: 2026-09-11T10:10:00Z, by: coverage-sweep/1.0.0 }
tags: [ coverage, frameworks ]
---

# Framework coverage

`Report: rpt-framework-coverage` `ACTIVE`

## Limits

This reads `example-engineering` and what it imports. An `Alignment` cell stays in the corpus that wrote it, so the
citations counted below are the ones written here. A reference an imported policy cites belongs to that producer's own
report.

No column here says a clause meets the reference it cites. A citation records that the clause named it. Whoever confirms
this report decides whether the clause covers that control or one corner of it.

## What a single citation carries

A reference cited by one clause is one edit from losing its coverage of a control. Its `Citations` count reads 1.
Deleting that clause, or rewording its `Alignment` cell, drops the reference from this corpus and nothing reports it.
The register goes on listing the framework as covered.

Standing decides what that costs. `Obliged` points outside the estate: an auditor traces a control back to the
commitment implementing it, and a control with no commitment left is a finding. `Inspiration` obliges nothing, so a
reference losing its last citation is provenance the estate stopped claiming.

**A count of one does not always mean one clause is at risk.** Six of a data subject's rights, from access to
objection, are each covered by `pol-DATA.RIGHTS` alone. One capability covers all six: finding, exporting, correcting,
deleting and restricting one person's data. Losing that clause would drop six articles at once, a sharper risk than the
column can show.

The `Note` column states the judgement the columns beside it cannot. It says what [frameworks.md](../frameworks.md)
records about a reference, and how far the clauses cited against it cover the control itself. A citation is honest
coverage, a nearest fit, or one instance of something wider, and only a reader can separate those. Most rows have
nothing to add, and an empty cell says so.

## Totals

| Framework | Standing | References | Cited once |
|-----------|----------|------------|------------|
| Azure WAF | Inspiration | 4 | 0 |
| DORA metrics | Inspiration | 4 | 1 |
| Equality Act 2010 | Obliged | 4 | 3 |
| ISO 27001:2022 | Obliged | 58 | 14 |
| NIST AI RMF 1.0 | Inspiration | 4 | 1 |
| NIST SSDF 1.1 | Inspiration | 15 | 1 |
| OWASP ASVS 4.0 | Inspiration | 5 | 3 |
| SLSA 1.1 | Inspiration | 2 | 2 |
| UK GDPR | Obliged | 23 | 16 |
| WCAG 2.2 AA | Self-obligated | 1 | 0 |
| **Total** | | **120** | **41** |

## References

### Azure WAF

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#azure-well-architected-framework).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| cost-optimization | 9 | `pol-COST.ATTRIB`, `pol-COST.VISIBLE`, `pol-COST.WEIGHED`, `pol-COST.SIZING`, `pol-COST.ANOMALY`, `pol-COST.UNUSED`, `pol-COST.UNOWNED`, `pol-COST.ERODE`, `pol-COST.PERUNIT` | `pol-COST` | The only framework covering `pol-COST`. No external obligation requires this estate to manage its own cloud spend. |
| operational-excellence | 7 | `pol-DERV.RUNLOG`, `pol-OBSV.CENTRAL`, `pol-OBSV.CLOCKS`, `pol-OBSV.RETAIN`, `pol-OBSV.HEALTH`, `pol-OBSV.ALERTS`, `pol-OBSV.BLIND` | `pol-DERV`, `pol-OBSV` |  |
| performance-efficiency | 5 | `pol-PERF.TARGETS`, `pol-PERF.MEASURE`, `pol-PERF.DEFECT`, `pol-PERF.PEAK`, `pol-PERF.NOTEST` | `pol-PERF` |  |
| reliability | 12 | `pol-OBSV.SLO`, `pol-RECV.RTORPO`, `pol-RECV.BACKUP`, `pol-RECV.RESTORE`, `pol-RECV.OFFSITE`, `pol-RECV.TIMEOUT`, `pol-RECV.DEGRADE`, `pol-RECV.IDEMPOT`, `pol-RECV.UNTEST`, `pol-RECV.RETRY`, `pol-RECV.SHED`, `pol-RECV.CHAOS` | `pol-OBSV`, `pol-RECV` | Cited at the pillar, not the recommendation, which is renumbered as the framework is revised. |

### DORA metrics

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#dora-metrics).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| change-failure-rate | 3 | `pol-AUTV.BLOCK`, `pol-AUTV.REGRESS`, `pol-AUTV.FLOW` | `pol-AUTV` |  |
| deploy-frequency | 1 | `pol-PIPE.DEPLOY` | `pol-PIPE` |  |
| lead-time | 6 | `pol-AUTV.INTEG`, `pol-AUTV.LEVELS`, `pol-AUTV.OFTEN`, `pol-AUTV.FLOW`, `pol-MNTN.SLIP`, `pol-MNTN.HARDER` | `pol-AUTV`, `pol-MNTN` | `pol-MNTN` cites this as a claim that acting on a maintainability measure moves lead time, and not as a control it satisfies. `TREND` has no citation, because measuring something moves nothing. `pol-AUTV.FLOW` commits to looking at the measure at all. |
| recovery-time | 2 | `pol-PIPE.REVERT`, `pol-PIPE.PROGDEL` | `pol-PIPE` |  |

### Equality Act 2010

Filed under `Obliged` in [`frameworks.md`](../frameworks.md#equality-act-2010).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| s.20 | 2 | `pol-A11Y.CONFORM`, `pol-A11Y.VENDOR` | `pol-A11Y` | The duty itself, which `s.29` and `s.39` apply. `VENDOR` cites it because a component the estate adopts becomes part of the service the duty applies to. |
| s.29 | 1 | `pol-A11Y.CONFORM` | `pol-A11Y` |  |
| s.39 | 1 | `pol-A11Y.CONFORM` | `pol-A11Y` | The employment duty, covering the internal tools `pol-A11Y` brings into scope. No clause is about staff alone, so `CONFORM` cites it. |
| Sch.2 | 1 | `pol-A11Y.UPFRONT` | `pol-A11Y` | The duty is anticipatory, and applies before anybody asks. `UPFRONT` puts the requirement at design time, and not in a backlog. |

### ISO 27001:2022

Filed under `Obliged` in [`frameworks.md`](../frameworks.md#iso-27001).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| A.5.3 | 2 | `pol-AGNT.DUTIES`, `pol-ACCS.DUTIES` | `pol-AGNT`, `pol-ACCS` |  |
| A.5.5 | 1 | `pol-INCR.NOTIFY` | `pol-INCR` | The control asks that contact with the relevant authorities be established and kept up. `NOTIFY` covers the statutory breach instance. `frameworks.md` places the rest with Legal and the ISMS owner. |
| A.5.8 | 2 | `pol-SECD.REQS`, `pol-SECD.ACTIONS` | `pol-SECD` |  |
| A.5.9 | 1 | `pol-DATA.INVENT` | `pol-DATA` |  |
| A.5.12 | 1 | `pol-DATA.CLASS` | `pol-DATA` |  |
| A.5.14 | 2 | `pol-DATA.CRYPTO`, `pol-MEXP.TRANSIT` | `pol-DATA`, `pol-MEXP` |  |
| A.5.15 | 1 | `pol-ACCS.LEAST` | `pol-ACCS` |  |
| A.5.16 | 2 | `pol-ACCS.NAMED`, `pol-ACCS.DIRECT` | `pol-ACCS` |  |
| A.5.17 | 6 | `pol-SCRT.STORE`, `pol-SCRT.ROTATE`, `pol-SCRT.LEAKED`, `pol-SCRT.EMBED`, `pol-SCRT.REUSE`, `pol-SCRT.LOGS` | `pol-SCRT` | The control is scoped to authentication information: issuing, resetting and storing passwords. `pol-SCRT` goes wider, to API keys, tokens and certificates. Annex A has no secrets-management control, so this is the nearest fit. |
| A.5.18 | 3 | `pol-ACCS.GRANT`, `pol-ACCS.RECERT`, `pol-ACCS.REVOKE` | `pol-ACCS` |  |
| A.5.19 | 2 | `pol-TRUS.SOURCE`, `pol-TRUS.UNTRUST` | `pol-TRUS` |  |
| A.5.21 | 4 | `pol-TRUS.INVENT`, `pol-TRUS.PINNED`, `pol-TRUS.SCREEN`, `pol-TRUS.TRACE` | `pol-TRUS` |  |
| A.5.22 | 2 | `pol-TRUS.REVIEW`, `pol-TRUS.SUPCHG` | `pol-TRUS` |  |
| A.5.23 | 2 | `pol-TRUS.CLOUD`, `pol-TRUS.EXIT` | `pol-TRUS` |  |
| A.5.24 | 2 | `pol-INCR.PROCESS`, `pol-INCR.DRILL` | `pol-INCR` |  |
| A.5.25 | 1 | `pol-INCR.DECLARE` | `pol-INCR` |  |
| A.5.26 | 3 | `pol-INCR.COMMS`, `pol-INCR.EVIDENC`, `pol-INCR.ADHOC` | `pol-INCR` |  |
| A.5.27 | 3 | `pol-INCR.LEARN`, `pol-INCR.ACTIONS`, `pol-INCR.TOOSOON` | `pol-INCR` |  |
| A.5.28 | 1 | `pol-INCR.FREEZE` | `pol-INCR` | Chain of custody and what makes evidence admissible are Legal's. `FREEZE` covers the other limb: keeping the state an investigation starts from. |
| A.5.29 | 1 | `pol-INCR.HOLD` | `pol-INCR` |  |
| A.5.30 | 1 | `pol-RECV.RTORPO` | `pol-RECV` |  |
| A.5.32 | 2 | `pol-TRUS.LICENCE`, `pol-TRUS.OBLIGE` | `pol-TRUS` |  |
| A.5.33 | 1 | `pol-DATA.KEEP` | `pol-DATA` | Identifying the records the estate must produce, and setting how long they are kept, belongs to the business data owner and the DPO. `KEEP` covers protecting them and keeping them retrievable for that period. |
| A.5.34 | 1 | `pol-DATA.LAWFUL` | `pol-DATA` |  |
| A.5.36 | 4 | `pol-DEVI.RECORD`, `pol-DEVI.CLOSE`, `pol-DEVI.PERM`, `pol-DEVI.CUSTOM` | `pol-DEVI` | Compliance review is the ISMS owner's, which `frameworks.md` records. These clauses cover the other limb: a departure recorded, owned, and closed by fixing the gap or re-accepting the risk. |
| A.5.37 | 3 | `pol-KNOW.DOCS`, `pol-KNOW.SYNC`, `pol-KNOW.HEADS` | `pol-KNOW` |  |
| A.6.8 | 1 | `pol-INCR.REPORT` | `pol-INCR` |  |
| A.8.2 | 4 | `pol-ACCS.ADMIN`, `pol-ACCS.SHARED`, `pol-ACCS.PERSIST`, `pol-ACCS.ZERO` | `pol-ACCS` |  |
| A.8.3 | 2 | `pol-ACCS.LEAST`, `pol-DATA.CLASS` | `pol-ACCS`, `pol-DATA` |  |
| A.8.4 | 2 | `pol-EVER.BRANCH`, `pol-ACCS.LEAST` | `pol-EVER`, `pol-ACCS` |  |
| A.8.5 | 1 | `pol-ACCS.AUTHN` | `pol-ACCS` |  |
| A.8.6 | 4 | `pol-PERF.TARGETS`, `pol-PERF.MEASURE`, `pol-PERF.PEAK`, `pol-OBSV.USAGE` | `pol-PERF`, `pol-OBSV` |  |
| A.8.7 | 2 | `pol-TRUS.MALWARE`, `pol-TRUS.RUNMAL` | `pol-TRUS` |  |
| A.8.8 | 6 | `pol-VURM.SCAN`, `pol-VURM.RANK`, `pol-VURM.TIMEBOX`, `pol-VURM.DISCLOS`, `pol-VURM.SHIP`, `pol-VURM.OVERDUE` | `pol-VURM` |  |
| A.8.9 | 6 | `pol-EVER.ASSETS`, `pol-EVER.ORPHAN`, `pol-PIPE.CONFIG`, `pol-PIPE.ASCODE`, `pol-PIPE.MANUAL`, `pol-ENVS.BASELIN` | `pol-EVER`, `pol-PIPE`, `pol-ENVS` |  |
| A.8.10 | 2 | `pol-DATA.DELETE`, `pol-DATA.LINGER` | `pol-DATA` |  |
| A.8.11 | 2 | `pol-DATA.MASK`, `pol-DATA.UNMASK` | `pol-DATA` |  |
| A.8.12 | 2 | `pol-DATA.LOGS`, `pol-DATA.LEAK` | `pol-DATA` | Data leakage prevention covers endpoints, networks and stores. `LOGS` covers the logging instance, and `LEAK` the content leaving by a route open for other traffic. `pol-MEXP.EGRESS` controls which routes are open, and cites `A.8.20` only. |
| A.8.13 | 4 | `pol-RECV.BACKUP`, `pol-RECV.RESTORE`, `pol-RECV.OFFSITE`, `pol-RECV.UNTEST` | `pol-RECV` |  |
| A.8.14 | 4 | `pol-RECV.OFFSITE`, `pol-RECV.DEGRADE`, `pol-RECV.FAILOVR`, `pol-RECV.REDUND` | `pol-RECV` | `REDUND` is a SHOULD, so the estate does not commit to running everything across more than one failure domain. `FAILOVR` requires that a failover it does claim is tested, as `UNTEST` does for a backup. |
| A.8.15 | 5 | `pol-OBSV.CENTRAL`, `pol-OBSV.RETAIN`, `pol-OBSV.SECRETS`, `pol-OBSV.CORREL`, `pol-SCRT.LOGS` | `pol-OBSV`, `pol-SCRT` |  |
| A.8.16 | 4 | `pol-OBSV.HEALTH`, `pol-OBSV.SECMON`, `pol-OBSV.ALERTS`, `pol-OBSV.BLIND` | `pol-OBSV` |  |
| A.8.17 | 1 | `pol-OBSV.CLOCKS` | `pol-OBSV` |  |
| A.8.18 | 1 | `pol-ACCS.UTILS` | `pol-ACCS` |  |
| A.8.19 | 5 | `pol-PIPE.DEPLOY`, `pol-PIPE.SAMEART`, `pol-PIPE.LOCAL`, `pol-TRUS.REPO`, `pol-TRUS.MUTATE` | `pol-PIPE`, `pol-TRUS` |  |
| A.8.20 | 4 | `pol-MEXP.DENY`, `pol-MEXP.TRANSIT`, `pol-MEXP.PEERID`, `pol-MEXP.EGRESS` | `pol-MEXP` |  |
| A.8.21 | 3 | `pol-MEXP.DENY`, `pol-MEXP.PRIVATE`, `pol-MEXP.PUBLIC` | `pol-MEXP` |  |
| A.8.22 | 2 | `pol-MEXP.SEGMENT`, `pol-MEXP.LATERAL` | `pol-MEXP` |  |
| A.8.24 | 5 | `pol-DATA.CRYPTO`, `pol-DATA.RETIRE`, `pol-MEXP.PEERID`, `pol-MEXP.WEAKEN`, `pol-SCRT.KEYS` | `pol-DATA`, `pol-MEXP`, `pol-SCRT` |  |
| A.8.25 | 4 | `pol-AUTV.INTEG`, `pol-EVER.PARITY`, `pol-AGNT.EQUAL`, `pol-SECD.HIRISK` | `pol-AUTV`, `pol-EVER`, `pol-AGNT`, `pol-SECD` |  |
| A.8.26 | 5 | `pol-INTC.SECURE`, `pol-INTC.HOLDS`, `pol-INTC.EXPOSE`, `pol-SECD.REQS`, `pol-SECD.HIRISK` | `pol-INTC`, `pol-SECD` |  |
| A.8.27 | 4 | `pol-INTC.SPEC`, `pol-INTC.VERSION`, `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-INTC`, `pol-SECD` | `INTC.VERSION` is a compatibility discipline. Nothing in its wording is motivated by security, so it is a poor fit beside `SECD.DESIGN` and `SECD.THREAT`. |
| A.8.28 | 2 | `pol-SECD.CODING`, `pol-SECD.CODEREV` | `pol-SECD` |  |
| A.8.29 | 6 | `pol-AUTV.INTEG`, `pol-AUTV.BLOCK`, `pol-AUTV.LEVELS`, `pol-AUTV.REGRESS`, `pol-AUTV.BYPASS`, `pol-VURM.REGRESS` | `pol-AUTV`, `pol-VURM` | The control asks for security testing in development and acceptance. `pol-AUTV` establishes a blocking test gate, and lists no security testing among the checks it runs. The scanning that covers the control is `pol-VURM.SCAN` and `pol-VURM.RANK`, cited to `A.8.8` alone. `pol-AUTV` and `pol-VURM` cover this control together, and neither covers it alone. |
| A.8.30 | 2 | `pol-AGNT.PROV`, `pol-AGNT.UNPROV` | `pol-AGNT` | Outsourced development assumes the supplier is accountable for the work, and `pol-AGNT` states that an agent is not. See the `NIST AI RMF 1.0` `GOVERN` row. Kept for the trail an assessor expects to follow, and not as an honest fit. |
| A.8.31 | 4 | `pol-ENVS.SPLIT`, `pol-ENVS.SAMEDEF`, `pol-ENVS.PROMOTE`, `pol-ENVS.DEBUG` | `pol-ENVS` |  |
| A.8.32 | 9 | `pol-EVER.HISTORY`, `pol-EVER.INTENT`, `pol-EVER.BRANCH`, `pol-EVER.PARITY`, `pol-PIPE.TRACE`, `pol-PIPE.REVERT`, `pol-PIPE.GATES`, `pol-PIPE.FLAGS`, `pol-PIPE.MANUAL` | `pol-EVER`, `pol-PIPE` |  |
| A.8.33 | 2 | `pol-ENVS.MASK`, `pol-ENVS.UNMASK` | `pol-ENVS` |  |

### NIST AI RMF 1.0

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#nist-ai-rmf).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| GOVERN | 1 | `pol-AGNT.ACCEPT` | `pol-AGNT` | The clause this framework exists for. `ISO 27001:2022` covers `pol-AGNT` with `A.8.30`, written for contracting a build out, and an agent is not accountable for its work. |
| MANAGE | 3 | `pol-AGNT.EQUAL`, `pol-AGNT.DUTIES`, `pol-AGNT.ACCESS` | `pol-AGNT` |  |
| MAP | 2 | `pol-AGNT.PROV`, `pol-AGNT.UNPROV` | `pol-AGNT` |  |
| MEASURE | 2 | `pol-AGNT.CONFID`, `pol-AGNT.SELFVER` | `pol-AGNT` |  |

### NIST SSDF 1.1

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#nist-ssdf).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| PO.1 | 2 | `pol-SECD.REQS`, `pol-SECD.HIRISK` | `pol-SECD` |  |
| PO.3 | 4 | `pol-AUTV.INTEG`, `pol-AUTV.MACHINE`, `pol-PIPE.DEPLOY`, `pol-PIPE.ASCODE` | `pol-AUTV`, `pol-PIPE` |  |
| PO.4 | 4 | `pol-AUTV.BLOCK`, `pol-AUTV.BYPASS`, `pol-AUTV.DISABLE`, `pol-PIPE.GATES` | `pol-AUTV`, `pol-PIPE` |  |
| PO.5 | 8 | `pol-ENVS.SPLIT`, `pol-ENVS.CREDS`, `pol-ENVS.SAMEDEF`, `pol-ENVS.PROMOTE`, `pol-ENVS.MASK`, `pol-ENVS.DEBUG`, `pol-ENVS.REUSE`, `pol-ENVS.UNMASK` | `pol-ENVS` | `pol-ENVS` covers `PO.5.1` across eight clauses. `PO.5.2` hardens the endpoints development happens on, and endpoint, workstation and laptop appear in no policy here. |
| PS.1 | 6 | `pol-EVER.ASSETS`, `pol-EVER.HISTORY`, `pol-EVER.BRANCH`, `pol-EVER.ORPHAN`, `pol-EVER.SHARED`, `pol-EVER.SIGNED` | `pol-EVER` |  |
| PS.2 | 4 | `pol-PIPE.SAMEART`, `pol-PIPE.TRACE`, `pol-PIPE.LOCAL`, `pol-TRUS.ATTEST` | `pol-PIPE`, `pol-TRUS` |  |
| PS.3 | 2 | `pol-TRUS.REPO`, `pol-TRUS.MUTATE` | `pol-TRUS` |  |
| PW.1 | 2 | `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-SECD` |  |
| PW.4 | 4 | `pol-TRUS.INVENT`, `pol-TRUS.SCREEN`, `pol-TRUS.SOURCE`, `pol-TRUS.UNTRUST` | `pol-TRUS` |  |
| PW.5 | 1 | `pol-SECD.CODING` | `pol-SECD` |  |
| PW.6 | 2 | `pol-AUTV.REPRO`, `pol-AUTV.BITWISE` | `pol-AUTV` |  |
| PW.7 | 2 | `pol-EVER.PARITY`, `pol-SECD.CODEREV` | `pol-EVER`, `pol-SECD` |  |
| PW.8 | 3 | `pol-AUTV.LEVELS`, `pol-AUTV.REGRESS`, `pol-VURM.SCAN` | `pol-AUTV`, `pol-VURM` | PW.8 is dynamic testing: DAST, fuzzing and penetration testing. `pol-VURM.SCAN` commits to looking through the lifecycle, and not only at release. `LEVELS` and `REGRESS` cover test levels and defect regression, the same pattern as `ISO 27001:2022`.A.8.29. |
| RV.1 | 3 | `pol-VURM.SCAN`, `pol-VURM.DISCLOS`, `pol-VURM.INDEP` | `pol-VURM` |  |
| RV.2 | 4 | `pol-VURM.RANK`, `pol-VURM.TIMEBOX`, `pol-VURM.SHIP`, `pol-VURM.OVERDUE` | `pol-VURM` |  |

### OWASP ASVS 4.0

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#owasp-asvs).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| V1 | 3 | `pol-SECD.REQS`, `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-SECD` |  |
| V2 | 1 | `pol-ACCS.AUTHN` | `pol-ACCS` |  |
| V4 | 3 | `pol-INTC.EXPOSE`, `pol-ACCS.LEAST`, `pol-MEXP.DENY` | `pol-INTC`, `pol-ACCS`, `pol-MEXP` |  |
| V5 | 1 | `pol-INTC.SECURE` | `pol-INTC` |  |
| V13 | 1 | `pol-INTC.SECURE` | `pol-INTC` |  |

### SLSA 1.1

Filed under `Inspiration` in [`frameworks.md`](../frameworks.md#slsa).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| build-L1 | 1 | `pol-TRUS.TRACE` | `pol-TRUS` | Provenance exists. `pol-TRUS.TRACE` follows a deployed artefact back to the build that produced it. |
| build-L2 | 1 | `pol-TRUS.ATTEST` | `pol-TRUS` | Provenance signed by a hosted build platform. `pol-TRUS.ATTEST` is a SHOULD, so a build that cannot prove where an artefact came from needs a reason, and not a deviation. |

### UK GDPR

Filed under `Obliged` in [`frameworks.md`](../frameworks.md#uk-gdpr).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| Art.5(1)(b) | 1 | `pol-DATA.PURPOSE` | `pol-DATA` |  |
| Art.5(1)(c) | 1 | `pol-DATA.MINIMAL` | `pol-DATA` |  |
| Art.5(1)(d) | 1 | `pol-DERV.CHECK` | `pol-DERV` | The principle covers all personal data. `pol-DERV`'s scope excludes what a user supplied, so the citation covers computed data alone. Its second limb, erasure or rectification without delay, is covered by no clause. |
| Art.5(1)(e) | 2 | `pol-DATA.DELETE`, `pol-DATA.LINGER` | `pol-DATA` |  |
| Art.5(1)(f) | 3 | `pol-ACCS.LEAST`, `pol-DATA.CRYPTO`, `pol-MEXP.TRANSIT` | `pol-ACCS`, `pol-DATA`, `pol-MEXP` |  |
| Art.6 | 2 | `pol-DATA.LAWFUL`, `pol-DATA.BASIS` | `pol-DATA` |  |
| Art.15 | 1 | `pol-DATA.RIGHTS` | `pol-DATA` |  |
| Art.16 | 1 | `pol-DATA.RIGHTS` | `pol-DATA` |  |
| Art.17 | 2 | `pol-DATA.RIGHTS`, `pol-DATA.REVIVE` | `pol-DATA` |  |
| Art.18 | 1 | `pol-DATA.RIGHTS` | `pol-DATA` |  |
| Art.20 | 1 | `pol-DATA.RIGHTS` | `pol-DATA` |  |
| Art.21 | 1 | `pol-DATA.RIGHTS` | `pol-DATA` |  |
| Art.25 | 1 | `pol-DATA.UNMASK` | `pol-DATA` | Article 25 is the duty to build data protection in by design and by default. `UNMASK` covers one instance of it, masking below production. `pol-SECD.REQS` states the same idea and has no citation to this article. |
| Art.28 | 1 | `pol-DATA.SHARE` | `pol-DATA` | Article 28 governs the controller-to-processor relationship. The written processing agreement `SHARE` requires is that instrument. Sharing with an independent controller is a case no clause here covers. |
| Art.30 | 1 | `pol-DATA.INVENT` | `pol-DATA` | Article 30 requires a documented record of processing activities. `INVENT` lists what the information is, where it is kept, who owns it and who it is shared with, which is most of that record. Purposes, categories of subject and retention are required by no clause. |
| Art.32 | 2 | `pol-DATA.CRYPTO`, `pol-DATA.RETIRE` | `pol-DATA` | Article 32(1)(c) requires restoring access to personal data in good time. `pol-RECV.RESTORE` cites `Art.32(1)(d)` for testing that it works, and the objectives in `RTORPO` and `BACKUP` have no UK GDPR citation. |
| Art.32(1)(a) | 2 | `pol-DATA.MASK`, `pol-DATA.CRYPTO` | `pol-DATA` |  |
| Art.32(1)(d) | 3 | `pol-INCR.DRILL`, `pol-RECV.RESTORE`, `pol-VURM.INDEP` | `pol-INCR`, `pol-RECV`, `pol-VURM` |  |
| Art.33 | 1 | `pol-INCR.NOTIFY` | `pol-INCR` |  |
| Art.33(5) | 1 | `pol-INCR.EVIDENC` | `pol-INCR` |  |
| Art.34 | 1 | `pol-INCR.INFORM` | `pol-INCR` |  |
| Art.35 | 1 | `pol-SECD.IMPACT` | `pol-SECD` |  |
| Art.44 | 1 | `pol-DATA.XBORDER` | `pol-DATA` |  |

### WCAG 2.2 AA

Filed under `Self-obligated` in [`frameworks.md`](../frameworks.md#wcag).

| Reference | Citations | Clauses | Policies | Note |
|-----------|-----------|---------|----------|------|
| the framework entire | 2 | `pol-A11Y.CONFORM`, `pol-A11Y.VENDOR` | `pol-A11Y` | Self-obligated under `pol-A11Y`, the only commitment binding the estate to it. `CONFORM` verifies against it, and `VENDOR` assesses components against it. Neither states a method, and the testing that would settle a criterion an automated check cannot is `ASSIST`, a SHOULD. |

## Controls this corpus owns and does not answer

`frameworks.md` asks that a control belonging to this estate and cited by nothing be named, and not mapped generously.
One is.

**`ISO 27001:2022`.A.5.7, threat intelligence.** The control collects external information about threats, to inform the
estate's own risk decisions. Nothing here produces or consumes it. `pol-VURM.DISCLOS` cites `A.8.8` instead: it is a
route inward, for somebody outside to report a vulnerability, which is a different activity. The absence is the
finding.

`A.5.7` is absent from the ownership table, so it belongs to nobody else either.

## What this leaves open

**A reference on one clause is one citation from losing its coverage.** The tool counts the citations it can see.
Whether a control still has honest coverage, and whether an uncited reference should be removed from the register,
belongs to whoever confirms this report.
