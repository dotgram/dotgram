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

**Read Q9 in `open-questions.md` beside this.** It was written the same evening from the other
direction and finds what this document does not: Artio's *flyweight* decoders, which decode
nothing until a field is asked for, and staffix reporting under a byte allocated per message —
and from those, that our own `IdealFixParser` floor allocates one `FixField` a field like
everything else we have, so the stand cannot see that choice at all, because the floor was built
to the same design as the thing it is a floor for. That is a finding about our yardstick and this
document is about theirs; neither replaces the other. Q9 leaves QuickFIX/n's licence to be read
before it is taken into the benchmarks, which is §3 here.

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
| validation while parsing | none; strict checks are the message layer's | with `validate: true`: header field order, then BodyLength and CheckSum. Dictionary validation is a separate `DataDictionary.Validate` | none at the raw entry point |
| session layer | none | included, separable — parsing does not depend on it | included, separable |

The row that matters most is the third. **QuickFIX/n parsed without a dictionary does less work
than we do**, because it does not find the groups. In `Message.FromString` the guard is
`if (msgMap is not null && msgMap.IsGroup(f.Tag))`, and `msgMap` comes from the application
dictionary. (Everything quoted from their source here was read from `QuickFIXn.Core` at 1.14.1,
`connamara/quickfixn` on 2026-09-19. The names will outlive their next release; the line numbers
would not, so there are none.) And `validate: true`
inside `FromString` is not dictionary validation: it checks that the first three header fields are
in order and then calls the instance `Validate()`, which checks BodyLength and CheckSum — the
framing, not the schema. The schema check is a separate static `DataDictionary.Validate` the
caller makes. So there are three readings, not one, and each does a different amount of work:
without a dictionary, with one, and with one plus the explicit validation.

*(This paragraph said "the first three header fields and nothing else" until the readings were
built and a hand-made message was refused for its BodyLength. Reading their file told me what
`validate` is tested against; only running it showed what it then calls. Both are needed.)*

### What a field map keyed by its tag cannot keep

`QuickFix.Message` derives from `FieldMap`, whose storage is a `SortedDictionary<int, IField>`.
Two consequences follow from the key being the tag, and both were read off a running comparison
rather than argued from the type:

**The wire's order is gone.** On a `NewOrderSingle` both sides accept, we return the tags as the
wire had them and they return them sorted within each scope:

```
ours    8 9 35 49 56 34 52 | 11 21 55 54 60 38 40 44 59 | 10
theirs  8 9 34 35 49 52 56 | 11 21 38 40 44 54 55 59 60 | 10
```

**A repeated tag is one entry.** Sixteen fields of tag 58 come back from us as sixteen fields and
from them as **one**: the second entry replaces the first. Without a dictionary that is also what
becomes of every member of a repeating group, which is the ordinary case in FIX rather than a
corner of it.

The second is why there can be no field-level row against this reference. Their reading of such an
input does a fraction of the work — a sixteenth, on that input — and a number cannot tell "faster"
from "read less". The caveat would not travel with the ratio, and the size of the difference
depends on the input, so even a careful reader could not correct for it in their head. The error
is in the flattering direction, and nobody goes looking behind a flattering number.

### The rest of what differs, from the same comparison

- **Where we recover, they throw.** A field with a malformed tag comes back from us as one
  `FixField.Invalid` with reading resumed after the next separator; `FromString` raises
  `FormatException` and there is no message.
- **A data field is not a concept there** — see below — so an input carrying one is refused
  outright rather than read differently.
- **Framing is required.** They need `8=`/`9=`/`10=` around anything; we read a bare run of
  fields, which is what a log line or a fragment is.

Of the five FIX inputs the stand already carries, exactly **one** passes a field-by-field
agreement check between the two sides, and it is the one with a single field in it. That is the
whole argument for checking agreement before timing rather than after: measuring first would have
produced five numbers, four of them comparing different work.

### The one difference that is not about speed

**QuickFIX/n has no general reading of data fields.** The .NET port special-cases exactly one
tag:

```csharp
StringField f = fieldTag == Tags.XmlData
    ? ExtractDataField(msgstr, Header.GetInt(Tags.XmlDataLen), ref pos)
    : ExtractField(msgstr, ref pos);
```

