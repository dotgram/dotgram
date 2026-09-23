<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — which layer to use, how to read fields and messages, and the mistakes
  that are easy to make. Read it before writing code against the package. In a
  restored package that is ~/.nuget/packages/dotgram.finance/<version>/SKILL.md.
-->

# DotGram.Finance

A standalone FIX 4.4 tag-value parser for `netstandard2.0` and `net10.0`.
DotGram compiles the handwritten dispatch grammar at build time; applications need no DotGram runtime,
grammar files, schema XML, reflection configuration or initialization step.

## Flat field parsing

```csharp
using System;
using System.IO;

using DotGram.Finance.Fix44;

// One message, with the separator written as a pipe so it can be read on a page.
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

// One door, and the verb says where the input is: Parse takes a buffer whole, Read consumes
// a reader or a stream. The plural says how many come back.
FixMessage message = FixParser.ParseMessage(wire);

FixField[] fields = FixParser.ParseFields(wire);
var logFields = FixParser.ParseFields("55=ABC | 38=100", FixContext.WithLogFraming);

using var input = File.OpenRead("messages.fix");

foreach (FixField field in FixParser.ReadFields(input))
    Console.WriteLine(field.Tag);
```

`FixParser` returns fields in source order, including repeated and unknown tags. It does
not assemble messages or groups, check required fields, code sets, BodyLength or
CheckSum. A failed primitive conversion sets `FixField.IsValid` to false; it does
not reject the field, except that a binary length must be valid to find the next
field boundary. Text values need no conversion validity check.

String, character-span, `TextReader`, byte-array and `Stream` inputs are supported.
`Parse(TextReader)` and `Parse(Stream)` return a lazy `IEnumerable<FixField>`.
The grammar directly yields `FixField`. Ordinary fields are returned immediately;
a binary length/data pair produces one typed data field after its payload is read.
For example, `95=3|96=a|b|` produces one `FixField.RawData`, without a separate
`RawDataLength` result. Its `Position` points to the start of the pair and
`ValuePosition`/`Length` describe the payload. Native char/byte buffers release each
completed field. `maxRetained` bounds one field, from its tag through the separator that
ends it, or a whole binary pair, in characters from a reader or bytes from a stream:
`FixParser.DefaultMaxRetained`, 16 Mi of either, unless given. A field that needs more
throws `IOException`; pass a larger `maxRetained` to read it.
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
foreach (var field in FixParser.ParseFields("55=ABC|broken|38=2", FixContext.WithLogFraming))
{
    if (field is FixField.Invalid invalid)
        Console.WriteLine($"{invalid.Position}: {invalid.Message}: {invalid.RawText}");
}
```

Valid binary payloads are consumed by length, including embedded separators.
After a malformed binary header or length, separator recovery is best effort:
the next separator may be inside damaged payload data. Primitive conversion
failures still come back as the tag's own typed field, with `IsValid == false`; `recover`
handles recognition failures, not semantic validation.
Use `.ToArray()` when a complete list is needed. String/span/byte-array overloads
materialize the complete result. Empty input returns no fields.
Concatenated messages are read as one ordered field sequence.

`FixParser.ParseFields` reads SOH-delimited wire input; given `FixContext.WithLogFraming` it reads logs
with bare `|`, spaced ` | `, or a mixture. Both names support strings, character
spans, byte arrays, `TextReader`, and byte `Stream`; stream overloads are lazy.
`FixContext` declares which framing to read and any length/data pairs of your own; it is the
one value a reading is done by and the one `Validate` holds a message to, and `FixContext.Default`
reads the wire by the standard alone.

The log grammar uses `LogSeparator = ' '* & '|' & ' '*`. ASCII spaces immediately
before or after a pipe belong to that separator. Spaces inside text values are
preserved; spaces before EOF are also preserved when no pipe follows. Length-delimited
binary payloads are never trimmed, even when they contain ` | ` or end in spaces.
Field positions refer to the original input, including its formatting spaces.
A streamed log field reads ahead through the padding to the next character or
EOF before yielding. Wire parsing can yield as soon as SOH is read.
Raw data and its immediately preceding Length field form one grammar rule.
The parser requires the correct tag pair and consumes exactly the declared number
of data bytes, including any delimiter bytes inside the payload. An orphaned
length or data field is rejected. The final field may end at EOF without a separator. Separators between fields
remain required; the declared binary length still determines the complete payload.
`FixParser.ParseFields` returns the completed field sequence, including `Invalid` fields.
Use `FixParser.TryParseMessage` or `FixParser.TryBuildMessage` to build a message; both refuse a
recovered syntax error with the first syntax diagnostic, whatever the framing. Typed values own their data;
no complete source string is retained by a field. Character-span input is copied
for recognition; native byte-stream parsing creates no complete character view.

## Computed dispatch parser

`FixParser` is the main parser, with string, character-span, byte-array, `TextReader`
and byte `Stream` input forms. Its small [grammar](Fix/FixGrammar.gram) reads a
numeric tag and uses `switch` to select text or a length/data pair. C# supplies
classification and typed field construction.

The large `Fix44` grammar is retained in
`tests/DotGram.Finance.Fix44` for regression tests and benchmarks.
It is not included in the Finance package.

```csharp
using System.Collections.Generic;

