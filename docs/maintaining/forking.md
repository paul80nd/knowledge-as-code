# Forking `kac`

A fork builds and passes its tests with nothing changed, except `round-trip.sh` below. To publish, replace the
identifiers on this page with your own and set up the accounts they name. `kac` is the tool, the site is this
documentation, and a corpus (one repository of knowledge records kept in git) is what `kac` checks. A plugin is a
corpus packaged for Claude Code, and an export is the set of files a corpus publishes for another corpus to consume.

```sh
git grep -l "paul80nd"               # every file naming the upstream owner
git grep -l "KnowledgeAsCode.Tool"   # every file naming the upstream package
```

Leave `tooling/kac/CHANGELOG.md` as it is. A released entry describes its own release.

## What a fork publishes

These workflows publish on a push to `main`. Each needs something from you.

| Workflow             | Publishes                                        | Needs                                                                              |
|----------------------|--------------------------------------------------|------------------------------------------------------------------------------------|
| `publish-tool.yml`   | `kac` to nuget.org                               | A package id of your own, a trusted publishing policy, a secret and an environment |
| `publish-docs.yml`   | The site to GitHub Pages                         | The site URL, and Pages set to deploy from GitHub Actions                          |
| `publish-corpus.yml` | Each corpus under `examples/` to GitHub Packages | Nothing beyond the Actions tab below                                               |
| `publish-plugin.yml` | A plugin marketplace to the `marketplace` branch | The marketplace `owner`, which the workflow writes as a literal                    |

GitHub turns Actions off in a new fork. Turn them on from the fork's **Actions** tab before your first push.

## The tool on nuget.org

nuget.org gives a package id to whoever publishes it first, so a fork needs an id of its own. Replace
`KnowledgeAsCode.Tool` with yours in these places:

| File                                 | What to change                                                                                                    |
|--------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| `tooling/kac/kac.csproj`             | `<PackageId>`, `<Authors>`, `<PackageProjectUrl>`, `<RepositoryUrl>` and `<PackageReleaseNotes>`                  |
| `tooling/kac/PACKAGE.md`             | The repository link at the foot of the page nuget.org shows                                                       |
| `.github/workflows/publish-tool.yml` | The `dotnet tool install` line, the flat-container URL in lower case, the `.nupkg` name and the environment `url` |
| `.github/workflows/kac.yml`          | The `dotnet tool install` line in the `tool` job                                                                  |
| `template/.github/workflows/kac.yml` | The `dotnet tool install` line a new corpus runs                                                                  |
| `template/azure-pipelines.yml`       | The same line, for a corpus on Azure Pipelines                                                                    |

Then set up the publish itself:

1. On nuget.org, add a trusted publishing policy. Name your repository's owner, the repository, the workflow file
   `publish-tool.yml` and the environment `nuget.org`.
2. In the repository's settings, create an environment named `nuget.org`. Add yourself as a required reviewer, so each
   publish waits for your approval.
3. Add a secret named `NUGET_USER`, holding your nuget.org profile name. The profile name is not your email address.

The trusted publishing policy replaces an API key. The job exchanges its GitHub identity for a key that lasts an hour.

## Where `kac new` takes the framework from

`Asking.DefaultFrom`, in `tooling/kac.core/Asking.cs`, is the repository `kac new` copies a new corpus from when
`--from` is not given. `kac new` writes it into the new corpus's `.corpus.yaml` twice:

* `upstream.url` is where `kac update` later takes a newer framework from.
* `framework.base` is where the corpus's readers report a problem with `kac`, the schema, the template or a skill.

Point `Asking.DefaultFrom` at your fork, or every corpus your tool creates takes the upstream framework.

The `new` golden fixture expects the upstream URL in `framework.base`. Regenerate it after the change, and read the
diff:

```sh
KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens --filter-trait "Scenario=new"
```

## The documentation site

1. In `mkdocs.yml`, change `site_url`, `repo_url`, `repo_name` and `site_author`.
2. In the repository's settings, under **Pages**, set the source to **GitHub Actions**.

`kac new` writes the site's address into a comment at the top of each `.corpus.yaml` it creates, from
`tooling/kac.core/New.cs`. Pages under `template/` and comments under `.schema/` cite the site by URL too.
`git grep -l "paul80nd.github.io"` lists every one.

Each specification page under `docs/maintaining/` links its feature files on GitHub. Change those links and
`FeatureUrl` in `tooling/kac.tests/ScenarioCitationTests.cs` together. The test reads only the links that start with
`FeatureUrl`.

## The corpora and the plugin

Each corpus under `examples/` points at this repository in its `.corpus.yaml`:

* `publishing.base` is where a link in the corpus's export points. `tooling/tests/round-trip.sh` fetches a record from
  it at your commit, so the script fails until `publishing.base` points at your fork.
* `framework.base` is where a problem with the framework is reported, as
  [`kac new` writes it](#where-kac-new-takes-the-framework-from).
* `author.url` is credited in the package and the plugin.
* `repos-base`, in `examples/dog-fooding` alone, is where a `services` record's repositories resolve.

`publish-corpus.yml` and `publish-plugin.yml` publish under whichever repository runs them, and need no secret.
`publish-plugin.yml` writes `owner: {name: "Paul Law"}` into the marketplace it publishes. Change the name to yours.

`.claude/settings.json` installs the plugin from the `marketplace` branch of `paul80nd/knowledge-as-code`. Change
`repo` to your fork to read your own corpus.

## The branch rule on `main`

Add a branch rule on `main` that blocks a direct push and requires the `validate` check.
[The checks that gate a merge](contributing.md#the-checks-that-gate-a-merge) lists the jobs `validate` waits for. No
file in the repository has a copy of this rule.

## Where to go next

[Build, test and release](build-test-release.md#release-a-version) moves the version that makes the first publish.
