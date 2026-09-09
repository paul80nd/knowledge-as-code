---
id: rpt-framework-coverage
type: report
tier: descriptive
status: active
owner: human:alex.doe
generated: { at: 2026-09-08T10:45:58Z, by: kac/0.23.0+955356490b0bfa1f7caa3863fa100346dd6849c1 }
sources:
  - { resource: example-engineering, version: "0.12.0" }
verified:
  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }
tags: [ coverage, frameworks ]
---

# Framework coverage

`Report: rpt-framework-coverage` `ACTIVE`

## Limits

This reads `example-engineering` and what it imports. A clause uncovered here may well be covered in a corpus consuming this one, and every consumer answers for its own coverage.

No column here says a clause is verified. A control names a standard and not a rule, so it vouches for a whole document whatever it checks inside it.

## What a single citation carries

A reference cited by one clause is one edit from losing its coverage of a control. Deleting that clause, or rewording
its `Alignment` cell, drops the reference from this corpus with nothing saying so, and the register goes on listing the
framework as though it were still reached.

That matters differently by standing. `Obliged` names something outside us: an auditor follows a control back to the
commitment implementing it, and a control with no commitment left is a finding. `Inspiration` binds nothing, so a
reference losing its last citation is provenance we stopped claiming rather than coverage we lost.

The `Note` column carries what [frameworks.md](../frameworks.md) says about a particular reference. Most rows have
nothing to add to the clauses beside them, and an empty cell says so.

## Totals

| Framework | Standing | References | Cited once |
|-----------|----------|------------|------------|
| Azure WAF | Inspiration | 4 | 0 |
| DORA metrics | Inspiration | 4 | 1 |
| EN 301 549 | Obliged | 1 | 0 |
| ISO 27001:2022 | Obliged | 59 | 18 |
| NIST AI RMF 1.0 | Inspiration | 4 | 1 |
| NIST SSDF 1.1 | Inspiration | 16 | 3 |
| OWASP ASVS 4.0 | Inspiration | 5 | 4 |
| PSBAR 2018 | Obliged | 1 | 1 |
| SLSA 1.1 | Inspiration | 2 | 2 |
| UK GDPR | Obliged | 17 | 15 |
| WCAG 2.2 AA | Self-obligated | 1 | 1 |
| **Total** | | **114** | **46** |

## References

### Azure WAF

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| cost-optimization | `pol-COST.ATTRIB`, `pol-COST.VISIBLE`, `pol-COST.WEIGHED`, `pol-COST.SIZING`, `pol-COST.ANOMALY`, `pol-COST.UNUSED`, `pol-COST.UNOWNED`, `pol-COST.ERODE` | `pol-COST` | The only framework reaching `pol-COST`. Nothing external will ever oblige us to manage our own cloud spend. |
| operational-excellence | `pol-DERV.RUNLOG`, `pol-OBSV.CENTRAL`, `pol-OBSV.CLOCKS`, `pol-OBSV.RETAIN`, `pol-OBSV.HEALTH`, `pol-OBSV.ALERTS`, `pol-OBSV.BLIND` | `pol-DERV`, `pol-OBSV` | |
| performance-efficiency | `pol-PERF.TARGETS`, `pol-PERF.MEASURE`, `pol-PERF.DEFECT`, `pol-PERF.PEAK`, `pol-PERF.NOTEST` | `pol-PERF` | |
| reliability | `pol-OBSV.SLO`, `pol-RECV.RTORPO`, `pol-RECV.BACKUP`, `pol-RECV.RESTORE`, `pol-RECV.OFFSITE`, `pol-RECV.TIMEOUT`, `pol-RECV.DEGRADE`, `pol-RECV.IDEMPOT`, `pol-RECV.UNTEST`, `pol-RECV.RETRY`, `pol-RECV.SHED`, `pol-RECV.CHAOS` | `pol-OBSV`, `pol-RECV` | Cited at the pillar rather than the recommendation, which is renumbered as the framework is revised. |

