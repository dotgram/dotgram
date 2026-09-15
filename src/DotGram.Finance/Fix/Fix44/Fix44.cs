using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotGram.Finance.Fix;

/// <summary>Parses one complete FIX 4.4 tag-value message.</summary>
public static partial class Fix44
{
	/// <summary>Parse a lossless octet string: each character must be in U+0000..U+00FF.</summary>
	public static FixMessage Parse(string input, FixParseMode mode = FixParseMode.Strict)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (TryParse(input, out var message, out var error, mode)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Copies the contiguous input once so the result owns its source.</summary>
	public static FixMessage Parse(ReadOnlySpan<char> input, FixParseMode mode = FixParseMode.Strict) => Parse(input.ToString(), mode);

	public static FixMessage Parse(string input, FixParseOptions options)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		if (TryParse(input, out var message, out var error, options)) return message!;
		throw new FormatException(error!.ToString());
	}

	public static FixMessage Parse(ReadOnlySpan<char> input, FixParseOptions options) => Parse(input.ToString(), options);
	public static bool TryParse(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixParseOptions options) => TryParse(input.ToString(), out message, out error, options);

	public static bool TryParse(string? input, out FixMessage? message, out FixParseError? error, FixParseOptions options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));
		return TryParseCore(input, out message, out error, options.Mode, options);
	}

	/// <summary>Returns false and diagnostic context for malformed or incomplete input.</summary>
	public static bool TryParse(string? input, out FixMessage? message, out FixParseError? error, FixParseMode mode = FixParseMode.Strict)
		=> TryParseCore(input, out message, out error, mode, null);

	static bool Envelope(string input, char separator, FixValue[]? fields, out string type, out FixParseError? error)
	{
		type = "";
		error = null;
		for (var i = 0; i < input.Length; i++)
			if (input[i] > 255) return Fail(i, null, null, "Input must preserve octets as characters U+0000 through U+00FF.", out error);
		if (!input.StartsWith("8=FIX.4.4" + separator + "9=", StringComparison.Ordinal)) return Fail(0, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);
		var lengthEnd = input.IndexOf(separator, 12);
		if (lengthEnd < 0) return Fail(input.Length, 9, null, "Truncated BodyLength.", out error);
		if (!int.TryParse(input.AsSpan(12, lengthEnd - 12), NumberStyles.None, CultureInfo.InvariantCulture, out var bodyLength)) return Fail(12, 9, null, "BodyLength must be a nonnegative integer within the input range.", out error);
		var bodyStart = lengthEnd + 1;
		if (input.Length - bodyStart < 4 || !input.AsSpan(bodyStart, 3).SequenceEqual("35=".AsSpan())) return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);
		var typeEnd = input.IndexOf(separator, bodyStart + 3);
		if (typeEnd < 0) return Fail(input.Length, 35, null, "Truncated MsgType.", out error);
		type = input.Substring(bodyStart + 3, typeEnd - bodyStart - 3);
		if (bodyLength != input.Length - bodyStart - 7) return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);
		var checksumStart = bodyStart + bodyLength;
		if (!input.AsSpan(checksumStart, 3).SequenceEqual("10=".AsSpan()) || input[input.Length - 1] != separator) return Fail(checksumStart, 10, type, "Expected final CheckSum field with three digits and SOH.", out error);
		if (!int.TryParse(input.AsSpan(checksumStart + 3, 3), NumberStyles.None, CultureInfo.InvariantCulture, out var expected)) return Fail(checksumStart + 3, 10, type, "CheckSum must contain exactly three digits.", out error);
		var checksum = 0;
		for (var i = 0; i < checksumStart; i++) checksum = (checksum + input[i]) & 255;
		if (separator != '\u0001')
		{
			if (fields == null) return true;
			foreach (var field in fields)
				if (field.ValuePosition + field.Length < checksumStart) checksum = (checksum - separator + 1) & 255;
		}
		if (checksum != expected) return Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);
		return true;
	}

	static bool TryParseCore(string? input, out FixMessage? message, out FixParseError? error, FixParseMode mode, FixParseOptions? options)
	{
		message = null;
		error = null;
		if (input == null) return Fail(0, null, null, "Input is null.", out error);
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) return Fail(0, null, null, "Unknown parsing mode.", out error);
		var separator = options?.Separator ?? '\u0001';
		if (!Envelope(input, separator, null, out var type, out error)) return false;
		var context = new FixContext(input, mode, options, separator) { MessageType = type };
		var match = separator == '|' ? FixGrammar.TryParseLogFields(input, context) : FixGrammar.TryParseFields(input, context);
		if (!match.IsSuccess) return Fail((int)match.Position, null, type, match.Error ?? "Message does not match FIX field grammar.", out error);
		if (separator == '|' && !Envelope(input, separator, match.Value, out _, out error)) return false;
		return FixSemantics.TryBuild(input, type, Nodes(match.Value), mode, options, out message, out error);
	}

	static bool Envelope(ReadOnlySpan<byte> input, char separator, FixValue[]? fields, out string type, out FixParseError? error)
	{
		type = "";
		error = null;
		if (input.Length < 12 || !input.Slice(0, 9).SequenceEqual("8=FIX.4.4"u8) || input[9] != separator || input[10] != '9' || input[11] != '=') return Fail(0, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);
		var lengthEnd = input.Slice(12).IndexOf((byte)separator);
		if (lengthEnd < 0) return Fail(input.Length, 9, null, "Truncated BodyLength.", out error);
		lengthEnd += 12;
		var bodyLength = FixContext.Tag(input.Slice(12, lengthEnd - 12));
		if (bodyLength < 0) return Fail(12, 9, null, "Invalid BodyLength.", out error);
		var bodyStart = lengthEnd + 1;
		if (input.Length - bodyStart < 4 || !input.Slice(bodyStart, 3).SequenceEqual("35="u8)) return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);
		var typeLength = input.Slice(bodyStart + 3).IndexOf((byte)separator);
		if (typeLength < 0) return Fail(input.Length, 35, null, "Truncated MsgType.", out error);
		FixConvert.Text(input.Slice(bodyStart + 3, typeLength), out type);
		if (bodyLength != input.Length - bodyStart - 7) return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);
		var checksumStart = bodyStart + bodyLength;
		if (!input.Slice(checksumStart, 3).SequenceEqual("10="u8) || input[input.Length - 1] != separator) return Fail(checksumStart, 10, type, "Expected final CheckSum field.", out error);
		var expected = FixContext.Tag(input.Slice(checksumStart + 3, 3));
		if (expected < 0) return Fail(checksumStart + 3, 10, type, "CheckSum must contain exactly three digits.", out error);
		var checksum = 0;
		for (var i = 0; i < checksumStart; i++) checksum = (checksum + input[i]) & 255;
		if (separator != '\u0001')
		{
			if (fields == null) return true;
			foreach (var field in fields)
				if (field.ValuePosition + field.Length < checksumStart) checksum = (checksum - separator + 1) & 255;
		}
		return checksum == expected || Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);
	}

	static bool TryParseBytes(byte[] input, out FixMessage? message, out FixParseError? error, FixParseMode mode, FixParseOptions? options)
	{
		message = null;
		var separator = options?.Separator ?? '\u0001';
		if (!Envelope(input, separator, null, out var type, out error)) return false;
		var context = new FixContext(input, mode, options, separator) { MessageType = type };
		using var stream = new MemoryStream(input, writable: false);
		var match = separator == '|' ? FixGrammar.TryParseLogFields(stream, context) : FixGrammar.TryParseFields(stream, context);
		if (!match.IsSuccess) return Fail((int)match.Position, null, type, match.Error ?? "Message does not match FIX field grammar.", out error);
		if (separator == '|' && !Envelope(input, separator, match.Value, out _, out error)) return false;
		FixConvert.Text(input, out var wire);
		return FixSemantics.TryBuild(wire, type, Nodes(match.Value), mode, options, out message, out error);
	}

	/// <summary>Parse a pipe-delimited rendering, checking the checksum of the original SOH-delimited message.</summary>
	public static FixMessage ParseLog(string input, FixParseMode mode = FixParseMode.Strict) => Parse(input, new FixParseOptions('|', mode));

	public static bool TryParse(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixParseMode mode = FixParseMode.Strict) => TryParse(input.ToString(), out message, out error, mode);

	static FixNode[] Nodes(FixValue[] values)
	{
		var fields = new FixNode[values.Length];
		for (var i = 0; i < fields.Length; i++)
		{
			var v = values[i];
			fields[i] = new FixNode(v.Tag, v.Position, v.ValuePosition, v.Length, typedValue: v);
		}
		return fields;
	}

	static bool Fail(int position, int? tag, string? type, string reason, out FixParseError? error)
	{
		error = new FixParseError(position, tag, type, reason);
		return false;
	}
}

