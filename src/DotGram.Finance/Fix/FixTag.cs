namespace DotGram.Finance.Fix;

// Written by generate.py from the FIX 4.4 repository; not edited by hand.

/// <summary>The tag of every field of FIX 4.4, named for the field: <c>FixField.Decimal { Tag: FixTag.OrderQty }</c>.</summary>
/// <remarks>A tag FIX 4.4 does not define is its number, <c>(FixTag)25005</c>, and has no name here.</remarks>
public enum FixTag
{
	/// <summary>Account, a <see cref="FixField.Text"/>.</summary>
	Account = 1,

	/// <summary>AdvId, a <see cref="FixField.Text"/>.</summary>
	AdvId = 2,

	/// <summary>AdvRefID, a <see cref="FixField.Text"/>.</summary>
	AdvRefID = 3,

	/// <summary>AdvSide, a <see cref="FixField.Character"/>.</summary>
	AdvSide = 4,

	/// <summary>AdvTransType, a <see cref="FixField.Text"/>.</summary>
	AdvTransType = 5,

	/// <summary>AvgPx, a <see cref="FixField.Decimal"/>.</summary>
	AvgPx = 6,

	/// <summary>BeginSeqNo, a <see cref="FixField.Integer"/>.</summary>
	BeginSeqNo = 7,

	/// <summary>BeginString, a <see cref="FixField.Text"/>.</summary>
	BeginString = 8,

	/// <summary>BodyLength, a <see cref="FixField.Integer"/>.</summary>
	BodyLength = 9,

	/// <summary>CheckSum, a <see cref="FixField.Text"/>.</summary>
	CheckSum = 10,

	/// <summary>ClOrdID, a <see cref="FixField.Text"/>.</summary>
	ClOrdID = 11,

	/// <summary>Commission, a <see cref="FixField.Decimal"/>.</summary>
	Commission = 12,

	/// <summary>CommType, a <see cref="FixField.Character"/>.</summary>
	CommType = 13,

	/// <summary>CumQty, a <see cref="FixField.Decimal"/>.</summary>
	CumQty = 14,

	/// <summary>Currency, a <see cref="FixField.Text"/>.</summary>
	Currency = 15,

	/// <summary>EndSeqNo, a <see cref="FixField.Integer"/>.</summary>
	EndSeqNo = 16,

	/// <summary>ExecID, a <see cref="FixField.Text"/>.</summary>
	ExecID = 17,

	/// <summary>ExecInst, a <see cref="FixField.Multiple"/>.</summary>
	ExecInst = 18,

	/// <summary>ExecRefID, a <see cref="FixField.Text"/>.</summary>
	ExecRefID = 19,

	/// <summary>HandlInst, a <see cref="FixField.Character"/>.</summary>
	HandlInst = 21,

	/// <summary>SecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	SecurityIDSource = 22,

	/// <summary>IOIid, a <see cref="FixField.Text"/>.</summary>
	IOIid = 23,

	/// <summary>IOIQltyInd, a <see cref="FixField.Character"/>.</summary>
	IOIQltyInd = 25,

	/// <summary>IOIRefID, a <see cref="FixField.Text"/>.</summary>
	IOIRefID = 26,

	/// <summary>IOIQty, a <see cref="FixField.Text"/>.</summary>
	IOIQty = 27,

	/// <summary>IOITransType, a <see cref="FixField.Character"/>.</summary>
	IOITransType = 28,

	/// <summary>LastCapacity, a <see cref="FixField.Character"/>.</summary>
	LastCapacity = 29,

	/// <summary>LastMkt, a <see cref="FixField.Text"/>.</summary>
	LastMkt = 30,

	/// <summary>LastPx, a <see cref="FixField.Decimal"/>.</summary>
	LastPx = 31,

	/// <summary>LastQty, a <see cref="FixField.Decimal"/>.</summary>
	LastQty = 32,

	/// <summary>LinesOfText, a <see cref="FixField.Integer"/>.</summary>
	LinesOfText = 33,

	/// <summary>MsgSeqNum, a <see cref="FixField.Integer"/>.</summary>
	MsgSeqNum = 34,

	/// <summary>MsgType, a <see cref="FixField.Text"/>.</summary>
	MsgType = 35,

	/// <summary>NewSeqNo, a <see cref="FixField.Integer"/>.</summary>
	NewSeqNo = 36,

	/// <summary>OrderID, a <see cref="FixField.Text"/>.</summary>
	OrderID = 37,

	/// <summary>OrderQty, a <see cref="FixField.Decimal"/>.</summary>
	OrderQty = 38,

	/// <summary>OrdStatus, a <see cref="FixField.Character"/>.</summary>
	OrdStatus = 39,

	/// <summary>OrdType, a <see cref="FixField.Character"/>.</summary>
	OrdType = 40,

	/// <summary>OrigClOrdID, a <see cref="FixField.Text"/>.</summary>
	OrigClOrdID = 41,

	/// <summary>OrigTime, a <see cref="FixField.Timestamp"/>.</summary>
	OrigTime = 42,

	/// <summary>PossDupFlag, a <see cref="FixField.Boolean"/>.</summary>
	PossDupFlag = 43,

	/// <summary>Price, a <see cref="FixField.Decimal"/>.</summary>
	Price = 44,

	/// <summary>RefSeqNum, a <see cref="FixField.Integer"/>.</summary>
	RefSeqNum = 45,

	/// <summary>SecurityID, a <see cref="FixField.Text"/>.</summary>
	SecurityID = 48,

	/// <summary>SenderCompID, a <see cref="FixField.Text"/>.</summary>
	SenderCompID = 49,

	/// <summary>SenderSubID, a <see cref="FixField.Text"/>.</summary>
	SenderSubID = 50,

	/// <summary>SendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	SendingTime = 52,

	/// <summary>Quantity, a <see cref="FixField.Decimal"/>.</summary>
	Quantity = 53,

	/// <summary>Side, a <see cref="FixField.Character"/>.</summary>
	Side = 54,

	/// <summary>Symbol, a <see cref="FixField.Text"/>.</summary>
	Symbol = 55,

	/// <summary>TargetCompID, a <see cref="FixField.Text"/>.</summary>
	TargetCompID = 56,

	/// <summary>TargetSubID, a <see cref="FixField.Text"/>.</summary>
	TargetSubID = 57,

	/// <summary>Text, a <see cref="FixField.Text"/>.</summary>
	Text = 58,

	/// <summary>TimeInForce, a <see cref="FixField.Character"/>.</summary>
	TimeInForce = 59,

	/// <summary>TransactTime, a <see cref="FixField.Timestamp"/>.</summary>
	TransactTime = 60,

	/// <summary>Urgency, a <see cref="FixField.Character"/>.</summary>
	Urgency = 61,

	/// <summary>ValidUntilTime, a <see cref="FixField.Timestamp"/>.</summary>
	ValidUntilTime = 62,

	/// <summary>SettlType, a <see cref="FixField.Character"/>.</summary>
	SettlType = 63,

	/// <summary>SettlDate, a <see cref="FixField.Date"/>.</summary>
	SettlDate = 64,

	/// <summary>SymbolSfx, a <see cref="FixField.Text"/>.</summary>
	SymbolSfx = 65,

	/// <summary>ListID, a <see cref="FixField.Text"/>.</summary>
	ListID = 66,

	/// <summary>ListSeqNo, a <see cref="FixField.Integer"/>.</summary>
	ListSeqNo = 67,

	/// <summary>TotNoOrders, a <see cref="FixField.Integer"/>.</summary>
	TotNoOrders = 68,

	/// <summary>ListExecInst, a <see cref="FixField.Text"/>.</summary>
	ListExecInst = 69,

	/// <summary>AllocID, a <see cref="FixField.Text"/>.</summary>
	AllocID = 70,

	/// <summary>AllocTransType, a <see cref="FixField.Character"/>.</summary>
	AllocTransType = 71,

	/// <summary>RefAllocID, a <see cref="FixField.Text"/>.</summary>
	RefAllocID = 72,

	/// <summary>NoOrders, a <see cref="FixField.Integer"/>.</summary>
	NoOrders = 73,

	/// <summary>AvgPxPrecision, a <see cref="FixField.Integer"/>.</summary>
	AvgPxPrecision = 74,

	/// <summary>TradeDate, a <see cref="FixField.Date"/>.</summary>
	TradeDate = 75,

	/// <summary>PositionEffect, a <see cref="FixField.Character"/>.</summary>
	PositionEffect = 77,

	/// <summary>NoAllocs, a <see cref="FixField.Integer"/>.</summary>
	NoAllocs = 78,

	/// <summary>AllocAccount, a <see cref="FixField.Text"/>.</summary>
	AllocAccount = 79,

	/// <summary>AllocQty, a <see cref="FixField.Decimal"/>.</summary>
	AllocQty = 80,

	/// <summary>ProcessCode, a <see cref="FixField.Character"/>.</summary>
	ProcessCode = 81,

	/// <summary>NoRpts, a <see cref="FixField.Integer"/>.</summary>
	NoRpts = 82,

	/// <summary>RptSeq, a <see cref="FixField.Integer"/>.</summary>
	RptSeq = 83,

	/// <summary>CxlQty, a <see cref="FixField.Decimal"/>.</summary>
	CxlQty = 84,

	/// <summary>NoDlvyInst, a <see cref="FixField.Integer"/>.</summary>
	NoDlvyInst = 85,

	/// <summary>AllocStatus, a <see cref="FixField.Integer"/>.</summary>
	AllocStatus = 87,

	/// <summary>AllocRejCode, a <see cref="FixField.Integer"/>.</summary>
	AllocRejCode = 88,

	/// <summary>Signature, a <see cref="FixField.Data"/>.</summary>
	Signature = 89,

	/// <summary>SecureDataLen, a <see cref="FixField.Integer"/>.</summary>
	SecureDataLen = 90,

	/// <summary>SecureData, a <see cref="FixField.Data"/>.</summary>
	SecureData = 91,

	/// <summary>SignatureLength, a <see cref="FixField.Integer"/>.</summary>
	SignatureLength = 93,

	/// <summary>EmailType, a <see cref="FixField.Character"/>.</summary>
	EmailType = 94,

	/// <summary>RawDataLength, a <see cref="FixField.Integer"/>.</summary>
	RawDataLength = 95,

	/// <summary>RawData, a <see cref="FixField.Data"/>.</summary>
	RawData = 96,

	/// <summary>PossResend, a <see cref="FixField.Boolean"/>.</summary>
	PossResend = 97,

	/// <summary>EncryptMethod, a <see cref="FixField.Integer"/>.</summary>
	EncryptMethod = 98,

	/// <summary>StopPx, a <see cref="FixField.Decimal"/>.</summary>
	StopPx = 99,

	/// <summary>ExDestination, a <see cref="FixField.Text"/>.</summary>
	ExDestination = 100,

	/// <summary>CxlRejReason, a <see cref="FixField.Integer"/>.</summary>
	CxlRejReason = 102,

	/// <summary>OrdRejReason, a <see cref="FixField.Integer"/>.</summary>
	OrdRejReason = 103,

	/// <summary>IOIQualifier, a <see cref="FixField.Character"/>.</summary>
	IOIQualifier = 104,

	/// <summary>Issuer, a <see cref="FixField.Text"/>.</summary>
	Issuer = 106,

	/// <summary>SecurityDesc, a <see cref="FixField.Text"/>.</summary>
	SecurityDesc = 107,

	/// <summary>HeartBtInt, a <see cref="FixField.Integer"/>.</summary>
	HeartBtInt = 108,

	/// <summary>MinQty, a <see cref="FixField.Decimal"/>.</summary>
	MinQty = 110,

	/// <summary>MaxFloor, a <see cref="FixField.Decimal"/>.</summary>
	MaxFloor = 111,

	/// <summary>TestReqID, a <see cref="FixField.Text"/>.</summary>
	TestReqID = 112,

	/// <summary>ReportToExch, a <see cref="FixField.Boolean"/>.</summary>
	ReportToExch = 113,

	/// <summary>LocateReqd, a <see cref="FixField.Boolean"/>.</summary>
	LocateReqd = 114,

	/// <summary>OnBehalfOfCompID, a <see cref="FixField.Text"/>.</summary>
	OnBehalfOfCompID = 115,

	/// <summary>OnBehalfOfSubID, a <see cref="FixField.Text"/>.</summary>
	OnBehalfOfSubID = 116,

	/// <summary>QuoteID, a <see cref="FixField.Text"/>.</summary>
	QuoteID = 117,

	/// <summary>NetMoney, a <see cref="FixField.Decimal"/>.</summary>
	NetMoney = 118,

	/// <summary>SettlCurrAmt, a <see cref="FixField.Decimal"/>.</summary>
	SettlCurrAmt = 119,

	/// <summary>SettlCurrency, a <see cref="FixField.Text"/>.</summary>
	SettlCurrency = 120,

	/// <summary>ForexReq, a <see cref="FixField.Boolean"/>.</summary>
	ForexReq = 121,

	/// <summary>OrigSendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	OrigSendingTime = 122,

	/// <summary>GapFillFlag, a <see cref="FixField.Boolean"/>.</summary>
	GapFillFlag = 123,

	/// <summary>NoExecs, a <see cref="FixField.Integer"/>.</summary>
	NoExecs = 124,

	/// <summary>ExpireTime, a <see cref="FixField.Timestamp"/>.</summary>
	ExpireTime = 126,

