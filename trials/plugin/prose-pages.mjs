#!/usr/bin/env node
// Gives the prose arm's plugin its text as SessionStart hooks, one a page, so no text goes on a command line.
//
//   prose-pages.mjs <prose file> <prose plugin dir>
//
// Writes <prose plugin dir>/pages/page-<n>.md and a hooks/hooks.json with one command printing each page. A page
// starts at a heading of a type or a record, and only a record longer than a page is cut inside it.
import { mkdirSync, readFileSync, writeFileSync, writeSync } from "node:fs";

// Prints a message to stderr and exits. The write is synchronous, so the exit cannot cut the message off.
function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

if (process.argv.length !== 4) stop(2, "usage: prose-pages.mjs <prose file> <prose plugin dir>");
const [proseFile, plugin] = process.argv.slice(2);

// Claude Code caps a hook's output at 10,000 characters. The page header takes the rest.
const room = 9900;

const text = readFileSync(proseFile, "utf8").replaceAll("\r\n", "\n");
if (text === "") stop(1, `prose-pages.mjs: ${proseFile} is empty`);

// The text as blocks, each starting at a heading of level 1 to 3, every line keeping its newline.
const blocks = [];
for (const line of text.match(/[^\n]*\n|[^\n]+$/g)) {
  if (blocks.length === 0 || /^#{1,3} /.test(line)) blocks.push(line);
  else blocks[blocks.length - 1] += line;
}

// A block too long for a page becomes its lines, and a line too long for a page becomes pieces of one.
const pieces = blocks.flatMap((block) => {
  if (block.length <= room) return [block];
  return block.match(/[^\n]*\n|[^\n]+$/g).flatMap((line) => line.match(new RegExp(`[^]{1,${room}}`, "g")));
});

const bodies = [];
for (const piece of pieces) {
  if (bodies.length > 0 && bodies[bodies.length - 1].length + piece.length <= room) bodies[bodies.length - 1] += piece;
  else bodies.push(piece);
}

mkdirSync(`${plugin}/pages`, { recursive: true });
mkdirSync(`${plugin}/hooks`, { recursive: true });
bodies.forEach((body, i) => {
  writeFileSync(`${plugin}/pages/page-${i + 1}.md`, `Page ${i + 1} of ${bodies.length}.\n\n${body}`);
});
const hooks = bodies.map((_, i) => ({ type: "command",
  command: `cat "\${CLAUDE_PLUGIN_ROOT}"/pages/page-${i + 1}.md` }));
const config = { hooks: { SessionStart: [{ matcher: "startup|resume|clear|compact", hooks }] } };
writeFileSync(`${plugin}/hooks/hooks.json`, JSON.stringify(config, null, 2) + "\n");
console.log(`the prose arm's text is ${bodies.length} page(s), one hook each`);
