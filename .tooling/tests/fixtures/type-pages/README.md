Type pages, and the two passes they get.

`adrs.md` is a type page, checked for links, for its generated blocks, and for carrying no frontmatter. It has an
unresolvable link; its `schema-adrs` block has lost its BEGIN marker, which leaves `kac index` writing nothing there
while `index --check` still calls the page fresh; and it carries a record's frontmatter, which is what a page left
behind by a type that used to be a single document looks like. Nothing else would report that: a page is forked, so it
is never compared against upstream.

`glossary/knowledge-as-code.md` is the framework's own vocabulary: a record, and a document every corpus shares. It is
the one file that gets both passes, so it is where the two are held apart. Its link to a type page is reported once, for
naming a type. Its link to a page that does not exist is reported **once and not twice** — a record has already had the
link pass, and the framework pass leaves it alone. `glossary.md` and `glossary/_template.md` are sound and are here only
so `type-setup` stays quiet about the folder.

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
