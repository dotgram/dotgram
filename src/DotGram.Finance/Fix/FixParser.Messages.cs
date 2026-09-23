using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

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
/// Reading is one act and holding a message to a schema is another: what holds a message to the
/// schema is asked of the message, not of an entry point here.
/// </para>
/// </remarks>
public static partial class FixParser
{
	/// <summary>
	/// The largest message a streamed call will read, in octets, when none is given.
	/// </summary>
	public const int DefaultMaxMessageLength = FixMessages.DefaultMaxMessageLength;

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

	/// <summary>Parses one complete message from a copy of the input.</summary>
	/// <param name="input">The message.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <exception cref="FormatException">The input is not a message under <paramref name="context"/>.</exception>
	/// <remarks>The message keeps its source, so the input is copied into a string once.</remarks>
	public static FixMessage ParseMessage(ReadOnlySpan<char> input, FixContext? context = null)
	{
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

	/// <summary>Reads one message from a copy of the octets it arrived as.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <exception cref="FormatException">The input is not one readable message.</exception>
	/// <remarks>
	/// <see cref="ParseMessage(byte[], FixContext)"/> says what the octet road is for. The
	/// parser reads an array, so the span is copied into one first.
	/// </remarks>
	public static FixMessage ParseMessage(ReadOnlySpan<byte> input, FixContext? context = null)
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
		return FixMessages.TryParse(input, out message, out error, context);
	}


	// The length half of a pair, built from the count it measured. The wire text it was read from
	// is not kept, and a FIX Length is plain digits, so the two agree wherever the input is valid.
	static FixField LengthField(int tag, int count, FixCustomFields? custom)
	{
		(bool, long) value = (true, count);

		return tag switch
		{
			 90 => new FixField.SecureDataLen(value),
			 93 => new FixField.SignatureLength(value),
			 95 => new FixField.RawDataLength(value),
			212 => new FixField.XmlDataLen(value),
			348 => new FixField.EncodedIssuerLen(value),
			350 => new FixField.EncodedSecurityDescLen(value),
			352 => new FixField.EncodedListExecInstLen(value),
			354 => new FixField.EncodedTextLen(value),
			356 => new FixField.EncodedSubjectLen(value),
			358 => new FixField.EncodedHeadlineLen(value),
			360 => new FixField.EncodedAllocTextLen(value),
			362 => new FixField.EncodedUnderlyingIssuerLen(value),
			364 => new FixField.EncodedUnderlyingSecurityDescLen(value),
			445 => new FixField.EncodedListStatusTextLen(value),
			618 => new FixField.EncodedLegIssuerLen(value),
			621 => new FixField.EncodedLegSecurityDescLen(value),
			  _ => (custom ?? FixSpareFields.Instance).Text(tag, count.ToString(CultureInfo.InvariantCulture).AsSpan()),
		};
	}

	/// <summary>Puts one message together from fields already read, in the order they were read.</summary>
	internal static bool TryParseMessage(IEnumerable<FixField> input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		message = null;

		var     started = false;
		var     ended   = false;
		var     fields  = new List<FixField>(input is ICollection c ? c.Count : 6);
		string? type    = null;

		foreach (var field in input)
		{
			// A length/data pair is read as the data field alone, since the length is how the reader
			// knew where to stop. The message carries both, so the length half is put back here, with
			// the count it measured and the extent it was read from.
			if (field.IsBinary)
			{
				var settings = context ?? FixContext.Default;
				var length   = LengthField(settings.LengthTag(field.Tag), field.Length, settings.CustomFields);

				length.Locate(field.Position, field.DataPosition - field.Position);
				fields.Add(length);
			}

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
				(1, '6', _  ) => new FixMessage.IndicationOfInterest                                    (fields),
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
				_             => new FixMessage.Custom                                 (type, fields),
			};
		}
	}

	/// <summary>Reads one message from a buffer; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as octets held one to a character.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryParseMessage(ReadOnlySpan<char> input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		return FixMessages.TryParse(input, out message, out error, context);
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
		return FixMessages.TryParse(input, out message, out error, context);
	}

	/// <summary>Reads one message from a copy of the octets; a malformed one answers with a diagnostic.</summary>
	/// <param name="input">The whole message, as the octets that came off the wire.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	/// <remarks>The parser reads an array, so the span is copied into one first.</remarks>
	public static bool TryParseMessage(ReadOnlySpan<byte> input, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		return FixMessages.TryParse(input, out message, out error, context);
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
			foreach (var message in FixMessages.ReadMessages(reader, context, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads every message of a buffer, in order.</summary>
	/// <param name="input">Concatenated messages, as octets held one to a character.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	public static FixMessage[] ParseMessages(ReadOnlySpan<char> input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return ParseMessages(input.ToString(), context, maxMessageLength);
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

		var messages = new List<FixMessage>();

		using (var stream = new MemoryStream(input, writable: false))
			foreach (var message in FixMessages.ReadMessages(stream, context, maxMessageLength))
				messages.Add(message);

		return messages.ToArray();
	}

	/// <summary>Reads every message of a copy of a buffer of octets, in order.</summary>
	/// <param name="input">Concatenated messages, as the octets that came off the wire.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input holds something that is not a message.</exception>
	/// <remarks>The input is read from an array, so the span is copied into one first.</remarks>
	public static FixMessage[] ParseMessages(ReadOnlySpan<byte> input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return ParseMessages(input.ToArray(), context, maxMessageLength);
	}

	/// <summary>Reads exactly one message from a reader, without closing it or reading past it.</summary>
	/// <param name="input">The reader, which is left open.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.Parse(input, context, maxMessageLength);
	}

	/// <summary>Reads exactly one message from a stream, without closing it or reading past it.</summary>
	/// <param name="input">The stream, which is left open.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage ReadMessage(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.Parse(input, context, maxMessageLength);
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
		return FixMessages.TryParse(input, out message, out error, context, maxMessageLength);
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
		return FixMessages.TryParse(input, out message, out error, context, maxMessageLength);
	}

	/// <summary>Reads concatenated messages from a reader, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The reader, which is left open and owned by the caller.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.ReadMessages(input, context, maxMessageLength);
	}

	/// <summary>Reads concatenated messages from a stream, one at a time, reusing a frame buffer.</summary>
	/// <param name="input">The stream, which is left open and owned by the caller.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		return FixMessages.ReadMessages(input, context, maxMessageLength);
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
		return FixMessages.Build(source, fields, context);
	}

	/// <summary>The same, answering with a diagnostic rather than throwing.</summary>
	/// <param name="source">The source those fields were read from.</param>
	/// <param name="fields">The fields, in the order they were read.</param>
	/// <param name="message">The message built, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data pairs.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>.</returns>
	public static bool TryBuildMessage(string source, FixField[] fields, out FixMessage? message, out FixParseError? error, FixContext? context = null)
	{
		return FixMessages.TryBuild(source, fields, out message, out error, context);
	}
}
