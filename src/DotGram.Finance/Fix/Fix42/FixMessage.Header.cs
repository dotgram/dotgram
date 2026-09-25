using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand.

public abstract partial class FixMessage
{
	/// <summary>
	/// Takes a field of the standard header or trailer, and answers whether the tag was one of
	/// theirs, so that a message type's own constructor can report what neither it nor this took.
	/// </summary>
	/// <param name="field">The field to take.</param>
	protected bool SetStandardField(FixField field)
	{
		switch (field.Tag)
		{
			case FixTag.BeginString: if (BeginString is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BeginString = (FixField.Text)field; break;
			case FixTag.BodyLength: if (BodyLength is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BodyLength = (FixField.Integer)field; break;
			case FixTag.MsgType: if (MsgType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MsgType = (FixField.Text)field; break;
			case FixTag.SenderCompID: if (SenderCompID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderCompID = (FixField.Text)field; break;
			case FixTag.TargetCompID: if (TargetCompID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetCompID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfCompID: if (OnBehalfOfCompID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfCompID = (FixField.Text)field; break;
			case FixTag.DeliverToCompID: if (DeliverToCompID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToCompID = (FixField.Text)field; break;
			case FixTag.SecureDataLen: if (SecureDataLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecureDataLen = (FixField.Integer)field; break;
			case FixTag.SecureData: if (SecureData is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecureData = (FixField.Data)field; break;
			case FixTag.MsgSeqNum: if (MsgSeqNum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MsgSeqNum = (FixField.Integer)field; break;
			case FixTag.SenderSubID: if (SenderSubID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderSubID = (FixField.Text)field; break;
			case FixTag.SenderLocationID: if (SenderLocationID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderLocationID = (FixField.Text)field; break;
			case FixTag.TargetSubID: if (TargetSubID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetSubID = (FixField.Text)field; break;
			case FixTag.TargetLocationID: if (TargetLocationID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetLocationID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfSubID: if (OnBehalfOfSubID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfSubID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfLocationID: if (OnBehalfOfLocationID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfLocationID = (FixField.Text)field; break;
			case FixTag.DeliverToSubID: if (DeliverToSubID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToSubID = (FixField.Text)field; break;
			case FixTag.DeliverToLocationID: if (DeliverToLocationID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToLocationID = (FixField.Text)field; break;
			case FixTag.PossDupFlag: if (PossDupFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PossDupFlag = (FixField.Boolean)field; break;
			case FixTag.PossResend: if (PossResend is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PossResend = (FixField.Boolean)field; break;
			case FixTag.SendingTime: if (SendingTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SendingTime = (FixField.Timestamp)field; break;
			case FixTag.OrigSendingTime: if (OrigSendingTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigSendingTime = (FixField.Timestamp)field; break;
			case FixTag.XmlDataLen: if (XmlDataLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); XmlDataLen = (FixField.Integer)field; break;
			case FixTag.XmlData: if (XmlData is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); XmlData = (FixField.Data)field; break;
			case FixTag.MessageEncoding: if (MessageEncoding is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MessageEncoding = (FixField.Text)field; break;
			case FixTag.LastMsgSeqNumProcessed: if (LastMsgSeqNumProcessed is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMsgSeqNumProcessed = (FixField.Integer)field; break;
			case FixTag.OnBehalfOfSendingTime: if (OnBehalfOfSendingTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfSendingTime = (FixField.Timestamp)field; break;
			case FixTag.SignatureLength: if (SignatureLength is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SignatureLength = (FixField.Integer)field; break;
			case FixTag.Signature: if (Signature is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Signature = (FixField.Data)field; break;
			case FixTag.CheckSum: if (CheckSum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CheckSum = (FixField.Text)field; break;

			default: return false;
		}

		return true;
	}

	/// <summary>The FIX BeginString, tag 8, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? BeginString { get; protected set; }

	/// <summary>The FIX BodyLength, tag 9, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? BodyLength { get; protected set; }

	/// <summary>The FIX MsgType, tag 35, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MsgType { get; protected set; }

	/// <summary>The FIX SenderCompID, tag 49, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderCompID { get; protected set; }

	/// <summary>The FIX TargetCompID, tag 56, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetCompID { get; protected set; }

	/// <summary>The FIX OnBehalfOfCompID, tag 115, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfCompID { get; protected set; }

	/// <summary>The FIX DeliverToCompID, tag 128, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToCompID { get; protected set; }

	/// <summary>The FIX SecureDataLen, tag 90, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? SecureDataLen { get; protected set; }

	/// <summary>The FIX SecureData, tag 91, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? SecureData { get; protected set; }

	/// <summary>The FIX MsgSeqNum, tag 34, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? MsgSeqNum { get; protected set; }

	/// <summary>The FIX SenderSubID, tag 50, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderSubID { get; protected set; }

	/// <summary>The FIX SenderLocationID, tag 142, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderLocationID { get; protected set; }

	/// <summary>The FIX TargetSubID, tag 57, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetSubID { get; protected set; }

	/// <summary>The FIX TargetLocationID, tag 143, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetLocationID { get; protected set; }

	/// <summary>The FIX OnBehalfOfSubID, tag 116, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfSubID { get; protected set; }

	/// <summary>The FIX OnBehalfOfLocationID, tag 144, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfLocationID { get; protected set; }

	/// <summary>The FIX DeliverToSubID, tag 129, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToSubID { get; protected set; }

	/// <summary>The FIX DeliverToLocationID, tag 145, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToLocationID { get; protected set; }

	/// <summary>The FIX PossDupFlag, tag 43, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossDupFlag { get; protected set; }

	/// <summary>The FIX PossResend, tag 97, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossResend { get; protected set; }

	/// <summary>The FIX SendingTime, tag 52, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? SendingTime { get; protected set; }

	/// <summary>The FIX OrigSendingTime, tag 122, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? OrigSendingTime { get; protected set; }

	/// <summary>The FIX XmlDataLen, tag 212, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? XmlDataLen { get; protected set; }

	/// <summary>The FIX XmlData, tag 213, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? XmlData { get; protected set; }

	/// <summary>The FIX MessageEncoding, tag 347, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MessageEncoding { get; protected set; }

	/// <summary>The FIX LastMsgSeqNumProcessed, tag 369, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? LastMsgSeqNumProcessed { get; protected set; }

	/// <summary>The FIX OnBehalfOfSendingTime, tag 370, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? OnBehalfOfSendingTime { get; protected set; }

	/// <summary>The FIX SignatureLength, tag 93, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Integer? SignatureLength { get; protected set; }

	/// <summary>The FIX Signature, tag 89, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Data? Signature { get; protected set; }

	/// <summary>The FIX CheckSum, tag 10, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Text? CheckSum { get; protected set; }
}
