ADRs whose `accepts:` entries take each path through the acceptance pass. The run exits 1, because three of them are
refused.

* `0001-accepted.md` has a link definition nothing references and accepts `unused-definition`, so the warning is
  reported as info with its reason.
* `0002-stale.md` accepts `unused-definition` and has no such warning, for `acceptance-stale`.
* `0003-refused.md` accepts `acceptance-stale`, an error check, an info check and an id no check reports, for each
  branch of `acceptance-check`.
* `0004-empty-reason.md` gives one entry no reason, for `entry-key`, and another a blank one, for `field-pattern`.
  Neither entry accepts anything, so the warning stands.

`Accepts.feature` reads this corpus for what `validate` promises about an acceptance.
