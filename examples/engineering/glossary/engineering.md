---
id: gls-engineering
type: glossary
tier: descriptive
status: draft
owner: human:paul.law
narrows: gls-knowledge-as-code
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

**Not:** a service we build. A component comes from outside, and is screened before we adopt it ([pol-TRUS].SCREEN).

### Could

The weakest obligation level a clause takes, below `SHOULD`, `MUST` and `MUST NOT`. It marks something we would like and
have not committed to. A team asking to take one on under continuous improvement is told yes, and that it is worth a
little time.

**Not:** RFC 2119's MAY, which expresses no preference either way. A `COULD` here expresses a preference for doing it.

### Must

The level a clause takes where a framework we are obliged to requires it, or where not doing it would be reckless. No
team negotiates one down. Departing from one is a recorded deviation ([pol-DEVI]).

**Not:** everything we would like. A `MUST` binds us. `SHOULD` and `COULD` state good practice.

### Must not

A `MUST` written as a prohibition, with the same weight and the same test. Some prohibitions admit no deviation at all.
A policy lists those in its Exceptions section.

**Not:** a firmly worded `SHOULD`. A policy whose prohibition is softened in practice is no longer a policy.

### Personal data

Information about a living person who is identified, or who could be identified from it combined with anything else we
store. PII is another name for the same thing.

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
opinion, religious belief, trade union membership, genetic data, and biometric data used to identify someone. This
estate treats criminal offence data the same way.

**Not:** personal data, which is the wider class. [pol-DATA].LOGS bars only this narrower set from a log line.

### Should

The level a clause takes where a capable engineering function would do this, and we would spend time to meet it. A team
asking to take one on under continuous improvement is told yes, and that it is worth real time.

**Not:** a `MUST` we are being polite about. Nothing obliges us to a `SHOULD`. A team that has not met one yet is
maturing, and is not in breach.

### We

The engineering function, working as one technology team within its roles. A clause binds the function, not any one
team. It applies to a person through whoever owns the thing it governs, and a thing nobody owns is a gap that clause has
found. Adherence is joint: somebody else might do the work but you still say so when it is missing or broken.

**Not:** the team that happens to be reading. A platform, a shared service or a specialist meets some clauses on
everybody's behalf, and an unanswered clause is still not somebody else's problem.

[pol-DATA]: ../policies/security/data-data-protection.md#clauses
[pol-DEVI]: ../policies/governance/devi-deviations-are-recorded.md#clauses
[pol-SCRT]: ../policies/security/scrt-secrets-are-never-embedded.md#clauses
[pol-TRUS]: ../policies/security/trus-trusted-components.md#clauses
[UK GDPR]: ../frameworks.md#uk-gdpr
