namespace DotGram.Finance.Fix;

// Written by generate.py from the FIX 4.4 repository; not edited by hand.

/// <summary>The number of every field of every FIX version this package reads, as a constant named for the field: <c>FixField.Decimal { Tag: FixTag.OrderQty }</c>.</summary>
/// <remarks>A tag is its number, and a tag no version defines is only that: <c>25005</c>.</remarks>
public static class FixTag
{
	/// <summary>Account, a <see cref="FixField.Text"/>.</summary>
	public const int Account = 1;

	/// <summary>AdvId, a <see cref="FixField.Text"/>.</summary>
	public const int AdvId = 2;

	/// <summary>AdvRefID, a <see cref="FixField.Text"/>.</summary>
	public const int AdvRefID = 3;

	/// <summary>AdvSide, a <see cref="FixField.Character"/>.</summary>
	public const int AdvSide = 4;

	/// <summary>AdvTransType, a <see cref="FixField.Text"/>.</summary>
	public const int AdvTransType = 5;

	/// <summary>AvgPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int AvgPx = 6;

	/// <summary>BeginSeqNo, a <see cref="FixField.Integer"/>.</summary>
	public const int BeginSeqNo = 7;

	/// <summary>BeginString, a <see cref="FixField.Text"/>.</summary>
	public const int BeginString = 8;

	/// <summary>BodyLength, a <see cref="FixField.Integer"/>.</summary>
	public const int BodyLength = 9;

	/// <summary>CheckSum, a <see cref="FixField.Text"/>.</summary>
	public const int CheckSum = 10;

	/// <summary>ClOrdID, a <see cref="FixField.Text"/>.</summary>
	public const int ClOrdID = 11;

	/// <summary>Commission, a <see cref="FixField.Decimal"/>.</summary>
	public const int Commission = 12;

	/// <summary>CommType, a <see cref="FixField.Character"/>.</summary>
	public const int CommType = 13;

	/// <summary>CumQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int CumQty = 14;

	/// <summary>Currency, a <see cref="FixField.Text"/>.</summary>
	public const int Currency = 15;

	/// <summary>EndSeqNo, a <see cref="FixField.Integer"/>.</summary>
	public const int EndSeqNo = 16;

	/// <summary>ExecID, a <see cref="FixField.Text"/>.</summary>
	public const int ExecID = 17;

	/// <summary>ExecInst, a <see cref="FixField.Multiple"/>.</summary>
	public const int ExecInst = 18;

	/// <summary>ExecRefID, a <see cref="FixField.Text"/>.</summary>
	public const int ExecRefID = 19;

	/// <summary>HandlInst, a <see cref="FixField.Character"/>.</summary>
	public const int HandlInst = 21;

	/// <summary>SecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityIDSource = 22;

	/// <summary>IOIID, a <see cref="FixField.Text"/>.</summary>
	public const int IOIID = 23;

	/// <summary>IOIQltyInd, a <see cref="FixField.Character"/>.</summary>
	public const int IOIQltyInd = 25;

	/// <summary>IOIRefID, a <see cref="FixField.Text"/>.</summary>
	public const int IOIRefID = 26;

	/// <summary>IOIQty, a <see cref="FixField.Text"/>.</summary>
	public const int IOIQty = 27;

	/// <summary>IOITransType, a <see cref="FixField.Character"/>.</summary>
	public const int IOITransType = 28;

	/// <summary>LastCapacity, a <see cref="FixField.Character"/>.</summary>
	public const int LastCapacity = 29;

	/// <summary>LastMkt, a <see cref="FixField.Text"/>.</summary>
	public const int LastMkt = 30;

	/// <summary>LastPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastPx = 31;

	/// <summary>LastQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastQty = 32;

	/// <summary>NoLinesOfText, a <see cref="FixField.Integer"/>.</summary>
	public const int NoLinesOfText = 33;

	/// <summary>MsgSeqNum, a <see cref="FixField.Integer"/>.</summary>
	public const int MsgSeqNum = 34;

	/// <summary>MsgType, a <see cref="FixField.Text"/>.</summary>
	public const int MsgType = 35;

	/// <summary>NewSeqNo, a <see cref="FixField.Integer"/>.</summary>
	public const int NewSeqNo = 36;

	/// <summary>OrderID, a <see cref="FixField.Text"/>.</summary>
	public const int OrderID = 37;

	/// <summary>OrderQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderQty = 38;

	/// <summary>OrdStatus, a <see cref="FixField.Character"/>.</summary>
	public const int OrdStatus = 39;

	/// <summary>OrdType, a <see cref="FixField.Character"/>.</summary>
	public const int OrdType = 40;

	/// <summary>OrigClOrdID, a <see cref="FixField.Text"/>.</summary>
	public const int OrigClOrdID = 41;

	/// <summary>OrigTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int OrigTime = 42;

	/// <summary>PossDupFlag, a <see cref="FixField.Boolean"/>.</summary>
	public const int PossDupFlag = 43;

	/// <summary>Price, a <see cref="FixField.Decimal"/>.</summary>
	public const int Price = 44;

	/// <summary>RefSeqNum, a <see cref="FixField.Integer"/>.</summary>
	public const int RefSeqNum = 45;

	/// <summary>SecurityID, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityID = 48;

	/// <summary>SenderCompID, a <see cref="FixField.Text"/>.</summary>
	public const int SenderCompID = 49;

	/// <summary>SenderSubID, a <see cref="FixField.Text"/>.</summary>
	public const int SenderSubID = 50;

	/// <summary>SendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int SendingTime = 52;

	/// <summary>Quantity, a <see cref="FixField.Decimal"/>.</summary>
	public const int Quantity = 53;

	/// <summary>Side, a <see cref="FixField.Character"/>.</summary>
	public const int Side = 54;

	/// <summary>Symbol, a <see cref="FixField.Text"/>.</summary>
	public const int Symbol = 55;

	/// <summary>TargetCompID, a <see cref="FixField.Text"/>.</summary>
	public const int TargetCompID = 56;

	/// <summary>TargetSubID, a <see cref="FixField.Text"/>.</summary>
	public const int TargetSubID = 57;

	/// <summary>Text, a <see cref="FixField.Text"/>.</summary>
	public const int Text = 58;

	/// <summary>TimeInForce, a <see cref="FixField.Character"/>.</summary>
	public const int TimeInForce = 59;

	/// <summary>TransactTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TransactTime = 60;

	/// <summary>Urgency, a <see cref="FixField.Character"/>.</summary>
	public const int Urgency = 61;

	/// <summary>ValidUntilTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int ValidUntilTime = 62;

	/// <summary>SettlType, a <see cref="FixField.Character"/>.</summary>
	public const int SettlType = 63;

	/// <summary>SettlDate, a <see cref="FixField.Date"/>.</summary>
	public const int SettlDate = 64;

	/// <summary>SymbolSfx, a <see cref="FixField.Text"/>.</summary>
	public const int SymbolSfx = 65;

	/// <summary>ListID, a <see cref="FixField.Text"/>.</summary>
	public const int ListID = 66;

	/// <summary>ListSeqNo, a <see cref="FixField.Integer"/>.</summary>
	public const int ListSeqNo = 67;

	/// <summary>TotNoOrders, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoOrders = 68;

	/// <summary>ListExecInst, a <see cref="FixField.Text"/>.</summary>
	public const int ListExecInst = 69;

	/// <summary>AllocID, a <see cref="FixField.Text"/>.</summary>
	public const int AllocID = 70;

	/// <summary>AllocTransType, a <see cref="FixField.Character"/>.</summary>
	public const int AllocTransType = 71;

	/// <summary>RefAllocID, a <see cref="FixField.Text"/>.</summary>
	public const int RefAllocID = 72;

	/// <summary>NoOrders, a <see cref="FixField.Integer"/>.</summary>
	public const int NoOrders = 73;

	/// <summary>AvgPxPrecision, a <see cref="FixField.Integer"/>.</summary>
	public const int AvgPxPrecision = 74;

	/// <summary>TradeDate, a <see cref="FixField.Date"/>.</summary>
	public const int TradeDate = 75;

	/// <summary>PositionEffect, a <see cref="FixField.Character"/>.</summary>
	public const int PositionEffect = 77;

	/// <summary>NoAllocs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoAllocs = 78;

	/// <summary>AllocAccount, a <see cref="FixField.Text"/>.</summary>
	public const int AllocAccount = 79;

	/// <summary>AllocQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocQty = 80;

	/// <summary>ProcessCode, a <see cref="FixField.Character"/>.</summary>
	public const int ProcessCode = 81;

	/// <summary>NoRpts, a <see cref="FixField.Integer"/>.</summary>
	public const int NoRpts = 82;

	/// <summary>RptSeq, a <see cref="FixField.Integer"/>.</summary>
	public const int RptSeq = 83;

	/// <summary>CxlQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int CxlQty = 84;

	/// <summary>NoDlvyInst, a <see cref="FixField.Integer"/>.</summary>
	public const int NoDlvyInst = 85;

	/// <summary>AllocStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocStatus = 87;

	/// <summary>AllocRejCode, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocRejCode = 88;

	/// <summary>Signature, a <see cref="FixField.Data"/>.</summary>
	public const int Signature = 89;

	/// <summary>SecureDataLen, a <see cref="FixField.Integer"/>.</summary>
	public const int SecureDataLen = 90;

	/// <summary>SecureData, a <see cref="FixField.Data"/>.</summary>
	public const int SecureData = 91;

	/// <summary>SignatureLength, a <see cref="FixField.Integer"/>.</summary>
	public const int SignatureLength = 93;

	/// <summary>EmailType, a <see cref="FixField.Character"/>.</summary>
	public const int EmailType = 94;

	/// <summary>RawDataLength, a <see cref="FixField.Integer"/>.</summary>
	public const int RawDataLength = 95;

	/// <summary>RawData, a <see cref="FixField.Data"/>.</summary>
	public const int RawData = 96;

	/// <summary>PossResend, a <see cref="FixField.Boolean"/>.</summary>
	public const int PossResend = 97;

	/// <summary>EncryptMethod, a <see cref="FixField.Integer"/>.</summary>
	public const int EncryptMethod = 98;

	/// <summary>StopPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int StopPx = 99;

	/// <summary>ExDestination, a <see cref="FixField.Text"/>.</summary>
	public const int ExDestination = 100;

	/// <summary>CxlRejReason, a <see cref="FixField.Integer"/>.</summary>
	public const int CxlRejReason = 102;

	/// <summary>OrdRejReason, a <see cref="FixField.Integer"/>.</summary>
	public const int OrdRejReason = 103;

	/// <summary>IOIQualifier, a <see cref="FixField.Character"/>.</summary>
	public const int IOIQualifier = 104;

	/// <summary>Issuer, a <see cref="FixField.Text"/>.</summary>
	public const int Issuer = 106;

	/// <summary>SecurityDesc, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityDesc = 107;

	/// <summary>HeartBtInt, a <see cref="FixField.Integer"/>.</summary>
	public const int HeartBtInt = 108;

	/// <summary>MinQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int MinQty = 110;

	/// <summary>MaxFloor, a <see cref="FixField.Decimal"/>.</summary>
	public const int MaxFloor = 111;

	/// <summary>TestReqID, a <see cref="FixField.Text"/>.</summary>
	public const int TestReqID = 112;

	/// <summary>ReportToExch, a <see cref="FixField.Boolean"/>.</summary>
	public const int ReportToExch = 113;

	/// <summary>LocateReqd, a <see cref="FixField.Boolean"/>.</summary>
	public const int LocateReqd = 114;

	/// <summary>OnBehalfOfCompID, a <see cref="FixField.Text"/>.</summary>
	public const int OnBehalfOfCompID = 115;

	/// <summary>OnBehalfOfSubID, a <see cref="FixField.Text"/>.</summary>
	public const int OnBehalfOfSubID = 116;

	/// <summary>QuoteID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteID = 117;

	/// <summary>NetMoney, a <see cref="FixField.Decimal"/>.</summary>
	public const int NetMoney = 118;

	/// <summary>SettlCurrAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int SettlCurrAmt = 119;

	/// <summary>SettlCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int SettlCurrency = 120;

	/// <summary>ForexReq, a <see cref="FixField.Boolean"/>.</summary>
	public const int ForexReq = 121;

