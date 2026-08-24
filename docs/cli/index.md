# CLI reference

One page per command. Each opens with the usage the parser accepts, generated from `kac` itself, and then says what the
command is for, what it is not, and how it works. Two more sections appear where a command has them: the decisions
behind it, and its known limits.

## The commands

<!-- BEGIN GENERATED: command-table -->

| Command                   | What it does                                            |
|---------------------------|---------------------------------------------------------|
| [`new`](new.md)           | Stand a corpus up in the folder you are in.             |
| [`validate`](validate.md) | Check the corpus against its schema.                    |
| [`generate`](generate.md) | Rewrite the parts of a corpus derived from its records. |
| [`export`](export.md)     | Write the corpus out as data a consumer can read.       |
| [`bundle`](bundle.md)     | Assemble the export into an installable agent plugin.   |
| [`checks`](checks.md)     | List every check the validator can report.              |
| [`update`](update.md)     | Take a newer framework into a corpus.                   |

<!-- END GENERATED: command-table -->

## Where a command runs

Every command but `new` answers a question about a corpus, meaning one repository of knowledge documents kept in git.
Each finds that corpus by walking up from the working directory for a `.corpus.yaml`, the descriptor naming the corpus.
It then walks up again from there for the `.schema/` that says what a record of each type carries, so one schema can
serve several corpora in one repository. Where the tool's own files sit says nothing about which corpus it reads.

`new` stands a corpus up where there is none, so it is the one command that expects neither file above it. `--help`
and `--version` are answered by the parser and need no corpus at all.

## Options every command takes

`--no-color` turns colour off, and every command reads `NO_COLOR` from the environment for the same request. `NO_COLOR`
is the cross-tool standard, and the flag is there for a caller who cannot set a variable.

A redirected stream carries no colour on its own. An environment naming a runner that renders escapes in its logs turns
it back on, and GitHub Actions is one such runner. Set `NO_COLOR` wherever the bytes have to be the same everywhere.

`--help` and `--version` belong to the parser rather than to any one command. `kac --help` lists the commands, and
`kac <command> --help` prints the same usage that command's page carries. `--version` names the release and the commit
it was built from.

## Exit codes

| Code | Meaning                                                                                 |
|------|-----------------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                        |
| `1`  | A corpus **error**, or a bad invocation (missing or unknown command or option).         |
| `2`  | A command found no corpus. `new`, `--version` and `--help` need none and answer anyway. |

Warnings never change the exit code.
