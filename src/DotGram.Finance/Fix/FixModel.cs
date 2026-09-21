using System;
using System.Globalization;

namespace DotGram.Finance.Fix;

/// <summary>
/// How the input separates one field from the next.
/// </summary>
public enum FixFraming
{
	/// <summary>
	/// Wire framing: every field ends with SOH, U+0001.
	/// </summary>
	Wire,

	/// <summary>
	/// Log framing: every field ends with a vertical bar, which may carry surrounding spaces.
	/// </summary>
	Log,
}

static class FixFramings
{
	/// <summary>
	/// The character the framing ends a field with.
	/// </summary>
	public static char Separator(this FixFraming framing)
	{
		return framing == FixFraming.Log ? '|' : '\u0001';
	}
}

/// <summary>A malformed message, identified by its zero-based character offset.</summary>
public sealed class FixParseError
{
	internal FixParseError(int position, int? tag, string? messageType, string reason)
	{
		Position = position;
		Tag = tag;
		MessageType = messageType;
		Reason = reason;
	}

	/// <summary>
	/// The zero-based character offset at which the problem was found.
	/// </summary>
	public int Position { get; }
	/// <summary>
	/// The tag of the field involved, or null when the problem is not in one field.
	/// </summary>
	public int? Tag { get; }
	/// <summary>
	/// The MsgType, or null when the problem comes before it is known.
	/// </summary>
	public string? MessageType { get; }
	/// <summary>
	/// What is wrong, in English.
	/// </summary>
	public string Reason { get; }
	/// <summary>
	/// Formats the message type, tag, offset and reason on one line.
	/// </summary>
	public override string ToString() => $"FIX {MessageType ?? "?"}, tag {Tag?.ToString(CultureInfo.InvariantCulture) ?? "?"}, offset {Position}: {Reason}";
}

