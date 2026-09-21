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
- **Messages** — `ParseMessage` and `ParseMessages` from a buffer, `ReadMessage` and
  `ReadMessages` from a reader or a stream. The envelope, BodyLength and CheckSum are
  checked, repeating groups are assembled, and the message's own class comes back. Use it
  when a message's groups matter, or when it is to be held to the schema — which is
  `Validate`, a separate call over the built message, not something the reading does on the
  way.

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
- **Wire or log.** The field calls read SOH-separated fields, and `FixFieldOptions.Log`
  makes them read pipe-separated ones, with or without spaces around the pipe. The same
  value goes to every other call. The message calls accept a bare `|` only; read a log
  padded with spaces with `ParseFields` and `FixFieldOptions.Log`.
- **Forms.** Fields: `string`, `ReadOnlySpan<char>`, `byte[]`, `TextReader` and
  `Stream`. Messages: `string`, `ReadOnlySpan<char>`, `TextReader` and `Stream`. The
  span overloads copy the input into a string first. `TextReader` and `Stream` are
  read lazily and left open: dispose them yourself, and enumerate the result once.

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
  `Value`: text as `string`, numbers as `decimal` or `BigInteger`, dates and times as
  `FixDate`, `FixTime`, `FixTimestamp` and `FixMonthYear`. `field.FieldType` gives the
  tag as a `FixFieldType`, for code that should not spell numbers.
- **`Value` throws when `IsValid` is false.** A field whose text does not convert —
  `38=abc`, a date of `20261340` — is still returned, with `IsValid` false. Test it
  first, or use `TryGetValue`.
- A tag the package does not know comes back as `FixField.Custom`, its value as bytes.
  Supply a `FixCustomFields` to `FixFieldOptions` to build your own field for such a tag
  instead — three methods, because a field is built from characters, from bytes and from the
  payload of a length/data pair, and answering one of them and not the others gives you two
  parses of one message that disagree. It builds field objects only: the tag stays unknown to
  the message schema, so `Validate` still reports it.
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
        string? status = report.OrdStatus;        // null when the field is absent
        if (report.LastPx is { } price && price.TryGetDecimal(out var px))
            Console.WriteLine(px);
        break;

    case FixMessage.NewOrderSingle order:
        foreach (var party in order.Parties)      // one FixFieldSet per entry, in order
            Console.WriteLine(party.GetField(448)?.ToString());
        break;
}
```

- The 93 standard messages are the cases of `FixMessage`, nested in it: write
  `FixMessage.NewOrderSingle`, and a `switch` over them reads as the closed set it is. A
  MsgType the schema does not know becomes `FixMessage.Custom`, and `Validate`
  reports the type as unknown — that case is why the set can be closed without covering
  every MsgType that exists.
- A message's properties are named after its fields. Text is `string?` and numbers are
  `FixNumber?`, which keeps the digits exactly as written; both are null when the field
  is absent. A group is `IReadOnlyList<FixFieldSet>`, empty when absent, and each entry
  is read with `GetField(tag)` and `GetGroup(counterTag)`, the way nested groups are.
- For the typed value behind a message field, `GetField(tag)?.TypedValue` gives its
  `FixField` case.
- `Header` and `Trailer` hold the standard header and trailer, `AllFields` walks the
  whole message in wire order, groups included, and `OriginalWire` is the exact input.
- `ParseMessage` throws `FormatException`; `TryParseMessage` returns false with a
  `FixParseError` saying where and why. The same pair for readers and streams is
  `ReadMessage` and `TryReadMessage`.

## Validation

Reading and checking are two acts. `Parse` recognises the wire — framing, `BodyLength`,
`CheckSum`, length/data pairs, group structure — and builds whatever it can read, keeping
unknown tags, unknown message types, reordered or repeated fields and malformed values.
Holding the result to the schema is then one call:

```csharp
var wire    = "8=FIX.4.4\u00019=51\u000135=0\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u000110=136\u0001";
var message = FixParser.ParseMessage(wire);

foreach (var finding in message.Validate())
    Console.WriteLine(finding);   // Body/453[1] tag 452 at 187: InvalidValue: ...
```

`Validate()` answers with every finding, not the first, because a built message is fully
known and every rule can be asked of it independently. A finding names its rule, the
scope, the tag, the entry of the repeating group it is in, and where in the source it
begins. A valid message answers with an empty array and allocates nothing.

**Do not carry over the shape of validation that stops at the first problem.** If the code
you are porting expects a throw, or reads one error and moves on, it will keep the first
finding here and discard the rest silently — `message.Validate().FirstOrDefault()` is that
shape said out loud, if it is really what you want. And where a message carries nine parties,
`finding.GroupTag`, `finding.EntryIndex` and `finding.Position` are the part that says which
one; a bare tag does not.

### Replacing a rule

**A message type and a class are the same thing, so the rule lives in the class's own field.**
There is no validator to make, nothing to pass and nothing to look up: `Validate()` asks the
`Rule` field of the class the message is, and you replace it by assigning to that field.

```csharp
using System.Collections.Generic;