	/// <summary>OrigSendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int OrigSendingTime = 122;

	/// <summary>GapFillFlag, a <see cref="FixField.Boolean"/>.</summary>
	public const int GapFillFlag = 123;

	/// <summary>NoExecs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoExecs = 124;

	/// <summary>ExpireTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int ExpireTime = 126;

	/// <summary>DKReason, a <see cref="FixField.Character"/>.</summary>
	public const int DKReason = 127;

	/// <summary>DeliverToCompID, a <see cref="FixField.Text"/>.</summary>
	public const int DeliverToCompID = 128;

	/// <summary>DeliverToSubID, a <see cref="FixField.Text"/>.</summary>
	public const int DeliverToSubID = 129;

	/// <summary>IOINaturalFlag, a <see cref="FixField.Boolean"/>.</summary>
	public const int IOINaturalFlag = 130;

	/// <summary>QuoteReqID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteReqID = 131;

	/// <summary>BidPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidPx = 132;

	/// <summary>OfferPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferPx = 133;

	/// <summary>BidSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidSize = 134;

	/// <summary>OfferSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferSize = 135;

	/// <summary>NoMiscFees, a <see cref="FixField.Integer"/>.</summary>
	public const int NoMiscFees = 136;

	/// <summary>MiscFeeAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int MiscFeeAmt = 137;

	/// <summary>MiscFeeCurr, a <see cref="FixField.Text"/>.</summary>
	public const int MiscFeeCurr = 138;

	/// <summary>MiscFeeType, a <see cref="FixField.Text"/>.</summary>
	public const int MiscFeeType = 139;

	/// <summary>PrevClosePx, a <see cref="FixField.Decimal"/>.</summary>
	public const int PrevClosePx = 140;

	/// <summary>ResetSeqNumFlag, a <see cref="FixField.Boolean"/>.</summary>
	public const int ResetSeqNumFlag = 141;

	/// <summary>SenderLocationID, a <see cref="FixField.Text"/>.</summary>
	public const int SenderLocationID = 142;

	/// <summary>TargetLocationID, a <see cref="FixField.Text"/>.</summary>
	public const int TargetLocationID = 143;

	/// <summary>OnBehalfOfLocationID, a <see cref="FixField.Text"/>.</summary>
	public const int OnBehalfOfLocationID = 144;

	/// <summary>DeliverToLocationID, a <see cref="FixField.Text"/>.</summary>
	public const int DeliverToLocationID = 145;

	/// <summary>NoRelatedSym, a <see cref="FixField.Integer"/>.</summary>
	public const int NoRelatedSym = 146;

	/// <summary>Subject, a <see cref="FixField.Text"/>.</summary>
	public const int Subject = 147;

	/// <summary>Headline, a <see cref="FixField.Text"/>.</summary>
	public const int Headline = 148;

	/// <summary>URLLink, a <see cref="FixField.Text"/>.</summary>
	public const int URLLink = 149;

	/// <summary>ExecType, a <see cref="FixField.Character"/>.</summary>
	public const int ExecType = 150;

	/// <summary>LeavesQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LeavesQty = 151;

	/// <summary>CashOrderQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int CashOrderQty = 152;

	/// <summary>AllocAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocAvgPx = 153;

	/// <summary>AllocNetMoney, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocNetMoney = 154;

	/// <summary>SettlCurrFxRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int SettlCurrFxRate = 155;

	/// <summary>SettlCurrFxRateCalc, a <see cref="FixField.Character"/>.</summary>
	public const int SettlCurrFxRateCalc = 156;

	/// <summary>NumDaysInterest, a <see cref="FixField.Integer"/>.</summary>
	public const int NumDaysInterest = 157;

	/// <summary>AccruedInterestRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int AccruedInterestRate = 158;

	/// <summary>AccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int AccruedInterestAmt = 159;

	/// <summary>SettlInstMode, a <see cref="FixField.Character"/>.</summary>
	public const int SettlInstMode = 160;

	/// <summary>AllocText, a <see cref="FixField.Text"/>.</summary>
	public const int AllocText = 161;

	/// <summary>SettlInstID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlInstID = 162;

	/// <summary>SettlInstTransType, a <see cref="FixField.Character"/>.</summary>
	public const int SettlInstTransType = 163;

	/// <summary>EmailThreadID, a <see cref="FixField.Text"/>.</summary>
	public const int EmailThreadID = 164;

	/// <summary>SettlInstSource, a <see cref="FixField.Character"/>.</summary>
	public const int SettlInstSource = 165;

	/// <summary>SecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityType = 167;

	/// <summary>EffectiveTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int EffectiveTime = 168;

	/// <summary>StandInstDbType, a <see cref="FixField.Integer"/>.</summary>
	public const int StandInstDbType = 169;

	/// <summary>StandInstDbName, a <see cref="FixField.Text"/>.</summary>
	public const int StandInstDbName = 170;

	/// <summary>StandInstDbID, a <see cref="FixField.Text"/>.</summary>
	public const int StandInstDbID = 171;

	/// <summary>SettlDeliveryType, a <see cref="FixField.Integer"/>.</summary>
	public const int SettlDeliveryType = 172;

	/// <summary>BidSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidSpotRate = 188;

	/// <summary>BidForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidForwardPoints = 189;

	/// <summary>OfferSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferSpotRate = 190;

	/// <summary>OfferForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferForwardPoints = 191;

	/// <summary>OrderQty2, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderQty2 = 192;

	/// <summary>SettlDate2, a <see cref="FixField.Date"/>.</summary>
	public const int SettlDate2 = 193;

	/// <summary>LastSpotRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastSpotRate = 194;

	/// <summary>LastForwardPoints, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastForwardPoints = 195;

	/// <summary>AllocLinkID, a <see cref="FixField.Text"/>.</summary>
	public const int AllocLinkID = 196;

	/// <summary>AllocLinkType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocLinkType = 197;

	/// <summary>SecondaryOrderID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryOrderID = 198;

	/// <summary>NoIOIQualifiers, a <see cref="FixField.Integer"/>.</summary>
	public const int NoIOIQualifiers = 199;

	/// <summary>MaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	public const int MaturityMonthYear = 200;

	/// <summary>PutOrCall, a <see cref="FixField.Integer"/>.</summary>
	public const int PutOrCall = 201;

	/// <summary>StrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int StrikePrice = 202;

	/// <summary>CoveredOrUncovered, a <see cref="FixField.Integer"/>.</summary>
	public const int CoveredOrUncovered = 203;

	/// <summary>OptAttribute, a <see cref="FixField.Character"/>.</summary>
	public const int OptAttribute = 206;

	/// <summary>SecurityExchange, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityExchange = 207;

	/// <summary>NotifyBrokerOfCredit, a <see cref="FixField.Boolean"/>.</summary>
	public const int NotifyBrokerOfCredit = 208;

	/// <summary>AllocHandlInst, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocHandlInst = 209;

	/// <summary>MaxShow, a <see cref="FixField.Decimal"/>.</summary>
	public const int MaxShow = 210;

	/// <summary>PegOffsetValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int PegOffsetValue = 211;

	/// <summary>XmlDataLen, a <see cref="FixField.Integer"/>.</summary>
	public const int XmlDataLen = 212;

	/// <summary>XmlData, a <see cref="FixField.Data"/>.</summary>
	public const int XmlData = 213;

	/// <summary>SettlInstRefID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlInstRefID = 214;

	/// <summary>NoRoutingIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoRoutingIDs = 215;

	/// <summary>RoutingType, a <see cref="FixField.Integer"/>.</summary>
	public const int RoutingType = 216;

	/// <summary>RoutingID, a <see cref="FixField.Text"/>.</summary>
	public const int RoutingID = 217;

	/// <summary>Spread, a <see cref="FixField.Decimal"/>.</summary>
	public const int Spread = 218;

	/// <summary>BenchmarkCurveCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int BenchmarkCurveCurrency = 220;

	/// <summary>BenchmarkCurveName, a <see cref="FixField.Text"/>.</summary>
	public const int BenchmarkCurveName = 221;

	/// <summary>BenchmarkCurvePoint, a <see cref="FixField.Text"/>.</summary>
	public const int BenchmarkCurvePoint = 222;

	/// <summary>CouponRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int CouponRate = 223;

	/// <summary>CouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	public const int CouponPaymentDate = 224;

	/// <summary>IssueDate, a <see cref="FixField.Date"/>.</summary>
	public const int IssueDate = 225;

	/// <summary>RepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	public const int RepurchaseTerm = 226;

	/// <summary>RepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int RepurchaseRate = 227;

	/// <summary>Factor, a <see cref="FixField.Decimal"/>.</summary>
	public const int Factor = 228;

	/// <summary>TradeOriginationDate, a <see cref="FixField.Date"/>.</summary>
	public const int TradeOriginationDate = 229;

	/// <summary>ExDate, a <see cref="FixField.Date"/>.</summary>
	public const int ExDate = 230;

	/// <summary>ContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	public const int ContractMultiplier = 231;

	/// <summary>NoStipulations, a <see cref="FixField.Integer"/>.</summary>
	public const int NoStipulations = 232;

	/// <summary>StipulationType, a <see cref="FixField.Text"/>.</summary>
	public const int StipulationType = 233;

	/// <summary>StipulationValue, a <see cref="FixField.Text"/>.</summary>
	public const int StipulationValue = 234;

	/// <summary>YieldType, a <see cref="FixField.Text"/>.</summary>
	public const int YieldType = 235;

	/// <summary>Yield, a <see cref="FixField.Decimal"/>.</summary>
	public const int Yield = 236;

	/// <summary>TotalTakedown, a <see cref="FixField.Decimal"/>.</summary>
	public const int TotalTakedown = 237;

	/// <summary>Concession, a <see cref="FixField.Decimal"/>.</summary>
	public const int Concession = 238;

	/// <summary>RepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int RepoCollateralSecurityType = 239;

	/// <summary>RedemptionDate, a <see cref="FixField.Date"/>.</summary>
	public const int RedemptionDate = 240;

	/// <summary>UnderlyingCouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	public const int UnderlyingCouponPaymentDate = 241;

	/// <summary>UnderlyingIssueDate, a <see cref="FixField.Date"/>.</summary>
	public const int UnderlyingIssueDate = 242;

	/// <summary>UnderlyingRepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingRepoCollateralSecurityType = 243;

	/// <summary>UnderlyingRepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	public const int UnderlyingRepurchaseTerm = 244;

	/// <summary>UnderlyingRepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingRepurchaseRate = 245;

	/// <summary>UnderlyingFactor, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingFactor = 246;

	/// <summary>UnderlyingRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	public const int UnderlyingRedemptionDate = 247;

	/// <summary>LegCouponPaymentDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegCouponPaymentDate = 248;

	/// <summary>LegIssueDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegIssueDate = 249;

	/// <summary>LegRepoCollateralSecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int LegRepoCollateralSecurityType = 250;

	/// <summary>LegRepurchaseTerm, a <see cref="FixField.Integer"/>.</summary>
	public const int LegRepurchaseTerm = 251;

	/// <summary>LegRepurchaseRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegRepurchaseRate = 252;

	/// <summary>LegFactor, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegFactor = 253;

	/// <summary>LegRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegRedemptionDate = 254;

	/// <summary>CreditRating, a <see cref="FixField.Text"/>.</summary>
	public const int CreditRating = 255;

	/// <summary>UnderlyingCreditRating, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCreditRating = 256;

	/// <summary>LegCreditRating, a <see cref="FixField.Text"/>.</summary>
	public const int LegCreditRating = 257;

	/// <summary>TradedFlatSwitch, a <see cref="FixField.Boolean"/>.</summary>
	public const int TradedFlatSwitch = 258;

	/// <summary>BasisFeatureDate, a <see cref="FixField.Date"/>.</summary>
	public const int BasisFeatureDate = 259;

	/// <summary>BasisFeaturePrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int BasisFeaturePrice = 260;

	/// <summary>MDReqID, a <see cref="FixField.Text"/>.</summary>
	public const int MDReqID = 262;

	/// <summary>SubscriptionRequestType, a <see cref="FixField.Character"/>.</summary>
	public const int SubscriptionRequestType = 263;

	/// <summary>MarketDepth, a <see cref="FixField.Integer"/>.</summary>
	public const int MarketDepth = 264;

