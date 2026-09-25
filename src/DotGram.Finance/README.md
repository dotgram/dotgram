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

## Flat field parsing

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

`FixParser` returns fields in source order, including repeated and unknown tags. It does
not assemble messages or groups, check required fields, code sets, BodyLength or
CheckSum. A failed primitive conversion sets `FixField.IsValid` to false; it does
not reject the field, except that a binary length must be valid to find the next
field boundary. Text values need no conversion validity check.

`ParseFields` takes a string, a byte array or a `ReadOnlyMemory<byte>` and returns the fields at
once; `ReadFields` takes a `TextReader` or a `Stream` and returns a lazy `IEnumerable<FixField>`.
The grammar directly yields `FixField`. Ordinary fields are returned immediately;
a binary length/data pair produces two, the length and then the data it measures.
For example, `95=3|96=a|b|` produces a `FixField.Integer` of 3 whose tag is `RawDataLength` and a
`FixField.Data` of `a|b` whose tag is `RawData`, each with its own `Position`, `ValuePosition` and `Length`. Native char/byte buffers
release each completed field. The context's `MaxRetained` bounds one field, from its tag through
the separator that ends it, in characters from a reader or bytes from a
stream: `FixParser.DefaultMaxRetained`, 16 Mi of either, unless given. A field that needs more
throws `IOException`; give the context a larger `MaxRetained` to read it. Its `BufferSize`,
4096 unless given, is the size of the buffer the input is read through.
Locations remain relative to the complete input. The input stays open on completion,
error or early disposal. Keep one enumeration per input: buffering can read ahead,
so restarting after an early stop can lose unread buffered data.

The grammar uses `Field* recover Separator`. A syntax failure returns one
`FixField.Invalid` in source order, then parsing resumes after the next separator
(or finishes at EOF). `Invalid.Position` and `Length` describe the rejected input,
excluding the synchronization separator; `Tag` is 0 and `IsValid` is false.
`RawText` owns the original character input, while `RawBytes` owns the original
byte input (`IsByteInput` distinguishes them). `Message` describes the failure.
I/O errors and exceptions from user C# code still propagate during enumeration.

```csharp
foreach (var field in FixParser.ParseFields("55=ABC|broken|38=2", Fix44Context.WithLogFraming))
{
    if (field is FixField.Invalid invalid)
        Console.WriteLine($"{invalid.Position}: {invalid.Message}: {invalid.RawText}");
}
```

Valid binary payloads are consumed by length, including embedded separators.
After a malformed binary header or length, separator recovery is best effort:
the next separator may be inside damaged payload data. Primitive conversion
failures still come back as the field of the tag's type, with `IsValid == false`; `recover`
handles recognition failures, not semantic validation.
Use `.ToArray()` when a complete list is needed. String, byte-array and memory overloads
materialize the complete result. Empty input returns no fields.
Concatenated messages are read as one ordered field sequence.

`FixParser.ParseFields` and `ReadFields` read SOH-delimited wire input; given
`Fix44Context.WithLogFraming` they read logs with bare `|`, spaced ` | `, or a mixture.
`Fix44Context` declares which framing to read and any length/data pairs of your own; it is the
one value a reading is done by and the one `Validate` holds a message to, and `Fix44Context.Default`
reads the wire by the standard alone. It derives from `FixContext`, the part every version of FIX
shares.

What every version shares — `FixField` and its classes, `FixTag`, `FixConvert`, `FixFinding`,
`FixFraming`, `FixDictionary` — is in the namespace `DotGram.Finance.Fix`. What is a version's own —
`FixParser`, `FixMessage` and its types, the components, its context — is in a namespace of its own:
FIX 4.4's is `DotGram.Finance.Fix.Fix44`, with 93 message types and `Fix44Context`.

Every version is a namespace of its own with the same names in it: FIX 4.2 is
`DotGram.Finance.Fix.Fix42`, whose `FixParser` reads a 4.2 message into its 46 types and whose
`Fix42Context` holds it to FIX 4.2. FIX 5.0 SP2 is `DotGram.Finance.Fix.Fix50` with `Fix50Context`:
its 116 types over the session layer FIXT 1.1, whose BeginString, `FIXT.1.1`, is the one its
messages carry. One of its messages is a class of another name, because it carries a field of its
own name and a C# class cannot: MsgType `f`, SecurityStatus, is `FixMessage.SecurityStatusMessage`.
A dictionary that names the message SecurityStatus is read as naming that class. The examples on
this page are FIX 4.4's; another version's calls are the same with its own namespace and context.

