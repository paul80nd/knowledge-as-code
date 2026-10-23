The `generate` corpus with one fault: `adrs/_index.md` is missing the row for the second ADR.

The run asserts `generate --check` exits 1 and that its output names `adrs/_index.md`, the one path in
`expected-stale.txt`. It does not assert that the fresh `adrs.md` goes unnamed.

`KAC_UPDATE_GOLDENS=1` leaves this fixture alone. Regenerating it would write the missing row back and remove the fault.