	/// <summary>MDUpdateType, a <see cref="FixField.Integer"/>.</summary>
	public const int MDUpdateType = 265;

	/// <summary>AggregatedBook, a <see cref="FixField.Boolean"/>.</summary>
	public const int AggregatedBook = 266;

	/// <summary>NoMDEntryTypes, a <see cref="FixField.Integer"/>.</summary>
	public const int NoMDEntryTypes = 267;

	/// <summary>NoMDEntries, a <see cref="FixField.Integer"/>.</summary>
	public const int NoMDEntries = 268;

	/// <summary>MDEntryType, a <see cref="FixField.Character"/>.</summary>
	public const int MDEntryType = 269;

	/// <summary>MDEntryPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int MDEntryPx = 270;

	/// <summary>MDEntrySize, a <see cref="FixField.Decimal"/>.</summary>
	public const int MDEntrySize = 271;

	/// <summary>MDEntryDate, a <see cref="FixField.Date"/>.</summary>
	public const int MDEntryDate = 272;

	/// <summary>MDEntryTime, a <see cref="FixField.Time"/>.</summary>
	public const int MDEntryTime = 273;

	/// <summary>TickDirection, a <see cref="FixField.Character"/>.</summary>
	public const int TickDirection = 274;

	/// <summary>MDMkt, a <see cref="FixField.Text"/>.</summary>
	public const int MDMkt = 275;

	/// <summary>QuoteCondition, a <see cref="FixField.Multiple"/>.</summary>
	public const int QuoteCondition = 276;

	/// <summary>TradeCondition, a <see cref="FixField.Multiple"/>.</summary>
	public const int TradeCondition = 277;

	/// <summary>MDEntryID, a <see cref="FixField.Text"/>.</summary>
	public const int MDEntryID = 278;

	/// <summary>MDUpdateAction, a <see cref="FixField.Character"/>.</summary>
	public const int MDUpdateAction = 279;

	/// <summary>MDEntryRefID, a <see cref="FixField.Text"/>.</summary>
	public const int MDEntryRefID = 280;

	/// <summary>MDReqRejReason, a <see cref="FixField.Character"/>.</summary>
	public const int MDReqRejReason = 281;

	/// <summary>MDEntryOriginator, a <see cref="FixField.Text"/>.</summary>
	public const int MDEntryOriginator = 282;

	/// <summary>LocationID, a <see cref="FixField.Text"/>.</summary>
	public const int LocationID = 283;

	/// <summary>DeskID, a <see cref="FixField.Text"/>.</summary>
	public const int DeskID = 284;

	/// <summary>DeleteReason, a <see cref="FixField.Character"/>.</summary>
	public const int DeleteReason = 285;

	/// <summary>OpenCloseSettlFlag, a <see cref="FixField.Multiple"/>.</summary>
	public const int OpenCloseSettlFlag = 286;

	/// <summary>SellerDays, a <see cref="FixField.Integer"/>.</summary>
	public const int SellerDays = 287;

	/// <summary>MDEntryBuyer, a <see cref="FixField.Text"/>.</summary>
	public const int MDEntryBuyer = 288;

	/// <summary>MDEntrySeller, a <see cref="FixField.Text"/>.</summary>
	public const int MDEntrySeller = 289;

	/// <summary>MDEntryPositionNo, a <see cref="FixField.Integer"/>.</summary>
	public const int MDEntryPositionNo = 290;

	/// <summary>FinancialStatus, a <see cref="FixField.Multiple"/>.</summary>
	public const int FinancialStatus = 291;

	/// <summary>CorporateAction, a <see cref="FixField.Multiple"/>.</summary>
	public const int CorporateAction = 292;

	/// <summary>DefBidSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int DefBidSize = 293;

	/// <summary>DefOfferSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int DefOfferSize = 294;

	/// <summary>NoQuoteEntries, a <see cref="FixField.Integer"/>.</summary>
	public const int NoQuoteEntries = 295;

	/// <summary>NoQuoteSets, a <see cref="FixField.Integer"/>.</summary>
	public const int NoQuoteSets = 296;

	/// <summary>QuoteStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteStatus = 297;

	/// <summary>QuoteCancelType, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteCancelType = 298;

	/// <summary>QuoteEntryID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteEntryID = 299;

	/// <summary>QuoteRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteRejectReason = 300;

	/// <summary>QuoteResponseLevel, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteResponseLevel = 301;

	/// <summary>QuoteSetID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteSetID = 302;

	/// <summary>QuoteRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteRequestType = 303;

	/// <summary>TotNoQuoteEntries, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoQuoteEntries = 304;

	/// <summary>UnderlyingSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityIDSource = 305;

	/// <summary>UnderlyingIssuer, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingIssuer = 306;

	/// <summary>UnderlyingSecurityDesc, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityDesc = 307;

	/// <summary>UnderlyingSecurityExchange, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityExchange = 308;

	/// <summary>UnderlyingSecurityID, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityID = 309;

	/// <summary>UnderlyingSecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityType = 310;

	/// <summary>UnderlyingSymbol, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSymbol = 311;

	/// <summary>UnderlyingSymbolSfx, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSymbolSfx = 312;

	/// <summary>UnderlyingMaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	public const int UnderlyingMaturityMonthYear = 313;

	/// <summary>UnderlyingPutOrCall, a <see cref="FixField.Integer"/>.</summary>
	public const int UnderlyingPutOrCall = 315;

	/// <summary>UnderlyingStrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingStrikePrice = 316;

	/// <summary>UnderlyingOptAttribute, a <see cref="FixField.Character"/>.</summary>
	public const int UnderlyingOptAttribute = 317;

	/// <summary>UnderlyingCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCurrency = 318;

	/// <summary>SecurityReqID, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityReqID = 320;

	/// <summary>SecurityRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int SecurityRequestType = 321;

	/// <summary>SecurityResponseID, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityResponseID = 322;

	/// <summary>SecurityResponseType, a <see cref="FixField.Integer"/>.</summary>
	public const int SecurityResponseType = 323;

	/// <summary>SecurityStatusReqID, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityStatusReqID = 324;

	/// <summary>UnsolicitedIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int UnsolicitedIndicator = 325;

	/// <summary>SecurityTradingStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int SecurityTradingStatus = 326;

	/// <summary>HaltReason, a <see cref="FixField.Character"/>.</summary>
	public const int HaltReason = 327;

	/// <summary>InViewOfCommon, a <see cref="FixField.Boolean"/>.</summary>
	public const int InViewOfCommon = 328;

	/// <summary>DueToRelated, a <see cref="FixField.Boolean"/>.</summary>
	public const int DueToRelated = 329;

	/// <summary>BuyVolume, a <see cref="FixField.Decimal"/>.</summary>
	public const int BuyVolume = 330;

	/// <summary>SellVolume, a <see cref="FixField.Decimal"/>.</summary>
	public const int SellVolume = 331;

	/// <summary>HighPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int HighPx = 332;

	/// <summary>LowPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LowPx = 333;

	/// <summary>Adjustment, a <see cref="FixField.Integer"/>.</summary>
	public const int Adjustment = 334;

	/// <summary>TradSesReqID, a <see cref="FixField.Text"/>.</summary>
	public const int TradSesReqID = 335;

	/// <summary>TradingSessionID, a <see cref="FixField.Text"/>.</summary>
	public const int TradingSessionID = 336;

	/// <summary>ContraTrader, a <see cref="FixField.Text"/>.</summary>
	public const int ContraTrader = 337;

	/// <summary>TradSesMethod, a <see cref="FixField.Integer"/>.</summary>
	public const int TradSesMethod = 338;

	/// <summary>TradSesMode, a <see cref="FixField.Integer"/>.</summary>
	public const int TradSesMode = 339;

	/// <summary>TradSesStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int TradSesStatus = 340;

	/// <summary>TradSesStartTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TradSesStartTime = 341;

	/// <summary>TradSesOpenTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TradSesOpenTime = 342;

	/// <summary>TradSesPreCloseTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TradSesPreCloseTime = 343;

	/// <summary>TradSesCloseTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TradSesCloseTime = 344;

	/// <summary>TradSesEndTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TradSesEndTime = 345;

	/// <summary>NumberOfOrders, a <see cref="FixField.Integer"/>.</summary>
	public const int NumberOfOrders = 346;

	/// <summary>MessageEncoding, a <see cref="FixField.Text"/>.</summary>
	public const int MessageEncoding = 347;

	/// <summary>EncodedIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedIssuerLen = 348;

	/// <summary>EncodedIssuer, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedIssuer = 349;

	/// <summary>EncodedSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedSecurityDescLen = 350;

	/// <summary>EncodedSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedSecurityDesc = 351;

	/// <summary>EncodedListExecInstLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedListExecInstLen = 352;

	/// <summary>EncodedListExecInst, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedListExecInst = 353;

	/// <summary>EncodedTextLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedTextLen = 354;

	/// <summary>EncodedText, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedText = 355;

	/// <summary>EncodedSubjectLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedSubjectLen = 356;

	/// <summary>EncodedSubject, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedSubject = 357;

	/// <summary>EncodedHeadlineLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedHeadlineLen = 358;

	/// <summary>EncodedHeadline, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedHeadline = 359;

	/// <summary>EncodedAllocTextLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedAllocTextLen = 360;

	/// <summary>EncodedAllocText, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedAllocText = 361;

	/// <summary>EncodedUnderlyingIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedUnderlyingIssuerLen = 362;

	/// <summary>EncodedUnderlyingIssuer, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedUnderlyingIssuer = 363;

	/// <summary>EncodedUnderlyingSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedUnderlyingSecurityDescLen = 364;

	/// <summary>EncodedUnderlyingSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedUnderlyingSecurityDesc = 365;

	/// <summary>AllocPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocPrice = 366;

	/// <summary>QuoteSetValidUntilTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int QuoteSetValidUntilTime = 367;

	/// <summary>QuoteEntryRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteEntryRejectReason = 368;

	/// <summary>LastMsgSeqNumProcessed, a <see cref="FixField.Integer"/>.</summary>
	public const int LastMsgSeqNumProcessed = 369;

	/// <summary>RefTagID, a <see cref="FixField.Integer"/>.</summary>
	public const int RefTagID = 371;

	/// <summary>RefMsgType, a <see cref="FixField.Text"/>.</summary>
	public const int RefMsgType = 372;

	/// <summary>SessionRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int SessionRejectReason = 373;

	/// <summary>BidRequestTransType, a <see cref="FixField.Character"/>.</summary>
	public const int BidRequestTransType = 374;

	/// <summary>ContraBroker, a <see cref="FixField.Text"/>.</summary>
	public const int ContraBroker = 375;

	/// <summary>ComplianceID, a <see cref="FixField.Text"/>.</summary>
	public const int ComplianceID = 376;

	/// <summary>SolicitedFlag, a <see cref="FixField.Boolean"/>.</summary>
	public const int SolicitedFlag = 377;

	/// <summary>ExecRestatementReason, a <see cref="FixField.Integer"/>.</summary>
	public const int ExecRestatementReason = 378;

	/// <summary>BusinessRejectRefID, a <see cref="FixField.Text"/>.</summary>
	public const int BusinessRejectRefID = 379;

	/// <summary>BusinessRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int BusinessRejectReason = 380;

	/// <summary>GrossTradeAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int GrossTradeAmt = 381;

	/// <summary>NoContraBrokers, a <see cref="FixField.Integer"/>.</summary>
	public const int NoContraBrokers = 382;

	/// <summary>MaxMessageSize, a <see cref="FixField.Integer"/>.</summary>
	public const int MaxMessageSize = 383;

	/// <summary>NoMsgTypes, a <see cref="FixField.Integer"/>.</summary>
	public const int NoMsgTypes = 384;

	/// <summary>MsgDirection, a <see cref="FixField.Character"/>.</summary>
	public const int MsgDirection = 385;

	/// <summary>NoTradingSessions, a <see cref="FixField.Integer"/>.</summary>
	public const int NoTradingSessions = 386;

	/// <summary>TotalVolumeTraded, a <see cref="FixField.Decimal"/>.</summary>
	public const int TotalVolumeTraded = 387;

	/// <summary>DiscretionInst, a <see cref="FixField.Character"/>.</summary>
	public const int DiscretionInst = 388;

	/// <summary>DiscretionOffsetValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int DiscretionOffsetValue = 389;

