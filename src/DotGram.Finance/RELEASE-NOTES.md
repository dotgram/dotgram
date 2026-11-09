Since 0.2.0. Validation is a question that keeps nothing, every finding says its way through the groups, the limits are the context's, and the types the reader builds are closed to code outside the package. The README has the whole surface; what follows is what changed and what to write instead.

BREAKING CHANGES, and what to write instead

- Validate keeps nothing. message.Validate(context) returns an IReadOnlyList<FixFinding>, empty when nothing is wrong and new on every call; InvalidFindings and IsValid are gone, and a second Validate asks again of the context it is given (it used to answer what the first call found). `if (!message.Validate(context)) Report(message.InvalidFindings)` -> `var findings = message.Validate(context); if (findings.Count > 0) Report(findings);`. Validate(context, findings) adds to a collection of the caller's and returns how many it added. What reading found is message.Findings, read-only and never null.
- FixFinding is a sealed class made by its factories: new FixFinding(rule, tag, position, field, entryIndex) -> FixFinding.Missing(tag), FixFinding.Of(rule, field) or FixFinding.Of(rule, tag), each with the path after it. EntryIndex -> Path, every group and entry on the way to the finding, outermost first, each a FixGroupStep(CounterTag, Index); a finding prints it (453[1].802[0] tag 803 at 86: DuplicateField). Position is -1 for a field the message does not have, where it was 0. Two findings are the same only if they are one object; a finding has no Deconstruct, and its properties no init, so a with expression over one no longer compiles.
- A custom message: OnValidate(context) -> OnValidate(context, findings), adding to the collection it is given. FixCustomMessage.AddFinding may be called only from Place, on the thread that reads the message; anywhere else it throws InvalidOperationException. FixMessage.AddFinding, SetStandardField and the setters of the standard header's and trailer's properties are the package's own.
- FixMessage.Invalid -> FixMessage.Unknown, the message of a MsgType nothing builds; its finding is UnknownMessageType.
- A group entry is built by the reader alone: the 644 entry classes lose their public constructor, and the entry's first field is a get-only property that is never null.
- FixField, the typed field classes under it and FixContext cannot be derived from outside the package: the constructors FixField(tag, isValid), FixField.Typed<T>(tag, value) and FixField.Typed<T>(tag, parsed) are gone. A field is built by its own class, new FixField.Decimal(tag, (true, value)).
- The limits are the context's. The largest message read from a reader or a stream is the context's MaxMessageLength (16 MiB unless given); the maxMessageLength parameter of ReadMessage, TryReadMessage, ReadMessages and ParseMessages and FixParser.DefaultMaxMessageLength are gone: ReadMessage(input, maxMessageLength: n) -> ReadMessage(input, context with { MaxMessageLength = n }). Fix44Context.WithLogFraming -> Fix44Context.Log, one shared value (likewise Fix42Context and Fix50Context). FixMessageFactory -> MessageFactory. The emitTo parameter of With, Load and LoadFile is gone: Load(text, directory) -> (context with { CheckSourceDirectory = directory }).Load(text). A Framing that is neither Wire nor Log throws ArgumentOutOfRangeException when it is set.
- FixDictionaryField takes its tag in its constructor, and applying a dictionary refuses a field stored under a key other than its tag.
- A null argument throws ArgumentNullException from every Try call; TryParseMessage of a null byte array used to answer false.

CHANGED

- A coded field whose text does not read as its type (Side 54=XYZ) is an InvalidValue finding; Validate used to throw InvalidOperationException from its Value.
- Every finding inside a group carries its path, and a finding of the whole message about a field inside an entry (a length not followed by its data, an encoded field without MessageEncoding) carries that entry's.
- The Try calls are annotated for nullability: after true the message is not null, after false the error is not null.
- BuildMessage reads a log with padded separators when the context frames a log; TryBuildMessage is public; both take the octets as a ReadOnlyMemory<byte> as well as a string.
- A stream reads a BeginString of any length and accepts another version's BeginString as a buffer does; Validate reports it.
- A tag nothing defines keeps its text in FixField.Invalid.RawText, or its octets in RawBytes.

FASTER

- A message with groups reads 5-10% faster, and large messages up to 11% (FIX 5.0 reports), with the same allocation: the reader stores every member in a field behind a get-only property. The assembly and the generated sources are smaller.
- A long data field read from a string is narrowed with a vectorized path on .NET.

KNOWN ISSUES

- A stream cannot read a log padded with spaces (read it whole), and a BodyLength that is missing or malformed ends the stream with a diagnostic, since without it there is no telling where the message ends.