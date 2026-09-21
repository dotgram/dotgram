using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// Constructs the message class a MsgType names.
/// </summary>
static class FixMessageFactory
{
	public static FixMessage Message(string type, string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
	{
		return type switch
		{
			"0" => new FixMessage.Heartbeat(source, header, body, trailer),
			"1" => new FixMessage.TestRequest(source, header, body, trailer),
			"2" => new FixMessage.ResendRequest(source, header, body, trailer),
			"3" => new FixMessage.Reject(source, header, body, trailer),
			"4" => new FixMessage.SequenceReset(source, header, body, trailer),
			"5" => new FixMessage.Logout(source, header, body, trailer),
			"6" => new FixMessage.IOI(source, header, body, trailer),
			"7" => new FixMessage.Advertisement(source, header, body, trailer),
			"8" => new FixMessage.ExecutionReport(source, header, body, trailer),
			"9" => new FixMessage.OrderCancelReject(source, header, body, trailer),
			"A" => new FixMessage.Logon(source, header, body, trailer),
			"B" => new FixMessage.News(source, header, body, trailer),
			"C" => new FixMessage.Email(source, header, body, trailer),
			"D" => new FixMessage.NewOrderSingle(source, header, body, trailer),
			"E" => new FixMessage.NewOrderList(source, header, body, trailer),
			"F" => new FixMessage.OrderCancelRequest(source, header, body, trailer),
			"G" => new FixMessage.OrderCancelReplaceRequest(source, header, body, trailer),
			"H" => new FixMessage.OrderStatusRequest(source, header, body, trailer),
			"J" => new FixMessage.AllocationInstruction(source, header, body, trailer),
			"K" => new FixMessage.ListCancelRequest(source, header, body, trailer),
			"L" => new FixMessage.ListExecute(source, header, body, trailer),
			"M" => new FixMessage.ListStatusRequest(source, header, body, trailer),
			"N" => new FixMessage.ListStatus(source, header, body, trailer),
			"P" => new FixMessage.AllocationInstructionAck(source, header, body, trailer),
			"Q" => new FixMessage.DontKnowTrade(source, header, body, trailer),
			"R" => new FixMessage.QuoteRequest(source, header, body, trailer),
			"S" => new FixMessage.Quote(source, header, body, trailer),
			"T" => new FixMessage.SettlementInstructions(source, header, body, trailer),
			"V" => new FixMessage.MarketDataRequest(source, header, body, trailer),
			"W" => new FixMessage.MarketDataSnapshotFullRefresh(source, header, body, trailer),
			"X" => new FixMessage.MarketDataIncrementalRefresh(source, header, body, trailer),
			"Y" => new FixMessage.MarketDataRequestReject(source, header, body, trailer),
			"Z" => new FixMessage.QuoteCancel(source, header, body, trailer),
			"a" => new FixMessage.QuoteStatusRequest(source, header, body, trailer),
			"b" => new FixMessage.MassQuoteAcknowledgement(source, header, body, trailer),
			"c" => new FixMessage.SecurityDefinitionRequest(source, header, body, trailer),
			"d" => new FixMessage.SecurityDefinition(source, header, body, trailer),
			"e" => new FixMessage.SecurityStatusRequest(source, header, body, trailer),
			"f" => new FixMessage.SecurityStatus(source, header, body, trailer),
			"g" => new FixMessage.TradingSessionStatusRequest(source, header, body, trailer),
			"h" => new FixMessage.TradingSessionStatus(source, header, body, trailer),
			"i" => new FixMessage.MassQuote(source, header, body, trailer),
			"j" => new FixMessage.BusinessMessageReject(source, header, body, trailer),
			"k" => new FixMessage.BidRequest(source, header, body, trailer),
			"l" => new FixMessage.BidResponse(source, header, body, trailer),
			"m" => new FixMessage.ListStrikePrice(source, header, body, trailer),
			"n" => new FixMessage.XMLnonFIX(source, header, body, trailer),
			"o" => new FixMessage.RegistrationInstructions(source, header, body, trailer),
			"p" => new FixMessage.RegistrationInstructionsResponse(source, header, body, trailer),
			"q" => new FixMessage.OrderMassCancelRequest(source, header, body, trailer),
			"r" => new FixMessage.OrderMassCancelReport(source, header, body, trailer),
			"s" => new FixMessage.NewOrderCross(source, header, body, trailer),
			"t" => new FixMessage.CrossOrderCancelReplaceRequest(source, header, body, trailer),
			"u" => new FixMessage.CrossOrderCancelRequest(source, header, body, trailer),
			"v" => new FixMessage.SecurityTypeRequest(source, header, body, trailer),
			"w" => new FixMessage.SecurityTypes(source, header, body, trailer),
			"x" => new FixMessage.SecurityListRequest(source, header, body, trailer),
			"y" => new FixMessage.SecurityList(source, header, body, trailer),
			"z" => new FixMessage.DerivativeSecurityListRequest(source, header, body, trailer),
			"AA" => new FixMessage.DerivativeSecurityList(source, header, body, trailer),
			"AB" => new FixMessage.NewOrderMultileg(source, header, body, trailer),
			"AC" => new FixMessage.MultilegOrderCancelReplace(source, header, body, trailer),
			"AD" => new FixMessage.TradeCaptureReportRequest(source, header, body, trailer),
			"AE" => new FixMessage.TradeCaptureReport(source, header, body, trailer),
			"AF" => new FixMessage.OrderMassStatusRequest(source, header, body, trailer),
			"AG" => new FixMessage.QuoteRequestReject(source, header, body, trailer),
			"AH" => new FixMessage.RFQRequest(source, header, body, trailer),
			"AI" => new FixMessage.QuoteStatusReport(source, header, body, trailer),
			"AJ" => new FixMessage.QuoteResponse(source, header, body, trailer),
			"AK" => new FixMessage.Confirmation(source, header, body, trailer),
			"AL" => new FixMessage.PositionMaintenanceRequest(source, header, body, trailer),
			"AM" => new FixMessage.PositionMaintenanceReport(source, header, body, trailer),
			"AN" => new FixMessage.RequestForPositions(source, header, body, trailer),
			"AO" => new FixMessage.RequestForPositionsAck(source, header, body, trailer),
			"AP" => new FixMessage.PositionReport(source, header, body, trailer),
			"AQ" => new FixMessage.TradeCaptureReportRequestAck(source, header, body, trailer),
			"AR" => new FixMessage.TradeCaptureReportAck(source, header, body, trailer),
			"AS" => new FixMessage.AllocationReport(source, header, body, trailer),
			"AT" => new FixMessage.AllocationReportAck(source, header, body, trailer),
			"AU" => new FixMessage.ConfirmationAck(source, header, body, trailer),
			"AV" => new FixMessage.SettlementInstructionRequest(source, header, body, trailer),
			"AW" => new FixMessage.AssignmentReport(source, header, body, trailer),
			"AX" => new FixMessage.CollateralRequest(source, header, body, trailer),
			"AY" => new FixMessage.CollateralAssignment(source, header, body, trailer),
			"AZ" => new FixMessage.CollateralResponse(source, header, body, trailer),
			"BA" => new FixMessage.CollateralReport(source, header, body, trailer),
			"BB" => new FixMessage.CollateralInquiry(source, header, body, trailer),
			"BC" => new FixMessage.NetworkCounterpartySystemStatusRequest(source, header, body, trailer),
			"BD" => new FixMessage.NetworkCounterpartySystemStatusResponse(source, header, body, trailer),
			"BE" => new FixMessage.UserRequest(source, header, body, trailer),
			"BF" => new FixMessage.UserResponse(source, header, body, trailer),
			"BG" => new FixMessage.CollateralInquiryAck(source, header, body, trailer),
			"BH" => new FixMessage.ConfirmationRequest(source, header, body, trailer),
			_ => new FixMessage.Custom(source, type, header, body, trailer),
		};
	}
}

/// <summary>
/// The ninety-three messages FIX 4.4 describes, each a case of <see cref="FixMessage"/>.
/// </summary>
/// <remarks>
/// Nested, because this is a closed set and not ninety-three free names: the set is fixed, every
/// case is a <see cref="FixMessage"/>, and a switch over them is meant to be exhaustive. The
/// package already writes the field hierarchy this way one layer down — <c>FixField.Custom</c>,
/// <c>FixField.SettlDate</c> — and two spellings of one idea in one package is what this removes.
/// At the point of use it reads as what it is: <c>FixMessage.NewOrderSingle</c> says what the
/// thing IS where it is written, and puts the base type in front of every arm of every switch,
/// which is what makes the hierarchy visibly closed where it is taken apart.
/// </remarks>
public abstract partial class FixMessage
{
	/// <summary>A vendor message of unknown MsgType; its body fields remain in wire order.</summary>
	/// <remarks>
	/// The case for everything the schema does not describe, and the reason the set can be closed
	/// without being complete: a MsgType nobody here has heard of is still a message, and still one
	/// of these.
	/// </remarks>
	public class Custom : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCustom"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCustom;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Custom(string source, string type, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, type, header, body, trailer)
		{
		}
	}

