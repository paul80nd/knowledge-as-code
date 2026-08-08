Type pages, and the two passes they get.

`adrs.md` is a collection's page: no frontmatter, checked for links and for its generated blocks. It has an
unresolvable link, and its `schema-adrs` block has lost its BEGIN marker — which leaves `kac index` writing nothing
there while `index --check` still calls the page fresh.

`glossary.md` is a single-document type's page, so it is a record. Its id disagrees with the literal value
`glossary.yaml` declares.

`adrs/_template.md` exists only so `type-setup` stays silent and the fixture asserts one thing.
