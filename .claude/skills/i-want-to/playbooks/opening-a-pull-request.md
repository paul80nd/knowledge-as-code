### Opening a pull request

Run at the end of every other playbook. A push to `main` is rejected, so this is how work lands.

1. **Ask which pages your change made wrong.** Nothing in CI reads prose for meaning, so this is yours. A change to a
   command reaches [`docs/`](../../../../docs/) and often `tooling/README.md`. A change to what the tool is for reaches
   the root `README.md` and `PACKAGE.md`. A change to the schema reaches `.schema/README.md`,
   `.schema/meta/type.schema.json` and `docs/schema/`, all three authored once at the root.
2. **Write the changelog entry**, where the tool changed. A line under `## Unreleased` in
   [`CHANGELOG.md`](../../../../tooling/kac/CHANGELOG.md), on this branch. One written after the merge reaches nobody.
3. **Ask whether to release, and recommend an answer.** Moving `<Version>` in
   [`kac.csproj`](../../../../tooling/kac/kac.csproj) is the release: a push to `main` publishes whenever it names a
   version nuget.org does not hold. The call is the branch owner's, so put it to them. Recommend releasing where the
   change stands on its own, and holding where it is one part of a group that is no use apart. Where the tool did not
   change there is nothing to ask. Releasing renames `## Unreleased` to `## <version> - <date>` and moves `<Version>`
   in the same commit.
4. **Check the trees still match.** Run `kac update --check --from ../../` inside each corpus under `examples/`, which
   holds the overlay files equal in both directions. A `seed` file is yours to copy across, and nothing catches it.
5. **Run the layers your change touches**, one `kac` invocation at a time. Where you are unsure, run all four.
6. **Write the commit message to `technical-writing`.** The subject says what changed, imperative and without a full
   stop. The body says why, and it is the one place describing what used to be true is correct.
7. **Separate a behaviour change from a refactor**, one commit each. A feature judged on the built thing may be dropped
   late, and keeping it apart makes removal a clean reset.
8. **Write the pull request body to carry the why and the evidence**, never a retelling of the diff. Name what you
   measured and what it said.
9. **Open the pull request.** Say what you did not do, and why.

**Reply:** the branch, what each commit carries, which layers you ran and their result, and what you left undone.
