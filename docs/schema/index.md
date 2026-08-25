# Schema reference

`kac` holds every record in a corpus to a schema, and this section is for whoever writes one. A **corpus** is one
repository of knowledge records kept in git. A **record** is one Markdown document in it, carrying YAML frontmatter
above its prose.

The schema is a folder of YAML files named `.schema/`, sitting at or above the corpus. One file declares one **type**,
meaning one kind of record: what its records are called, where they live, what fields they carry, and what CI holds them
to. Four shared files sit beside those.

Nothing here is hard-coded in the tool. A corpus that adds a type file gets a validated type, and a corpus that adds a
rule to one gets a check, with no release of `kac` in between.

## The pages here

* **[Rule expressions](expressions.md)** is the reference for the one-line conditions a type declares, and the facts one
  may ask about a record.
* **[What the schema is held to](held-to.md)** says what `kac` refuses when it loads the schema, and why a declaration
  the tool ignores counts as a defect.

## Where the rest of the schema is documented

**The keys a type file may carry** are described in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). Each
type file opens with a modeline pointing at it, so an editor with YAML language-server support offers the keys,
describes each one on hover, and marks a wrong one as you type:

```yaml
# yaml-language-server: $schema=./meta/type.schema.json
```

No build reads that file. It advises an author and gates nothing, and a type file written outside an editor meets the
same checks as one written in it.

**The folder itself**, meaning which file holds what and what the generator writes from it, is
[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/README.md). It travels with the
schema, so a corpus reads its own copy.

**What a record carries in frontmatter**, and how an id is formed, is
[Metadata](../framework/metadata.md).

## Where to go next

[Checks](../checks.md) is the page for deciding whether the check you want already exists.
