---
id: dsc-comment-moves-to-a-new-method
type: discovery
tier: observed
status: open
source: session
confidence: unverified
expires: "2026-12-07"
provenance: An agent session adding a method to a C# file under tooling/, where it happened twice in one sitting.
applies-to:
  - svc-kac
promoted-to:
owner: human:paul.law
tags: [ comments, csharp, review ]
---

# A comment above a method attaches to a method inserted beneath it

`Discovery: dsc-comment-moves-to-a-new-method` `OPEN`

## What I saw

A new method landed between an existing comment and the method that comment described. The comment then read as
documentation for the new method, and the old method had none. It happened twice in one sitting.

## Context

Seen in `tooling/kac.core`, where the house style puts a plain `//` comment above a declaration. The compiler never
inspects one of those, and `Exporter.cs` is the only file here using `/// <summary>` at all.

## Why it might matter

Nothing reports it: a comment binds to whatever declaration follows it, and that is valid C#. The build stays green,
both methods look documented, and a reader trusts a comment describing the wrong code.