	/// <summary>BidID, a <see cref="FixField.Text"/>.</summary>
	public const int BidID = 390;

	/// <summary>ClientBidID, a <see cref="FixField.Text"/>.</summary>
	public const int ClientBidID = 391;

	/// <summary>ListName, a <see cref="FixField.Text"/>.</summary>
	public const int ListName = 392;

	/// <summary>TotNoRelatedSym, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoRelatedSym = 393;

	/// <summary>BidType, a <see cref="FixField.Integer"/>.</summary>
	public const int BidType = 394;

	/// <summary>NumTickets, a <see cref="FixField.Integer"/>.</summary>
	public const int NumTickets = 395;

	/// <summary>SideValue1, a <see cref="FixField.Decimal"/>.</summary>
	public const int SideValue1 = 396;

	/// <summary>SideValue2, a <see cref="FixField.Decimal"/>.</summary>
	public const int SideValue2 = 397;

	/// <summary>NoBidDescriptors, a <see cref="FixField.Integer"/>.</summary>
	public const int NoBidDescriptors = 398;

	/// <summary>BidDescriptorType, a <see cref="FixField.Integer"/>.</summary>
	public const int BidDescriptorType = 399;

	/// <summary>BidDescriptor, a <see cref="FixField.Text"/>.</summary>
	public const int BidDescriptor = 400;

	/// <summary>SideValueInd, a <see cref="FixField.Integer"/>.</summary>
	public const int SideValueInd = 401;

	/// <summary>LiquidityPctLow, a <see cref="FixField.Decimal"/>.</summary>
	public const int LiquidityPctLow = 402;

	/// <summary>LiquidityPctHigh, a <see cref="FixField.Decimal"/>.</summary>
	public const int LiquidityPctHigh = 403;

	/// <summary>LiquidityValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int LiquidityValue = 404;

	/// <summary>EFPTrackingError, a <see cref="FixField.Decimal"/>.</summary>
	public const int EFPTrackingError = 405;

	/// <summary>FairValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int FairValue = 406;

	/// <summary>OutsideIndexPct, a <see cref="FixField.Decimal"/>.</summary>
	public const int OutsideIndexPct = 407;

	/// <summary>ValueOfFutures, a <see cref="FixField.Decimal"/>.</summary>
	public const int ValueOfFutures = 408;

	/// <summary>LiquidityIndType, a <see cref="FixField.Integer"/>.</summary>
	public const int LiquidityIndType = 409;

	/// <summary>WtAverageLiquidity, a <see cref="FixField.Decimal"/>.</summary>
	public const int WtAverageLiquidity = 410;

	/// <summary>ExchangeForPhysical, a <see cref="FixField.Boolean"/>.</summary>
	public const int ExchangeForPhysical = 411;

	/// <summary>OutMainCntryUIndex, a <see cref="FixField.Decimal"/>.</summary>
	public const int OutMainCntryUIndex = 412;

	/// <summary>CrossPercent, a <see cref="FixField.Decimal"/>.</summary>
	public const int CrossPercent = 413;

	/// <summary>ProgRptReqs, a <see cref="FixField.Integer"/>.</summary>
	public const int ProgRptReqs = 414;

	/// <summary>ProgPeriodInterval, a <see cref="FixField.Integer"/>.</summary>
	public const int ProgPeriodInterval = 415;

	/// <summary>IncTaxInd, a <see cref="FixField.Integer"/>.</summary>
	public const int IncTaxInd = 416;

	/// <summary>NumBidders, a <see cref="FixField.Integer"/>.</summary>
	public const int NumBidders = 417;

	/// <summary>BidTradeType, a <see cref="FixField.Character"/>.</summary>
	public const int BidTradeType = 418;

	/// <summary>BasisPxType, a <see cref="FixField.Character"/>.</summary>
	public const int BasisPxType = 419;

	/// <summary>NoBidComponents, a <see cref="FixField.Integer"/>.</summary>
	public const int NoBidComponents = 420;

	/// <summary>Country, a <see cref="FixField.Text"/>.</summary>
	public const int Country = 421;

	/// <summary>TotNoStrikes, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoStrikes = 422;

	/// <summary>PriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int PriceType = 423;

	/// <summary>DayOrderQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int DayOrderQty = 424;

	/// <summary>DayCumQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int DayCumQty = 425;

	/// <summary>DayAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int DayAvgPx = 426;

	/// <summary>GTBookingInst, a <see cref="FixField.Integer"/>.</summary>
	public const int GTBookingInst = 427;

	/// <summary>NoStrikes, a <see cref="FixField.Integer"/>.</summary>
	public const int NoStrikes = 428;

	/// <summary>ListStatusType, a <see cref="FixField.Integer"/>.</summary>
	public const int ListStatusType = 429;

	/// <summary>NetGrossInd, a <see cref="FixField.Integer"/>.</summary>
	public const int NetGrossInd = 430;

	/// <summary>ListOrderStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int ListOrderStatus = 431;

	/// <summary>ExpireDate, a <see cref="FixField.Date"/>.</summary>
	public const int ExpireDate = 432;

	/// <summary>ListExecInstType, a <see cref="FixField.Character"/>.</summary>
	public const int ListExecInstType = 433;

	/// <summary>CxlRejResponseTo, a <see cref="FixField.Character"/>.</summary>
	public const int CxlRejResponseTo = 434;

	/// <summary>UnderlyingCouponRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingCouponRate = 435;

	/// <summary>UnderlyingContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingContractMultiplier = 436;

	/// <summary>ContraTradeQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int ContraTradeQty = 437;

	/// <summary>ContraTradeTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int ContraTradeTime = 438;

	/// <summary>LiquidityNumSecurities, a <see cref="FixField.Integer"/>.</summary>
	public const int LiquidityNumSecurities = 441;

	/// <summary>MultiLegReportingType, a <see cref="FixField.Character"/>.</summary>
	public const int MultiLegReportingType = 442;

	/// <summary>StrikeTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int StrikeTime = 443;

	/// <summary>ListStatusText, a <see cref="FixField.Text"/>.</summary>
	public const int ListStatusText = 444;

	/// <summary>EncodedListStatusTextLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedListStatusTextLen = 445;

	/// <summary>EncodedListStatusText, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedListStatusText = 446;

	/// <summary>PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	public const int PartyIDSource = 447;

	/// <summary>PartyID, a <see cref="FixField.Text"/>.</summary>
	public const int PartyID = 448;

	/// <summary>NetChgPrevDay, a <see cref="FixField.Decimal"/>.</summary>
	public const int NetChgPrevDay = 451;

	/// <summary>PartyRole, a <see cref="FixField.Integer"/>.</summary>
	public const int PartyRole = 452;

	/// <summary>NoPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoPartyIDs = 453;

	/// <summary>NoSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSecurityAltID = 454;

	/// <summary>SecurityAltID, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityAltID = 455;

	/// <summary>SecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int SecurityAltIDSource = 456;

	/// <summary>NoUnderlyingSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	public const int NoUnderlyingSecurityAltID = 457;

	/// <summary>UnderlyingSecurityAltID, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityAltID = 458;

	/// <summary>UnderlyingSecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecurityAltIDSource = 459;

	/// <summary>Product, a <see cref="FixField.Integer"/>.</summary>
	public const int Product = 460;

	/// <summary>CFICode, a <see cref="FixField.Text"/>.</summary>
	public const int CFICode = 461;

	/// <summary>UnderlyingProduct, a <see cref="FixField.Integer"/>.</summary>
	public const int UnderlyingProduct = 462;

	/// <summary>UnderlyingCFICode, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCFICode = 463;

	/// <summary>TestMessageIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int TestMessageIndicator = 464;

	/// <summary>BookingRefID, a <see cref="FixField.Text"/>.</summary>
	public const int BookingRefID = 466;

	/// <summary>IndividualAllocID, a <see cref="FixField.Text"/>.</summary>
	public const int IndividualAllocID = 467;

	/// <summary>RoundingDirection, a <see cref="FixField.Character"/>.</summary>
	public const int RoundingDirection = 468;

	/// <summary>RoundingModulus, a <see cref="FixField.Decimal"/>.</summary>
	public const int RoundingModulus = 469;

	/// <summary>CountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int CountryOfIssue = 470;

	/// <summary>StateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int StateOrProvinceOfIssue = 471;

	/// <summary>LocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int LocaleOfIssue = 472;

	/// <summary>NoRegistDtls, a <see cref="FixField.Integer"/>.</summary>
	public const int NoRegistDtls = 473;

	/// <summary>MailingDtls, a <see cref="FixField.Text"/>.</summary>
	public const int MailingDtls = 474;

	/// <summary>InvestorCountryOfResidence, a <see cref="FixField.Text"/>.</summary>
	public const int InvestorCountryOfResidence = 475;

	/// <summary>PaymentRef, a <see cref="FixField.Text"/>.</summary>
	public const int PaymentRef = 476;

	/// <summary>DistribPaymentMethod, a <see cref="FixField.Integer"/>.</summary>
	public const int DistribPaymentMethod = 477;

	/// <summary>CashDistribCurr, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribCurr = 478;

	/// <summary>CommCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int CommCurrency = 479;

	/// <summary>CancellationRights, a <see cref="FixField.Character"/>.</summary>
	public const int CancellationRights = 480;

	/// <summary>MoneyLaunderingStatus, a <see cref="FixField.Character"/>.</summary>
	public const int MoneyLaunderingStatus = 481;

	/// <summary>MailingInst, a <see cref="FixField.Text"/>.</summary>
	public const int MailingInst = 482;

	/// <summary>TransBkdTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TransBkdTime = 483;

	/// <summary>ExecPriceType, a <see cref="FixField.Character"/>.</summary>
	public const int ExecPriceType = 484;

	/// <summary>ExecPriceAdjustment, a <see cref="FixField.Decimal"/>.</summary>
	public const int ExecPriceAdjustment = 485;

	/// <summary>DateOfBirth, a <see cref="FixField.Date"/>.</summary>
	public const int DateOfBirth = 486;

	/// <summary>TradeReportTransType, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeReportTransType = 487;

	/// <summary>CardHolderName, a <see cref="FixField.Text"/>.</summary>
	public const int CardHolderName = 488;

	/// <summary>CardNumber, a <see cref="FixField.Text"/>.</summary>
	public const int CardNumber = 489;

	/// <summary>CardExpDate, a <see cref="FixField.Date"/>.</summary>
	public const int CardExpDate = 490;

	/// <summary>CardIssNum, a <see cref="FixField.Text"/>.</summary>
	public const int CardIssNum = 491;

	/// <summary>PaymentMethod, a <see cref="FixField.Integer"/>.</summary>
	public const int PaymentMethod = 492;

	/// <summary>RegistAcctType, a <see cref="FixField.Text"/>.</summary>
	public const int RegistAcctType = 493;

	/// <summary>Designation, a <see cref="FixField.Text"/>.</summary>
	public const int Designation = 494;

	/// <summary>TaxAdvantageType, a <see cref="FixField.Integer"/>.</summary>
	public const int TaxAdvantageType = 495;

	/// <summary>RegistRejReasonText, a <see cref="FixField.Text"/>.</summary>
	public const int RegistRejReasonText = 496;

	/// <summary>FundRenewWaiv, a <see cref="FixField.Character"/>.</summary>
	public const int FundRenewWaiv = 497;

	/// <summary>CashDistribAgentName, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribAgentName = 498;

	/// <summary>CashDistribAgentCode, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribAgentCode = 499;

	/// <summary>CashDistribAgentAcctNumber, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribAgentAcctNumber = 500;

	/// <summary>CashDistribPayRef, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribPayRef = 501;

	/// <summary>CashDistribAgentAcctName, a <see cref="FixField.Text"/>.</summary>
	public const int CashDistribAgentAcctName = 502;

	/// <summary>CardStartDate, a <see cref="FixField.Date"/>.</summary>
	public const int CardStartDate = 503;

	/// <summary>PaymentDate, a <see cref="FixField.Date"/>.</summary>
	public const int PaymentDate = 504;

	/// <summary>PaymentRemitterID, a <see cref="FixField.Text"/>.</summary>
	public const int PaymentRemitterID = 505;

	/// <summary>RegistStatus, a <see cref="FixField.Character"/>.</summary>
	public const int RegistStatus = 506;

