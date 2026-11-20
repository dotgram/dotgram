# DotGram.QueryLanguage

A closed query language for text a host does not trust: a query names only the entities and the
fields the host exposes, each only where the host allows it, and is translated into an
`IQueryable` that the host's own LINQ provider runs.

```text
from orders
where status in @statuses && total >= @min
select id, total - total / 6 as net, status, created
order by created desc, id
skip @skip take 20
```

The package is built and tested with every change and is not yet published. This page is
written out in full with the rest of its documentation.
