---
id: fix-0001
type: fix
tier: normative
status: active
symptom-keywords: [actionlint, brew, homebrew, linters, macos, "no matching distribution found", pip, python, requirements.txt, yamllint]
verified:
  - { at: 2026-09-02T17:27:31Z, by: human:paul.law }
review-by: "2027-03-09"
owner: human:paul.law
tags: [ linting, macos, setup ]
---

# pip cannot find the yamllint version the lint job pins

`Fix: fix-0001` `ACTIVE`

## Symptom

`pip install -r .github/requirements.txt` fails on a Mac, and the release it says is missing is the one the file pins.

```text
ERROR: Could not find a version that satisfies the requirement yamllint==1.38.0 (from versions: 0.1.0, ... 1.37.1)
ERROR: No matching distribution found for yamllint==1.38.0
```

The versions it offers stop one release short of the pin.

## Cause

macOS ships `/usr/bin/python3` at 3.9, and yamllint 1.38.0 needs Python 3.10 or newer. pip narrows the index to the
releases the running interpreter can take, so the pinned one is on PyPI and invisible from here. The error quotes the
pin because that is what it was asked for, and says nothing about the interpreter.

## Resolution

1. Install both linters from Homebrew, which brings a Python of its own.

   ```sh
   brew install yamllint actionlint
   ```

2. Check each version against what the `lint` job pins.

   ```sh
   yamllint --version    # 1.38.0
   actionlint --version  # 1.7.12
   ```

Homebrew serves the pinned yamllint and the pinned actionlint together, and the `lint` job takes actionlint from `go
install` rather than from the requirements file.

## Why it happens

The `lint` job sets Python up at 3.13 before it installs, so CI never meets this. `.github/requirements.txt` records
what that job takes rather than what a laptop can run. Dependabot moves the pin, so the floor rises again whenever
yamllint drops a Python version.

## How we found it

Read the version list rather than the message above it. A list that stops just short of the pin is the interpreter
filtering the index, and `/usr/bin/python3 --version` is the next command. A genuinely bad pin offers no neighbouring
releases either.

## Related

* [std-CI] carries what a workflow here owes, and the `lint` job is one of its checks.

[std-CI]: ../standards/workflows.md
