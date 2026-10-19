using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix50;

// Written by generate.cs from the FIX 5.0 SP2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

public abstract partial class FixMessage
{
	/// <summary>
	/// Takes a field of the standard header or trailer, and answers whether the tag was one of
	/// theirs, so that a message type's own constructor can report what neither it nor this took.
	/// </summary>
	/// <param name="field">The field to take.</param>
	private protected bool SetStandardField(FixField field)
	{
		switch (field.Tag)
		{
			case FixTag.BeginString: if (BeginString is not null) Duplicate(field); BeginString = (FixField.Text)field; break;
			case FixTag.BodyLength: if (BodyLength is not null) Duplicate(field); BodyLength = (FixField.Integer)field; break;
			case FixTag.MsgType: if (MsgType is not null) Duplicate(field); MsgType = (FixField.Text)field; break;
			case FixTag.ApplVerID: if (ApplVerID is not null) Duplicate(field); ApplVerID = (FixField.Text)field; break;
			case FixTag.ApplExtID: if (ApplExtID is not null) Duplicate(field); ApplExtID = (FixField.Integer)field; break;
			case FixTag.CstmApplVerID: if (CstmApplVerID is not null) Duplicate(field); CstmApplVerID = (FixField.Text)field; break;
			case FixTag.SenderCompID: if (SenderCompID is not null) Duplicate(field); SenderCompID = (FixField.Text)field; break;
			case FixTag.TargetCompID: if (TargetCompID is not null) Duplicate(field); TargetCompID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfCompID: if (OnBehalfOfCompID is not null) Duplicate(field); OnBehalfOfCompID = (FixField.Text)field; break;
			case FixTag.DeliverToCompID: if (DeliverToCompID is not null) Duplicate(field); DeliverToCompID = (FixField.Text)field; break;
			case FixTag.SecureDataLen: if (SecureDataLen is not null) Duplicate(field); SecureDataLen = (FixField.Integer)field; break;
			case FixTag.SecureData: if (SecureData is not null) Duplicate(field); SecureData = (FixField.Data)field; break;
			case FixTag.MsgSeqNum: if (MsgSeqNum is not null) Duplicate(field); MsgSeqNum = (FixField.Integer)field; break;
			case FixTag.SenderSubID: if (SenderSubID is not null) Duplicate(field); SenderSubID = (FixField.Text)field; break;
			case FixTag.SenderLocationID: if (SenderLocationID is not null) Duplicate(field); SenderLocationID = (FixField.Text)field; break;
			case FixTag.TargetSubID: if (TargetSubID is not null) Duplicate(field); TargetSubID = (FixField.Text)field; break;
			case FixTag.TargetLocationID: if (TargetLocationID is not null) Duplicate(field); TargetLocationID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfSubID: if (OnBehalfOfSubID is not null) Duplicate(field); OnBehalfOfSubID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfLocationID: if (OnBehalfOfLocationID is not null) Duplicate(field); OnBehalfOfLocationID = (FixField.Text)field; break;
			case FixTag.DeliverToSubID: if (DeliverToSubID is not null) Duplicate(field); DeliverToSubID = (FixField.Text)field; break;
			case FixTag.DeliverToLocationID: if (DeliverToLocationID is not null) Duplicate(field); DeliverToLocationID = (FixField.Text)field; break;
			case FixTag.PossDupFlag: if (PossDupFlag is not null) Duplicate(field); PossDupFlag = (FixField.Boolean)field; break;
			case FixTag.PossResend: if (PossResend is not null) Duplicate(field); PossResend = (FixField.Boolean)field; break;
			case FixTag.SendingTime: if (SendingTime is not null) Duplicate(field); SendingTime = (FixField.Timestamp)field; break;
			case FixTag.OrigSendingTime: if (OrigSendingTime is not null) Duplicate(field); OrigSendingTime = (FixField.Timestamp)field; break;
			case FixTag.XmlDataLen: if (XmlDataLen is not null) Duplicate(field); XmlDataLen = (FixField.Integer)field; break;
			case FixTag.XmlData: if (XmlData is not null) Duplicate(field); XmlData = (FixField.Data)field; break;
			case FixTag.MessageEncoding: if (MessageEncoding is not null) Duplicate(field); MessageEncoding = (FixField.Text)field; break;
			case FixTag.LastMsgSeqNumProcessed: if (LastMsgSeqNumProcessed is not null) Duplicate(field); LastMsgSeqNumProcessed = (FixField.Integer)field; break;
			case FixTag.NoHops: if (NoHops is not null) Duplicate(field); NoHops = (FixField.Integer)field; break;
			case FixTag.HopCompID:
				if (noHopsGroups is null && NoHops is null) Uncounted(FixTag.NoHops, field);
				(noHopsGroups ??= []).Add(new ((FixField.Text)field));
				break;
			case FixTag.HopSendingTime:
				if (noHopsGroups is null || noHopsGroups.Count == 0)
					Unentered(field);
				else if (noHopsGroups![^1].HopSendingTime is not null)
					Duplicate(field, FixTag.NoHops, noHopsGroups!.Count - 1);
				else
					noHopsGroups![^1].HopSendingTime = (FixField.Timestamp)field;
				break;
			case FixTag.HopRefID:
				if (noHopsGroups is null || noHopsGroups.Count == 0)
					Unentered(field);
				else if (noHopsGroups![^1].HopRefID is not null)
					Duplicate(field, FixTag.NoHops, noHopsGroups!.Count - 1);
				else
					noHopsGroups![^1].HopRefID = (FixField.Integer)field;
				break;
			case FixTag.SignatureLength: if (SignatureLength is not null) Duplicate(field); SignatureLength = (FixField.Integer)field; break;
			case FixTag.Signature: if (Signature is not null) Duplicate(field); Signature = (FixField.Data)field; break;
			case FixTag.CheckSum: if (CheckSum is not null) Duplicate(field); CheckSum = (FixField.Text)field; break;

			default: return false;
		}

		return true;
	}

