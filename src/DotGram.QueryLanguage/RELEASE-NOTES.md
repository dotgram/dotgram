Not released yet. The first version is published with the milestone that brings associations between entities; until then the package is built and tested with every change.

Added: the head of a query may be written `from o in orders` as well as `from orders o` and `from orders as o`, and `orderby`, `ascending` and `descending` are read as `order by`, `asc` and `desc`. An alias written without `as` can no longer be `in`, `join`, `left` or `on`.

Changed: the JSON of `Describe(text).ToJson()` opens with `"status":"started"` and closes with `"result":"completed"`, as the document of the rows does; it opened with `"status":"ok"` and had no `result`. The refusal document is unchanged.
