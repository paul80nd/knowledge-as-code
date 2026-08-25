### Adding a record

**The tier decides how it is written, and the type decides what it holds.** Nothing in CI tells you that you used the
wrong tier's rules.

1. **Pick the type.** [`knowledge-as-code/taxonomy.md`](../../../../examples/library/knowledge-as-code/taxonomy.md)
   carries the decision table. Where two types both fit, the record belongs in the more general one.
2. **Read the type's root page and its `_template.md`.** The page says what the type holds and what it is not. The
   template says which sections the schema requires.
3. **Run `kac checks` for that type**, and read its `rules:` block in `.schema/<type>.yaml`. A rule declaring no
   `severity:` binds you and fails nothing, so the build will not remind you.
4. **Load `technical-writing`, then `writing-a-record`.** Read the section for the tier the type carries, not the one
   you are used to.
5. **List what the record needs and what you were told.** A request is usually one sentence and a template is a dozen
   required fields. Go back and ask for the difference rather than inventing it. A field you cannot answer is a question
   for a person, not a placeholder.
6. **Copy the template and fill it.** Keep every required section. A section with nothing to say is a content gap to
   report, never a heading to delete.
7. **Write the frontmatter last**, once the record has settled. Field order is topological and `key-order` checks it.
8. **Ask which records now point at this one.** An edge such as `depends-on` is written one way and nothing generates
   the reverse view, so no check will prompt you. A new service its neighbours call is a change to their records too.
9. **Run `kac validate`, then `kac generate`.** The record's H1 lands in a generated index, so a new record leaves the
   corpus stale until you regenerate.
10. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** the id and where it sits, which tier's rules you applied, and anything the type declared that you could not
answer.