	/// <summary>RegistRejReasonCode, a <see cref="FixField.Integer"/>.</summary>
	public const int RegistRejReasonCode = 507;

	/// <summary>RegistRefID, a <see cref="FixField.Text"/>.</summary>
	public const int RegistRefID = 508;

	/// <summary>RegistDtls, a <see cref="FixField.Text"/>.</summary>
	public const int RegistDtls = 509;

	/// <summary>NoDistribInsts, a <see cref="FixField.Integer"/>.</summary>
	public const int NoDistribInsts = 510;

	/// <summary>RegistEmail, a <see cref="FixField.Text"/>.</summary>
	public const int RegistEmail = 511;

	/// <summary>DistribPercentage, a <see cref="FixField.Decimal"/>.</summary>
	public const int DistribPercentage = 512;

	/// <summary>RegistID, a <see cref="FixField.Text"/>.</summary>
	public const int RegistID = 513;

	/// <summary>RegistTransType, a <see cref="FixField.Character"/>.</summary>
	public const int RegistTransType = 514;

	/// <summary>ExecValuationPoint, a <see cref="FixField.Timestamp"/>.</summary>
	public const int ExecValuationPoint = 515;

	/// <summary>OrderPercent, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderPercent = 516;

	/// <summary>OwnershipType, a <see cref="FixField.Character"/>.</summary>
	public const int OwnershipType = 517;

	/// <summary>NoContAmts, a <see cref="FixField.Integer"/>.</summary>
	public const int NoContAmts = 518;

	/// <summary>ContAmtType, a <see cref="FixField.Integer"/>.</summary>
	public const int ContAmtType = 519;

	/// <summary>ContAmtValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int ContAmtValue = 520;

	/// <summary>ContAmtCurr, a <see cref="FixField.Text"/>.</summary>
	public const int ContAmtCurr = 521;

	/// <summary>OwnerType, a <see cref="FixField.Integer"/>.</summary>
	public const int OwnerType = 522;

	/// <summary>PartySubID, a <see cref="FixField.Text"/>.</summary>
	public const int PartySubID = 523;

	/// <summary>NestedPartyID, a <see cref="FixField.Text"/>.</summary>
	public const int NestedPartyID = 524;

	/// <summary>NestedPartyIDSource, a <see cref="FixField.Character"/>.</summary>
	public const int NestedPartyIDSource = 525;

	/// <summary>SecondaryClOrdID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryClOrdID = 526;

	/// <summary>SecondaryExecID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryExecID = 527;

	/// <summary>OrderCapacity, a <see cref="FixField.Character"/>.</summary>
	public const int OrderCapacity = 528;

	/// <summary>OrderRestrictions, a <see cref="FixField.Multiple"/>.</summary>
	public const int OrderRestrictions = 529;

	/// <summary>MassCancelRequestType, a <see cref="FixField.Character"/>.</summary>
	public const int MassCancelRequestType = 530;

	/// <summary>MassCancelResponse, a <see cref="FixField.Character"/>.</summary>
	public const int MassCancelResponse = 531;

	/// <summary>MassCancelRejectReason, a <see cref="FixField.Text"/>.</summary>
	public const int MassCancelRejectReason = 532;

	/// <summary>TotalAffectedOrders, a <see cref="FixField.Integer"/>.</summary>
	public const int TotalAffectedOrders = 533;

	/// <summary>NoAffectedOrders, a <see cref="FixField.Integer"/>.</summary>
	public const int NoAffectedOrders = 534;

	/// <summary>AffectedOrderID, a <see cref="FixField.Text"/>.</summary>
	public const int AffectedOrderID = 535;

	/// <summary>AffectedSecondaryOrderID, a <see cref="FixField.Text"/>.</summary>
	public const int AffectedSecondaryOrderID = 536;

	/// <summary>QuoteType, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteType = 537;

	/// <summary>NestedPartyRole, a <see cref="FixField.Integer"/>.</summary>
	public const int NestedPartyRole = 538;

	/// <summary>NoNestedPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNestedPartyIDs = 539;

	/// <summary>TotalAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int TotalAccruedInterestAmt = 540;

	/// <summary>MaturityDate, a <see cref="FixField.Date"/>.</summary>
	public const int MaturityDate = 541;

	/// <summary>UnderlyingMaturityDate, a <see cref="FixField.Date"/>.</summary>
	public const int UnderlyingMaturityDate = 542;

	/// <summary>InstrRegistry, a <see cref="FixField.Text"/>.</summary>
	public const int InstrRegistry = 543;

	/// <summary>CashMargin, a <see cref="FixField.Character"/>.</summary>
	public const int CashMargin = 544;

	/// <summary>NestedPartySubID, a <see cref="FixField.Text"/>.</summary>
	public const int NestedPartySubID = 545;

	/// <summary>Scope, a <see cref="FixField.Multiple"/>.</summary>
	public const int Scope = 546;

	/// <summary>MDImplicitDelete, a <see cref="FixField.Boolean"/>.</summary>
	public const int MDImplicitDelete = 547;

	/// <summary>CrossID, a <see cref="FixField.Text"/>.</summary>
	public const int CrossID = 548;

	/// <summary>CrossType, a <see cref="FixField.Integer"/>.</summary>
	public const int CrossType = 549;

	/// <summary>CrossPrioritization, a <see cref="FixField.Integer"/>.</summary>
	public const int CrossPrioritization = 550;

	/// <summary>OrigCrossID, a <see cref="FixField.Text"/>.</summary>
	public const int OrigCrossID = 551;

	/// <summary>NoSides, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSides = 552;

	/// <summary>Username, a <see cref="FixField.Text"/>.</summary>
	public const int Username = 553;

	/// <summary>Password, a <see cref="FixField.Text"/>.</summary>
	public const int Password = 554;

	/// <summary>NoLegs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoLegs = 555;

	/// <summary>LegCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int LegCurrency = 556;

	/// <summary>TotNoSecurityTypes, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoSecurityTypes = 557;

	/// <summary>NoSecurityTypes, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSecurityTypes = 558;

	/// <summary>SecurityListRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int SecurityListRequestType = 559;

	/// <summary>SecurityRequestResult, a <see cref="FixField.Integer"/>.</summary>
	public const int SecurityRequestResult = 560;

	/// <summary>RoundLot, a <see cref="FixField.Decimal"/>.</summary>
	public const int RoundLot = 561;

	/// <summary>MinTradeVol, a <see cref="FixField.Decimal"/>.</summary>
	public const int MinTradeVol = 562;

	/// <summary>MultiLegRptTypeReq, a <see cref="FixField.Integer"/>.</summary>
	public const int MultiLegRptTypeReq = 563;

	/// <summary>LegPositionEffect, a <see cref="FixField.Character"/>.</summary>
	public const int LegPositionEffect = 564;

	/// <summary>LegCoveredOrUncovered, a <see cref="FixField.Integer"/>.</summary>
	public const int LegCoveredOrUncovered = 565;

	/// <summary>LegPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegPrice = 566;

	/// <summary>TradSesStatusRejReason, a <see cref="FixField.Integer"/>.</summary>
	public const int TradSesStatusRejReason = 567;

	/// <summary>TradeRequestID, a <see cref="FixField.Text"/>.</summary>
	public const int TradeRequestID = 568;

	/// <summary>TradeRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeRequestType = 569;

	/// <summary>PreviouslyReported, a <see cref="FixField.Boolean"/>.</summary>
	public const int PreviouslyReported = 570;

	/// <summary>TradeReportID, a <see cref="FixField.Text"/>.</summary>
	public const int TradeReportID = 571;

	/// <summary>TradeReportRefID, a <see cref="FixField.Text"/>.</summary>
	public const int TradeReportRefID = 572;

	/// <summary>MatchStatus, a <see cref="FixField.Character"/>.</summary>
	public const int MatchStatus = 573;

	/// <summary>MatchType, a <see cref="FixField.Text"/>.</summary>
	public const int MatchType = 574;

	/// <summary>OddLot, a <see cref="FixField.Boolean"/>.</summary>
	public const int OddLot = 575;

	/// <summary>NoClearingInstructions, a <see cref="FixField.Integer"/>.</summary>
	public const int NoClearingInstructions = 576;

	/// <summary>ClearingInstruction, a <see cref="FixField.Integer"/>.</summary>
	public const int ClearingInstruction = 577;

	/// <summary>TradeInputSource, a <see cref="FixField.Text"/>.</summary>
	public const int TradeInputSource = 578;

	/// <summary>TradeInputDevice, a <see cref="FixField.Text"/>.</summary>
	public const int TradeInputDevice = 579;

	/// <summary>NoDates, a <see cref="FixField.Integer"/>.</summary>
	public const int NoDates = 580;

	/// <summary>AccountType, a <see cref="FixField.Integer"/>.</summary>
	public const int AccountType = 581;

	/// <summary>CustOrderCapacity, a <see cref="FixField.Integer"/>.</summary>
	public const int CustOrderCapacity = 582;

	/// <summary>ClOrdLinkID, a <see cref="FixField.Text"/>.</summary>
	public const int ClOrdLinkID = 583;

	/// <summary>MassStatusReqID, a <see cref="FixField.Text"/>.</summary>
	public const int MassStatusReqID = 584;

	/// <summary>MassStatusReqType, a <see cref="FixField.Integer"/>.</summary>
	public const int MassStatusReqType = 585;

	/// <summary>OrigOrdModTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int OrigOrdModTime = 586;

	/// <summary>LegSettlType, a <see cref="FixField.Character"/>.</summary>
	public const int LegSettlType = 587;

	/// <summary>LegSettlDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegSettlDate = 588;

	/// <summary>DayBookingInst, a <see cref="FixField.Character"/>.</summary>
	public const int DayBookingInst = 589;

	/// <summary>BookingUnit, a <see cref="FixField.Character"/>.</summary>
	public const int BookingUnit = 590;

	/// <summary>PreallocMethod, a <see cref="FixField.Character"/>.</summary>
	public const int PreallocMethod = 591;

	/// <summary>UnderlyingCountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCountryOfIssue = 592;

	/// <summary>UnderlyingStateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingStateOrProvinceOfIssue = 593;

	/// <summary>UnderlyingLocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingLocaleOfIssue = 594;

	/// <summary>UnderlyingInstrRegistry, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingInstrRegistry = 595;

	/// <summary>LegCountryOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int LegCountryOfIssue = 596;

	/// <summary>LegStateOrProvinceOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int LegStateOrProvinceOfIssue = 597;

	/// <summary>LegLocaleOfIssue, a <see cref="FixField.Text"/>.</summary>
	public const int LegLocaleOfIssue = 598;

	/// <summary>LegInstrRegistry, a <see cref="FixField.Text"/>.</summary>
	public const int LegInstrRegistry = 599;

	/// <summary>LegSymbol, a <see cref="FixField.Text"/>.</summary>
	public const int LegSymbol = 600;

	/// <summary>LegSymbolSfx, a <see cref="FixField.Text"/>.</summary>
	public const int LegSymbolSfx = 601;

	/// <summary>LegSecurityID, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityID = 602;

	/// <summary>LegSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityIDSource = 603;

	/// <summary>NoLegSecurityAltID, a <see cref="FixField.Integer"/>.</summary>
	public const int NoLegSecurityAltID = 604;

	/// <summary>LegSecurityAltID, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityAltID = 605;

	/// <summary>LegSecurityAltIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityAltIDSource = 606;

	/// <summary>LegProduct, a <see cref="FixField.Integer"/>.</summary>
	public const int LegProduct = 607;

	/// <summary>LegCFICode, a <see cref="FixField.Text"/>.</summary>
	public const int LegCFICode = 608;

	/// <summary>LegSecurityType, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityType = 609;

	/// <summary>LegMaturityMonthYear, a <see cref="FixField.MonthYear"/>.</summary>
	public const int LegMaturityMonthYear = 610;

	/// <summary>LegMaturityDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegMaturityDate = 611;

	/// <summary>LegStrikePrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegStrikePrice = 612;

	/// <summary>LegOptAttribute, a <see cref="FixField.Character"/>.</summary>
	public const int LegOptAttribute = 613;

	/// <summary>LegContractMultiplier, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegContractMultiplier = 614;

	/// <summary>LegCouponRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegCouponRate = 615;

	/// <summary>LegSecurityExchange, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityExchange = 616;

