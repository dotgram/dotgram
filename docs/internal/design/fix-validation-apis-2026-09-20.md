# How validation is asked for, and what none of them carries (2026-09-20)

Igor asked what the validation API of other FIX libraries looks like, because a consumer arriving
from one of them brings habits. This document is that review: **ten other libraries read from their
source or their shipped assembly**, across .NET, the JVM, C++, Python, Go and Rust.

**The answer, and it is the same everywhere.**

- **Not one of them returns more than one finding.** Every library that validates at all throws or
  returns on the first problem. The single partial exception in ten is Artio's on-the-fly path,
  which reports every *missing required* tag at completion while stopping at the first of everything
  else.
- **Not one finding carries a position in the source.** The best any of them does is a bare tag.
- **Not one finding carries the entry index of a repeating group** — and this is the part worth
  reading twice, because in three of them the code *has* the index when it reports. quickfix-go
  walks entries with the index in hand as `groupCount` and reports the tag alone. Artio accumulates
  every unknown and every missing field in hash sets and hands back one tag, taken from a hash set,
  not even in wire order. fefix makes `entry_index` part of a field's very identity — and has no
  finding to attach it to.
- **Three of the ten do no validation whatsoever**, and two say so in their own documentation.
  pyfixmsg is the instructive one: it loads a full spec with types and enum values and uses it only
  for grouping and field order.

So both decisions this package made on that axis — all findings at once, and a finding that names
which entry of which group — are differences a consumer will **meet**, not habits they arrive with.
That has to be said in the documentation rather than assumed obvious. And the strongest argument for
the first is not ours: it is that two libraries had already collected the whole answer internally
and published a door one finding wide.

What is here was read rather than recalled. Ours comes from the code, in both of the shapes it
currently has. QuickFIX/n comes from the assembly installed on this machine — a better source than
its page, because a page describes what was intended and an assembly describes what is there. The
rest come from their source at named tags or commits. What is still missing is named in §6 rather
than filled from memory. **Every claim names what it was read from**: a source file and revision, an
assembly and its version, a licence file, or the shipped XML documentation.

Two documents already cover ground this one does not repeat: `fix-libraries-2026-09-19.md` (what
exists on .NET, what may sit in a benchmark, and the QuickFIX/n licence) and
`fix-dictionaries-2026-09-19.md` (what a validation dictionary holds). This one is about the shape
of the *call*, across platforms.

## 1. Ours, and the fact that there are two of them

There is no single answer to "what does our validation look like", and that is the first finding.
The shipped package and the branch answer differently, and they answer **opposite** on the question
that matters most — whether you get one problem or all of them.

### 1a. What ships today — `origin/main` at `3919d383` (2026-09-20 18:12)

```csharp
// FixMessages.cs
public static bool Validate(FixMessage message, FixParseMode mode, FixParseOptions? options, out FixParseError? error)
```

- **Where the verb lives:** a static method on `FixMessages`, taking the message. Not a method on
  the message. Validation is also folded into parsing: `FixMessages.TryParse(..., FixParseMode.Strict)`
  runs the same checks while building.
- **What comes back:** `bool`, and **one** error through an `out` parameter.
- **Does it stop:** yes, at the first. `FixValidation.Validate` is a chain of
  `if (!Scope(...)) return false;` — header, trailer, body — and each `Scope` returns on its first
  refusal.
- **What the finding carries** (`FixParseError`, `FixModel.cs`): `Position` (zero-based character
  offset), `Tag` (`int?`), `MessageType` (`string?`), `Reason` (a sentence). There is no rule
  identifier, no scope, and nothing about repeating groups: a finding inside the ninth `Parties`
  entry is indistinguishable from one in the first.

### 1b. What is being built — `codex/finance` at `644f91ea` (2026-09-20 20:16)

*(Read at the owner's branch head, not at `main`. An area with an owner is read on their branch:
their work is committed and simply not pushed, so `main` is honestly current and honestly wrong
for the question. This section first quoted `cda1a41f` because that was the head when it was
written two hours earlier; the shape below is unchanged between the two, but the revision is
named rather than assumed.)*

