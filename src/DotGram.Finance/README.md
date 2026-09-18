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
using DotGram.Finance.Fix;

FixField[] fields = FixParser.Parse(wire);
var logFields = FixParser.ParseLog("55=ABC | 38=100");
using var input = File.OpenRead("messages.fix");
foreach (FixField field in FixParser.Parse(input))
    Console.WriteLine(field.Tag);

// Explicit, optional semantics; reuses the already parsed field objects.
FixMessage message = FixMessages.Build(wire, fields);
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
completed field; `maxRetained` must accommodate the whole binary pair.
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
foreach (var field in FixParser.ParseLog("55=ABC|broken|38=2"))
{
    if (field is FixField.Invalid invalid)
        Console.WriteLine($"{invalid.Position}: {invalid.Message}: {invalid.RawText}");
}
```

Valid binary payloads are consumed by length, including embedded separators.
After a malformed binary header or length, separator recovery is best effort:
the next separator may be inside damaged payload data. Primitive conversion
failures keep their existing typed field with `IsValid == false`; `recover`
handles recognition failures, not semantic validation.
Use `.ToArray()` when a complete list is needed. String/span/byte-array overloads
materialize the complete result. Empty input returns no fields.
Concatenated messages are read as one ordered field sequence.

`FixParser.Parse` reads SOH-delimited wire input. `FixParser.ParseLog` reads logs
with bare `|`, spaced ` | `, or a mixture. Both names support strings, character
spans, byte arrays, `TextReader`, and byte `Stream`; stream overloads are lazy.
`FixFieldOptions` configures only replacement Length/Data pairs, not the delimiter.

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
`FixParser.Parse` returns the completed field sequence, including `Invalid` fields.
Use `FixMessages.TryParse` or `FixMessages.TryBuild` for validation; both reject
recovered syntax errors in strict and lenient modes with the first syntax diagnostic. Typed values own their data;
no complete source string is retained by a field. Character-span input is copied
for recognition; native byte-stream parsing creates no complete character view.

## Computed dispatch parser

`FixParser` is the main parser, with string, character-span, byte-array, `TextReader`
and byte `Stream` input forms. Its small [grammar](Fix/FixGrammar.gram) reads a
numeric tag and uses `switch` to select text or a length/data pair. C# supplies
classification and typed field construction.

The large `Fix44` grammar is retained in
`examples/DotGram.Examples/Finance/Fix44` for regression tests and benchmarks.
It is not included in the Finance package.

```csharp
var fields = FixParser.ParseLog("55=ABC|38=100|");
var options = new FixFieldOptions(new Dictionary<int, int>
{
    [95] = 96,
    [5000] = 5001,
});
var custom = FixParser.ParseLog("5000=3 | 5001=a|b | ", options);
```

A supplied length/data dictionary **replaces** the standard pairs and is copied
at construction. Omit it to use the standard dictionary. `FixParseOptions` takes the same object for
message parsing. Each length tag must
immediately precede its configured data tag. The pair produces one binary field;
unknown data tags produce `FixField.Unknown` with binary metadata. Standalone data
tags are rejected. The parser recognizes binary boundaries; message and business
validation remain in the explicitly called semantic API.

## Explicit message semantics

The following APIs belong to `FixMessages` and run only when called explicitly.
This layer restores separate length nodes for the wire message model; that work
is not part of flat field parsing.
`Build(source, fields)` requires fields parsed from that exact source and performs
validation and group assembly without parsing it again. `Parse` combines both steps.

```csharp
using DotGram.Finance.Fix;

if (FixMessages.TryParse(wire, out var message, out var error))
{
    if (message is NewOrderSingle order)
    {
        Console.WriteLine(order.Symbol);
        Console.WriteLine(order.OrderQty);
        Console.WriteLine(order.Header.SenderCompID);

        foreach (var party in order.Parties)
            Console.WriteLine(party.PartyID);
    }
}
else
{
    Console.WriteLine($"{error!.Position}: tag {error.Tag}, " +
                      $"MsgType {error.MessageType}: {error.Reason}");
}
```

`FixMessages.Parse(wire)` returns the same model and throws `FormatException` on malformed
input. `TryParse` returns false and leaves `message` null. Null input also returns
false in `TryParse`; invalid options passed to an options overload are programming
errors. String and `ReadOnlySpan<char>` overloads accept one complete message.
Concatenated messages are rejected by the contiguous-input overloads. Use
`ReadMessages` to consume a sequence from a reader or stream.

## Streaming input

```csharp
using var input = File.OpenRead("messages.fix");
foreach (var message in FixMessages.ReadMessages(input))
    Console.WriteLine(message.MessageType);

