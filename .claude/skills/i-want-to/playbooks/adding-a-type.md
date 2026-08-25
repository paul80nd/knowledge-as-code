### Adding a knowledge type

**Adding a type is adding a YAML file to `.schema/`, and three files beside it.** A type with a schema and no folder
counts as absent, and one with a folder and no page fails `type-setup`.

1. **Argue it belongs.** [`knowledge-as-code/taxonomy.md`](../../../../examples/library/knowledge-as-code/taxonomy.md)
   carries the types that exist and what each is not. A type earning its place holds records no existing type would
   take.
2. **Pick the tier before the fields.** The tier fixes how its records are written and what the review bar is, and
   several types share one.
3. **Write `.schema/<type>.yaml`.** Run [changing-the-schema](changing-the-schema.md) for the rules that govern it.
4. **Write the root page `<type>.md` and the `<type>/_template.md`.** Both are `seed`, so nothing holds the two trees
   equal and the copy across is yours.
5. **Record the lineage.** `<type>.yaml`'s `lineage:` and `collision:` blocks are read into
   `knowledge-as-code/lineage.md`, which says where the name came from and where it already means something else.
6. **Add the type to `types:` in `.corpus.yaml`.** Generation and validation cover the types a corpus adopted and no
   others.
7. **Write at least one record.** Twelve of the type folders here hold none, so their rules have never run. A throwaway
   record is how you find out whether the schema says what you meant.
8. **Run `kac generate` and `kac validate` in both corpora**, then the golden suite.
9. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what the type holds, what it is not, which existing type it was nearly, and what the first record found.