### DORA metrics

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| change-failure-rate | `pol-AUTV.BLOCK`, `pol-AUTV.REGRESS` | `pol-AUTV` | |
| deploy-frequency | `pol-PIPE.DEPLOY` | `pol-PIPE` | |
| lead-time | `pol-AUTV.INTEG`, `pol-AUTV.LEVELS`, `pol-AUTV.OFTEN` | `pol-AUTV` | |
| recovery-time | `pol-PIPE.REVERT`, `pol-PIPE.PROGDEL` | `pol-PIPE` | |

### EN 301 549

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| §9 | `pol-A11Y.CONFORM`, `pol-A11Y.VENDOR` | `pol-A11Y` | Incorporates WCAG 2.1 level AA by reference. `WCAG 2.2 AA` is the later version and the bar we hold. |

### ISO 27001:2022

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| A.5.3 | `pol-AGNT.DUTIES`, `pol-ACCS.DUTIES` | `pol-AGNT`, `pol-ACCS` | |
| A.5.4 | `pol-DEVI.OWNER`, `pol-DEVI.CUSTOM` | `pol-DEVI` | |
| A.5.5 | `pol-INCR.NOTIFY` | `pol-INCR` | |
| A.5.7 | `pol-VURM.DISCLOS` | `pol-VURM` | |
| A.5.8 | `pol-SECD.REQS`, `pol-SECD.ACTIONS` | `pol-SECD` | |
| A.5.9 | `pol-DATA.LOCATE` | `pol-DATA` | |
| A.5.12 | `pol-DATA.CLASS` | `pol-DATA` | |
| A.5.14 | `pol-DATA.CRYPTO`, `pol-DATA.XBORDER`, `pol-DATA.SHARE` | `pol-DATA` | |
| A.5.15 | `pol-ACCS.LEAST` | `pol-ACCS` | |
| A.5.16 | `pol-ACCS.NAMED`, `pol-ACCS.DIRECT` | `pol-ACCS` | |
| A.5.17 | `pol-SCRT.STORE`, `pol-SCRT.ROTATE`, `pol-SCRT.LEAKED`, `pol-SCRT.EMBED`, `pol-SCRT.REUSE`, `pol-SCRT.LOGS` | `pol-SCRT` | |
| A.5.18 | `pol-ACCS.RECERT`, `pol-ACCS.REVOKE` | `pol-ACCS` | |
| A.5.19 | `pol-TRUS.SOURCE`, `pol-TRUS.UNTRUST` | `pol-TRUS` | |
| A.5.21 | `pol-TRUS.INVENT`, `pol-TRUS.SCREEN`, `pol-TRUS.TRACE` | `pol-TRUS` | |
| A.5.22 | `pol-TRUS.REVIEW` | `pol-TRUS` | |
| A.5.23 | `pol-TRUS.CLOUD`, `pol-TRUS.EXIT` | `pol-TRUS` | |
| A.5.24 | `pol-INCR.PROCESS`, `pol-INCR.DRILL` | `pol-INCR` | |
| A.5.25 | `pol-INCR.TRIAGE` | `pol-INCR` | |
| A.5.26 | `pol-INCR.COMMS`, `pol-INCR.EVIDENC`, `pol-INCR.ADHOC` | `pol-INCR` | |
| A.5.27 | `pol-INCR.LEARN`, `pol-INCR.ACTIONS`, `pol-INCR.TOOSOON` | `pol-INCR` | |
| A.5.28 | `pol-INCR.EVIDENC` | `pol-INCR` | |
| A.5.29 | `pol-INCR.RECOVER`, `pol-RECV.DEGRADE` | `pol-INCR`, `pol-RECV` | |
| A.5.30 | `pol-RECV.RTORPO` | `pol-RECV` | |
| A.5.32 | `pol-TRUS.LICENCE` | `pol-TRUS` | |
| A.5.34 | `pol-DATA.LAWFUL` | `pol-DATA` | |
| A.5.36 | `pol-DEVI.RECORD`, `pol-DEVI.CLOSE`, `pol-DEVI.PERM` | `pol-DEVI` | |
| A.5.37 | `pol-KNOW.DOCS`, `pol-KNOW.SYNC`, `pol-KNOW.HEADS` | `pol-KNOW` | |
| A.6.8 | `pol-INCR.REPORT` | `pol-INCR` | |
| A.8.2 | `pol-ACCS.ADMIN`, `pol-ACCS.SHARED`, `pol-ACCS.PERSIST`, `pol-ACCS.ZERO` | `pol-ACCS` | |
| A.8.3 | `pol-ACCS.LEAST`, `pol-ENVS.CREDS`, `pol-ENVS.REUSE` | `pol-ACCS`, `pol-ENVS` | |
| A.8.4 | `pol-EVER.BRANCH`, `pol-EVER.SHARED` | `pol-EVER` | |
| A.8.5 | `pol-ACCS.AUTHN` | `pol-ACCS` | |
| A.8.6 | `pol-PERF.TARGETS`, `pol-PERF.MEASURE`, `pol-PERF.PEAK` | `pol-PERF` | |
| A.8.7 | `pol-TRUS.MALWARE` | `pol-TRUS` | |
| A.8.8 | `pol-VURM.SCAN`, `pol-VURM.RANK`, `pol-VURM.TIMEBOX`, `pol-VURM.SHIP`, `pol-VURM.OVERDUE` | `pol-VURM` | |
| A.8.9 | `pol-EVER.ASSETS`, `pol-EVER.ORPHAN`, `pol-PIPE.CONFIG`, `pol-PIPE.ASCODE`, `pol-PIPE.MANUAL`, `pol-ENVS.BASELIN` | `pol-EVER`, `pol-PIPE`, `pol-ENVS` | |
| A.8.10 | `pol-DATA.DELETE`, `pol-DATA.LINGER` | `pol-DATA` | |
| A.8.11 | `pol-DATA.UNMASK` | `pol-DATA` | |
| A.8.12 | `pol-DATA.LOGS` | `pol-DATA` | |
| A.8.13 | `pol-RECV.BACKUP`, `pol-RECV.RESTORE`, `pol-RECV.OFFSITE`, `pol-RECV.UNTEST` | `pol-RECV` | |
| A.8.14 | `pol-RECV.OFFSITE`, `pol-RECV.DEGRADE`, `pol-RECV.REDUND` | `pol-RECV` | |
| A.8.15 | `pol-OBSV.CENTRAL`, `pol-OBSV.RETAIN`, `pol-OBSV.SECRETS`, `pol-OBSV.CORREL`, `pol-SCRT.LOGS` | `pol-OBSV`, `pol-SCRT` | |
| A.8.16 | `pol-OBSV.HEALTH`, `pol-OBSV.SECMON`, `pol-OBSV.ALERTS`, `pol-OBSV.BLIND` | `pol-OBSV` | |
| A.8.17 | `pol-OBSV.CLOCKS` | `pol-OBSV` | |
| A.8.18 | `pol-ACCS.ADMIN` | `pol-ACCS` | |
| A.8.19 | `pol-PIPE.DEPLOY`, `pol-PIPE.SAMEART`, `pol-PIPE.LOCAL`, `pol-TRUS.REPO`, `pol-TRUS.MUTATE` | `pol-PIPE`, `pol-TRUS` | |
| A.8.20 | `pol-MEXP.DENY`, `pol-MEXP.TRANSIT`, `pol-MEXP.PEERID`, `pol-MEXP.EGRESS` | `pol-MEXP` | |
| A.8.21 | `pol-MEXP.DENY`, `pol-MEXP.PRIVATE`, `pol-MEXP.PUBLIC` | `pol-MEXP` | |
| A.8.22 | `pol-MEXP.SEGMENT`, `pol-MEXP.LATERAL` | `pol-MEXP` | |
| A.8.24 | `pol-DATA.CRYPTO`, `pol-DATA.RETIRE`, `pol-MEXP.PEERID`, `pol-MEXP.WEAKEN`, `pol-SCRT.KEYS` | `pol-DATA`, `pol-MEXP`, `pol-SCRT` | |
| A.8.25 | `pol-AUTV.INTEG`, `pol-EVER.PARITY`, `pol-AGNT.EQUAL`, `pol-SECD.HIRISK` | `pol-AUTV`, `pol-EVER`, `pol-AGNT`, `pol-SECD` | |
| A.8.26 | `pol-INTC.SECURE`, `pol-INTC.HOLDS`, `pol-INTC.EXPOSE`, `pol-SECD.REQS`, `pol-SECD.HIRISK` | `pol-INTC`, `pol-SECD` | |
| A.8.27 | `pol-INTC.SPEC`, `pol-INTC.VERSION`, `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-INTC`, `pol-SECD` | |
| A.8.28 | `pol-SECD.CODING`, `pol-SECD.CODEREV` | `pol-SECD` | |
| A.8.29 | `pol-AUTV.INTEG`, `pol-AUTV.BLOCK`, `pol-AUTV.LEVELS`, `pol-AUTV.REGRESS`, `pol-AUTV.BYPASS`, `pol-VURM.REGRESS` | `pol-AUTV`, `pol-VURM` | |
| A.8.30 | `pol-AGNT.PROV`, `pol-AGNT.UNPROV` | `pol-AGNT` | |
| A.8.31 | `pol-ENVS.SPLIT`, `pol-ENVS.SAMEDEF`, `pol-ENVS.PROMOTE`, `pol-ENVS.DEBUG` | `pol-ENVS` | |
| A.8.32 | `pol-EVER.HISTORY`, `pol-EVER.INTENT`, `pol-EVER.BRANCH`, `pol-EVER.PARITY`, `pol-PIPE.TRACE`, `pol-PIPE.REVERT`, `pol-PIPE.GATES`, `pol-PIPE.FLAGS`, `pol-PIPE.MANUAL` | `pol-EVER`, `pol-PIPE` | |
| A.8.33 | `pol-ENVS.MASK`, `pol-ENVS.UNMASK` | `pol-ENVS` | |

