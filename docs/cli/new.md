# `new`: stand a corpus up in the folder you are in

> **Draft, pending implementation.** This page is the specification `kac new` is built to, written before the command
> exists. It describes the command as it will be, in the same voice as its siblings, and becomes an ordinary feature
> page the moment the command ships.

## Intent

`new` turns the folder you are standing in into a corpus. It takes the framework from a template repository at a ref,
writes the files that template says a corpus receives, and writes the one file no template can supply: `.corpus.yaml`,
which names the corpus and records where the framework came from.

Its reader is someone who has installed the tool and has nothing else. Every answer the command needs it either asks
for, infers from the folder, or defaults, and a person who answers nothing at all still ends with a corpus that
validates.

## What it is not

**It is not a copy of `example/`.** That is a worked corpus with a fictional estate in it, kept as a reference for what
real records look like. Copying it hands you somebody else's library consortium to delete.

**It is not `update`.** `new` runs where there is no corpus and refuses where there is one. `update` refuses the
reverse. Between them a corpus is created once and kept current after that, and neither command has to guess which it is
doing.

**It does not decide what your repository looks like.** It writes what a corpus is made of. Branch protection,
reviewers, issue templates and the rest are questions about your repository, and it asks none of them.

## Approach

### Everything that can fail, fails first

The order matters more than the steps. All of this runs before the first prompt, so that nobody answers six questions
and is then told the URL was unreachable:

1. **A corpus here already?** A `.corpus.yaml` at or above the working directory means this is one. Stop, and name
   `update`.
2. **Git.** A repository with a dirty tree stops the run: commit or stash first, so that what `new` writes is legible as
   a diff. A folder with no repository at all is offered `git init`. The choice is between running it and cancelling,
   because discovery reads the git listing and an ungitted corpus is a corpus the tool cannot read.
3. **A folder holding files** is a warning and a confirmation, not a refusal. Without a committed baseline there is
   nothing to tell your files from the ones about to arrive.
4. **The template**, cloned shallow at its ref into a temporary folder. A failure here is a URL, a ref or a credential,
   and the message says which.
5. **The tool.** The template's manifest declares `minimum-tool`. An older tool stops rather than half-reading a
   template it cannot understand.

### What it asks

Every answer has a flag, so nothing is reachable only by typing. A flag given is never asked for. `--yes` takes the
default for everything unasked, and a run with no terminal and a missing answer exits rather than waiting: a hung
pipeline is worse than a failed one.

| Asked              | Default                          | Flag           |
|--------------------|----------------------------------|----------------|
| The corpus's name  | the folder's name                | `--name`       |
| Which types        | every type the template declares | `--types`      |
| Where it publishes | `none`                           | `--publishing` |
| Which CI system    | `none`                           | `--ci`         |

Publishing and CI are asked separately because they are separate facts: a corpus can be built by one system and read on
another. Where a publishing target needs base URLs, they are pre-filled from `git remote get-url origin` rather than
asked for: a URL nobody can recall is a URL nobody should be made to type.

Types are asked as a multi-select with everything ticked, because declining is the exception. A declined type's schema
file is not written, rather than written and then ignored, and `types:` in `.corpus.yaml` records the decision so that
validation can hold the corpus to it.

### What it writes

The manifest decides, and `new` writes both of its layers: a corpus is created by taking everything at once.
`layer: withheld` is the template's own machinery and reaches no corpus. Where a rule declares `to:`, the file lands
there rather than where it sat upstream, which is how a template serving its schema from a repository root places it at
a corpus's own root.

`.corpus.yaml` is written rather than copied. No template can carry a descriptor without carrying somebody else's name
in it, so the file is composed from the answers above and stamped with the `upstream:` block: the URL, the path within
it, the ref followed, the commit resolved, the template's version and the date. That block is what `update` reads later.

`README.md` is written too, and for the same reason. The template's own is `withheld`, because it describes the
template rather than a corpus. A corpus that copied everything would therefore arrive with no README at all. What
`new` writes is short: the corpus's name, what it holds, and how to run the tool against it. It is a starting point,
not a document, and the corpus owns it from the moment it lands.

One file needs more than its bytes: `.plugin/hooks/breadcrumb` is executable, and a hook that arrives without its mode
bit fails silently on Unix.

### What it does last

`generate`, then `validate`, then `git add -A`.

Generation first, because it writes the `_index.md` files and the generated blocks that validation then checks.
Validation second, because a corpus that `new` created and cannot validate is a defect in the template or the tool, and
the person who just ran the command is not the one who should discover it. Staging last, so that everything the command
did is visible in one place before it is committed.

It stops short of committing. A first commit is a person's own act, and staging shows them everything first.

## Known limits

**It needs a network and a git client.** The template is fetched rather than carried, so a machine that cannot reach the
template repository cannot create a corpus. `--from` accepts a local path as well as a URL, which is the offline escape
hatch and is also what the tool's own tests use.

**It is not idempotent and does not try to be.** Running it twice in the same folder stops on the first check. Taking a
newer template into a corpus that already exists is `update`, which is a different question with a different answer.

**The default upstream is compiled in.** A tool that cannot bootstrap without a URL you have to look up is a tool people
get wrong, so `--from` defaults to the framework's own repository. A corpus taking its framework from elsewhere passes
the flag once, at creation, and `.corpus.yaml` remembers.
