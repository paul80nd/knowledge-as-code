# `bundle` — nothing left to ship

The corpus adopted no type, so the export carries none, so every component the plugin declares is trimmed. What comes
out still installs and does nothing, and this fixture pins that it comes out at all.

## What the fixture is shaped to reach

**A corpus with no type folder.** Not a corpus whose glossary is empty — one that never adopted the type. The export
writes a manifest with an empty type list, which is a valid statement of what a corpus holds, and the bundle has to
survive being handed it.

**A plugin whose only component requires that type.** So trimming empties it. Refusing to build here would leave the
corpus unable to produce the thing that would have told it why, which is why the run warns and writes anyway. The
empty plugin is itself the report: it installs, does nothing, and `bundle.json` beside it names the component that was
dropped and the type it needed.

**A corpus stating no content version.** `.corpus.yaml` is absent, so the export carries `contentVersion: null` and
the plugin keeps the version its own manifest states. The run says so rather than stamping nothing. The
[`bundle`](../bundle/README.md) fixture is the corpus that states one, and pins the stamp.

## What is asserted where

The same three files the `bundle` fixture uses, and they carry the whole of it. `expected-bundle.txt` holds the two
warnings, which are what a person needs and what nothing else will tell them. `expected-files.txt` names the manifest,
the record and the export as present and the trimmed skill as absent. `expected-content.txt` pins the kept version,
the emptied `components` array in the manifest, and what `bundle.json` says about a run that shipped nothing.

**The hook is declared here too, and trimmed with everything else.** A corpus with no glossary ships neither the hook
nor a breadcrumb for it to print. That is the arm worth pinning: a hook that survived a trim would print an empty file
at every session, and an empty breadcrumb reads as a corpus that knows nothing rather than as a component that was
never shipped.