	/// <summary>DKReason, a <see cref="FixField.Character"/>.</summary>
	DKReason = 127,

	/// <summary>DeliverToCompID, a <see cref="FixField.Text"/>.</summary>
	DeliverToCompID = 128,

	/// <summary>DeliverToSubID, a <see cref="FixField.Text"/>.</summary>
	DeliverToSubID = 129,

	/// <summary>IOINaturalFlag, a <see cref="FixField.Boolean"/>.</summary>
	IOINaturalFlag = 130,

	/// <summary>QuoteReqID, a <see cref="FixField.Text"/>.</summary>
	QuoteReqID = 131,

	/// <summary>BidPx, a <see cref="FixField.Decimal"/>.</summary>
	BidPx = 132,

	/// <summary>OfferPx, a <see cref="FixField.Decimal"/>.</summary>
	OfferPx = 133,

	/// <summary>BidSize, a <see cref="FixField.Decimal"/>.</summary>
	BidSize = 134,

	/// <summary>OfferSize, a <see cref="FixField.Decimal"/>.</summary>
	OfferSize = 135,

	/// <summary>NoMiscFees, a <see cref="FixField.Integer"/>.</summary>
	NoMiscFees = 136,

	/// <summary>MiscFeeAmt, a <see cref="FixField.Decimal"/>.</summary>
	MiscFeeAmt = 137,

	/// <summary>MiscFeeCurr, a <see cref="FixField.Text"/>.</summary>
	MiscFeeCurr = 138,

	/// <summary>MiscFeeType, a <see cref="FixField.Text"/>.</summary>
	MiscFeeType = 139,

	/// <summary>PrevClosePx, a <see cref="FixField.Decimal"/>.</summary>
	PrevClosePx = 140,

	/// <summary>ResetSeqNumFlag, a <see cref="FixField.Boolean"/>.</summary>
	ResetSeqNumFlag = 141,

	/// <summary>SenderLocationID, a <see cref="FixField.Text"/>.</summary>
	SenderLocationID = 142,

	/// <summary>TargetLocationID, a <see cref="FixField.Text"/>.</summary>
	TargetLocationID = 143,

	/// <summary>OnBehalfOfLocationID, a <see cref="FixField.Text"/>.</summary>
	OnBehalfOfLocationID = 144,

	/// <summary>DeliverToLocationID, a <see cref="FixField.Text"/>.</summary>
	DeliverToLocationID = 145,

	/// <summary>NoRelatedSym, a <see cref="FixField.Integer"/>.</summary>
	NoRelatedSym = 146,

	/// <summary>Subject, a <see cref="FixField.Text"/>.</summary>
	Subject = 147,

	/// <summary>Headline, a <see cref="FixField.Text"/>.</summary>
	Headline = 148,

	/// <summary>URLLink, a <see cref="FixField.Text"/>.</summary>
	URLLink = 149,

	/// <summary>ExecType, a <see cref="FixField.Character"/>.</summary>
	ExecType = 150,

	/// <summary>LeavesQty, a <see cref="FixField.Decimal"/>.</summary>
	LeavesQty = 151,

	/// <summary>CashOrderQty, a <see cref="FixField.Decimal"/>.</summary>
	CashOrderQty = 152,

	/// <summary>AllocAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	AllocAvgPx = 153,

	/// <summary>AllocNetMoney, a <see cref="FixField.Decimal"/>.</summary>
	AllocNetMoney = 154,

	/// <summary>SettlCurrFxRate, a <see cref="FixField.Decimal"/>.</summary>
	SettlCurrFxRate = 155,

	/// <summary>SettlCurrFxRateCalc, a <see cref="FixField.Character"/>.</summary>
	SettlCurrFxRateCalc = 156,

	/// <summary>NumDaysInterest, a <see cref="FixField.Integer"/>.</summary>
	NumDaysInterest = 157,

	/// <summary>AccruedInterestRate, a <see cref="FixField.Decimal"/>.</summary>
	AccruedInterestRate = 158,

	/// <summary>AccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	AccruedInterestAmt = 159,

	/// <summary>SettlInstMode, a <see cref="FixField.Character"/>.</summary>
	SettlInstMode = 160,

	/// <summary>AllocText, a <see cref="FixField.Text"/>.</summary>
	AllocText = 161,

	/// <summary>SettlInstID, a <see cref="FixField.Text"/>.</summary>
	SettlInstID = 162,

	/// <summary>SettlInstTransType, a <see cref="FixField.Character"/>.</summary>
	SettlInstTransType = 163,

	/// <summary>EmailThreadID, a <see cref="FixField.Text"/>.</summary>
	EmailThreadID = 164,

	/// <summary>SettlInstSource, a <see cref="FixField.Character"/>.</summary>
	SettlInstSource = 165,

	/// <summary>SecurityType, a <see cref="FixField.Text"/>.</summary>
	SecurityType = 167,

	/// <summary>EffectiveTime, a <see cref="FixField.Timestamp"/>.</summary>
	EffectiveTime = 168,

	/// <summary>StandInstDbType, a <see cref="FixField.Integer"/>.</summary>
	StandInstDbType = 169,

	/// <summary>StandInstDbName, a <see cref="FixField.Text"/>.</summary>
	StandInstDbName = 170,

	/// <summary>StandInstDbID, a <see cref="FixField.Text"/>.</summary>
	StandInstDbID = 171,

	/// <summary>SettlDeliveryType, a <see cref="FixField.Integer"/>.</summary>
	SettlDeliveryType = 172,

	/// <summary>BidSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	BidSpotRate = 188,

	/// <summary>BidForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	BidForwardPoints = 189,

	/// <summary>OfferSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	OfferSpotRate = 190,

	/// <summary>OfferForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	OfferForwardPoints = 191,

	/// <summary>OrderQty2, a <see cref="FixField.Decimal"/>.</summary>
	OrderQty2 = 192,

	/// <summary>SettlDate2, a <see cref="FixField.Date"/>.</summary>
	SettlDate2 = 193,

	/// <summary>LastSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	LastSpotRate = 194,

	/// <summary>LastForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	LastForwardPoints = 195,

	/// <summary>AllocLinkID, a <see cref="FixField.Text"/>.</summary>
	AllocLinkID = 196,

	/// <summary>AllocLinkType, a <see cref="FixField.Integer"/>.</summary>
	AllocLinkType = 197,

	/// <summary>SecondaryOrderID, a <see cref="FixField.Text"/>.</summary>
	SecondaryOrderID = 198,

	/// <summary>NoIOIQualifiers, a <see cref="FixField.Integer"/>.</summary>
	NoIOIQualifiers = 199,

	/// <summary>MaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	MaturityMonthYear = 200,

	/// <summary>PutOrCall, a <see cref="FixField.Integer"/>.</summary>
	PutOrCall = 201,

	/// <summary>StrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	StrikePrice = 202,

	/// <summary>CoveredOrUncovered, a <see cref="FixField.Integer"/>.</summary>
	CoveredOrUncovered = 203,

	/// <summary>OptAttribute, a <see cref="FixField.Character"/>.</summary>
	OptAttribute = 206,

	/// <summary>SecurityExchange, a <see cref="FixField.Text"/>.</summary>
	SecurityExchange = 207,

	/// <summary>NotifyBrokerOfCredit, a <see cref="FixField.Boolean"/>.</summary>
	NotifyBrokerOfCredit = 208,

	/// <summary>AllocHandlInst, a <see cref="FixField.Integer"/>.</summary>
	AllocHandlInst = 209,

	/// <summary>MaxShow, a <see cref="FixField.Decimal"/>.</summary>
	MaxShow = 210,

	/// <summary>PegOffsetValue, a <see cref="FixField.Decimal"/>.</summary>
	PegOffsetValue = 211,

	/// <summary>XmlDataLen, a <see cref="FixField.Integer"/>.</summary>
	XmlDataLen = 212,

	/// <summary>XmlData, a <see cref="FixField.Data"/>.</summary>
	XmlData = 213,

	/// <summary>SettlInstRefID, a <see cref="FixField.Text"/>.</summary>
	SettlInstRefID = 214,

	/// <summary>NoRoutingIDs, a <see cref="FixField.Integer"/>.</summary>
	NoRoutingIDs = 215,

	/// <summary>RoutingType, a <see cref="FixField.Integer"/>.</summary>
	RoutingType = 216,

	/// <summary>RoutingID, a <see cref="FixField.Text"/>.</summary>
	RoutingID = 217,

	/// <summary>Spread, a <see cref="FixField.Decimal"/>.</summary>
	Spread = 218,

	/// <summary>BenchmarkCurveCurrency, a <see cref="FixField.Text"/>.</summary>
	BenchmarkCurveCurrency = 220,

	/// <summary>BenchmarkCurveName, a <see cref="FixField.Text"/>.</summary>
	BenchmarkCurveName = 221,

	/// <summary>BenchmarkCurvePoint, a <see cref="FixField.Text"/>.</summary>
	BenchmarkCurvePoint = 222,

	/// <summary>CouponRate, a <see cref="FixField.Decimal"/>.</summary>
	CouponRate = 223,

	/// <summary>CouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	CouponPaymentDate = 224,

	/// <summary>IssueDate, a <see cref="FixField.Date"/>.</summary>
	IssueDate = 225,

	/// <summary>RepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	RepurchaseTerm = 226,

	/// <summary>RepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	RepurchaseRate = 227,

	/// <summary>Factor, a <see cref="FixField.Decimal"/>.</summary>
	Factor = 228,

	/// <summary>TradeOriginationDate, a <see cref="FixField.Date"/>.</summary>
	TradeOriginationDate = 229,

	/// <summary>ExDate, a <see cref="FixField.Date"/>.</summary>
	ExDate = 230,

	/// <summary>ContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	ContractMultiplier = 231,

	/// <summary>NoStipulations, a <see cref="FixField.Integer"/>.</summary>
	NoStipulations = 232,

	/// <summary>StipulationType, a <see cref="FixField.Text"/>.</summary>
	StipulationType = 233,

	/// <summary>StipulationValue, a <see cref="FixField.Text"/>.</summary>
	StipulationValue = 234,

	/// <summary>YieldType, a <see cref="FixField.Text"/>.</summary>
	YieldType = 235,

	/// <summary>Yield, a <see cref="FixField.Decimal"/>.</summary>
	Yield = 236,

	/// <summary>TotalTakedown, a <see cref="FixField.Decimal"/>.</summary>
	TotalTakedown = 237,

	/// <summary>Concession, a <see cref="FixField.Decimal"/>.</summary>
	Concession = 238,

	/// <summary>RepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	RepoCollateralSecurityType = 239,

	/// <summary>RedemptionDate, a <see cref="FixField.Date"/>.</summary>
	RedemptionDate = 240,

	/// <summary>UnderlyingCouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	UnderlyingCouponPaymentDate = 241,

	/// <summary>UnderlyingIssueDate, a <see cref="FixField.Date"/>.</summary>
	UnderlyingIssueDate = 242,

	/// <summary>UnderlyingRepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	UnderlyingRepoCollateralSecurityType = 243,

	/// <summary>UnderlyingRepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	UnderlyingRepurchaseTerm = 244,

	/// <summary>UnderlyingRepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingRepurchaseRate = 245,

	/// <summary>UnderlyingFactor, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingFactor = 246,

	/// <summary>UnderlyingRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	UnderlyingRedemptionDate = 247,

	/// <summary>LegCouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	LegCouponPaymentDate = 248,

	/// <summary>LegIssueDate, a <see cref="FixField.Date"/>.</summary>
	LegIssueDate = 249,

	/// <summary>LegRepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	LegRepoCollateralSecurityType = 250,

	/// <summary>LegRepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	LegRepurchaseTerm = 251,

	/// <summary>LegRepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	LegRepurchaseRate = 252,

	/// <summary>LegFactor, a <see cref="FixField.Decimal"/>.</summary>
	LegFactor = 253,

	/// <summary>LegRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	LegRedemptionDate = 254,

	/// <summary>CreditRating, a <see cref="FixField.Text"/>.</summary>
	CreditRating = 255,

	/// <summary>UnderlyingCreditRating, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCreditRating = 256,

	/// <summary>LegCreditRating, a <see cref="FixField.Text"/>.</summary>
	LegCreditRating = 257,

	/// <summary>TradedFlatSwitch, a <see cref="FixField.Boolean"/>.</summary>
	TradedFlatSwitch = 258,

	/// <summary>BasisFeatureDate, a <see cref="FixField.Date"/>.</summary>
	BasisFeatureDate = 259,

	/// <summary>BasisFeaturePrice, a <see cref="FixField.Decimal"/>.</summary>
	BasisFeaturePrice = 260,

	/// <summary>MDReqID, a <see cref="FixField.Text"/>.</summary>
	MDReqID = 262,

	/// <summary>SubscriptionRequestType, a <see cref="FixField.Character"/>.</summary>
	SubscriptionRequestType = 263,

	/// <summary>MarketDepth, a <see cref="FixField.Integer"/>.</summary>
	MarketDepth = 264,

	/// <summary>MDUpdateType, a <see cref="FixField.Integer"/>.</summary>
	MDUpdateType = 265,

	/// <summary>AggregatedBook, a <see cref="FixField.Boolean"/>.</summary>
	AggregatedBook = 266,

	/// <summary>NoMDEntryTypes, a <see cref="FixField.Integer"/>.</summary>
	NoMDEntryTypes = 267,

