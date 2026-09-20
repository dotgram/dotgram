# Validation dictionaries: what they hold, what we already have, and where the line falls (2026-09-19)

Igor asked for a study of FIX validation dictionaries — QuickFIX's XML and the FIX Trading
Community's Orchestra — against what this package already does, and then for a design whose first
question is stated rather than discovered: by D25 the generator decides how a parser reads, so a
dictionary that changes *reading* belongs to build time, while one that only checks an
already-built message may be run-time data. The architect asked that the line be drawn with
examples rather than in general terms. That is §5, and the answer turns out to depend on our own
architecture more than on the rule.

Facts about this package were read off the tree at `1775d57e`. Facts about the two formats and
about what venues actually publish were checked against the readers' own source and the
published files; where a claim could not be traced to a primary source it is marked so.

## 1. Two formats, and the fact that reframes the question

**The QuickFIX dictionary** is an XML file — `<fix major minor>` over `<fields>`, `<messages>`,
`<components>`, `<header>`, `<trailer>` — that QuickFIX reads to learn the tags, their types,
their enumerations, the composition of each message, and the repeating groups. It has **no DTD
and no XSD anywhere in the QuickFIX tree**: the format is whatever the readers accept, and they
differ from one another.

**Orchestra** is the FIX Trading Community's successor: a repository of the same material plus
scenarios, actors, state machines, flows, responses and a real expression language for
conditional rules. v1.0 is a ratified Technical Standard (Feb 2021); v1.1 is at RC3.

Now the fact that changes the shape of the task. **For QuickFIX, the dictionary is a parsing
input, not only a validation input.** Two independent reasons, both in `Message::setString`:

- a field of type `DATA` is extracted by taking its length from the preceding tag instead of
  scanning for the separator, and `IsDataField` consults nothing but the dictionaries;
- repeating groups are recovered only for tags the dictionary declares as groups — without one,
  a message parses into a flat list and the group boundaries are gone.

**For us, only the first of those is a parsing question, and the second is not.** `FixParser.Parse`
produces a flat `FixField[]`; groups are recovered afterwards by `FixMessages`/`FixSemantics`
from the schema. The parse asks the schema exactly one thing — whether a tag begins a length/data
pair — and `FixContext` reaches the options in exactly two places to do it. So where QuickFIX
needs a dictionary to read the wire at all, we need one datum, and everything else a dictionary
holds is consumed after the wire has already been split.

That is the whole reason §5 can draw the line where it does.

## 2. What a dictionary holds, against what we have

| What the format holds | QuickFIX XML | Orchestra | This package today |
| --- | --- | --- | --- |
| tag → name | `<field number name>` | `<field id name>` | `FixField.*` classes, one a tag |
| tag → type | `type=` from a fixed 28-name vocabulary | `<datatype>` declared, with mappings per encoding | `FixSchema.TypeCodes`, a byte a tag, 24 names |
| enumerations | `<value enum description>` inline on the field | `<codeSet>` as its own object, reusable, may be external (ISO 4217) | `FixSchema.Codes(tag)`, ~384 sets |
| components | `<component>` + reference | `<component>`, `<group>` | `FixSchema.Component(id)` |
| repeating groups | `<group name>`, counter implied by the name, **delimiter implied by the first member** | `<group>` with an explicit `<numInGroup>` child | `SchemaRef` kind 2 + `FixSchema.Counter(id)` |
| requiredness | `required='Y'|'N'` | a five-valued `presence`: required, optional, **forbidden**, **ignored**, **constant** | `SchemaRef.Required`, binary |
| message composition | `<message name msgtype msgcat>` | `<message>` per `(id, scenario)` | `FixSchema.Message(type)` |
| length/data wiring | implicit: the length tag is the data tag **minus one**, with `Signature` hard-coded as the exception | explicit `lengthId=` on the field | `FixSchema.DataTag`/`LengthTag`, sixteen pairs, listed |
| conditional rules | **none** — the docs disown them | `<rule><when>` in the Score language | none |
| scenarios | none, one shape per message type | `scenario=` on everything | none |
| workflow, actors, state | none | actors, states, transitions, flows, responses | none, and out of scope |

So of what a QuickFIX dictionary holds, **we already have every part** — as compiled C# rather
than as a file. `FixSchema.cs` is 8,279 lines and `Fix/Specification/` is empty, so nothing is
read at build time either; the tables are written and kept in step by hand, fixed to FIX 4.4.

And our strict mode already checks, from those tables: unknown message type; a field not
permitted in its scope; a duplicate field in a scope; group fields out of schema order; the
length field immediately preceding its data field and the data field immediately following its
length field, with the count matching; the value's format against the tag's type; the value's
membership in the tag's code set; required fields and required components, recursively; and the
rule that `MessageEncoding` must be present when an `Encoded*` field is.

