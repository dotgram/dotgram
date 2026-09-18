using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// Constructs the message class a MsgType names.
/// </summary>
static class FixMessageFactory
{
	public static FixMessage Message(string type, string source, FixNode[] header, FixNode[] body, FixNode[] trailer) => type switch
	{
		"0"  => new Heartbeat                              (source, header, body, trailer),
		"1"  => new TestRequest                            (source, header, body, trailer),
		"2"  => new ResendRequest                          (source, header, body, trailer),
		"3"  => new Reject                                 (source, header, body, trailer),
		"4"  => new SequenceReset                          (source, header, body, trailer),
		"5"  => new Logout                                 (source, header, body, trailer),
		"6"  => new IOI                                    (source, header, body, trailer),
		"7"  => new Advertisement                          (source, header, body, trailer),
		"8"  => new ExecutionReport                        (source, header, body, trailer),
		"9"  => new OrderCancelReject                      (source, header, body, trailer),
		"A"  => new Logon                                  (source, header, body, trailer),
		"B"  => new News                                   (source, header, body, trailer),
		"C"  => new Email                                  (source, header, body, trailer),
		"D"  => new NewOrderSingle                         (source, header, body, trailer),
		"E"  => new NewOrderList                           (source, header, body, trailer),
		"F"  => new OrderCancelRequest                     (source, header, body, trailer),
		"G"  => new OrderCancelReplaceRequest              (source, header, body, trailer),
		"H"  => new OrderStatusRequest                     (source, header, body, trailer),
		"J"  => new AllocationInstruction                  (source, header, body, trailer),
		"K"  => new ListCancelRequest                      (source, header, body, trailer),
		"L"  => new ListExecute                            (source, header, body, trailer),
		"M"  => new ListStatusRequest                      (source, header, body, trailer),
		"N"  => new ListStatus                             (source, header, body, trailer),
		"P"  => new AllocationInstructionAck               (source, header, body, trailer),
		"Q"  => new DontKnowTrade                          (source, header, body, trailer),
		"R"  => new QuoteRequest                           (source, header, body, trailer),
		"S"  => new Quote                                  (source, header, body, trailer),
		"T"  => new SettlementInstructions                 (source, header, body, trailer),
		"V"  => new MarketDataRequest                      (source, header, body, trailer),
		"W"  => new MarketDataSnapshotFullRefresh          (source, header, body, trailer),
		"X"  => new MarketDataIncrementalRefresh           (source, header, body, trailer),
		"Y"  => new MarketDataRequestReject                (source, header, body, trailer),
		"Z"  => new QuoteCancel                            (source, header, body, trailer),
		"a"  => new QuoteStatusRequest                     (source, header, body, trailer),
		"b"  => new MassQuoteAcknowledgement               (source, header, body, trailer),
		"c"  => new SecurityDefinitionRequest              (source, header, body, trailer),
		"d"  => new SecurityDefinition                     (source, header, body, trailer),
		"e"  => new SecurityStatusRequest                  (source, header, body, trailer),
		"f"  => new SecurityStatus                         (source, header, body, trailer),
		"g"  => new TradingSessionStatusRequest            (source, header, body, trailer),
		"h"  => new TradingSessionStatus                   (source, header, body, trailer),
		"i"  => new MassQuote                              (source, header, body, trailer),
		"j"  => new BusinessMessageReject                  (source, header, body, trailer),
		"k"  => new BidRequest                             (source, header, body, trailer),
		"l"  => new BidResponse                            (source, header, body, trailer),
		"m"  => new ListStrikePrice                        (source, header, body, trailer),
		"n"  => new XMLnonFIX                              (source, header, body, trailer),
		"o"  => new RegistrationInstructions               (source, header, body, trailer),
		"p"  => new RegistrationInstructionsResponse       (source, header, body, trailer),
		"q"  => new OrderMassCancelRequest                 (source, header, body, trailer),
		"r"  => new OrderMassCancelReport                  (source, header, body, trailer),
		"s"  => new NewOrderCross                          (source, header, body, trailer),
		"t"  => new CrossOrderCancelReplaceRequest         (source, header, body, trailer),
		"u"  => new CrossOrderCancelRequest                (source, header, body, trailer),
		"v"  => new SecurityTypeRequest                    (source, header, body, trailer),
		"w"  => new SecurityTypes                          (source, header, body, trailer),
		"x"  => new SecurityListRequest                    (source, header, body, trailer),
		"y"  => new SecurityList                           (source, header, body, trailer),
		"z"  => new DerivativeSecurityListRequest          (source, header, body, trailer),
		"AA" => new DerivativeSecurityList                 (source, header, body, trailer),
		"AB" => new NewOrderMultileg                       (source, header, body, trailer),
		"AC" => new MultilegOrderCancelReplace             (source, header, body, trailer),
		"AD" => new TradeCaptureReportRequest              (source, header, body, trailer),
		"AE" => new TradeCaptureReport                     (source, header, body, trailer),
		"AF" => new OrderMassStatusRequest                 (source, header, body, trailer),
		"AG" => new QuoteRequestReject                     (source, header, body, trailer),
		"AH" => new RFQRequest                             (source, header, body, trailer),
		"AI" => new QuoteStatusReport                      (source, header, body, trailer),
		"AJ" => new QuoteResponse                          (source, header, body, trailer),
		"AK" => new Confirmation                           (source, header, body, trailer),
		"AL" => new PositionMaintenanceRequest             (source, header, body, trailer),
		"AM" => new PositionMaintenanceReport              (source, header, body, trailer),
		"AN" => new RequestForPositions                    (source, header, body, trailer),
		"AO" => new RequestForPositionsAck                 (source, header, body, trailer),
		"AP" => new PositionReport                         (source, header, body, trailer),
		"AQ" => new TradeCaptureReportRequestAck           (source, header, body, trailer),
		"AR" => new TradeCaptureReportAck                  (source, header, body, trailer),
		"AS" => new AllocationReport                       (source, header, body, trailer),
		"AT" => new AllocationReportAck                    (source, header, body, trailer),
		"AU" => new ConfirmationAck                        (source, header, body, trailer),
		"AV" => new SettlementInstructionRequest           (source, header, body, trailer),
		"AW" => new AssignmentReport                       (source, header, body, trailer),
		"AX" => new CollateralRequest                      (source, header, body, trailer),
		"AY" => new CollateralAssignment                   (source, header, body, trailer),
		"AZ" => new CollateralResponse                     (source, header, body, trailer),
		"BA" => new CollateralReport                       (source, header, body, trailer),
		"BB" => new CollateralInquiry                      (source, header, body, trailer),
		"BC" => new NetworkCounterpartySystemStatusRequest (source, header, body, trailer),
		"BD" => new NetworkCounterpartySystemStatusResponse(source, header, body, trailer),
		"BE" => new UserRequest                            (source, header, body, trailer),
		"BF" => new UserResponse                           (source, header, body, trailer),
		"BG" => new CollateralInquiryAck                   (source, header, body, trailer),
		"BH" => new ConfirmationRequest                    (source, header, body, trailer),
		_    => new CustomFixMessage                       (source, type, header, body, trailer),
	};
}

/// <summary>FIX 4.4 Heartbeat, MsgType 0.</summary>
public sealed class Heartbeat : FixMessage
{
	internal Heartbeat(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "0", header, body, trailer)
	{
	}

	public string? TestReqID => GetText(112);
}

/// <summary>FIX 4.4 TestRequest, MsgType 1.</summary>
public sealed class TestRequest : FixMessage
{
	internal TestRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "1", header, body, trailer)
	{
	}

	public string? TestReqID => GetText(112);
}

/// <summary>FIX 4.4 ResendRequest, MsgType 2.</summary>
public sealed class ResendRequest : FixMessage
{
	internal ResendRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "2", header, body, trailer)
	{
	}

	public FixNumber? BeginSeqNo => GetNumber(7);
	public FixNumber? EndSeqNo => GetNumber(16);
}

/// <summary>FIX 4.4 Reject, MsgType 3.</summary>
public sealed class Reject : FixMessage
{
	internal Reject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "3", header, body, trailer)
	{
	}

	public FixNumber? RefSeqNum => GetNumber(45);
	public FixNumber? RefTagID => GetNumber(371);
	public string? RefMsgType => GetText(372);
	public FixNumber? SessionRejectReason => GetNumber(373);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 SequenceReset, MsgType 4.</summary>
public sealed class SequenceReset : FixMessage
{
	internal SequenceReset(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "4", header, body, trailer)
	{
	}

	public string? GapFillFlag => GetText(123);
	public FixNumber? NewSeqNo => GetNumber(36);
}

/// <summary>FIX 4.4 Logout, MsgType 5.</summary>
public sealed class Logout : FixMessage
{
	internal Logout(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "5", header, body, trailer)
	{
	}

	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 IOI, MsgType 6.</summary>
public sealed class IOI : FixMessage
{
	internal IOI(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "6", header, body, trailer)
	{
	}

	public string? IOIID => GetText(23);
	public string? IOITransType => GetText(28);
	public string? IOIRefID => GetText(26);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? IOIQty => GetText(27);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public IReadOnlyList<FixFieldSet> InstrmtLegIOIGrp => GetGroup(555);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public string? ValidUntilTime => GetText(62);
	public string? IOIQltyInd => GetText(25);
	public string? IOINaturalFlag => GetText(130);
	public IReadOnlyList<FixFieldSet> IOIQualGrp => GetGroup(199);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TransactTime => GetText(60);
	public string? URLLink => GetText(149);
	public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
}

/// <summary>FIX 4.4 Advertisement, MsgType 7.</summary>
public sealed class Advertisement : FixMessage
{
	internal Advertisement(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "7", header, body, trailer)
	{
	}

	public string? AdvId => GetText(2);
	public string? AdvTransType => GetText(5);
	public string? AdvRefID => GetText(3);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? AdvSide => GetText(4);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? Price => GetNumber(44);
	public string? Currency => GetText(15);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? URLLink => GetText(149);
	public string? LastMkt => GetText(30);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
}

/// <summary>FIX 4.4 ExecutionReport, MsgType 8.</summary>
public sealed class ExecutionReport : FixMessage
{
	internal ExecutionReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "8", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public string? SecondaryExecID => GetText(527);
	public string? ClOrdID => GetText(11);
	public string? OrigClOrdID => GetText(41);
	public string? ClOrdLinkID => GetText(583);
	public string? QuoteRespID => GetText(693);
	public string? OrdStatusReqID => GetText(790);
	public string? MassStatusReqID => GetText(584);
	public FixNumber? TotNumReports => GetNumber(911);
	public string? LastRptRequested => GetText(912);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeOriginationDate => GetText(229);
	public IReadOnlyList<FixFieldSet> ContraGrp => GetGroup(382);
	public string? ListID => GetText(66);
	public string? CrossID => GetText(548);
	public string? OrigCrossID => GetText(551);
	public FixNumber? CrossType => GetNumber(549);
	public string? ExecID => GetText(17);
	public string? ExecRefID => GetText(19);
	public string? ExecType => GetText(150);
	public string? OrdStatus => GetText(39);
	public string? WorkingIndicator => GetText(636);
	public FixNumber? OrdRejReason => GetNumber(103);
	public FixNumber? ExecRestatementReason => GetNumber(378);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? DayBookingInst => GetText(589);
	public string? BookingUnit => GetText(590);
	public string? PreallocMethod => GetText(591);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? CashMargin => GetText(544);
	public string? ClearingFeeIndicator => GetText(635);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? PeggedPrice => GetNumber(839);
	public FixNumber? DiscretionPrice => GetNumber(845);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public FixNumber? TargetStrategyPerformance => GetNumber(850);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? SolicitedFlag => GetText(377);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public string? ExecInst => GetText(18);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public FixNumber? LastQty => GetNumber(32);
	public FixNumber? UnderlyingLastQty => GetNumber(652);
	public FixNumber? LastPx => GetNumber(31);
	public FixNumber? UnderlyingLastPx => GetNumber(651);
	public FixNumber? LastParPx => GetNumber(669);
	public FixNumber? LastSpotRate => GetNumber(194);
	public FixNumber? LastForwardPoints => GetNumber(195);
	public string? LastMkt => GetText(30);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? TimeBracket => GetText(943);
	public string? LastCapacity => GetText(29);
	public FixNumber? LeavesQty => GetNumber(151);
	public FixNumber? CumQty => GetNumber(14);
	public FixNumber? AvgPx => GetNumber(6);
	public FixNumber? DayOrderQty => GetNumber(424);
	public FixNumber? DayCumQty => GetNumber(425);
	public FixNumber? DayAvgPx => GetNumber(426);
	public FixNumber? GTBookingInst => GetNumber(427);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public string? ReportToExch => GetText(113);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? GrossTradeAmt => GetNumber(381);
	public FixNumber? NumDaysInterest => GetNumber(157);
	public string? ExDate => GetText(230);
	public FixNumber? AccruedInterestRate => GetNumber(158);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? InterestAtMaturity => GetNumber(738);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public string? TradedFlatSwitch => GetText(258);
	public string? BasisFeatureDate => GetText(259);
	public FixNumber? BasisFeaturePrice => GetNumber(260);
	public FixNumber? Concession => GetNumber(238);
	public FixNumber? TotalTakedown => GetNumber(237);
	public FixNumber? NetMoney => GetNumber(118);
	public FixNumber? SettlCurrAmt => GetNumber(119);
	public string? SettlCurrency => GetText(120);
	public FixNumber? SettlCurrFxRate => GetNumber(155);
	public string? SettlCurrFxRateCalc => GetText(156);
	public string? HandlInst => GetText(21);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? PositionEffect => GetText(77);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? BookingType => GetNumber(775);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public FixNumber? LastForwardPoints2 => GetNumber(641);
	public string? MultiLegReportingType => GetText(442);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
	public string? TransBkdTime => GetText(483);
	public string? ExecValuationPoint => GetText(515);
	public string? ExecPriceType => GetText(484);
	public FixNumber? ExecPriceAdjustment => GetNumber(485);
	public FixNumber? PriorityIndicator => GetNumber(638);
	public FixNumber? PriceImprovement => GetNumber(639);
	public FixNumber? LastLiquidityInd => GetNumber(851);
	public IReadOnlyList<FixFieldSet> ContAmtGrp => GetGroup(518);
	public IReadOnlyList<FixFieldSet> InstrmtLegExecGrp => GetGroup(555);
	public string? CopyMsgIndicator => GetText(797);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
}

/// <summary>FIX 4.4 OrderCancelReject, MsgType 9.</summary>
public sealed class OrderCancelReject : FixMessage
{
	internal OrderCancelReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "9", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdID => GetText(11);
	public string? ClOrdLinkID => GetText(583);
	public string? OrigClOrdID => GetText(41);
	public string? OrdStatus => GetText(39);
	public string? WorkingIndicator => GetText(636);
	public string? OrigOrdModTime => GetText(586);
	public string? ListID => GetText(66);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public string? CxlRejResponseTo => GetText(434);
	public FixNumber? CxlRejReason => GetNumber(102);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 Logon, MsgType A.</summary>
public sealed class Logon : FixMessage
{
	internal Logon(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "A", header, body, trailer)
	{
	}