var fields  = FixParser.ParseFields("55=ABC|38=100|", FixContext.WithLogFraming);
var context = new FixContext
{
    LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 },   // the standard's own sixteen pairs hold as well, and may not be redeclared
};
var custom  = FixParser.ParseFields("5000=3 | 5001=a|b | ", context with { Framing = FixFraming.Log });
```

A supplied length/data dictionary **adds to** the standard's sixteen pairs and is copied
at construction; the standard's own pairs always hold, so neither tag of a supplied pair may be one
the standard already defines. Omit it to use the standard pairs alone. The same object is what
the message calls take, so one value describes both kinds of answer. Each length tag must
immediately precede its configured data tag. The pair produces one binary field;
a data tag the package does not define is built by `FixFieldFactory`, or is a `FixField.Invalid`
of that tag carrying the payload. Standalone
data tags are rejected. The parser recognizes binary boundaries; message and business
validation remain in the explicitly called semantic API.

## Explicit message semantics

The message calls run only when called explicitly.
A length/data pair is one field to `FixParser`; the message model holds the length
and the data as two nodes, and this is the layer that separates them.
`Build(source, fields)` requires fields parsed from that exact source and performs
recognition and group assembly without parsing it again. `Parse` combines both steps.
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
false in `TryParseMessage`; a context that cannot be made — a framing that is neither wire nor log, a pair the standard defines — is a programming
errors. The `string`, `ReadOnlySpan<char>`, `byte[]` and `ReadOnlySpan<byte>` overloads accept one
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

`Parse`, `TryParse`, and `ReadMessages` accept either `TextReader` or `Stream`,
with an optional `FixContext`. Byte streams use the generated native
byte machine and `ReadOnlySpan<byte>` conversion hooks. Character readers preserve
the lossless octet mapping described below. Non-seekable inputs and short reads are supported. These APIs are
synchronous and leave the input open, including when enumeration stops early.

The adapter frames messages using `BodyLength`, then performs the same complete
checksum, grammar, raw-data and group recognition as the string API. Checking a
message against the schema is a separate call: see **Validation** below.
It reuses a growing buffer across `ReadMessages` iterations and creates an owned
source string for each result. Buffering is bounded by the largest frame seen,
not the length of the stream. Keeping all returned messages also keeps all their
source strings and models alive. The default `maxMessageLength` is 16 MiB per
message, including header and trailer; callers can set a different positive
limit. This is a frame-size limit, not a total allocation budget.

Clean EOF ends `ReadMessages`; EOF before a complete frame is an error.
`TryParse` returns false with a diagnostic for malformed, oversized, truncated,
or empty input. `Parse` and `ReadMessages` throw `FormatException` for these
errors (except clean EOF for enumeration). Diagnostic positions are relative to
the current frame. I/O exceptions propagate. A failed parse may consume input;
there is no automatic resynchronization or rollback of the underlying stream.

The framing adapter prevents read-ahead from consuming the next message. The byte
path retains a byte frame and passes it to the generated buffered byte parser;
field conversion does not transcode numeric input. A result owns its source, and a
lossless character view is built after recognition for validation and
`OriginalWire`. This is not a zero-copy API. Results own their data independently
of subsequent stream reads. See the finance benchmarks for total parsing costs.

## Input and ownership

The input is a **lossless octet string**: every character represents one octet,
U+0000 through U+00FF. The default delimiter is SOH (`\u0001`); `FixContext.WithLogFraming`
reads pipe-delimited logs. Characters above U+00FF are rejected. `BodyLength` and
`CheckSum` therefore count exactly the octets present on the wire, including raw
and encoded data. Decode a wire file with Latin-1, not UTF-8; an Encoded field's
payload remains opaque and its declared `MessageEncoding` remains available.

**Or hand over the octets and make no claim at all.** Every field and message call also takes
`byte[]` and `ReadOnlySpan<byte>`: `ParseFields`, `ParseMessage`, `TryParseMessage` and
`ParseMessages`. There the package decodes, knowing that the specification counts octets, so
nothing depends on the caller having chosen an encoding. What this buys is correctness, not
allocation: a message keeps its source, so the octets are still materialised one character to one
octet, and the cost is about what the string road costs.

String input is retained without copying. Span and octet input are materialised once because the
returned model owns its source; `ParseFields(byte[])` reads the array where it lies and builds no
model, so nothing is copied there. Keeping a field or message alive retains that
source. Networking and FIX session state are outside this package.

## Model

The 93 standard message types are the cases of `FixMessage`, nested in it and written
`FixMessage.NewOrderSingle`: a closed set, so a `switch` over it reads as one and the base
type stands in front of every arm. A MsgType the schema does not describe is built by the
context's `FixMessageFactory`, or is a `FixMessage.Invalid`, which is what lets the set be closed
without being complete.
Messages, the header and the trailer have named properties. A group property returns the group's entries, each a
`FixFieldSet` read with `GetField` and `GetGroup`.
Flattened component fields are properties of their containing scope.

- Missing scalar fields return null; absent groups return an empty read-only list.
- Text ADT cases own their converted string. The original named message properties
  remain available; accessing a text projection may allocate another string.
- Numeric properties return `FixNumber?`. Its `Value` preserves all decimal digits;
  `TryGetDecimal` uses .NET's decimal conversion rules. Values outside CLR numeric
  ranges can still be parsed and inspected without overflow or loss of wire text.
- Temporal properties are `DateOnly`, `TimeOnly` and `DateTimeOffset`; on
  netstandard2.0 and .NET Framework the first two come from the
  `Portable.System.DateTimeOnly` package, under the same names.
- `GetField(tag)` returns the first field in the current scope. `FixFieldView.Value`
  and `FixFieldView.Wire` expose non-allocating spans. `Fields` preserves scope order;
  `AllFields` traverses the complete message, including nested groups, in wire order.
- `OriginalWire` is the exact input. No serializer is necessary to recover it.

## Recognition and validation

Reading the wire and holding the result to a schema are two acts, and they refuse at
different times for different reasons.

**Recognition refuses, because after it the input's meaning is unknown.** Complete framing,
`BeginString` and the first three fields, the terminal `CheckSum`, `BodyLength`, a
length/data pair and the octets it measures including an embedded SOH, a `NumInGroup` that
would size an array past the fields left, a group entry that does not begin with its
delimiter, and a field the parser could not read at all. None of these can wait for a
message to exist: they are how the reader finds where one ends.

**Everything else is a finding about a message that was built.** `message.Validate()` holds
it to FIX 4.4 and answers with all of them at once:

| What it reports | |
| --- | --- |
| `RequiredFieldMissing` | a field the schema requires in that scope is absent |
| `RequiredComponentMissing` | a required component has none of its fields; the sentence names the first tag it would have held |
| `InvalidValue` | the value does not fit the tag's type, or is not one of its code set |
| `DuplicateField` | a tag appears twice in one scope |
| `FieldNotInScope` | the schema defines no such tag, or defines it and not there |
| `FieldOutOfOrder` | a group entry's fields are not in the schema's order |
| `GroupCountMismatch` | a required group announces no entries |
| `UnknownMessageType` | the schema describes no message of that `MsgType` |
| `MessageEncodingMissing` | an `Encoded` field is present and tag 347 is not |

Each finding names its rule, its scope, its tag, the entry of the repeating group it is in,
and where in the source it begins — "tag 448 is wrong" says nothing where a message carries
nine parties.

Body fields may be reordered, and an unknown tag between the header and the trailer belongs
to the body rather than ending it: whether it should be there is a finding, and a message the
reader will not build is a message nothing can report on. The exact wire and every offset are
preserved whatever is found.

**A port from a library that stops at the first problem needs looking at.** Code written
against one — where validation throws the first thing wrong, or returns it — handles one
problem per message, and against `Validate()` it keeps the first finding and drops the rest
without a word. `Validate()` answers "is it valid" as `Length == 0`, and
`message.Validate().FirstOrDefault()` is that older shape written out, if it is what you
want. A finding is the same story: where a message carries nine parties, `GroupTag`,
`EntryIndex` and `Position` are what say which one, and code that reads `Tag` alone throws
that away.

**The rule lives in the class, because a message type and a class are the same thing here.**
`Validate()` asks the `Rule` field of the class the message is; replacing a rule is assigning to
that field — `FixMessage.NewOrderSingle.Rule = (message, findings) => …` — and undoing it is
assigning the name back, `FixValidator.ValidateNewOrderSingle`. `FixValidator` is not an entry
point: it holds the ninety-four rules this package compiles in, one named method a type, so that
a replacement can be taken back. Whether a rule is still in place is asked with `==` rather than
`ReferenceEquals`: the two may or may not be one object, and that is the compiler's business.

`FixParser.LoadDictionary(stream)` reads a counterparty's QuickFIX dictionary and writes the rule
of every type it describes. It answers with nothing and throws where the file is not a dictionary
it accepts, because validation holding half of one schema and half of another is worse than a
refusal at the door. A type this package has no class for keeps no rule of its own and stays
unknown; what to do about such a type is to write its class. The package ships nobody's
dictionary: the file is yours, in your repository, and its licence obligations are yours with it.

One field a class means **one configuration for the process**: two counterparties with two
schemas at once is not expressible, and that is the trade this shape was chosen for.

It is not a trading or session validator. Prose-only conditional requirements,
sequence-number state, order economics, live ISO registry assignments and announced
leap-second dates are outside it; ISO identifiers are checked for their lexical shape.

## Field ADT and typed values

`FixFieldView.TypedValue` is a `FixField` with one concrete `FixField` case per
standard tag. Each case declares its tag and primitive type:

```csharp
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