Held against QuickFIX's own list, we check **more** in two places and **less** in none that I
found. We check group field order where QuickFIX C++ does not check body field order at all
(QuickFIX/J does check group order). We check length/data adjacency in both directions where
QuickFIX derives the length tag by the "minus one" convention and hard-codes `Signature`. What
QuickFIX has and we do not is the ability to be handed a *different* dictionary.

**One difference worth recording because it is a defect in the other direction.** QuickFIX's
requiredness silently evaporates inside an optional container: a required field within an
optional component is never reported missing, because the reader only calls `addRequiredField`
when the member and the container are both required. Ours recurses into a component only when it
is present and then demands its required members — which is the behaviour a reader expects, and
we should not copy theirs when reading their files.

## 3. What we do not have

1. **Any dictionary but FIX 4.4.** Not 4.2, not 5.0SP2, not FIXT. The tables are one version.
2. **Anyone else's dictionary.** A consumer cannot hand us a file at all.
3. **`presence` beyond required/optional.** No `forbidden`, no `ignored`, no `constant`. `ignored`
   in particular has a real use: it is how a counterparty says "we send this, do not check it".
4. **Conditional rules.** `StopPx` required when `OrdType == Stop` is not expressible.
5. **Scenarios.** One `ExecutionReport` shape, where a venue may have five.
6. **Declared datatypes.** Our type vocabulary is a closed byte code.

## 4. What a dictionary would buy that we cannot do today

This is the part Igor asked to be answered concretely, and the concrete answer is venue
extensions. Taking the one venue that publishes a real QuickFIX dictionary, Eurex's T7 FIX LF
(Deutsche Börse ships `FIXLF44_Derivatives.xml` in a public download):

- **263 distinct tags**, of which 195 are standard, 1 is in the registered user range and **67 are
  in 20000-39999** — `25005 MatchingEngineStatus`, `28585 SideLastPx`, `28905 ActivationDate` and
  so on.
- **37 message types, 13 of them venue-specific** in the `U` range: `U6`, `UBZ`, `UCA`, `UDS` …
- 27 groups and 28 components of their own.

Today all 67 of those tags come back as `FixField.Unknown` carrying octets, and the strict mode
rejects them outright, because the type table answers nothing for them and the primitive check
fails on a null type. A consumer trading Eurex therefore has to use the lenient mode and lose
every check, for the whole message, because of tags the venue publishes a description of.

