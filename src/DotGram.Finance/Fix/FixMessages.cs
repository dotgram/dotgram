using System;
using System.Globalization;

namespace DotGram.Finance.Fix;

/// <summary>Parses one complete FIX 4.4 tag-value message.</summary>
public static partial class FixMessages
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

	static bool Envelope(string input, char separator, FixField[]? fields, out string type, out FixParseError? error)
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
			{
				if (field.ValuePosition + field.Length < checksumStart) checksum = (checksum - separator + 1) & 255;
				if (field.IsBinary && field.DataPosition - 1 < checksumStart) checksum = (checksum - separator + 1) & 255;
			}
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
		var fields = separator == '|'
			? FixParser.ParseLog(input, options?.FieldOptions)
			: FixParser.Parse(input, options?.FieldOptions);
		if (!CheckSyntax(fields, out error))
			return false;
		if (separator == '|' && !Envelope(input, separator, fields, out _, out error)) return false;
		return FixSemantics.TryBuild(input, type, Nodes(input, fields), mode, options, out message, out error);
	}

	static bool Envelope(ReadOnlySpan<byte> input, char separator, FixField[]? fields, out string type, out FixParseError? error)
	{
		type = "";
		error = null;
		if (input.Length < 12 || !input.Slice(0, 9).SequenceEqual("8=FIX.4.4"u8) || input[9] != separator || input[10] != '9' || input[11] != '=') return Fail(0, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);
		var lengthEnd = input.Slice(12).IndexOf((byte)separator);
		if (lengthEnd < 0) return Fail(input.Length, 9, null, "Truncated BodyLength.", out error);
		lengthEnd += 12;
		var bodyLength = FixConvert.Tag(input.Slice(12, lengthEnd - 12));
		if (bodyLength < 0) return Fail(12, 9, null, "Invalid BodyLength.", out error);
		var bodyStart = lengthEnd + 1;
		if (input.Length - bodyStart < 4 || !input.Slice(bodyStart, 3).SequenceEqual("35="u8)) return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);
		var typeLength = input.Slice(bodyStart + 3).IndexOf((byte)separator);
		if (typeLength < 0) return Fail(input.Length, 35, null, "Truncated MsgType.", out error);
		type = FixConvert.Text(input.Slice(bodyStart + 3, typeLength));
		if (bodyLength != input.Length - bodyStart - 7) return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);
		var checksumStart = bodyStart + bodyLength;
		if (!input.Slice(checksumStart, 3).SequenceEqual("10="u8) || input[input.Length - 1] != separator) return Fail(checksumStart, 10, type, "Expected final CheckSum field.", out error);
		var expected = FixConvert.Tag(input.Slice(checksumStart + 3, 3));
		if (expected < 0) return Fail(checksumStart + 3, 10, type, "CheckSum must contain exactly three digits.", out error);
		var checksum = 0;
		for (var i = 0; i < checksumStart; i++) checksum = (checksum + input[i]) & 255;
		if (separator != '\u0001')
		{
			if (fields == null) return true;
			foreach (var field in fields)
			{
				if (field.ValuePosition + field.Length < checksumStart) checksum = (checksum - separator + 1) & 255;
				if (field.IsBinary && field.DataPosition - 1 < checksumStart) checksum = (checksum - separator + 1) & 255;
			}
		}
		return checksum == expected || Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);
	}

	static bool TryParseBytes(byte[] input, out FixMessage? message, out FixParseError? error, FixParseMode mode, FixParseOptions? options)
	{
		message = null;
		var separator = options?.Separator ?? '\u0001';
		if (!Envelope(input, separator, null, out var type, out error)) return false;
		var fields = separator == '|'
			? FixParser.ParseLog(input, options?.FieldOptions)
			: FixParser.Parse(input, options?.FieldOptions);
		if (!CheckSyntax(fields, out error))
			return false;
		if (separator == '|' && !Envelope(input, separator, fields, out _, out error)) return false;
		var wire = FixConvert.Text(input);
		return FixSemantics.TryBuild(wire, type, Nodes(wire, fields), mode, options, out message, out error);
	}

	/// <summary>Parse a pipe-delimited rendering, checking the checksum of the original SOH-delimited message.</summary>
	public static FixMessage ParseLog(string input, FixParseMode mode = FixParseMode.Strict) => Parse(input, new FixParseOptions('|', mode));

	public static bool TryParse(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixParseMode mode = FixParseMode.Strict) => TryParse(input.ToString(), out message, out error, mode);

	/// <summary>Validate and build one message from fields already parsed from the supplied source.</summary>
	public static FixMessage Build(string source, FixField[] fields, FixParseOptions? options = null)
	{
		if (TryBuild(source, fields, out var message, out var error, options)) return message!;
		throw new FormatException(error!.ToString());
	}

	public static bool TryBuild(string source, FixField[] fields, out FixMessage? message, out FixParseError? error, FixParseOptions? options = null)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (fields == null) throw new ArgumentNullException(nameof(fields));
		message = null;
		var separator = options?.Separator ?? '\u0001';
		if (!Envelope(source, separator, null, out var type, out error)) return false;
		if (!CheckSyntax(fields, out error))
			return false;
		var position = 0;
		foreach (var field in fields)
		{
			if (field == null || field.Position != position || field.Length < 0 ||
				field.ValuePosition < position || field.ValuePosition > source.Length - 1 ||
				field.Length >= source.Length - field.ValuePosition)
				return Fail(position, null, type, "Field locations do not cover the supplied source.", out error);
			var prefix = field.Tag.ToString(CultureInfo.InvariantCulture) + "=";
			var tagPosition = field.IsBinary ? field.DataPosition : position;
			if (tagPosition < position || field.ValuePosition - tagPosition != prefix.Length || !source.AsSpan(tagPosition, prefix.Length).SequenceEqual(prefix.AsSpan()) ||
				source[field.ValuePosition + field.Length] != separator)
				return Fail(position, field.Tag, type, "Field locations do not match the supplied source.", out error);
			if (field.IsBinary)
			{
				var lengthTag = FixSchema.LengthTag(field.Tag);
				var lengthPrefix = lengthTag.ToString(CultureInfo.InvariantCulture) + "=";
				var header = source.AsSpan(position, tagPosition - position);
				if (header.Length <= lengthPrefix.Length || !header.StartsWith(lengthPrefix.AsSpan()) || header[header.Length - 1] != separator ||
					FixConvert.Tag(header.Slice(lengthPrefix.Length, header.Length - lengthPrefix.Length - 1)) != field.Length)
					return Fail(position, lengthTag, type, "Binary length does not match the supplied source.", out error);
			}
			position = field.ValuePosition + field.Length + 1;
		}
		if (position != source.Length) return Fail(position, null, type, "Field locations do not cover the supplied source.", out error);
		if (separator == '|' && !Envelope(source, separator, fields, out _, out error)) return false;
		return FixSemantics.TryBuild(source, type, Nodes(source, fields), options?.Mode ?? FixParseMode.Strict, options, out message, out error);
	}

	static bool CheckSyntax(FixField[] fields, out FixParseError? error)
	{
		foreach (var field in fields)
			if (field is FixField.Invalid invalid)
				return Fail(invalid.Position, null, null, invalid.Message, out error);

		error = null;

		return true;
	}

	static FixNode[] Nodes(string source, FixField[] values)
	{
		var count = values.Length;

		foreach (var value in values)
			if (value.IsBinary)
				count++;

		var fields = new FixNode[count];
		var index  = 0;

		foreach (var value in values)
		{
			if (value.IsBinary)
			{
				var header = source.AsSpan(value.Position, value.DataPosition - value.Position - 1);
				var equals = header.IndexOf('=');
				var tag    = FixConvert.Tag(header.Slice(0, equals));
				var length = LengthField(tag, header.Slice(equals + 1));

				length.Locate(value.Position, value.DataPosition - value.Position);

				fields[index++] = new FixNode(tag, length.Position, length.ValuePosition, length.Length, typedValue: length);
			}

			fields[index++] = new FixNode(value.Tag, value.IsBinary ? value.DataPosition : value.Position, value.ValuePosition, value.Length, typedValue: value);
		}

		return fields;
	}

	// The optional message model exposes both wire fields; the parser returns only data.
	static FixField LengthField(int tag, ReadOnlySpan<char> value) => tag switch
	{
		 90 => new FixField.SecureDataLen                   (FixConvert.Integer(value)),
		 93 => new FixField.SignatureLength                 (FixConvert.Integer(value)),
		 95 => new FixField.RawDataLength                   (FixConvert.Integer(value)),
		212 => new FixField.XmlDataLen                      (FixConvert.Integer(value)),
		348 => new FixField.EncodedIssuerLen                (FixConvert.Integer(value)),
		350 => new FixField.EncodedSecurityDescLen          (FixConvert.Integer(value)),
		352 => new FixField.EncodedListExecInstLen          (FixConvert.Integer(value)),
		354 => new FixField.EncodedTextLen                  (FixConvert.Integer(value)),
		356 => new FixField.EncodedSubjectLen               (FixConvert.Integer(value)),
		358 => new FixField.EncodedHeadlineLen              (FixConvert.Integer(value)),
		360 => new FixField.EncodedAllocTextLen             (FixConvert.Integer(value)),
		362 => new FixField.EncodedUnderlyingIssuerLen      (FixConvert.Integer(value)),
		364 => new FixField.EncodedUnderlyingSecurityDescLen(FixConvert.Integer(value)),
		445 => new FixField.EncodedListStatusTextLen        (FixConvert.Integer(value)),
		618 => new FixField.EncodedLegIssuerLen             (FixConvert.Integer(value)),
		621 => new FixField.EncodedLegSecurityDescLen       (FixConvert.Integer(value)),
		_   => new FixField.Unknown                         (tag, FixConvert.Data(value)),
	};

	static bool Fail(int position, int? tag, string? type, string reason, out FixParseError? error)
	{
		error = new FixParseError(position, tag, type, reason);
		return false;
	}
}

