using System;

namespace DotGram.Finance.Fix;

/// <summary>The rule a finding comes from.</summary>
/// <remarks>
/// A finding names its rule rather than only describing itself, so that a caller can act on one
/// kind without reading prose, and so that rules added later — a counterparty's dictionary brings
/// conditional ones — do not change what already reads a finding.
/// </remarks>
public enum FixRule
{
	/// <summary>The message type is not one the schema defines.</summary>
	UnknownMessageType = 1,

	/// <summary>A field the schema marks required is absent from its scope.</summary>
	RequiredFieldMissing,

	/// <summary>A component the schema marks required has none of its fields present.</summary>
	RequiredComponentMissing,

	/// <summary>A tag appears twice in one scope.</summary>
	DuplicateField,

	/// <summary>The schema does not place this tag in the scope the message puts it in.</summary>
	FieldNotInScope,

	/// <summary>A header field after the body has begun, or <c>BeginString</c>, <c>BodyLength</c> and <c>MsgType</c> not first, second and third.</summary>
	FieldOutOfOrder,

	/// <summary>A value does not fit its field's type, or is not one of its code set.</summary>
	InvalidValue,

	/// <summary>A length field does not immediately precede the data field it measures.</summary>
	LengthFieldNotBeforeData,

	/// <summary>A group's count does not match the entries that follow it.</summary>
	GroupCountMismatch,

	/// <summary><c>MessageEncoding</c> is absent where an <c>Encoded</c> field is present; said at the first such field, by tag 347.</summary>
	MessageEncodingMissing,

	/// <summary><c>BodyLength</c> is not the number of octets of the body it was read with.</summary>
	BodyLengthMismatch,

	/// <summary><c>CheckSum</c> is not the sum, modulo 256, of the octets before it as they were read.</summary>
	CheckSumMismatch,
}

/// <summary>One thing wrong with a message, and where.</summary>
/// <remarks>
/// <para>
/// A validator answers with every finding rather than the first, so a reader reconciling a
/// disagreement with a counterparty sees the whole of it at once. That is the opposite of
/// parsing, which stops at the first error because after it the input's meaning is unknown: a
/// built message is fully known, and every rule can be asked of it independently.
/// </para>
/// <para>
/// The path matters as much as the tag. "Tag 448 is wrong" says nothing where a message carries
/// nine parties, so a finding inside a repeating group names the group and which entry it is.
/// </para>
/// </remarks>
/// <param name="Rule">Which rule found it.</param>
/// <param name="Tag">The tag it is about; zero where the finding is about the message itself.</param>
/// <param name="Position">Where the fault is: the field it is about, or the field it was expected beside.</param>
/// <param name="Field">The field it is about, or null where the finding is that there is none.</param>
/// <param name="EntryIndex">Which entry of the repeating group it is in, counting from zero, or -1 outside one.</param>
public readonly record struct FixFinding(
	FixRule   Rule,
	FixTag    Tag,
	int       Position,
	FixField? Field,
	int       EntryIndex)
{
	/// <summary>The finding as a line: what is wrong, which tag, and where.</summary>
	public override string ToString()
	{
		var where = EntryIndex < 0 ? "" : $"[{EntryIndex}] ";

		return Tag == 0
			? $"{where}at {Position}: {Rule}"
			: $"{where}tag {(int)Tag} at {Position}: {Rule}";
	}
}