	public FixNumber? EncryptMethod => GetNumber(98);
	public FixNumber? HeartBtInt => GetNumber(108);
	public FixNumber? RawDataLength => GetNumber(95);
	public string? RawData => GetText(96);
	public string? ResetSeqNumFlag => GetText(141);
	public FixNumber? NextExpectedMsgSeqNum => GetNumber(789);
	public FixNumber? MaxMessageSize => GetNumber(383);
	public IReadOnlyList<FixFieldSet> MsgTypeGrp => GetGroup(384);
	public string? TestMessageIndicator => GetText(464);
	public string? Username => GetText(553);
	public string? Password => GetText(554);
}

/// <summary>FIX 4.4 News, MsgType B.</summary>
public sealed class News : FixMessage
{
	internal News(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "B", header, body, trailer)
	{
	}

	public string? OrigTime => GetText(42);
	public string? Urgency => GetText(61);
	public string? Headline => GetText(148);
	public FixNumber? EncodedHeadlineLen => GetNumber(358);
	public string? EncodedHeadline => GetText(359);
	public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
	public IReadOnlyList<FixFieldSet> InstrmtGrp => GetGroup(146);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> LinesOfTextGrp => GetGroup(33);
	public string? URLLink => GetText(149);
	public FixNumber? RawDataLength => GetNumber(95);
	public string? RawData => GetText(96);
}

/// <summary>FIX 4.4 Email, MsgType C.</summary>
public sealed class Email : FixMessage
{
	internal Email(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "C", header, body, trailer)
	{
	}

	public string? EmailThreadID => GetText(164);
	public string? EmailType => GetText(94);
	public string? OrigTime => GetText(42);
	public string? Subject => GetText(147);
	public FixNumber? EncodedSubjectLen => GetNumber(356);
	public string? EncodedSubject => GetText(357);
	public IReadOnlyList<FixFieldSet> RoutingGrp => GetGroup(215);
	public IReadOnlyList<FixFieldSet> InstrmtGrp => GetGroup(146);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? OrderID => GetText(37);
	public string? ClOrdID => GetText(11);
	public IReadOnlyList<FixFieldSet> LinesOfTextGrp => GetGroup(33);
	public FixNumber? RawDataLength => GetNumber(95);
	public string? RawData => GetText(96);
}

/// <summary>FIX 4.4 NewOrderSingle, MsgType D.</summary>
public sealed class NewOrderSingle : FixMessage
{
	internal NewOrderSingle(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "D", header, body, trailer)
	{
	}

	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? DayBookingInst => GetText(589);
	public string? BookingUnit => GetText(590);
	public string? PreallocMethod => GetText(591);
	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> PreAllocGrp => GetGroup(78);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? CashMargin => GetText(544);
	public string? ClearingFeeIndicator => GetText(635);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? ProcessCode => GetText(81);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? PrevClosePx => GetNumber(140);
	public string? Side => GetText(54);
	public string? LocateReqd => GetText(114);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? SolicitedFlag => GetText(377);
	public string? IOIID => GetText(23);
	public string? QuoteID => GetText(117);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ForexReq => GetText(121);
	public string? SettlCurrency => GetText(120);
	public FixNumber? BookingType => GetNumber(775);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public FixNumber? Price2 => GetNumber(640);
	public string? PositionEffect => GetText(77);
	public FixNumber? CoveredOrUncovered => GetNumber(203);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
}

/// <summary>FIX 4.4 NewOrderList, MsgType E.</summary>
public sealed class NewOrderList : FixMessage
{
	internal NewOrderList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "E", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public string? BidID => GetText(390);
	public string? ClientBidID => GetText(391);
	public FixNumber? ProgRptReqs => GetNumber(414);
	public FixNumber? BidType => GetNumber(394);
	public FixNumber? ProgPeriodInterval => GetNumber(415);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? ListExecInstType => GetText(433);
	public string? ListExecInst => GetText(69);
	public FixNumber? EncodedListExecInstLen => GetNumber(352);
	public string? EncodedListExecInst => GetText(353);
	public FixNumber? AllowableOneSidednessPct => GetNumber(765);
	public FixNumber? AllowableOneSidednessValue => GetNumber(766);
	public string? AllowableOneSidednessCurr => GetText(767);
	public FixNumber? TotNoOrders => GetNumber(68);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> ListOrdGrp => GetGroup(73);
}

/// <summary>FIX 4.4 OrderCancelRequest, MsgType F.</summary>
public sealed class OrderCancelRequest : FixMessage
{
	internal OrderCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "F", header, body, trailer)
	{
	}

	public string? OrigClOrdID => GetText(41);
	public string? OrderID => GetText(37);
	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public string? ListID => GetText(66);
	public string? OrigOrdModTime => GetText(586);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public string? TransactTime => GetText(60);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? ComplianceID => GetText(376);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 OrderCancelReplaceRequest, MsgType G.</summary>
public sealed class OrderCancelReplaceRequest : FixMessage
{
	internal OrderCancelReplaceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "G", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? OrigClOrdID => GetText(41);
	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public string? ListID => GetText(66);
	public string? OrigOrdModTime => GetText(586);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? DayBookingInst => GetText(589);
	public string? BookingUnit => GetText(590);
	public string? PreallocMethod => GetText(591);
	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> PreAllocGrp => GetGroup(78);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? CashMargin => GetText(544);
	public string? ClearingFeeIndicator => GetText(635);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public string? TransactTime => GetText(60);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? ComplianceID => GetText(376);
	public string? SolicitedFlag => GetText(377);
	public string? Currency => GetText(15);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ForexReq => GetText(121);
	public string? SettlCurrency => GetText(120);
	public FixNumber? BookingType => GetNumber(775);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public FixNumber? Price2 => GetNumber(640);
	public string? PositionEffect => GetText(77);
	public FixNumber? CoveredOrUncovered => GetNumber(203);
	public FixNumber? MaxShow => GetNumber(210);
	public string? LocateReqd => GetText(114);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
}

/// <summary>FIX 4.4 OrderStatusRequest, MsgType H.</summary>
public sealed class OrderStatusRequest : FixMessage
{
	internal OrderStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "H", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? OrdStatusReqID => GetText(790);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
}

/// <summary>FIX 4.4 AllocationInstruction, MsgType J.</summary>
public sealed class AllocationInstruction : FixMessage
{
	internal AllocationInstruction(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "J", header, body, trailer)
	{
	}

