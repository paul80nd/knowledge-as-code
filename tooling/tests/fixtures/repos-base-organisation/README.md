A corpus whose `repos-base` is an Azure DevOps organisation, with its services in several projects.

`payments` states two repositories in two projects, each spelt with spaces and capitals. One link is written on
`dev.azure.com` in lower case and percent-encoded, and the other on `acme.visualstudio.com`. Both resolve, and it
reports nothing.

`ledger` states a bare repository name, which an organisation cannot resolve. `mirrors-repo-links` reports the entry
once, naming `<project>/<repo>` as the form it needs, and passes over the link to that repository.

`settlement` states a repository and links nothing. The finding suggests the link percent-encoded a segment at a time,
so it works as written in Markdown.
