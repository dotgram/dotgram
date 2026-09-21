# How validation is asked for: ours from the code, QuickFIX/n from the assembly (2026-09-20)

Igor asked what the validation API of other FIX libraries looks like, because a consumer arriving
from one of them brings habits. This document is that review, and it is **unfinished**: two columns
are filled and the rest are not.

What is here was read rather than recalled. Ours comes from the code, in both of the shapes it
currently has. QuickFIX/n comes from the assembly installed on this machine — which is a better
source than its page, because a page describes what was intended and an assembly describes what is
there. The libraries on other platforms are missing because this session has no network, and §4
says so rather than filling them from memory — and that list is short by count rather than by
guess: of the 737 packages in this machine's cache, the only third-party FIX ones are
`quickfixn.core` and `quickfixn.fix44` (enumerated by the FIX owner). There is no OnixS, no Fix8,
nothing else to read offline. **Every claim names what it was read from**: a source
file and revision, an assembly and its version, a licence file, or the shipped XML documentation.

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

## 3. Their licence and their dictionary, read from the files

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

## 4. What is missing, and why it is missing rather than guessed

The other columns — where the verb lives, what comes back, whether it stops, and what a finding
carries — are **not written**, for QuickFIX/J, Artio, Philadelphia, QuickFIX (C++), Fix8, hffix,
simplefix, pyfixmsg, quickfix-go, fefix, FIX Antenna .NET Core, Geh.Fix, or the commercial engines.

The reason is that this session cannot reach the network: `WebFetch` returns
`connect ECONNREFUSED 10.0.0.1:443`, `curl` returns status 000, and four parallel readings failed
with connection refused. The rule for this review is that a claim about someone else's code comes
from their source with the version named. Writing the columns from memory would produce a document
whose every line carried "unverified", which is honest and useless at once — and the error would
land exactly on what looks common knowledge: the review from 2026-09-19 found by reading that the
.NET port of QuickFIX handles DATA fields differently from the C++ original, which nobody would
have guessed.

The questionnaire is written and was given to four readings; it survives the network outage and is
recorded here so the work resumes without being re-derived:

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

The question the FIX owner asked to have answered first was **is there any library that returns all
findings at once?** — and for the one library that could be read, the answer is no, twice over:
QuickFIX/n throws on the first problem, and its finding carries neither the position nor the entry
index of a repeating group. So on the evidence so far, two of our four decisions are differences a
consumer will meet rather than expectations they arrive with, and the documentation has to say so
instead of assuming it is obvious. Whether that survives the other twelve libraries is exactly what
the missing columns are for: one library is a data point, not a pattern.
