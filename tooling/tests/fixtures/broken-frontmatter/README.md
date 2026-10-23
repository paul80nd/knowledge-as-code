One ADR whose body is valid and whose frontmatter breaks every frontmatter rule.

The body has its H1, every required section, a Y-statement and a verdict on each alternative. So every finding in the
golden comes from the frontmatter, and a change to any frontmatter rule shows up here alone.

* `priority` is a key the type does not declare, for `unknown-key`.
* `status` comes before `tier`, for `key-order`.
* `owner` is missing, for `required-field`.
* `type: policy` sits in `adrs/`, for `type-matches-folder`.
* `tier: descriptive` is not the ADR's tier, for `tier-matches-type`.
* `decided-on: 2026/06/12` is unquoted and in the wrong shape, for `date-quoted` and `date-format` together.
* `bare-key` fires twice. `supersedes` opens with an unquoted placeholder, which YAML reads as a mapping, and
  `deciders: null` writes an absent value as something other than a bare key.

The template beside it is valid, and the golden counts it as one template with no findings.
