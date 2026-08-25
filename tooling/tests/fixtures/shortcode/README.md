A corpus whose shortcode is spelled two ways it may not be.

`.corpus.yaml` declares `shortcode: STD`, which is wrong twice over, and the pass reports both at once.

* **It is miscased.** A shortcode opens on a lower-case letter and carries lower-case letters and digits after it,
  because it is read far more often than it is written.
* **The spelling underneath is the standards prefix.** A citation opening `std:` reads as that type rather than as
  this corpus, so the comparison against the schema's prefixes ignores case. An author told only about the casing
  would correct it and meet the second refusal on the next run.

Both findings land against `.corpus.yaml`. What is wrong is the declaration, and the file holding it is the one a
corpus owner edits.

The other spellings a shortcode may not take are held by `ShortcodeTests`, which builds a corpus from values. The
coverage gate reads check ids, so one fixture is what this id needs and the rest would only run the same code again.

A corpus declaring no shortcode is silent, which is every other fixture here.
