---
id: gls-search
tier: descriptive
status: draft
owner: mira.okonjo
narrows: gls-example-libraries
review-by: "2027-08-12"
tags: [ search ]
---

# Search

`Glossary: gls-search` `DRAFT`

The words the search service uses, including the ones the rest of the estate uses differently.

## Scope

How a reader's words reach a catalogue record: what is typed, what is matched, and what orders the results. The things
being searched for belong to the whole estate, so they sit in its glossary rather than here.

## Terms

### Facet

A field a reader narrows results by — branch, format, availability — offered with a count beside each value.

### Query

What a reader typed, after parsing and before matching.

**Not:** the request the service received. One request carries a query, its facets and its paging.

### Relevance

The score that orders results, drawn from how often and where a query's words appear.

### Title

The indexed field carrying a work's name, and one of several a query matches against.

**Not:** a title in the catalogue, which is the work itself — see [gls-example-libraries.title].

Owned by [svc-search].

[gls-example-libraries.title]: example-libraries.md#title
[svc-search]: ../services/search.md