// Alternatively, read exactly one frame from a fresh stream:
using var single = File.OpenRead("messages.fix");
var next = FixMessages.Parse(single, maxMessageLength: 4 * 1024 * 1024);
```

`Parse`, `TryParse`, and `ReadMessages` accept either `TextReader` or `Stream`,
with a `FixParseMode` or `FixParseOptions`. Byte streams use the generated native
byte machine and `ReadOnlySpan<byte>` conversion hooks. Character readers preserve
the lossless octet mapping described below. Non-seekable inputs and short reads are supported. These APIs are
synchronous and leave the input open, including when enumeration stops early.

The adapter frames messages using `BodyLength`, then performs the same complete
checksum, grammar, schema, raw-data and group validation as the string API.
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
field conversion does not transcode numeric input. The existing source-owning model
also creates a lossless character view after recognition for validation and
`OriginalWire`. This is not a zero-copy API. Results own their data independently
of subsequent stream reads. See the finance benchmarks for total parsing costs.

## Input and ownership

The input is a **lossless octet string**: every character represents one octet,
U+0000 through U+00FF. The default delimiter is SOH (`\u0001`). Use `ParseLog`
or `new FixParseOptions(FixFraming.Log)` for pipe-delimited logs. Characters above U+00FF are rejected. `BodyLength` and
`CheckSum` therefore count exactly the octets present on the wire, including raw
and encoded data. Decode a wire file with Latin-1, not UTF-8; an Encoded field's
payload remains opaque and its declared `MessageEncoding` remains available.

String input is retained without copying. Span input is copied once because the
returned model owns its source. Keeping a field or message alive retains that
source. Networking and FIX session state are outside this package.

## Model

All 93 standard message types have public classes. Messages, the header and the
trailer have named properties. A group property returns the group's entries, each a
`FixFieldSet` read with `GetField` and `GetGroup`.
Flattened component fields are properties of their containing scope.

- Missing scalar fields return null; absent groups return an empty read-only list.
- Text ADT cases own their converted string. The original named message properties
  remain available; accessing a text projection may allocate another string.
- Numeric properties return `FixNumber?`. Its `Value` preserves all decimal digits;
  `TryGetDecimal` uses .NET's decimal conversion rules. Values outside CLR numeric
  ranges can still be parsed and inspected without overflow or loss of wire text.
- Temporal properties preserve FIX text, including year zero and leap-second
  notation, rather than forcing values into `DateTime`.
- `GetField(tag)` returns the first field in the current scope. `FixFieldView.Value`
  and `FixFieldView.Wire` expose non-allocating spans. `Fields` preserves scope order;
  `AllFields` traverses the complete message, including nested groups, in wire order.
- `OriginalWire` is the exact input. No serializer is necessary to recover it.

## Validation policies

| Check | Strict (default) | Lenient |
| --- | --- | --- |
| Complete framing, BeginString, first three fields, terminal CheckSum | Required | Required |
| BodyLength and CheckSum | Checked | Checked |
| Length/data pairs, including embedded SOH | Checked | Checked |
| Standard group delimiters, boundaries and counts | Checked during semantic assembly | Checked during semantic assembly |
| Explicit schema required fields and component activation | Checked | Relaxed |
| Primitive lexical/calendar syntax and code sets | Checked | Values preserved |
| Group field order and duplicate standard fields | Checked | Order/duplicates preserved |
| Unknown scalar tags | Rejected | Preserved |
| Unknown vendor MsgType | Rejected | `CustomFixMessage` with an ordered flat body |

Body fields may be reordered. Group entries must begin with their schema's first
field in both modes. Unknown scalar tags inside an entry belong to the current
entry until a known delimiter or field establishes the next scope. Without a
vendor schema, their business meaning or a different intended scope cannot be
inferred. The exact wire and offsets are always preserved.

Strict validates the explicit machine-readable schema and wire constraints, including
`MessageEncoding` when Encoded fields occur. It is not a trading or session business
validator: prose-only conditional trading requirements, sequence-number state,
order economics, live ISO registry assignments and announced leap-second dates are
outside its checks. ISO identifiers are checked for their lexical shape.

## Field ADT and typed values

`FixFieldView.TypedValue` is a `FixField` with one concrete `FixField` case per
standard tag. Each case declares its tag and primitive type:

```csharp
var order = (NewOrderSingle)FixMessages.Parse(wire);
var symbol = (FixField.Symbol)order.GetField(55)!.Value.TypedValue!;
var quantity = (FixField.OrderQty)order.GetField(38)!.Value.TypedValue!;
Console.WriteLine(symbol.Value);             // string
Console.WriteLine(quantity.Value);           // decimal
```

| FIX primitive | ADT value |
| --- | --- |
| int, Length, NumInGroup, SeqNum, TagNum, DayOfMonth | BigInteger |
| float, Qty, Price, PriceOffset, Amt, Percentage | decimal |
| char, Boolean | char, bool |
| String, Currency, Country, Exchange | string |
| MultipleValueString | string[] |
| UTCDateOnly, LocalMktDate | FixDate |
| UTCTimeOnly, UTCTimestamp, MonthYear | FixTime, FixTimestamp, FixMonthYear |
| data | ReadOnlyMemory<byte> |

Code sets are validated against the schema tables in Strict mode; their underlying
primitive remains the value type. Dates retain year zero and leap-second notation.
The character numeric hooks use invariant .NET parsing; byte decimal hooks use
UTF-8 decimal parsing with the same FIX syntax checks. Integer values retain
arbitrary precision. Decimal values must fit `System.Decimal` exactly: overflow
and loss of fractional precision set `IsValid` to false rather than rounding.
Trailing fractional zeros do not cause a loss of precision.

The source-backed semantic model retains malformed primitive text in Lenient mode. Such a field has
`TypedValue.IsValid == false`; `TryGetValue` returns false and `Value` throws.
This flag describes primitive conversion, not code-set or message-schema validity.
Unknown tags use `FixField.Unknown` with their original value octets.

## Pipe-delimited logs

```csharp
var message = FixMessages.ParseLog(logLine);
var options = new FixParseOptions(FixFraming.Log, FixParseMode.Lenient);
foreach (var item in FixMessages.ReadMessages(logReader, options))
    Console.WriteLine(item.MessageType);