readonly struct SchemaRef
{
	public SchemaRef(int id, bool required, int kind) { Id = id; Required = required; Kind = kind; }
	public readonly int Id;
	public readonly bool Required;
	public readonly int Kind;
}

static class FixValidation
{
	public static bool Validate(FixMessage message, FixParseMode mode, FixParseOptions? options, out FixParseError? error)
	{
		error = null;
		if (mode == FixParseMode.Strict && message.Header.GetField(347) == null && (Encoded(message.Header) || Encoded(message)))
			return Missing(message.Header, message.MessageType, 347, "MessageEncoding is required when Encoded fields are present.", out error);
		if (!Scope(message.Header, FixSchema.Component(1024), message.MessageType, mode, options, out error)) return false;
		if (!Scope(message.Trailer, FixSchema.Component(1025), message.MessageType, mode, options, out error)) return false;
		var schema = FixSchema.Message(message.MessageType);
		return Scope(message, schema, message.MessageType, mode, options, out error);
	}

	static bool Encoded(FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
		{
			if (FixSchema.RequiresEncoding(node.Tag)) return true;
			if (node.Entries != null)
				foreach (var entry in node.Entries)
					if (Encoded(entry)) return true;
		}
		return false;
	}

	static bool Scope(FixFieldSet scope, SchemaRef[] schema, string type, FixParseMode mode, FixParseOptions? options, out FixParseError? error, bool ordered = false)
	{
		error = null;
		Span<ulong> seen = stackalloc ulong[15];
		seen.Clear();
		var previousRank = -1;
		HashSet<int>? extendedSeen = null;
		for (var i = 0; i < scope.Nodes.Length; i++)
		{
			var node = scope.Nodes[i];
			var field = node.Field(scope.Source);
			if (ordered && mode == FixParseMode.Strict)
			{
				var ordinal = 0;
				var rank = Rank(schema, node.Tag, ref ordinal);
				if (rank >= 0 && rank < previousRank) return Fail(field, type, "Repeating group fields are out of schema order.", out error);
				if (rank >= 0) previousRank = rank;
			}
			if (node.Tag < 957 && node.Tag > 0)
			{
				var bit = 1UL << (node.Tag & 63);
				if ((seen[node.Tag >> 6] & bit) != 0 && mode == FixParseMode.Strict) return Fail(field, type, "Duplicate field in the same scope.", out error);
				seen[node.Tag >> 6] |= bit;
			}
			else if (mode == FixParseMode.Strict && !(extendedSeen ??= new HashSet<int>()).Add(node.Tag))
				return Fail(field, type, "Duplicate extension field in the same scope.", out error);
			var lengthTag = FixSchema.LengthTag(node.Tag);
			if (lengthTag == 0) lengthTag = options?.LengthTag(node.Tag) ?? 0;
			if (lengthTag != 0 && (i == 0 || scope.Nodes[i - 1].Tag != lengthTag || !scope.Nodes[i - 1].Field(scope.Source).TryGetInt64(out var count) || count != node.Length)) return Fail(field, type, "Data field must immediately follow its matching length field.", out error);
			var dataTag = FixSchema.DataTag(node.Tag);
			if (dataTag == 0) dataTag = options?.DataTag(node.Tag) ?? 0;
			if (dataTag != 0 && (i + 1 == scope.Nodes.Length || scope.Nodes[i + 1].Tag != dataTag)) return Fail(field, type, "Length field must immediately precede its matching data field.", out error);
			if (mode == FixParseMode.Strict && !FixPrimitives.Valid(field, FixSchema.Type(node.Tag) ?? options?.Type(node.Tag), FixSchema.Codes(node.Tag))) return Fail(field, type, "Invalid FIX primitive value or code set value.", out error);
		}
		return References(scope, schema, type, mode, options, out error);
	}

