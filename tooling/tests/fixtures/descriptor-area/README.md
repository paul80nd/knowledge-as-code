A corpus stating an area path on a tracker that has none.

Azure DevOps divides one project's backlog into area paths. No other target has them, so `area` beside
either of the two below names nothing the client that files can set.

* **`tracker:` states an area and no target at all.** The target comes from `publishing-target: github`,
  and the message names it. Reading the stated key alone would pass this corpus, so the check asks the
  effective target. A corpus publishing to Azure Repos and filing on its project states the area and no
  target, and that is the ordinary case the check has to let through.
* **`framework:` states an area beside `target: none`.** A tracker that files nowhere has nowhere to put
  one. The two blocks are asked the same question, and `framework:` is never derived.

Both findings land against `.corpus.yaml`. What is wrong is the declaration, and the file holding it is
the one a corpus owner edits.

`TrackerTests` holds the values that pass, and holds the drop the exporter makes where one does not.
