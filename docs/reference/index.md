# Schema reference

This section is for the schema owner: whoever writes or changes the type files a corpus is checked against. It covers
the keys a type file takes, the rule expression grammar, the checks, and the export contract.

A corpus is one repository of knowledge records kept in git. A record is one Markdown document in it, with YAML
frontmatter above its prose.

## The schema

`kac` checks every record in a corpus against a schema, and the corpus has its own. The schema is a folder of YAML files
named `.schema/`, sitting at or above the corpus. One file declares one **type**, meaning one kind of record: what its
records are called, where they live, what fields they have, and what CI checks them for. Shared files sit beside those.

Nothing here is hard-coded in the tool. A corpus that adds a type file gets a validated type, and a corpus that adds a
rule to one gets a check, with no release of `kac` in between.

### The keys a type file may take

[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json)
describes them. Each type file opens with a modeline pointing at it, so an editor with YAML language-server support
offers the keys, describes each one on hover, and marks a wrong one as you type:

```yaml
# yaml-language-server: $schema=./meta/type.schema.json
```

No build reads that file. It advises an author and gates nothing, and a type file written outside an editor meets the
same checks as one written in it.

### What each file in `.schema/` holds

[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/README.md) says which file holds
what, and what the generator writes from each. It travels with the schema, so a corpus reads its own copy.

## The pages here

* **[Rule expressions](expressions.md)** is the grammar for the one-line conditions a type declares, and the facts one
  may ask about a record.
* **[Checks](checks.md)** says where a check comes from, how a type's own rules run, and which check reports a fault.
* **[What the schema is held to](held-to.md)** says what `kac` refuses when it loads the schema.
* **[Generation](generation.md)** says what a corpus derives from its own records, and where each derived thing lands.
* **[The export format](export.md)** is the contract an export meets: what each file holds, what a type decides
  about its own records, and which version number moves when either changes.

## Where to go next

[Shaping a type](../framework/shaping-a-type.md) says what a record of each type is for, and why its fields and sections
serve that.
