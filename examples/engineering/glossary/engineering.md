---
id: gls-engineering
type: glossary
tier: descriptive
status: draft
owner: human:paul.law
review-by: "2027-09-02"
tags: [ data-protection, dependencies, obligation-levels, privacy, secrets ]
---

# Engineering

`Glossary: gls-engineering` `DRAFT`

The words this estate uses about itself. The records here assume these meanings.

## Scope

This glossary covers the estate: what our systems are built from, what data they store, and our obligations to the
people that data describes. A word about the framework this corpus runs on belongs in the framework's own glossary,
which every corpus shares.

An entry here may cite a record; the shared glossary may not. Where a policy depends on a word, the entry cites the
clause that states the detail.

## Terms

### Component

A third-party or open-source part of a system we did not write: a library, a base image, a build tool, or a service we
call. Most arrive through a package manager such as npm or NuGet. That route in is the supply chain [pol-TRUS]
governs.

**Not:** a service we build, which the framework's `component-type` field also calls a component. A component here
comes from outside, and is screened before we adopt it ([pol-TRUS].SCREEN).

### Could

The weakest obligation level a clause takes, below `SHOULD`, `MUST` and `MUST NOT`. It marks something we would like and
have not committed to. A team asking to take one on under continuous improvement is told yes, and that it is worth a
little time.

**Not:** RFC 2119's MAY, which expresses no preference either way. A `COULD` here expresses a preference for doing it.

### May

The level a rule in a standard takes to permit something a reader might otherwise think is ruled out. A policy grants no
permission, so a policy clause never takes it.

**Not:** a `COULD`, which states something we would like and permits nothing.

### Must

The level a clause takes where a framework we are obliged to requires it, or where not doing it would be reckless. A
`MUST` binds us, where `SHOULD` and `COULD` state good practice. No team negotiates one down.

### Must not

A `MUST` written as a prohibition, with the same weight and the same test. Some prohibitions admit no deviation at all.
A policy lists those in its Exceptions section.

**Not:** a firmly worded `SHOULD`.

### Personal data

Information about a living person who is identified, or who could be identified from it. Identification counts any
means reasonably likely to be used, by us or by another person, as [UK GDPR] Article 4(1) and recital 26 set out.

**Avoid:** PII, a US term with a different scope. Write personal data.

**Not:** sensitive personal data, which is a narrower set inside it. A name, a postal address and an email address are
personal data, and none of them is sensitive.

### Secret

A value that grants access, so a leak lets somebody in: a password, an API key, a session token, a certificate private
key, a connection string. [pol-SCRT] states where one is stored.

**Not:** sensitive data. A secret protects data. It is not the data it protects.

### Sensitive data

Data whose exposure would cause harm, whether or not it describes a person: an unpublished figure, a security finding,
a customer list. [pol-DATA].CLASS defines the classes and states which data belongs to each.

**Not:** sensitive personal data. A security finding is sensitive and describes nobody, so no rights attach to it.

### Sensitive personal data

The special categories [UK GDPR] sets apart: health, sex life, sexual orientation, racial or ethnic origin, political
opinion, religious or philosophical belief, trade union membership, genetic data, and biometric data used to identify
someone. This estate treats criminal offence data the same way.

**Also:** special category data, which is what [UK GDPR] calls it.

**Not:** personal data, which is the wider class. [pol-DATA].LOGS bars only this narrower set from a log line.

### Should

The level a clause takes where a capable engineering function would do this, and we would spend time to meet it. A team
asking to take one on under continuous improvement is told yes, and that it is worth real time.

**Not:** a `MUST` we are being polite about, or the `SHOULD` in a standard. Nothing in a policy obliges us to a
`SHOULD`. A team that has not met one yet is maturing, and is not in breach. In a [standard](../standards.md), a
`SHOULD` names the usual way to reach that standard's `MUST`, and whoever departs from it says why in the change.

### We

The engineering function, working as one technology team within its roles. A clause binds the function and not any one
team, so a platform, a shared service or a specialist meets some clauses on everybody's behalf.

**Not:** whichever team did the work. A clause applies to a person through whoever owns the thing it governs, and a
thing nobody owns is a gap that clause has found. Adherence is joint: somebody else might do the work, but you still say
so when it is missing or broken, and an unanswered clause is never somebody else's problem.

[pol-DATA]: ../policies/security/data-data-protection.md#clauses
[pol-SCRT]: ../policies/security/scrt-secrets-management.md#clauses
[pol-TRUS]: ../policies/security/trus-trusted-components.md#clauses
[UK GDPR]: ../frameworks.md#uk-gdpr
