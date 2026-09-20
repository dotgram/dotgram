# Somebody else's dictionary

`FIX44.xml` is the FIX 4.4 data dictionary of **QuickFIX/n**, copied byte for byte from
its `QuickFIXn.FIX44` package, version 1.14.1, where it sits at `DataDictionary/FIX44.xml`.
It describes the protocol as data: 3,783 fields with their types and value sets, 451
components, and 93 messages with what each requires and which of its tags begin a
repeating group.

Licensed under the QuickFIX Software License 1.0, BSD-three-clause in shape; the notice,
the conditions and the disclaimer are kept beside the file they cover, in `LICENSE`. The
licence asks that the end-user documentation of a redistribution acknowledge it, so here
it is: this product includes software developed by quickfixengine.org
(<http://www.quickfixengine.org/>). The names are not used to endorse anything, and
nothing here is called QuickFIX.

**It reaches no package.** It is material for development and for tests, like the SQL
corpus beside it. What this repository ships carries no dictionary but its own: a
consumer points us at the file their counterparty gave them, and we never redistribute
it. That obligation moves to the consumer the moment a dictionary is an input to their
build, and the pages describing that feature say so.

## Why it is here

Two questions it answers that a dictionary written here could not.

**What a real one contains.** Ours is a schema compiled into the package from the
specification. A counterparty's is a file, written by somebody else, with their own
fields above 5000, their own value sets, and their own idea of what a message requires.
Reading this one is how the reader we build is held to something it did not shape.

**Where the two disagree.** Its tables and ours describe the same protocol version, so a
walk over both says where our schema and a published dictionary differ — which is the
staleness guard a consumer gets at start-up when a counterparty issues a newer file.

A second dictionary, one that a venue actually issues with its own extensions, is what
this one cannot be: it is the standard protocol and nothing more. The first venue file we
are given is worth keeping beside it, if its licence lets us.
