# Filing a finding on GitHub

Read this where the tracker you picked has a `target` of `github`. You have the body in a file, and a yes to it.

This file states what each step achieves and what GitHub does that you could not work out, and gives no command. Write
each command yourself, for the shell and the client you have, because a command written for another shell fails in
yours.

## What GitHub does

| Fact                                                                                   | Why it matters                                                         |
|----------------------------------------------------------------------------------------|------------------------------------------------------------------------|
| `base` is the repository, as `https://github.com/<owner>/<repo>`                       | the owner and the repository are what the issue is filed against       |
| `gh` refuses a label the repository does not have, and refuses the whole issue with it | a repository nobody set up for findings has no `kac:finding` label     |
| The REST API drops every label silently for an account without push access             | the check on the issue is what shows a label that did not land         |
| GitHub stores an issue body as the Markdown it was sent                                | the block reaches triage with its fences, so the body needs no marking |

## File the issue

**Open one issue on that repository**, with your title, the body from the file character for character, and the label
`kac:finding`. `gh` is the client that signs in to GitHub.

**Where the client refuses the label, open the issue once more with no label.** The refusal names the label, and nothing
was filed, so the second attempt is the only issue. Say in your reply that you filed it unlabelled and why, because the
observation matters more than the mark.

## Check the issue

Read the issue back, and check two things:

* **The body opens on the line ```` ```yaml kac-finding ````**, so the block arrived with its fences.
* **The labels include `kac:finding`**, unless you filed it unlabelled.

**Where a check fails, report the field and the issue's URL, and file nothing again.** The issue exists, and a second
copy is a duplicate somebody has to close. Whoever is with you can correct the one that is there.

## Where filing fails

**A failure on sign-in or on permission ends with the body printed.** Check whether you are signed in to that host. A
repository with issues turned off, or an account without the rights to open one, ends the same way. Say which it was.

Then say what you did, as `SKILL.md` describes.
