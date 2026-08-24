### Sweeping prose

**One agent per folder, in the main checkout**, so it can read what it needs. Three agents against one folder was a
trial design, and testing a rule is a different job from applying one.

1. **Count first, from the files.** A folder nobody listed is a folder nobody swept. Every "what is left" table on this
   branch missed the type templates and `glossary/` because they were counted as folders rather than as files.
2. **Put a version check at the top of the prompt.** Name three or four lines only the current skills carry, and tell
   the agent to stop if any is missing. Load skills with the Read tool: the Skill tool has served a stale render.
3. **Name the trap for that batch.** A schema sweep meets plain YAML scalars where a colon is a parse error. A type page
   meets generated regions. A policy meets the clause override. A named trap has never fired.
4. **Say which files belong to somebody else**, including a human reading a folder right now.
5. **Forbid `kac` and `dotnet` while agents run in parallel.** They build the same project and contend over its output.
   Run every check yourself at the end.
6. **Verify from the files, not from the report.** Count the marks, then diff headings, frontmatter and generated
   regions against `HEAD`. Every report this branch produced held up, and checking cost seconds.
7. **Read the whole file, not the diff.** A sweep that leaves a document reading in two voices has failed even where
   every rule was obeyed.
8. **Copy the overlay files across.** `knowledge-as-code/**` and `glossary/knowledge-as-code.md` live in both trees,
   and `kac update --check --from ../` inside `example/` holds them equal. `.schema/**` is authored once at the root,
   so it needs no copy. A root page and a `_template.md` are `seed`, so nothing catches drift and the copy is yours.
9. **Ask the agents where a rule failed them.** A rule two readers understand differently is a defect, however good
   either result looks.
10. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** the count before and after per file, what you deliberately left and the rule exempting it, and every place a
rule did not decide it.