The log grammar uses `LogSeparator = ' '* & '|' & ' '*`. ASCII spaces immediately
before or after a pipe belong to that separator. Spaces inside text values are
preserved; spaces before EOF are also preserved when no pipe follows. Length-delimited
binary payloads are never trimmed, even when they contain ` | ` or end in spaces.
Field positions refer to the original input, including its formatting spaces.
A streamed log field reads ahead through the padding to the next character or
EOF before yielding. Wire parsing can yield as soon as SOH is read.
A length field makes the field right after it its data: when that field has the paired data tag,
the parser consumes exactly the declared number of data bytes, including any delimiter bytes
inside the payload. A data field anywhere else is rejected as an `Invalid`; a length followed by
something else is still a length, and the message calls refuse it. The final field may end at EOF without a separator. Separators between fields
remain required; the declared binary length still determines the complete payload.
`FixParser.ParseFields` returns the completed field sequence, including `Invalid` fields.
Use `FixParser.TryParseMessage` or `FixParser.BuildMessage` to build a message; both refuse a
recovered syntax error with the first syntax diagnostic, whatever the framing. Typed values own their data;
no complete source string is retained by a field. Octets held in an array or memory are read
where they lie; native byte-stream parsing creates no complete character view.

## Computed dispatch parser

`FixParser` is the main parser, with string, byte-array, `ReadOnlyMemory<byte>`, `TextReader`
and byte `Stream` input forms. Its small [grammar](Fix/FixGrammar.gram) reads a
numeric tag and uses `switch` to select text or a length/data pair. C# supplies
classification and typed field construction.

The large `Fix44` grammar is retained in
`tests/DotGram.Finance.Fix44` for regression tests and benchmarks.
It is not included in the Finance package.

```csharp
using System.Collections.Generic;

var fields  = FixParser.ParseFields("55=ABC|38=100|", Fix44Context.WithLogFraming);
var context = new Fix44Context
{
    LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 },   // added to the version's own pairs
};
var custom  = FixParser.ParseFields("5000=3 | 5001=a|b | ", context with { Framing = FixFraming.Log });
```

A supplied length/data dictionary, length tag to data tag, **adds to** the version's own pairs
(fourteen in FIX 4.2, sixteen in 4.4, twenty-four in 5.0 SP2) and is copied at construction; a length tag it repeats has the pair it gives. Omit it to use
the standard pairs alone. The same object is what the message calls take, so one value describes
both kinds of answer. The pair produces two fields, a `FixField.Integer` and a `FixField.Data` where
nothing gives the tags another type; a tag neither the standard nor a loaded dictionary defines is a
`FixField.Invalid` of that tag carrying its value. Standalone
data tags are rejected. The parser recognizes binary boundaries; message and business
validation remain in the explicitly called semantic API.

## Explicit message semantics

The message calls run only when called explicitly.
`BuildMessage(source, fields)` requires fields parsed from that exact source and performs
recognition and group assembly without parsing it again. `ParseMessage` combines both steps.
Holding the result to a schema is `Validate`, a call of its own.

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

`FixParser.ParseMessage(wire)` returns the same model and throws `FormatException` on malformed
input. `TryParseMessage` returns false and leaves `message` null. Null input also returns
false in `TryParseMessage`; a context that cannot be made, a framing that is neither wire nor log, is a programming
error. The `string`, `byte[]` and `ReadOnlyMemory<byte>` overloads accept one
complete message. Concatenated messages are rejected by these overloads; `ParseMessages` reads a
buffer of them, and `ReadMessages` consumes a sequence from a reader or stream.

## Streaming input

```csharp
using var input = File.OpenRead("messages.fix");
foreach (var message in FixParser.ReadMessages(input))
    Console.WriteLine(message.MessageType);

// Alternatively, read exactly one frame from a fresh stream:
using var single = File.OpenRead("messages.fix");
var next = FixParser.ReadMessage(single, maxMessageLength: 4 * 1024 * 1024);
```

