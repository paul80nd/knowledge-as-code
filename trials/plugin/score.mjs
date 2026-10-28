#!/usr/bin/env node
// Scores one run of the plugin trial from the result files `claude plugin eval` wrote.
//
//   score.mjs [-a <adjudication.tsv>] [-k <hook stage aggregate-result.json>] \
//     <plugin stage aggregate-result.json> <prose stage aggregate-result.json> <run folder> [<excluded.tsv>]
//
// Writes sessions.tsv, aggregate.md, and each result file with every absolute path cut to its last part.
// A case listed in <excluded.tsv> is left out of sessions.tsv and every count, and aggregate.md reports how many.
// A session <adjudication.tsv> lists is counted by the verdict read by hand, and the graders' verdict is kept beside
// it. Without -k the hook arm has no sessions, and every hook field is 0 or empty. A prose result of - does the same
// for the prose arm, and each lead over an arm with no sessions is empty.
import { mkdirSync, readFileSync, writeFileSync, writeSync } from "node:fs";

const method = 6;

// Prints a message to stderr and exits. The write is synchronous, so the exit cannot cut the message off.
function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

const usage = "usage: score.mjs [-a <adjudication.tsv>] [-k <hook stage result>] <plugin stage result> "
  + "<prose stage result | -> <run folder> [<excluded.tsv>]";
let adjudication = "";
let hookJson = "";
const args = process.argv.slice(2);
while (args.length > 0 && args[0].startsWith("-") && args[0] !== "-") {
  const opt = args.shift();
  if (opt === "--") break;
  if (opt[1] !== "a" && opt[1] !== "k") stop(2, usage);
  const value = opt.length > 2 ? opt.slice(2) : args.shift();
  if (value === undefined) stop(2, usage);
  if (opt[1] === "a") adjudication = value;
  else hookJson = value;
}
if (args.length !== 3 && args.length !== 4) stop(2, usage);

const [pluginJson, proseArg, out, excludedFile] = args;
const proseJson = proseArg === "-" ? "" : proseArg;

// The lines of a text file, less the carriage return a Windows editor writes before each line end.
function lines(path) {
  const body = readFileSync(path, "utf8").replaceAll("\r\n", "\n");
  const all = body.split("\n");
  if (body.endsWith("\n")) all.pop();
  return all;
}

