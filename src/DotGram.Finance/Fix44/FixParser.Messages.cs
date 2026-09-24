using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DotGram.Finance.Fix44;

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
/// Reading is one act and holding a message to a schema is another: what holds a message to the
/// schema is asked of the message, not of an entry point here.
/// </para>
/// </remarks>
public static partial class FixParser
{
	/// <summary>
	/// The largest message a streamed call will read, in octets, when none is given.
	/// </summary>
	public const int DefaultMaxMessageLength = 16 * 1024 * 1024;

	/// <summary>
	/// Parses one complete message from a lossless octet string: every character in U+0000..U+00FF.
	/// </summary>
	/// <param name="input">The message.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input is not a message under <paramref name="context"/>.</exception>
	/// <remarks>
	/// The context are one optional argument rather than a second method, which is the shape
	/// <c>FixParser</c> beside it already uses. A pair of methods where one passes a default is ten
	/// pairs across this class, and the tenth has to be written twice by whoever adds a setting.
	/// </remarks>
	public static FixMessage ParseMessage(string input, FixContext? context = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		if (TryParseMessage(input, out var message, out var error, context))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message from the octets it arrived as.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
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
	/// <strong>What this road buys is correctness, not allocation.</strong> Every field keeps the
	/// value it was read from, so the octets are still materialised into strings, one character to
	/// one octet. The difference is who decides how:
	/// here it is the package, which knows that the specification counts octets, rather than a
	/// consumer choosing an encoding for a file whose framing arithmetic that choice then changes.
	/// </para>
	/// </remarks>
	public static FixMessage ParseMessage(byte[] input, FixContext? context = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		if (TryParseMessage(input, out var message, out var error, context))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message from octets the caller already holds.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	/// <remarks>
	/// <see cref="ParseMessage(byte[], FixContext)"/> says what the octet road is for. The octets
	/// are read where they lie, with no copy.
	/// </remarks>
	public static FixMessage ParseMessage(ReadOnlyMemory<byte> input, FixContext? context = null)
	{
		if (TryParseMessage(input, out var message, out var error, context))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message from a buffer; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParseMessage(string input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		// The same road as every other overload: the envelope is checked, the fields are read, and
		// the message is put together from them.
		return TryParseText(input, out message, out error, context);
	}


	/// <summary>Puts one message together from fields already read, in the order they were read.</summary>
	internal static bool TryParseMessage(IEnumerable<FixField> input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		message = null;

		var     started = false;
		var     ended   = false;
		var     fields  = new List<FixField>(input is ICollection c ? c.Count : 6);
		string? type    = null;
		var     pairs   = context ?? FixContext.Default;
		var     length  = 0;

		foreach (var field in input)
		{
			// A length field is followed by the data it measures, and by nothing else.
			if (length != 0 && field.Tag != pairs.DataTag(length))
			{
				error = new FixParseError(field.Position, length, null, $"Length ({length}) is not followed by its data ({pairs.DataTag(length)})");
				return false;
			}

			length = pairs.Kind(field.Tag) > 0 ? field.Tag : 0;

			fields.Add(field);

			switch (field.Tag)
			{
				case 8:
					if (started)
					{
						error = new FixParseError(field.Position, field.Tag, null, "Message has multiple BeginString (8)");
						return false;
					}

					started = true;
					break;

				case 10:
					if (!started)
					{
						error = new FixParseError(field.Position, field.Tag, null, "Message has CheckSum (10) before BeginString (8)");
						return false;
					}

					if (ended)
					{
						error = new FixParseError(field.Position, field.Tag, null, "Message has multiple CheckSum (10)");
						return false;
					}

					ended = true;
					break;

				case 35:
					if (!started)
					{
						error = new FixParseError(field.Position, field.Tag, null, "Message has MsgType (35) before BeginString (8)");
						return false;
					}

					type = ((FixField.MsgType)field).Value;

					break;
			}
		}

		if (length != 0)
		{
			error = new FixParseError(0, length, null, $"Length ({length}) is not followed by its data ({pairs.DataTag(length)})");
			return false;
		}

		if (!started)
		{
			error = new FixParseError(0, null, null, "Message has no BeginString (8)");
			return false;
		}

		if (!ended)
		{
			error = new FixParseError(0, null, null, "Message has no CheckSum (10)");
			return false;
		}

		if (type is null)
		{
			error = new FixParseError(0, null, null, "Message has no MsgType (35)");
			return false;
		}

		message = CreateMessage();
		error   = null;

		return true;

		FixMessage CreateMessage()
		{
			return (type.Length, type.Length > 0 ? type[0] : '\0', type.Length > 1 ? type[1] : '\0') switch
			{
				(1, '0', _  ) => new FixMessage.Heartbeat                              (fields),
				(1, '1', _  ) => new FixMessage.TestRequest                            (fields),
				(1, '2', _  ) => new FixMessage.ResendRequest                          (fields),
				(1, '3', _  ) => new FixMessage.Reject                                 (fields),
				(1, '4', _  ) => new FixMessage.SequenceReset                          (fields),
				(1, '5', _  ) => new FixMessage.Logout                                 (fields),
				(1, '6', _  ) => new FixMessage.IndicationOfInterest                   (fields),
				(1, '7', _  ) => new FixMessage.Advertisement                          (fields),
				(1, '8', _  ) => new FixMessage.ExecutionReport                        (fields),
				(1, '9', _  ) => new FixMessage.OrderCancelReject                      (fields),
				(1, 'A', _  ) => new FixMessage.Logon                                  (fields),
				(1, 'B', _  ) => new FixMessage.News                                   (fields),
				(1, 'C', _  ) => new FixMessage.Email                                  (fields),
				(1, 'D', _  ) => new FixMessage.NewOrderSingle                         (fields),
				(1, 'E', _  ) => new FixMessage.NewOrderList                           (fields),
				(1, 'F', _  ) => new FixMessage.OrderCancelRequest                     (fields),
				(1, 'G', _  ) => new FixMessage.OrderCancelReplaceRequest              (fields),
				(1, 'H', _  ) => new FixMessage.OrderStatusRequest                     (fields),
				(1, 'J', _  ) => new FixMessage.AllocationInstruction                  (fields),
				(1, 'K', _  ) => new FixMessage.ListCancelRequest                      (fields),
				(1, 'L', _  ) => new FixMessage.ListExecute                            (fields),
				(1, 'M', _  ) => new FixMessage.ListStatusRequest                      (fields),
				(1, 'N', _  ) => new FixMessage.ListStatus                             (fields),
				(1, 'P', _  ) => new FixMessage.AllocationInstructionAck               (fields),
				(1, 'Q', _  ) => new FixMessage.DontKnowTrade                          (fields),
				(1, 'R', _  ) => new FixMessage.QuoteRequest                           (fields),
				(1, 'S', _  ) => new FixMessage.Quote                                  (fields),
				(1, 'T', _  ) => new FixMessage.SettlementInstructions                 (fields),
				(1, 'V', _  ) => new FixMessage.MarketDataRequest                      (fields),
				(1, 'W', _  ) => new FixMessage.MarketDataSnapshotFullRefresh          (fields),
				(1, 'X', _  ) => new FixMessage.MarketDataIncrementalRefresh           (fields),
				(1, 'Y', _  ) => new FixMessage.MarketDataRequestReject                (fields),
				(1, 'Z', _  ) => new FixMessage.QuoteCancel                            (fields),
				(1, 'a', _  ) => new FixMessage.QuoteStatusRequest                     (fields),
				(1, 'b', _  ) => new FixMessage.MassQuoteAcknowledgement               (fields),
				(1, 'c', _  ) => new FixMessage.SecurityDefinitionRequest              (fields),
				(1, 'd', _  ) => new FixMessage.SecurityDefinition                     (fields),
				(1, 'e', _  ) => new FixMessage.SecurityStatusRequest                  (fields),
				(1, 'f', _  ) => new FixMessage.SecurityStatus                         (fields),
				(1, 'g', _  ) => new FixMessage.TradingSessionStatusRequest            (fields),
				(1, 'h', _  ) => new FixMessage.TradingSessionStatus                   (fields),
				(1, 'i', _  ) => new FixMessage.MassQuote                              (fields),
				(1, 'j', _  ) => new FixMessage.BusinessMessageReject                  (fields),
				(1, 'k', _  ) => new FixMessage.BidRequest                             (fields),
				(1, 'l', _  ) => new FixMessage.BidResponse                            (fields),
				(1, 'm', _  ) => new FixMessage.ListStrikePrice                        (fields),
				(1, 'n', _  ) => new FixMessage.XMLnonFIX                              (fields),
				(1, 'o', _  ) => new FixMessage.RegistrationInstructions               (fields),
				(1, 'p', _  ) => new FixMessage.RegistrationInstructionsResponse       (fields),
				(1, 'q', _  ) => new FixMessage.OrderMassCancelRequest                 (fields),
				(1, 'r', _  ) => new FixMessage.OrderMassCancelReport                  (fields),
				(1, 's', _  ) => new FixMessage.NewOrderCross                          (fields),
				(1, 't', _  ) => new FixMessage.CrossOrderCancelReplaceRequest         (fields),
				(1, 'u', _  ) => new FixMessage.CrossOrderCancelRequest                (fields),
				(1, 'v', _  ) => new FixMessage.SecurityTypeRequest                    (fields),
				(1, 'w', _  ) => new FixMessage.SecurityTypes                          (fields),
				(1, 'x', _  ) => new FixMessage.SecurityListRequest                    (fields),
				(1, 'y', _  ) => new FixMessage.SecurityList                           (fields),
				(1, 'z', _  ) => new FixMessage.DerivativeSecurityListRequest          (fields),
				(2, 'A', 'A') => new FixMessage.DerivativeSecurityList                 (fields),
				(2, 'A', 'B') => new FixMessage.NewOrderMultileg                       (fields),
				(2, 'A', 'C') => new FixMessage.MultilegOrderCancelReplaceRequest             (fields),
				(2, 'A', 'D') => new FixMessage.TradeCaptureReportRequest              (fields),
				(2, 'A', 'E') => new FixMessage.TradeCaptureReport                     (fields),
				(2, 'A', 'F') => new FixMessage.OrderMassStatusRequest                 (fields),
				(2, 'A', 'G') => new FixMessage.QuoteRequestReject                     (fields),
				(2, 'A', 'H') => new FixMessage.RFQRequest                             (fields),
				(2, 'A', 'I') => new FixMessage.QuoteStatusReport                      (fields),
				(2, 'A', 'J') => new FixMessage.QuoteResponse                          (fields),
				(2, 'A', 'K') => new FixMessage.Confirmation                           (fields),
				(2, 'A', 'L') => new FixMessage.PositionMaintenanceRequest             (fields),
				(2, 'A', 'M') => new FixMessage.PositionMaintenanceReport              (fields),
				(2, 'A', 'N') => new FixMessage.RequestForPositions                    (fields),
				(2, 'A', 'O') => new FixMessage.RequestForPositionsAck                 (fields),
				(2, 'A', 'P') => new FixMessage.PositionReport                         (fields),
				(2, 'A', 'Q') => new FixMessage.TradeCaptureReportRequestAck           (fields),
				(2, 'A', 'R') => new FixMessage.TradeCaptureReportAck                  (fields),
				(2, 'A', 'S') => new FixMessage.AllocationReport                       (fields),
				(2, 'A', 'T') => new FixMessage.AllocationReportAck                    (fields),
				(2, 'A', 'U') => new FixMessage.ConfirmationAck                        (fields),
				(2, 'A', 'V') => new FixMessage.SettlementInstructionRequest           (fields),
				(2, 'A', 'W') => new FixMessage.AssignmentReport                       (fields),
				(2, 'A', 'X') => new FixMessage.CollateralRequest                      (fields),
				(2, 'A', 'Y') => new FixMessage.CollateralAssignment                   (fields),
				(2, 'A', 'Z') => new FixMessage.CollateralResponse                     (fields),
				(2, 'B', 'A') => new FixMessage.CollateralReport                       (fields),
				(2, 'B', 'B') => new FixMessage.CollateralInquiry                      (fields),
				(2, 'B', 'C') => new FixMessage.NetworkStatusRequest (fields),
				(2, 'B', 'D') => new FixMessage.NetworkStatusResponse(fields),
				(2, 'B', 'E') => new FixMessage.UserRequest                            (fields),
				(2, 'B', 'F') => new FixMessage.UserResponse                           (fields),
				(2, 'B', 'G') => new FixMessage.CollateralInquiryAck                   (fields),
				(2, 'B', 'H') => new FixMessage.ConfirmationRequest                    (fields),
				_             => (context?.FixMessageFactory?.Invoke(type) is { } made ? made.Read(type, fields) : new FixMessage.Invalid(type, fields)),
			};
		}
	}

	/// <summary>Reads one message from octets; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire; null is refused with a diagnostic.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks><see cref="ParseMessage(byte[], FixContext)"/> says what the octet road is for.</remarks>
	public static bool TryParseMessage(byte[]? input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		if (input == null)
		{
			message = null;
			return Fail(0, null, null, "Input is null.", out error);
		}

		return TryParseBytes(input, out message, out error, context);
	}

	/// <summary>Reads one message from octets the caller already holds; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks>The octets are read where they lie, with no copy.</remarks>
	public static bool TryParseMessage(ReadOnlyMemory<byte> input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		return TryParseBytes(input, out message, out error, context);
	}

	/// <summary>Reads every message of a buffer, in order.</summary>
	/// <param name="input">Concatenated messages, as octets held one to a character.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>
	/// The whole input is in hand, so every message is built before the call returns; a log too
	/// large to hold that way is what <see cref="ReadMessages(TextReader, FixContext, int)"/>
	/// is for.
	/// </remarks>
	public static FixMessage[] ParseMessages(string input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		var messages = new List<FixMessage>();

		using (var reader = new StringReader(input))
			foreach (var message in ReadMessages(reader, context, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads every message of a buffer of octets, in order.</summary>
	/// <param name="input">Concatenated messages, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>
	/// <see cref="ParseMessage(byte[], FixContext)"/> says what the octet road is for. The whole
	/// input is in hand, so every message is built before the call returns; a log too large to hold
	/// that way is what <see cref="ReadMessages(Stream, FixContext, int)"/> is for, and this is
	/// that call over the octets already read.
	/// </remarks>
	public static FixMessage[] ParseMessages(byte[] input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		return ParseMessages(new ReadOnlyMemory<byte>(input), context, maxMessageLength);
	}

	/// <summary>Reads every message of octets the caller already holds, in order.</summary>
	/// <param name="input">Concatenated messages, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>
	/// Octets held in an array are read where they lie; memory of any other kind is copied into one
	/// first.
	/// </remarks>
	public static FixMessage[] ParseMessages(ReadOnlyMemory<byte> input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		var held = MemoryMarshal.TryGetArray(input, out var segment)
			? segment
			: new ArraySegment<byte>(input.ToArray());

		var messages = new List<FixMessage>();

		using (var stream = new MemoryStream(held.Array!, held.Offset, held.Count, writable: false))
			foreach (var message in ReadMessages(stream, context, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads exactly one message from a reader, without closing it or reading past it.</summary>
	/// <param name="input">The reader, which is left open.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryReadMessage(input, out var message, out var error, context, maxMessageLength))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads exactly one message from a stream, without closing it or reading past it.</summary>
	/// <param name="input">The stream, which is left open.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryReadMessage(input, out var message, out var error, context, maxMessageLength))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message from a reader; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The reader, which is left open.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryReadMessage(TextReader input, out FixMessage? message, out FixParseError? error, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		message = null;

		var reader = new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator());

		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null)
				Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}

		return TryParseFrame(wire, out message, out error, context);
	}

	/// <summary>Reads one message from a stream; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The stream, which is left open.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryReadMessage(Stream input, out FixMessage? message, out FixParseError? error, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		message = null;

		var reader = new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator());

		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null)
				Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}

		return TryParseFrame(wire, out message, out error, context);
	}

	/// <summary>Reads concatenated messages from a reader, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The reader, which is left open and owned by the caller.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		return ReadFrames(new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator()), context);
	}