	/// <summary>NoMDEntries, a <see cref="FixField.Integer"/>.</summary>
	NoMDEntries = 268,

	/// <summary>MDEntryType, a <see cref="FixField.Character"/>.</summary>
	MDEntryType = 269,

	/// <summary>MDEntryPx, a <see cref="FixField.Decimal"/>.</summary>
	MDEntryPx = 270,

	/// <summary>MDEntrySize, a <see cref="FixField.Decimal"/>.</summary>
	MDEntrySize = 271,

	/// <summary>MDEntryDate, a <see cref="FixField.Date"/>.</summary>
	MDEntryDate = 272,

	/// <summary>MDEntryTime, a <see cref="FixField.Time"/>.</summary>
	MDEntryTime = 273,

	/// <summary>TickDirection, a <see cref="FixField.Character"/>.</summary>
	TickDirection = 274,

	/// <summary>MDMkt, a <see cref="FixField.Text"/>.</summary>
	MDMkt = 275,

	/// <summary>QuoteCondition, a <see cref="FixField.Multiple"/>.</summary>
	QuoteCondition = 276,

	/// <summary>TradeCondition, a <see cref="FixField.Multiple"/>.</summary>
	TradeCondition = 277,

	/// <summary>MDEntryID, a <see cref="FixField.Text"/>.</summary>
	MDEntryID = 278,

	/// <summary>MDUpdateAction, a <see cref="FixField.Character"/>.</summary>
	MDUpdateAction = 279,

	/// <summary>MDEntryRefID, a <see cref="FixField.Text"/>.</summary>
	MDEntryRefID = 280,

	/// <summary>MDReqRejReason, a <see cref="FixField.Character"/>.</summary>
	MDReqRejReason = 281,

	/// <summary>MDEntryOriginator, a <see cref="FixField.Text"/>.</summary>
	MDEntryOriginator = 282,

	/// <summary>LocationID, a <see cref="FixField.Text"/>.</summary>
	LocationID = 283,

	/// <summary>DeskID, a <see cref="FixField.Text"/>.</summary>
	DeskID = 284,

	/// <summary>DeleteReason, a <see cref="FixField.Character"/>.</summary>
	DeleteReason = 285,

	/// <summary>OpenCloseSettlFlag, a <see cref="FixField.Multiple"/>.</summary>
	OpenCloseSettlFlag = 286,

	/// <summary>SellerDays, a <see cref="FixField.Integer"/>.</summary>
	SellerDays = 287,

	/// <summary>MDEntryBuyer, a <see cref="FixField.Text"/>.</summary>
	MDEntryBuyer = 288,

	/// <summary>MDEntrySeller, a <see cref="FixField.Text"/>.</summary>
	MDEntrySeller = 289,

	/// <summary>MDEntryPositionNo, a <see cref="FixField.Integer"/>.</summary>
	MDEntryPositionNo = 290,

	/// <summary>FinancialStatus, a <see cref="FixField.Multiple"/>.</summary>
	FinancialStatus = 291,

	/// <summary>CorporateAction, a <see cref="FixField.Multiple"/>.</summary>
	CorporateAction = 292,

	/// <summary>DefBidSize, a <see cref="FixField.Decimal"/>.</summary>
	DefBidSize = 293,

	/// <summary>DefOfferSize, a <see cref="FixField.Decimal"/>.</summary>
	DefOfferSize = 294,

	/// <summary>NoQuoteEntries, a <see cref="FixField.Integer"/>.</summary>
	NoQuoteEntries = 295,

	/// <summary>NoQuoteSets, a <see cref="FixField.Integer"/>.</summary>
	NoQuoteSets = 296,

	/// <summary>QuoteStatus, a <see cref="FixField.Integer"/>.</summary>
	QuoteStatus = 297,

	/// <summary>QuoteCancelType, a <see cref="FixField.Integer"/>.</summary>
	QuoteCancelType = 298,

	/// <summary>QuoteEntryID, a <see cref="FixField.Text"/>.</summary>
	QuoteEntryID = 299,

	/// <summary>QuoteRejectReason, a <see cref="FixField.Integer"/>.</summary>
	QuoteRejectReason = 300,

	/// <summary>QuoteResponseLevel, a <see cref="FixField.Integer"/>.</summary>
	QuoteResponseLevel = 301,

	/// <summary>QuoteSetID, a <see cref="FixField.Text"/>.</summary>
	QuoteSetID = 302,

	/// <summary>QuoteRequestType, a <see cref="FixField.Integer"/>.</summary>
	QuoteRequestType = 303,

	/// <summary>TotNoQuoteEntries, a <see cref="FixField.Integer"/>.</summary>
	TotNoQuoteEntries = 304,

	/// <summary>UnderlyingSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityIDSource = 305,

	/// <summary>UnderlyingIssuer, a <see cref="FixField.Text"/>.</summary>
	UnderlyingIssuer = 306,

	/// <summary>UnderlyingSecurityDesc, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityDesc = 307,

	/// <summary>UnderlyingSecurityExchange, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityExchange = 308,

	/// <summary>UnderlyingSecurityID, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityID = 309,

	/// <summary>UnderlyingSecurityType, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityType = 310,

	/// <summary>UnderlyingSymbol, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSymbol = 311,

	/// <summary>UnderlyingSymbolSfx, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSymbolSfx = 312,

	/// <summary>UnderlyingMaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	UnderlyingMaturityMonthYear = 313,

	/// <summary>UnderlyingPutOrCall, a <see cref="FixField.Integer"/>.</summary>
	UnderlyingPutOrCall = 315,

	/// <summary>UnderlyingStrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingStrikePrice = 316,

	/// <summary>UnderlyingOptAttribute, a <see cref="FixField.Character"/>.</summary>
	UnderlyingOptAttribute = 317,

	/// <summary>UnderlyingCurrency, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCurrency = 318,

	/// <summary>SecurityReqID, a <see cref="FixField.Text"/>.</summary>
	SecurityReqID = 320,

	/// <summary>SecurityRequestType, a <see cref="FixField.Integer"/>.</summary>
	SecurityRequestType = 321,

	/// <summary>SecurityResponseID, a <see cref="FixField.Text"/>.</summary>
	SecurityResponseID = 322,

	/// <summary>SecurityResponseType, a <see cref="FixField.Integer"/>.</summary>
	SecurityResponseType = 323,

	/// <summary>SecurityStatusReqID, a <see cref="FixField.Text"/>.</summary>
	SecurityStatusReqID = 324,

	/// <summary>UnsolicitedIndicator, a <see cref="FixField.Boolean"/>.</summary>
	UnsolicitedIndicator = 325,

	/// <summary>SecurityTradingStatus, a <see cref="FixField.Integer"/>.</summary>
	SecurityTradingStatus = 326,

	/// <summary>HaltReason, a <see cref="FixField.Character"/>.</summary>
	HaltReason = 327,

	/// <summary>InViewOfCommon, a <see cref="FixField.Boolean"/>.</summary>
	InViewOfCommon = 328,

	/// <summary>DueToRelated, a <see cref="FixField.Boolean"/>.</summary>
	DueToRelated = 329,

	/// <summary>BuyVolume, a <see cref="FixField.Decimal"/>.</summary>
	BuyVolume = 330,

	/// <summary>SellVolume, a <see cref="FixField.Decimal"/>.</summary>
	SellVolume = 331,

	/// <summary>HighPx, a <see cref="FixField.Decimal"/>.</summary>
	HighPx = 332,

	/// <summary>LowPx, a <see cref="FixField.Decimal"/>.</summary>
	LowPx = 333,

	/// <summary>Adjustment, a <see cref="FixField.Integer"/>.</summary>
	Adjustment = 334,

	/// <summary>TradSesReqID, a <see cref="FixField.Text"/>.</summary>
	TradSesReqID = 335,

	/// <summary>TradingSessionID, a <see cref="FixField.Text"/>.</summary>
	TradingSessionID = 336,

	/// <summary>ContraTrader, a <see cref="FixField.Text"/>.</summary>
	ContraTrader = 337,

	/// <summary>TradSesMethod, a <see cref="FixField.Integer"/>.</summary>
	TradSesMethod = 338,

	/// <summary>TradSesMode, a <see cref="FixField.Integer"/>.</summary>
	TradSesMode = 339,

	/// <summary>TradSesStatus, a <see cref="FixField.Integer"/>.</summary>
	TradSesStatus = 340,

	/// <summary>TradSesStartTime, a <see cref="FixField.Timestamp"/>.</summary>
	TradSesStartTime = 341,

	/// <summary>TradSesOpenTime, a <see cref="FixField.Timestamp"/>.</summary>
	TradSesOpenTime = 342,

	/// <summary>TradSesPreCloseTime, a <see cref="FixField.Timestamp"/>.</summary>
	TradSesPreCloseTime = 343,

	/// <summary>TradSesCloseTime, a <see cref="FixField.Timestamp"/>.</summary>
	TradSesCloseTime = 344,

	/// <summary>TradSesEndTime, a <see cref="FixField.Timestamp"/>.</summary>
	TradSesEndTime = 345,

	/// <summary>NumberOfOrders, a <see cref="FixField.Integer"/>.</summary>
	NumberOfOrders = 346,

	/// <summary>MessageEncoding, a <see cref="FixField.Text"/>.</summary>
	MessageEncoding = 347,

	/// <summary>EncodedIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedIssuerLen = 348,

	/// <summary>EncodedIssuer, a <see cref="FixField.Data"/>.</summary>
	EncodedIssuer = 349,

	/// <summary>EncodedSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedSecurityDescLen = 350,

	/// <summary>EncodedSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	EncodedSecurityDesc = 351,

	/// <summary>EncodedListExecInstLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedListExecInstLen = 352,

	/// <summary>EncodedListExecInst, a <see cref="FixField.Data"/>.</summary>
	EncodedListExecInst = 353,

	/// <summary>EncodedTextLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedTextLen = 354,

	/// <summary>EncodedText, a <see cref="FixField.Data"/>.</summary>
	EncodedText = 355,

	/// <summary>EncodedSubjectLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedSubjectLen = 356,

	/// <summary>EncodedSubject, a <see cref="FixField.Data"/>.</summary>
	EncodedSubject = 357,

	/// <summary>EncodedHeadlineLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedHeadlineLen = 358,

	/// <summary>EncodedHeadline, a <see cref="FixField.Data"/>.</summary>
	EncodedHeadline = 359,

	/// <summary>EncodedAllocTextLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedAllocTextLen = 360,

	/// <summary>EncodedAllocText, a <see cref="FixField.Data"/>.</summary>
	EncodedAllocText = 361,

	/// <summary>EncodedUnderlyingIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedUnderlyingIssuerLen = 362,

	/// <summary>EncodedUnderlyingIssuer, a <see cref="FixField.Data"/>.</summary>
	EncodedUnderlyingIssuer = 363,

	/// <summary>EncodedUnderlyingSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedUnderlyingSecurityDescLen = 364,

	/// <summary>EncodedUnderlyingSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	EncodedUnderlyingSecurityDesc = 365,

	/// <summary>AllocPrice, a <see cref="FixField.Decimal"/>.</summary>
	AllocPrice = 366,

	/// <summary>QuoteSetValidUntilTime, a <see cref="FixField.Timestamp"/>.</summary>
	QuoteSetValidUntilTime = 367,

	/// <summary>QuoteEntryRejectReason, a <see cref="FixField.Integer"/>.</summary>
	QuoteEntryRejectReason = 368,

	/// <summary>LastMsgSeqNumProcessed, a <see cref="FixField.Integer"/>.</summary>
	LastMsgSeqNumProcessed = 369,

	/// <summary>RefTagID, a <see cref="FixField.Integer"/>.</summary>
	RefTagID = 371,

	/// <summary>RefMsgType, a <see cref="FixField.Text"/>.</summary>
	RefMsgType = 372,

	/// <summary>SessionRejectReason, a <see cref="FixField.Integer"/>.</summary>
	SessionRejectReason = 373,

	/// <summary>BidRequestTransType, a <see cref="FixField.Character"/>.</summary>
	BidRequestTransType = 374,

	/// <summary>ContraBroker, a <see cref="FixField.Text"/>.</summary>
	ContraBroker = 375,

	/// <summary>ComplianceID, a <see cref="FixField.Text"/>.</summary>
	ComplianceID = 376,

	/// <summary>SolicitedFlag, a <see cref="FixField.Boolean"/>.</summary>
	SolicitedFlag = 377,

	/// <summary>ExecRestatementReason, a <see cref="FixField.Integer"/>.</summary>
	ExecRestatementReason = 378,

	/// <summary>BusinessRejectRefID, a <see cref="FixField.Text"/>.</summary>
	BusinessRejectRefID = 379,

	/// <summary>BusinessRejectReason, a <see cref="FixField.Integer"/>.</summary>
	BusinessRejectReason = 380,

	/// <summary>GrossTradeAmt, a <see cref="FixField.Decimal"/>.</summary>
	GrossTradeAmt = 381,

	/// <summary>NoContraBrokers, a <see cref="FixField.Integer"/>.</summary>
	NoContraBrokers = 382,

	/// <summary>MaxMessageSize, a <see cref="FixField.Integer"/>.</summary>
	MaxMessageSize = 383,

	/// <summary>NoMsgTypes, a <see cref="FixField.Integer"/>.</summary>
	NoMsgTypes = 384,