`ReadMessage`, `TryReadMessage` and `ReadMessages` accept either `TextReader` or `Stream`,
with an optional context of the version. Byte streams use the generated native
byte machine and `ReadOnlySpan<byte>` conversion hooks. Character readers preserve
the lossless octet mapping described below. Non-seekable inputs and short reads are supported. These APIs are
synchronous and leave the input open, including when enumeration stops early.

The adapter frames messages using `BodyLength`, then performs the same grammar, raw-data and
group recognition as the string API. Checking a
message against the schema is a separate call: see **Validation** below.
It reuses a growing buffer across `ReadMessages` iterations and copies each frame out of it
before reading it. Buffering is bounded by the largest frame seen, not the length of the stream;
a message keeps its fields and not the frame. The default `maxMessageLength` is 16 MiB per
message, including header and trailer; callers can set a different positive
limit. This is a frame-size limit, not a total allocation budget.

Clean EOF ends `ReadMessages`; EOF before a complete frame is an error.
`TryReadMessage` returns false with a diagnostic for malformed, oversized, truncated,
or empty input. `ReadMessage` and `ReadMessages` throw `FormatException` for these
errors (except clean EOF for enumeration). Diagnostic positions are relative to
the current frame. I/O exceptions propagate. A failed parse may consume input;
there is no automatic resynchronization or rollback of the underlying stream.

The framing adapter prevents read-ahead from consuming the next message. The byte
path retains a byte frame and passes it to the generated buffered byte parser;
field conversion does not transcode numeric input. This is not a zero-copy API: each field owns
its value, independently of subsequent stream reads. See the finance benchmarks for total
parsing costs.

## Input and ownership

The input is a **lossless octet string**: every character represents one octet,
U+0000 through U+00FF. The default delimiter is SOH (`\u0001`); `Fix44Context.WithLogFraming`
reads pipe-delimited logs. `BodyLength` and `CheckSum` are held to the octets present on the
wire, including raw and encoded data, so a string that is not one character an octet is found
wrong by them. Decode a wire file with Latin-1, not UTF-8; an Encoded field's
payload remains opaque and its declared `MessageEncoding` remains available.

**Or hand over the octets and make no claim at all.** Every field and message call also takes
`byte[]` and `ReadOnlyMemory<byte>`: `ParseFields`, `ParseMessage`, `TryParseMessage` and
`ParseMessages`. There the package decodes, knowing that the specification counts octets, so
nothing depends on the caller having chosen an encoding. What this buys is correctness, not
allocation: the values are materialised one character to one octet either way, and the cost is
about what the string road costs.

Input is read where it lies and is not retained: a field owns its value, and a message its fields.
Networking and FIX session state are outside this package.

## Model

A version's message types — 46 in FIX 4.2, 93 in 4.4, 116 in 5.0 SP2 — are the cases of its
`FixMessage`, nested in it and written
`FixMessage.NewOrderSingle`: a closed set, so a `switch` over it reads as one and the base
type stands in front of every arm. A MsgType the schema does not describe is built by the
context's `FixMessageFactory`, or is a `FixMessage.Invalid`, which is what lets the set be closed
without being complete.
A message's properties are named after its fields and are the typed fields themselves, the
standard header's and trailer's included: `order.OrderQty` is a `FixField.Decimal?`,
`order.SenderCompID` a `FixField.Text?`. A component's fields are properties of whatever carries
it, which implements the component's interface (`IInstrument`). A repeating group is named for
its counter: the list `<Counter>Groups` beside its counter, of entries of the class
`<Counter>Group` nested in what carries it — `order.NoPartyIDsGroups`, of `IParties.NoPartyIDsGroup`.

- An absent field is null, and so is an absent group.
- `Fields` is the whole message in wire order, the fields of every group included.
- A value that does not convert leaves the field present with `IsValid` false.
- `Date` and `Time` hold `DateOnly` and `TimeOnly`; on netstandard2.0 and .NET Framework
  these come from the `Portable.System.DateTimeOnly` package, under the same names.

## Recognition and validation

Reading the wire and holding the result to a schema are two acts, and they refuse at
different times for different reasons.

**Recognition refuses, because after it the input's meaning is unknown.** A message that does
not end with its `CheckSum` closed by a separator, one without a `MsgType`, a data field whose
length does not measure it, a `NumInGroup` that would size an array past the fields left, a group
entry that does not begin with its delimiter, and a field the parser could not read at all. None
of these can wait for a message to exist: they are how the reader finds where one ends. A stream
is cut into messages by their `BodyLength`, so there a wrong one is a refusal too.

