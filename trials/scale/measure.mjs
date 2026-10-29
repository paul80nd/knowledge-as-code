#!/usr/bin/env node
// Times `kac validate`, `export` and `bundle` in a corpus, and measures what export and bundle wrote.
//
//   measure.mjs <kac> <corpus dir> [runs]
//
// <kac> is the tool `dotnet build -c Release -o <dir>` wrote, as <dir>/kac, so no build is timed. Each command runs
// [runs] times, 3 by default, in the order validate, export, bundle, and the median is reported. The corpus must
// already be restored. Prints a Markdown table of the timings and sizes.
import { spawnSync } from "node:child_process";
import { readdirSync, readFileSync, statSync, writeSync } from "node:fs";
import { join, resolve } from "node:path";

function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

if (process.argv.length < 4 || process.argv.length > 5) stop(2, "usage: measure.mjs <kac> <corpus dir> [runs]");
const kac = resolve(process.argv[2]);
const corpus = resolve(process.argv[3]);
const runs = Number(process.argv[4] ?? 3);

// Runs one kac command in the corpus and returns the seconds it took. A non-zero exit stops the measurement.
function time(command) {
  const start = process.hrtime.bigint();
  const r = spawnSync(kac, [command], { cwd: corpus, encoding: "utf8" });
  const seconds = Number(process.hrtime.bigint() - start) / 1e9;
  if (r.status !== 0) stop(1, `measure.mjs: kac ${command} exited ${r.status}\n${r.stdout}${r.stderr}`);
  return seconds;
}

const median = (xs) => [...xs].sort((a, b) => a - b)[Math.floor(xs.length / 2)];

const times = { validate: [], export: [], bundle: [] };
for (let i = 0; i < runs; i++) for (const c of Object.keys(times)) times[c].push(time(c));

// Every file below a folder.
function files(dir) {
  return readdirSync(dir, { withFileTypes: true }).flatMap((e) =>
    e.isDirectory() ? files(join(dir, e.name)) : [join(dir, e.name)]);
}
const bytes = (dir) => files(dir).reduce((sum, f) => sum + statSync(f).size, 0);
const chars = (f) => [...readFileSync(f, "utf8")].length;

const exportDir = join(corpus, ".dist/export");
const manifest = JSON.parse(readFileSync(join(exportDir, "manifest.json"), "utf8"));
const recordCount = files(exportDir).filter((f) => f.endsWith(".json") && !f.endsWith("manifest.json")).length;
const jsonl = files(exportDir).filter((f) => f.endsWith(".jsonl"));
const lines = jsonl.reduce((sum, f) => sum + readFileSync(f, "utf8").split("\n").filter((l) => l.trim()).length, 0);
const index = join(corpus, ".dist/plugin/views/build-index.md");

const s = (xs) => `${median(xs).toFixed(2)} s (${xs.map((x) => x.toFixed(2)).join(", ")})`;
const rows = [
  ["`kac validate`, median of " + runs, s(times.validate)],
  ["`kac export`, median of " + runs, s(times.export)],
  ["`kac bundle`, median of " + runs, s(times.bundle)],
  ["Records exported", `${recordCount}, across ${manifest.types.length} types`],
  ["Lines in the `.jsonl` files", String(lines)],
  ["Export size", `${bytes(exportDir)} bytes`],
  ["Plugin size", `${bytes(join(corpus, ".dist/plugin"))} bytes`],
  ["Build index", `${chars(index)} characters`],
];
console.log("| Measure | Value |\n|---|--:|");
for (const [k, v] of rows) console.log(`| ${k} | ${v} |`);