/// <summary>A field backed by the original message; reading Value allocates nothing.</summary>
public readonly struct FixFieldView
{
	readonly string source;

	internal FixFieldView(string source, int tag, int position, int valuePosition, int length, FixField? typedValue = null)
	{
		this.source = source;
		TypedValue = typedValue;
		Tag = tag;
		Position = position;
		ValuePosition = valuePosition;
		Length = length;
	}

	/// <summary>
	/// The typed field parsed at this position, or null when none is attached.
	/// </summary>
	public FixField? TypedValue { get; }
	/// <summary>
	/// The field's tag.
	/// </summary>
	public int Tag { get; }
	/// <summary>
	/// The zero-based offset of the field's first character, where its tag begins.
	/// </summary>
	public int Position { get; }
	/// <summary>
	/// The zero-based offset of the value's first character, after the equals sign.
	/// </summary>
	public int ValuePosition { get; }
	/// <summary>
	/// The length of the value, not counting the separator that ends it.
	/// </summary>
	public int Length { get; }
	/// <summary>
	/// The value text, read from the original message without allocating.
	/// </summary>
	public ReadOnlySpan<char> Value => source.AsSpan(ValuePosition, Length);
	/// <summary>The exact original tag=value field, including the separator that ends it.</summary>
	public ReadOnlySpan<char> Wire => source.AsSpan(Position, ValuePosition + Length + 1 - Position);
	/// <summary>
	/// Returns the value text as a new string.
	/// </summary>
	public override string ToString() => Value.ToString();
	/// <summary>
	/// Reads the value as a decimal with an optional sign and decimal point.
	/// </summary>
	/// <returns>False when the value is not such a number or does not fit a decimal.</returns>
	public bool TryGetDecimal(out decimal value) => decimal.TryParse(Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
	/// <summary>
	/// Reads the value as a 64-bit integer with an optional sign.
	/// </summary>
	/// <returns>False when the value is not such a number or does not fit a long.</returns>
	public bool TryGetInt64(out long value) => long.TryParse(Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
}

/// <summary>A decimal wire value without a CLR decimal range or precision restriction.</summary>
public readonly struct FixNumber
{
	readonly FixFieldView wireField;
	internal FixNumber(FixFieldView field) => wireField = field;
	/// <summary>
	/// The number exactly as written.
	/// </summary>
	public ReadOnlySpan<char> Value => wireField.Value;
	/// <summary>
	/// Reads the number as a decimal.
	/// </summary>
	/// <returns>False when the number does not fit a decimal; FIX sets no limit on range or precision.</returns>
	public bool TryGetDecimal(out decimal value) => wireField.TryGetDecimal(out value);
	/// <summary>
	/// Reads the number as a 64-bit integer.
	/// </summary>
	/// <returns>False when the number is not a whole number that fits a long.</returns>
	/// <remarks>
	/// The accessors that answer with a number include the sequence numbers and the lengths —
	/// <c>BeginSeqNo</c>, <c>EndSeqNo</c>, <c>NewSeqNo</c>, <c>RefSeqNum</c>, <c>RefTagID</c>,
	/// <c>EncodedTextLen</c> — every one of them an integer. Without this, reading one went
	/// through a decimal or through the string, in a type whose only job is to forward.
	/// </remarks>
	public bool TryGetInt64(out long value) => wireField.TryGetInt64(out value);
	/// <summary>
	/// Returns the number as written.
	/// </summary>
	public override string ToString() => wireField.ToString();
}

/// <summary>An ordered field scope: a message part or one repeating group entry.</summary>
public class FixFieldSet
{
	internal readonly FixNode[] Nodes;
	internal readonly string Source;

	internal FixFieldSet(string source, FixNode[] nodes)
	{
		Source = source;
		Nodes = nodes;
	}

	/// <summary>Fields in this scope, including group counters but excluding group entries.</summary>
	public IEnumerable<FixFieldView> Fields
	{
		get
		{
			foreach (var node in Nodes)
				yield return node.Field(Source);
		}
	}

	/// <summary>
	/// Returns the first field with the tag in this scope, or null when there is none.
	/// </summary>
	/// <remarks>
	/// Fields inside group entries belong to those entries and are not searched.
	/// </remarks>
	public FixFieldView? GetField(int tag)
	{
		foreach (var node in Nodes)
			if (node.Tag == tag) return node.Field(Source);
		return null;
	}

	/// <summary>Returns a group's entries, or an empty list when the group is absent.</summary>
	public IReadOnlyList<FixFieldSet> GetGroup(int counterTag)
	{
		foreach (var node in Nodes)
			if (node.Tag == counterTag && node.Entries != null)
				return node.Entries;
		return Array.Empty<FixFieldSet>();
	}

	/// <summary>
	/// Returns the value text of the first field with the tag, or null when there is none.
	/// </summary>
	protected string? GetText(int tag) => GetField(tag)?.ToString();
	/// <summary>
	/// Returns the first field with the tag as a number, or null when there is none.
	/// </summary>
	protected FixNumber? GetNumber(int tag) => GetField(tag) is { } field ? new FixNumber(field) : null;
}

/// <summary>A complete FIX message with its exact original wire representation.</summary>
public abstract partial class FixMessage : FixFieldSet
{
	internal FixMessage(string source, string messageType, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, body)
	{
		MessageType = messageType;
		Header = new StandardHeader(source, header);
		Trailer = new StandardTrailer(source, trailer);
	}

	/// <summary>
	/// The MsgType, tag 35.
	/// </summary>
	public string MessageType { get; }
	/// <summary>
	/// The exact text the message was read from: the wire message, or its pipe rendering when read with log framing.
	/// </summary>
	public string OriginalWire => Source;
	/// <summary>
	/// The standard header.
	/// </summary>
	public StandardHeader Header { get; }
	/// <summary>
	/// The standard trailer.
	/// </summary>
	public StandardTrailer Trailer { get; }

	/// <summary>
	/// Everything wrong with this message against the schema this package compiles in, or nothing
	/// where it is right.
	/// </summary>
	/// <remarks>
	/// What a message of this package's own ninety-three types is held to is the <c>Rule</c> field
	/// of its own class — a message type and a class are the same thing here, so there is nothing
	/// to look up and nothing to pass. A consumer's own message type is a class they wrote, so it
	/// overrides this and answers for itself. Reading
	/// a message and checking it are two acts, not one (D53): nothing here is asked while the wire
	/// is being read, and a message that was built is a message whose fields are all reachable
	/// whatever this answers.
	/// </remarks>
	public virtual FixFinding[] Validate()
	{
		var found = new List<FixFinding>();

		Checking(this, found);

		return found.Count == 0 ? Nothing : found.ToArray();
	}

	/// <summary>The rule this message's own class is holding, which <see cref="Validate"/> asks.</summary>
	private protected abstract FixMessageRule Checking { get; }

	static readonly FixFinding[] Nothing = [];

	/// <summary>All fields in wire order, recursively including group entries.</summary>
	public IEnumerable<FixFieldView> AllFields
	{
		get
		{
			foreach (var item in Walk(Header)) yield return item;
			foreach (var item in Walk(this)) yield return item;
			foreach (var item in Walk(Trailer)) yield return item;
		}
	}

	static IEnumerable<FixFieldView> Walk(FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
		{
			yield return node.Field(scope.Source);

			if (node.Entries != null)
				foreach (var entry in node.Entries)
					foreach (var field in Walk(entry))
						yield return field;
		}
	}
}

readonly struct FixNode
{
	public FixNode(int tag, int position, int valuePosition, int length, IReadOnlyList<FixFieldSet>? entries = null, FixField? typedValue = null, int groupId = 0)
	{
		Tag           = tag;
		Position      = position;
		ValuePosition = valuePosition;
		Length        = length;
		Entries       = entries;
		TypedValue    = typedValue;
		GroupId       = groupId;
	}

	public readonly FixField?                   TypedValue;
	public readonly int                         Tag;
	public readonly int                         Position;
	public readonly int                         ValuePosition;
	public readonly int                         Length;
	public readonly IReadOnlyList<FixFieldSet>? Entries;

	/// <summary>
	/// The schema group these entries were cut from; zero when the node is not a group counter.
	/// </summary>
	public readonly int                         GroupId;

	public FixFieldView Field(string source)
	{
		return new FixFieldView(source, Tag, Position, ValuePosition, Length, TypedValue);
	}
}
