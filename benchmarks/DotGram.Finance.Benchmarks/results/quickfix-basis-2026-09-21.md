# The basis of the QuickFIX/n comparison

What was chosen on each side and why, written before the numbers so that the choices are recorded
when they are made rather than reconstructed when they are questioned.

**These are comparative figures about a named third-party library. They are internal and dated:
not for a shipped page, a README or release notes without Igor's word.** The harness that produced
them is not in this repository — QuickFIX/n is not brought into what we build as our own — and
lives on the machine under `.work/qfcompare`.

## What is compared

| | ours | QuickFIX/n |
| --- | --- | --- |
| parse | `FixParser.ParseMessage(wire)` | `Message.FromString(wire, validate: false, dd, dd, factory)` |
| parse and validate | `FixParser.ParseMessage(wire)` then `message.Validate()` | `Message.FromString(wire, validate: true, dd, dd, factory)` then `DataDictionary.Validate(message, dd, dd, "FIX.4.4", msgType)` |
| read the dictionary | `FixParser.LoadDictionary(stream)` | `new DataDictionary(path)` |

Parse is paired with parse and validate with validate. It takes two calls on each side to hold a
message to the schema — theirs because the flag alone does not check required fields, ours because
reading and checking are two acts — and a row that faced one call with two would be measuring the
difference in API shape and calling it speed.

**Every row reads fields.** Ours reads a field from the source when asked; theirs builds a field
map while parsing. A row that parsed and never looked at a field would measure our laziness. The
tags read are the same on both sides, in the same order, and `--against-check` prints what each
side read.

## Their side, and why this call

`Message.FromString` is the documented entry point (QuickFix.xml, the shipped documentation of
QuickFIXn.Core 1.14.1). The FIX 4.4 message factory is passed so that they build the typed message
we build — with `null` they would construct generic groups, which is less work and less function,
and would not be the same answer. The dictionary is passed because groups need it.

**Their validating side is two calls, and this was measured rather than assumed.** The pairing
first had `FromString(..., validate: true, ...)` alone facing our `Validate()`. `--flag-check`
asked their library about a well-framed, schema-invalid message — a NewOrderSingle with no
ClOrdID — and the answer was:

    ours:                                    1 finding
    validate:false                           accepted
    validate:true                            accepted
    validate:true + DataDictionary.Validate  refused: RequiredTagMissing

So their `validate` flag does not check required fields; the static `DataDictionary.Validate` —
what a session performs after parsing — does. Their validating row therefore makes both calls.
Left as it was, the table would have had their side doing less than ours and would have flattered
us, and no corpus of VALID messages could ever have shown it: on valid input both sides answer
the same at every shape and size.

Versions read from the package, not remembered: **QuickFIXn.Core 1.14.1**, **QuickFIXn.FIX44
1.14.1**. The dictionary both sides read is the `FIX44.xml` that ships with QuickFIXn.FIX44 1.14.1.

## Levelled in both directions, and the list that says so

Their side was made two calls so that it would catch what ours catches. The symmetric question —
does their pair now do something OURS does not — is answered by asking, one defect at a time,
rather than by reasoning about it (`--matrix`):

| defect | ours | QuickFIX/n (their pair) |
| --- | --- | --- |
| valid | accepted | accepted |
| no ClOrdID (required) | 1 finding: RequiredFieldMissing | refused: RequiredTagMissing |
| Side not in code set | 1 finding: InvalidValue | refused: IncorrectTagValue |
| OrderQty not a number | 1 finding: InvalidValue | refused: IncorrectDataFormat |
| a tag the schema does not place here | 1 finding: FieldNotInScope | refused: InvalidTagNumber |
| CheckSum wrong | refused reading: FormatException | refused: InvalidMessage (CheckSum) |
| BodyLength wrong | refused reading: FormatException | refused: InvalidMessage (BodyLength) |

On these seven, neither side does work the other does not: the coverage is the same and the FORM
differs — ours answers with every finding, theirs throws on the first. Seven defects are evidence
about seven defects and not a proof of equal coverage, and what is timed is the valid row, where
both sides do the whole sweep and neither can stop early.

Their second call does not throw on a valid message: `--against-check` runs it on every shape and
it returns, which is measured rather than assumed.

## The asymmetry, which is the point

Our schema is compiled into the package **and** can be loaded from a dictionary at run time.
QuickFIX/n has only the second. So the table runs our two modes against their one, and our loaded
mode reads the same file they read. The compiled mode costs nothing before the first message; the
loaded mode costs what the `read the dictionary` row says.

A rule lives in a static field on the message class, so the mode is process-wide state: every case
asserts which schema its process is holding, and `--guard-check` is the proof that the assertion
fires, taken from the side where it must refuse.

## Four shapes

| shape | what it exercises |
| --- | --- |
| `Order` | a flat message: eight fields, no group, no binary |
| `Parties` | a repeating group of three entries, read entry by entry |
| `PartiesLarge` | the same group at a thousand entries |
| `Binary` | a length/data pair, read by its length |

`Parties` against `PartiesLarge` is the only pair here that can see a change of mechanism rather
than a price per entry. The other three differ in what the reader does, not in size.

## What is not measured, and why

- **Invalid messages.** Their validation stops at the first problem and ours reports every
  finding: on invalid input those are two different jobs, and a row comparing them would need an
  explanation rather than a number.
- **Log framing, streaming, messages of thousands of fields other than the group.** Out of the
  question asked.
- **A ratio without its A/A.** Every run carries A/A rows — the same work under a second name —
  and a ratio is read against their spread. Two identical methods have come out 21% and 28% apart
  on this machine in a bad hour; the resolution belongs to the hour, not to the harness.

## How the numbers are taken

- Allocations: any time, pinned to cores 16-31. `MemoryDiagnoser` is deterministic, and the time
  column of such a run is not read.
- Times: in a window the stand announces, with its pinning and its filing.
- The dictionary rows: cold, one invocation a sample, ten fresh processes, the file read once
  first so that neither side pays the disk for both. BenchmarkDotNet's warm iteration would
  measure the warm cost of a construction, which is a real number about a thing nobody does.