	/// <summary>LegIssuer, a <see cref="FixField.Text"/>.</summary>
	public const int LegIssuer = 617;

	/// <summary>EncodedLegIssuerLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedLegIssuerLen = 618;

	/// <summary>EncodedLegIssuer, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedLegIssuer = 619;

	/// <summary>LegSecurityDesc, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecurityDesc = 620;

	/// <summary>EncodedLegSecurityDescLen, a <see cref="FixField.Integer"/>.</summary>
	public const int EncodedLegSecurityDescLen = 621;

	/// <summary>EncodedLegSecurityDesc, a <see cref="FixField.Data"/>.</summary>
	public const int EncodedLegSecurityDesc = 622;

	/// <summary>LegRatioQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegRatioQty = 623;

	/// <summary>LegSide, a <see cref="FixField.Character"/>.</summary>
	public const int LegSide = 624;

	/// <summary>TradingSessionSubID, a <see cref="FixField.Text"/>.</summary>
	public const int TradingSessionSubID = 625;

	/// <summary>AllocType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocType = 626;

	/// <summary>NoHops, a <see cref="FixField.Integer"/>.</summary>
	public const int NoHops = 627;

	/// <summary>HopCompID, a <see cref="FixField.Text"/>.</summary>
	public const int HopCompID = 628;

	/// <summary>HopSendingTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int HopSendingTime = 629;

	/// <summary>HopRefID, a <see cref="FixField.Integer"/>.</summary>
	public const int HopRefID = 630;

	/// <summary>MidPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int MidPx = 631;

	/// <summary>BidYield, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidYield = 632;

	/// <summary>MidYield, a <see cref="FixField.Decimal"/>.</summary>
	public const int MidYield = 633;

	/// <summary>OfferYield, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferYield = 634;

	/// <summary>ClearingFeeIndicator, a <see cref="FixField.Text"/>.</summary>
	public const int ClearingFeeIndicator = 635;

	/// <summary>WorkingIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int WorkingIndicator = 636;

	/// <summary>LegLastPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegLastPx = 637;

	/// <summary>PriorityIndicator, a <see cref="FixField.Integer"/>.</summary>
	public const int PriorityIndicator = 638;

	/// <summary>PriceImprovement, a <see cref="FixField.Decimal"/>.</summary>
	public const int PriceImprovement = 639;

	/// <summary>Price2, a <see cref="FixField.Decimal"/>.</summary>
	public const int Price2 = 640;

	/// <summary>LastForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastForwardPoints2 = 641;

	/// <summary>BidForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	public const int BidForwardPoints2 = 642;

	/// <summary>OfferForwardPoints2, a <see cref="FixField.Decimal"/>.</summary>
	public const int OfferForwardPoints2 = 643;

	/// <summary>RFQReqID, a <see cref="FixField.Text"/>.</summary>
	public const int RFQReqID = 644;

	/// <summary>MktBidPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int MktBidPx = 645;

	/// <summary>MktOfferPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int MktOfferPx = 646;

	/// <summary>MinBidSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int MinBidSize = 647;

	/// <summary>MinOfferSize, a <see cref="FixField.Decimal"/>.</summary>
	public const int MinOfferSize = 648;

	/// <summary>QuoteStatusReqID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteStatusReqID = 649;

	/// <summary>LegalConfirm, a <see cref="FixField.Boolean"/>.</summary>
	public const int LegalConfirm = 650;

	/// <summary>UnderlyingLastPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingLastPx = 651;

	/// <summary>UnderlyingLastQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingLastQty = 652;

	/// <summary>LegRefID, a <see cref="FixField.Text"/>.</summary>
	public const int LegRefID = 654;

	/// <summary>ContraLegRefID, a <see cref="FixField.Text"/>.</summary>
	public const int ContraLegRefID = 655;

	/// <summary>SettlCurrBidFxRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int SettlCurrBidFxRate = 656;

	/// <summary>SettlCurrOfferFxRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int SettlCurrOfferFxRate = 657;

	/// <summary>QuoteRequestRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteRequestRejectReason = 658;

	/// <summary>SideComplianceID, a <see cref="FixField.Text"/>.</summary>
	public const int SideComplianceID = 659;

	/// <summary>AcctIDSource, a <see cref="FixField.Integer"/>.</summary>
	public const int AcctIDSource = 660;

	/// <summary>AllocAcctIDSource, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocAcctIDSource = 661;

	/// <summary>BenchmarkPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int BenchmarkPrice = 662;

	/// <summary>BenchmarkPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int BenchmarkPriceType = 663;

	/// <summary>ConfirmID, a <see cref="FixField.Text"/>.</summary>
	public const int ConfirmID = 664;

	/// <summary>ConfirmStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int ConfirmStatus = 665;

	/// <summary>ConfirmTransType, a <see cref="FixField.Integer"/>.</summary>
	public const int ConfirmTransType = 666;

	/// <summary>ContractSettlMonth, a <see cref="FixField.MonthYear"/>.</summary>
	public const int ContractSettlMonth = 667;

	/// <summary>DeliveryForm, a <see cref="FixField.Integer"/>.</summary>
	public const int DeliveryForm = 668;

	/// <summary>LastParPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LastParPx = 669;

	/// <summary>NoLegAllocs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoLegAllocs = 670;

	/// <summary>LegAllocAccount, a <see cref="FixField.Text"/>.</summary>
	public const int LegAllocAccount = 671;

	/// <summary>LegIndividualAllocID, a <see cref="FixField.Text"/>.</summary>
	public const int LegIndividualAllocID = 672;

	/// <summary>LegAllocQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegAllocQty = 673;

	/// <summary>LegAllocAcctIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int LegAllocAcctIDSource = 674;

	/// <summary>LegSettlCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int LegSettlCurrency = 675;

	/// <summary>LegBenchmarkCurveCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int LegBenchmarkCurveCurrency = 676;

	/// <summary>LegBenchmarkCurveName, a <see cref="FixField.Text"/>.</summary>
	public const int LegBenchmarkCurveName = 677;

	/// <summary>LegBenchmarkCurvePoint, a <see cref="FixField.Text"/>.</summary>
	public const int LegBenchmarkCurvePoint = 678;

	/// <summary>LegBenchmarkPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegBenchmarkPrice = 679;

	/// <summary>LegBenchmarkPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int LegBenchmarkPriceType = 680;

	/// <summary>LegBidPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegBidPx = 681;

	/// <summary>LegIOIQty, a <see cref="FixField.Text"/>.</summary>
	public const int LegIOIQty = 682;

	/// <summary>NoLegStipulations, a <see cref="FixField.Integer"/>.</summary>
	public const int NoLegStipulations = 683;

	/// <summary>LegOfferPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegOfferPx = 684;

	/// <summary>LegPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int LegPriceType = 686;

	/// <summary>LegQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LegQty = 687;

	/// <summary>LegStipulationType, a <see cref="FixField.Text"/>.</summary>
	public const int LegStipulationType = 688;

	/// <summary>LegStipulationValue, a <see cref="FixField.Text"/>.</summary>
	public const int LegStipulationValue = 689;

	/// <summary>LegSwapType, a <see cref="FixField.Integer"/>.</summary>
	public const int LegSwapType = 690;

	/// <summary>Pool, a <see cref="FixField.Text"/>.</summary>
	public const int Pool = 691;

	/// <summary>QuotePriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int QuotePriceType = 692;

	/// <summary>QuoteRespID, a <see cref="FixField.Text"/>.</summary>
	public const int QuoteRespID = 693;

	/// <summary>QuoteRespType, a <see cref="FixField.Integer"/>.</summary>
	public const int QuoteRespType = 694;

	/// <summary>QuoteQualifier, a <see cref="FixField.Character"/>.</summary>
	public const int QuoteQualifier = 695;

	/// <summary>YieldRedemptionDate, a <see cref="FixField.Date"/>.</summary>
	public const int YieldRedemptionDate = 696;

	/// <summary>YieldRedemptionPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int YieldRedemptionPrice = 697;

	/// <summary>YieldRedemptionPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int YieldRedemptionPriceType = 698;

	/// <summary>BenchmarkSecurityID, a <see cref="FixField.Text"/>.</summary>
	public const int BenchmarkSecurityID = 699;

	/// <summary>ReversalIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int ReversalIndicator = 700;

	/// <summary>YieldCalcDate, a <see cref="FixField.Date"/>.</summary>
	public const int YieldCalcDate = 701;

	/// <summary>NoPositions, a <see cref="FixField.Integer"/>.</summary>
	public const int NoPositions = 702;

	/// <summary>PosType, a <see cref="FixField.Text"/>.</summary>
	public const int PosType = 703;

	/// <summary>LongQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int LongQty = 704;

	/// <summary>ShortQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int ShortQty = 705;

	/// <summary>PosQtyStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int PosQtyStatus = 706;

	/// <summary>PosAmtType, a <see cref="FixField.Text"/>.</summary>
	public const int PosAmtType = 707;

	/// <summary>PosAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int PosAmt = 708;

	/// <summary>PosTransType, a <see cref="FixField.Integer"/>.</summary>
	public const int PosTransType = 709;

	/// <summary>PosReqID, a <see cref="FixField.Text"/>.</summary>
	public const int PosReqID = 710;

	/// <summary>NoUnderlyings, a <see cref="FixField.Integer"/>.</summary>
	public const int NoUnderlyings = 711;

	/// <summary>PosMaintAction, a <see cref="FixField.Integer"/>.</summary>
	public const int PosMaintAction = 712;

	/// <summary>OrigPosReqRefID, a <see cref="FixField.Text"/>.</summary>
	public const int OrigPosReqRefID = 713;

	/// <summary>PosMaintRptRefID, a <see cref="FixField.Text"/>.</summary>
	public const int PosMaintRptRefID = 714;

	/// <summary>ClearingBusinessDate, a <see cref="FixField.Date"/>.</summary>
	public const int ClearingBusinessDate = 715;

	/// <summary>SettlSessID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlSessID = 716;

	/// <summary>SettlSessSubID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlSessSubID = 717;

	/// <summary>AdjustmentType, a <see cref="FixField.Integer"/>.</summary>
	public const int AdjustmentType = 718;

	/// <summary>ContraryInstructionIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int ContraryInstructionIndicator = 719;

	/// <summary>PriorSpreadIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int PriorSpreadIndicator = 720;

	/// <summary>PosMaintRptID, a <see cref="FixField.Text"/>.</summary>
	public const int PosMaintRptID = 721;

	/// <summary>PosMaintStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int PosMaintStatus = 722;

	/// <summary>PosMaintResult, a <see cref="FixField.Integer"/>.</summary>
	public const int PosMaintResult = 723;

	/// <summary>PosReqType, a <see cref="FixField.Integer"/>.</summary>
	public const int PosReqType = 724;

	/// <summary>ResponseTransportType, a <see cref="FixField.Integer"/>.</summary>
	public const int ResponseTransportType = 725;

	/// <summary>ResponseDestination, a <see cref="FixField.Text"/>.</summary>
	public const int ResponseDestination = 726;

	/// <summary>TotalNumPosReports, a <see cref="FixField.Integer"/>.</summary>
	public const int TotalNumPosReports = 727;

	/// <summary>PosReqResult, a <see cref="FixField.Integer"/>.</summary>
	public const int PosReqResult = 728;

	/// <summary>PosReqStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int PosReqStatus = 729;

	/// <summary>SettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int SettlPrice = 730;

	/// <summary>SettlPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int SettlPriceType = 731;

	/// <summary>UnderlyingSettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingSettlPrice = 732;

	/// <summary>UnderlyingSettlPriceType, a <see cref="FixField.Integer"/>.</summary>
	public const int UnderlyingSettlPriceType = 733;

	/// <summary>PriorSettlPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int PriorSettlPrice = 734;

	/// <summary>NoQuoteQualifiers, a <see cref="FixField.Integer"/>.</summary>
	public const int NoQuoteQualifiers = 735;

	/// <summary>AllocSettlCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int AllocSettlCurrency = 736;

	/// <summary>AllocSettlCurrAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocSettlCurrAmt = 737;

	/// <summary>InterestAtMaturity, a <see cref="FixField.Decimal"/>.</summary>
	public const int InterestAtMaturity = 738;

	/// <summary>LegDatedDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegDatedDate = 739;

