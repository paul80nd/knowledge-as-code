# Filing a finding on GitHub

Read this where the tracker you picked has a `target` of `github`. You have the body in a file, and a yes to it.

`base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh issue create --repo <owner>/<repo> --title "<the title>" --body-file <path> --label kac:finding
```

**Run it again with no `--label` where the repository has no `kac:finding` label.** The error names the label. Say in
your reply that you filed it unlabelled and why, because the observation matters more than the mark.

**A failure on sign-in or on permission ends with the body printed.** `gh auth status` says whether you are signed in
to that host. A repository with issues turned off, or an account without the rights to open one, ends the same way.
Say which it was.

Then say what you did, as `SKILL.md` describes.
