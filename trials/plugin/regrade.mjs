#!/usr/bin/env node
// Re-applies the graders in a case folder to the workspaces and traces one stage's `--keep-temp` run kept.
//
//   regrade.mjs <cases dir> <aggregate-result.json> <out.json>
//
// Writes <out.json> in the shape of the input, with each case's grader definitions and each run's verdicts replaced.
// It reports, for every grader whose name and definition are unchanged, whether it agrees with the tool's verdict,
// and exits 1 on any disagreement. Open each kept folder with `chmod 700 <folder> <folder>/sealed` first.
import { readFileSync, readdirSync, writeFileSync, existsSync, statSync } from "node:fs";
import { dirname, join } from "node:path";
import { isDeepStrictEqual } from "node:util";

if (process.argv.length !== 5) {
  console.error("usage: regrade.mjs <cases dir> <aggregate-result.json> <out.json>");
  process.exit(2);
}
const [casesDir, resultPath, outPath] = process.argv.slice(2);

// The frontmatter graders use: plain and single-quoted scalars, and one level of inline mapping.
function scalar(text) {
  const t = text.trim();
  if (t.startsWith("'")) {
    if (!t.endsWith("'") || t.length < 2) throw new Error(`unterminated quoted value: ${t}`);
    return t.slice(1, -1).replaceAll("''", "'");
  }
  if (t.startsWith('"')) return JSON.parse(t);
  if (/^-?\d+$/.test(t)) return Number(t);
  return t;
}

function parseGrader(file) {
  const text = readFileSync(file, "utf8");
  const m = text.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  if (!m) throw new Error(`${file}: no frontmatter`);
  const fields = {};
  for (const line of m[1].split(/\r?\n/)) {
    if (line.trim() === "") continue;
    const kv = line.match(/^([a-z_]+):[ \t]*(.*)$/);
    if (!kv) throw new Error(`${file}: cannot read line: ${line}`);
    const [, key, raw] = kv;
    if (raw.startsWith("{")) {
      if (!raw.endsWith("}")) throw new Error(`${file}: unterminated mapping: ${raw}`);
      fields[key] = Object.fromEntries(raw.slice(1, -1).split(",").map((pair) => {
        const i = pair.indexOf(":");
        return [pair.slice(0, i).trim(), scalar(pair.slice(i + 1))];
      }));
    } else {
      fields[key] = scalar(raw);
    }
  }
  return fields;
}

// The config the tool records for a grader, in the tool's key order.
function config(fields) {
  if (fields.type === "regex") {
    return {
      target: fields.target ?? "last_message",
      pattern: fields.pattern,
      flags: fields.flags ?? "",
      match: fields.match ?? "contains",
    };
  }
  if (fields.type === "tool_used") {
    const c = { tool: fields.tool };
    if (fields.input_match !== undefined) c.input_match = fields.input_match;
    if (fields.min !== undefined) c.min = fields.min;
    if (fields.max !== undefined) c.max = fields.max;
    return c;
  }
  throw new Error(`grader type ${fields.type} is not one this script applies`);
}

function graders(caseDir) {
  const dir = join(caseDir, "graders");
  return readdirSync(dir).filter((f) => f.endsWith(".md")).sort().map((f) => {
    const fields = parseGrader(join(dir, f));
    return { name: f.slice(0, -3), type: fields.type, weight: fields.weight ?? 1, config: config(fields) };
  });
}

function readTrace(path) {
  const calls = new Map();
  let result = null;
  let lastText = null;
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.trim() === "") continue;
    const event = JSON.parse(line);
    if (event.type === "result") result = event.result;
    if (event.type !== "assistant") continue;
    const texts = [];
    for (const block of event.message?.content ?? []) {
      if (block.type === "tool_use") calls.set(block.id, block);
      if (block.type === "text") texts.push(block.text);
    }
    if (texts.length > 0) lastText = texts.join("\n");
  }
  return { calls: [...calls.values()], lastMessage: result ?? lastText ?? "" };
}

