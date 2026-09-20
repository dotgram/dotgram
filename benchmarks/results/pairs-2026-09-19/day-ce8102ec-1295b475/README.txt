The whole day between two commits: 1295b475 against ce8102ec, 67 commits apart (the generator's lazy tokens, the guard record built in place,
the arcs' own tables, the positional forms starting at the first token after the position, and the T-SQL grammar's alternative order).
It was taken as the pair of sql-39's InlineReturn and is not: that commit is one file, three lines, and its own pair is against its parent a93e06a8.
The -46% of sql/select20 and the -2..-6% of T-SQL here belong to the generator's commits in between, not to the grammar.
