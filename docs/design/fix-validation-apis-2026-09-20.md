# How validation is asked for: ours, read from the code, and theirs, not yet read (2026-09-20)

Igor asked what the validation API of other FIX libraries looks like, because a consumer arriving
from one of them brings habits. This document is that review. It is **half written**: the column it
compares against — ours — is here, read from the code rather than from anybody's account of it, and
the other columns are not, because this session has no network and their source cannot be read.
What is missing is named in §3 rather than filled from memory.

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

### 1b. What is being built — `codex/finance` at `cda1a41f` (2026-09-20 18:18)

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

**All four of the design claims finance-24 described are in the code as described**, which is worth
stating plainly because this review was given them in advance precisely so that it would check them
rather than repeat them. What was not in the account, and matters for the review, is that they are
in a branch: on `main` the shape is still 1a, and `fix-validation-layer-2026-09-20.md` — the
proposal — opens with "No code is changed". So a consumer of the shipped package today meets the
`bool` + first-error form, and the review's comparison must say which of our two shapes it compares.

## 2. What is already read of theirs

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

## 3. What is missing, and why it is missing rather than guessed

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

The question finance-24 asked to have answered first, and which this document cannot yet answer:
**is there any library that returns all findings at once?** If none does, our point 3 is a
difference that has to be named and explained in the documentation rather than assumed obvious —
and the same for the verb's place, since a consumer arriving from a library where validation is a
service brings that habit with them.
