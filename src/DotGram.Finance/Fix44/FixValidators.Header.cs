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
		if (message.BeginString  is null) Missing(message, 8);
		if (message.BodyLength   is null) Missing(message, 9);
		if (message.MsgType      is null) Missing(message, 35);
		if (message.SenderCompID is null) Missing(message, 49);
		if (message.TargetCompID is null) Missing(message, 56);
		if (message.MsgSeqNum    is null) Missing(message, 34);
		if (message.SendingTime  is null) Missing(message, 52);
		if (message.CheckSum     is null) Missing(message, 10);

		// BeginString, BodyLength and MsgType are the first three fields, in that order.
		First(message, 0, message.BeginString);
		First(message, 1, message.BodyLength);
		First(message, 2, message.MsgType);

		if (message.BeginString is { IsValid: true } begin && begin.Value != "FIX.4.4")
			message.AddFinding(new FixFinding(FixRule.InvalidValue, 8, begin.Position, begin, -1));

		// What the reading measured of the octets, which the message does not keep.
		if (message.BodyLength is { IsValid: true } declared && message.MeasuredBodyLength >= 0 && declared.Value != message.MeasuredBodyLength)
			message.AddFinding(new FixFinding(FixRule.BodyLengthMismatch, 9, declared.Position, declared, -1));

		if (message.CheckSum is { } sum)
		{
			if (sum.Value.Length != 3 || sum.Value.AsSpan().ToTag() is var expected && expected < 0)
				message.AddFinding(new FixFinding(FixRule.InvalidValue, 10, sum.Position, sum, -1));
			else if (message.MeasuredCheckSum >= 0 && expected != message.MeasuredCheckSum)
				message.AddFinding(new FixFinding(FixRule.CheckSumMismatch, 10, sum.Position, sum, -1));
		}

		// The header is read before the body, and a header tag met after the body has begun is
		// out of the order the protocol requires. The trailer is the other way about and is not
		// asked here.
		var body = false;

		var length = 0;

		foreach (var field in message.Fields)
		{
			// A length field is followed by the data it measures, and by nothing else.
			if (length != 0 && field.Tag != context.DataTag(length))
				message.AddFinding(new FixFinding(FixRule.LengthFieldNotBeforeData, length, field.Position, field, -1));

			length = context.Kind(field.Tag) > 0 ? field.Tag : 0;

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

	static bool IsHeader(int tag)
	{
		return tag is 8 or 9 or 35 or 49 or 56 or 115 or 128 or 90 or 91 or 34 or 50 or 142 or 57 or 143 or 116 or 144 or 129 or 145
			or 43 or 97 or 52 or 122 or 212 or 213 or 347 or 369 or 627 or 628 or 629 or 630;
	}

	static bool IsTrailer(int tag)
	{
		return tag is 93 or 89 or 10;
	}
}
