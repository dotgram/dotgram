# DotGram.Finance: FIX 4.4 parser

## Scope and sources

Parse one complete contiguous tag-value message. No transport, session engine,
streaming, persistence or serialization is included. Preserve the original wire.

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
preceding length determines the extent. First prove a bounded external recognizer
for that extent, with ordinary tags, delimiters and text recognized by grammar.
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

Each MsgType has a specialized generated grammar. Ordinary body tags share a
literal-set rule; groups retain specialized delimiters and nested productions.
Explicit factories combine header arrays: an array-valued rule is not implicitly
flattened by a surrounding sequence in the current compiler.

`NumInGroup` is interpreted during recognition through caller context. Group and
entry recognition, entry repetition and body repetition are atomic. This matters:
the counter stack cannot be replayed after a successful group is abandoned by
backtracking. A failed group records a sticky error, and the public wrapper rejects
that context even if another recognition path were to return a value. Malformed
group-count tests cover every reachable group definition.

Raw-data recognition reads the preceding numeric length and advances across the
payload as one extent. Grammar recognizes the tags and delimiters; validation checks
the exact length/data association. Core DotGram requires no changes. Internal parser
hosts use `Portable = false` because their grammar metadata is not a consumer API.

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
