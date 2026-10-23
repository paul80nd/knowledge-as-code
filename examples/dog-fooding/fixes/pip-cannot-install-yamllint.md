---
id: fix-0001
type: fix
tier: normative
status: active
symptom-keywords: [ actionlint, brew, homebrew, linters, macos, "no matching distribution found", pip, python,
  requirements.txt, yamllint, zizmor ]
verified:
  - { at: 2026-09-18T11:40:00Z, by: human:paul.law }
review-by: "2027-03-09"
owner: human:paul.law
tags: [ linting, macos, setup ]
---

# pip cannot find the linter versions the lint job pins

`Fix: fix-0001` `ACTIVE`

## Symptom

`pip install -r .github/requirements.txt` fails on a Mac, and the release it says is missing is the one the file pins.

```text
ERROR: Could not find a version that satisfies the requirement yamllint==1.38.0 (from versions: 0.1.0, ... 1.37.1)
ERROR: No matching distribution found for yamllint==1.38.0
```

pip stops at the first pin it cannot meet, which is yamllint. zizmor fails the same way on its own:

```text
ERROR: Could not find a version that satisfies the requirement zizmor==1.31.1 (from versions: 0.8.0, ... 1.16.3)
ERROR: No matching distribution found for zizmor==1.31.1
```

The versions each error offers stop short of the pin.

## Environment

macOS, running Apple's `/usr/bin/python3`, which is 3.9. `.github/requirements.txt` pins yamllint at 1.38.0 and zizmor
at 1.31.1.

## Cause

yamllint 1.38.0 needs Python 3.10 or newer, and zizmor 1.31.1 needs Python 3.11 or newer. pip filters the index down to
the releases the running interpreter supports, so each pinned release is on PyPI and invisible from 3.9. The error
quotes the pin it was given, and says nothing about the interpreter.

## Resolution

1. Install the three linters from Homebrew, which installs a Python of its own.

   ```sh
   brew install yamllint actionlint zizmor
   ```

2. Check each version against what the `lint` job pins.

   ```sh
   yamllint --version    # 1.38.0
   actionlint --version  # 1.7.12
   zizmor --version      # 1.31.1
   ```

Homebrew installs its current release of each, so a version can differ from its pin until Dependabot moves the pin. The
`lint` job installs actionlint with a `go install` line, so `.github/requirements.txt` does not list it.

## Why it happens

The `lint` job installs Python 3.13 before it installs the requirements, so CI never sees this failure.
`.github/requirements.txt` records what that job installs, not what a laptop can run. Dependabot moves both pins, so the
minimum Python version rises again whenever yamllint or zizmor drops an old one.

## How we found it

A session installing the linters on this Mac read the list of versions in the error rather than the message above it.
The list stopped one release short of a release PyPI does have, and that gap is the signal.
`/usr/bin/python3 --version` reported 3.9, which the error never mentions. A later session met the same gap on zizmor,
where the list stopped at 1.16.3.

## Related

* [std-CI] states the rules a workflow here follows, and the `lint` job is one of its checks.

[std-CI]: ../standards/workflows.md
