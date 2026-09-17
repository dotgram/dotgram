using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotGram.Finance;

/// <summary>The validation policy applied after wire recognition.</summary>
public enum FixParseMode
{
	Strict,
	Lenient,
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

	public int Position { get; }
	public int? Tag { get; }
	public string? MessageType { get; }
	public string Reason { get; }
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

	public FixField? TypedValue { get; }
	public int Tag { get; }
	public int Position { get; }
	public int ValuePosition { get; }
	public int Length { get; }
	public ReadOnlySpan<char> Value => source.AsSpan(ValuePosition, Length);
	/// <summary>The exact original tag=value field, including its final SOH.</summary>
	public ReadOnlySpan<char> Wire => source.AsSpan(Position, ValuePosition + Length + 1 - Position);
	public override string ToString() => Value.ToString();
	public bool TryGetDecimal(out decimal value) => decimal.TryParse(Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
	public bool TryGetInt64(out long value) => long.TryParse(Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
}

/// <summary>A decimal wire value without a CLR decimal range or precision restriction.</summary>
public readonly struct FixNumber
{
	readonly FixFieldView wireField;
	internal FixNumber(FixFieldView field) => wireField = field;
	public ReadOnlySpan<char> Value => wireField.Value;
	public bool TryGetDecimal(out decimal value) => wireField.TryGetDecimal(out value);
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

	protected string? GetText(int tag) => GetField(tag)?.ToString();
	protected FixNumber? GetNumber(int tag) => GetField(tag) is { } field ? new FixNumber(field) : null;
	protected IReadOnlyList<T> GetTypedGroup<T>(int counterTag) where T : FixFieldSet
	{
		foreach (var node in Nodes)
			if (node.Tag == counterTag && node.Entries is IReadOnlyList<T> entries) return entries;
		return Array.Empty<T>();
	}
}

/// <summary>A complete FIX message with its exact original wire representation.</summary>
public abstract class FixMessage : FixFieldSet
{
	internal FixMessage(string source, string messageType, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, body)
	{
		MessageType = messageType;
		Header = new StandardHeader(source, header);
		Trailer = new StandardTrailer(source, trailer);
	}

	public string MessageType { get; }
	public string OriginalWire => Source;
	public StandardHeader Header { get; }
	public StandardTrailer Trailer { get; }

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
					foreach (var field in Walk(entry)) yield return field;
		}
	}
}

/// <summary>A vendor message of unknown MsgType; its body fields remain in wire order.</summary>
public sealed class CustomFixMessage : FixMessage
{
	internal CustomFixMessage(string source, string type, FixNode[] header, FixNode[] body, FixNode[] trailer) : base(source, type, header, body, trailer) { }
}

readonly struct FixNode
{
	public FixNode(int tag, int position, int valuePosition, int length, IReadOnlyList<FixFieldSet>? entries = null, FixField? typedValue = null)
	{
		Tag = tag;
		Position = position;
		ValuePosition = valuePosition;
		Length = length;
		Entries = entries;
		TypedValue = typedValue;
	}

	public readonly FixField? TypedValue;
	public readonly int Tag;
	public readonly int Position;
	public readonly int ValuePosition;
	public readonly int Length;
	public readonly IReadOnlyList<FixFieldSet>? Entries;
	public FixFieldView Field(string source) => new(source, Tag, Position, ValuePosition, Length, TypedValue);
	public FixNode WithEntries<T>(T[] entries) where T : FixFieldSet => new(Tag, Position, ValuePosition, Length, Array.AsReadOnly(entries), TypedValue);
}
