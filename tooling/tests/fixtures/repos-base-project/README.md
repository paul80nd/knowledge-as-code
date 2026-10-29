A corpus whose `repos-base` is an Azure DevOps project, written percent-encoded.

`payments` states `My Repo` and links it under `_git` in the project, in lower case. It reports nothing.

`ledger` states `Proj X/Ledger`, naming a project the base already names. `mirrors-repo-links` reports the entry once,
naming `<repo>` as the form it needs, and passes over the link to that repository.
