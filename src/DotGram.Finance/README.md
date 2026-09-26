<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — which layer to use, how to read fields and messages, and the mistakes
  that are easy to make. Read it before writing code against the package. In a
  restored package that is ~/.nuget/packages/dotgram.finance/<version>/SKILL.md.
-->

# DotGram.Finance

Reads and checks FIX 4.2, FIX 4.4 and FIX 5.0 SP2 tag-value messages wherever they are kept rather
than traded: logs, archives, files, message buses. For `netstandard2.0` and `net10.0`; DotGram
compiles the grammar at build time, so applications need no DotGram runtime, grammar files, schema
XML, reflection configuration or initialization step.

## What it is for

- **Logs and archives.** Every FIX engine journals what it sent and received, often with `|` for
  the separator and spaces around it. Incident review, audit, trade reconstruction, regulatory
  reporting and execution analysis are read from those journals — gigabytes a day, read from a
  stream in bounded memory, pipe-delimited or not.
- **Checking messages against a counterparty's dictionary** before a certification or in a test
  suite: apply their data dictionary, and `Validate` reports every finding of a message at once,
  each with its rule, tag, position and group entry, rather than the first.
- **Buses and stores.** FIX kept in Kafka, a database or a queue — drop copy, post-trade, clearing —
  and read by a consumer that has no session to hold.
- **Files and other transports.** End-of-day allocation and confirmation files, FIX carried over a
  queue, an HTTP or a WebSocket gateway.
- **Tools.** Simulators, load generators, test harnesses, anonymizers, converters to other formats.
- **An engine of your own.** Where the session layer is written in house, this is the parser and
  the checks under it.

## What it is not

It is not a FIX engine. There is no socket, no session: no logon, sequence numbers, heartbeats,
resend requests, message store or schedule; a live trading connection needs an engine, which reads
with its own parser. Where you hold the bytes, this reads them: `ReadMessages` takes the `Stream`
your transport hands you.

## Versions and namespaces

What every version shares — `FixField` and its classes, `FixTag`, `FixConvert`, `FixFinding`,
`FixFraming`, `FixDictionary` — is in `DotGram.Finance.Fix`. Each version is a namespace of its own
with the same names in it: `FixParser`, `FixMessage` and its types, the components, and its context.

| Version | Namespace | Context | Message types | BeginString |
| --- | --- | --- | ---: | --- |
| FIX 4.2 | `DotGram.Finance.Fix.Fix42` | `Fix42Context` | 46 | `FIX.4.2` |
| FIX 4.4 | `DotGram.Finance.Fix.Fix44` | `Fix44Context` | 93 | `FIX.4.4` |
| FIX 5.0 SP2 | `DotGram.Finance.Fix.Fix50` | `Fix50Context` | 116 | `FIXT.1.1` |

FIX 5.0 SP2 travels over the session layer FIXT 1.1, whose header, trailer and session messages it
reads. One of its messages is a class of another name, because it carries a field of its own name
and a C# class cannot: MsgType `f`, SecurityStatus, is `FixMessage.SecurityStatusMessage`. The
examples on this page are FIX 4.4's; another version's calls are the same with its namespace and
context. Pick the namespace by the BeginString of the data: a version's parser reads another's
messages too, but builds its own classes and holds them to its own version.

The context is the one value a reading is done by and the one `Validate` holds a message to:
framing, length/data pairs, bounds, the schema. `Fix44Context.Default` reads the wire by the
standard alone; it is immutable and changed with `with`.

## Fields

```csharp
using System;
using System.IO;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

// One message, with the separator written as a pipe so it can be read on a page.
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

// One door, and the verb says where the input is: Parse takes a buffer whole, Read consumes
// a reader or a stream. The plural says how many come back.
FixMessage message = FixParser.ParseMessage(wire);

FixField[] fields = FixParser.ParseFields(wire);
var logFields = FixParser.ParseFields("55=ABC | 38=100", Fix44Context.WithLogFraming);

using var input = File.OpenRead("messages.fix");

foreach (FixField field in FixParser.ReadFields(input))
    Console.WriteLine(field.Tag);
```

