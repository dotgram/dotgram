Not released yet. The first version is published with the milestone that brings associations between entities; until then the package is built and tested with every change.

Added: the head of a query may be written `from o in orders` as well as `from orders o` and `from orders as o`, and `orderby`, `ascending` and `descending` are read as `order by`, `asc` and `desc`. An alias written without `as` can no longer be `in`, `join`, `left`, `on` or `orderby`.

Added: `*` is an item of a `select` list (`select 1 as one, *, total`): the columns of the row as it stands, with the `let`s; a column written by name takes the place of the field of that name. A query returns the row as the last clause leaves it, as `select *` gives it: the `let`s made after the last `select` (or any, with none) are returned, and no longer reported as unused. `select * select id` no longer reports every field the `*` spread as unused.

Changed: the JSON of `Describe(text).ToJson()` opens with `"status":"started"` and closes with `"result":"completed"`, as the document of the rows does; it opened with `"status":"ok"` and had no `result`. The refusal document is unchanged.
