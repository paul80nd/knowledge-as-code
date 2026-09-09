# Fixes

Problems we have hit before, with the resolution that worked.

**[→ Index](fixes/_index.md)**

## What is a fix?

One document per problem: the symptom as you would encounter it, what causes it, how to resolve it, and why it happens.
*"pip cannot find the version the lint job pins."* *"An `.editorconfig` change does nothing in the IDE."* Somebody has
verified it, so a fix carries authority.

Add one when an investigation cost real time. You will hit the same problem again, and so will the next session.

## Why we use them

The same problems come back, and the next person to hit one pays the cost again. A fix turns two hours of debugging
into a thirty-second search, provided the words that person searches for are in `symptom-keywords`.

An agent session does most of the searching here, and it arrives with the symptom and nothing else. Every session that
finds its answer here is one that does not spend an hour reaching the answer the last one already had.

A [discovery](discoveries.md) can also become a fix. A human promotes it once the observation proves real, general and
current.

## Scope

A fix is **verified**. Somebody has checked that the problem is real, that the resolution works, and that both are
still current. A [discovery](discoveries.md) arrives with none of that: nobody reviews one, and it might be wrong or
already fixed.

**Who did the checking is recorded, and a reader weighs it.** An agent that reproduced the symptom and ran the
resolution has done real work, and `verified` names it with its version the way a tool names itself. Read the list to
see how far the fix has been taken: agents alone leave it machine-confirmed, and one `human:` line makes it
human-reviewed. An export carries that reading as `trust`, derived from the list so that one place names who checked.

**Never write straight to a fix from a session.** An agent cannot verify its own observation, so capture a discovery
and let somebody else check it at promotion.

Other boundaries:

* **[Runbook](runbooks.md).** If it needs a diagnosis tree and an escalation path, it is a runbook. A fix has one known
  resolution.
* **[Standard](standards.md).** If the real answer is "people should stop doing the thing that causes this", the
  resolution is a rule, and it belongs in a standard.
* **One problem per document.** Someone arriving with a symptom matches the first one on the page and never reads the
  second.

## Metadata

<!-- BEGIN GENERATED: schema-fixes -->

| Field                | Value                                  | Notes                                                                                       |
|----------------------|----------------------------------------|---------------------------------------------------------------------------------------------|
| `id` *†              | string                                 | Stable, unique across the corpus, never reused. Format set by the type.                     |
| `type` *†            | string                                 | The type's singular name. Fixed for the type. CI checks it matches the folder.              |
| `tier` *†            | `normative`                            | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.         |
| `status` *†          | `active` `superseded` `fixed-upstream` | `fixed-upstream` means the cause is gone. The entry stays for whoever searches for it.      |
| `owner` *†           | string                                 | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                      |
| `sources` †          | list                                   | Where the content came from, one entry per source.                                          |
| `tags` †             | list                                   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                            |
| `symptom-keywords` * | list                                   | Over-fill it: error text, service names, and what someone types before they know the cause. |
| `applies-to`         | list                                   | Service ids this fix concerns.                                                              |
| `promoted-from`      | id                                     | The discovery this was promoted from.                                                       |
| `verified` *         | list                                   | Every verification this fix has had, oldest first, one line each.                           |
| `review-by` *        | date                                   | Quoted. The date by which someone verifies this is still true.                              |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-fixes -->

## Adding a fix

1. Copy [`_template.md`](fixes/_template.md) to `<slug>.md`, named for the symptom rather than the cause. That is what
   people search for.
2. Make the H1 the symptom as encountered, in the words the error message or the user would use.
3. Over-fill `symptom-keywords` with the search terms that failed you the day you hit the problem.
4. Add a `verified` line naming who checked the resolution and the moment they did it.
5. Set `review-by`. A resolution goes stale when the thing it repairs is rewritten.

**Conventions**

* **Symptom first, cause second, resolution third.** The reader arrives with a symptom and nothing else.
* **Record how you found it**, not just what it was. The diagnostic route is often more reusable than the resolution.
* **If the root cause is still open**, say so, and raise it somewhere it can be tracked. A fix is not a place to park
  unowned work.

## What CI checks

<!-- BEGIN GENERATED: checks-fixes -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `empty-optional-key`        | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `timestamp-format`          | error   | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                                |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `min-items`                 | error   | A list field carries at least as many entries as its schema asks for.                                           |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `entry-shape / entry-key`   | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.           |
| `type-matches-folder`       | error   | `type` matches the singular type name the record's folder declares.                                             |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label is the id of the record it leads to, written as that record carries it.                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `verified-by-a-known-actor` | error   | A verification names a person or a producer, and never a post.                                                  |
| `one-problem-per-document`  | warning | One Symptom section, because a fix is found by its symptom.                                                     |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                     | What it would verify                      |
|--------------------------|-------------------------------------------|
| `raiser-does-not-verify` | An agent does not verify a fix it raised. |

<!-- END GENERATED: checks-fixes -->
