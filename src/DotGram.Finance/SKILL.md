---
name: dotgram-finance
description: Read FIX 4.2, FIX 4.4 and FIX 5.0 SP2 tag-value data with DotGram.Finance — flat typed fields from wire or log input, and, where a message must be known correct, validated messages with their repeating groups. Use when a project references the DotGram.Finance package, or when asked to parse FIX tag-value messages or FIX logs in .NET. Not for FIX session handling, sending or composing messages, FIXML, or a FIX version other than 4.2, 4.4 and 5.0 SP2; this package only reads.
---

# DotGram.Finance

Reads FIX 4.2, FIX 4.4 and FIX 5.0 SP2 tag-value input: wire messages whose fields end with SOH, and the
pipe-separated log renderings people keep of them. What every version of FIX shares — the
fields, `FixTag`, `FixConvert`, the findings — is in `DotGram.Finance.Fix`; a version's own —
`FixParser`, the messages, its context — in a namespace of its own with the same names in it:
`DotGram.Finance.Fix.Fix44` with `Fix44Context`, `DotGram.Finance.Fix.Fix42` with `Fix42Context`,
`DotGram.Finance.Fix.Fix50` with `Fix50Context` for FIX 5.0 SP2 over FIXT 1.1 (BeginString `FIXT.1.1`).
The examples here are FIX 4.4's. There is no runtime to deploy, nothing to configure and nothing
to initialize.

**Pick the namespace by the BeginString of the data.** Each version's `FixParser` reads the other's
messages too, since the framing is the same, but builds its own classes and holds them to its own
version: a 4.2 order read through `Fix44` is told its BeginString is wrong and meets 4.4's fields.

The [README][readme] beside this file is the reference. This is the order to decide
things in, and the mistakes that are easy to make.

[readme]: https://github.com/dotgram/dotgram/tree/main/src/DotGram.Finance

## Choose the answer first

`FixParser` is the whole door, and the name of the call says what comes back. Most wrong
code asks for the wrong one of these two.

- **Fields** — `ParseFields` from a buffer, `ReadFields` from a reader or a stream. Every
  field in source order as a typed `FixField`, repeated and unknown tags included. Nothing
  is validated beyond what it takes to find the next field: no message boundaries, no
  groups, no BodyLength, no CheckSum, no required fields. Use it for logs, for pulling a
  few values out, and for anything that must not reject input.
- **Messages** — `ParseMessage` and `ParseMessages` from a buffer of characters or of
  octets, `ReadMessage` and `ReadMessages` from a reader or a stream. Repeating groups are assembled
  and the message's own class comes back; a message that does not end with its CheckSum is
  refused, and a wrong BodyLength or CheckSum is a finding of `Validate`. Use it
  when a message's groups matter, or when its fields are wanted as the typed properties of
  the class it is.

The verb says where the input is and what happens to it: `Parse` takes a buffer whole,
`Read` consumes a reader or a stream and leaves it open. The plural says how many come back.

`FixParser.BuildMessage(source, fields)` joins the two: it builds the message from fields
`ParseFields` already returned, without reading the input again.

## Input

- **A string is octets, not text.** Every character stands for one byte,
  U+0000 through U+00FF. Decode files and sockets with Latin-1
  (`Encoding.Latin1`), never UTF-8: UTF-8 changes the byte count, and `Validate` then
  finds BodyLength and CheckSum wrong.
- **Better: hand over the octets.** `byte[]` and `ReadOnlyMemory<byte>` go to every field
  and message call, and there is then no encoding for anyone to get wrong — the package
  decodes, knowing the specification counts octets. Prefer it wherever the octets are in
  hand. It is not cheaper: the values are materialised one character to one octet either way.
  It is right.
- **Wire or log.** The field calls read SOH-separated fields, and `Fix44Context.WithLogFraming`
  makes them read pipe-separated ones, with or without spaces around the pipe. The same
  value goes to every other call. A message read whole may pad its pipes with spaces;
  streamed messages, cut by their BodyLength, want a bare `|`.
- **Forms.** Fields and messages both take `string`, `byte[]`, `ReadOnlyMemory<byte>`,
  `TextReader` and `Stream`. An array or memory is read where it lies, without a copy.
  There is no span overload: it could only copy the span into an array, so a caller who
  holds one writes `span.ToArray()` and sees the copy. `TextReader` and `Stream` are read
  lazily and left open: dispose them yourself, and enumerate the result once.

## Reading fields

```csharp
using System;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

foreach (var field in FixParser.ParseFields(wire))
{
    switch (field)
    {
        case FixField.Invalid invalid:            // unreadable input, skipped to the next separator
            Console.WriteLine($"{invalid.Position}: {invalid.Message}");
            break;

        case FixField.Decimal { Tag: FixTag.OrderQty, IsValid: true } quantity:
            decimal value = quantity.Value;
            break;

        case FixField.Text { Tag: FixTag.Symbol } symbol:   // text fields are always valid
            string text = symbol.Value;
            break;
    }
}
```

