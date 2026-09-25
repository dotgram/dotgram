# The FIX Repository, as its authors publish it

`FIX.4.2/Base`, `FIX.4.4/Base`, `FIX.5.0SP2/Base`, `FIXT.1.1/Base` and `schema` are copied byte for
byte from the **FIX Unified Repository**,
`fix_repository_2010_edition_20200402`, downloaded from
<https://www.fixtrading.org/standards/fix-repository/>. This is the specification's own
machine-readable form — not a dictionary written by an implementor, which is what
`../Fix/FIX44.xml` is.

What FIX 4.4 comes to here: **93 messages** (`Messages.xml`), **912 fields** with their types
(`Fields.xml`), **106 components** (`Components.xml`), **3,420 membership rows** — a message or
component, a tag or a component name, a position and whether it is required (`MsgContents.xml`) —
plus the code sets (`Enums.xml`), the primitive types (`Datatypes.xml`), the abbreviations, the
categories and the sections. `schema` holds the XSDs those files declare themselves against.

The record is one element per row and says what it is, which is the point of preferring it to a
dictionary: a message names its `ComponentID`, its `MsgType` and its `Name`, and its membership is
rows in a separate file keyed by that id.

FIX 5.0 SP2 travels over FIXT 1.1: the header, the trailer and the seven session messages are
FIXT's, the application messages SP2's, so the two are read together. The four versions were
taken from one download of the archive; its `FIX.4.4` is byte for byte the one kept here since
September, which is how the others are known to be the same edition.

## What was left out

The archive also carries FIX 4.0, 4.1, 4.3, 5.0 and 5.0 SP1, the `Unified` phrase files (15 MB of
English text) and the XSL used to publish the documents. The package reads the versions kept
here; the rest is a download away and does not need to live here to be got.

## Its terms, which are not the ones the website states

The download page says the current repository is under the Apache License 2.0; its license page,
read on 2026-09-25, says so of this archive too: "the FIX Unified Repository ... was initially
published under the proprietary license (FIX Repository License Agreement) ... Going forward,
when downloading the current format, which includes the FIX Unified Repository, as well as the
Orchestra format the Apache License, Version 2.0 applies." **Every file in this archive says
otherwise in its own header**: `Copyright 2003-2009 FIX Protocol Limited, all
rights reserved`, followed by a grant to reproduce the specification in its entirety provided the
copyright statement is retained, and to extract or cite portions in other documents provided the
origin is referenced and the specification itself is named as `Copyright FIX Protocol Limited`.

The two statements are not the same permission, and the disagreement is recorded here rather than
resolved: a page describes a policy, a file carries the terms it was published under. The headers
are intact in every file, which is what both readings ask for. **Whether generated code derived
from these tables carries an attribution line, and what it says, is not settled here.**

## It reaches no package

Like the SQL corpus and the dictionary beside it, this is material for development. What this
repository ships carries no third party's tables.
