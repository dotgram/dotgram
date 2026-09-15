# DotGram.Finance

A standalone FIX 4.4 tag-value parser for `netstandard2.0` and `net10.0`.
DotGram generates one shared field grammar at build time; applications need no DotGram runtime,
grammar files, schema XML, reflection configuration or initialization step.

```csharp
using DotGram.Finance.Fix;

if (Fix44.TryParse(wire, out var message, out var error))
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

`Fix44.Parse(wire)` returns the same model and throws `FormatException` on malformed
input. `TryParse` returns false and leaves `message` null. Null input also returns
false in `TryParse`; invalid options passed to an options overload are programming
errors. String and `ReadOnlySpan<char>` overloads accept one complete message.
Concatenated messages are rejected by the contiguous-input overloads. Use
`ReadMessages` to consume a sequence from a reader or stream.

## Streaming input

```csharp
using var input = File.OpenRead("messages.fix");
foreach (var message in Fix44.ReadMessages(input))
    Console.WriteLine(message.MessageType);

// Alternatively, read exactly one frame from a fresh stream:
using var single = File.OpenRead("messages.fix");
var next = Fix44.Parse(single, maxMessageLength: 4 * 1024 * 1024);
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
or `new FixParseOptions('|')` for pipe-delimited logs. Characters above U+00FF are rejected. `BodyLength` and
`CheckSum` therefore count exactly the octets present on the wire, including raw
and encoded data. Decode a wire file with Latin-1, not UTF-8; an Encoded field's
payload remains opaque and its declared `MessageEncoding` remains available.

String input is retained without copying. Span input is copied once because the
returned model owns its source. Keeping a field or message alive retains that
source. Networking and FIX session state are outside this package.

## Model

All 93 standard message types have public classes. Header, trailer and repeating
group entries have named properties, including typed nested group collections.
Flattened component fields are properties of their containing scope.

- Missing scalar fields return null; absent groups return an empty read-only list.
- Text ADT cases own their converted string. The original named message properties
  remain available; accessing a text projection may allocate another string.
- Numeric properties return `FixNumber?`. Its `Value` preserves all decimal digits;
  `TryGetDecimal` uses .NET's decimal conversion rules. Values outside CLR numeric
  ranges can still be parsed and inspected without overflow or loss of wire text.
- Temporal properties preserve FIX text, including year zero and leap-second
  notation, rather than forcing values into `DateTime`.
- `GetField(tag)` returns the first field in the current scope. `FixField.Value`
  and `FixField.Wire` expose non-allocating spans. `Fields` preserves scope order;
  `AllFields` traverses the complete message, including nested groups, in wire order.
- `OriginalWire` is the exact input. No serializer is necessary to recover it.

## Validation policies

| Check | Strict (default) | Lenient |
| --- | --- | --- |
| Complete framing, BeginString, first three fields, terminal CheckSum | Required | Required |
| BodyLength and CheckSum | Checked | Checked |
| Length/data pairs, including embedded SOH | Checked | Checked |
| Standard group delimiters, boundaries and counts | Checked during recognition | Checked during recognition |
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

`FixField.TypedValue` is a `FixValue` with one concrete `FixFields` case per
standard tag. The tag and primitive type come from the pinned specification:

```csharp
var order = (NewOrderSingle)Fix44.Parse(wire);
var symbol = (FixFields.Symbol)order.GetField(55)!.Value.TypedValue!;
var quantity = (FixFields.OrderQty)order.GetField(38)!.Value.TypedValue!;
Console.WriteLine(symbol.Value);             // string
Console.WriteLine(quantity.Value.Coefficient); // BigInteger
Console.WriteLine(quantity.Value.Scale);       // decimal scale
```

| FIX primitive | ADT value |
| --- | --- |
| int, Length, NumInGroup, SeqNum, TagNum, DayOfMonth | BigInteger |
| float, Qty, Price, PriceOffset, Amt, Percentage | FixDecimal: exact coefficient and scale |
| char, Boolean | char, bool |
| String, Currency, Country, Exchange | string |
| MultipleValueString | string[] |
| UTCDateOnly, LocalMktDate | FixDate |
| UTCTimeOnly, UTCTimestamp, MonthYear | FixTime, FixTimestamp, FixMonthYear |
| data | ReadOnlyMemory<byte> |

Code sets are validated against the specification in Strict mode; their underlying
primitive remains the value type. Dates retain year zero and leap-second notation.
The char numeric hooks use invariant .NET parsing. Byte numeric hooks accumulate
ASCII digits directly, retaining arbitrary integer and decimal precision.
`FixDecimal.TryGetDecimal` succeeds only when the value is exactly representable.

Lenient parsing retains malformed primitive text. Such a field has
`TypedValue.IsValid == false`; `TryGetValue` returns false and `Value` throws.
This flag describes primitive conversion, not code-set or message-schema validity.
Unknown tags use `UnknownFixValue` with their original value octets.

## Pipe-delimited logs

```csharp
var message = Fix44.ParseLog(logLine);
var options = new FixParseOptions('|', FixParseMode.Lenient);
foreach (var item in Fix44.ReadMessages(logReader, options))
    Console.WriteLine(item.MessageType);
```

The common grammar declares `Separator` and specializes the log publication with
`with (Separator = LogSeparator)`. Text termination changes with that rule too.
Only structural SOH separators are rendered as pipes. Raw-data payload octets must
remain untouched; a log that replaces or escapes payload bytes is not lossless and
requires its own decoding before this API. Arbitrary log prefixes are not accepted.
BodyLength is unchanged. CheckSum is verified against the original SOH representation
by normalizing only the recognized field delimiters, never pipes inside raw data.
`OriginalWire` preserves the supplied log representation.

## Vendor data fields

An unknown data field cannot safely be split at SOH. Register its length/data tags
before parsing; the immutable options can be reused concurrently:

```csharp
var options = new FixParseOptions(
    FixParseMode.Lenient,
    new FixDataPair(lengthTag: 9000, dataTag: 9001));

var message = Fix44.Parse(wire, options);
var payload = message.GetField(9001)!.Value.Value;
```

Registered pairs are also accepted and validated in Strict. Tags must be positive,
unique and outside the standard schema. Registration does not redefine FIX fields.
Custom group schemas are not inferred or dynamically compiled: an unknown vendor
message preserves their fields flat. Extending the generated typed schema requires
an explicit repository definition and a library build.

## Specification and reproducibility

The source is FIX Trading Community's
[Orchestra FIX 4.4](https://github.com/FIXTradingCommunity/orchestrations/blob/cd24169a2abd8daba7c360987c7a46ca11873a12/FIX%20Standard/OrchestraFIX44.xml),
`FIX.4.4_EP311`, pinned to commit
`cd24169a2abd8daba7c360987c7a46ca11873a12`.
It contains 912 fields, 247 code sets, 15 components and 92 group definitions;
91 groups are reachable from the 93 standard messages. Wire rules follow
[FIX TagValue Encoding](https://www.fixtrading.org/standards/tagvalue-online/).

Run `python tools/generate-fix44.py` from the repository to reproduce checked-in
grammars, model types, schema tables and test fixtures. Generation uses only the
Python standard library and the pinned local XML; package consumers do not run it.
The original source and its Apache 2.0 license remain unmodified. See the packaged
third-party notices for attribution.

Tests and BenchmarkDotNet workloads are separate solution projects. The coverage
and measurement records are in `docs/design/finance-fix44.md` and
`benchmarks/DotGram.Finance.Benchmarks/README.md` in the source repository.
