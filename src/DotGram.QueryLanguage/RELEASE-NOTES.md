Not released yet. The first version is published with the milestone that brings associations between entities; until then the package is built and tested with every change.

Changed: the JSON of `Describe(text).ToJson()` opens with `"status":"started"` and closes with `"result":"completed"`, as the document of the rows does; it opened with `"status":"ok"` and had no `result`. The refusal document is unchanged.
