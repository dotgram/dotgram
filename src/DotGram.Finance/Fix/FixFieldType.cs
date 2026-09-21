using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Known FIX field tags. Custom tags retain their numeric value when cast to this enum.
/// </summary>
public enum FixFieldType
{
	/// <summary>
	/// Identifies Account, FIX tag 1, with wire type <c>String</c>.
	/// </summary>
	Account                              = 1,
	/// <summary>
	/// Identifies AdvId, FIX tag 2, with wire type <c>String</c>.
	/// </summary>
	AdvId                                = 2,
	/// <summary>
	/// Identifies AdvRefID, FIX tag 3, with wire type <c>String</c>.
	/// </summary>
	AdvRefID                             = 3,
	/// <summary>
	/// Identifies AdvSide, FIX tag 4, with wire type <c>char</c>.
	/// </summary>
	AdvSide                              = 4,
	/// <summary>
	/// Identifies AdvTransType, FIX tag 5, with wire type <c>String</c>.
	/// </summary>
	AdvTransType                         = 5,
	/// <summary>
	/// Identifies AvgPx, FIX tag 6, with wire type <c>Price</c>.
	/// </summary>
	AvgPx                                = 6,
	/// <summary>
	/// Identifies BeginSeqNo, FIX tag 7, with wire type <c>SeqNum</c>.
	/// </summary>
	BeginSeqNo                           = 7,
	/// <summary>
	/// Identifies BeginString, FIX tag 8, with wire type <c>String</c>.
	/// </summary>
	BeginString                          = 8,
	/// <summary>
	/// Identifies BodyLength, FIX tag 9, with wire type <c>Length</c>.
	/// </summary>
	BodyLength                           = 9,
	/// <summary>
	/// Identifies CheckSum, FIX tag 10, with wire type <c>String</c>.
	/// </summary>
	CheckSum                             = 10,
	/// <summary>
	/// Identifies ClOrdID, FIX tag 11, with wire type <c>String</c>.
	/// </summary>
	ClOrdID                              = 11,
	/// <summary>
	/// Identifies Commission, FIX tag 12, with wire type <c>Amt</c>.
	/// </summary>
	Commission                           = 12,
	/// <summary>
	/// Identifies CommType, FIX tag 13, with wire type <c>char</c>.
	/// </summary>
	CommType                             = 13,
	/// <summary>
	/// Identifies CumQty, FIX tag 14, with wire type <c>Qty</c>.
	/// </summary>
	CumQty                               = 14,
	/// <summary>
	/// Identifies Currency, FIX tag 15, with wire type <c>Currency</c>.
	/// </summary>
	Currency                             = 15,
	/// <summary>
	/// Identifies EndSeqNo, FIX tag 16, with wire type <c>SeqNum</c>.
	/// </summary>
	EndSeqNo                             = 16,
	/// <summary>
	/// Identifies ExecID, FIX tag 17, with wire type <c>String</c>.
	/// </summary>
	ExecID                               = 17,
	/// <summary>
	/// Identifies ExecInst, FIX tag 18, with wire type <c>MultipleValueString</c>.
	/// </summary>
	ExecInst                             = 18,
	/// <summary>
	/// Identifies ExecRefID, FIX tag 19, with wire type <c>String</c>.
	/// </summary>
	ExecRefID                            = 19,
	/// <summary>
	/// Identifies HandlInst, FIX tag 21, with wire type <c>char</c>.
	/// </summary>
	HandlInst                            = 21,
	/// <summary>
	/// Identifies SecurityIDSource, FIX tag 22, with wire type <c>String</c>.
	/// </summary>
	SecurityIDSource                     = 22,
	/// <summary>
	/// Identifies IOIID, FIX tag 23, with wire type <c>String</c>.
	/// </summary>
	IOIID                                = 23,
	/// <summary>
	/// Identifies IOIQltyInd, FIX tag 25, with wire type <c>char</c>.
	/// </summary>
	IOIQltyInd                           = 25,
	/// <summary>
	/// Identifies IOIRefID, FIX tag 26, with wire type <c>String</c>.
	/// </summary>
	IOIRefID                             = 26,
	/// <summary>
	/// Identifies IOIQty, FIX tag 27, with wire type <c>String</c>.
	/// </summary>
	IOIQty                               = 27,
	/// <summary>
	/// Identifies IOITransType, FIX tag 28, with wire type <c>char</c>.
	/// </summary>
	IOITransType                         = 28,
	/// <summary>
	/// Identifies LastCapacity, FIX tag 29, with wire type <c>char</c>.
	/// </summary>
	LastCapacity                         = 29,
	/// <summary>
	/// Identifies LastMkt, FIX tag 30, with wire type <c>Exchange</c>.
	/// </summary>
	LastMkt                              = 30,
	/// <summary>
	/// Identifies LastPx, FIX tag 31, with wire type <c>Price</c>.
	/// </summary>
	LastPx                               = 31,
	/// <summary>
	/// Identifies LastQty, FIX tag 32, with wire type <c>Qty</c>.
	/// </summary>
	LastQty                              = 32,
	/// <summary>
	/// Identifies NoLinesOfText, FIX tag 33, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoLinesOfText                        = 33,
	/// <summary>
	/// Identifies MsgSeqNum, FIX tag 34, with wire type <c>SeqNum</c>.
	/// </summary>
	MsgSeqNum                            = 34,
	/// <summary>
	/// Identifies MsgType, FIX tag 35, with wire type <c>String</c>.
	/// </summary>
	MsgType                              = 35,
	/// <summary>
	/// Identifies NewSeqNo, FIX tag 36, with wire type <c>SeqNum</c>.
	/// </summary>
	NewSeqNo                             = 36,
	/// <summary>
	/// Identifies OrderID, FIX tag 37, with wire type <c>String</c>.
	/// </summary>
	OrderID                              = 37,
	/// <summary>
	/// Identifies OrderQty, FIX tag 38, with wire type <c>Qty</c>.
	/// </summary>
	OrderQty                             = 38,
	/// <summary>
	/// Identifies OrdStatus, FIX tag 39, with wire type <c>char</c>.
	/// </summary>
	OrdStatus                            = 39,
	/// <summary>
	/// Identifies OrdType, FIX tag 40, with wire type <c>char</c>.
	/// </summary>
	OrdType                              = 40,
	/// <summary>
	/// Identifies OrigClOrdID, FIX tag 41, with wire type <c>String</c>.
	/// </summary>
	OrigClOrdID                          = 41,
	/// <summary>
	/// Identifies OrigTime, FIX tag 42, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	OrigTime                             = 42,
	/// <summary>
	/// Identifies PossDupFlag, FIX tag 43, with wire type <c>Boolean</c>.
	/// </summary>
	PossDupFlag                          = 43,
	/// <summary>
	/// Identifies Price, FIX tag 44, with wire type <c>Price</c>.
	/// </summary>
	Price                                = 44,
	/// <summary>
	/// Identifies RefSeqNum, FIX tag 45, with wire type <c>SeqNum</c>.
	/// </summary>
	RefSeqNum                            = 45,
	/// <summary>
	/// Identifies SecurityID, FIX tag 48, with wire type <c>String</c>.
	/// </summary>
	SecurityID                           = 48,
	/// <summary>
	/// Identifies SenderCompID, FIX tag 49, with wire type <c>String</c>.
	/// </summary>
	SenderCompID                         = 49,
	/// <summary>
	/// Identifies SenderSubID, FIX tag 50, with wire type <c>String</c>.
	/// </summary>
	SenderSubID                          = 50,
	/// <summary>
	/// Identifies SendingTime, FIX tag 52, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	SendingTime                          = 52,
	/// <summary>
	/// Identifies Quantity, FIX tag 53, with wire type <c>Qty</c>.
	/// </summary>
	Quantity                             = 53,
	/// <summary>
	/// Identifies Side, FIX tag 54, with wire type <c>char</c>.
	/// </summary>
	Side                                 = 54,
	/// <summary>
	/// Identifies Symbol, FIX tag 55, with wire type <c>String</c>.
	/// </summary>
	Symbol                               = 55,
	/// <summary>
	/// Identifies TargetCompID, FIX tag 56, with wire type <c>String</c>.
	/// </summary>
	TargetCompID                         = 56,
	/// <summary>
	/// Identifies TargetSubID, FIX tag 57, with wire type <c>String</c>.
	/// </summary>
	TargetSubID                          = 57,
	/// <summary>
	/// Identifies Text, FIX tag 58, with wire type <c>String</c>.
	/// </summary>
	Text                                 = 58,
	/// <summary>
	/// Identifies TimeInForce, FIX tag 59, with wire type <c>char</c>.
	/// </summary>
	TimeInForce                          = 59,
	/// <summary>
	/// Identifies TransactTime, FIX tag 60, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TransactTime                         = 60,
	/// <summary>
	/// Identifies Urgency, FIX tag 61, with wire type <c>char</c>.
	/// </summary>
	Urgency                              = 61,
	/// <summary>
	/// Identifies ValidUntilTime, FIX tag 62, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	ValidUntilTime                       = 62,
	/// <summary>
	/// Identifies SettlType, FIX tag 63, with wire type <c>char</c>.
	/// </summary>
	SettlType                            = 63,
	/// <summary>
	/// Identifies SettlDate, FIX tag 64, with wire type <c>LocalMktDate</c>.
	/// </summary>
	SettlDate                            = 64,
	/// <summary>
	/// Identifies SymbolSfx, FIX tag 65, with wire type <c>String</c>.
	/// </summary>
	SymbolSfx                            = 65,
	/// <summary>
	/// Identifies ListID, FIX tag 66, with wire type <c>String</c>.
	/// </summary>
	ListID                               = 66,
	/// <summary>
	/// Identifies ListSeqNo, FIX tag 67, with wire type <c>int</c>.
	/// </summary>
	ListSeqNo                            = 67,
	/// <summary>
	/// Identifies TotNoOrders, FIX tag 68, with wire type <c>int</c>.
	/// </summary>
	TotNoOrders                          = 68,
	/// <summary>
	/// Identifies ListExecInst, FIX tag 69, with wire type <c>String</c>.
	/// </summary>
	ListExecInst                         = 69,
	/// <summary>
	/// Identifies AllocID, FIX tag 70, with wire type <c>String</c>.
	/// </summary>
	AllocID                              = 70,
	/// <summary>
	/// Identifies AllocTransType, FIX tag 71, with wire type <c>char</c>.
	/// </summary>
	AllocTransType                       = 71,
	/// <summary>
	/// Identifies RefAllocID, FIX tag 72, with wire type <c>String</c>.
	/// </summary>
	RefAllocID                           = 72,
	/// <summary>
	/// Identifies NoOrders, FIX tag 73, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoOrders                             = 73,
	/// <summary>
	/// Identifies AvgPxPrecision, FIX tag 74, with wire type <c>int</c>.
	/// </summary>
	AvgPxPrecision                       = 74,
	/// <summary>
	/// Identifies TradeDate, FIX tag 75, with wire type <c>LocalMktDate</c>.
	/// </summary>
	TradeDate                            = 75,
	/// <summary>
	/// Identifies PositionEffect, FIX tag 77, with wire type <c>char</c>.
	/// </summary>
	PositionEffect                       = 77,
	/// <summary>
	/// Identifies NoAllocs, FIX tag 78, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoAllocs                             = 78,
	/// <summary>
	/// Identifies AllocAccount, FIX tag 79, with wire type <c>String</c>.
	/// </summary>
	AllocAccount                         = 79,
	/// <summary>
	/// Identifies AllocQty, FIX tag 80, with wire type <c>Qty</c>.
	/// </summary>
	AllocQty                             = 80,
	/// <summary>
	/// Identifies ProcessCode, FIX tag 81, with wire type <c>char</c>.
	/// </summary>
	ProcessCode                          = 81,
	/// <summary>
	/// Identifies NoRpts, FIX tag 82, with wire type <c>int</c>.
	/// </summary>
	NoRpts                               = 82,
	/// <summary>
	/// Identifies RptSeq, FIX tag 83, with wire type <c>int</c>.
	/// </summary>
	RptSeq                               = 83,
	/// <summary>
	/// Identifies CxlQty, FIX tag 84, with wire type <c>Qty</c>.
	/// </summary>
	CxlQty                               = 84,
	/// <summary>
	/// Identifies NoDlvyInst, FIX tag 85, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoDlvyInst                           = 85,
	/// <summary>
	/// Identifies AllocStatus, FIX tag 87, with wire type <c>int</c>.
	/// </summary>
	AllocStatus                          = 87,
	/// <summary>
	/// Identifies AllocRejCode, FIX tag 88, with wire type <c>int</c>.
	/// </summary>
	AllocRejCode                         = 88,
	/// <summary>
	/// Identifies Signature, FIX tag 89, with wire type <c>data</c>.
	/// </summary>
	Signature                            = 89,
	/// <summary>
	/// Identifies SecureDataLen, FIX tag 90, with wire type <c>Length</c>.
	/// </summary>
	SecureDataLen                        = 90,
	/// <summary>
	/// Identifies SecureData, FIX tag 91, with wire type <c>data</c>.
	/// </summary>
	SecureData                           = 91,
	/// <summary>
	/// Identifies SignatureLength, FIX tag 93, with wire type <c>Length</c>.
	/// </summary>
	SignatureLength                      = 93,
	/// <summary>
	/// Identifies EmailType, FIX tag 94, with wire type <c>char</c>.
	/// </summary>
	EmailType                            = 94,
	/// <summary>
	/// Identifies RawDataLength, FIX tag 95, with wire type <c>Length</c>.
	/// </summary>
	RawDataLength                        = 95,
	/// <summary>
	/// Identifies RawData, FIX tag 96, with wire type <c>data</c>.
	/// </summary>
	RawData                              = 96,
	/// <summary>
	/// Identifies PossResend, FIX tag 97, with wire type <c>Boolean</c>.
	/// </summary>
	PossResend                           = 97,
	/// <summary>
	/// Identifies EncryptMethod, FIX tag 98, with wire type <c>int</c>.
	/// </summary>
	EncryptMethod                        = 98,
	/// <summary>
	/// Identifies StopPx, FIX tag 99, with wire type <c>Price</c>.
	/// </summary>
	StopPx                               = 99,
	/// <summary>
	/// Identifies ExDestination, FIX tag 100, with wire type <c>Exchange</c>.
	/// </summary>
	ExDestination                        = 100,
	/// <summary>
	/// Identifies CxlRejReason, FIX tag 102, with wire type <c>int</c>.
	/// </summary>
	CxlRejReason                         = 102,
	/// <summary>
	/// Identifies OrdRejReason, FIX tag 103, with wire type <c>int</c>.
	/// </summary>
	OrdRejReason                         = 103,
	/// <summary>
	/// Identifies IOIQualifier, FIX tag 104, with wire type <c>char</c>.
	/// </summary>
	IOIQualifier                         = 104,
	/// <summary>
	/// Identifies Issuer, FIX tag 106, with wire type <c>String</c>.
	/// </summary>
	Issuer                               = 106,
	/// <summary>
	/// Identifies SecurityDesc, FIX tag 107, with wire type <c>String</c>.
	/// </summary>
	SecurityDesc                         = 107,
	/// <summary>
	/// Identifies HeartBtInt, FIX tag 108, with wire type <c>int</c>.
	/// </summary>
	HeartBtInt                           = 108,
	/// <summary>
	/// Identifies MinQty, FIX tag 110, with wire type <c>Qty</c>.
	/// </summary>
	MinQty                               = 110,
	/// <summary>
	/// Identifies MaxFloor, FIX tag 111, with wire type <c>Qty</c>.
	/// </summary>
	MaxFloor                             = 111,
	/// <summary>
	/// Identifies TestReqID, FIX tag 112, with wire type <c>String</c>.
	/// </summary>
	TestReqID                            = 112,
	/// <summary>
	/// Identifies ReportToExch, FIX tag 113, with wire type <c>Boolean</c>.
	/// </summary>
	ReportToExch                         = 113,
	/// <summary>
	/// Identifies LocateReqd, FIX tag 114, with wire type <c>Boolean</c>.
	/// </summary>
	LocateReqd                           = 114,
	/// <summary>
	/// Identifies OnBehalfOfCompID, FIX tag 115, with wire type <c>String</c>.
	/// </summary>
	OnBehalfOfCompID                     = 115,
	/// <summary>
	/// Identifies OnBehalfOfSubID, FIX tag 116, with wire type <c>String</c>.
	/// </summary>
	OnBehalfOfSubID                      = 116,
	/// <summary>
	/// Identifies QuoteID, FIX tag 117, with wire type <c>String</c>.
	/// </summary>
	QuoteID                              = 117,
	/// <summary>
	/// Identifies NetMoney, FIX tag 118, with wire type <c>Amt</c>.
	/// </summary>
	NetMoney                             = 118,
	/// <summary>
	/// Identifies SettlCurrAmt, FIX tag 119, with wire type <c>Amt</c>.
	/// </summary>
	SettlCurrAmt                         = 119,
	/// <summary>
	/// Identifies SettlCurrency, FIX tag 120, with wire type <c>Currency</c>.
	/// </summary>
	SettlCurrency                        = 120,
	/// <summary>
	/// Identifies ForexReq, FIX tag 121, with wire type <c>Boolean</c>.
	/// </summary>
	ForexReq                             = 121,
	/// <summary>
	/// Identifies OrigSendingTime, FIX tag 122, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	OrigSendingTime                      = 122,
	/// <summary>
	/// Identifies GapFillFlag, FIX tag 123, with wire type <c>Boolean</c>.
	/// </summary>
	GapFillFlag                          = 123,
	/// <summary>
	/// Identifies NoExecs, FIX tag 124, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoExecs                              = 124,
	/// <summary>
	/// Identifies ExpireTime, FIX tag 126, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	ExpireTime                           = 126,
	/// <summary>
	/// Identifies DKReason, FIX tag 127, with wire type <c>char</c>.
	/// </summary>
	DKReason                             = 127,
	/// <summary>
	/// Identifies DeliverToCompID, FIX tag 128, with wire type <c>String</c>.
	/// </summary>
	DeliverToCompID                      = 128,
	/// <summary>
	/// Identifies DeliverToSubID, FIX tag 129, with wire type <c>String</c>.
	/// </summary>
	DeliverToSubID                       = 129,
	/// <summary>
	/// Identifies IOINaturalFlag, FIX tag 130, with wire type <c>Boolean</c>.
	/// </summary>
	IOINaturalFlag                       = 130,
	/// <summary>
	/// Identifies QuoteReqID, FIX tag 131, with wire type <c>String</c>.
	/// </summary>
	QuoteReqID                           = 131,
	/// <summary>
	/// Identifies BidPx, FIX tag 132, with wire type <c>Price</c>.
	/// </summary>
	BidPx                                = 132,
	/// <summary>
	/// Identifies OfferPx, FIX tag 133, with wire type <c>Price</c>.
	/// </summary>
	OfferPx                              = 133,
	/// <summary>
	/// Identifies BidSize, FIX tag 134, with wire type <c>Qty</c>.
	/// </summary>
	BidSize                              = 134,
	/// <summary>
	/// Identifies OfferSize, FIX tag 135, with wire type <c>Qty</c>.
	/// </summary>
	OfferSize                            = 135,
	/// <summary>
	/// Identifies NoMiscFees, FIX tag 136, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoMiscFees                           = 136,
	/// <summary>
	/// Identifies MiscFeeAmt, FIX tag 137, with wire type <c>Amt</c>.
	/// </summary>
	MiscFeeAmt                           = 137,
	/// <summary>
	/// Identifies MiscFeeCurr, FIX tag 138, with wire type <c>Currency</c>.
	/// </summary>
	MiscFeeCurr                          = 138,
	/// <summary>
	/// Identifies MiscFeeType, FIX tag 139, with wire type <c>String</c>.
	/// </summary>
	MiscFeeType                          = 139,
	/// <summary>
	/// Identifies PrevClosePx, FIX tag 140, with wire type <c>Price</c>.
	/// </summary>
	PrevClosePx                          = 140,
	/// <summary>
	/// Identifies ResetSeqNumFlag, FIX tag 141, with wire type <c>Boolean</c>.
	/// </summary>
	ResetSeqNumFlag                      = 141,
	/// <summary>
	/// Identifies SenderLocationID, FIX tag 142, with wire type <c>String</c>.
	/// </summary>
	SenderLocationID                     = 142,
	/// <summary>
	/// Identifies TargetLocationID, FIX tag 143, with wire type <c>String</c>.
	/// </summary>
	TargetLocationID                     = 143,
	/// <summary>
	/// Identifies OnBehalfOfLocationID, FIX tag 144, with wire type <c>String</c>.
	/// </summary>
	OnBehalfOfLocationID                 = 144,
	/// <summary>
	/// Identifies DeliverToLocationID, FIX tag 145, with wire type <c>String</c>.
	/// </summary>
	DeliverToLocationID                  = 145,
	/// <summary>
	/// Identifies NoRelatedSym, FIX tag 146, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoRelatedSym                         = 146,
	/// <summary>
	/// Identifies Subject, FIX tag 147, with wire type <c>String</c>.
	/// </summary>
	Subject                              = 147,
	/// <summary>
	/// Identifies Headline, FIX tag 148, with wire type <c>String</c>.
	/// </summary>
	Headline                             = 148,
	/// <summary>
	/// Identifies URLLink, FIX tag 149, with wire type <c>String</c>.
	/// </summary>
	URLLink                              = 149,
	/// <summary>
	/// Identifies ExecType, FIX tag 150, with wire type <c>char</c>.
	/// </summary>
	ExecType                             = 150,
	/// <summary>
	/// Identifies LeavesQty, FIX tag 151, with wire type <c>Qty</c>.
	/// </summary>
	LeavesQty                            = 151,
	/// <summary>
	/// Identifies CashOrderQty, FIX tag 152, with wire type <c>Qty</c>.
	/// </summary>
	CashOrderQty                         = 152,
	/// <summary>
	/// Identifies AllocAvgPx, FIX tag 153, with wire type <c>Price</c>.
	/// </summary>
	AllocAvgPx                           = 153,
	/// <summary>
	/// Identifies AllocNetMoney, FIX tag 154, with wire type <c>Amt</c>.
	/// </summary>
	AllocNetMoney                        = 154,
	/// <summary>
	/// Identifies SettlCurrFxRate, FIX tag 155, with wire type <c>float</c>.
	/// </summary>
	SettlCurrFxRate                      = 155,
	/// <summary>
	/// Identifies SettlCurrFxRateCalc, FIX tag 156, with wire type <c>char</c>.
	/// </summary>
	SettlCurrFxRateCalc                  = 156,
	/// <summary>
	/// Identifies NumDaysInterest, FIX tag 157, with wire type <c>int</c>.
	/// </summary>
	NumDaysInterest                      = 157,
	/// <summary>
	/// Identifies AccruedInterestRate, FIX tag 158, with wire type <c>Percentage</c>.
	/// </summary>
	AccruedInterestRate                  = 158,
	/// <summary>
	/// Identifies AccruedInterestAmt, FIX tag 159, with wire type <c>Amt</c>.
	/// </summary>
	AccruedInterestAmt                   = 159,
	/// <summary>
	/// Identifies SettlInstMode, FIX tag 160, with wire type <c>char</c>.
	/// </summary>
	SettlInstMode                        = 160,
	/// <summary>
	/// Identifies AllocText, FIX tag 161, with wire type <c>String</c>.
	/// </summary>
	AllocText                            = 161,
	/// <summary>
	/// Identifies SettlInstID, FIX tag 162, with wire type <c>String</c>.
	/// </summary>
	SettlInstID                          = 162,
	/// <summary>
	/// Identifies SettlInstTransType, FIX tag 163, with wire type <c>char</c>.
	/// </summary>
	SettlInstTransType                   = 163,
	/// <summary>
	/// Identifies EmailThreadID, FIX tag 164, with wire type <c>String</c>.
	/// </summary>
	EmailThreadID                        = 164,
	/// <summary>
	/// Identifies SettlInstSource, FIX tag 165, with wire type <c>char</c>.
	/// </summary>
	SettlInstSource                      = 165,
	/// <summary>
	/// Identifies SecurityType, FIX tag 167, with wire type <c>String</c>.
	/// </summary>
	SecurityType                         = 167,
	/// <summary>
	/// Identifies EffectiveTime, FIX tag 168, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	EffectiveTime                        = 168,
	/// <summary>
	/// Identifies StandInstDbType, FIX tag 169, with wire type <c>int</c>.
	/// </summary>
	StandInstDbType                      = 169,
	/// <summary>
	/// Identifies StandInstDbName, FIX tag 170, with wire type <c>String</c>.
	/// </summary>
	StandInstDbName                      = 170,
	/// <summary>
	/// Identifies StandInstDbID, FIX tag 171, with wire type <c>String</c>.
	/// </summary>
	StandInstDbID                        = 171,
	/// <summary>
	/// Identifies SettlDeliveryType, FIX tag 172, with wire type <c>int</c>.
	/// </summary>
	SettlDeliveryType                    = 172,
	/// <summary>
	/// Identifies BidSpotRate, FIX tag 188, with wire type <c>Price</c>.
	/// </summary>
	BidSpotRate                          = 188,
	/// <summary>
	/// Identifies BidForwardPoints, FIX tag 189, with wire type <c>PriceOffset</c>.
	/// </summary>
	BidForwardPoints                     = 189,
	/// <summary>
	/// Identifies OfferSpotRate, FIX tag 190, with wire type <c>Price</c>.
	/// </summary>
	OfferSpotRate                        = 190,
	/// <summary>
	/// Identifies OfferForwardPoints, FIX tag 191, with wire type <c>PriceOffset</c>.
	/// </summary>
	OfferForwardPoints                   = 191,
	/// <summary>
	/// Identifies OrderQty2, FIX tag 192, with wire type <c>Qty</c>.
	/// </summary>
	OrderQty2                            = 192,
	/// <summary>
	/// Identifies SettlDate2, FIX tag 193, with wire type <c>LocalMktDate</c>.
	/// </summary>
	SettlDate2                           = 193,
	/// <summary>
	/// Identifies LastSpotRate, FIX tag 194, with wire type <c>Price</c>.
	/// </summary>
	LastSpotRate                         = 194,
	/// <summary>
	/// Identifies LastForwardPoints, FIX tag 195, with wire type <c>PriceOffset</c>.
	/// </summary>
	LastForwardPoints                    = 195,
	/// <summary>
	/// Identifies AllocLinkID, FIX tag 196, with wire type <c>String</c>.
	/// </summary>
	AllocLinkID                          = 196,
	/// <summary>
	/// Identifies AllocLinkType, FIX tag 197, with wire type <c>int</c>.
	/// </summary>
	AllocLinkType                        = 197,
	/// <summary>
	/// Identifies SecondaryOrderID, FIX tag 198, with wire type <c>String</c>.
	/// </summary>
	SecondaryOrderID                     = 198,
	/// <summary>
	/// Identifies NoIOIQualifiers, FIX tag 199, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoIOIQualifiers                      = 199,
	/// <summary>
	/// Identifies MaturityMonthYear, FIX tag 200, with wire type <c>MonthYear</c>.
	/// </summary>
	MaturityMonthYear                    = 200,
	/// <summary>
	/// Identifies PutOrCall, FIX tag 201, with wire type <c>int</c>.
	/// </summary>
	PutOrCall                            = 201,
	/// <summary>
	/// Identifies StrikePrice, FIX tag 202, with wire type <c>Price</c>.
	/// </summary>
	StrikePrice                          = 202,
	/// <summary>
	/// Identifies CoveredOrUncovered, FIX tag 203, with wire type <c>int</c>.
	/// </summary>
	CoveredOrUncovered                   = 203,
	/// <summary>
	/// Identifies OptAttribute, FIX tag 206, with wire type <c>char</c>.
	/// </summary>
	OptAttribute                         = 206,
	/// <summary>
	/// Identifies SecurityExchange, FIX tag 207, with wire type <c>Exchange</c>.
	/// </summary>
	SecurityExchange                     = 207,
	/// <summary>
	/// Identifies NotifyBrokerOfCredit, FIX tag 208, with wire type <c>Boolean</c>.
	/// </summary>
	NotifyBrokerOfCredit                 = 208,
	/// <summary>
	/// Identifies AllocHandlInst, FIX tag 209, with wire type <c>int</c>.
	/// </summary>
	AllocHandlInst                       = 209,
	/// <summary>
	/// Identifies MaxShow, FIX tag 210, with wire type <c>Qty</c>.
	/// </summary>
	MaxShow                              = 210,
	/// <summary>
	/// Identifies PegOffsetValue, FIX tag 211, with wire type <c>float</c>.
	/// </summary>
	PegOffsetValue                       = 211,
	/// <summary>
	/// Identifies XmlDataLen, FIX tag 212, with wire type <c>Length</c>.
	/// </summary>
	XmlDataLen                           = 212,
	/// <summary>
	/// Identifies XmlData, FIX tag 213, with wire type <c>data</c>.
	/// </summary>
	XmlData                              = 213,
	/// <summary>
	/// Identifies SettlInstRefID, FIX tag 214, with wire type <c>String</c>.
	/// </summary>
	SettlInstRefID                       = 214,
	/// <summary>
	/// Identifies NoRoutingIDs, FIX tag 215, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoRoutingIDs                         = 215,
	/// <summary>
	/// Identifies RoutingType, FIX tag 216, with wire type <c>int</c>.
	/// </summary>
	RoutingType                          = 216,
	/// <summary>
	/// Identifies RoutingID, FIX tag 217, with wire type <c>String</c>.
	/// </summary>
	RoutingID                            = 217,
	/// <summary>
	/// Identifies Spread, FIX tag 218, with wire type <c>PriceOffset</c>.
	/// </summary>
	Spread                               = 218,
	/// <summary>
	/// Identifies BenchmarkCurveCurrency, FIX tag 220, with wire type <c>Currency</c>.
	/// </summary>
	BenchmarkCurveCurrency               = 220,
	/// <summary>
	/// Identifies BenchmarkCurveName, FIX tag 221, with wire type <c>String</c>.
	/// </summary>
	BenchmarkCurveName                   = 221,
	/// <summary>
	/// Identifies BenchmarkCurvePoint, FIX tag 222, with wire type <c>String</c>.
	/// </summary>
	BenchmarkCurvePoint                  = 222,
	/// <summary>
	/// Identifies CouponRate, FIX tag 223, with wire type <c>Percentage</c>.
	/// </summary>
	CouponRate                           = 223,
	/// <summary>
	/// Identifies CouponPaymentDate, FIX tag 224, with wire type <c>LocalMktDate</c>.
	/// </summary>
	CouponPaymentDate                    = 224,
	/// <summary>
	/// Identifies IssueDate, FIX tag 225, with wire type <c>LocalMktDate</c>.
	/// </summary>
	IssueDate                            = 225,
	/// <summary>
	/// Identifies RepurchaseTerm, FIX tag 226, with wire type <c>int</c>.
	/// </summary>
	RepurchaseTerm                       = 226,
	/// <summary>
	/// Identifies RepurchaseRate, FIX tag 227, with wire type <c>Percentage</c>.
	/// </summary>
	RepurchaseRate                       = 227,
	/// <summary>
	/// Identifies Factor, FIX tag 228, with wire type <c>float</c>.
	/// </summary>
	Factor                               = 228,
	/// <summary>
	/// Identifies TradeOriginationDate, FIX tag 229, with wire type <c>LocalMktDate</c>.
	/// </summary>
	TradeOriginationDate                 = 229,
	/// <summary>
	/// Identifies ExDate, FIX tag 230, with wire type <c>LocalMktDate</c>.
	/// </summary>
	ExDate                               = 230,
	/// <summary>
	/// Identifies ContractMultiplier, FIX tag 231, with wire type <c>float</c>.
	/// </summary>
	ContractMultiplier                   = 231,
	/// <summary>
	/// Identifies NoStipulations, FIX tag 232, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoStipulations                       = 232,
	/// <summary>
	/// Identifies StipulationType, FIX tag 233, with wire type <c>String</c>.
	/// </summary>
	StipulationType                      = 233,
	/// <summary>
	/// Identifies StipulationValue, FIX tag 234, with wire type <c>String</c>.
	/// </summary>
	StipulationValue                     = 234,
	/// <summary>
	/// Identifies YieldType, FIX tag 235, with wire type <c>String</c>.
	/// </summary>
	YieldType                            = 235,
	/// <summary>
	/// Identifies Yield, FIX tag 236, with wire type <c>Percentage</c>.
	/// </summary>
	Yield                                = 236,
	/// <summary>
	/// Identifies TotalTakedown, FIX tag 237, with wire type <c>Amt</c>.
	/// </summary>
	TotalTakedown                        = 237,
	/// <summary>
	/// Identifies Concession, FIX tag 238, with wire type <c>Amt</c>.
	/// </summary>
	Concession                           = 238,
	/// <summary>
	/// Identifies RepoCollateralSecurityType, FIX tag 239, with wire type <c>String</c>.
	/// </summary>
	RepoCollateralSecurityType           = 239,
	/// <summary>
	/// Identifies RedemptionDate, FIX tag 240, with wire type <c>LocalMktDate</c>.
	/// </summary>
	RedemptionDate                       = 240,
	/// <summary>
	/// Identifies UnderlyingCouponPaymentDate, FIX tag 241, with wire type <c>LocalMktDate</c>.
	/// </summary>
	UnderlyingCouponPaymentDate          = 241,
	/// <summary>
	/// Identifies UnderlyingIssueDate, FIX tag 242, with wire type <c>LocalMktDate</c>.
	/// </summary>
	UnderlyingIssueDate                  = 242,
	/// <summary>
	/// Identifies UnderlyingRepoCollateralSecurityType, FIX tag 243, with wire type <c>String</c>.
	/// </summary>
	UnderlyingRepoCollateralSecurityType = 243,
	/// <summary>
	/// Identifies UnderlyingRepurchaseTerm, FIX tag 244, with wire type <c>int</c>.
	/// </summary>
	UnderlyingRepurchaseTerm             = 244,
	/// <summary>
	/// Identifies UnderlyingRepurchaseRate, FIX tag 245, with wire type <c>Percentage</c>.
	/// </summary>
	UnderlyingRepurchaseRate             = 245,
	/// <summary>
	/// Identifies UnderlyingFactor, FIX tag 246, with wire type <c>float</c>.
	/// </summary>
	UnderlyingFactor                     = 246,
	/// <summary>
	/// Identifies UnderlyingRedemptionDate, FIX tag 247, with wire type <c>LocalMktDate</c>.
	/// </summary>
	UnderlyingRedemptionDate             = 247,
	/// <summary>
	/// Identifies LegCouponPaymentDate, FIX tag 248, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegCouponPaymentDate                 = 248,
	/// <summary>
	/// Identifies LegIssueDate, FIX tag 249, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegIssueDate                         = 249,
	/// <summary>
	/// Identifies LegRepoCollateralSecurityType, FIX tag 250, with wire type <c>String</c>.
	/// </summary>
	LegRepoCollateralSecurityType        = 250,
	/// <summary>
	/// Identifies LegRepurchaseTerm, FIX tag 251, with wire type <c>int</c>.
	/// </summary>
	LegRepurchaseTerm                    = 251,
	/// <summary>
	/// Identifies LegRepurchaseRate, FIX tag 252, with wire type <c>Percentage</c>.
	/// </summary>
	LegRepurchaseRate                    = 252,
	/// <summary>
	/// Identifies LegFactor, FIX tag 253, with wire type <c>float</c>.
	/// </summary>
	LegFactor                            = 253,
	/// <summary>
	/// Identifies LegRedemptionDate, FIX tag 254, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegRedemptionDate                    = 254,
	/// <summary>
	/// Identifies CreditRating, FIX tag 255, with wire type <c>String</c>.
	/// </summary>
	CreditRating                         = 255,
	/// <summary>
	/// Identifies UnderlyingCreditRating, FIX tag 256, with wire type <c>String</c>.
	/// </summary>
	UnderlyingCreditRating               = 256,
	/// <summary>
	/// Identifies LegCreditRating, FIX tag 257, with wire type <c>String</c>.
	/// </summary>
	LegCreditRating                      = 257,
	/// <summary>
	/// Identifies TradedFlatSwitch, FIX tag 258, with wire type <c>Boolean</c>.
	/// </summary>
	TradedFlatSwitch                     = 258,
	/// <summary>
	/// Identifies BasisFeatureDate, FIX tag 259, with wire type <c>LocalMktDate</c>.
	/// </summary>
	BasisFeatureDate                     = 259,
	/// <summary>
	/// Identifies BasisFeaturePrice, FIX tag 260, with wire type <c>Price</c>.
	/// </summary>
	BasisFeaturePrice                    = 260,
	/// <summary>
	/// Identifies MDReqID, FIX tag 262, with wire type <c>String</c>.
	/// </summary>
	MDReqID                              = 262,
	/// <summary>
	/// Identifies SubscriptionRequestType, FIX tag 263, with wire type <c>char</c>.
	/// </summary>
	SubscriptionRequestType              = 263,
	/// <summary>
	/// Identifies MarketDepth, FIX tag 264, with wire type <c>int</c>.
	/// </summary>
	MarketDepth                          = 264,
	/// <summary>
	/// Identifies MDUpdateType, FIX tag 265, with wire type <c>int</c>.
	/// </summary>
	MDUpdateType                         = 265,
	/// <summary>
	/// Identifies AggregatedBook, FIX tag 266, with wire type <c>Boolean</c>.
	/// </summary>
	AggregatedBook                       = 266,
	/// <summary>
	/// Identifies NoMDEntryTypes, FIX tag 267, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoMDEntryTypes                       = 267,
	/// <summary>
	/// Identifies NoMDEntries, FIX tag 268, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoMDEntries                          = 268,
	/// <summary>
	/// Identifies MDEntryType, FIX tag 269, with wire type <c>char</c>.
	/// </summary>
	MDEntryType                          = 269,
	/// <summary>
	/// Identifies MDEntryPx, FIX tag 270, with wire type <c>Price</c>.
	/// </summary>
	MDEntryPx                            = 270,
	/// <summary>
	/// Identifies MDEntrySize, FIX tag 271, with wire type <c>Qty</c>.
	/// </summary>
	MDEntrySize                          = 271,
	/// <summary>
	/// Identifies MDEntryDate, FIX tag 272, with wire type <c>UTCDateOnly</c>.
	/// </summary>
	MDEntryDate                          = 272,
	/// <summary>
	/// Identifies MDEntryTime, FIX tag 273, with wire type <c>UTCTimeOnly</c>.
	/// </summary>
	MDEntryTime                          = 273,
	/// <summary>
	/// Identifies TickDirection, FIX tag 274, with wire type <c>char</c>.
	/// </summary>
	TickDirection                        = 274,
	/// <summary>
	/// Identifies MDMkt, FIX tag 275, with wire type <c>Exchange</c>.
	/// </summary>
	MDMkt                                = 275,
	/// <summary>
	/// Identifies QuoteCondition, FIX tag 276, with wire type <c>MultipleValueString</c>.
	/// </summary>
	QuoteCondition                       = 276,
	/// <summary>
	/// Identifies TradeCondition, FIX tag 277, with wire type <c>MultipleValueString</c>.
	/// </summary>
	TradeCondition                       = 277,
	/// <summary>
	/// Identifies MDEntryID, FIX tag 278, with wire type <c>String</c>.
	/// </summary>
	MDEntryID                            = 278,
	/// <summary>
	/// Identifies MDUpdateAction, FIX tag 279, with wire type <c>char</c>.
	/// </summary>
	MDUpdateAction                       = 279,
	/// <summary>
	/// Identifies MDEntryRefID, FIX tag 280, with wire type <c>String</c>.
	/// </summary>
	MDEntryRefID                         = 280,
	/// <summary>
	/// Identifies MDReqRejReason, FIX tag 281, with wire type <c>char</c>.
	/// </summary>
	MDReqRejReason                       = 281,
	/// <summary>
	/// Identifies MDEntryOriginator, FIX tag 282, with wire type <c>String</c>.
	/// </summary>
	MDEntryOriginator                    = 282,
	/// <summary>
	/// Identifies LocationID, FIX tag 283, with wire type <c>String</c>.
	/// </summary>
	LocationID                           = 283,
	/// <summary>
	/// Identifies DeskID, FIX tag 284, with wire type <c>String</c>.
	/// </summary>
	DeskID                               = 284,
	/// <summary>
	/// Identifies DeleteReason, FIX tag 285, with wire type <c>char</c>.
	/// </summary>
	DeleteReason                         = 285,
	/// <summary>
	/// Identifies OpenCloseSettlFlag, FIX tag 286, with wire type <c>MultipleValueString</c>.
	/// </summary>
	OpenCloseSettlFlag                   = 286,
	/// <summary>
	/// Identifies SellerDays, FIX tag 287, with wire type <c>int</c>.
	/// </summary>
	SellerDays                           = 287,
	/// <summary>
	/// Identifies MDEntryBuyer, FIX tag 288, with wire type <c>String</c>.
	/// </summary>
	MDEntryBuyer                         = 288,
	/// <summary>
	/// Identifies MDEntrySeller, FIX tag 289, with wire type <c>String</c>.
	/// </summary>
	MDEntrySeller                        = 289,
	/// <summary>
	/// Identifies MDEntryPositionNo, FIX tag 290, with wire type <c>int</c>.
	/// </summary>
	MDEntryPositionNo                    = 290,
	/// <summary>
	/// Identifies FinancialStatus, FIX tag 291, with wire type <c>MultipleValueString</c>.
	/// </summary>
	FinancialStatus                      = 291,
	/// <summary>
	/// Identifies CorporateAction, FIX tag 292, with wire type <c>MultipleValueString</c>.
	/// </summary>
	CorporateAction                      = 292,
	/// <summary>
	/// Identifies DefBidSize, FIX tag 293, with wire type <c>Qty</c>.
	/// </summary>
	DefBidSize                           = 293,
	/// <summary>
	/// Identifies DefOfferSize, FIX tag 294, with wire type <c>Qty</c>.
	/// </summary>
	DefOfferSize                         = 294,
	/// <summary>
	/// Identifies NoQuoteEntries, FIX tag 295, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoQuoteEntries                       = 295,
	/// <summary>
	/// Identifies NoQuoteSets, FIX tag 296, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoQuoteSets                          = 296,
	/// <summary>
	/// Identifies QuoteStatus, FIX tag 297, with wire type <c>int</c>.
	/// </summary>
	QuoteStatus                          = 297,
	/// <summary>
	/// Identifies QuoteCancelType, FIX tag 298, with wire type <c>int</c>.
	/// </summary>
	QuoteCancelType                      = 298,
	/// <summary>
	/// Identifies QuoteEntryID, FIX tag 299, with wire type <c>String</c>.
	/// </summary>
	QuoteEntryID                         = 299,
	/// <summary>
	/// Identifies QuoteRejectReason, FIX tag 300, with wire type <c>int</c>.
	/// </summary>
	QuoteRejectReason                    = 300,
	/// <summary>
	/// Identifies QuoteResponseLevel, FIX tag 301, with wire type <c>int</c>.
	/// </summary>
	QuoteResponseLevel                   = 301,
	/// <summary>
	/// Identifies QuoteSetID, FIX tag 302, with wire type <c>String</c>.
	/// </summary>
	QuoteSetID                           = 302,
	/// <summary>
	/// Identifies QuoteRequestType, FIX tag 303, with wire type <c>int</c>.
	/// </summary>
	QuoteRequestType                     = 303,
	/// <summary>
	/// Identifies TotNoQuoteEntries, FIX tag 304, with wire type <c>int</c>.
	/// </summary>
	TotNoQuoteEntries                    = 304,
	/// <summary>
	/// Identifies UnderlyingSecurityIDSource, FIX tag 305, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityIDSource           = 305,
	/// <summary>
	/// Identifies UnderlyingIssuer, FIX tag 306, with wire type <c>String</c>.
	/// </summary>
	UnderlyingIssuer                     = 306,
	/// <summary>
	/// Identifies UnderlyingSecurityDesc, FIX tag 307, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityDesc               = 307,
	/// <summary>
	/// Identifies UnderlyingSecurityExchange, FIX tag 308, with wire type <c>Exchange</c>.
	/// </summary>
	UnderlyingSecurityExchange           = 308,
	/// <summary>
	/// Identifies UnderlyingSecurityID, FIX tag 309, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityID                 = 309,
	/// <summary>
	/// Identifies UnderlyingSecurityType, FIX tag 310, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityType               = 310,
	/// <summary>
	/// Identifies UnderlyingSymbol, FIX tag 311, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSymbol                     = 311,
	/// <summary>
	/// Identifies UnderlyingSymbolSfx, FIX tag 312, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSymbolSfx                  = 312,
	/// <summary>
	/// Identifies UnderlyingMaturityMonthYear, FIX tag 313, with wire type <c>MonthYear</c>.
	/// </summary>
	UnderlyingMaturityMonthYear          = 313,
	/// <summary>
	/// Identifies UnderlyingPutOrCall, FIX tag 315, with wire type <c>int</c>.
	/// </summary>
	UnderlyingPutOrCall                  = 315,
	/// <summary>
	/// Identifies UnderlyingStrikePrice, FIX tag 316, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingStrikePrice                = 316,
	/// <summary>
	/// Identifies UnderlyingOptAttribute, FIX tag 317, with wire type <c>char</c>.
	/// </summary>
	UnderlyingOptAttribute               = 317,
	/// <summary>
	/// Identifies UnderlyingCurrency, FIX tag 318, with wire type <c>Currency</c>.
	/// </summary>
	UnderlyingCurrency                   = 318,
	/// <summary>
	/// Identifies SecurityReqID, FIX tag 320, with wire type <c>String</c>.
	/// </summary>
	SecurityReqID                        = 320,
	/// <summary>
	/// Identifies SecurityRequestType, FIX tag 321, with wire type <c>int</c>.
	/// </summary>
	SecurityRequestType                  = 321,
	/// <summary>
	/// Identifies SecurityResponseID, FIX tag 322, with wire type <c>String</c>.
	/// </summary>
	SecurityResponseID                   = 322,
	/// <summary>
	/// Identifies SecurityResponseType, FIX tag 323, with wire type <c>int</c>.
	/// </summary>
	SecurityResponseType                 = 323,
	/// <summary>
	/// Identifies SecurityStatusReqID, FIX tag 324, with wire type <c>String</c>.
	/// </summary>
	SecurityStatusReqID                  = 324,
	/// <summary>
	/// Identifies UnsolicitedIndicator, FIX tag 325, with wire type <c>Boolean</c>.
	/// </summary>
	UnsolicitedIndicator                 = 325,
	/// <summary>
	/// Identifies SecurityTradingStatus, FIX tag 326, with wire type <c>int</c>.
	/// </summary>
	SecurityTradingStatus                = 326,
	/// <summary>
	/// Identifies HaltReason, FIX tag 327, with wire type <c>char</c>.
	/// </summary>
	HaltReason                           = 327,
	/// <summary>
	/// Identifies InViewOfCommon, FIX tag 328, with wire type <c>Boolean</c>.
	/// </summary>
	InViewOfCommon                       = 328,
	/// <summary>
	/// Identifies DueToRelated, FIX tag 329, with wire type <c>Boolean</c>.
	/// </summary>
	DueToRelated                         = 329,
	/// <summary>
	/// Identifies BuyVolume, FIX tag 330, with wire type <c>Qty</c>.
	/// </summary>
	BuyVolume                            = 330,
	/// <summary>
	/// Identifies SellVolume, FIX tag 331, with wire type <c>Qty</c>.
	/// </summary>
	SellVolume                           = 331,
	/// <summary>
	/// Identifies HighPx, FIX tag 332, with wire type <c>Price</c>.
	/// </summary>
	HighPx                               = 332,
	/// <summary>
	/// Identifies LowPx, FIX tag 333, with wire type <c>Price</c>.
	/// </summary>
	LowPx                                = 333,
	/// <summary>
	/// Identifies Adjustment, FIX tag 334, with wire type <c>int</c>.
	/// </summary>
	Adjustment                           = 334,
	/// <summary>
	/// Identifies TradSesReqID, FIX tag 335, with wire type <c>String</c>.
	/// </summary>
	TradSesReqID                         = 335,
	/// <summary>
	/// Identifies TradingSessionID, FIX tag 336, with wire type <c>String</c>.
	/// </summary>
	TradingSessionID                     = 336,
	/// <summary>
	/// Identifies ContraTrader, FIX tag 337, with wire type <c>String</c>.
	/// </summary>
	ContraTrader                         = 337,
	/// <summary>
	/// Identifies TradSesMethod, FIX tag 338, with wire type <c>int</c>.
	/// </summary>
	TradSesMethod                        = 338,
	/// <summary>
	/// Identifies TradSesMode, FIX tag 339, with wire type <c>int</c>.
	/// </summary>
	TradSesMode                          = 339,
	/// <summary>
	/// Identifies TradSesStatus, FIX tag 340, with wire type <c>int</c>.
	/// </summary>
	TradSesStatus                        = 340,
	/// <summary>
	/// Identifies TradSesStartTime, FIX tag 341, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TradSesStartTime                     = 341,
	/// <summary>
	/// Identifies TradSesOpenTime, FIX tag 342, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TradSesOpenTime                      = 342,
	/// <summary>
	/// Identifies TradSesPreCloseTime, FIX tag 343, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TradSesPreCloseTime                  = 343,
	/// <summary>
	/// Identifies TradSesCloseTime, FIX tag 344, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TradSesCloseTime                     = 344,
	/// <summary>
	/// Identifies TradSesEndTime, FIX tag 345, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TradSesEndTime                       = 345,
	/// <summary>
	/// Identifies NumberOfOrders, FIX tag 346, with wire type <c>int</c>.
	/// </summary>
	NumberOfOrders                       = 346,
	/// <summary>
	/// Identifies MessageEncoding, FIX tag 347, with wire type <c>String</c>.
	/// </summary>
	MessageEncoding                      = 347,
	/// <summary>
	/// Identifies EncodedIssuerLen, FIX tag 348, with wire type <c>Length</c>.
	/// </summary>
	EncodedIssuerLen                     = 348,
	/// <summary>
	/// Identifies EncodedIssuer, FIX tag 349, with wire type <c>data</c>.
	/// </summary>
	EncodedIssuer                        = 349,
	/// <summary>
	/// Identifies EncodedSecurityDescLen, FIX tag 350, with wire type <c>Length</c>.
	/// </summary>
	EncodedSecurityDescLen               = 350,
	/// <summary>
	/// Identifies EncodedSecurityDesc, FIX tag 351, with wire type <c>data</c>.
	/// </summary>
	EncodedSecurityDesc                  = 351,
	/// <summary>
	/// Identifies EncodedListExecInstLen, FIX tag 352, with wire type <c>Length</c>.
	/// </summary>
	EncodedListExecInstLen               = 352,
	/// <summary>
	/// Identifies EncodedListExecInst, FIX tag 353, with wire type <c>data</c>.
	/// </summary>
	EncodedListExecInst                  = 353,
	/// <summary>
	/// Identifies EncodedTextLen, FIX tag 354, with wire type <c>Length</c>.
	/// </summary>
	EncodedTextLen                       = 354,
	/// <summary>
	/// Identifies EncodedText, FIX tag 355, with wire type <c>data</c>.
	/// </summary>
	EncodedText                          = 355,
	/// <summary>
	/// Identifies EncodedSubjectLen, FIX tag 356, with wire type <c>Length</c>.
	/// </summary>
	EncodedSubjectLen                    = 356,
	/// <summary>
	/// Identifies EncodedSubject, FIX tag 357, with wire type <c>data</c>.
	/// </summary>
	EncodedSubject                       = 357,
	/// <summary>
	/// Identifies EncodedHeadlineLen, FIX tag 358, with wire type <c>Length</c>.
	/// </summary>
	EncodedHeadlineLen                   = 358,
	/// <summary>
	/// Identifies EncodedHeadline, FIX tag 359, with wire type <c>data</c>.
	/// </summary>
	EncodedHeadline                      = 359,
	/// <summary>
	/// Identifies EncodedAllocTextLen, FIX tag 360, with wire type <c>Length</c>.
	/// </summary>
	EncodedAllocTextLen                  = 360,
	/// <summary>
	/// Identifies EncodedAllocText, FIX tag 361, with wire type <c>data</c>.
	/// </summary>
	EncodedAllocText                     = 361,
	/// <summary>
	/// Identifies EncodedUnderlyingIssuerLen, FIX tag 362, with wire type <c>Length</c>.
	/// </summary>
	EncodedUnderlyingIssuerLen           = 362,
	/// <summary>
	/// Identifies EncodedUnderlyingIssuer, FIX tag 363, with wire type <c>data</c>.
	/// </summary>
	EncodedUnderlyingIssuer              = 363,
	/// <summary>
	/// Identifies EncodedUnderlyingSecurityDescLen, FIX tag 364, with wire type <c>Length</c>.
	/// </summary>
	EncodedUnderlyingSecurityDescLen     = 364,
	/// <summary>
	/// Identifies EncodedUnderlyingSecurityDesc, FIX tag 365, with wire type <c>data</c>.
	/// </summary>
	EncodedUnderlyingSecurityDesc        = 365,
	/// <summary>
	/// Identifies AllocPrice, FIX tag 366, with wire type <c>Price</c>.
	/// </summary>
	AllocPrice                           = 366,
	/// <summary>
	/// Identifies QuoteSetValidUntilTime, FIX tag 367, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	QuoteSetValidUntilTime               = 367,
	/// <summary>
	/// Identifies QuoteEntryRejectReason, FIX tag 368, with wire type <c>int</c>.
	/// </summary>
	QuoteEntryRejectReason               = 368,
	/// <summary>
	/// Identifies LastMsgSeqNumProcessed, FIX tag 369, with wire type <c>SeqNum</c>.
	/// </summary>
	LastMsgSeqNumProcessed               = 369,
	/// <summary>
	/// Identifies RefTagID, FIX tag 371, with wire type <c>int</c>.
	/// </summary>
	RefTagID                             = 371,
	/// <summary>
	/// Identifies RefMsgType, FIX tag 372, with wire type <c>String</c>.
	/// </summary>
	RefMsgType                           = 372,
	/// <summary>
	/// Identifies SessionRejectReason, FIX tag 373, with wire type <c>int</c>.
	/// </summary>
	SessionRejectReason                  = 373,
	/// <summary>
	/// Identifies BidRequestTransType, FIX tag 374, with wire type <c>char</c>.
	/// </summary>
	BidRequestTransType                  = 374,
	/// <summary>
	/// Identifies ContraBroker, FIX tag 375, with wire type <c>String</c>.
	/// </summary>
	ContraBroker                         = 375,
	/// <summary>
	/// Identifies ComplianceID, FIX tag 376, with wire type <c>String</c>.
	/// </summary>
	ComplianceID                         = 376,
	/// <summary>
	/// Identifies SolicitedFlag, FIX tag 377, with wire type <c>Boolean</c>.
	/// </summary>
	SolicitedFlag                        = 377,
	/// <summary>
	/// Identifies ExecRestatementReason, FIX tag 378, with wire type <c>int</c>.
	/// </summary>
	ExecRestatementReason                = 378,
	/// <summary>
	/// Identifies BusinessRejectRefID, FIX tag 379, with wire type <c>String</c>.
	/// </summary>
	BusinessRejectRefID                  = 379,
	/// <summary>
	/// Identifies BusinessRejectReason, FIX tag 380, with wire type <c>int</c>.
	/// </summary>
	BusinessRejectReason                 = 380,
	/// <summary>
	/// Identifies GrossTradeAmt, FIX tag 381, with wire type <c>Amt</c>.
	/// </summary>
	GrossTradeAmt                        = 381,
	/// <summary>
	/// Identifies NoContraBrokers, FIX tag 382, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoContraBrokers                      = 382,
	/// <summary>
	/// Identifies MaxMessageSize, FIX tag 383, with wire type <c>Length</c>.
	/// </summary>
	MaxMessageSize                       = 383,
	/// <summary>
	/// Identifies NoMsgTypes, FIX tag 384, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoMsgTypes                           = 384,
	/// <summary>
	/// Identifies MsgDirection, FIX tag 385, with wire type <c>char</c>.
	/// </summary>
	MsgDirection                         = 385,
	/// <summary>
	/// Identifies NoTradingSessions, FIX tag 386, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoTradingSessions                    = 386,
	/// <summary>
	/// Identifies TotalVolumeTraded, FIX tag 387, with wire type <c>Qty</c>.
	/// </summary>
	TotalVolumeTraded                    = 387,
	/// <summary>
	/// Identifies DiscretionInst, FIX tag 388, with wire type <c>char</c>.
	/// </summary>
	DiscretionInst                       = 388,
	/// <summary>
	/// Identifies DiscretionOffsetValue, FIX tag 389, with wire type <c>float</c>.
	/// </summary>
	DiscretionOffsetValue                = 389,
	/// <summary>
	/// Identifies BidID, FIX tag 390, with wire type <c>String</c>.
	/// </summary>
	BidID                                = 390,
	/// <summary>
	/// Identifies ClientBidID, FIX tag 391, with wire type <c>String</c>.
	/// </summary>
	ClientBidID                          = 391,
	/// <summary>
	/// Identifies ListName, FIX tag 392, with wire type <c>String</c>.
	/// </summary>
	ListName                             = 392,
	/// <summary>
	/// Identifies TotNoRelatedSym, FIX tag 393, with wire type <c>int</c>.
	/// </summary>
	TotNoRelatedSym                      = 393,
	/// <summary>
	/// Identifies BidType, FIX tag 394, with wire type <c>int</c>.
	/// </summary>
	BidType                              = 394,
	/// <summary>
	/// Identifies NumTickets, FIX tag 395, with wire type <c>int</c>.
	/// </summary>
	NumTickets                           = 395,
	/// <summary>
	/// Identifies SideValue1, FIX tag 396, with wire type <c>Amt</c>.
	/// </summary>
	SideValue1                           = 396,
	/// <summary>
	/// Identifies SideValue2, FIX tag 397, with wire type <c>Amt</c>.
	/// </summary>
	SideValue2                           = 397,
	/// <summary>
	/// Identifies NoBidDescriptors, FIX tag 398, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoBidDescriptors                     = 398,
	/// <summary>
	/// Identifies BidDescriptorType, FIX tag 399, with wire type <c>int</c>.
	/// </summary>
	BidDescriptorType                    = 399,
	/// <summary>
	/// Identifies BidDescriptor, FIX tag 400, with wire type <c>String</c>.
	/// </summary>
	BidDescriptor                        = 400,
	/// <summary>
	/// Identifies SideValueInd, FIX tag 401, with wire type <c>int</c>.
	/// </summary>
	SideValueInd                         = 401,
	/// <summary>
	/// Identifies LiquidityPctLow, FIX tag 402, with wire type <c>Percentage</c>.
	/// </summary>
	LiquidityPctLow                      = 402,
	/// <summary>
	/// Identifies LiquidityPctHigh, FIX tag 403, with wire type <c>Percentage</c>.
	/// </summary>
	LiquidityPctHigh                     = 403,
	/// <summary>
	/// Identifies LiquidityValue, FIX tag 404, with wire type <c>Amt</c>.
	/// </summary>
	LiquidityValue                       = 404,
	/// <summary>
	/// Identifies EFPTrackingError, FIX tag 405, with wire type <c>Percentage</c>.
	/// </summary>
	EFPTrackingError                     = 405,
	/// <summary>
	/// Identifies FairValue, FIX tag 406, with wire type <c>Amt</c>.
	/// </summary>
	FairValue                            = 406,
	/// <summary>
	/// Identifies OutsideIndexPct, FIX tag 407, with wire type <c>Percentage</c>.
	/// </summary>
	OutsideIndexPct                      = 407,
	/// <summary>
	/// Identifies ValueOfFutures, FIX tag 408, with wire type <c>Amt</c>.
	/// </summary>
	ValueOfFutures                       = 408,
	/// <summary>
	/// Identifies LiquidityIndType, FIX tag 409, with wire type <c>int</c>.
	/// </summary>
	LiquidityIndType                     = 409,
	/// <summary>
	/// Identifies WtAverageLiquidity, FIX tag 410, with wire type <c>Percentage</c>.
	/// </summary>
	WtAverageLiquidity                   = 410,
	/// <summary>
	/// Identifies ExchangeForPhysical, FIX tag 411, with wire type <c>Boolean</c>.
	/// </summary>
	ExchangeForPhysical                  = 411,
	/// <summary>
	/// Identifies OutMainCntryUIndex, FIX tag 412, with wire type <c>Amt</c>.
	/// </summary>
	OutMainCntryUIndex                   = 412,
	/// <summary>
	/// Identifies CrossPercent, FIX tag 413, with wire type <c>Percentage</c>.
	/// </summary>
	CrossPercent                         = 413,
	/// <summary>
	/// Identifies ProgRptReqs, FIX tag 414, with wire type <c>int</c>.
	/// </summary>
	ProgRptReqs                          = 414,
	/// <summary>
	/// Identifies ProgPeriodInterval, FIX tag 415, with wire type <c>int</c>.
	/// </summary>
	ProgPeriodInterval                   = 415,
	/// <summary>
	/// Identifies IncTaxInd, FIX tag 416, with wire type <c>int</c>.
	/// </summary>
	IncTaxInd                            = 416,
	/// <summary>
	/// Identifies NumBidders, FIX tag 417, with wire type <c>int</c>.
	/// </summary>
	NumBidders                           = 417,
	/// <summary>
	/// Identifies BidTradeType, FIX tag 418, with wire type <c>char</c>.
	/// </summary>
	BidTradeType                         = 418,
	/// <summary>
	/// Identifies BasisPxType, FIX tag 419, with wire type <c>char</c>.
	/// </summary>
	BasisPxType                          = 419,
	/// <summary>
	/// Identifies NoBidComponents, FIX tag 420, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoBidComponents                      = 420,
	/// <summary>
	/// Identifies Country, FIX tag 421, with wire type <c>Country</c>.
	/// </summary>
	Country                              = 421,
	/// <summary>
	/// Identifies TotNoStrikes, FIX tag 422, with wire type <c>int</c>.
	/// </summary>
	TotNoStrikes                         = 422,
	/// <summary>
	/// Identifies PriceType, FIX tag 423, with wire type <c>int</c>.
	/// </summary>
	PriceType                            = 423,
	/// <summary>
	/// Identifies DayOrderQty, FIX tag 424, with wire type <c>Qty</c>.
	/// </summary>
	DayOrderQty                          = 424,
	/// <summary>
	/// Identifies DayCumQty, FIX tag 425, with wire type <c>Qty</c>.
	/// </summary>
	DayCumQty                            = 425,
	/// <summary>
	/// Identifies DayAvgPx, FIX tag 426, with wire type <c>Price</c>.
	/// </summary>
	DayAvgPx                             = 426,
	/// <summary>
	/// Identifies GTBookingInst, FIX tag 427, with wire type <c>int</c>.
	/// </summary>
	GTBookingInst                        = 427,
	/// <summary>
	/// Identifies NoStrikes, FIX tag 428, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoStrikes                            = 428,
	/// <summary>
	/// Identifies ListStatusType, FIX tag 429, with wire type <c>int</c>.
	/// </summary>
	ListStatusType                       = 429,
	/// <summary>
	/// Identifies NetGrossInd, FIX tag 430, with wire type <c>int</c>.
	/// </summary>
	NetGrossInd                          = 430,
	/// <summary>
	/// Identifies ListOrderStatus, FIX tag 431, with wire type <c>int</c>.
	/// </summary>
	ListOrderStatus                      = 431,
	/// <summary>
	/// Identifies ExpireDate, FIX tag 432, with wire type <c>LocalMktDate</c>.
	/// </summary>
	ExpireDate                           = 432,
	/// <summary>
	/// Identifies ListExecInstType, FIX tag 433, with wire type <c>char</c>.
	/// </summary>
	ListExecInstType                     = 433,
	/// <summary>
	/// Identifies CxlRejResponseTo, FIX tag 434, with wire type <c>char</c>.
	/// </summary>
	CxlRejResponseTo                     = 434,
	/// <summary>
	/// Identifies UnderlyingCouponRate, FIX tag 435, with wire type <c>Percentage</c>.
	/// </summary>
	UnderlyingCouponRate                 = 435,
	/// <summary>
	/// Identifies UnderlyingContractMultiplier, FIX tag 436, with wire type <c>float</c>.
	/// </summary>
	UnderlyingContractMultiplier         = 436,
	/// <summary>
	/// Identifies ContraTradeQty, FIX tag 437, with wire type <c>Qty</c>.
	/// </summary>
	ContraTradeQty                       = 437,
	/// <summary>
	/// Identifies ContraTradeTime, FIX tag 438, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	ContraTradeTime                      = 438,
	/// <summary>
	/// Identifies LiquidityNumSecurities, FIX tag 441, with wire type <c>int</c>.
	/// </summary>
	LiquidityNumSecurities               = 441,
	/// <summary>
	/// Identifies MultiLegReportingType, FIX tag 442, with wire type <c>char</c>.
	/// </summary>
	MultiLegReportingType                = 442,
	/// <summary>
	/// Identifies StrikeTime, FIX tag 443, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	StrikeTime                           = 443,
	/// <summary>
	/// Identifies ListStatusText, FIX tag 444, with wire type <c>String</c>.
	/// </summary>
	ListStatusText                       = 444,
	/// <summary>
	/// Identifies EncodedListStatusTextLen, FIX tag 445, with wire type <c>Length</c>.
	/// </summary>
	EncodedListStatusTextLen             = 445,
	/// <summary>
	/// Identifies EncodedListStatusText, FIX tag 446, with wire type <c>data</c>.
	/// </summary>
	EncodedListStatusText                = 446,
	/// <summary>
	/// Identifies PartyIDSource, FIX tag 447, with wire type <c>char</c>.
	/// </summary>
	PartyIDSource                        = 447,
	/// <summary>
	/// Identifies PartyID, FIX tag 448, with wire type <c>String</c>.
	/// </summary>
	PartyID                              = 448,
	/// <summary>
	/// Identifies NetChgPrevDay, FIX tag 451, with wire type <c>PriceOffset</c>.
	/// </summary>
	NetChgPrevDay                        = 451,
	/// <summary>
	/// Identifies PartyRole, FIX tag 452, with wire type <c>int</c>.
	/// </summary>
	PartyRole                            = 452,
	/// <summary>
	/// Identifies NoPartyIDs, FIX tag 453, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoPartyIDs                           = 453,
	/// <summary>
	/// Identifies NoSecurityAltID, FIX tag 454, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSecurityAltID                      = 454,
	/// <summary>
	/// Identifies SecurityAltID, FIX tag 455, with wire type <c>String</c>.
	/// </summary>
	SecurityAltID                        = 455,
	/// <summary>
	/// Identifies SecurityAltIDSource, FIX tag 456, with wire type <c>String</c>.
	/// </summary>
	SecurityAltIDSource                  = 456,
	/// <summary>
	/// Identifies NoUnderlyingSecurityAltID, FIX tag 457, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoUnderlyingSecurityAltID            = 457,
	/// <summary>
	/// Identifies UnderlyingSecurityAltID, FIX tag 458, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityAltID              = 458,
	/// <summary>
	/// Identifies UnderlyingSecurityAltIDSource, FIX tag 459, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecurityAltIDSource        = 459,
	/// <summary>
	/// Identifies Product, FIX tag 460, with wire type <c>int</c>.
	/// </summary>
	Product                              = 460,
	/// <summary>
	/// Identifies CFICode, FIX tag 461, with wire type <c>String</c>.
	/// </summary>
	CFICode                              = 461,
	/// <summary>
	/// Identifies UnderlyingProduct, FIX tag 462, with wire type <c>int</c>.
	/// </summary>
	UnderlyingProduct                    = 462,
	/// <summary>
	/// Identifies UnderlyingCFICode, FIX tag 463, with wire type <c>String</c>.
	/// </summary>
	UnderlyingCFICode                    = 463,
	/// <summary>
	/// Identifies TestMessageIndicator, FIX tag 464, with wire type <c>Boolean</c>.
	/// </summary>
	TestMessageIndicator                 = 464,
	/// <summary>
	/// Identifies BookingRefID, FIX tag 466, with wire type <c>String</c>.
	/// </summary>
	BookingRefID                         = 466,
	/// <summary>
	/// Identifies IndividualAllocID, FIX tag 467, with wire type <c>String</c>.
	/// </summary>
	IndividualAllocID                    = 467,
	/// <summary>
	/// Identifies RoundingDirection, FIX tag 468, with wire type <c>char</c>.
	/// </summary>
	RoundingDirection                    = 468,
	/// <summary>
	/// Identifies RoundingModulus, FIX tag 469, with wire type <c>float</c>.
	/// </summary>
	RoundingModulus                      = 469,
	/// <summary>
	/// Identifies CountryOfIssue, FIX tag 470, with wire type <c>Country</c>.
	/// </summary>
	CountryOfIssue                       = 470,
	/// <summary>
	/// Identifies StateOrProvinceOfIssue, FIX tag 471, with wire type <c>String</c>.
	/// </summary>
	StateOrProvinceOfIssue               = 471,
	/// <summary>
	/// Identifies LocaleOfIssue, FIX tag 472, with wire type <c>String</c>.
	/// </summary>
	LocaleOfIssue                        = 472,
	/// <summary>
	/// Identifies NoRegistDtls, FIX tag 473, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoRegistDtls                         = 473,
	/// <summary>
	/// Identifies MailingDtls, FIX tag 474, with wire type <c>String</c>.
	/// </summary>
	MailingDtls                          = 474,
	/// <summary>
	/// Identifies InvestorCountryOfResidence, FIX tag 475, with wire type <c>Country</c>.
	/// </summary>
	InvestorCountryOfResidence           = 475,
	/// <summary>
	/// Identifies PaymentRef, FIX tag 476, with wire type <c>String</c>.
	/// </summary>
	PaymentRef                           = 476,
	/// <summary>
	/// Identifies DistribPaymentMethod, FIX tag 477, with wire type <c>int</c>.
	/// </summary>
	DistribPaymentMethod                 = 477,
	/// <summary>
	/// Identifies CashDistribCurr, FIX tag 478, with wire type <c>Currency</c>.
	/// </summary>
	CashDistribCurr                      = 478,
	/// <summary>
	/// Identifies CommCurrency, FIX tag 479, with wire type <c>Currency</c>.
	/// </summary>
	CommCurrency                         = 479,
	/// <summary>
	/// Identifies CancellationRights, FIX tag 480, with wire type <c>char</c>.
	/// </summary>
	CancellationRights                   = 480,
	/// <summary>
	/// Identifies MoneyLaunderingStatus, FIX tag 481, with wire type <c>char</c>.
	/// </summary>
	MoneyLaunderingStatus                = 481,
	/// <summary>
	/// Identifies MailingInst, FIX tag 482, with wire type <c>String</c>.
	/// </summary>
	MailingInst                          = 482,
	/// <summary>
	/// Identifies TransBkdTime, FIX tag 483, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TransBkdTime                         = 483,
	/// <summary>
	/// Identifies ExecPriceType, FIX tag 484, with wire type <c>char</c>.
	/// </summary>
	ExecPriceType                        = 484,
	/// <summary>
	/// Identifies ExecPriceAdjustment, FIX tag 485, with wire type <c>float</c>.
	/// </summary>
	ExecPriceAdjustment                  = 485,
	/// <summary>
	/// Identifies DateOfBirth, FIX tag 486, with wire type <c>LocalMktDate</c>.
	/// </summary>
	DateOfBirth                          = 486,
	/// <summary>
	/// Identifies TradeReportTransType, FIX tag 487, with wire type <c>int</c>.
	/// </summary>
	TradeReportTransType                 = 487,
	/// <summary>
	/// Identifies CardHolderName, FIX tag 488, with wire type <c>String</c>.
	/// </summary>
	CardHolderName                       = 488,
	/// <summary>
	/// Identifies CardNumber, FIX tag 489, with wire type <c>String</c>.
	/// </summary>
	CardNumber                           = 489,
	/// <summary>
	/// Identifies CardExpDate, FIX tag 490, with wire type <c>LocalMktDate</c>.
	/// </summary>
	CardExpDate                          = 490,
	/// <summary>
	/// Identifies CardIssNum, FIX tag 491, with wire type <c>String</c>.
	/// </summary>
	CardIssNum                           = 491,
	/// <summary>
	/// Identifies PaymentMethod, FIX tag 492, with wire type <c>int</c>.
	/// </summary>
	PaymentMethod                        = 492,
	/// <summary>
	/// Identifies RegistAcctType, FIX tag 493, with wire type <c>String</c>.
	/// </summary>
	RegistAcctType                       = 493,
	/// <summary>
	/// Identifies Designation, FIX tag 494, with wire type <c>String</c>.
	/// </summary>
	Designation                          = 494,
	/// <summary>
	/// Identifies TaxAdvantageType, FIX tag 495, with wire type <c>int</c>.
	/// </summary>
	TaxAdvantageType                     = 495,
	/// <summary>
	/// Identifies RegistRejReasonText, FIX tag 496, with wire type <c>String</c>.
	/// </summary>
	RegistRejReasonText                  = 496,
	/// <summary>
	/// Identifies FundRenewWaiv, FIX tag 497, with wire type <c>char</c>.
	/// </summary>
	FundRenewWaiv                        = 497,
	/// <summary>
	/// Identifies CashDistribAgentName, FIX tag 498, with wire type <c>String</c>.
	/// </summary>
	CashDistribAgentName                 = 498,
	/// <summary>
	/// Identifies CashDistribAgentCode, FIX tag 499, with wire type <c>String</c>.
	/// </summary>
	CashDistribAgentCode                 = 499,
	/// <summary>
	/// Identifies CashDistribAgentAcctNumber, FIX tag 500, with wire type <c>String</c>.
	/// </summary>
	CashDistribAgentAcctNumber           = 500,
	/// <summary>
	/// Identifies CashDistribPayRef, FIX tag 501, with wire type <c>String</c>.
	/// </summary>
	CashDistribPayRef                    = 501,
	/// <summary>
	/// Identifies CashDistribAgentAcctName, FIX tag 502, with wire type <c>String</c>.
	/// </summary>
	CashDistribAgentAcctName             = 502,
	/// <summary>
	/// Identifies CardStartDate, FIX tag 503, with wire type <c>LocalMktDate</c>.
	/// </summary>
	CardStartDate                        = 503,
	/// <summary>
	/// Identifies PaymentDate, FIX tag 504, with wire type <c>LocalMktDate</c>.
	/// </summary>
	PaymentDate                          = 504,
	/// <summary>
	/// Identifies PaymentRemitterID, FIX tag 505, with wire type <c>String</c>.
	/// </summary>
	PaymentRemitterID                    = 505,
	/// <summary>
	/// Identifies RegistStatus, FIX tag 506, with wire type <c>char</c>.
	/// </summary>
	RegistStatus                         = 506,
	/// <summary>
	/// Identifies RegistRejReasonCode, FIX tag 507, with wire type <c>int</c>.
	/// </summary>
	RegistRejReasonCode                  = 507,
	/// <summary>
	/// Identifies RegistRefID, FIX tag 508, with wire type <c>String</c>.
	/// </summary>
	RegistRefID                          = 508,
	/// <summary>
	/// Identifies RegistDtls, FIX tag 509, with wire type <c>String</c>.
	/// </summary>
	RegistDtls                           = 509,
	/// <summary>
	/// Identifies NoDistribInsts, FIX tag 510, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoDistribInsts                       = 510,
	/// <summary>
	/// Identifies RegistEmail, FIX tag 511, with wire type <c>String</c>.
	/// </summary>
	RegistEmail                          = 511,
	/// <summary>
	/// Identifies DistribPercentage, FIX tag 512, with wire type <c>Percentage</c>.
	/// </summary>
	DistribPercentage                    = 512,
	/// <summary>
	/// Identifies RegistID, FIX tag 513, with wire type <c>String</c>.
	/// </summary>
	RegistID                             = 513,
	/// <summary>
	/// Identifies RegistTransType, FIX tag 514, with wire type <c>char</c>.
	/// </summary>
	RegistTransType                      = 514,
	/// <summary>
	/// Identifies ExecValuationPoint, FIX tag 515, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	ExecValuationPoint                   = 515,
	/// <summary>
	/// Identifies OrderPercent, FIX tag 516, with wire type <c>Percentage</c>.
	/// </summary>
	OrderPercent                         = 516,
	/// <summary>
	/// Identifies OwnershipType, FIX tag 517, with wire type <c>char</c>.
	/// </summary>
	OwnershipType                        = 517,
	/// <summary>
	/// Identifies NoContAmts, FIX tag 518, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoContAmts                           = 518,
	/// <summary>
	/// Identifies ContAmtType, FIX tag 519, with wire type <c>int</c>.
	/// </summary>
	ContAmtType                          = 519,
	/// <summary>
	/// Identifies ContAmtValue, FIX tag 520, with wire type <c>float</c>.
	/// </summary>
	ContAmtValue                         = 520,
	/// <summary>
	/// Identifies ContAmtCurr, FIX tag 521, with wire type <c>Currency</c>.
	/// </summary>
	ContAmtCurr                          = 521,
	/// <summary>
	/// Identifies OwnerType, FIX tag 522, with wire type <c>int</c>.
	/// </summary>
	OwnerType                            = 522,
	/// <summary>
	/// Identifies PartySubID, FIX tag 523, with wire type <c>String</c>.
	/// </summary>
	PartySubID                           = 523,
	/// <summary>
	/// Identifies NestedPartyID, FIX tag 524, with wire type <c>String</c>.
	/// </summary>
	NestedPartyID                        = 524,
	/// <summary>
	/// Identifies NestedPartyIDSource, FIX tag 525, with wire type <c>char</c>.
	/// </summary>
	NestedPartyIDSource                  = 525,
	/// <summary>
	/// Identifies SecondaryClOrdID, FIX tag 526, with wire type <c>String</c>.
	/// </summary>
	SecondaryClOrdID                     = 526,
	/// <summary>
	/// Identifies SecondaryExecID, FIX tag 527, with wire type <c>String</c>.
	/// </summary>
	SecondaryExecID                      = 527,
	/// <summary>
	/// Identifies OrderCapacity, FIX tag 528, with wire type <c>char</c>.
	/// </summary>
	OrderCapacity                        = 528,
	/// <summary>
	/// Identifies OrderRestrictions, FIX tag 529, with wire type <c>MultipleValueString</c>.
	/// </summary>
	OrderRestrictions                    = 529,
	/// <summary>
	/// Identifies MassCancelRequestType, FIX tag 530, with wire type <c>char</c>.
	/// </summary>
	MassCancelRequestType                = 530,
	/// <summary>
	/// Identifies MassCancelResponse, FIX tag 531, with wire type <c>char</c>.
	/// </summary>
	MassCancelResponse                   = 531,
	/// <summary>
	/// Identifies MassCancelRejectReason, FIX tag 532, with wire type <c>String</c>.
	/// </summary>
	MassCancelRejectReason               = 532,
	/// <summary>
	/// Identifies TotalAffectedOrders, FIX tag 533, with wire type <c>int</c>.
	/// </summary>
	TotalAffectedOrders                  = 533,
	/// <summary>
	/// Identifies NoAffectedOrders, FIX tag 534, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoAffectedOrders                     = 534,
	/// <summary>
	/// Identifies AffectedOrderID, FIX tag 535, with wire type <c>String</c>.
	/// </summary>
	AffectedOrderID                      = 535,
	/// <summary>
	/// Identifies AffectedSecondaryOrderID, FIX tag 536, with wire type <c>String</c>.
	/// </summary>
	AffectedSecondaryOrderID             = 536,
	/// <summary>
	/// Identifies QuoteType, FIX tag 537, with wire type <c>int</c>.
	/// </summary>
	QuoteType                            = 537,
	/// <summary>
	/// Identifies NestedPartyRole, FIX tag 538, with wire type <c>int</c>.
	/// </summary>
	NestedPartyRole                      = 538,
	/// <summary>
	/// Identifies NoNestedPartyIDs, FIX tag 539, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNestedPartyIDs                     = 539,
	/// <summary>
	/// Identifies TotalAccruedInterestAmt, FIX tag 540, with wire type <c>Amt</c>.
	/// </summary>
	TotalAccruedInterestAmt              = 540,
	/// <summary>
	/// Identifies MaturityDate, FIX tag 541, with wire type <c>LocalMktDate</c>.
	/// </summary>
	MaturityDate                         = 541,
	/// <summary>
	/// Identifies UnderlyingMaturityDate, FIX tag 542, with wire type <c>LocalMktDate</c>.
	/// </summary>
	UnderlyingMaturityDate               = 542,
	/// <summary>
	/// Identifies InstrRegistry, FIX tag 543, with wire type <c>String</c>.
	/// </summary>
	InstrRegistry                        = 543,
	/// <summary>
	/// Identifies CashMargin, FIX tag 544, with wire type <c>char</c>.
	/// </summary>
	CashMargin                           = 544,
	/// <summary>
	/// Identifies NestedPartySubID, FIX tag 545, with wire type <c>String</c>.
	/// </summary>
	NestedPartySubID                     = 545,
	/// <summary>
	/// Identifies Scope, FIX tag 546, with wire type <c>MultipleValueString</c>.
	/// </summary>
	Scope                                = 546,
	/// <summary>
	/// Identifies MDImplicitDelete, FIX tag 547, with wire type <c>Boolean</c>.
	/// </summary>
	MDImplicitDelete                     = 547,
	/// <summary>
	/// Identifies CrossID, FIX tag 548, with wire type <c>String</c>.
	/// </summary>
	CrossID                              = 548,
	/// <summary>
	/// Identifies CrossType, FIX tag 549, with wire type <c>int</c>.
	/// </summary>
	CrossType                            = 549,
	/// <summary>
	/// Identifies CrossPrioritization, FIX tag 550, with wire type <c>int</c>.
	/// </summary>
	CrossPrioritization                  = 550,
	/// <summary>
	/// Identifies OrigCrossID, FIX tag 551, with wire type <c>String</c>.
	/// </summary>
	OrigCrossID                          = 551,
	/// <summary>
	/// Identifies NoSides, FIX tag 552, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSides                              = 552,
	/// <summary>
	/// Identifies Username, FIX tag 553, with wire type <c>String</c>.
	/// </summary>
	Username                             = 553,
	/// <summary>
	/// Identifies Password, FIX tag 554, with wire type <c>String</c>.
	/// </summary>
	Password                             = 554,
	/// <summary>
	/// Identifies NoLegs, FIX tag 555, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoLegs                               = 555,
	/// <summary>
	/// Identifies LegCurrency, FIX tag 556, with wire type <c>Currency</c>.
	/// </summary>
	LegCurrency                          = 556,
	/// <summary>
	/// Identifies TotNoSecurityTypes, FIX tag 557, with wire type <c>int</c>.
	/// </summary>
	TotNoSecurityTypes                   = 557,
	/// <summary>
	/// Identifies NoSecurityTypes, FIX tag 558, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSecurityTypes                      = 558,
	/// <summary>
	/// Identifies SecurityListRequestType, FIX tag 559, with wire type <c>int</c>.
	/// </summary>
	SecurityListRequestType              = 559,
	/// <summary>
	/// Identifies SecurityRequestResult, FIX tag 560, with wire type <c>int</c>.
	/// </summary>
	SecurityRequestResult                = 560,
	/// <summary>
	/// Identifies RoundLot, FIX tag 561, with wire type <c>Qty</c>.
	/// </summary>
	RoundLot                             = 561,
	/// <summary>
	/// Identifies MinTradeVol, FIX tag 562, with wire type <c>Qty</c>.
	/// </summary>
	MinTradeVol                          = 562,
	/// <summary>
	/// Identifies MultiLegRptTypeReq, FIX tag 563, with wire type <c>int</c>.
	/// </summary>
	MultiLegRptTypeReq                   = 563,
	/// <summary>
	/// Identifies LegPositionEffect, FIX tag 564, with wire type <c>char</c>.
	/// </summary>
	LegPositionEffect                    = 564,
	/// <summary>
	/// Identifies LegCoveredOrUncovered, FIX tag 565, with wire type <c>int</c>.
	/// </summary>
	LegCoveredOrUncovered                = 565,
	/// <summary>
	/// Identifies LegPrice, FIX tag 566, with wire type <c>Price</c>.
	/// </summary>
	LegPrice                             = 566,
	/// <summary>
	/// Identifies TradSesStatusRejReason, FIX tag 567, with wire type <c>int</c>.
	/// </summary>
	TradSesStatusRejReason               = 567,
	/// <summary>
	/// Identifies TradeRequestID, FIX tag 568, with wire type <c>String</c>.
	/// </summary>
	TradeRequestID                       = 568,
	/// <summary>
	/// Identifies TradeRequestType, FIX tag 569, with wire type <c>int</c>.
	/// </summary>
	TradeRequestType                     = 569,
	/// <summary>
	/// Identifies PreviouslyReported, FIX tag 570, with wire type <c>Boolean</c>.
	/// </summary>
	PreviouslyReported                   = 570,
	/// <summary>
	/// Identifies TradeReportID, FIX tag 571, with wire type <c>String</c>.
	/// </summary>
	TradeReportID                        = 571,
	/// <summary>
	/// Identifies TradeReportRefID, FIX tag 572, with wire type <c>String</c>.
	/// </summary>
	TradeReportRefID                     = 572,
	/// <summary>
	/// Identifies MatchStatus, FIX tag 573, with wire type <c>char</c>.
	/// </summary>
	MatchStatus                          = 573,
	/// <summary>
	/// Identifies MatchType, FIX tag 574, with wire type <c>String</c>.
	/// </summary>
	MatchType                            = 574,
	/// <summary>
	/// Identifies OddLot, FIX tag 575, with wire type <c>Boolean</c>.
	/// </summary>
	OddLot                               = 575,
	/// <summary>
	/// Identifies NoClearingInstructions, FIX tag 576, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoClearingInstructions               = 576,
	/// <summary>
	/// Identifies ClearingInstruction, FIX tag 577, with wire type <c>int</c>.
	/// </summary>
	ClearingInstruction                  = 577,
	/// <summary>
	/// Identifies TradeInputSource, FIX tag 578, with wire type <c>String</c>.
	/// </summary>
	TradeInputSource                     = 578,
	/// <summary>
	/// Identifies TradeInputDevice, FIX tag 579, with wire type <c>String</c>.
	/// </summary>
	TradeInputDevice                     = 579,
	/// <summary>
	/// Identifies NoDates, FIX tag 580, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoDates                              = 580,
	/// <summary>
	/// Identifies AccountType, FIX tag 581, with wire type <c>int</c>.
	/// </summary>
	AccountType                          = 581,
	/// <summary>
	/// Identifies CustOrderCapacity, FIX tag 582, with wire type <c>int</c>.
	/// </summary>
	CustOrderCapacity                    = 582,
	/// <summary>
	/// Identifies ClOrdLinkID, FIX tag 583, with wire type <c>String</c>.
	/// </summary>
	ClOrdLinkID                          = 583,
	/// <summary>
	/// Identifies MassStatusReqID, FIX tag 584, with wire type <c>String</c>.
	/// </summary>
	MassStatusReqID                      = 584,
	/// <summary>
	/// Identifies MassStatusReqType, FIX tag 585, with wire type <c>int</c>.
	/// </summary>
	MassStatusReqType                    = 585,
	/// <summary>
	/// Identifies OrigOrdModTime, FIX tag 586, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	OrigOrdModTime                       = 586,
	/// <summary>
	/// Identifies LegSettlType, FIX tag 587, with wire type <c>char</c>.
	/// </summary>
	LegSettlType                         = 587,
	/// <summary>
	/// Identifies LegSettlDate, FIX tag 588, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegSettlDate                         = 588,
	/// <summary>
	/// Identifies DayBookingInst, FIX tag 589, with wire type <c>char</c>.
	/// </summary>
	DayBookingInst                       = 589,
	/// <summary>
	/// Identifies BookingUnit, FIX tag 590, with wire type <c>char</c>.
	/// </summary>
	BookingUnit                          = 590,
	/// <summary>
	/// Identifies PreallocMethod, FIX tag 591, with wire type <c>char</c>.
	/// </summary>
	PreallocMethod                       = 591,
	/// <summary>
	/// Identifies UnderlyingCountryOfIssue, FIX tag 592, with wire type <c>Country</c>.
	/// </summary>
	UnderlyingCountryOfIssue             = 592,
	/// <summary>
	/// Identifies UnderlyingStateOrProvinceOfIssue, FIX tag 593, with wire type <c>String</c>.
	/// </summary>
	UnderlyingStateOrProvinceOfIssue     = 593,
	/// <summary>
	/// Identifies UnderlyingLocaleOfIssue, FIX tag 594, with wire type <c>String</c>.
	/// </summary>
	UnderlyingLocaleOfIssue              = 594,
	/// <summary>
	/// Identifies UnderlyingInstrRegistry, FIX tag 595, with wire type <c>String</c>.
	/// </summary>
	UnderlyingInstrRegistry              = 595,
	/// <summary>
	/// Identifies LegCountryOfIssue, FIX tag 596, with wire type <c>Country</c>.
	/// </summary>
	LegCountryOfIssue                    = 596,
	/// <summary>
	/// Identifies LegStateOrProvinceOfIssue, FIX tag 597, with wire type <c>String</c>.
	/// </summary>
	LegStateOrProvinceOfIssue            = 597,
	/// <summary>
	/// Identifies LegLocaleOfIssue, FIX tag 598, with wire type <c>String</c>.
	/// </summary>
	LegLocaleOfIssue                     = 598,
	/// <summary>
	/// Identifies LegInstrRegistry, FIX tag 599, with wire type <c>String</c>.
	/// </summary>
	LegInstrRegistry                     = 599,
	/// <summary>
	/// Identifies LegSymbol, FIX tag 600, with wire type <c>String</c>.
	/// </summary>
	LegSymbol                            = 600,
	/// <summary>
	/// Identifies LegSymbolSfx, FIX tag 601, with wire type <c>String</c>.
	/// </summary>
	LegSymbolSfx                         = 601,
	/// <summary>
	/// Identifies LegSecurityID, FIX tag 602, with wire type <c>String</c>.
	/// </summary>
	LegSecurityID                        = 602,
	/// <summary>
	/// Identifies LegSecurityIDSource, FIX tag 603, with wire type <c>String</c>.
	/// </summary>
	LegSecurityIDSource                  = 603,
	/// <summary>
	/// Identifies NoLegSecurityAltID, FIX tag 604, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoLegSecurityAltID                   = 604,
	/// <summary>
	/// Identifies LegSecurityAltID, FIX tag 605, with wire type <c>String</c>.
	/// </summary>
	LegSecurityAltID                     = 605,
	/// <summary>
	/// Identifies LegSecurityAltIDSource, FIX tag 606, with wire type <c>String</c>.
	/// </summary>
	LegSecurityAltIDSource               = 606,
	/// <summary>
	/// Identifies LegProduct, FIX tag 607, with wire type <c>int</c>.
	/// </summary>
	LegProduct                           = 607,
	/// <summary>
	/// Identifies LegCFICode, FIX tag 608, with wire type <c>String</c>.
	/// </summary>
	LegCFICode                           = 608,
	/// <summary>
	/// Identifies LegSecurityType, FIX tag 609, with wire type <c>String</c>.
	/// </summary>
	LegSecurityType                      = 609,
	/// <summary>
	/// Identifies LegMaturityMonthYear, FIX tag 610, with wire type <c>MonthYear</c>.
	/// </summary>
	LegMaturityMonthYear                 = 610,
	/// <summary>
	/// Identifies LegMaturityDate, FIX tag 611, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegMaturityDate                      = 611,
	/// <summary>
	/// Identifies LegStrikePrice, FIX tag 612, with wire type <c>Price</c>.
	/// </summary>
	LegStrikePrice                       = 612,
	/// <summary>
	/// Identifies LegOptAttribute, FIX tag 613, with wire type <c>char</c>.
	/// </summary>
	LegOptAttribute                      = 613,
	/// <summary>
	/// Identifies LegContractMultiplier, FIX tag 614, with wire type <c>float</c>.
	/// </summary>
	LegContractMultiplier                = 614,
	/// <summary>
	/// Identifies LegCouponRate, FIX tag 615, with wire type <c>Percentage</c>.
	/// </summary>
	LegCouponRate                        = 615,
	/// <summary>
	/// Identifies LegSecurityExchange, FIX tag 616, with wire type <c>Exchange</c>.
	/// </summary>
	LegSecurityExchange                  = 616,
	/// <summary>
	/// Identifies LegIssuer, FIX tag 617, with wire type <c>String</c>.
	/// </summary>
	LegIssuer                            = 617,
	/// <summary>
	/// Identifies EncodedLegIssuerLen, FIX tag 618, with wire type <c>Length</c>.
	/// </summary>
	EncodedLegIssuerLen                  = 618,
	/// <summary>
	/// Identifies EncodedLegIssuer, FIX tag 619, with wire type <c>data</c>.
	/// </summary>
	EncodedLegIssuer                     = 619,
	/// <summary>
	/// Identifies LegSecurityDesc, FIX tag 620, with wire type <c>String</c>.
	/// </summary>
	LegSecurityDesc                      = 620,
	/// <summary>
	/// Identifies EncodedLegSecurityDescLen, FIX tag 621, with wire type <c>Length</c>.
	/// </summary>
	EncodedLegSecurityDescLen            = 621,
	/// <summary>
	/// Identifies EncodedLegSecurityDesc, FIX tag 622, with wire type <c>data</c>.
	/// </summary>
	EncodedLegSecurityDesc               = 622,
	/// <summary>
	/// Identifies LegRatioQty, FIX tag 623, with wire type <c>float</c>.
	/// </summary>
	LegRatioQty                          = 623,
	/// <summary>
	/// Identifies LegSide, FIX tag 624, with wire type <c>char</c>.
	/// </summary>
	LegSide                              = 624,
	/// <summary>
	/// Identifies TradingSessionSubID, FIX tag 625, with wire type <c>String</c>.
	/// </summary>
	TradingSessionSubID                  = 625,
	/// <summary>
	/// Identifies AllocType, FIX tag 626, with wire type <c>int</c>.
	/// </summary>
	AllocType                            = 626,
	/// <summary>
	/// Identifies NoHops, FIX tag 627, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoHops                               = 627,
	/// <summary>
	/// Identifies HopCompID, FIX tag 628, with wire type <c>String</c>.
	/// </summary>
	HopCompID                            = 628,
	/// <summary>
	/// Identifies HopSendingTime, FIX tag 629, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	HopSendingTime                       = 629,
	/// <summary>
	/// Identifies HopRefID, FIX tag 630, with wire type <c>SeqNum</c>.
	/// </summary>
	HopRefID                             = 630,
	/// <summary>
	/// Identifies MidPx, FIX tag 631, with wire type <c>Price</c>.
	/// </summary>
	MidPx                                = 631,
	/// <summary>
	/// Identifies BidYield, FIX tag 632, with wire type <c>Percentage</c>.
	/// </summary>
	BidYield                             = 632,
	/// <summary>
	/// Identifies MidYield, FIX tag 633, with wire type <c>Percentage</c>.
	/// </summary>
	MidYield                             = 633,
	/// <summary>
	/// Identifies OfferYield, FIX tag 634, with wire type <c>Percentage</c>.
	/// </summary>
	OfferYield                           = 634,
	/// <summary>
	/// Identifies ClearingFeeIndicator, FIX tag 635, with wire type <c>String</c>.
	/// </summary>
	ClearingFeeIndicator                 = 635,
	/// <summary>
	/// Identifies WorkingIndicator, FIX tag 636, with wire type <c>Boolean</c>.
	/// </summary>
	WorkingIndicator                     = 636,
	/// <summary>
	/// Identifies LegLastPx, FIX tag 637, with wire type <c>Price</c>.
	/// </summary>
	LegLastPx                            = 637,
	/// <summary>
	/// Identifies PriorityIndicator, FIX tag 638, with wire type <c>int</c>.
	/// </summary>
	PriorityIndicator                    = 638,
	/// <summary>
	/// Identifies PriceImprovement, FIX tag 639, with wire type <c>PriceOffset</c>.
	/// </summary>
	PriceImprovement                     = 639,
	/// <summary>
	/// Identifies Price2, FIX tag 640, with wire type <c>Price</c>.
	/// </summary>
	Price2                               = 640,
	/// <summary>
	/// Identifies LastForwardPoints2, FIX tag 641, with wire type <c>PriceOffset</c>.
	/// </summary>
	LastForwardPoints2                   = 641,
	/// <summary>
	/// Identifies BidForwardPoints2, FIX tag 642, with wire type <c>PriceOffset</c>.
	/// </summary>
	BidForwardPoints2                    = 642,
	/// <summary>
	/// Identifies OfferForwardPoints2, FIX tag 643, with wire type <c>PriceOffset</c>.
	/// </summary>
	OfferForwardPoints2                  = 643,
	/// <summary>
	/// Identifies RFQReqID, FIX tag 644, with wire type <c>String</c>.
	/// </summary>
	RFQReqID                             = 644,
	/// <summary>
	/// Identifies MktBidPx, FIX tag 645, with wire type <c>Price</c>.
	/// </summary>
	MktBidPx                             = 645,
	/// <summary>
	/// Identifies MktOfferPx, FIX tag 646, with wire type <c>Price</c>.
	/// </summary>
	MktOfferPx                           = 646,
	/// <summary>
	/// Identifies MinBidSize, FIX tag 647, with wire type <c>Qty</c>.
	/// </summary>
	MinBidSize                           = 647,
	/// <summary>
	/// Identifies MinOfferSize, FIX tag 648, with wire type <c>Qty</c>.
	/// </summary>
	MinOfferSize                         = 648,
	/// <summary>
	/// Identifies QuoteStatusReqID, FIX tag 649, with wire type <c>String</c>.
	/// </summary>
	QuoteStatusReqID                     = 649,
	/// <summary>
	/// Identifies LegalConfirm, FIX tag 650, with wire type <c>Boolean</c>.
	/// </summary>
	LegalConfirm                         = 650,
	/// <summary>
	/// Identifies UnderlyingLastPx, FIX tag 651, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingLastPx                     = 651,
	/// <summary>
	/// Identifies UnderlyingLastQty, FIX tag 652, with wire type <c>Qty</c>.
	/// </summary>
	UnderlyingLastQty                    = 652,
	/// <summary>
	/// Identifies LegRefID, FIX tag 654, with wire type <c>String</c>.
	/// </summary>
	LegRefID                             = 654,
	/// <summary>
	/// Identifies ContraLegRefID, FIX tag 655, with wire type <c>String</c>.
	/// </summary>
	ContraLegRefID                       = 655,
	/// <summary>
	/// Identifies SettlCurrBidFxRate, FIX tag 656, with wire type <c>float</c>.
	/// </summary>
	SettlCurrBidFxRate                   = 656,
	/// <summary>
	/// Identifies SettlCurrOfferFxRate, FIX tag 657, with wire type <c>float</c>.
	/// </summary>
	SettlCurrOfferFxRate                 = 657,
	/// <summary>
	/// Identifies QuoteRequestRejectReason, FIX tag 658, with wire type <c>int</c>.
	/// </summary>
	QuoteRequestRejectReason             = 658,
	/// <summary>
	/// Identifies SideComplianceID, FIX tag 659, with wire type <c>String</c>.
	/// </summary>
	SideComplianceID                     = 659,
	/// <summary>
	/// Identifies AcctIDSource, FIX tag 660, with wire type <c>int</c>.
	/// </summary>
	AcctIDSource                         = 660,
	/// <summary>
	/// Identifies AllocAcctIDSource, FIX tag 661, with wire type <c>int</c>.
	/// </summary>
	AllocAcctIDSource                    = 661,
	/// <summary>
	/// Identifies BenchmarkPrice, FIX tag 662, with wire type <c>Price</c>.
	/// </summary>
	BenchmarkPrice                       = 662,
	/// <summary>
	/// Identifies BenchmarkPriceType, FIX tag 663, with wire type <c>int</c>.
	/// </summary>
	BenchmarkPriceType                   = 663,
	/// <summary>
	/// Identifies ConfirmID, FIX tag 664, with wire type <c>String</c>.
	/// </summary>
	ConfirmID                            = 664,
	/// <summary>
	/// Identifies ConfirmStatus, FIX tag 665, with wire type <c>int</c>.
	/// </summary>
	ConfirmStatus                        = 665,
	/// <summary>
	/// Identifies ConfirmTransType, FIX tag 666, with wire type <c>int</c>.
	/// </summary>
	ConfirmTransType                     = 666,
	/// <summary>
	/// Identifies ContractSettlMonth, FIX tag 667, with wire type <c>MonthYear</c>.
	/// </summary>
	ContractSettlMonth                   = 667,
	/// <summary>
	/// Identifies DeliveryForm, FIX tag 668, with wire type <c>int</c>.
	/// </summary>
	DeliveryForm                         = 668,
	/// <summary>
	/// Identifies LastParPx, FIX tag 669, with wire type <c>Price</c>.
	/// </summary>
	LastParPx                            = 669,
	/// <summary>
	/// Identifies NoLegAllocs, FIX tag 670, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoLegAllocs                          = 670,
	/// <summary>
	/// Identifies LegAllocAccount, FIX tag 671, with wire type <c>String</c>.
	/// </summary>
	LegAllocAccount                      = 671,
	/// <summary>
	/// Identifies LegIndividualAllocID, FIX tag 672, with wire type <c>String</c>.
	/// </summary>
	LegIndividualAllocID                 = 672,
	/// <summary>
	/// Identifies LegAllocQty, FIX tag 673, with wire type <c>Qty</c>.
	/// </summary>
	LegAllocQty                          = 673,
	/// <summary>
	/// Identifies LegAllocAcctIDSource, FIX tag 674, with wire type <c>int</c>.
	/// </summary>
	LegAllocAcctIDSource                 = 674,
	/// <summary>
	/// Identifies LegSettlCurrency, FIX tag 675, with wire type <c>Currency</c>.
	/// </summary>
	LegSettlCurrency                     = 675,
	/// <summary>
	/// Identifies LegBenchmarkCurveCurrency, FIX tag 676, with wire type <c>Currency</c>.
	/// </summary>
	LegBenchmarkCurveCurrency            = 676,
	/// <summary>
	/// Identifies LegBenchmarkCurveName, FIX tag 677, with wire type <c>String</c>.
	/// </summary>
	LegBenchmarkCurveName                = 677,
	/// <summary>
	/// Identifies LegBenchmarkCurvePoint, FIX tag 678, with wire type <c>String</c>.
	/// </summary>
	LegBenchmarkCurvePoint               = 678,
	/// <summary>
	/// Identifies LegBenchmarkPrice, FIX tag 679, with wire type <c>Price</c>.
	/// </summary>
	LegBenchmarkPrice                    = 679,
	/// <summary>
	/// Identifies LegBenchmarkPriceType, FIX tag 680, with wire type <c>int</c>.
	/// </summary>
	LegBenchmarkPriceType                = 680,
	/// <summary>
	/// Identifies LegBidPx, FIX tag 681, with wire type <c>Price</c>.
	/// </summary>
	LegBidPx                             = 681,
	/// <summary>
	/// Identifies LegIOIQty, FIX tag 682, with wire type <c>String</c>.
	/// </summary>
	LegIOIQty                            = 682,
	/// <summary>
	/// Identifies NoLegStipulations, FIX tag 683, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoLegStipulations                    = 683,
	/// <summary>
	/// Identifies LegOfferPx, FIX tag 684, with wire type <c>Price</c>.
	/// </summary>
	LegOfferPx                           = 684,
	/// <summary>
	/// Identifies LegPriceType, FIX tag 686, with wire type <c>int</c>.
	/// </summary>
	LegPriceType                         = 686,
	/// <summary>
	/// Identifies LegQty, FIX tag 687, with wire type <c>Qty</c>.
	/// </summary>
	LegQty                               = 687,
	/// <summary>
	/// Identifies LegStipulationType, FIX tag 688, with wire type <c>String</c>.
	/// </summary>
	LegStipulationType                   = 688,
	/// <summary>
	/// Identifies LegStipulationValue, FIX tag 689, with wire type <c>String</c>.
	/// </summary>
	LegStipulationValue                  = 689,
	/// <summary>
	/// Identifies LegSwapType, FIX tag 690, with wire type <c>int</c>.
	/// </summary>
	LegSwapType                          = 690,
	/// <summary>
	/// Identifies Pool, FIX tag 691, with wire type <c>String</c>.
	/// </summary>
	Pool                                 = 691,
	/// <summary>
	/// Identifies QuotePriceType, FIX tag 692, with wire type <c>int</c>.
	/// </summary>
	QuotePriceType                       = 692,
	/// <summary>
	/// Identifies QuoteRespID, FIX tag 693, with wire type <c>String</c>.
	/// </summary>
	QuoteRespID                          = 693,
	/// <summary>
	/// Identifies QuoteRespType, FIX tag 694, with wire type <c>int</c>.
	/// </summary>
	QuoteRespType                        = 694,
	/// <summary>
	/// Identifies QuoteQualifier, FIX tag 695, with wire type <c>char</c>.
	/// </summary>
	QuoteQualifier                       = 695,
	/// <summary>
	/// Identifies YieldRedemptionDate, FIX tag 696, with wire type <c>LocalMktDate</c>.
	/// </summary>
	YieldRedemptionDate                  = 696,
	/// <summary>
	/// Identifies YieldRedemptionPrice, FIX tag 697, with wire type <c>Price</c>.
	/// </summary>
	YieldRedemptionPrice                 = 697,
	/// <summary>
	/// Identifies YieldRedemptionPriceType, FIX tag 698, with wire type <c>int</c>.
	/// </summary>
	YieldRedemptionPriceType             = 698,
	/// <summary>
	/// Identifies BenchmarkSecurityID, FIX tag 699, with wire type <c>String</c>.
	/// </summary>
	BenchmarkSecurityID                  = 699,
	/// <summary>
	/// Identifies ReversalIndicator, FIX tag 700, with wire type <c>Boolean</c>.
	/// </summary>
	ReversalIndicator                    = 700,
	/// <summary>
	/// Identifies YieldCalcDate, FIX tag 701, with wire type <c>LocalMktDate</c>.
	/// </summary>
	YieldCalcDate                        = 701,
	/// <summary>
	/// Identifies NoPositions, FIX tag 702, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoPositions                          = 702,
	/// <summary>
	/// Identifies PosType, FIX tag 703, with wire type <c>String</c>.
	/// </summary>
	PosType                              = 703,
	/// <summary>
	/// Identifies LongQty, FIX tag 704, with wire type <c>Qty</c>.
	/// </summary>
	LongQty                              = 704,
	/// <summary>
	/// Identifies ShortQty, FIX tag 705, with wire type <c>Qty</c>.
	/// </summary>
	ShortQty                             = 705,
	/// <summary>
	/// Identifies PosQtyStatus, FIX tag 706, with wire type <c>int</c>.
	/// </summary>
	PosQtyStatus                         = 706,
	/// <summary>
	/// Identifies PosAmtType, FIX tag 707, with wire type <c>String</c>.
	/// </summary>
	PosAmtType                           = 707,
	/// <summary>
	/// Identifies PosAmt, FIX tag 708, with wire type <c>Amt</c>.
	/// </summary>
	PosAmt                               = 708,
	/// <summary>
	/// Identifies PosTransType, FIX tag 709, with wire type <c>int</c>.
	/// </summary>
	PosTransType                         = 709,
	/// <summary>
	/// Identifies PosReqID, FIX tag 710, with wire type <c>String</c>.
	/// </summary>
	PosReqID                             = 710,
	/// <summary>
	/// Identifies NoUnderlyings, FIX tag 711, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoUnderlyings                        = 711,
	/// <summary>
	/// Identifies PosMaintAction, FIX tag 712, with wire type <c>int</c>.
	/// </summary>
	PosMaintAction                       = 712,
	/// <summary>
	/// Identifies OrigPosReqRefID, FIX tag 713, with wire type <c>String</c>.
	/// </summary>
	OrigPosReqRefID                      = 713,
	/// <summary>
	/// Identifies PosMaintRptRefID, FIX tag 714, with wire type <c>String</c>.
	/// </summary>
	PosMaintRptRefID                     = 714,
	/// <summary>
	/// Identifies ClearingBusinessDate, FIX tag 715, with wire type <c>LocalMktDate</c>.
	/// </summary>
	ClearingBusinessDate                 = 715,
	/// <summary>
	/// Identifies SettlSessID, FIX tag 716, with wire type <c>String</c>.
	/// </summary>
	SettlSessID                          = 716,
	/// <summary>
	/// Identifies SettlSessSubID, FIX tag 717, with wire type <c>String</c>.
	/// </summary>
	SettlSessSubID                       = 717,
	/// <summary>
	/// Identifies AdjustmentType, FIX tag 718, with wire type <c>int</c>.
	/// </summary>
	AdjustmentType                       = 718,
	/// <summary>
	/// Identifies ContraryInstructionIndicator, FIX tag 719, with wire type <c>Boolean</c>.
	/// </summary>
	ContraryInstructionIndicator         = 719,
	/// <summary>
	/// Identifies PriorSpreadIndicator, FIX tag 720, with wire type <c>Boolean</c>.
	/// </summary>
	PriorSpreadIndicator                 = 720,
	/// <summary>
	/// Identifies PosMaintRptID, FIX tag 721, with wire type <c>String</c>.
	/// </summary>
	PosMaintRptID                        = 721,
	/// <summary>
	/// Identifies PosMaintStatus, FIX tag 722, with wire type <c>int</c>.
	/// </summary>
	PosMaintStatus                       = 722,
	/// <summary>
	/// Identifies PosMaintResult, FIX tag 723, with wire type <c>int</c>.
	/// </summary>
	PosMaintResult                       = 723,
	/// <summary>
	/// Identifies PosReqType, FIX tag 724, with wire type <c>int</c>.
	/// </summary>
	PosReqType                           = 724,
	/// <summary>
	/// Identifies ResponseTransportType, FIX tag 725, with wire type <c>int</c>.
	/// </summary>
	ResponseTransportType                = 725,
	/// <summary>
	/// Identifies ResponseDestination, FIX tag 726, with wire type <c>String</c>.
	/// </summary>
	ResponseDestination                  = 726,
	/// <summary>
	/// Identifies TotalNumPosReports, FIX tag 727, with wire type <c>int</c>.
	/// </summary>
	TotalNumPosReports                   = 727,
	/// <summary>
	/// Identifies PosReqResult, FIX tag 728, with wire type <c>int</c>.
	/// </summary>
	PosReqResult                         = 728,
	/// <summary>
	/// Identifies PosReqStatus, FIX tag 729, with wire type <c>int</c>.
	/// </summary>
	PosReqStatus                         = 729,
	/// <summary>
	/// Identifies SettlPrice, FIX tag 730, with wire type <c>Price</c>.
	/// </summary>
	SettlPrice                           = 730,
	/// <summary>
	/// Identifies SettlPriceType, FIX tag 731, with wire type <c>int</c>.
	/// </summary>
	SettlPriceType                       = 731,
	/// <summary>
	/// Identifies UnderlyingSettlPrice, FIX tag 732, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingSettlPrice                 = 732,
	/// <summary>
	/// Identifies UnderlyingSettlPriceType, FIX tag 733, with wire type <c>int</c>.
	/// </summary>
	UnderlyingSettlPriceType             = 733,
	/// <summary>
	/// Identifies PriorSettlPrice, FIX tag 734, with wire type <c>Price</c>.
	/// </summary>
	PriorSettlPrice                      = 734,
	/// <summary>
	/// Identifies NoQuoteQualifiers, FIX tag 735, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoQuoteQualifiers                    = 735,
	/// <summary>
	/// Identifies AllocSettlCurrency, FIX tag 736, with wire type <c>Currency</c>.
	/// </summary>
	AllocSettlCurrency                   = 736,
	/// <summary>
	/// Identifies AllocSettlCurrAmt, FIX tag 737, with wire type <c>Amt</c>.
	/// </summary>
	AllocSettlCurrAmt                    = 737,
	/// <summary>
	/// Identifies InterestAtMaturity, FIX tag 738, with wire type <c>Amt</c>.
	/// </summary>
	InterestAtMaturity                   = 738,
	/// <summary>
	/// Identifies LegDatedDate, FIX tag 739, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegDatedDate                         = 739,
	/// <summary>
	/// Identifies LegPool, FIX tag 740, with wire type <c>String</c>.
	/// </summary>
	LegPool                              = 740,
	/// <summary>
	/// Identifies AllocInterestAtMaturity, FIX tag 741, with wire type <c>Amt</c>.
	/// </summary>
	AllocInterestAtMaturity              = 741,
	/// <summary>
	/// Identifies AllocAccruedInterestAmt, FIX tag 742, with wire type <c>Amt</c>.
	/// </summary>
	AllocAccruedInterestAmt              = 742,
	/// <summary>
	/// Identifies DeliveryDate, FIX tag 743, with wire type <c>LocalMktDate</c>.
	/// </summary>
	DeliveryDate                         = 743,
	/// <summary>
	/// Identifies AssignmentMethod, FIX tag 744, with wire type <c>char</c>.
	/// </summary>
	AssignmentMethod                     = 744,
	/// <summary>
	/// Identifies AssignmentUnit, FIX tag 745, with wire type <c>Qty</c>.
	/// </summary>
	AssignmentUnit                       = 745,
	/// <summary>
	/// Identifies OpenInterest, FIX tag 746, with wire type <c>Amt</c>.
	/// </summary>
	OpenInterest                         = 746,
	/// <summary>
	/// Identifies ExerciseMethod, FIX tag 747, with wire type <c>char</c>.
	/// </summary>
	ExerciseMethod                       = 747,
	/// <summary>
	/// Identifies TotNumTradeReports, FIX tag 748, with wire type <c>int</c>.
	/// </summary>
	TotNumTradeReports                   = 748,
	/// <summary>
	/// Identifies TradeRequestResult, FIX tag 749, with wire type <c>int</c>.
	/// </summary>
	TradeRequestResult                   = 749,
	/// <summary>
	/// Identifies TradeRequestStatus, FIX tag 750, with wire type <c>int</c>.
	/// </summary>
	TradeRequestStatus                   = 750,
	/// <summary>
	/// Identifies TradeReportRejectReason, FIX tag 751, with wire type <c>int</c>.
	/// </summary>
	TradeReportRejectReason              = 751,
	/// <summary>
	/// Identifies SideMultiLegReportingType, FIX tag 752, with wire type <c>int</c>.
	/// </summary>
	SideMultiLegReportingType            = 752,
	/// <summary>
	/// Identifies NoPosAmt, FIX tag 753, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoPosAmt                             = 753,
	/// <summary>
	/// Identifies AutoAcceptIndicator, FIX tag 754, with wire type <c>Boolean</c>.
	/// </summary>
	AutoAcceptIndicator                  = 754,
	/// <summary>
	/// Identifies AllocReportID, FIX tag 755, with wire type <c>String</c>.
	/// </summary>
	AllocReportID                        = 755,
	/// <summary>
	/// Identifies NoNested2PartyIDs, FIX tag 756, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNested2PartyIDs                    = 756,
	/// <summary>
	/// Identifies Nested2PartyID, FIX tag 757, with wire type <c>String</c>.
	/// </summary>
	Nested2PartyID                       = 757,
	/// <summary>
	/// Identifies Nested2PartyIDSource, FIX tag 758, with wire type <c>char</c>.
	/// </summary>
	Nested2PartyIDSource                 = 758,
	/// <summary>
	/// Identifies Nested2PartyRole, FIX tag 759, with wire type <c>int</c>.
	/// </summary>
	Nested2PartyRole                     = 759,
	/// <summary>
	/// Identifies Nested2PartySubID, FIX tag 760, with wire type <c>String</c>.
	/// </summary>
	Nested2PartySubID                    = 760,
	/// <summary>
	/// Identifies BenchmarkSecurityIDSource, FIX tag 761, with wire type <c>String</c>.
	/// </summary>
	BenchmarkSecurityIDSource            = 761,
	/// <summary>
	/// Identifies SecuritySubType, FIX tag 762, with wire type <c>String</c>.
	/// </summary>
	SecuritySubType                      = 762,
	/// <summary>
	/// Identifies UnderlyingSecuritySubType, FIX tag 763, with wire type <c>String</c>.
	/// </summary>
	UnderlyingSecuritySubType            = 763,
	/// <summary>
	/// Identifies LegSecuritySubType, FIX tag 764, with wire type <c>String</c>.
	/// </summary>
	LegSecuritySubType                   = 764,
	/// <summary>
	/// Identifies AllowableOneSidednessPct, FIX tag 765, with wire type <c>Percentage</c>.
	/// </summary>
	AllowableOneSidednessPct             = 765,
	/// <summary>
	/// Identifies AllowableOneSidednessValue, FIX tag 766, with wire type <c>Amt</c>.
	/// </summary>
	AllowableOneSidednessValue           = 766,
	/// <summary>
	/// Identifies AllowableOneSidednessCurr, FIX tag 767, with wire type <c>Currency</c>.
	/// </summary>
	AllowableOneSidednessCurr            = 767,
	/// <summary>
	/// Identifies NoTrdRegTimestamps, FIX tag 768, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoTrdRegTimestamps                   = 768,
	/// <summary>
	/// Identifies TrdRegTimestamp, FIX tag 769, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	TrdRegTimestamp                      = 769,
	/// <summary>
	/// Identifies TrdRegTimestampType, FIX tag 770, with wire type <c>int</c>.
	/// </summary>
	TrdRegTimestampType                  = 770,
	/// <summary>
	/// Identifies TrdRegTimestampOrigin, FIX tag 771, with wire type <c>String</c>.
	/// </summary>
	TrdRegTimestampOrigin                = 771,
	/// <summary>
	/// Identifies ConfirmRefID, FIX tag 772, with wire type <c>String</c>.
	/// </summary>
	ConfirmRefID                         = 772,
	/// <summary>
	/// Identifies ConfirmType, FIX tag 773, with wire type <c>int</c>.
	/// </summary>
	ConfirmType                          = 773,
	/// <summary>
	/// Identifies ConfirmRejReason, FIX tag 774, with wire type <c>int</c>.
	/// </summary>
	ConfirmRejReason                     = 774,
	/// <summary>
	/// Identifies BookingType, FIX tag 775, with wire type <c>int</c>.
	/// </summary>
	BookingType                          = 775,
	/// <summary>
	/// Identifies IndividualAllocRejCode, FIX tag 776, with wire type <c>int</c>.
	/// </summary>
	IndividualAllocRejCode               = 776,
	/// <summary>
	/// Identifies SettlInstMsgID, FIX tag 777, with wire type <c>String</c>.
	/// </summary>
	SettlInstMsgID                       = 777,
	/// <summary>
	/// Identifies NoSettlInst, FIX tag 778, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSettlInst                          = 778,
	/// <summary>
	/// Identifies LastUpdateTime, FIX tag 779, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	LastUpdateTime                       = 779,
	/// <summary>
	/// Identifies AllocSettlInstType, FIX tag 780, with wire type <c>int</c>.
	/// </summary>
	AllocSettlInstType                   = 780,
	/// <summary>
	/// Identifies NoSettlPartyIDs, FIX tag 781, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSettlPartyIDs                      = 781,
	/// <summary>
	/// Identifies SettlPartyID, FIX tag 782, with wire type <c>String</c>.
	/// </summary>
	SettlPartyID                         = 782,
	/// <summary>
	/// Identifies SettlPartyIDSource, FIX tag 783, with wire type <c>char</c>.
	/// </summary>
	SettlPartyIDSource                   = 783,
	/// <summary>
	/// Identifies SettlPartyRole, FIX tag 784, with wire type <c>int</c>.
	/// </summary>
	SettlPartyRole                       = 784,
	/// <summary>
	/// Identifies SettlPartySubID, FIX tag 785, with wire type <c>String</c>.
	/// </summary>
	SettlPartySubID                      = 785,
	/// <summary>
	/// Identifies SettlPartySubIDType, FIX tag 786, with wire type <c>int</c>.
	/// </summary>
	SettlPartySubIDType                  = 786,
	/// <summary>
	/// Identifies DlvyInstType, FIX tag 787, with wire type <c>char</c>.
	/// </summary>
	DlvyInstType                         = 787,
	/// <summary>
	/// Identifies TerminationType, FIX tag 788, with wire type <c>int</c>.
	/// </summary>
	TerminationType                      = 788,
	/// <summary>
	/// Identifies NextExpectedMsgSeqNum, FIX tag 789, with wire type <c>SeqNum</c>.
	/// </summary>
	NextExpectedMsgSeqNum                = 789,
	/// <summary>
	/// Identifies OrdStatusReqID, FIX tag 790, with wire type <c>String</c>.
	/// </summary>
	OrdStatusReqID                       = 790,
	/// <summary>
	/// Identifies SettlInstReqID, FIX tag 791, with wire type <c>String</c>.
	/// </summary>
	SettlInstReqID                       = 791,
	/// <summary>
	/// Identifies SettlInstReqRejCode, FIX tag 792, with wire type <c>int</c>.
	/// </summary>
	SettlInstReqRejCode                  = 792,
	/// <summary>
	/// Identifies SecondaryAllocID, FIX tag 793, with wire type <c>String</c>.
	/// </summary>
	SecondaryAllocID                     = 793,
	/// <summary>
	/// Identifies AllocReportType, FIX tag 794, with wire type <c>int</c>.
	/// </summary>
	AllocReportType                      = 794,
	/// <summary>
	/// Identifies AllocReportRefID, FIX tag 795, with wire type <c>String</c>.
	/// </summary>
	AllocReportRefID                     = 795,
	/// <summary>
	/// Identifies AllocCancReplaceReason, FIX tag 796, with wire type <c>int</c>.
	/// </summary>
	AllocCancReplaceReason               = 796,
	/// <summary>
	/// Identifies CopyMsgIndicator, FIX tag 797, with wire type <c>Boolean</c>.
	/// </summary>
	CopyMsgIndicator                     = 797,
	/// <summary>
	/// Identifies AllocAccountType, FIX tag 798, with wire type <c>int</c>.
	/// </summary>
	AllocAccountType                     = 798,
	/// <summary>
	/// Identifies OrderAvgPx, FIX tag 799, with wire type <c>Price</c>.
	/// </summary>
	OrderAvgPx                           = 799,
	/// <summary>
	/// Identifies OrderBookingQty, FIX tag 800, with wire type <c>Qty</c>.
	/// </summary>
	OrderBookingQty                      = 800,
	/// <summary>
	/// Identifies NoSettlPartySubIDs, FIX tag 801, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoSettlPartySubIDs                   = 801,
	/// <summary>
	/// Identifies NoPartySubIDs, FIX tag 802, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoPartySubIDs                        = 802,
	/// <summary>
	/// Identifies PartySubIDType, FIX tag 803, with wire type <c>int</c>.
	/// </summary>
	PartySubIDType                       = 803,
	/// <summary>
	/// Identifies NoNestedPartySubIDs, FIX tag 804, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNestedPartySubIDs                  = 804,
	/// <summary>
	/// Identifies NestedPartySubIDType, FIX tag 805, with wire type <c>int</c>.
	/// </summary>
	NestedPartySubIDType                 = 805,
	/// <summary>
	/// Identifies NoNested2PartySubIDs, FIX tag 806, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNested2PartySubIDs                 = 806,
	/// <summary>
	/// Identifies Nested2PartySubIDType, FIX tag 807, with wire type <c>int</c>.
	/// </summary>
	Nested2PartySubIDType                = 807,
	/// <summary>
	/// Identifies AllocIntermedReqType, FIX tag 808, with wire type <c>int</c>.
	/// </summary>
	AllocIntermedReqType                 = 808,
	/// <summary>
	/// Identifies UnderlyingPx, FIX tag 810, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingPx                         = 810,
	/// <summary>
	/// Identifies PriceDelta, FIX tag 811, with wire type <c>float</c>.
	/// </summary>
	PriceDelta                           = 811,
	/// <summary>
	/// Identifies ApplQueueMax, FIX tag 812, with wire type <c>int</c>.
	/// </summary>
	ApplQueueMax                         = 812,
	/// <summary>
	/// Identifies ApplQueueDepth, FIX tag 813, with wire type <c>int</c>.
	/// </summary>
	ApplQueueDepth                       = 813,
	/// <summary>
	/// Identifies ApplQueueResolution, FIX tag 814, with wire type <c>int</c>.
	/// </summary>
	ApplQueueResolution                  = 814,
	/// <summary>
	/// Identifies ApplQueueAction, FIX tag 815, with wire type <c>int</c>.
	/// </summary>
	ApplQueueAction                      = 815,
	/// <summary>
	/// Identifies NoAltMDSource, FIX tag 816, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoAltMDSource                        = 816,
	/// <summary>
	/// Identifies AltMDSourceID, FIX tag 817, with wire type <c>String</c>.
	/// </summary>
	AltMDSourceID                        = 817,
	/// <summary>
	/// Identifies SecondaryTradeReportID, FIX tag 818, with wire type <c>String</c>.
	/// </summary>
	SecondaryTradeReportID               = 818,
	/// <summary>
	/// Identifies AvgPxIndicator, FIX tag 819, with wire type <c>int</c>.
	/// </summary>
	AvgPxIndicator                       = 819,
	/// <summary>
	/// Identifies TradeLinkID, FIX tag 820, with wire type <c>String</c>.
	/// </summary>
	TradeLinkID                          = 820,
	/// <summary>
	/// Identifies OrderInputDevice, FIX tag 821, with wire type <c>String</c>.
	/// </summary>
	OrderInputDevice                     = 821,
	/// <summary>
	/// Identifies UnderlyingTradingSessionID, FIX tag 822, with wire type <c>String</c>.
	/// </summary>
	UnderlyingTradingSessionID           = 822,
	/// <summary>
	/// Identifies UnderlyingTradingSessionSubID, FIX tag 823, with wire type <c>String</c>.
	/// </summary>
	UnderlyingTradingSessionSubID        = 823,
	/// <summary>
	/// Identifies TradeLegRefID, FIX tag 824, with wire type <c>String</c>.
	/// </summary>
	TradeLegRefID                        = 824,
	/// <summary>
	/// Identifies ExchangeRule, FIX tag 825, with wire type <c>String</c>.
	/// </summary>
	ExchangeRule                         = 825,
	/// <summary>
	/// Identifies TradeAllocIndicator, FIX tag 826, with wire type <c>int</c>.
	/// </summary>
	TradeAllocIndicator                  = 826,
	/// <summary>
	/// Identifies ExpirationCycle, FIX tag 827, with wire type <c>int</c>.
	/// </summary>
	ExpirationCycle                      = 827,
	/// <summary>
	/// Identifies TrdType, FIX tag 828, with wire type <c>int</c>.
	/// </summary>
	TrdType                              = 828,
	/// <summary>
	/// Identifies TrdSubType, FIX tag 829, with wire type <c>int</c>.
	/// </summary>
	TrdSubType                           = 829,
	/// <summary>
	/// Identifies TransferReason, FIX tag 830, with wire type <c>String</c>.
	/// </summary>
	TransferReason                       = 830,
	/// <summary>
	/// Identifies TotNumAssignmentReports, FIX tag 832, with wire type <c>int</c>.
	/// </summary>
	TotNumAssignmentReports              = 832,
	/// <summary>
	/// Identifies AsgnRptID, FIX tag 833, with wire type <c>String</c>.
	/// </summary>
	AsgnRptID                            = 833,
	/// <summary>
	/// Identifies ThresholdAmount, FIX tag 834, with wire type <c>PriceOffset</c>.
	/// </summary>
	ThresholdAmount                      = 834,
	/// <summary>
	/// Identifies PegMoveType, FIX tag 835, with wire type <c>int</c>.
	/// </summary>
	PegMoveType                          = 835,
	/// <summary>
	/// Identifies PegOffsetType, FIX tag 836, with wire type <c>int</c>.
	/// </summary>
	PegOffsetType                        = 836,
	/// <summary>
	/// Identifies PegLimitType, FIX tag 837, with wire type <c>int</c>.
	/// </summary>
	PegLimitType                         = 837,
	/// <summary>
	/// Identifies PegRoundDirection, FIX tag 838, with wire type <c>int</c>.
	/// </summary>
	PegRoundDirection                    = 838,
	/// <summary>
	/// Identifies PeggedPrice, FIX tag 839, with wire type <c>Price</c>.
	/// </summary>
	PeggedPrice                          = 839,
	/// <summary>
	/// Identifies PegScope, FIX tag 840, with wire type <c>int</c>.
	/// </summary>
	PegScope                             = 840,
	/// <summary>
	/// Identifies DiscretionMoveType, FIX tag 841, with wire type <c>int</c>.
	/// </summary>
	DiscretionMoveType                   = 841,
	/// <summary>
	/// Identifies DiscretionOffsetType, FIX tag 842, with wire type <c>int</c>.
	/// </summary>
	DiscretionOffsetType                 = 842,
	/// <summary>
	/// Identifies DiscretionLimitType, FIX tag 843, with wire type <c>int</c>.
	/// </summary>
	DiscretionLimitType                  = 843,
	/// <summary>
	/// Identifies DiscretionRoundDirection, FIX tag 844, with wire type <c>int</c>.
	/// </summary>
	DiscretionRoundDirection             = 844,
	/// <summary>
	/// Identifies DiscretionPrice, FIX tag 845, with wire type <c>Price</c>.
	/// </summary>
	DiscretionPrice                      = 845,
	/// <summary>
	/// Identifies DiscretionScope, FIX tag 846, with wire type <c>int</c>.
	/// </summary>
	DiscretionScope                      = 846,
	/// <summary>
	/// Identifies TargetStrategy, FIX tag 847, with wire type <c>int</c>.
	/// </summary>
	TargetStrategy                       = 847,
	/// <summary>
	/// Identifies TargetStrategyParameters, FIX tag 848, with wire type <c>String</c>.
	/// </summary>
	TargetStrategyParameters             = 848,
	/// <summary>
	/// Identifies ParticipationRate, FIX tag 849, with wire type <c>Percentage</c>.
	/// </summary>
	ParticipationRate                    = 849,
	/// <summary>
	/// Identifies TargetStrategyPerformance, FIX tag 850, with wire type <c>float</c>.
	/// </summary>
	TargetStrategyPerformance            = 850,
	/// <summary>
	/// Identifies LastLiquidityInd, FIX tag 851, with wire type <c>int</c>.
	/// </summary>
	LastLiquidityInd                     = 851,
	/// <summary>
	/// Identifies PublishTrdIndicator, FIX tag 852, with wire type <c>Boolean</c>.
	/// </summary>
	PublishTrdIndicator                  = 852,
	/// <summary>
	/// Identifies ShortSaleReason, FIX tag 853, with wire type <c>int</c>.
	/// </summary>
	ShortSaleReason                      = 853,
	/// <summary>
	/// Identifies QtyType, FIX tag 854, with wire type <c>int</c>.
	/// </summary>
	QtyType                              = 854,
	/// <summary>
	/// Identifies SecondaryTrdType, FIX tag 855, with wire type <c>int</c>.
	/// </summary>
	SecondaryTrdType                     = 855,
	/// <summary>
	/// Identifies TradeReportType, FIX tag 856, with wire type <c>int</c>.
	/// </summary>
	TradeReportType                      = 856,
	/// <summary>
	/// Identifies AllocNoOrdersType, FIX tag 857, with wire type <c>int</c>.
	/// </summary>
	AllocNoOrdersType                    = 857,
	/// <summary>
	/// Identifies SharedCommission, FIX tag 858, with wire type <c>Amt</c>.
	/// </summary>
	SharedCommission                     = 858,
	/// <summary>
	/// Identifies ConfirmReqID, FIX tag 859, with wire type <c>String</c>.
	/// </summary>
	ConfirmReqID                         = 859,
	/// <summary>
	/// Identifies AvgParPx, FIX tag 860, with wire type <c>Price</c>.
	/// </summary>
	AvgParPx                             = 860,
	/// <summary>
	/// Identifies ReportedPx, FIX tag 861, with wire type <c>Price</c>.
	/// </summary>
	ReportedPx                           = 861,
	/// <summary>
	/// Identifies NoCapacities, FIX tag 862, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoCapacities                         = 862,
	/// <summary>
	/// Identifies OrderCapacityQty, FIX tag 863, with wire type <c>Qty</c>.
	/// </summary>
	OrderCapacityQty                     = 863,
	/// <summary>
	/// Identifies NoEvents, FIX tag 864, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoEvents                             = 864,
	/// <summary>
	/// Identifies EventType, FIX tag 865, with wire type <c>int</c>.
	/// </summary>
	EventType                            = 865,
	/// <summary>
	/// Identifies EventDate, FIX tag 866, with wire type <c>LocalMktDate</c>.
	/// </summary>
	EventDate                            = 866,
	/// <summary>
	/// Identifies EventPx, FIX tag 867, with wire type <c>Price</c>.
	/// </summary>
	EventPx                              = 867,
	/// <summary>
	/// Identifies EventText, FIX tag 868, with wire type <c>String</c>.
	/// </summary>
	EventText                            = 868,
	/// <summary>
	/// Identifies PctAtRisk, FIX tag 869, with wire type <c>Percentage</c>.
	/// </summary>
	PctAtRisk                            = 869,
	/// <summary>
	/// Identifies NoInstrAttrib, FIX tag 870, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoInstrAttrib                        = 870,
	/// <summary>
	/// Identifies InstrAttribType, FIX tag 871, with wire type <c>int</c>.
	/// </summary>
	InstrAttribType                      = 871,
	/// <summary>
	/// Identifies InstrAttribValue, FIX tag 872, with wire type <c>String</c>.
	/// </summary>
	InstrAttribValue                     = 872,
	/// <summary>
	/// Identifies DatedDate, FIX tag 873, with wire type <c>LocalMktDate</c>.
	/// </summary>
	DatedDate                            = 873,
	/// <summary>
	/// Identifies InterestAccrualDate, FIX tag 874, with wire type <c>LocalMktDate</c>.
	/// </summary>
	InterestAccrualDate                  = 874,
	/// <summary>
	/// Identifies CPProgram, FIX tag 875, with wire type <c>int</c>.
	/// </summary>
	CPProgram                            = 875,
	/// <summary>
	/// Identifies CPRegType, FIX tag 876, with wire type <c>String</c>.
	/// </summary>
	CPRegType                            = 876,
	/// <summary>
	/// Identifies UnderlyingCPProgram, FIX tag 877, with wire type <c>String</c>.
	/// </summary>
	UnderlyingCPProgram                  = 877,
	/// <summary>
	/// Identifies UnderlyingCPRegType, FIX tag 878, with wire type <c>String</c>.
	/// </summary>
	UnderlyingCPRegType                  = 878,
	/// <summary>
	/// Identifies UnderlyingQty, FIX tag 879, with wire type <c>Qty</c>.
	/// </summary>
	UnderlyingQty                        = 879,
	/// <summary>
	/// Identifies TrdMatchID, FIX tag 880, with wire type <c>String</c>.
	/// </summary>
	TrdMatchID                           = 880,
	/// <summary>
	/// Identifies SecondaryTradeReportRefID, FIX tag 881, with wire type <c>String</c>.
	/// </summary>
	SecondaryTradeReportRefID            = 881,
	/// <summary>
	/// Identifies UnderlyingDirtyPrice, FIX tag 882, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingDirtyPrice                 = 882,
	/// <summary>
	/// Identifies UnderlyingEndPrice, FIX tag 883, with wire type <c>Price</c>.
	/// </summary>
	UnderlyingEndPrice                   = 883,
	/// <summary>
	/// Identifies UnderlyingStartValue, FIX tag 884, with wire type <c>Amt</c>.
	/// </summary>
	UnderlyingStartValue                 = 884,
	/// <summary>
	/// Identifies UnderlyingCurrentValue, FIX tag 885, with wire type <c>Amt</c>.
	/// </summary>
	UnderlyingCurrentValue               = 885,
	/// <summary>
	/// Identifies UnderlyingEndValue, FIX tag 886, with wire type <c>Amt</c>.
	/// </summary>
	UnderlyingEndValue                   = 886,
	/// <summary>
	/// Identifies NoUnderlyingStips, FIX tag 887, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoUnderlyingStips                    = 887,
	/// <summary>
	/// Identifies UnderlyingStipType, FIX tag 888, with wire type <c>String</c>.
	/// </summary>
	UnderlyingStipType                   = 888,
	/// <summary>
	/// Identifies UnderlyingStipValue, FIX tag 889, with wire type <c>String</c>.
	/// </summary>
	UnderlyingStipValue                  = 889,
	/// <summary>
	/// Identifies MaturityNetMoney, FIX tag 890, with wire type <c>Amt</c>.
	/// </summary>
	MaturityNetMoney                     = 890,
	/// <summary>
	/// Identifies MiscFeeBasis, FIX tag 891, with wire type <c>int</c>.
	/// </summary>
	MiscFeeBasis                         = 891,
	/// <summary>
	/// Identifies TotNoAllocs, FIX tag 892, with wire type <c>int</c>.
	/// </summary>
	TotNoAllocs                          = 892,
	/// <summary>
	/// Identifies LastFragment, FIX tag 893, with wire type <c>Boolean</c>.
	/// </summary>
	LastFragment                         = 893,
	/// <summary>
	/// Identifies CollReqID, FIX tag 894, with wire type <c>String</c>.
	/// </summary>
	CollReqID                            = 894,
	/// <summary>
	/// Identifies CollAsgnReason, FIX tag 895, with wire type <c>int</c>.
	/// </summary>
	CollAsgnReason                       = 895,
	/// <summary>
	/// Identifies CollInquiryQualifier, FIX tag 896, with wire type <c>int</c>.
	/// </summary>
	CollInquiryQualifier                 = 896,
	/// <summary>
	/// Identifies NoTrades, FIX tag 897, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoTrades                             = 897,
	/// <summary>
	/// Identifies MarginRatio, FIX tag 898, with wire type <c>Percentage</c>.
	/// </summary>
	MarginRatio                          = 898,
	/// <summary>
	/// Identifies MarginExcess, FIX tag 899, with wire type <c>Amt</c>.
	/// </summary>
	MarginExcess                         = 899,
	/// <summary>
	/// Identifies TotalNetValue, FIX tag 900, with wire type <c>Amt</c>.
	/// </summary>
	TotalNetValue                        = 900,
	/// <summary>
	/// Identifies CashOutstanding, FIX tag 901, with wire type <c>Amt</c>.
	/// </summary>
	CashOutstanding                      = 901,
	/// <summary>
	/// Identifies CollAsgnID, FIX tag 902, with wire type <c>String</c>.
	/// </summary>
	CollAsgnID                           = 902,
	/// <summary>
	/// Identifies CollAsgnTransType, FIX tag 903, with wire type <c>int</c>.
	/// </summary>
	CollAsgnTransType                    = 903,
	/// <summary>
	/// Identifies CollRespID, FIX tag 904, with wire type <c>String</c>.
	/// </summary>
	CollRespID                           = 904,
	/// <summary>
	/// Identifies CollAsgnRespType, FIX tag 905, with wire type <c>int</c>.
	/// </summary>
	CollAsgnRespType                     = 905,
	/// <summary>
	/// Identifies CollAsgnRejectReason, FIX tag 906, with wire type <c>int</c>.
	/// </summary>
	CollAsgnRejectReason                 = 906,
	/// <summary>
	/// Identifies CollAsgnRefID, FIX tag 907, with wire type <c>String</c>.
	/// </summary>
	CollAsgnRefID                        = 907,
	/// <summary>
	/// Identifies CollRptID, FIX tag 908, with wire type <c>String</c>.
	/// </summary>
	CollRptID                            = 908,
	/// <summary>
	/// Identifies CollInquiryID, FIX tag 909, with wire type <c>String</c>.
	/// </summary>
	CollInquiryID                        = 909,
	/// <summary>
	/// Identifies CollStatus, FIX tag 910, with wire type <c>int</c>.
	/// </summary>
	CollStatus                           = 910,
	/// <summary>
	/// Identifies TotNumReports, FIX tag 911, with wire type <c>int</c>.
	/// </summary>
	TotNumReports                        = 911,
	/// <summary>
	/// Identifies LastRptRequested, FIX tag 912, with wire type <c>Boolean</c>.
	/// </summary>
	LastRptRequested                     = 912,
	/// <summary>
	/// Identifies AgreementDesc, FIX tag 913, with wire type <c>String</c>.
	/// </summary>
	AgreementDesc                        = 913,
	/// <summary>
	/// Identifies AgreementID, FIX tag 914, with wire type <c>String</c>.
	/// </summary>
	AgreementID                          = 914,
	/// <summary>
	/// Identifies AgreementDate, FIX tag 915, with wire type <c>LocalMktDate</c>.
	/// </summary>
	AgreementDate                        = 915,
	/// <summary>
	/// Identifies StartDate, FIX tag 916, with wire type <c>LocalMktDate</c>.
	/// </summary>
	StartDate                            = 916,
	/// <summary>
	/// Identifies EndDate, FIX tag 917, with wire type <c>LocalMktDate</c>.
	/// </summary>
	EndDate                              = 917,
	/// <summary>
	/// Identifies AgreementCurrency, FIX tag 918, with wire type <c>Currency</c>.
	/// </summary>
	AgreementCurrency                    = 918,
	/// <summary>
	/// Identifies DeliveryType, FIX tag 919, with wire type <c>int</c>.
	/// </summary>
	DeliveryType                         = 919,
	/// <summary>
	/// Identifies EndAccruedInterestAmt, FIX tag 920, with wire type <c>Amt</c>.
	/// </summary>
	EndAccruedInterestAmt                = 920,
	/// <summary>
	/// Identifies StartCash, FIX tag 921, with wire type <c>Amt</c>.
	/// </summary>
	StartCash                            = 921,
	/// <summary>
	/// Identifies EndCash, FIX tag 922, with wire type <c>Amt</c>.
	/// </summary>
	EndCash                              = 922,
	/// <summary>
	/// Identifies UserRequestID, FIX tag 923, with wire type <c>String</c>.
	/// </summary>
	UserRequestID                        = 923,
	/// <summary>
	/// Identifies UserRequestType, FIX tag 924, with wire type <c>int</c>.
	/// </summary>
	UserRequestType                      = 924,
	/// <summary>
	/// Identifies NewPassword, FIX tag 925, with wire type <c>String</c>.
	/// </summary>
	NewPassword                          = 925,
	/// <summary>
	/// Identifies UserStatus, FIX tag 926, with wire type <c>int</c>.
	/// </summary>
	UserStatus                           = 926,
	/// <summary>
	/// Identifies UserStatusText, FIX tag 927, with wire type <c>String</c>.
	/// </summary>
	UserStatusText                       = 927,
	/// <summary>
	/// Identifies StatusValue, FIX tag 928, with wire type <c>int</c>.
	/// </summary>
	StatusValue                          = 928,
	/// <summary>
	/// Identifies StatusText, FIX tag 929, with wire type <c>String</c>.
	/// </summary>
	StatusText                           = 929,
	/// <summary>
	/// Identifies RefCompID, FIX tag 930, with wire type <c>String</c>.
	/// </summary>
	RefCompID                            = 930,
	/// <summary>
	/// Identifies RefSubID, FIX tag 931, with wire type <c>String</c>.
	/// </summary>
	RefSubID                             = 931,
	/// <summary>
	/// Identifies NetworkResponseID, FIX tag 932, with wire type <c>String</c>.
	/// </summary>
	NetworkResponseID                    = 932,
	/// <summary>
	/// Identifies NetworkRequestID, FIX tag 933, with wire type <c>String</c>.
	/// </summary>
	NetworkRequestID                     = 933,
	/// <summary>
	/// Identifies LastNetworkResponseID, FIX tag 934, with wire type <c>String</c>.
	/// </summary>
	LastNetworkResponseID                = 934,
	/// <summary>
	/// Identifies NetworkRequestType, FIX tag 935, with wire type <c>int</c>.
	/// </summary>
	NetworkRequestType                   = 935,
	/// <summary>
	/// Identifies NoCompIDs, FIX tag 936, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoCompIDs                            = 936,
	/// <summary>
	/// Identifies NetworkStatusResponseType, FIX tag 937, with wire type <c>int</c>.
	/// </summary>
	NetworkStatusResponseType            = 937,
	/// <summary>
	/// Identifies NoCollInquiryQualifier, FIX tag 938, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoCollInquiryQualifier               = 938,
	/// <summary>
	/// Identifies TrdRptStatus, FIX tag 939, with wire type <c>int</c>.
	/// </summary>
	TrdRptStatus                         = 939,
	/// <summary>
	/// Identifies AffirmStatus, FIX tag 940, with wire type <c>int</c>.
	/// </summary>
	AffirmStatus                         = 940,
	/// <summary>
	/// Identifies UnderlyingStrikeCurrency, FIX tag 941, with wire type <c>Currency</c>.
	/// </summary>
	UnderlyingStrikeCurrency             = 941,
	/// <summary>
	/// Identifies LegStrikeCurrency, FIX tag 942, with wire type <c>Currency</c>.
	/// </summary>
	LegStrikeCurrency                    = 942,
	/// <summary>
	/// Identifies TimeBracket, FIX tag 943, with wire type <c>String</c>.
	/// </summary>
	TimeBracket                          = 943,
	/// <summary>
	/// Identifies CollAction, FIX tag 944, with wire type <c>int</c>.
	/// </summary>
	CollAction                           = 944,
	/// <summary>
	/// Identifies CollInquiryStatus, FIX tag 945, with wire type <c>int</c>.
	/// </summary>
	CollInquiryStatus                    = 945,
	/// <summary>
	/// Identifies CollInquiryResult, FIX tag 946, with wire type <c>int</c>.
	/// </summary>
	CollInquiryResult                    = 946,
	/// <summary>
	/// Identifies StrikeCurrency, FIX tag 947, with wire type <c>Currency</c>.
	/// </summary>
	StrikeCurrency                       = 947,
	/// <summary>
	/// Identifies NoNested3PartyIDs, FIX tag 948, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNested3PartyIDs                    = 948,
	/// <summary>
	/// Identifies Nested3PartyID, FIX tag 949, with wire type <c>String</c>.
	/// </summary>
	Nested3PartyID                       = 949,
	/// <summary>
	/// Identifies Nested3PartyIDSource, FIX tag 950, with wire type <c>char</c>.
	/// </summary>
	Nested3PartyIDSource                 = 950,
	/// <summary>
	/// Identifies Nested3PartyRole, FIX tag 951, with wire type <c>int</c>.
	/// </summary>
	Nested3PartyRole                     = 951,
	/// <summary>
	/// Identifies NoNested3PartySubIDs, FIX tag 952, with wire type <c>NumInGroup</c>.
	/// </summary>
	NoNested3PartySubIDs                 = 952,
	/// <summary>
	/// Identifies Nested3PartySubID, FIX tag 953, with wire type <c>String</c>.
	/// </summary>
	Nested3PartySubID                    = 953,
	/// <summary>
	/// Identifies Nested3PartySubIDType, FIX tag 954, with wire type <c>int</c>.
	/// </summary>
	Nested3PartySubIDType                = 954,
	/// <summary>
	/// Identifies LegContractSettlMonth, FIX tag 955, with wire type <c>MonthYear</c>.
	/// </summary>
	LegContractSettlMonth                = 955,
	/// <summary>
	/// Identifies LegInterestAccrualDate, FIX tag 956, with wire type <c>LocalMktDate</c>.
	/// </summary>
	LegInterestAccrualDate               = 956,
}