	/// <summary>Reads concatenated messages from a stream, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The stream, which is left open and owned by the caller.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		return ReadFrames(new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator()), context);
	}

	/// <summary>
	/// Builds one message from fields already read from that exact source, without reading it again.
	/// </summary>
	/// <param name="source">The source those fields were read from.</param>
	/// <param name="fields">The fields, in the order they were read.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The fields are not one readable message of that source.</exception>
	/// <remarks>
	/// Its own verb because its own act: nothing is read and nothing is consumed — the reading
	/// happened in <see cref="ParseFields(string, FixContext)"/>, and this puts a message
	/// together from what came back.
	/// </remarks>
	public static FixMessage BuildMessage(string source, FixField[] fields, FixContext? context = null)
	{
		if (TryBuildMessage(source, fields, out var message, out var error, context))
			return message!;

		throw new FormatException(error!.ToString());
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
			}
		}

		if (checksum != expected)
			return Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);

		return true;
	}

	static bool TryParseText(string? input, out FixMessage? message, out FixParseError? error, FixContext? context)
	{
		message = null;
		error   = null;

		if (input == null)
			return Fail(0, null, null, "Input is null.", out error);

		var framing = context?.Framing ?? FixFraming.Wire;

		if (!Envelope(input, framing, null, out var type, out error))
			return false;

		var fields = FixParser.ParseFields(input, context);

		if (!CheckSyntax(fields, out error))
			return false;

		if (framing == FixFraming.Log && !Envelope(input, framing, fields, out _, out error))
			return false;

		return FixParser.TryParseMessage(fields, out message, out error, context);
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

		var bodyLength = FixConvert.ToTag(input.Slice(12, lengthEnd - 12));

		if (bodyLength < 0)
			return Fail(12, 9, null, "Invalid BodyLength.", out error);

		var bodyStart = lengthEnd + 1;

		if (input.Length - bodyStart < 4 || !input.Slice(bodyStart, 3).SequenceEqual("35="u8))
			return Fail(bodyStart, 35, null, "MsgType must be the third field.", out error);

		var typeLength = input.Slice(bodyStart + 3).IndexOf((byte)separator);

		if (typeLength < 0)
			return Fail(input.Length, 35, null, "Truncated MsgType.", out error);

		type = FixConvert.ToText(input.Slice(bodyStart + 3, typeLength));

		if (bodyLength != input.Length - bodyStart - 7)
			return Fail(12, 9, type, "BodyLength does not match the octets before CheckSum.", out error);

		var checksumStart = bodyStart + bodyLength;

		if (!input.Slice(checksumStart, 3).SequenceEqual("10="u8) || input[input.Length - 1] != separator)
			return Fail(checksumStart, 10, type, "Expected final CheckSum field.", out error);

		var expected = FixConvert.ToTag(input.Slice(checksumStart + 3, 3));

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
			}
		}
		return checksum == expected || Fail(checksumStart + 3, 10, type, "CheckSum does not match the octet sum modulo 256.", out error);
	}

	static bool TryParseBytes(ReadOnlyMemory<byte> input, out FixMessage? message, out FixParseError? error, FixContext? context)
	{
		message = null;

		var framing = context?.Framing ?? FixFraming.Wire;

		if (!Envelope(input.Span, framing, null, out _, out error))
			return false;

		var fields = FixParser.ParseFields(input, context);

		if (!CheckSyntax(fields, out error))
			return false;

		if (framing == FixFraming.Log && !Envelope(input.Span, framing, fields, out _, out error))
			return false;

		return FixParser.TryParseMessage(fields, out message, out error, context);
	}

	/// <summary>
	/// <see cref="BuildMessage"/>, answering with a diagnostic rather than throwing.
	/// </summary>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	internal static bool TryBuildMessage(string source, FixField[] fields, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (fields == null) throw new ArgumentNullException(nameof(fields));

		message = null;

		var framing   = context?.Framing ?? FixFraming.Wire;
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

			if (field.ValuePosition - position < 2 || source[field.ValuePosition - 1] != '=' ||
				!IsTag(source.AsSpan(position, field.ValuePosition - 1 - position), field.Tag) ||
				source[field.ValuePosition + field.Length] != separator)
			{
				return Fail(position, field.Tag, type, "Field locations do not match the supplied source.", out error);
			}

			position = field.ValuePosition + field.Length + 1;
		}

		if (position != source.Length)
			return Fail(position, null, type, "Field locations do not cover the supplied source.", out error);

		if (framing == FixFraming.Log && !Envelope(source, framing, fields, out _, out error))
			return false;

		return FixParser.TryParseMessage(fields, out message, out error, context);
	}

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
			// Skipped input stops the message; a tag nothing builds a field of is a field of it, out of scope.
			if (field is FixField.Invalid { Tag: 0 } invalid)
				return Fail(invalid.Position, null, null, invalid.Message, out error);

		error = null;

		return true;
	}

	static bool Fail(int position, int? tag, string? type, string reason, out FixParseError? error)
	{
		error = new FixParseError(position, tag, type, reason);
		return false;
	}
}