	/// <summary>MsgDirection, a <see cref="FixField.Character"/>.</summary>
	MsgDirection = 385,

	/// <summary>NoTradingSessions, a <see cref="FixField.Integer"/>.</summary>
	NoTradingSessions = 386,

	/// <summary>TotalVolumeTraded, a <see cref="FixField.Decimal"/>.</summary>
	TotalVolumeTraded = 387,

	/// <summary>DiscretionInst, a <see cref="FixField.Character"/>.</summary>
	DiscretionInst = 388,

	/// <summary>DiscretionOffsetValue, a <see cref="FixField.Decimal"/>.</summary>
	DiscretionOffsetValue = 389,

	/// <summary>BidID, a <see cref="FixField.Text"/>.</summary>
	BidID = 390,

	/// <summary>ClientBidID, a <see cref="FixField.Text"/>.</summary>
	ClientBidID = 391,

	/// <summary>ListName, a <see cref="FixField.Text"/>.</summary>
	ListName = 392,

	/// <summary>TotNoRelatedSym, a <see cref="FixField.Integer"/>.</summary>
	TotNoRelatedSym = 393,

	/// <summary>BidType, a <see cref="FixField.Integer"/>.</summary>
	BidType = 394,

	/// <summary>NumTickets, a <see cref="FixField.Integer"/>.</summary>
	NumTickets = 395,

	/// <summary>SideValue1, a <see cref="FixField.Decimal"/>.</summary>
	SideValue1 = 396,

	/// <summary>SideValue2, a <see cref="FixField.Decimal"/>.</summary>
	SideValue2 = 397,

	/// <summary>NoBidDescriptors, a <see cref="FixField.Integer"/>.</summary>
	NoBidDescriptors = 398,

	/// <summary>BidDescriptorType, a <see cref="FixField.Integer"/>.</summary>
	BidDescriptorType = 399,

	/// <summary>BidDescriptor, a <see cref="FixField.Text"/>.</summary>
	BidDescriptor = 400,

	/// <summary>SideValueInd, a <see cref="FixField.Integer"/>.</summary>
	SideValueInd = 401,

	/// <summary>LiquidityPctLow, a <see cref="FixField.Decimal"/>.</summary>
	LiquidityPctLow = 402,

	/// <summary>LiquidityPctHigh, a <see cref="FixField.Decimal"/>.</summary>
	LiquidityPctHigh = 403,

	/// <summary>LiquidityValue, a <see cref="FixField.Decimal"/>.</summary>
	LiquidityValue = 404,

	/// <summary>EFPTrackingError, a <see cref="FixField.Decimal"/>.</summary>
	EFPTrackingError = 405,

	/// <summary>FairValue, a <see cref="FixField.Decimal"/>.</summary>
	FairValue = 406,

	/// <summary>OutsideIndexPct, a <see cref="FixField.Decimal"/>.</summary>
	OutsideIndexPct = 407,

	/// <summary>ValueOfFutures, a <see cref="FixField.Decimal"/>.</summary>
	ValueOfFutures = 408,

	/// <summary>LiquidityIndType, a <see cref="FixField.Integer"/>.</summary>
	LiquidityIndType = 409,

	/// <summary>WtAverageLiquidity, a <see cref="FixField.Decimal"/>.</summary>
	WtAverageLiquidity = 410,

	/// <summary>ExchangeForPhysical, a <see cref="FixField.Boolean"/>.</summary>
	ExchangeForPhysical = 411,

	/// <summary>OutMainCntryUIndex, a <see cref="FixField.Decimal"/>.</summary>
	OutMainCntryUIndex = 412,

	/// <summary>CrossPercent, a <see cref="FixField.Decimal"/>.</summary>
	CrossPercent = 413,

	/// <summary>ProgRptReqs, a <see cref="FixField.Integer"/>.</summary>
	ProgRptReqs = 414,

	/// <summary>ProgPeriodInterval, a <see cref="FixField.Integer"/>.</summary>
	ProgPeriodInterval = 415,

	/// <summary>IncTaxInd, a <see cref="FixField.Integer"/>.</summary>
	IncTaxInd = 416,

	/// <summary>NumBidders, a <see cref="FixField.Integer"/>.</summary>
	NumBidders = 417,

	/// <summary>BidTradeType, a <see cref="FixField.Character"/>.</summary>
	BidTradeType = 418,

	/// <summary>BasisPxType, a <see cref="FixField.Character"/>.</summary>
	BasisPxType = 419,

	/// <summary>NoBidComponents, a <see cref="FixField.Integer"/>.</summary>
	NoBidComponents = 420,

	/// <summary>Country, a <see cref="FixField.Text"/>.</summary>
	Country = 421,

	/// <summary>TotNoStrikes, a <see cref="FixField.Integer"/>.</summary>
	TotNoStrikes = 422,

	/// <summary>PriceType, a <see cref="FixField.Integer"/>.</summary>
	PriceType = 423,

	/// <summary>DayOrderQty, a <see cref="FixField.Decimal"/>.</summary>
	DayOrderQty = 424,

	/// <summary>DayCumQty, a <see cref="FixField.Decimal"/>.</summary>
	DayCumQty = 425,

	/// <summary>DayAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	DayAvgPx = 426,

	/// <summary>GTBookingInst, a <see cref="FixField.Integer"/>.</summary>
	GTBookingInst = 427,

	/// <summary>NoStrikes, a <see cref="FixField.Integer"/>.</summary>
	NoStrikes = 428,

	/// <summary>ListStatusType, a <see cref="FixField.Integer"/>.</summary>
	ListStatusType = 429,

	/// <summary>NetGrossInd, a <see cref="FixField.Integer"/>.</summary>
	NetGrossInd = 430,

	/// <summary>ListOrderStatus, a <see cref="FixField.Integer"/>.</summary>
	ListOrderStatus = 431,

	/// <summary>ExpireDate, a <see cref="FixField.Date"/>.</summary>
	ExpireDate = 432,

	/// <summary>ListExecInstType, a <see cref="FixField.Character"/>.</summary>
	ListExecInstType = 433,

	/// <summary>CxlRejResponseTo, a <see cref="FixField.Character"/>.</summary>
	CxlRejResponseTo = 434,

	/// <summary>UnderlyingCouponRate, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingCouponRate = 435,

	/// <summary>UnderlyingContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingContractMultiplier = 436,

	/// <summary>ContraTradeQty, a <see cref="FixField.Decimal"/>.</summary>
	ContraTradeQty = 437,

	/// <summary>ContraTradeTime, a <see cref="FixField.Timestamp"/>.</summary>
	ContraTradeTime = 438,

	/// <summary>LiquidityNumSecurities, a <see cref="FixField.Integer"/>.</summary>
	LiquidityNumSecurities = 441,

	/// <summary>MultiLegReportingType, a <see cref="FixField.Character"/>.</summary>
	MultiLegReportingType = 442,

	/// <summary>StrikeTime, a <see cref="FixField.Timestamp"/>.</summary>
	StrikeTime = 443,

	/// <summary>ListStatusText, a <see cref="FixField.Text"/>.</summary>
	ListStatusText = 444,

	/// <summary>EncodedListStatusTextLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedListStatusTextLen = 445,

	/// <summary>EncodedListStatusText, a <see cref="FixField.Data"/>.</summary>
	EncodedListStatusText = 446,

	/// <summary>PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	PartyIDSource = 447,

	/// <summary>PartyID, a <see cref="FixField.Text"/>.</summary>
	PartyID = 448,

	/// <summary>NetChgPrevDay, a <see cref="FixField.Decimal"/>.</summary>
	NetChgPrevDay = 451,

	/// <summary>PartyRole, a <see cref="FixField.Integer"/>.</summary>
	PartyRole = 452,

	/// <summary>NoPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	NoPartyIDs = 453,

	/// <summary>NoSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	NoSecurityAltID = 454,

	/// <summary>SecurityAltID, a <see cref="FixField.Text"/>.</summary>
	SecurityAltID = 455,

	/// <summary>SecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	SecurityAltIDSource = 456,

	/// <summary>NoUnderlyingSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	NoUnderlyingSecurityAltID = 457,

	/// <summary>UnderlyingSecurityAltID, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityAltID = 458,

	/// <summary>UnderlyingSecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecurityAltIDSource = 459,

	/// <summary>Product, a <see cref="FixField.Integer"/>.</summary>
	Product = 460,

	/// <summary>CFICode, a <see cref="FixField.Text"/>.</summary>
	CFICode = 461,

	/// <summary>UnderlyingProduct, a <see cref="FixField.Integer"/>.</summary>
	UnderlyingProduct = 462,

	/// <summary>UnderlyingCFICode, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCFICode = 463,

	/// <summary>TestMessageIndicator, a <see cref="FixField.Boolean"/>.</summary>
	TestMessageIndicator = 464,

	/// <summary>BookingRefID, a <see cref="FixField.Text"/>.</summary>
	BookingRefID = 466,

	/// <summary>IndividualAllocID, a <see cref="FixField.Text"/>.</summary>
	IndividualAllocID = 467,

	/// <summary>RoundingDirection, a <see cref="FixField.Character"/>.</summary>
	RoundingDirection = 468,

	/// <summary>RoundingModulus, a <see cref="FixField.Decimal"/>.</summary>
	RoundingModulus = 469,

	/// <summary>CountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	CountryOfIssue = 470,

	/// <summary>StateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	StateOrProvinceOfIssue = 471,

	/// <summary>LocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	LocaleOfIssue = 472,

	/// <summary>NoRegistDtls, a <see cref="FixField.Integer"/>.</summary>
	NoRegistDtls = 473,

	/// <summary>MailingDtls, a <see cref="FixField.Text"/>.</summary>
	MailingDtls = 474,

	/// <summary>InvestorCountryOfResidence, a <see cref="FixField.Text"/>.</summary>
	InvestorCountryOfResidence = 475,

	/// <summary>PaymentRef, a <see cref="FixField.Text"/>.</summary>
	PaymentRef = 476,

	/// <summary>DistribPaymentMethod, a <see cref="FixField.Integer"/>.</summary>
	DistribPaymentMethod = 477,

	/// <summary>CashDistribCurr, a <see cref="FixField.Text"/>.</summary>
	CashDistribCurr = 478,

	/// <summary>CommCurrency, a <see cref="FixField.Text"/>.</summary>
	CommCurrency = 479,

	/// <summary>CancellationRights, a <see cref="FixField.Character"/>.</summary>
	CancellationRights = 480,

	/// <summary>MoneyLaunderingStatus, a <see cref="FixField.Character"/>.</summary>
	MoneyLaunderingStatus = 481,

	/// <summary>MailingInst, a <see cref="FixField.Text"/>.</summary>
	MailingInst = 482,

	/// <summary>TransBkdTime, a <see cref="FixField.Timestamp"/>.</summary>
	TransBkdTime = 483,

	/// <summary>ExecPriceType, a <see cref="FixField.Character"/>.</summary>
	ExecPriceType = 484,

	/// <summary>ExecPriceAdjustment, a <see cref="FixField.Decimal"/>.</summary>
	ExecPriceAdjustment = 485,

	/// <summary>DateOfBirth, a <see cref="FixField.Date"/>.</summary>
	DateOfBirth = 486,

	/// <summary>TradeReportTransType, a <see cref="FixField.Integer"/>.</summary>
	TradeReportTransType = 487,

	/// <summary>CardHolderName, a <see cref="FixField.Text"/>.</summary>
	CardHolderName = 488,

	/// <summary>CardNumber, a <see cref="FixField.Text"/>.</summary>
	CardNumber = 489,

	/// <summary>CardExpDate, a <see cref="FixField.Date"/>.</summary>
	CardExpDate = 490,

	/// <summary>CardIssNum, a <see cref="FixField.Text"/>.</summary>
	CardIssNum = 491,

	/// <summary>PaymentMethod, a <see cref="FixField.Integer"/>.</summary>
	PaymentMethod = 492,

	/// <summary>RegistAcctType, a <see cref="FixField.Text"/>.</summary>
	RegistAcctType = 493,

	/// <summary>Designation, a <see cref="FixField.Text"/>.</summary>
	Designation = 494,

	/// <summary>TaxAdvantageType, a <see cref="FixField.Integer"/>.</summary>
	TaxAdvantageType = 495,

	/// <summary>RegistRejReasonText, a <see cref="FixField.Text"/>.</summary>
	RegistRejReasonText = 496,

	/// <summary>FundRenewWaiv, a <see cref="FixField.Character"/>.</summary>
	FundRenewWaiv = 497,

	/// <summary>CashDistribAgentName, a <see cref="FixField.Text"/>.</summary>
	CashDistribAgentName = 498,

	/// <summary>CashDistribAgentCode, a <see cref="FixField.Text"/>.</summary>
	CashDistribAgentCode = 499,

	/// <summary>CashDistribAgentAcctNumber, a <see cref="FixField.Text"/>.</summary>
	CashDistribAgentAcctNumber = 500,

	/// <summary>CashDistribPayRef, a <see cref="FixField.Text"/>.</summary>
	CashDistribPayRef = 501,

	/// <summary>CashDistribAgentAcctName, a <see cref="FixField.Text"/>.</summary>
	CashDistribAgentAcctName = 502,

	/// <summary>CardStartDate, a <see cref="FixField.Date"/>.</summary>
	CardStartDate = 503,

