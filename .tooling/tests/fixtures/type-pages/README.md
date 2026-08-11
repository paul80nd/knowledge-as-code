Type pages, and the two passes they get.

`adrs.md` is a collection's page: no frontmatter, checked for links and for its generated blocks. It has an
unresolvable link, and its `schema-adrs` block has lost its BEGIN marker — which leaves `kac index` writing nothing
there while `index --check` still calls the page fresh.

`glossary.md` is a single-document type's page, so it is a record. Its id disagrees with the literal value
`glossary.yaml` declares.

`adrs/_template.md` is valid, so `type-setup` stays silent and the fixture asserts one thing. The scenario that
breaks a template deliberately is `broken-template`.

`knowledge-as-code/taxonomy.md` is a framework document, which may name a type and may not link to one. It carries a link to a type page and a link to a record inside a type's folder, plus two that must stay silent: one inside a generated block, which is written from the corpus's own types, and one at a root page that is not a type at all.
