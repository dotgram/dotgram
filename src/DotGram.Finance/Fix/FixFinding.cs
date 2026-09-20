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

	/// <summary>The fields of a group entry are not in the order the schema gives them.</summary>
	FieldOutOfOrder,

	/// <summary>A value does not fit its field's type, or is not one of its code set.</summary>
	InvalidValue,

	/// <summary>A data field does not immediately follow the length field that measures it.</summary>
	DataFieldNotAfterLength,

	/// <summary>A length field does not immediately precede the data field it measures.</summary>
	LengthFieldNotBeforeData,

	/// <summary>A group's count does not match the entries that follow it.</summary>
	GroupCountMismatch,

	/// <summary><c>MessageEncoding</c> is absent where an <c>Encoded</c> field is present.</summary>
	MessageEncodingMissing,
}

/// <summary>Which of a message's three scopes a finding is in.</summary>
public enum FixScope
{
	/// <summary>The header.</summary>
	Header = 1,

	/// <summary>The body.</summary>
	Body,

	/// <summary>The trailer.</summary>
	Trailer,
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
/// <param name="Scope">The header, the body or the trailer.</param>
/// <param name="Tag">The tag it is about, or null where the finding is about a component or a message.</param>
/// <param name="GroupTag">The counter tag of the group this is inside, or zero where it is not inside one.</param>
/// <param name="EntryIndex">Which entry of that group, counting from zero, or -1 outside a group.</param>
/// <param name="Position">Where in the source the field begins, or zero where there is no field.</param>
/// <param name="Reason">What is wrong, in one sentence.</param>
public readonly record struct FixFinding(
	FixRule  Rule,
	FixScope Scope,
	int?     Tag,
	int      GroupTag,
	int      EntryIndex,
	int      Position,
	string   Reason)
{
	/// <summary>The finding as a line: where it is, what rule, and what is wrong.</summary>
	public override string ToString()
	{
		var where = GroupTag == 0
			? Scope.ToString()
			: $"{Scope}/{GroupTag}[{EntryIndex}]";

		return Tag is { } tag
			? $"{where} tag {tag} at {Position}: {Rule}: {Reason}"
			: $"{where} at {Position}: {Rule}: {Reason}";
	}
}