### NIST AI RMF 1.0

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| GOVERN | `pol-AGNT.ACCEPT` | `pol-AGNT` | The clause this framework exists for. 27001 answers `pol-AGNT` with `A.8.30`, written for contracting a build out, and an agent is not accountable for its work. |
| MANAGE | `pol-AGNT.EQUAL`, `pol-AGNT.DUTIES`, `pol-AGNT.ACCESS` | `pol-AGNT` | |
| MAP | `pol-AGNT.PROV`, `pol-AGNT.UNPROV` | `pol-AGNT` | |
| MEASURE | `pol-AGNT.CONFID`, `pol-AGNT.SELFVER` | `pol-AGNT` | |

### NIST SSDF 1.1

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| PO.1 | `pol-SECD.REQS`, `pol-SECD.HIRISK` | `pol-SECD` | |
| PO.3 | `pol-AUTV.INTEG`, `pol-AUTV.MACHINE`, `pol-PIPE.DEPLOY`, `pol-PIPE.ASCODE` | `pol-AUTV`, `pol-PIPE` | |
| PO.4 | `pol-AUTV.BLOCK`, `pol-AUTV.BYPASS`, `pol-AUTV.DISABLE`, `pol-PIPE.GATES` | `pol-AUTV`, `pol-PIPE` | |
| PO.5 | `pol-ENVS.SPLIT`, `pol-ENVS.CREDS`, `pol-ENVS.SAMEDEF`, `pol-ENVS.PROMOTE`, `pol-ENVS.MASK`, `pol-ENVS.DEBUG`, `pol-ENVS.REUSE`, `pol-ENVS.UNMASK` | `pol-ENVS` | `pol-ENVS` is this practice nearly in its entirety, arrived at independently. |
| PS.1 | `pol-EVER.ASSETS`, `pol-EVER.HISTORY`, `pol-EVER.BRANCH`, `pol-EVER.ORPHAN`, `pol-EVER.SHARED`, `pol-EVER.SIGNED` | `pol-EVER` | |
| PS.2 | `pol-PIPE.SAMEART`, `pol-PIPE.TRACE`, `pol-PIPE.LOCAL`, `pol-TRUS.ATTEST` | `pol-PIPE`, `pol-TRUS` | |
| PS.3 | `pol-TRUS.REPO`, `pol-TRUS.MUTATE` | `pol-TRUS` | |
| PW.1 | `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-SECD` | |
| PW.4 | `pol-TRUS.INVENT`, `pol-TRUS.SCREEN`, `pol-TRUS.SOURCE`, `pol-TRUS.UNTRUST` | `pol-TRUS` | |
| PW.5 | `pol-SECD.CODING` | `pol-SECD` | |
| PW.6 | `pol-AUTV.REPRO`, `pol-AUTV.BITWISE` | `pol-AUTV` | |
| PW.7 | `pol-EVER.PARITY` | `pol-EVER` | |
| PW.8 | `pol-AUTV.LEVELS`, `pol-AUTV.REGRESS` | `pol-AUTV` | |
| RV.1 | `pol-VURM.SCAN`, `pol-VURM.DISCLOS`, `pol-VURM.INDEP` | `pol-VURM` | |
| RV.2 | `pol-VURM.RANK`, `pol-VURM.TIMEBOX`, `pol-VURM.SHIP`, `pol-VURM.OVERDUE` | `pol-VURM` | |
| RV.3 | `pol-VURM.REGRESS` | `pol-VURM` | |

