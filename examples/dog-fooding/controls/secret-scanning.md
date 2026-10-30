---
id: ctl-secret-scanning
type: control
tier: normative
status: active
verifies: [ eng:std-SECRET.NOWHERE ]
mechanism: runtime-alert
frequency: continuous
evidence: The repository's secret scanning alerts, under the Security tab on GitHub.
applies-to:
  - all
review-by: "2027-10-30"
owner: human:paul.law
tags: [ github, secrets, security ]
---

# GitHub secret scanning alerts on a secret anywhere in the history

`Control: ctl-secret-scanning` `ACTIVE`

GitHub scans this repository and its history for secrets, and raises an alert on each one it finds.

## What it checks

* `eng:std-SECRET.NOWHERE` says a pipeline "**MUST** run a secret scanner over the repository and its history, and fail
  on a finding". This control shows that a scanner reads the repository and its history. It fails nothing.

## How it works

Secret scanning is enabled in the repository's settings. GitHub scans every branch and the whole git history for
secrets that match its provider patterns, and scans each new push the same way. A match opens an alert on
the repository's Security tab.

Read the open alerts with `gh api repos/paul80nd/knowledge-as-code/secret-scanning/alerts`. An empty list means no
alert is open.

Push protection is enabled as well, and blocks a push that contains a secret matching a provider pattern. A pusher can
bypass the block by giving a reason, so this control does not count push protection as proof. It is a second layer in
front of the scan.

## Coverage and gaps

The scan fails no pipeline. An alert waits for somebody to read it, and a pull request merges while one is open.

The scan matches provider patterns alone. Scanning for non-provider patterns is disabled, and so are validity checks.
A secret that matches no provider pattern reaches the history unseen.

The scan cannot remove a secret from the history. An alert says that a secret has already been pushed.
