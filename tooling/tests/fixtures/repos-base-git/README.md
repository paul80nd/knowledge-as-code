A corpus whose `repos-base` ends in an Azure DevOps project's `_git` segment.

`payments` states `My Repo` and links it on `acme.visualstudio.com`, the organisation's other host. It reports nothing.

`ledger` states `Proj X/Ledger`, naming a project the base already names. `mirrors-repo-links` reports the entry once,
naming `<repo>` as the form it needs, and passes over the link to that repository.
