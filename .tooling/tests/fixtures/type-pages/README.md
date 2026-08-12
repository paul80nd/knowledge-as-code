Type pages, and the two passes they get.

`adrs.md` is a type page, checked for links, for its generated blocks, and for carrying no frontmatter. It has an
unresolvable link; its `schema-adrs` block has lost its BEGIN marker, which leaves `kac index` writing nothing there
while `index --check` still calls the page fresh; and it carries a record's frontmatter, which is what a page left
behind by a type that used to be a single document looks like. Nothing else would report that: a page is forked, so it
is never compared against upstream.

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