```

For logs with added presentation spaces, use the flat `FixParser.ParseLog` API.
The message-validation APIs below require the lossless representation: replace each
structural SOH with one pipe without adding formatting spaces, so BodyLength and
CheckSum can still be verified.

The common grammar declares `Separator` and specializes the log publication with
`with (Separator = LogSeparator)`. The generator recognizes the guarded text run
`(?!Separator & any)+` and emits a linear scan for these delimiters.
Only structural SOH separators are rendered as pipes. Raw-data payload octets must
remain untouched; a log that replaces or escapes payload bytes is not lossless and
requires its own decoding before this API. Arbitrary log prefixes are not accepted.
BodyLength is unchanged. CheckSum is verified against the original SOH representation
by normalizing only the recognized field delimiters, never pipes inside raw data.
`OriginalWire` preserves the supplied log representation.

## Custom fields

Unknown tags are read as delimiter-terminated text unless their binary length/data
pair is configured through `FixFieldOptions` as described above.

## Definition maintenance

Field declarations, model types, schema tables, the example field grammar and test
fixtures are maintained manually. When changing definitions, update the affected
factory cases, schema entries, models, example grammar and test cases together.

- `Fix/FixField.cs`: field base, typed-value access, locations and typed field cases.
- `Fix/FixFieldFactory.cs`: construction of typed fields.
- `Fix/FixMessageTypes.cs`: message models and the MsgType table that constructs them.
- `Fix/FixSchema.cs`: field types, code sets and message/group definitions.

The definitions cover 912 fields, 247 code sets, 15 components and 92 group
definitions; 91 groups are reachable from the 93 standard messages.
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

The example `Fix44Grammar` inherits `FixFieldGrammar`, whose
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
`Length` retain their existing meanings, including for unknown and binary fields.