	/// <summary>FIX 4.4 Heartbeat, MsgType 0.</summary>
	public sealed class Heartbeat : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateHeartbeat"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateHeartbeat;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Heartbeat(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "0", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 112, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TestReqID => GetText(112);
	}

	/// <summary>FIX 4.4 TestRequest, MsgType 1.</summary>
	public sealed class TestRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTestRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTestRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TestRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "1", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 112, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TestReqID => GetText(112);
	}

	/// <summary>FIX 4.4 ResendRequest, MsgType 2.</summary>
	public sealed class ResendRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateResendRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateResendRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ResendRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "2", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 7, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BeginSeqNo => GetNumber(7);
		/// <summary>
		/// FIX tag 16, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndSeqNo => GetNumber(16);
	}

	/// <summary>FIX 4.4 Reject, MsgType 3.</summary>
	public sealed class Reject : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateReject"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateReject;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Reject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "3", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 45, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RefSeqNum => GetNumber(45);
		/// <summary>
		/// FIX tag 371, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RefTagID => GetNumber(371);
		/// <summary>
		/// FIX tag 372, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RefMsgType => GetText(372);
		/// <summary>
		/// FIX tag 373, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SessionRejectReason => GetNumber(373);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 SequenceReset, MsgType 4.</summary>
	public sealed class SequenceReset : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSequenceReset"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSequenceReset;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SequenceReset(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "4", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 123, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? GapFillFlag => GetText(123);
		/// <summary>
		/// FIX tag 36, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NewSeqNo => GetNumber(36);
	}

	/// <summary>FIX 4.4 Logout, MsgType 5.</summary>
	public sealed class Logout : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateLogout"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateLogout;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Logout(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "5", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 IOI, MsgType 6.</summary>
	public sealed class IOI : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateIOI"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateIOI;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal IOI(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "6", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 28, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? IOITransType => GetText(28);
		/// <summary>
		/// FIX tag 26, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIRefID => GetText(26);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 27, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIQty => GetText(27);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegIOIGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ValidUntilTime => GetText(62);
		/// <summary>
		/// FIX tag 25, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? IOIQltyInd => GetText(25);
		/// <summary>
		/// FIX tag 130, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? IOINaturalFlag => GetText(130);
		/// <summary>
		/// Entries of the group counted by NoIOIQualifiers, FIX tag 199; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> IOIQualGrp => GetGroup(199);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 149, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? URLLink => GetText(149);
		/// <summary>
		/// Entries of the group counted by NoRoutingIDs, FIX tag 215; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	}

	/// <summary>FIX 4.4 Advertisement, MsgType 7.</summary>
	public sealed class Advertisement : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAdvertisement"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAdvertisement;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Advertisement(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "7", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 2, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AdvId => GetText(2);
		/// <summary>
		/// FIX tag 5, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AdvTransType => GetText(5);
		/// <summary>
		/// FIX tag 3, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AdvRefID => GetText(3);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 4, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? AdvSide => GetText(4);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 149, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? URLLink => GetText(149);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
	}

	/// <summary>FIX 4.4 ExecutionReport, MsgType 8.</summary>
	public sealed class ExecutionReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateExecutionReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateExecutionReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ExecutionReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "8", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 527, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryExecID => GetText(527);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 41, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigClOrdID => GetText(41);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// FIX tag 693, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteRespID => GetText(693);
		/// <summary>
		/// FIX tag 790, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrdStatusReqID => GetText(790);
		/// <summary>
		/// FIX tag 584, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MassStatusReqID => GetText(584);
		/// <summary>
		/// FIX tag 911, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumReports => GetNumber(911);
		/// <summary>
		/// FIX tag 912, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastRptRequested => GetText(912);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// Entries of the group counted by NoContraBrokers, FIX tag 382; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ContraGrp => GetGroup(382);
		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 548, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CrossID => GetText(548);
		/// <summary>
		/// FIX tag 551, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigCrossID => GetText(551);
		/// <summary>
		/// FIX tag 549, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossType => GetNumber(549);
		/// <summary>
		/// FIX tag 17, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecID => GetText(17);
		/// <summary>
		/// FIX tag 19, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecRefID => GetText(19);
		/// <summary>
		/// FIX tag 150, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExecType => GetText(150);
		/// <summary>
		/// FIX tag 39, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdStatus => GetText(39);
		/// <summary>
		/// FIX tag 636, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? WorkingIndicator => GetText(636);
		/// <summary>
		/// FIX tag 103, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrdRejReason => GetNumber(103);
		/// <summary>
		/// FIX tag 378, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ExecRestatementReason => GetNumber(378);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 589, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DayBookingInst => GetText(589);
		/// <summary>
		/// FIX tag 590, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BookingUnit => GetText(590);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 544, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CashMargin => GetText(544);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 839, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PeggedPrice => GetNumber(839);
		/// <summary>
		/// FIX tag 845, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionPrice => GetNumber(845);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 850, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategyPerformance => GetNumber(850);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 377, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? SolicitedFlag => GetText(377);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 32, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastQty => GetNumber(32);
		/// <summary>
		/// FIX tag 652, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingLastQty => GetNumber(652);
		/// <summary>
		/// FIX tag 31, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastPx => GetNumber(31);
		/// <summary>
		/// FIX tag 651, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingLastPx => GetNumber(651);
		/// <summary>
		/// FIX tag 669, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastParPx => GetNumber(669);
		/// <summary>
		/// FIX tag 194, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastSpotRate => GetNumber(194);
		/// <summary>
		/// FIX tag 195, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastForwardPoints => GetNumber(195);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 943, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TimeBracket => GetText(943);
		/// <summary>
		/// FIX tag 29, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? LastCapacity => GetText(29);
		/// <summary>
		/// FIX tag 151, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LeavesQty => GetNumber(151);
		/// <summary>
		/// FIX tag 14, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CumQty => GetNumber(14);
		/// <summary>
		/// FIX tag 6, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPx => GetNumber(6);
		/// <summary>
		/// FIX tag 424, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DayOrderQty => GetNumber(424);
		/// <summary>
		/// FIX tag 425, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DayCumQty => GetNumber(425);
		/// <summary>
		/// FIX tag 426, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DayAvgPx => GetNumber(426);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 113, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ReportToExch => GetText(113);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 381, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GrossTradeAmt => GetNumber(381);
		/// <summary>
		/// FIX tag 157, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumDaysInterest => GetNumber(157);
		/// <summary>
		/// FIX tag 230, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExDate => GetText(230);
		/// <summary>
		/// FIX tag 158, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestRate => GetNumber(158);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 738, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? InterestAtMaturity => GetNumber(738);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 258, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? TradedFlatSwitch => GetText(258);
		/// <summary>
		/// FIX tag 259, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? BasisFeatureDate => GetText(259);
		/// <summary>
		/// FIX tag 260, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BasisFeaturePrice => GetNumber(260);
		/// <summary>
		/// FIX tag 238, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Concession => GetNumber(238);
		/// <summary>
		/// FIX tag 237, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalTakedown => GetNumber(237);
		/// <summary>
		/// FIX tag 118, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetMoney => GetNumber(118);
		/// <summary>
		/// FIX tag 119, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrAmt => GetNumber(119);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 155, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrFxRate => GetNumber(155);
		/// <summary>
		/// FIX tag 156, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrFxRateCalc => GetText(156);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 641, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastForwardPoints2 => GetNumber(641);
		/// <summary>
		/// FIX tag 442, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MultiLegReportingType => GetText(442);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
		/// <summary>
		/// FIX tag 483, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransBkdTime => GetText(483);
		/// <summary>
		/// FIX tag 515, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExecValuationPoint => GetText(515);
		/// <summary>
		/// FIX tag 484, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExecPriceType => GetText(484);
		/// <summary>
		/// FIX tag 485, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ExecPriceAdjustment => GetNumber(485);
		/// <summary>
		/// FIX tag 638, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriorityIndicator => GetNumber(638);
		/// <summary>
		/// FIX tag 639, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceImprovement => GetNumber(639);
		/// <summary>
		/// FIX tag 851, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastLiquidityInd => GetNumber(851);
		/// <summary>
		/// Entries of the group counted by NoContAmts, FIX tag 518; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ContAmtGrp => GetGroup(518);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegExecGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 797, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? CopyMsgIndicator => GetText(797);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	}

	/// <summary>FIX 4.4 OrderCancelReject, MsgType 9.</summary>
	public sealed class OrderCancelReject : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderCancelReject"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderCancelReject;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderCancelReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "9", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// FIX tag 41, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigClOrdID => GetText(41);
		/// <summary>
		/// FIX tag 39, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdStatus => GetText(39);
		/// <summary>
		/// FIX tag 636, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? WorkingIndicator => GetText(636);
		/// <summary>
		/// FIX tag 586, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigOrdModTime => GetText(586);
		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 434, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CxlRejResponseTo => GetText(434);
		/// <summary>
		/// FIX tag 102, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CxlRejReason => GetNumber(102);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 Logon, MsgType A.</summary>
	public sealed class Logon : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateLogon"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateLogon;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Logon(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "A", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 98, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncryptMethod => GetNumber(98);
		/// <summary>
		/// FIX tag 108, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? HeartBtInt => GetNumber(108);
		/// <summary>
		/// FIX tag 95, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RawDataLength => GetNumber(95);
		/// <summary>
		/// FIX tag 96, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? RawData => GetText(96);
		/// <summary>
		/// FIX tag 141, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ResetSeqNumFlag => GetText(141);
		/// <summary>
		/// FIX tag 789, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NextExpectedMsgSeqNum => GetNumber(789);
		/// <summary>
		/// FIX tag 383, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxMessageSize => GetNumber(383);
		/// <summary>
		/// Entries of the group counted by NoMsgTypes, FIX tag 384; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MsgTypeGrp => GetGroup(384);
		/// <summary>
		/// FIX tag 464, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? TestMessageIndicator => GetText(464);
		/// <summary>
		/// FIX tag 553, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Username => GetText(553);
		/// <summary>
		/// FIX tag 554, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Password => GetText(554);
	}

	/// <summary>FIX 4.4 News, MsgType B.</summary>
	public sealed class News : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNews"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNews;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal News(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "B", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigTime => GetText(42);
		/// <summary>
		/// FIX tag 61, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Urgency => GetText(61);
		/// <summary>
		/// FIX tag 148, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Headline => GetText(148);
		/// <summary>
		/// FIX tag 358, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedHeadlineLen => GetNumber(358);
		/// <summary>
		/// FIX tag 359, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedHeadline => GetText(359);
		/// <summary>
		/// Entries of the group counted by NoRoutingIDs, FIX tag 215; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtGrp => GetGroup(146);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLinesOfText, FIX tag 33; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LinesOfTextGrp => GetGroup(33);
		/// <summary>
		/// FIX tag 149, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? URLLink => GetText(149);
		/// <summary>
		/// FIX tag 95, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RawDataLength => GetNumber(95);
		/// <summary>
		/// FIX tag 96, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? RawData => GetText(96);
	}

	/// <summary>FIX 4.4 Email, MsgType C.</summary>
	public sealed class Email : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateEmail"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateEmail;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Email(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "C", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 164, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? EmailThreadID => GetText(164);
		/// <summary>
		/// FIX tag 94, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? EmailType => GetText(94);
		/// <summary>
		/// FIX tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigTime => GetText(42);
		/// <summary>
		/// FIX tag 147, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Subject => GetText(147);
		/// <summary>
		/// FIX tag 356, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSubjectLen => GetNumber(356);
		/// <summary>
		/// FIX tag 357, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSubject => GetText(357);
		/// <summary>
		/// Entries of the group counted by NoRoutingIDs, FIX tag 215; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtGrp => GetGroup(146);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// Entries of the group counted by NoLinesOfText, FIX tag 33; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LinesOfTextGrp => GetGroup(33);
		/// <summary>
		/// FIX tag 95, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RawDataLength => GetNumber(95);
		/// <summary>
		/// FIX tag 96, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? RawData => GetText(96);
	}

	/// <summary>FIX 4.4 NewOrderSingle, MsgType D.</summary>
	public sealed class NewOrderSingle : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNewOrderSingle"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNewOrderSingle;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NewOrderSingle(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "D", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 589, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DayBookingInst => GetText(589);
		/// <summary>
		/// FIX tag 590, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BookingUnit => GetText(590);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PreAllocGrp => GetGroup(78);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 544, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CashMargin => GetText(544);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 140, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PrevClosePx => GetNumber(140);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 377, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? SolicitedFlag => GetText(377);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 121, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ForexReq => GetText(121);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 640, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price2 => GetNumber(640);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 203, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CoveredOrUncovered => GetNumber(203);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
	}

	/// <summary>FIX 4.4 NewOrderList, MsgType E.</summary>
	public sealed class NewOrderList : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNewOrderList"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNewOrderList;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NewOrderList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "E", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 390, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BidID => GetText(390);
		/// <summary>
		/// FIX tag 391, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClientBidID => GetText(391);
		/// <summary>
		/// FIX tag 414, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ProgRptReqs => GetNumber(414);
		/// <summary>
		/// FIX tag 394, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidType => GetNumber(394);
		/// <summary>
		/// FIX tag 415, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ProgPeriodInterval => GetNumber(415);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 433, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ListExecInstType => GetText(433);
		/// <summary>
		/// FIX tag 69, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListExecInst => GetText(69);
		/// <summary>
		/// FIX tag 352, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedListExecInstLen => GetNumber(352);
		/// <summary>
		/// FIX tag 353, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedListExecInst => GetText(353);
		/// <summary>
		/// FIX tag 765, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllowableOneSidednessPct => GetNumber(765);
		/// <summary>
		/// FIX tag 766, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllowableOneSidednessValue => GetNumber(766);
		/// <summary>
		/// FIX tag 767, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AllowableOneSidednessCurr => GetText(767);
		/// <summary>
		/// FIX tag 68, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoOrders => GetNumber(68);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ListOrdGrp => GetGroup(73);
	}

	/// <summary>FIX 4.4 OrderCancelRequest, MsgType F.</summary>
	public sealed class OrderCancelRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderCancelRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderCancelRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "F", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 41, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigClOrdID => GetText(41);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 586, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigOrdModTime => GetText(586);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 OrderCancelReplaceRequest, MsgType G.</summary>
	public sealed class OrderCancelReplaceRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderCancelReplaceRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderCancelReplaceRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderCancelReplaceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "G", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 41, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigClOrdID => GetText(41);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 586, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigOrdModTime => GetText(586);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 589, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DayBookingInst => GetText(589);
		/// <summary>
		/// FIX tag 590, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BookingUnit => GetText(590);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PreAllocGrp => GetGroup(78);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 544, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CashMargin => GetText(544);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 377, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? SolicitedFlag => GetText(377);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 121, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ForexReq => GetText(121);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 640, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price2 => GetNumber(640);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 203, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CoveredOrUncovered => GetNumber(203);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
	}

	/// <summary>FIX 4.4 OrderStatusRequest, MsgType H.</summary>
	public sealed class OrderStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "H", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 790, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrdStatusReqID => GetText(790);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
	}

	/// <summary>FIX 4.4 AllocationInstruction, MsgType J.</summary>
	public sealed class AllocationInstruction : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAllocationInstruction"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAllocationInstruction;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal AllocationInstruction(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "J", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// FIX tag 71, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? AllocTransType => GetText(71);
		/// <summary>
		/// FIX tag 626, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocType => GetNumber(626);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 72, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RefAllocID => GetText(72);
		/// <summary>
		/// FIX tag 796, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocCancReplaceReason => GetNumber(796);
		/// <summary>
		/// FIX tag 808, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocIntermedReqType => GetNumber(808);
		/// <summary>
		/// FIX tag 196, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocLinkID => GetText(196);
		/// <summary>
		/// FIX tag 197, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocLinkType => GetNumber(197);
		/// <summary>
		/// FIX tag 466, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BookingRefID => GetText(466);
		/// <summary>
		/// FIX tag 857, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocNoOrdersType => GetNumber(857);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecAllocGrp => GetGroup(124);
		/// <summary>
		/// FIX tag 570, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? PreviouslyReported => GetText(570);
		/// <summary>
		/// FIX tag 700, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ReversalIndicator => GetText(700);
		/// <summary>
		/// FIX tag 574, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MatchType => GetText(574);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 6, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPx => GetNumber(6);
		/// <summary>
		/// FIX tag 860, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgParPx => GetNumber(860);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 74, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPxPrecision => GetNumber(74);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 381, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GrossTradeAmt => GetNumber(381);
		/// <summary>
		/// FIX tag 238, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Concession => GetNumber(238);
		/// <summary>
		/// FIX tag 237, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalTakedown => GetNumber(237);
		/// <summary>
		/// FIX tag 118, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetMoney => GetNumber(118);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 754, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? AutoAcceptIndicator => GetText(754);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 157, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumDaysInterest => GetNumber(157);
		/// <summary>
		/// FIX tag 158, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestRate => GetNumber(158);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 540, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalAccruedInterestAmt => GetNumber(540);
		/// <summary>
		/// FIX tag 738, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? InterestAtMaturity => GetNumber(738);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 650, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LegalConfirm => GetText(650);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 892, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoAllocs => GetNumber(892);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AllocGrp => GetGroup(78);
	}

	/// <summary>FIX 4.4 ListCancelRequest, MsgType K.</summary>
	public sealed class ListCancelRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateListCancelRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateListCancelRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ListCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "K", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 ListExecute, MsgType L.</summary>
	public sealed class ListExecute : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateListExecute"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateListExecute;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ListExecute(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "L", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 391, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClientBidID => GetText(391);
		/// <summary>
		/// FIX tag 390, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BidID => GetText(390);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 ListStatusRequest, MsgType M.</summary>
	public sealed class ListStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateListStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateListStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ListStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "M", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 ListStatus, MsgType N.</summary>
	public sealed class ListStatus : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateListStatus"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateListStatus;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ListStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "N", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 429, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ListStatusType => GetNumber(429);
		/// <summary>
		/// FIX tag 82, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NoRpts => GetNumber(82);
		/// <summary>
		/// FIX tag 431, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ListOrderStatus => GetNumber(431);
		/// <summary>
		/// FIX tag 83, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RptSeq => GetNumber(83);
		/// <summary>
		/// FIX tag 444, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListStatusText => GetText(444);
		/// <summary>
		/// FIX tag 445, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedListStatusTextLen => GetNumber(445);
		/// <summary>
		/// FIX tag 446, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedListStatusText => GetText(446);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 68, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoOrders => GetNumber(68);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> OrdListStatGrp => GetGroup(73);
	}

	/// <summary>FIX 4.4 AllocationInstructionAck, MsgType P.</summary>
	public sealed class AllocationInstructionAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAllocationInstructionAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAllocationInstructionAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal AllocationInstructionAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "P", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 87, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocStatus => GetNumber(87);
		/// <summary>
		/// FIX tag 88, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocRejCode => GetNumber(88);
		/// <summary>
		/// FIX tag 626, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocType => GetNumber(626);
		/// <summary>
		/// FIX tag 808, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocIntermedReqType => GetNumber(808);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AllocAckGrp => GetGroup(78);
	}

	/// <summary>FIX 4.4 DontKnowTrade, MsgType Q.</summary>
	public sealed class DontKnowTrade : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateDontKnowTrade"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateDontKnowTrade;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal DontKnowTrade(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "Q", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 17, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecID => GetText(17);
		/// <summary>
		/// FIX tag 127, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DKReason => GetText(127);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 32, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastQty => GetNumber(32);
		/// <summary>
		/// FIX tag 31, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastPx => GetNumber(31);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 QuoteRequest, MsgType R.</summary>
	public sealed class QuoteRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "R", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 644, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RFQReqID => GetText(644);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotReqGrp => GetGroup(146);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 Quote, MsgType S.</summary>
	public sealed class Quote : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuote"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuote;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Quote(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "S", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 693, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteRespID => GetText(693);
		/// <summary>
		/// FIX tag 537, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteType => GetNumber(537);
		/// <summary>
		/// Entries of the group counted by NoQuoteQualifiers, FIX tag 735; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
		/// <summary>
		/// FIX tag 301, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteResponseLevel => GetNumber(301);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LegQuotGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 132, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidPx => GetNumber(132);
		/// <summary>
		/// FIX tag 133, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferPx => GetNumber(133);
		/// <summary>
		/// FIX tag 645, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktBidPx => GetNumber(645);
		/// <summary>
		/// FIX tag 646, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktOfferPx => GetNumber(646);
		/// <summary>
		/// FIX tag 647, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinBidSize => GetNumber(647);
		/// <summary>
		/// FIX tag 134, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSize => GetNumber(134);
		/// <summary>
		/// FIX tag 648, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinOfferSize => GetNumber(648);
		/// <summary>
		/// FIX tag 135, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSize => GetNumber(135);
		/// <summary>
		/// FIX tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ValidUntilTime => GetText(62);
		/// <summary>
		/// FIX tag 188, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSpotRate => GetNumber(188);
		/// <summary>
		/// FIX tag 190, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSpotRate => GetNumber(190);
		/// <summary>
		/// FIX tag 189, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints => GetNumber(189);
		/// <summary>
		/// FIX tag 191, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints => GetNumber(191);
		/// <summary>
		/// FIX tag 631, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidPx => GetNumber(631);
		/// <summary>
		/// FIX tag 632, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidYield => GetNumber(632);
		/// <summary>
		/// FIX tag 633, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidYield => GetNumber(633);
		/// <summary>
		/// FIX tag 634, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferYield => GetNumber(634);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 642, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints2 => GetNumber(642);
		/// <summary>
		/// FIX tag 643, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints2 => GetNumber(643);
		/// <summary>
		/// FIX tag 656, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrBidFxRate => GetNumber(656);
		/// <summary>
		/// FIX tag 657, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
		/// <summary>
		/// FIX tag 156, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrFxRateCalc => GetText(156);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 SettlementInstructions, MsgType T.</summary>
	public sealed class SettlementInstructions : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSettlementInstructions"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSettlementInstructions;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SettlementInstructions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "T", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 777, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlInstMsgID => GetText(777);
		/// <summary>
		/// FIX tag 791, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlInstReqID => GetText(791);
		/// <summary>
		/// FIX tag 160, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlInstMode => GetText(160);
		/// <summary>
		/// FIX tag 792, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlInstReqRejCode => GetNumber(792);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoSettlInst, FIX tag 778; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SettlInstGrp => GetGroup(778);
	}

	/// <summary>FIX 4.4 MarketDataRequest, MsgType V.</summary>
	public sealed class MarketDataRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMarketDataRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMarketDataRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MarketDataRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "V", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 262, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MDReqID => GetText(262);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 264, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarketDepth => GetNumber(264);
		/// <summary>
		/// FIX tag 265, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MDUpdateType => GetNumber(265);
		/// <summary>
		/// FIX tag 266, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? AggregatedBook => GetText(266);
		/// <summary>
		/// FIX tag 286, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OpenCloseSettlFlag => GetText(286);
		/// <summary>
		/// FIX tag 546, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? Scope => GetText(546);
		/// <summary>
		/// FIX tag 547, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? MDImplicitDelete => GetText(547);
		/// <summary>
		/// Entries of the group counted by NoMDEntryTypes, FIX tag 267; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MDReqGrp => GetGroup(267);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtMDReqGrp => GetGroup(146);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 815, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueAction => GetNumber(815);
		/// <summary>
		/// FIX tag 812, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueMax => GetNumber(812);
	}

	/// <summary>FIX 4.4 MarketDataSnapshotFullRefresh, MsgType W.</summary>
	public sealed class MarketDataSnapshotFullRefresh : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMarketDataSnapshotFullRefresh"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMarketDataSnapshotFullRefresh;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MarketDataSnapshotFullRefresh(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "W", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 262, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MDReqID => GetText(262);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 291, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? FinancialStatus => GetText(291);
		/// <summary>
		/// FIX tag 292, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? CorporateAction => GetText(292);
		/// <summary>
		/// FIX tag 451, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetChgPrevDay => GetNumber(451);
		/// <summary>
		/// Entries of the group counted by NoMDEntries, FIX tag 268; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MDFullGrp => GetGroup(268);
		/// <summary>
		/// FIX tag 813, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueDepth => GetNumber(813);
		/// <summary>
		/// FIX tag 814, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueResolution => GetNumber(814);
	}

	/// <summary>FIX 4.4 MarketDataIncrementalRefresh, MsgType X.</summary>
	public sealed class MarketDataIncrementalRefresh : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMarketDataIncrementalRefresh"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMarketDataIncrementalRefresh;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MarketDataIncrementalRefresh(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "X", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 262, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MDReqID => GetText(262);
		/// <summary>
		/// Entries of the group counted by NoMDEntries, FIX tag 268; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MDIncGrp => GetGroup(268);
		/// <summary>
		/// FIX tag 813, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueDepth => GetNumber(813);
		/// <summary>
		/// FIX tag 814, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ApplQueueResolution => GetNumber(814);
	}

	/// <summary>FIX 4.4 MarketDataRequestReject, MsgType Y.</summary>
	public sealed class MarketDataRequestReject : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMarketDataRequestReject"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMarketDataRequestReject;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MarketDataRequestReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "Y", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 262, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MDReqID => GetText(262);
		/// <summary>
		/// FIX tag 281, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MDReqRejReason => GetText(281);
		/// <summary>
		/// Entries of the group counted by NoAltMDSource, FIX tag 816; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MDRjctGrp => GetGroup(816);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 QuoteCancel, MsgType Z.</summary>
	public sealed class QuoteCancel : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteCancel"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteCancel;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteCancel(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "Z", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 298, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteCancelType => GetNumber(298);
		/// <summary>
		/// FIX tag 301, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteResponseLevel => GetNumber(301);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// Entries of the group counted by NoQuoteEntries, FIX tag 295; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotCxlEntriesGrp => GetGroup(295);
	}

	/// <summary>FIX 4.4 QuoteStatusRequest, MsgType a.</summary>
	public sealed class QuoteStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "a", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 649, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteStatusReqID => GetText(649);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 MassQuoteAcknowledgement, MsgType b.</summary>
	public sealed class MassQuoteAcknowledgement : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMassQuoteAcknowledgement"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMassQuoteAcknowledgement;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MassQuoteAcknowledgement(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "b", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 297, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteStatus => GetNumber(297);
		/// <summary>
		/// FIX tag 300, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteRejectReason => GetNumber(300);
		/// <summary>
		/// FIX tag 301, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteResponseLevel => GetNumber(301);
		/// <summary>
		/// FIX tag 537, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteType => GetNumber(537);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// Entries of the group counted by NoQuoteSets, FIX tag 296; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotSetAckGrp => GetGroup(296);
	}

	/// <summary>FIX 4.4 SecurityDefinitionRequest, MsgType c.</summary>
	public sealed class SecurityDefinitionRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityDefinitionRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityDefinitionRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityDefinitionRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "c", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 321, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityRequestType => GetNumber(321);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 827, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ExpirationCycle => GetNumber(827);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 SecurityDefinition, MsgType d.</summary>
	public sealed class SecurityDefinition : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityDefinition"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityDefinition;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityDefinition(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "d", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 322, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityResponseID => GetText(322);
		/// <summary>
		/// FIX tag 323, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityResponseType => GetNumber(323);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 827, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ExpirationCycle => GetNumber(827);
		/// <summary>
		/// FIX tag 561, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundLot => GetNumber(561);
		/// <summary>
		/// FIX tag 562, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinTradeVol => GetNumber(562);
	}

	/// <summary>FIX 4.4 SecurityStatusRequest, MsgType e.</summary>
	public sealed class SecurityStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "e", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 324, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityStatusReqID => GetText(324);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
	}

	/// <summary>FIX 4.4 SecurityStatus, MsgType f.</summary>
	public sealed class SecurityStatus : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityStatus"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityStatus;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "f", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 324, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityStatusReqID => GetText(324);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 325, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? UnsolicitedIndicator => GetText(325);
		/// <summary>
		/// FIX tag 326, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityTradingStatus => GetNumber(326);
		/// <summary>
		/// FIX tag 291, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? FinancialStatus => GetText(291);
		/// <summary>
		/// FIX tag 292, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? CorporateAction => GetText(292);
		/// <summary>
		/// FIX tag 327, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HaltReason => GetText(327);
		/// <summary>
		/// FIX tag 328, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? InViewOfCommon => GetText(328);
		/// <summary>
		/// FIX tag 329, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? DueToRelated => GetText(329);
		/// <summary>
		/// FIX tag 330, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BuyVolume => GetNumber(330);
		/// <summary>
		/// FIX tag 331, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SellVolume => GetNumber(331);
		/// <summary>
		/// FIX tag 332, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? HighPx => GetNumber(332);
		/// <summary>
		/// FIX tag 333, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LowPx => GetNumber(333);
		/// <summary>
		/// FIX tag 31, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastPx => GetNumber(31);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 334, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Adjustment => GetNumber(334);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 TradingSessionStatusRequest, MsgType g.</summary>
	public sealed class TradingSessionStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradingSessionStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradingSessionStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradingSessionStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "g", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 335, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradSesReqID => GetText(335);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 338, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesMethod => GetNumber(338);
		/// <summary>
		/// FIX tag 339, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesMode => GetNumber(339);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 TradingSessionStatus, MsgType h.</summary>
	public sealed class TradingSessionStatus : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradingSessionStatus"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradingSessionStatus;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradingSessionStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "h", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 335, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradSesReqID => GetText(335);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 338, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesMethod => GetNumber(338);
		/// <summary>
		/// FIX tag 339, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesMode => GetNumber(339);
		/// <summary>
		/// FIX tag 325, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? UnsolicitedIndicator => GetText(325);
		/// <summary>
		/// FIX tag 340, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesStatus => GetNumber(340);
		/// <summary>
		/// FIX tag 567, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradSesStatusRejReason => GetNumber(567);
		/// <summary>
		/// FIX tag 341, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TradSesStartTime => GetText(341);
		/// <summary>
		/// FIX tag 342, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TradSesOpenTime => GetText(342);
		/// <summary>
		/// FIX tag 343, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TradSesPreCloseTime => GetText(343);
		/// <summary>
		/// FIX tag 344, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TradSesCloseTime => GetText(344);
		/// <summary>
		/// FIX tag 345, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TradSesEndTime => GetText(345);
		/// <summary>
		/// FIX tag 387, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalVolumeTraded => GetNumber(387);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 MassQuote, MsgType i.</summary>
	public sealed class MassQuote : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMassQuote"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMassQuote;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MassQuote(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "i", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 537, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteType => GetNumber(537);
		/// <summary>
		/// FIX tag 301, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteResponseLevel => GetNumber(301);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 293, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DefBidSize => GetNumber(293);
		/// <summary>
		/// FIX tag 294, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DefOfferSize => GetNumber(294);
		/// <summary>
		/// Entries of the group counted by NoQuoteSets, FIX tag 296; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotSetGrp => GetGroup(296);
	}

	/// <summary>FIX 4.4 BusinessMessageReject, MsgType j.</summary>
	public sealed class BusinessMessageReject : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateBusinessMessageReject"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateBusinessMessageReject;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal BusinessMessageReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "j", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 45, wire type <c>SeqNum</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RefSeqNum => GetNumber(45);
		/// <summary>
		/// FIX tag 372, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RefMsgType => GetText(372);
		/// <summary>
		/// FIX tag 379, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BusinessRejectRefID => GetText(379);
		/// <summary>
		/// FIX tag 380, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BusinessRejectReason => GetNumber(380);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 BidRequest, MsgType k.</summary>
	public sealed class BidRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateBidRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateBidRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal BidRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "k", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 390, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BidID => GetText(390);
		/// <summary>
		/// FIX tag 391, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClientBidID => GetText(391);
		/// <summary>
		/// FIX tag 374, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BidRequestTransType => GetText(374);
		/// <summary>
		/// FIX tag 392, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListName => GetText(392);
		/// <summary>
		/// FIX tag 393, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoRelatedSym => GetNumber(393);
		/// <summary>
		/// FIX tag 394, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidType => GetNumber(394);
		/// <summary>
		/// FIX tag 395, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumTickets => GetNumber(395);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 396, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SideValue1 => GetNumber(396);
		/// <summary>
		/// FIX tag 397, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SideValue2 => GetNumber(397);
		/// <summary>
		/// Entries of the group counted by NoBidDescriptors, FIX tag 398; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> BidDescReqGrp => GetGroup(398);
		/// <summary>
		/// Entries of the group counted by NoBidComponents, FIX tag 420; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> BidCompReqGrp => GetGroup(420);
		/// <summary>
		/// FIX tag 409, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LiquidityIndType => GetNumber(409);
		/// <summary>
		/// FIX tag 410, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? WtAverageLiquidity => GetNumber(410);
		/// <summary>
		/// FIX tag 411, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ExchangeForPhysical => GetText(411);
		/// <summary>
		/// FIX tag 412, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OutMainCntryUIndex => GetNumber(412);
		/// <summary>
		/// FIX tag 413, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossPercent => GetNumber(413);
		/// <summary>
		/// FIX tag 414, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ProgRptReqs => GetNumber(414);
		/// <summary>
		/// FIX tag 415, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ProgPeriodInterval => GetNumber(415);
		/// <summary>
		/// FIX tag 416, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? IncTaxInd => GetNumber(416);
		/// <summary>
		/// FIX tag 121, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ForexReq => GetText(121);
		/// <summary>
		/// FIX tag 417, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumBidders => GetNumber(417);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 418, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BidTradeType => GetText(418);
		/// <summary>
		/// FIX tag 419, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BasisPxType => GetText(419);
		/// <summary>
		/// FIX tag 443, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? StrikeTime => GetText(443);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 BidResponse, MsgType l.</summary>
	public sealed class BidResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateBidResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateBidResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal BidResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "l", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 390, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BidID => GetText(390);
		/// <summary>
		/// FIX tag 391, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClientBidID => GetText(391);
		/// <summary>
		/// Entries of the group counted by NoBidComponents, FIX tag 420; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> BidCompRspGrp => GetGroup(420);
	}

	/// <summary>FIX 4.4 ListStrikePrice, MsgType m.</summary>
	public sealed class ListStrikePrice : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateListStrikePrice"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateListStrikePrice;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ListStrikePrice(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "m", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 66, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ListID => GetText(66);
		/// <summary>
		/// FIX tag 422, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoStrikes => GetNumber(422);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoStrikes, FIX tag 428; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtStrkPxGrp => GetGroup(428);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtStrkPxGrp => GetGroup(711);
	}

	/// <summary>FIX 4.4 XMLnonFIX, MsgType n.</summary>
	public sealed class XMLnonFIX : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateXMLnonFIX"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateXMLnonFIX;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal XMLnonFIX(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "n", header, body, trailer)
		{
		}

	}

	/// <summary>FIX 4.4 RegistrationInstructions, MsgType o.</summary>
	public sealed class RegistrationInstructions : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateRegistrationInstructions"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateRegistrationInstructions;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal RegistrationInstructions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "o", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 514, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RegistTransType => GetText(514);
		/// <summary>
		/// FIX tag 508, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistRefID => GetText(508);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 493, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistAcctType => GetText(493);
		/// <summary>
		/// FIX tag 495, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TaxAdvantageType => GetNumber(495);
		/// <summary>
		/// FIX tag 517, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OwnershipType => GetText(517);
		/// <summary>
		/// Entries of the group counted by NoRegistDtls, FIX tag 473; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RgstDtlsGrp => GetGroup(473);
		/// <summary>
		/// Entries of the group counted by NoDistribInsts, FIX tag 510; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RgstDistInstGrp => GetGroup(510);
	}

	/// <summary>FIX 4.4 RegistrationInstructionsResponse, MsgType p.</summary>
	public sealed class RegistrationInstructionsResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateRegistrationInstructionsResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateRegistrationInstructionsResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal RegistrationInstructionsResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "p", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 514, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RegistTransType => GetText(514);
		/// <summary>
		/// FIX tag 508, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistRefID => GetText(508);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 506, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RegistStatus => GetText(506);
		/// <summary>
		/// FIX tag 507, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RegistRejReasonCode => GetNumber(507);
		/// <summary>
		/// FIX tag 496, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistRejReasonText => GetText(496);
	}

	/// <summary>FIX 4.4 OrderMassCancelRequest, MsgType q.</summary>
	public sealed class OrderMassCancelRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderMassCancelRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderMassCancelRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderMassCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "q", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 530, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MassCancelRequestType => GetText(530);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 311, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbol => GetText(311);
		/// <summary>
		/// FIX tag 312, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbolSfx => GetText(312);
		/// <summary>
		/// FIX tag 309, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityID => GetText(309);
		/// <summary>
		/// FIX tag 305, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityIDSource => GetText(305);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingSecurityAltID, FIX tag 457; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
		/// <summary>
		/// FIX tag 462, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingProduct => GetNumber(462);
		/// <summary>
		/// FIX tag 463, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCFICode => GetText(463);
		/// <summary>
		/// FIX tag 310, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityType => GetText(310);
		/// <summary>
		/// FIX tag 763, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecuritySubType => GetText(763);
		/// <summary>
		/// FIX tag 313, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityMonthYear => GetText(313);
		/// <summary>
		/// FIX tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityDate => GetText(542);
		/// <summary>
		/// FIX tag 315, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPutOrCall => GetNumber(315);
		/// <summary>
		/// FIX tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCouponPaymentDate => GetText(241);
		/// <summary>
		/// FIX tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssueDate => GetText(242);
		/// <summary>
		/// FIX tag 243, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRepoCollateralSecurityType => GetText(243);
		/// <summary>
		/// FIX tag 244, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
		/// <summary>
		/// FIX tag 245, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
		/// <summary>
		/// FIX tag 246, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingFactor => GetNumber(246);
		/// <summary>
		/// FIX tag 256, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCreditRating => GetText(256);
		/// <summary>
		/// FIX tag 595, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingInstrRegistry => GetText(595);
		/// <summary>
		/// FIX tag 592, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCountryOfIssue => GetText(592);
		/// <summary>
		/// FIX tag 593, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
		/// <summary>
		/// FIX tag 594, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingLocaleOfIssue => GetText(594);
		/// <summary>
		/// FIX tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRedemptionDate => GetText(247);
		/// <summary>
		/// FIX tag 316, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStrikePrice => GetNumber(316);
		/// <summary>
		/// FIX tag 941, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStrikeCurrency => GetText(941);
		/// <summary>
		/// FIX tag 317, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingOptAttribute => GetText(317);
		/// <summary>
		/// FIX tag 436, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
		/// <summary>
		/// FIX tag 435, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCouponRate => GetNumber(435);
		/// <summary>
		/// FIX tag 308, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityExchange => GetText(308);
		/// <summary>
		/// FIX tag 306, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssuer => GetText(306);
		/// <summary>
		/// FIX tag 362, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
		/// <summary>
		/// FIX tag 363, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingIssuer => GetText(363);
		/// <summary>
		/// FIX tag 307, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityDesc => GetText(307);
		/// <summary>
		/// FIX tag 364, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
		/// <summary>
		/// FIX tag 365, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingSecurityDesc => GetText(365);
		/// <summary>
		/// FIX tag 877, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPProgram => GetText(877);
		/// <summary>
		/// FIX tag 878, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPRegType => GetText(878);
		/// <summary>
		/// FIX tag 318, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCurrency => GetText(318);
		/// <summary>
		/// FIX tag 879, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingQty => GetNumber(879);
		/// <summary>
		/// FIX tag 810, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPx => GetNumber(810);
		/// <summary>
		/// FIX tag 882, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
		/// <summary>
		/// FIX tag 883, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndPrice => GetNumber(883);
		/// <summary>
		/// FIX tag 884, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStartValue => GetNumber(884);
		/// <summary>
		/// FIX tag 885, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCurrentValue => GetNumber(885);
		/// <summary>
		/// FIX tag 886, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndValue => GetNumber(886);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingStips, FIX tag 887; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 OrderMassCancelReport, MsgType r.</summary>
	public sealed class OrderMassCancelReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderMassCancelReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderMassCancelReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderMassCancelReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "r", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 530, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MassCancelRequestType => GetText(530);
		/// <summary>
		/// FIX tag 531, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MassCancelResponse => GetText(531);
		/// <summary>
		/// FIX tag 532, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MassCancelRejectReason => GetNumber(532);
		/// <summary>
		/// FIX tag 533, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalAffectedOrders => GetNumber(533);
		/// <summary>
		/// Entries of the group counted by NoAffectedOrders, FIX tag 534; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AffectedOrdGrp => GetGroup(534);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 311, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbol => GetText(311);
		/// <summary>
		/// FIX tag 312, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbolSfx => GetText(312);
		/// <summary>
		/// FIX tag 309, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityID => GetText(309);
		/// <summary>
		/// FIX tag 305, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityIDSource => GetText(305);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingSecurityAltID, FIX tag 457; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
		/// <summary>
		/// FIX tag 462, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingProduct => GetNumber(462);
		/// <summary>
		/// FIX tag 463, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCFICode => GetText(463);
		/// <summary>
		/// FIX tag 310, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityType => GetText(310);
		/// <summary>
		/// FIX tag 763, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecuritySubType => GetText(763);
		/// <summary>
		/// FIX tag 313, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityMonthYear => GetText(313);
		/// <summary>
		/// FIX tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityDate => GetText(542);
		/// <summary>
		/// FIX tag 315, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPutOrCall => GetNumber(315);
		/// <summary>
		/// FIX tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCouponPaymentDate => GetText(241);
		/// <summary>
		/// FIX tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssueDate => GetText(242);
		/// <summary>
		/// FIX tag 243, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRepoCollateralSecurityType => GetText(243);
		/// <summary>
		/// FIX tag 244, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
		/// <summary>
		/// FIX tag 245, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
		/// <summary>
		/// FIX tag 246, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingFactor => GetNumber(246);
		/// <summary>
		/// FIX tag 256, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCreditRating => GetText(256);
		/// <summary>
		/// FIX tag 595, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingInstrRegistry => GetText(595);
		/// <summary>
		/// FIX tag 592, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCountryOfIssue => GetText(592);
		/// <summary>
		/// FIX tag 593, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
		/// <summary>
		/// FIX tag 594, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingLocaleOfIssue => GetText(594);
		/// <summary>
		/// FIX tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRedemptionDate => GetText(247);
		/// <summary>
		/// FIX tag 316, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStrikePrice => GetNumber(316);
		/// <summary>
		/// FIX tag 941, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStrikeCurrency => GetText(941);
		/// <summary>
		/// FIX tag 317, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingOptAttribute => GetText(317);
		/// <summary>
		/// FIX tag 436, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
		/// <summary>
		/// FIX tag 435, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCouponRate => GetNumber(435);
		/// <summary>
		/// FIX tag 308, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityExchange => GetText(308);
		/// <summary>
		/// FIX tag 306, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssuer => GetText(306);
		/// <summary>
		/// FIX tag 362, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
		/// <summary>
		/// FIX tag 363, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingIssuer => GetText(363);
		/// <summary>
		/// FIX tag 307, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityDesc => GetText(307);
		/// <summary>
		/// FIX tag 364, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
		/// <summary>
		/// FIX tag 365, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingSecurityDesc => GetText(365);
		/// <summary>
		/// FIX tag 877, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPProgram => GetText(877);
		/// <summary>
		/// FIX tag 878, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPRegType => GetText(878);
		/// <summary>
		/// FIX tag 318, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCurrency => GetText(318);
		/// <summary>
		/// FIX tag 879, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingQty => GetNumber(879);
		/// <summary>
		/// FIX tag 810, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPx => GetNumber(810);
		/// <summary>
		/// FIX tag 882, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
		/// <summary>
		/// FIX tag 883, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndPrice => GetNumber(883);
		/// <summary>
		/// FIX tag 884, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStartValue => GetNumber(884);
		/// <summary>
		/// FIX tag 885, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCurrentValue => GetNumber(885);
		/// <summary>
		/// FIX tag 886, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndValue => GetNumber(886);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingStips, FIX tag 887; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 NewOrderCross, MsgType s.</summary>
	public sealed class NewOrderCross : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNewOrderCross"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNewOrderCross;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NewOrderCross(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "s", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 548, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CrossID => GetText(548);
		/// <summary>
		/// FIX tag 549, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossType => GetNumber(549);
		/// <summary>
		/// FIX tag 550, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossPrioritization => GetNumber(550);
		/// <summary>
		/// Entries of the group counted by NoSides, FIX tag 552; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SideCrossOrdModGrp => GetGroup(552);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 140, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PrevClosePx => GetNumber(140);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
	}

	/// <summary>FIX 4.4 CrossOrderCancelReplaceRequest, MsgType t.</summary>
	public sealed class CrossOrderCancelReplaceRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCrossOrderCancelReplaceRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCrossOrderCancelReplaceRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CrossOrderCancelReplaceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "t", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 548, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CrossID => GetText(548);
		/// <summary>
		/// FIX tag 551, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigCrossID => GetText(551);
		/// <summary>
		/// FIX tag 549, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossType => GetNumber(549);
		/// <summary>
		/// FIX tag 550, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossPrioritization => GetNumber(550);
		/// <summary>
		/// Entries of the group counted by NoSides, FIX tag 552; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SideCrossOrdModGrp => GetGroup(552);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 140, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PrevClosePx => GetNumber(140);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
	}

	/// <summary>FIX 4.4 CrossOrderCancelRequest, MsgType u.</summary>
	public sealed class CrossOrderCancelRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCrossOrderCancelRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCrossOrderCancelRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CrossOrderCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "u", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 548, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CrossID => GetText(548);
		/// <summary>
		/// FIX tag 551, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigCrossID => GetText(551);
		/// <summary>
		/// FIX tag 549, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossType => GetNumber(549);
		/// <summary>
		/// FIX tag 550, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CrossPrioritization => GetNumber(550);
		/// <summary>
		/// Entries of the group counted by NoSides, FIX tag 552; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SideCrossOrdCxlGrp => GetGroup(552);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
	}

	/// <summary>FIX 4.4 SecurityTypeRequest, MsgType v.</summary>
	public sealed class SecurityTypeRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityTypeRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityTypeRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityTypeRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "v", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
	}

	/// <summary>FIX 4.4 SecurityTypes, MsgType w.</summary>
	public sealed class SecurityTypes : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityTypes"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityTypes;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityTypes(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "w", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 322, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityResponseID => GetText(322);
		/// <summary>
		/// FIX tag 323, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityResponseType => GetNumber(323);
		/// <summary>
		/// FIX tag 557, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoSecurityTypes => GetNumber(557);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoSecurityTypes, FIX tag 558; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecTypesGrp => GetGroup(558);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 SecurityListRequest, MsgType x.</summary>
	public sealed class SecurityListRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityListRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityListRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityListRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "x", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 559, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityListRequestType => GetNumber(559);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 SecurityList, MsgType y.</summary>
	public sealed class SecurityList : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSecurityList"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSecurityList;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SecurityList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "y", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 322, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityResponseID => GetText(322);
		/// <summary>
		/// FIX tag 560, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityRequestResult => GetNumber(560);
		/// <summary>
		/// FIX tag 393, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoRelatedSym => GetNumber(393);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecListGrp => GetGroup(146);
	}

	/// <summary>FIX 4.4 DerivativeSecurityListRequest, MsgType z.</summary>
	public sealed class DerivativeSecurityListRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateDerivativeSecurityListRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateDerivativeSecurityListRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal DerivativeSecurityListRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "z", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 559, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityListRequestType => GetNumber(559);
		/// <summary>
		/// FIX tag 311, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbol => GetText(311);
		/// <summary>
		/// FIX tag 312, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbolSfx => GetText(312);
		/// <summary>
		/// FIX tag 309, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityID => GetText(309);
		/// <summary>
		/// FIX tag 305, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityIDSource => GetText(305);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingSecurityAltID, FIX tag 457; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
		/// <summary>
		/// FIX tag 462, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingProduct => GetNumber(462);
		/// <summary>
		/// FIX tag 463, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCFICode => GetText(463);
		/// <summary>
		/// FIX tag 310, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityType => GetText(310);
		/// <summary>
		/// FIX tag 763, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecuritySubType => GetText(763);
		/// <summary>
		/// FIX tag 313, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityMonthYear => GetText(313);
		/// <summary>
		/// FIX tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityDate => GetText(542);
		/// <summary>
		/// FIX tag 315, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPutOrCall => GetNumber(315);
		/// <summary>
		/// FIX tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCouponPaymentDate => GetText(241);
		/// <summary>
		/// FIX tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssueDate => GetText(242);
		/// <summary>
		/// FIX tag 243, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRepoCollateralSecurityType => GetText(243);
		/// <summary>
		/// FIX tag 244, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
		/// <summary>
		/// FIX tag 245, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
		/// <summary>
		/// FIX tag 246, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingFactor => GetNumber(246);
		/// <summary>
		/// FIX tag 256, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCreditRating => GetText(256);
		/// <summary>
		/// FIX tag 595, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingInstrRegistry => GetText(595);
		/// <summary>
		/// FIX tag 592, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCountryOfIssue => GetText(592);
		/// <summary>
		/// FIX tag 593, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
		/// <summary>
		/// FIX tag 594, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingLocaleOfIssue => GetText(594);
		/// <summary>
		/// FIX tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRedemptionDate => GetText(247);
		/// <summary>
		/// FIX tag 316, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStrikePrice => GetNumber(316);
		/// <summary>
		/// FIX tag 941, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStrikeCurrency => GetText(941);
		/// <summary>
		/// FIX tag 317, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingOptAttribute => GetText(317);
		/// <summary>
		/// FIX tag 436, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
		/// <summary>
		/// FIX tag 435, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCouponRate => GetNumber(435);
		/// <summary>
		/// FIX tag 308, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityExchange => GetText(308);
		/// <summary>
		/// FIX tag 306, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssuer => GetText(306);
		/// <summary>
		/// FIX tag 362, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
		/// <summary>
		/// FIX tag 363, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingIssuer => GetText(363);
		/// <summary>
		/// FIX tag 307, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityDesc => GetText(307);
		/// <summary>
		/// FIX tag 364, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
		/// <summary>
		/// FIX tag 365, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingSecurityDesc => GetText(365);
		/// <summary>
		/// FIX tag 877, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPProgram => GetText(877);
		/// <summary>
		/// FIX tag 878, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPRegType => GetText(878);
		/// <summary>
		/// FIX tag 318, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCurrency => GetText(318);
		/// <summary>
		/// FIX tag 879, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingQty => GetNumber(879);
		/// <summary>
		/// FIX tag 810, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPx => GetNumber(810);
		/// <summary>
		/// FIX tag 882, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
		/// <summary>
		/// FIX tag 883, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndPrice => GetNumber(883);
		/// <summary>
		/// FIX tag 884, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStartValue => GetNumber(884);
		/// <summary>
		/// FIX tag 885, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCurrentValue => GetNumber(885);
		/// <summary>
		/// FIX tag 886, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndValue => GetNumber(886);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingStips, FIX tag 887; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 DerivativeSecurityList, MsgType AA.</summary>
	public sealed class DerivativeSecurityList : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateDerivativeSecurityList"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateDerivativeSecurityList;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal DerivativeSecurityList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AA", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 320, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityReqID => GetText(320);
		/// <summary>
		/// FIX tag 322, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityResponseID => GetText(322);
		/// <summary>
		/// FIX tag 560, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecurityRequestResult => GetNumber(560);
		/// <summary>
		/// FIX tag 311, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbol => GetText(311);
		/// <summary>
		/// FIX tag 312, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbolSfx => GetText(312);
		/// <summary>
		/// FIX tag 309, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityID => GetText(309);
		/// <summary>
		/// FIX tag 305, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityIDSource => GetText(305);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingSecurityAltID, FIX tag 457; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
		/// <summary>
		/// FIX tag 462, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingProduct => GetNumber(462);
		/// <summary>
		/// FIX tag 463, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCFICode => GetText(463);
		/// <summary>
		/// FIX tag 310, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityType => GetText(310);
		/// <summary>
		/// FIX tag 763, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecuritySubType => GetText(763);
		/// <summary>
		/// FIX tag 313, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityMonthYear => GetText(313);
		/// <summary>
		/// FIX tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityDate => GetText(542);
		/// <summary>
		/// FIX tag 315, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPutOrCall => GetNumber(315);
		/// <summary>
		/// FIX tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCouponPaymentDate => GetText(241);
		/// <summary>
		/// FIX tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssueDate => GetText(242);
		/// <summary>
		/// FIX tag 243, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRepoCollateralSecurityType => GetText(243);
		/// <summary>
		/// FIX tag 244, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
		/// <summary>
		/// FIX tag 245, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
		/// <summary>
		/// FIX tag 246, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingFactor => GetNumber(246);
		/// <summary>
		/// FIX tag 256, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCreditRating => GetText(256);
		/// <summary>
		/// FIX tag 595, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingInstrRegistry => GetText(595);
		/// <summary>
		/// FIX tag 592, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCountryOfIssue => GetText(592);
		/// <summary>
		/// FIX tag 593, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
		/// <summary>
		/// FIX tag 594, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingLocaleOfIssue => GetText(594);
		/// <summary>
		/// FIX tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRedemptionDate => GetText(247);
		/// <summary>
		/// FIX tag 316, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStrikePrice => GetNumber(316);
		/// <summary>
		/// FIX tag 941, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStrikeCurrency => GetText(941);
		/// <summary>
		/// FIX tag 317, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingOptAttribute => GetText(317);
		/// <summary>
		/// FIX tag 436, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
		/// <summary>
		/// FIX tag 435, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCouponRate => GetNumber(435);
		/// <summary>
		/// FIX tag 308, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityExchange => GetText(308);
		/// <summary>
		/// FIX tag 306, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssuer => GetText(306);
		/// <summary>
		/// FIX tag 362, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
		/// <summary>
		/// FIX tag 363, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingIssuer => GetText(363);
		/// <summary>
		/// FIX tag 307, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityDesc => GetText(307);
		/// <summary>
		/// FIX tag 364, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
		/// <summary>
		/// FIX tag 365, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingSecurityDesc => GetText(365);
		/// <summary>
		/// FIX tag 877, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPProgram => GetText(877);
		/// <summary>
		/// FIX tag 878, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPRegType => GetText(878);
		/// <summary>
		/// FIX tag 318, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCurrency => GetText(318);
		/// <summary>
		/// FIX tag 879, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingQty => GetNumber(879);
		/// <summary>
		/// FIX tag 810, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPx => GetNumber(810);
		/// <summary>
		/// FIX tag 882, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
		/// <summary>
		/// FIX tag 883, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndPrice => GetNumber(883);
		/// <summary>
		/// FIX tag 884, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStartValue => GetNumber(884);
		/// <summary>
		/// FIX tag 885, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCurrentValue => GetNumber(885);
		/// <summary>
		/// FIX tag 886, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndValue => GetNumber(886);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingStips, FIX tag 887; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
		/// <summary>
		/// FIX tag 393, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoRelatedSym => GetNumber(393);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RelSymDerivSecGrp => GetGroup(146);
	}

	/// <summary>FIX 4.4 NewOrderMultileg, MsgType AB.</summary>
	public sealed class NewOrderMultileg : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNewOrderMultileg"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNewOrderMultileg;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NewOrderMultileg(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AB", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 589, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DayBookingInst => GetText(589);
		/// <summary>
		/// FIX tag 590, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BookingUnit => GetText(590);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PreAllocMlegGrp => GetGroup(78);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 544, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CashMargin => GetText(544);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 140, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PrevClosePx => GetNumber(140);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LegOrdGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 377, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? SolicitedFlag => GetText(377);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 121, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ForexReq => GetText(121);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 203, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CoveredOrUncovered => GetNumber(203);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
		/// <summary>
		/// FIX tag 563, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MultiLegRptTypeReq => GetNumber(563);
	}

	/// <summary>FIX 4.4 MultilegOrderCancelReplace, MsgType AC.</summary>
	public sealed class MultilegOrderCancelReplace : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateMultilegOrderCancelReplace"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateMultilegOrderCancelReplace;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal MultilegOrderCancelReplace(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AC", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 41, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigClOrdID => GetText(41);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// FIX tag 583, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdLinkID => GetText(583);
		/// <summary>
		/// FIX tag 586, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? OrigOrdModTime => GetText(586);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 589, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DayBookingInst => GetText(589);
		/// <summary>
		/// FIX tag 590, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? BookingUnit => GetText(590);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PreAllocMlegGrp => GetGroup(78);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 544, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CashMargin => GetText(544);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 21, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? HandlInst => GetText(21);
		/// <summary>
		/// FIX tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? ExecInst => GetText(18);
		/// <summary>
		/// FIX tag 110, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinQty => GetNumber(110);
		/// <summary>
		/// FIX tag 111, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxFloor => GetNumber(111);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 140, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PrevClosePx => GetNumber(140);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LegOrdGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 114, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LocateReqd => GetText(114);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 99, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StopPx => GetNumber(99);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 376, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ComplianceID => GetText(376);
		/// <summary>
		/// FIX tag 377, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? SolicitedFlag => GetText(377);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 59, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? TimeInForce => GetText(59);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 427, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GTBookingInst => GetNumber(427);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 121, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ForexReq => GetText(121);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 203, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CoveredOrUncovered => GetNumber(203);
		/// <summary>
		/// FIX tag 210, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaxShow => GetNumber(210);
		/// <summary>
		/// FIX tag 211, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetValue => GetNumber(211);
		/// <summary>
		/// FIX tag 835, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegMoveType => GetNumber(835);
		/// <summary>
		/// FIX tag 836, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegOffsetType => GetNumber(836);
		/// <summary>
		/// FIX tag 837, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegLimitType => GetNumber(837);
		/// <summary>
		/// FIX tag 838, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegRoundDirection => GetNumber(838);
		/// <summary>
		/// FIX tag 840, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PegScope => GetNumber(840);
		/// <summary>
		/// FIX tag 388, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? DiscretionInst => GetText(388);
		/// <summary>
		/// FIX tag 389, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetValue => GetNumber(389);
		/// <summary>
		/// FIX tag 841, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionMoveType => GetNumber(841);
		/// <summary>
		/// FIX tag 842, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionOffsetType => GetNumber(842);
		/// <summary>
		/// FIX tag 843, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionLimitType => GetNumber(843);
		/// <summary>
		/// FIX tag 844, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionRoundDirection => GetNumber(844);
		/// <summary>
		/// FIX tag 846, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DiscretionScope => GetNumber(846);
		/// <summary>
		/// FIX tag 847, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TargetStrategy => GetNumber(847);
		/// <summary>
		/// FIX tag 848, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TargetStrategyParameters => GetText(848);
		/// <summary>
		/// FIX tag 849, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ParticipationRate => GetNumber(849);
		/// <summary>
		/// FIX tag 480, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CancellationRights => GetText(480);
		/// <summary>
		/// FIX tag 481, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MoneyLaunderingStatus => GetText(481);
		/// <summary>
		/// FIX tag 513, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RegistID => GetText(513);
		/// <summary>
		/// FIX tag 494, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Designation => GetText(494);
		/// <summary>
		/// FIX tag 563, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MultiLegRptTypeReq => GetNumber(563);
	}

	/// <summary>FIX 4.4 TradeCaptureReportRequest, MsgType AD.</summary>
	public sealed class TradeCaptureReportRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradeCaptureReportRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradeCaptureReportRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradeCaptureReportRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AD", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 568, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeRequestID => GetText(568);
		/// <summary>
		/// FIX tag 569, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeRequestType => GetNumber(569);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 571, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeReportID => GetText(571);
		/// <summary>
		/// FIX tag 818, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryTradeReportID => GetText(818);
		/// <summary>
		/// FIX tag 17, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecID => GetText(17);
		/// <summary>
		/// FIX tag 150, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExecType => GetText(150);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 828, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdType => GetNumber(828);
		/// <summary>
		/// FIX tag 829, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdSubType => GetNumber(829);
		/// <summary>
		/// FIX tag 830, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TransferReason => GetText(830);
		/// <summary>
		/// FIX tag 855, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecondaryTrdType => GetNumber(855);
		/// <summary>
		/// FIX tag 820, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeLinkID => GetText(820);
		/// <summary>
		/// FIX tag 880, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TrdMatchID => GetText(880);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoDates, FIX tag 580; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCapDtGrp => GetGroup(580);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 943, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TimeBracket => GetText(943);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 442, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MultiLegReportingType => GetText(442);
		/// <summary>
		/// FIX tag 578, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeInputSource => GetText(578);
		/// <summary>
		/// FIX tag 579, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeInputDevice => GetText(579);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 TradeCaptureReport, MsgType AE.</summary>
	public sealed class TradeCaptureReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradeCaptureReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradeCaptureReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradeCaptureReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AE", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 571, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeReportID => GetText(571);
		/// <summary>
		/// FIX tag 487, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeReportTransType => GetNumber(487);
		/// <summary>
		/// FIX tag 856, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeReportType => GetNumber(856);
		/// <summary>
		/// FIX tag 568, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeRequestID => GetText(568);
		/// <summary>
		/// FIX tag 828, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdType => GetNumber(828);
		/// <summary>
		/// FIX tag 829, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdSubType => GetNumber(829);
		/// <summary>
		/// FIX tag 855, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecondaryTrdType => GetNumber(855);
		/// <summary>
		/// FIX tag 830, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TransferReason => GetText(830);
		/// <summary>
		/// FIX tag 150, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExecType => GetText(150);
		/// <summary>
		/// FIX tag 748, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumTradeReports => GetNumber(748);
		/// <summary>
		/// FIX tag 912, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastRptRequested => GetText(912);
		/// <summary>
		/// FIX tag 325, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? UnsolicitedIndicator => GetText(325);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 572, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeReportRefID => GetText(572);
		/// <summary>
		/// FIX tag 881, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryTradeReportRefID => GetText(881);
		/// <summary>
		/// FIX tag 818, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryTradeReportID => GetText(818);
		/// <summary>
		/// FIX tag 820, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeLinkID => GetText(820);
		/// <summary>
		/// FIX tag 880, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TrdMatchID => GetText(880);
		/// <summary>
		/// FIX tag 17, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecID => GetText(17);
		/// <summary>
		/// FIX tag 39, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdStatus => GetText(39);
		/// <summary>
		/// FIX tag 527, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryExecID => GetText(527);
		/// <summary>
		/// FIX tag 378, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ExecRestatementReason => GetNumber(378);
		/// <summary>
		/// FIX tag 570, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? PreviouslyReported => GetText(570);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 822, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingTradingSessionID => GetText(822);
		/// <summary>
		/// FIX tag 823, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingTradingSessionSubID => GetText(823);
		/// <summary>
		/// FIX tag 32, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastQty => GetNumber(32);
		/// <summary>
		/// FIX tag 31, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastPx => GetNumber(31);
		/// <summary>
		/// FIX tag 669, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastParPx => GetNumber(669);
		/// <summary>
		/// FIX tag 194, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastSpotRate => GetNumber(194);
		/// <summary>
		/// FIX tag 195, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? LastForwardPoints => GetNumber(195);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 6, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPx => GetNumber(6);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 819, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPxIndicator => GetNumber(819);
		/// <summary>
		/// Entries of the group counted by NoPosAmt, FIX tag 753; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
		/// <summary>
		/// FIX tag 442, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MultiLegReportingType => GetText(442);
		/// <summary>
		/// FIX tag 824, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeLegRefID => GetText(824);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdInstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 574, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MatchType => GetText(574);
		/// <summary>
		/// Entries of the group counted by NoSides, FIX tag 552; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCapRptSideGrp => GetGroup(552);
		/// <summary>
		/// FIX tag 797, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? CopyMsgIndicator => GetText(797);
		/// <summary>
		/// FIX tag 852, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? PublishTrdIndicator => GetText(852);
		/// <summary>
		/// FIX tag 853, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ShortSaleReason => GetNumber(853);
	}

	/// <summary>FIX 4.4 OrderMassStatusRequest, MsgType AF.</summary>
	public sealed class OrderMassStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateOrderMassStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateOrderMassStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal OrderMassStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AF", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 584, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MassStatusReqID => GetText(584);
		/// <summary>
		/// FIX tag 585, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MassStatusReqType => GetNumber(585);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 311, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbol => GetText(311);
		/// <summary>
		/// FIX tag 312, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSymbolSfx => GetText(312);
		/// <summary>
		/// FIX tag 309, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityID => GetText(309);
		/// <summary>
		/// FIX tag 305, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityIDSource => GetText(305);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingSecurityAltID, FIX tag 457; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
		/// <summary>
		/// FIX tag 462, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingProduct => GetNumber(462);
		/// <summary>
		/// FIX tag 463, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCFICode => GetText(463);
		/// <summary>
		/// FIX tag 310, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityType => GetText(310);
		/// <summary>
		/// FIX tag 763, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecuritySubType => GetText(763);
		/// <summary>
		/// FIX tag 313, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityMonthYear => GetText(313);
		/// <summary>
		/// FIX tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingMaturityDate => GetText(542);
		/// <summary>
		/// FIX tag 315, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPutOrCall => GetNumber(315);
		/// <summary>
		/// FIX tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCouponPaymentDate => GetText(241);
		/// <summary>
		/// FIX tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssueDate => GetText(242);
		/// <summary>
		/// FIX tag 243, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRepoCollateralSecurityType => GetText(243);
		/// <summary>
		/// FIX tag 244, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
		/// <summary>
		/// FIX tag 245, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
		/// <summary>
		/// FIX tag 246, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingFactor => GetNumber(246);
		/// <summary>
		/// FIX tag 256, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCreditRating => GetText(256);
		/// <summary>
		/// FIX tag 595, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingInstrRegistry => GetText(595);
		/// <summary>
		/// FIX tag 592, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCountryOfIssue => GetText(592);
		/// <summary>
		/// FIX tag 593, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
		/// <summary>
		/// FIX tag 594, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingLocaleOfIssue => GetText(594);
		/// <summary>
		/// FIX tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingRedemptionDate => GetText(247);
		/// <summary>
		/// FIX tag 316, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStrikePrice => GetNumber(316);
		/// <summary>
		/// FIX tag 941, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingStrikeCurrency => GetText(941);
		/// <summary>
		/// FIX tag 317, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingOptAttribute => GetText(317);
		/// <summary>
		/// FIX tag 436, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
		/// <summary>
		/// FIX tag 435, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCouponRate => GetNumber(435);
		/// <summary>
		/// FIX tag 308, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityExchange => GetText(308);
		/// <summary>
		/// FIX tag 306, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingIssuer => GetText(306);
		/// <summary>
		/// FIX tag 362, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
		/// <summary>
		/// FIX tag 363, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingIssuer => GetText(363);
		/// <summary>
		/// FIX tag 307, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingSecurityDesc => GetText(307);
		/// <summary>
		/// FIX tag 364, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
		/// <summary>
		/// FIX tag 365, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedUnderlyingSecurityDesc => GetText(365);
		/// <summary>
		/// FIX tag 877, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPProgram => GetText(877);
		/// <summary>
		/// FIX tag 878, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCPRegType => GetText(878);
		/// <summary>
		/// FIX tag 318, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? UnderlyingCurrency => GetText(318);
		/// <summary>
		/// FIX tag 879, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingQty => GetNumber(879);
		/// <summary>
		/// FIX tag 810, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingPx => GetNumber(810);
		/// <summary>
		/// FIX tag 882, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
		/// <summary>
		/// FIX tag 883, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndPrice => GetNumber(883);
		/// <summary>
		/// FIX tag 884, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingStartValue => GetNumber(884);
		/// <summary>
		/// FIX tag 885, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingCurrentValue => GetNumber(885);
		/// <summary>
		/// FIX tag 886, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingEndValue => GetNumber(886);
		/// <summary>
		/// Entries of the group counted by NoUnderlyingStips, FIX tag 887; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
	}

	/// <summary>FIX 4.4 QuoteRequestReject, MsgType AG.</summary>
	public sealed class QuoteRequestReject : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteRequestReject"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteRequestReject;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteRequestReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AG", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 644, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RFQReqID => GetText(644);
		/// <summary>
		/// FIX tag 658, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteRequestRejectReason => GetNumber(658);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotReqRjctGrp => GetGroup(146);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 RFQRequest, MsgType AH.</summary>
	public sealed class RFQRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateRFQRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateRFQRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal RFQRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AH", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 644, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RFQReqID => GetText(644);
		/// <summary>
		/// Entries of the group counted by NoRelatedSym, FIX tag 146; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> RFQReqGrp => GetGroup(146);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
	}

	/// <summary>FIX 4.4 QuoteStatusReport, MsgType AI.</summary>
	public sealed class QuoteStatusReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteStatusReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteStatusReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteStatusReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AI", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 649, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteStatusReqID => GetText(649);
		/// <summary>
		/// FIX tag 131, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteReqID => GetText(131);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 693, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteRespID => GetText(693);
		/// <summary>
		/// FIX tag 537, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteType => GetNumber(537);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LegQuotStatGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoQuoteQualifiers, FIX tag 735; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 132, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidPx => GetNumber(132);
		/// <summary>
		/// FIX tag 133, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferPx => GetNumber(133);
		/// <summary>
		/// FIX tag 645, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktBidPx => GetNumber(645);
		/// <summary>
		/// FIX tag 646, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktOfferPx => GetNumber(646);
		/// <summary>
		/// FIX tag 647, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinBidSize => GetNumber(647);
		/// <summary>
		/// FIX tag 134, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSize => GetNumber(134);
		/// <summary>
		/// FIX tag 648, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinOfferSize => GetNumber(648);
		/// <summary>
		/// FIX tag 135, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSize => GetNumber(135);
		/// <summary>
		/// FIX tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ValidUntilTime => GetText(62);
		/// <summary>
		/// FIX tag 188, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSpotRate => GetNumber(188);
		/// <summary>
		/// FIX tag 190, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSpotRate => GetNumber(190);
		/// <summary>
		/// FIX tag 189, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints => GetNumber(189);
		/// <summary>
		/// FIX tag 191, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints => GetNumber(191);
		/// <summary>
		/// FIX tag 631, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidPx => GetNumber(631);
		/// <summary>
		/// FIX tag 632, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidYield => GetNumber(632);
		/// <summary>
		/// FIX tag 633, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidYield => GetNumber(633);
		/// <summary>
		/// FIX tag 634, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferYield => GetNumber(634);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 642, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints2 => GetNumber(642);
		/// <summary>
		/// FIX tag 643, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints2 => GetNumber(643);
		/// <summary>
		/// FIX tag 656, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrBidFxRate => GetNumber(656);
		/// <summary>
		/// FIX tag 657, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
		/// <summary>
		/// FIX tag 156, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrFxRateCalc => GetText(156);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// FIX tag 297, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteStatus => GetNumber(297);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 QuoteResponse, MsgType AJ.</summary>
	public sealed class QuoteResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateQuoteResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateQuoteResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal QuoteResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AJ", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 693, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteRespID => GetText(693);
		/// <summary>
		/// FIX tag 117, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? QuoteID => GetText(117);
		/// <summary>
		/// FIX tag 694, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteRespType => GetNumber(694);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 23, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IOIID => GetText(23);
		/// <summary>
		/// FIX tag 537, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QuoteType => GetNumber(537);
		/// <summary>
		/// Entries of the group counted by NoQuoteQualifiers, FIX tag 735; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 38, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty => GetNumber(38);
		/// <summary>
		/// FIX tag 152, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOrderQty => GetNumber(152);
		/// <summary>
		/// FIX tag 516, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderPercent => GetNumber(516);
		/// <summary>
		/// FIX tag 468, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RoundingDirection => GetText(468);
		/// <summary>
		/// FIX tag 469, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RoundingModulus => GetNumber(469);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate2 => GetText(193);
		/// <summary>
		/// FIX tag 192, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OrderQty2 => GetNumber(192);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> LegQuotGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 132, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidPx => GetNumber(132);
		/// <summary>
		/// FIX tag 133, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferPx => GetNumber(133);
		/// <summary>
		/// FIX tag 645, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktBidPx => GetNumber(645);
		/// <summary>
		/// FIX tag 646, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MktOfferPx => GetNumber(646);
		/// <summary>
		/// FIX tag 647, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinBidSize => GetNumber(647);
		/// <summary>
		/// FIX tag 134, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSize => GetNumber(134);
		/// <summary>
		/// FIX tag 648, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MinOfferSize => GetNumber(648);
		/// <summary>
		/// FIX tag 135, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSize => GetNumber(135);
		/// <summary>
		/// FIX tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ValidUntilTime => GetText(62);
		/// <summary>
		/// FIX tag 188, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidSpotRate => GetNumber(188);
		/// <summary>
		/// FIX tag 190, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferSpotRate => GetNumber(190);
		/// <summary>
		/// FIX tag 189, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints => GetNumber(189);
		/// <summary>
		/// FIX tag 191, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints => GetNumber(191);
		/// <summary>
		/// FIX tag 631, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidPx => GetNumber(631);
		/// <summary>
		/// FIX tag 632, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidYield => GetNumber(632);
		/// <summary>
		/// FIX tag 633, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MidYield => GetNumber(633);
		/// <summary>
		/// FIX tag 634, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferYield => GetNumber(634);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 40, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrdType => GetText(40);
		/// <summary>
		/// FIX tag 642, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BidForwardPoints2 => GetNumber(642);
		/// <summary>
		/// FIX tag 643, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OfferForwardPoints2 => GetNumber(643);
		/// <summary>
		/// FIX tag 656, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrBidFxRate => GetNumber(656);
		/// <summary>
		/// FIX tag 657, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
		/// <summary>
		/// FIX tag 156, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrFxRateCalc => GetText(156);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 100, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? ExDestination => GetText(100);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	}

	/// <summary>FIX 4.4 Confirmation, MsgType AK.</summary>
	public sealed class Confirmation : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateConfirmation"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateConfirmation;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal Confirmation(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AK", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 664, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ConfirmID => GetText(664);
		/// <summary>
		/// FIX tag 772, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ConfirmRefID => GetText(772);
		/// <summary>
		/// FIX tag 859, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ConfirmReqID => GetText(859);
		/// <summary>
		/// FIX tag 666, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ConfirmTransType => GetNumber(666);
		/// <summary>
		/// FIX tag 773, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ConfirmType => GetNumber(773);
		/// <summary>
		/// FIX tag 797, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? CopyMsgIndicator => GetText(797);
		/// <summary>
		/// FIX tag 650, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LegalConfirm => GetText(650);
		/// <summary>
		/// FIX tag 665, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ConfirmStatus => GetNumber(665);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 467, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IndividualAllocID => GetText(467);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 80, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocQty => GetNumber(80);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// Entries of the group counted by NoCapacities, FIX tag 862; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> CpctyConfGrp => GetGroup(862);
		/// <summary>
		/// FIX tag 79, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocAccount => GetText(79);
		/// <summary>
		/// FIX tag 661, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocAcctIDSource => GetNumber(661);
		/// <summary>
		/// FIX tag 798, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocAccountType => GetNumber(798);
		/// <summary>
		/// FIX tag 6, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPx => GetNumber(6);
		/// <summary>
		/// FIX tag 74, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPxPrecision => GetNumber(74);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 860, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgParPx => GetNumber(860);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 861, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ReportedPx => GetNumber(861);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 81, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ProcessCode => GetText(81);
		/// <summary>
		/// FIX tag 381, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GrossTradeAmt => GetNumber(381);
		/// <summary>
		/// FIX tag 157, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumDaysInterest => GetNumber(157);
		/// <summary>
		/// FIX tag 230, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExDate => GetText(230);
		/// <summary>
		/// FIX tag 158, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestRate => GetNumber(158);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 738, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? InterestAtMaturity => GetNumber(738);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 238, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Concession => GetNumber(238);
		/// <summary>
		/// FIX tag 237, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalTakedown => GetNumber(237);
		/// <summary>
		/// FIX tag 118, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetMoney => GetNumber(118);
		/// <summary>
		/// FIX tag 890, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MaturityNetMoney => GetNumber(890);
		/// <summary>
		/// FIX tag 119, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrAmt => GetNumber(119);
		/// <summary>
		/// FIX tag 120, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrency => GetText(120);
		/// <summary>
		/// FIX tag 155, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlCurrFxRate => GetNumber(155);
		/// <summary>
		/// FIX tag 156, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlCurrFxRateCalc => GetText(156);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 172, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlDeliveryType => GetNumber(172);
		/// <summary>
		/// FIX tag 169, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StandInstDbType => GetNumber(169);
		/// <summary>
		/// FIX tag 170, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbName => GetText(170);
		/// <summary>
		/// FIX tag 171, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbID => GetText(171);
		/// <summary>
		/// Entries of the group counted by NoDlvyInst, FIX tag 85; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
		/// <summary>
		/// FIX tag 12, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Commission => GetNumber(12);
		/// <summary>
		/// FIX tag 13, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? CommType => GetText(13);
		/// <summary>
		/// FIX tag 479, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? CommCurrency => GetText(479);
		/// <summary>
		/// FIX tag 497, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? FundRenewWaiv => GetText(497);
		/// <summary>
		/// FIX tag 858, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SharedCommission => GetNumber(858);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	}

	/// <summary>FIX 4.4 PositionMaintenanceRequest, MsgType AL.</summary>
	public sealed class PositionMaintenanceRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidatePositionMaintenanceRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidatePositionMaintenanceRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal PositionMaintenanceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AL", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 710, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosReqID => GetText(710);
		/// <summary>
		/// FIX tag 709, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosTransType => GetNumber(709);
		/// <summary>
		/// FIX tag 712, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosMaintAction => GetNumber(712);
		/// <summary>
		/// FIX tag 713, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigPosReqRefID => GetText(713);
		/// <summary>
		/// FIX tag 714, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosMaintRptRefID => GetText(714);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoPositions, FIX tag 702; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
		/// <summary>
		/// FIX tag 718, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AdjustmentType => GetNumber(718);
		/// <summary>
		/// FIX tag 719, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ContraryInstructionIndicator => GetText(719);
		/// <summary>
		/// FIX tag 720, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? PriorSpreadIndicator => GetText(720);
		/// <summary>
		/// FIX tag 834, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ThresholdAmount => GetNumber(834);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 PositionMaintenanceReport, MsgType AM.</summary>
	public sealed class PositionMaintenanceReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidatePositionMaintenanceReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidatePositionMaintenanceReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal PositionMaintenanceReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AM", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 721, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosMaintRptID => GetText(721);
		/// <summary>
		/// FIX tag 709, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosTransType => GetNumber(709);
		/// <summary>
		/// FIX tag 710, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosReqID => GetText(710);
		/// <summary>
		/// FIX tag 712, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosMaintAction => GetNumber(712);
		/// <summary>
		/// FIX tag 713, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrigPosReqRefID => GetText(713);
		/// <summary>
		/// FIX tag 722, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosMaintStatus => GetNumber(722);
		/// <summary>
		/// FIX tag 723, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosMaintResult => GetNumber(723);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoPositions, FIX tag 702; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
		/// <summary>
		/// Entries of the group counted by NoPosAmt, FIX tag 753; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
		/// <summary>
		/// FIX tag 718, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AdjustmentType => GetNumber(718);
		/// <summary>
		/// FIX tag 834, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ThresholdAmount => GetNumber(834);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 RequestForPositions, MsgType AN.</summary>
	public sealed class RequestForPositions : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateRequestForPositions"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateRequestForPositions;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal RequestForPositions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AN", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 710, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosReqID => GetText(710);
		/// <summary>
		/// FIX tag 724, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosReqType => GetNumber(724);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// Entries of the group counted by NoTradingSessions, FIX tag 386; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 RequestForPositionsAck, MsgType AO.</summary>
	public sealed class RequestForPositionsAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateRequestForPositionsAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateRequestForPositionsAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal RequestForPositionsAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AO", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 721, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosMaintRptID => GetText(721);
		/// <summary>
		/// FIX tag 710, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosReqID => GetText(710);
		/// <summary>
		/// FIX tag 727, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNumPosReports => GetNumber(727);
		/// <summary>
		/// FIX tag 325, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? UnsolicitedIndicator => GetText(325);
		/// <summary>
		/// FIX tag 728, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosReqResult => GetNumber(728);
		/// <summary>
		/// FIX tag 729, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosReqStatus => GetNumber(729);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 PositionReport, MsgType AP.</summary>
	public sealed class PositionReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidatePositionReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidatePositionReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal PositionReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AP", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 721, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosMaintRptID => GetText(721);
		/// <summary>
		/// FIX tag 710, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? PosReqID => GetText(710);
		/// <summary>
		/// FIX tag 724, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosReqType => GetNumber(724);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 727, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNumPosReports => GetNumber(727);
		/// <summary>
		/// FIX tag 325, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? UnsolicitedIndicator => GetText(325);
		/// <summary>
		/// FIX tag 728, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PosReqResult => GetNumber(728);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 730, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlPrice => GetNumber(730);
		/// <summary>
		/// FIX tag 731, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlPriceType => GetNumber(731);
		/// <summary>
		/// FIX tag 734, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriorSettlPrice => GetNumber(734);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PosUndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoPositions, FIX tag 702; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
		/// <summary>
		/// Entries of the group counted by NoPosAmt, FIX tag 753; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
		/// <summary>
		/// FIX tag 506, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? RegistStatus => GetText(506);
		/// <summary>
		/// FIX tag 743, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DeliveryDate => GetText(743);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 TradeCaptureReportRequestAck, MsgType AQ.</summary>
	public sealed class TradeCaptureReportRequestAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradeCaptureReportRequestAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradeCaptureReportRequestAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradeCaptureReportRequestAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AQ", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 568, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeRequestID => GetText(568);
		/// <summary>
		/// FIX tag 569, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeRequestType => GetNumber(569);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 748, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumTradeReports => GetNumber(748);
		/// <summary>
		/// FIX tag 749, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeRequestResult => GetNumber(749);
		/// <summary>
		/// FIX tag 750, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeRequestStatus => GetNumber(750);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 442, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MultiLegReportingType => GetText(442);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 TradeCaptureReportAck, MsgType AR.</summary>
	public sealed class TradeCaptureReportAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateTradeCaptureReportAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateTradeCaptureReportAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal TradeCaptureReportAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AR", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 571, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeReportID => GetText(571);
		/// <summary>
		/// FIX tag 487, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeReportTransType => GetNumber(487);
		/// <summary>
		/// FIX tag 856, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeReportType => GetNumber(856);
		/// <summary>
		/// FIX tag 828, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdType => GetNumber(828);
		/// <summary>
		/// FIX tag 829, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdSubType => GetNumber(829);
		/// <summary>
		/// FIX tag 855, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SecondaryTrdType => GetNumber(855);
		/// <summary>
		/// FIX tag 830, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TransferReason => GetText(830);
		/// <summary>
		/// FIX tag 150, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExecType => GetText(150);
		/// <summary>
		/// FIX tag 572, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeReportRefID => GetText(572);
		/// <summary>
		/// FIX tag 881, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryTradeReportRefID => GetText(881);
		/// <summary>
		/// FIX tag 939, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TrdRptStatus => GetNumber(939);
		/// <summary>
		/// FIX tag 751, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TradeReportRejectReason => GetNumber(751);
		/// <summary>
		/// FIX tag 818, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryTradeReportID => GetText(818);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 820, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradeLinkID => GetText(820);
		/// <summary>
		/// FIX tag 880, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TrdMatchID => GetText(880);
		/// <summary>
		/// FIX tag 17, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ExecID => GetText(17);
		/// <summary>
		/// FIX tag 527, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryExecID => GetText(527);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdInstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 635, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClearingFeeIndicator => GetText(635);
		/// <summary>
		/// FIX tag 528, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OrderCapacity => GetText(528);
		/// <summary>
		/// FIX tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.
		/// </summary>
		public string? OrderRestrictions => GetText(529);
		/// <summary>
		/// FIX tag 582, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CustOrderCapacity => GetNumber(582);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 660, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AcctIDSource => GetNumber(660);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 591, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PreallocMethod => GetText(591);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdAllocGrp => GetGroup(78);
	}

	/// <summary>FIX 4.4 AllocationReport, MsgType AS.</summary>
	public sealed class AllocationReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAllocationReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAllocationReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal AllocationReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AS", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 755, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocReportID => GetText(755);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// FIX tag 71, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? AllocTransType => GetText(71);
		/// <summary>
		/// FIX tag 795, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocReportRefID => GetText(795);
		/// <summary>
		/// FIX tag 796, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocCancReplaceReason => GetNumber(796);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 794, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocReportType => GetNumber(794);
		/// <summary>
		/// FIX tag 87, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocStatus => GetNumber(87);
		/// <summary>
		/// FIX tag 88, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocRejCode => GetNumber(88);
		/// <summary>
		/// FIX tag 72, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RefAllocID => GetText(72);
		/// <summary>
		/// FIX tag 808, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocIntermedReqType => GetNumber(808);
		/// <summary>
		/// FIX tag 196, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocLinkID => GetText(196);
		/// <summary>
		/// FIX tag 197, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocLinkType => GetNumber(197);
		/// <summary>
		/// FIX tag 466, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BookingRefID => GetText(466);
		/// <summary>
		/// FIX tag 857, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocNoOrdersType => GetNumber(857);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecAllocGrp => GetGroup(124);
		/// <summary>
		/// FIX tag 570, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? PreviouslyReported => GetText(570);
		/// <summary>
		/// FIX tag 700, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? ReversalIndicator => GetText(700);
		/// <summary>
		/// FIX tag 574, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? MatchType => GetText(574);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 668, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryForm => GetNumber(668);
		/// <summary>
		/// FIX tag 869, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PctAtRisk => GetNumber(869);
		/// <summary>
		/// Entries of the group counted by NoInstrAttrib, FIX tag 870; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 30, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? LastMkt => GetText(30);
		/// <summary>
		/// FIX tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeOriginationDate => GetText(229);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 6, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPx => GetNumber(6);
		/// <summary>
		/// FIX tag 860, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgParPx => GetNumber(860);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// FIX tag 74, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AvgPxPrecision => GetNumber(74);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 63, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SettlType => GetText(63);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 775, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BookingType => GetNumber(775);
		/// <summary>
		/// FIX tag 381, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? GrossTradeAmt => GetNumber(381);
		/// <summary>
		/// FIX tag 238, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Concession => GetNumber(238);
		/// <summary>
		/// FIX tag 237, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalTakedown => GetNumber(237);
		/// <summary>
		/// FIX tag 118, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetMoney => GetNumber(118);
		/// <summary>
		/// FIX tag 77, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? PositionEffect => GetText(77);
		/// <summary>
		/// FIX tag 754, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? AutoAcceptIndicator => GetText(754);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// FIX tag 157, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NumDaysInterest => GetNumber(157);
		/// <summary>
		/// FIX tag 158, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestRate => GetNumber(158);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 540, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalAccruedInterestAmt => GetNumber(540);
		/// <summary>
		/// FIX tag 738, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? InterestAtMaturity => GetNumber(738);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 650, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LegalConfirm => GetText(650);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 235, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? YieldType => GetText(235);
		/// <summary>
		/// FIX tag 236, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Yield => GetNumber(236);
		/// <summary>
		/// FIX tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldCalcDate => GetText(701);
		/// <summary>
		/// FIX tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? YieldRedemptionDate => GetText(696);
		/// <summary>
		/// FIX tag 697, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPrice => GetNumber(697);
		/// <summary>
		/// FIX tag 698, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? YieldRedemptionPriceType => GetNumber(698);
		/// <summary>
		/// FIX tag 892, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNoAllocs => GetNumber(892);
		/// <summary>
		/// FIX tag 893, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastFragment => GetText(893);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AllocGrp => GetGroup(78);
	}

	/// <summary>FIX 4.4 AllocationReportAck, MsgType AT.</summary>
	public sealed class AllocationReportAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAllocationReportAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAllocationReportAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal AllocationReportAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AT", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 755, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocReportID => GetText(755);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 87, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocStatus => GetNumber(87);
		/// <summary>
		/// FIX tag 88, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocRejCode => GetNumber(88);
		/// <summary>
		/// FIX tag 794, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocReportType => GetNumber(794);
		/// <summary>
		/// FIX tag 808, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocIntermedReqType => GetNumber(808);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
		/// <summary>
		/// Entries of the group counted by NoAllocs, FIX tag 78; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> AllocAckGrp => GetGroup(78);
	}

	/// <summary>FIX 4.4 ConfirmationAck, MsgType AU.</summary>
	public sealed class ConfirmationAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateConfirmationAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateConfirmationAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ConfirmationAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AU", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 664, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ConfirmID => GetText(664);
		/// <summary>
		/// FIX tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? TradeDate => GetText(75);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 940, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AffirmStatus => GetNumber(940);
		/// <summary>
		/// FIX tag 774, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ConfirmRejReason => GetNumber(774);
		/// <summary>
		/// FIX tag 573, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? MatchStatus => GetText(573);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 SettlementInstructionRequest, MsgType AV.</summary>
	public sealed class SettlementInstructionRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateSettlementInstructionRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateSettlementInstructionRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal SettlementInstructionRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AV", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 791, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlInstReqID => GetText(791);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 79, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocAccount => GetText(79);
		/// <summary>
		/// FIX tag 661, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocAcctIDSource => GetNumber(661);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? EffectiveTime => GetText(168);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// FIX tag 779, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? LastUpdateTime => GetText(779);
		/// <summary>
		/// FIX tag 169, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StandInstDbType => GetNumber(169);
		/// <summary>
		/// FIX tag 170, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbName => GetText(170);
		/// <summary>
		/// FIX tag 171, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbID => GetText(171);
	}

	/// <summary>FIX 4.4 AssignmentReport, MsgType AW.</summary>
	public sealed class AssignmentReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateAssignmentReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateAssignmentReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal AssignmentReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AW", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 833, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AsgnRptID => GetText(833);
		/// <summary>
		/// FIX tag 832, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumAssignmentReports => GetNumber(832);
		/// <summary>
		/// FIX tag 912, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastRptRequested => GetText(912);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// Entries of the group counted by NoPositions, FIX tag 702; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
		/// <summary>
		/// Entries of the group counted by NoPosAmt, FIX tag 753; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
		/// <summary>
		/// FIX tag 834, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ThresholdAmount => GetNumber(834);
		/// <summary>
		/// FIX tag 730, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlPrice => GetNumber(730);
		/// <summary>
		/// FIX tag 731, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlPriceType => GetNumber(731);
		/// <summary>
		/// FIX tag 732, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UnderlyingSettlPrice => GetNumber(732);
		/// <summary>
		/// FIX tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ExpireDate => GetText(432);
		/// <summary>
		/// FIX tag 744, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? AssignmentMethod => GetText(744);
		/// <summary>
		/// FIX tag 745, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AssignmentUnit => GetNumber(745);
		/// <summary>
		/// FIX tag 746, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? OpenInterest => GetNumber(746);
		/// <summary>
		/// FIX tag 747, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? ExerciseMethod => GetText(747);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 CollateralRequest, MsgType AX.</summary>
	public sealed class CollateralRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AX", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 894, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollReqID => GetText(894);
		/// <summary>
		/// FIX tag 895, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnReason => GetNumber(895);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 899, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginExcess => GetNumber(899);
		/// <summary>
		/// FIX tag 900, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNetValue => GetNumber(900);
		/// <summary>
		/// FIX tag 901, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOutstanding => GetNumber(901);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 CollateralAssignment, MsgType AY.</summary>
	public sealed class CollateralAssignment : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralAssignment"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralAssignment;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralAssignment(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AY", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 902, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollAsgnID => GetText(902);
		/// <summary>
		/// FIX tag 894, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollReqID => GetText(894);
		/// <summary>
		/// FIX tag 895, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnReason => GetNumber(895);
		/// <summary>
		/// FIX tag 903, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnTransType => GetNumber(903);
		/// <summary>
		/// FIX tag 907, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollAsgnRefID => GetText(907);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? ExpireTime => GetText(126);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 899, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginExcess => GetNumber(899);
		/// <summary>
		/// FIX tag 900, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNetValue => GetNumber(900);
		/// <summary>
		/// FIX tag 901, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOutstanding => GetNumber(901);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 172, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlDeliveryType => GetNumber(172);
		/// <summary>
		/// FIX tag 169, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StandInstDbType => GetNumber(169);
		/// <summary>
		/// FIX tag 170, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbName => GetText(170);
		/// <summary>
		/// FIX tag 171, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbID => GetText(171);
		/// <summary>
		/// Entries of the group counted by NoDlvyInst, FIX tag 85; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 CollateralResponse, MsgType AZ.</summary>
	public sealed class CollateralResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "AZ", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 904, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollRespID => GetText(904);
		/// <summary>
		/// FIX tag 902, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollAsgnID => GetText(902);
		/// <summary>
		/// FIX tag 894, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollReqID => GetText(894);
		/// <summary>
		/// FIX tag 895, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnReason => GetNumber(895);
		/// <summary>
		/// FIX tag 903, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnTransType => GetNumber(903);
		/// <summary>
		/// FIX tag 905, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnRespType => GetNumber(905);
		/// <summary>
		/// FIX tag 906, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollAsgnRejectReason => GetNumber(906);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 899, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginExcess => GetNumber(899);
		/// <summary>
		/// FIX tag 900, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNetValue => GetNumber(900);
		/// <summary>
		/// FIX tag 901, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOutstanding => GetNumber(901);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 CollateralReport, MsgType BA.</summary>
	public sealed class CollateralReport : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralReport"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralReport;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BA", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 908, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollRptID => GetText(908);
		/// <summary>
		/// FIX tag 909, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollInquiryID => GetText(909);
		/// <summary>
		/// FIX tag 910, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollStatus => GetNumber(910);
		/// <summary>
		/// FIX tag 911, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumReports => GetNumber(911);
		/// <summary>
		/// FIX tag 912, wire type <c>Boolean</c>; null when the field is absent.
		/// </summary>
		public string? LastRptRequested => GetText(912);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 899, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginExcess => GetNumber(899);
		/// <summary>
		/// FIX tag 900, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNetValue => GetNumber(900);
		/// <summary>
		/// FIX tag 901, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOutstanding => GetNumber(901);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// Entries of the group counted by NoMiscFees, FIX tag 136; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 172, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlDeliveryType => GetNumber(172);
		/// <summary>
		/// FIX tag 169, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StandInstDbType => GetNumber(169);
		/// <summary>
		/// FIX tag 170, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbName => GetText(170);
		/// <summary>
		/// FIX tag 171, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbID => GetText(171);
		/// <summary>
		/// Entries of the group counted by NoDlvyInst, FIX tag 85; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 CollateralInquiry, MsgType BB.</summary>
	public sealed class CollateralInquiry : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralInquiry"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralInquiry;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralInquiry(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BB", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 909, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollInquiryID => GetText(909);
		/// <summary>
		/// Entries of the group counted by NoCollInquiryQualifier, FIX tag 938; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> CollInqQualGrp => GetGroup(938);
		/// <summary>
		/// FIX tag 263, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? SubscriptionRequestType => GetText(263);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 899, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginExcess => GetNumber(899);
		/// <summary>
		/// FIX tag 900, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotalNetValue => GetNumber(900);
		/// <summary>
		/// FIX tag 901, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CashOutstanding => GetNumber(901);
		/// <summary>
		/// Entries of the group counted by NoTrdRegTimestamps, FIX tag 768; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
		/// <summary>
		/// FIX tag 54, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? Side => GetText(54);
		/// <summary>
		/// FIX tag 44, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Price => GetNumber(44);
		/// <summary>
		/// FIX tag 423, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PriceType => GetNumber(423);
		/// <summary>
		/// FIX tag 159, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccruedInterestAmt => GetNumber(159);
		/// <summary>
		/// FIX tag 920, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndAccruedInterestAmt => GetNumber(920);
		/// <summary>
		/// FIX tag 921, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StartCash => GetNumber(921);
		/// <summary>
		/// FIX tag 922, wire type <c>Amt</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EndCash => GetNumber(922);
		/// <summary>
		/// FIX tag 218, wire type <c>PriceOffset</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Spread => GetNumber(218);
		/// <summary>
		/// FIX tag 220, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveCurrency => GetText(220);
		/// <summary>
		/// FIX tag 221, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurveName => GetText(221);
		/// <summary>
		/// FIX tag 222, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkCurvePoint => GetText(222);
		/// <summary>
		/// FIX tag 662, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPrice => GetNumber(662);
		/// <summary>
		/// FIX tag 663, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? BenchmarkPriceType => GetNumber(663);
		/// <summary>
		/// FIX tag 699, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityID => GetText(699);
		/// <summary>
		/// FIX tag 761, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? BenchmarkSecurityIDSource => GetText(761);
		/// <summary>
		/// Entries of the group counted by NoStipulations, FIX tag 232; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
		/// <summary>
		/// FIX tag 172, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? SettlDeliveryType => GetNumber(172);
		/// <summary>
		/// FIX tag 169, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StandInstDbType => GetNumber(169);
		/// <summary>
		/// FIX tag 170, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbName => GetText(170);
		/// <summary>
		/// FIX tag 171, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StandInstDbID => GetText(171);
		/// <summary>
		/// Entries of the group counted by NoDlvyInst, FIX tag 85; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 NetworkCounterpartySystemStatusRequest, MsgType BC.</summary>
	public sealed class NetworkCounterpartySystemStatusRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNetworkCounterpartySystemStatusRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNetworkCounterpartySystemStatusRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NetworkCounterpartySystemStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BC", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 935, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetworkRequestType => GetNumber(935);
		/// <summary>
		/// FIX tag 933, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? NetworkRequestID => GetText(933);
		/// <summary>
		/// Entries of the group counted by NoCompIDs, FIX tag 936; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> CompIDReqGrp => GetGroup(936);
	}

	/// <summary>FIX 4.4 NetworkCounterpartySystemStatusResponse, MsgType BD.</summary>
	public sealed class NetworkCounterpartySystemStatusResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateNetworkCounterpartySystemStatusResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateNetworkCounterpartySystemStatusResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal NetworkCounterpartySystemStatusResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BD", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 937, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? NetworkStatusResponseType => GetNumber(937);
		/// <summary>
		/// FIX tag 933, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? NetworkRequestID => GetText(933);
		/// <summary>
		/// FIX tag 932, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? NetworkResponseID => GetText(932);
		/// <summary>
		/// FIX tag 934, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LastNetworkResponseID => GetText(934);
		/// <summary>
		/// Entries of the group counted by NoCompIDs, FIX tag 936; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> CompIDStatGrp => GetGroup(936);
	}

	/// <summary>FIX 4.4 UserRequest, MsgType BE.</summary>
	public sealed class UserRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateUserRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateUserRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal UserRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BE", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 923, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UserRequestID => GetText(923);
		/// <summary>
		/// FIX tag 924, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UserRequestType => GetNumber(924);
		/// <summary>
		/// FIX tag 553, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Username => GetText(553);
		/// <summary>
		/// FIX tag 554, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Password => GetText(554);
		/// <summary>
		/// FIX tag 925, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? NewPassword => GetText(925);
		/// <summary>
		/// FIX tag 95, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RawDataLength => GetNumber(95);
		/// <summary>
		/// FIX tag 96, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? RawData => GetText(96);
	}

	/// <summary>FIX 4.4 UserResponse, MsgType BF.</summary>
	public sealed class UserResponse : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateUserResponse"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateUserResponse;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal UserResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BF", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 923, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UserRequestID => GetText(923);
		/// <summary>
		/// FIX tag 553, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Username => GetText(553);
		/// <summary>
		/// FIX tag 926, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? UserStatus => GetNumber(926);
		/// <summary>
		/// FIX tag 927, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? UserStatusText => GetText(927);
	}

	/// <summary>FIX 4.4 CollateralInquiryAck, MsgType BG.</summary>
	public sealed class CollateralInquiryAck : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateCollateralInquiryAck"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateCollateralInquiryAck;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal CollateralInquiryAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BG", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 909, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CollInquiryID => GetText(909);
		/// <summary>
		/// FIX tag 945, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollInquiryStatus => GetNumber(945);
		/// <summary>
		/// FIX tag 946, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CollInquiryResult => GetNumber(946);
		/// <summary>
		/// Entries of the group counted by NoCollInquiryQualifier, FIX tag 938; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> CollInqQualGrp => GetGroup(938);
		/// <summary>
		/// FIX tag 911, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TotNumReports => GetNumber(911);
		/// <summary>
		/// Entries of the group counted by NoPartyIDs, FIX tag 453; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
		/// <summary>
		/// FIX tag 1, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Account => GetText(1);
		/// <summary>
		/// FIX tag 581, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AccountType => GetNumber(581);
		/// <summary>
		/// FIX tag 11, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ClOrdID => GetText(11);
		/// <summary>
		/// FIX tag 37, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? OrderID => GetText(37);
		/// <summary>
		/// FIX tag 198, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryOrderID => GetText(198);
		/// <summary>
		/// FIX tag 526, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryClOrdID => GetText(526);
		/// <summary>
		/// Entries of the group counted by NoExecs, FIX tag 124; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
		/// <summary>
		/// Entries of the group counted by NoTrades, FIX tag 897; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
		/// <summary>
		/// FIX tag 55, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Symbol => GetText(55);
		/// <summary>
		/// FIX tag 65, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SymbolSfx => GetText(65);
		/// <summary>
		/// FIX tag 48, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityID => GetText(48);
		/// <summary>
		/// FIX tag 22, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityIDSource => GetText(22);
		/// <summary>
		/// Entries of the group counted by NoSecurityAltID, FIX tag 454; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
		/// <summary>
		/// FIX tag 460, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Product => GetNumber(460);
		/// <summary>
		/// FIX tag 461, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CFICode => GetText(461);
		/// <summary>
		/// FIX tag 167, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityType => GetText(167);
		/// <summary>
		/// FIX tag 762, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecuritySubType => GetText(762);
		/// <summary>
		/// FIX tag 200, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? MaturityMonthYear => GetText(200);
		/// <summary>
		/// FIX tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? MaturityDate => GetText(541);
		/// <summary>
		/// FIX tag 201, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? PutOrCall => GetNumber(201);
		/// <summary>
		/// FIX tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? CouponPaymentDate => GetText(224);
		/// <summary>
		/// FIX tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? IssueDate => GetText(225);
		/// <summary>
		/// FIX tag 239, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? RepoCollateralSecurityType => GetText(239);
		/// <summary>
		/// FIX tag 226, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseTerm => GetNumber(226);
		/// <summary>
		/// FIX tag 227, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? RepurchaseRate => GetNumber(227);
		/// <summary>
		/// FIX tag 228, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Factor => GetNumber(228);
		/// <summary>
		/// FIX tag 255, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CreditRating => GetText(255);
		/// <summary>
		/// FIX tag 543, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? InstrRegistry => GetText(543);
		/// <summary>
		/// FIX tag 470, wire type <c>Country</c>; null when the field is absent.
		/// </summary>
		public string? CountryOfIssue => GetText(470);
		/// <summary>
		/// FIX tag 471, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? StateOrProvinceOfIssue => GetText(471);
		/// <summary>
		/// FIX tag 472, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? LocaleOfIssue => GetText(472);
		/// <summary>
		/// FIX tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? RedemptionDate => GetText(240);
		/// <summary>
		/// FIX tag 202, wire type <c>Price</c>; null when the field is absent.
		/// </summary>
		public FixNumber? StrikePrice => GetNumber(202);
		/// <summary>
		/// FIX tag 947, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? StrikeCurrency => GetText(947);
		/// <summary>
		/// FIX tag 206, wire type <c>char</c>; null when the field is absent.
		/// </summary>
		public string? OptAttribute => GetText(206);
		/// <summary>
		/// FIX tag 231, wire type <c>float</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ContractMultiplier => GetNumber(231);
		/// <summary>
		/// FIX tag 223, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CouponRate => GetNumber(223);
		/// <summary>
		/// FIX tag 207, wire type <c>Exchange</c>; null when the field is absent.
		/// </summary>
		public string? SecurityExchange => GetText(207);
		/// <summary>
		/// FIX tag 106, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Issuer => GetText(106);
		/// <summary>
		/// FIX tag 348, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedIssuerLen => GetNumber(348);
		/// <summary>
		/// FIX tag 349, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedIssuer => GetText(349);
		/// <summary>
		/// FIX tag 107, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecurityDesc => GetText(107);
		/// <summary>
		/// FIX tag 350, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedSecurityDescLen => GetNumber(350);
		/// <summary>
		/// FIX tag 351, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedSecurityDesc => GetText(351);
		/// <summary>
		/// FIX tag 691, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Pool => GetText(691);
		/// <summary>
		/// FIX tag 667, wire type <c>MonthYear</c>; null when the field is absent.
		/// </summary>
		public string? ContractSettlMonth => GetText(667);
		/// <summary>
		/// FIX tag 875, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? CPProgram => GetNumber(875);
		/// <summary>
		/// FIX tag 876, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? CPRegType => GetText(876);
		/// <summary>
		/// Entries of the group counted by NoEvents, FIX tag 864; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
		/// <summary>
		/// FIX tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? DatedDate => GetText(873);
		/// <summary>
		/// FIX tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? InterestAccrualDate => GetText(874);
		/// <summary>
		/// FIX tag 913, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDesc => GetText(913);
		/// <summary>
		/// FIX tag 914, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AgreementID => GetText(914);
		/// <summary>
		/// FIX tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? AgreementDate => GetText(915);
		/// <summary>
		/// FIX tag 918, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? AgreementCurrency => GetText(918);
		/// <summary>
		/// FIX tag 788, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? TerminationType => GetNumber(788);
		/// <summary>
		/// FIX tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? StartDate => GetText(916);
		/// <summary>
		/// FIX tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? EndDate => GetText(917);
		/// <summary>
		/// FIX tag 919, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? DeliveryType => GetNumber(919);
		/// <summary>
		/// FIX tag 898, wire type <c>Percentage</c>; null when the field is absent.
		/// </summary>
		public FixNumber? MarginRatio => GetNumber(898);
		/// <summary>
		/// FIX tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? SettlDate => GetText(64);
		/// <summary>
		/// FIX tag 53, wire type <c>Qty</c>; null when the field is absent.
		/// </summary>
		public FixNumber? Quantity => GetNumber(53);
		/// <summary>
		/// FIX tag 854, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? QtyType => GetNumber(854);
		/// <summary>
		/// FIX tag 15, wire type <c>Currency</c>; null when the field is absent.
		/// </summary>
		public string? Currency => GetText(15);
		/// <summary>
		/// Entries of the group counted by NoLegs, FIX tag 555; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
		/// <summary>
		/// Entries of the group counted by NoUnderlyings, FIX tag 711; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
		/// <summary>
		/// FIX tag 336, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionID => GetText(336);
		/// <summary>
		/// FIX tag 625, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? TradingSessionSubID => GetText(625);
		/// <summary>
		/// FIX tag 716, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessID => GetText(716);
		/// <summary>
		/// FIX tag 717, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SettlSessSubID => GetText(717);
		/// <summary>
		/// FIX tag 715, wire type <c>LocalMktDate</c>; null when the field is absent.
		/// </summary>
		public string? ClearingBusinessDate => GetText(715);
		/// <summary>
		/// FIX tag 725, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ResponseTransportType => GetNumber(725);
		/// <summary>
		/// FIX tag 726, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ResponseDestination => GetText(726);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}

	/// <summary>FIX 4.4 ConfirmationRequest, MsgType BH.</summary>
	public sealed class ConfirmationRequest : FixMessage
	{
		/// <summary>The rule this type is held to, and yours to replace.</summary>
		/// <remarks>
		/// A message type and a class are one and the same thing here, so the rule lives where
		/// the type does: no table, no lookup, and the class is the key. Assign to replace it —
		/// a reference assignment, so a reader sees one rule or the other and never half of a
		/// change — and assign <see cref="FixValidator.ValidateConfirmationRequest"/> to put it back.
		/// </remarks>
		public static FixMessageRule Rule = FixValidator.ValidateConfirmationRequest;

		/// <inheritdoc/>
		private protected override FixMessageRule Checking => Rule;

		internal ConfirmationRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
			: base(source, "BH", header, body, trailer)
		{
		}

		/// <summary>
		/// FIX tag 859, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? ConfirmReqID => GetText(859);
		/// <summary>
		/// FIX tag 773, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? ConfirmType => GetNumber(773);
		/// <summary>
		/// Entries of the group counted by NoOrders, FIX tag 73; empty when the group is absent.
		/// </summary>
		public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
		/// <summary>
		/// FIX tag 70, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocID => GetText(70);
		/// <summary>
		/// FIX tag 793, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? SecondaryAllocID => GetText(793);
		/// <summary>
		/// FIX tag 467, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? IndividualAllocID => GetText(467);
		/// <summary>
		/// FIX tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.
		/// </summary>
		public string? TransactTime => GetText(60);
		/// <summary>
		/// FIX tag 79, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? AllocAccount => GetText(79);
		/// <summary>
		/// FIX tag 661, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocAcctIDSource => GetNumber(661);
		/// <summary>
		/// FIX tag 798, wire type <c>int</c>; null when the field is absent.
		/// </summary>
		public FixNumber? AllocAccountType => GetNumber(798);
		/// <summary>
		/// FIX tag 58, wire type <c>String</c>; null when the field is absent.
		/// </summary>
		public string? Text => GetText(58);
		/// <summary>
		/// FIX tag 354, wire type <c>Length</c>; null when the field is absent.
		/// </summary>
		public FixNumber? EncodedTextLen => GetNumber(354);
		/// <summary>
		/// FIX tag 355, wire type <c>data</c>; null when the field is absent.
		/// </summary>
		public string? EncodedText => GetText(355);
	}
}