- A field is a class of the type of its value, and its `Tag` says which field it is:
  `FixField.Text`, `Character`, `Boolean`, `Integer`, `Decimal`, `Timestamp`, `Time`, `ZonedTime`,
  `Date`, `MonthYear`, `Multiple` and `Data`, each with a typed `Value` — text as `string`, numbers as
  `decimal` or `long`, dates and times as `DateOnly`, `TimeOnly` and `DateTimeOffset` (a
  MonthYear stays a `string`; FIX 5.0's TZTimeOnly is `(TimeOnly Time, TimeSpan Offset)`, and its
  TZTimestamp a `Timestamp` at the offset it was written with). `Tag` is the tag's number, and `FixTag` holds every version's tags as constants
  named as the FIX repository names them: `FixField.Decimal { Tag: FixTag.OrderQty }`. A message's
  property keeps its version's name where that differs: FIX 4.4's `IOIid` is `FixTag.IOIID`. A tag it does not name is just its number, `25005`.
- **`Value` throws when `IsValid` is false.** A field whose text does not convert —
  `38=abc`, a date of `20261340` — is still returned, with `IsValid` false. Test it
  first, or use `TryGetValue`.
- A tag the package does not know is read as the type a dictionary loaded into the context gives
  it: `Fix44Context.Default.Load(venueXml)`, or read, merge and edit first and then apply:
  `Fix44Context.Default.With(FixDictionary.LoadFile(path).Merge(FixDictionary.Parse(venueXml)))`. A MsgType it does not know is built by
  `FixMessageFactory` (a `FixCustomMessage` that places its own fields). A tag nothing defines is a
  `FixField.Invalid` of that tag with its octets, and a type the factory answers null for a
  `FixMessage.Invalid`. A standard message has no property for a tag outside FIX 4.4, so such a
  field is out of scope in it.
- A syntax error does not throw. It becomes one `FixField.Invalid`, and reading
  resumes after the next separator.

## Reading messages

```csharp
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

if (!FixParser.TryParseMessage(wire, out var message, out var error))
{
    Console.WriteLine(error);                     // message type, tag, offset and reason
    return;
}

switch (message)
{
    case FixMessage.ExecutionReport report:
        char? status = report.OrdStatus?.Value;   // null when the field is absent
        if (report.LastPx is { IsValid: true } price)
            Console.WriteLine(price.Value);
        break;

    case FixMessage.NewOrderSingle order:
        foreach (var party in order.NoPartyIDsGroups ?? [])   // the entries, in order
            Console.WriteLine(party.PartyID.Value);
        break;
}
```

- The 93 standard messages are the cases of `FixMessage`, nested in it: write
  `FixMessage.NewOrderSingle`, and a `switch` over them reads as the closed set it is. A
  MsgType the schema does not know is the consumer's `FixCustomMessage` or a
  `FixMessage.Invalid`, carrying the type it read and its fields — that case is why the set can
  be closed without covering every MsgType that exists.
- A message's properties are named after its fields and are the typed fields themselves:
  `order.Symbol` is a `FixField.Text?`, `order.OrderQty` a `FixField.Decimal?`, null when the field is absent. `Value` on one is
  the CLR value — a `string`, a `decimal`, a `long` — and `IsValid` says whether the
  characters fitted it. A repeating group is named for its counter: the list
  `<Counter>Groups` beside its counter, of entries of the class `<Counter>Group` nested in
  what carries it — `order.NoPartyIDsGroups`, of `IParties.NoPartyIDsGroup` — and an entry
  reads the same way.
- The standard header and trailer are properties of every message, and `Fields` is the whole
  message in wire order, the fields of every group included.
- `ParseMessage` throws `FormatException`; `TryParseMessage` returns false with a
  `FixParseError` saying where and why. The same pair for readers and streams is
  `ReadMessage` and `TryReadMessage`.

## Binary data

A length/data pair — `95=5` then `96=` and five octets — is read by its length, so the
payload may contain separators. It comes back as two fields, the length and the data, each
with its own position. Counterparty-defined pairs go in `Fix44Context`:

```csharp
using System.Collections.Generic;

var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

var context = new Fix44Context
{
    LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 },   // length tag to data tag, added to the standard's sixteen
};

var fields  = FixParser.ParseFields(wire, context);
var message = FixParser.ParseMessage(wire, context);   // the same value reads both layers and holds the message to its schema
```

The dictionary **adds to** the standard's sixteen pairs: list only what the standard does
not define. A pair's tags that nothing gives a type are read as a `FixField.Integer` and a
`FixField.Data`. The same object goes to the message calls, which refuse a length not followed by
its data. When reading a stream, the context's `MaxRetained` bounds one field, from its tag
through the separator that ends it: 16 Mi characters from a `TextReader` or bytes from a
`Stream` by default. A field that needs more throws `IOException`, so give the context a larger
`MaxRetained` for large binary data: `context with { MaxRetained = 64 << 20 }`. `BufferSize`
is the size of the buffer the input is read through.

## Streams

`FixParser.ReadFields(Stream)` and `FixParser.ReadFields(TextReader)` return a lazy
`IEnumerable<FixField>`. `FixParser.ReadMessages` returns a lazy
`IEnumerable<FixMessage>`, one message at a time, each at most `maxMessageLength`,
16 MiB by default. Neither closes its input.

## Mistakes to avoid

1. Decoding input as UTF-8. Use Latin-1.
2. Reading `Value` from a field without checking `IsValid`.
3. Expecting a message call to refuse a wrong BodyLength or CheckSum. It builds the message;
   `Validate` reports `BodyLengthMismatch` and `CheckSumMismatch`.
4. Streaming a space-padded log as messages. A stream is cut by BodyLength and wants a bare `|`.
5. Writing a pair's data tag without its length right before it. The field calls return it
   as a `FixField.Invalid`: only a length says where the data ends.
