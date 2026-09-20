# FIX libraries for .NET, and what may honestly be put beside ours (2026-09-19)

Igor asked what else reads FIX on .NET, whether QuickFIX/n can sit in the benchmarks, and what
each of them does. Nothing here is a decision, and nothing here is a number: the measuring is the
stand's, and this document says what it should be asked to measure and what it must not be asked
to compare. Facts about this package were read off the tree at `caaee0c9`; facts about other
libraries carry their source, and the two licences the conclusion rests on were read from the
repositories themselves rather than from anybody's summary.

## 1. What we are, so that "the same work" has a meaning

The comparison is worthless without this, so it comes first. This package does two separable
things, and no other library divides them where we do.

- **`FixParser.Parse`** takes the wire and returns `FixField[]` — one **typed** object a field,
  already converted: `FixField.Price` holds a decimal, `RefSeqNum` a `BigInteger`, `PossResend` a
  bool. A tag nobody knows comes back as `FixField.Unknown` carrying the octets. It consults the
  schema for one thing only — whether a tag begins a length/data pair — and otherwise reads to
  the separator.
- **`FixMessages.Build`** takes those fields and returns a message: header, body and trailer as
  scopes, repeating groups as entries, and, in the strict mode, the whole of §3 below checked.

There is **no session layer at all**. There is no connection, no logon, no sequence numbers, no
resend, no store. `FixField.SessionRejectReason` and its neighbours are field classes for session
*tags*; nothing in the package speaks to a counterparty.

The dictionary is not a file. `FixSchema.cs` is 8,279 lines of compiled C# — types per tag, about
384 code sets, components, groups and their counters, requiredness, message composition, the
sixteen length/data pairs — and `Fix/Specification/` is empty, so nothing is read at build time
either. It is fixed to FIX 4.4 and kept in step by hand. That is the subject of the companion
document on dictionaries; here it matters only because it is why our parse asks almost nothing.

## 2. The candidates

Three libraries can go into a public benchmark: obtainable from NuGet, licence permitting, and
built by anyone who clones the repository. The rest cannot, and saying so is part of the answer.

### Can be put beside us

**QuickFIX/n** — `connamara/quickfixn`, the .NET port of the C++ QuickFIX. Alive: v1.14.1
published to NuGet 2026-06-05, commits through July 2026, fifteen open issues being triaged,
v1.15 in progress. Packages are `QuickFIXn.Core` plus a package per FIX version
(`QuickFIXn.FIX44`, …; note the 1.14 rename that dropped the period from `QuickFIX.FIX4.4`).
Beware the stale third-party forks on NuGet — `QuickFix.Net.NETCore`, `sylr.QuickFIXn.*`,
`ArtexFIX.Core` — which are not it.

