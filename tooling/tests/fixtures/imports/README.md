A corpus that declares an import and has not restored it.

`.corpus.yaml` names `example-engineering` in `consumes:` and no `.imports/eng/` stands beside it. That is the state a
fresh clone is in, because a restored corpus is never committed, so it is the state every pipeline meets before its
first command.

The finding lands against `.corpus.yaml`, which is the file naming what did not arrive, and it names `kac restore`.

**One finding, however many references there are.** A citation into a shortcode declared and not restored reports
nothing of its own. Whoever has not restored wants the line telling them to, and not one for each reference they wrote
correctly.

The spelling rules a citation across a boundary is held to are held by `ResolverTests`, which builds the graph from
values. They report under `part-ref` and `ref-resolves`, which fixtures here already cover, so a second corpus on disk
would run the same code again and prove nothing new.