```csharp
// FixModel.cs
public FixFinding[] Validate();
public FixFinding[] Validate(FixValidator validator);

// FixFinding.cs
public readonly record struct FixFinding(
    FixRule  Rule,        // enum, eleven rules: UnknownMessageType, RequiredFieldMissing, …
    FixScope Scope,       // Header | Body | Trailer
    int?     Tag,         // null where the finding is about a component or the message
    int      GroupTag,    // the group's counter tag, 0 outside a group
    int      EntryIndex,  // which entry of that group, -1 outside a group
    int      Position,    // where the field begins in the source
    string   Reason);

// FixValidator.cs
public delegate void FixMessageRule(FixMessage message, List<FixFinding> findings);
public static FixValidator Standard { get; }          // the compiled-in schema, shared, not written to
public void Load(FixDictionary dictionary);            // a counterparty's dictionary
public void Load(IReadOnlyDictionary<string, FixMessageRule> rules);
public FixMessageRule this[string messageType] { get; set; }
static readonly FixFinding[] Nothing = [];             // a right message allocates nothing
```

- **Where the verb lives:** on the message. The validator is a **parameter**, not the receiver —
  a dictionary is loaded once and asked many times, so it cannot be an argument built per call, and
  `Standard` is shared rather than a mutable static anyone can write to.
- **What comes back:** `FixFinding[]`, empty for a right message, and the empty one is shared.
- **Does it stop:** no. Every rule is asked and every finding is returned. The reason is written
  beside the type: parsing stops at the first error because after it the input's meaning is
  unknown, but a built message is fully known, so each rule can be asked independently.
- **What the finding carries:** rule as an **enum** rather than a string, the scope, the tag, and —
  the part no other column has yet been checked for — the group's counter tag **and the entry
  index**. "Tag 448 is wrong" says nothing in a message carrying nine parties.

Two things stand beside the validator at this revision and belong in the comparison: `FixDictionary`
— a counterparty's dictionary as data a validator loads — and `DotGram.Finance.Generator`, an
analyzer that reads a QuickFIX dictionary and compiles it into code. So the same two roads QuickFIX/n
carries in one package are both here too, and the line between them is the one the dictionary study
drew: what is *read* and what is *checked* may be run-time tables, what is *constructed* stays build
time.

**All four of the design claims the FIX owner described are in the code as described**, which is worth
stating plainly because this review was given them in advance precisely so that it would check them
rather than repeat them. What was not in the account, and matters for the review, is that they are
in a branch: on `main` the shape is still 1a, and `fix-validation-layer-2026-09-20.md` — the
proposal — opens with "No code is changed". So a consumer of the shipped package today meets the
`bool` + first-error form, and the review's comparison must say which of our two shapes it compares.

## 2. QuickFIX/n, read from the installed assembly

The package is on this machine, so this column is answered without network — and from a better
source than a page, because **a page describes the intention and an assembly describes what is
there**. Everything below was read by reflecting over
`P:\.packages\.nuget\packages\quickfixn.core\1.14.1\lib\net10.0\QuickFix.dll` (assembly version
1.14.1.0), types reflected over and never constructed. Where a line comes from the licence file or
the shipped XML documentation instead, it says so.

**Where the verb lives — a static service, and a second one on the message.**

```csharp
// QuickFix.DataDictionary.DataDictionary
static void Validate(Message message, DataDictionary transportDataDict, DataDictionary appDataDict,
                     string beginString, string msgType);

// QuickFix.Message : FieldMap
void Validate();
void FromString(string msgstr, bool validate, DataDictionary transportDict, DataDictionary appDict,
                IMessageFactory msgFactory, bool ignoreBody);
```

The schema check is a **static method taking the message and two dictionaries** — not a method on
the message, and not an instance the dictionary owns. The message's own `Validate()` is a different
check (framing: BodyLength and CheckSum), and `FromString`'s `validate` flag runs it during parsing.
So there are three readings, and two of the three are the caller's to combine.

**What comes back: nothing.** Both `Validate` overloads return `void`. A finding is therefore an
**exception**, and the first one ends the check — there is no shape in which a second could be
reported. The same is true of the dictionary's own checks, which are the rules one at a time:
`CheckHasRequired`, `CheckIsInGroup`, `CheckGroupCount`, `CheckValidFormat`, `CheckValue`,
`CheckHasNoRepeatedTags`, `CheckMsgType`, `CheckValidTagNumber` — every one of them `void`.

**What a finding carries**, from the exception types:

