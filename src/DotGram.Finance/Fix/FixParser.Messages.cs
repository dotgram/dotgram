using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Finance.Fix;

/// <summary>
/// The message half of the package's one door: the same input read as messages rather than as a
/// flat list of fields.
/// </summary>
/// <remarks>
/// <para>
/// One class, and the name says what comes back rather than which class it came from. The verb
/// says where the input is and what happens to it — <c>Parse</c> takes a buffer whole,
/// <c>Read</c> consumes a reader or a stream and leaves it open — and the plural says how many
/// come back. So <c>ParseFields</c> and <c>ReadFields</c>, <c>ParseMessage</c> and
/// <c>ReadMessage</c>, <c>ParseMessages</c> and <c>ReadMessages</c>.
/// </para>
/// <para>
/// The names are not decoration: with one door, <c>Parse(reader)</c> would have to answer both a
/// field sequence and a message, which is one signature with two return types and no program at
/// all. What the scheme buys beside that is a reader who can see the whole surface at once.
/// </para>
/// <para>
/// Reading is one act and holding a message to a schema is another:
/// <see cref="FixMessage.Validate()"/> is a call on the message, not an entry point here.
/// </para>
/// </remarks>
public static partial class FixParser
{
	/// <summary>The largest message a streamed call will read, in octets, when none is given.</summary>
	public const int DefaultMaxMessageLength = FixMessages.DefaultMaxMessageLength;

	/// <summary>Reads one message from a buffer, checking framing, BodyLength and CheckSum.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	public static FixMessage ParseMessage(string input, FixFieldOptions? options = null)
	{
		return FixMessages.Parse(input, options);
	}

	/// <summary>Reads one message from a buffer, checking framing, BodyLength and CheckSum.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	public static FixMessage ParseMessage(ReadOnlySpan<char> input, FixFieldOptions? options = null)
	{
		return FixMessages.Parse(input, options);
	}

	/// <summary>Reads one message from the octets it arrived as.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	/// <remarks>
	/// <para>
	/// <strong>This road asks nothing of the caller that they can get wrong.</strong> FIX counts
	/// its envelope in octets — BodyLength is a length in bytes and CheckSum a sum of bytes — so a
	/// string input is a claim that somebody has already decoded the wire one character to one
	/// octet. Reading a file as UTF-8, which is what is done by default, breaks that claim, and
	/// what the package can say about it is that the length does not match: a true statement that
	/// sends the reader to look at their counterparty. Octets carry no such claim.
	/// </para>
	/// <para>
	/// A field of type <c>data</c> comes back as the octets it was, cut by the length its paired
	/// length field gives and not by the separator, so a payload may hold the separator itself.
	/// Nothing here decodes anything: <c>MessageEncoding</c> (347) is delivered as a value, and
	/// what an <c>Encoded</c> field's octets mean is the consumer's to decide.
	/// </para>
	/// <para>
	/// <strong>What this road buys is correctness, not allocation.</strong> A message keeps its
	/// source — <see cref="FixMessage.OriginalWire"/> is that source — so the octets are still
	/// materialised into a string, one character to one octet. The difference is who decides how:
	/// here it is the package, which knows that the specification counts octets, rather than a
	/// consumer choosing an encoding for a file whose framing arithmetic that choice then changes.
	/// </para>
	/// </remarks>
	public static FixMessage ParseMessage(byte[] input, FixFieldOptions? options = null)
	{
		return FixMessages.Parse(input, options);
	}

	/// <summary>Reads one message from a copy of the octets it arrived as.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	/// <remarks>
	/// <see cref="ParseMessage(byte[], FixFieldOptions)"/> says what the octet road is for. The
	/// parser reads an array, so the span is copied into one first.
	/// </remarks>
	public static FixMessage ParseMessage(ReadOnlySpan<byte> input, FixFieldOptions? options = null)
	{
		return FixMessages.Parse(input, options);
	}

