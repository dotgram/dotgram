<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — which layer to use, how to read fields and messages, and the mistakes
  that are easy to make. Read it before writing code against the package. In a
  restored package that is ~/.nuget/packages/dotgram.finance/<version>/SKILL.md.
-->

# DotGram.Finance

Reads and checks finance protocols and formats wherever they are kept rather than traded: logs,
archives, files, message buses. Today that is FIX 4.2, FIX 4.4 and FIX 5.0 SP2 tag-value messages;
other protocols and formats join the package as they are added. For `netstandard2.0` and
`net10.0`; DotGram compiles the grammar at build time, so applications need no DotGram runtime,
grammar files, schema XML, reflection configuration or initialization step.

```
dotnet add package DotGram.Finance
```

```csharp
using System;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

// One message, with the separator written as a pipe so it can be read on a page.
var wire = ("8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|" +
            "38=100|40=2|44=12.50|10=000|").Replace('|', '\u0001');

if (FixParser.TryParseMessage(wire, out var message, out var error) && message is FixMessage.NewOrderSingle order)
{
    Console.WriteLine(order.Symbol?.Value);
    Console.WriteLine(order.OrderQty?.Value);
}
else
{
    Console.WriteLine(error);
}
```

## What it is for

- **Logs and archives.** Every FIX engine journals what it sent and received, often with `|` for
  the separator and spaces around it. Incident review, audit, trade reconstruction, regulatory
  reporting and execution analysis are read from those journals — gigabytes a day, read from a
  stream in bounded memory, pipe-delimited or not.
- **Checking messages against a counterparty's dictionary** before a certification or in a test
  suite: apply their data dictionary, and `Validate` reports every finding of a message at once,
  each with its rule, tag, position and group entry, rather than the first.
- **Buses and stores.** Data kept in Kafka, a database or a queue — drop copy, post-trade, clearing —
  and read by a consumer that has no session to hold.
- **Files and other transports.** End-of-day allocation and confirmation files, FIX carried over a
  queue, an HTTP or a WebSocket gateway.
- **Tools.** Simulators, load generators, test harnesses, anonymizers, converters to other formats.
- **An engine of your own.** Where the session layer is written in house, this is the parser and
  the checks under it.

## Capabilities

- **FIX 4.2, FIX 4.4, FIX 5.0 SP2.** Tag-value messages: fields, messages, streams, pipe-delimited
  logs, validation, dictionaries, and custom fields and messages. See
  [the FIX document](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Finance/Fix/README.md)
  for all of it.

## What it is not

It is not a FIX engine. There is no socket, no session: no logon, sequence numbers, heartbeats,
resend requests, message store or schedule; a live trading connection needs an engine, which reads
with its own parser. Where you hold the bytes, this reads them: `ReadMessages` takes the `Stream`
your transport hands you.