### OWASP ASVS 4.0

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| V1 | `pol-SECD.REQS`, `pol-SECD.DESIGN`, `pol-SECD.THREAT` | `pol-SECD` | |
| V2 | `pol-ACCS.AUTHN` | `pol-ACCS` | |
| V4 | `pol-INTC.EXPOSE` | `pol-INTC` | |
| V5 | `pol-SECD.CODING` | `pol-SECD` | |
| V13 | `pol-INTC.SECURE` | `pol-INTC` | |

### PSBAR 2018

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| reg.8 | `pol-A11Y.PUBLISH` | `pol-A11Y` | Obliged in law in the markets we serve, and reached by one clause. The duty includes keeping a published accessibility statement current. |

### SLSA 1.1

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| build-L1 | `pol-TRUS.TRACE` | `pol-TRUS` | Provenance exists. `pol-TRUS.TRACE` follows a deployed artefact back to the build that produced it. |
| build-L2 | `pol-TRUS.ATTEST` | `pol-TRUS` | Provenance signed by a hosted build platform. `pol-TRUS.ATTEST` is a SHOULD, so a build that cannot prove where an artefact came from needs a reason rather than a deviation. |

### UK GDPR

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| Art.5(1)(c) | `pol-DATA.MINIMAL` | `pol-DATA` | |
| Art.5(1)(d) | `pol-DERV.CHECK` | `pol-DERV` | |
| Art.5(1)(e) | `pol-DATA.DELETE`, `pol-DATA.LINGER` | `pol-DATA` | |
| Art.5(1)(f) | `pol-DATA.LOGS` | `pol-DATA` | |
| Art.6 | `pol-DATA.LAWFUL` | `pol-DATA` | |
| Art.25 | `pol-DATA.UNMASK` | `pol-DATA` | |
| Art.28 | `pol-DATA.SHARE` | `pol-DATA` | |
| Art.30 | `pol-DATA.LOCATE` | `pol-DATA` | |
| Art.32 | `pol-DATA.CRYPTO`, `pol-DATA.RETIRE` | `pol-DATA` | |
| Art.32(1)(a) | `pol-DATA.CLEAR` | `pol-DATA` | |
| Art.32(1)(d) | `pol-VURM.SCAN` | `pol-VURM` | |
| Art.33 | `pol-INCR.NOTIFY` | `pol-INCR` | |
| Art.33(5) | `pol-INCR.EVIDENC` | `pol-INCR` | |
| Art.34 | `pol-INCR.INFORM` | `pol-INCR` | |
| Art.35 | `pol-SECD.IMPACT` | `pol-SECD` | |
| Art.44 | `pol-DATA.XBORDER` | `pol-DATA` | |
| Ch.III | `pol-DATA.RIGHTS` | `pol-DATA` | |

### WCAG 2.2 AA

| Reference | Clauses | Policies | Note |
|-----------|---------|----------|------|
| the framework entire | `pol-A11Y.CONFORM` | `pol-A11Y` | Self-obligated under `pol-A11Y`, which is the only thing holding us to it. |

## What this leaves open

**A reference on one clause is one citation from losing its coverage.** The tool counts the citations it can see. Whether a control still has honest coverage, and whether an uncited reference should be removed from the register, belongs to whoever verifies this report.

