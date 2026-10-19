using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

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

/// <summary>One step of the way to a finding inside repeating groups: the group, by its counter's tag, and which entry.</summary>
/// <param name="CounterTag">The tag of the group's counter, NoPartyIDs' 453.</param>
/// <param name="Index">Which entry of the group, counting from zero.</param>
public readonly record struct FixGroupStep(int CounterTag, int Index)
{
	/// <summary>The step as a finding's path writes it: <c>453[1]</c>.</summary>
	public override string ToString()
	{
		return CounterTag.ToString(CultureInfo.InvariantCulture) + "[" + Index.ToString(CultureInfo.InvariantCulture) + "]";
	}
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
/// nine parties, so a finding inside repeating groups names every group and entry on the way to
/// it, outermost first: <c>453[1].802[0]</c> is the first sub-ID entry of the second party.
/// </para>
/// <para>
/// Made by the package, or by a custom message or a consumer's own rules through
/// <see cref="Missing"/> and <see cref="Of(FixRule, FixField, FixGroupStep[])"/>; a finding is never
/// changed once made, and two are the same finding only if they are the same object.
/// </para>
/// </remarks>
public sealed class FixFinding
{
	// A read-only view of the steps, never the array itself, so that a caller cannot cast the path back to
	// an array and move a finding, or the findings of one entry, which share it.
	readonly ReadOnlyCollection<FixGroupStep>? _path;

	FixFinding(FixRule rule, int tag, int position, FixField? field, ReadOnlyCollection<FixGroupStep>? path)
	{
		Rule     = rule;
		Tag      = tag;
		Position = position;
		Field    = field;
		_path    = path;
	}

	/// <summary>Which rule found it.</summary>
	public FixRule   Rule     { get; }

	/// <summary>The tag it is about: the field's, or the one that is missing.</summary>
	public int       Tag      { get; }

	/// <summary>Where the fault is: the field it is about, or the field it was expected beside; -1 where there is no place, a field the message lacks.</summary>
	public int       Position { get; }

	/// <summary>The field it is about, or null where the finding is that there is none.</summary>
	public FixField? Field    { get; }

	/// <summary>The groups and entries the finding is in, outermost first; empty outside every group.</summary>
	public IReadOnlyList<FixGroupStep> Path => (IReadOnlyList<FixGroupStep>?)_path ?? Array.Empty<FixGroupStep>();

	/// <summary>A required field that is absent: from the message, or from the entry the path names.</summary>
	/// <param name="tag">The field's tag.</param>
	/// <param name="path">The groups and entries it is missing from, outermost first; none for the message itself.</param>
	public static FixFinding Missing(int tag, params FixGroupStep[] path)
	{
		return new FixFinding(FixRule.RequiredFieldMissing, tag, -1, null, Copy(path));
	}

	/// <summary>A finding about a field the message has, said where the field is.</summary>
	/// <param name="rule">Which rule found it.</param>
	/// <param name="field">The field it is about.</param>
	/// <param name="path">The groups and entries the field is in, outermost first; none outside every group.</param>
	public static FixFinding Of(FixRule rule, FixField field, params FixGroupStep[] path)
	{
		if (field == null) throw new ArgumentNullException(nameof(field));

		return new FixFinding(rule, field.Tag, field.Position, field, Copy(path));
	}

	/// <summary>A finding about a tag with no field to point at.</summary>
	/// <param name="rule">Which rule found it.</param>
	/// <param name="tag">The tag it is about.</param>
	/// <param name="path">The groups and entries it is about, outermost first; none for the message itself.</param>
	public static FixFinding Of(FixRule rule, int tag, params FixGroupStep[] path)
	{
		return new FixFinding(rule, tag, -1, null, Copy(path));
	}

	// The package's own: a path it made for this finding and does not change.
	internal static FixFinding At(FixRule rule, int tag, int position, FixField? field, FixGroupStep[]? path)
	{
		return new FixFinding(rule, tag, position, field, path is null ? null : new ReadOnlyCollection<FixGroupStep>(path));
	}

	// The check's: a path it shares between the findings of one entry.
	internal static FixFinding Shared(FixRule rule, int tag, int position, FixField? field, ReadOnlyCollection<FixGroupStep>? path)
	{
		return new FixFinding(rule, tag, position, field, path);
	}

	/// <summary>The finding as a line: where in the groups, which tag, where in the input, and what is wrong.</summary>
	public override string ToString()
	{
		var line = new StringBuilder();

		if (_path is not null)
		{
			foreach (var step in _path)
				line.Append(line.Length == 0 ? "" : ".").Append(step.ToString());

			line.Append(' ');
		}

		if (Tag != 0)
			line.Append("tag ").Append(Tag.ToString(CultureInfo.InvariantCulture)).Append(' ');

		if (Position >= 0)
			line.Append("at ").Append(Position.ToString(CultureInfo.InvariantCulture));
		else if (line.Length > 0)
			line.Length--;

		return line.Length == 0 ? Rule.ToString() : line.Append(": ").Append(Rule.ToString()).ToString();
	}

	// A caller's path is copied, so that what it does with its array afterwards does not move the finding.
	static ReadOnlyCollection<FixGroupStep>? Copy(FixGroupStep[]? path)
	{
		if (path is null || path.Length == 0)
			return null;

		return new ReadOnlyCollection<FixGroupStep>((FixGroupStep[])path.Clone());
	}
}
