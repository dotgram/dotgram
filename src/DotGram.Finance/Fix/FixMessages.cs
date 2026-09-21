using System;
using System.Globalization;

namespace DotGram.Finance.Fix;

/// <summary>Parses one complete FIX 4.4 tag-value message.</summary>
static partial class FixMessages
{
	/// <summary>
	/// Parses one complete message from a lossless octet string: every character in U+0000..U+00FF.
	/// </summary>
	/// <param name="input">The message.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input is not a message under <paramref name="options"/>.</exception>
	/// <remarks>
	/// The options are one optional argument rather than a second method, which is the shape
	/// <c>FixParser</c> beside it already uses. A pair of methods where one passes a default is ten
	/// pairs across this class, and the tenth has to be written twice by whoever adds a setting.
	/// </remarks>
	public static FixMessage Parse(string input, FixFieldOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (TryParse(input, out var message, out var error, options)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Parses one complete message from a copy of the input.</summary>
	/// <param name="input">The message.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="FormatException">The input is not a message under <paramref name="options"/>.</exception>
	/// <remarks>The message keeps its source, so the input is copied into a string once.</remarks>
	public static FixMessage Parse(ReadOnlySpan<char> input, FixFieldOptions? options = null)
	{
		return Parse(input.ToString(), options);
	}

	/// <summary>Tries to parse one complete message from a copy of the input.</summary>
	/// <param name="input">The message.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks>The message keeps its source, so the input is copied into a string once.</remarks>
	public static bool TryParse(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return TryParse(input.ToString(), out message, out error, options);
	}

	/// <summary>Tries to parse one complete message.</summary>
	/// <param name="input">The message; null is refused with a diagnostic rather than thrown for.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParse(string? input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return TryParseCore(input, out message, out error, options);
	}

	static bool Envelope(string input, FixFraming framing, FixField[]? fields, out string type, out FixParseError? error)
	{
		type  = "";
		error = null;

		var separator = framing.Separator();

		for (var i = 0; i < input.Length; i++)
			if (input[i] > 255)
				return Fail(i, null, null, "Input must preserve octets as characters U+0000 through U+00FF.", out error);

		if (!input.StartsWith("8=FIX.4.4" + separator + "9=", StringComparison.Ordinal))
			return Fail(0, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);

		var lengthEnd = input.IndexOf(separator, 12);

		if (lengthEnd < 0)
			return Fail(input.Length, 9, null, "Truncated BodyLength.", out error);

		if (!int.TryParse(input.AsSpan(12, lengthEnd - 12), NumberStyles.None, CultureInfo.InvariantCulture, out var bodyLength))
			return Fail(12, 9, null, "BodyLength must be a nonnegative integer within the input range.", out error);

		var bodyStart = lengthEnd + 1;

		if (input.Length - bodyStart < 4 || !input.AsSpan(bodyStart, 3).SequenceEqual("35=".AsSpan()))
			return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);

		var typeEnd = input.IndexOf(separator, bodyStart + 3);

		if (typeEnd < 0)
			return Fail(input.Length, 35, null, "Truncated MsgType.", out error);

		type = input.Substring(bodyStart + 3, typeEnd - bodyStart - 3);

		if (bodyLength != input.Length - bodyStart - 7)
			return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);

		var checksumStart = bodyStart + bodyLength;

		if (!input.AsSpan(checksumStart, 3).SequenceEqual("10=".AsSpan()) || input[input.Length - 1] != separator)
			return Fail(checksumStart, 10, type, "Expected final CheckSum field with three digits and SOH.", out error);

		if (!int.TryParse(input.AsSpan(checksumStart + 3, 3), NumberStyles.None, CultureInfo.InvariantCulture, out var expected))
			return Fail(checksumStart + 3, 10, type, "CheckSum must contain exactly three digits.", out error);

		var checksum = 0;

		for (var i = 0; i < checksumStart; i++) checksum = (checksum + input[i]) & 255;

		if (framing == FixFraming.Log)
		{
			if (fields == null) return true;

			foreach (var field in fields)
			{
				if (field.ValuePosition + field.Length < checksumStart)
					checksum = (checksum - separator + 1) & 255;

				if (field.IsBinary && field.DataPosition - 1 < checksumStart)
					checksum = (checksum - separator + 1) & 255;
			}
		}