	/// <summary>PaymentDate, a <see cref="FixField.Date"/>.</summary>
	PaymentDate = 504,

	/// <summary>PaymentRemitterID, a <see cref="FixField.Text"/>.</summary>
	PaymentRemitterID = 505,

	/// <summary>RegistStatus, a <see cref="FixField.Character"/>.</summary>
	RegistStatus = 506,

	/// <summary>RegistRejReasonCode, a <see cref="FixField.Integer"/>.</summary>
	RegistRejReasonCode = 507,

	/// <summary>RegistRefID, a <see cref="FixField.Text"/>.</summary>
	RegistRefID = 508,

	/// <summary>RegistDtls, a <see cref="FixField.Text"/>.</summary>
	RegistDtls = 509,

	/// <summary>NoDistribInsts, a <see cref="FixField.Integer"/>.</summary>
	NoDistribInsts = 510,

	/// <summary>RegistEmail, a <see cref="FixField.Text"/>.</summary>
	RegistEmail = 511,

	/// <summary>DistribPercentage, a <see cref="FixField.Decimal"/>.</summary>
	DistribPercentage = 512,

	/// <summary>RegistID, a <see cref="FixField.Text"/>.</summary>
	RegistID = 513,

	/// <summary>RegistTransType, a <see cref="FixField.Character"/>.</summary>
	RegistTransType = 514,

	/// <summary>ExecValuationPoint, a <see cref="FixField.Timestamp"/>.</summary>
	ExecValuationPoint = 515,

	/// <summary>OrderPercent, a <see cref="FixField.Decimal"/>.</summary>
	OrderPercent = 516,

	/// <summary>OwnershipType, a <see cref="FixField.Character"/>.</summary>
	OwnershipType = 517,

	/// <summary>NoContAmts, a <see cref="FixField.Integer"/>.</summary>
	NoContAmts = 518,

	/// <summary>ContAmtType, a <see cref="FixField.Integer"/>.</summary>
	ContAmtType = 519,

	/// <summary>ContAmtValue, a <see cref="FixField.Decimal"/>.</summary>
	ContAmtValue = 520,

	/// <summary>ContAmtCurr, a <see cref="FixField.Text"/>.</summary>
	ContAmtCurr = 521,

	/// <summary>OwnerType, a <see cref="FixField.Integer"/>.</summary>
	OwnerType = 522,

	/// <summary>PartySubID, a <see cref="FixField.Text"/>.</summary>
	PartySubID = 523,

	/// <summary>NestedPartyID, a <see cref="FixField.Text"/>.</summary>
	NestedPartyID = 524,

	/// <summary>NestedPartyIDSource, a <see cref="FixField.Character"/>.</summary>
	NestedPartyIDSource = 525,

	/// <summary>SecondaryClOrdID, a <see cref="FixField.Text"/>.</summary>
	SecondaryClOrdID = 526,

	/// <summary>SecondaryExecID, a <see cref="FixField.Text"/>.</summary>
	SecondaryExecID = 527,

	/// <summary>OrderCapacity, a <see cref="FixField.Character"/>.</summary>
	OrderCapacity = 528,

	/// <summary>OrderRestrictions, a <see cref="FixField.Multiple"/>.</summary>
	OrderRestrictions = 529,

	/// <summary>MassCancelRequestType, a <see cref="FixField.Character"/>.</summary>
	MassCancelRequestType = 530,

	/// <summary>MassCancelResponse, a <see cref="FixField.Character"/>.</summary>
	MassCancelResponse = 531,

	/// <summary>MassCancelRejectReason, a <see cref="FixField.Text"/>.</summary>
	MassCancelRejectReason = 532,

	/// <summary>TotalAffectedOrders, a <see cref="FixField.Integer"/>.</summary>
	TotalAffectedOrders = 533,

	/// <summary>NoAffectedOrders, a <see cref="FixField.Integer"/>.</summary>
	NoAffectedOrders = 534,

	/// <summary>AffectedOrderID, a <see cref="FixField.Text"/>.</summary>
	AffectedOrderID = 535,

	/// <summary>AffectedSecondaryOrderID, a <see cref="FixField.Text"/>.</summary>
	AffectedSecondaryOrderID = 536,

	/// <summary>QuoteType, a <see cref="FixField.Integer"/>.</summary>
	QuoteType = 537,

	/// <summary>NestedPartyRole, a <see cref="FixField.Integer"/>.</summary>
	NestedPartyRole = 538,

	/// <summary>NoNestedPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNestedPartyIDs = 539,

	/// <summary>TotalAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	TotalAccruedInterestAmt = 540,

	/// <summary>MaturityDate, a <see cref="FixField.Date"/>.</summary>
	MaturityDate = 541,

	/// <summary>UnderlyingMaturityDate, a <see cref="FixField.Date"/>.</summary>
	UnderlyingMaturityDate = 542,

	/// <summary>InstrRegistry, a <see cref="FixField.Text"/>.</summary>
	InstrRegistry = 543,

	/// <summary>CashMargin, a <see cref="FixField.Character"/>.</summary>
	CashMargin = 544,

	/// <summary>NestedPartySubID, a <see cref="FixField.Text"/>.</summary>
	NestedPartySubID = 545,

	/// <summary>Scope, a <see cref="FixField.Multiple"/>.</summary>
	Scope = 546,

	/// <summary>MDImplicitDelete, a <see cref="FixField.Boolean"/>.</summary>
	MDImplicitDelete = 547,

	/// <summary>CrossID, a <see cref="FixField.Text"/>.</summary>
	CrossID = 548,

	/// <summary>CrossType, a <see cref="FixField.Integer"/>.</summary>
	CrossType = 549,

	/// <summary>CrossPrioritization, a <see cref="FixField.Integer"/>.</summary>
	CrossPrioritization = 550,

	/// <summary>OrigCrossID, a <see cref="FixField.Text"/>.</summary>
	OrigCrossID = 551,

	/// <summary>NoSides, a <see cref="FixField.Integer"/>.</summary>
	NoSides = 552,

	/// <summary>Username, a <see cref="FixField.Text"/>.</summary>
	Username = 553,

	/// <summary>Password, a <see cref="FixField.Text"/>.</summary>
	Password = 554,

	/// <summary>NoLegs, a <see cref="FixField.Integer"/>.</summary>
	NoLegs = 555,

	/// <summary>LegCurrency, a <see cref="FixField.Text"/>.</summary>
	LegCurrency = 556,

	/// <summary>TotNoSecurityTypes, a <see cref="FixField.Integer"/>.</summary>
	TotNoSecurityTypes = 557,

	/// <summary>NoSecurityTypes, a <see cref="FixField.Integer"/>.</summary>
	NoSecurityTypes = 558,

	/// <summary>SecurityListRequestType, a <see cref="FixField.Integer"/>.</summary>
	SecurityListRequestType = 559,

	/// <summary>SecurityRequestResult, a <see cref="FixField.Integer"/>.</summary>
	SecurityRequestResult = 560,

	/// <summary>RoundLot, a <see cref="FixField.Decimal"/>.</summary>
	RoundLot = 561,

	/// <summary>MinTradeVol, a <see cref="FixField.Decimal"/>.</summary>
	MinTradeVol = 562,

	/// <summary>MultiLegRptTypeReq, a <see cref="FixField.Integer"/>.</summary>
	MultiLegRptTypeReq = 563,

	/// <summary>LegPositionEffect, a <see cref="FixField.Character"/>.</summary>
	LegPositionEffect = 564,

	/// <summary>LegCoveredOrUncovered, a <see cref="FixField.Integer"/>.</summary>
	LegCoveredOrUncovered = 565,

	/// <summary>LegPrice, a <see cref="FixField.Decimal"/>.</summary>
	LegPrice = 566,

	/// <summary>TradSesStatusRejReason, a <see cref="FixField.Integer"/>.</summary>
	TradSesStatusRejReason = 567,

	/// <summary>TradeRequestID, a <see cref="FixField.Text"/>.</summary>
	TradeRequestID = 568,

	/// <summary>TradeRequestType, a <see cref="FixField.Integer"/>.</summary>
	TradeRequestType = 569,

	/// <summary>PreviouslyReported, a <see cref="FixField.Boolean"/>.</summary>
	PreviouslyReported = 570,

	/// <summary>TradeReportID, a <see cref="FixField.Text"/>.</summary>
	TradeReportID = 571,

	/// <summary>TradeReportRefID, a <see cref="FixField.Text"/>.</summary>
	TradeReportRefID = 572,

	/// <summary>MatchStatus, a <see cref="FixField.Character"/>.</summary>
	MatchStatus = 573,

	/// <summary>MatchType, a <see cref="FixField.Text"/>.</summary>
	MatchType = 574,

	/// <summary>OddLot, a <see cref="FixField.Boolean"/>.</summary>
	OddLot = 575,

	/// <summary>NoClearingInstructions, a <see cref="FixField.Integer"/>.</summary>
	NoClearingInstructions = 576,

	/// <summary>ClearingInstruction, a <see cref="FixField.Integer"/>.</summary>
	ClearingInstruction = 577,

	/// <summary>TradeInputSource, a <see cref="FixField.Text"/>.</summary>
	TradeInputSource = 578,

	/// <summary>TradeInputDevice, a <see cref="FixField.Text"/>.</summary>
	TradeInputDevice = 579,

	/// <summary>NoDates, a <see cref="FixField.Integer"/>.</summary>
	NoDates = 580,

	/// <summary>AccountType, a <see cref="FixField.Integer"/>.</summary>
	AccountType = 581,

	/// <summary>CustOrderCapacity, a <see cref="FixField.Integer"/>.</summary>
	CustOrderCapacity = 582,

	/// <summary>ClOrdLinkID, a <see cref="FixField.Text"/>.</summary>
	ClOrdLinkID = 583,

	/// <summary>MassStatusReqID, a <see cref="FixField.Text"/>.</summary>
	MassStatusReqID = 584,

	/// <summary>MassStatusReqType, a <see cref="FixField.Integer"/>.</summary>
	MassStatusReqType = 585,

	/// <summary>OrigOrdModTime, a <see cref="FixField.Timestamp"/>.</summary>
	OrigOrdModTime = 586,

	/// <summary>LegSettlType, a <see cref="FixField.Character"/>.</summary>
	LegSettlType = 587,

	/// <summary>LegSettlDate, a <see cref="FixField.Date"/>.</summary>
	LegSettlDate = 588,

	/// <summary>DayBookingInst, a <see cref="FixField.Character"/>.</summary>
	DayBookingInst = 589,

	/// <summary>BookingUnit, a <see cref="FixField.Character"/>.</summary>
	BookingUnit = 590,

	/// <summary>PreallocMethod, a <see cref="FixField.Character"/>.</summary>
	PreallocMethod = 591,

	/// <summary>UnderlyingCountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCountryOfIssue = 592,

	/// <summary>UnderlyingStateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	UnderlyingStateOrProvinceOfIssue = 593,

	/// <summary>UnderlyingLocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	UnderlyingLocaleOfIssue = 594,

	/// <summary>UnderlyingInstrRegistry, a <see cref="FixField.Text"/>.</summary>
	UnderlyingInstrRegistry = 595,

	/// <summary>LegCountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	LegCountryOfIssue = 596,

	/// <summary>LegStateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	LegStateOrProvinceOfIssue = 597,

	/// <summary>LegLocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	LegLocaleOfIssue = 598,

	/// <summary>LegInstrRegistry, a <see cref="FixField.Text"/>.</summary>
	LegInstrRegistry = 599,

	/// <summary>LegSymbol, a <see cref="FixField.Text"/>.</summary>
	LegSymbol = 600,

	/// <summary>LegSymbolSfx, a <see cref="FixField.Text"/>.</summary>
	LegSymbolSfx = 601,

	/// <summary>LegSecurityID, a <see cref="FixField.Text"/>.</summary>
	LegSecurityID = 602,

	/// <summary>LegSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	LegSecurityIDSource = 603,

	/// <summary>NoLegSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	NoLegSecurityAltID = 604,

	/// <summary>LegSecurityAltID, a <see cref="FixField.Text"/>.</summary>
	LegSecurityAltID = 605,

	/// <summary>LegSecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	LegSecurityAltIDSource = 606,

	/// <summary>LegProduct, a <see cref="FixField.Integer"/>.</summary>
	LegProduct = 607,

	/// <summary>LegCFICode, a <see cref="FixField.Text"/>.</summary>
	LegCFICode = 608,

	/// <summary>LegSecurityType, a <see cref="FixField.Text"/>.</summary>
	LegSecurityType = 609,

	/// <summary>LegMaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	LegMaturityMonthYear = 610,

	/// <summary>LegMaturityDate, a <see cref="FixField.Date"/>.</summary>
	LegMaturityDate = 611,

	/// <summary>LegStrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	LegStrikePrice = 612,

	/// <summary>LegOptAttribute, a <see cref="FixField.Character"/>.</summary>
	LegOptAttribute = 613,

	/// <summary>LegContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	LegContractMultiplier = 614,

	/// <summary>LegCouponRate, a <see cref="FixField.Decimal"/>.</summary>
	LegCouponRate = 615,

	/// <summary>LegSecurityExchange, a <see cref="FixField.Text"/>.</summary>
	LegSecurityExchange = 616,

	/// <summary>LegIssuer, a <see cref="FixField.Text"/>.</summary>
	LegIssuer = 617,

