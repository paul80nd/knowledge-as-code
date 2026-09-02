---
id: gls-engineering
tier: descriptive
status: draft
owner: paul.law
narrows: gls-knowledge-as-code
review-by: "2027-09-02"
tags: [ data-protection, dependencies, privacy, secrets ]
---

# Engineering

`Glossary: gls-engineering` `DRAFT`

The words this estate uses about itself, which the records here assume.

## Scope

The estate: what our systems are built from, what they hold, and what we owe the people that data describes. A word
about the framework this corpus runs on belongs to the framework's own glossary, which every corpus shares.

An entry here may cite a record, which the shared glossary may not. Where a policy turns on a word, the entry names the
clause that owns the detail.

## Terms

### Component

A third-party or open-source part of a system we did not write: a library, a base image, a build tool, or a service we
call. Most reach us through a package manager such as npm or NuGet, and the whole route in is the supply chain
[pol-TRUS] governs.

**Not:** a service we build. A component is admitted from outside and screened before we adopt it ([pol-TRUS].SCREEN).

### Personal data

Information about a living person who is identified, or who could be identified from it together with anything else we
hold. PII is the same thing under another name.

**Not:** sensitive personal data, which is a narrow set inside it. A name, a postal address and an email address are
personal data, and none of them is sensitive.

### Secret

A value that grants access and would let somebody in if it leaked: a password, an API key, a session token, a
certificate private key, a connection string. [pol-SCRT] says where one is held.

**Not:** sensitive data. A secret protects data. It is not the data it protects.

### Sensitive data

Data whose exposure would cause harm, whether or not it describes a person: an unpublished figure, a security finding,
a customer list. [pol-DATA].CLASS owns the classes and decides which data falls in which.

**Not:** sensitive personal data. A security finding is sensitive and describes nobody, so no rights attach to it.

### Sensitive personal data

The special categories [UK GDPR] sets apart: health, sex life, sexual orientation, racial or ethnic origin, political
opinion, religious belief, trade union membership, genetic data, and biometric data used to identify someone. This
estate handles criminal offence data the same way.

**Not:** personal data, which is the wider class. [pol-DATA].LOGS bars only this narrower set from a log line.

[pol-DATA]: ../policies/security/data-data-protection.md#clauses
[pol-SCRT]: ../policies/security/scrt-secrets-are-never-embedded.md#clauses
[pol-TRUS]: ../policies/security/trus-trusted-components.md#clauses
[UK GDPR]: ../frameworks.md#uk-gdpr
