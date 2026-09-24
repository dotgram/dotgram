using System;

namespace DotGram.Finance.Fix44;

/// <summary>
/// The standard header and trailer: what every message owes before its own type is asked anything.
/// </summary>
/// <remarks>
/// Written by hand rather than generated, because the header is one block carried by every type
/// and its order rule is about the message as a whole: a header field that follows a body field
/// is out of order (SessionRejectReason 14), which no check of one type can see, since each
/// reads its fields one at a time.
/// </remarks>
partial class FixValidators
{
	/// <summary>Holds the standard header and trailer of any message to the schema.</summary>
	public Func<FixContext, FixMessage, bool> StandardHeader { get; set; } = ValidateStandardHeader;

	static bool ValidateStandardHeader(FixContext context, FixMessage message)
	{
		if (message.BeginString  is null) Missing(message, FixTag.BeginString);
		if (message.BodyLength   is null) Missing(message, FixTag.BodyLength);
		if (message.MsgType      is null) Missing(message, FixTag.MsgType);
		if (message.SenderCompID is null) Missing(message, FixTag.SenderCompID);
		if (message.TargetCompID is null) Missing(message, FixTag.TargetCompID);
		if (message.MsgSeqNum    is null) Missing(message, FixTag.MsgSeqNum);
		if (message.SendingTime  is null) Missing(message, FixTag.SendingTime);
		if (message.CheckSum     is null) Missing(message, FixTag.CheckSum);

		// BeginString, BodyLength and MsgType are the first three fields, in that order.
		First(message, 0, message.BeginString);
		First(message, 1, message.BodyLength);
		First(message, 2, message.MsgType);

		if (message.BeginString is { IsValid: true } begin && begin.Value != "FIX.4.4")
			message.AddFinding(new FixFinding(FixRule.InvalidValue, FixTag.BeginString, begin.Position, begin, -1));

		// What the reading measured of the octets, which the message does not keep.
		if (message.BodyLength is { IsValid: true } declared && message.MeasuredBodyLength >= 0 && declared.Value != message.MeasuredBodyLength)
			message.AddFinding(new FixFinding(FixRule.BodyLengthMismatch, FixTag.BodyLength, declared.Position, declared, -1));

		if (message.CheckSum is { } sum)
		{
			if (sum.Value.Length != 3 || sum.Value.AsSpan().ToTag() is var expected && expected < 0)
				message.AddFinding(new FixFinding(FixRule.InvalidValue, FixTag.CheckSum, sum.Position, sum, -1));
			else if (message.MeasuredCheckSum >= 0 && expected != message.MeasuredCheckSum)
				message.AddFinding(new FixFinding(FixRule.CheckSumMismatch, FixTag.CheckSum, sum.Position, sum, -1));
		}

		// The header is read before the body, and a header tag met after the body has begun is
		// out of the order the protocol requires. The trailer is the other way about and is not
		// asked here.
		var body = false;

		var length = (FixTag)0;

		// An Encoded field is text in the encoding MessageEncoding names, and without it nobody can
		// say what the octets are: said once, at the first such field.
		var encoding = message.MessageEncoding is not null;

		foreach (var field in message.Fields)
		{
			// A length field is followed by the data it measures, and by nothing else.
			if (length != 0 && (int)field.Tag != context.DataTag((int)length))
				message.AddFinding(new FixFinding(FixRule.LengthFieldNotBeforeData, length, field.Position, field, -1));

			length = context.Kind((int)field.Tag) > 0 ? field.Tag : 0;

			if (!encoding && IsEncoded(field.Tag))
			{
				message.AddFinding(new FixFinding(FixRule.MessageEncodingMissing, FixTag.MessageEncoding, field.Position, field, -1));
				encoding = true;
			}

			if (IsTrailer(field.Tag))
				continue;

			if (!IsHeader(field.Tag))
				body = true;
			else if (body)
				message.AddFinding(new FixFinding(FixRule.FieldOutOfOrder, field.Tag, field.Position, field, -1));
		}

		Counted(message, message.NoHops, message.NoHopsGroups);

		return message.IsValid;
	}

	// A field of the header that has to stand at an index of its own, and does not.
	static void First(FixMessage message, int index, FixField? field)
	{
		if (field is not null && (message.Fields.Count <= index || !ReferenceEquals(message.Fields[index], field)))
			message.AddFinding(new FixFinding(FixRule.FieldOutOfOrder, field.Tag, field.Position, field, -1));
	}

	static bool IsHeader(FixTag tag)
	{
		return tag is FixTag.BeginString or FixTag.BodyLength or FixTag.MsgType or FixTag.SenderCompID or FixTag.TargetCompID
			or FixTag.OnBehalfOfCompID or FixTag.DeliverToCompID or FixTag.SecureDataLen or FixTag.SecureData or FixTag.MsgSeqNum
			or FixTag.SenderSubID or FixTag.SenderLocationID or FixTag.TargetSubID or FixTag.TargetLocationID or FixTag.OnBehalfOfSubID
			or FixTag.OnBehalfOfLocationID or FixTag.DeliverToSubID or FixTag.DeliverToLocationID or FixTag.PossDupFlag
			or FixTag.PossResend or FixTag.SendingTime or FixTag.OrigSendingTime or FixTag.XmlDataLen or FixTag.XmlData
			or FixTag.MessageEncoding or FixTag.LastMsgSeqNumProcessed or FixTag.NoHops or FixTag.HopCompID or FixTag.HopSendingTime
			or FixTag.HopRefID;
	}

	static bool IsEncoded(FixTag tag)
	{
		return tag is FixTag.EncodedIssuer or FixTag.EncodedSecurityDesc or FixTag.EncodedListExecInst or FixTag.EncodedText
			or FixTag.EncodedSubject or FixTag.EncodedHeadline or FixTag.EncodedAllocText or FixTag.EncodedUnderlyingIssuer
			or FixTag.EncodedUnderlyingSecurityDesc or FixTag.EncodedListStatusText or FixTag.EncodedLegIssuer
			or FixTag.EncodedLegSecurityDesc;
	}

	static bool IsTrailer(FixTag tag)
	{
		return tag is FixTag.SignatureLength or FixTag.Signature or FixTag.CheckSum;
	}
}