var order = (FixMessage.NewOrderSingle)FixParser.ParseMessage(wire);
FixField.Symbol symbol = order.Symbol!;
FixField.OrderQty quantity = order.OrderQty!;
Console.WriteLine(symbol.Value);             // string
Console.WriteLine(quantity.Value);           // decimal
```

| FIX primitive | ADT value |
| --- | --- |
| int, Length, NumInGroup, SeqNum, TagNum, DayOfMonth | long |
| float, Qty, Price, PriceOffset, Amt, Percentage | decimal |
| char, Boolean | char, bool |
| String, Currency, Country, Exchange | string |
| MultipleValueString | string[] |
| UTCDateOnly, LocalMktDate | DateOnly |
| UTCTimeOnly | TimeOnly |
| UTCTimestamp | DateTimeOffset, at offset zero |
| MonthYear | string, checked for its shape (`YYYYMM`, `YYYYMMDD`, `YYYYMMw1`..`w5`) |
| data | ReadOnlyMemory<byte> |

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

The source-backed semantic model retains malformed primitive text. Such a field has
`TypedValue.IsValid == false`; `TryGetValue` returns false and `Value` throws.
This flag describes primitive conversion, not code-set or message-schema validity.
A tag the package does not define is built by the context's `FixFieldFactory`, typed as the
consumer likes, or is a `FixField.Invalid` of that tag with its value's octets. A standard message
has no property for such a tag, so it is out of scope there whoever built the field; a message the
consumer builds places it (below).

## Pipe-delimited logs

```csharp
var logLine   = "55=ABC | 38=100";
using var logReader = new StringReader(logLine);