| | carries |
| --- | --- |
| `TagException : QuickFIXException` | `int Field`, `SessionRejectReason sessionRejectReason` |
| `FieldNotFoundException` | `int Field` |
| `MissingRequiredFieldException` | the tag, through a constructor; no public member |
| `GroupDelimiterTagException : TagException` | counter tag and delimiter tag |
| `RepeatedTagWithoutGroupDelimiterTagException : TagException` | counter tag and the offending tag |

So: the **rule** is the exception's type, plus `SessionRejectReason` where there is one — an
enumeration, as ours is. The **tag** is there. **Position in the source is not.** **Scope —
header, body or trailer — is not.** And the **entry index of a repeating group is not**: two
exception types name a group's counter tag, which is as close as it comes, and neither says which
entry. That is the question the FIX owner asked to have answered first, and for this library the
answer is no.

**The packaging answers a question about the model, and it is the one thing here nobody would
think to look for.** `quickfixn.fix44` 1.14.1 ships `DataDictionary/FIX44.xml` — 340,702 bytes —
**beside** `QuickFix.FIX44.dll`: generated typed classes and a run-time dictionary, in one package.
They do not choose between the two roads; they carry both. (Found by the FIX owner while enumerating
the cache for other libraries, and worth more than the enumeration itself.) Our own corpus copy at
`tests/Corpus/Fix/FIX44.xml` is byte for byte the same file, sha `a8111ec5…`, identical in 1.14.0
and 1.14.1 — so the dictionary did not change between those versions, which makes "byte for byte
from 1.14.1" a checked statement rather than a copied one.

**Model**, confirmed from the assembly rather than restated: `QuickFix.Message : FieldMap`, which
is what the 2026-09-19 review found by reading the source — storage keyed by tag, so wire order and
repeated tags do not survive. Dictionary construction is `new DataDictionary(path)` or
`(Stream)`, and its `AllowUnknownMessageFields`, `CheckFieldsOutOfOrder`, `CheckFieldsHaveValues`,
`CheckUserDefinedFields`, `AllowUnknownEnumValues` are settable properties — the strictness is
configuration on a long-lived object, which is the one place its shape and ours agree.

**Not in the shipped XML documentation.** `QuickFix.xml` ships beside the assembly and carries
2,098 documented members, and `DataDictionary.Validate` is not among them: the only documented
`Validate` in the file is an unrelated `AsciiValidator.Validate(System.String)`. A consumer reading
the documentation does not meet the validation entry point at all.

## 3. The JVM: QuickFIX/J, Artio, Philadelphia

Read from source at named tags — QuickFIX/J at `QFJ_RELEASE_3_0_2`, Artio at `0.184`, Philadelphia
at `2.0.0` — and not from their documentation.

**QuickFIX/J — the verb is on the dictionary, and it throws.**

```java
// quickfixj-base/src/main/java/quickfix/DataDictionary.java
public        void validate(Message message, ValidationSettings settings)
        throws IncorrectTagValue, FieldNotFound, IncorrectDataFormat;
public static void validate(Message, DataDictionary session, DataDictionary application,
                            ValidationSettings);
```

First problem only: `validate` walks the fields and throws out of the first failing check, with no
accumulation anywhere. The finding is a `FieldException` carrying exactly two things — the tag and
an **int** session-reject-reason constant (not an enum) — plus a sentence built from them. No
offset, no path, no entry index. A second, narrower channel exists: parsing defers one error into a
single `FieldException` field, which `hasValidStructure()` exposes and `validate` re-throws — still
exactly one. The dictionary is long-lived and loaded from XML; the seven strictness booleans are a
`ValidationSettings` passed per call.

**Artio — a boolean and two ints, and the all-errors information exists but is not reachable.**

```java
// artio-codecs/.../builder/Decoder.java
boolean validate();      // never throws
int     invalidTagId();  // NO_ERROR == -1
int     rejectReason();  // int, deliberately not an enum
```

This is the finding that matters most for us. The generated decoder **internally accumulates**
`IntHashSet unknownFields` and `IntHashSet missingRequiredFields` — it knows every problem — and
`validate()` surfaces `iterator.nextValue()`: one tag, taken from a hash set, so not even in wire
order. The whole answer is in the object and has no public accessor. Nothing carries an offset, a
path or an entry index; group validation counts entries but copies up only the tag and the reason.

