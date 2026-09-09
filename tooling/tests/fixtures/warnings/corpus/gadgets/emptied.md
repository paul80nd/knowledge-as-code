---
id: gad-emptied
type: gadget
tier: descriptive
status: live
owner: human:alex.doe
facets:
---

# A gadget carrying an optional field nobody filled in

`Gadget: gad-emptied` `LIVE`

## What it does

Carries `facets` with no value. The field is optional, so the empty key says what leaving the key out says, and
`empty-optional-key` reports it. A required field left empty is the other case, and `required-field` is what reports
that one.