**FIX Antenna .NET Core** — `epam/fix-antenna-net-core`, `Epam.FixAntenna.NetCore`, **Apache-2.0**
(read from the repository's LICENSE). The open-source sibling of the commercial B2BITS line;
byte-buffer oriented, with a session layer, validation and dictionaries, but with a genuinely
dictionary-free entry point. Pushed 2026-07-09, though releases are rare — 1.2.3 is dated
2025-02-19.

**Geh.Fix** — `GaryHughes/FixClient`, Apache-2.0, the library behind a Windows FIX testing tool.
Small (a few thousand downloads) and built around FIX 5.0SP2. A third data point if we want one;
I would not add it first.

### Cannot

- **OnixS .NET FIX Engine** — commercial, and decisive on its own terms: it *requires a licence
  key file at run time* and throws without one, and there is nothing on nuget.org. A benchmark
  nobody else can run is not a benchmark.
- **B2BITS FIX Antenna .NET** (the commercial one, distinct from the Apache-2.0 repository above)
  — purchase or trial request, `engine.license` file.
- **RA FIX Engine** (Rapid Addition) — no published licence or price, "request a demo" only.
- **Fix8** — C++; the open-source Community Edition has no .NET binding, only the commercial
  Fix8Pro does. The `fix8.dev` NuGet packages are native and were last published in 2019.
- **VersaFix** — dead since 2012, and its repository carries no LICENSE file at all, so its
  licence cannot be established from the source. Twice disqualified.
- **CoralFIX** — Java. No .NET product was found.

A sweep of GitHub by stars and by `topic:fix-protocol language:C#` turned up no modern .NET-first
FIX engine beyond those three. The current new work in this space is Rust and C++.

## 3. The licences, read rather than summarised

This is the part Igor asked to settle before anything is added, and it is the reason the answer
is "benchmarks only, with the terms written down".

**QuickFIX/n is not under an OSI licence.** It is the "QuickFIX Software License, Version 1.0",
derived from Apache 1.1, with the advertising clause intact. I read the LICENSE file in the
repository; GitHub itself classifies it as `NOASSERTION`. Its five conditions, in substance:

1. source redistributions keep the notice, the conditions and the disclaimer;
2. binary redistributions reproduce them in the documentation or other materials supplied with
   the distribution;
3. **end-user documentation included with a redistribution must carry the exact string** "This
   product includes software developed by quickfixengine.org (http://www.quickfixengine.org/)" —
   or the acknowledgment may appear in the software itself;
4. the names "QuickFIX" and "quickfixengine.org" may not be used to endorse or promote derived
   products without written permission;
5. a derived product may not be called "QuickFIX".

Every one of 1–3 is conditioned on **redistribution**. `DotGram.Finance.Benchmarks` is
`IsPackable=false`, ships in no package, and is built from source by whoever clones the
repository, so a `PackageReference` to QuickFIX/n redistributes nothing and triggers none of
them. What would trigger clause 2 is publishing built artifacts that contain its DLLs — a release
zip, a container image.

Conditions 4 and 5 are about *derived products*. Naming QuickFIX/n as the measured subject in a
comparison table is not endorsing or promoting a derived product on the face of the text. It is,
however, the clause with the least modern wording, so the table should name it plainly as a
subject of measurement and claim nothing about it.

Recommendation: add `benchmarks/DotGram.Finance.Benchmarks/THIRD-PARTY-NOTICES.md` carrying the
full licence text and the clause-3 acknowledgment, whether or not we ever ship an artifact. It
costs nothing, it records the terms as Igor asked, and it removes the question before someone has
to ask it under time pressure. **Nothing goes into a shipped package**: this is the notice
problem we have just finished removing from the packages, and it stays out.

FIX Antenna .NET Core and Geh.Fix are plain Apache-2.0 and raise none of this.

## 4. How their model differs from ours

The difference is not a detail; it decides what may be compared.

| | this package | QuickFIX/n | FIX Antenna .NET Core |
| --- | --- | --- | --- |
| what a parse returns | `FixField[]`, one typed object a field, values converted | `Message : FieldMap` over `SortedDictionary<int, IField>`, values as strings | an index into the byte buffer; values fetched on demand |
| typed messages | `FixMessages.Build`, a second step | generated classes, only if an `IMessageFactory` is supplied, from a separate package | separate |
| dictionary needed to parse | no — only the length/data pairs, from compiled tables | no — and **without one, repeating groups are not recognised at all** | no — group indexing is a separate opt-in call |
| validation while parsing | none; strict checks are the message layer's | header field order only; full validation is a separate `DataDictionary.Validate` | none at the raw entry point |
| session layer | none | included, separable — parsing does not depend on it | included, separable |

The row that matters most is the third. **QuickFIX/n parsed without a dictionary does less work
than we do**, because it does not find the groups; parsed with one, it does more, because it also
validates on the way. Neither setting is "the same work as ours", and a single number against
either would be a claim we cannot support.

## 5. What I propose to give the stand

Following the rule the stand adopted for the Web libraries this week: another library's reader is
a **reference**, not a yardstick. It goes in the plain stand only, its reading is named for what
it is, and the ratio cell prints `reference` so that no ratio exists to misread.

- Readings named `reference-QuickFIXn` and, if we take it, `reference-FixAntenna` — never `hand`
  and never a bare library name that invites a ratio.
- Rows: the existing `fix/One.text` and `fix/One.bytes` first. Our reading stays `generated`
  against `hand` and `ideal`; the reference sits beside them with no ratio.
- The entry points, so that both sides are asked one thing:
  - ours `FixParser.Parse(string)` / `Parse(byte[])`;
  - QuickFIX/n `new QuickFix.Message(raw, validate: false)`, no dictionary, no factory;
  - FIX Antenna `RawFixUtil.GetFixMessage(byte[])`.
- **The check each row must carry**, and this is mine to write before the stand times anything:
  both sides answer the same thing on the same input. For a reference that returns strings and
  ours that returns typed values, "the same" is the tag sequence and the field text — so the row
  compares tag-by-tag over the input's own fields, and a message with a repeating group is either
  excluded from the reference row or the row says in words that the reference did not group it.
- **What must be said in words beside the table**, not hidden in a number: QuickFIX/n's parse
  produces strings and ours produces converted values, so part of any gap is conversion we do and
  it defers; QuickFIX/n without a dictionary does not read groups; QuickFIX/n's session layer,
  which we do not have at all, is not in the row and is not in the number either.

## 6. What I would not do

- I would not put a message-layer row against QuickFIX/n's `DataDictionary.Validate` yet. Our
  strict mode and its validation overlap but are not the same set of checks, and until the
  companion dictionary document has settled what each actually verifies, a row would be comparing
  two different lists of rules.
- I would not add Geh.Fix in the first pass. Its per-message entry point allocates a
  `MemoryStream` and a `Reader`, and it consults a built-in FIX 5.0SP2 field table, so it is
  neither dictionary-free nor allocation-comparable; it would need its own paragraph of caveats
  to earn a row, and two references are enough to start.
- I would not quote any vendor's own latency figures anywhere, for the engines we cannot run or
  the ones we can.

## 7. Open, for Igor and the architect

1. Do we take one reference (QuickFIX/n, the one everybody knows) or two (adding FIX Antenna,
   which is Apache-2.0 and has the cleanest byte entry point of the three)?
2. Is `THIRD-PARTY-NOTICES.md` under `benchmarks/` the right home for the QuickFIX terms, given
   that the packages must stay clear of them?
3. The benchmarks project would gain its first `PackageReference` to a third-party library that
   is not a test or measurement tool. If that is unwelcome in itself, the alternative is a
   separate project outside the solution, as `DotGram.PackageSmoke` already is.
