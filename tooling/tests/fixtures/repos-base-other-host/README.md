A corpus whose `repos-base` is a host `kac` knows no form for, so an entry resolves to `<base>/<entry>` as written.

`payments` states `Acme/Payments Api` and links it inside the repository. `ledger` states a bare `ledger` and links
it. Neither form is reported, because only an Azure DevOps base declares one, and the fixture reports nothing.

`settlement` states `Acme/Settlement` and links a second repository it does not state. The finding names the link in
the owner form the record's entries take, percent-encoded.