var message = FixParser.ParseMessage(logLine, FixContext.WithLogFraming);
var context = FixContext.WithLogFraming;
foreach (var item in FixParser.ReadMessages(logReader, context))
    Console.WriteLine(item.MessageType);
```

For logs with added presentation spaces, use the flat `FixParser` API.
The message-validation APIs below require the lossless representation: replace each
structural SOH with one pipe without adding formatting spaces, so BodyLength and
CheckSum can still be verified.

The common grammar declares `Separator` and specializes the log publication with
`with (Separator = LogSeparator)`. The generator recognizes the guarded text run
`(?!Separator & any)+` and emits a linear scan for these delimiters.
Only structural SOH separators are rendered as pipes. Raw-data payload octets must
remain untouched; a log that replaces or escapes payload bytes is not lossless and
requires its own decoding before this API. Arbitrary log prefixes are not accepted.
BodyLength counts the wire's octets and the pipe rendering does not change it.
CheckSum is verified against the original SOH representation
by normalizing only the recognized field delimiters, never pipes inside raw data.
`OriginalWire` preserves the supplied log representation.

## Custom fields and messages

A tag or a MsgType FIX 4.4 does not define is built by a factory the context holds, asked with the
tag or the type and answering what to build, or null:

```csharp
using System.Collections.Generic;

