A corpus that adopted one of the two types it holds files for.

`.corpus.yaml` names `adrs` and nothing else. `policies` is stood up all the same — a page, a folder and a template
left behind by the copy the corpus began as — and `.schema/` covers both, as it covers every type the framework
declares.

Generation follows the lock. `adrs/_index.md` and the two blocks on `adrs.md` are rebuilt; `policies/_index.md` is never
written, and the hand-written text between the markers on `policies.md` is left where it is. A generator reading the
schema instead would create an index for a type no generated list of this corpus's types names, and `index --check`
would hold the corpus to keeping it fresh ever after.

That `policies` is stood up and not adopted is a defect, and `validate` is the voice that says so — `type-adoption`
pins that half. Here it is only the state generation has to survive.
