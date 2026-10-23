Records whose frontmatter values break the rules on a value's type, range, pattern or count.

Each record has one bad value, and its filename says which. ADRs carry most of them. A fix and a tool are here
because only those types declare the field a rule reads.

* `enum` and `enum-lowercase` land together on a `status` of `Draft`, which is out of range and capitalised.
* `list` fires on a `tags` written as a scalar.
* `field-pattern` fires on two `tags` entries, on an `owner` with no `human:` or `role:` prefix, and on a tool's
  `licence`.
* `date-format` fires on `2026-13-40`, a date the calendar does not have.
* `min-items` fires on a fix with two `symptom-keywords` where the schema asks for three.
* `timestamp-format` fires twice on a fix's `verified.at`: once on a moment the calendar does not have, and once on a
  date written where a moment belongs.
* `frontmatter-parses` fires on an unterminated quote. It is the only finding against that record, because the
  validator stops there.

`ValueChecks.feature` reads this corpus for what `validate` promises about a field's value.
