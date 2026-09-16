# DotGram.Finance: FIX 4.4 parser

## Flat parser and explicit semantics

`Fix44.Parse` returns `FixField[]` for contiguous inputs and lazy
`IEnumerable<FixField>` for `TextReader` and native byte `Stream` inputs. The
generated buffered machine yields each complete field, releases consumed input,
and preserves global locations. The explicit `yield : @FixField` publication handles strict consecutive parsing;
no `find` fallback or exception-producing grammar branch is needed.
The syntax path uses no message schema or envelope validator.
Length fields provide raw-data boundaries; matching tag pairs are checked later.

`FixMessages` owns framing, message/group assembly and validation. Call
`FixMessages.Build(source, fields)` to validate an already parsed field array, or
`FixMessages.Parse` / `ReadMessages` to combine parsing with semantic processing.
The historical measurements below include semantics, not only field recognition.

## Scope and sources

Parse complete tag-value messages from strings, character readers or byte streams.
No transport, session engine, persistence or serialization is included. Preserve
the original wire and frame each message independently on a shared stream.

The source of schema generation is FIX Trading Community's
[OrchestraFIX44.xml](https://github.com/FIXTradingCommunity/orchestrations/blob/cd24169a2abd8daba7c360987c7a46ca11873a12/FIX%20Standard/OrchestraFIX44.xml),
version `FIX.4.4_EP311`, pinned to commit
`cd24169a2abd8daba7c360987c7a46ca11873a12`. This is the maintained FIX 4.4
repository, not FIX Latest with its additional messages. Its Apache 2.0 license
is retained beside the unmodified XML. The inventory is 93 messages, 912 fields,
15 components, 92 groups and 247 code sets.

Wire rules follow the official
[FIX TagValue Encoding](https://www.fixtrading.org/standards/tagvalue-online/).
Application scope follows the
[FIX 4.4 specification with 20030618 errata](https://www.fixtrading.org/documents/fix-4-4/).
Conditional business requirements written in narrative documentation are not
automatically executable schema constraints.

## Architecture assessment before implementation

DotGram is a build-time generator with no runtime assembly. Existing standalone
format packages reference it as an analyzer and package their generated code.
The current compiler supports spans, value construction, guards, caller context,
atomic recognition, external recognizers and specialized rule parameters.
Parameters accept compile-time values; an input capture cannot be passed as a
runtime repetition count. The lexical split design note is not authoritative
about implementation; `docs/status.md` and the emitter are.

Use generated grammar for recognition and generated schema definitions for
structural validation and typed access. Raw data cannot be split on SOH: its
preceding length determines the extent. Buffered input does not support external
recognizers, so guards bound ordinary grammar repetitions to that extent.
Do not add FIX names, types or switches to DotGram core. Any necessary core
extension must be specified and tested as a general language capability first.

## Representation

Own one source string for a parsed message, preserving every wire character.
Represent fields by integer tag and source offsets, with no per-field substring
or dictionary of strings. Expose ordered fields and nested group entries through
read-only views. Generate one public message type per standard MsgType, with
named field accessors and named group accessors. Keep optional presence distinct
from a value equal to zero. Numeric and temporal representations must preserve
values that do not fit CLR decimal or DateTime, including leap seconds.

The character input contract must define how octets are represented: length and
checksum are octet operations, not Unicode character operations. A lossless
one-character-per-octet representation permits encoded fields without assuming
that their payload is UTF-8. A future byte input can share source extents and
schema shape without an interface call per character.

## Validation contract

Both modes require unambiguous field boundaries, complete input and valid framing.
Strict additionally checks schema membership, required presence, primitive syntax,
code sets, duplicate fields, group counts and group order. Optional components
activate their required children only when present. Message body field order is
otherwise free; header/body/trailer boundaries and the first three fields are not.
Lenient preserves unknown fields and values. Unknown repeating-group structure
cannot be inferred from tag numbers: vendor definitions are needed to interpret it.
No silent loss of extensions is acceptable.

Parse failures expose source offset, known tag, known MsgType and a reason.
TryParse must not catch exceptions as its ordinary malformed-input path.

## Implemented grammar strategy

The handwritten `FixGrammar` host inherits `FixFieldGrammar`. Its generated
`FixField.gram` declares a single `KnownField` choice with one alternative per
standard numeric tag. Alternatives contain full literals such as `"607="` and
construct their ADT case directly. Text, length and data readers are parameters.
The common handwritten `Field` consumes the separator after the choice. There is
no manually expanded prefix tree and no separate rule per field.
The handwritten `FixGrammar.gram` provides `ValueText`, length-delimited `Data`,
`Separator`, unknown-field recognition and the two parse publications.

`FixField.Generated.cs` supplies only nested case declarations in `partial class
FixField`. The handwritten `FixField.cs` owns location and typed-value behavior;
`FixConvert.cs` owns primitive conversions. Cases share `FixField` as their grammar
result, so the machine does not need a separate value stack per case. The original
wire view is called `FixFieldView`.

Each field directly constructs its named case from the native value span.
Conversions return `(Valid, Value)`; plain text returns a string.
`LocationType = typeof(IFixLocation)` supplies the complete field extent.

`Separator` is an elementary rule. The pipe publication uses
`with (Separator = LogSeparator)`. `ValueText` tests the rule with negative lookahead;
it must retain that reference through specialization rather than flatten a named
set before `with` is applied.

The parser host requests native character spans and buffered byte/character input.
Handwritten conversion hooks have ReadOnlySpan<char> and ReadOnlySpan<byte> overloads.
Numbers preserve exact precision; FIX calendar values preserve year zero and leap
seconds. Standard code sets still constrain the underlying primitive in Strict mode.

Raw data uses an atomic grammar rule: a guard computes the end from the immediately
preceding registered length/data pair, and the grammar consumes blocks of 4096, 256 and 16 octets, then individual
octets up to that end. Each attempt installs its own bound before consuming input. Guards use the end
of parserSpan (the current position), which also works when rules are inlined.
There is no external recognizer and no group-counter stack during recognition.

`FixSemantics` interprets the flat fields using cached schema membership tables,
constructs typed nested groups, and delegates field/requiredness/order validation
to `FixValidation`. Group boundaries and counts are checked independently of the
lexical grammar. Unknown vendor messages retain ordered flat body fields.

A frame adapter reads exactly one BodyLength-delimited message without consuming
the next one. Byte frames are recognized and converted natively; the legacy model
also owns a character view for lossless field spans and validation. Streaming bounds
retention to a frame, not an entire connection, but is not allocation-free.

The shared grammar exposed general generator size issues. Identity alternatives
that all return the same typed capture now share one materialization call, rather
than a switch with hundreds of equivalent bodies. The parser uses `Direct = false`
to keep large choices in the split automaton.

The earlier shared grammar exposed another generator size issue. Large split machines now
use a state-to-part lookup table in their outer dispatcher, and identical root-value
reads share switch arms. Small machines keep their existing emitted shape. A
600-rule regression test compares contiguous, character-stream and byte-stream
results and rejection behavior. Neither emitter change refers to FIX.

## Coverage matrix

Fixtures are generated schema coverage cases. Hand-authored tests separately check
framing, malformed input, primitive boundaries, public API behavior and extensions.

| Area | Source | Evidence |
| --- | --- | --- |
| 93 message types | messages/message | Minimal and fully populated fixture for each type |
| 912 fields | fields/field | Union of fields actually parsed from fixtures equals all 912 definitions |
| Header and trailer | components 1024/1025 | Fully populated headers and signed trailers, framing and checksum failures |
| Required/optional components | reference presence | Minimal/full fixtures, missing required fields and component activation checks |
| 91 reachable groups | group graph | Typed entries in full fixtures; excessive count mutation for every reachable group |
| Unused group definition | ExecsGrp (2016) | Retained in inventory/model/schema; no FIX 4.4 message references it |
| Nested groups | groupRef graph | Full recursive fixtures and a hand-authored Parties/subgroup case |
| 247 code sets | codeSets/codeSet | Every declared code tested; invalid code and IOIQty numeric cases |
| Primitive types | datatype definitions | Invariant parsing; numeric, calendar, precision, multi-value and identifier boundaries |
| Length/data | all 16 lengthId references | Every pair tested with embedded SOH, equals, NUL and non-ASCII octets |
| BodyLength/CheckSum | tag-value specification | Exact octet length/sum, malformed length, overflow and truncation |
| Extensions | explicit parser policy | Scalar tags, tags inside groups, unknown MsgType, registered vendor data pairs |
| ADT and input forms | 912 generated cases | Full/minimal fixtures agree for chars, bytes and pipes; tiny-buffer raw-data checks |
| Wire preservation | field extents | Every fixture reconstructed byte-for-byte from ordered field wire spans |
| Malformed input | grammar and validation | Every prefix of a message, targeted failures and 1,000 deterministic syntax mutations |
| Performance | BenchmarkDotNet | Ordinary messages, 64 KiB data and 1,000 entries; allocation and messages/sec recorded |
| Packaging | independent package consumer | Two target frameworks; package-only consumer smoke project and CI shape checks |

Custom vendor groups without a schema are retained as ordered flat fields in a
custom message. Runtime schema compilation, live ISO assignments and session or
trading business rules are not part of this parser. Registered vendor data pairs
provide the information required to preserve embedded delimiters safely.

The pinned XML SHA-256 is
`A36262895E90BBCAD2948E0C98072173A571A253BA63107A89617441C67A9F86`.
