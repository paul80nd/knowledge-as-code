#!/usr/bin/env python3
"""Count each surface kac and this repository present, and name what reaches each one.

Run from the repository root: python3 trials/surfaces/measure.py > trials/surfaces/runs/<date>.md
With --corpus <path> it reads that one corpus instead, and prints only its types, fields and rules.
It reads tracked files only, so two runs over the same commit print the same reading.
"""

import json
import os
import re
import subprocess
import sys
from collections import Counter, defaultdict

import yaml

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
KAC = ["dotnet", "run", "--no-build", "--project", os.path.join(ROOT, "tooling/kac"), "--"]

if len(sys.argv) == 3 and sys.argv[1] == "--corpus":
    SOURCE = os.path.abspath(sys.argv[2])
    CORPORA = [os.path.basename(SOURCE)]
    BASE = {CORPORA[0]: SOURCE}
elif len(sys.argv) == 1:
    SOURCE = None
    CORPORA = ["examples/dog-fooding", "examples/engineering", "examples/library", "examples/payments", "template"]
    BASE = {c: os.path.join(ROOT, c) for c in CORPORA}
else:
    sys.exit("usage: measure.py [--corpus <path>]")

# Where a mention counts as a use, and the column it lands in.
GROUPS = [
    ("CI", lambda p: p.startswith(".github/workflows/") or p.startswith("template/.github/workflows/")
     or p.endswith("azure-pipelines.yml")),
    ("plugin", lambda p: p.startswith("template/.plugin/")),
    ("docs", lambda p: p.startswith("docs/") or p in ("README.md", "tooling/kac/PACKAGE.md")),
    ("guidance", lambda p: p.endswith("CLAUDE.md") or p.startswith(".claude/skills/")
     or "/processes/" in p),
    ("tests", lambda p: p.startswith("tooling/kac.tests/") or p.startswith("tooling/kac.features/")
     or p.startswith("tooling/kac.goldens/") or p.startswith("tooling/tests/")),
    ("trials", lambda p: p.startswith("trials/") and not p.startswith("trials/surfaces/")),
    ("tool", lambda p: p.startswith("tooling/kac/") or p.startswith("tooling/kac.core/")),
    ("schema", lambda p: p.startswith(".schema/")),
    ("corpora", lambda p: p.startswith("examples/") or p.startswith("template/")),
]
# How a command is named: on a command line, as a test's quoted argument, or as a feature step such as "When I export".
INVOKE = r'(\bkac |-- |"|\bI )'
TEXT = (".md", ".yml", ".yaml", ".cs", ".feature", ".sh", ".json", ".mjs", ".txt", ".csv")


def tracked(root):
    out = subprocess.run(["git", "ls-files", "."], cwd=root, capture_output=True, text=True, check=True).stdout
    return [p for p in out.splitlines() if p.endswith(TEXT)]


# A corpus read with --corpus is keyed under its folder name, as an example corpus is keyed under its path.
CONTENT = {}
for _p in tracked(SOURCE or ROOT):
    try:
        with open(os.path.join(SOURCE or ROOT, _p), encoding="utf-8") as f:
            CONTENT[f"{CORPORA[0]}/{_p}" if SOURCE else _p] = f.read()
    except (UnicodeDecodeError, FileNotFoundError):
        pass


def group_of(path):
    for name, test in GROUPS:
        if test(path):
            return name
    return None


def uses(pattern, exclude=()):
    """Files matching a line regex, counted per group."""
    rx = re.compile(pattern, re.M)
    counts = Counter()
    for p, text in CONTENT.items():
        g = group_of(p)
        if g and p not in exclude and rx.search(text):
            counts[g] += 1
    return counts


def cells(counts):
    return [str(counts.get(name, 0) or "") for name, _ in GROUPS]


def table(head, rows):
    lines = ["| " + " | ".join(head) + " |", "|" + "|".join("---" for _ in head) + "|"]
    lines += ["| " + " | ".join(str(c) for c in r) + " |" for r in rows]
    return "\n".join(lines)


def words(path):
    text = CONTENT.get(path, "")
    text = re.sub(r"^---\n.*?\n---\n", "", text, flags=re.S)
    return len(re.findall(r"\S+", text))


def kac(*args):
    return subprocess.run(KAC + list(args), cwd=os.path.join(ROOT, "examples/dog-fooding"),
                          capture_output=True, text=True).stdout


def frontmatter(path):
    m = re.match(r"^---\n(.*?)\n---\n", CONTENT.get(path, ""), re.S)
    if not m:
        return {}
    try:
        return yaml.safe_load(m.group(1)) or {}
    except yaml.YAMLError:
        return {}