	/// <summary>EncodedLegIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedLegIssuerLen = 618,

	/// <summary>EncodedLegIssuer, a <see cref="FixField.Data"/>.</summary>
	EncodedLegIssuer = 619,

	/// <summary>LegSecurityDesc, a <see cref="FixField.Text"/>.</summary>
	LegSecurityDesc = 620,

	/// <summary>EncodedLegSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	EncodedLegSecurityDescLen = 621,

	/// <summary>EncodedLegSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	EncodedLegSecurityDesc = 622,

	/// <summary>LegRatioQty, a <see cref="FixField.Decimal"/>.</summary>
	LegRatioQty = 623,

	/// <summary>LegSide, a <see cref="FixField.Character"/>.</summary>
	LegSide = 624,

	/// <summary>TradingSessionSubID, a <see cref="FixField.Text"/>.</summary>
	TradingSessionSubID = 625,

	/// <summary>AllocType, a <see cref="FixField.Integer"/>.</summary>
	AllocType = 626,

	/// <summary>NoHops, a <see cref="FixField.Integer"/>.</summary>
	NoHops = 627,

	/// <summary>HopCompID, a <see cref="FixField.Text"/>.</summary>
	HopCompID = 628,

	/// <summary>HopSendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	HopSendingTime = 629,

	/// <summary>HopRefID, a <see cref="FixField.Integer"/>.</summary>
	HopRefID = 630,

	/// <summary>MidPx, a <see cref="FixField.Decimal"/>.</summary>
	MidPx = 631,

	/// <summary>BidYield, a <see cref="FixField.Decimal"/>.</summary>
	BidYield = 632,

	/// <summary>MidYield, a <see cref="FixField.Decimal"/>.</summary>
	MidYield = 633,

	/// <summary>OfferYield, a <see cref="FixField.Decimal"/>.</summary>
	OfferYield = 634,

	/// <summary>ClearingFeeIndicator, a <see cref="FixField.Text"/>.</summary>
	ClearingFeeIndicator = 635,

	/// <summary>WorkingIndicator, a <see cref="FixField.Boolean"/>.</summary>
	WorkingIndicator = 636,

	/// <summary>LegLastPx, a <see cref="FixField.Decimal"/>.</summary>
	LegLastPx = 637,

	/// <summary>PriorityIndicator, a <see cref="FixField.Integer"/>.</summary>
	PriorityIndicator = 638,

	/// <summary>PriceImprovement, a <see cref="FixField.Decimal"/>.</summary>
	PriceImprovement = 639,

	/// <summary>Price2, a <see cref="FixField.Decimal"/>.</summary>
	Price2 = 640,

	/// <summary>LastForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	LastForwardPoints2 = 641,

	/// <summary>BidForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	BidForwardPoints2 = 642,

	/// <summary>OfferForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	OfferForwardPoints2 = 643,

	/// <summary>RFQReqID, a <see cref="FixField.Text"/>.</summary>
	RFQReqID = 644,

	/// <summary>MktBidPx, a <see cref="FixField.Decimal"/>.</summary>
	MktBidPx = 645,

	/// <summary>MktOfferPx, a <see cref="FixField.Decimal"/>.</summary>
	MktOfferPx = 646,

	/// <summary>MinBidSize, a <see cref="FixField.Decimal"/>.</summary>
	MinBidSize = 647,

	/// <summary>MinOfferSize, a <see cref="FixField.Decimal"/>.</summary>
	MinOfferSize = 648,

	/// <summary>QuoteStatusReqID, a <see cref="FixField.Text"/>.</summary>
	QuoteStatusReqID = 649,

	/// <summary>LegalConfirm, a <see cref="FixField.Boolean"/>.</summary>
	LegalConfirm = 650,

	/// <summary>UnderlyingLastPx, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingLastPx = 651,

	/// <summary>UnderlyingLastQty, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingLastQty = 652,

	/// <summary>LegRefID, a <see cref="FixField.Text"/>.</summary>
	LegRefID = 654,

	/// <summary>ContraLegRefID, a <see cref="FixField.Text"/>.</summary>
	ContraLegRefID = 655,

	/// <summary>SettlCurrBidFxRate, a <see cref="FixField.Decimal"/>.</summary>
	SettlCurrBidFxRate = 656,

	/// <summary>SettlCurrOfferFxRate, a <see cref="FixField.Decimal"/>.</summary>
	SettlCurrOfferFxRate = 657,

	/// <summary>QuoteRequestRejectReason, a <see cref="FixField.Integer"/>.</summary>
	QuoteRequestRejectReason = 658,

	/// <summary>SideComplianceID, a <see cref="FixField.Text"/>.</summary>
	SideComplianceID = 659,

	/// <summary>AcctIDSource, a <see cref="FixField.Integer"/>.</summary>
	AcctIDSource = 660,

	/// <summary>AllocAcctIDSource, a <see cref="FixField.Integer"/>.</summary>
	AllocAcctIDSource = 661,

	/// <summary>BenchmarkPrice, a <see cref="FixField.Decimal"/>.</summary>
	BenchmarkPrice = 662,

	/// <summary>BenchmarkPriceType, a <see cref="FixField.Integer"/>.</summary>
	BenchmarkPriceType = 663,

	/// <summary>ConfirmID, a <see cref="FixField.Text"/>.</summary>
	ConfirmID = 664,

	/// <summary>ConfirmStatus, a <see cref="FixField.Integer"/>.</summary>
	ConfirmStatus = 665,

	/// <summary>ConfirmTransType, a <see cref="FixField.Integer"/>.</summary>
	ConfirmTransType = 666,

	/// <summary>ContractSettlMonth, a <see cref="FixField.MonthYear"/>.</summary>
	ContractSettlMonth = 667,

	/// <summary>DeliveryForm, a <see cref="FixField.Integer"/>.</summary>
	DeliveryForm = 668,

	/// <summary>LastParPx, a <see cref="FixField.Decimal"/>.</summary>
	LastParPx = 669,

	/// <summary>NoLegAllocs, a <see cref="FixField.Integer"/>.</summary>
	NoLegAllocs = 670,

	/// <summary>LegAllocAccount, a <see cref="FixField.Text"/>.</summary>
	LegAllocAccount = 671,

	/// <summary>LegIndividualAllocID, a <see cref="FixField.Text"/>.</summary>
	LegIndividualAllocID = 672,

	/// <summary>LegAllocQty, a <see cref="FixField.Decimal"/>.</summary>
	LegAllocQty = 673,

	/// <summary>LegAllocAcctIDSource, a <see cref="FixField.Text"/>.</summary>
	LegAllocAcctIDSource = 674,

	/// <summary>LegSettlCurrency, a <see cref="FixField.Text"/>.</summary>
	LegSettlCurrency = 675,

	/// <summary>LegBenchmarkCurveCurrency, a <see cref="FixField.Text"/>.</summary>
	LegBenchmarkCurveCurrency = 676,

	/// <summary>LegBenchmarkCurveName, a <see cref="FixField.Text"/>.</summary>
	LegBenchmarkCurveName = 677,

	/// <summary>LegBenchmarkCurvePoint, a <see cref="FixField.Text"/>.</summary>
	LegBenchmarkCurvePoint = 678,

	/// <summary>LegBenchmarkPrice, a <see cref="FixField.Decimal"/>.</summary>
	LegBenchmarkPrice = 679,

	/// <summary>LegBenchmarkPriceType, a <see cref="FixField.Integer"/>.</summary>
	LegBenchmarkPriceType = 680,

	/// <summary>LegBidPx, a <see cref="FixField.Decimal"/>.</summary>
	LegBidPx = 681,

	/// <summary>LegIOIQty, a <see cref="FixField.Text"/>.</summary>
	LegIOIQty = 682,

	/// <summary>NoLegStipulations, a <see cref="FixField.Integer"/>.</summary>
	NoLegStipulations = 683,

	/// <summary>LegOfferPx, a <see cref="FixField.Decimal"/>.</summary>
	LegOfferPx = 684,

	/// <summary>LegPriceType, a <see cref="FixField.Integer"/>.</summary>
	LegPriceType = 686,

	/// <summary>LegQty, a <see cref="FixField.Decimal"/>.</summary>
	LegQty = 687,

	/// <summary>LegStipulationType, a <see cref="FixField.Text"/>.</summary>
	LegStipulationType = 688,

	/// <summary>LegStipulationValue, a <see cref="FixField.Text"/>.</summary>
	LegStipulationValue = 689,

	/// <summary>LegSwapType, a <see cref="FixField.Integer"/>.</summary>
	LegSwapType = 690,

	/// <summary>Pool, a <see cref="FixField.Text"/>.</summary>
	Pool = 691,

	/// <summary>QuotePriceType, a <see cref="FixField.Integer"/>.</summary>
	QuotePriceType = 692,

	/// <summary>QuoteRespID, a <see cref="FixField.Text"/>.</summary>
	QuoteRespID = 693,

	/// <summary>QuoteRespType, a <see cref="FixField.Integer"/>.</summary>
	QuoteRespType = 694,

	/// <summary>QuoteQualifier, a <see cref="FixField.Character"/>.</summary>
	QuoteQualifier = 695,

	/// <summary>YieldRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	YieldRedemptionDate = 696,

	/// <summary>YieldRedemptionPrice, a <see cref="FixField.Decimal"/>.</summary>
	YieldRedemptionPrice = 697,

	/// <summary>YieldRedemptionPriceType, a <see cref="FixField.Integer"/>.</summary>
	YieldRedemptionPriceType = 698,

	/// <summary>BenchmarkSecurityID, a <see cref="FixField.Text"/>.</summary>
	BenchmarkSecurityID = 699,

	/// <summary>ReversalIndicator, a <see cref="FixField.Boolean"/>.</summary>
	ReversalIndicator = 700,

	/// <summary>YieldCalcDate, a <see cref="FixField.Date"/>.</summary>
	YieldCalcDate = 701,

	/// <summary>NoPositions, a <see cref="FixField.Integer"/>.</summary>
	NoPositions = 702,

	/// <summary>PosType, a <see cref="FixField.Text"/>.</summary>
	PosType = 703,

	/// <summary>LongQty, a <see cref="FixField.Decimal"/>.</summary>
	LongQty = 704,

	/// <summary>ShortQty, a <see cref="FixField.Decimal"/>.</summary>
	ShortQty = 705,

	/// <summary>PosQtyStatus, a <see cref="FixField.Integer"/>.</summary>
	PosQtyStatus = 706,

	/// <summary>PosAmtType, a <see cref="FixField.Text"/>.</summary>
	PosAmtType = 707,

	/// <summary>PosAmt, a <see cref="FixField.Decimal"/>.</summary>
	PosAmt = 708,

	/// <summary>PosTransType, a <see cref="FixField.Integer"/>.</summary>
	PosTransType = 709,

	/// <summary>PosReqID, a <see cref="FixField.Text"/>.</summary>
	PosReqID = 710,

	/// <summary>NoUnderlyings, a <see cref="FixField.Integer"/>.</summary>
	NoUnderlyings = 711,

	/// <summary>PosMaintAction, a <see cref="FixField.Integer"/>.</summary>
	PosMaintAction = 712,

	/// <summary>OrigPosReqRefID, a <see cref="FixField.Text"/>.</summary>
	OrigPosReqRefID = 713,

	/// <summary>PosMaintRptRefID, a <see cref="FixField.Text"/>.</summary>
	PosMaintRptRefID = 714,

	/// <summary>ClearingBusinessDate, a <see cref="FixField.Date"/>.</summary>
	ClearingBusinessDate = 715,

	/// <summary>SettlSessID, a <see cref="FixField.Text"/>.</summary>
	SettlSessID = 716,

	/// <summary>SettlSessSubID, a <see cref="FixField.Text"/>.</summary>
	SettlSessSubID = 717,

	/// <summary>AdjustmentType, a <see cref="FixField.Integer"/>.</summary>
	AdjustmentType = 718,

	/// <summary>ContraryInstructionIndicator, a <see cref="FixField.Boolean"/>.</summary>
	ContraryInstructionIndicator = 719,

	/// <summary>PriorSpreadIndicator, a <see cref="FixField.Boolean"/>.</summary>
	PriorSpreadIndicator = 720,

	/// <summary>PosMaintRptID, a <see cref="FixField.Text"/>.</summary>
	PosMaintRptID = 721,

	/// <summary>PosMaintStatus, a <see cref="FixField.Integer"/>.</summary>
	PosMaintStatus = 722,

	/// <summary>PosMaintResult, a <see cref="FixField.Integer"/>.</summary>
	PosMaintResult = 723,

	/// <summary>PosReqType, a <see cref="FixField.Integer"/>.</summary>
	PosReqType = 724,

	/// <summary>ResponseTransportType, a <see cref="FixField.Integer"/>.</summary>
	ResponseTransportType = 725,

	/// <summary>ResponseDestination, a <see cref="FixField.Text"/>.</summary>
	ResponseDestination = 726,

	/// <summary>TotalNumPosReports, a <see cref="FixField.Integer"/>.</summary>
	TotalNumPosReports = 727,

	/// <summary>PosReqResult, a <see cref="FixField.Integer"/>.</summary>
	PosReqResult = 728,

	/// <summary>PosReqStatus, a <see cref="FixField.Integer"/>.</summary>
	PosReqStatus = 729,

	/// <summary>SettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	SettlPrice = 730,