	/// <summary>The FIX BeginString, tag 8, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? BeginString { get; private protected set; }

	/// <summary>The FIX BodyLength, tag 9, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? BodyLength { get; private protected set; }

	/// <summary>The FIX MsgType, tag 35, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MsgType { get; private protected set; }

	/// <summary>The FIX ApplVerID, tag 1128, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? ApplVerID { get; private protected set; }

	/// <summary>The FIX ApplExtID, tag 1156, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? ApplExtID { get; private protected set; }

	/// <summary>The FIX CstmApplVerID, tag 1129, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? CstmApplVerID { get; private protected set; }

	/// <summary>The FIX SenderCompID, tag 49, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderCompID { get; private protected set; }

	/// <summary>The FIX TargetCompID, tag 56, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetCompID { get; private protected set; }

	/// <summary>The FIX OnBehalfOfCompID, tag 115, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfCompID { get; private protected set; }

	/// <summary>The FIX DeliverToCompID, tag 128, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToCompID { get; private protected set; }

	/// <summary>The FIX SecureDataLen, tag 90, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? SecureDataLen { get; private protected set; }

	/// <summary>The FIX SecureData, tag 91, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? SecureData { get; private protected set; }

	/// <summary>The FIX MsgSeqNum, tag 34, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? MsgSeqNum { get; private protected set; }

	/// <summary>The FIX SenderSubID, tag 50, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderSubID { get; private protected set; }

	/// <summary>The FIX SenderLocationID, tag 142, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderLocationID { get; private protected set; }

	/// <summary>The FIX TargetSubID, tag 57, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetSubID { get; private protected set; }

	/// <summary>The FIX TargetLocationID, tag 143, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetLocationID { get; private protected set; }

	/// <summary>The FIX OnBehalfOfSubID, tag 116, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfSubID { get; private protected set; }

	/// <summary>The FIX OnBehalfOfLocationID, tag 144, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfLocationID { get; private protected set; }

	/// <summary>The FIX DeliverToSubID, tag 129, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToSubID { get; private protected set; }

	/// <summary>The FIX DeliverToLocationID, tag 145, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToLocationID { get; private protected set; }

	/// <summary>The FIX PossDupFlag, tag 43, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossDupFlag { get; private protected set; }

	/// <summary>The FIX PossResend, tag 97, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossResend { get; private protected set; }

	/// <summary>The FIX SendingTime, tag 52, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? SendingTime { get; private protected set; }

	/// <summary>The FIX OrigSendingTime, tag 122, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? OrigSendingTime { get; private protected set; }

	/// <summary>The FIX XmlDataLen, tag 212, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? XmlDataLen { get; private protected set; }

	/// <summary>The FIX XmlData, tag 213, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? XmlData { get; private protected set; }

	/// <summary>The FIX MessageEncoding, tag 347, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MessageEncoding { get; private protected set; }

	/// <summary>The FIX LastMsgSeqNumProcessed, tag 369, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? LastMsgSeqNumProcessed { get; private protected set; }

	/// <summary>The FIX NoHops, tag 627, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? NoHops { get; private protected set; }

	/// <summary>The entries counted by NoHops, tag 627; null when the group is absent.</summary>
	public List<FixMessage.NoHopsGroup>? NoHopsGroups => noHopsGroups;
	internal List<NoHopsGroup>? noHopsGroups;

	/// <summary>The FIX SignatureLength, tag 93, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Integer? SignatureLength { get; private protected set; }

	/// <summary>The FIX Signature, tag 89, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Data? Signature { get; private protected set; }

	/// <summary>The FIX CheckSum, tag 10, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Text? CheckSum { get; private protected set; }

	/// <summary>One entry of the group counted by NoHops, tag 627.</summary>
	public sealed class NoHopsGroup
	{
		internal NoHopsGroup(FixField.Text delimiter)
		{
			HopCompID = delimiter;
		}

		/// <summary>The FIX HopCompID, tag 628, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
		public FixField.Text HopCompID { get; }

		/// <summary>The FIX HopSendingTime, tag 629, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? HopSendingTime { get; internal set; }

		/// <summary>The FIX HopRefID, tag 630, wire type <c>SeqNum</c>; null when the field is absent.</summary>
		public FixField.Integer? HopRefID { get; internal set; }
	}
}
