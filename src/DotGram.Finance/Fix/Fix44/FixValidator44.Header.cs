using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.cs from the FIX 4.4 repository; not edited by hand. From Templates/Validator.Header.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The standard header and trailer: what every message owes before its own type is asked anything.
/// </summary>
/// <remarks>
/// Apart from the generated checks of each type, because the header is one block carried by every
/// type and its order rule is about the message as a whole: a header field that follows a body
/// field is out of order (SessionRejectReason 14), which no check of one type can see, since each
/// reads its fields one at a time.
/// </remarks>
partial class FixValidator44 : FixValidator
{
	private protected override string Using => "using DotGram.Finance.Fix;\nusing DotGram.Finance.Fix.Fix44;\n";

	private protected override string ContextName => nameof(Fix44Context);

	/// <summary>Holds the standard header and trailer of any message to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage> StandardHeader { get; set; } = ValidateStandardHeader;

	static void ValidateStandardHeader(Fix44Context context, FixCheck check, FixMessage message)
	{
		if (message.BeginString  is null) Missing(check, FixTag.BeginString);
		if (message.BodyLength   is null) Missing(check, FixTag.BodyLength);
		if (message.MsgType      is null) Missing(check, FixTag.MsgType);
		if (message.SenderCompID is null) Missing(check, FixTag.SenderCompID);
		if (message.TargetCompID is null) Missing(check, FixTag.TargetCompID);
		if (message.MsgSeqNum    is null) Missing(check, FixTag.MsgSeqNum);
		if (message.SendingTime  is null) Missing(check, FixTag.SendingTime);
		if (message.CheckSum     is null) Missing(check, FixTag.CheckSum);

		// BeginString, BodyLength and MsgType are the first three fields, in that order.
		First(check, message, 0, message.BeginString);
		First(check, message, 1, message.BodyLength);
		First(check, message, 2, message.MsgType);

		if (message.BeginString is { IsValid: true } begin && begin.Value != "FIX.4.4")
			check.Add(FixRule.InvalidValue, FixTag.BeginString, begin.Position, begin);

		// What the reading measured of the octets, which the message does not keep.
		if (message.BodyLength is { IsValid: true } declared && message.MeasuredBodyLength >= 0 && declared.Value != message.MeasuredBodyLength)
			check.Add(FixRule.BodyLengthMismatch, FixTag.BodyLength, declared.Position, declared);

		if (message.CheckSum is { } sum)
		{
			if (sum.Value.Length != 3 || sum.Value.AsSpan().ToTag() is var expected && expected < 0)
				check.Add(FixRule.InvalidValue, FixTag.CheckSum, sum.Position, sum);
			else if (message.MeasuredCheckSum >= 0 && expected != message.MeasuredCheckSum)
				check.Add(FixRule.CheckSumMismatch, FixTag.CheckSum, sum.Position, sum);
		}

		// The header is read before the body, and a header tag met after the body has begun is
		// out of the order the protocol requires. The trailer is the other way about and is not
		// asked here.
		var body = false;

		var length = 0;

		// An Encoded field is text in the encoding MessageEncoding names, and without it nobody can
		// say what the octets are: said once, at the first such field.
		var encoding = message.MessageEncoding is not null;

		foreach (var field in message.Fields)
		{
			// A length field is followed by the data it measures, and by nothing else.
			if (length != 0 && field.Tag != context.DataTag(length))
				check.Add(FixRule.LengthFieldNotBeforeData, length, field.Position, field);

			length = context.Kind(field.Tag) > 0 ? field.Tag : 0;

			if (!encoding && IsEncoded(field.Tag))
			{
				check.Add(FixRule.MessageEncodingMissing, FixTag.MessageEncoding, field.Position, field);
				encoding = true;
			}

			if (IsTrailer(field.Tag))
				continue;

			if (!IsHeader(field.Tag))
				body = true;
			else if (body)
				check.Add(FixRule.FieldOutOfOrder, field.Tag, field.Position, field);
		}

		Counted(check, message.NoHops, message.NoHopsGroups);
	}

	/// <summary>Holds each entry of a group to its check, in order; nothing where the group is absent.</summary>
	/// <remarks>
	/// What every check calls for a group, rather than writing the loop in its text: the walk is where the
	/// check enters the group and moves from entry to entry, which every finding inside says with its path.
	/// </remarks>
	internal static void Each<TEntry>(Fix44Context context, FixCheck check, int counter, List<TEntry>? entries, Action<Fix44Context, FixCheck, TEntry> slot)
	{
		if (entries is null)
			return;

		check.Enter(counter);

		for (var i = 0; i < entries.Count; i++)
		{
			check.At(i);
			slot(context, check, entries[i]);
		}

		check.Leave();
	}

	// A field of the header that has to stand at an index of its own, and does not.
	static void First(FixCheck check, FixMessage message, int index, FixField? field)
	{
		if (field is not null && (message.Fields.Count <= index || !ReferenceEquals(message.Fields[index], field)))
			check.Add(FixRule.FieldOutOfOrder, field.Tag, field.Position, field);
	}

	static bool IsHeader(int tag)
	{
		return tag is FixTag.BeginString or FixTag.BodyLength or FixTag.MsgType or FixTag.SenderCompID or FixTag.TargetCompID or
			FixTag.OnBehalfOfCompID or FixTag.DeliverToCompID or FixTag.SecureDataLen or FixTag.SecureData or FixTag.MsgSeqNum or
			FixTag.SenderSubID or FixTag.SenderLocationID or FixTag.TargetSubID or FixTag.TargetLocationID or
			FixTag.OnBehalfOfSubID or FixTag.OnBehalfOfLocationID or FixTag.DeliverToSubID or FixTag.DeliverToLocationID or
			FixTag.PossDupFlag or FixTag.PossResend or FixTag.SendingTime or FixTag.OrigSendingTime or FixTag.XmlDataLen or
			FixTag.XmlData or FixTag.MessageEncoding or FixTag.LastMsgSeqNumProcessed or FixTag.NoHops or FixTag.HopCompID or
			FixTag.HopSendingTime or FixTag.HopRefID;
	}

	static bool IsEncoded(int tag)
	{
		return tag is FixTag.EncodedIssuer or FixTag.EncodedSecurityDesc or FixTag.EncodedListExecInst or FixTag.EncodedText or
			FixTag.EncodedSubject or FixTag.EncodedHeadline or FixTag.EncodedAllocText or FixTag.EncodedUnderlyingIssuer or
			FixTag.EncodedUnderlyingSecurityDesc or FixTag.EncodedListStatusText or FixTag.EncodedLegIssuer or
			FixTag.EncodedLegSecurityDesc;
	}

	static bool IsTrailer(int tag)
	{
		return tag is FixTag.SignatureLength or FixTag.Signature or FixTag.CheckSum;
	}
}