using DotGram.Finance.Fix;

FixMessage.NewOrderSingle.Rule = (message, findings) =>
{
    var order = (FixMessage.NewOrderSingle)message;   // the class is the key, so you know which it is

    if (order.Symbol is null)
        findings.Add(new FixFinding(FixRule.RequiredFieldMissing, FixScope.Body, 55, 0, -1, 0, "mine"));
};

// And back again: what the package compiles in is reached by name.
FixMessage.NewOrderSingle.Rule = FixValidator.ValidateNewOrderSingle;
```

`FixValidator` is not a way in — nothing there is called to validate a message. It holds the
ninety-four rules this package compiles in, one named method a type, so that a replacement can be
undone by assigning the name back.

**Ask whether a rule is still in place with `==`, not with `ReferenceEquals`.**
`FixMessage.NewOrderSingle.Rule == FixValidator.ValidateNewOrderSingle` is the question; whether
the two are also one object is the compiler's business and not something to rely on.

**One field means one configuration for the process.** Two counterparties with two different
schemas in one process is not expressible, and that is the trade the shape was chosen for.

### A counterparty's dictionary

One call reads a QuickFIX dictionary and writes the rules of every type it describes:

```csharp
using System.IO;

using DotGram.Finance.Fix;

using (var file = File.OpenRead("FIX44-venue.xml"))
    FixParser.LoadDictionary(file);

var wire    = "8=FIX.4.49=5135=049=SENDER56=TARGET34=152=20260915-12:00:0010=136";
var message = FixParser.ParseMessage(wire);

foreach (var finding in message.Validate())
    Console.WriteLine(finding);
```

It answers with nothing and it **throws** where the file is not a dictionary it accepts: check the
file before you deploy it, because validation holding half of one schema and half of another is
worse than a refusal. A type the file does not describe keeps the rule it had.

**A type this package has no class for stays unknown.** A venue's own MsgType has no class, so it
has no field for a rule to live in: loading a dictionary moves the ninety-three and no more, and
a message of such a type still reports `UnknownMessageType`. What to do about it is to write the
class — see **Custom fields** for the same seam one layer down.

This package ships no dictionary of anyone's. The file is yours, in your repository, read by your
code, and its licence obligations are yours with it.

### Before you trust a new dictionary

**What this tells you is what your own messages exercise, and nothing else.** A dictionary that
changes a message type you have never received says nothing here until the day you receive one.
Read that first: a guard whose limits you learn on the day it misses is worse than no guard,
because no guard does not reassure.

With that said, the question worth asking before a new file goes live is not "do these two
descriptions differ" — they will, in tags you never send — but "does this file change what
validation says about *my* traffic". Ask it with the messages you already have, and in this order,
because one field holds one rule at a time:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix;

var captured = File.ReadAllLines("yesterday.log");   // your own, in wire or log framing
var messages = new List<FixMessage>();
var before   = new List<FixFinding[]>();

foreach (var line in captured)
    if (FixParser.TryParseMessage(line, out var message, out _, FixFieldOptions.Log))
    {
        messages.Add(message!);
        before.Add(message!.Validate());          // what today's rules say
    }

using (var file = File.OpenRead("FIX44-venue-new.xml"))
    FixParser.LoadDictionary(file);

for (var i = 0; i < messages.Count; i++)
{
    var after = messages[i].Validate();

    // In ORDER, not as sets: two schemas can report the same findings in a different
    // sequence, and that is a difference a reader sees.
    if (!before[i].SequenceEqual(after))
        Console.WriteLine($"{messages[i].MessageType}: [{string.Join("; ", before[i])}] -> [{string.Join("; ", after)}]");
}
```

Every difference it prints is one the new file would have made yesterday. An empty run means the
file changes nothing about the traffic you fed it — not that it changes nothing.

The other half of this is already done and is not yours to write: the tags, the code sets and
the message compositions where this package's own FIX 4.4 tables and the published QuickFIX
dictionary disagree are pinned in this package's test suite, and a change on either side
fails a build here. What is left for you is the half about *your* counterparty's file.

It is not a trading validator. Sequence numbers, session state and business rules are
the application's.

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
3. Expecting the field calls to reject a bad message. They do not validate; the message
   calls check the envelope, and `Validate` holds the result to the schema.
4. Feeding a space-padded log to a message call. Only the field calls read padding.
5. Repeating a standard length/data pair in a dictionary of your own to keep it. They hold
   without being listed, and redeclaring one is refused.
6. Holding a `FixMessage` longer than needed. It keeps its whole source string alive.
