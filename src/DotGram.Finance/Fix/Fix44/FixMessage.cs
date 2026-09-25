using System;

namespace DotGram.Finance.Fix.Fix44;

/// <summary>
/// The ninety-three messages FIX 4.4 describes, each a case of <see cref="FixMessage"/>.
/// </summary>
/// <remarks>
/// Nested, because this is a closed set and not ninety-three free names: the set is fixed, every
/// case is a <see cref="FixMessage"/>, and a switch over them is meant to be exhaustive. The
/// package already writes the field hierarchy this way one layer down — <c>FixField.Invalid</c>,
/// <c>FixField.Date</c> — and two spellings of one idea in one package is what this removes.
/// At the point of use it reads as what it is: <c>FixMessage.NewOrderSingle</c> says what the
/// thing IS where it is written, and puts the base type in front of every arm of every switch,
/// which is what makes the hierarchy visibly closed where it is taken apart.
/// </remarks>
public abstract partial class FixMessage : IFixFindings
{
	internal FixMessage(string messageType, List<FixField> fields)
	{
		MessageType = messageType;
		Fields      = fields;
	}

	/// <summary>
	/// Takes a field of the standard header or trailer, and answers whether the tag was one of
	/// theirs, so that a message type's own constructor can report what neither it nor this took.
	/// </summary>
	/// <param name="field">The field to take.</param>
	protected bool SetStandardField(FixField field)
	{
		switch (field.Tag)
		{
			case FixTag.BeginString           : if (BeginString            is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BeginString            = (FixField.Text)     field; break;
			case FixTag.BodyLength            : if (BodyLength             is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BodyLength             = (FixField.Integer)  field; break;
			case FixTag.CheckSum              : if (CheckSum               is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CheckSum               = (FixField.Text)     field; break;
			case FixTag.MsgSeqNum             : if (MsgSeqNum              is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MsgSeqNum              = (FixField.Integer)  field; break;
			case FixTag.MsgType               : if (MsgType                is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MsgType                = (FixField.Text)     field; break;
			case FixTag.PossDupFlag           : if (PossDupFlag            is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PossDupFlag            = (FixField.Boolean)  field; break;
			case FixTag.SenderCompID          : if (SenderCompID           is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderCompID           = (FixField.Text)     field; break;
			case FixTag.SenderSubID           : if (SenderSubID            is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderSubID            = (FixField.Text)     field; break;
			case FixTag.SendingTime           : if (SendingTime            is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SendingTime            = (FixField.Timestamp)field; break;
			case FixTag.TargetCompID          : if (TargetCompID           is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetCompID           = (FixField.Text)     field; break;
			case FixTag.TargetSubID           : if (TargetSubID            is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetSubID            = (FixField.Text)     field; break;
			case FixTag.Signature             : if (Signature              is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Signature              = (FixField.Data)     field; break;
			case FixTag.SecureDataLen         : if (SecureDataLen          is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecureDataLen          = (FixField.Integer)  field; break;
			case FixTag.SecureData            : if (SecureData             is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecureData             = (FixField.Data)     field; break;
			case FixTag.SignatureLength       : if (SignatureLength        is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SignatureLength        = (FixField.Integer)  field; break;
			case FixTag.PossResend            : if (PossResend             is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PossResend             = (FixField.Boolean)  field; break;
			case FixTag.OnBehalfOfCompID      : if (OnBehalfOfCompID       is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfCompID       = (FixField.Text)     field; break;
			case FixTag.OnBehalfOfSubID       : if (OnBehalfOfSubID        is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfSubID        = (FixField.Text)     field; break;
			case FixTag.OrigSendingTime       : if (OrigSendingTime        is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigSendingTime        = (FixField.Timestamp)field; break;
			case FixTag.DeliverToCompID       : if (DeliverToCompID        is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToCompID        = (FixField.Text)     field; break;
			case FixTag.DeliverToSubID        : if (DeliverToSubID         is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToSubID         = (FixField.Text)     field; break;
			case FixTag.SenderLocationID      : if (SenderLocationID       is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SenderLocationID       = (FixField.Text)     field; break;
			case FixTag.TargetLocationID      : if (TargetLocationID       is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TargetLocationID       = (FixField.Text)     field; break;
			case FixTag.OnBehalfOfLocationID  : if (OnBehalfOfLocationID   is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OnBehalfOfLocationID   = (FixField.Text)     field; break;
			case FixTag.DeliverToLocationID   : if (DeliverToLocationID    is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DeliverToLocationID    = (FixField.Text)     field; break;
			case FixTag.XmlDataLen            : if (XmlDataLen             is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); XmlDataLen             = (FixField.Integer)  field; break;
			case FixTag.XmlData               : if (XmlData                is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); XmlData                = (FixField.Data)     field; break;
			case FixTag.MessageEncoding       : if (MessageEncoding        is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MessageEncoding        = (FixField.Text)     field; break;
			case FixTag.LastMsgSeqNumProcessed: if (LastMsgSeqNumProcessed is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMsgSeqNumProcessed = (FixField.Integer)  field; break;

			// The one repeating group of the standard header: where a message has been, hop by hop.
			case FixTag.NoHops                : if (NoHops                 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoHops                 = (FixField.Integer)  field; break;
			case FixTag.HopCompID:
				if (NoHopsGroups is null && NoHops is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoHops, field.Position, field, -1));
				(NoHopsGroups ??= []).Add(new () { HopCompID = (FixField.Text)field });
				break;
			case FixTag.HopSendingTime:
				if (NoHopsGroups is null || NoHopsGroups.Count == 0)
					AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
				else if (NoHopsGroups[^1].HopSendingTime is not null)
					AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoHopsGroups.Count - 1));
				else
					NoHopsGroups[^1].HopSendingTime = (FixField.Timestamp)field;
				break;
			case FixTag.HopRefID:
				if (NoHopsGroups is null || NoHopsGroups.Count == 0)
					AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
				else if (NoHopsGroups[^1].HopRefID is not null)
					AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoHopsGroups.Count - 1));
				else
					NoHopsGroups[^1].HopRefID = (FixField.Integer)field;
				break;

			default: return false;
		}

		return true;
	}

	/// <summary>
	/// The MsgType, tag 35.
	/// </summary>
	public string         MessageType { get; private protected set; }
	/// <summary>
	/// All fields in wire order, including header and trailer; group entries are flattened into the list.
	/// </summary>
	public List<FixField> Fields      { get; private protected set; }


	/// <summary>Everything found wrong with this message, or null while nothing has been.</summary>
	public List<FixFinding>? InvalidFindings { get; private set; }

	/// <summary>Whether nothing has been found wrong with this message.</summary>
	/// <remarks>
	/// True before <see cref="Validate"/> is called means only that the reading found nothing: the
	/// message has not been held to the schema yet.
	/// </remarks>
	public bool IsValid => InvalidFindings is null;

	// What the reading measured of the octets this message was read from, which it does not keep: the
	// octets of the body and their sum before CheckSum, for Validate to hold BodyLength and CheckSum to;
	// -1 where it was not read from octets, or has no BodyLength.
	internal int MeasuredBodyLength = -1;
	internal int MeasuredCheckSum   = -1;

	// Only so that a second Validate does not fill the findings twice. One pass over an input is
	// one context, so a message is validated once and a second call answers what the first did.
	bool _validated;

	/// <summary>Holds this message to the schema, adding what is wrong to <see cref="InvalidFindings"/>.</summary>
	/// <param name="context">The schema to hold it to.</param>
	/// <returns>Whether nothing is wrong with it.</returns>
	public bool Validate(Fix44Context context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		if (_validated)
			return IsValid;

		_validated = true;

		// The standard header and trailer first, which every type carries, then the type itself.
		context.Validators.StandardHeader(context, this);
		Check(context);

		return IsValid;
	}

	/// <summary>Asks the context for the check of this message type and runs it.</summary>
	private protected abstract void Check(Fix44Context context);

	/// <summary>Adds one finding to this message.</summary>
	/// <param name="finding">What is wrong.</param>
	protected internal void AddFinding(FixFinding finding)
	{
		(InvalidFindings ??= []).Add(finding);
	}

	void IFixFindings.AddFinding(FixFinding finding)
	{
		AddFinding(finding);
	}

	/// <summary>
	/// The FIX NoHops, tag 627, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   NoHops                 { get; protected set; }

	/// <summary>
	/// The entries of the NoHopsGroups group of this message's StandardHeader; null when the group is absent.
	/// </summary>
	public List<NoHopsGroup>? NoHopsGroups { get; protected set; }

	/// <summary>
	/// The FIX BeginString, tag 8, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      BeginString            { get; protected set; }

	/// <summary>
	/// The FIX BodyLength, tag 9, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   BodyLength             { get; protected set; }

	/// <summary>
	/// The FIX SenderCompID, tag 49, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      SenderCompID           { get; protected set; }

	/// <summary>
	/// The FIX TargetCompID, tag 56, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      TargetCompID           { get; protected set; }

	/// <summary>
	/// The FIX OnBehalfOfCompID, tag 115, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      OnBehalfOfCompID       { get; protected set; }

	/// <summary>
	/// The FIX DeliverToCompID, tag 128, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      DeliverToCompID        { get; protected set; }

	/// <summary>
	/// The FIX SecureDataLen, tag 90, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   SecureDataLen          { get; protected set; }

	/// <summary>
	/// The FIX SecureData, tag 91, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Data?      SecureData             { get; protected set; }

	/// <summary>
	/// The FIX MsgSeqNum, tag 34, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   MsgSeqNum              { get; protected set; }

	/// <summary>
	/// The FIX MsgType, tag 35, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      MsgType                { get; protected set; }

	/// <summary>
	/// The FIX SenderSubID, tag 50, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      SenderSubID            { get; protected set; }

	/// <summary>
	/// The FIX SenderLocationID, tag 142, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      SenderLocationID       { get; protected set; }

	/// <summary>
	/// The FIX TargetSubID, tag 57, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      TargetSubID            { get; protected set; }

	/// <summary>
	/// The FIX TargetLocationID, tag 143, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      TargetLocationID       { get; protected set; }

	/// <summary>
	/// The FIX OnBehalfOfSubID, tag 116, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      OnBehalfOfSubID        { get; protected set; }

	/// <summary>
	/// The FIX OnBehalfOfLocationID, tag 144, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      OnBehalfOfLocationID   { get; protected set; }

	/// <summary>
	/// The FIX DeliverToSubID, tag 129, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      DeliverToSubID         { get; protected set; }

	/// <summary>
	/// The FIX DeliverToLocationID, tag 145, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      DeliverToLocationID    { get; protected set; }

	/// <summary>
	/// The FIX PossDupFlag, tag 43, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Boolean?   PossDupFlag            { get; protected set; }

	/// <summary>
	/// The FIX PossResend, tag 97, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Boolean?   PossResend             { get; protected set; }

	/// <summary>
	/// The FIX SendingTime, tag 52, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Timestamp? SendingTime            { get; protected set; }

	/// <summary>
	/// The FIX OrigSendingTime, tag 122, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Timestamp? OrigSendingTime        { get; protected set; }

	/// <summary>
	/// The FIX XmlDataLen, tag 212, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   XmlDataLen             { get; protected set; }

	/// <summary>
	/// The FIX XmlData, tag 213, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Data?      XmlData                { get; protected set; }

	/// <summary>
	/// The FIX MessageEncoding, tag 347, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Text?      MessageEncoding        { get; protected set; }

	/// <summary>
	/// The FIX LastMsgSeqNumProcessed, tag 369, of this message's StandardHeader; null when the field is absent.
	/// </summary>
	public FixField.Integer?   LastMsgSeqNumProcessed { get; protected set; }

	/// <summary>
	/// The FIX SignatureLength, tag 93, of this message's StandardTrailer; null when the field is absent.
	/// </summary>
	public FixField.Integer?   SignatureLength        { get; protected set; }

	/// <summary>
	/// The FIX Signature, tag 89, of this message's StandardTrailer; null when the field is absent.
	/// </summary>
	public FixField.Data?      Signature              { get; protected set; }

	/// <summary>
	/// The FIX CheckSum, tag 10, of this message's StandardTrailer; null when the field is absent.
	/// </summary>
	public FixField.Text?      CheckSum               { get; protected set; }

	/// <summary>One entry of the group counted by NoHops, tag 627, in the standard header.</summary>
	public sealed class NoHopsGroup
	{
		/// <summary>The FIX HopCompID, tag 628, wire type <c>String</c>.</summary>
		public required FixField.Text HopCompID { get; init; }

		/// <summary>The FIX HopSendingTime, tag 629, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? HopSendingTime { get; internal set; }

		/// <summary>The FIX HopRefID, tag 630, wire type <c>SeqNum</c>; null when the field is absent.</summary>
		public FixField.Integer? HopRefID { get; internal set; }
	}

	/// <summary>A message of a MsgType FIX 4.4 does not define and no message factory builds; its fields remain in wire order.</summary>
	/// <remarks>
	/// Not valid from the moment it is read: its type is what is wrong with it, and that is said once,
	/// as <see cref="FixRule.UnknownMessageType"/>. The standard header and trailer are the standard's
	/// and are read as themselves; <see cref="Validate"/> holds them to it and asks nothing more.
	/// </remarks>
	public sealed class Invalid : FixMessage
	{
		internal Invalid(string type, List<FixField> fields) : base(type, fields)
		{
			foreach (var field in fields)
				SetStandardField(field);

			AddFinding(new FixFinding(FixRule.UnknownMessageType, FixTag.MsgType, MsgType?.Position ?? 0, MsgType, -1));
		}

		/// <summary>Nothing: the type is not one the schema describes, and that has been said.</summary>
		private protected override void Check(Fix44Context context)
		{
		}
	}
}
