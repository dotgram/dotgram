using System;

namespace DotGram.Finance.Fix;

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

		// The header is read before the body, and a header tag met after the body has begun is
		// out of the order the protocol requires. The trailer is the other way about and is not
		// asked here.
		var body = false;

		foreach (var field in message.Fields)
		{
			if (IsTrailer(field.Tag))
				continue;

			if (!IsHeader(field.Tag))
				body = true;
			else if (body)
				message.AddFinding(new FixFinding(FixRule.FieldOutOfOrder, field.Tag, field.Position, field, -1));
		}

		Counted(message, message.NoHops, message.Hop);

		return message.IsValid;
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
