The generated index of a corpus that files its records in sub-folders. `generate` covers the flat case, so this
fixture pins only what depth adds.

The corpus has a policy one folder down and two standards, one at each of two depths. The run asserts
`generate --check` finds both committed `_index.md` files fresh.

* Each first-level folder becomes a heading in the index: `Security` in `policies/_index.md`, and `Common` and
  `Platform` in `standards/_index.md`.
* A `Category` column appears only in a group whose records sit deeper than that heading. It reads `platform/node` for
  the standard two folders down, and is absent from the other groups.
* Each title links to its record by the path below the type folder, so a link two folders deep has to resolve.

The two standards share the filename `testing.md`, as they do in `clean`.
