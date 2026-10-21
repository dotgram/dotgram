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
			case FixTag.BeginString: if (_beginString is not null) Duplicate(field); _beginString = (FixField.Text)field; break;
			case FixTag.BodyLength: if (_bodyLength is not null) Duplicate(field); _bodyLength = (FixField.Integer)field; break;
			case FixTag.MsgType: if (_msgType is not null) Duplicate(field); _msgType = (FixField.Text)field; break;
			case FixTag.ApplVerID: if (_applVerID is not null) Duplicate(field); _applVerID = (FixField.Text)field; break;
			case FixTag.ApplExtID: if (_applExtID is not null) Duplicate(field); _applExtID = (FixField.Integer)field; break;
			case FixTag.CstmApplVerID: if (_cstmApplVerID is not null) Duplicate(field); _cstmApplVerID = (FixField.Text)field; break;
			case FixTag.SenderCompID: if (_senderCompID is not null) Duplicate(field); _senderCompID = (FixField.Text)field; break;
			case FixTag.TargetCompID: if (_targetCompID is not null) Duplicate(field); _targetCompID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfCompID: if (_onBehalfOfCompID is not null) Duplicate(field); _onBehalfOfCompID = (FixField.Text)field; break;
			case FixTag.DeliverToCompID: if (_deliverToCompID is not null) Duplicate(field); _deliverToCompID = (FixField.Text)field; break;
			case FixTag.SecureDataLen: if (_secureDataLen is not null) Duplicate(field); _secureDataLen = (FixField.Integer)field; break;
			case FixTag.SecureData: if (_secureData is not null) Duplicate(field); _secureData = (FixField.Data)field; break;
			case FixTag.MsgSeqNum: if (_msgSeqNum is not null) Duplicate(field); _msgSeqNum = (FixField.Integer)field; break;
			case FixTag.SenderSubID: if (_senderSubID is not null) Duplicate(field); _senderSubID = (FixField.Text)field; break;
			case FixTag.SenderLocationID: if (_senderLocationID is not null) Duplicate(field); _senderLocationID = (FixField.Text)field; break;
			case FixTag.TargetSubID: if (_targetSubID is not null) Duplicate(field); _targetSubID = (FixField.Text)field; break;
			case FixTag.TargetLocationID: if (_targetLocationID is not null) Duplicate(field); _targetLocationID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfSubID: if (_onBehalfOfSubID is not null) Duplicate(field); _onBehalfOfSubID = (FixField.Text)field; break;
			case FixTag.OnBehalfOfLocationID: if (_onBehalfOfLocationID is not null) Duplicate(field); _onBehalfOfLocationID = (FixField.Text)field; break;
			case FixTag.DeliverToSubID: if (_deliverToSubID is not null) Duplicate(field); _deliverToSubID = (FixField.Text)field; break;
			case FixTag.DeliverToLocationID: if (_deliverToLocationID is not null) Duplicate(field); _deliverToLocationID = (FixField.Text)field; break;
			case FixTag.PossDupFlag: if (_possDupFlag is not null) Duplicate(field); _possDupFlag = (FixField.Boolean)field; break;
			case FixTag.PossResend: if (_possResend is not null) Duplicate(field); _possResend = (FixField.Boolean)field; break;
			case FixTag.SendingTime: if (_sendingTime is not null) Duplicate(field); _sendingTime = (FixField.Timestamp)field; break;
			case FixTag.OrigSendingTime: if (_origSendingTime is not null) Duplicate(field); _origSendingTime = (FixField.Timestamp)field; break;
			case FixTag.XmlDataLen: if (_xmlDataLen is not null) Duplicate(field); _xmlDataLen = (FixField.Integer)field; break;
			case FixTag.XmlData: if (_xmlData is not null) Duplicate(field); _xmlData = (FixField.Data)field; break;
			case FixTag.MessageEncoding: if (_messageEncoding is not null) Duplicate(field); _messageEncoding = (FixField.Text)field; break;
			case FixTag.LastMsgSeqNumProcessed: if (_lastMsgSeqNumProcessed is not null) Duplicate(field); _lastMsgSeqNumProcessed = (FixField.Integer)field; break;
			case FixTag.NoHops: if (_noHops is not null) Duplicate(field); _noHops = (FixField.Integer)field; break;
			case FixTag.HopCompID:
				if (noHopsGroups is null && _noHops is null) Uncounted(FixTag.NoHops, field);
				(noHopsGroups ??= []).Add(new ((FixField.Text)field));
				break;
			case FixTag.HopSendingTime:
				if (None(noHopsGroups))
					Unentered(field);
				else if (noHopsGroups![^1]._hopSendingTime is not null)
					Duplicate(field, FixTag.NoHops, noHopsGroups!.Count - 1);
				else
					noHopsGroups![^1]._hopSendingTime = (FixField.Timestamp)field;
				break;
			case FixTag.HopRefID:
				if (None(noHopsGroups))
					Unentered(field);
				else if (noHopsGroups![^1]._hopRefID is not null)
					Duplicate(field, FixTag.NoHops, noHopsGroups!.Count - 1);
				else
					noHopsGroups![^1]._hopRefID = (FixField.Integer)field;
				break;
			case FixTag.SignatureLength: if (_signatureLength is not null) Duplicate(field); _signatureLength = (FixField.Integer)field; break;
			case FixTag.Signature: if (_signature is not null) Duplicate(field); _signature = (FixField.Data)field; break;
			case FixTag.CheckSum: if (_checkSum is not null) Duplicate(field); _checkSum = (FixField.Text)field; break;

			default: return false;
		}

		return true;
	}

	/// <summary>The FIX BeginString, tag 8, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? BeginString => _beginString;
	internal FixField.Text? _beginString;

	/// <summary>The FIX BodyLength, tag 9, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? BodyLength => _bodyLength;
	internal FixField.Integer? _bodyLength;

	/// <summary>The FIX MsgType, tag 35, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MsgType => _msgType;
	internal FixField.Text? _msgType;

	/// <summary>The FIX ApplVerID, tag 1128, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? ApplVerID => _applVerID;
	internal FixField.Text? _applVerID;

	/// <summary>The FIX ApplExtID, tag 1156, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? ApplExtID => _applExtID;
	internal FixField.Integer? _applExtID;

	/// <summary>The FIX CstmApplVerID, tag 1129, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? CstmApplVerID => _cstmApplVerID;
	internal FixField.Text? _cstmApplVerID;

	/// <summary>The FIX SenderCompID, tag 49, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderCompID => _senderCompID;
	internal FixField.Text? _senderCompID;

	/// <summary>The FIX TargetCompID, tag 56, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetCompID => _targetCompID;
	internal FixField.Text? _targetCompID;

	/// <summary>The FIX OnBehalfOfCompID, tag 115, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfCompID => _onBehalfOfCompID;
	internal FixField.Text? _onBehalfOfCompID;

	/// <summary>The FIX DeliverToCompID, tag 128, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToCompID => _deliverToCompID;
	internal FixField.Text? _deliverToCompID;

	/// <summary>The FIX SecureDataLen, tag 90, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? SecureDataLen => _secureDataLen;
	internal FixField.Integer? _secureDataLen;

	/// <summary>The FIX SecureData, tag 91, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? SecureData => _secureData;
	internal FixField.Data? _secureData;

	/// <summary>The FIX MsgSeqNum, tag 34, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? MsgSeqNum => _msgSeqNum;
	internal FixField.Integer? _msgSeqNum;

	/// <summary>The FIX SenderSubID, tag 50, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderSubID => _senderSubID;
	internal FixField.Text? _senderSubID;

	/// <summary>The FIX SenderLocationID, tag 142, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? SenderLocationID => _senderLocationID;
	internal FixField.Text? _senderLocationID;

	/// <summary>The FIX TargetSubID, tag 57, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetSubID => _targetSubID;
	internal FixField.Text? _targetSubID;

	/// <summary>The FIX TargetLocationID, tag 143, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? TargetLocationID => _targetLocationID;
	internal FixField.Text? _targetLocationID;

	/// <summary>The FIX OnBehalfOfSubID, tag 116, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfSubID => _onBehalfOfSubID;
	internal FixField.Text? _onBehalfOfSubID;

	/// <summary>The FIX OnBehalfOfLocationID, tag 144, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? OnBehalfOfLocationID => _onBehalfOfLocationID;
	internal FixField.Text? _onBehalfOfLocationID;

	/// <summary>The FIX DeliverToSubID, tag 129, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToSubID => _deliverToSubID;
	internal FixField.Text? _deliverToSubID;

	/// <summary>The FIX DeliverToLocationID, tag 145, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? DeliverToLocationID => _deliverToLocationID;
	internal FixField.Text? _deliverToLocationID;

	/// <summary>The FIX PossDupFlag, tag 43, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossDupFlag => _possDupFlag;
	internal FixField.Boolean? _possDupFlag;

	/// <summary>The FIX PossResend, tag 97, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Boolean? PossResend => _possResend;
	internal FixField.Boolean? _possResend;

	/// <summary>The FIX SendingTime, tag 52, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? SendingTime => _sendingTime;
	internal FixField.Timestamp? _sendingTime;

	/// <summary>The FIX OrigSendingTime, tag 122, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Timestamp? OrigSendingTime => _origSendingTime;
	internal FixField.Timestamp? _origSendingTime;

	/// <summary>The FIX XmlDataLen, tag 212, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? XmlDataLen => _xmlDataLen;
	internal FixField.Integer? _xmlDataLen;

	/// <summary>The FIX XmlData, tag 213, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Data? XmlData => _xmlData;
	internal FixField.Data? _xmlData;

	/// <summary>The FIX MessageEncoding, tag 347, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Text? MessageEncoding => _messageEncoding;
	internal FixField.Text? _messageEncoding;

	/// <summary>The FIX LastMsgSeqNumProcessed, tag 369, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? LastMsgSeqNumProcessed => _lastMsgSeqNumProcessed;
	internal FixField.Integer? _lastMsgSeqNumProcessed;

	/// <summary>The FIX NoHops, tag 627, of this message's StandardHeader; null when the field is absent.</summary>
	public FixField.Integer? NoHops => _noHops;
	internal FixField.Integer? _noHops;

	/// <summary>The entries counted by NoHops, tag 627; null when the group is absent.</summary>
	public List<FixMessage.NoHopsGroup>? NoHopsGroups => noHopsGroups;
	internal List<NoHopsGroup>? noHopsGroups;

	/// <summary>The FIX SignatureLength, tag 93, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Integer? SignatureLength => _signatureLength;
	internal FixField.Integer? _signatureLength;

	/// <summary>The FIX Signature, tag 89, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Data? Signature => _signature;
	internal FixField.Data? _signature;

	/// <summary>The FIX CheckSum, tag 10, of this message's StandardTrailer; null when the field is absent.</summary>
	public FixField.Text? CheckSum => _checkSum;
	internal FixField.Text? _checkSum;

	/// <summary>One entry of the group counted by NoHops, tag 627.</summary>
	public sealed class NoHopsGroup : IFixEntry
	{
		internal NoHopsGroup(FixField.Text delimiter)
		{
			HopCompID = delimiter;
		}

		/// <summary>The FIX HopCompID, tag 628, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
		public FixField.Text HopCompID { get; }

		/// <summary>The FIX HopSendingTime, tag 629, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? HopSendingTime => _hopSendingTime;
		internal FixField.Timestamp? _hopSendingTime;

		/// <summary>The FIX HopRefID, tag 630, wire type <c>SeqNum</c>; null when the field is absent.</summary>
		public FixField.Integer? HopRefID => _hopRefID;
		internal FixField.Integer? _hopRefID;

		FixField IFixEntry.Opener => HopCompID;

		bool IFixEntry.Has(int tag)
		{
			return tag is 628 or 629 or 630;
		}

		IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
		{
			step = default;

			return null;
		}
	}
}