	static bool References(FixFieldSet scope, SchemaRef[] schema, string type, FixParseMode mode, FixParseOptions? options, out FixParseError? error)
	{
		error = null;
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				if (reference.Id is 1024 or 1025) continue;
				var child = FixSchema.Component(reference.Id);
				var present = Present(scope, child);
				if (mode == FixParseMode.Strict && reference.Required && !present) return Missing(scope, type, null, "Required component is missing.", out error);
				if (present && !References(scope, child, type, mode, options, out error)) return false;
				continue;
			}
			var tag = reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id;
			var field = scope.GetField(tag);
			if (field == null)
			{
				if (reference.Required && mode == FixParseMode.Strict) return Missing(scope, type, tag, "Required field is missing.", out error);
				continue;
			}
			if (reference.Kind != 2) continue;
			var entries = scope.GetGroup(tag);
			if (!field.Value.TryGetInt64(out var count) || count != entries.Count || count < 0 || reference.Required && count == 0 && mode == FixParseMode.Strict) return Fail(field.Value, type, "NumInGroup does not match the number of group entries.", out error);
			foreach (var entry in entries)
				if (!Scope(entry, FixSchema.Group(reference.Id), type, mode, options, out error, ordered: true)) return false;
		}
		return true;
	}

	static bool Present(FixFieldSet scope, SchemaRef[] schema)
	{
		foreach (var reference in schema)
			if (reference.Kind == 1 ? Present(scope, FixSchema.Component(reference.Id)) : scope.GetField(reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id) != null) return true;
		return false;
	}

	static int Rank(SchemaRef[] schema, int tag, ref int ordinal)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				var rank = Rank(FixSchema.Component(reference.Id), tag, ref ordinal);
				if (rank >= 0) return rank;
			}
			else
			{
				if ((reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id) == tag) return ordinal;
				ordinal++;
			}
		}
		return -1;
	}

	static bool Fail(FixField field, string type, string reason, out FixParseError? error)
	{
		error = new FixParseError(field.ValuePosition, field.Tag, type, reason);
		return false;
	}

	static bool Missing(FixFieldSet scope, string type, int? tag, string reason, out FixParseError? error)
	{
		error = new FixParseError(scope.Nodes.Length == 0 ? 0 : scope.Nodes[scope.Nodes.Length - 1].ValuePosition + scope.Nodes[scope.Nodes.Length - 1].Length + 1, tag, type, reason);
		return false;
	}
}