var context = new FixContext
{
    FixFieldFactory   = tag  => tag == 25005 ? new Status() : null,
    FixMessageFactory = type => type == "U1" ? new VenueQuote() : null,
    LengthDataPairs   = new Dictionary<int, int> { [25000] = 25001 },
};

sealed class Status() : FixCustomField(25005)
{
    public string? Value { get; private set; }

    // The value as the wire had it; what this returns is the field's IsValid.
    protected override bool Read(ReadOnlySpan<char> value)
    {
        Value = value.ToString();

        return Value is "OPEN" or "CLOSED";
    }
}

sealed class VenueQuote : FixCustomMessage
{
    public Status? Status { get; private set; }

    // Every field of the body in turn, the header's and the trailer's taken already; false is out of scope.
    protected override bool Place(FixField field)
    {
        if (field is not Status status)
            return false;

        Status = status;

        return true;
    }

    protected override void OnValidate(FixContext context)
    {
        if (Status is null)
            AddFinding(new FixFinding(FixRule.RequiredFieldMissing, 25005, 0, null, -1));
    }
}
```

A field is handed its value as characters; bytes arrive as the characters of the same codes unless
the field overrides the byte form, and the payload of a declared length/data pair arrives as memory
through a third form, read as bytes by default. A value may not be kept: copy what is needed. The
factory is asked only of a tag the package has no class for, so a standard tag pays nothing, and
the message layer builds the length half of a declared pair through the same factory.

Where a factory answers null, or there is none, the tag is a `FixField.Invalid` of that tag with its
value's octets in `RawBytes`, and the type is a `FixMessage.Invalid`, not valid from the moment it is
read, its finding `UnknownMessageType`.

## Definition maintenance

Field declarations, message classes, the checks, the Fix44 field grammar and test
fixtures are maintained together. When changing definitions, update the affected
factory cases, message classes, checks, Fix44 grammar and test cases together.

- `Fix44/FixField.cs`: field base, typed-value access, locations and typed field cases.
- `Fix44/FixFieldFactory.cs`: construction of typed fields.
- `Fix44/FixMessage.cs`: the message base, the standard header and trailer.
- `Fix44/FixMessage.Types.cs`, `Fix44/FixComponents.cs`, `Fix44/FixValidators.cs`: the 93 message
  classes, the 24 component interfaces and the check of every message type, component and group
  entry — written by `Fix44/generate.py` from the FIX 4.4 repository, not by hand.
- `Fix44/FixValidators.Fields.cs`: the check of every field against its type and its code set.

A group is named as QuickFIX names it: the class `<Counter>Group`, nested in whatever carries it —
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
`FixFieldFactory.cs` constructs the corresponding typed field in C#.
`Field` reads the field contents. `Fields` repeats a constructing group that adds
the separator or EOF and records the actual separator length. Publications support
eager and `yield` parsing.

The `Fix44Grammar` fixture inherits `FixFieldGrammar`, whose
`FixField.gram` contains one alternative per standard field. Tests compare
its results with the production parser using the same shared field model.

`FixField.cs` contains the field base, `FixField.Typed<T>` and all nested field
cases. The base classes implement locations and typed-value access; case
declarations contain no conversion or location logic. `FixFieldView` provides
access to the original source text.

The C# factory constructs field cases, for example
`new FixField.LegProduct(FixConvert.Integer(value))`. Primitive conversions return
`(Valid, Value)` for the field constructor. Plain text conversion returns a string
without a validation flag; a string's typed value is always available. Restrictions
on a particular field (such as currency syntax or a code set) remain semantic checks.

`LocationType = typeof(IFixLocation)` supplies field coordinates through `Locate`.
The constructing group in `Fields` covers the complete tag, equals sign, value
and optional final separator; it supplies the field's source extent. `Position`, `ValuePosition` and
`Length` mean here what they mean everywhere else on this page, unknown and binary
fields included.
