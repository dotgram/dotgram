using System;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.cs from the FIX 4.4 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>The check of every field of FIX 4.4 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator44
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 4.4 Account, tag 1, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Account                              { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 4.4 AdvId, tag 2, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AdvId                                { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 4.4 AdvRefID, tag 3, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AdvRefID                             { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 4.4 AdvSide, tag 4, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> AdvSide                              { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.4 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AdvTransType                         { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.4 AvgPx, tag 6, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AvgPx                                { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 4.4 BeginSeqNo, tag 7, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BeginSeqNo                           { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 4.4 BeginString, tag 8, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BeginString                          { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 4.4 BodyLength, tag 9, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BodyLength                           { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 4.4 CheckSum, tag 10, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CheckSum                             { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 4.4 ClOrdID, tag 11, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ClOrdID                              { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 4.4 Commission, tag 12, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Commission                           { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 4.4 CommType, tag 13, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> CommType                             { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.4 CumQty, tag 14, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CumQty                               { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 4.4 Currency, tag 15, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Currency                             { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 4.4 EndSeqNo, tag 16, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EndSeqNo                             { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 4.4 ExecID, tag 17, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ExecID                               { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 4.4 ExecInst, tag 18, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  ExecInst                             { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.4 ExecRefID, tag 19, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ExecRefID                            { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 4.4 HandlInst, tag 21, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> HandlInst                            { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.4 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityIDSource                     { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 4.4 IOIid, tag 23, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      IOIid                                { get; set; } = ValidateIOIid;

	/// <summary>Holds a FIX 4.4 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> IOIQltyInd                           { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.4 IOIRefID, tag 26, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      IOIRefID                             { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 4.4 IOIQty, tag 27, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      IOIQty                               { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 4.4 IOITransType, tag 28, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> IOITransType                         { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.4 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> LastCapacity                         { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.4 LastMkt, tag 30, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LastMkt                              { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 4.4 LastPx, tag 31, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastPx                               { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 4.4 LastQty, tag 32, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastQty                              { get; set; } = ValidateLastQty;

	/// <summary>Holds a FIX 4.4 LinesOfText, tag 33, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LinesOfText                          { get; set; } = ValidateLinesOfText;

	/// <summary>Holds a FIX 4.4 MsgSeqNum, tag 34, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MsgSeqNum                            { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 4.4 MsgType, tag 35, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MsgType                              { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.4 NewSeqNo, tag 36, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NewSeqNo                             { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 4.4 OrderID, tag 37, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrderID                              { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 4.4 OrderQty, tag 38, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderQty                             { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 4.4 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> OrdStatus                            { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.4 OrdType, tag 40, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> OrdType                              { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.4 OrigClOrdID, tag 41, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrigClOrdID                          { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 4.4 OrigTime, tag 42, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> OrigTime                             { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 4.4 PossDupFlag, tag 43, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   PossDupFlag                          { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 4.4 Price, tag 44, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Price                                { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 4.4 RefSeqNum, tag 45, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RefSeqNum                            { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 4.4 SecurityID, tag 48, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityID                           { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 4.4 SenderCompID, tag 49, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SenderCompID                         { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 4.4 SenderSubID, tag 50, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SenderSubID                          { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 4.4 SendingTime, tag 52, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> SendingTime                          { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 4.4 Quantity, tag 53, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Quantity                             { get; set; } = ValidateQuantity;

	/// <summary>Holds a FIX 4.4 Side, tag 54, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> Side                                 { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.4 Symbol, tag 55, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Symbol                               { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 4.4 TargetCompID, tag 56, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TargetCompID                         { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 4.4 TargetSubID, tag 57, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TargetSubID                          { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 4.4 Text, tag 58, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Text                                 { get; set; } = ValidateText;

	/// <summary>Holds a FIX 4.4 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> TimeInForce                          { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.4 TransactTime, tag 60, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TransactTime                         { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 4.4 Urgency, tag 61, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> Urgency                              { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.4 ValidUntilTime, tag 62, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> ValidUntilTime                       { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 4.4 SettlType, tag 63, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlType                            { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 4.4 SettlDate, tag 64, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      SettlDate                            { get; set; } = ValidateSettlDate;

	/// <summary>Holds a FIX 4.4 SymbolSfx, tag 65, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SymbolSfx                            { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 4.4 ListID, tag 66, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ListID                               { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 4.4 ListSeqNo, tag 67, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ListSeqNo                            { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 4.4 TotNoOrders, tag 68, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoOrders                          { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 4.4 ListExecInst, tag 69, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ListExecInst                         { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 4.4 AllocID, tag 70, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocID                              { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 4.4 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> AllocTransType                       { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.4 RefAllocID, tag 72, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RefAllocID                           { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 4.4 NoOrders, tag 73, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoOrders                             { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 4.4 AvgPxPrecision, tag 74, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AvgPxPrecision                       { get; set; } = ValidateAvgPxPrecision;

	/// <summary>Holds a FIX 4.4 TradeDate, tag 75, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      TradeDate                            { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 4.4 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> PositionEffect                       { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 4.4 NoAllocs, tag 78, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoAllocs                             { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 4.4 AllocAccount, tag 79, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocAccount                         { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 4.4 AllocQty, tag 80, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocQty                             { get; set; } = ValidateAllocQty;

	/// <summary>Holds a FIX 4.4 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> ProcessCode                          { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.4 NoRpts, tag 82, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoRpts                               { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 4.4 RptSeq, tag 83, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RptSeq                               { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 4.4 CxlQty, tag 84, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CxlQty                               { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 4.4 NoDlvyInst, tag 85, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoDlvyInst                           { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 4.4 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocStatus                          { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.4 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocRejCode                         { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.4 Signature, tag 89, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      Signature                            { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 4.4 SecureDataLen, tag 90, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecureDataLen                        { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 4.4 SecureData, tag 91, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      SecureData                           { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 4.4 SignatureLength, tag 93, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SignatureLength                      { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 4.4 EmailType, tag 94, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> EmailType                            { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.4 RawDataLength, tag 95, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RawDataLength                        { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 4.4 RawData, tag 96, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      RawData                              { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 4.4 PossResend, tag 97, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   PossResend                           { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 4.4 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncryptMethod                        { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.4 StopPx, tag 99, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   StopPx                               { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 4.4 ExDestination, tag 100, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ExDestination                        { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 4.4 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CxlRejReason                         { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.4 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   OrdRejReason                         { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.4 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> IOIQualifier                         { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.4 Issuer, tag 106, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Issuer                               { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 4.4 SecurityDesc, tag 107, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityDesc                         { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 4.4 HeartBtInt, tag 108, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   HeartBtInt                           { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 4.4 MinQty, tag 110, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MinQty                               { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 4.4 MaxFloor, tag 111, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MaxFloor                             { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 4.4 TestReqID, tag 112, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TestReqID                            { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 4.4 ReportToExch, tag 113, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ReportToExch                         { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 4.4 LocateReqd, tag 114, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   LocateReqd                           { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 4.4 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OnBehalfOfCompID                     { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OnBehalfOfSubID                      { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 4.4 QuoteID, tag 117, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteID                              { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 4.4 NetMoney, tag 118, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   NetMoney                             { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrAmt, tag 119, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SettlCurrAmt                         { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 SettlCurrency, tag 120, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlCurrency                        { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 4.4 ForexReq, tag 121, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ForexReq                             { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 4.4 OrigSendingTime, tag 122, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> OrigSendingTime                      { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 4.4 GapFillFlag, tag 123, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   GapFillFlag                          { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 4.4 NoExecs, tag 124, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoExecs                              { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 4.4 ExpireTime, tag 126, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> ExpireTime                           { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 4.4 DKReason, tag 127, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> DKReason                             { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.4 DeliverToCompID, tag 128, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      DeliverToCompID                      { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 4.4 DeliverToSubID, tag 129, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      DeliverToSubID                       { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 4.4 IOINaturalFlag, tag 130, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   IOINaturalFlag                       { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 4.4 QuoteReqID, tag 131, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteReqID                           { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 4.4 BidPx, tag 132, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidPx                                { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 4.4 OfferPx, tag 133, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferPx                              { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 4.4 BidSize, tag 134, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidSize                              { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 4.4 OfferSize, tag 135, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferSize                            { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 4.4 NoMiscFees, tag 136, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoMiscFees                           { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 4.4 MiscFeeAmt, tag 137, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MiscFeeAmt                           { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 4.4 MiscFeeCurr, tag 138, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MiscFeeCurr                          { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 4.4 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MiscFeeType                          { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.4 PrevClosePx, tag 140, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PrevClosePx                          { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 4.4 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ResetSeqNumFlag                      { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 4.4 SenderLocationID, tag 142, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SenderLocationID                     { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 4.4 TargetLocationID, tag 143, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TargetLocationID                     { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OnBehalfOfLocationID                 { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 4.4 DeliverToLocationID, tag 145, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      DeliverToLocationID                  { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 4.4 NoRelatedSym, tag 146, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoRelatedSym                         { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 4.4 Subject, tag 147, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Subject                              { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 4.4 Headline, tag 148, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Headline                             { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 4.4 URLLink, tag 149, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      URLLink                              { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 4.4 ExecType, tag 150, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> ExecType                             { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.4 LeavesQty, tag 151, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LeavesQty                            { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 4.4 CashOrderQty, tag 152, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CashOrderQty                         { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 4.4 AllocAvgPx, tag 153, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocAvgPx                           { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 4.4 AllocNetMoney, tag 154, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocNetMoney                        { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRate, tag 155, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SettlCurrFxRate                      { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlCurrFxRateCalc                  { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.4 NumDaysInterest, tag 157, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NumDaysInterest                      { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 4.4 AccruedInterestRate, tag 158, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AccruedInterestRate                  { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 4.4 AccruedInterestAmt, tag 159, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AccruedInterestAmt                   { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlInstMode                        { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.4 AllocText, tag 161, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocText                            { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 4.4 SettlInstID, tag 162, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlInstID                          { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 4.4 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlInstTransType                   { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.4 EmailThreadID, tag 164, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      EmailThreadID                        { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 4.4 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlInstSource                      { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.4 SecurityType, tag 167, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityType                         { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.4 EffectiveTime, tag 168, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> EffectiveTime                        { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 4.4 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   StandInstDbType                      { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.4 StandInstDbName, tag 170, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StandInstDbName                      { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 4.4 StandInstDbID, tag 171, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StandInstDbID                        { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 4.4 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SettlDeliveryType                    { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.4 BidSpotRate, tag 188, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidSpotRate                          { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 4.4 BidForwardPoints, tag 189, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidForwardPoints                     { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 4.4 OfferSpotRate, tag 190, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferSpotRate                        { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints, tag 191, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferForwardPoints                   { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 4.4 OrderQty2, tag 192, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderQty2                            { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 4.4 SettlDate2, tag 193, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      SettlDate2                           { get; set; } = ValidateSettlDate2;

	/// <summary>Holds a FIX 4.4 LastSpotRate, tag 194, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastSpotRate                         { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 4.4 LastForwardPoints, tag 195, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastForwardPoints                    { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 4.4 AllocLinkID, tag 196, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocLinkID                          { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 4.4 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocLinkType                        { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.4 SecondaryOrderID, tag 198, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryOrderID                     { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 NoIOIQualifiers, tag 199, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoIOIQualifiers                      { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 4.4 MaturityMonthYear, tag 200, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.MonthYear> MaturityMonthYear                    { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PutOrCall                            { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.4 StrikePrice, tag 202, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   StrikePrice                          { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 4.4 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CoveredOrUncovered                   { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 OptAttribute, tag 206, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> OptAttribute                         { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 4.4 SecurityExchange, tag 207, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityExchange                     { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 4.4 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   NotifyBrokerOfCredit                 { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 4.4 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocHandlInst                       { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.4 MaxShow, tag 210, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MaxShow                              { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 4.4 PegOffsetValue, tag 211, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PegOffsetValue                       { get; set; } = ValidatePegOffsetValue;

	/// <summary>Holds a FIX 4.4 XmlDataLen, tag 212, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   XmlDataLen                           { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 4.4 XmlData, tag 213, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      XmlData                              { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 4.4 SettlInstRefID, tag 214, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlInstRefID                       { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 4.4 NoRoutingIDs, tag 215, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoRoutingIDs                         { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 4.4 RoutingType, tag 216, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RoutingType                          { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.4 RoutingID, tag 217, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RoutingID                            { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 4.4 Spread, tag 218, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Spread                               { get; set; } = ValidateSpread;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveCurrency, tag 220, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BenchmarkCurveCurrency               { get; set; } = ValidateBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveName, tag 221, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BenchmarkCurveName                   { get; set; } = ValidateBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 BenchmarkCurvePoint, tag 222, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BenchmarkCurvePoint                  { get; set; } = ValidateBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 CouponRate, tag 223, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CouponRate                           { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 4.4 CouponPaymentDate, tag 224, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      CouponPaymentDate                    { get; set; } = ValidateCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 IssueDate, tag 225, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      IssueDate                            { get; set; } = ValidateIssueDate;

	/// <summary>Holds a FIX 4.4 RepurchaseTerm, tag 226, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RepurchaseTerm                       { get; set; } = ValidateRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 RepurchaseRate, tag 227, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   RepurchaseRate                       { get; set; } = ValidateRepurchaseRate;

	/// <summary>Holds a FIX 4.4 Factor, tag 228, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Factor                               { get; set; } = ValidateFactor;

	/// <summary>Holds a FIX 4.4 TradeOriginationDate, tag 229, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      TradeOriginationDate                 { get; set; } = ValidateTradeOriginationDate;

	/// <summary>Holds a FIX 4.4 ExDate, tag 230, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      ExDate                               { get; set; } = ValidateExDate;

	/// <summary>Holds a FIX 4.4 ContractMultiplier, tag 231, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ContractMultiplier                   { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 4.4 NoStipulations, tag 232, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoStipulations                       { get; set; } = ValidateNoStipulations;

	/// <summary>Holds a FIX 4.4 StipulationType, tag 233, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StipulationType                      { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 4.4 StipulationValue, tag 234, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StipulationValue                     { get; set; } = ValidateStipulationValue;

	/// <summary>Holds a FIX 4.4 YieldType, tag 235, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      YieldType                            { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 4.4 Yield, tag 236, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Yield                                { get; set; } = ValidateYield;

	/// <summary>Holds a FIX 4.4 TotalTakedown, tag 237, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   TotalTakedown                        { get; set; } = ValidateTotalTakedown;

	/// <summary>Holds a FIX 4.4 Concession, tag 238, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Concession                           { get; set; } = ValidateConcession;

	/// <summary>Holds a FIX 4.4 RepoCollateralSecurityType, tag 239, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RepoCollateralSecurityType           { get; set; } = ValidateRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 RedemptionDate, tag 240, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      RedemptionDate                       { get; set; } = ValidateRedemptionDate;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponPaymentDate, tag 241, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      UnderlyingCouponPaymentDate          { get; set; } = ValidateUnderlyingCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 UnderlyingIssueDate, tag 242, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      UnderlyingIssueDate                  { get; set; } = ValidateUnderlyingIssueDate;

	/// <summary>Holds a FIX 4.4 UnderlyingRepoCollateralSecurityType, tag 243, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingRepoCollateralSecurityType { get; set; } = ValidateUnderlyingRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseTerm, tag 244, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UnderlyingRepurchaseTerm             { get; set; } = ValidateUnderlyingRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseRate, tag 245, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingRepurchaseRate             { get; set; } = ValidateUnderlyingRepurchaseRate;

	/// <summary>Holds a FIX 4.4 UnderlyingFactor, tag 246, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingFactor                     { get; set; } = ValidateUnderlyingFactor;

	/// <summary>Holds a FIX 4.4 UnderlyingRedemptionDate, tag 247, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      UnderlyingRedemptionDate             { get; set; } = ValidateUnderlyingRedemptionDate;

	/// <summary>Holds a FIX 4.4 LegCouponPaymentDate, tag 248, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegCouponPaymentDate                 { get; set; } = ValidateLegCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 LegIssueDate, tag 249, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegIssueDate                         { get; set; } = ValidateLegIssueDate;

	/// <summary>Holds a FIX 4.4 LegRepoCollateralSecurityType, tag 250, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegRepoCollateralSecurityType        { get; set; } = ValidateLegRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 LegRepurchaseTerm, tag 251, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegRepurchaseTerm                    { get; set; } = ValidateLegRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 LegRepurchaseRate, tag 252, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegRepurchaseRate                    { get; set; } = ValidateLegRepurchaseRate;

	/// <summary>Holds a FIX 4.4 LegFactor, tag 253, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegFactor                            { get; set; } = ValidateLegFactor;

	/// <summary>Holds a FIX 4.4 LegRedemptionDate, tag 254, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegRedemptionDate                    { get; set; } = ValidateLegRedemptionDate;

	/// <summary>Holds a FIX 4.4 CreditRating, tag 255, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CreditRating                         { get; set; } = ValidateCreditRating;

	/// <summary>Holds a FIX 4.4 UnderlyingCreditRating, tag 256, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCreditRating               { get; set; } = ValidateUnderlyingCreditRating;

	/// <summary>Holds a FIX 4.4 LegCreditRating, tag 257, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegCreditRating                      { get; set; } = ValidateLegCreditRating;

	/// <summary>Holds a FIX 4.4 TradedFlatSwitch, tag 258, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   TradedFlatSwitch                     { get; set; } = ValidateTradedFlatSwitch;

	/// <summary>Holds a FIX 4.4 BasisFeatureDate, tag 259, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      BasisFeatureDate                     { get; set; } = ValidateBasisFeatureDate;

	/// <summary>Holds a FIX 4.4 BasisFeaturePrice, tag 260, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BasisFeaturePrice                    { get; set; } = ValidateBasisFeaturePrice;

	/// <summary>Holds a FIX 4.4 MDReqID, tag 262, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDReqID                              { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 4.4 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SubscriptionRequestType              { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.4 MarketDepth, tag 264, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MarketDepth                          { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 4.4 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MDUpdateType                         { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.4 AggregatedBook, tag 266, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   AggregatedBook                       { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 4.4 NoMDEntryTypes, tag 267, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoMDEntryTypes                       { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 4.4 NoMDEntries, tag 268, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoMDEntries                          { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 4.4 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MDEntryType                          { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.4 MDEntryPx, tag 270, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MDEntryPx                            { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 4.4 MDEntrySize, tag 271, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MDEntrySize                          { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 4.4 MDEntryDate, tag 272, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      MDEntryDate                          { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 4.4 MDEntryTime, tag 273, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Time>      MDEntryTime                          { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 4.4 TickDirection, tag 274, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> TickDirection                        { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.4 MDMkt, tag 275, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDMkt                                { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 4.4 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  QuoteCondition                       { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.4 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  TradeCondition                       { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.4 MDEntryID, tag 278, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDEntryID                            { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 4.4 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MDUpdateAction                       { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.4 MDEntryRefID, tag 280, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDEntryRefID                         { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 4.4 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MDReqRejReason                       { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.4 MDEntryOriginator, tag 282, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDEntryOriginator                    { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 4.4 LocationID, tag 283, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LocationID                           { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 4.4 DeskID, tag 284, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      DeskID                               { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 4.4 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> DeleteReason                         { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.4 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  OpenCloseSettlFlag                   { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 4.4 SellerDays, tag 287, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SellerDays                           { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 4.4 MDEntryBuyer, tag 288, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDEntryBuyer                         { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 4.4 MDEntrySeller, tag 289, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MDEntrySeller                        { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 4.4 MDEntryPositionNo, tag 290, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MDEntryPositionNo                    { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 4.4 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  FinancialStatus                      { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.4 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  CorporateAction                      { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.4 DefBidSize, tag 293, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DefBidSize                           { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 4.4 DefOfferSize, tag 294, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DefOfferSize                         { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 4.4 NoQuoteEntries, tag 295, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoQuoteEntries                       { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 NoQuoteSets, tag 296, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoQuoteSets                          { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 4.4 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteStatus                          { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 4.4 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteCancelType                      { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.4 QuoteEntryID, tag 299, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteEntryID                         { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 4.4 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteRejectReason                    { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.4 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteResponseLevel                   { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.4 QuoteSetID, tag 302, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteSetID                           { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 4.4 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteRequestType                     { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.4 TotNoQuoteEntries, tag 304, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoQuoteEntries                    { get; set; } = ValidateTotNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityIDSource, tag 305, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityIDSource           { get; set; } = ValidateUnderlyingSecurityIDSource;

	/// <summary>Holds a FIX 4.4 UnderlyingIssuer, tag 306, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingIssuer                     { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityDesc               { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityExchange           { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityID                 { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityType               { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbol, tag 311, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSymbol                     { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSymbolSfx                  { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.MonthYear> UnderlyingMaturityMonthYear          { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UnderlyingPutOrCall                  { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingStrikePrice                { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 4.4 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> UnderlyingOptAttribute               { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrency, tag 318, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCurrency                   { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 4.4 SecurityReqID, tag 320, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityReqID                        { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 4.4 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecurityRequestType                  { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.4 SecurityResponseID, tag 322, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityResponseID                   { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 4.4 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecurityResponseType                 { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.4 SecurityStatusReqID, tag 324, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityStatusReqID                  { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 4.4 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   UnsolicitedIndicator                 { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 4.4 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecurityTradingStatus                { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.4 HaltReason, tag 327, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> HaltReason                           { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.4 InViewOfCommon, tag 328, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   InViewOfCommon                       { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 4.4 DueToRelated, tag 329, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   DueToRelated                         { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 4.4 BuyVolume, tag 330, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BuyVolume                            { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 4.4 SellVolume, tag 331, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SellVolume                           { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 4.4 HighPx, tag 332, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   HighPx                               { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 4.4 LowPx, tag 333, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LowPx                                { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 4.4 Adjustment, tag 334, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Adjustment                           { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.4 TradSesReqID, tag 335, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradSesReqID                         { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 4.4 TradingSessionID, tag 336, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradingSessionID                     { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 4.4 ContraTrader, tag 337, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ContraTrader                         { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 4.4 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradSesMethod                        { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.4 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradSesMode                          { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.4 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradSesStatus                        { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.4 TradSesStartTime, tag 341, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TradSesStartTime                     { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 4.4 TradSesOpenTime, tag 342, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TradSesOpenTime                      { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 4.4 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TradSesPreCloseTime                  { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesCloseTime, tag 344, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TradSesCloseTime                     { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesEndTime, tag 345, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TradSesEndTime                       { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 4.4 NumberOfOrders, tag 346, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NumberOfOrders                       { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 4.4 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MessageEncoding                      { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.4 EncodedIssuerLen, tag 348, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedIssuerLen                     { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedIssuer, tag 349, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedIssuer                        { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedSecurityDescLen               { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedSecurityDesc                  { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedListExecInstLen               { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 4.4 EncodedListExecInst, tag 353, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedListExecInst                  { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 4.4 EncodedTextLen, tag 354, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedTextLen                       { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 4.4 EncodedText, tag 355, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedText                          { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 4.4 EncodedSubjectLen, tag 356, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedSubjectLen                    { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 4.4 EncodedSubject, tag 357, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedSubject                       { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 4.4 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedHeadlineLen                   { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 4.4 EncodedHeadline, tag 359, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedHeadline                      { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 4.4 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedAllocTextLen                  { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 4.4 EncodedAllocText, tag 361, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedAllocText                     { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedUnderlyingIssuerLen           { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedUnderlyingIssuer              { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedUnderlyingSecurityDescLen     { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedUnderlyingSecurityDesc        { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 AllocPrice, tag 366, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocPrice                           { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 4.4 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> QuoteSetValidUntilTime               { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 4.4 QuoteEntryRejectReason, tag 368, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteEntryRejectReason               { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 4.4 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LastMsgSeqNumProcessed               { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 4.4 RefTagID, tag 371, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RefTagID                             { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 4.4 RefMsgType, tag 372, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RefMsgType                           { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 4.4 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SessionRejectReason                  { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.4 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> BidRequestTransType                  { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.4 ContraBroker, tag 375, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ContraBroker                         { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 4.4 ComplianceID, tag 376, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ComplianceID                         { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 4.4 SolicitedFlag, tag 377, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   SolicitedFlag                        { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 4.4 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ExecRestatementReason                { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.4 BusinessRejectRefID, tag 379, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BusinessRejectRefID                  { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 4.4 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BusinessRejectReason                 { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.4 GrossTradeAmt, tag 381, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   GrossTradeAmt                        { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 4.4 NoContraBrokers, tag 382, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoContraBrokers                      { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 4.4 MaxMessageSize, tag 383, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MaxMessageSize                       { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 4.4 NoMsgTypes, tag 384, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoMsgTypes                           { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 4.4 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MsgDirection                         { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.4 NoTradingSessions, tag 386, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoTradingSessions                    { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 4.4 TotalVolumeTraded, tag 387, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   TotalVolumeTraded                    { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 4.4 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> DiscretionInst                       { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetValue, tag 389, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DiscretionOffsetValue                { get; set; } = ValidateDiscretionOffsetValue;

	/// <summary>Holds a FIX 4.4 BidID, tag 390, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BidID                                { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 4.4 ClientBidID, tag 391, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ClientBidID                          { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 4.4 ListName, tag 392, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ListName                             { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 4.4 TotNoRelatedSym, tag 393, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoRelatedSym                      { get; set; } = ValidateTotNoRelatedSym;

	/// <summary>Holds a FIX 4.4 BidType, tag 394, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BidType                              { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.4 NumTickets, tag 395, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NumTickets                           { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 4.4 SideValue1, tag 396, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SideValue1                           { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 4.4 SideValue2, tag 397, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SideValue2                           { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 4.4 NoBidDescriptors, tag 398, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoBidDescriptors                     { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 4.4 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BidDescriptorType                    { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.4 BidDescriptor, tag 400, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BidDescriptor                        { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 4.4 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SideValueInd                         { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.4 LiquidityPctLow, tag 402, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LiquidityPctLow                      { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 4.4 LiquidityPctHigh, tag 403, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LiquidityPctHigh                     { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 4.4 LiquidityValue, tag 404, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LiquidityValue                       { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 4.4 EFPTrackingError, tag 405, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   EFPTrackingError                     { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 4.4 FairValue, tag 406, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   FairValue                            { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 4.4 OutsideIndexPct, tag 407, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OutsideIndexPct                      { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 4.4 ValueOfFutures, tag 408, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ValueOfFutures                       { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 4.4 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LiquidityIndType                     { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.4 WtAverageLiquidity, tag 410, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   WtAverageLiquidity                   { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 4.4 ExchangeForPhysical, tag 411, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ExchangeForPhysical                  { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 4.4 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OutMainCntryUIndex                   { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 4.4 CrossPercent, tag 413, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CrossPercent                         { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 4.4 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ProgRptReqs                          { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.4 ProgPeriodInterval, tag 415, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ProgPeriodInterval                   { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 4.4 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   IncTaxInd                            { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.4 NumBidders, tag 417, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NumBidders                           { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 4.4 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> BidTradeType                         { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 4.4 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> BasisPxType                          { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.4 NoBidComponents, tag 420, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoBidComponents                      { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 4.4 Country, tag 421, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Country                              { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 4.4 TotNoStrikes, tag 422, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoStrikes                         { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 4.4 PriceType, tag 423, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PriceType                            { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.4 DayOrderQty, tag 424, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DayOrderQty                          { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 4.4 DayCumQty, tag 425, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DayCumQty                            { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 4.4 DayAvgPx, tag 426, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DayAvgPx                             { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 4.4 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   GTBookingInst                        { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.4 NoStrikes, tag 428, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoStrikes                            { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 4.4 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ListStatusType                       { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.4 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NetGrossInd                          { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.4 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ListOrderStatus                      { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.4 ExpireDate, tag 432, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      ExpireDate                           { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 4.4 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> ListExecInstType                     { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.4 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> CxlRejResponseTo                     { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingCouponRate                 { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 4.4 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingContractMultiplier         { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 4.4 ContraTradeQty, tag 437, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ContraTradeQty                       { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 4.4 ContraTradeTime, tag 438, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> ContraTradeTime                      { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 4.4 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LiquidityNumSecurities               { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 4.4 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MultiLegReportingType                { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 StrikeTime, tag 443, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> StrikeTime                           { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 4.4 ListStatusText, tag 444, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ListStatusText                       { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 4.4 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedListStatusTextLen             { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 4.4 EncodedListStatusText, tag 446, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedListStatusText                { get; set; } = ValidateEncodedListStatusText;

	/// <summary>Holds a FIX 4.4 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> PartyIDSource                        { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 4.4 PartyID, tag 448, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PartyID                              { get; set; } = ValidatePartyID;

	/// <summary>Holds a FIX 4.4 NetChgPrevDay, tag 451, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   NetChgPrevDay                        { get; set; } = ValidateNetChgPrevDay;

	/// <summary>Holds a FIX 4.4 PartyRole, tag 452, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PartyRole                            { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 4.4 NoPartyIDs, tag 453, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoPartyIDs                           { get; set; } = ValidateNoPartyIDs;

	/// <summary>Holds a FIX 4.4 NoSecurityAltID, tag 454, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSecurityAltID                      { get; set; } = ValidateNoSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltID, tag 455, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityAltID                        { get; set; } = ValidateSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltIDSource, tag 456, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecurityAltIDSource                  { get; set; } = ValidateSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 NoUnderlyingSecurityAltID, tag 457, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoUnderlyingSecurityAltID            { get; set; } = ValidateNoUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltID, tag 458, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityAltID              { get; set; } = ValidateUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltIDSource, tag 459, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecurityAltIDSource        { get; set; } = ValidateUnderlyingSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 Product, tag 460, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Product                              { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 4.4 CFICode, tag 461, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CFICode                              { get; set; } = ValidateCFICode;

	/// <summary>Holds a FIX 4.4 UnderlyingProduct, tag 462, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UnderlyingProduct                    { get; set; } = ValidateUnderlyingProduct;

	/// <summary>Holds a FIX 4.4 UnderlyingCFICode, tag 463, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCFICode                    { get; set; } = ValidateUnderlyingCFICode;

	/// <summary>Holds a FIX 4.4 TestMessageIndicator, tag 464, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   TestMessageIndicator                 { get; set; } = ValidateTestMessageIndicator;

	/// <summary>Holds a FIX 4.4 BookingRefID, tag 466, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BookingRefID                         { get; set; } = ValidateBookingRefID;

	/// <summary>Holds a FIX 4.4 IndividualAllocID, tag 467, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      IndividualAllocID                    { get; set; } = ValidateIndividualAllocID;

	/// <summary>Holds a FIX 4.4 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> RoundingDirection                    { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 4.4 RoundingModulus, tag 469, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   RoundingModulus                      { get; set; } = ValidateRoundingModulus;

	/// <summary>Holds a FIX 4.4 CountryOfIssue, tag 470, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CountryOfIssue                       { get; set; } = ValidateCountryOfIssue;

	/// <summary>Holds a FIX 4.4 StateOrProvinceOfIssue, tag 471, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StateOrProvinceOfIssue               { get; set; } = ValidateStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LocaleOfIssue, tag 472, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LocaleOfIssue                        { get; set; } = ValidateLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 NoRegistDtls, tag 473, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoRegistDtls                         { get; set; } = ValidateNoRegistDtls;

	/// <summary>Holds a FIX 4.4 MailingDtls, tag 474, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MailingDtls                          { get; set; } = ValidateMailingDtls;

	/// <summary>Holds a FIX 4.4 InvestorCountryOfResidence, tag 475, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      InvestorCountryOfResidence           { get; set; } = ValidateInvestorCountryOfResidence;

	/// <summary>Holds a FIX 4.4 PaymentRef, tag 476, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PaymentRef                           { get; set; } = ValidatePaymentRef;

	/// <summary>Holds a FIX 4.4 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DistribPaymentMethod                 { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 4.4 CashDistribCurr, tag 478, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribCurr                      { get; set; } = ValidateCashDistribCurr;

	/// <summary>Holds a FIX 4.4 CommCurrency, tag 479, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CommCurrency                         { get; set; } = ValidateCommCurrency;

	/// <summary>Holds a FIX 4.4 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> CancellationRights                   { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 4.4 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MoneyLaunderingStatus                { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 4.4 MailingInst, tag 482, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MailingInst                          { get; set; } = ValidateMailingInst;

	/// <summary>Holds a FIX 4.4 TransBkdTime, tag 483, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TransBkdTime                         { get; set; } = ValidateTransBkdTime;

	/// <summary>Holds a FIX 4.4 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> ExecPriceType                        { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 4.4 ExecPriceAdjustment, tag 485, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ExecPriceAdjustment                  { get; set; } = ValidateExecPriceAdjustment;

	/// <summary>Holds a FIX 4.4 DateOfBirth, tag 486, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      DateOfBirth                          { get; set; } = ValidateDateOfBirth;

	/// <summary>Holds a FIX 4.4 TradeReportTransType, tag 487, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeReportTransType                 { get; set; } = ValidateTradeReportTransType;

	/// <summary>Holds a FIX 4.4 CardHolderName, tag 488, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CardHolderName                       { get; set; } = ValidateCardHolderName;

	/// <summary>Holds a FIX 4.4 CardNumber, tag 489, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CardNumber                           { get; set; } = ValidateCardNumber;

	/// <summary>Holds a FIX 4.4 CardExpDate, tag 490, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      CardExpDate                          { get; set; } = ValidateCardExpDate;

	/// <summary>Holds a FIX 4.4 CardIssNum, tag 491, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CardIssNum                           { get; set; } = ValidateCardIssNum;

	/// <summary>Holds a FIX 4.4 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PaymentMethod                        { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 4.4 RegistAcctType, tag 493, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistAcctType                       { get; set; } = ValidateRegistAcctType;

	/// <summary>Holds a FIX 4.4 Designation, tag 494, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Designation                          { get; set; } = ValidateDesignation;

	/// <summary>Holds a FIX 4.4 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TaxAdvantageType                     { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 4.4 RegistRejReasonText, tag 496, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistRejReasonText                  { get; set; } = ValidateRegistRejReasonText;

	/// <summary>Holds a FIX 4.4 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> FundRenewWaiv                        { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 4.4 CashDistribAgentName, tag 498, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribAgentName                 { get; set; } = ValidateCashDistribAgentName;

	/// <summary>Holds a FIX 4.4 CashDistribAgentCode, tag 499, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribAgentCode                 { get; set; } = ValidateCashDistribAgentCode;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctNumber, tag 500, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribAgentAcctNumber           { get; set; } = ValidateCashDistribAgentAcctNumber;

	/// <summary>Holds a FIX 4.4 CashDistribPayRef, tag 501, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribPayRef                    { get; set; } = ValidateCashDistribPayRef;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctName, tag 502, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CashDistribAgentAcctName             { get; set; } = ValidateCashDistribAgentAcctName;

	/// <summary>Holds a FIX 4.4 CardStartDate, tag 503, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      CardStartDate                        { get; set; } = ValidateCardStartDate;

	/// <summary>Holds a FIX 4.4 PaymentDate, tag 504, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      PaymentDate                          { get; set; } = ValidatePaymentDate;

	/// <summary>Holds a FIX 4.4 PaymentRemitterID, tag 505, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PaymentRemitterID                    { get; set; } = ValidatePaymentRemitterID;

	/// <summary>Holds a FIX 4.4 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> RegistStatus                         { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 4.4 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   RegistRejReasonCode                  { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 4.4 RegistRefID, tag 508, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistRefID                          { get; set; } = ValidateRegistRefID;

	/// <summary>Holds a FIX 4.4 RegistDtls, tag 509, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistDtls                           { get; set; } = ValidateRegistDtls;

	/// <summary>Holds a FIX 4.4 NoDistribInsts, tag 510, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoDistribInsts                       { get; set; } = ValidateNoDistribInsts;

	/// <summary>Holds a FIX 4.4 RegistEmail, tag 511, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistEmail                          { get; set; } = ValidateRegistEmail;

	/// <summary>Holds a FIX 4.4 DistribPercentage, tag 512, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DistribPercentage                    { get; set; } = ValidateDistribPercentage;

	/// <summary>Holds a FIX 4.4 RegistID, tag 513, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RegistID                             { get; set; } = ValidateRegistID;

	/// <summary>Holds a FIX 4.4 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> RegistTransType                      { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 4.4 ExecValuationPoint, tag 515, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> ExecValuationPoint                   { get; set; } = ValidateExecValuationPoint;

	/// <summary>Holds a FIX 4.4 OrderPercent, tag 516, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderPercent                         { get; set; } = ValidateOrderPercent;

	/// <summary>Holds a FIX 4.4 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> OwnershipType                        { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 4.4 NoContAmts, tag 518, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoContAmts                           { get; set; } = ValidateNoContAmts;

	/// <summary>Holds a FIX 4.4 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ContAmtType                          { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 4.4 ContAmtValue, tag 520, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ContAmtValue                         { get; set; } = ValidateContAmtValue;

	/// <summary>Holds a FIX 4.4 ContAmtCurr, tag 521, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ContAmtCurr                          { get; set; } = ValidateContAmtCurr;

	/// <summary>Holds a FIX 4.4 OwnerType, tag 522, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   OwnerType                            { get; set; } = ValidateOwnerType;

	/// <summary>Holds a FIX 4.4 PartySubID, tag 523, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PartySubID                           { get; set; } = ValidatePartySubID;

	/// <summary>Holds a FIX 4.4 NestedPartyID, tag 524, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      NestedPartyID                        { get; set; } = ValidateNestedPartyID;

	/// <summary>Holds a FIX 4.4 NestedPartyIDSource, tag 525, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> NestedPartyIDSource                  { get; set; } = ValidateNestedPartyIDSource;

	/// <summary>Holds a FIX 4.4 SecondaryClOrdID, tag 526, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryClOrdID                     { get; set; } = ValidateSecondaryClOrdID;

	/// <summary>Holds a FIX 4.4 SecondaryExecID, tag 527, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryExecID                      { get; set; } = ValidateSecondaryExecID;

	/// <summary>Holds a FIX 4.4 OrderCapacity, tag 528, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> OrderCapacity                        { get; set; } = ValidateOrderCapacity;

	/// <summary>Holds a FIX 4.4 OrderRestrictions, tag 529, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  OrderRestrictions                    { get; set; } = ValidateOrderRestrictions;

	/// <summary>Holds a FIX 4.4 MassCancelRequestType, tag 530, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MassCancelRequestType                { get; set; } = ValidateMassCancelRequestType;

	/// <summary>Holds a FIX 4.4 MassCancelResponse, tag 531, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MassCancelResponse                   { get; set; } = ValidateMassCancelResponse;

	/// <summary>Holds a FIX 4.4 MassCancelRejectReason, tag 532, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MassCancelRejectReason               { get; set; } = ValidateMassCancelRejectReason;

	/// <summary>Holds a FIX 4.4 TotalAffectedOrders, tag 533, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotalAffectedOrders                  { get; set; } = ValidateTotalAffectedOrders;

	/// <summary>Holds a FIX 4.4 NoAffectedOrders, tag 534, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoAffectedOrders                     { get; set; } = ValidateNoAffectedOrders;

	/// <summary>Holds a FIX 4.4 AffectedOrderID, tag 535, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AffectedOrderID                      { get; set; } = ValidateAffectedOrderID;

	/// <summary>Holds a FIX 4.4 AffectedSecondaryOrderID, tag 536, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AffectedSecondaryOrderID             { get; set; } = ValidateAffectedSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 QuoteType, tag 537, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteType                            { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 4.4 NestedPartyRole, tag 538, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NestedPartyRole                      { get; set; } = ValidateNestedPartyRole;

	/// <summary>Holds a FIX 4.4 NoNestedPartyIDs, tag 539, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNestedPartyIDs                     { get; set; } = ValidateNoNestedPartyIDs;

	/// <summary>Holds a FIX 4.4 TotalAccruedInterestAmt, tag 540, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   TotalAccruedInterestAmt              { get; set; } = ValidateTotalAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 MaturityDate, tag 541, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      MaturityDate                         { get; set; } = ValidateMaturityDate;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityDate, tag 542, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      UnderlyingMaturityDate               { get; set; } = ValidateUnderlyingMaturityDate;

	/// <summary>Holds a FIX 4.4 InstrRegistry, tag 543, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      InstrRegistry                        { get; set; } = ValidateInstrRegistry;

	/// <summary>Holds a FIX 4.4 CashMargin, tag 544, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> CashMargin                           { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 4.4 NestedPartySubID, tag 545, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      NestedPartySubID                     { get; set; } = ValidateNestedPartySubID;

	/// <summary>Holds a FIX 4.4 Scope, tag 546, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Multiple>  Scope                                { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 4.4 MDImplicitDelete, tag 547, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   MDImplicitDelete                     { get; set; } = ValidateMDImplicitDelete;

	/// <summary>Holds a FIX 4.4 CrossID, tag 548, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CrossID                              { get; set; } = ValidateCrossID;

	/// <summary>Holds a FIX 4.4 CrossType, tag 549, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CrossType                            { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 4.4 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CrossPrioritization                  { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 4.4 OrigCrossID, tag 551, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrigCrossID                          { get; set; } = ValidateOrigCrossID;

	/// <summary>Holds a FIX 4.4 NoSides, tag 552, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSides                              { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 4.4 Username, tag 553, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Username                             { get; set; } = ValidateUsername;

	/// <summary>Holds a FIX 4.4 Password, tag 554, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Password                             { get; set; } = ValidatePassword;

	/// <summary>Holds a FIX 4.4 NoLegs, tag 555, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoLegs                               { get; set; } = ValidateNoLegs;

	/// <summary>Holds a FIX 4.4 LegCurrency, tag 556, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegCurrency                          { get; set; } = ValidateLegCurrency;

	/// <summary>Holds a FIX 4.4 TotNoSecurityTypes, tag 557, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoSecurityTypes                   { get; set; } = ValidateTotNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 NoSecurityTypes, tag 558, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSecurityTypes                      { get; set; } = ValidateNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecurityListRequestType              { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 4.4 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecurityRequestResult                { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 4.4 RoundLot, tag 561, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   RoundLot                             { get; set; } = ValidateRoundLot;

	/// <summary>Holds a FIX 4.4 MinTradeVol, tag 562, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MinTradeVol                          { get; set; } = ValidateMinTradeVol;

	/// <summary>Holds a FIX 4.4 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MultiLegRptTypeReq                   { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 4.4 LegPositionEffect, tag 564, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> LegPositionEffect                    { get; set; } = ValidateLegPositionEffect;

	/// <summary>Holds a FIX 4.4 LegCoveredOrUncovered, tag 565, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegCoveredOrUncovered                { get; set; } = ValidateLegCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 LegPrice, tag 566, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegPrice                             { get; set; } = ValidateLegPrice;

	/// <summary>Holds a FIX 4.4 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradSesStatusRejReason               { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 4.4 TradeRequestID, tag 568, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeRequestID                       { get; set; } = ValidateTradeRequestID;

	/// <summary>Holds a FIX 4.4 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeRequestType                     { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 4.4 PreviouslyReported, tag 570, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   PreviouslyReported                   { get; set; } = ValidatePreviouslyReported;

	/// <summary>Holds a FIX 4.4 TradeReportID, tag 571, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeReportID                        { get; set; } = ValidateTradeReportID;

	/// <summary>Holds a FIX 4.4 TradeReportRefID, tag 572, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeReportRefID                     { get; set; } = ValidateTradeReportRefID;

	/// <summary>Holds a FIX 4.4 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> MatchStatus                          { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 4.4 MatchType, tag 574, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MatchType                            { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 4.4 OddLot, tag 575, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   OddLot                               { get; set; } = ValidateOddLot;

	/// <summary>Holds a FIX 4.4 NoClearingInstructions, tag 576, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoClearingInstructions               { get; set; } = ValidateNoClearingInstructions;

	/// <summary>Holds a FIX 4.4 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ClearingInstruction                  { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 4.4 TradeInputSource, tag 578, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeInputSource                     { get; set; } = ValidateTradeInputSource;

	/// <summary>Holds a FIX 4.4 TradeInputDevice, tag 579, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeInputDevice                     { get; set; } = ValidateTradeInputDevice;

	/// <summary>Holds a FIX 4.4 NoDates, tag 580, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoDates                              { get; set; } = ValidateNoDates;

	/// <summary>Holds a FIX 4.4 AccountType, tag 581, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AccountType                          { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 4.4 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CustOrderCapacity                    { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 4.4 ClOrdLinkID, tag 583, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ClOrdLinkID                          { get; set; } = ValidateClOrdLinkID;

	/// <summary>Holds a FIX 4.4 MassStatusReqID, tag 584, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      MassStatusReqID                      { get; set; } = ValidateMassStatusReqID;

	/// <summary>Holds a FIX 4.4 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MassStatusReqType                    { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 4.4 OrigOrdModTime, tag 586, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> OrigOrdModTime                       { get; set; } = ValidateOrigOrdModTime;

	/// <summary>Holds a FIX 4.4 LegSettlType, tag 587, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> LegSettlType                         { get; set; } = ValidateLegSettlType;

	/// <summary>Holds a FIX 4.4 LegSettlDate, tag 588, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegSettlDate                         { get; set; } = ValidateLegSettlDate;

	/// <summary>Holds a FIX 4.4 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> DayBookingInst                       { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 4.4 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> BookingUnit                          { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 4.4 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> PreallocMethod                       { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 4.4 UnderlyingCountryOfIssue, tag 592, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCountryOfIssue             { get; set; } = ValidateUnderlyingCountryOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingStateOrProvinceOfIssue, tag 593, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingStateOrProvinceOfIssue     { get; set; } = ValidateUnderlyingStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingLocaleOfIssue, tag 594, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingLocaleOfIssue              { get; set; } = ValidateUnderlyingLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrRegistry, tag 595, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingInstrRegistry              { get; set; } = ValidateUnderlyingInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegCountryOfIssue, tag 596, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegCountryOfIssue                    { get; set; } = ValidateLegCountryOfIssue;

	/// <summary>Holds a FIX 4.4 LegStateOrProvinceOfIssue, tag 597, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegStateOrProvinceOfIssue            { get; set; } = ValidateLegStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LegLocaleOfIssue, tag 598, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegLocaleOfIssue                     { get; set; } = ValidateLegLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 LegInstrRegistry, tag 599, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegInstrRegistry                     { get; set; } = ValidateLegInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegSymbol, tag 600, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSymbol                            { get; set; } = ValidateLegSymbol;

	/// <summary>Holds a FIX 4.4 LegSymbolSfx, tag 601, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSymbolSfx                         { get; set; } = ValidateLegSymbolSfx;

	/// <summary>Holds a FIX 4.4 LegSecurityID, tag 602, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityID                        { get; set; } = ValidateLegSecurityID;

	/// <summary>Holds a FIX 4.4 LegSecurityIDSource, tag 603, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityIDSource                  { get; set; } = ValidateLegSecurityIDSource;

	/// <summary>Holds a FIX 4.4 NoLegSecurityAltID, tag 604, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoLegSecurityAltID                   { get; set; } = ValidateNoLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltID, tag 605, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityAltID                     { get; set; } = ValidateLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltIDSource, tag 606, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityAltIDSource               { get; set; } = ValidateLegSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 LegProduct, tag 607, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegProduct                           { get; set; } = ValidateLegProduct;

	/// <summary>Holds a FIX 4.4 LegCFICode, tag 608, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegCFICode                           { get; set; } = ValidateLegCFICode;

	/// <summary>Holds a FIX 4.4 LegSecurityType, tag 609, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityType                      { get; set; } = ValidateLegSecurityType;

	/// <summary>Holds a FIX 4.4 LegMaturityMonthYear, tag 610, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.MonthYear> LegMaturityMonthYear                 { get; set; } = ValidateLegMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 LegMaturityDate, tag 611, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegMaturityDate                      { get; set; } = ValidateLegMaturityDate;

	/// <summary>Holds a FIX 4.4 LegStrikePrice, tag 612, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegStrikePrice                       { get; set; } = ValidateLegStrikePrice;

	/// <summary>Holds a FIX 4.4 LegOptAttribute, tag 613, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> LegOptAttribute                      { get; set; } = ValidateLegOptAttribute;

	/// <summary>Holds a FIX 4.4 LegContractMultiplier, tag 614, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegContractMultiplier                { get; set; } = ValidateLegContractMultiplier;

	/// <summary>Holds a FIX 4.4 LegCouponRate, tag 615, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegCouponRate                        { get; set; } = ValidateLegCouponRate;

	/// <summary>Holds a FIX 4.4 LegSecurityExchange, tag 616, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityExchange                  { get; set; } = ValidateLegSecurityExchange;

	/// <summary>Holds a FIX 4.4 LegIssuer, tag 617, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegIssuer                            { get; set; } = ValidateLegIssuer;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuerLen, tag 618, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedLegIssuerLen                  { get; set; } = ValidateEncodedLegIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuer, tag 619, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedLegIssuer                     { get; set; } = ValidateEncodedLegIssuer;

	/// <summary>Holds a FIX 4.4 LegSecurityDesc, tag 620, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecurityDesc                      { get; set; } = ValidateLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDescLen, tag 621, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EncodedLegSecurityDescLen            { get; set; } = ValidateEncodedLegSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDesc, tag 622, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Data>      EncodedLegSecurityDesc               { get; set; } = ValidateEncodedLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 LegRatioQty, tag 623, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegRatioQty                          { get; set; } = ValidateLegRatioQty;

	/// <summary>Holds a FIX 4.4 LegSide, tag 624, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> LegSide                              { get; set; } = ValidateLegSide;

	/// <summary>Holds a FIX 4.4 TradingSessionSubID, tag 625, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradingSessionSubID                  { get; set; } = ValidateTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 AllocType, tag 626, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocType                            { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 4.4 NoHops, tag 627, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoHops                               { get; set; } = ValidateNoHops;

	/// <summary>Holds a FIX 4.4 HopCompID, tag 628, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      HopCompID                            { get; set; } = ValidateHopCompID;

	/// <summary>Holds a FIX 4.4 HopSendingTime, tag 629, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> HopSendingTime                       { get; set; } = ValidateHopSendingTime;

	/// <summary>Holds a FIX 4.4 HopRefID, tag 630, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   HopRefID                             { get; set; } = ValidateHopRefID;

	/// <summary>Holds a FIX 4.4 MidPx, tag 631, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MidPx                                { get; set; } = ValidateMidPx;

	/// <summary>Holds a FIX 4.4 BidYield, tag 632, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidYield                             { get; set; } = ValidateBidYield;

	/// <summary>Holds a FIX 4.4 MidYield, tag 633, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MidYield                             { get; set; } = ValidateMidYield;

	/// <summary>Holds a FIX 4.4 OfferYield, tag 634, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferYield                           { get; set; } = ValidateOfferYield;

	/// <summary>Holds a FIX 4.4 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ClearingFeeIndicator                 { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 4.4 WorkingIndicator, tag 636, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   WorkingIndicator                     { get; set; } = ValidateWorkingIndicator;

	/// <summary>Holds a FIX 4.4 LegLastPx, tag 637, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegLastPx                            { get; set; } = ValidateLegLastPx;

	/// <summary>Holds a FIX 4.4 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PriorityIndicator                    { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 4.4 PriceImprovement, tag 639, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PriceImprovement                     { get; set; } = ValidatePriceImprovement;

	/// <summary>Holds a FIX 4.4 Price2, tag 640, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   Price2                               { get; set; } = ValidatePrice2;

	/// <summary>Holds a FIX 4.4 LastForwardPoints2, tag 641, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastForwardPoints2                   { get; set; } = ValidateLastForwardPoints2;

	/// <summary>Holds a FIX 4.4 BidForwardPoints2, tag 642, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BidForwardPoints2                    { get; set; } = ValidateBidForwardPoints2;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints2, tag 643, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OfferForwardPoints2                  { get; set; } = ValidateOfferForwardPoints2;

	/// <summary>Holds a FIX 4.4 RFQReqID, tag 644, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RFQReqID                             { get; set; } = ValidateRFQReqID;

	/// <summary>Holds a FIX 4.4 MktBidPx, tag 645, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MktBidPx                             { get; set; } = ValidateMktBidPx;

	/// <summary>Holds a FIX 4.4 MktOfferPx, tag 646, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MktOfferPx                           { get; set; } = ValidateMktOfferPx;

	/// <summary>Holds a FIX 4.4 MinBidSize, tag 647, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MinBidSize                           { get; set; } = ValidateMinBidSize;

	/// <summary>Holds a FIX 4.4 MinOfferSize, tag 648, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MinOfferSize                         { get; set; } = ValidateMinOfferSize;

	/// <summary>Holds a FIX 4.4 QuoteStatusReqID, tag 649, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteStatusReqID                     { get; set; } = ValidateQuoteStatusReqID;

	/// <summary>Holds a FIX 4.4 LegalConfirm, tag 650, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   LegalConfirm                         { get; set; } = ValidateLegalConfirm;

	/// <summary>Holds a FIX 4.4 UnderlyingLastPx, tag 651, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingLastPx                     { get; set; } = ValidateUnderlyingLastPx;

	/// <summary>Holds a FIX 4.4 UnderlyingLastQty, tag 652, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingLastQty                    { get; set; } = ValidateUnderlyingLastQty;

	/// <summary>Holds a FIX 4.4 LegRefID, tag 654, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegRefID                             { get; set; } = ValidateLegRefID;

	/// <summary>Holds a FIX 4.4 ContraLegRefID, tag 655, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ContraLegRefID                       { get; set; } = ValidateContraLegRefID;

	/// <summary>Holds a FIX 4.4 SettlCurrBidFxRate, tag 656, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SettlCurrBidFxRate                   { get; set; } = ValidateSettlCurrBidFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrOfferFxRate, tag 657, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SettlCurrOfferFxRate                 { get; set; } = ValidateSettlCurrOfferFxRate;

	/// <summary>Holds a FIX 4.4 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteRequestRejectReason             { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 4.4 SideComplianceID, tag 659, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SideComplianceID                     { get; set; } = ValidateSideComplianceID;

	/// <summary>Holds a FIX 4.4 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AcctIDSource                         { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 4.4 AllocAcctIDSource, tag 661, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocAcctIDSource                    { get; set; } = ValidateAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 BenchmarkPrice, tag 662, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   BenchmarkPrice                       { get; set; } = ValidateBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 BenchmarkPriceType, tag 663, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BenchmarkPriceType                   { get; set; } = ValidateBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 ConfirmID, tag 664, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ConfirmID                            { get; set; } = ValidateConfirmID;

	/// <summary>Holds a FIX 4.4 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ConfirmStatus                        { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 4.4 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ConfirmTransType                     { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 4.4 ContractSettlMonth, tag 667, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.MonthYear> ContractSettlMonth                   { get; set; } = ValidateContractSettlMonth;

	/// <summary>Holds a FIX 4.4 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DeliveryForm                         { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 4.4 LastParPx, tag 669, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LastParPx                            { get; set; } = ValidateLastParPx;

	/// <summary>Holds a FIX 4.4 NoLegAllocs, tag 670, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoLegAllocs                          { get; set; } = ValidateNoLegAllocs;

	/// <summary>Holds a FIX 4.4 LegAllocAccount, tag 671, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegAllocAccount                      { get; set; } = ValidateLegAllocAccount;

	/// <summary>Holds a FIX 4.4 LegIndividualAllocID, tag 672, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegIndividualAllocID                 { get; set; } = ValidateLegIndividualAllocID;

	/// <summary>Holds a FIX 4.4 LegAllocQty, tag 673, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegAllocQty                          { get; set; } = ValidateLegAllocQty;

	/// <summary>Holds a FIX 4.4 LegAllocAcctIDSource, tag 674, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegAllocAcctIDSource                 { get; set; } = ValidateLegAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 LegSettlCurrency, tag 675, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSettlCurrency                     { get; set; } = ValidateLegSettlCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveCurrency, tag 676, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegBenchmarkCurveCurrency            { get; set; } = ValidateLegBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveName, tag 677, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegBenchmarkCurveName                { get; set; } = ValidateLegBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurvePoint, tag 678, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegBenchmarkCurvePoint               { get; set; } = ValidateLegBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPrice, tag 679, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegBenchmarkPrice                    { get; set; } = ValidateLegBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPriceType, tag 680, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegBenchmarkPriceType                { get; set; } = ValidateLegBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 LegBidPx, tag 681, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegBidPx                             { get; set; } = ValidateLegBidPx;

	/// <summary>Holds a FIX 4.4 LegIOIQty, tag 682, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegIOIQty                            { get; set; } = ValidateLegIOIQty;

	/// <summary>Holds a FIX 4.4 NoLegStipulations, tag 683, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoLegStipulations                    { get; set; } = ValidateNoLegStipulations;

	/// <summary>Holds a FIX 4.4 LegOfferPx, tag 684, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegOfferPx                           { get; set; } = ValidateLegOfferPx;

	/// <summary>Holds a FIX 4.4 LegPriceType, tag 686, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegPriceType                         { get; set; } = ValidateLegPriceType;

	/// <summary>Holds a FIX 4.4 LegQty, tag 687, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LegQty                               { get; set; } = ValidateLegQty;

	/// <summary>Holds a FIX 4.4 LegStipulationType, tag 688, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegStipulationType                   { get; set; } = ValidateLegStipulationType;

	/// <summary>Holds a FIX 4.4 LegStipulationValue, tag 689, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegStipulationValue                  { get; set; } = ValidateLegStipulationValue;

	/// <summary>Holds a FIX 4.4 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LegSwapType                          { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 4.4 Pool, tag 691, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Pool                                 { get; set; } = ValidatePool;

	/// <summary>Holds a FIX 4.4 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuotePriceType                       { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 4.4 QuoteRespID, tag 693, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      QuoteRespID                          { get; set; } = ValidateQuoteRespID;

	/// <summary>Holds a FIX 4.4 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QuoteRespType                        { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 4.4 QuoteQualifier, tag 695, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> QuoteQualifier                       { get; set; } = ValidateQuoteQualifier;

	/// <summary>Holds a FIX 4.4 YieldRedemptionDate, tag 696, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      YieldRedemptionDate                  { get; set; } = ValidateYieldRedemptionDate;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPrice, tag 697, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   YieldRedemptionPrice                 { get; set; } = ValidateYieldRedemptionPrice;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPriceType, tag 698, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   YieldRedemptionPriceType             { get; set; } = ValidateYieldRedemptionPriceType;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityID, tag 699, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BenchmarkSecurityID                  { get; set; } = ValidateBenchmarkSecurityID;

	/// <summary>Holds a FIX 4.4 ReversalIndicator, tag 700, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ReversalIndicator                    { get; set; } = ValidateReversalIndicator;

	/// <summary>Holds a FIX 4.4 YieldCalcDate, tag 701, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      YieldCalcDate                        { get; set; } = ValidateYieldCalcDate;

	/// <summary>Holds a FIX 4.4 NoPositions, tag 702, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoPositions                          { get; set; } = ValidateNoPositions;

	/// <summary>Holds a FIX 4.4 PosType, tag 703, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PosType                              { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 4.4 LongQty, tag 704, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   LongQty                              { get; set; } = ValidateLongQty;

	/// <summary>Holds a FIX 4.4 ShortQty, tag 705, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ShortQty                             { get; set; } = ValidateShortQty;

	/// <summary>Holds a FIX 4.4 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosQtyStatus                         { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 4.4 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PosAmtType                           { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 4.4 PosAmt, tag 708, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PosAmt                               { get; set; } = ValidatePosAmt;

	/// <summary>Holds a FIX 4.4 PosTransType, tag 709, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosTransType                         { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 4.4 PosReqID, tag 710, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PosReqID                             { get; set; } = ValidatePosReqID;

	/// <summary>Holds a FIX 4.4 NoUnderlyings, tag 711, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoUnderlyings                        { get; set; } = ValidateNoUnderlyings;

	/// <summary>Holds a FIX 4.4 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosMaintAction                       { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 4.4 OrigPosReqRefID, tag 713, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrigPosReqRefID                      { get; set; } = ValidateOrigPosReqRefID;

	/// <summary>Holds a FIX 4.4 PosMaintRptRefID, tag 714, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PosMaintRptRefID                     { get; set; } = ValidatePosMaintRptRefID;

	/// <summary>Holds a FIX 4.4 ClearingBusinessDate, tag 715, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      ClearingBusinessDate                 { get; set; } = ValidateClearingBusinessDate;

	/// <summary>Holds a FIX 4.4 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlSessID                          { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 4.4 SettlSessSubID, tag 717, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlSessSubID                       { get; set; } = ValidateSettlSessSubID;

	/// <summary>Holds a FIX 4.4 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AdjustmentType                       { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 4.4 ContraryInstructionIndicator, tag 719, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   ContraryInstructionIndicator         { get; set; } = ValidateContraryInstructionIndicator;

	/// <summary>Holds a FIX 4.4 PriorSpreadIndicator, tag 720, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   PriorSpreadIndicator                 { get; set; } = ValidatePriorSpreadIndicator;

	/// <summary>Holds a FIX 4.4 PosMaintRptID, tag 721, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      PosMaintRptID                        { get; set; } = ValidatePosMaintRptID;

	/// <summary>Holds a FIX 4.4 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosMaintStatus                       { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 4.4 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosMaintResult                       { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 4.4 PosReqType, tag 724, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosReqType                           { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 4.4 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ResponseTransportType                { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 4.4 ResponseDestination, tag 726, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ResponseDestination                  { get; set; } = ValidateResponseDestination;

	/// <summary>Holds a FIX 4.4 TotalNumPosReports, tag 727, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotalNumPosReports                   { get; set; } = ValidateTotalNumPosReports;

	/// <summary>Holds a FIX 4.4 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosReqResult                         { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 4.4 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PosReqStatus                         { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 4.4 SettlPrice, tag 730, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SettlPrice                           { get; set; } = ValidateSettlPrice;

	/// <summary>Holds a FIX 4.4 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SettlPriceType                       { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPrice, tag 732, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingSettlPrice                 { get; set; } = ValidateUnderlyingSettlPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPriceType, tag 733, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UnderlyingSettlPriceType             { get; set; } = ValidateUnderlyingSettlPriceType;

	/// <summary>Holds a FIX 4.4 PriorSettlPrice, tag 734, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PriorSettlPrice                      { get; set; } = ValidatePriorSettlPrice;

	/// <summary>Holds a FIX 4.4 NoQuoteQualifiers, tag 735, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoQuoteQualifiers                    { get; set; } = ValidateNoQuoteQualifiers;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrency, tag 736, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocSettlCurrency                   { get; set; } = ValidateAllocSettlCurrency;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrAmt, tag 737, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocSettlCurrAmt                    { get; set; } = ValidateAllocSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 InterestAtMaturity, tag 738, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   InterestAtMaturity                   { get; set; } = ValidateInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 LegDatedDate, tag 739, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegDatedDate                         { get; set; } = ValidateLegDatedDate;

	/// <summary>Holds a FIX 4.4 LegPool, tag 740, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegPool                              { get; set; } = ValidateLegPool;

	/// <summary>Holds a FIX 4.4 AllocInterestAtMaturity, tag 741, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocInterestAtMaturity              { get; set; } = ValidateAllocInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 AllocAccruedInterestAmt, tag 742, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllocAccruedInterestAmt              { get; set; } = ValidateAllocAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 DeliveryDate, tag 743, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      DeliveryDate                         { get; set; } = ValidateDeliveryDate;

	/// <summary>Holds a FIX 4.4 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> AssignmentMethod                     { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 4.4 AssignmentUnit, tag 745, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AssignmentUnit                       { get; set; } = ValidateAssignmentUnit;

	/// <summary>Holds a FIX 4.4 OpenInterest, tag 746, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OpenInterest                         { get; set; } = ValidateOpenInterest;

	/// <summary>Holds a FIX 4.4 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> ExerciseMethod                       { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 4.4 TotNumTradeReports, tag 748, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNumTradeReports                   { get; set; } = ValidateTotNumTradeReports;

	/// <summary>Holds a FIX 4.4 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeRequestResult                   { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 4.4 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeRequestStatus                   { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 4.4 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeReportRejectReason              { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 4.4 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SideMultiLegReportingType            { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 NoPosAmt, tag 753, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoPosAmt                             { get; set; } = ValidateNoPosAmt;

	/// <summary>Holds a FIX 4.4 AutoAcceptIndicator, tag 754, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   AutoAcceptIndicator                  { get; set; } = ValidateAutoAcceptIndicator;

	/// <summary>Holds a FIX 4.4 AllocReportID, tag 755, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocReportID                        { get; set; } = ValidateAllocReportID;

	/// <summary>Holds a FIX 4.4 NoNested2PartyIDs, tag 756, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNested2PartyIDs                    { get; set; } = ValidateNoNested2PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartyID, tag 757, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Nested2PartyID                       { get; set; } = ValidateNested2PartyID;

	/// <summary>Holds a FIX 4.4 Nested2PartyIDSource, tag 758, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> Nested2PartyIDSource                 { get; set; } = ValidateNested2PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested2PartyRole, tag 759, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Nested2PartyRole                     { get; set; } = ValidateNested2PartyRole;

	/// <summary>Holds a FIX 4.4 Nested2PartySubID, tag 760, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Nested2PartySubID                    { get; set; } = ValidateNested2PartySubID;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityIDSource, tag 761, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      BenchmarkSecurityIDSource            { get; set; } = ValidateBenchmarkSecurityIDSource;

	/// <summary>Holds a FIX 4.4 SecuritySubType, tag 762, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecuritySubType                      { get; set; } = ValidateSecuritySubType;

	/// <summary>Holds a FIX 4.4 UnderlyingSecuritySubType, tag 763, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingSecuritySubType            { get; set; } = ValidateUnderlyingSecuritySubType;

	/// <summary>Holds a FIX 4.4 LegSecuritySubType, tag 764, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegSecuritySubType                   { get; set; } = ValidateLegSecuritySubType;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessPct, tag 765, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllowableOneSidednessPct             { get; set; } = ValidateAllowableOneSidednessPct;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessValue, tag 766, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AllowableOneSidednessValue           { get; set; } = ValidateAllowableOneSidednessValue;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessCurr, tag 767, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllowableOneSidednessCurr            { get; set; } = ValidateAllowableOneSidednessCurr;

	/// <summary>Holds a FIX 4.4 NoTrdRegTimestamps, tag 768, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoTrdRegTimestamps                   { get; set; } = ValidateNoTrdRegTimestamps;

	/// <summary>Holds a FIX 4.4 TrdRegTimestamp, tag 769, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> TrdRegTimestamp                      { get; set; } = ValidateTrdRegTimestamp;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TrdRegTimestampType                  { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampOrigin, tag 771, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TrdRegTimestampOrigin                { get; set; } = ValidateTrdRegTimestampOrigin;

	/// <summary>Holds a FIX 4.4 ConfirmRefID, tag 772, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ConfirmRefID                         { get; set; } = ValidateConfirmRefID;

	/// <summary>Holds a FIX 4.4 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ConfirmType                          { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 4.4 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ConfirmRejReason                     { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 4.4 BookingType, tag 775, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   BookingType                          { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 4.4 IndividualAllocRejCode, tag 776, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   IndividualAllocRejCode               { get; set; } = ValidateIndividualAllocRejCode;

	/// <summary>Holds a FIX 4.4 SettlInstMsgID, tag 777, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlInstMsgID                       { get; set; } = ValidateSettlInstMsgID;

	/// <summary>Holds a FIX 4.4 NoSettlInst, tag 778, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSettlInst                          { get; set; } = ValidateNoSettlInst;

	/// <summary>Holds a FIX 4.4 LastUpdateTime, tag 779, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Timestamp> LastUpdateTime                       { get; set; } = ValidateLastUpdateTime;

	/// <summary>Holds a FIX 4.4 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocSettlInstType                   { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 4.4 NoSettlPartyIDs, tag 781, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSettlPartyIDs                      { get; set; } = ValidateNoSettlPartyIDs;

	/// <summary>Holds a FIX 4.4 SettlPartyID, tag 782, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlPartyID                         { get; set; } = ValidateSettlPartyID;

	/// <summary>Holds a FIX 4.4 SettlPartyIDSource, tag 783, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> SettlPartyIDSource                   { get; set; } = ValidateSettlPartyIDSource;

	/// <summary>Holds a FIX 4.4 SettlPartyRole, tag 784, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SettlPartyRole                       { get; set; } = ValidateSettlPartyRole;

	/// <summary>Holds a FIX 4.4 SettlPartySubID, tag 785, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlPartySubID                      { get; set; } = ValidateSettlPartySubID;

	/// <summary>Holds a FIX 4.4 SettlPartySubIDType, tag 786, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SettlPartySubIDType                  { get; set; } = ValidateSettlPartySubIDType;

	/// <summary>Holds a FIX 4.4 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> DlvyInstType                         { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 4.4 TerminationType, tag 788, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TerminationType                      { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 4.4 NextExpectedMsgSeqNum, tag 789, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NextExpectedMsgSeqNum                { get; set; } = ValidateNextExpectedMsgSeqNum;

	/// <summary>Holds a FIX 4.4 OrdStatusReqID, tag 790, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrdStatusReqID                       { get; set; } = ValidateOrdStatusReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqID, tag 791, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SettlInstReqID                       { get; set; } = ValidateSettlInstReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SettlInstReqRejCode                  { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 4.4 SecondaryAllocID, tag 793, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryAllocID                     { get; set; } = ValidateSecondaryAllocID;

	/// <summary>Holds a FIX 4.4 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocReportType                      { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 4.4 AllocReportRefID, tag 795, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AllocReportRefID                     { get; set; } = ValidateAllocReportRefID;

	/// <summary>Holds a FIX 4.4 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocCancReplaceReason               { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 4.4 CopyMsgIndicator, tag 797, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   CopyMsgIndicator                     { get; set; } = ValidateCopyMsgIndicator;

	/// <summary>Holds a FIX 4.4 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocAccountType                     { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 4.4 OrderAvgPx, tag 799, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderAvgPx                           { get; set; } = ValidateOrderAvgPx;

	/// <summary>Holds a FIX 4.4 OrderBookingQty, tag 800, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderBookingQty                      { get; set; } = ValidateOrderBookingQty;

	/// <summary>Holds a FIX 4.4 NoSettlPartySubIDs, tag 801, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoSettlPartySubIDs                   { get; set; } = ValidateNoSettlPartySubIDs;

	/// <summary>Holds a FIX 4.4 NoPartySubIDs, tag 802, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoPartySubIDs                        { get; set; } = ValidateNoPartySubIDs;

	/// <summary>Holds a FIX 4.4 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PartySubIDType                       { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNestedPartySubIDs, tag 804, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNestedPartySubIDs                  { get; set; } = ValidateNoNestedPartySubIDs;

	/// <summary>Holds a FIX 4.4 NestedPartySubIDType, tag 805, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NestedPartySubIDType                 { get; set; } = ValidateNestedPartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNested2PartySubIDs, tag 806, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNested2PartySubIDs                 { get; set; } = ValidateNoNested2PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartySubIDType, tag 807, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Nested2PartySubIDType                { get; set; } = ValidateNested2PartySubIDType;

	/// <summary>Holds a FIX 4.4 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocIntermedReqType                 { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 4.4 UnderlyingPx, tag 810, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingPx                         { get; set; } = ValidateUnderlyingPx;

	/// <summary>Holds a FIX 4.4 PriceDelta, tag 811, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PriceDelta                           { get; set; } = ValidatePriceDelta;

	/// <summary>Holds a FIX 4.4 ApplQueueMax, tag 812, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ApplQueueMax                         { get; set; } = ValidateApplQueueMax;

	/// <summary>Holds a FIX 4.4 ApplQueueDepth, tag 813, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ApplQueueDepth                       { get; set; } = ValidateApplQueueDepth;

	/// <summary>Holds a FIX 4.4 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ApplQueueResolution                  { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 4.4 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ApplQueueAction                      { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 4.4 NoAltMDSource, tag 816, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoAltMDSource                        { get; set; } = ValidateNoAltMDSource;

	/// <summary>Holds a FIX 4.4 AltMDSourceID, tag 817, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AltMDSourceID                        { get; set; } = ValidateAltMDSourceID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportID, tag 818, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryTradeReportID               { get; set; } = ValidateSecondaryTradeReportID;

	/// <summary>Holds a FIX 4.4 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AvgPxIndicator                       { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 4.4 TradeLinkID, tag 820, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeLinkID                          { get; set; } = ValidateTradeLinkID;

	/// <summary>Holds a FIX 4.4 OrderInputDevice, tag 821, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      OrderInputDevice                     { get; set; } = ValidateOrderInputDevice;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionID, tag 822, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingTradingSessionID           { get; set; } = ValidateUnderlyingTradingSessionID;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionSubID, tag 823, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingTradingSessionSubID        { get; set; } = ValidateUnderlyingTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 TradeLegRefID, tag 824, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TradeLegRefID                        { get; set; } = ValidateTradeLegRefID;

	/// <summary>Holds a FIX 4.4 ExchangeRule, tag 825, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ExchangeRule                         { get; set; } = ValidateExchangeRule;

	/// <summary>Holds a FIX 4.4 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeAllocIndicator                  { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 4.4 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ExpirationCycle                      { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 4.4 TrdType, tag 828, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TrdType                              { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 4.4 TrdSubType, tag 829, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TrdSubType                           { get; set; } = ValidateTrdSubType;

	/// <summary>Holds a FIX 4.4 TransferReason, tag 830, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TransferReason                       { get; set; } = ValidateTransferReason;

	/// <summary>Holds a FIX 4.4 TotNumAssignmentReports, tag 832, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNumAssignmentReports              { get; set; } = ValidateTotNumAssignmentReports;

	/// <summary>Holds a FIX 4.4 AsgnRptID, tag 833, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AsgnRptID                            { get; set; } = ValidateAsgnRptID;

	/// <summary>Holds a FIX 4.4 ThresholdAmount, tag 834, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ThresholdAmount                      { get; set; } = ValidateThresholdAmount;

	/// <summary>Holds a FIX 4.4 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PegMoveType                          { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 4.4 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PegOffsetType                        { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 4.4 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PegLimitType                         { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 4.4 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PegRoundDirection                    { get; set; } = ValidatePegRoundDirection;

	/// <summary>Holds a FIX 4.4 PeggedPrice, tag 839, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PeggedPrice                          { get; set; } = ValidatePeggedPrice;

	/// <summary>Holds a FIX 4.4 PegScope, tag 840, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   PegScope                             { get; set; } = ValidatePegScope;

	/// <summary>Holds a FIX 4.4 DiscretionMoveType, tag 841, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DiscretionMoveType                   { get; set; } = ValidateDiscretionMoveType;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetType, tag 842, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DiscretionOffsetType                 { get; set; } = ValidateDiscretionOffsetType;

	/// <summary>Holds a FIX 4.4 DiscretionLimitType, tag 843, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DiscretionLimitType                  { get; set; } = ValidateDiscretionLimitType;

	/// <summary>Holds a FIX 4.4 DiscretionRoundDirection, tag 844, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DiscretionRoundDirection             { get; set; } = ValidateDiscretionRoundDirection;

	/// <summary>Holds a FIX 4.4 DiscretionPrice, tag 845, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   DiscretionPrice                      { get; set; } = ValidateDiscretionPrice;

	/// <summary>Holds a FIX 4.4 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DiscretionScope                      { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 4.4 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TargetStrategy                       { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 4.4 TargetStrategyParameters, tag 848, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TargetStrategyParameters             { get; set; } = ValidateTargetStrategyParameters;

	/// <summary>Holds a FIX 4.4 ParticipationRate, tag 849, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ParticipationRate                    { get; set; } = ValidateParticipationRate;

	/// <summary>Holds a FIX 4.4 TargetStrategyPerformance, tag 850, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   TargetStrategyPerformance            { get; set; } = ValidateTargetStrategyPerformance;

	/// <summary>Holds a FIX 4.4 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   LastLiquidityInd                     { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 4.4 PublishTrdIndicator, tag 852, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   PublishTrdIndicator                  { get; set; } = ValidatePublishTrdIndicator;

	/// <summary>Holds a FIX 4.4 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   ShortSaleReason                      { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 4.4 QtyType, tag 854, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   QtyType                              { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 4.4 SecondaryTrdType, tag 855, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   SecondaryTrdType                     { get; set; } = ValidateSecondaryTrdType;

	/// <summary>Holds a FIX 4.4 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TradeReportType                      { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 4.4 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AllocNoOrdersType                    { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 4.4 SharedCommission, tag 858, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   SharedCommission                     { get; set; } = ValidateSharedCommission;

	/// <summary>Holds a FIX 4.4 ConfirmReqID, tag 859, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      ConfirmReqID                         { get; set; } = ValidateConfirmReqID;

	/// <summary>Holds a FIX 4.4 AvgParPx, tag 860, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   AvgParPx                             { get; set; } = ValidateAvgParPx;

	/// <summary>Holds a FIX 4.4 ReportedPx, tag 861, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   ReportedPx                           { get; set; } = ValidateReportedPx;

	/// <summary>Holds a FIX 4.4 NoCapacities, tag 862, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoCapacities                         { get; set; } = ValidateNoCapacities;

	/// <summary>Holds a FIX 4.4 OrderCapacityQty, tag 863, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   OrderCapacityQty                     { get; set; } = ValidateOrderCapacityQty;

	/// <summary>Holds a FIX 4.4 NoEvents, tag 864, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoEvents                             { get; set; } = ValidateNoEvents;

	/// <summary>Holds a FIX 4.4 EventType, tag 865, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   EventType                            { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 4.4 EventDate, tag 866, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      EventDate                            { get; set; } = ValidateEventDate;

	/// <summary>Holds a FIX 4.4 EventPx, tag 867, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   EventPx                              { get; set; } = ValidateEventPx;

	/// <summary>Holds a FIX 4.4 EventText, tag 868, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      EventText                            { get; set; } = ValidateEventText;

	/// <summary>Holds a FIX 4.4 PctAtRisk, tag 869, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   PctAtRisk                            { get; set; } = ValidatePctAtRisk;

	/// <summary>Holds a FIX 4.4 NoInstrAttrib, tag 870, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoInstrAttrib                        { get; set; } = ValidateNoInstrAttrib;

	/// <summary>Holds a FIX 4.4 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   InstrAttribType                      { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 4.4 InstrAttribValue, tag 872, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      InstrAttribValue                     { get; set; } = ValidateInstrAttribValue;

	/// <summary>Holds a FIX 4.4 DatedDate, tag 873, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      DatedDate                            { get; set; } = ValidateDatedDate;

	/// <summary>Holds a FIX 4.4 InterestAccrualDate, tag 874, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      InterestAccrualDate                  { get; set; } = ValidateInterestAccrualDate;

	/// <summary>Holds a FIX 4.4 CPProgram, tag 875, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CPProgram                            { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 4.4 CPRegType, tag 876, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CPRegType                            { get; set; } = ValidateCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingCPProgram, tag 877, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCPProgram                  { get; set; } = ValidateUnderlyingCPProgram;

	/// <summary>Holds a FIX 4.4 UnderlyingCPRegType, tag 878, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingCPRegType                  { get; set; } = ValidateUnderlyingCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingQty, tag 879, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingQty                        { get; set; } = ValidateUnderlyingQty;

	/// <summary>Holds a FIX 4.4 TrdMatchID, tag 880, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TrdMatchID                           { get; set; } = ValidateTrdMatchID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportRefID, tag 881, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      SecondaryTradeReportRefID            { get; set; } = ValidateSecondaryTradeReportRefID;

	/// <summary>Holds a FIX 4.4 UnderlyingDirtyPrice, tag 882, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingDirtyPrice                 { get; set; } = ValidateUnderlyingDirtyPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingEndPrice, tag 883, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingEndPrice                   { get; set; } = ValidateUnderlyingEndPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingStartValue, tag 884, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingStartValue                 { get; set; } = ValidateUnderlyingStartValue;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrentValue, tag 885, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingCurrentValue               { get; set; } = ValidateUnderlyingCurrentValue;

	/// <summary>Holds a FIX 4.4 UnderlyingEndValue, tag 886, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   UnderlyingEndValue                   { get; set; } = ValidateUnderlyingEndValue;

	/// <summary>Holds a FIX 4.4 NoUnderlyingStips, tag 887, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoUnderlyingStips                    { get; set; } = ValidateNoUnderlyingStips;

	/// <summary>Holds a FIX 4.4 UnderlyingStipType, tag 888, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingStipType                   { get; set; } = ValidateUnderlyingStipType;

	/// <summary>Holds a FIX 4.4 UnderlyingStipValue, tag 889, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingStipValue                  { get; set; } = ValidateUnderlyingStipValue;

	/// <summary>Holds a FIX 4.4 MaturityNetMoney, tag 890, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MaturityNetMoney                     { get; set; } = ValidateMaturityNetMoney;

	/// <summary>Holds a FIX 4.4 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   MiscFeeBasis                         { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 4.4 TotNoAllocs, tag 892, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNoAllocs                          { get; set; } = ValidateTotNoAllocs;

	/// <summary>Holds a FIX 4.4 LastFragment, tag 893, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   LastFragment                         { get; set; } = ValidateLastFragment;

	/// <summary>Holds a FIX 4.4 CollReqID, tag 894, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollReqID                            { get; set; } = ValidateCollReqID;

	/// <summary>Holds a FIX 4.4 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollAsgnReason                       { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 4.4 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollInquiryQualifier                 { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 NoTrades, tag 897, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoTrades                             { get; set; } = ValidateNoTrades;

	/// <summary>Holds a FIX 4.4 MarginRatio, tag 898, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MarginRatio                          { get; set; } = ValidateMarginRatio;

	/// <summary>Holds a FIX 4.4 MarginExcess, tag 899, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   MarginExcess                         { get; set; } = ValidateMarginExcess;

	/// <summary>Holds a FIX 4.4 TotalNetValue, tag 900, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   TotalNetValue                        { get; set; } = ValidateTotalNetValue;

	/// <summary>Holds a FIX 4.4 CashOutstanding, tag 901, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   CashOutstanding                      { get; set; } = ValidateCashOutstanding;

	/// <summary>Holds a FIX 4.4 CollAsgnID, tag 902, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollAsgnID                           { get; set; } = ValidateCollAsgnID;

	/// <summary>Holds a FIX 4.4 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollAsgnTransType                    { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 4.4 CollRespID, tag 904, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollRespID                           { get; set; } = ValidateCollRespID;

	/// <summary>Holds a FIX 4.4 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollAsgnRespType                     { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 4.4 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollAsgnRejectReason                 { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 4.4 CollAsgnRefID, tag 907, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollAsgnRefID                        { get; set; } = ValidateCollAsgnRefID;

	/// <summary>Holds a FIX 4.4 CollRptID, tag 908, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollRptID                            { get; set; } = ValidateCollRptID;

	/// <summary>Holds a FIX 4.4 CollInquiryID, tag 909, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      CollInquiryID                        { get; set; } = ValidateCollInquiryID;

	/// <summary>Holds a FIX 4.4 CollStatus, tag 910, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollStatus                           { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 4.4 TotNumReports, tag 911, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TotNumReports                        { get; set; } = ValidateTotNumReports;

	/// <summary>Holds a FIX 4.4 LastRptRequested, tag 912, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Boolean>   LastRptRequested                     { get; set; } = ValidateLastRptRequested;

	/// <summary>Holds a FIX 4.4 AgreementDesc, tag 913, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AgreementDesc                        { get; set; } = ValidateAgreementDesc;

	/// <summary>Holds a FIX 4.4 AgreementID, tag 914, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AgreementID                          { get; set; } = ValidateAgreementID;

	/// <summary>Holds a FIX 4.4 AgreementDate, tag 915, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      AgreementDate                        { get; set; } = ValidateAgreementDate;

	/// <summary>Holds a FIX 4.4 StartDate, tag 916, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      StartDate                            { get; set; } = ValidateStartDate;

	/// <summary>Holds a FIX 4.4 EndDate, tag 917, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      EndDate                              { get; set; } = ValidateEndDate;

	/// <summary>Holds a FIX 4.4 AgreementCurrency, tag 918, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      AgreementCurrency                    { get; set; } = ValidateAgreementCurrency;

	/// <summary>Holds a FIX 4.4 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   DeliveryType                         { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 4.4 EndAccruedInterestAmt, tag 920, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   EndAccruedInterestAmt                { get; set; } = ValidateEndAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 StartCash, tag 921, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   StartCash                            { get; set; } = ValidateStartCash;

	/// <summary>Holds a FIX 4.4 EndCash, tag 922, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Decimal>   EndCash                              { get; set; } = ValidateEndCash;

	/// <summary>Holds a FIX 4.4 UserRequestID, tag 923, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UserRequestID                        { get; set; } = ValidateUserRequestID;

	/// <summary>Holds a FIX 4.4 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UserRequestType                      { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 4.4 NewPassword, tag 925, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      NewPassword                          { get; set; } = ValidateNewPassword;

	/// <summary>Holds a FIX 4.4 UserStatus, tag 926, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   UserStatus                           { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 4.4 UserStatusText, tag 927, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UserStatusText                       { get; set; } = ValidateUserStatusText;

	/// <summary>Holds a FIX 4.4 StatusValue, tag 928, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   StatusValue                          { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 4.4 StatusText, tag 929, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StatusText                           { get; set; } = ValidateStatusText;

	/// <summary>Holds a FIX 4.4 RefCompID, tag 930, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RefCompID                            { get; set; } = ValidateRefCompID;

	/// <summary>Holds a FIX 4.4 RefSubID, tag 931, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      RefSubID                             { get; set; } = ValidateRefSubID;

	/// <summary>Holds a FIX 4.4 NetworkResponseID, tag 932, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      NetworkResponseID                    { get; set; } = ValidateNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestID, tag 933, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      NetworkRequestID                     { get; set; } = ValidateNetworkRequestID;

	/// <summary>Holds a FIX 4.4 LastNetworkResponseID, tag 934, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LastNetworkResponseID                { get; set; } = ValidateLastNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NetworkRequestType                   { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 4.4 NoCompIDs, tag 936, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoCompIDs                            { get; set; } = ValidateNoCompIDs;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NetworkStatusResponseType            { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 4.4 NoCollInquiryQualifier, tag 938, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoCollInquiryQualifier               { get; set; } = ValidateNoCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   TrdRptStatus                         { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 4.4 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   AffirmStatus                         { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikeCurrency, tag 941, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      UnderlyingStrikeCurrency             { get; set; } = ValidateUnderlyingStrikeCurrency;

	/// <summary>Holds a FIX 4.4 LegStrikeCurrency, tag 942, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      LegStrikeCurrency                    { get; set; } = ValidateLegStrikeCurrency;

	/// <summary>Holds a FIX 4.4 TimeBracket, tag 943, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      TimeBracket                          { get; set; } = ValidateTimeBracket;

	/// <summary>Holds a FIX 4.4 CollAction, tag 944, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollAction                           { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 4.4 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollInquiryStatus                    { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 4.4 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   CollInquiryResult                    { get; set; } = ValidateCollInquiryResult;

	/// <summary>Holds a FIX 4.4 StrikeCurrency, tag 947, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      StrikeCurrency                       { get; set; } = ValidateStrikeCurrency;

	/// <summary>Holds a FIX 4.4 NoNested3PartyIDs, tag 948, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNested3PartyIDs                    { get; set; } = ValidateNoNested3PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartyID, tag 949, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Nested3PartyID                       { get; set; } = ValidateNested3PartyID;

	/// <summary>Holds a FIX 4.4 Nested3PartyIDSource, tag 950, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Character> Nested3PartyIDSource                 { get; set; } = ValidateNested3PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested3PartyRole, tag 951, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Nested3PartyRole                     { get; set; } = ValidateNested3PartyRole;

	/// <summary>Holds a FIX 4.4 NoNested3PartySubIDs, tag 952, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   NoNested3PartySubIDs                 { get; set; } = ValidateNoNested3PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartySubID, tag 953, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Text>      Nested3PartySubID                    { get; set; } = ValidateNested3PartySubID;

	/// <summary>Holds a FIX 4.4 Nested3PartySubIDType, tag 954, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Integer>   Nested3PartySubIDType                { get; set; } = ValidateNested3PartySubIDType;

	/// <summary>Holds a FIX 4.4 LegContractSettlMonth, tag 955, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.MonthYear> LegContractSettlMonth                { get; set; } = ValidateLegContractSettlMonth;

	/// <summary>Holds a FIX 4.4 LegInterestAccrualDate, tag 956, to its type.</summary>
	public Action<Fix44Context, FixCheck, FixField.Date>      LegInterestAccrualDate               { get; set; } = ValidateLegInterestAccrualDate;

	static void ValidateAccount(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvId(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvSide(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(check, field);
	}

	static void ValidateAdvTransType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("C" or "N" or "R"))
			Invalid(check, field);
	}

	static void ValidateAvgPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginSeqNo(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginString(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBodyLength(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCheckSum(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClOrdID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommission(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateCumQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndSeqNo(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecInst(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
				"C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or "P" or
				"Q" or "R" or "S" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or "e"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateExecRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHandlInst(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateSecurityIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
			"D" or "E" or "F" or "G" or "H" or "I" or "J"))
			Invalid(check, field);
	}

	static void ValidateIOIid(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIQltyInd(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
			Invalid(check, field);
	}

	static void ValidateIOIRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIQty(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("L" or "M" or "S"))
			Invalid(check, field);
	}

	static void ValidateIOITransType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateLastCapacity(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateLastMkt(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLinesOfText(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgSeqNum(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "AA" or
			"AB" or "AC" or "AD" or "AE" or "AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or "AL" or "AM" or "AN" or
			"AO" or "AP" or "AQ" or "AR" or "AS" or "AT" or "AU" or "AV" or "AW" or "AX" or "AY" or "AZ" or "B" or
			"BA" or "BB" or "BC" or "BD" or "BE" or "BF" or "BG" or "BH" or "C" or "D" or "E" or "F" or "G" or
			"H" or "J" or "K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or "T" or "V" or "W" or "X" or "Y" or
			"Z" or "a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or
			"o" or "p" or "q" or "r" or "s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
			Invalid(check, field);
	}

	static void ValidateNewSeqNo(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrdStatus(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E'))
			Invalid(check, field);
	}

	static void ValidateOrdType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'D' or 'E' or 'G' or 'I' or
			'J' or 'K' or 'L' or 'M' or 'P'))
			Invalid(check, field);
	}

	static void ValidateOrigClOrdID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossDupFlag(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefSeqNum(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSendingTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuantity(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSide(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G'))
			Invalid(check, field);
	}

	static void ValidateSymbol(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeInForce(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(check, field);
	}

	static void ValidateTransactTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUrgency(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateValidUntilTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateSettlDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSymbolSfx(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListSeqNo(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoOrders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInst(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocTransType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRefAllocID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoOrders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgPxPrecision(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePositionEffect(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(check, field);
	}

	static void ValidateNoAllocs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccount(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProcessCode(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateNoRpts(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRptSeq(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDlvyInst(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateAllocRejCode(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateSignature(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureDataLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureData(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSignatureLength(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEmailType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRawDataLength(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRawData(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossResend(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptMethod(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateStopPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDestination(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlRejReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 99))
			Invalid(check, field);
	}

	static void ValidateOrdRejReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or
			99))
			Invalid(check, field);
	}

	static void ValidateIOIQualifier(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or 'S' or
			'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateIssuer(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityDesc(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeartBtInt(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxFloor(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTestReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReportToExch(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocateReqd(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetMoney(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateForexReq(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigSendingTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGapFillFlag(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoExecs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExpireTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDKReason(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateDeliverToCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOINaturalFlag(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMiscFees(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeCurr(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("1" or "10" or "11" or "12" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(check, field);
	}

	static void ValidatePrevClosePx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateResetSeqNumFlag(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderLocationID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetLocationID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfLocationID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToLocationID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRelatedSym(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubject(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeadline(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateURLLink(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(check, field);
	}

	static void ValidateLeavesQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashOrderQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAvgPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocNetMoney(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRateCalc(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(check, field);
	}

	static void ValidateNumDaysInterest(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstMode(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateAllocText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstTransType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(check, field);
	}

	static void ValidateEmailThreadID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("ABS" or "AMENDED" or "AN" or "BA" or "BN" or "BOX" or "BRADY" or "BRIDGE" or
			"BUYSELL" or "CB" or "CD" or "CL" or "CMBS" or "CMO" or "COFO" or "COFP" or "CORP" or "CP" or "CPP" or
			"CS" or "DEFLTED" or "DINP" or "DN" or "DUAL" or "EUCD" or "EUCORP" or "EUCP" or "EUSOV" or "EUSUPRA" or
			"FAC" or "FADN" or "FOR" or "FORWARD" or "FUT" or "GO" or "IET" or "LOFC" or "LQN" or "MATURED" or
			"MBS" or "MF" or "MIO" or "MLEG" or "MPO" or "MPP" or "MPT" or "MT" or "MTN" or "NONE" or "ONITE" or
			"OPT" or "PEF" or "PFAND" or "PN" or "PS" or "PZFJ" or "RAN" or "REPLACD" or "REPO" or "RETIRED" or
			"REV" or "RVLV" or "RVLVTRM" or "SECLOAN" or "SECPLEDGE" or "SPCLA" or "SPCLO" or "SPCLT" or "STN" or
			"STRUCT" or "SUPRA" or "SWING" or "TAN" or "TAXA" or "TBA" or "TBILL" or "TBOND" or "TCAL" or "TD" or
			"TECP" or "TERM" or "TINT" or "TIPS" or "TNOTE" or "TPRN" or "TRAN" or "UST" or "USTB" or "VRDN" or
			"WAR" or "WITHDRN" or "XCN" or "XLINKD" or "YANK" or "YCD"))
			Invalid(check, field);
	}

	static void ValidateEffectiveTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateStandInstDbName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDeliveryType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateBidSpotRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidForwardPoints(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSpotRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferForwardPoints(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDate2(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastSpotRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastForwardPoints(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSecondaryOrderID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoIOIQualifiers(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYear(Fix44Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePutOrCall(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateStrikePrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCoveredOrUncovered(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateOptAttribute(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityExchange(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNotifyBrokerOfCredit(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocHandlInst(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMaxShow(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegOffsetValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlDataLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlData(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRoutingIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRoutingType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateRoutingID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSpread(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurveCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurveName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurvePoint(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCouponRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCouponPaymentDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIssueDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepurchaseTerm(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepurchaseRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFactor(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeOriginationDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContractMultiplier(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoStipulations(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStipulationType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("AMT" or "AUTOREINV" or "BANKQUAL" or "BGNCON" or "COUPON" or "CURRENCY" or
			"CUSTOMDATE" or "GEOG" or "HAIRCUT" or "INSURED" or "ISSUE" or "ISSUER" or "ISSUESIZE" or "LOOKBACK" or
			"LOT" or "LOTVAR" or "MAT" or "MATURITY" or "MAXSUBS" or "MINDNOM" or "MININCR" or "MINQTY" or
			"PAYFREQ" or "PIECES" or "PMAX" or "PPL" or "PPM" or "PPT" or "PRICE" or "PRICEFREQ" or "PROD" or
			"PROTECT" or "PURPOSE" or "PXSOURCE" or "RATING" or "REDEMPTION" or "RESTRICTED" or "SECTOR" or
			"SECTYPE" or "STRUCT" or "SUBSFREQ" or "SUBSLEFT" or "TEXT" or "TRDVAR" or "WAC" or "WAL" or "WALA" or
			"WAM" or "WHOLE" or "YIELD"))
			Invalid(check, field);
	}

	static void ValidateStipulationValue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("AFTERTAX" or "ANNUAL" or "ATISSUE" or "AVGMATURITY" or "BOOK" or "CALL" or
			"CHANGE" or "CLOSE" or "COMPOUND" or "CURRENT" or "GOVTEQUIV" or "GROSS" or "INFLATION" or
			"INVERSEFLOATER" or "LASTCLOSE" or "LASTMONTH" or "LASTQUARTER" or "LASTYEAR" or "LONGAVGLIFE" or
			"MARK" or "MATURITY" or "NEXTREFUND" or "OPENAVG" or "PREVCLOSE" or "PROCEEDS" or "PUT" or
			"SEMIANNUAL" or "SHORTAVGLIFE" or "SIMPLE" or "TAXEQUIV" or "TENDER" or "TRUE" or "VALUE1/32" or "WORST"))
			Invalid(check, field);
	}

	static void ValidateYield(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalTakedown(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConcession(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepoCollateralSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRedemptionDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCouponPaymentDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIssueDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepoCollateralSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepurchaseTerm(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepurchaseRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingFactor(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRedemptionDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCouponPaymentDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIssueDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepoCollateralSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepurchaseTerm(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepurchaseRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegFactor(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRedemptionDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCreditRating(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCreditRating(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCreditRating(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradedFlatSwitch(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBasisFeatureDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBasisFeaturePrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubscriptionRequestType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMarketDepth(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateAggregatedBook(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntryTypes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntries(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(check, field);
	}

	static void ValidateMDEntryPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryTime(Fix44Context context, FixCheck check, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTickDirection(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateMDMkt(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteCondition(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateTradeCondition(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or
				"M" or "N" or "P" or "Q" or "R"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateMDEntryID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateAction(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMDEntryRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqRejReason(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(check, field);
	}

	static void ValidateMDEntryOriginator(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocationID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeskID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeleteReason(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateOpenCloseSettlFlag(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateSellerDays(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryBuyer(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySeller(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryPositionNo(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFinancialStatus(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateCorporateAction(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateDefBidSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefOfferSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteEntries(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteSets(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(check, field);
	}

	static void ValidateQuoteCancelType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateQuoteEntryID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateQuoteResponseLevel(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateQuoteSetID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateTotNoQuoteEntries(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIssuer(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityDesc(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityExchange(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbol(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbolSfx(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityMonthYear(Fix44Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPutOrCall(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStrikePrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingOptAttribute(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateSecurityResponseID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityResponseType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateSecurityStatusReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnsolicitedIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityTradingStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 21 or
			22 or 23 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateHaltReason(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(check, field);
	}

	static void ValidateInViewOfCommon(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDueToRelated(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBuyVolume(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSellVolume(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHighPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLowPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdjustment(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTrader(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesMethod(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesMode(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateTradSesStartTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesOpenTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesPreCloseTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesCloseTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesEndTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNumberOfOrders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMessageEncoding(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(check, field);
	}

	static void ValidateEncodedIssuerLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedIssuer(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDescLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDesc(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInstLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInst(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedTextLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedText(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubjectLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubject(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadlineLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadline(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocTextLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocText(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuerLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuer(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDescLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDesc(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteSetValidUntilTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteEntryRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastMsgSeqNumProcessed(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefTagID(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefMsgType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSessionRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateBidRequestTransType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(check, field);
	}

	static void ValidateContraBroker(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplianceID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSolicitedFlag(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecRestatementReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateBusinessRejectRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBusinessRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateGrossTradeAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoContraBrokers(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxMessageSize(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMsgTypes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgDirection(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(check, field);
	}

	static void ValidateNoTradingSessions(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalVolumeTraded(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDiscretionInst(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateDiscretionOffsetValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClientBidID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoRelatedSym(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNumTickets(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue1(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoBidDescriptors(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidDescriptorType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateBidDescriptor(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValueInd(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateLiquidityPctLow(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityPctHigh(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEFPTrackingError(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFairValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutsideIndexPct(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateValueOfFutures(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityIndType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateWtAverageLiquidity(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeForPhysical(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutMainCntryUIndex(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossPercent(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProgRptReqs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateProgPeriodInterval(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIncTaxInd(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNumBidders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidTradeType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(check, field);
	}

	static void ValidateBasisPxType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'Z'))
			Invalid(check, field);
	}

	static void ValidateNoBidComponents(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCountry(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoStrikes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateDayOrderQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayCumQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayAvgPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGTBookingInst(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoStrikes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateNetGrossInd(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateListOrderStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateExpireDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInstType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateCxlRejResponseTo(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingCouponRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingContractMultiplier(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityNumSecurities(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMultiLegReportingType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateStrikeTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusTextLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusText(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartyIDSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(check, field);
	}

	static void ValidatePartyID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetChgPrevDay(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartyRole(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or
			38 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateNoPartyIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSecurityAltID(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityAltID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityAltIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingSecurityAltID(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityAltID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityAltIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProduct(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateCFICode(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingProduct(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCFICode(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTestMessageIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBookingRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIndividualAllocID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRoundingDirection(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRoundingModulus(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCountryOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStateOrProvinceOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocaleOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRegistDtls(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMailingDtls(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInvestorCountryOfResidence(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentRef(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDistribPaymentMethod(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateCashDistribCurr(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCancellationRights(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('M' or 'N' or 'O' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateMoneyLaunderingStatus(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or 'N' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateMailingInst(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTransBkdTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecPriceType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(check, field);
	}

	static void ValidateExecPriceAdjustment(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDateOfBirth(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportTransType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardHolderName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardNumber(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardExpDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardIssNum(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentMethod(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateRegistAcctType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDesignation(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTaxAdvantageType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateRegistRejReasonText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFundRenewWaiv(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('N' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentCode(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentAcctNumber(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribPayRef(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentAcctName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardStartDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentRemitterID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistStatus(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'H' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateRegistRejReasonCode(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateRegistRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistDtls(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDistribInsts(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistEmail(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDistribPercentage(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistTransType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateExecValuationPoint(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderPercent(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOwnershipType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('2' or 'J' or 'T'))
			Invalid(check, field);
	}

	static void ValidateNoContAmts(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContAmtType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateContAmtValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContAmtCurr(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOwnerType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidatePartySubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartyID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartyIDSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryClOrdID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryExecID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderCapacity(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'I' or 'P' or 'R' or 'W'))
			Invalid(check, field);
	}

	static void ValidateOrderRestrictions(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateMassCancelRequestType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(check, field);
	}

	static void ValidateMassCancelResponse(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(check, field);
	}

	static void ValidateMassCancelRejectReason(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "99"))
			Invalid(check, field);
	}

	static void ValidateTotalAffectedOrders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoAffectedOrders(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAffectedOrderID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAffectedSecondaryOrderID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNestedPartyRole(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNestedPartyIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalAccruedInterestAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrRegistry(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashMargin(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateNestedPartySubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateScope(Fix44Context context, FixCheck check, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3"))
			{
				Invalid(check, field);

				break;
			}
	}

	static void ValidateMDImplicitDelete(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCrossPrioritization(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateOrigCrossID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSides(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateUsername(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePassword(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoSecurityTypes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSecurityTypes(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateSecurityRequestResult(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateRoundLot(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinTradeVol(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMultiLegRptTypeReq(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateLegPositionEffect(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCoveredOrUncovered(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesStatusRejReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 99))
			Invalid(check, field);
	}

	static void ValidateTradeRequestID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidatePreviouslyReported(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMatchStatus(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMatchType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("A1" or "A2" or "A3" or "A4" or "A5" or "AQ" or "M1" or "M2" or "M3" or "M4" or
			"M5" or "M6" or "MT" or "S1" or "S2" or "S3" or "S4" or "S5"))
			Invalid(check, field);
	}

	static void ValidateOddLot(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoClearingInstructions(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingInstruction(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateTradeInputSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeInputDevice(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDates(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccountType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateCustOrderCapacity(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateClOrdLinkID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMassStatusReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMassStatusReqType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateOrigOrdModTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayBookingInst(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateBookingUnit(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidatePreallocMethod(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingCountryOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStateOrProvinceOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLocaleOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrRegistry(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCountryOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStateOrProvinceOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLocaleOfIssue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegInstrRegistry(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSymbol(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSymbolSfx(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegSecurityAltID(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityAltID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityAltIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegProduct(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCFICode(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegMaturityMonthYear(Fix44Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegMaturityDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStrikePrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOptAttribute(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegContractMultiplier(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCouponRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityExchange(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIssuer(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegIssuerLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegIssuer(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityDesc(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegSecurityDescLen(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegSecurityDesc(Fix44Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRatioQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSide(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 5 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateNoHops(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopSendingTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopRefID(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMidPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidYield(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMidYield(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferYield(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingFeeIndicator(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "9" or "B" or "C" or "E" or "F" or "H" or "I" or
			"L" or "M"))
			Invalid(check, field);
	}

	static void ValidateWorkingIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLastPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorityIndicator(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidatePriceImprovement(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrice2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastForwardPoints2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidForwardPoints2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferForwardPoints2(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRFQReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMktBidPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMktOfferPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinBidSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinOfferSize(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteStatusReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegalConfirm(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLastPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLastQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraLegRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrBidFxRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrOfferFxRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRequestRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateSideComplianceID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAcctIDSource(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 99))
			Invalid(check, field);
	}

	static void ValidateAllocAcctIDSource(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateConfirmTransType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateContractSettlMonth(Fix44Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryForm(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateLastParPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegAllocs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocAccount(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIndividualAllocID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocAcctIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurveCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurveName(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurvePoint(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBidPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIOIQty(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegStipulations(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOfferPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStipulationType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStipulationValue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSwapType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidatePool(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuotePriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateQuoteRespID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRespType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateQuoteQualifier(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkSecurityID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReversalIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldCalcDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoPositions(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("ALC" or "AS" or "ASF" or "DLV" or "ETR" or "EX" or "FIN" or "IAS" or "IES" or
			"PA" or "PIT" or "SOD" or "SPL" or "TA" or "TOT" or "TQ" or "TRF" or "TX" or "XM"))
			Invalid(check, field);
	}

	static void ValidateLongQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateShortQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosQtyStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidatePosAmtType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("CASH" or "CRES" or "FMTM" or "IMTM" or "PREM" or "SMTM" or "TVAR" or "VADJ"))
			Invalid(check, field);
	}

	static void ValidatePosAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosTransType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidatePosReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyings(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintAction(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateOrigPosReqRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintRptRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingBusinessDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlSessID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (field.Value is not ("ETH" or "ITD" or "RTH"))
			Invalid(check, field);
	}

	static void ValidateSettlSessSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdjustmentType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateContraryInstructionIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorSpreadIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintRptID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidatePosMaintResult(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 99))
			Invalid(check, field);
	}

	static void ValidatePosReqType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateResponseTransportType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateResponseDestination(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalNumPosReports(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosReqResult(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(check, field);
	}

	static void ValidatePosReqStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateSettlPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlPriceType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorSettlPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteQualifiers(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlCurrAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInterestAtMaturity(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegDatedDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPool(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocInterestAtMaturity(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccruedInterestAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAssignmentMethod(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(check, field);
	}

	static void ValidateAssignmentUnit(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOpenInterest(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExerciseMethod(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('A' or 'M'))
			Invalid(check, field);
	}

	static void ValidateTotNumTradeReports(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeRequestResult(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateTradeRequestStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTradeReportRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(check, field);
	}

	static void ValidateSideMultiLegReportingType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNoPosAmt(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAutoAcceptIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocReportID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested2PartyIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyIDSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyRole(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartySubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkSecurityIDSource(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySubType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecuritySubType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecuritySubType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessPct(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessCurr(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTrdRegTimestamps(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestamp(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestampType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestampOrigin(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateConfirmRejReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateBookingType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateIndividualAllocRejCode(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstMsgID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSettlInst(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastUpdateTime(Fix44Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlInstType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNoSettlPartyIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyIDSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyRole(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartySubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartySubIDType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDlvyInstType(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (field.Value is not ('C' or 'S'))
			Invalid(check, field);
	}

	static void ValidateTerminationType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNextExpectedMsgSeqNum(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrdStatusReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstReqRejCode(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateSecondaryAllocID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocReportType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (3 or 4 or 5 or 8))
			Invalid(check, field);
	}

	static void ValidateAllocReportRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocCancReplaceReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateCopyMsgIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccountType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateOrderAvgPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderBookingQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSettlPartySubIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoPartySubIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartySubIDType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateNoNestedPartySubIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartySubIDType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested2PartySubIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartySubIDType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocIntermedReqType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateUnderlyingPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceDelta(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueMax(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueDepth(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueResolution(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateApplQueueAction(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNoAltMDSource(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAltMDSourceID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradeReportID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgPxIndicator(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTradeLinkID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderInputDevice(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingTradingSessionID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingTradingSessionSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeLegRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeRule(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeAllocIndicator(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateExpirationCycle(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateTrdType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateTrdSubType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTransferReason(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNumAssignmentReports(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAsgnRptID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateThresholdAmount(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegMoveType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidatePegOffsetType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidatePegLimitType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidatePegRoundDirection(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidatePeggedPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegScope(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateDiscretionMoveType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateDiscretionOffsetType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateDiscretionLimitType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateDiscretionRoundDirection(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateDiscretionPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDiscretionScope(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateTargetStrategy(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTargetStrategyParameters(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateParticipationRate(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetStrategyPerformance(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastLiquidityInd(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidatePublishTrdIndicator(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateShortSaleReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateQtyType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSecondaryTrdType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateAllocNoOrdersType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSharedCommission(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgParPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReportedPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoCapacities(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderCapacityQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoEvents(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 99))
			Invalid(check, field);
	}

	static void ValidateEventDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventPx(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePctAtRisk(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoInstrAttrib(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrAttribType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateInstrAttribValue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDatedDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInterestAccrualDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCPProgram(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateCPRegType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCPProgram(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCPRegType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingQty(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdMatchID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradeReportRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingDirtyPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingEndPrice(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStartValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCurrentValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingEndValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingStips(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStipType(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStipValue(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityNetMoney(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeBasis(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTotNoAllocs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastFragment(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollReqID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateCollInquiryQualifier(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateNoTrades(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarginRatio(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarginExcess(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalNetValue(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashOutstanding(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnTransType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCollRespID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnRespType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateCollAsgnRejectReason(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 99))
			Invalid(check, field);
	}

	static void ValidateCollAsgnRefID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollRptID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollInquiryID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateTotNumReports(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastRptRequested(Fix44Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementDesc(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateEndAccruedInterestAmt(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartCash(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndCash(Fix44Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserRequestID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNewPassword(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateUserStatusText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStatusValue(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateStatusText(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefCompID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefSubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkResponseID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkRequestID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastNetworkResponseID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkRequestType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 4 or 8))
			Invalid(check, field);
	}

	static void ValidateNoCompIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkStatusResponseType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoCollInquiryQualifier(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRptStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateAffirmStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateUnderlyingStrikeCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStrikeCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeBracket(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAction(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateCollInquiryStatus(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCollInquiryResult(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateStrikeCurrency(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested3PartyIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyIDSource(Fix44Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyRole(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested3PartySubIDs(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartySubID(Fix44Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartySubIDType(Fix44Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegContractSettlMonth(Fix44Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegInterestAccrualDate(Fix44Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}
}