# --- Load the schema once -----------------------------------------------------------------------------------------

def schema_dir(start):
    """The nearest `.schema/` at or above the corpus, as `kac` finds it."""
    here = start
    while not os.path.isdir(os.path.join(here, ".schema")):
        if os.path.dirname(here) == here:
            sys.exit(f"no .schema/ at or above {start}")
        here = os.path.dirname(here)
    return os.path.join(here, ".schema")


SCHEMA_DIR = schema_dir(SOURCE or ROOT)
SCHEMA = {}
for name in sorted(os.listdir(SCHEMA_DIR)):
    if name.endswith(".yaml") and not name.startswith("_"):
        with open(os.path.join(SCHEMA_DIR, name), encoding="utf-8") as f:
            SCHEMA[name[:-5]] = yaml.safe_load(f)
with open(os.path.join(SCHEMA_DIR, "_universal.yaml"), encoding="utf-8") as f:
    UNIVERSAL = yaml.safe_load(f)


def records(corpus, folder):
    prefix = f"{corpus}/{folder}/"
    return [p for p in CONTENT if p.startswith(prefix) and p.endswith(".md")
            and os.path.basename(p) not in ("_index.md", "_template.md", "README.md")]


RECORDS = {t: {c: records(c, s["folder"]) for c in CORPORA} for t, s in SCHEMA.items()}


def adopted(corpus):
    with open(os.path.join(BASE[corpus], ".corpus.yaml"), encoding="utf-8") as f:
        return (yaml.safe_load(f) or {}).get("types") or []


ADOPTED = {c: adopted(c) for c in CORPORA}
SHORT = {"examples/dog-fooding": "dog", "examples/engineering": "eng", "examples/library": "lib",
         "examples/payments": "pay", "template": "tpl"}
GROUP_HEAD = [name for name, _ in GROUPS]


# --- Commands and flags -------------------------------------------------------------------------------------------

def commands():
    top = kac("--help")
    names = re.findall(r"^    ([a-z]+)\b", top.split("COMMANDS:")[1], re.M)
    rows, flag_rows = [], []
    for cmd in names:
        rows.append([f"`{cmd}`"] + cells(uses(rf"{INVOKE}{cmd}\b")))
        flags = re.findall(r"^\s+(?:-\w, )?(--[a-z-]+)", kac(cmd, "--help").split("OPTIONS:")[1], re.M)
        for flag in flags:
            if flag in ("--help", "--no-color"):
                continue
            flag_rows.append([f"`{cmd} {flag}`"] + cells(uses(rf"{INVOKE}{cmd}\b.*{re.escape(flag)}\b")))
    return names, rows, flag_rows


# --- Checks -------------------------------------------------------------------------------------------------------

def checks():
    catalogue = json.loads(kac("checks", "--json"))["checks"]
    return catalogue, Counter(c["severity"] for c in catalogue)


def rules():
    # A rule with no severity reports on no record: it is unenforced, or a report reads it.
    return [[f"`{rule['id']}`", rule.get("severity", "none"), f"`{t}`"] + [len(RECORDS[t][c]) for c in CORPORA]
            for t, s in SCHEMA.items() for rule in s.get("rules") or []]


# --- Types and fields ---------------------------------------------------------------------------------------------

def set_in(path, field):
    value = frontmatter(path).get(field)
    return value not in (None, "", [], {})


def types():
    rows = []
    for t, s in SCHEMA.items():
        held = [len(RECORDS[t][c]) for c in CORPORA]
        where = ", ".join(SHORT.get(c, c) for c in CORPORA if t in ADOPTED[c])
        rows.append([f"`{t}`", s.get("tier", "")] + held + [where])
    return rows


def fields():
    rows = []
    every = [p for t in SCHEMA for c in CORPORA for p in RECORDS[t][c]]
    for f in (UNIVERSAL.get("fields") or UNIVERSAL):
        if not isinstance(f, str):
            continue
        rows.append(["(universal)", f"`{f}`", "", sum(set_in(p, f) for p in every), len(every)])
    for t, s in SCHEMA.items():
        held = [p for c in CORPORA for p in RECORDS[t][c]]
        exported = set((s.get("export") or {}).get("fields") or [])
        for f, spec in (s.get("fields") or {}).items():
            # A field with `from:` takes its value from the record's folders, never from its frontmatter.
            setting = "derived" if (spec or {}).get("from") else sum(set_in(p, f) for p in held)
            rows.append([f"`{t}`", f"`{f}`", "yes" if f in exported else "", setting, len(held)])
    return rows