	public string? AllocID => GetText(70);
	public string? AllocTransType => GetText(71);
	public FixNumber? AllocType => GetNumber(626);
	public string? SecondaryAllocID => GetText(793);
	public string? RefAllocID => GetText(72);
	public FixNumber? AllocCancReplaceReason => GetNumber(796);
	public FixNumber? AllocIntermedReqType => GetNumber(808);
	public string? AllocLinkID => GetText(196);
	public FixNumber? AllocLinkType => GetNumber(197);
	public string? BookingRefID => GetText(466);
	public FixNumber? AllocNoOrdersType => GetNumber(857);
	public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
	public IReadOnlyList<FixFieldSet> ExecAllocGrp => GetGroup(124);
	public string? PreviouslyReported => GetText(570);
	public string? ReversalIndicator => GetText(700);
	public string? MatchType => GetText(574);
	public string? Side => GetText(54);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? LastMkt => GetText(30);
	public string? TradeOriginationDate => GetText(229);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AvgPx => GetNumber(6);
	public FixNumber? AvgParPx => GetNumber(860);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? Currency => GetText(15);
	public FixNumber? AvgPxPrecision => GetNumber(74);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public FixNumber? BookingType => GetNumber(775);
	public FixNumber? GrossTradeAmt => GetNumber(381);
	public FixNumber? Concession => GetNumber(238);
	public FixNumber? TotalTakedown => GetNumber(237);
	public FixNumber? NetMoney => GetNumber(118);
	public string? PositionEffect => GetText(77);
	public string? AutoAcceptIndicator => GetText(754);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public FixNumber? NumDaysInterest => GetNumber(157);
	public FixNumber? AccruedInterestRate => GetNumber(158);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? TotalAccruedInterestAmt => GetNumber(540);
	public FixNumber? InterestAtMaturity => GetNumber(738);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public string? LegalConfirm => GetText(650);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? TotNoAllocs => GetNumber(892);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> AllocGrp => GetGroup(78);
}

/// <summary>FIX 4.4 ListCancelRequest, MsgType K.</summary>
public sealed class ListCancelRequest : FixMessage
{
	internal ListCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "K", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public string? TransactTime => GetText(60);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 ListExecute, MsgType L.</summary>
public sealed class ListExecute : FixMessage
{
	internal ListExecute(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "L", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public string? ClientBidID => GetText(391);
	public string? BidID => GetText(390);
	public string? TransactTime => GetText(60);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 ListStatusRequest, MsgType M.</summary>
public sealed class ListStatusRequest : FixMessage
{
	internal ListStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "M", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 ListStatus, MsgType N.</summary>
public sealed class ListStatus : FixMessage
{
	internal ListStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "N", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public FixNumber? ListStatusType => GetNumber(429);
	public FixNumber? NoRpts => GetNumber(82);
	public FixNumber? ListOrderStatus => GetNumber(431);
	public FixNumber? RptSeq => GetNumber(83);
	public string? ListStatusText => GetText(444);
	public FixNumber? EncodedListStatusTextLen => GetNumber(445);
	public string? EncodedListStatusText => GetText(446);
	public string? TransactTime => GetText(60);
	public FixNumber? TotNoOrders => GetNumber(68);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> OrdListStatGrp => GetGroup(73);
}

/// <summary>FIX 4.4 AllocationInstructionAck, MsgType P.</summary>
public sealed class AllocationInstructionAck : FixMessage
{
	internal AllocationInstructionAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "P", header, body, trailer)
	{
	}

	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? SecondaryAllocID => GetText(793);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public FixNumber? AllocStatus => GetNumber(87);
	public FixNumber? AllocRejCode => GetNumber(88);
	public FixNumber? AllocType => GetNumber(626);
	public FixNumber? AllocIntermedReqType => GetNumber(808);
	public string? MatchStatus => GetText(573);
	public FixNumber? Product => GetNumber(460);
	public string? SecurityType => GetText(167);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public IReadOnlyList<FixFieldSet> AllocAckGrp => GetGroup(78);
}

/// <summary>FIX 4.4 DontKnowTrade, MsgType Q.</summary>
public sealed class DontKnowTrade : FixMessage
{
	internal DontKnowTrade(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "Q", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? ExecID => GetText(17);
	public string? DKReason => GetText(127);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? Side => GetText(54);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public FixNumber? LastQty => GetNumber(32);
	public FixNumber? LastPx => GetNumber(31);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 QuoteRequest, MsgType R.</summary>
public sealed class QuoteRequest : FixMessage
{
	internal QuoteRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "R", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? RFQReqID => GetText(644);
	public string? ClOrdID => GetText(11);
	public string? OrderCapacity => GetText(528);
	public IReadOnlyList<FixFieldSet> QuotReqGrp => GetGroup(146);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 Quote, MsgType S.</summary>
public sealed class Quote : FixMessage
{
	internal Quote(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "S", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? QuoteID => GetText(117);
	public string? QuoteRespID => GetText(693);
	public FixNumber? QuoteType => GetNumber(537);
	public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
	public FixNumber? QuoteResponseLevel => GetNumber(301);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public IReadOnlyList<FixFieldSet> LegQuotGrp => GetGroup(555);
	public FixNumber? BidPx => GetNumber(132);
	public FixNumber? OfferPx => GetNumber(133);
	public FixNumber? MktBidPx => GetNumber(645);
	public FixNumber? MktOfferPx => GetNumber(646);
	public FixNumber? MinBidSize => GetNumber(647);
	public FixNumber? BidSize => GetNumber(134);
	public FixNumber? MinOfferSize => GetNumber(648);
	public FixNumber? OfferSize => GetNumber(135);
	public string? ValidUntilTime => GetText(62);
	public FixNumber? BidSpotRate => GetNumber(188);
	public FixNumber? OfferSpotRate => GetNumber(190);
	public FixNumber? BidForwardPoints => GetNumber(189);
	public FixNumber? OfferForwardPoints => GetNumber(191);
	public FixNumber? MidPx => GetNumber(631);
	public FixNumber? BidYield => GetNumber(632);
	public FixNumber? MidYield => GetNumber(633);
	public FixNumber? OfferYield => GetNumber(634);
	public string? TransactTime => GetText(60);
	public string? OrdType => GetText(40);
	public FixNumber? BidForwardPoints2 => GetNumber(642);
	public FixNumber? OfferForwardPoints2 => GetNumber(643);
	public FixNumber? SettlCurrBidFxRate => GetNumber(656);
	public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
	public string? SettlCurrFxRateCalc => GetText(156);
	public string? CommType => GetText(13);
	public FixNumber? Commission => GetNumber(12);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ExDestination => GetText(100);
	public string? OrderCapacity => GetText(528);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 SettlementInstructions, MsgType T.</summary>
public sealed class SettlementInstructions : FixMessage
{
	internal SettlementInstructions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "T", header, body, trailer)
	{
	}

	public string? SettlInstMsgID => GetText(777);
	public string? SettlInstReqID => GetText(791);
	public string? SettlInstMode => GetText(160);
	public FixNumber? SettlInstReqRejCode => GetNumber(792);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? ClOrdID => GetText(11);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> SettlInstGrp => GetGroup(778);
}

/// <summary>FIX 4.4 MarketDataRequest, MsgType V.</summary>
public sealed class MarketDataRequest : FixMessage
{
	internal MarketDataRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "V", header, body, trailer)
	{
	}

	public string? MDReqID => GetText(262);
	public string? SubscriptionRequestType => GetText(263);
	public FixNumber? MarketDepth => GetNumber(264);
	public FixNumber? MDUpdateType => GetNumber(265);
	public string? AggregatedBook => GetText(266);
	public string? OpenCloseSettlFlag => GetText(286);
	public string? Scope => GetText(546);
	public string? MDImplicitDelete => GetText(547);
	public IReadOnlyList<FixFieldSet> MDReqGrp => GetGroup(267);
	public IReadOnlyList<FixFieldSet> InstrmtMDReqGrp => GetGroup(146);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public FixNumber? ApplQueueAction => GetNumber(815);
	public FixNumber? ApplQueueMax => GetNumber(812);
}

/// <summary>FIX 4.4 MarketDataSnapshotFullRefresh, MsgType W.</summary>
public sealed class MarketDataSnapshotFullRefresh : FixMessage
{
	internal MarketDataSnapshotFullRefresh(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "W", header, body, trailer)
	{
	}

	public string? MDReqID => GetText(262);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? FinancialStatus => GetText(291);
	public string? CorporateAction => GetText(292);
	public FixNumber? NetChgPrevDay => GetNumber(451);
	public IReadOnlyList<FixFieldSet> MDFullGrp => GetGroup(268);
	public FixNumber? ApplQueueDepth => GetNumber(813);
	public FixNumber? ApplQueueResolution => GetNumber(814);
}

/// <summary>FIX 4.4 MarketDataIncrementalRefresh, MsgType X.</summary>
public sealed class MarketDataIncrementalRefresh : FixMessage
{
	internal MarketDataIncrementalRefresh(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "X", header, body, trailer)
	{
	}

	public string? MDReqID => GetText(262);
	public IReadOnlyList<FixFieldSet> MDIncGrp => GetGroup(268);
	public FixNumber? ApplQueueDepth => GetNumber(813);
	public FixNumber? ApplQueueResolution => GetNumber(814);
}

/// <summary>FIX 4.4 MarketDataRequestReject, MsgType Y.</summary>
public sealed class MarketDataRequestReject : FixMessage
{
	internal MarketDataRequestReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "Y", header, body, trailer)
	{
	}

	public string? MDReqID => GetText(262);
	public string? MDReqRejReason => GetText(281);
	public IReadOnlyList<FixFieldSet> MDRjctGrp => GetGroup(816);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 QuoteCancel, MsgType Z.</summary>
public sealed class QuoteCancel : FixMessage
{
	internal QuoteCancel(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "Z", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? QuoteID => GetText(117);
	public FixNumber? QuoteCancelType => GetNumber(298);
	public FixNumber? QuoteResponseLevel => GetNumber(301);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public IReadOnlyList<FixFieldSet> QuotCxlEntriesGrp => GetGroup(295);
}

/// <summary>FIX 4.4 QuoteStatusRequest, MsgType a.</summary>
public sealed class QuoteStatusRequest : FixMessage
{
	internal QuoteStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "a", header, body, trailer)
	{
	}

	public string? QuoteStatusReqID => GetText(649);
	public string? QuoteID => GetText(117);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 MassQuoteAcknowledgement, MsgType b.</summary>
public sealed class MassQuoteAcknowledgement : FixMessage
{
	internal MassQuoteAcknowledgement(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "b", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? QuoteID => GetText(117);
	public FixNumber? QuoteStatus => GetNumber(297);
	public FixNumber? QuoteRejectReason => GetNumber(300);
	public FixNumber? QuoteResponseLevel => GetNumber(301);
	public FixNumber? QuoteType => GetNumber(537);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public IReadOnlyList<FixFieldSet> QuotSetAckGrp => GetGroup(296);
}

/// <summary>FIX 4.4 SecurityDefinitionRequest, MsgType c.</summary>
public sealed class SecurityDefinitionRequest : FixMessage
{
	internal SecurityDefinitionRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "c", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public FixNumber? SecurityRequestType => GetNumber(321);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Currency => GetText(15);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public FixNumber? ExpirationCycle => GetNumber(827);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 SecurityDefinition, MsgType d.</summary>
public sealed class SecurityDefinition : FixMessage
{
	internal SecurityDefinition(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "d", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public string? SecurityResponseID => GetText(322);
	public FixNumber? SecurityResponseType => GetNumber(323);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Currency => GetText(15);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public FixNumber? ExpirationCycle => GetNumber(827);
	public FixNumber? RoundLot => GetNumber(561);
	public FixNumber? MinTradeVol => GetNumber(562);
}

/// <summary>FIX 4.4 SecurityStatusRequest, MsgType e.</summary>
public sealed class SecurityStatusRequest : FixMessage
{
	internal SecurityStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "e", header, body, trailer)
	{
	}

	public string? SecurityStatusReqID => GetText(324);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? Currency => GetText(15);
	public string? SubscriptionRequestType => GetText(263);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
}

/// <summary>FIX 4.4 SecurityStatus, MsgType f.</summary>
public sealed class SecurityStatus : FixMessage
{
	internal SecurityStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "f", header, body, trailer)
	{
	}

	public string? SecurityStatusReqID => GetText(324);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? Currency => GetText(15);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? UnsolicitedIndicator => GetText(325);
	public FixNumber? SecurityTradingStatus => GetNumber(326);
	public string? FinancialStatus => GetText(291);
	public string? CorporateAction => GetText(292);
	public string? HaltReason => GetText(327);
	public string? InViewOfCommon => GetText(328);
	public string? DueToRelated => GetText(329);
	public FixNumber? BuyVolume => GetNumber(330);
	public FixNumber? SellVolume => GetNumber(331);
	public FixNumber? HighPx => GetNumber(332);
	public FixNumber? LowPx => GetNumber(333);
	public FixNumber? LastPx => GetNumber(31);
	public string? TransactTime => GetText(60);
	public FixNumber? Adjustment => GetNumber(334);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 TradingSessionStatusRequest, MsgType g.</summary>
public sealed class TradingSessionStatusRequest : FixMessage
{
	internal TradingSessionStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "g", header, body, trailer)
	{
	}

	public string? TradSesReqID => GetText(335);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public FixNumber? TradSesMethod => GetNumber(338);
	public FixNumber? TradSesMode => GetNumber(339);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 TradingSessionStatus, MsgType h.</summary>
public sealed class TradingSessionStatus : FixMessage
{
	internal TradingSessionStatus(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "h", header, body, trailer)
	{
	}

	public string? TradSesReqID => GetText(335);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public FixNumber? TradSesMethod => GetNumber(338);
	public FixNumber? TradSesMode => GetNumber(339);
	public string? UnsolicitedIndicator => GetText(325);
	public FixNumber? TradSesStatus => GetNumber(340);
	public FixNumber? TradSesStatusRejReason => GetNumber(567);
	public string? TradSesStartTime => GetText(341);
	public string? TradSesOpenTime => GetText(342);
	public string? TradSesPreCloseTime => GetText(343);
	public string? TradSesCloseTime => GetText(344);
	public string? TradSesEndTime => GetText(345);
	public FixNumber? TotalVolumeTraded => GetNumber(387);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 MassQuote, MsgType i.</summary>
public sealed class MassQuote : FixMessage
{
	internal MassQuote(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "i", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? QuoteID => GetText(117);
	public FixNumber? QuoteType => GetNumber(537);
	public FixNumber? QuoteResponseLevel => GetNumber(301);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public FixNumber? DefBidSize => GetNumber(293);
	public FixNumber? DefOfferSize => GetNumber(294);
	public IReadOnlyList<FixFieldSet> QuotSetGrp => GetGroup(296);
}

/// <summary>FIX 4.4 BusinessMessageReject, MsgType j.</summary>
public sealed class BusinessMessageReject : FixMessage
{
	internal BusinessMessageReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "j", header, body, trailer)
	{
	}

	public FixNumber? RefSeqNum => GetNumber(45);
	public string? RefMsgType => GetText(372);
	public string? BusinessRejectRefID => GetText(379);
	public FixNumber? BusinessRejectReason => GetNumber(380);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 BidRequest, MsgType k.</summary>
public sealed class BidRequest : FixMessage
{
	internal BidRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "k", header, body, trailer)
	{
	}

	public string? BidID => GetText(390);
	public string? ClientBidID => GetText(391);
	public string? BidRequestTransType => GetText(374);
	public string? ListName => GetText(392);
	public FixNumber? TotNoRelatedSym => GetNumber(393);
	public FixNumber? BidType => GetNumber(394);
	public FixNumber? NumTickets => GetNumber(395);
	public string? Currency => GetText(15);
	public FixNumber? SideValue1 => GetNumber(396);
	public FixNumber? SideValue2 => GetNumber(397);
	public IReadOnlyList<FixFieldSet> BidDescReqGrp => GetGroup(398);
	public IReadOnlyList<FixFieldSet> BidCompReqGrp => GetGroup(420);
	public FixNumber? LiquidityIndType => GetNumber(409);
	public FixNumber? WtAverageLiquidity => GetNumber(410);
	public string? ExchangeForPhysical => GetText(411);
	public FixNumber? OutMainCntryUIndex => GetNumber(412);
	public FixNumber? CrossPercent => GetNumber(413);
	public FixNumber? ProgRptReqs => GetNumber(414);
	public FixNumber? ProgPeriodInterval => GetNumber(415);
	public FixNumber? IncTaxInd => GetNumber(416);
	public string? ForexReq => GetText(121);
	public FixNumber? NumBidders => GetNumber(417);
	public string? TradeDate => GetText(75);
	public string? BidTradeType => GetText(418);
	public string? BasisPxType => GetText(419);
	public string? StrikeTime => GetText(443);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 BidResponse, MsgType l.</summary>
public sealed class BidResponse : FixMessage
{
	internal BidResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "l", header, body, trailer)
	{
	}

	public string? BidID => GetText(390);
	public string? ClientBidID => GetText(391);
	public IReadOnlyList<FixFieldSet> BidCompRspGrp => GetGroup(420);
}

/// <summary>FIX 4.4 ListStrikePrice, MsgType m.</summary>
public sealed class ListStrikePrice : FixMessage
{
	internal ListStrikePrice(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "m", header, body, trailer)
	{
	}

	public string? ListID => GetText(66);
	public FixNumber? TotNoStrikes => GetNumber(422);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> InstrmtStrkPxGrp => GetGroup(428);
	public IReadOnlyList<FixFieldSet> UndInstrmtStrkPxGrp => GetGroup(711);
}

/// <summary>FIX 4.4 XMLnonFIX, MsgType n.</summary>
public sealed class XMLnonFIX : FixMessage
{
	internal XMLnonFIX(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "n", header, body, trailer)
	{
	}

}

/// <summary>FIX 4.4 RegistrationInstructions, MsgType o.</summary>
public sealed class RegistrationInstructions : FixMessage
{
	internal RegistrationInstructions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "o", header, body, trailer)
	{
	}

	public string? RegistID => GetText(513);
	public string? RegistTransType => GetText(514);
	public string? RegistRefID => GetText(508);
	public string? ClOrdID => GetText(11);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public string? RegistAcctType => GetText(493);
	public FixNumber? TaxAdvantageType => GetNumber(495);
	public string? OwnershipType => GetText(517);
	public IReadOnlyList<FixFieldSet> RgstDtlsGrp => GetGroup(473);
	public IReadOnlyList<FixFieldSet> RgstDistInstGrp => GetGroup(510);
}

/// <summary>FIX 4.4 RegistrationInstructionsResponse, MsgType p.</summary>
public sealed class RegistrationInstructionsResponse : FixMessage
{
	internal RegistrationInstructionsResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "p", header, body, trailer)
	{
	}

	public string? RegistID => GetText(513);
	public string? RegistTransType => GetText(514);
	public string? RegistRefID => GetText(508);
	public string? ClOrdID => GetText(11);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public string? RegistStatus => GetText(506);
	public FixNumber? RegistRejReasonCode => GetNumber(507);
	public string? RegistRejReasonText => GetText(496);
}

/// <summary>FIX 4.4 OrderMassCancelRequest, MsgType q.</summary>
public sealed class OrderMassCancelRequest : FixMessage
{
	internal OrderMassCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "q", header, body, trailer)
	{
	}

	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? MassCancelRequestType => GetText(530);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? UnderlyingSymbol => GetText(311);
	public string? UnderlyingSymbolSfx => GetText(312);
	public string? UnderlyingSecurityID => GetText(309);
	public string? UnderlyingSecurityIDSource => GetText(305);
	public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
	public FixNumber? UnderlyingProduct => GetNumber(462);
	public string? UnderlyingCFICode => GetText(463);
	public string? UnderlyingSecurityType => GetText(310);
	public string? UnderlyingSecuritySubType => GetText(763);
	public string? UnderlyingMaturityMonthYear => GetText(313);
	public string? UnderlyingMaturityDate => GetText(542);
	public FixNumber? UnderlyingPutOrCall => GetNumber(315);
	public string? UnderlyingCouponPaymentDate => GetText(241);
	public string? UnderlyingIssueDate => GetText(242);
	public string? UnderlyingRepoCollateralSecurityType => GetText(243);
	public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
	public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
	public FixNumber? UnderlyingFactor => GetNumber(246);
	public string? UnderlyingCreditRating => GetText(256);
	public string? UnderlyingInstrRegistry => GetText(595);
	public string? UnderlyingCountryOfIssue => GetText(592);
	public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
	public string? UnderlyingLocaleOfIssue => GetText(594);
	public string? UnderlyingRedemptionDate => GetText(247);
	public FixNumber? UnderlyingStrikePrice => GetNumber(316);
	public string? UnderlyingStrikeCurrency => GetText(941);
	public string? UnderlyingOptAttribute => GetText(317);
	public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
	public FixNumber? UnderlyingCouponRate => GetNumber(435);
	public string? UnderlyingSecurityExchange => GetText(308);
	public string? UnderlyingIssuer => GetText(306);
	public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
	public string? EncodedUnderlyingIssuer => GetText(363);
	public string? UnderlyingSecurityDesc => GetText(307);
	public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
	public string? EncodedUnderlyingSecurityDesc => GetText(365);
	public string? UnderlyingCPProgram => GetText(877);
	public string? UnderlyingCPRegType => GetText(878);
	public string? UnderlyingCurrency => GetText(318);
	public FixNumber? UnderlyingQty => GetNumber(879);
	public FixNumber? UnderlyingPx => GetNumber(810);
	public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
	public FixNumber? UnderlyingEndPrice => GetNumber(883);
	public FixNumber? UnderlyingStartValue => GetNumber(884);
	public FixNumber? UnderlyingCurrentValue => GetNumber(885);
	public FixNumber? UnderlyingEndValue => GetNumber(886);
	public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
	public string? Side => GetText(54);
	public string? TransactTime => GetText(60);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 OrderMassCancelReport, MsgType r.</summary>
public sealed class OrderMassCancelReport : FixMessage
{
	internal OrderMassCancelReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "r", header, body, trailer)
	{
	}

	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? MassCancelRequestType => GetText(530);
	public string? MassCancelResponse => GetText(531);
	public FixNumber? MassCancelRejectReason => GetNumber(532);
	public FixNumber? TotalAffectedOrders => GetNumber(533);
	public IReadOnlyList<FixFieldSet> AffectedOrdGrp => GetGroup(534);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? UnderlyingSymbol => GetText(311);
	public string? UnderlyingSymbolSfx => GetText(312);
	public string? UnderlyingSecurityID => GetText(309);
	public string? UnderlyingSecurityIDSource => GetText(305);
	public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
	public FixNumber? UnderlyingProduct => GetNumber(462);
	public string? UnderlyingCFICode => GetText(463);
	public string? UnderlyingSecurityType => GetText(310);
	public string? UnderlyingSecuritySubType => GetText(763);
	public string? UnderlyingMaturityMonthYear => GetText(313);
	public string? UnderlyingMaturityDate => GetText(542);
	public FixNumber? UnderlyingPutOrCall => GetNumber(315);
	public string? UnderlyingCouponPaymentDate => GetText(241);
	public string? UnderlyingIssueDate => GetText(242);
	public string? UnderlyingRepoCollateralSecurityType => GetText(243);
	public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
	public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
	public FixNumber? UnderlyingFactor => GetNumber(246);
	public string? UnderlyingCreditRating => GetText(256);
	public string? UnderlyingInstrRegistry => GetText(595);
	public string? UnderlyingCountryOfIssue => GetText(592);
	public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
	public string? UnderlyingLocaleOfIssue => GetText(594);
	public string? UnderlyingRedemptionDate => GetText(247);
	public FixNumber? UnderlyingStrikePrice => GetNumber(316);
	public string? UnderlyingStrikeCurrency => GetText(941);
	public string? UnderlyingOptAttribute => GetText(317);
	public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
	public FixNumber? UnderlyingCouponRate => GetNumber(435);
	public string? UnderlyingSecurityExchange => GetText(308);
	public string? UnderlyingIssuer => GetText(306);
	public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
	public string? EncodedUnderlyingIssuer => GetText(363);
	public string? UnderlyingSecurityDesc => GetText(307);
	public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
	public string? EncodedUnderlyingSecurityDesc => GetText(365);
	public string? UnderlyingCPProgram => GetText(877);
	public string? UnderlyingCPRegType => GetText(878);
	public string? UnderlyingCurrency => GetText(318);
	public FixNumber? UnderlyingQty => GetNumber(879);
	public FixNumber? UnderlyingPx => GetNumber(810);
	public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
	public FixNumber? UnderlyingEndPrice => GetNumber(883);
	public FixNumber? UnderlyingStartValue => GetNumber(884);
	public FixNumber? UnderlyingCurrentValue => GetNumber(885);
	public FixNumber? UnderlyingEndValue => GetNumber(886);
	public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
	public string? Side => GetText(54);
	public string? TransactTime => GetText(60);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 NewOrderCross, MsgType s.</summary>
public sealed class NewOrderCross : FixMessage
{
	internal NewOrderCross(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "s", header, body, trailer)
	{
	}

	public string? CrossID => GetText(548);
	public FixNumber? CrossType => GetNumber(549);
	public FixNumber? CrossPrioritization => GetNumber(550);
	public IReadOnlyList<FixFieldSet> SideCrossOrdModGrp => GetGroup(552);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? ProcessCode => GetText(81);
	public FixNumber? PrevClosePx => GetNumber(140);
	public string? LocateReqd => GetText(114);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? IOIID => GetText(23);
	public string? QuoteID => GetText(117);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
}

/// <summary>FIX 4.4 CrossOrderCancelReplaceRequest, MsgType t.</summary>
public sealed class CrossOrderCancelReplaceRequest : FixMessage
{
	internal CrossOrderCancelReplaceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "t", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? CrossID => GetText(548);
	public string? OrigCrossID => GetText(551);
	public FixNumber? CrossType => GetNumber(549);
	public FixNumber? CrossPrioritization => GetNumber(550);
	public IReadOnlyList<FixFieldSet> SideCrossOrdModGrp => GetGroup(552);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? ProcessCode => GetText(81);
	public FixNumber? PrevClosePx => GetNumber(140);
	public string? LocateReqd => GetText(114);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? IOIID => GetText(23);
	public string? QuoteID => GetText(117);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
}

/// <summary>FIX 4.4 CrossOrderCancelRequest, MsgType u.</summary>
public sealed class CrossOrderCancelRequest : FixMessage
{
	internal CrossOrderCancelRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "u", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? CrossID => GetText(548);
	public string? OrigCrossID => GetText(551);
	public FixNumber? CrossType => GetNumber(549);
	public FixNumber? CrossPrioritization => GetNumber(550);
	public IReadOnlyList<FixFieldSet> SideCrossOrdCxlGrp => GetGroup(552);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? TransactTime => GetText(60);
}

/// <summary>FIX 4.4 SecurityTypeRequest, MsgType v.</summary>
public sealed class SecurityTypeRequest : FixMessage
{
	internal SecurityTypeRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "v", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public FixNumber? Product => GetNumber(460);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
}

/// <summary>FIX 4.4 SecurityTypes, MsgType w.</summary>
public sealed class SecurityTypes : FixMessage
{
	internal SecurityTypes(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "w", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public string? SecurityResponseID => GetText(322);
	public FixNumber? SecurityResponseType => GetNumber(323);
	public FixNumber? TotNoSecurityTypes => GetNumber(557);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> SecTypesGrp => GetGroup(558);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 SecurityListRequest, MsgType x.</summary>
public sealed class SecurityListRequest : FixMessage
{
	internal SecurityListRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "x", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public FixNumber? SecurityListRequestType => GetNumber(559);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? Currency => GetText(15);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 SecurityList, MsgType y.</summary>
public sealed class SecurityList : FixMessage
{
	internal SecurityList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "y", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public string? SecurityResponseID => GetText(322);
	public FixNumber? SecurityRequestResult => GetNumber(560);
	public FixNumber? TotNoRelatedSym => GetNumber(393);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> SecListGrp => GetGroup(146);
}

/// <summary>FIX 4.4 DerivativeSecurityListRequest, MsgType z.</summary>
public sealed class DerivativeSecurityListRequest : FixMessage
{
	internal DerivativeSecurityListRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "z", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public FixNumber? SecurityListRequestType => GetNumber(559);
	public string? UnderlyingSymbol => GetText(311);
	public string? UnderlyingSymbolSfx => GetText(312);
	public string? UnderlyingSecurityID => GetText(309);
	public string? UnderlyingSecurityIDSource => GetText(305);
	public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
	public FixNumber? UnderlyingProduct => GetNumber(462);
	public string? UnderlyingCFICode => GetText(463);
	public string? UnderlyingSecurityType => GetText(310);
	public string? UnderlyingSecuritySubType => GetText(763);
	public string? UnderlyingMaturityMonthYear => GetText(313);
	public string? UnderlyingMaturityDate => GetText(542);
	public FixNumber? UnderlyingPutOrCall => GetNumber(315);
	public string? UnderlyingCouponPaymentDate => GetText(241);
	public string? UnderlyingIssueDate => GetText(242);
	public string? UnderlyingRepoCollateralSecurityType => GetText(243);
	public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
	public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
	public FixNumber? UnderlyingFactor => GetNumber(246);
	public string? UnderlyingCreditRating => GetText(256);
	public string? UnderlyingInstrRegistry => GetText(595);
	public string? UnderlyingCountryOfIssue => GetText(592);
	public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
	public string? UnderlyingLocaleOfIssue => GetText(594);
	public string? UnderlyingRedemptionDate => GetText(247);
	public FixNumber? UnderlyingStrikePrice => GetNumber(316);
	public string? UnderlyingStrikeCurrency => GetText(941);
	public string? UnderlyingOptAttribute => GetText(317);
	public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
	public FixNumber? UnderlyingCouponRate => GetNumber(435);
	public string? UnderlyingSecurityExchange => GetText(308);
	public string? UnderlyingIssuer => GetText(306);
	public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
	public string? EncodedUnderlyingIssuer => GetText(363);
	public string? UnderlyingSecurityDesc => GetText(307);
	public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
	public string? EncodedUnderlyingSecurityDesc => GetText(365);
	public string? UnderlyingCPProgram => GetText(877);
	public string? UnderlyingCPRegType => GetText(878);
	public string? UnderlyingCurrency => GetText(318);
	public FixNumber? UnderlyingQty => GetNumber(879);
	public FixNumber? UnderlyingPx => GetNumber(810);
	public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
	public FixNumber? UnderlyingEndPrice => GetNumber(883);
	public FixNumber? UnderlyingStartValue => GetNumber(884);
	public FixNumber? UnderlyingCurrentValue => GetNumber(885);
	public FixNumber? UnderlyingEndValue => GetNumber(886);
	public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
	public string? SecuritySubType => GetText(762);
	public string? Currency => GetText(15);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 DerivativeSecurityList, MsgType AA.</summary>
public sealed class DerivativeSecurityList : FixMessage
{
	internal DerivativeSecurityList(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AA", header, body, trailer)
	{
	}

	public string? SecurityReqID => GetText(320);
	public string? SecurityResponseID => GetText(322);
	public FixNumber? SecurityRequestResult => GetNumber(560);
	public string? UnderlyingSymbol => GetText(311);
	public string? UnderlyingSymbolSfx => GetText(312);
	public string? UnderlyingSecurityID => GetText(309);
	public string? UnderlyingSecurityIDSource => GetText(305);
	public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
	public FixNumber? UnderlyingProduct => GetNumber(462);
	public string? UnderlyingCFICode => GetText(463);
	public string? UnderlyingSecurityType => GetText(310);
	public string? UnderlyingSecuritySubType => GetText(763);
	public string? UnderlyingMaturityMonthYear => GetText(313);
	public string? UnderlyingMaturityDate => GetText(542);
	public FixNumber? UnderlyingPutOrCall => GetNumber(315);
	public string? UnderlyingCouponPaymentDate => GetText(241);
	public string? UnderlyingIssueDate => GetText(242);
	public string? UnderlyingRepoCollateralSecurityType => GetText(243);
	public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
	public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
	public FixNumber? UnderlyingFactor => GetNumber(246);
	public string? UnderlyingCreditRating => GetText(256);
	public string? UnderlyingInstrRegistry => GetText(595);
	public string? UnderlyingCountryOfIssue => GetText(592);
	public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
	public string? UnderlyingLocaleOfIssue => GetText(594);
	public string? UnderlyingRedemptionDate => GetText(247);
	public FixNumber? UnderlyingStrikePrice => GetNumber(316);
	public string? UnderlyingStrikeCurrency => GetText(941);
	public string? UnderlyingOptAttribute => GetText(317);
	public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
	public FixNumber? UnderlyingCouponRate => GetNumber(435);
	public string? UnderlyingSecurityExchange => GetText(308);
	public string? UnderlyingIssuer => GetText(306);
	public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
	public string? EncodedUnderlyingIssuer => GetText(363);
	public string? UnderlyingSecurityDesc => GetText(307);
	public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
	public string? EncodedUnderlyingSecurityDesc => GetText(365);
	public string? UnderlyingCPProgram => GetText(877);
	public string? UnderlyingCPRegType => GetText(878);
	public string? UnderlyingCurrency => GetText(318);
	public FixNumber? UnderlyingQty => GetNumber(879);
	public FixNumber? UnderlyingPx => GetNumber(810);
	public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
	public FixNumber? UnderlyingEndPrice => GetNumber(883);
	public FixNumber? UnderlyingStartValue => GetNumber(884);
	public FixNumber? UnderlyingCurrentValue => GetNumber(885);
	public FixNumber? UnderlyingEndValue => GetNumber(886);
	public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
	public FixNumber? TotNoRelatedSym => GetNumber(393);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> RelSymDerivSecGrp => GetGroup(146);
}

/// <summary>FIX 4.4 NewOrderMultileg, MsgType AB.</summary>
public sealed class NewOrderMultileg : FixMessage
{
	internal NewOrderMultileg(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AB", header, body, trailer)
	{
	}

	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? DayBookingInst => GetText(589);
	public string? BookingUnit => GetText(590);
	public string? PreallocMethod => GetText(591);
	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> PreAllocMlegGrp => GetGroup(78);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? CashMargin => GetText(544);
	public string? ClearingFeeIndicator => GetText(635);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? ProcessCode => GetText(81);
	public string? Side => GetText(54);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? PrevClosePx => GetNumber(140);
	public IReadOnlyList<FixFieldSet> LegOrdGrp => GetGroup(555);
	public string? LocateReqd => GetText(114);
	public string? TransactTime => GetText(60);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? SolicitedFlag => GetText(377);
	public string? IOIID => GetText(23);
	public string? QuoteID => GetText(117);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ForexReq => GetText(121);
	public string? SettlCurrency => GetText(120);
	public FixNumber? BookingType => GetNumber(775);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? PositionEffect => GetText(77);
	public FixNumber? CoveredOrUncovered => GetNumber(203);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
	public FixNumber? MultiLegRptTypeReq => GetNumber(563);
}

/// <summary>FIX 4.4 MultilegOrderCancelReplace, MsgType AC.</summary>
public sealed class MultilegOrderCancelReplace : FixMessage
{
	internal MultilegOrderCancelReplace(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AC", header, body, trailer)
	{
	}

	public string? OrderID => GetText(37);
	public string? OrigClOrdID => GetText(41);
	public string? ClOrdID => GetText(11);
	public string? SecondaryClOrdID => GetText(526);
	public string? ClOrdLinkID => GetText(583);
	public string? OrigOrdModTime => GetText(586);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeOriginationDate => GetText(229);
	public string? TradeDate => GetText(75);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? DayBookingInst => GetText(589);
	public string? BookingUnit => GetText(590);
	public string? PreallocMethod => GetText(591);
	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> PreAllocMlegGrp => GetGroup(78);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? CashMargin => GetText(544);
	public string? ClearingFeeIndicator => GetText(635);
	public string? HandlInst => GetText(21);
	public string? ExecInst => GetText(18);
	public FixNumber? MinQty => GetNumber(110);
	public FixNumber? MaxFloor => GetNumber(111);
	public string? ExDestination => GetText(100);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? ProcessCode => GetText(81);
	public string? Side => GetText(54);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? PrevClosePx => GetNumber(140);
	public IReadOnlyList<FixFieldSet> LegOrdGrp => GetGroup(555);
	public string? LocateReqd => GetText(114);
	public string? TransactTime => GetText(60);
	public FixNumber? QtyType => GetNumber(854);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? OrdType => GetText(40);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? StopPx => GetNumber(99);
	public string? Currency => GetText(15);
	public string? ComplianceID => GetText(376);
	public string? SolicitedFlag => GetText(377);
	public string? IOIID => GetText(23);
	public string? QuoteID => GetText(117);
	public string? TimeInForce => GetText(59);
	public string? EffectiveTime => GetText(168);
	public string? ExpireDate => GetText(432);
	public string? ExpireTime => GetText(126);
	public FixNumber? GTBookingInst => GetNumber(427);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ForexReq => GetText(121);
	public string? SettlCurrency => GetText(120);
	public FixNumber? BookingType => GetNumber(775);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? PositionEffect => GetText(77);
	public FixNumber? CoveredOrUncovered => GetNumber(203);
	public FixNumber? MaxShow => GetNumber(210);
	public FixNumber? PegOffsetValue => GetNumber(211);
	public FixNumber? PegMoveType => GetNumber(835);
	public FixNumber? PegOffsetType => GetNumber(836);
	public FixNumber? PegLimitType => GetNumber(837);
	public FixNumber? PegRoundDirection => GetNumber(838);
	public FixNumber? PegScope => GetNumber(840);
	public string? DiscretionInst => GetText(388);
	public FixNumber? DiscretionOffsetValue => GetNumber(389);
	public FixNumber? DiscretionMoveType => GetNumber(841);
	public FixNumber? DiscretionOffsetType => GetNumber(842);
	public FixNumber? DiscretionLimitType => GetNumber(843);
	public FixNumber? DiscretionRoundDirection => GetNumber(844);
	public FixNumber? DiscretionScope => GetNumber(846);
	public FixNumber? TargetStrategy => GetNumber(847);
	public string? TargetStrategyParameters => GetText(848);
	public FixNumber? ParticipationRate => GetNumber(849);
	public string? CancellationRights => GetText(480);
	public string? MoneyLaunderingStatus => GetText(481);
	public string? RegistID => GetText(513);
	public string? Designation => GetText(494);
	public FixNumber? MultiLegRptTypeReq => GetNumber(563);
}

/// <summary>FIX 4.4 TradeCaptureReportRequest, MsgType AD.</summary>
public sealed class TradeCaptureReportRequest : FixMessage
{
	internal TradeCaptureReportRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AD", header, body, trailer)
	{
	}

	public string? TradeRequestID => GetText(568);
	public FixNumber? TradeRequestType => GetNumber(569);
	public string? SubscriptionRequestType => GetText(263);
	public string? TradeReportID => GetText(571);
	public string? SecondaryTradeReportID => GetText(818);
	public string? ExecID => GetText(17);
	public string? ExecType => GetText(150);
	public string? OrderID => GetText(37);
	public string? ClOrdID => GetText(11);
	public string? MatchStatus => GetText(573);
	public FixNumber? TrdType => GetNumber(828);
	public FixNumber? TrdSubType => GetNumber(829);
	public string? TransferReason => GetText(830);
	public FixNumber? SecondaryTrdType => GetNumber(855);
	public string? TradeLinkID => GetText(820);
	public string? TrdMatchID => GetText(880);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> TrdCapDtGrp => GetGroup(580);
	public string? ClearingBusinessDate => GetText(715);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? TimeBracket => GetText(943);
	public string? Side => GetText(54);
	public string? MultiLegReportingType => GetText(442);
	public string? TradeInputSource => GetText(578);
	public string? TradeInputDevice => GetText(579);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 TradeCaptureReport, MsgType AE.</summary>
public sealed class TradeCaptureReport : FixMessage
{
	internal TradeCaptureReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AE", header, body, trailer)
	{
	}

	public string? TradeReportID => GetText(571);
	public FixNumber? TradeReportTransType => GetNumber(487);
	public FixNumber? TradeReportType => GetNumber(856);
	public string? TradeRequestID => GetText(568);
	public FixNumber? TrdType => GetNumber(828);
	public FixNumber? TrdSubType => GetNumber(829);
	public FixNumber? SecondaryTrdType => GetNumber(855);
	public string? TransferReason => GetText(830);
	public string? ExecType => GetText(150);
	public FixNumber? TotNumTradeReports => GetNumber(748);
	public string? LastRptRequested => GetText(912);
	public string? UnsolicitedIndicator => GetText(325);
	public string? SubscriptionRequestType => GetText(263);
	public string? TradeReportRefID => GetText(572);
	public string? SecondaryTradeReportRefID => GetText(881);
	public string? SecondaryTradeReportID => GetText(818);
	public string? TradeLinkID => GetText(820);
	public string? TrdMatchID => GetText(880);
	public string? ExecID => GetText(17);
	public string? OrdStatus => GetText(39);
	public string? SecondaryExecID => GetText(527);
	public FixNumber? ExecRestatementReason => GetNumber(378);
	public string? PreviouslyReported => GetText(570);
	public FixNumber? PriceType => GetNumber(423);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public FixNumber? QtyType => GetNumber(854);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? UnderlyingTradingSessionID => GetText(822);
	public string? UnderlyingTradingSessionSubID => GetText(823);
	public FixNumber? LastQty => GetNumber(32);
	public FixNumber? LastPx => GetNumber(31);
	public FixNumber? LastParPx => GetNumber(669);
	public FixNumber? LastSpotRate => GetNumber(194);
	public FixNumber? LastForwardPoints => GetNumber(195);
	public string? LastMkt => GetText(30);
	public string? TradeDate => GetText(75);
	public string? ClearingBusinessDate => GetText(715);
	public FixNumber? AvgPx => GetNumber(6);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public FixNumber? AvgPxIndicator => GetNumber(819);
	public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
	public string? MultiLegReportingType => GetText(442);
	public string? TradeLegRefID => GetText(824);
	public IReadOnlyList<FixFieldSet> TrdInstrmtLegGrp => GetGroup(555);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? MatchStatus => GetText(573);
	public string? MatchType => GetText(574);
	public IReadOnlyList<FixFieldSet> TrdCapRptSideGrp => GetGroup(552);
	public string? CopyMsgIndicator => GetText(797);
	public string? PublishTrdIndicator => GetText(852);
	public FixNumber? ShortSaleReason => GetNumber(853);
}

/// <summary>FIX 4.4 OrderMassStatusRequest, MsgType AF.</summary>
public sealed class OrderMassStatusRequest : FixMessage
{
	internal OrderMassStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AF", header, body, trailer)
	{
	}

	public string? MassStatusReqID => GetText(584);
	public FixNumber? MassStatusReqType => GetNumber(585);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? UnderlyingSymbol => GetText(311);
	public string? UnderlyingSymbolSfx => GetText(312);
	public string? UnderlyingSecurityID => GetText(309);
	public string? UnderlyingSecurityIDSource => GetText(305);
	public IReadOnlyList<FixFieldSet> UndSecAltIDGrp => GetGroup(457);
	public FixNumber? UnderlyingProduct => GetNumber(462);
	public string? UnderlyingCFICode => GetText(463);
	public string? UnderlyingSecurityType => GetText(310);
	public string? UnderlyingSecuritySubType => GetText(763);
	public string? UnderlyingMaturityMonthYear => GetText(313);
	public string? UnderlyingMaturityDate => GetText(542);
	public FixNumber? UnderlyingPutOrCall => GetNumber(315);
	public string? UnderlyingCouponPaymentDate => GetText(241);
	public string? UnderlyingIssueDate => GetText(242);
	public string? UnderlyingRepoCollateralSecurityType => GetText(243);
	public FixNumber? UnderlyingRepurchaseTerm => GetNumber(244);
	public FixNumber? UnderlyingRepurchaseRate => GetNumber(245);
	public FixNumber? UnderlyingFactor => GetNumber(246);
	public string? UnderlyingCreditRating => GetText(256);
	public string? UnderlyingInstrRegistry => GetText(595);
	public string? UnderlyingCountryOfIssue => GetText(592);
	public string? UnderlyingStateOrProvinceOfIssue => GetText(593);
	public string? UnderlyingLocaleOfIssue => GetText(594);
	public string? UnderlyingRedemptionDate => GetText(247);
	public FixNumber? UnderlyingStrikePrice => GetNumber(316);
	public string? UnderlyingStrikeCurrency => GetText(941);
	public string? UnderlyingOptAttribute => GetText(317);
	public FixNumber? UnderlyingContractMultiplier => GetNumber(436);
	public FixNumber? UnderlyingCouponRate => GetNumber(435);
	public string? UnderlyingSecurityExchange => GetText(308);
	public string? UnderlyingIssuer => GetText(306);
	public FixNumber? EncodedUnderlyingIssuerLen => GetNumber(362);
	public string? EncodedUnderlyingIssuer => GetText(363);
	public string? UnderlyingSecurityDesc => GetText(307);
	public FixNumber? EncodedUnderlyingSecurityDescLen => GetNumber(364);
	public string? EncodedUnderlyingSecurityDesc => GetText(365);
	public string? UnderlyingCPProgram => GetText(877);
	public string? UnderlyingCPRegType => GetText(878);
	public string? UnderlyingCurrency => GetText(318);
	public FixNumber? UnderlyingQty => GetNumber(879);
	public FixNumber? UnderlyingPx => GetNumber(810);
	public FixNumber? UnderlyingDirtyPrice => GetNumber(882);
	public FixNumber? UnderlyingEndPrice => GetNumber(883);
	public FixNumber? UnderlyingStartValue => GetNumber(884);
	public FixNumber? UnderlyingCurrentValue => GetNumber(885);
	public FixNumber? UnderlyingEndValue => GetNumber(886);
	public IReadOnlyList<FixFieldSet> UnderlyingStipulations => GetGroup(887);
	public string? Side => GetText(54);
}

/// <summary>FIX 4.4 QuoteRequestReject, MsgType AG.</summary>
public sealed class QuoteRequestReject : FixMessage
{
	internal QuoteRequestReject(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AG", header, body, trailer)
	{
	}

	public string? QuoteReqID => GetText(131);
	public string? RFQReqID => GetText(644);
	public FixNumber? QuoteRequestRejectReason => GetNumber(658);
	public IReadOnlyList<FixFieldSet> QuotReqRjctGrp => GetGroup(146);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 RFQRequest, MsgType AH.</summary>
public sealed class RFQRequest : FixMessage
{
	internal RFQRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AH", header, body, trailer)
	{
	}

	public string? RFQReqID => GetText(644);
	public IReadOnlyList<FixFieldSet> RFQReqGrp => GetGroup(146);
	public string? SubscriptionRequestType => GetText(263);
}

/// <summary>FIX 4.4 QuoteStatusReport, MsgType AI.</summary>
public sealed class QuoteStatusReport : FixMessage
{
	internal QuoteStatusReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AI", header, body, trailer)
	{
	}

	public string? QuoteStatusReqID => GetText(649);
	public string? QuoteReqID => GetText(131);
	public string? QuoteID => GetText(117);
	public string? QuoteRespID => GetText(693);
	public FixNumber? QuoteType => GetNumber(537);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public IReadOnlyList<FixFieldSet> LegQuotStatGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
	public string? ExpireTime => GetText(126);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? BidPx => GetNumber(132);
	public FixNumber? OfferPx => GetNumber(133);
	public FixNumber? MktBidPx => GetNumber(645);
	public FixNumber? MktOfferPx => GetNumber(646);
	public FixNumber? MinBidSize => GetNumber(647);
	public FixNumber? BidSize => GetNumber(134);
	public FixNumber? MinOfferSize => GetNumber(648);
	public FixNumber? OfferSize => GetNumber(135);
	public string? ValidUntilTime => GetText(62);
	public FixNumber? BidSpotRate => GetNumber(188);
	public FixNumber? OfferSpotRate => GetNumber(190);
	public FixNumber? BidForwardPoints => GetNumber(189);
	public FixNumber? OfferForwardPoints => GetNumber(191);
	public FixNumber? MidPx => GetNumber(631);
	public FixNumber? BidYield => GetNumber(632);
	public FixNumber? MidYield => GetNumber(633);
	public FixNumber? OfferYield => GetNumber(634);
	public string? TransactTime => GetText(60);
	public string? OrdType => GetText(40);
	public FixNumber? BidForwardPoints2 => GetNumber(642);
	public FixNumber? OfferForwardPoints2 => GetNumber(643);
	public FixNumber? SettlCurrBidFxRate => GetNumber(656);
	public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
	public string? SettlCurrFxRateCalc => GetText(156);
	public string? CommType => GetText(13);
	public FixNumber? Commission => GetNumber(12);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ExDestination => GetText(100);
	public FixNumber? QuoteStatus => GetNumber(297);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 QuoteResponse, MsgType AJ.</summary>
public sealed class QuoteResponse : FixMessage
{
	internal QuoteResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AJ", header, body, trailer)
	{
	}

	public string? QuoteRespID => GetText(693);
	public string? QuoteID => GetText(117);
	public FixNumber? QuoteRespType => GetNumber(694);
	public string? ClOrdID => GetText(11);
	public string? OrderCapacity => GetText(528);
	public string? IOIID => GetText(23);
	public FixNumber? QuoteType => GetNumber(537);
	public IReadOnlyList<FixFieldSet> QuotQualGrp => GetGroup(735);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? Side => GetText(54);
	public FixNumber? OrderQty => GetNumber(38);
	public FixNumber? CashOrderQty => GetNumber(152);
	public FixNumber? OrderPercent => GetNumber(516);
	public string? RoundingDirection => GetText(468);
	public FixNumber? RoundingModulus => GetNumber(469);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public string? SettlDate2 => GetText(193);
	public FixNumber? OrderQty2 => GetNumber(192);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public IReadOnlyList<FixFieldSet> LegQuotGrp => GetGroup(555);
	public FixNumber? BidPx => GetNumber(132);
	public FixNumber? OfferPx => GetNumber(133);
	public FixNumber? MktBidPx => GetNumber(645);
	public FixNumber? MktOfferPx => GetNumber(646);
	public FixNumber? MinBidSize => GetNumber(647);
	public FixNumber? BidSize => GetNumber(134);
	public FixNumber? MinOfferSize => GetNumber(648);
	public FixNumber? OfferSize => GetNumber(135);
	public string? ValidUntilTime => GetText(62);
	public FixNumber? BidSpotRate => GetNumber(188);
	public FixNumber? OfferSpotRate => GetNumber(190);
	public FixNumber? BidForwardPoints => GetNumber(189);
	public FixNumber? OfferForwardPoints => GetNumber(191);
	public FixNumber? MidPx => GetNumber(631);
	public FixNumber? BidYield => GetNumber(632);
	public FixNumber? MidYield => GetNumber(633);
	public FixNumber? OfferYield => GetNumber(634);
	public string? TransactTime => GetText(60);
	public string? OrdType => GetText(40);
	public FixNumber? BidForwardPoints2 => GetNumber(642);
	public FixNumber? OfferForwardPoints2 => GetNumber(643);
	public FixNumber? SettlCurrBidFxRate => GetNumber(656);
	public FixNumber? SettlCurrOfferFxRate => GetNumber(657);
	public string? SettlCurrFxRateCalc => GetText(156);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? ExDestination => GetText(100);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
}

/// <summary>FIX 4.4 Confirmation, MsgType AK.</summary>
public sealed class Confirmation : FixMessage
{
	internal Confirmation(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AK", header, body, trailer)
	{
	}

	public string? ConfirmID => GetText(664);
	public string? ConfirmRefID => GetText(772);
	public string? ConfirmReqID => GetText(859);
	public FixNumber? ConfirmTransType => GetNumber(666);
	public FixNumber? ConfirmType => GetNumber(773);
	public string? CopyMsgIndicator => GetText(797);
	public string? LegalConfirm => GetText(650);
	public FixNumber? ConfirmStatus => GetNumber(665);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
	public string? AllocID => GetText(70);
	public string? SecondaryAllocID => GetText(793);
	public string? IndividualAllocID => GetText(467);
	public string? TransactTime => GetText(60);
	public string? TradeDate => GetText(75);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? AllocQty => GetNumber(80);
	public FixNumber? QtyType => GetNumber(854);
	public string? Side => GetText(54);
	public string? Currency => GetText(15);
	public string? LastMkt => GetText(30);
	public IReadOnlyList<FixFieldSet> CpctyConfGrp => GetGroup(862);
	public string? AllocAccount => GetText(79);
	public FixNumber? AllocAcctIDSource => GetNumber(661);
	public FixNumber? AllocAccountType => GetNumber(798);
	public FixNumber? AvgPx => GetNumber(6);
	public FixNumber? AvgPxPrecision => GetNumber(74);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AvgParPx => GetNumber(860);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public FixNumber? ReportedPx => GetNumber(861);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public string? ProcessCode => GetText(81);
	public FixNumber? GrossTradeAmt => GetNumber(381);
	public FixNumber? NumDaysInterest => GetNumber(157);
	public string? ExDate => GetText(230);
	public FixNumber? AccruedInterestRate => GetNumber(158);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? InterestAtMaturity => GetNumber(738);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Concession => GetNumber(238);
	public FixNumber? TotalTakedown => GetNumber(237);
	public FixNumber? NetMoney => GetNumber(118);
	public FixNumber? MaturityNetMoney => GetNumber(890);
	public FixNumber? SettlCurrAmt => GetNumber(119);
	public string? SettlCurrency => GetText(120);
	public FixNumber? SettlCurrFxRate => GetNumber(155);
	public string? SettlCurrFxRateCalc => GetText(156);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public FixNumber? SettlDeliveryType => GetNumber(172);
	public FixNumber? StandInstDbType => GetNumber(169);
	public string? StandInstDbName => GetText(170);
	public string? StandInstDbID => GetText(171);
	public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
	public FixNumber? Commission => GetNumber(12);
	public string? CommType => GetText(13);
	public string? CommCurrency => GetText(479);
	public string? FundRenewWaiv => GetText(497);
	public FixNumber? SharedCommission => GetNumber(858);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
}

/// <summary>FIX 4.4 PositionMaintenanceRequest, MsgType AL.</summary>
public sealed class PositionMaintenanceRequest : FixMessage
{
	internal PositionMaintenanceRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AL", header, body, trailer)
	{
	}

	public string? PosReqID => GetText(710);
	public FixNumber? PosTransType => GetNumber(709);
	public FixNumber? PosMaintAction => GetNumber(712);
	public string? OrigPosReqRefID => GetText(713);
	public string? PosMaintRptRefID => GetText(714);
	public string? ClearingBusinessDate => GetText(715);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
	public FixNumber? AdjustmentType => GetNumber(718);
	public string? ContraryInstructionIndicator => GetText(719);
	public string? PriorSpreadIndicator => GetText(720);
	public FixNumber? ThresholdAmount => GetNumber(834);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 PositionMaintenanceReport, MsgType AM.</summary>
public sealed class PositionMaintenanceReport : FixMessage
{
	internal PositionMaintenanceReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AM", header, body, trailer)
	{
	}

	public string? PosMaintRptID => GetText(721);
	public FixNumber? PosTransType => GetNumber(709);
	public string? PosReqID => GetText(710);
	public FixNumber? PosMaintAction => GetNumber(712);
	public string? OrigPosReqRefID => GetText(713);
	public FixNumber? PosMaintStatus => GetNumber(722);
	public FixNumber? PosMaintResult => GetNumber(723);
	public string? ClearingBusinessDate => GetText(715);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
	public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
	public FixNumber? AdjustmentType => GetNumber(718);
	public FixNumber? ThresholdAmount => GetNumber(834);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 RequestForPositions, MsgType AN.</summary>
public sealed class RequestForPositions : FixMessage
{
	internal RequestForPositions(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AN", header, body, trailer)
	{
	}

	public string? PosReqID => GetText(710);
	public FixNumber? PosReqType => GetNumber(724);
	public string? MatchStatus => GetText(573);
	public string? SubscriptionRequestType => GetText(263);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? ClearingBusinessDate => GetText(715);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public IReadOnlyList<FixFieldSet> TrdgSesGrp => GetGroup(386);
	public string? TransactTime => GetText(60);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 RequestForPositionsAck, MsgType AO.</summary>
public sealed class RequestForPositionsAck : FixMessage
{
	internal RequestForPositionsAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AO", header, body, trailer)
	{
	}

	public string? PosMaintRptID => GetText(721);
	public string? PosReqID => GetText(710);
	public FixNumber? TotalNumPosReports => GetNumber(727);
	public string? UnsolicitedIndicator => GetText(325);
	public FixNumber? PosReqResult => GetNumber(728);
	public FixNumber? PosReqStatus => GetNumber(729);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 PositionReport, MsgType AP.</summary>
public sealed class PositionReport : FixMessage
{
	internal PositionReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AP", header, body, trailer)
	{
	}

	public string? PosMaintRptID => GetText(721);
	public string? PosReqID => GetText(710);
	public FixNumber? PosReqType => GetNumber(724);
	public string? SubscriptionRequestType => GetText(263);
	public FixNumber? TotalNumPosReports => GetNumber(727);
	public string? UnsolicitedIndicator => GetText(325);
	public FixNumber? PosReqResult => GetNumber(728);
	public string? ClearingBusinessDate => GetText(715);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public FixNumber? SettlPrice => GetNumber(730);
	public FixNumber? SettlPriceType => GetNumber(731);
	public FixNumber? PriorSettlPrice => GetNumber(734);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> PosUndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
	public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
	public string? RegistStatus => GetText(506);
	public string? DeliveryDate => GetText(743);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 TradeCaptureReportRequestAck, MsgType AQ.</summary>
public sealed class TradeCaptureReportRequestAck : FixMessage
{
	internal TradeCaptureReportRequestAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AQ", header, body, trailer)
	{
	}

	public string? TradeRequestID => GetText(568);
	public FixNumber? TradeRequestType => GetNumber(569);
	public string? SubscriptionRequestType => GetText(263);
	public FixNumber? TotNumTradeReports => GetNumber(748);
	public FixNumber? TradeRequestResult => GetNumber(749);
	public FixNumber? TradeRequestStatus => GetNumber(750);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public string? MultiLegReportingType => GetText(442);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 TradeCaptureReportAck, MsgType AR.</summary>
public sealed class TradeCaptureReportAck : FixMessage
{
	internal TradeCaptureReportAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AR", header, body, trailer)
	{
	}

	public string? TradeReportID => GetText(571);
	public FixNumber? TradeReportTransType => GetNumber(487);
	public FixNumber? TradeReportType => GetNumber(856);
	public FixNumber? TrdType => GetNumber(828);
	public FixNumber? TrdSubType => GetNumber(829);
	public FixNumber? SecondaryTrdType => GetNumber(855);
	public string? TransferReason => GetText(830);
	public string? ExecType => GetText(150);
	public string? TradeReportRefID => GetText(572);
	public string? SecondaryTradeReportRefID => GetText(881);
	public FixNumber? TrdRptStatus => GetNumber(939);
	public FixNumber? TradeReportRejectReason => GetNumber(751);
	public string? SecondaryTradeReportID => GetText(818);
	public string? SubscriptionRequestType => GetText(263);
	public string? TradeLinkID => GetText(820);
	public string? TrdMatchID => GetText(880);
	public string? ExecID => GetText(17);
	public string? SecondaryExecID => GetText(527);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public IReadOnlyList<FixFieldSet> TrdInstrmtLegGrp => GetGroup(555);
	public string? ClearingFeeIndicator => GetText(635);
	public string? OrderCapacity => GetText(528);
	public string? OrderRestrictions => GetText(529);
	public FixNumber? CustOrderCapacity => GetNumber(582);
	public string? Account => GetText(1);
	public FixNumber? AcctIDSource => GetNumber(660);
	public FixNumber? AccountType => GetNumber(581);
	public string? PositionEffect => GetText(77);
	public string? PreallocMethod => GetText(591);
	public IReadOnlyList<FixFieldSet> TrdAllocGrp => GetGroup(78);
}

/// <summary>FIX 4.4 AllocationReport, MsgType AS.</summary>
public sealed class AllocationReport : FixMessage
{
	internal AllocationReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AS", header, body, trailer)
	{
	}

	public string? AllocReportID => GetText(755);
	public string? AllocID => GetText(70);
	public string? AllocTransType => GetText(71);
	public string? AllocReportRefID => GetText(795);
	public FixNumber? AllocCancReplaceReason => GetNumber(796);
	public string? SecondaryAllocID => GetText(793);
	public FixNumber? AllocReportType => GetNumber(794);
	public FixNumber? AllocStatus => GetNumber(87);
	public FixNumber? AllocRejCode => GetNumber(88);
	public string? RefAllocID => GetText(72);
	public FixNumber? AllocIntermedReqType => GetNumber(808);
	public string? AllocLinkID => GetText(196);
	public FixNumber? AllocLinkType => GetNumber(197);
	public string? BookingRefID => GetText(466);
	public FixNumber? AllocNoOrdersType => GetNumber(857);
	public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
	public IReadOnlyList<FixFieldSet> ExecAllocGrp => GetGroup(124);
	public string? PreviouslyReported => GetText(570);
	public string? ReversalIndicator => GetText(700);
	public string? MatchType => GetText(574);
	public string? Side => GetText(54);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public FixNumber? DeliveryForm => GetNumber(668);
	public FixNumber? PctAtRisk => GetNumber(869);
	public IReadOnlyList<FixFieldSet> AttrbGrp => GetGroup(870);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? LastMkt => GetText(30);
	public string? TradeOriginationDate => GetText(229);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AvgPx => GetNumber(6);
	public FixNumber? AvgParPx => GetNumber(860);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public string? Currency => GetText(15);
	public FixNumber? AvgPxPrecision => GetNumber(74);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public string? SettlType => GetText(63);
	public string? SettlDate => GetText(64);
	public FixNumber? BookingType => GetNumber(775);
	public FixNumber? GrossTradeAmt => GetNumber(381);
	public FixNumber? Concession => GetNumber(238);
	public FixNumber? TotalTakedown => GetNumber(237);
	public FixNumber? NetMoney => GetNumber(118);
	public string? PositionEffect => GetText(77);
	public string? AutoAcceptIndicator => GetText(754);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public FixNumber? NumDaysInterest => GetNumber(157);
	public FixNumber? AccruedInterestRate => GetNumber(158);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? TotalAccruedInterestAmt => GetNumber(540);
	public FixNumber? InterestAtMaturity => GetNumber(738);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public string? LegalConfirm => GetText(650);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? YieldType => GetText(235);
	public FixNumber? Yield => GetNumber(236);
	public string? YieldCalcDate => GetText(701);
	public string? YieldRedemptionDate => GetText(696);
	public FixNumber? YieldRedemptionPrice => GetNumber(697);
	public FixNumber? YieldRedemptionPriceType => GetNumber(698);
	public FixNumber? TotNoAllocs => GetNumber(892);
	public string? LastFragment => GetText(893);
	public IReadOnlyList<FixFieldSet> AllocGrp => GetGroup(78);
}

/// <summary>FIX 4.4 AllocationReportAck, MsgType AT.</summary>
public sealed class AllocationReportAck : FixMessage
{
	internal AllocationReportAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AT", header, body, trailer)
	{
	}

	public string? AllocReportID => GetText(755);
	public string? AllocID => GetText(70);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? SecondaryAllocID => GetText(793);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public FixNumber? AllocStatus => GetNumber(87);
	public FixNumber? AllocRejCode => GetNumber(88);
	public FixNumber? AllocReportType => GetNumber(794);
	public FixNumber? AllocIntermedReqType => GetNumber(808);
	public string? MatchStatus => GetText(573);
	public FixNumber? Product => GetNumber(460);
	public string? SecurityType => GetText(167);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
	public IReadOnlyList<FixFieldSet> AllocAckGrp => GetGroup(78);
}

/// <summary>FIX 4.4 ConfirmationAck, MsgType AU.</summary>
public sealed class ConfirmationAck : FixMessage
{
	internal ConfirmationAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AU", header, body, trailer)
	{
	}

	public string? ConfirmID => GetText(664);
	public string? TradeDate => GetText(75);
	public string? TransactTime => GetText(60);
	public FixNumber? AffirmStatus => GetNumber(940);
	public FixNumber? ConfirmRejReason => GetNumber(774);
	public string? MatchStatus => GetText(573);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 SettlementInstructionRequest, MsgType AV.</summary>
public sealed class SettlementInstructionRequest : FixMessage
{
	internal SettlementInstructionRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AV", header, body, trailer)
	{
	}

	public string? SettlInstReqID => GetText(791);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? AllocAccount => GetText(79);
	public FixNumber? AllocAcctIDSource => GetNumber(661);
	public string? Side => GetText(54);
	public FixNumber? Product => GetNumber(460);
	public string? SecurityType => GetText(167);
	public string? CFICode => GetText(461);
	public string? EffectiveTime => GetText(168);
	public string? ExpireTime => GetText(126);
	public string? LastUpdateTime => GetText(779);
	public FixNumber? StandInstDbType => GetNumber(169);
	public string? StandInstDbName => GetText(170);
	public string? StandInstDbID => GetText(171);
}

/// <summary>FIX 4.4 AssignmentReport, MsgType AW.</summary>
public sealed class AssignmentReport : FixMessage
{
	internal AssignmentReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AW", header, body, trailer)
	{
	}

	public string? AsgnRptID => GetText(833);
	public FixNumber? TotNumAssignmentReports => GetNumber(832);
	public string? LastRptRequested => GetText(912);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public IReadOnlyList<FixFieldSet> PositionQty => GetGroup(702);
	public IReadOnlyList<FixFieldSet> PositionAmountData => GetGroup(753);
	public FixNumber? ThresholdAmount => GetNumber(834);
	public FixNumber? SettlPrice => GetNumber(730);
	public FixNumber? SettlPriceType => GetNumber(731);
	public FixNumber? UnderlyingSettlPrice => GetNumber(732);
	public string? ExpireDate => GetText(432);
	public string? AssignmentMethod => GetText(744);
	public FixNumber? AssignmentUnit => GetNumber(745);
	public FixNumber? OpenInterest => GetNumber(746);
	public string? ExerciseMethod => GetText(747);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 CollateralRequest, MsgType AX.</summary>
public sealed class CollateralRequest : FixMessage
{
	internal CollateralRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AX", header, body, trailer)
	{
	}

	public string? CollReqID => GetText(894);
	public FixNumber? CollAsgnReason => GetNumber(895);
	public string? TransactTime => GetText(60);
	public string? ExpireTime => GetText(126);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
	public FixNumber? MarginExcess => GetNumber(899);
	public FixNumber? TotalNetValue => GetNumber(900);
	public FixNumber? CashOutstanding => GetNumber(901);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Side => GetText(54);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 CollateralAssignment, MsgType AY.</summary>
public sealed class CollateralAssignment : FixMessage
{
	internal CollateralAssignment(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AY", header, body, trailer)
	{
	}

	public string? CollAsgnID => GetText(902);
	public string? CollReqID => GetText(894);
	public FixNumber? CollAsgnReason => GetNumber(895);
	public FixNumber? CollAsgnTransType => GetNumber(903);
	public string? CollAsgnRefID => GetText(907);
	public string? TransactTime => GetText(60);
	public string? ExpireTime => GetText(126);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
	public FixNumber? MarginExcess => GetNumber(899);
	public FixNumber? TotalNetValue => GetNumber(900);
	public FixNumber? CashOutstanding => GetNumber(901);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Side => GetText(54);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public FixNumber? SettlDeliveryType => GetNumber(172);
	public FixNumber? StandInstDbType => GetNumber(169);
	public string? StandInstDbName => GetText(170);
	public string? StandInstDbID => GetText(171);
	public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 CollateralResponse, MsgType AZ.</summary>
public sealed class CollateralResponse : FixMessage
{
	internal CollateralResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "AZ", header, body, trailer)
	{
	}

	public string? CollRespID => GetText(904);
	public string? CollAsgnID => GetText(902);
	public string? CollReqID => GetText(894);
	public FixNumber? CollAsgnReason => GetNumber(895);
	public FixNumber? CollAsgnTransType => GetNumber(903);
	public FixNumber? CollAsgnRespType => GetNumber(905);
	public FixNumber? CollAsgnRejectReason => GetNumber(906);
	public string? TransactTime => GetText(60);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtCollGrp => GetGroup(711);
	public FixNumber? MarginExcess => GetNumber(899);
	public FixNumber? TotalNetValue => GetNumber(900);
	public FixNumber? CashOutstanding => GetNumber(901);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Side => GetText(54);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 CollateralReport, MsgType BA.</summary>
public sealed class CollateralReport : FixMessage
{
	internal CollateralReport(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BA", header, body, trailer)
	{
	}

	public string? CollRptID => GetText(908);
	public string? CollInquiryID => GetText(909);
	public FixNumber? CollStatus => GetNumber(910);
	public FixNumber? TotNumReports => GetNumber(911);
	public string? LastRptRequested => GetText(912);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? MarginExcess => GetNumber(899);
	public FixNumber? TotalNetValue => GetNumber(900);
	public FixNumber? CashOutstanding => GetNumber(901);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Side => GetText(54);
	public IReadOnlyList<FixFieldSet> MiscFeesGrp => GetGroup(136);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public FixNumber? SettlDeliveryType => GetNumber(172);
	public FixNumber? StandInstDbType => GetNumber(169);
	public string? StandInstDbName => GetText(170);
	public string? StandInstDbID => GetText(171);
	public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 CollateralInquiry, MsgType BB.</summary>
public sealed class CollateralInquiry : FixMessage
{
	internal CollateralInquiry(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BB", header, body, trailer)
	{
	}

	public string? CollInquiryID => GetText(909);
	public IReadOnlyList<FixFieldSet> CollInqQualGrp => GetGroup(938);
	public string? SubscriptionRequestType => GetText(263);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public FixNumber? MarginExcess => GetNumber(899);
	public FixNumber? TotalNetValue => GetNumber(900);
	public FixNumber? CashOutstanding => GetNumber(901);
	public IReadOnlyList<FixFieldSet> TrdRegTimestamps => GetGroup(768);
	public string? Side => GetText(54);
	public FixNumber? Price => GetNumber(44);
	public FixNumber? PriceType => GetNumber(423);
	public FixNumber? AccruedInterestAmt => GetNumber(159);
	public FixNumber? EndAccruedInterestAmt => GetNumber(920);
	public FixNumber? StartCash => GetNumber(921);
	public FixNumber? EndCash => GetNumber(922);
	public FixNumber? Spread => GetNumber(218);
	public string? BenchmarkCurveCurrency => GetText(220);
	public string? BenchmarkCurveName => GetText(221);
	public string? BenchmarkCurvePoint => GetText(222);
	public FixNumber? BenchmarkPrice => GetNumber(662);
	public FixNumber? BenchmarkPriceType => GetNumber(663);
	public string? BenchmarkSecurityID => GetText(699);
	public string? BenchmarkSecurityIDSource => GetText(761);
	public IReadOnlyList<FixFieldSet> Stipulations => GetGroup(232);
	public FixNumber? SettlDeliveryType => GetNumber(172);
	public FixNumber? StandInstDbType => GetNumber(169);
	public string? StandInstDbName => GetText(170);
	public string? StandInstDbID => GetText(171);
	public IReadOnlyList<FixFieldSet> DlvyInstGrp => GetGroup(85);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 NetworkCounterpartySystemStatusRequest, MsgType BC.</summary>
public sealed class NetworkCounterpartySystemStatusRequest : FixMessage
{
	internal NetworkCounterpartySystemStatusRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BC", header, body, trailer)
	{
	}

	public FixNumber? NetworkRequestType => GetNumber(935);
	public string? NetworkRequestID => GetText(933);
	public IReadOnlyList<FixFieldSet> CompIDReqGrp => GetGroup(936);
}

/// <summary>FIX 4.4 NetworkCounterpartySystemStatusResponse, MsgType BD.</summary>
public sealed class NetworkCounterpartySystemStatusResponse : FixMessage
{
	internal NetworkCounterpartySystemStatusResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BD", header, body, trailer)
	{
	}

	public FixNumber? NetworkStatusResponseType => GetNumber(937);
	public string? NetworkRequestID => GetText(933);
	public string? NetworkResponseID => GetText(932);
	public string? LastNetworkResponseID => GetText(934);
	public IReadOnlyList<FixFieldSet> CompIDStatGrp => GetGroup(936);
}

/// <summary>FIX 4.4 UserRequest, MsgType BE.</summary>
public sealed class UserRequest : FixMessage
{
	internal UserRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BE", header, body, trailer)
	{
	}

	public string? UserRequestID => GetText(923);
	public FixNumber? UserRequestType => GetNumber(924);
	public string? Username => GetText(553);
	public string? Password => GetText(554);
	public string? NewPassword => GetText(925);
	public FixNumber? RawDataLength => GetNumber(95);
	public string? RawData => GetText(96);
}

/// <summary>FIX 4.4 UserResponse, MsgType BF.</summary>
public sealed class UserResponse : FixMessage
{
	internal UserResponse(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BF", header, body, trailer)
	{
	}

	public string? UserRequestID => GetText(923);
	public string? Username => GetText(553);
	public FixNumber? UserStatus => GetNumber(926);
	public string? UserStatusText => GetText(927);
}

/// <summary>FIX 4.4 CollateralInquiryAck, MsgType BG.</summary>
public sealed class CollateralInquiryAck : FixMessage
{
	internal CollateralInquiryAck(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BG", header, body, trailer)
	{
	}

	public string? CollInquiryID => GetText(909);
	public FixNumber? CollInquiryStatus => GetNumber(945);
	public FixNumber? CollInquiryResult => GetNumber(946);
	public IReadOnlyList<FixFieldSet> CollInqQualGrp => GetGroup(938);
	public FixNumber? TotNumReports => GetNumber(911);
	public IReadOnlyList<FixFieldSet> Parties => GetGroup(453);
	public string? Account => GetText(1);
	public FixNumber? AccountType => GetNumber(581);
	public string? ClOrdID => GetText(11);
	public string? OrderID => GetText(37);
	public string? SecondaryOrderID => GetText(198);
	public string? SecondaryClOrdID => GetText(526);
	public IReadOnlyList<FixFieldSet> ExecCollGrp => GetGroup(124);
	public IReadOnlyList<FixFieldSet> TrdCollGrp => GetGroup(897);
	public string? Symbol => GetText(55);
	public string? SymbolSfx => GetText(65);
	public string? SecurityID => GetText(48);
	public string? SecurityIDSource => GetText(22);
	public IReadOnlyList<FixFieldSet> SecAltIDGrp => GetGroup(454);
	public FixNumber? Product => GetNumber(460);
	public string? CFICode => GetText(461);
	public string? SecurityType => GetText(167);
	public string? SecuritySubType => GetText(762);
	public string? MaturityMonthYear => GetText(200);
	public string? MaturityDate => GetText(541);
	public FixNumber? PutOrCall => GetNumber(201);
	public string? CouponPaymentDate => GetText(224);
	public string? IssueDate => GetText(225);
	public string? RepoCollateralSecurityType => GetText(239);
	public FixNumber? RepurchaseTerm => GetNumber(226);
	public FixNumber? RepurchaseRate => GetNumber(227);
	public FixNumber? Factor => GetNumber(228);
	public string? CreditRating => GetText(255);
	public string? InstrRegistry => GetText(543);
	public string? CountryOfIssue => GetText(470);
	public string? StateOrProvinceOfIssue => GetText(471);
	public string? LocaleOfIssue => GetText(472);
	public string? RedemptionDate => GetText(240);
	public FixNumber? StrikePrice => GetNumber(202);
	public string? StrikeCurrency => GetText(947);
	public string? OptAttribute => GetText(206);
	public FixNumber? ContractMultiplier => GetNumber(231);
	public FixNumber? CouponRate => GetNumber(223);
	public string? SecurityExchange => GetText(207);
	public string? Issuer => GetText(106);
	public FixNumber? EncodedIssuerLen => GetNumber(348);
	public string? EncodedIssuer => GetText(349);
	public string? SecurityDesc => GetText(107);
	public FixNumber? EncodedSecurityDescLen => GetNumber(350);
	public string? EncodedSecurityDesc => GetText(351);
	public string? Pool => GetText(691);
	public string? ContractSettlMonth => GetText(667);
	public FixNumber? CPProgram => GetNumber(875);
	public string? CPRegType => GetText(876);
	public IReadOnlyList<FixFieldSet> EvntGrp => GetGroup(864);
	public string? DatedDate => GetText(873);
	public string? InterestAccrualDate => GetText(874);
	public string? AgreementDesc => GetText(913);
	public string? AgreementID => GetText(914);
	public string? AgreementDate => GetText(915);
	public string? AgreementCurrency => GetText(918);
	public FixNumber? TerminationType => GetNumber(788);
	public string? StartDate => GetText(916);
	public string? EndDate => GetText(917);
	public FixNumber? DeliveryType => GetNumber(919);
	public FixNumber? MarginRatio => GetNumber(898);
	public string? SettlDate => GetText(64);
	public FixNumber? Quantity => GetNumber(53);
	public FixNumber? QtyType => GetNumber(854);
	public string? Currency => GetText(15);
	public IReadOnlyList<FixFieldSet> InstrmtLegGrp => GetGroup(555);
	public IReadOnlyList<FixFieldSet> UndInstrmtGrp => GetGroup(711);
	public string? TradingSessionID => GetText(336);
	public string? TradingSessionSubID => GetText(625);
	public string? SettlSessID => GetText(716);
	public string? SettlSessSubID => GetText(717);
	public string? ClearingBusinessDate => GetText(715);
	public FixNumber? ResponseTransportType => GetNumber(725);
	public string? ResponseDestination => GetText(726);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 ConfirmationRequest, MsgType BH.</summary>
public sealed class ConfirmationRequest : FixMessage
{
	internal ConfirmationRequest(string source, FixNode[] header, FixNode[] body, FixNode[] trailer)
		: base(source, "BH", header, body, trailer)
	{
	}

	public string? ConfirmReqID => GetText(859);
	public FixNumber? ConfirmType => GetNumber(773);
	public IReadOnlyList<FixFieldSet> OrdAllocGrp => GetGroup(73);
	public string? AllocID => GetText(70);
	public string? SecondaryAllocID => GetText(793);
	public string? IndividualAllocID => GetText(467);
	public string? TransactTime => GetText(60);
	public string? AllocAccount => GetText(79);
	public FixNumber? AllocAcctIDSource => GetNumber(661);
	public FixNumber? AllocAccountType => GetNumber(798);
	public string? Text => GetText(58);
	public FixNumber? EncodedTextLen => GetNumber(354);
	public string? EncodedText => GetText(355);
}

/// <summary>FIX 4.4 StandardHeader field scope.</summary>
public sealed class StandardHeader : FixFieldSet
{
	internal StandardHeader(string source, FixNode[] fields)
		: base(source, fields)
	{
	}

	public string? BeginString => GetText(8);
	public FixNumber? BodyLength => GetNumber(9);
	public string? MsgType => GetText(35);
	public string? SenderCompID => GetText(49);
	public string? TargetCompID => GetText(56);
	public string? OnBehalfOfCompID => GetText(115);
	public string? DeliverToCompID => GetText(128);
	public FixNumber? SecureDataLen => GetNumber(90);
	public string? SecureData => GetText(91);
	public FixNumber? MsgSeqNum => GetNumber(34);
	public string? SenderSubID => GetText(50);
	public string? SenderLocationID => GetText(142);
	public string? TargetSubID => GetText(57);
	public string? TargetLocationID => GetText(143);
	public string? OnBehalfOfSubID => GetText(116);
	public string? OnBehalfOfLocationID => GetText(144);
	public string? DeliverToSubID => GetText(129);
	public string? DeliverToLocationID => GetText(145);
	public string? PossDupFlag => GetText(43);
	public string? PossResend => GetText(97);
	public string? SendingTime => GetText(52);
	public string? OrigSendingTime => GetText(122);
	public FixNumber? XmlDataLen => GetNumber(212);
	public string? XmlData => GetText(213);
	public string? MessageEncoding => GetText(347);
	public FixNumber? LastMsgSeqNumProcessed => GetNumber(369);
	public IReadOnlyList<FixFieldSet> HopGrp => GetGroup(627);
}

/// <summary>FIX 4.4 StandardTrailer field scope.</summary>
public sealed class StandardTrailer : FixFieldSet
{
	internal StandardTrailer(string source, FixNode[] fields)
		: base(source, fields)
	{
	}

	public FixNumber? SignatureLength => GetNumber(93);
	public string? Signature => GetText(89);
	public string? CheckSum => GetText(10);
}