	/// <summary>SettlPriceType, a <see cref="FixField.Integer"/>.</summary>
	SettlPriceType = 731,

	/// <summary>UnderlyingSettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingSettlPrice = 732,

	/// <summary>UnderlyingSettlPriceType, a <see cref="FixField.Integer"/>.</summary>
	UnderlyingSettlPriceType = 733,

	/// <summary>PriorSettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	PriorSettlPrice = 734,

	/// <summary>NoQuoteQualifiers, a <see cref="FixField.Integer"/>.</summary>
	NoQuoteQualifiers = 735,

	/// <summary>AllocSettlCurrency, a <see cref="FixField.Text"/>.</summary>
	AllocSettlCurrency = 736,

	/// <summary>AllocSettlCurrAmt, a <see cref="FixField.Decimal"/>.</summary>
	AllocSettlCurrAmt = 737,

	/// <summary>InterestAtMaturity, a <see cref="FixField.Decimal"/>.</summary>
	InterestAtMaturity = 738,

	/// <summary>LegDatedDate, a <see cref="FixField.Date"/>.</summary>
	LegDatedDate = 739,

	/// <summary>LegPool, a <see cref="FixField.Text"/>.</summary>
	LegPool = 740,

	/// <summary>AllocInterestAtMaturity, a <see cref="FixField.Decimal"/>.</summary>
	AllocInterestAtMaturity = 741,

	/// <summary>AllocAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	AllocAccruedInterestAmt = 742,

	/// <summary>DeliveryDate, a <see cref="FixField.Date"/>.</summary>
	DeliveryDate = 743,

	/// <summary>AssignmentMethod, a <see cref="FixField.Character"/>.</summary>
	AssignmentMethod = 744,

	/// <summary>AssignmentUnit, a <see cref="FixField.Decimal"/>.</summary>
	AssignmentUnit = 745,

	/// <summary>OpenInterest, a <see cref="FixField.Decimal"/>.</summary>
	OpenInterest = 746,

	/// <summary>ExerciseMethod, a <see cref="FixField.Character"/>.</summary>
	ExerciseMethod = 747,

	/// <summary>TotNumTradeReports, a <see cref="FixField.Integer"/>.</summary>
	TotNumTradeReports = 748,

	/// <summary>TradeRequestResult, a <see cref="FixField.Integer"/>.</summary>
	TradeRequestResult = 749,

	/// <summary>TradeRequestStatus, a <see cref="FixField.Integer"/>.</summary>
	TradeRequestStatus = 750,

	/// <summary>TradeReportRejectReason, a <see cref="FixField.Integer"/>.</summary>
	TradeReportRejectReason = 751,

	/// <summary>SideMultiLegReportingType, a <see cref="FixField.Integer"/>.</summary>
	SideMultiLegReportingType = 752,

	/// <summary>NoPosAmt, a <see cref="FixField.Integer"/>.</summary>
	NoPosAmt = 753,

	/// <summary>AutoAcceptIndicator, a <see cref="FixField.Boolean"/>.</summary>
	AutoAcceptIndicator = 754,

	/// <summary>AllocReportID, a <see cref="FixField.Text"/>.</summary>
	AllocReportID = 755,

	/// <summary>NoNested2PartyIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNested2PartyIDs = 756,

	/// <summary>Nested2PartyID, a <see cref="FixField.Text"/>.</summary>
	Nested2PartyID = 757,

	/// <summary>Nested2PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	Nested2PartyIDSource = 758,

	/// <summary>Nested2PartyRole, a <see cref="FixField.Integer"/>.</summary>
	Nested2PartyRole = 759,

	/// <summary>Nested2PartySubID, a <see cref="FixField.Text"/>.</summary>
	Nested2PartySubID = 760,

	/// <summary>BenchmarkSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	BenchmarkSecurityIDSource = 761,

	/// <summary>SecuritySubType, a <see cref="FixField.Text"/>.</summary>
	SecuritySubType = 762,

	/// <summary>UnderlyingSecuritySubType, a <see cref="FixField.Text"/>.</summary>
	UnderlyingSecuritySubType = 763,

	/// <summary>LegSecuritySubType, a <see cref="FixField.Text"/>.</summary>
	LegSecuritySubType = 764,

	/// <summary>AllowableOneSidednessPct, a <see cref="FixField.Decimal"/>.</summary>
	AllowableOneSidednessPct = 765,

	/// <summary>AllowableOneSidednessValue, a <see cref="FixField.Decimal"/>.</summary>
	AllowableOneSidednessValue = 766,

	/// <summary>AllowableOneSidednessCurr, a <see cref="FixField.Text"/>.</summary>
	AllowableOneSidednessCurr = 767,

	/// <summary>NoTrdRegTimestamps, a <see cref="FixField.Integer"/>.</summary>
	NoTrdRegTimestamps = 768,

	/// <summary>TrdRegTimestamp, a <see cref="FixField.Timestamp"/>.</summary>
	TrdRegTimestamp = 769,

	/// <summary>TrdRegTimestampType, a <see cref="FixField.Integer"/>.</summary>
	TrdRegTimestampType = 770,

	/// <summary>TrdRegTimestampOrigin, a <see cref="FixField.Text"/>.</summary>
	TrdRegTimestampOrigin = 771,

	/// <summary>ConfirmRefID, a <see cref="FixField.Text"/>.</summary>
	ConfirmRefID = 772,

	/// <summary>ConfirmType, a <see cref="FixField.Integer"/>.</summary>
	ConfirmType = 773,

	/// <summary>ConfirmRejReason, a <see cref="FixField.Integer"/>.</summary>
	ConfirmRejReason = 774,

	/// <summary>BookingType, a <see cref="FixField.Integer"/>.</summary>
	BookingType = 775,

	/// <summary>IndividualAllocRejCode, a <see cref="FixField.Integer"/>.</summary>
	IndividualAllocRejCode = 776,

	/// <summary>SettlInstMsgID, a <see cref="FixField.Text"/>.</summary>
	SettlInstMsgID = 777,

	/// <summary>NoSettlInst, a <see cref="FixField.Integer"/>.</summary>
	NoSettlInst = 778,

	/// <summary>LastUpdateTime, a <see cref="FixField.Timestamp"/>.</summary>
	LastUpdateTime = 779,

	/// <summary>AllocSettlInstType, a <see cref="FixField.Integer"/>.</summary>
	AllocSettlInstType = 780,

	/// <summary>NoSettlPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	NoSettlPartyIDs = 781,

	/// <summary>SettlPartyID, a <see cref="FixField.Text"/>.</summary>
	SettlPartyID = 782,

	/// <summary>SettlPartyIDSource, a <see cref="FixField.Character"/>.</summary>
	SettlPartyIDSource = 783,

	/// <summary>SettlPartyRole, a <see cref="FixField.Integer"/>.</summary>
	SettlPartyRole = 784,

	/// <summary>SettlPartySubID, a <see cref="FixField.Text"/>.</summary>
	SettlPartySubID = 785,

	/// <summary>SettlPartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	SettlPartySubIDType = 786,

	/// <summary>DlvyInstType, a <see cref="FixField.Character"/>.</summary>
	DlvyInstType = 787,

	/// <summary>TerminationType, a <see cref="FixField.Integer"/>.</summary>
	TerminationType = 788,

	/// <summary>NextExpectedMsgSeqNum, a <see cref="FixField.Integer"/>.</summary>
	NextExpectedMsgSeqNum = 789,

	/// <summary>OrdStatusReqID, a <see cref="FixField.Text"/>.</summary>
	OrdStatusReqID = 790,

	/// <summary>SettlInstReqID, a <see cref="FixField.Text"/>.</summary>
	SettlInstReqID = 791,

	/// <summary>SettlInstReqRejCode, a <see cref="FixField.Integer"/>.</summary>
	SettlInstReqRejCode = 792,

	/// <summary>SecondaryAllocID, a <see cref="FixField.Text"/>.</summary>
	SecondaryAllocID = 793,

	/// <summary>AllocReportType, a <see cref="FixField.Integer"/>.</summary>
	AllocReportType = 794,

	/// <summary>AllocReportRefID, a <see cref="FixField.Text"/>.</summary>
	AllocReportRefID = 795,

	/// <summary>AllocCancReplaceReason, a <see cref="FixField.Integer"/>.</summary>
	AllocCancReplaceReason = 796,

	/// <summary>CopyMsgIndicator, a <see cref="FixField.Boolean"/>.</summary>
	CopyMsgIndicator = 797,

	/// <summary>AllocAccountType, a <see cref="FixField.Integer"/>.</summary>
	AllocAccountType = 798,

	/// <summary>OrderAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	OrderAvgPx = 799,

	/// <summary>OrderBookingQty, a <see cref="FixField.Decimal"/>.</summary>
	OrderBookingQty = 800,

	/// <summary>NoSettlPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	NoSettlPartySubIDs = 801,

	/// <summary>NoPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	NoPartySubIDs = 802,

	/// <summary>PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	PartySubIDType = 803,

	/// <summary>NoNestedPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNestedPartySubIDs = 804,

	/// <summary>NestedPartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	NestedPartySubIDType = 805,

	/// <summary>NoNested2PartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNested2PartySubIDs = 806,

	/// <summary>Nested2PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	Nested2PartySubIDType = 807,

	/// <summary>AllocIntermedReqType, a <see cref="FixField.Integer"/>.</summary>
	AllocIntermedReqType = 808,

	/// <summary>UnderlyingPx, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingPx = 810,

	/// <summary>PriceDelta, a <see cref="FixField.Decimal"/>.</summary>
	PriceDelta = 811,

	/// <summary>ApplQueueMax, a <see cref="FixField.Integer"/>.</summary>
	ApplQueueMax = 812,

	/// <summary>ApplQueueDepth, a <see cref="FixField.Integer"/>.</summary>
	ApplQueueDepth = 813,

	/// <summary>ApplQueueResolution, a <see cref="FixField.Integer"/>.</summary>
	ApplQueueResolution = 814,

	/// <summary>ApplQueueAction, a <see cref="FixField.Integer"/>.</summary>
	ApplQueueAction = 815,

	/// <summary>NoAltMDSource, a <see cref="FixField.Integer"/>.</summary>
	NoAltMDSource = 816,

	/// <summary>AltMDSourceID, a <see cref="FixField.Text"/>.</summary>
	AltMDSourceID = 817,

	/// <summary>SecondaryTradeReportID, a <see cref="FixField.Text"/>.</summary>
	SecondaryTradeReportID = 818,

	/// <summary>AvgPxIndicator, a <see cref="FixField.Integer"/>.</summary>
	AvgPxIndicator = 819,

	/// <summary>TradeLinkID, a <see cref="FixField.Text"/>.</summary>
	TradeLinkID = 820,

	/// <summary>OrderInputDevice, a <see cref="FixField.Text"/>.</summary>
	OrderInputDevice = 821,

	/// <summary>UnderlyingTradingSessionID, a <see cref="FixField.Text"/>.</summary>
	UnderlyingTradingSessionID = 822,

	/// <summary>UnderlyingTradingSessionSubID, a <see cref="FixField.Text"/>.</summary>
	UnderlyingTradingSessionSubID = 823,

	/// <summary>TradeLegRefID, a <see cref="FixField.Text"/>.</summary>
	TradeLegRefID = 824,

	/// <summary>ExchangeRule, a <see cref="FixField.Text"/>.</summary>
	ExchangeRule = 825,

	/// <summary>TradeAllocIndicator, a <see cref="FixField.Integer"/>.</summary>
	TradeAllocIndicator = 826,

	/// <summary>ExpirationCycle, a <see cref="FixField.Integer"/>.</summary>
	ExpirationCycle = 827,

	/// <summary>TrdType, a <see cref="FixField.Integer"/>.</summary>
	TrdType = 828,

	/// <summary>TrdSubType, a <see cref="FixField.Integer"/>.</summary>
	TrdSubType = 829,

	/// <summary>TransferReason, a <see cref="FixField.Text"/>.</summary>
	TransferReason = 830,

	/// <summary>TotNumAssignmentReports, a <see cref="FixField.Integer"/>.</summary>
	TotNumAssignmentReports = 832,

	/// <summary>AsgnRptID, a <see cref="FixField.Text"/>.</summary>
	AsgnRptID = 833,

	/// <summary>ThresholdAmount, a <see cref="FixField.Decimal"/>.</summary>
	ThresholdAmount = 834,

	/// <summary>PegMoveType, a <see cref="FixField.Integer"/>.</summary>
	PegMoveType = 835,

	/// <summary>PegOffsetType, a <see cref="FixField.Integer"/>.</summary>
	PegOffsetType = 836,

	/// <summary>PegLimitType, a <see cref="FixField.Integer"/>.</summary>
	PegLimitType = 837,

	/// <summary>PegRoundDirection, a <see cref="FixField.Integer"/>.</summary>
	PegRoundDirection = 838,

	/// <summary>PeggedPrice, a <see cref="FixField.Decimal"/>.</summary>
	PeggedPrice = 839,

	/// <summary>PegScope, a <see cref="FixField.Integer"/>.</summary>
	PegScope = 840,

	/// <summary>DiscretionMoveType, a <see cref="FixField.Integer"/>.</summary>
	DiscretionMoveType = 841,

	/// <summary>DiscretionOffsetType, a <see cref="FixField.Integer"/>.</summary>
	DiscretionOffsetType = 842,

