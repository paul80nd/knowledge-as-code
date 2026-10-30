---
id: ctl-alert-owner-and-action
type: control
tier: normative
status: active
verifies: [ std-OBS.ALERT ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ alerting, observability, on-call ]
---

# Nothing checks that an alert names an owner and a first action

`Control: ctl-alert-owner-and-action` `ACTIVE`

No check fails an alert rule that leaves out who acts on it, or what they do first.

## What it checks

* [std-OBS.ALERT] says an alert "**MUST** name the accountable owner and the first action to
  take".

## How it works

Nothing runs. The check this rule needs is a pipeline job over the alert rule files. It lists every rule with an
`alert:` key whose `owner` label or `first_action` annotation is empty:

```sh
yq '.groups[].rules[] | select(has("alert"))
    | select((.labels.owner // "") == "" or (.annotations.first_action // "") == "") | .alert' alerts/*.yaml
```

The job fails where that prints a name. It fails as well where either value is a placeholder: `TODO`, `TBD`, `FIXME`
or a value in angle brackets. The same query can run as a pre-commit check, or as a Claude Code hook on `Write` and
`Edit`.

## Coverage and gaps

A reviewer reading the alert rules is the only thing that notices a missing owner today.

The standard names no field for the owner or the first action, so the check assumes `owner` and `first_action`. It
proves a value is present, and a reviewer still judges whether the first action is one somebody can take. The other
rules under [std-OBS.ALERT] go unchecked, and so does
[std-OBS.TRACE].

[std-OBS.TRACE]: ../standards/common/observability.md#trace-one-request-reads-as-one-timeline
[std-OBS.ALERT]: ../standards/common/observability.md#alert-somebody-acts-on-every-alert