`ParseFields` takes a string, a byte array or a `ReadOnlyMemory<byte>` and returns the fields at
once; `ReadFields` takes a `TextReader` or a `Stream` and returns them lazily. Both return every
field in source order, repeated and unknown tags included, and assemble nothing: no messages, no
groups, no required fields, no BodyLength or CheckSum. Use them for logs, for pulling a few values
out, and for anything that must not reject input.

A syntax error does not throw: it becomes one `FixField.Invalid`, with the position, the length and
the raw input of what was refused, and reading resumes after the next separator. A value that does
not convert — `38=abc` — is still its field, with `IsValid` false.

A field is a class of the type of its value, and its `Tag` says which field it is: an `OrderQty` is
a `FixField.Decimal` whose `Tag` is `FixTag.OrderQty`. `FixTag` holds every tag of every version as
a constant named as the FIX repository names it; a tag none defines is just its number, `25005`. A
message's property keeps its version's name where that differs: FIX 4.4's `IOIid` is `FixTag.IOIID`.

| FIX primitive | Field | Value |
| --- | --- | --- |
| int, Length, NumInGroup, SeqNum, TagNum, DayOfMonth | `Integer` | long |
| float, Qty, Price, PriceOffset, Amt, Percentage | `Decimal` | decimal |
| char | `Character` | char |
| Boolean | `Boolean` | bool |
| String, Currency, Country, Exchange, Language | `Text` | string |
| MultipleValueString, MultipleCharValue | `Multiple` | string[], each value one character |
| MultipleStringValue | `Multiple` | string[], each value a word |
| UTCDateOnly, UTCDate, LocalMktDate | `Date` | DateOnly |
| UTCTimeOnly | `Time` | TimeOnly |
| TZTimeOnly | `ZonedTime` | (TimeOnly Time, TimeSpan Offset), the clock as written and its offset |
| UTCTimestamp | `Timestamp` | DateTimeOffset, at offset zero |
| TZTimestamp | `Timestamp` | DateTimeOffset, at the offset it was written with |
| MonthYear | `MonthYear` | string, checked for its shape (`YYYYMM`, `YYYYMMDD`, `YYYYMMw1`..`w5`) |
| data, XMLData | `Data` | ReadOnlyMemory<byte> |

- `Value` throws when `IsValid` is false; test it first, or use `TryGetValue`. A code set is held
  by `Validate`, not while the field is read.
- A field declared `char` whose code set lists values a character cannot hold is `Text`: in FIX
  4.4, MiscFeeType and MassCancelRejectReason.
- A zoned value is the clock to the minute, seconds and a fraction optional, then `Z` or a sign and
  hours with minutes optional, up to fourteen hours either way (`13:09+05:30`); one without an
  offset is not valid.
- A leap second, `23:59:60`, is read as the second after it; the year `0000` is not a value. A
  fraction of a second is kept to the tick. An integer must fit a `long` and a decimal must fit
  `System.Decimal` exactly, or the field is not valid.
- `Date` and `Time` hold `DateOnly` and `TimeOnly`; on netstandard2.0 and .NET Framework these come
  from the `Portable.System.DateTimeOnly` package, under the same names.

## Messages

```csharp
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

if (FixParser.TryParseMessage(wire, out var message, out var error))
{
    if (message is FixMessage.NewOrderSingle order)
    {
        Console.WriteLine(order.Symbol?.Value);
        Console.WriteLine(order.OrderQty?.Value);
        Console.WriteLine(order.SenderCompID?.Value);

        foreach (var party in order.NoPartyIDsGroups ?? [])
            Console.WriteLine(party.PartyID.Value);   // tag 448
    }
}
else
{
    Console.WriteLine($"{error!.Position}: tag {error.Tag}, " +
                      $"MsgType {error.MessageType}: {error.Reason}");
}
```

A version's message types are the cases of its `FixMessage`, nested in it and written
`FixMessage.NewOrderSingle`: a closed set, so a `switch` over it reads as one. A message's
properties are named after its fields and are the typed fields themselves, the standard header's
and trailer's included: `order.OrderQty` is a `FixField.Decimal?`, null when the field is absent. A
component's fields are properties of whatever carries it, which implements the component's
interface (`IInstrument`). A repeating group is named for its counter: the list `<Counter>Groups`
beside its counter, of entries of the class `<Counter>Group` nested in what carries it —
`order.NoPartyIDsGroups`, of `IParties.NoPartyIDsGroup`. `Fields` is the whole message in wire
order, the fields of every group included.

