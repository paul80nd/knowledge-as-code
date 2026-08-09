# FAQs

Problems we have hit before, with the fix that worked.

**[→ Index](faqs/_index.md)**

## What is an FAQ?

One document per problem: the symptom as you would actually encounter it, what causes it, how to fix it, and why it
happens. Confirmed by a human, so it carries authority.

Add one when an investigation cost real time. Future you will be grateful, and so will the next session that hits it.

## Why we use them

The same problems resurface, and the cost is paid again by whoever hits them next. An FAQ converts two hours of
debugging into a thirty-second search — provided it is findable, which is what `symptom-keywords` is for.

They are also the destination for the promotion path: a [discovery](/discoveries) that turns out to be real, general and
current becomes an FAQ.

## Scope

An FAQ is **confirmed**. A human has verified the problem is real, the fix works, and both are still current. That is
what separates it from a [discovery](/discoveries), which is captured cheaply and might be wrong or already fixed.

**Never write straight to an FAQ from a session.** Capture as a discovery and let promotion do the work — an agent
cannot confirm its own observations.

Other boundaries:

* **[Runbook](/runbooks)** — if it needs a diagnosis tree and an escalation path, it is a runbook. An FAQ has a known
  fix, not a decision procedure.
* **[Standard](/standards)** — if the real answer is "people should stop doing the thing that causes this", the fix is a
  rule, and that needs an [ADR](/adrs) first.
* **One problem per document.** A page of assorted gotchas cannot be found by symptom, which defeats the purpose.

## Metadata

<!-- BEGIN GENERATED: schema-faqs -->

| Field              | Req | Type   | Notes                                                                                               |
|--------------------|-----|--------|-----------------------------------------------------------------------------------------------------|
| `id` †             | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                               |
| `tier` †           | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.                |
| `status` †         | ●   | enum   | `fixed-upstream` means the cause is gone; the entry stays for whoever searches for it.              |
| `owner` †          | ●   | string | A named person, never a team alias.                                                                 |
| `tags` †           |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                    |
| `symptom-keywords` | ●   | list   | Over-fill it: error text, service names, and what someone types before they know the cause.         |
| `applies-to`       |     | list   | Service ids this answer concerns.                                                                   |
| `promoted-from`    |     | id     | The discovery this was promoted from.                                                               |
| `confirmed-by`     | ●   | string | A named human. An FAQ nobody confirmed is a discovery — this field is what separates the two tiers. |
| `confirmed-on`     | ●   | date   | Quoted. When a human last confirmed the answer still holds.                                         |
| `review-by`        | ●   | date   | Quoted. Drives the staleness report.                                                                |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `active` · `superseded` · `fixed-upstream`                          |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-faqs -->

## Adding an FAQ

1. Copy [`_template.md`](faqs/_template.md) to `<slug>.md`, named for the symptom rather than the cause — that is what
   people search for.
2. Make the H1 the symptom as encountered, in the words the error message or the user would use.
3. Be generous with `symptom-keywords`. Include the literal error text, the service names, and the words someone would
   type who doesn't yet know what is wrong.
4. Record `confirmed-by` and `confirmed-on` — a named person and a real date. An FAQ nobody confirmed is a discovery.
5. Set `review-by`. Fixes go stale when the thing they fix gets rewritten.

**Conventions**

* **Symptom first, cause second, fix third.** The reader arrives with a symptom and nothing else.
* **Record how you found it**, not just what it was. The diagnostic route is often more reusable than the fix.
* **If the root cause is still open**, say so, and raise it somewhere it can be tracked. An FAQ is not a place to park
  unowned work.

## What CI checks

<!-- BEGIN GENERATED: checks-faqs -->

| Check                       | Level   | What it verifies                                                                                                              |
|-----------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                                           |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                                |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                                             |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                                       |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                                |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                                       |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                                  |
| `min-items`                 | error   | A list field carries at least as many entries as its schema asks for.                                                         |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                                    |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                                    |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                                                 |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                               |
| `h1`                        | error   | The document has an H1.                                                                                                       |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.                      |
| `required-section`          | error   | Every required section heading is present.                                                                                    |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                                                |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                               |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                                      |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                                      |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                              |
| `unused-definition`         | warning | A link definition that nothing references.                                                                                    |
| `one-problem-per-document`  | warning | One Symptom section, because an FAQ is found by its symptom. A page of assorted gotchas cannot be, which defeats the purpose. |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule              | What it would verify                                                                   |
|-------------------|----------------------------------------------------------------------------------------|
| `human-confirmed` | `confirmed-by` must be present and must not be an agent, a session id or a team alias. |

<!-- END GENERATED: checks-faqs -->
