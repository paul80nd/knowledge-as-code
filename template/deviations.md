# Deviations

Where we knowingly break one of our own rules, who agreed to it, and when it is looked at again.

**[→ Index](deviations/_index.md)**

## What is a deviation?

A written, owned, time-bound departure from a [policy](policies.md) or a [standard](standards.md). It names the rule it
departs from, says what we are doing instead and why, and gives a date somebody has to come back to it.

It also covers a shortcut that breaks no rule. Skipping the retry logic to ship on Friday, knowing the next person has
to add it, is technical debt. Nothing forbids it, and the debt is real, so it is recorded the same way. Such a record
names `none` in `departs-from`.

## Why we use them

A rule with no way out gets broken quietly. Somebody ships under pressure and tells nobody. A year later the estate
carries a departure that nobody can tell from never having known the rule.

A deviation is the way out, and it costs one page. The departure is decided rather than drifted into, one person has
their name against the risk, and a date brings it back. That is what makes the exception clause in a policy mean
something.

The count going up is the type working. A folder holding twelve deviations is not a worse estate than one holding
none. It is the same estate with twelve things written down.

## Scope

A deviation records a departure from a rule. It does not change the rule.

| Rule                                                | Deviation                                                                           |
|-----------------------------------------------------|-------------------------------------------------------------------------------------|
| "Secrets **MUST** come from the vault."             | "The batch importer reads one key from an environment variable until 2027-03-31."   |
| "Every endpoint **MUST** carry a conformance test." | "The legacy reports endpoint has none, and is retired in the next quarter instead." |

Where everybody is deviating from the same clause, the clause is wrong. Rewrite the policy or the standard, and close
the deviations it was carrying.

A deviation is also not:

* **A decision about how we build.** "We chose Postgres over MySQL" is an [ADR](adrs.md), and it stays true. A deviation
  is written to be closed.
* **An audit finding.** A finding is something an auditor noticed afterwards. A deviation is something we agreed to
  before, or immediately after an incident left no time.
* **A backlog item.** The work that closes a deviation belongs in the issue tracker. The record says what has to be true
  for it to close, and the tracker carries the work.

**A deviation sits in the corpus whose people accepted the risk.** It names the clause it departs from, wherever that
clause was written. A corpus consuming a governance layer holds its own departures from that layer's policies. Nothing
flows upward in an export: a corpus reads what it consumes and writes nothing back. The corpus stating a policy never
sees what was taken against it, so an auditor reads every corpus in the estate rather than the governance layer alone.

## Metadata

<!-- BEGIN GENERATED: schema-deviations -->

| Field            | Value                     | Notes                                                                                             |
|------------------|---------------------------|---------------------------------------------------------------------------------------------------|
| `id` *†          | string                    | Stable, unique across the corpus, never reused. Format set by the type.                           |
| `type` *†        | string                    | The type's singular name. Fixed for the type. CI checks it matches the folder.                    |
| `tier` *†        | `normative`               | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.               |
| `status` *†      | `active` `draft` `closed` | Whether the deviation is in force, still being agreed, or closed.                                 |
| `owner` *†       | string                    | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                            |
| `tags` †         | list                      | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                  |
| `departs-from` * | list                      | Policy or standard clause ids this departs from, as `pol-TRUS.SCREEN`, or `none`.                 |
| `accepted-on`    | date                      | The day the named owner accepted the risk. Required when `status != draft`.                       |
| `review-by` *    | date                      | The day this is looked at again. Every deviation carries one.                                     |
| `closed-on`      | date                      | The day the gap was fixed, or the risk consciously re-accepted. Required when `status == closed`. |
| `applies-to`     | list                      | Service ids, or `all`.                                                                            |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-deviations -->

## Adding a deviation

1. Copy [`_template.md`](deviations/_template.md) to `<slug>.md`. Deviations use slug ids: `dev-legacy-report-secrets`.
2. Name each clause you are departing from in `departs-from`, one entry per clause. Write `none` for a shortcut that
   breaks no rule.
3. Put the person who accepted the risk in `owner`. Someone with the authority to accept it, never a team.
4. Set `accepted-on` to the day they agreed.
5. Set `review-by` to the day somebody has to look at this again.
6. Say what compensates. A monitoring alert, a manual check, a smaller scope: something that makes the risk survivable.

**Conventions**

* **Write it before you depart**, or immediately afterwards where an incident left no time.
* **One deviation per departure.** A page collecting every exception to one policy loses the owner, the date and the id
  that a citation needs.
* **Name the risk plainly.** A deviation that reads as a defence of the departure hides what the reviewer needs.
* **Close it by fixing the gap, or by re-accepting the risk with the same scrutiny as the first time.** Set `closed-on`
  to the day that happened, and move the status with it.

## What CI checks

<!-- BEGIN GENERATED: checks-deviations -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
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
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `review-after-acceptance`   | error   | The review date falls after the day the risk was accepted.                                                      |
| `not-open-ended`            | warning | The record does not claim the departure is permanent or open-ended.                                             |
| `expiry`                    | warning | An active deviation is still inside the review date it carries.                                                 |

<!-- END GENERATED: checks-deviations -->