readonly struct SchemaRef(int id, bool required, int kind)
{
	public readonly int  Id       = id;
	public readonly bool Required = required;
	public readonly int  Kind     = kind;
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
		var           previousRank = -1;
		HashSet<int>? extendedSeen = null;

		seen.Clear();

		for (var i = 0; i < scope.Nodes.Length; i++)
		{
			var node  = scope.Nodes[i];
			var field = node.Field(scope.Source);

			if (ordered && mode == FixParseMode.Strict)
			{
				var ordinal = 0;
				var rank    = Rank(schema, node.Tag, ref ordinal);

				switch (rank)
				{
					case >= 0 when rank < previousRank: return Fail(field, type, "Repeating group fields are out of schema order.", out error);
					case >= 0                         : previousRank = rank; break;
				}
			}

			if (node.Tag is < 957 and > 0)
			{
				var bit = 1UL << (node.Tag & 63);

				if ((seen[node.Tag >> 6] & bit) != 0 && mode == FixParseMode.Strict)
					return Fail(field, type, "Duplicate field in the same scope.", out error);

				seen[node.Tag >> 6] |= bit;
			}
			else if (mode == FixParseMode.Strict && !(extendedSeen ??= new HashSet<int>()).Add(node.Tag))
			{
				return Fail(field, type, "Duplicate extension field in the same scope.", out error);
			}

			var lengthTag = FixSchema.LengthTag(node.Tag);

			if (lengthTag != 0 && (i == 0 || scope.Nodes[i - 1].Tag != lengthTag || !scope.Nodes[i - 1].Field(scope.Source).TryGetInt64(out var count) || count != node.Length))
				return Fail(field, type, "Data field must immediately follow its matching length field.", out error);

			var dataTag = FixSchema.DataTag(node.Tag);

			if (dataTag != 0 && (i + 1 == scope.Nodes.Length || scope.Nodes[i + 1].Tag != dataTag)) return Fail(field, type, "Length field must immediately precede its matching data field.", out error);
			if (mode == FixParseMode.Strict && !FixPrimitives.Valid(field, FixSchema.Type(node.Tag), FixSchema.Codes(node.Tag))) return Fail(field, type, "Invalid FIX primitive value or code set value.", out error);
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

	static bool Fail(FixFieldView field, string type, string reason, out FixParseError? error)
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