`ParseMessage` reads one complete message and throws `FormatException` where it cannot;
`TryParseMessage` returns false with a `FixParseError` saying where and why. `ParseMessages` reads a
buffer of several. `BuildMessage(source, fields)` builds the message from fields `ParseFields`
already returned, without reading the input again.

## Streams and input

```csharp
using var input = File.OpenRead("messages.fix");
foreach (var message in FixParser.ReadMessages(input))
    Console.WriteLine(message.MessageType);

// Alternatively, read exactly one frame from a fresh stream:
using var single = File.OpenRead("messages.fix");
var next = FixParser.ReadMessage(single, maxMessageLength: 4 * 1024 * 1024);
```

`ReadMessage`, `TryReadMessage` and `ReadMessages` take a `TextReader` or a `Stream` and cut it into
messages by their `BodyLength`, 16 MiB each at most unless given. They are synchronous, read lazily
and leave the input open; enumerate once. Clean EOF ends `ReadMessages`, and EOF inside a frame is
an error. A failed read may have consumed input: there is no resynchronization.

**A string is octets, not text.** Every character stands for one octet, U+0000 through U+00FF,
because `BodyLength` and `CheckSum` count octets. Decode a wire file with Latin-1, never UTF-8. Better,
hand over the octets: every field and message call also takes `byte[]` and `ReadOnlyMemory<byte>`,
and then no encoding can be got wrong.

A length/data pair — `95=5` then `96=` and five octets — is read by its length, so the payload may
contain separators; it comes back as two fields, the length and the data. Counterparty-defined pairs
go in the context, and **add to** the version's own (fourteen in FIX 4.2, sixteen in 4.4,
twenty-four in 5.0 SP2):

```csharp
using System.Collections.Generic;

var fields  = FixParser.ParseFields("55=ABC|38=100|", Fix44Context.WithLogFraming);
var context = new Fix44Context
{
    LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 },   // added to the version's own pairs
};
var custom  = FixParser.ParseFields("5000=3 | 5001=a|b | ", context with { Framing = FixFraming.Log });
```

The context's `MaxRetained` bounds one field read from a reader or a stream, 16 Mi by default; a
field that needs more throws `IOException`. A field owns its value and a message its fields: no
input is retained.

## Pipe-delimited logs

```csharp
var logLine   = "55=ABC | 38=100";
using var logReader = new StringReader(logLine);

var message = FixParser.ParseMessage(logLine, Fix44Context.WithLogFraming);
var context = Fix44Context.WithLogFraming;
foreach (var item in FixParser.ReadMessages(logReader, context))
    Console.WriteLine(item.MessageType);
```

`WithLogFraming` reads a bare `|`, a spaced ` | `, or a mixture; spaces inside values are kept, and
positions refer to the log as it was given. A log may pad its pipes: BodyLength and CheckSum are
measured over the fields as the wire had them. Streamed messages, cut by their BodyLength, want a
bare `|`. Only structural separators may be rendered as pipes; a log that rewrites raw-data octets
is not lossless and needs its own decoding.

## Validation

Reading the wire and holding the result to a schema are two acts. **Recognition refuses** what
leaves the input's meaning unknown: a message not ended by its `CheckSum`, one without a `MsgType`,
a data field its length does not measure, a `NumInGroup` that would size an array past the fields
left, a group entry that does not begin with its delimiter. **Everything else is a finding** about a
message that was built: `message.Validate(context)` holds it to the context's schema, answers
whether nothing is wrong, and puts everything that is in `message.InvalidFindings` at once:

| What it reports | |
| --- | --- |
| `RequiredFieldMissing` | a field the schema requires in that scope is absent |
| `RequiredComponentMissing` | a required component has none of its fields; the sentence names the first tag it would have held |
| `InvalidValue` | the value does not fit the tag's type, or is not one of its code set; a BeginString not the version's |
| `DuplicateField` | a tag appears twice in one scope |
| `FieldNotInScope` | the schema defines no such tag, or defines it and not there |
| `FieldOutOfOrder` | a header field after the body has begun, or `BeginString`, `BodyLength` and `MsgType` not first, second and third |
| `GroupCountMismatch` | a counter that is not the number of entries after it, or entries no counter announces |
| `UnknownMessageType` | the schema describes no message of that `MsgType` |
| `MessageEncodingMissing` | an `Encoded` field is present and `MessageEncoding` (347) is not; said once, at the first such field |
| `BodyLengthMismatch` | `BodyLength` is not the number of octets of the body it was read with |
| `CheckSumMismatch` | `CheckSum` is not the sum, modulo 256, of the octets before it |
| `LengthFieldNotBeforeData` | a length field is not followed by the data it measures |