**Everything else is a finding about a message that was built.** `message.Validate(context)` holds
it to the context's schema, the version's own unless a dictionary was applied, answers whether nothing is
wrong, and puts everything that is in `message.InvalidFindings` at once:

| What it reports | |
| --- | --- |
| `RequiredFieldMissing` | a field the schema requires in that scope is absent |
| `RequiredComponentMissing` | a required component has none of its fields; the sentence names the first tag it would have held |
| `InvalidValue` | the value does not fit the tag's type, or is not one of its code set |
| `DuplicateField` | a tag appears twice in one scope |
| `FieldNotInScope` | the schema defines no such tag, or defines it and not there |
| `FieldOutOfOrder` | a header field after the body has begun, or `BeginString`, `BodyLength` and `MsgType` not first, second and third |
| `GroupCountMismatch` | a counter that is not the number of entries after it, or entries no counter announces |
| `UnknownMessageType` | the schema describes no message of that `MsgType` |
| `MessageEncodingMissing` | an `Encoded` field is present and `MessageEncoding` (347) is not; said once, at the first such field |
| `BodyLengthMismatch` | `BodyLength` is not the number of octets of the body it was read with |
| `CheckSumMismatch` | `CheckSum` is not the sum, modulo 256, of the octets before it |
| `LengthFieldNotBeforeData` | a length field is not followed by the data it measures |

A `BeginString` other than the version's own — `FIX.4.4` under `Fix44Context` — is an `InvalidValue`. The octets the length and the sum are
held to are measured while the message is read, since it keeps its fields and not its source.

Each `FixFinding` names its rule, its tag, the field, where in the source it begins and the entry
of the repeating group it is in — "tag 448 is wrong" says nothing where a message carries nine
parties.

Body fields may be reordered, and an unknown tag between the header and the trailer belongs
to the body rather than ending it: whether it should be there is a finding, and a message the
reader will not build is a message nothing can report on. The exact wire and every offset are
preserved whatever is found.

**A port from a library that stops at the first problem needs looking at.** Code written
against one — where validation throws the first thing wrong, or returns it — handles one
problem per message; `InvalidFindings` holds all of them, and code that reads only the first
drops the rest without a word. A finding is the same story: where a message carries nine parties,
`EntryIndex` and `Position` are what say which one, and code that reads `Tag` alone throws that away.

**A counterparty's dictionary is applied to a context.** Reading and applying are two steps, so
that dictionaries can be merged and edited in between:

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
dictionary. `With` answers a new context: a message type, component, group entry or field the
dictionary describes is held to its whole check from then on, and the context it was applied to is
unchanged. It takes what the dictionary says at that moment; editing the dictionary afterwards changes
nothing in a context already made. A dictionary that places a field where the model has no property
for it, lists a code its field's type cannot hold, or gives one name to two tags, is refused whole,
there, because a schema that is half one file and half another is worse than a refusal at the door.
Each check is compiled when it is first asked, so applying a whole dictionary costs what reading it
does, and the first message of each type held to it pays for its own check.

`Load(text)`, `LoadFile(path)`, and `Load` of a `TextReader`, a `Stream` or several texts merged in
order are the short forms: read and apply in one call. A type this package has no class for is
still unknown; what to do about such a type is a `FixCustomMessage`. The package ships nobody's
dictionary: the file is yours, in your repository, and its licence obligations are yours with it.

The context is a value and not a setting of the process, so two counterparties with two schemas
are two contexts, and a pass over one input never sees the other's.

It is not a trading or session validator. Prose-only conditional requirements,
sequence-number state, order economics, live ISO registry assignments and announced
leap-second dates are outside it; ISO identifiers are checked for their lexical shape.

## Fields and typed values

A field is a class of the type of its value, and its `Tag` says which field it is: an `OrderQty` is
a `FixField.Decimal` whose `Tag` is `FixTag.OrderQty`. `Tag` is the tag's number, and `FixTag` holds
every tag of every version this package reads as a constant named as the FIX repository names it; a
tag none defines is just its number, `25005`. A message's property keeps its own version's name
where that differs: FIX 4.4's `IOIid` is `FixTag.IOIID`. A message's properties are typed the same way:

