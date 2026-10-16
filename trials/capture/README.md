# The capture trial

This trial measures whether a team's knowledge is getting written down in a corpus (one repository of knowledge
records). It takes two readings of one corpus. The first is on the day the corpus took its first record. The second is
three months later.

The corpus owner runs it, in the repository where the corpus lives. The owner needs only `capture.sh` and
`question-log.csv` from this folder, and nobody to ask. Whoever decides what the framework changes reads the result.

Each reading counts these, from git and from the tracker the corpus states:

* the records written, per type
* the authors, and how many of them are not the corpus owner
* the days from a new author's first commit to their first merged record
* the findings filed, and how many became records
* the types the corpus adopted and has no record of
* the records moved from one type's folder to another's

For two weeks at each reading, the corpus owner also logs each question put to them about how something is done.

## What leaves the organisation

Each reading writes two outputs:

* **`raw/`** lists every record, every author's email address, and every finding's address. It stays with the
  organisation that ran the reading. Never publish it.
* **`aggregate.md`** contains only counts, rates and type names, under the fields [defined below](#the-aggregate).
  It contains no person's name, no path and no address, so you can publish it without a review.

Publish `aggregate.md` alone. Where somebody asked you for the reading, post it where they asked.

## What you need

| Tool                                    | When                           | Why                                                                                                                              |
|-----------------------------------------|--------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `sh`                                    | always                         | The script is POSIX shell. macOS and Linux both have it.                                                                         |
| `git`                                   | always                         | Every count except the findings comes from git history.                                                                          |
| `jq`                                    | always                         | The script converts dates and reads GitHub's replies.                                                                            |
| `gh`                                    | the tracker is on GitHub       | The script queries the findings with it. Sign in with `gh auth login` before you start.                                          |
| `az`, with the `azure-devops` extension | the tracker is on Azure DevOps | You write the findings file with it. Add the extension with `az extension add --name azure-devops`, and sign in with `az login`. |

Copy [`capture.sh`](capture.sh) and [`question-log.csv`](question-log.csv) out of this folder. The script needs nothing
else from this repository. Run it as `./capture.sh`, or as `sh capture.sh` where the copy lost its execute permission.

## When to take each reading

Each reading has an as-of date, which `-d` takes, and a two-week question log.

| Reading | As-of date                                | Its question log                                   | Run the script                 |
|---------|-------------------------------------------|----------------------------------------------------|--------------------------------|
| 1       | the day the corpus took its first record  | the 14 days starting on the day you take reading 1 | on or after the log's last day |
| 2       | three months after reading 1's as-of date | the 14 days ending on reading 2's as-of date       | on or after the as-of date     |

**You take reading 1 on the day you start its log.** That day is usually after the corpus took its first record, so
reading 1's as-of date is in the past. The script counts the history as it stood at the end of the as-of day, in UTC,
so a past date gives the same counts as a reading taken on it. `question_log_baseline` then says `yes`.

**You take reading 2 on its as-of date, the day its log ends.** Where that date is still ahead, wait for it. Start the
log 13 days before it.

**Where reading 2's as-of date has already passed, reading 2 still waits three months for its log.** Its git counts
are ready, but the two logs must be three months apart to compare. Start its log three months after reading 1's log
started. Pass its first day to `-s`, and run the script on or after its last day.

## Taking a reading

Do every step for each reading. The second reading repeats the first, so use the same addresses for `-o` and `-x`.

1. **Update the clone.** Work in a clone of the repository that contains the corpus. Fetch, then point `origin/HEAD`
   at the default branch:

   ```sh
   git fetch origin
   git remote set-head origin --auto
   ```

   The script reads the default branch from `origin/HEAD`, and stops where it is not set. Pass `-b origin/<branch>`
   where `set-head` does not work. Name the remote's branch, because a local branch can be behind it.

2. **Find the day the corpus took its first record.** Run the script once with today's date, into an empty scratch
   folder:

   ```sh
   ./capture.sh -c <corpus> -d <today> -r 1 -o <your email> /tmp/capture-first
   ```

   Read `first_record` in `/tmp/capture-first/raw/reading.tsv`. Then delete the folder.

3. **Choose the reading's as-of date and log days** from [When to take each reading](#when-to-take-each-reading).

4. **List the addresses you commit under.** Every one of them goes to `-o`, separated by commas:

   ```sh
   git log --format='%aE' -- <corpus> | sort | uniq -c
   ```

   The script stops where no commit to the corpus is by an address under `-o`. Pass the address of any automation
   account that commits to the corpus, such as a bot, to `-x`. Where one person commits under two addresses, a
   `.mailmap` file at the root of the repository merges them, and the script reads it.

5. **Keep the question log for two weeks.** Copy `question-log.csv`, and start it on the log's first day.
   [The question log](#the-question-log) says what goes in it.

6. **Write the findings file, where the script cannot query the tracker.** [Findings](#findings) says when that is,
   and what the file contains. Pass it to `-f`.

7. **Run the script** into an empty folder, on the day [the table](#when-to-take-each-reading) gives:

   ```sh
   ./capture.sh -c <corpus> -d <as-of> -r <1 or 2> -o <your emails> -l <your log> -s <log's first day> <empty folder>
   ```

   Run it from anywhere. `-c` is the folder that contains the corpus's `.corpus.yaml`, at the root of a repository or
   in a folder inside one. Leave `-s` out where the log's first day is the default the arguments table gives.

8. **Keep `raw/`, and publish `aggregate.md`.**

The script stops with a message, and writes no `aggregate.md`, where it cannot take a count. Fix what the message says
and run it again into an empty folder.

### The script's arguments

| Argument | Required | What it takes                                                                                                              |
|----------|----------|----------------------------------------------------------------------------------------------------------------------------|
| `-c`     | yes      | the folder that contains the corpus's `.corpus.yaml`                                                                       |
| `-d`     | yes      | the as-of date, as `YYYY-MM-DD`                                                                                            |
| `-r`     | yes      | the reading: `1` or `2`                                                                                                    |
| `-o`     | yes      | every email address the corpus owner commits under, separated by commas                                                    |
| `-x`     | no       | email addresses of automation accounts to leave out, separated by commas                                                   |
| `-b`     | no       | the default branch, as `origin/<branch>`, where `origin/HEAD` is not set                                                   |
| `-f`     | no       | the findings file, where the script cannot query the tracker                                                               |
| `-l`     | no       | the question log                                                                                                           |
| `-s`     | no       | the log's first day, as `YYYY-MM-DD`. It defaults to the as-of date for reading 1, and to 13 days before it for reading 2. |
| last     | yes      | an empty folder to write the reading to                                                                                    |

## How the script counts

### The snapshot

The snapshot is the last commit on the default branch made on or before the as-of day, following the branch by first
parents. Every git count reads the history the snapshot contains, and nothing after it.

The corpus's descriptor at the snapshot lists the adopted types. The descriptor in your working tree gives the
tracker, because the findings are queried where they are today.

### Records

A record is a Markdown file in the folder of an adopted type, at any depth, whose name does not start with `_`. So
`_index.md` and `_template.md` are not records, and neither is a type's root page beside its folder.

A record's author is the git author of the commit that first added it. The script follows the file back across
renames to find that commit. It does not follow a copy, so a record started as a copy of another is credited to the
person who added the copy.

### Authors

An author is a person who added or changed a record, in any commit up to the snapshot. A person is one email address,
as `.mailmap` resolves it. The corpus owner is one person, however many addresses you pass to `-o`.

An address under `-x` is no author. Its records count in `records` and in `records_by_excluded`, and in neither
`records_by_owner` nor `records_by_others`.

A new author is a person who is not the corpus owner, and made at least one commit to the corpus folder. Where the
corpus is the whole repository, that is every other person who committed to the repository. For each new author the
script finds two days:

* **The first commit** is the day of their earliest commit to the corpus folder, by author date.
* **The first merged record** is the first record they added to reach the default branch. Its day is the commit
  date of the commit on the branch that brought it in: the merge commit, or the commit itself where nothing was merged.

The days between them are counted as calendar days in UTC.

#### Squash merges

Where your repository squashes a pull request into one commit, the branch commits never reach the default branch. The
first commit is then the squashed one, and an author whose first commit added a record scores 0 days.

Some platforms set a squashed commit's author to the person who completed the merge. Check yours before the first
reading. Take three pull requests that other people opened, and compare each one's author with the author of the
commit that landed it:

```sh
git log -1 --format='%an <%aE>' <the commit that landed the pull request>
```

Where they differ, every author count credits the person who merged. The counts are only as good as that attribution.

### Findings

A finding is an issue on the corpus's tracker, filed with the `kac:finding` label or tag. The tracker is the
descriptor's `tracker` block. Where the descriptor states none, it is derived from `publishing`, as `kac export`
derives it.

| Tracker               | How the script reads the findings                                                |
|-----------------------|----------------------------------------------------------------------------------|
| `github`              | It queries GitHub with `gh`, on the host the address gives, unless you pass `-f` |
| `azure-devops`        | It reads the file you pass to `-f`, and stops where you pass none                |
| `none`, or no tracker | It counts no findings, and leaves the findings fields empty                      |
| any other target      | It stops                                                                         |

On GitHub, the script counts every issue, open or closed, created on or before the as-of day, that either:

* has the `kac:finding` label
* has no label, and its body opens with a `yaml kac-finding` block

The `raise-finding` skill files a finding without the label where the repository has none. The script leaves out an
issue whose block states a `corpus:` other than this corpus's name, because one repository can contain several corpora.

The script stops where `gh` is not signed in to the host, cannot read the issues, or returns 1000 issues, its limit.
Write the findings file and pass `-f` where you cannot fix that.

#### The findings file

Write one line for each finding: the day it was filed, a tab, and its address. Write the address as a record cites it
in `sources`. A line starting with `#` is ignored. The script stops at any other line it cannot read.

```text
# filed<tab>address
2026-11-03	https://dev.azure.com/<org>/<project>/_workitems/edit/412
```

On Azure DevOps, this command writes the file. Give `--project` the project's name as it is spelled, in quotes. Give
`base` the address of the project as `tracker.base` in the descriptor states it. Where the `tracker` block states an
`area`, add `AND [System.AreaPath] UNDER '<project>\<area>'` to the query, so it leaves out other corpora's work items.

```sh
az boards query --org https://dev.azure.com/<org> --project "<project>" \
  --wiql "SELECT [System.Id], [System.CreatedDate] FROM WorkItems WHERE [System.Tags] CONTAINS 'kac:finding'" \
  --query '[].[fields."System.CreatedDate", id]' -o tsv |
  awk -F'\t' -v base='https://dev.azure.com/<org>/<project>' \
    '{ print substr($1, 1, 10) "\t" base "/_workitems/edit/" $2 }' >findings.tsv
```

Write every finding in the file, whatever its date. The script counts only those filed on or before the as-of day.

#### A finding that became a record

A finding became a record when a record at the snapshot cites the finding's address in its frontmatter, as a
`resource` under `sources`. That is the link the `harvest-findings` skill writes:

```yaml
sources:
  - { resource: https://github.com/<owner>/<repo>/issues/<issue> }
```

The script compares the two addresses in lower case. It removes the scheme, a leading `www.`, a trailing `.git` and a
trailing `/`, and writes each space as `%20`. So `My Project` and `My%20Project` in a project's name match.

### Types with no record

A type the descriptor at the snapshot adopts, with no record in its folder.

### Moves

A move is a rename, between one commit on the default branch and the one before it, that takes a record from the
folder of one type to the folder of another. A type counts where any version of the descriptor up to the snapshot
adopted it, so a move out of a type the corpus later dropped still counts. Git finds a rename where the two files are
at least half the same.

### A corpus that moved

Where the corpus moved to its folder from somewhere else in the repository, the script reads the move as the day its
records were written. Their history starts there.

## The question log

For the two weeks starting on the log's first day, write one line for each question somebody asks you about how
something is done. Write the day, and `yes` where your answer included a link to a record, or `no` where it did not.
Write nothing else: no name, and no words of the question.

```text
date,linked
2026-11-03,yes
2026-11-04,no
```

The script counts the lines from the log's first day to 13 days after it. It reports any line outside those days, and
any line not marked `yes` or `no`, and leaves it out.

**Where reading 1's log starts after the corpus took its first record, that log is the baseline.** The corpus already
existed before the log started, so the log cannot show the team without it. `question_log_baseline` says `yes`.

## The aggregate

`aggregate.md` is a table of fields and values. A value is empty where the reading could not measure it: no new
author, no tracker the script could read, or no question log. A rate is a fraction to two decimal places, and is empty
where it would divide by 0.

| Field                               | Value                                                                                                   |
|-------------------------------------|---------------------------------------------------------------------------------------------------------|
| `trial`                             | `capture`                                                                                               |
| `method`                            | the version of this method. It moves, here and in `capture.sh`, when a definition on this page changes. |
| `reading`                           | `1` or `2`                                                                                              |
| `days_since_first_record`           | the days from the corpus's first record to the as-of date                                               |
| `types_adopted`                     | the types the descriptor at the snapshot adopts                                                         |
| `types_without_record`              | of those, the types with no record                                                                      |
| `records`                           | the records at the snapshot                                                                             |
| `records.<type>`                    | the records of that type at the snapshot. One field for each adopted type, zero included.               |
| `records_by_owner`                  | the records whose author is the corpus owner                                                            |
| `records_by_others`                 | the records whose author is neither the corpus owner nor an address under `-x`                          |
| `records_by_excluded`               | the records whose author is an address under `-x`                                                       |
| `records_by_others_rate`            | `records_by_others` divided by `records`                                                                |
| `authors`                           | the people who added or changed a record, with the corpus owner counted once                            |
| `authors_not_owner`                 | of those, the people who are not the corpus owner                                                       |
| `new_authors`                       | the people other than the owner who committed to the corpus folder                                      |
| `new_authors_with_record`           | of those, the people with a merged record                                                               |
| `days_to_first_record_median`       | the median of the days from first commit to first merged record, over those people                      |
| `days_to_first_record_min`          | the smallest of those days                                                                              |
| `days_to_first_record_max`          | the largest of those days                                                                               |
| `tracker_target`                    | `github`, `azure-devops` or `none`                                                                      |
| `findings_source`                   | `github` where the script queried GitHub, `file` where it read `-f`, `none` otherwise                   |
| `findings_filed`                    | the findings filed on or before the as-of day                                                           |
| `findings_became_records`           | of those, the findings a record at the snapshot cites                                                   |
| `findings_became_records_rate`      | `findings_became_records` divided by `findings_filed`                                                   |
| `records_moved`                     | the moves up to the snapshot                                                                            |
| `moved.<from>.<to>`                 | the moves from one type's folder to another's. One field for each pair that had a move.                 |
| `question_log_baseline`             | `yes` where this is reading 1 and its log started after the first record                                |
| `questions_logged`                  | the questions in the log                                                                                |
| `questions_answered_with_link`      | of those, the questions answered with a link to a record                                                |
| `questions_answered_with_link_rate` | `questions_answered_with_link` divided by `questions_logged`                                            |

## The raw results

| File                   | What it lists                                                                                                              |
|------------------------|----------------------------------------------------------------------------------------------------------------------------|
| `reading.tsv`          | the branch, the snapshot commit, the as-of date, the log's days, the first record's day, the corpus's name and the tracker |
| `descriptor.tsv`       | the descriptor at the snapshot, as the script read it                                                                      |
| `descriptor-today.tsv` | the descriptor in the working tree, as the script read it                                                                  |
| `records.tsv`          | each record at the snapshot, with its type, its author, and whether that is the owner                                      |
| `landings.tsv`         | each record as it reached the default branch, with the time and its author                                                 |
| `authors.tsv`          | each person who committed to the corpus, with their first commit and first merged record                                   |
| `moves.tsv`            | each move, with its day, both types and both paths                                                                         |
| `sources.tsv`          | each address a record cites in `sources`                                                                                   |
| `findings.tsv`         | each finding counted, with its day, its address and the record that cites it                                               |
| `question-log.csv`     | the log's lines the script counted. It is there only where you passed `-l`.                                                |

Check a surprising count against these files before you publish the aggregate.

## Reading the result

Compare the second reading with the first, one signal at a time. Give each signal a verdict, and give the run no
overall pass. One overall pass would hide which signal moved. A signal passes where it moved, so the verdicts are this
trial's pass rule.

| Signal                              | Field                               | Moved when                       |
|-------------------------------------|-------------------------------------|----------------------------------|
| Somebody else writes records        | `records_by_others`                 | It is above the first reading's. |
| A new author gets a record merged   | `new_authors_with_record`           | It is above the first reading's. |
| Findings become records             | `findings_became_records_rate`      | It is above the first reading's. |
| Answers point to records more often | `questions_answered_with_link_rate` | It is above the first reading's. |

Each verdict is `moved`, `did not move` or `no baseline`:

* **A count** whose first value is empty or 0 is compared with 0.
* **A rate** whose first value is empty has no baseline. Its verdict is `no baseline`.
* **A rate** whose second value is empty did not move.

`new_authors_with_record` counts every new author up to the snapshot, so the second reading includes everyone the first
counted. It moved where somebody new got a record merged between the readings.

Where reading 1's `question_log_baseline` is `yes`, its log was kept by a team that already had the corpus. The
verdict for `questions_answered_with_link_rate` then compares with that team, not with a team before the corpus. Say
so beside that verdict.

The per-type counts, the types with no record and the moves are read separately, to decide which types the corpus has
earned.

## Runs

| Run                           | Corpus                 | What it measured                                                  |
|-------------------------------|------------------------|-------------------------------------------------------------------|
| [Dry run](runs/2026-10-16.md) | `examples/engineering` | The method, run over a public example corpus. It is not a result. |