function grade(grader, workspace, trace) {
  const c = grader.config;
  if (grader.type === "regex") {
    let text;
    let where;
    if (c.target === "last_message") {
      text = trace.lastMessage;
      where = "last_message";
    } else {
      const file = join(workspace, c.target.path);
      if (!existsSync(file) || !statSync(file).isFile()) {
        return { passed: false, explanation: `path "${c.target.path}" does not exist` };
      }
      text = readFileSync(file, "utf8");
      where = `file ${c.target.path}`;
    }
    const found = new RegExp(c.pattern, c.flags).test(text);
    if (c.match === "not_contains") {
      return { passed: !found, explanation: found ? "pattern found (expected absent)" : "pattern absent as expected" };
    }
    return { passed: found, explanation: found ? `matched ${c.pattern}` : `pattern not found in ${where}` };
  }
  const re = c.input_match === undefined ? null : new RegExp(c.input_match);
  const n = trace.calls.filter((call) => call.name === c.tool && (!re || re.test(JSON.stringify(call.input)))).length;
  const min = c.min ?? 1;
  const max = c.max ?? Infinity;
  const passed = n >= min && n <= max;
  return { passed, explanation: `${c.tool} called ${n}x (expected ${min}..${max === Infinity ? "∞" : max})` };
}

// Under with-without, the tool marks a `tool_used` grader on `Skill` with-only: it shows the plugin fired.
const isWithOnly = (g) => g.type === "tool_used" && g.config.tool === "Skill";

const result = JSON.parse(readFileSync(resultPath, "utf8"));
const withWithout = result.suite?.ablation === "with-without";
let agreed = 0;
const disagreements = [];
let changed = 0;

for (const kase of result.cases) {
  const kind = kase.dir.split(/[/\\]/).at(-2);
  const caseDir = join(casesDir, kind, kase.name);
  if (!existsSync(caseDir)) throw new Error(`no case folder ${caseDir}`);
  const before = new Map(kase.graders.map((g) => [g.name, g]));
  const after = graders(caseDir);
  const unchanged = new Set(after.filter((g) => isDeepStrictEqual(before.get(g.name)?.config, g.config)
    && before.get(g.name)?.type === g.type).map((g) => g.name));
  changed += after.length - unchanged.size;
  kase.graders = after;
  for (const [arm, runs] of Object.entries(kase.arms)) {
    for (const run of runs) {
      const workspace = join(dirname(dirname(run.tracePath)), "sealed", "home", "cwd");
      if (!existsSync(workspace)) throw new Error(`cannot read ${workspace}: run chmod 700 on the kept folder`);
      const trace = readTrace(run.tracePath);
      const old = new Map(run.graders.map((v) => [v.name, v]));
      // The tool runs no with-only grader in a baseline arm.
      const applied = after.filter((g) => !(withWithout && arm === "without" && isWithOnly(g)));
      run.graders = applied.map((g) => {
        const verdict = grade(g, workspace, trace);
        const prior = old.get(g.name);
        if (unchanged.has(g.name) && prior) {
          if (prior.passed === verdict.passed) agreed++;
          else disagreements.push(`${kase.name} [${arm}] ${g.name}: tool ${prior.passed}, here ${verdict.passed}`);
        }
        const withOnly = withWithout && isWithOnly(g);
        return { name: g.name, passed: verdict.passed, weight: g.weight, explanation: verdict.explanation,
          withOnly, scored: !withOnly };
      });
    }
  }
}

result.regraded = { by: "trials/plugin/regrade.mjs", at: new Date().toISOString() };
writeFileSync(outPath, JSON.stringify(result, null, 2) + "\n");
console.log(`regraded ${result.cases.length} case(s): ${agreed} unchanged verdict(s) agree with the tool, `
  + `${disagreements.length} disagree; ${changed} grader definition(s) are new or changed`);
for (const d of disagreements) console.log(`  disagrees: ${d}`);
process.exit(disagreements.length === 0 ? 0 : 1);
