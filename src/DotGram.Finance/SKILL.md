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

## Choose the layer first

There are two, and most wrong code starts from the wrong one.

- **`FixParser` reads fields.** It returns every field in source order as a typed
  `FixField`, repeated and unknown tags included. It validates nothing beyond what it
  needs to find the next field: no message boundaries, no groups, no BodyLength, no
  CheckSum, no required fields. Use it for logs, for pulling a few values out, and for
  anything that must not reject input.
- **`FixMessages` reads messages.** It checks the envelope, BodyLength and CheckSum,
  assembles repeating groups, validates against the FIX 4.4 schema and returns the
  message's own class. Use it when a message has to be known correct, or when its
  groups matter.

`FixMessages.Build(source, fields)` joins them: it validates fields that `FixParser`
already returned without reading the input again.

## Input

- **A string is octets, not text.** Every character stands for one byte,
  U+0000 through U+00FF. Decode files and sockets with Latin-1
  (`Encoding.Latin1`), never UTF-8: UTF-8 changes the byte count, and `FixMessages`
  then rejects the message on BodyLength or CheckSum. `FixMessages` rejects
  characters above U+00FF.
- **Wire or log.** `FixParser.Parse` reads SOH-separated fields and
  `FixParser.ParseLog` pipe-separated ones, with or without spaces around the pipe.
  For messages use `FixMessages.ParseLog`, or `new FixParseOptions(FixFraming.Log)`
  with any other entry point. The message layer accepts a bare `|` only; read a log
  padded with spaces through `FixParser.ParseLog`.
- **Forms.** Fields: `string`, `ReadOnlySpan<char>`, `byte[]`, `TextReader` and
  `Stream`. Messages: `string`, `ReadOnlySpan<char>`, `TextReader` and `Stream`. The
  span overloads copy the input into a string first. `TextReader` and `Stream` are
  read lazily and left open: dispose them yourself, and enumerate the result once.

## Reading fields

```csharp
using DotGram.Finance.Fix;

foreach (var field in FixParser.Parse(wire))
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
  `Value`: text as `string`, numbers as `decimal` or `BigInteger`, dates and times as
  `FixDate`, `FixTime`, `FixTimestamp` and `FixMonthYear`. `field.FieldType` gives the
  tag as a `FixFieldType`, for code that should not spell numbers.
- **`Value` throws when `IsValid` is false.** A field whose text does not convert —
  `38=abc`, a date of `20261340` — is still returned, with `IsValid` false. Test it
  first, or use `TryGetValue`.
- A tag the package does not know comes back as `FixField.Unknown`, its value as bytes.
- A syntax error does not throw. It becomes one `FixField.Invalid`, and reading
  resumes after the next separator.

## Reading messages

```csharp
if (!FixMessages.TryParse(wire, out var message, out var error))
{
    Console.WriteLine(error);                     // message type, tag, offset and reason
    return;
}

switch (message)
{
    case ExecutionReport report:
        string? status = report.OrdStatus;        // null when the field is absent
        if (report.LastPx is { } price && price.TryGetDecimal(out var px))
            Console.WriteLine(px);
        break;

    case NewOrderSingle order:
        foreach (var party in order.Parties)      // one FixFieldSet per entry, in order
            Console.WriteLine(party.GetField(448)?.ToString());
        break;
}
```

- All 93 standard messages have a class; match on it. Under `Lenient`, a MsgType the
  schema does not know becomes a `CustomFixMessage`.
- A message's properties are named after its fields. Text is `string?` and numbers are
  `FixNumber?`, which keeps the digits exactly as written; both are null when the field
  is absent. A group is `IReadOnlyList<FixFieldSet>`, empty when absent, and each entry
  is read with `GetField(tag)` and `GetGroup(counterTag)`, the way nested groups are.
- For the typed value behind a message field, `GetField(tag)?.TypedValue` gives its
  `FixField` case.
- `Header` and `Trailer` hold the standard header and trailer, `AllFields` walks the
  whole message in wire order, groups included, and `OriginalWire` is the exact input.
- `Parse` throws `FormatException`; `TryParse` returns false with a `FixParseError`
  saying where and why.

## Validation

`FixParseMode.Strict`, the default, enforces the schema: required fields, the order and
uniqueness of fields in groups, primitive syntax and code sets. Unknown tags and message
types are rejected. `FixParseMode.Lenient` still checks the envelope, length/data pairs
and group structure, but keeps unknown tags, unknown message types, reordered or repeated
fields and malformed values.

Neither is a trading validator. Sequence numbers, session state and business rules are
the application's.

## Binary data

A length/data pair — `95=5` then `96=` and five octets — is read by its length, so the
payload may contain separators. It comes back as one field, the data field, whose
`Position` covers both. Counterparty-defined pairs go in `FixFieldOptions`:

```csharp
var pairs = new FixFieldOptions(new Dictionary<int, int>
{
    [95]   = 96,                                  // a supplied dictionary replaces the standard one
    [5000] = 5001,
});

var fields  = FixParser.Parse(wire, pairs);
var options = new FixParseOptions(FixFraming.Wire, fieldOptions: pairs);
```

The dictionary **replaces** the standard pairs rather than adding to them, so list the
standard ones still needed. Pass the same object to `FixParseOptions` for messages. When
reading a stream, `maxRetained` bounds one field, from its tag through the separator that
ends it, or a whole pair: 16 Mi characters from a `TextReader` or bytes from a `Stream` by
default. A field that needs more throws `IOException`, so pass a larger `maxRetained` for
large binary data.

## Streams

`FixParser.Parse(Stream)` and `FixParser.Parse(TextReader)` return a lazy
`IEnumerable<FixField>`. `FixMessages.ReadMessages` returns a lazy
`IEnumerable<FixMessage>`, one message at a time, each at most `maxMessageLength`,
16 MiB by default. Neither closes its input.

## Mistakes to avoid

1. Decoding input as UTF-8. Use Latin-1.
2. Reading `Value` from a field without checking `IsValid`.
3. Expecting `FixParser` to reject a bad message. It does not validate; `FixMessages`
   does.
4. Feeding a space-padded log to `FixMessages`. Only `FixParser.ParseLog` reads padding.
5. Supplying a length/data dictionary and dropping the standard pairs it replaced.
6. Holding a `FixMessage` longer than needed. It keeps its whole source string alive.