There is no `IsDataField` and no set of data fields in its `DataDictionary`. The C++ original and
QuickFIX/J both do this generally and from the dictionary — `type='DATA'` is what tells them to
take a value's length from the preceding field — so this is a difference between the ports and
not a property of QuickFIX.

The consequence is not that one side is slower. It is that **on a message carrying `RawData`,
`SecureData`, `EncodedText` or `Signature`, the two sides read different things**: we take the
value's length from its length field, and they scan for the next separator, which a binary
payload may contain. Neither number is then measuring the other's work, and the stand's own check
— that both sides answer the same on the same input — would fail, correctly.

This belongs in the account rather than in a table, and it is stated as what it is: a difference
in what is read, established by reading their file, not a verdict on their library.

It also settles a question that was open here for a day. D25's first wording would have removed
`FixFieldOptions` — a consumer declaring length/data pairs of their own — as the parser being
told how to read. But the most widely used FIX engine on .NET has no general reading of data
fields **at all**, for the standard's sixteen pairs let alone a counterparty's. So that capability
is not an untidiness a stricter rule would have swept away; on this platform it is the only way
anyone has it. Igor's second wording, which kept it, was the right way round. (critic's, from
reading the same file independently.)

## 5. What I propose to give the stand

Following the rule the stand adopted for the Web libraries this week: another library's reader is
a **reference**, not a yardstick. It goes in the plain stand only, its reading is named for what
it is, and the ratio cell prints `reference` so that no ratio exists to misread.

- Readings named `reference-QuickFIXn` and, if we take it, `reference-FixAntenna` — never `hand`
  and never a bare library name that invites a ratio.
- **Two pairings, each naming itself in the row**, because a reader who is told only
  "QuickFIX/n" cannot tell which of the three readings they are looking at, and a row travels
  further than the document around it:
  - against `FixMessages` — `FromString(msgstr, transportDict, appDict, validate: true)` followed
    by an explicit `DataDictionary.Validate`. Only in this form do their groups get assembled, so
    only in this form is the comparison honest. It makes `QuickFIXn.FIX44` a requirement rather
    than an option — and that package **ships `DataDictionary/FIX44.xml` inside itself**, so the
    dictionary arrives with the reference and no third-party file enters this repository. Both
    forms were built and run before this was written; the dictionary one accepts a correctly
    framed `NewOrderSingle` and its `DataDictionary.Validate` returns without throwing. Note the
    package version: `QuickFIXn.Core` is 1.14.1 and `QuickFIXn.FIX44` is **1.14.0** — 1.13.0 does
    not exist for it, and asking for 1.13.0 silently resolves 1.14.0 with an NU1603 warning.
  - **against `FixParser.Parse` — not taken.** It was in this document's first draft and the
    agreement check killed it: their field map sorts by tag and collapses repeats, so the row
    would compare a reading of sixteen fields with a reading of one. See "What a field map keyed
    by its tag cannot keep" above.
- FIX Antenna, if taken: `RawFixUtil.GetFixMessage(byte[])`.
- **The input is built for the row rather than borrowed from the existing ones.** None of the
  five the stand carries will do: `Order` differs by field order, `OrderMalformed` by recovery
  against an exception, `BinaryMany` is `RawData` and they refuse it, `slope-16` collapses
  sixteen fields into one, and `One` is a single field with no framing. What both sides accept is
  a correctly framed `NewOrderSingle` with no repeated tag outside a group and no data field —
  built with its BodyLength and CheckSum computed rather than written, since a hand-made frame is
  refused by their `Validate()` and the failure looks like a disagreement about parsing when it is
  arithmetic.
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

- I argued here against a message-layer row until the two validators had been held side by side,
  and the decision went the other way: D26 takes it, with the explicit `DataDictionary.Validate`.
  That is the right call — the alternative, pairing their dictionary-less form against
  `FixMessages`, is the regex comparison turned in *our* favour, which is worse than the ordinary
  kind because we would not notice. What survives of the objection is not a reason to refuse the
  row but something the row must carry: the two strict modes check overlapping and unequal lists
  (we check group field order and length/data adjacency in both directions; they check
  out-of-order header fields and a group count against its entries), so the number is two
  validators, not one validator twice. The companion dictionary document has the two lists.
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