The same holds, smaller, across the crypto venues, where publishing a QuickFIX dictionary is the
convention rather than the exception: **Coinbase Exchange** (a dated ZIP of five dictionaries for
market data and order entry), **Gemini** ("we maintain our custom dictionary in QuickFIX XML
form"), **Bullish** (ten non-standard tags, seven of them squatting in 8000-8006), and **Paxos**
(FIX 4.2, rendered inline on their reference page). Kraken and LMAX publish prose; Crypto.com's
documentation claims a QuickFIX dictionary and does not publish the file.

Two further facts that bear on how much this is worth:

- **The registered user range 5000-9999 is exhausted** — FIX says so in as many words — so
  anything new is bilateral in 20000-39999, which by construction no standard dictionary will
  ever hold. A package that cannot be given a dictionary can never read those tags as anything
  but unknown.
- **The low-latency venues have left tag=value entirely, and there is no longer a fallback.**
  CME's iLink 3 is SBE over FIXP and publishes an SBE schema, not a FIX dictionary; the legacy
  tag=value iLink 2 stopped accepting new sessions in October 2024 and was decommissioned in
  April 2025. So the population this feature serves is the venues that still speak tag=value —
  Eurex, the crypto exchanges, the ECNs — and not all of them publish machine-readable files:
  Cboe and LSE ship prose (HTML, PDF, Markdown).
- **No venue appears to have published an Orchestra file.** The community's own `orchestrations`
  repository holds the FIX standard repositories and one NYSE Pillar *demonstration* which its
  README says is not maintained and is not a production interface. Read that as absence of
  evidence rather than proof: the standard repositories folder was not swept exhaustively. Even
  so, Orchestra is for now a standard with no public venue adoption that anyone found.

## 5. The line, with examples

The rule says the generator decides how to read. The question is which parts of a dictionary are
"how to read". For this package the answer is unusually narrow, for the reason in §1.

**Build time — it decides how the wire is split.** Exactly one thing:

- **which tags are a length/data pair.** `95/96` is read as "a size, a separator, a tag, then that
  many raw bytes"; `25007 FreeText1` is read to the separator. Get this wrong and the parse is
  wrong, not merely unvalidated — a payload containing the separator is split inside itself.

And even this is build time only in the sense that *the shape* is: the grammar says how a pair is
read, and it was written without knowing which tag it reads, so *which* tags those are is a table
the guard indexes. Igor made that point himself. So this datum may perfectly well arrive at run
time, provided it is resolved once into a table rather than asked per field — which is what
`FixFieldOptions` is being changed to do (`docs/design/fix-field-options-2026-09-19.md`).

**Run time — it checks a message that is already built.** Everything else, and each of these is
consumed after the wire is split, from a `FixField[]` that would be identical without it:

- *a field is not permitted in this message* — `28585 SideLastPx` in a message that does not take
  it. The field was read correctly either way.
- *a required field is missing* — `55 Symbol` absent from a `NewOrderSingle`.
- *a value is not in the code set* — `54 Side` = `Z`.
- *a value does not fit its type* — `44 Price` = `abc`.
- *a group's count does not match its entries* — `453 NoPartyIDs` = 3 with two entries. Note this
  one is a check for us and a **parsing** question for QuickFIX, because they recover groups
  during the parse and we recover them afterwards.
- *a conditional rule* — `99 StopPx` required when `40 OrdType` is `3`.
- *a presence of `forbidden` or `ignored`* — a counterparty saying "never send this" or "we send
  it, do not check it".

**The boundary case, and it is the one worth arguing about: typed fields.** A consumer with the
Eurex dictionary does not only want `25005` validated; they want `FixField.MatchingEngineStatus`
with a typed value, the way every standard tag has one. That is neither reading nor checking — it
is *construction*, and it is where our real cost lives: `FixField.cs` is 6,816 lines,
`FixFieldFactory.cs` 2,041 and `FixMessageTypes.cs` 20,350, all written and kept in step by hand.
Construction cannot be run-time data without giving back the typed model that is the point of the
package. So:

> **The line: what is read and what is checked may both be run-time tables. What is
> *constructed* — a class per tag, a factory arm, a message model — is build time, because a
> type is not data.**

That is a different line from the one the rule suggested, and it is the honest one for this
package. It also matches D25 as reformulated: the generator decides what it can decide
statically, a consumer-controlled fork is legitimate, and the wish is that the fork be resolved
into a table off the hot path.

## 6. Licences: what we may read, and what we must not ship

This decides the shape of the feature more than any technical question, and it echoes what Igor
said about third-party notices in shipped packages.

- **The Orchestra specification is CC BY-ND 4.0** — NoDerivatives. It may be redistributed
  verbatim and not in modified form. The schemas and the reference implementation are Apache-2.0,
  which is the part that matters for reading files.
- **The standard Orchestra data files** carry `Copyright (c) FIX Protocol Ltd. All Rights
  Reserved.` in their own metadata, which is more restrictive than the Apache-2.0 tooling around
  them. Exactly which licence governs them I could not establish.
- **Eurex's dictionary is © Deutsche Börse AG**, distributed on their terms.
- **The community's `omi-fix-dictionaries`** — 188 protocols, transcribed from prose specs rather
  than published by the venues — has **no licence file on the repository** and file headers saying
  "Public/GPLv3". That is not a licence a shipped package can rest on.

The conclusion is the same in every case: **we ship no dictionary but our own.** A consumer points
us at the file their counterparty gave them; we never redistribute it. And a converter that reads
someone's dictionary is a tool, not a payload — which is what keeps this clear of the problem we
have just finished removing from the packages.

## 7. What I would propose, and what I would not

**Would.** Start where the value is and the risk is not: a **build-time reader for the QuickFIX
dictionary format** that generates, into the consumer's own compilation, the field classes, the
factory arms and the schema tables for the tags their venue adds — exactly what the generator
already does for a grammar, over a different input. It answers the Eurex case in full, it needs
no new run-time machinery, it ships nobody's file, and it is the shape this repository is for.

**Would not, yet.** Orchestra. It is the better format on paper — `presence`, conditional rules,
explicit `lengthId`, scenarios — but it has no venue adoption in public, no .NET binding at all,
a NoDerivatives specification, and data files of uncertain licence. Reading it is worth doing
when a consumer has a file to read; today that consumer does not exist. The one thing worth
taking from it now is its vocabulary: if we ever widen `required/optional`, widen it to
Orchestra's five and not to something of our own.

**Open, for Igor and the architect:**

1. Is a build-time dictionary reader in scope at all for this package, or is it a separate one?
   It is a generator over a foreign input, which is this repository's business, but it is not
   FIX's.
2. If a dictionary is read at build time, which of the three things it can produce do we want:
   the parse's length/data pairs, the schema tables, or the typed field classes? They are three
   sizes of feature, and only the third answers the Eurex case.
3. Do we widen requiredness to Orchestra's `presence`, or leave it binary until something needs
   more?
4. `Fix/Specification/` is empty. Was a specification file ever meant to live there — in which
   case this design has a place already prepared — or is the directory a leftover?
