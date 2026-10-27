The history checks, which run only under `validate --since`. The runner commits `base/` as `HEAD`, puts `corpus/` in
its place in the working tree, and runs `kac validate --json --since HEAD`.

* **`immutable-after-accepted`.** `adr-0001` was accepted at the base, and its `Decision` now says something else.
* **A correction passes.** `adr-0002` fixes a typo, reflows a line and moves a link target, and reports nothing.
* **`retire-not-delete`.** `adr-0003` was accepted at the base and is gone. `adr-0004` was only proposed, so its
  deletion reports nothing.
* **A moved file passes.** `adr-0005` moved from `0005-old-name.md` to `0005-new-name.md` with its text unchanged, and
  is paired by its id.
* **`immutable-after-published`.** `pmt-0001` was published at the base, and its `Root cause` now says something else.
* **`clause-id-kept`.** `pol-KEPT` was active at the base, and its clause `LOCAL` is now `NOLINK`. It also rewords a
  clause, adds one and takes a new title, and none of that reports.
* **`rule-id-kept`.** `std-KEPT` was active at the base, and its rule `FAST` is gone. Its other rule is reworded, which
  reports nothing.
* **A record out of force loses a part freely.** `pol-DRFT` loses a clause in the change that makes it active, and
  `std-WDRN` loses a rule while deprecated. Neither reports anything.

`svc-catalogue` is there only so the postmortem's `affected` resolves.
