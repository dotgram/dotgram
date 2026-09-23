---
name: dotgram-finance
description: Read FIX 4.4 tag-value data with DotGram.Finance — flat typed fields from wire or log input, and, where a message must be known correct, validated messages with their repeating groups. Use when a project references the DotGram.Finance package, or when asked to parse FIX tag-value messages or FIX logs in .NET. Not for FIX session handling, sending or composing messages, FIXML, or another FIX version's schema; this package only reads, and only FIX 4.4.
---

# DotGram.Finance

Reads FIX 4.4 tag-value input: wire messages whose fields end with SOH, and the
pipe-separated log renderings people keep of them. Everything is in the
`DotGram.Finance.Fix` namespace. There is no runtime to deploy, nothing to configure
and nothing to initialize.

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
  octets, `ReadMessage` and `ReadMessages` from a reader or a stream. The envelope, BodyLength and CheckSum are
  checked, repeating groups are assembled, and the message's own class comes back. Use it
  when a message's groups matter, or when its fields are wanted as the typed properties of
  the class it is.

The verb says where the input is and what happens to it: `Parse` takes a buffer whole,
`Read` consumes a reader or a stream and leaves it open. The plural says how many come back.

`FixParser.BuildMessage(source, fields)` joins the two: it builds the message from fields
`ParseFields` already returned, without reading the input again.

## Input

- **A string is octets, not text.** Every character stands for one byte,
  U+0000 through U+00FF. Decode files and sockets with Latin-1
  (`Encoding.Latin1`), never UTF-8: UTF-8 changes the byte count, and a message call
  then rejects the input on BodyLength or CheckSum. A character above U+00FF is
  refused.
- **Better: hand over the octets.** `byte[]` and `ReadOnlySpan<byte>` go to every field
  and message call, and there is then no encoding for anyone to get wrong — the package
  decodes, knowing the specification counts octets. Prefer it wherever the octets are in
  hand. It is not cheaper: the model keeps its source, so the octets are materialised one
  character to one octet either way. It is right.
- **Wire or log.** The field calls read SOH-separated fields, and `FixFieldOptions.Log`
  makes them read pipe-separated ones, with or without spaces around the pipe. The same
  value goes to every other call. The message calls accept a bare `|` only; read a log
  padded with spaces with `ParseFields` and `FixFieldOptions.Log`.
- **Forms.** Fields and messages both take `string`, `ReadOnlySpan<char>`, `byte[]`,
  `ReadOnlySpan<byte>`, `TextReader` and `Stream`. The span overloads copy the input
  first, into a string or an array. `TextReader` and `Stream` are read lazily and left
  open: dispose them yourself, and enumerate the result once.

## Reading fields

```csharp
using System;

using DotGram.Finance.Fix;

var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

foreach (var field in FixParser.ParseFields(wire))
{
    switch (field)
    {
        case FixField.Invalid invalid:            // unreadable input, skipped to the next separator
            Console.WriteLine($"{invalid.Position}: {invalid.Message}");
            break;

        case FixField.OrderQty quantity when quantity.IsValid:
            decimal value = quantity.Value;
            break;

        case FixField.Symbol symbol:              // text fields are always valid
            string text = symbol.Value;
            break;
    }
}
```

- Each of the 912 standard tags has its own case, `FixField.<Name>`, with a typed
  `Value`: text as `string`, numbers as `decimal` or `long`, dates and times as
  `FixDate`, `FixTime`, `FixTimestamp` and `FixMonthYear`. `field.FieldType` gives the
  tag as a `FixFieldType`, for code that should not spell numbers.
- **`Value` throws when `IsValid` is false.** A field whose text does not convert —
  `38=abc`, a date of `20261340` — is still returned, with `IsValid` false. Test it
  first, or use `TryGetValue`.
- A tag the package does not know comes back as `FixField.Custom`, its value as bytes.
  Supply a `FixCustomFields` to `FixFieldOptions` to build your own field for such a tag
  instead — three methods, because a field is built from characters, from bytes and from the
  payload of a length/data pair, and answering one of them and not the others gives you two
  parses of one message that disagree. It builds field objects only: a tag outside FIX 4.4 has
  no property of a message to sit in, and is carried in `Fields` like any other.
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
        foreach (var party in order.Parties ?? [])   // one entry class per entry, in order
            Console.WriteLine(party.PartyID.Value);
        break;
}
```

- The 93 standard messages are the cases of `FixMessage`, nested in it: write
  `FixMessage.NewOrderSingle`, and a `switch` over them reads as the closed set it is. A
  MsgType the schema does not know becomes `FixMessage.Custom`, carrying the type it read and
  its fields — that case is why the set can be closed without covering every MsgType that
  exists.
- A message's properties are named after its fields and are the typed fields themselves:
  `FixField.Symbol?`, `FixField.OrderQty?`, null when the field is absent. `Value` on one is
  the CLR value — a `string`, a `decimal`, a `long` — and `IsValid` says whether the
  characters fitted it. A repeating group is a `List<T>?` of entries, one class per group,
  and an entry reads the same way.
- The standard header and trailer are properties of every message, and `Fields` is the whole
  message in wire order, the fields of every group included.
- `ParseMessage` throws `FormatException`; `TryParseMessage` returns false with a
  `FixParseError` saying where and why. The same pair for readers and streams is
  `ReadMessage` and `TryReadMessage`.

## Binary data

A length/data pair — `95=5` then `96=` and five octets — is read by its length, so the
payload may contain separators. It comes back as one field, the data field, whose
`Position` covers both. Counterparty-defined pairs go in `FixFieldOptions`:

```csharp
using System.Collections.Generic;

var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

var options = new FixFieldOptions(new Dictionary<int, int>
{
    [5000] = 5001,   // added to the standard's sixteen pairs, which hold and may not be redeclared
});

var fields  = FixParser.ParseFields(wire, options);
var message = FixParser.ParseMessage(wire, options);   // the same value describes both layers
```

The dictionary **adds to** the standard's sixteen pairs, which always hold: list only
what the standard does not define, and repeating one of its pairs is refused with the tag
named. The same object goes to the message calls. When
reading a stream, `maxRetained` bounds one field, from its tag through the separator that
ends it, or a whole pair: 16 Mi characters from a `TextReader` or bytes from a `Stream` by
default. A field that needs more throws `IOException`, so pass a larger `maxRetained` for
large binary data.

## Streams

`FixParser.ReadFields(Stream)` and `FixParser.ReadFields(TextReader)` return a lazy
`IEnumerable<FixField>`. `FixParser.ReadMessages` returns a lazy
`IEnumerable<FixMessage>`, one message at a time, each at most `maxMessageLength`,
16 MiB by default. Neither closes its input.

## Mistakes to avoid

1. Decoding input as UTF-8. Use Latin-1.
2. Reading `Value` from a field without checking `IsValid`.
3. Expecting the field calls to reject a bad message. They do not: the message calls are
   what check the envelope, BodyLength and CheckSum.
4. Feeding a space-padded log to a message call. Only the field calls read padding.
5. Repeating a standard length/data pair in a dictionary of your own to keep it. They hold
   without being listed, and redeclaring one is refused.