Artio's on-the-fly path is **the one exception found anywhere to "first problem only"**:
`OtfValidator.onComplete()` computes the difference between required and present fields and calls
`onError` for **every** missing tag, while every other check stops at the first. Its
`ValidationError` is a real enum, of five coarse members. The group entry index is available to the
acceptor during parsing (`onGroupBegin(tag, numInGroup, index)`) and is *not* carried on `onError`.

**Philadelphia — no validation API at all.** No dictionary, no validator, no `validate` anywhere in
the core. The parser checks framing only, and a message that fails it is labelled garbled and
**silently skipped**: no callback, no error object, no exception. That is a real answer rather than
a gap: a library can decide that validation is the application's business and say so by having none.

**And Philadelphia is the one library here whose model is ours.** `FIXMessage` is two parallel
arrays, `int[] tags` and `FIXValue[] values`, read positionally — so **wire order survives and
repeated tags survive**, which is true of no other library read so far and is exactly what
`FixParser.Parse` returns. It has no repeating-group model at all; a group is a flat run the
application interprets. QuickFIX/J is the opposite: a `TreeMap` keyed by tag, where order is lost
and a repeated tag is refused outright.

**Licences**: Artio and Philadelphia are plain Apache-2.0. QuickFIX/J carries the same
QuickFIX Software License as the .NET port, advertising clause and all.

## 4. C++, Python, Go and Rust

Read from source at named commits: QuickFIX C++ at `386ce46e`, quickfix-go at `2600e522`, hffix at
`797ce7db`, simplefix at `007e3d7f`, pyfixmsg at `b7f1bfbd`, fefix at tag `v0.7.0`.

**QuickFIX C++ — the dictionary again, throwing again.** `static void validate(const Message&,
const DataDictionary* session, const DataDictionary* app)`, a straight-line sequence of throwing
checks with no accumulation. The finding is the **exception class** — `RequiredTagMissing`,
`IncorrectDataFormat`, `RepeatedTag`, `RepeatingGroupCountMismatch` and their siblings — carrying a
type string, a detail string and the tag. No offset, no entry index.

Two things here that the .NET port does not have. First, **DATA fields are general and
dictionary-driven**: `XMLTypeToType` maps `type="DATA"` to `TYPE::Data`, `isDataField` consults the
set, and `Message::extractField` takes the value by length for any of them — where QuickFIX/n
special-cases `XmlData` alone. Second, and less flattering: the pairing of a data field to its
length field is the hardcoded convention `tag − 1` (with `Signature` → `SignatureLength` as the one
exception), not something the dictionary states. Worth knowing before we treat "reads DATA from the
dictionary" as one thing.

A coverage gap found by reading rather than by using: `iterate()` walks the flat field map, and
groups live in a separate structure, so per-field format and enum checks **do not descend into
repeating-group entries**. Only the required-field check recurses, and its finding carries the tag
alone.

**quickfix-go — a separate validator object, and one error.**

```go
type Validator interface { Validate(*Message) MessageRejectError }
func NewValidator(settings ValidatorSettings, appDD, transportDD *datadictionary.DataDictionary) Validator
```

Long-lived, built once from config. Returns a single `MessageRejectError` — an interface carrying a
reject-reason int, a sentence, an optional `*Tag`, and a business/session flag. Every level is
`if err != nil { return err }`; nothing is collected.

And here is the same shape as Artio's, from the other end: `validateVisitGroupField` walks group
entries with the index in hand as `groupCount`, and when a required field is missing inside entry N
it reports `RequiredTagMissing(childTag)` — **N is in scope and is not put in the error**.

**Three of the six do no validation at all**, and two say so themselves. simplefix's parser
docstring states it verbatim: it does not check fields, presence, types or enumerations. hffix has
`is_valid()`, which means only that the framing parses — and it is a flag rather than an exception
precisely so the reader can resynchronise. Both keep wire order and repeated tags exactly, as
Philadelphia does and as we do.

**pyfixmsg is the instructive one: the material without the verb.** It loads a QuickFIX-style XML
spec into `FixSpec` with types, enum values by name and by value, components, groups and a field
order — and uses all of it for exactly two things: splitting repeating groups while decoding, and
re-ordering tags when serialising. Nothing ever consults `tagtype` or the enum values to check a
message. Its model also loses most of what a check would need: a `dict` keyed by tag, so wire order
is not represented and a duplicate tag outside a group collapses.