Each `FixFinding` names its rule, its tag, the field, where in the source it begins and the entry of
the repeating group it is in — "tag 448 is wrong" says nothing where a message carries nine
parties. Code ported from a library that stops at the first problem handles one finding per
message; `InvalidFindings` holds all of them.

It is not a trading or session validator: prose-only conditional requirements, sequence-number
state, order economics and live ISO registry assignments are outside it; ISO identifiers are
checked for their lexical shape.

## Dictionaries

A counterparty's data dictionary is read, merged and edited as a value, then applied to a context:

```csharp
var standard = FixDictionary.LoadFile("FIX44.xml");
var venue    = FixDictionary.LoadFile("venue.xml");
var merged   = standard.Merge(venue);                // what venue describes replaces standard's, whole

merged.Messages["D"].Members.Add(FixDictionaryMember.Field("ExDestination", required: true));
merged.Fields[40].Codes.Remove("P");

var context = Fix44Context.Default.With(merged);
```

`FixDictionary` is the file's messages (by MsgType), components (by name) and fields (by tag), and
the standard header and trailer, by name and unresolved: reading refuses only what is not a
dictionary. `With` answers a new context in which a message type, component, group entry or field
the dictionary describes is held to its whole check; it takes what the dictionary says at that
moment, and editing the dictionary afterwards changes nothing in the context. A dictionary that
places a field where the model has no property for it, lists a code its field's type cannot hold,
or gives one name to two tags, is refused there, whole. Applying costs what reading does: the checks
are then compiled in the background, side by side, and a message that arrives before its check is
ready compiles it itself, once. `context.Prepare()` waits until all of them are ready, for a caller who
wants no message to pay for that. `Load(text)` and `LoadFile(path)` on the context are the short forms. The package ships nobody's dictionary: the file is yours, and
its licence obligations are yours with it.

## Custom fields and messages

A tag the version does not define is read as the type a dictionary applied to the context gives it.
A MsgType the version does not define goes through the context's message factory, which is handed
the MsgType and answers the message to build, or null:

```csharp
using System.Collections.Generic;

var context = new Fix44Context
{
    FixMessageFactory = type => type == "U1" ? new VenueQuote() : null,
    LengthDataPairs   = new Dictionary<int, int> { [25000] = 25001 },
}.Load("""
    <fix>
      <fields>
        <field number="25005" name="VenueStatus" type="STRING" />
        <field number="25006" name="VenuePrice" type="PRICE" />
      </fields>
    </fix>
    """);

sealed class VenueQuote : FixCustomMessage
{
    public FixField.Text? Status { get; private set; }

    // Every field of the body in turn, the header's and the trailer's taken already; false is out of scope.
    protected override bool Place(FixField field)
    {
        if (field is not FixField.Text { Tag: 25005 } status)
            return false;

        Status = status;

        return true;
    }

    protected override void OnValidate(Fix44Context context)
    {
        if (Status is null)
            AddFinding(new FixFinding(FixRule.RequiredFieldMissing, 25005, 0, null, -1));
        else if (Status.Value is not ("OPEN" or "CLOSED"))
            AddFinding(new FixFinding(FixRule.InvalidValue, Status.Tag, Status.Position, Status, -1));
    }
}
```

A tag neither the version nor a dictionary defines is a `FixField.Invalid` of that tag with its
octets, and a type the factory answers null for is a `FixMessage.Invalid`, its finding
`UnknownMessageType`.

## Attribution

The tables of each FIX version — its fields, their types and code sets, its messages and
components — are derived from the FIX Protocol specification (FIX Unified Repository, 2010
edition). The FIX Protocol specification is Copyright FIX Protocol Limited,
<https://www.fixtrading.org>. This package's code is under the MIT licence, as the rest of DotGram
is; the attribution says where the data came from and grants or restricts nothing.