	/// <summary>DiscretionLimitType, a <see cref="FixField.Integer"/>.</summary>
	DiscretionLimitType = 843,

	/// <summary>DiscretionRoundDirection, a <see cref="FixField.Integer"/>.</summary>
	DiscretionRoundDirection = 844,

	/// <summary>DiscretionPrice, a <see cref="FixField.Decimal"/>.</summary>
	DiscretionPrice = 845,

	/// <summary>DiscretionScope, a <see cref="FixField.Integer"/>.</summary>
	DiscretionScope = 846,

	/// <summary>TargetStrategy, a <see cref="FixField.Integer"/>.</summary>
	TargetStrategy = 847,

	/// <summary>TargetStrategyParameters, a <see cref="FixField.Text"/>.</summary>
	TargetStrategyParameters = 848,

	/// <summary>ParticipationRate, a <see cref="FixField.Decimal"/>.</summary>
	ParticipationRate = 849,

	/// <summary>TargetStrategyPerformance, a <see cref="FixField.Decimal"/>.</summary>
	TargetStrategyPerformance = 850,

	/// <summary>LastLiquidityInd, a <see cref="FixField.Integer"/>.</summary>
	LastLiquidityInd = 851,

	/// <summary>PublishTrdIndicator, a <see cref="FixField.Boolean"/>.</summary>
	PublishTrdIndicator = 852,

	/// <summary>ShortSaleReason, a <see cref="FixField.Integer"/>.</summary>
	ShortSaleReason = 853,

	/// <summary>QtyType, a <see cref="FixField.Integer"/>.</summary>
	QtyType = 854,

	/// <summary>SecondaryTrdType, a <see cref="FixField.Integer"/>.</summary>
	SecondaryTrdType = 855,

	/// <summary>TradeReportType, a <see cref="FixField.Integer"/>.</summary>
	TradeReportType = 856,

	/// <summary>AllocNoOrdersType, a <see cref="FixField.Integer"/>.</summary>
	AllocNoOrdersType = 857,

	/// <summary>SharedCommission, a <see cref="FixField.Decimal"/>.</summary>
	SharedCommission = 858,

	/// <summary>ConfirmReqID, a <see cref="FixField.Text"/>.</summary>
	ConfirmReqID = 859,

	/// <summary>AvgParPx, a <see cref="FixField.Decimal"/>.</summary>
	AvgParPx = 860,

	/// <summary>ReportedPx, a <see cref="FixField.Decimal"/>.</summary>
	ReportedPx = 861,

	/// <summary>NoCapacities, a <see cref="FixField.Integer"/>.</summary>
	NoCapacities = 862,

	/// <summary>OrderCapacityQty, a <see cref="FixField.Decimal"/>.</summary>
	OrderCapacityQty = 863,

	/// <summary>NoEvents, a <see cref="FixField.Integer"/>.</summary>
	NoEvents = 864,

	/// <summary>EventType, a <see cref="FixField.Integer"/>.</summary>
	EventType = 865,

	/// <summary>EventDate, a <see cref="FixField.Date"/>.</summary>
	EventDate = 866,

	/// <summary>EventPx, a <see cref="FixField.Decimal"/>.</summary>
	EventPx = 867,

	/// <summary>EventText, a <see cref="FixField.Text"/>.</summary>
	EventText = 868,

	/// <summary>PctAtRisk, a <see cref="FixField.Decimal"/>.</summary>
	PctAtRisk = 869,

	/// <summary>NoInstrAttrib, a <see cref="FixField.Integer"/>.</summary>
	NoInstrAttrib = 870,

	/// <summary>InstrAttribType, a <see cref="FixField.Integer"/>.</summary>
	InstrAttribType = 871,

	/// <summary>InstrAttribValue, a <see cref="FixField.Text"/>.</summary>
	InstrAttribValue = 872,

	/// <summary>DatedDate, a <see cref="FixField.Date"/>.</summary>
	DatedDate = 873,

	/// <summary>InterestAccrualDate, a <see cref="FixField.Date"/>.</summary>
	InterestAccrualDate = 874,

	/// <summary>CPProgram, a <see cref="FixField.Integer"/>.</summary>
	CPProgram = 875,

	/// <summary>CPRegType, a <see cref="FixField.Text"/>.</summary>
	CPRegType = 876,

	/// <summary>UnderlyingCPProgram, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCPProgram = 877,

	/// <summary>UnderlyingCPRegType, a <see cref="FixField.Text"/>.</summary>
	UnderlyingCPRegType = 878,

	/// <summary>UnderlyingQty, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingQty = 879,

	/// <summary>TrdMatchID, a <see cref="FixField.Text"/>.</summary>
	TrdMatchID = 880,

	/// <summary>SecondaryTradeReportRefID, a <see cref="FixField.Text"/>.</summary>
	SecondaryTradeReportRefID = 881,

	/// <summary>UnderlyingDirtyPrice, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingDirtyPrice = 882,

	/// <summary>UnderlyingEndPrice, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingEndPrice = 883,

	/// <summary>UnderlyingStartValue, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingStartValue = 884,

	/// <summary>UnderlyingCurrentValue, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingCurrentValue = 885,

	/// <summary>UnderlyingEndValue, a <see cref="FixField.Decimal"/>.</summary>
	UnderlyingEndValue = 886,

	/// <summary>NoUnderlyingStips, a <see cref="FixField.Integer"/>.</summary>
	NoUnderlyingStips = 887,

	/// <summary>UnderlyingStipType, a <see cref="FixField.Text"/>.</summary>
	UnderlyingStipType = 888,

	/// <summary>UnderlyingStipValue, a <see cref="FixField.Text"/>.</summary>
	UnderlyingStipValue = 889,

	/// <summary>MaturityNetMoney, a <see cref="FixField.Decimal"/>.</summary>
	MaturityNetMoney = 890,

	/// <summary>MiscFeeBasis, a <see cref="FixField.Integer"/>.</summary>
	MiscFeeBasis = 891,

	/// <summary>TotNoAllocs, a <see cref="FixField.Integer"/>.</summary>
	TotNoAllocs = 892,

	/// <summary>LastFragment, a <see cref="FixField.Boolean"/>.</summary>
	LastFragment = 893,

	/// <summary>CollReqID, a <see cref="FixField.Text"/>.</summary>
	CollReqID = 894,

	/// <summary>CollAsgnReason, a <see cref="FixField.Integer"/>.</summary>
	CollAsgnReason = 895,

	/// <summary>CollInquiryQualifier, a <see cref="FixField.Integer"/>.</summary>
	CollInquiryQualifier = 896,

	/// <summary>NoTrades, a <see cref="FixField.Integer"/>.</summary>
	NoTrades = 897,

	/// <summary>MarginRatio, a <see cref="FixField.Decimal"/>.</summary>
	MarginRatio = 898,

	/// <summary>MarginExcess, a <see cref="FixField.Decimal"/>.</summary>
	MarginExcess = 899,

	/// <summary>TotalNetValue, a <see cref="FixField.Decimal"/>.</summary>
	TotalNetValue = 900,

	/// <summary>CashOutstanding, a <see cref="FixField.Decimal"/>.</summary>
	CashOutstanding = 901,

	/// <summary>CollAsgnID, a <see cref="FixField.Text"/>.</summary>
	CollAsgnID = 902,

	/// <summary>CollAsgnTransType, a <see cref="FixField.Integer"/>.</summary>
	CollAsgnTransType = 903,

	/// <summary>CollRespID, a <see cref="FixField.Text"/>.</summary>
	CollRespID = 904,

	/// <summary>CollAsgnRespType, a <see cref="FixField.Integer"/>.</summary>
	CollAsgnRespType = 905,

	/// <summary>CollAsgnRejectReason, a <see cref="FixField.Integer"/>.</summary>
	CollAsgnRejectReason = 906,

	/// <summary>CollAsgnRefID, a <see cref="FixField.Text"/>.</summary>
	CollAsgnRefID = 907,

	/// <summary>CollRptID, a <see cref="FixField.Text"/>.</summary>
	CollRptID = 908,

	/// <summary>CollInquiryID, a <see cref="FixField.Text"/>.</summary>
	CollInquiryID = 909,

	/// <summary>CollStatus, a <see cref="FixField.Integer"/>.</summary>
	CollStatus = 910,

	/// <summary>TotNumReports, a <see cref="FixField.Integer"/>.</summary>
	TotNumReports = 911,

	/// <summary>LastRptRequested, a <see cref="FixField.Boolean"/>.</summary>
	LastRptRequested = 912,

	/// <summary>AgreementDesc, a <see cref="FixField.Text"/>.</summary>
	AgreementDesc = 913,

	/// <summary>AgreementID, a <see cref="FixField.Text"/>.</summary>
	AgreementID = 914,

	/// <summary>AgreementDate, a <see cref="FixField.Date"/>.</summary>
	AgreementDate = 915,

	/// <summary>StartDate, a <see cref="FixField.Date"/>.</summary>
	StartDate = 916,

	/// <summary>EndDate, a <see cref="FixField.Date"/>.</summary>
	EndDate = 917,

	/// <summary>AgreementCurrency, a <see cref="FixField.Text"/>.</summary>
	AgreementCurrency = 918,

	/// <summary>DeliveryType, a <see cref="FixField.Integer"/>.</summary>
	DeliveryType = 919,

	/// <summary>EndAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	EndAccruedInterestAmt = 920,

	/// <summary>StartCash, a <see cref="FixField.Decimal"/>.</summary>
	StartCash = 921,

	/// <summary>EndCash, a <see cref="FixField.Decimal"/>.</summary>
	EndCash = 922,

	/// <summary>UserRequestID, a <see cref="FixField.Text"/>.</summary>
	UserRequestID = 923,

	/// <summary>UserRequestType, a <see cref="FixField.Integer"/>.</summary>
	UserRequestType = 924,

	/// <summary>NewPassword, a <see cref="FixField.Text"/>.</summary>
	NewPassword = 925,

	/// <summary>UserStatus, a <see cref="FixField.Integer"/>.</summary>
	UserStatus = 926,

	/// <summary>UserStatusText, a <see cref="FixField.Text"/>.</summary>
	UserStatusText = 927,

	/// <summary>StatusValue, a <see cref="FixField.Integer"/>.</summary>
	StatusValue = 928,

	/// <summary>StatusText, a <see cref="FixField.Text"/>.</summary>
	StatusText = 929,

	/// <summary>RefCompID, a <see cref="FixField.Text"/>.</summary>
	RefCompID = 930,

	/// <summary>RefSubID, a <see cref="FixField.Text"/>.</summary>
	RefSubID = 931,

	/// <summary>NetworkResponseID, a <see cref="FixField.Text"/>.</summary>
	NetworkResponseID = 932,

	/// <summary>NetworkRequestID, a <see cref="FixField.Text"/>.</summary>
	NetworkRequestID = 933,

	/// <summary>LastNetworkResponseID, a <see cref="FixField.Text"/>.</summary>
	LastNetworkResponseID = 934,

	/// <summary>NetworkRequestType, a <see cref="FixField.Integer"/>.</summary>
	NetworkRequestType = 935,

	/// <summary>NoCompIDs, a <see cref="FixField.Integer"/>.</summary>
	NoCompIDs = 936,

	/// <summary>NetworkStatusResponseType, a <see cref="FixField.Integer"/>.</summary>
	NetworkStatusResponseType = 937,

	/// <summary>NoCollInquiryQualifier, a <see cref="FixField.Integer"/>.</summary>
	NoCollInquiryQualifier = 938,

	/// <summary>TrdRptStatus, a <see cref="FixField.Integer"/>.</summary>
	TrdRptStatus = 939,

	/// <summary>AffirmStatus, a <see cref="FixField.Integer"/>.</summary>
	AffirmStatus = 940,

	/// <summary>UnderlyingStrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	UnderlyingStrikeCurrency = 941,

	/// <summary>LegStrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	LegStrikeCurrency = 942,

	/// <summary>TimeBracket, a <see cref="FixField.Text"/>.</summary>
	TimeBracket = 943,

	/// <summary>CollAction, a <see cref="FixField.Integer"/>.</summary>
	CollAction = 944,

	/// <summary>CollInquiryStatus, a <see cref="FixField.Integer"/>.</summary>
	CollInquiryStatus = 945,

	/// <summary>CollInquiryResult, a <see cref="FixField.Integer"/>.</summary>
	CollInquiryResult = 946,

	/// <summary>StrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	StrikeCurrency = 947,

	/// <summary>NoNested3PartyIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNested3PartyIDs = 948,

	/// <summary>Nested3PartyID, a <see cref="FixField.Text"/>.</summary>
	Nested3PartyID = 949,

	/// <summary>Nested3PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	Nested3PartyIDSource = 950,

	/// <summary>Nested3PartyRole, a <see cref="FixField.Integer"/>.</summary>
	Nested3PartyRole = 951,

	/// <summary>NoNested3PartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	NoNested3PartySubIDs = 952,

	/// <summary>Nested3PartySubID, a <see cref="FixField.Text"/>.</summary>
	Nested3PartySubID = 953,

	/// <summary>Nested3PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	Nested3PartySubIDType = 954,

	/// <summary>LegContractSettlMonth, a <see cref="FixField.MonthYear"/>.</summary>
	LegContractSettlMonth = 955,

	/// <summary>LegInterestAccrualDate, a <see cref="FixField.Date"/>.</summary>
	LegInterestAccrualDate = 956,
}