# --- Bundled skills, hooks and views ------------------------------------------------------------------------------

def plugin():
    with open(os.path.join(ROOT, "template/.plugin/.claude-plugin/plugin.json"), encoding="utf-8") as f:
        components = json.load(f)["metadata"]["components"]
    rows = []
    for c in components:
        name = os.path.basename(c["path"])
        path = f"template/.plugin/{c['path']}/SKILL.md" if c["path"].startswith("skills/") else None
        reads = ", ".join(c.get("requires", []) + [f"({o})" for o in c.get("optional", [])])
        runs = sum(1 for p, text in CONTENT.items() if p.startswith("trials/") and "/runs/" in p
                   and re.search(rf"\b{re.escape(name)}\b", text))
        rows.append([f"`{c['path']}`", reads or "none", words(path) if path else ""]
                    + cells(uses(rf"\b{re.escape(name)}\b", exclude={path} if path else ())) + [runs])
    return rows


# --- Documentation pages ------------------------------------------------------------------------------------------

def nav_pages(node, out):
    if isinstance(node, str):
        out.append(node)
    elif isinstance(node, list):
        for n in node:
            nav_pages(n, out)
    elif isinstance(node, dict):
        for v in node.values():
            nav_pages(v, out)
    return out


LINK = re.compile(r"\]\(([^)\s#]+)(?:#[^)]*)?\)|^\[[^\]]+\]:\s*(\S+)", re.M)
SITE = "paul80nd.github.io/knowledge-as-code/"


def inbound():
    """Each docs page's inbound links, keyed by its path under docs/."""
    links = defaultdict(set)
    for p, text in CONTENT.items():
        for m in re.finditer(re.escape(SITE) + r"([a-z0-9/_-]*)", text):
            slug = m.group(1).strip("/")
            target = f"{slug}.md" if slug else "index.md"
            if f"docs/{slug}/index.md" in CONTENT:
                target = f"{slug}/index.md"
            links[target].add(p)
        if p.endswith(".md"):
            for m in LINK.finditer(text):
                href = (m.group(1) or m.group(2) or "").split("#")[0]
                if not href or "://" in href or not href.endswith(".md"):
                    continue
                target = os.path.normpath(os.path.join(os.path.dirname(p), href))
                if target.startswith("docs/") and target != p:
                    links[target[5:]].add(p)
    return links


def docs():
    # mkdocs.yml carries Python tags safe_load refuses, so only the nav block is parsed.
    block = re.search(r"^nav:\n(?:[ -].*\n)+", CONTENT["mkdocs.yml"], re.M).group(0)
    pages = nav_pages(yaml.safe_load(block)["nav"], [])
    links = inbound()
    rows = []
    for page in pages:
        src = links.get(page, set())
        site = sum(1 for p in src if p.startswith("docs/"))
        by = Counter("README or PACKAGE" if group_of(p) == "docs" else group_of(p) or "other"
                     for p in src if not p.startswith("docs/"))
        rows.append([f"`{page}`", words(f"docs/{page}"), site, len(src) - site,
                     ", ".join(f"{k} {v}" for k, v in sorted(by.items()))])
    return rows


# --- Processes, playbooks and the repository's own skills ---------------------------------------------------------

def guidance():
    rows = []
    for p in sorted(CONTENT):
        if p.startswith("examples/dog-fooding/processes/") and not os.path.basename(p).startswith("_"):
            key = frontmatter(p).get("id") or os.path.basename(p)
            kind = "process"
        elif p.startswith(".claude/skills/i-want-to/playbooks/"):
            key, kind = os.path.basename(p), "playbook"
        elif p.startswith(".claude/skills/") and p.endswith("/SKILL.md"):
            key, kind = p.split("/")[2], "repo skill"
        else:
            continue
        rx = rf"\b{re.escape(key)}\b" if kind != "playbook" else re.escape(key)
        cited = [q for q, text in CONTENT.items() if q != p and re.search(rx, text)
                 and not q.endswith("_index.md")]
        rows.append([kind, f"`{key}`", words(p), len(cited)]
                    + cells(Counter(group_of(q) for q in cited if group_of(q))))
    return rows


# --- The reading --------------------------------------------------------------------------------------------------

