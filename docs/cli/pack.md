# `pack` seal the export into a versioned package

<!-- BEGIN GENERATED: usage-pack -->

```text
kac pack [--no-color] [--repository <URL>]
```

| Option               | What it does                                                                   |
|----------------------|--------------------------------------------------------------------------------|
| `--no-color`         | Turn colour off. NO_COLOR in the environment does the same.                    |
| `--repository <URL>` | Where the corpus's source lives. Some registries refuse a package naming none. |

<!-- END GENERATED: usage-pack -->

## What it does

A directory is not something another repository can depend on. `pack` reads what [`export`](export.md) wrote and seals
it into one file under `.dist/package/`, named for the corpus and the version its content is on. A registry stores that
file, keeps every version it has ever accepted, and hands one back to whoever asks for it.

The file is a `.nupkg`, which is a zip with a small XML manifest inside naming the package and its version. GitHub
Packages and Azure DevOps Artifacts both store one, and neither this command nor anything that reads the result needs a
NuGet client. What a consumer acts on is `corpus/manifest.json` inside the archive, which is the export's own manifest,
unchanged.

The version is `content-version` from [`.corpus.yaml`](../corpus-descriptor.md), the number a corpus moves by hand when
its records change meaning. The package also has to name a `shortcode`, because that is the word a consuming corpus will
cite it by.

What a registry lists comes from the corpus too. `description` and `author` in
[`.corpus.yaml`](../corpus-descriptor.md) open the package's description and name who publishes it, and `license` is
stated where the corpus chose one. A corpus that has named nobody is filed under its own id rather than under whoever
wrote the template it copied.

Run [`export`](export.md) first. `pack` reads that output and never the corpus, so what gets published is the tree that
was proved, and the four keys reach it through the export's own `about` block.

## Examples

### Seal an export

```bash
kac export
kac pack
```

The run names the file it wrote, then says what a consumer will receive:

```text
wrote .dist/package/example-engineering.0.1.0.nupkg
pack: sealed 29 file(s) as example-engineering 0.1.0, cited as 'eng:'.
pack: 0.1.0 is content-version from .corpus.yaml. A registry never replaces a published version.
```

### Look inside what you built

The package is a zip, so any unzip tool opens it. The listing below is cut short after the first few entries:

```bash
unzip -l .dist/package/example-engineering.0.1.0.nupkg
```

```text
  Length      Date    Time    Name
---------  ---------- -----   ----
      419  01-01-1980 00:00   [Content_Types].xml
      283  01-01-1980 00:00   _rels/.rels
      366  01-01-1980 00:00   example-engineering.nuspec
     1332  01-01-1980 00:00   corpus/glossary/gls-knowledge-as-code.json
     8804  01-01-1980 00:00   corpus/glossary/terms.jsonl
     1183  01-01-1980 00:00   corpus/manifest.json
     1283  01-01-1980 00:00   corpus/policies/pol-A11Y.json
     ...
---------                     -------
   111955                     29 files
```

The three files at the root are the envelope a registry reads. Everything under `corpus/` is the export, byte for byte.
Every entry carries the same timestamp, so two runs over one export produce one file.

### Publish it

A registry takes the file over its own API. GitHub Packages reads it from `dotnet nuget push`:

```bash
dotnet nuget push .dist/package/example-engineering.0.1.0.nupkg \
  --source https://nuget.pkg.github.com/OWNER/index.json \
  --api-key "$GITHUB_TOKEN"
```

Some registries decide which repository a package belongs to by reading a URL inside it. GitHub Packages is one, and a
token scoped to a repository refuses a package naming none. Pass `--repository` when you pack for one:

```bash
kac pack --repository https://github.com/OWNER/REPO
```

A version the registry already holds is refused there rather than overwritten. Bump `content-version` and pack again.
[`publish-corpus.yml`](https://github.com/paul80nd/knowledge-as-code/blob/main/.github/workflows/publish-corpus.yml)
is the pipeline this repository publishes from, and it asks the registry first so the refusal names the version.

### Find out why a corpus cannot be packed

A corpus missing any of the three facts a package is named and cited by is told which one, and exits `1`:

```text
pack: the export declares no shortcode, and a consumer cites what it imports by one. Write `shortcode:` in .corpus.yaml and export again.
```

The other two are `corpus:`, which becomes the package id, and `content-version:`, which becomes the version.

## Known limits

**It does not publish.** The command writes a file and stops. Pushing it is the registry's own tool or your pipeline's
step, because authenticating to a registry is a thing your organisation has already decided how to do.

**It cannot tell you whether the version is new.** Only the registry knows what it already holds, and `pack` reaches no
network. Ask the registry in the step before the push.

**A package id is the corpus name, and a registry constrains it.** Letters, digits and underscores, joined by a dot, a
dash or an underscore. A corpus called something else is refused by name rather than renamed for you.

[The export format](../design/export.md) is the contract the payload answers to, and it is what a consumer reads once
the package is unpacked.
