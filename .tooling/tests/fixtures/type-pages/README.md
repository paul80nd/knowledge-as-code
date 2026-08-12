Type pages, and the two passes they get.

`adrs.md` is a collection's page: no frontmatter, checked for links and for its generated blocks. It has an
unresolvable link, and its `schema-adrs` block has lost its BEGIN marker — which leaves `kac index` writing nothing
there while `index --check` still calls the page fresh.

`trinkets.md` is a single-document type's page, so it is a record. Its id disagrees with the literal value
`trinkets.yaml` declares. That type is the fixture's own, laid over the real schema: no type the framework declares is
single-document, so the shape and its `literal` id style need a type of their own to be tested against.

`adrs/_template.md` is valid, so `type-setup` stays silent and the fixture asserts one thing. The scenario that
breaks a template deliberately is `broken-template`.

`knowledge-as-code/taxonomy.md` is a framework document, and gets two passes nothing used to give it.

`framework-names-types` reports its link to a type page and its link to a record inside a type's folder — three in all,
since it links the ADR page in both the extensionless and the `.md` form and Azure DevOps resolves either.

The ordinary link pass reports a target that resolves to no file and a fragment naming no heading. Neither had ever
been asked of these documents: they are excluded from discovery, and the page pass visits only type pages.

Silent, and deliberately so: the link inside the generated block, which is written from the types the corpus stood up,
and every type named in prose rather than linked.

Two of its links draw two findings each. That is not double-reporting: a link that should not be there and does not
resolve is wrong twice over. The real corpus sees only one of the two, because the record it names exists.