```csharp
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

var order = (FixMessage.NewOrderSingle)FixParser.ParseMessage(wire);
FixField.Text symbol = order.Symbol!;
FixField.Decimal quantity = order.OrderQty!;
Console.WriteLine(symbol.Value);             // string
Console.WriteLine(quantity.Value);           // decimal

foreach (var field in order.Fields)
    if (field is FixField.Decimal { Tag: FixTag.Price, IsValid: true } price)
        Console.WriteLine(price.Value);
```

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

A field declared `char` whose code set lists values a character cannot hold is `Text`: in FIX 4.4,
MiscFeeType and MassCancelRejectReason.

A zoned value is the clock to the minute, seconds and a fraction optional, then `Z` or a sign and
hours with minutes optional, up to fourteen hours either way (`13:09+05:30`), as the zones of the
world run. One without an offset does not say which instant it is, and is not valid.

A code set is held against the schema by `Validate`, not while the field is read; the
underlying primitive remains the value type. A leap second, `23:59:60`, which the protocol
admits, is read as the second after it, so that the value is the instant and only the
notation is lost; the year `0000`, which the protocol also admits, is not a value. A fraction
of a second is kept to the tick.
The character numeric hooks use invariant .NET parsing; byte decimal hooks use
UTF-8 decimal parsing with the same FIX syntax checks. Integer values must fit a
`long`; a value that does not sets `IsValid` to false. Decimal values must fit
`System.Decimal` exactly: overflow
and loss of fractional precision set `IsValid` to false rather than rounding.
Trailing fractional zeros do not cause a loss of precision.

A value that does not convert leaves its field with `IsValid == false`; `TryGetValue` returns false
and `Value` throws. This flag describes primitive conversion, not code-set or message-schema validity.
A tag the package does not define is read as the type a loaded dictionary gives it, or is a
`FixField.Invalid` of that tag with its value's octets. A standard message has no property for such a
tag, so it is out of scope there; a message the consumer builds places it (below).

## Pipe-delimited logs

```csharp
var logLine   = "55=ABC | 38=100";
using var logReader = new StringReader(logLine);

var message = FixParser.ParseMessage(logLine, Fix44Context.WithLogFraming);
var context = Fix44Context.WithLogFraming;
foreach (var item in FixParser.ReadMessages(logReader, context))
    Console.WriteLine(item.MessageType);
```

A log may pad its pipes with spaces: BodyLength and CheckSum are measured over the fields as the
wire had them, each ended by one separator, so a rendering is held to the message it renders.
Streamed messages are cut by their BodyLength and want a bare `|`.

The common grammar declares `Separator` and specializes the log publication with
`with (Separator = LogSeparator)`. The generator recognizes the guarded text run
`(?!Separator & any)+` and emits a linear scan for these delimiters.
Only structural SOH separators are rendered as pipes. Raw-data payload octets must
remain untouched; a log that replaces or escapes payload bytes is not lossless and
requires its own decoding before this API. Arbitrary log prefixes are not accepted.
BodyLength counts the wire's octets and the pipe rendering does not change it; CheckSum is summed
as the SOH representation would be, a separator counted as one SOH, never pipes inside raw data.
Positions refer to the log as it was given.

## Custom fields and messages

A tag the version does not define is typed by a dictionary applied to the context: each field it
describes that the standard does not is read, from then on, as the type the dictionary gives it,
into the same classes the standard's fields are. A dictionary does not retype a tag the standard
defines. A MsgType the version does not define goes through the message factory the context holds,
which is handed the MsgType and answers the message to build, or null:

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

The tags of a declared pair that nothing gives a type are read as their halves: the length a
`FixField.Integer`, the data a `FixField.Data`. A tag neither the standard nor a loaded dictionary
defines is a `FixField.Invalid` of that tag with its value's octets in `RawBytes`, and a type the
message factory answers null for is a `FixMessage.Invalid`, not valid from the moment it is read,
its finding `UnknownMessageType`.

## Definition maintenance

A version's field types, message classes, components and checks are written by `Fix/generate.py`
from the FIX repository and are not edited by hand: a change is made in the script or a template
and the versions are written again. The FIX 4.4 field grammar the tests hold the package against,
and the test fixtures, are maintained by hand beside them.

