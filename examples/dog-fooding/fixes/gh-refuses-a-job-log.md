---
id: fix-gh-refuses-a-job-log
type: fix
tier: normative
status: draft
symptom-keywords: [ "##[error]", --allow-escape-sequences, --log-failed, actions, ci, "failing check", gh, "gh api",
  "gh run view", "job log", "logs will be available when it is complete", "red check", "run is still in progress",
  "terminal escape sequences" ]
review-by: "2027-04-30"
owner: human:paul.law
tags: [ ci, debugging, github-cli ]
---

# gh refuses to print a failing CI job's log

`Fix: fix-gh-refuses-a-job-log` `DRAFT`

## Symptom

You want the `##[error]` line behind a red check on a pull request, and `gh` prints no log.

`gh run view --job <job-id> --log-failed` refuses while any other job in the run is still going:

```text
run is still in progress; logs will be available when it is complete
```

`gh api` against the job's log refuses as soon as its output is not a terminal. It prints this, and writes nothing to
the file or the pipe you gave it:

```text
the response contains terminal escape sequences; pass --allow-escape-sequences to output it anyway
```

Piped to `grep`, the call finds no match, which reads as a log with no error in it.

## Environment

The `gh` CLI, seen on 2.100.0, reading a job of `.github/workflows/kac.yml` on this repository. The job's log contains
ANSI colour codes.

## Cause

`gh run view` fetches logs for a whole run, so it waits until every job has finished. `gh api` reads one job's log
straight away. It refuses to print terminal escape sequences unless you pass `--allow-escape-sequences`. The refusal
goes to standard error, so a file or a pipe receives no log.

## Resolution

1. Read the job id from the check's URL, which `gh pr checks <pr>` prints. It is the number after `/job/`.
2. Fetch that job's log with `gh api`, allow the escape sequences, and strip them with `sed`:

   ```sh
   gh api --allow-escape-sequences repos/paul80nd/knowledge-as-code/actions/jobs/<job-id>/logs \
     | sed 's/\x1b\[[0-9;]*m//g' | cut -c30-
   ```

   `cut -c30-` drops the timestamp at the head of each line.

3. Add `| grep -n '##\[error\]'` to go straight to the error line.

The command prints the log as plain text, and `grep -c $'\x1b'` over its output counts no escape byte. It works while
the rest of the run is still in progress.

## Why it happens

`gh` guards a terminal against escape sequences an API response could carry, and treats a job log as one of those
responses. `gh run view` has no `--allow-escape-sequences` flag, so only `gh api` can pass the guard.

## How we found it

A session reacting to a red `corpora` check took five commands to read one error line. `--log-failed` refused because
the run was still going, and `gh api` piped to `grep` matched nothing. Redirecting `gh api` to a file left the file
empty, and the refusal on standard error named the flag to pass.

## Related

* [std-CI] states the rules the workflow follows, and `kac.yml` is the workflow whose jobs this reads.

[std-CI]: ../standards/workflows.md