	/// <summary>LegPool, a <see cref="FixField.Text"/>.</summary>
	public const int LegPool = 740;

	/// <summary>AllocInterestAtMaturity, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocInterestAtMaturity = 741;

	/// <summary>AllocAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllocAccruedInterestAmt = 742;

	/// <summary>DeliveryDate, a <see cref="FixField.Date"/>.</summary>
	public const int DeliveryDate = 743;

	/// <summary>AssignmentMethod, a <see cref="FixField.Character"/>.</summary>
	public const int AssignmentMethod = 744;

	/// <summary>AssignmentUnit, a <see cref="FixField.Decimal"/>.</summary>
	public const int AssignmentUnit = 745;

	/// <summary>OpenInterest, a <see cref="FixField.Decimal"/>.</summary>
	public const int OpenInterest = 746;

	/// <summary>ExerciseMethod, a <see cref="FixField.Character"/>.</summary>
	public const int ExerciseMethod = 747;

	/// <summary>TotNumTradeReports, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNumTradeReports = 748;

	/// <summary>TradeRequestResult, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeRequestResult = 749;

	/// <summary>TradeRequestStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeRequestStatus = 750;

	/// <summary>TradeReportRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeReportRejectReason = 751;

	/// <summary>SideMultiLegReportingType, a <see cref="FixField.Integer"/>.</summary>
	public const int SideMultiLegReportingType = 752;

	/// <summary>NoPosAmt, a <see cref="FixField.Integer"/>.</summary>
	public const int NoPosAmt = 753;

	/// <summary>AutoAcceptIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int AutoAcceptIndicator = 754;

	/// <summary>AllocReportID, a <see cref="FixField.Text"/>.</summary>
	public const int AllocReportID = 755;

	/// <summary>NoNested2PartyIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNested2PartyIDs = 756;

	/// <summary>Nested2PartyID, a <see cref="FixField.Text"/>.</summary>
	public const int Nested2PartyID = 757;

	/// <summary>Nested2PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	public const int Nested2PartyIDSource = 758;

	/// <summary>Nested2PartyRole, a <see cref="FixField.Integer"/>.</summary>
	public const int Nested2PartyRole = 759;

	/// <summary>Nested2PartySubID, a <see cref="FixField.Text"/>.</summary>
	public const int Nested2PartySubID = 760;

	/// <summary>BenchmarkSecurityIDSource, a <see cref="FixField.Text"/>.</summary>
	public const int BenchmarkSecurityIDSource = 761;

	/// <summary>SecuritySubType, a <see cref="FixField.Text"/>.</summary>
	public const int SecuritySubType = 762;

	/// <summary>UnderlyingSecuritySubType, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingSecuritySubType = 763;

	/// <summary>LegSecuritySubType, a <see cref="FixField.Text"/>.</summary>
	public const int LegSecuritySubType = 764;

	/// <summary>AllowableOneSidednessPct, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllowableOneSidednessPct = 765;

	/// <summary>AllowableOneSidednessValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int AllowableOneSidednessValue = 766;

	/// <summary>AllowableOneSidednessCurr, a <see cref="FixField.Text"/>.</summary>
	public const int AllowableOneSidednessCurr = 767;

	/// <summary>NoTrdRegTimestamps, a <see cref="FixField.Integer"/>.</summary>
	public const int NoTrdRegTimestamps = 768;

	/// <summary>TrdRegTimestamp, a <see cref="FixField.Timestamp"/>.</summary>
	public const int TrdRegTimestamp = 769;

	/// <summary>TrdRegTimestampType, a <see cref="FixField.Integer"/>.</summary>
	public const int TrdRegTimestampType = 770;

	/// <summary>TrdRegTimestampOrigin, a <see cref="FixField.Text"/>.</summary>
	public const int TrdRegTimestampOrigin = 771;

	/// <summary>ConfirmRefID, a <see cref="FixField.Text"/>.</summary>
	public const int ConfirmRefID = 772;

	/// <summary>ConfirmType, a <see cref="FixField.Integer"/>.</summary>
	public const int ConfirmType = 773;

	/// <summary>ConfirmRejReason, a <see cref="FixField.Integer"/>.</summary>
	public const int ConfirmRejReason = 774;

	/// <summary>BookingType, a <see cref="FixField.Integer"/>.</summary>
	public const int BookingType = 775;

	/// <summary>IndividualAllocRejCode, a <see cref="FixField.Integer"/>.</summary>
	public const int IndividualAllocRejCode = 776;

	/// <summary>SettlInstMsgID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlInstMsgID = 777;

	/// <summary>NoSettlInst, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSettlInst = 778;

	/// <summary>LastUpdateTime, a <see cref="FixField.Timestamp"/>.</summary>
	public const int LastUpdateTime = 779;

	/// <summary>AllocSettlInstType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocSettlInstType = 780;

	/// <summary>NoSettlPartyIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSettlPartyIDs = 781;

	/// <summary>SettlPartyID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlPartyID = 782;

	/// <summary>SettlPartyIDSource, a <see cref="FixField.Character"/>.</summary>
	public const int SettlPartyIDSource = 783;

	/// <summary>SettlPartyRole, a <see cref="FixField.Integer"/>.</summary>
	public const int SettlPartyRole = 784;

	/// <summary>SettlPartySubID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlPartySubID = 785;

	/// <summary>SettlPartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	public const int SettlPartySubIDType = 786;

	/// <summary>DlvyInstType, a <see cref="FixField.Character"/>.</summary>
	public const int DlvyInstType = 787;

	/// <summary>TerminationType, a <see cref="FixField.Integer"/>.</summary>
	public const int TerminationType = 788;

	/// <summary>NextExpectedMsgSeqNum, a <see cref="FixField.Integer"/>.</summary>
	public const int NextExpectedMsgSeqNum = 789;

	/// <summary>OrdStatusReqID, a <see cref="FixField.Text"/>.</summary>
	public const int OrdStatusReqID = 790;

	/// <summary>SettlInstReqID, a <see cref="FixField.Text"/>.</summary>
	public const int SettlInstReqID = 791;

	/// <summary>SettlInstReqRejCode, a <see cref="FixField.Integer"/>.</summary>
	public const int SettlInstReqRejCode = 792;

	/// <summary>SecondaryAllocID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryAllocID = 793;

	/// <summary>AllocReportType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocReportType = 794;

	/// <summary>AllocReportRefID, a <see cref="FixField.Text"/>.</summary>
	public const int AllocReportRefID = 795;

	/// <summary>AllocCancReplaceReason, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocCancReplaceReason = 796;

	/// <summary>CopyMsgIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int CopyMsgIndicator = 797;

	/// <summary>AllocAccountType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocAccountType = 798;

	/// <summary>OrderAvgPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderAvgPx = 799;

	/// <summary>OrderBookingQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderBookingQty = 800;

	/// <summary>NoSettlPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoSettlPartySubIDs = 801;

	/// <summary>NoPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoPartySubIDs = 802;

	/// <summary>PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	public const int PartySubIDType = 803;

	/// <summary>NoNestedPartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNestedPartySubIDs = 804;

	/// <summary>NestedPartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	public const int NestedPartySubIDType = 805;

	/// <summary>NoNested2PartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNested2PartySubIDs = 806;

	/// <summary>Nested2PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	public const int Nested2PartySubIDType = 807;

	/// <summary>AllocIntermedReqType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocIntermedReqType = 808;

	/// <summary>UnderlyingPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingPx = 810;

	/// <summary>PriceDelta, a <see cref="FixField.Decimal"/>.</summary>
	public const int PriceDelta = 811;

	/// <summary>ApplQueueMax, a <see cref="FixField.Integer"/>.</summary>
	public const int ApplQueueMax = 812;

	/// <summary>ApplQueueDepth, a <see cref="FixField.Integer"/>.</summary>
	public const int ApplQueueDepth = 813;

	/// <summary>ApplQueueResolution, a <see cref="FixField.Integer"/>.</summary>
	public const int ApplQueueResolution = 814;

	/// <summary>ApplQueueAction, a <see cref="FixField.Integer"/>.</summary>
	public const int ApplQueueAction = 815;

	/// <summary>NoAltMDSource, a <see cref="FixField.Integer"/>.</summary>
	public const int NoAltMDSource = 816;

	/// <summary>AltMDSourceID, a <see cref="FixField.Text"/>.</summary>
	public const int AltMDSourceID = 817;

	/// <summary>SecondaryTradeReportID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryTradeReportID = 818;

	/// <summary>AvgPxIndicator, a <see cref="FixField.Integer"/>.</summary>
	public const int AvgPxIndicator = 819;

	/// <summary>TradeLinkID, a <see cref="FixField.Text"/>.</summary>
	public const int TradeLinkID = 820;

	/// <summary>OrderInputDevice, a <see cref="FixField.Text"/>.</summary>
	public const int OrderInputDevice = 821;

	/// <summary>UnderlyingTradingSessionID, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingTradingSessionID = 822;

	/// <summary>UnderlyingTradingSessionSubID, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingTradingSessionSubID = 823;

	/// <summary>TradeLegRefID, a <see cref="FixField.Text"/>.</summary>
	public const int TradeLegRefID = 824;

	/// <summary>ExchangeRule, a <see cref="FixField.Text"/>.</summary>
	public const int ExchangeRule = 825;

	/// <summary>TradeAllocIndicator, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeAllocIndicator = 826;

	/// <summary>ExpirationCycle, a <see cref="FixField.Integer"/>.</summary>
	public const int ExpirationCycle = 827;

	/// <summary>TrdType, a <see cref="FixField.Integer"/>.</summary>
	public const int TrdType = 828;

	/// <summary>TrdSubType, a <see cref="FixField.Integer"/>.</summary>
	public const int TrdSubType = 829;

	/// <summary>TransferReason, a <see cref="FixField.Text"/>.</summary>
	public const int TransferReason = 830;

	/// <summary>TotNumAssignmentReports, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNumAssignmentReports = 832;

	/// <summary>AsgnRptID, a <see cref="FixField.Text"/>.</summary>
	public const int AsgnRptID = 833;

	/// <summary>ThresholdAmount, a <see cref="FixField.Decimal"/>.</summary>
	public const int ThresholdAmount = 834;

	/// <summary>PegMoveType, a <see cref="FixField.Integer"/>.</summary>
	public const int PegMoveType = 835;

	/// <summary>PegOffsetType, a <see cref="FixField.Integer"/>.</summary>
	public const int PegOffsetType = 836;

	/// <summary>PegLimitType, a <see cref="FixField.Integer"/>.</summary>
	public const int PegLimitType = 837;

	/// <summary>PegRoundDirection, a <see cref="FixField.Integer"/>.</summary>
	public const int PegRoundDirection = 838;

	/// <summary>PeggedPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int PeggedPrice = 839;

	/// <summary>PegScope, a <see cref="FixField.Integer"/>.</summary>
	public const int PegScope = 840;

	/// <summary>DiscretionMoveType, a <see cref="FixField.Integer"/>.</summary>
	public const int DiscretionMoveType = 841;

	/// <summary>DiscretionOffsetType, a <see cref="FixField.Integer"/>.</summary>
	public const int DiscretionOffsetType = 842;

	/// <summary>DiscretionLimitType, a <see cref="FixField.Integer"/>.</summary>
	public const int DiscretionLimitType = 843;

	/// <summary>DiscretionRoundDirection, a <see cref="FixField.Integer"/>.</summary>
	public const int DiscretionRoundDirection = 844;

	/// <summary>DiscretionPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int DiscretionPrice = 845;

	/// <summary>DiscretionScope, a <see cref="FixField.Integer"/>.</summary>
	public const int DiscretionScope = 846;

	/// <summary>TargetStrategy, a <see cref="FixField.Integer"/>.</summary>
	public const int TargetStrategy = 847;

	/// <summary>TargetStrategyParameters, a <see cref="FixField.Text"/>.</summary>
	public const int TargetStrategyParameters = 848;

	/// <summary>ParticipationRate, a <see cref="FixField.Decimal"/>.</summary>
	public const int ParticipationRate = 849;

	/// <summary>TargetStrategyPerformance, a <see cref="FixField.Decimal"/>.</summary>
	public const int TargetStrategyPerformance = 850;

	/// <summary>LastLiquidityInd, a <see cref="FixField.Integer"/>.</summary>
	public const int LastLiquidityInd = 851;