What every version shares is in `Fix/`:

- `Fix/FixField.cs`: field base, typed-value access, locations and the class of each value type.
- `Fix/FixFieldBuilder.cs`: construction of a field from its tag's type.
- `Fix/FixContext.cs`: the context every version's derives from, and the table the reader indexes.
- `Fix/FixDictionary.cs`: a data dictionary as a value, read, merged and edited.
- `Fix/FixValidator.cs`, `Fix/FixValidator.Load.cs`: what every check says a finding with, and the
  application of a dictionary to a version's checks, each compiled when it is first asked.
- `Fix/FixTag.cs`: the number of every tag of every version as a constant named for it.
- `Fix/generate.py`, `Fix/Templates/`: the script that writes every version's directory and
  `FixTag.cs` from the FIX repository; nothing in a version's directory is edited by hand.

FIX 4.4's own is in `Fix/Fix44/`:

- `Fix/Fix44/Fix44Context.cs`: the version's context, its standard pairs, checks and loads.
- `Fix/Fix44/FixMessage.cs`, `Fix/Fix44/FixMessage.Header.cs`: the message base, the standard header and trailer.
- `Fix/Fix44/FixMessage.Types.cs`, `Fix/Fix44/FixComponents.cs`, `Fix/Fix44/FixValidator44.cs`,
  `Fix/Fix44/FixStandard.cs`: the 93 message classes, the 24 component interfaces, the check of
  every message type, component and group entry, and the type of every tag.
- `Fix/Fix44/FixValidator44.Fields.cs`: the check of every field against its type and its code set.

FIX 4.2's is in `Fix/Fix42/`, the same files with 42 for 44 and no `FixComponents.cs`: FIX 4.2
writes its groups inline and names no component. FIX 5.0 SP2's is in `Fix/Fix50/`, over FIXT 1.1's
header, trailer and session messages.

A group is named for its counter: the class `<Counter>Group`, nested in whatever carries it —
a message, a component's interface or another group's entry — and its entries are the list
`<Counter>Groups` beside the counter: `order.NoPartyIDsGroups`, of `IParties.NoPartyIDsGroup`.
`FieldCases.json` holds field IDs and code-value regression cases;
`Fixtures.json` holds message test inputs. Maintain both alongside the definitions.
DotGram compiles `.gram` files during builds.

Tests and BenchmarkDotNet workloads are separate solution projects. The coverage
and measurement records are in `docs/design/finance-fix44.md` and
`benchmarks/DotGram.Finance.Benchmarks/README.md` in the source repository.

### Field construction and locations

`FixGrammar` parses the tag and selects one branch through `switch`.
`FixFieldBuilder.cs` constructs the field of the tag's type in C#.
`Field` reads the field contents. `Fields` repeats a constructing group that adds
the separator or EOF and records the actual separator length. Publications support
eager and `yield` parsing.

The `Fix44Grammar` fixture inherits `FixFieldGrammar`, whose
`FixField.gram` contains one alternative per standard field. Tests compare
its results with the production parser using the same shared field model.

`FixField.cs` contains the field base, `FixField.Typed<T>` and a class a value type.
The base classes implement locations and typed-value access; the classes contain no
conversion or location logic.

The builder constructs a field of the tag's type, for example
`new FixField.Integer(FixTag.LegProduct, value.ToInteger())`. Primitive conversions return
`(Valid, Value)` for the field constructor. Plain text conversion returns a string
without a validation flag; a string's typed value is always available. Restrictions
on a particular field (such as currency syntax or a code set) remain semantic checks.

`LocationType = typeof(IFixLocation)` supplies field coordinates through `Locate`.
The constructing group in `Fields` covers the complete tag, equals sign, value
and optional final separator; it supplies the field's source extent. `Position`, `ValuePosition` and
`Length` mean here what they mean everywhere else on this page, unknown and binary
fields included.

## Attribution

The tables of each FIX version — its fields, their types and code sets, its messages and
components — are derived from the FIX Protocol specification (FIX Unified Repository, 2010
edition). The FIX Protocol specification is Copyright FIX Protocol Limited,
<https://www.fixtrading.org>. This package's code is under the MIT licence, as the rest of DotGram is; the
attribution says where the data came from and grants or restricts nothing.