// Each case excluded.tsv lists, by the last part of the path in its first column.
let excluded = [];
if (excludedFile !== undefined) {
  try {
    excluded = lines(excludedFile).map((line) => line.split("\t")[0].replace(/.*\//, ""));
  } catch {
    writeSync(2, `score.mjs: cannot read ${excludedFile}, so no case is excluded\n`);
  }
}
const skip = new Set(excluded.join(" ").split(" "));

// Every result file given, in the order their costs are added.
const given = [pluginJson, proseJson, hookJson].filter((f) => f !== "");
const results = new Map();
for (const f of given) {
  try {
    results.set(f, JSON.parse(readFileSync(f, "utf8")));
  } catch (error) {
    stop(1, `score.mjs: cannot read ${f}: ${error.message}`);
  }
}

// A value as JSON on one line, with DEL written as \u007f.
const json = (v) => (v === undefined ? "null" : JSON.stringify(v).replaceAll("\u007f", "\\u007f"));
// A value as text: a string as it is, anything else as JSON.
const text = (v) => (typeof v === "string" ? v : json(v));
// Whether a value counts as present. Only null and false do not.
const present = (v) => v !== null && v !== undefined && v !== false;

for (const f of given) {
  if (text(results.get(f).partial) !== "false") {
    stop(1, `score.mjs: ${f} is a partial run, which this trial does not score`);
  }
}
for (const f of [proseJson, hookJson]) {
  if (f !== "" && text(results.get(f).suite?.ablation) !== "none") {
    stop(1, `score.mjs: ${f} did not run under --ablation none`);
  }
}

mkdirSync(`${out}/results`, { recursive: true });

// A value as one TSV cell, with each backslash, tab, line feed and carriage return escaped.
function cell(v) {
  if (v === null || v === undefined) return "";
  if (typeof v !== "string") return String(v);
  return v.replace(/[\\\t\n\r]/g, (c) => ({ "\\": "\\\\", "\t": "\\t", "\n": "\\n", "\r": "\\r" })[c]);
}

// One line per session. Graders whose names share the part before `--` are alternatives, and pass where one does.
// A verdict is 1 where every grader or set of alternatives with that prefix passed, 0 where one failed, and empty
// where the case has none. A case's kind is the folder it sits in, house or control. A session's run is its place in
// its arm, so run 2 is the second batch's where batches were merged. A stage run under --ablation none takes the arm
// it is given, and the plugin stage's two arms are plugin and none.
function sessions(result, only) {
  const verdict = (run, prefix) => {
    const sets = new Map();
    for (const g of (run.graders ?? []).filter((g) => g.name.startsWith(prefix))) {
      const set = g.name.split("--")[0];
      sets.set(set, (sets.get(set) ?? false) || present(g.passed));
    }
    if (sets.size === 0) return "";
    return [...sets.values()].every(Boolean) ? "1" : "0";
  };
  const rows = [];
  for (const kase of result.cases.filter((c) => !skip.has(c.name))) {
    const kind = kase.dir.split(/[/\\]/).at(-2);
    for (const [key, runs] of Object.entries(kase.arms)) {
      const arm = only !== "" ? only : key === "with" ? "plugin" : "none";
      runs.forEach((run, i) => {
        rows.push([kind, arm, kase.name, i + 1, run.error === null || run.error === undefined ? "0" : "1",
          verdict(run, "task-"), verdict(run, "comply-"), verdict(run, "cite-"), verdict(run, "fired-"),
          present(run.costUsd) ? run.costUsd : 0, present(run.turns) ? run.turns : ""].map(cell));
      });
    }
  }
  return rows;
}

const graded = [
  ...sessions(results.get(pluginJson), ""),
  ...(proseJson !== "" ? sessions(results.get(proseJson), "prose") : []),
  ...(hookJson !== "" ? sessions(results.get(hookJson), "hook") : []),
];

// Appends each session's adjudicated verdict, which is empty where nobody read the session by hand. A row whose graders
// column disagrees with the verdict here, or that names no session, means the file belongs to another run.
const byHand = new Map();
if (adjudication !== "") {
  let adjudicationLines;
  try {
    adjudicationLines = lines(adjudication);
  } catch {
    stop(2, `score.mjs: cannot read ${adjudication}`);
  }
  for (const line of adjudicationLines.slice(1)) {
    const [kase = "", arm = "", run = "", graders = "", verdict = ""] = line.split("\t");
    byHand.set(`${kase}\t${arm}\t${run}`, { graders, verdict });
  }
}
const verdictOf = (task, comply) => (task === "0" ? "no task" : comply === "1" ? "pass" : "fail");
let bad = false;
const used = new Set();
const rows = graded.map((row) => {
  const key = `${row[2]}\t${row[1]}\t${row[3]}`;
  const read = byHand.get(key);
  if (read === undefined) return [...row, ""];
  const theirs = verdictOf(row[5], row[6]);
  if (read.graders !== theirs) {
    writeSync(2, `score.mjs: the graders gave ${row[2]} ${row[1]} run ${row[3]} ${theirs}, not ${read.graders}\n`);
    bad = true;
  }
  used.add(key);
  return [...row, read.verdict];
});
const header = ["kind", "arm", "case", "run", "error", "task", "comply", "cite", "fired", "cost_usd", "turns",
  "adjudicated"];
writeFileSync(`${out}/sessions.tsv`, [header, ...rows].map((r) => r.join("\t") + "\n").join(""));
for (const key of byHand.keys()) {
  if (!used.has(key)) {
    writeSync(2, `score.mjs: no session matches ${key}\n`);
    bad = true;
  }
}
if (bad) process.exit(1);

const modelOf = (result) => text(present(result.suite?.modelOverride) ? result.suite.modelOverride : "");
const model = modelOf(results.get(pluginJson));
for (const f of [proseJson, hookJson]) {
  if (f === "") continue;
  const otherModel = modelOf(results.get(f));
  if (model !== otherModel) stop(1, `score.mjs: the stages ran on different models, ${model} and ${otherModel}`);
}
const version = text(results.get(pluginJson).claudeVersion);
const runLengths = results.get(pluginJson).cases.flatMap((c) => Object.values(c.arms).map((runs) => runs.length));
const runs = runLengths.length === 0 ? "null" : String(Math.max(...runLengths));
// The cost to the cent, rounded half away from zero.
const costTotal = given.reduce((sum, f) => sum + (results.get(f).costUsd ?? 0), 0) * 100;
const cost = Math.sign(costTotal) * Math.round(Math.abs(costTotal)) / 100;

const excludedCases = excluded.join(" ").split(/\s+/).filter((w) => w !== "").length;
const adjudicated = rows.filter((r) => r[11] !== "").length;

// A number to two decimal places as C's printf writes it, so an exact tie goes to the even digit.
function fixed2(x) {
  const [whole, fraction] = Math.abs(x).toFixed(100).split(".");
  let cents = BigInt(whole + fraction.slice(0, 2));
  const rest = fraction.slice(2).replace(/0+$/, "");
  if (rest > "5" || (rest === "5" && cents % 2n === 1n)) cents += 1n;
  const digits = cents.toString().padStart(3, "0");
  return `${x < 0 || Object.is(x, -0) ? "-" : ""}${digits.slice(0, -2)}.${digits.slice(-2)}`;
}

const kinds = ["house", "control"];
const arms = ["plugin", "hook", "prose", "none"];
const tally = () => ({ sessions: 0, errors: 0, graded: 0, done: 0, complied: 0, gradersDone: 0, gradersComplied: 0,
  overturned: 0, cited: 0, fired: 0 });
const count = {};
for (const k of kinds.flatMap((kind) => arms.map((arm) => `${kind}.${arm}`))) count[k] = tally();
const cases = new Set();
let total = 0;
let misplaced = "";
for (const [kind, arm, kase, , error, task, comply, cite, fired, , , byHandVerdict] of rows) {
  const k = `${kind}.${arm}`;
  const c = (count[k] ??= tally());
  cases.add(kase);
  if (kind !== "house" && kind !== "control") misplaced = kase;
  c.sessions++;
  total++;
  if (error === "1") {
    c.errors++;
    continue;
  }
  c.graded++;
  if (task !== "0") {
    c.gradersDone++;
    if (comply === "1") c.gradersComplied++;
  }
  let done = task !== "0";
  let complies = comply === "1";
  if (byHandVerdict !== "") {
    done = byHandVerdict !== "no task";
    complies = byHandVerdict === "pass";
    if (byHandVerdict !== verdictOf(task, comply)) c.overturned++;
  }
  if (done) {
    c.done++;
    if (complies) c.complied++;
  }
  if (cite === "1") c.cited++;
  if (fired === "1") c.fired++;
}
if (misplaced !== "") {
  writeFileSync(`${out}/aggregate.md`, "");
  stop(1, `score.mjs: case ${misplaced} is in neither house/ nor control/`);
}

const rate = (n, d) => (d === 0 ? "" : fixed2(n / d));
const lead = (a, b) => (a === "" || b === "" ? "" : fixed2(Number(a) - Number(b)));
const table = ["| Field | Value |", "|-------|-------|"];
const row = (field, value) => table.push(`| \`${field}\` | ${value} |`);
row("trial", "`plugin`");
row("method", method);
row("model", `\`${model}\``);
row("claude_version", version);
row("cases", cases.size);
row("excluded_cases", excludedCases);
row("runs_per_case", runs);
row("sessions", total);
row("adjudicated", adjudicated);
for (const kind of kinds) {
  const compliance = {};
  const gradersCompliance = {};
  for (const arm of arms) {
    const k = `${kind}.${arm}`;
    const c = count[k];
    row(`${k}.sessions`, c.sessions);
    row(`${k}.errors`, c.errors);
    row(`${k}.task_done`, c.done);
    row(`${k}.complied`, c.complied);
    compliance[arm] = rate(c.complied, c.done);
    row(`${k}.compliance_rate`, compliance[arm]);
    row(`${k}.graders_task_done`, c.gradersDone);
    row(`${k}.graders_complied`, c.gradersComplied);
    gradersCompliance[arm] = rate(c.gradersComplied, c.gradersDone);
    row(`${k}.graders_compliance_rate`, gradersCompliance[arm]);
    row(`${k}.overturned`, c.overturned);
    row(`${k}.cited`, c.cited);
    row(`${k}.citation_rate`, rate(c.cited, c.graded));
  }
  row(`${kind}.plugin.fired`, count[`${kind}.plugin`].fired);
  row(`${kind}.hook.fired`, count[`${kind}.hook`].fired);
  row(`${kind}.lead_over_prose`, lead(compliance.plugin, compliance.prose));
  row(`${kind}.lead_over_none`, lead(compliance.plugin, compliance.none));
  row(`${kind}.graders_lead_over_prose`, lead(gradersCompliance.plugin, gradersCompliance.prose));
  row(`${kind}.graders_lead_over_none`, lead(gradersCompliance.plugin, gradersCompliance.none));
  row(`${kind}.hook_lead_over_plugin`, lead(compliance.hook, compliance.plugin));
  row(`${kind}.hook_lead_over_prose`, lead(compliance.hook, compliance.prose));
  row(`${kind}.hook_lead_over_none`, lead(compliance.hook, compliance.none));
  row(`${kind}.graders_hook_lead_over_plugin`, lead(gradersCompliance.hook, gradersCompliance.plugin));
}
row("cost_usd", fixed2(cost));
row("cost_per_session_usd", total === 0 ? "" : fixed2(cost / total));
writeFileSync(`${out}/aggregate.md`, table.map((line) => line + "\n").join(""));

// A path on the machine that ran the trial says nothing to a reader, and may name its user.
const strip = (v) => {
  if (typeof v === "string") return /^(\/|[A-Za-z]:[\\/])/.test(v) ? v.split(/[/\\]/).at(-1) : v;
  if (Array.isArray(v)) return v.map(strip);
  if (v !== null && typeof v === "object") return Object.fromEntries(Object.entries(v).map(([k, e]) => [k, strip(e)]));
  return v;
};
const stripped = (f) => JSON.stringify(strip(results.get(f)), null, 2).replaceAll("\u007f", "\\u007f") + "\n";
writeFileSync(`${out}/results/plugin-stage.json`, stripped(pluginJson));
if (proseJson !== "") writeFileSync(`${out}/results/prose-stage.json`, stripped(proseJson));
if (hookJson !== "") writeFileSync(`${out}/results/hook-stage.json`, stripped(hookJson));

writeSync(1, `scored ${rows.length} session(s) into ${out}\n`);
