# Trials

This folder contains repeatable experiments on the documentation in this repository. Each trial keeps its questions,
its method and every run, so you can run it again after a large change and compare the result with the last run.

| Trial                                    | What it measures                                                             |
|------------------------------------------|------------------------------------------------------------------------------|
| [Comprehension](comprehension/README.md) | Whether each section of the documentation site lets its reader act           |
| [Capture](capture/README.md)             | Whether an adopting team's knowledge is getting written down in its corpus   |
| [Plugin](plugin/README.md)               | Whether a corpus's plugin makes an agent's code follow the corpus's rules    |
| [Process level](process-level/README.md) | Whether a process written as outcomes draws fewer findings asking for a step |

## Adding a trial

Add a trial when you will judge a change by evidence that a later session can gather again. Give it its own folder,
with a README that states:

* what it measures, and for which reader
* how to stage and run it, word for word, so a second run is comparable with the first
* how each run is scored, and what a later run must show to pass

Write each run to a `runs/` folder in the trial, named by its date. Keep what each reader returned beside the run, so
a later session can check the scoring.

Add the trial to the table above.