**fefix carries an entry index — as an address, not as a finding.** A field is located by
`FieldLocator { tag, context }`, where the context is `TopLevel` or
`WithinGroup { index_of_group_tag, entry_index }`. It is **the only library of the eleven read here
that treats "which entry of which group" as part of a field's identity** — and it has no findings to
attach it to: decoding returns a four-variant `DecodeError` that carries no tag at all, and type
checking happens per field, at the call site, when you ask for a value. Its DATA handling is the
cleanest found: the decoder keys off the preceding field's dictionary datatype being `Length`, with
no tag−1 assumption and no special cases.

**Licences**: QuickFIX C++ and quickfix-go carry the QuickFIX Software License, with the same
clause-3 acknowledgment and clause 4-5 name restrictions as the other two QuickFIX ports. hffix is
BSD-2, simplefix MIT, pyfixmsg Apache-2.0, fefix MIT-or-Apache-2.0 — all clean. (fefix bundles FIX
Repository data under separate terms, not read.)

## 5. Their licence and their dictionary, read from the files

**QuickFIX/n's licence, read from the file** — `P:\.packages\.nuget\packages\quickfixn.core\1.14.1\LICENSE`,
not from a summary: "The QuickFIX Software License, Version 1.0", copyright 2001-2010
quickfixengine.org, five conditions, with the Apache-1.1-style **advertising clause intact** as
condition 3 (end-user documentation included with a redistribution must carry the acknowledgment,
or it may appear in the software itself). Conditions 4 and 5 forbid using the names to endorse
derived products and forbid calling a derived product "QuickFIX". This confirms from the file what
`fix-libraries-2026-09-19.md` concluded; nothing here is new, and the point of repeating it is that
the cached package now makes it verifiable without network.

`quickfixn.fix44` 1.14.0/1.14.1 carries `DataDictionary/FIX44.xml` inside the package, so a
dictionary to read with a tool is available locally. **It is not copied into this repository**, per
Igor's rule that no third-party file enters what we ship.

## 6. What is still missing, and the questionnaire that found the rest

The questionnaire below is what every column above answers. It is recorded because the next
library is read against the same three questions, and because the first attempt at this review was
made with no network at all — the columns were left empty rather than filled from memory, on the
ground that a document whose every line carries "unverified" is honest and useless at once. The
2026-09-19 review is the reason that rule is not pedantry: it found by reading that the .NET port
of QuickFIX handles DATA fields differently from the C++ original, which nobody would have guessed
and which this review has now confirmed from both sides.

1. **Model** — what parsing turns the wire into (field map keyed by tag, flat list, buffer index
   with lazy accessors, generated typed classes); whether **wire order** survives; whether a
   **repeated tag** survives or is collapsed; how repeating groups are represented; whether parsing
   and validation are one pass or two.
2. **Validation API** — where the verb lives (message, dictionary, separate service, flag on
   parse); what it returns (bool, first error, list, or a throw); whether it **stops at the first**;
   what a finding carries (tag, position, path, **group entry index**, rule as enum or string,
   sentence); whether the same check runs both during and after parsing; whether the validator is
   long-lived or built per call.
3. **Licence** — read from the LICENSE file: what may be read with a tool, what may not be kept in
   this repository, what may not be quoted in documentation.

**What is left unread**, so that the ten do not read as "everything": the commercial engines — OnixS
on .NET and C++, B2BITS FIX Antenna, RA — whose source is not public and which this review does not
guess about; EPAM's Apache-2.0 FIX Antenna .NET Core and Geh.Fix, which have public source and were
not reached; and anything on a platform not named here. Ten libraries agreeing is a strong pattern
and not a proof, and the commercial engines are exactly where a different answer would most likely
live, since they are sold to people who reconcile breaks for a living.

**One thing to carry beyond validation**, found while answering these questions and belonging to
whoever writes our DATA handling: the four libraries that read length-prefixed fields do it four
different ways. QuickFIX C++ takes the set of DATA fields from the dictionary but pairs each to its
length field by the hardcoded convention `tag − 1` (with `Signature` the one exception). QuickFIX/n
special-cases `XmlData` and has no general reading at all. hffix and simplefix carry hardcoded pair
tables — and hffix's own comment says why it dare not assume the convention. fefix is the only one
that does it from the data: it keys off the preceding field's dictionary datatype being `Length`.
quickfix-go does not handle DATA at parse time at all, so a value containing the separator
mis-parses.
