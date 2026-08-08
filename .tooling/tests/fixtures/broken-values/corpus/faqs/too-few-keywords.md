---
id: faq-0001
tier: normative
status: active
symptom-keywords: [timeout, upload]
confirmed-by: alex.doe
confirmed-on: "2026-06-12"
review-by: "2026-12-31"
owner: alex.doe
---

# Too few keywords

`FAQ: faq-0001` `ACTIVE`

## Symptom

Covering `min-items`. `symptom-keywords` declares `min-items: 3` and this carries two, which is the one thing about a
list that neither `list` nor `field-pattern` asks: both read the entries that are there and neither counts them.

## Cause

The floor is reported against the field rather than against an entry, because no entry is at fault — what is wrong is
how few of them there are.

## Fix

A third keyword. The two present are well-formed, so nothing else here fires and the golden is one finding.
