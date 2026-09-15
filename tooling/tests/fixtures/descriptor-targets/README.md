A corpus naming two targets the tool cannot act on.

`.corpus.yaml` gets one wrong in each vocabulary, and the pass reports both at once.

* **`publishing-target: gihtub` is nobody's spelling.** A target the tool does not know publishes
  nowhere, so an export from this corpus would carry no link and say nothing about why.
* **`tracker.target: azure-devops-wiki` is a real target that files nowhere.** A wiki holds pages and
  holds no backlog, so the block addresses nothing. The message names the three values a tracker takes.

Both findings land against `.corpus.yaml`. What is wrong is the declaration, and the file holding it is
the one a corpus owner edits.

`framework.target` takes the same three values and is asked the same question. The coverage gate reads
check ids, so one fixture is what this id needs, and `DescriptorTargetTests` holds every other spelling.
