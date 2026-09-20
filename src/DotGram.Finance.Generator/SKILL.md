# Compiling a FIX dictionary

For an agent working in a project that has restored `DotGram.Finance.Generator`.

## When this package is wanted, and when it is not

`DotGram.Finance` compiles FIX 4.4 in. If the schema being checked is FIX 4.4, nothing here is
needed: `message.Validate()` already answers.

Reach for this package when the schema is somebody else's — a counterparty's dictionary, a version
other than 4.4, a venue's own message types, or a tighter reading of the standard than the
standard's own.

## The whole of the wiring

```xml
<ItemGroup>
  <AdditionalFiles Include="FIX44-venue.xml" />
</ItemGroup>
```

```csharp
using DotGram.Finance.Fix;

[FixDictionaryFile("FIX44-venue.xml")]
public static partial class VenueRules;
```

The file is named as the project lists it; a bare file name matches wherever the build found it.
The class must be `partial`, and may be `static`.

`VenueRules.Validator` is a `FixValidator` holding a rule for every message type the dictionary
describes. `VenueRules.Entries` is the same rules as a table, for mixing two dictionaries.

## What the generated rules check

Membership of each field in its scope, duplicates within a scope, the value held to its tag's type
and code set, required fields, required components, every group entry with its count and the order
of its fields, and `MessageEncoding` where an `Encoded` field is present.

What they do not check is what the reader checks first and cannot wait for: framing, `BodyLength`,
`CheckSum`, and the shape of a length/data pair. A message that does not parse never reaches a
rule.

## Diagnostics

`FIXGEN001` — the dictionary named is not among the build's `AdditionalFiles`. Add it.

`FIXGEN002` — the file is not a dictionary this reader accepts. The message names the element and
the line.

## One rule worth knowing

A rule is replaceable. `validator["D"] = (message, findings) => ...` writes your own for one type,
and `FixValidator.Compiled` is what to write back to undo it. `FixValidator.Standard` is shared by
the whole process and refuses to be written to; make your own with `new FixValidator()`.

To hold a value to a FIX type inside a rule of your own, call `FixValues.Valid` — the one
implementation, which the generated code calls too. Do not write a second one.