def main():
    commit = subprocess.run(["git", "rev-parse", "--short", "HEAD"], cwd=ROOT,
                            capture_output=True, text=True).stdout.strip()
    type_rows, field_rows, check_rows = types(), fields(), rules()
    idle_rules = sum(1 for r in check_rows if sum(r[3:]) == 0)
    unset = sum(1 for r in field_rows if r[3] == 0)
    unchecked = sum(1 for r in check_rows if r[1] == "none")
    out = sys.stdout

    if SOURCE:
        # One corpus is measured against the types it adopted, so a type it declined adds nothing unset.
        kept = {f"`{t}`" for t in ADOPTED[CORPORA[0]]} | {"(universal)"}
        type_rows, field_rows, check_rows = ([r for r in rows if r[0] in kept or r[2] in kept]
                                             for rows in (type_rows, field_rows, check_rows))
        unset = sum(1 for r in field_rows if r[3] == 0)
        unchecked = sum(1 for r in check_rows if r[1] == "none")
        idle_rules = sum(1 for r in check_rows if sum(r[3:]) == 0)
        out.write(f"# Surface measurement of `{CORPORA[0]}`\n\n")
        out.write("Taken by `measure.py --corpus`.\n[The README](../README.md) says what each column counts.\n\n")
        out.write("## Sizes\n\n" + table(["Surface", "Count"], [
            ["Types the schema declares", len(SCHEMA)],
            ["Types adopted", len(ADOPTED[CORPORA[0]])],
            ["Schema fields, universal and per type", len(field_rows)],
            ["Schema fields no record sets", unset],
            ["Rules a type declares", len(check_rows)],
            ["Rules with no severity", unchecked],
            ["Rules over a type with no record", idle_rules],
        ]) + "\n\n")
        out.write("## Types\n\n" + table(["Type", "Tier", "Records", "Adopted by"], type_rows) + "\n\n")
        out.write("## Schema fields\n\n" + table(["Type", "Field", "Exported", "Records setting it", "Records"],
                                                 field_rows) + "\n\n")
        out.write("## Rules a type declares\n\n" + table(["Check", "Severity", "Type", "Records"], check_rows) + "\n")
        return

    version = kac("--version").strip()
    names, cmd_rows, flag_rows = commands()
    catalogue, severity = checks()
    plugin_rows, doc_rows, guide_rows = plugin(), docs(), guidance()
    rule_checks = len(check_rows)
    out.write(f"# Surface measurement at `{commit}`\n\n")
    out.write(f"Taken by `trials/surfaces/measure.py` against `kac` {version.split('+')[0]}.\n"
              "[The README](../README.md) says what each column counts.\n\n")
    out.write("## Sizes\n\n")
    out.write(table(["Surface", "Count"], [
        ["Commands", len(names)],
        ["Flags, leaving out `--help` and `--no-color`", len(flag_rows)],
        ["Checks", f"{len(catalogue)}: " + ", ".join(f"{v} {k}" for k, v in sorted(severity.items()))],
        ["Rules a type declares, counted once for each type declaring it", rule_checks],
        ["Rules with no severity, so no record is reported against them", unchecked],
        ["Rules over a type no corpus here holds a record of", idle_rules],
        ["Types", len(type_rows)],
        ["Schema fields, universal and per type", len(field_rows)],
        ["Schema fields no record in any corpus here sets", unset],
        ["Plugin components", len(plugin_rows)],
        ["Documentation pages in the nav", len(doc_rows)],
        ["Processes, playbooks and repository skills", len(guide_rows)],
    ]) + "\n\n")
    out.write("## Commands\n\n" + table(["Command"] + GROUP_HEAD, cmd_rows) + "\n\n")
    out.write("## Flags\n\n" + table(["Flag"] + GROUP_HEAD, flag_rows) + "\n\n")
    out.write("## Types\n\n" + table(["Type", "Tier"] + [SHORT.get(c, c) for c in CORPORA] + ["Adopted by"],
                                     type_rows) + "\n\n")
    out.write("## Plugin components\n\n" + table(["Component", "Reads", "Words"] + GROUP_HEAD + ["Trial runs"],
                                                 plugin_rows) + "\n\n")
    out.write("## Processes, playbooks and repository skills\n\n"
              + table(["Kind", "Name", "Words", "Cited by"] + GROUP_HEAD, guide_rows) + "\n\n")
    out.write("## Documentation pages\n\n"
              + table(["Page", "Words", "Linked from the site", "Linked from elsewhere", "Where elsewhere"],
                      doc_rows) + "\n\n")
    out.write("## Schema fields\n\n" + table(["Type", "Field", "Exported", "Records setting it", "Records"],
                                             field_rows) + "\n\n")
    out.write("## Rules a type declares\n\n"
              + table(["Check", "Severity", "Type"] + [SHORT.get(c, c) for c in CORPORA], check_rows) + "\n")


if __name__ == "__main__":
    main()