	/// <summary>PublishTrdIndicator, a <see cref="FixField.Boolean"/>.</summary>
	public const int PublishTrdIndicator = 852;

	/// <summary>ShortSaleReason, a <see cref="FixField.Integer"/>.</summary>
	public const int ShortSaleReason = 853;

	/// <summary>QtyType, a <see cref="FixField.Integer"/>.</summary>
	public const int QtyType = 854;

	/// <summary>SecondaryTrdType, a <see cref="FixField.Integer"/>.</summary>
	public const int SecondaryTrdType = 855;

	/// <summary>TradeReportType, a <see cref="FixField.Integer"/>.</summary>
	public const int TradeReportType = 856;

	/// <summary>AllocNoOrdersType, a <see cref="FixField.Integer"/>.</summary>
	public const int AllocNoOrdersType = 857;

	/// <summary>SharedCommission, a <see cref="FixField.Decimal"/>.</summary>
	public const int SharedCommission = 858;

	/// <summary>ConfirmReqID, a <see cref="FixField.Text"/>.</summary>
	public const int ConfirmReqID = 859;

	/// <summary>AvgParPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int AvgParPx = 860;

	/// <summary>ReportedPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int ReportedPx = 861;

	/// <summary>NoCapacities, a <see cref="FixField.Integer"/>.</summary>
	public const int NoCapacities = 862;

	/// <summary>OrderCapacityQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int OrderCapacityQty = 863;

	/// <summary>NoEvents, a <see cref="FixField.Integer"/>.</summary>
	public const int NoEvents = 864;

	/// <summary>EventType, a <see cref="FixField.Integer"/>.</summary>
	public const int EventType = 865;

	/// <summary>EventDate, a <see cref="FixField.Date"/>.</summary>
	public const int EventDate = 866;

	/// <summary>EventPx, a <see cref="FixField.Decimal"/>.</summary>
	public const int EventPx = 867;

	/// <summary>EventText, a <see cref="FixField.Text"/>.</summary>
	public const int EventText = 868;

	/// <summary>PctAtRisk, a <see cref="FixField.Decimal"/>.</summary>
	public const int PctAtRisk = 869;

	/// <summary>NoInstrAttrib, a <see cref="FixField.Integer"/>.</summary>
	public const int NoInstrAttrib = 870;

	/// <summary>InstrAttribType, a <see cref="FixField.Integer"/>.</summary>
	public const int InstrAttribType = 871;

	/// <summary>InstrAttribValue, a <see cref="FixField.Text"/>.</summary>
	public const int InstrAttribValue = 872;

	/// <summary>DatedDate, a <see cref="FixField.Date"/>.</summary>
	public const int DatedDate = 873;

	/// <summary>InterestAccrualDate, a <see cref="FixField.Date"/>.</summary>
	public const int InterestAccrualDate = 874;

	/// <summary>CPProgram, a <see cref="FixField.Integer"/>.</summary>
	public const int CPProgram = 875;

	/// <summary>CPRegType, a <see cref="FixField.Text"/>.</summary>
	public const int CPRegType = 876;

	/// <summary>UnderlyingCPProgram, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCPProgram = 877;

	/// <summary>UnderlyingCPRegType, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingCPRegType = 878;

	/// <summary>UnderlyingQty, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingQty = 879;

	/// <summary>TrdMatchID, a <see cref="FixField.Text"/>.</summary>
	public const int TrdMatchID = 880;

	/// <summary>SecondaryTradeReportRefID, a <see cref="FixField.Text"/>.</summary>
	public const int SecondaryTradeReportRefID = 881;

	/// <summary>UnderlyingDirtyPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingDirtyPrice = 882;

	/// <summary>UnderlyingEndPrice, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingEndPrice = 883;

	/// <summary>UnderlyingStartValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingStartValue = 884;

	/// <summary>UnderlyingCurrentValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingCurrentValue = 885;

	/// <summary>UnderlyingEndValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int UnderlyingEndValue = 886;

	/// <summary>NoUnderlyingStips, a <see cref="FixField.Integer"/>.</summary>
	public const int NoUnderlyingStips = 887;

	/// <summary>UnderlyingStipType, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingStipType = 888;

	/// <summary>UnderlyingStipValue, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingStipValue = 889;

	/// <summary>MaturityNetMoney, a <see cref="FixField.Decimal"/>.</summary>
	public const int MaturityNetMoney = 890;

	/// <summary>MiscFeeBasis, a <see cref="FixField.Integer"/>.</summary>
	public const int MiscFeeBasis = 891;

	/// <summary>TotNoAllocs, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNoAllocs = 892;

	/// <summary>LastFragment, a <see cref="FixField.Boolean"/>.</summary>
	public const int LastFragment = 893;

	/// <summary>CollReqID, a <see cref="FixField.Text"/>.</summary>
	public const int CollReqID = 894;

	/// <summary>CollAsgnReason, a <see cref="FixField.Integer"/>.</summary>
	public const int CollAsgnReason = 895;

	/// <summary>CollInquiryQualifier, a <see cref="FixField.Integer"/>.</summary>
	public const int CollInquiryQualifier = 896;

	/// <summary>NoTrades, a <see cref="FixField.Integer"/>.</summary>
	public const int NoTrades = 897;

	/// <summary>MarginRatio, a <see cref="FixField.Decimal"/>.</summary>
	public const int MarginRatio = 898;

	/// <summary>MarginExcess, a <see cref="FixField.Decimal"/>.</summary>
	public const int MarginExcess = 899;

	/// <summary>TotalNetValue, a <see cref="FixField.Decimal"/>.</summary>
	public const int TotalNetValue = 900;

	/// <summary>CashOutstanding, a <see cref="FixField.Decimal"/>.</summary>
	public const int CashOutstanding = 901;

	/// <summary>CollAsgnID, a <see cref="FixField.Text"/>.</summary>
	public const int CollAsgnID = 902;

	/// <summary>CollAsgnTransType, a <see cref="FixField.Integer"/>.</summary>
	public const int CollAsgnTransType = 903;

	/// <summary>CollRespID, a <see cref="FixField.Text"/>.</summary>
	public const int CollRespID = 904;

	/// <summary>CollAsgnRespType, a <see cref="FixField.Integer"/>.</summary>
	public const int CollAsgnRespType = 905;

	/// <summary>CollAsgnRejectReason, a <see cref="FixField.Integer"/>.</summary>
	public const int CollAsgnRejectReason = 906;

	/// <summary>CollAsgnRefID, a <see cref="FixField.Text"/>.</summary>
	public const int CollAsgnRefID = 907;

	/// <summary>CollRptID, a <see cref="FixField.Text"/>.</summary>
	public const int CollRptID = 908;

	/// <summary>CollInquiryID, a <see cref="FixField.Text"/>.</summary>
	public const int CollInquiryID = 909;

	/// <summary>CollStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int CollStatus = 910;

	/// <summary>TotNumReports, a <see cref="FixField.Integer"/>.</summary>
	public const int TotNumReports = 911;

	/// <summary>LastRptRequested, a <see cref="FixField.Boolean"/>.</summary>
	public const int LastRptRequested = 912;

	/// <summary>AgreementDesc, a <see cref="FixField.Text"/>.</summary>
	public const int AgreementDesc = 913;

	/// <summary>AgreementID, a <see cref="FixField.Text"/>.</summary>
	public const int AgreementID = 914;

	/// <summary>AgreementDate, a <see cref="FixField.Date"/>.</summary>
	public const int AgreementDate = 915;

	/// <summary>StartDate, a <see cref="FixField.Date"/>.</summary>
	public const int StartDate = 916;

	/// <summary>EndDate, a <see cref="FixField.Date"/>.</summary>
	public const int EndDate = 917;

	/// <summary>AgreementCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int AgreementCurrency = 918;

	/// <summary>DeliveryType, a <see cref="FixField.Integer"/>.</summary>
	public const int DeliveryType = 919;

	/// <summary>EndAccruedInterestAmt, a <see cref="FixField.Decimal"/>.</summary>
	public const int EndAccruedInterestAmt = 920;

	/// <summary>StartCash, a <see cref="FixField.Decimal"/>.</summary>
	public const int StartCash = 921;

	/// <summary>EndCash, a <see cref="FixField.Decimal"/>.</summary>
	public const int EndCash = 922;

	/// <summary>UserRequestID, a <see cref="FixField.Text"/>.</summary>
	public const int UserRequestID = 923;

	/// <summary>UserRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int UserRequestType = 924;

	/// <summary>NewPassword, a <see cref="FixField.Text"/>.</summary>
	public const int NewPassword = 925;

	/// <summary>UserStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int UserStatus = 926;

	/// <summary>UserStatusText, a <see cref="FixField.Text"/>.</summary>
	public const int UserStatusText = 927;

	/// <summary>StatusValue, a <see cref="FixField.Integer"/>.</summary>
	public const int StatusValue = 928;

	/// <summary>StatusText, a <see cref="FixField.Text"/>.</summary>
	public const int StatusText = 929;

	/// <summary>RefCompID, a <see cref="FixField.Text"/>.</summary>
	public const int RefCompID = 930;

	/// <summary>RefSubID, a <see cref="FixField.Text"/>.</summary>
	public const int RefSubID = 931;

	/// <summary>NetworkResponseID, a <see cref="FixField.Text"/>.</summary>
	public const int NetworkResponseID = 932;

	/// <summary>NetworkRequestID, a <see cref="FixField.Text"/>.</summary>
	public const int NetworkRequestID = 933;

	/// <summary>LastNetworkResponseID, a <see cref="FixField.Text"/>.</summary>
	public const int LastNetworkResponseID = 934;

	/// <summary>NetworkRequestType, a <see cref="FixField.Integer"/>.</summary>
	public const int NetworkRequestType = 935;

	/// <summary>NoCompIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoCompIDs = 936;

	/// <summary>NetworkStatusResponseType, a <see cref="FixField.Integer"/>.</summary>
	public const int NetworkStatusResponseType = 937;

	/// <summary>NoCollInquiryQualifier, a <see cref="FixField.Integer"/>.</summary>
	public const int NoCollInquiryQualifier = 938;

	/// <summary>TrdRptStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int TrdRptStatus = 939;

	/// <summary>AffirmStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int AffirmStatus = 940;

	/// <summary>UnderlyingStrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int UnderlyingStrikeCurrency = 941;

	/// <summary>LegStrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int LegStrikeCurrency = 942;

	/// <summary>TimeBracket, a <see cref="FixField.Text"/>.</summary>
	public const int TimeBracket = 943;

	/// <summary>CollAction, a <see cref="FixField.Integer"/>.</summary>
	public const int CollAction = 944;

	/// <summary>CollInquiryStatus, a <see cref="FixField.Integer"/>.</summary>
	public const int CollInquiryStatus = 945;

	/// <summary>CollInquiryResult, a <see cref="FixField.Integer"/>.</summary>
	public const int CollInquiryResult = 946;

	/// <summary>StrikeCurrency, a <see cref="FixField.Text"/>.</summary>
	public const int StrikeCurrency = 947;

	/// <summary>NoNested3PartyIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNested3PartyIDs = 948;

	/// <summary>Nested3PartyID, a <see cref="FixField.Text"/>.</summary>
	public const int Nested3PartyID = 949;

	/// <summary>Nested3PartyIDSource, a <see cref="FixField.Character"/>.</summary>
	public const int Nested3PartyIDSource = 950;

	/// <summary>Nested3PartyRole, a <see cref="FixField.Integer"/>.</summary>
	public const int Nested3PartyRole = 951;

	/// <summary>NoNested3PartySubIDs, a <see cref="FixField.Integer"/>.</summary>
	public const int NoNested3PartySubIDs = 952;

	/// <summary>Nested3PartySubID, a <see cref="FixField.Text"/>.</summary>
	public const int Nested3PartySubID = 953;

	/// <summary>Nested3PartySubIDType, a <see cref="FixField.Integer"/>.</summary>
	public const int Nested3PartySubIDType = 954;

	/// <summary>LegContractSettlMonth, a <see cref="FixField.MonthYear"/>.</summary>
	public const int LegContractSettlMonth = 955;

	/// <summary>LegInterestAccrualDate, a <see cref="FixField.Date"/>.</summary>
	public const int LegInterestAccrualDate = 956;
}
