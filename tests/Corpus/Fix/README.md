# Somebody else's dictionary

`FIX44.xml` is the FIX 4.4 data dictionary of **QuickFIX/n**, copied byte for byte from
its `QuickFIXn.FIX44` package, version 1.14.1, where it sits at `DataDictionary/FIX44.xml`.
It describes the protocol as data: 916 fields declared with their types and value sets,
24 components declared, 93 messages with what each requires, and 59 group names used in
226 places. The larger figures a naive count gives — 3,783 and 451 — are *references* to
fields and components from inside messages and components, not declarations, and the
difference matters: this format declares a group at the place it is used, so one name
carries a different membership in different messages, and a reader that keys a group by
its name merges two shapes into one.

`FIX42.xml` is the same project's FIX 4.2 dictionary, from the same release: its source tree at the
tag `v1.14.1`, `spec/fix/FIX42.xml`, from which `FIX44.xml` comes out byte for byte the same as the
package's copy. Its fields are named as the FIX 4.2 repository names them; four messages are not
(`IndicationofInterest`, `NewOrderSingle`, `NewOrderList`, `AllocationACK`), and those are the names
`DotGram.Finance.Fix.Fix42` gives them. Unlike `FIX44.xml`, it loads whole over the standard
(`Fix42Tests`).

`FIXT11.xml` and `FIX50SP2.xml` are the same release's session layer and FIX 5.0 SP2, from the same
tree. They describe a later edition than the repository this package is written from, with
messages and members the repository does not have (`PartyDetailsListRequest`, a `Username` on
`UserNotification`); read as one over the standard they are refused at the first such place, and
`FIXT11.xml` alone loads (`Fix50Tests`). What they place differently from the repository is one
component: their `UsernameGrp` is the field `Username` alone, where the repository's is the group
`NoUsernames` (809) of `Username`s, and their file does not declare 809. `quickfixn-fix50sp2-errata.xml`
says it the repository's way; merged over theirs, with the two later message types removed in code,
the three apply (`Fix50Tests`). One field is named otherwise than the repository names it,
`HaltReasonInt` for tag 327, and that is the name `DotGram.Finance.Fix.Fix50` gives it.

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

## The errata

`quickfixn-fix44-errata.xml` is ours, not theirs: the thirteen message types their file places
differently from the FIX 4.4 repository, each written whole the way the repository has it, in
their format. Loaded after their file — `Fix44Context.Default.Load([FIX44.xml, errata])` —
it puts those types back to the standard, so that a schema read from their file holds a
message to the protocol and not to their reading of it. Written by `quickfixn-fix44-errata.py` beside it from
`../FixRepository`, never edited by hand; the twenty-seven places it answers are the
`Unplaced` lines that loading their file reports (`FixLoadTests`).
