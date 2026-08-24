### Opening a pull request

Run at the end of every other playbook. A push to `main` is rejected, so this is how work lands.

1. **Ask which pages your change made wrong.** Nothing in CI reads prose for meaning, so this is yours. A change to a
   command reaches [`docs/`](../../../../docs/) and often `tooling/README.md`. A change to what the tool is for reaches
   the root `README.md` and `PACKAGE.md`. A change to the schema reaches the `.schema/README.md`
   in both trees.
2. **Move `<Version>` and write the changelog section together**, where the tool changed. A push to `main` publishes
   whenever [`kac.csproj`](../../../../tooling/kac/kac.csproj) names a version nuget.org does not hold, and the release
   carries the matching section from [`CHANGELOG.md`](../../../../tooling/kac/CHANGELOG.md). A section written after the
   merge reaches nobody, and `ChangelogTests` fails a version with none.
3. **Check the trees still match.** `TemplateTests` holds the overlay files byte-equal, and a `seed` file is yours to
   copy across.
4. **Run the layers your change touches**, one `kac` invocation at a time. Where you are unsure, run all four.
5. **Write the commit message to `technical-writing`.** The subject says what changed, imperative and without a full
   stop. The body says why, and it is the one place describing what used to be true is correct.
6. **Separate a behaviour change from a refactor**, one commit each. A feature judged on the built thing may be dropped
   late, and keeping it apart makes removal a clean reset.
7. **Write the pull request body to carry the why and the evidence**, never a retelling of the diff. Name what you
   measured and what it said.
8. **Open the pull request.** Say what you did not do, and why.

**Reply:** the branch, what each commit carries, which layers you ran and their result, and what you left undone.