		if (checksum != expected)
			return Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);

		return true;
	}

	static bool TryParseCore(string? input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options)
	{
		message = null;
		error   = null;

		if (input == null)
			return Fail(0, null, null, "Input is null.", out error);

		var framing = options?.Framing ?? FixFraming.Wire;

		if (!Envelope(input, framing, null, out var type, out error))
			return false;

		var fields = FixParser.ParseFields(input, options);

		if (!CheckSyntax(fields, out error))
			return false;

		if (framing == FixFraming.Log && !Envelope(input, framing, fields, out _, out error))
			return false;

		return FixSemantics.TryBuild(input, type, Nodes(input, fields, Custom(options)), options, out message, out error);
	}

	static bool Envelope(ReadOnlySpan<byte> input, FixFraming framing, FixField[]? fields, out string type, out FixParseError? error)
	{
		type  = "";
		error = null;

		var separator = framing.Separator();

		if (input.Length < 12 || !input.Slice(0, 9).SequenceEqual("8=FIX.4.4"u8) || input[9] != separator || input[10] != '9' || input[11] != '=')
			return Fail(0, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);

		var lengthEnd = input.Slice(12).IndexOf((byte)separator);

		if (lengthEnd < 0)
			return Fail(input.Length, 9, null, "Truncated BodyLength.", out error);

		lengthEnd += 12;

		var bodyLength = FixConvert.Tag(input.Slice(12, lengthEnd - 12));

		if (bodyLength < 0)
			return Fail(12, 9, null, "Invalid BodyLength.", out error);

		var bodyStart = lengthEnd + 1;

		if (input.Length - bodyStart < 4 || !input.Slice(bodyStart, 3).SequenceEqual("35="u8))
			return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);

		var typeLength = input.Slice(bodyStart + 3).IndexOf((byte)separator);

		if (typeLength < 0)
			return Fail(input.Length, 35, null, "Truncated MsgType.", out error);

		type = FixConvert.Text(input.Slice(bodyStart + 3, typeLength));

		if (bodyLength != input.Length - bodyStart - 7)
			return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);

		var checksumStart = bodyStart + bodyLength;

		if (!input.Slice(checksumStart, 3).SequenceEqual("10="u8) || input[input.Length - 1] != separator)
			return Fail(checksumStart, 10, type, "Expected final CheckSum field.", out error);

		var expected = FixConvert.Tag(input.Slice(checksumStart + 3, 3));

		if (expected < 0)
			return Fail(checksumStart + 3, 10, type, "CheckSum must contain exactly three digits.", out error);

		var checksum = 0;

		for (var i = 0; i < checksumStart; i++)
			checksum = (checksum + input[i]) & 255;

		if (framing == FixFraming.Log)
		{
			if (fields == null)
				return true;

			foreach (var field in fields)
			{
				if (field.ValuePosition + field.Length < checksumStart)
					checksum = (checksum - separator + 1) & 255;

				if (field.IsBinary && field.DataPosition - 1 < checksumStart)
					checksum = (checksum - separator + 1) & 255;
			}
		}
		return checksum == expected || Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);
	}

	/// <summary>Parses one complete message from the octets it arrived as.</summary>
	/// <param name="input">The message's octets.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input is not a message under <paramref name="options"/>.</exception>
	/// <remarks>
	/// The road the specification describes: BodyLength counts octets and CheckSum sums octets, and
	/// on this road they are counted and summed over the octets themselves rather than over
	/// characters somebody has already decoded them into.
	/// </remarks>
	public static FixMessage Parse(byte[] input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		if (TryParseBytes(input, out var message, out var error, options))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Parses one complete message from a copy of the octets it arrived as.</summary>
	/// <param name="input">The message's octets.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="FormatException">The input is not a message under <paramref name="options"/>.</exception>
	/// <remarks>The parser reads an array, so the span is copied into one first.</remarks>
	public static FixMessage Parse(ReadOnlySpan<byte> input, FixFieldOptions? options = null)
	{
		return Parse(input.ToArray(), options);
	}

	/// <summary>Tries to parse one complete message from the octets it arrived as.</summary>
	/// <param name="input">The message's octets; null is refused with a diagnostic rather than thrown for.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParse(byte[]? input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		message = null;
		error   = null;

		return input == null
			? Fail(0, null, null, "Input is null.", out error)
			: TryParseBytes(input, out message, out error, options);
	}

	/// <summary>Tries to parse one complete message from a copy of the octets it arrived as.</summary>
	/// <param name="input">The message's octets.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data dictionary.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks>The parser reads an array, so the span is copied into one first.</remarks>
	public static bool TryParse(ReadOnlySpan<byte> input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return TryParse(input.ToArray(), out message, out error, options);
	}

	static bool TryParseBytes(byte[] input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options)
	{
		message = null;

		var framing = options?.Framing ?? FixFraming.Wire;

		if (!Envelope(input, framing, null, out var type, out error))
			return false;

		var fields = FixParser.ParseFields(input, options);

		if (!CheckSyntax(fields, out error))
			return false;

		if (framing == FixFraming.Log && !Envelope(input, framing, fields, out _, out error))
			return false;

		var wire = FixConvert.Text(input);

		return FixSemantics.TryBuild(wire, type, Nodes(wire, fields, Custom(options)), options, out message, out error);
	}

	/// <summary>Build one message from fields already parsed from the supplied source.</summary>
	public static FixMessage Build(string source, FixField[] fields, FixFieldOptions? options = null)
	{
		if (TryBuild(source, fields, out var message, out var error, options))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>
	/// Tries to build one message from fields already parsed from the supplied source.
	/// </summary>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryBuild(string source, FixField[] fields, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (fields == null) throw new ArgumentNullException(nameof(fields));

		message = null;

		var framing   = options?.Framing ?? FixFraming.Wire;
		var separator = framing.Separator();

		if (!Envelope(source, framing, null, out var type, out error))
			return false;

		if (!CheckSyntax(fields, out error))
			return false;

		var position = 0;

		foreach (var field in fields)
		{
			if (field == null || field.Position != position || field.Length < 0 ||
				field.ValuePosition < position || field.ValuePosition > source.Length - 1 ||
				field.Length >= source.Length - field.ValuePosition)
			{
				return Fail(position, null, type, "Field locations do not cover the supplied source.", out error);
			}

			var tagPosition = field.IsBinary ? field.DataPosition : position;

			if (tagPosition < position || field.ValuePosition - tagPosition < 2 || source[field.ValuePosition - 1] != '=' ||
				!IsTag(source.AsSpan(tagPosition, field.ValuePosition - 1 - tagPosition), field.Tag) ||
				source[field.ValuePosition + field.Length] != separator)
			{
				return Fail(position, field.Tag, type, "Field locations do not match the supplied source.", out error);
			}

			if (field.IsBinary)
			{
				var lengthTag = FixSchema.LengthTag(field.Tag);
				var header    = source.AsSpan(position, tagPosition - position);
				var equals    = header.IndexOf('=');

				if (equals <= 0 || equals >= header.Length - 2 || !IsTag(header.Slice(0, equals), lengthTag) || header[header.Length - 1] != separator ||
					FixConvert.Tag(header.Slice(equals + 1, header.Length - equals - 2)) != field.Length)
				{
					return Fail(position, lengthTag, type, "Binary length does not match the supplied source.", out error);
				}
			}

			position = field.ValuePosition + field.Length + 1;
		}

		if (position != source.Length)
			return Fail(position, null, type, "Field locations do not cover the supplied source.", out error);

		if (framing == FixFraming.Log && !Envelope(source, framing, fields, out _, out error))
			return false;

		return FixSemantics.TryBuild(source, type, Nodes(source, fields, Custom(options)), options, out message, out error);
	}

	// Whether text is exactly the decimal digits of tag, as ToString writes them: no sign, and
	// no leading zero.
	static bool IsTag(ReadOnlySpan<char> text, int tag)
	{
		for (var i = text.Length - 1; i >= 0; i--)
		{
			if (text[i] != (char)('0' + tag % 10))
				return false;

			tag /= 10;

			if (tag == 0)
				return i == 0;
		}

		return false;
	}

	static bool CheckSyntax(FixField[] fields, out FixParseError? error)
	{
		foreach (var field in fields)
			if (field is FixField.Invalid invalid)
				return Fail(invalid.Position, null, null, invalid.Message, out error);

		error = null;

		return true;
	}

	static FixNode[] Nodes(string source, FixField[] values, FixCustomFields custom)
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
				var length = LengthField(tag, header.Slice(equals + 1), custom);

				length.Locate(value.Position, value.DataPosition - value.Position);

				fields[index++] = new FixNode(tag, length.Position, length.ValuePosition, length.Length, typedValue: length);
			}

			fields[index++] = new FixNode(value.Tag, value.IsBinary ? value.DataPosition : value.Position, value.ValuePosition, value.Length, typedValue: value);
		}

		return fields;
	}

	// The optional message model exposes both wire fields; the parser returns only data.
	static FixField LengthField(int tag, ReadOnlySpan<char> value, FixCustomFields custom)
	{
		return tag switch
		{
			90 => new FixField.SecureDataLen(FixConvert.Integer(value)),
			93 => new FixField.SignatureLength(FixConvert.Integer(value)),
			95 => new FixField.RawDataLength(FixConvert.Integer(value)),
			212 => new FixField.XmlDataLen(FixConvert.Integer(value)),
			348 => new FixField.EncodedIssuerLen(FixConvert.Integer(value)),
			350 => new FixField.EncodedSecurityDescLen(FixConvert.Integer(value)),
			352 => new FixField.EncodedListExecInstLen(FixConvert.Integer(value)),
			354 => new FixField.EncodedTextLen(FixConvert.Integer(value)),
			356 => new FixField.EncodedSubjectLen(FixConvert.Integer(value)),
			358 => new FixField.EncodedHeadlineLen(FixConvert.Integer(value)),
			360 => new FixField.EncodedAllocTextLen(FixConvert.Integer(value)),
			362 => new FixField.EncodedUnderlyingIssuerLen(FixConvert.Integer(value)),
			364 => new FixField.EncodedUnderlyingSecurityDescLen(FixConvert.Integer(value)),
			445 => new FixField.EncodedListStatusTextLen(FixConvert.Integer(value)),
			618 => new FixField.EncodedLegIssuerLen(FixConvert.Integer(value)),
			621 => new FixField.EncodedLegSecurityDescLen(FixConvert.Integer(value)),
			_ => custom.Text(tag, value),
		};
	}

	// Never null, so that the one path holds here as it does in the reader.
	static FixCustomFields Custom(FixFieldOptions? options)
	{
		return options?.CustomFields ?? FixSpareFields.Instance;
	}

	static bool Fail(int position, int? tag, string? type, string reason, out FixParseError? error)
	{
		error = new FixParseError(position, tag, type, reason);
		return false;
	}
}