	/// <summary>Reads one message from a buffer; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParseMessage(string? input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return FixMessages.TryParse(input, out message, out error, options);
	}

	/// <summary>Reads one message from a buffer; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParseMessage(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return FixMessages.TryParse(input, out message, out error, options);
	}

	/// <summary>Reads one message from octets; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire; null is refused with a diagnostic.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks><see cref="ParseMessage(byte[], FixFieldOptions)"/> says what the octet road is for.</remarks>
	public static bool TryParseMessage(byte[]? input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return FixMessages.TryParse(input, out message, out error, options);
	}

	/// <summary>Reads one message from a copy of the octets; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks>The parser reads an array, so the span is copied into one first.</remarks>
	public static bool TryParseMessage(ReadOnlySpan<byte> input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return FixMessages.TryParse(input, out message, out error, options);
	}

	/// <summary>Reads every message of a buffer, in order.</summary>
	/// <param name="input">Concatenated messages, as octets held one to a character.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>
	/// The whole input is in hand, so every message is built before the call returns; a log too
	/// large to hold that way is what <see cref="ReadMessages(TextReader, FixFieldOptions, int)"/>
	/// is for.
	/// </remarks>
	public static FixMessage[] ParseMessages(string input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		var messages = new List<FixMessage>();

		using (var reader = new StringReader(input))
			foreach (var message in FixMessages.ReadMessages(reader, options, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads every message of a buffer, in order.</summary>
	/// <param name="input">Concatenated messages, as octets held one to a character.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	public static FixMessage[] ParseMessages(ReadOnlySpan<char> input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return ParseMessages(input.ToString(), options, maxMessageLength);
	}

	/// <summary>Reads every message of a buffer of octets, in order.</summary>
	/// <param name="input">Concatenated messages, as the octets that came off the wire.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>
	/// <see cref="ParseMessage(byte[], FixFieldOptions)"/> says what the octet road is for. The whole
	/// input is in hand, so every message is built before the call returns; a log too large to hold
	/// that way is what <see cref="ReadMessages(Stream, FixFieldOptions, int)"/> is for, and this is
	/// that call over the octets already read.
	/// </remarks>
	public static FixMessage[] ParseMessages(byte[] input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		var messages = new List<FixMessage>();

		using (var stream = new MemoryStream(input, writable: false))
			foreach (var message in FixMessages.ReadMessages(stream, options, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads every message of a copy of a buffer of octets, in order.</summary>
	/// <param name="input">Concatenated messages, as the octets that came off the wire.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>The input is read from an array, so the span is copied into one first.</remarks>
	public static FixMessage[] ParseMessages(ReadOnlySpan<byte> input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return ParseMessages(input.ToArray(), options, maxMessageLength);
	}

	/// <summary>Reads exactly one message from a reader, without closing it or reading past it.</summary>
	/// <param name="input">The reader, which is left open.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(TextReader input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.Parse(input, options, maxMessageLength);
	}

	/// <summary>Reads exactly one message from a stream, without closing it or reading past it.</summary>
	/// <param name="input">The stream, which is left open.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(Stream input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.Parse(input, options, maxMessageLength);
	}

	/// <summary>Reads one message from a reader; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The reader, which is left open.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryReadMessage(TextReader input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.TryParse(input, out message, out error, options, maxMessageLength);
	}

	/// <summary>Reads one message from a stream; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The stream, which is left open.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryReadMessage(Stream input, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.TryParse(input, out message, out error, options, maxMessageLength);
	}

	/// <summary>Reads concatenated messages from a reader, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The reader, which is left open and owned by the caller.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.ReadMessages(input, options, maxMessageLength);
	}

	/// <summary>Reads concatenated messages from a stream, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The stream, which is left open and owned by the caller.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixFieldOptions? options = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.ReadMessages(input, options, maxMessageLength);
	}

	/// <summary>
	/// Builds one message from fields already read from that exact source, without reading it again.
	/// </summary>
	/// <param name="source">The source those fields were read from.</param>
	/// <param name="fields">The fields, in the order they were read.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The fields are not one readable message of that source.</exception>
	/// <remarks>
	/// Its own verb because its own act: nothing is read and nothing is consumed — the reading
	/// happened in <see cref="ParseFields(string, FixFieldOptions)"/>, and this puts a message
	/// together from what came back.
	/// </remarks>
	public static FixMessage BuildMessage(string source, FixField[] fields, FixFieldOptions? options = null)
	{
		return FixMessages.Build(source, fields, options);
	}

	/// <summary>The same, answering with a diagnostic rather than throwing.</summary>
	/// <param name="source">The source those fields were read from.</param>
	/// <param name="fields">The fields, in the order they were read.</param>
	/// <param name="message">The message built, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="options">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryBuildMessage(string source, FixField[] fields, out FixMessage? message, out FixParseError? error, FixFieldOptions? options = null)
	{
		return FixMessages.TryBuild(source, fields, out message, out error, options);
	}
}