/// <summary>FIX 4.4 StandardHeader field scope.</summary>
public sealed class StandardHeader : FixFieldSet
{
	internal StandardHeader(string source, FixNode[] fields)
		: base(source, fields)
	{
	}

	/// <summary>
	/// FIX tag 8, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? BeginString => GetText(8);
	/// <summary>
	/// FIX tag 9, wire type <c>Length</c>; null when the field is absent.
	/// </summary>
	public FixNumber? BodyLength => GetNumber(9);
	/// <summary>
	/// FIX tag 35, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? MsgType => GetText(35);
	/// <summary>
	/// FIX tag 49, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? SenderCompID => GetText(49);
	/// <summary>
	/// FIX tag 56, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? TargetCompID => GetText(56);
	/// <summary>
	/// FIX tag 115, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? OnBehalfOfCompID => GetText(115);
	/// <summary>
	/// FIX tag 128, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? DeliverToCompID => GetText(128);
	/// <summary>
	/// FIX tag 90, wire type <c>Length</c>; null when the field is absent.
	/// </summary>
	public FixNumber? SecureDataLen => GetNumber(90);
	/// <summary>
	/// FIX tag 91, wire type <c>data</c>; null when the field is absent.
	/// </summary>
	public string? SecureData => GetText(91);
	/// <summary>
	/// FIX tag 34, wire type <c>SeqNum</c>; null when the field is absent.
	/// </summary>
	public FixNumber? MsgSeqNum => GetNumber(34);
	/// <summary>
	/// FIX tag 50, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? SenderSubID => GetText(50);
	/// <summary>
	/// FIX tag 142, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? SenderLocationID => GetText(142);
	/// <summary>
	/// FIX tag 57, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? TargetSubID => GetText(57);
	/// <summary>
	/// FIX tag 143, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? TargetLocationID => GetText(143);
	/// <summary>
	/// FIX tag 116, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? OnBehalfOfSubID => GetText(116);
	/// <summary>
	/// FIX tag 144, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? OnBehalfOfLocationID => GetText(144);
	/// <summary>
	/// FIX tag 129, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? DeliverToSubID => GetText(129);
	/// <summary>
	/// FIX tag 145, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? DeliverToLocationID => GetText(145);
	/// <summary>
	/// FIX tag 43, wire type <c>Boolean</c>; null when the field is absent.
	/// </summary>
	public string? PossDupFlag => GetText(43);
	/// <summary>
	/// FIX tag 97, wire type <c>Boolean</c>; null when the field is absent.
	/// </summary>
	public string? PossResend => GetText(97);
	/// <summary>
	/// FIX tag 52, wire type <c>UTCTimestamp</c>; null when the field is absent.
	/// </summary>
	public string? SendingTime => GetText(52);
	/// <summary>
	/// FIX tag 122, wire type <c>UTCTimestamp</c>; null when the field is absent.
	/// </summary>
	public string? OrigSendingTime => GetText(122);
	/// <summary>
	/// FIX tag 212, wire type <c>Length</c>; null when the field is absent.
	/// </summary>
	public FixNumber? XmlDataLen => GetNumber(212);
	/// <summary>
	/// FIX tag 213, wire type <c>data</c>; null when the field is absent.
	/// </summary>
	public string? XmlData => GetText(213);
	/// <summary>
	/// FIX tag 347, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? MessageEncoding => GetText(347);
	/// <summary>
	/// FIX tag 369, wire type <c>SeqNum</c>; null when the field is absent.
	/// </summary>
	public FixNumber? LastMsgSeqNumProcessed => GetNumber(369);
	/// <summary>
	/// Entries of the group counted by NoHops, FIX tag 627; empty when the group is absent.
	/// </summary>
	public IReadOnlyList<FixFieldSet> HopGrp => GetGroup(627);
}

/// <summary>FIX 4.4 StandardTrailer field scope.</summary>
public sealed class StandardTrailer : FixFieldSet
{
	internal StandardTrailer(string source, FixNode[] fields)
		: base(source, fields)
	{
	}

	/// <summary>
	/// FIX tag 93, wire type <c>Length</c>; null when the field is absent.
	/// </summary>
	public FixNumber? SignatureLength => GetNumber(93);
	/// <summary>
	/// FIX tag 89, wire type <c>data</c>; null when the field is absent.
	/// </summary>
	public string? Signature => GetText(89);
	/// <summary>
	/// FIX tag 10, wire type <c>String</c>; null when the field is absent.
	/// </summary>
	public string? CheckSum => GetText(10);
}
