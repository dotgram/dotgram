using System;

namespace DotGram.Finance.Fix.Fix50;

// Written by generate.cs from the FIX 5.0 SP2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>The check of every field of FIX 5.0 SP2 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator50
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 5.0 SP2 Account, tag 1, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Account                                         { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 5.0 SP2 AdvId, tag 2, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AdvId                                           { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 5.0 SP2 AdvRefID, tag 3, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AdvRefID                                        { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 5.0 SP2 AdvSide, tag 4, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> AdvSide                                         { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 5.0 SP2 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AdvTransType                                    { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 5.0 SP2 AvgPx, tag 6, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AvgPx                                           { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 BeginSeqNo, tag 7, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BeginSeqNo                                      { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 BeginString, tag 8, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BeginString                                     { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 5.0 SP2 BodyLength, tag 9, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BodyLength                                      { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 5.0 SP2 CheckSum, tag 10, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CheckSum                                        { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 5.0 SP2 ClOrdID, tag 11, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ClOrdID                                         { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 Commission, tag 12, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Commission                                      { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 5.0 SP2 CommType, tag 13, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> CommType                                        { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 5.0 SP2 CumQty, tag 14, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CumQty                                          { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 5.0 SP2 Currency, tag 15, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Currency                                        { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 5.0 SP2 EndSeqNo, tag 16, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EndSeqNo                                        { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 ExecID, tag 17, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ExecID                                          { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 5.0 SP2 ExecInst, tag 18, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  ExecInst                                        { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 5.0 SP2 ExecRefID, tag 19, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ExecRefID                                       { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 5.0 SP2 HandlInst, tag 21, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> HandlInst                                       { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 5.0 SP2 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityIDSource                                { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 IOIID, tag 23, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      IOIID                                           { get; set; } = ValidateIOIID;

	/// <summary>Holds a FIX 5.0 SP2 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> IOIQltyInd                                      { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 5.0 SP2 IOIRefID, tag 26, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      IOIRefID                                        { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 5.0 SP2 IOIQty, tag 27, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      IOIQty                                          { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 5.0 SP2 IOITransType, tag 28, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> IOITransType                                    { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 5.0 SP2 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LastCapacity                                    { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 5.0 SP2 LastMkt, tag 30, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LastMkt                                         { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 5.0 SP2 LastPx, tag 31, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastPx                                          { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 5.0 SP2 LastQty, tag 32, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastQty                                         { get; set; } = ValidateLastQty;

	/// <summary>Holds a FIX 5.0 SP2 NoLinesOfText, tag 33, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLinesOfText                                   { get; set; } = ValidateNoLinesOfText;

	/// <summary>Holds a FIX 5.0 SP2 MsgSeqNum, tag 34, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MsgSeqNum                                       { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 MsgType, tag 35, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MsgType                                         { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 5.0 SP2 NewSeqNo, tag 36, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NewSeqNo                                        { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 OrderID, tag 37, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrderID                                         { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 5.0 SP2 OrderQty, tag 38, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderQty                                        { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OrdStatus                                       { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 5.0 SP2 OrdType, tag 40, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OrdType                                         { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 5.0 SP2 OrigClOrdID, tag 41, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrigClOrdID                                     { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 OrigTime, tag 42, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> OrigTime                                        { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 5.0 SP2 PossDupFlag, tag 43, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PossDupFlag                                     { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 5.0 SP2 Price, tag 44, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Price                                           { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 5.0 SP2 RefSeqNum, tag 45, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RefSeqNum                                       { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 SecurityID, tag 48, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityID                                      { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 SenderCompID, tag 49, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SenderCompID                                    { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 5.0 SP2 SenderSubID, tag 50, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SenderSubID                                     { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 5.0 SP2 SendingTime, tag 52, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> SendingTime                                     { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 Quantity, tag 53, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Quantity                                        { get; set; } = ValidateQuantity;

	/// <summary>Holds a FIX 5.0 SP2 Side, tag 54, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> Side                                            { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 5.0 SP2 Symbol, tag 55, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Symbol                                          { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 5.0 SP2 TargetCompID, tag 56, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TargetCompID                                    { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 5.0 SP2 TargetSubID, tag 57, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TargetSubID                                     { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 5.0 SP2 Text, tag 58, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Text                                            { get; set; } = ValidateText;

	/// <summary>Holds a FIX 5.0 SP2 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TimeInForce                                     { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 5.0 SP2 TransactTime, tag 60, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TransactTime                                    { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 Urgency, tag 61, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> Urgency                                         { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 5.0 SP2 ValidUntilTime, tag 62, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ValidUntilTime                                  { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 5.0 SP2 SettlType, tag 63, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlType                                       { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 5.0 SP2 SettlDate, tag 64, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      SettlDate                                       { get; set; } = ValidateSettlDate;

	/// <summary>Holds a FIX 5.0 SP2 SymbolSfx, tag 65, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SymbolSfx                                       { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 ListID, tag 66, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ListID                                          { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 5.0 SP2 ListSeqNo, tag 67, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ListSeqNo                                       { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 TotNoOrders, tag 68, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoOrders                                     { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 5.0 SP2 ListExecInst, tag 69, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ListExecInst                                    { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 5.0 SP2 AllocID, tag 70, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocID                                         { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 5.0 SP2 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> AllocTransType                                  { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 5.0 SP2 RefAllocID, tag 72, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefAllocID                                      { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 5.0 SP2 NoOrders, tag 73, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoOrders                                        { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 5.0 SP2 AvgPxPrecision, tag 74, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AvgPxPrecision                                  { get; set; } = ValidateAvgPxPrecision;

	/// <summary>Holds a FIX 5.0 SP2 TradeDate, tag 75, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      TradeDate                                       { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 5.0 SP2 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> PositionEffect                                  { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 NoAllocs, tag 78, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoAllocs                                        { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccount, tag 79, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocAccount                                    { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 5.0 SP2 AllocQty, tag 80, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocQty                                        { get; set; } = ValidateAllocQty;

	/// <summary>Holds a FIX 5.0 SP2 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ProcessCode                                     { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 5.0 SP2 NoRpts, tag 82, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRpts                                          { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 5.0 SP2 RptSeq, tag 83, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RptSeq                                          { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 5.0 SP2 CxlQty, tag 84, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CxlQty                                          { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 5.0 SP2 NoDlvyInst, tag 85, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDlvyInst                                      { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 5.0 SP2 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocStatus                                     { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 5.0 SP2 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocRejCode                                    { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 5.0 SP2 Signature, tag 89, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      Signature                                       { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 5.0 SP2 SecureDataLen, tag 90, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecureDataLen                                   { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 5.0 SP2 SecureData, tag 91, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      SecureData                                      { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 5.0 SP2 SignatureLength, tag 93, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SignatureLength                                 { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 5.0 SP2 EmailType, tag 94, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> EmailType                                       { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 5.0 SP2 RawDataLength, tag 95, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RawDataLength                                   { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 5.0 SP2 RawData, tag 96, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      RawData                                         { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 5.0 SP2 PossResend, tag 97, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PossResend                                      { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 5.0 SP2 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncryptMethod                                   { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 5.0 SP2 StopPx, tag 99, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StopPx                                          { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 5.0 SP2 ExDestination, tag 100, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ExDestination                                   { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 5.0 SP2 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CxlRejReason                                    { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 5.0 SP2 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OrdRejReason                                    { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 5.0 SP2 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> IOIQualifier                                    { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 5.0 SP2 Issuer, tag 106, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Issuer                                          { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 5.0 SP2 SecurityDesc, tag 107, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityDesc                                    { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 HeartBtInt, tag 108, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   HeartBtInt                                      { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 5.0 SP2 MinQty, tag 110, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinQty                                          { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 5.0 SP2 MaxFloor, tag 111, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MaxFloor                                        { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 5.0 SP2 TestReqID, tag 112, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TestReqID                                       { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 5.0 SP2 ReportToExch, tag 113, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ReportToExch                                    { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 5.0 SP2 LocateReqd, tag 114, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   LocateReqd                                      { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OnBehalfOfCompID                                { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OnBehalfOfSubID                                 { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteID, tag 117, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteID                                         { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 5.0 SP2 NetMoney, tag 118, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   NetMoney                                        { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrAmt, tag 119, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SettlCurrAmt                                    { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrency, tag 120, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlCurrency                                   { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 ForexReq, tag 121, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ForexReq                                        { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 5.0 SP2 OrigSendingTime, tag 122, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> OrigSendingTime                                 { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 GapFillFlag, tag 123, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   GapFillFlag                                     { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 5.0 SP2 NoExecs, tag 124, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoExecs                                         { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 5.0 SP2 ExpireTime, tag 126, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ExpireTime                                      { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 5.0 SP2 DKReason, tag 127, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DKReason                                        { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToCompID, tag 128, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DeliverToCompID                                 { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToSubID, tag 129, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DeliverToSubID                                  { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 5.0 SP2 IOINaturalFlag, tag 130, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   IOINaturalFlag                                  { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 5.0 SP2 QuoteReqID, tag 131, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteReqID                                      { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 5.0 SP2 BidPx, tag 132, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidPx                                           { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 5.0 SP2 OfferPx, tag 133, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferPx                                         { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 BidSize, tag 134, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidSize                                         { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 5.0 SP2 OfferSize, tag 135, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferSize                                       { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 NoMiscFees, tag 136, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMiscFees                                      { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeAmt, tag 137, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MiscFeeAmt                                      { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeCurr, tag 138, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MiscFeeCurr                                     { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MiscFeeType                                     { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 5.0 SP2 PrevClosePx, tag 140, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PrevClosePx                                     { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 5.0 SP2 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ResetSeqNumFlag                                 { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 5.0 SP2 SenderLocationID, tag 142, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SenderLocationID                                { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 5.0 SP2 TargetLocationID, tag 143, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TargetLocationID                                { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OnBehalfOfLocationID                            { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToLocationID, tag 145, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DeliverToLocationID                             { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 5.0 SP2 NoRelatedSym, tag 146, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRelatedSym                                    { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 5.0 SP2 Subject, tag 147, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Subject                                         { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 5.0 SP2 Headline, tag 148, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Headline                                        { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 5.0 SP2 URLLink, tag 149, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      URLLink                                         { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 5.0 SP2 ExecType, tag 150, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExecType                                        { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 5.0 SP2 LeavesQty, tag 151, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LeavesQty                                       { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 5.0 SP2 CashOrderQty, tag 152, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CashOrderQty                                    { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 AllocAvgPx, tag 153, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocAvgPx                                      { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 AllocNetMoney, tag 154, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocNetMoney                                   { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrFxRate, tag 155, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SettlCurrFxRate                                 { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlCurrFxRateCalc                             { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 5.0 SP2 NumDaysInterest, tag 157, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NumDaysInterest                                 { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 5.0 SP2 AccruedInterestRate, tag 158, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AccruedInterestRate                             { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 5.0 SP2 AccruedInterestAmt, tag 159, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AccruedInterestAmt                              { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlInstMode                                   { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 5.0 SP2 AllocText, tag 161, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocText                                       { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstID, tag 162, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlInstID                                     { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlInstTransType                              { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 5.0 SP2 EmailThreadID, tag 164, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      EmailThreadID                                   { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlInstSource                                 { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 5.0 SP2 SecurityType, tag 167, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityType                                    { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 EffectiveTime, tag 168, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> EffectiveTime                                   { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StandInstDbType                                 { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbName, tag 170, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StandInstDbName                                 { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbID, tag 171, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StandInstDbID                                   { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 5.0 SP2 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlDeliveryType                               { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 5.0 SP2 BidSpotRate, tag 188, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidSpotRate                                     { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 BidForwardPoints, tag 189, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidForwardPoints                                { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 OfferSpotRate, tag 190, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferSpotRate                                   { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 OfferForwardPoints, tag 191, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferForwardPoints                              { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 OrderQty2, tag 192, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderQty2                                       { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 5.0 SP2 SettlDate2, tag 193, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      SettlDate2                                      { get; set; } = ValidateSettlDate2;

	/// <summary>Holds a FIX 5.0 SP2 LastSpotRate, tag 194, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastSpotRate                                    { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 LastForwardPoints, tag 195, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastForwardPoints                               { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 AllocLinkID, tag 196, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocLinkID                                     { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 5.0 SP2 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocLinkType                                   { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryOrderID, tag 198, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryOrderID                                { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 5.0 SP2 NoIOIQualifiers, tag 199, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoIOIQualifiers                                 { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYear, tag 200, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> MaturityMonthYear                               { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PutOrCall                                       { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 StrikePrice, tag 202, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StrikePrice                                     { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CoveredOrUncovered                              { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 5.0 SP2 OptAttribute, tag 206, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OptAttribute                                    { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 SecurityExchange, tag 207, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityExchange                                { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   NotifyBrokerOfCredit                            { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 5.0 SP2 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocHandlInst                                  { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 5.0 SP2 MaxShow, tag 210, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MaxShow                                         { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 5.0 SP2 PegOffsetValue, tag 211, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PegOffsetValue                                  { get; set; } = ValidatePegOffsetValue;

	/// <summary>Holds a FIX 5.0 SP2 XmlDataLen, tag 212, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   XmlDataLen                                      { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 5.0 SP2 XmlData, tag 213, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      XmlData                                         { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstRefID, tag 214, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlInstRefID                                  { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 5.0 SP2 NoRoutingIDs, tag 215, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRoutingIDs                                    { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 5.0 SP2 RoutingType, tag 216, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RoutingType                                     { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 5.0 SP2 RoutingID, tag 217, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RoutingID                                       { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 5.0 SP2 Spread, tag 218, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Spread                                          { get; set; } = ValidateSpread;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurveCurrency, tag 220, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BenchmarkCurveCurrency                          { get; set; } = ValidateBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurveName, tag 221, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BenchmarkCurveName                              { get; set; } = ValidateBenchmarkCurveName;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurvePoint, tag 222, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BenchmarkCurvePoint                             { get; set; } = ValidateBenchmarkCurvePoint;

	/// <summary>Holds a FIX 5.0 SP2 CouponRate, tag 223, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CouponRate                                      { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 CouponPaymentDate, tag 224, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      CouponPaymentDate                               { get; set; } = ValidateCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 IssueDate, tag 225, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      IssueDate                                       { get; set; } = ValidateIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 RepurchaseTerm, tag 226, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RepurchaseTerm                                  { get; set; } = ValidateRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 RepurchaseRate, tag 227, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RepurchaseRate                                  { get; set; } = ValidateRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 Factor, tag 228, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Factor                                          { get; set; } = ValidateFactor;

	/// <summary>Holds a FIX 5.0 SP2 TradeOriginationDate, tag 229, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      TradeOriginationDate                            { get; set; } = ValidateTradeOriginationDate;

	/// <summary>Holds a FIX 5.0 SP2 ExDate, tag 230, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      ExDate                                          { get; set; } = ValidateExDate;

	/// <summary>Holds a FIX 5.0 SP2 ContractMultiplier, tag 231, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ContractMultiplier                              { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 NoStipulations, tag 232, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoStipulations                                  { get; set; } = ValidateNoStipulations;

	/// <summary>Holds a FIX 5.0 SP2 StipulationType, tag 233, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StipulationType                                 { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 5.0 SP2 StipulationValue, tag 234, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StipulationValue                                { get; set; } = ValidateStipulationValue;

	/// <summary>Holds a FIX 5.0 SP2 YieldType, tag 235, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      YieldType                                       { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 5.0 SP2 Yield, tag 236, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Yield                                           { get; set; } = ValidateYield;

	/// <summary>Holds a FIX 5.0 SP2 TotalTakedown, tag 237, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TotalTakedown                                   { get; set; } = ValidateTotalTakedown;

	/// <summary>Holds a FIX 5.0 SP2 Concession, tag 238, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Concession                                      { get; set; } = ValidateConcession;

	/// <summary>Holds a FIX 5.0 SP2 RepoCollateralSecurityType, tag 239, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RepoCollateralSecurityType                      { get; set; } = ValidateRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 RedemptionDate, tag 240, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      RedemptionDate                                  { get; set; } = ValidateRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCouponPaymentDate, tag 241, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingCouponPaymentDate                     { get; set; } = ValidateUnderlyingCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingIssueDate, tag 242, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingIssueDate                             { get; set; } = ValidateUnderlyingIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepoCollateralSecurityType, tag 243, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingRepoCollateralSecurityType            { get; set; } = ValidateUnderlyingRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepurchaseTerm, tag 244, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingRepurchaseTerm                        { get; set; } = ValidateUnderlyingRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepurchaseRate, tag 245, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingRepurchaseRate                        { get; set; } = ValidateUnderlyingRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFactor, tag 246, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingFactor                                { get; set; } = ValidateUnderlyingFactor;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRedemptionDate, tag 247, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingRedemptionDate                        { get; set; } = ValidateUnderlyingRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 LegCouponPaymentDate, tag 248, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegCouponPaymentDate                            { get; set; } = ValidateLegCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 LegIssueDate, tag 249, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegIssueDate                                    { get; set; } = ValidateLegIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 LegRepoCollateralSecurityType, tag 250, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegRepoCollateralSecurityType                   { get; set; } = ValidateLegRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 LegRepurchaseTerm, tag 251, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegRepurchaseTerm                               { get; set; } = ValidateLegRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 LegRepurchaseRate, tag 252, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegRepurchaseRate                               { get; set; } = ValidateLegRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 LegFactor, tag 253, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegFactor                                       { get; set; } = ValidateLegFactor;

	/// <summary>Holds a FIX 5.0 SP2 LegRedemptionDate, tag 254, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegRedemptionDate                               { get; set; } = ValidateLegRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 CreditRating, tag 255, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CreditRating                                    { get; set; } = ValidateCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCreditRating, tag 256, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCreditRating                          { get; set; } = ValidateUnderlyingCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 LegCreditRating, tag 257, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegCreditRating                                 { get; set; } = ValidateLegCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 TradedFlatSwitch, tag 258, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   TradedFlatSwitch                                { get; set; } = ValidateTradedFlatSwitch;

	/// <summary>Holds a FIX 5.0 SP2 BasisFeatureDate, tag 259, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      BasisFeatureDate                                { get; set; } = ValidateBasisFeatureDate;

	/// <summary>Holds a FIX 5.0 SP2 BasisFeaturePrice, tag 260, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BasisFeaturePrice                               { get; set; } = ValidateBasisFeaturePrice;

	/// <summary>Holds a FIX 5.0 SP2 MDReqID, tag 262, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDReqID                                         { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 5.0 SP2 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SubscriptionRequestType                         { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 5.0 SP2 MarketDepth, tag 264, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MarketDepth                                     { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 5.0 SP2 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDUpdateType                                    { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 5.0 SP2 AggregatedBook, tag 266, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   AggregatedBook                                  { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 5.0 SP2 NoMDEntryTypes, tag 267, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMDEntryTypes                                  { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 5.0 SP2 NoMDEntries, tag 268, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMDEntries                                     { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MDEntryType                                     { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryPx, tag 270, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MDEntryPx                                       { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySize, tag 271, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MDEntrySize                                     { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryDate, tag 272, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      MDEntryDate                                     { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryTime, tag 273, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Time>      MDEntryTime                                     { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 5.0 SP2 TickDirection, tag 274, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TickDirection                                   { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 5.0 SP2 MDMkt, tag 275, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDMkt                                           { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 5.0 SP2 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  QuoteCondition                                  { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 5.0 SP2 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  TradeCondition                                  { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryID, tag 278, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDEntryID                                       { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 5.0 SP2 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MDUpdateAction                                  { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryRefID, tag 280, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDEntryRefID                                    { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 5.0 SP2 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MDReqRejReason                                  { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryOriginator, tag 282, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDEntryOriginator                               { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 5.0 SP2 LocationID, tag 283, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LocationID                                      { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 5.0 SP2 DeskID, tag 284, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DeskID                                          { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 5.0 SP2 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DeleteReason                                    { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 5.0 SP2 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  OpenCloseSettlFlag                              { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 5.0 SP2 SellerDays, tag 287, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SellerDays                                      { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryBuyer, tag 288, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDEntryBuyer                                    { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySeller, tag 289, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDEntrySeller                                   { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryPositionNo, tag 290, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDEntryPositionNo                               { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 5.0 SP2 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  FinancialStatus                                 { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 5.0 SP2 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  CorporateAction                                 { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 5.0 SP2 DefBidSize, tag 293, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DefBidSize                                      { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 5.0 SP2 DefOfferSize, tag 294, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DefOfferSize                                    { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteEntries, tag 295, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoQuoteEntries                                  { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteSets, tag 296, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoQuoteSets                                     { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteStatus                                     { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 5.0 SP2 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteCancelType                                 { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryID, tag 299, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteEntryID                                    { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteRejectReason                               { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteResponseLevel                              { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 5.0 SP2 QuoteSetID, tag 302, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteSetID                                      { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteRequestType                                { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 5.0 SP2 TotNoQuoteEntries, tag 304, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoQuoteEntries                               { get; set; } = ValidateTotNoQuoteEntries;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityIDSource, tag 305, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityIDSource                      { get; set; } = ValidateUnderlyingSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingIssuer, tag 306, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingIssuer                                { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityDesc                          { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityExchange                      { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityID                            { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityType                          { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSymbol, tag 311, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSymbol                                { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSymbolSfx                             { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> UnderlyingMaturityMonthYear                     { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingPutOrCall                             { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingStrikePrice                           { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> UnderlyingOptAttribute                          { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCurrency, tag 318, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCurrency                              { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 5.0 SP2 SecurityReqID, tag 320, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityReqID                                   { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityRequestType                             { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityResponseID, tag 322, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityResponseID                              { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityResponseType                            { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatusReqID, tag 324, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityStatusReqID                             { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   UnsolicitedIndicator                            { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityTradingStatus                           { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 5.0 SP2 HaltReasonInt, tag 327, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   HaltReasonInt                                   { get; set; } = ValidateHaltReasonInt;

	/// <summary>Holds a FIX 5.0 SP2 InViewOfCommon, tag 328, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   InViewOfCommon                                  { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 5.0 SP2 DueToRelated, tag 329, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   DueToRelated                                    { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 5.0 SP2 BuyVolume, tag 330, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BuyVolume                                       { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 5.0 SP2 SellVolume, tag 331, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SellVolume                                      { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 5.0 SP2 HighPx, tag 332, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   HighPx                                          { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 5.0 SP2 LowPx, tag 333, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LowPx                                           { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 5.0 SP2 Adjustment, tag 334, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Adjustment                                      { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 5.0 SP2 TradSesReqID, tag 335, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradSesReqID                                    { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionID, tag 336, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradingSessionID                                { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 ContraTrader, tag 337, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ContraTrader                                    { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 5.0 SP2 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradSesMethod                                   { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 5.0 SP2 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradSesMode                                     { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradSesStatus                                   { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStartTime, tag 341, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TradSesStartTime                                { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesOpenTime, tag 342, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TradSesOpenTime                                 { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TradSesPreCloseTime                             { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesCloseTime, tag 344, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TradSesCloseTime                                { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesEndTime, tag 345, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TradSesEndTime                                  { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 5.0 SP2 NumberOfOrders, tag 346, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NumberOfOrders                                  { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 5.0 SP2 MessageEncoding, tag 347, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MessageEncoding                                 { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 5.0 SP2 EncodedIssuerLen, tag 348, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedIssuerLen                                { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedIssuer, tag 349, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedIssuer                                   { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedSecurityDescLen                          { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedSecurityDesc                             { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedListExecInstLen                          { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListExecInst, tag 353, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedListExecInst                             { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 5.0 SP2 EncodedTextLen, tag 354, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedTextLen                                  { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedText, tag 355, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedText                                     { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSubjectLen, tag 356, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedSubjectLen                               { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSubject, tag 357, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedSubject                                  { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 5.0 SP2 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedHeadlineLen                              { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedHeadline, tag 359, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedHeadline                                 { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 5.0 SP2 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedAllocTextLen                             { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedAllocText, tag 361, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedAllocText                                { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedUnderlyingIssuerLen                      { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedUnderlyingIssuer                         { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedUnderlyingSecurityDescLen                { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedUnderlyingSecurityDesc                   { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 AllocPrice, tag 366, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocPrice                                      { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 5.0 SP2 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> QuoteSetValidUntilTime                          { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryRejectReason, tag 368, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteEntryRejectReason                          { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LastMsgSeqNumProcessed                          { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 5.0 SP2 RefTagID, tag 371, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RefTagID                                        { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 5.0 SP2 RefMsgType, tag 372, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefMsgType                                      { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 5.0 SP2 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SessionRejectReason                             { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> BidRequestTransType                             { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 5.0 SP2 ContraBroker, tag 375, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ContraBroker                                    { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 5.0 SP2 ComplianceID, tag 376, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ComplianceID                                    { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 5.0 SP2 SolicitedFlag, tag 377, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   SolicitedFlag                                   { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 5.0 SP2 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ExecRestatementReason                           { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 5.0 SP2 BusinessRejectRefID, tag 379, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BusinessRejectRefID                             { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 5.0 SP2 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BusinessRejectReason                            { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 GrossTradeAmt, tag 381, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   GrossTradeAmt                                   { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 NoContraBrokers, tag 382, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoContraBrokers                                 { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 5.0 SP2 MaxMessageSize, tag 383, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MaxMessageSize                                  { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 5.0 SP2 NoMsgTypes, tag 384, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMsgTypes                                      { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 5.0 SP2 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MsgDirection                                    { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 5.0 SP2 NoTradingSessions, tag 386, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTradingSessions                               { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 5.0 SP2 TotalVolumeTraded, tag 387, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TotalVolumeTraded                               { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DiscretionInst                                  { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionOffsetValue, tag 389, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DiscretionOffsetValue                           { get; set; } = ValidateDiscretionOffsetValue;

	/// <summary>Holds a FIX 5.0 SP2 BidID, tag 390, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BidID                                           { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 5.0 SP2 ClientBidID, tag 391, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ClientBidID                                     { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 5.0 SP2 ListName, tag 392, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ListName                                        { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 5.0 SP2 TotNoRelatedSym, tag 393, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoRelatedSym                                 { get; set; } = ValidateTotNoRelatedSym;

	/// <summary>Holds a FIX 5.0 SP2 BidType, tag 394, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BidType                                         { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 5.0 SP2 NumTickets, tag 395, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NumTickets                                      { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 5.0 SP2 SideValue1, tag 396, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SideValue1                                      { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 5.0 SP2 SideValue2, tag 397, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SideValue2                                      { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 5.0 SP2 NoBidDescriptors, tag 398, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoBidDescriptors                                { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 5.0 SP2 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BidDescriptorType                               { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 5.0 SP2 BidDescriptor, tag 400, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BidDescriptor                                   { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 5.0 SP2 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideValueInd                                    { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityPctLow, tag 402, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LiquidityPctLow                                 { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityPctHigh, tag 403, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LiquidityPctHigh                                { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityValue, tag 404, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LiquidityValue                                  { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 5.0 SP2 EFPTrackingError, tag 405, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EFPTrackingError                                { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 5.0 SP2 FairValue, tag 406, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FairValue                                       { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 5.0 SP2 OutsideIndexPct, tag 407, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OutsideIndexPct                                 { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 5.0 SP2 ValueOfFutures, tag 408, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ValueOfFutures                                  { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LiquidityIndType                                { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 5.0 SP2 WtAverageLiquidity, tag 410, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   WtAverageLiquidity                              { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeForPhysical, tag 411, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ExchangeForPhysical                             { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 5.0 SP2 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OutMainCntryUIndex                              { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 5.0 SP2 CrossPercent, tag 413, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CrossPercent                                    { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 5.0 SP2 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ProgRptReqs                                     { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 5.0 SP2 ProgPeriodInterval, tag 415, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ProgPeriodInterval                              { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 5.0 SP2 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   IncTaxInd                                       { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 5.0 SP2 NumBidders, tag 417, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NumBidders                                      { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 5.0 SP2 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> BidTradeType                                    { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 5.0 SP2 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> BasisPxType                                     { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 5.0 SP2 NoBidComponents, tag 420, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoBidComponents                                 { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 5.0 SP2 Country, tag 421, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Country                                         { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 5.0 SP2 TotNoStrikes, tag 422, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoStrikes                                    { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 5.0 SP2 PriceType, tag 423, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PriceType                                       { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 5.0 SP2 DayOrderQty, tag 424, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DayOrderQty                                     { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 DayCumQty, tag 425, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DayCumQty                                       { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 5.0 SP2 DayAvgPx, tag 426, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DayAvgPx                                        { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   GTBookingInst                                   { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 5.0 SP2 NoStrikes, tag 428, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoStrikes                                       { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 5.0 SP2 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ListStatusType                                  { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 5.0 SP2 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NetGrossInd                                     { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 5.0 SP2 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ListOrderStatus                                 { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 5.0 SP2 ExpireDate, tag 432, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      ExpireDate                                      { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 5.0 SP2 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ListExecInstType                                { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 5.0 SP2 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> CxlRejResponseTo                                { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingCouponRate                            { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingContractMultiplier                    { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 ContraTradeQty, tag 437, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ContraTradeQty                                  { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 5.0 SP2 ContraTradeTime, tag 438, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ContraTradeTime                                 { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LiquidityNumSecurities                          { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 5.0 SP2 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MultiLegReportingType                           { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 5.0 SP2 StrikeTime, tag 443, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> StrikeTime                                      { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 5.0 SP2 ListStatusText, tag 444, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ListStatusText                                  { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedListStatusTextLen                        { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListStatusText, tag 446, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedListStatusText                           { get; set; } = ValidateEncodedListStatusText;

	/// <summary>Holds a FIX 5.0 SP2 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> PartyIDSource                                   { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 PartyID, tag 448, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PartyID                                         { get; set; } = ValidatePartyID;

	/// <summary>Holds a FIX 5.0 SP2 NetChgPrevDay, tag 451, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   NetChgPrevDay                                   { get; set; } = ValidateNetChgPrevDay;

	/// <summary>Holds a FIX 5.0 SP2 PartyRole, tag 452, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PartyRole                                       { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoPartyIDs, tag 453, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoPartyIDs                                      { get; set; } = ValidateNoPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoSecurityAltID, tag 454, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSecurityAltID                                 { get; set; } = ValidateNoSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityAltID, tag 455, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityAltID                                   { get; set; } = ValidateSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityAltIDSource, tag 456, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityAltIDSource                             { get; set; } = ValidateSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingSecurityAltID, tag 457, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUnderlyingSecurityAltID                       { get; set; } = ValidateNoUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityAltID, tag 458, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityAltID                         { get; set; } = ValidateUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityAltIDSource, tag 459, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecurityAltIDSource                   { get; set; } = ValidateUnderlyingSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Product, tag 460, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Product                                         { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 5.0 SP2 CFICode, tag 461, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CFICode                                         { get; set; } = ValidateCFICode;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingProduct, tag 462, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingProduct                               { get; set; } = ValidateUnderlyingProduct;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCFICode, tag 463, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCFICode                               { get; set; } = ValidateUnderlyingCFICode;

	/// <summary>Holds a FIX 5.0 SP2 TestMessageIndicator, tag 464, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   TestMessageIndicator                            { get; set; } = ValidateTestMessageIndicator;

	/// <summary>Holds a FIX 5.0 SP2 BookingRefID, tag 466, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BookingRefID                                    { get; set; } = ValidateBookingRefID;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocID, tag 467, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      IndividualAllocID                               { get; set; } = ValidateIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> RoundingDirection                               { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 5.0 SP2 RoundingModulus, tag 469, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RoundingModulus                                 { get; set; } = ValidateRoundingModulus;

	/// <summary>Holds a FIX 5.0 SP2 CountryOfIssue, tag 470, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CountryOfIssue                                  { get; set; } = ValidateCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 StateOrProvinceOfIssue, tag 471, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StateOrProvinceOfIssue                          { get; set; } = ValidateStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LocaleOfIssue, tag 472, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LocaleOfIssue                                   { get; set; } = ValidateLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 NoRegistDtls, tag 473, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRegistDtls                                    { get; set; } = ValidateNoRegistDtls;

	/// <summary>Holds a FIX 5.0 SP2 MailingDtls, tag 474, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MailingDtls                                     { get; set; } = ValidateMailingDtls;

	/// <summary>Holds a FIX 5.0 SP2 InvestorCountryOfResidence, tag 475, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InvestorCountryOfResidence                      { get; set; } = ValidateInvestorCountryOfResidence;

	/// <summary>Holds a FIX 5.0 SP2 PaymentRef, tag 476, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PaymentRef                                      { get; set; } = ValidatePaymentRef;

	/// <summary>Holds a FIX 5.0 SP2 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DistribPaymentMethod                            { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribCurr, tag 478, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribCurr                                 { get; set; } = ValidateCashDistribCurr;

	/// <summary>Holds a FIX 5.0 SP2 CommCurrency, tag 479, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CommCurrency                                    { get; set; } = ValidateCommCurrency;

	/// <summary>Holds a FIX 5.0 SP2 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> CancellationRights                              { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 5.0 SP2 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MoneyLaunderingStatus                           { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 5.0 SP2 MailingInst, tag 482, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MailingInst                                     { get; set; } = ValidateMailingInst;

	/// <summary>Holds a FIX 5.0 SP2 TransBkdTime, tag 483, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TransBkdTime                                    { get; set; } = ValidateTransBkdTime;

	/// <summary>Holds a FIX 5.0 SP2 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExecPriceType                                   { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 5.0 SP2 ExecPriceAdjustment, tag 485, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ExecPriceAdjustment                             { get; set; } = ValidateExecPriceAdjustment;

	/// <summary>Holds a FIX 5.0 SP2 DateOfBirth, tag 486, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DateOfBirth                                     { get; set; } = ValidateDateOfBirth;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportTransType, tag 487, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeReportTransType                            { get; set; } = ValidateTradeReportTransType;

	/// <summary>Holds a FIX 5.0 SP2 CardHolderName, tag 488, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CardHolderName                                  { get; set; } = ValidateCardHolderName;

	/// <summary>Holds a FIX 5.0 SP2 CardNumber, tag 489, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CardNumber                                      { get; set; } = ValidateCardNumber;

	/// <summary>Holds a FIX 5.0 SP2 CardExpDate, tag 490, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      CardExpDate                                     { get; set; } = ValidateCardExpDate;

	/// <summary>Holds a FIX 5.0 SP2 CardIssNum, tag 491, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CardIssNum                                      { get; set; } = ValidateCardIssNum;

	/// <summary>Holds a FIX 5.0 SP2 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PaymentMethod                                   { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 5.0 SP2 RegistAcctType, tag 493, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistAcctType                                  { get; set; } = ValidateRegistAcctType;

	/// <summary>Holds a FIX 5.0 SP2 Designation, tag 494, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Designation                                     { get; set; } = ValidateDesignation;

	/// <summary>Holds a FIX 5.0 SP2 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TaxAdvantageType                                { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 5.0 SP2 RegistRejReasonText, tag 496, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistRejReasonText                             { get; set; } = ValidateRegistRejReasonText;

	/// <summary>Holds a FIX 5.0 SP2 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> FundRenewWaiv                                   { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentName, tag 498, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribAgentName                            { get; set; } = ValidateCashDistribAgentName;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentCode, tag 499, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribAgentCode                            { get; set; } = ValidateCashDistribAgentCode;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentAcctNumber, tag 500, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribAgentAcctNumber                      { get; set; } = ValidateCashDistribAgentAcctNumber;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribPayRef, tag 501, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribPayRef                               { get; set; } = ValidateCashDistribPayRef;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentAcctName, tag 502, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CashDistribAgentAcctName                        { get; set; } = ValidateCashDistribAgentAcctName;

	/// <summary>Holds a FIX 5.0 SP2 CardStartDate, tag 503, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      CardStartDate                                   { get; set; } = ValidateCardStartDate;

	/// <summary>Holds a FIX 5.0 SP2 PaymentDate, tag 504, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      PaymentDate                                     { get; set; } = ValidatePaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 PaymentRemitterID, tag 505, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PaymentRemitterID                               { get; set; } = ValidatePaymentRemitterID;

	/// <summary>Holds a FIX 5.0 SP2 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> RegistStatus                                    { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 5.0 SP2 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RegistRejReasonCode                             { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 5.0 SP2 RegistRefID, tag 508, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistRefID                                     { get; set; } = ValidateRegistRefID;

	/// <summary>Holds a FIX 5.0 SP2 RegistDtls, tag 509, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistDtls                                      { get; set; } = ValidateRegistDtls;

	/// <summary>Holds a FIX 5.0 SP2 NoDistribInsts, tag 510, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDistribInsts                                  { get; set; } = ValidateNoDistribInsts;

	/// <summary>Holds a FIX 5.0 SP2 RegistEmail, tag 511, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistEmail                                     { get; set; } = ValidateRegistEmail;

	/// <summary>Holds a FIX 5.0 SP2 DistribPercentage, tag 512, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DistribPercentage                               { get; set; } = ValidateDistribPercentage;

	/// <summary>Holds a FIX 5.0 SP2 RegistID, tag 513, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RegistID                                        { get; set; } = ValidateRegistID;

	/// <summary>Holds a FIX 5.0 SP2 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> RegistTransType                                 { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 5.0 SP2 ExecValuationPoint, tag 515, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ExecValuationPoint                              { get; set; } = ValidateExecValuationPoint;

	/// <summary>Holds a FIX 5.0 SP2 OrderPercent, tag 516, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderPercent                                    { get; set; } = ValidateOrderPercent;

	/// <summary>Holds a FIX 5.0 SP2 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OwnershipType                                   { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 5.0 SP2 NoContAmts, tag 518, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoContAmts                                      { get; set; } = ValidateNoContAmts;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ContAmtType                                     { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtValue, tag 520, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ContAmtValue                                    { get; set; } = ValidateContAmtValue;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtCurr, tag 521, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ContAmtCurr                                     { get; set; } = ValidateContAmtCurr;

	/// <summary>Holds a FIX 5.0 SP2 OwnerType, tag 522, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OwnerType                                       { get; set; } = ValidateOwnerType;

	/// <summary>Holds a FIX 5.0 SP2 PartySubID, tag 523, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PartySubID                                      { get; set; } = ValidatePartySubID;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyID, tag 524, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NestedPartyID                                   { get; set; } = ValidateNestedPartyID;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyIDSource, tag 525, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> NestedPartyIDSource                             { get; set; } = ValidateNestedPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryClOrdID, tag 526, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryClOrdID                                { get; set; } = ValidateSecondaryClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryExecID, tag 527, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryExecID                                 { get; set; } = ValidateSecondaryExecID;

	/// <summary>Holds a FIX 5.0 SP2 OrderCapacity, tag 528, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OrderCapacity                                   { get; set; } = ValidateOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 OrderRestrictions, tag 529, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  OrderRestrictions                               { get; set; } = ValidateOrderRestrictions;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelRequestType, tag 530, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MassCancelRequestType                           { get; set; } = ValidateMassCancelRequestType;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelResponse, tag 531, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MassCancelResponse                              { get; set; } = ValidateMassCancelResponse;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelRejectReason, tag 532, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassCancelRejectReason                          { get; set; } = ValidateMassCancelRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 TotalAffectedOrders, tag 533, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotalAffectedOrders                             { get; set; } = ValidateTotalAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 NoAffectedOrders, tag 534, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoAffectedOrders                                { get; set; } = ValidateNoAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 AffectedOrderID, tag 535, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AffectedOrderID                                 { get; set; } = ValidateAffectedOrderID;

	/// <summary>Holds a FIX 5.0 SP2 AffectedSecondaryOrderID, tag 536, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AffectedSecondaryOrderID                        { get; set; } = ValidateAffectedSecondaryOrderID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteType, tag 537, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteType                                       { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyRole, tag 538, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NestedPartyRole                                 { get; set; } = ValidateNestedPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedPartyIDs, tag 539, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNestedPartyIDs                                { get; set; } = ValidateNoNestedPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 TotalAccruedInterestAmt, tag 540, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TotalAccruedInterestAmt                         { get; set; } = ValidateTotalAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 MaturityDate, tag 541, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      MaturityDate                                    { get; set; } = ValidateMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityDate, tag 542, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingMaturityDate                          { get; set; } = ValidateUnderlyingMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 InstrRegistry, tag 543, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InstrRegistry                                   { get; set; } = ValidateInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 CashMargin, tag 544, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> CashMargin                                      { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartySubID, tag 545, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NestedPartySubID                                { get; set; } = ValidateNestedPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 Scope, tag 546, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  Scope                                           { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 5.0 SP2 MDImplicitDelete, tag 547, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   MDImplicitDelete                                { get; set; } = ValidateMDImplicitDelete;

	/// <summary>Holds a FIX 5.0 SP2 CrossID, tag 548, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CrossID                                         { get; set; } = ValidateCrossID;

	/// <summary>Holds a FIX 5.0 SP2 CrossType, tag 549, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CrossType                                       { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 5.0 SP2 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CrossPrioritization                             { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 5.0 SP2 OrigCrossID, tag 551, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrigCrossID                                     { get; set; } = ValidateOrigCrossID;

	/// <summary>Holds a FIX 5.0 SP2 NoSides, tag 552, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSides                                         { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 5.0 SP2 Username, tag 553, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Username                                        { get; set; } = ValidateUsername;

	/// <summary>Holds a FIX 5.0 SP2 Password, tag 554, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Password                                        { get; set; } = ValidatePassword;

	/// <summary>Holds a FIX 5.0 SP2 NoLegs, tag 555, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLegs                                          { get; set; } = ValidateNoLegs;

	/// <summary>Holds a FIX 5.0 SP2 LegCurrency, tag 556, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegCurrency                                     { get; set; } = ValidateLegCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TotNoSecurityTypes, tag 557, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoSecurityTypes                              { get; set; } = ValidateTotNoSecurityTypes;

	/// <summary>Holds a FIX 5.0 SP2 NoSecurityTypes, tag 558, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSecurityTypes                                 { get; set; } = ValidateNoSecurityTypes;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityListRequestType                         { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityRequestResult                           { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 5.0 SP2 RoundLot, tag 561, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RoundLot                                        { get; set; } = ValidateRoundLot;

	/// <summary>Holds a FIX 5.0 SP2 MinTradeVol, tag 562, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinTradeVol                                     { get; set; } = ValidateMinTradeVol;

	/// <summary>Holds a FIX 5.0 SP2 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MultiLegRptTypeReq                              { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 5.0 SP2 LegPositionEffect, tag 564, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LegPositionEffect                               { get; set; } = ValidateLegPositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 LegCoveredOrUncovered, tag 565, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegCoveredOrUncovered                           { get; set; } = ValidateLegCoveredOrUncovered;

	/// <summary>Holds a FIX 5.0 SP2 LegPrice, tag 566, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegPrice                                        { get; set; } = ValidateLegPrice;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradSesStatusRejReason                          { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestID, tag 568, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeRequestID                                  { get; set; } = ValidateTradeRequestID;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeRequestType                                { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 5.0 SP2 PreviouslyReported, tag 570, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PreviouslyReported                              { get; set; } = ValidatePreviouslyReported;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportID, tag 571, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeReportID                                   { get; set; } = ValidateTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportRefID, tag 572, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeReportRefID                                { get; set; } = ValidateTradeReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MatchStatus                                     { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 5.0 SP2 MatchType, tag 574, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MatchType                                       { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 5.0 SP2 OddLot, tag 575, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   OddLot                                          { get; set; } = ValidateOddLot;

	/// <summary>Holds a FIX 5.0 SP2 NoClearingInstructions, tag 576, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoClearingInstructions                          { get; set; } = ValidateNoClearingInstructions;

	/// <summary>Holds a FIX 5.0 SP2 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ClearingInstruction                             { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 5.0 SP2 TradeInputSource, tag 578, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeInputSource                                { get; set; } = ValidateTradeInputSource;

	/// <summary>Holds a FIX 5.0 SP2 TradeInputDevice, tag 579, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeInputDevice                                { get; set; } = ValidateTradeInputDevice;

	/// <summary>Holds a FIX 5.0 SP2 NoDates, tag 580, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDates                                         { get; set; } = ValidateNoDates;

	/// <summary>Holds a FIX 5.0 SP2 AccountType, tag 581, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AccountType                                     { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 5.0 SP2 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CustOrderCapacity                               { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 ClOrdLinkID, tag 583, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ClOrdLinkID                                     { get; set; } = ValidateClOrdLinkID;

	/// <summary>Holds a FIX 5.0 SP2 MassStatusReqID, tag 584, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MassStatusReqID                                 { get; set; } = ValidateMassStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassStatusReqType                               { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 5.0 SP2 OrigOrdModTime, tag 586, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> OrigOrdModTime                                  { get; set; } = ValidateOrigOrdModTime;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlType, tag 587, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LegSettlType                                    { get; set; } = ValidateLegSettlType;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlDate, tag 588, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegSettlDate                                    { get; set; } = ValidateLegSettlDate;

	/// <summary>Holds a FIX 5.0 SP2 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DayBookingInst                                  { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 5.0 SP2 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> BookingUnit                                     { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 5.0 SP2 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> PreallocMethod                                  { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCountryOfIssue, tag 592, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCountryOfIssue                        { get; set; } = ValidateUnderlyingCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStateOrProvinceOfIssue, tag 593, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingStateOrProvinceOfIssue                { get; set; } = ValidateUnderlyingStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLocaleOfIssue, tag 594, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLocaleOfIssue                         { get; set; } = ValidateUnderlyingLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrRegistry, tag 595, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingInstrRegistry                         { get; set; } = ValidateUnderlyingInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 LegCountryOfIssue, tag 596, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegCountryOfIssue                               { get; set; } = ValidateLegCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegStateOrProvinceOfIssue, tag 597, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegStateOrProvinceOfIssue                       { get; set; } = ValidateLegStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegLocaleOfIssue, tag 598, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegLocaleOfIssue                                { get; set; } = ValidateLegLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegInstrRegistry, tag 599, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegInstrRegistry                                { get; set; } = ValidateLegInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 LegSymbol, tag 600, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSymbol                                       { get; set; } = ValidateLegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 LegSymbolSfx, tag 601, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSymbolSfx                                    { get; set; } = ValidateLegSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityID, tag 602, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityID                                   { get; set; } = ValidateLegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityIDSource, tag 603, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityIDSource                             { get; set; } = ValidateLegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoLegSecurityAltID, tag 604, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLegSecurityAltID                              { get; set; } = ValidateNoLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityAltID, tag 605, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityAltID                                { get; set; } = ValidateLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityAltIDSource, tag 606, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityAltIDSource                          { get; set; } = ValidateLegSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 LegProduct, tag 607, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegProduct                                      { get; set; } = ValidateLegProduct;

	/// <summary>Holds a FIX 5.0 SP2 LegCFICode, tag 608, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegCFICode                                      { get; set; } = ValidateLegCFICode;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityType, tag 609, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityType                                 { get; set; } = ValidateLegSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityMonthYear, tag 610, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> LegMaturityMonthYear                            { get; set; } = ValidateLegMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityDate, tag 611, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegMaturityDate                                 { get; set; } = ValidateLegMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 LegStrikePrice, tag 612, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegStrikePrice                                  { get; set; } = ValidateLegStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 LegOptAttribute, tag 613, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LegOptAttribute                                 { get; set; } = ValidateLegOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 LegContractMultiplier, tag 614, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegContractMultiplier                           { get; set; } = ValidateLegContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 LegCouponRate, tag 615, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegCouponRate                                   { get; set; } = ValidateLegCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityExchange, tag 616, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityExchange                             { get; set; } = ValidateLegSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 LegIssuer, tag 617, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegIssuer                                       { get; set; } = ValidateLegIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegIssuerLen, tag 618, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedLegIssuerLen                             { get; set; } = ValidateEncodedLegIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegIssuer, tag 619, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedLegIssuer                                { get; set; } = ValidateEncodedLegIssuer;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityDesc, tag 620, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecurityDesc                                 { get; set; } = ValidateLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegSecurityDescLen, tag 621, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedLegSecurityDescLen                       { get; set; } = ValidateEncodedLegSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegSecurityDesc, tag 622, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedLegSecurityDesc                          { get; set; } = ValidateEncodedLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 LegRatioQty, tag 623, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegRatioQty                                     { get; set; } = ValidateLegRatioQty;

	/// <summary>Holds a FIX 5.0 SP2 LegSide, tag 624, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LegSide                                         { get; set; } = ValidateLegSide;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionSubID, tag 625, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradingSessionSubID                             { get; set; } = ValidateTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 AllocType, tag 626, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocType                                       { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 5.0 SP2 NoHops, tag 627, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoHops                                          { get; set; } = ValidateNoHops;

	/// <summary>Holds a FIX 5.0 SP2 HopCompID, tag 628, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      HopCompID                                       { get; set; } = ValidateHopCompID;

	/// <summary>Holds a FIX 5.0 SP2 HopSendingTime, tag 629, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> HopSendingTime                                  { get; set; } = ValidateHopSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 HopRefID, tag 630, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   HopRefID                                        { get; set; } = ValidateHopRefID;

	/// <summary>Holds a FIX 5.0 SP2 MidPx, tag 631, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MidPx                                           { get; set; } = ValidateMidPx;

	/// <summary>Holds a FIX 5.0 SP2 BidYield, tag 632, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidYield                                        { get; set; } = ValidateBidYield;

	/// <summary>Holds a FIX 5.0 SP2 MidYield, tag 633, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MidYield                                        { get; set; } = ValidateMidYield;

	/// <summary>Holds a FIX 5.0 SP2 OfferYield, tag 634, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferYield                                      { get; set; } = ValidateOfferYield;

	/// <summary>Holds a FIX 5.0 SP2 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ClearingFeeIndicator                            { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 5.0 SP2 WorkingIndicator, tag 636, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   WorkingIndicator                                { get; set; } = ValidateWorkingIndicator;

	/// <summary>Holds a FIX 5.0 SP2 LegLastPx, tag 637, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegLastPx                                       { get; set; } = ValidateLegLastPx;

	/// <summary>Holds a FIX 5.0 SP2 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PriorityIndicator                               { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PriceImprovement, tag 639, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PriceImprovement                                { get; set; } = ValidatePriceImprovement;

	/// <summary>Holds a FIX 5.0 SP2 Price2, tag 640, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Price2                                          { get; set; } = ValidatePrice2;

	/// <summary>Holds a FIX 5.0 SP2 LastForwardPoints2, tag 641, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastForwardPoints2                              { get; set; } = ValidateLastForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 BidForwardPoints2, tag 642, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidForwardPoints2                               { get; set; } = ValidateBidForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 OfferForwardPoints2, tag 643, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferForwardPoints2                             { get; set; } = ValidateOfferForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 RFQReqID, tag 644, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RFQReqID                                        { get; set; } = ValidateRFQReqID;

	/// <summary>Holds a FIX 5.0 SP2 MktBidPx, tag 645, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MktBidPx                                        { get; set; } = ValidateMktBidPx;

	/// <summary>Holds a FIX 5.0 SP2 MktOfferPx, tag 646, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MktOfferPx                                      { get; set; } = ValidateMktOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 MinBidSize, tag 647, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinBidSize                                      { get; set; } = ValidateMinBidSize;

	/// <summary>Holds a FIX 5.0 SP2 MinOfferSize, tag 648, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinOfferSize                                    { get; set; } = ValidateMinOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatusReqID, tag 649, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteStatusReqID                                { get; set; } = ValidateQuoteStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 LegalConfirm, tag 650, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   LegalConfirm                                    { get; set; } = ValidateLegalConfirm;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLastPx, tag 651, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingLastPx                                { get; set; } = ValidateUnderlyingLastPx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLastQty, tag 652, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingLastQty                               { get; set; } = ValidateUnderlyingLastQty;

	/// <summary>Holds a FIX 5.0 SP2 LegRefID, tag 654, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegRefID                                        { get; set; } = ValidateLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 ContraLegRefID, tag 655, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ContraLegRefID                                  { get; set; } = ValidateContraLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrBidFxRate, tag 656, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SettlCurrBidFxRate                              { get; set; } = ValidateSettlCurrBidFxRate;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrOfferFxRate, tag 657, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SettlCurrOfferFxRate                            { get; set; } = ValidateSettlCurrOfferFxRate;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteRequestRejectReason                        { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 SideComplianceID, tag 659, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideComplianceID                                { get; set; } = ValidateSideComplianceID;

	/// <summary>Holds a FIX 5.0 SP2 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AcctIDSource                                    { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 AllocAcctIDSource, tag 661, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocAcctIDSource                               { get; set; } = ValidateAllocAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkPrice, tag 662, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BenchmarkPrice                                  { get; set; } = ValidateBenchmarkPrice;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkPriceType, tag 663, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BenchmarkPriceType                              { get; set; } = ValidateBenchmarkPriceType;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmID, tag 664, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ConfirmID                                       { get; set; } = ValidateConfirmID;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ConfirmStatus                                   { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ConfirmTransType                                { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 5.0 SP2 ContractSettlMonth, tag 667, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> ContractSettlMonth                              { get; set; } = ValidateContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DeliveryForm                                    { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 5.0 SP2 LastParPx, tag 669, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastParPx                                       { get; set; } = ValidateLastParPx;

	/// <summary>Holds a FIX 5.0 SP2 NoLegAllocs, tag 670, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLegAllocs                                     { get; set; } = ValidateNoLegAllocs;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocAccount, tag 671, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegAllocAccount                                 { get; set; } = ValidateLegAllocAccount;

	/// <summary>Holds a FIX 5.0 SP2 LegIndividualAllocID, tag 672, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegIndividualAllocID                            { get; set; } = ValidateLegIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocQty, tag 673, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegAllocQty                                     { get; set; } = ValidateLegAllocQty;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocAcctIDSource, tag 674, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegAllocAcctIDSource                            { get; set; } = ValidateLegAllocAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlCurrency, tag 675, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSettlCurrency                                { get; set; } = ValidateLegSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurveCurrency, tag 676, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegBenchmarkCurveCurrency                       { get; set; } = ValidateLegBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurveName, tag 677, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegBenchmarkCurveName                           { get; set; } = ValidateLegBenchmarkCurveName;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurvePoint, tag 678, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegBenchmarkCurvePoint                          { get; set; } = ValidateLegBenchmarkCurvePoint;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkPrice, tag 679, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegBenchmarkPrice                               { get; set; } = ValidateLegBenchmarkPrice;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkPriceType, tag 680, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegBenchmarkPriceType                           { get; set; } = ValidateLegBenchmarkPriceType;

	/// <summary>Holds a FIX 5.0 SP2 LegBidPx, tag 681, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegBidPx                                        { get; set; } = ValidateLegBidPx;

	/// <summary>Holds a FIX 5.0 SP2 LegIOIQty, tag 682, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegIOIQty                                       { get; set; } = ValidateLegIOIQty;

	/// <summary>Holds a FIX 5.0 SP2 NoLegStipulations, tag 683, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLegStipulations                               { get; set; } = ValidateNoLegStipulations;

	/// <summary>Holds a FIX 5.0 SP2 LegOfferPx, tag 684, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegOfferPx                                      { get; set; } = ValidateLegOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 LegOrderQty, tag 685, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegOrderQty                                     { get; set; } = ValidateLegOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceType, tag 686, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegPriceType                                    { get; set; } = ValidateLegPriceType;

	/// <summary>Holds a FIX 5.0 SP2 LegQty, tag 687, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegQty                                          { get; set; } = ValidateLegQty;

	/// <summary>Holds a FIX 5.0 SP2 LegStipulationType, tag 688, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegStipulationType                              { get; set; } = ValidateLegStipulationType;

	/// <summary>Holds a FIX 5.0 SP2 LegStipulationValue, tag 689, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegStipulationValue                             { get; set; } = ValidateLegStipulationValue;

	/// <summary>Holds a FIX 5.0 SP2 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegSwapType                                     { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 5.0 SP2 Pool, tag 691, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Pool                                            { get; set; } = ValidatePool;

	/// <summary>Holds a FIX 5.0 SP2 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuotePriceType                                  { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRespID, tag 693, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteRespID                                     { get; set; } = ValidateQuoteRespID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteRespType                                   { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteQualifier, tag 695, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> QuoteQualifier                                  { get; set; } = ValidateQuoteQualifier;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionDate, tag 696, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      YieldRedemptionDate                             { get; set; } = ValidateYieldRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionPrice, tag 697, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   YieldRedemptionPrice                            { get; set; } = ValidateYieldRedemptionPrice;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionPriceType, tag 698, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   YieldRedemptionPriceType                        { get; set; } = ValidateYieldRedemptionPriceType;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkSecurityID, tag 699, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BenchmarkSecurityID                             { get; set; } = ValidateBenchmarkSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 ReversalIndicator, tag 700, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ReversalIndicator                               { get; set; } = ValidateReversalIndicator;

	/// <summary>Holds a FIX 5.0 SP2 YieldCalcDate, tag 701, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      YieldCalcDate                                   { get; set; } = ValidateYieldCalcDate;

	/// <summary>Holds a FIX 5.0 SP2 NoPositions, tag 702, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoPositions                                     { get; set; } = ValidateNoPositions;

	/// <summary>Holds a FIX 5.0 SP2 PosType, tag 703, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PosType                                         { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 5.0 SP2 LongQty, tag 704, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LongQty                                         { get; set; } = ValidateLongQty;

	/// <summary>Holds a FIX 5.0 SP2 ShortQty, tag 705, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ShortQty                                        { get; set; } = ValidateShortQty;

	/// <summary>Holds a FIX 5.0 SP2 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosQtyStatus                                    { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 5.0 SP2 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PosAmtType                                      { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 5.0 SP2 PosAmt, tag 708, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PosAmt                                          { get; set; } = ValidatePosAmt;

	/// <summary>Holds a FIX 5.0 SP2 PosTransType, tag 709, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosTransType                                    { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 5.0 SP2 PosReqID, tag 710, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PosReqID                                        { get; set; } = ValidatePosReqID;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyings, tag 711, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUnderlyings                                   { get; set; } = ValidateNoUnderlyings;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosMaintAction                                  { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 5.0 SP2 OrigPosReqRefID, tag 713, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrigPosReqRefID                                 { get; set; } = ValidateOrigPosReqRefID;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintRptRefID, tag 714, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PosMaintRptRefID                                { get; set; } = ValidatePosMaintRptRefID;

	/// <summary>Holds a FIX 5.0 SP2 ClearingBusinessDate, tag 715, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      ClearingBusinessDate                            { get; set; } = ValidateClearingBusinessDate;

	/// <summary>Holds a FIX 5.0 SP2 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlSessID                                     { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 5.0 SP2 SettlSessSubID, tag 717, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlSessSubID                                  { get; set; } = ValidateSettlSessSubID;

	/// <summary>Holds a FIX 5.0 SP2 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AdjustmentType                                  { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 5.0 SP2 ContraryInstructionIndicator, tag 719, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ContraryInstructionIndicator                    { get; set; } = ValidateContraryInstructionIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PriorSpreadIndicator, tag 720, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PriorSpreadIndicator                            { get; set; } = ValidatePriorSpreadIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintRptID, tag 721, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PosMaintRptID                                   { get; set; } = ValidatePosMaintRptID;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosMaintStatus                                  { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosMaintResult                                  { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 5.0 SP2 PosReqType, tag 724, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosReqType                                      { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 5.0 SP2 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ResponseTransportType                           { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 5.0 SP2 ResponseDestination, tag 726, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ResponseDestination                             { get; set; } = ValidateResponseDestination;

	/// <summary>Holds a FIX 5.0 SP2 TotalNumPosReports, tag 727, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotalNumPosReports                              { get; set; } = ValidateTotalNumPosReports;

	/// <summary>Holds a FIX 5.0 SP2 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosReqResult                                    { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 5.0 SP2 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PosReqStatus                                    { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 5.0 SP2 SettlPrice, tag 730, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SettlPrice                                      { get; set; } = ValidateSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlPriceType                                  { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlPrice, tag 732, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingSettlPrice                            { get; set; } = ValidateUnderlyingSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlPriceType, tag 733, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingSettlPriceType                        { get; set; } = ValidateUnderlyingSettlPriceType;

	/// <summary>Holds a FIX 5.0 SP2 PriorSettlPrice, tag 734, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PriorSettlPrice                                 { get; set; } = ValidatePriorSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteQualifiers, tag 735, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoQuoteQualifiers                               { get; set; } = ValidateNoQuoteQualifiers;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlCurrency, tag 736, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocSettlCurrency                              { get; set; } = ValidateAllocSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlCurrAmt, tag 737, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocSettlCurrAmt                               { get; set; } = ValidateAllocSettlCurrAmt;

	/// <summary>Holds a FIX 5.0 SP2 InterestAtMaturity, tag 738, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   InterestAtMaturity                              { get; set; } = ValidateInterestAtMaturity;

	/// <summary>Holds a FIX 5.0 SP2 LegDatedDate, tag 739, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegDatedDate                                    { get; set; } = ValidateLegDatedDate;

	/// <summary>Holds a FIX 5.0 SP2 LegPool, tag 740, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegPool                                         { get; set; } = ValidateLegPool;

	/// <summary>Holds a FIX 5.0 SP2 AllocInterestAtMaturity, tag 741, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocInterestAtMaturity                         { get; set; } = ValidateAllocInterestAtMaturity;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccruedInterestAmt, tag 742, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllocAccruedInterestAmt                         { get; set; } = ValidateAllocAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryDate, tag 743, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DeliveryDate                                    { get; set; } = ValidateDeliveryDate;

	/// <summary>Holds a FIX 5.0 SP2 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> AssignmentMethod                                { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 AssignmentUnit, tag 745, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AssignmentUnit                                  { get; set; } = ValidateAssignmentUnit;

	/// <summary>Holds a FIX 5.0 SP2 OpenInterest, tag 746, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OpenInterest                                    { get; set; } = ValidateOpenInterest;

	/// <summary>Holds a FIX 5.0 SP2 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExerciseMethod                                  { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 5.0 SP2 TotNumTradeReports, tag 748, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNumTradeReports                              { get; set; } = ValidateTotNumTradeReports;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeRequestResult                              { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeRequestStatus                              { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeReportRejectReason                         { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideMultiLegReportingType                       { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 5.0 SP2 NoPosAmt, tag 753, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoPosAmt                                        { get; set; } = ValidateNoPosAmt;

	/// <summary>Holds a FIX 5.0 SP2 AutoAcceptIndicator, tag 754, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   AutoAcceptIndicator                             { get; set; } = ValidateAutoAcceptIndicator;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportID, tag 755, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocReportID                                   { get; set; } = ValidateAllocReportID;

	/// <summary>Holds a FIX 5.0 SP2 NoNested2PartyIDs, tag 756, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested2PartyIDs                               { get; set; } = ValidateNoNested2PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyID, tag 757, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested2PartyID                                  { get; set; } = ValidateNested2PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyIDSource, tag 758, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> Nested2PartyIDSource                            { get; set; } = ValidateNested2PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyRole, tag 759, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested2PartyRole                                { get; set; } = ValidateNested2PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartySubID, tag 760, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested2PartySubID                               { get; set; } = ValidateNested2PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkSecurityIDSource, tag 761, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      BenchmarkSecurityIDSource                       { get; set; } = ValidateBenchmarkSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecuritySubType, tag 762, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecuritySubType                                 { get; set; } = ValidateSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecuritySubType, tag 763, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSecuritySubType                       { get; set; } = ValidateUnderlyingSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 LegSecuritySubType, tag 764, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegSecuritySubType                              { get; set; } = ValidateLegSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessPct, tag 765, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllowableOneSidednessPct                        { get; set; } = ValidateAllowableOneSidednessPct;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessValue, tag 766, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AllowableOneSidednessValue                      { get; set; } = ValidateAllowableOneSidednessValue;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessCurr, tag 767, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllowableOneSidednessCurr                       { get; set; } = ValidateAllowableOneSidednessCurr;

	/// <summary>Holds a FIX 5.0 SP2 NoTrdRegTimestamps, tag 768, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTrdRegTimestamps                              { get; set; } = ValidateNoTrdRegTimestamps;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestamp, tag 769, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TrdRegTimestamp                                 { get; set; } = ValidateTrdRegTimestamp;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TrdRegTimestampType                             { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestampOrigin, tag 771, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TrdRegTimestampOrigin                           { get; set; } = ValidateTrdRegTimestampOrigin;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmRefID, tag 772, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ConfirmRefID                                    { get; set; } = ValidateConfirmRefID;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ConfirmType                                     { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ConfirmRejReason                                { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 5.0 SP2 BookingType, tag 775, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   BookingType                                     { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocRejCode, tag 776, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   IndividualAllocRejCode                          { get; set; } = ValidateIndividualAllocRejCode;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstMsgID, tag 777, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlInstMsgID                                  { get; set; } = ValidateSettlInstMsgID;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlInst, tag 778, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSettlInst                                     { get; set; } = ValidateNoSettlInst;

	/// <summary>Holds a FIX 5.0 SP2 LastUpdateTime, tag 779, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> LastUpdateTime                                  { get; set; } = ValidateLastUpdateTime;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocSettlInstType                              { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlPartyIDs, tag 781, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSettlPartyIDs                                 { get; set; } = ValidateNoSettlPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyID, tag 782, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlPartyID                                    { get; set; } = ValidateSettlPartyID;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyIDSource, tag 783, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlPartyIDSource                              { get; set; } = ValidateSettlPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyRole, tag 784, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlPartyRole                                  { get; set; } = ValidateSettlPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartySubID, tag 785, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlPartySubID                                 { get; set; } = ValidateSettlPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartySubIDType, tag 786, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlPartySubIDType                             { get; set; } = ValidateSettlPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DlvyInstType                                    { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 5.0 SP2 TerminationType, tag 788, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TerminationType                                 { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 5.0 SP2 NextExpectedMsgSeqNum, tag 789, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NextExpectedMsgSeqNum                           { get; set; } = ValidateNextExpectedMsgSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 OrdStatusReqID, tag 790, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrdStatusReqID                                  { get; set; } = ValidateOrdStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstReqID, tag 791, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlInstReqID                                  { get; set; } = ValidateSettlInstReqID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlInstReqRejCode                             { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryAllocID, tag 793, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryAllocID                                { get; set; } = ValidateSecondaryAllocID;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocReportType                                 { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportRefID, tag 795, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocReportRefID                                { get; set; } = ValidateAllocReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocCancReplaceReason                          { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 5.0 SP2 CopyMsgIndicator, tag 797, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   CopyMsgIndicator                                { get; set; } = ValidateCopyMsgIndicator;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocAccountType                                { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 5.0 SP2 OrderAvgPx, tag 799, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderAvgPx                                      { get; set; } = ValidateOrderAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 OrderBookingQty, tag 800, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderBookingQty                                 { get; set; } = ValidateOrderBookingQty;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlPartySubIDs, tag 801, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSettlPartySubIDs                              { get; set; } = ValidateNoSettlPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoPartySubIDs, tag 802, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoPartySubIDs                                   { get; set; } = ValidateNoPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PartySubIDType                                  { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedPartySubIDs, tag 804, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNestedPartySubIDs                             { get; set; } = ValidateNoNestedPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartySubIDType, tag 805, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NestedPartySubIDType                            { get; set; } = ValidateNestedPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 NoNested2PartySubIDs, tag 806, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested2PartySubIDs                            { get; set; } = ValidateNoNested2PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartySubIDType, tag 807, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested2PartySubIDType                           { get; set; } = ValidateNested2PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocIntermedReqType                            { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 5.0 SP2 NoUsernames, tag 809, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUsernames                                     { get; set; } = ValidateNoUsernames;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPx, tag 810, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingPx                                    { get; set; } = ValidateUnderlyingPx;

	/// <summary>Holds a FIX 5.0 SP2 PriceDelta, tag 811, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PriceDelta                                      { get; set; } = ValidatePriceDelta;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueMax, tag 812, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplQueueMax                                    { get; set; } = ValidateApplQueueMax;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueDepth, tag 813, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplQueueDepth                                  { get; set; } = ValidateApplQueueDepth;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplQueueResolution                             { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplQueueAction                                 { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 5.0 SP2 NoAltMDSource, tag 816, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoAltMDSource                                   { get; set; } = ValidateNoAltMDSource;

	/// <summary>Holds a FIX 5.0 SP2 AltMDSourceID, tag 817, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AltMDSourceID                                   { get; set; } = ValidateAltMDSourceID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeReportID, tag 818, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryTradeReportID                          { get; set; } = ValidateSecondaryTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AvgPxIndicator                                  { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradeLinkID, tag 820, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeLinkID                                     { get; set; } = ValidateTradeLinkID;

	/// <summary>Holds a FIX 5.0 SP2 OrderInputDevice, tag 821, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrderInputDevice                                { get; set; } = ValidateOrderInputDevice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTradingSessionID, tag 822, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingTradingSessionID                      { get; set; } = ValidateUnderlyingTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTradingSessionSubID, tag 823, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingTradingSessionSubID                   { get; set; } = ValidateUnderlyingTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 TradeLegRefID, tag 824, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeLegRefID                                   { get; set; } = ValidateTradeLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeRule, tag 825, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ExchangeRule                                    { get; set; } = ValidateExchangeRule;

	/// <summary>Holds a FIX 5.0 SP2 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeAllocIndicator                             { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 5.0 SP2 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ExpirationCycle                                 { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 5.0 SP2 TrdType, tag 828, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TrdType                                         { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 5.0 SP2 TrdSubType, tag 829, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TrdSubType                                      { get; set; } = ValidateTrdSubType;

	/// <summary>Holds a FIX 5.0 SP2 TransferReason, tag 830, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TransferReason                                  { get; set; } = ValidateTransferReason;

	/// <summary>Holds a FIX 5.0 SP2 TotNumAssignmentReports, tag 832, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNumAssignmentReports                         { get; set; } = ValidateTotNumAssignmentReports;

	/// <summary>Holds a FIX 5.0 SP2 AsgnRptID, tag 833, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AsgnRptID                                       { get; set; } = ValidateAsgnRptID;

	/// <summary>Holds a FIX 5.0 SP2 ThresholdAmount, tag 834, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ThresholdAmount                                 { get; set; } = ValidateThresholdAmount;

	/// <summary>Holds a FIX 5.0 SP2 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegMoveType                                     { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 5.0 SP2 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegOffsetType                                   { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 5.0 SP2 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegLimitType                                    { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 5.0 SP2 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegRoundDirection                               { get; set; } = ValidatePegRoundDirection;

	/// <summary>Holds a FIX 5.0 SP2 PeggedPrice, tag 839, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PeggedPrice                                     { get; set; } = ValidatePeggedPrice;

	/// <summary>Holds a FIX 5.0 SP2 PegScope, tag 840, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegScope                                        { get; set; } = ValidatePegScope;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionMoveType, tag 841, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DiscretionMoveType                              { get; set; } = ValidateDiscretionMoveType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionOffsetType, tag 842, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DiscretionOffsetType                            { get; set; } = ValidateDiscretionOffsetType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionLimitType, tag 843, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DiscretionLimitType                             { get; set; } = ValidateDiscretionLimitType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionRoundDirection, tag 844, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DiscretionRoundDirection                        { get; set; } = ValidateDiscretionRoundDirection;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionPrice, tag 845, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DiscretionPrice                                 { get; set; } = ValidateDiscretionPrice;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DiscretionScope                                 { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TargetStrategy                                  { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategyParameters, tag 848, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TargetStrategyParameters                        { get; set; } = ValidateTargetStrategyParameters;

	/// <summary>Holds a FIX 5.0 SP2 ParticipationRate, tag 849, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ParticipationRate                               { get; set; } = ValidateParticipationRate;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategyPerformance, tag 850, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TargetStrategyPerformance                       { get; set; } = ValidateTargetStrategyPerformance;

	/// <summary>Holds a FIX 5.0 SP2 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LastLiquidityInd                                { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 PublishTrdIndicator, tag 852, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PublishTrdIndicator                             { get; set; } = ValidatePublishTrdIndicator;

	/// <summary>Holds a FIX 5.0 SP2 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ShortSaleReason                                 { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 5.0 SP2 QtyType, tag 854, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QtyType                                         { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTrdType, tag 855, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecondaryTrdType                                { get; set; } = ValidateSecondaryTrdType;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradeReportType                                 { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 5.0 SP2 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocNoOrdersType                               { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 5.0 SP2 SharedCommission, tag 858, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SharedCommission                                { get; set; } = ValidateSharedCommission;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmReqID, tag 859, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ConfirmReqID                                    { get; set; } = ValidateConfirmReqID;

	/// <summary>Holds a FIX 5.0 SP2 AvgParPx, tag 860, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AvgParPx                                        { get; set; } = ValidateAvgParPx;

	/// <summary>Holds a FIX 5.0 SP2 ReportedPx, tag 861, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ReportedPx                                      { get; set; } = ValidateReportedPx;

	/// <summary>Holds a FIX 5.0 SP2 NoCapacities, tag 862, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoCapacities                                    { get; set; } = ValidateNoCapacities;

	/// <summary>Holds a FIX 5.0 SP2 OrderCapacityQty, tag 863, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OrderCapacityQty                                { get; set; } = ValidateOrderCapacityQty;

	/// <summary>Holds a FIX 5.0 SP2 NoEvents, tag 864, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoEvents                                        { get; set; } = ValidateNoEvents;

	/// <summary>Holds a FIX 5.0 SP2 EventType, tag 865, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EventType                                       { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 5.0 SP2 EventDate, tag 866, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      EventDate                                       { get; set; } = ValidateEventDate;

	/// <summary>Holds a FIX 5.0 SP2 EventPx, tag 867, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EventPx                                         { get; set; } = ValidateEventPx;

	/// <summary>Holds a FIX 5.0 SP2 EventText, tag 868, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      EventText                                       { get; set; } = ValidateEventText;

	/// <summary>Holds a FIX 5.0 SP2 PctAtRisk, tag 869, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PctAtRisk                                       { get; set; } = ValidatePctAtRisk;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrAttrib, tag 870, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoInstrAttrib                                   { get; set; } = ValidateNoInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   InstrAttribType                                 { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 InstrAttribValue, tag 872, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InstrAttribValue                                { get; set; } = ValidateInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 DatedDate, tag 873, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DatedDate                                       { get; set; } = ValidateDatedDate;

	/// <summary>Holds a FIX 5.0 SP2 InterestAccrualDate, tag 874, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      InterestAccrualDate                             { get; set; } = ValidateInterestAccrualDate;

	/// <summary>Holds a FIX 5.0 SP2 CPProgram, tag 875, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CPProgram                                       { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 5.0 SP2 CPRegType, tag 876, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CPRegType                                       { get; set; } = ValidateCPRegType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCPProgram, tag 877, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCPProgram                             { get; set; } = ValidateUnderlyingCPProgram;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCPRegType, tag 878, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCPRegType                             { get; set; } = ValidateUnderlyingCPRegType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingQty, tag 879, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingQty                                   { get; set; } = ValidateUnderlyingQty;

	/// <summary>Holds a FIX 5.0 SP2 TrdMatchID, tag 880, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TrdMatchID                                      { get; set; } = ValidateTrdMatchID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeReportRefID, tag 881, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryTradeReportRefID                       { get; set; } = ValidateSecondaryTradeReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDirtyPrice, tag 882, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingDirtyPrice                            { get; set; } = ValidateUnderlyingDirtyPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingEndPrice, tag 883, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingEndPrice                              { get; set; } = ValidateUnderlyingEndPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStartValue, tag 884, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingStartValue                            { get; set; } = ValidateUnderlyingStartValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCurrentValue, tag 885, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingCurrentValue                          { get; set; } = ValidateUnderlyingCurrentValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingEndValue, tag 886, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingEndValue                              { get; set; } = ValidateUnderlyingEndValue;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingStips, tag 887, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUnderlyingStips                               { get; set; } = ValidateNoUnderlyingStips;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStipType, tag 888, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingStipType                              { get; set; } = ValidateUnderlyingStipType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStipValue, tag 889, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingStipValue                             { get; set; } = ValidateUnderlyingStipValue;

	/// <summary>Holds a FIX 5.0 SP2 MaturityNetMoney, tag 890, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MaturityNetMoney                                { get; set; } = ValidateMaturityNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MiscFeeBasis                                    { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 5.0 SP2 TotNoAllocs, tag 892, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoAllocs                                     { get; set; } = ValidateTotNoAllocs;

	/// <summary>Holds a FIX 5.0 SP2 LastFragment, tag 893, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   LastFragment                                    { get; set; } = ValidateLastFragment;

	/// <summary>Holds a FIX 5.0 SP2 CollReqID, tag 894, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollReqID                                       { get; set; } = ValidateCollReqID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollAsgnReason                                  { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollInquiryQualifier                            { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 5.0 SP2 NoTrades, tag 897, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTrades                                        { get; set; } = ValidateNoTrades;

	/// <summary>Holds a FIX 5.0 SP2 MarginRatio, tag 898, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MarginRatio                                     { get; set; } = ValidateMarginRatio;

	/// <summary>Holds a FIX 5.0 SP2 MarginExcess, tag 899, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MarginExcess                                    { get; set; } = ValidateMarginExcess;

	/// <summary>Holds a FIX 5.0 SP2 TotalNetValue, tag 900, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TotalNetValue                                   { get; set; } = ValidateTotalNetValue;

	/// <summary>Holds a FIX 5.0 SP2 CashOutstanding, tag 901, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CashOutstanding                                 { get; set; } = ValidateCashOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnID, tag 902, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollAsgnID                                      { get; set; } = ValidateCollAsgnID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollAsgnTransType                               { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 5.0 SP2 CollRespID, tag 904, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollRespID                                      { get; set; } = ValidateCollRespID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollAsgnRespType                                { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollAsgnRejectReason                            { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRefID, tag 907, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollAsgnRefID                                   { get; set; } = ValidateCollAsgnRefID;

	/// <summary>Holds a FIX 5.0 SP2 CollRptID, tag 908, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollRptID                                       { get; set; } = ValidateCollRptID;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryID, tag 909, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CollInquiryID                                   { get; set; } = ValidateCollInquiryID;

	/// <summary>Holds a FIX 5.0 SP2 CollStatus, tag 910, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollStatus                                      { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 5.0 SP2 TotNumReports, tag 911, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNumReports                                   { get; set; } = ValidateTotNumReports;

	/// <summary>Holds a FIX 5.0 SP2 LastRptRequested, tag 912, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   LastRptRequested                                { get; set; } = ValidateLastRptRequested;

	/// <summary>Holds a FIX 5.0 SP2 AgreementDesc, tag 913, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AgreementDesc                                   { get; set; } = ValidateAgreementDesc;

	/// <summary>Holds a FIX 5.0 SP2 AgreementID, tag 914, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AgreementID                                     { get; set; } = ValidateAgreementID;

	/// <summary>Holds a FIX 5.0 SP2 AgreementDate, tag 915, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      AgreementDate                                   { get; set; } = ValidateAgreementDate;

	/// <summary>Holds a FIX 5.0 SP2 StartDate, tag 916, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      StartDate                                       { get; set; } = ValidateStartDate;

	/// <summary>Holds a FIX 5.0 SP2 EndDate, tag 917, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      EndDate                                         { get; set; } = ValidateEndDate;

	/// <summary>Holds a FIX 5.0 SP2 AgreementCurrency, tag 918, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AgreementCurrency                               { get; set; } = ValidateAgreementCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DeliveryType                                    { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 5.0 SP2 EndAccruedInterestAmt, tag 920, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EndAccruedInterestAmt                           { get; set; } = ValidateEndAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 StartCash, tag 921, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StartCash                                       { get; set; } = ValidateStartCash;

	/// <summary>Holds a FIX 5.0 SP2 EndCash, tag 922, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EndCash                                         { get; set; } = ValidateEndCash;

	/// <summary>Holds a FIX 5.0 SP2 UserRequestID, tag 923, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UserRequestID                                   { get; set; } = ValidateUserRequestID;

	/// <summary>Holds a FIX 5.0 SP2 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UserRequestType                                 { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 5.0 SP2 NewPassword, tag 925, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NewPassword                                     { get; set; } = ValidateNewPassword;

	/// <summary>Holds a FIX 5.0 SP2 UserStatus, tag 926, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UserStatus                                      { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 5.0 SP2 UserStatusText, tag 927, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UserStatusText                                  { get; set; } = ValidateUserStatusText;

	/// <summary>Holds a FIX 5.0 SP2 StatusValue, tag 928, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StatusValue                                     { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 5.0 SP2 StatusText, tag 929, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StatusText                                      { get; set; } = ValidateStatusText;

	/// <summary>Holds a FIX 5.0 SP2 RefCompID, tag 930, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefCompID                                       { get; set; } = ValidateRefCompID;

	/// <summary>Holds a FIX 5.0 SP2 RefSubID, tag 931, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefSubID                                        { get; set; } = ValidateRefSubID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkResponseID, tag 932, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NetworkResponseID                               { get; set; } = ValidateNetworkResponseID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkRequestID, tag 933, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NetworkRequestID                                { get; set; } = ValidateNetworkRequestID;

	/// <summary>Holds a FIX 5.0 SP2 LastNetworkResponseID, tag 934, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LastNetworkResponseID                           { get; set; } = ValidateLastNetworkResponseID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NetworkRequestType                              { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 5.0 SP2 NoCompIDs, tag 936, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoCompIDs                                       { get; set; } = ValidateNoCompIDs;

	/// <summary>Holds a FIX 5.0 SP2 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NetworkStatusResponseType                       { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 5.0 SP2 NoCollInquiryQualifier, tag 938, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoCollInquiryQualifier                          { get; set; } = ValidateNoCollInquiryQualifier;

	/// <summary>Holds a FIX 5.0 SP2 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TrdRptStatus                                    { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 5.0 SP2 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AffirmStatus                                    { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStrikeCurrency, tag 941, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingStrikeCurrency                        { get; set; } = ValidateUnderlyingStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegStrikeCurrency, tag 942, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegStrikeCurrency                               { get; set; } = ValidateLegStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TimeBracket, tag 943, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TimeBracket                                     { get; set; } = ValidateTimeBracket;

	/// <summary>Holds a FIX 5.0 SP2 CollAction, tag 944, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollAction                                      { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollInquiryStatus                               { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollInquiryResult                               { get; set; } = ValidateCollInquiryResult;

	/// <summary>Holds a FIX 5.0 SP2 StrikeCurrency, tag 947, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StrikeCurrency                                  { get; set; } = ValidateStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 NoNested3PartyIDs, tag 948, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested3PartyIDs                               { get; set; } = ValidateNoNested3PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyID, tag 949, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested3PartyID                                  { get; set; } = ValidateNested3PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyIDSource, tag 950, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> Nested3PartyIDSource                            { get; set; } = ValidateNested3PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyRole, tag 951, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested3PartyRole                                { get; set; } = ValidateNested3PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoNested3PartySubIDs, tag 952, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested3PartySubIDs                            { get; set; } = ValidateNoNested3PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartySubID, tag 953, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested3PartySubID                               { get; set; } = ValidateNested3PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartySubIDType, tag 954, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested3PartySubIDType                           { get; set; } = ValidateNested3PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 LegContractSettlMonth, tag 955, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> LegContractSettlMonth                           { get; set; } = ValidateLegContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 LegInterestAccrualDate, tag 956, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      LegInterestAccrualDate                          { get; set; } = ValidateLegInterestAccrualDate;

	/// <summary>Holds a FIX 5.0 SP2 NoStrategyParameters, tag 957, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoStrategyParameters                            { get; set; } = ValidateNoStrategyParameters;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterName, tag 958, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StrategyParameterName                           { get; set; } = ValidateStrategyParameterName;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterType, tag 959, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StrategyParameterType                           { get; set; } = ValidateStrategyParameterType;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterValue, tag 960, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StrategyParameterValue                          { get; set; } = ValidateStrategyParameterValue;

	/// <summary>Holds a FIX 5.0 SP2 HostCrossID, tag 961, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      HostCrossID                                     { get; set; } = ValidateHostCrossID;

	/// <summary>Holds a FIX 5.0 SP2 SideTimeInForce, tag 962, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> SideTimeInForce                                 { get; set; } = ValidateSideTimeInForce;

	/// <summary>Holds a FIX 5.0 SP2 MDReportID, tag 963, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDReportID                                      { get; set; } = ValidateMDReportID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityReportID, tag 964, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityReportID                                { get; set; } = ValidateSecurityReportID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatus, tag 965, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityStatus                                  { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 5.0 SP2 SettleOnOpenFlag, tag 966, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettleOnOpenFlag                                { get; set; } = ValidateSettleOnOpenFlag;

	/// <summary>Holds a FIX 5.0 SP2 StrikeMultiplier, tag 967, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StrikeMultiplier                                { get; set; } = ValidateStrikeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 StrikeValue, tag 968, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StrikeValue                                     { get; set; } = ValidateStrikeValue;

	/// <summary>Holds a FIX 5.0 SP2 MinPriceIncrement, tag 969, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinPriceIncrement                               { get; set; } = ValidateMinPriceIncrement;

	/// <summary>Holds a FIX 5.0 SP2 PositionLimit, tag 970, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PositionLimit                                   { get; set; } = ValidatePositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 NTPositionLimit, tag 971, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NTPositionLimit                                 { get; set; } = ValidateNTPositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAllocationPercent, tag 972, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingAllocationPercent                     { get; set; } = ValidateUnderlyingAllocationPercent;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCashAmount, tag 973, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingCashAmount                            { get; set; } = ValidateUnderlyingCashAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCashType, tag 974, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingCashType                              { get; set; } = ValidateUnderlyingCashType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementType, tag 975, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingSettlementType                        { get; set; } = ValidateUnderlyingSettlementType;

	/// <summary>Holds a FIX 5.0 SP2 QuantityDate, tag 976, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      QuantityDate                                    { get; set; } = ValidateQuantityDate;

	/// <summary>Holds a FIX 5.0 SP2 ContIntRptID, tag 977, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ContIntRptID                                    { get; set; } = ValidateContIntRptID;

	/// <summary>Holds a FIX 5.0 SP2 LateIndicator, tag 978, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   LateIndicator                                   { get; set; } = ValidateLateIndicator;

	/// <summary>Holds a FIX 5.0 SP2 InputSource, tag 979, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InputSource                                     { get; set; } = ValidateInputSource;

	/// <summary>Holds a FIX 5.0 SP2 SecurityUpdateAction, tag 980, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SecurityUpdateAction                            { get; set; } = ValidateSecurityUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 NoExpiration, tag 981, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoExpiration                                    { get; set; } = ValidateNoExpiration;

	/// <summary>Holds a FIX 5.0 SP2 ExpirationQtyType, tag 982, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ExpirationQtyType                               { get; set; } = ValidateExpirationQtyType;

	/// <summary>Holds a FIX 5.0 SP2 ExpQty, tag 983, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ExpQty                                          { get; set; } = ValidateExpQty;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingAmounts, tag 984, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUnderlyingAmounts                             { get; set; } = ValidateNoUnderlyingAmounts;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPayAmount, tag 985, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingPayAmount                             { get; set; } = ValidateUnderlyingPayAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCollectAmount, tag 986, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingCollectAmount                         { get; set; } = ValidateUnderlyingCollectAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementDate, tag 987, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingSettlementDate                        { get; set; } = ValidateUnderlyingSettlementDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementStatus, tag 988, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSettlementStatus                      { get; set; } = ValidateUnderlyingSettlementStatus;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryIndividualAllocID, tag 989, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryIndividualAllocID                      { get; set; } = ValidateSecondaryIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegReportID, tag 990, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegReportID                                     { get; set; } = ValidateLegReportID;

	/// <summary>Holds a FIX 5.0 SP2 RndPx, tag 991, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RndPx                                           { get; set; } = ValidateRndPx;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocType, tag 992, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   IndividualAllocType                             { get; set; } = ValidateIndividualAllocType;

	/// <summary>Holds a FIX 5.0 SP2 AllocCustomerCapacity, tag 993, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocCustomerCapacity                           { get; set; } = ValidateAllocCustomerCapacity;

	/// <summary>Holds a FIX 5.0 SP2 TierCode, tag 994, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TierCode                                        { get; set; } = ValidateTierCode;

	/// <summary>Holds a FIX 5.0 SP2 UnitOfMeasure, tag 996, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnitOfMeasure                                   { get; set; } = ValidateUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 TimeUnit, tag 997, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TimeUnit                                        { get; set; } = ValidateTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingUnitOfMeasure, tag 998, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingUnitOfMeasure                         { get; set; } = ValidateUnderlyingUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 LegUnitOfMeasure, tag 999, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegUnitOfMeasure                                { get; set; } = ValidateLegUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTimeUnit, tag 1000, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingTimeUnit                              { get; set; } = ValidateUnderlyingTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 LegTimeUnit, tag 1001, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegTimeUnit                                     { get; set; } = ValidateLegTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 AllocMethod, tag 1002, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   AllocMethod                                     { get; set; } = ValidateAllocMethod;

	/// <summary>Holds a FIX 5.0 SP2 TradeID, tag 1003, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradeID                                         { get; set; } = ValidateTradeID;

	/// <summary>Holds a FIX 5.0 SP2 SideTradeReportID, tag 1005, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideTradeReportID                               { get; set; } = ValidateSideTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 SideFillStationCd, tag 1006, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideFillStationCd                               { get; set; } = ValidateSideFillStationCd;

	/// <summary>Holds a FIX 5.0 SP2 SideReasonCd, tag 1007, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideReasonCd                                    { get; set; } = ValidateSideReasonCd;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdSubTyp, tag 1008, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideTrdSubTyp                                   { get; set; } = ValidateSideTrdSubTyp;

	/// <summary>Holds a FIX 5.0 SP2 SideLastQty, tag 1009, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideLastQty                                     { get; set; } = ValidateSideLastQty;

	/// <summary>Holds a FIX 5.0 SP2 MessageEventSource, tag 1011, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MessageEventSource                              { get; set; } = ValidateMessageEventSource;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestamp, tag 1012, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> SideTrdRegTimestamp                             { get; set; } = ValidateSideTrdRegTimestamp;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestampType, tag 1013, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideTrdRegTimestampType                         { get; set; } = ValidateSideTrdRegTimestampType;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestampSrc, tag 1014, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideTrdRegTimestampSrc                          { get; set; } = ValidateSideTrdRegTimestampSrc;

	/// <summary>Holds a FIX 5.0 SP2 AsOfIndicator, tag 1015, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> AsOfIndicator                                   { get; set; } = ValidateAsOfIndicator;

	/// <summary>Holds a FIX 5.0 SP2 NoSideTrdRegTS, tag 1016, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSideTrdRegTS                                  { get; set; } = ValidateNoSideTrdRegTS;

	/// <summary>Holds a FIX 5.0 SP2 LegOptionRatio, tag 1017, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegOptionRatio                                  { get; set; } = ValidateLegOptionRatio;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrumentParties, tag 1018, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoInstrumentParties                             { get; set; } = ValidateNoInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyID, tag 1019, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InstrumentPartyID                               { get; set; } = ValidateInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 TradeVolume, tag 1020, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TradeVolume                                     { get; set; } = ValidateTradeVolume;

	/// <summary>Holds a FIX 5.0 SP2 MDBookType, tag 1021, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDBookType                                      { get; set; } = ValidateMDBookType;

	/// <summary>Holds a FIX 5.0 SP2 MDFeedType, tag 1022, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDFeedType                                      { get; set; } = ValidateMDFeedType;

	/// <summary>Holds a FIX 5.0 SP2 MDPriceLevel, tag 1023, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDPriceLevel                                    { get; set; } = ValidateMDPriceLevel;

	/// <summary>Holds a FIX 5.0 SP2 MDOriginType, tag 1024, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDOriginType                                    { get; set; } = ValidateMDOriginType;

	/// <summary>Holds a FIX 5.0 SP2 FirstPx, tag 1025, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FirstPx                                         { get; set; } = ValidateFirstPx;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySpotRate, tag 1026, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MDEntrySpotRate                                 { get; set; } = ValidateMDEntrySpotRate;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryForwardPoints, tag 1027, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MDEntryForwardPoints                            { get; set; } = ValidateMDEntryForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 ManualOrderIndicator, tag 1028, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ManualOrderIndicator                            { get; set; } = ValidateManualOrderIndicator;

	/// <summary>Holds a FIX 5.0 SP2 CustDirectedOrder, tag 1029, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   CustDirectedOrder                               { get; set; } = ValidateCustDirectedOrder;

	/// <summary>Holds a FIX 5.0 SP2 ReceivedDeptID, tag 1030, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ReceivedDeptID                                  { get; set; } = ValidateReceivedDeptID;

	/// <summary>Holds a FIX 5.0 SP2 CustOrderHandlingInst, tag 1031, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  CustOrderHandlingInst                           { get; set; } = ValidateCustOrderHandlingInst;

	/// <summary>Holds a FIX 5.0 SP2 OrderHandlingInstSource, tag 1032, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OrderHandlingInstSource                         { get; set; } = ValidateOrderHandlingInstSource;

	/// <summary>Holds a FIX 5.0 SP2 DeskType, tag 1033, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DeskType                                        { get; set; } = ValidateDeskType;

	/// <summary>Holds a FIX 5.0 SP2 DeskTypeSource, tag 1034, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DeskTypeSource                                  { get; set; } = ValidateDeskTypeSource;

	/// <summary>Holds a FIX 5.0 SP2 DeskOrderHandlingInst, tag 1035, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  DeskOrderHandlingInst                           { get; set; } = ValidateDeskOrderHandlingInst;

	/// <summary>Holds a FIX 5.0 SP2 ExecAckStatus, tag 1036, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExecAckStatus                                   { get; set; } = ValidateExecAckStatus;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDeliveryAmount, tag 1037, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingDeliveryAmount                        { get; set; } = ValidateUnderlyingDeliveryAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCapValue, tag 1038, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingCapValue                              { get; set; } = ValidateUnderlyingCapValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlMethod, tag 1039, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSettlMethod                           { get; set; } = ValidateUnderlyingSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeID, tag 1040, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryTradeID                                { get; set; } = ValidateSecondaryTradeID;

	/// <summary>Holds a FIX 5.0 SP2 FirmTradeID, tag 1041, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      FirmTradeID                                     { get; set; } = ValidateFirmTradeID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryFirmTradeID, tag 1042, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecondaryFirmTradeID                            { get; set; } = ValidateSecondaryFirmTradeID;

	/// <summary>Holds a FIX 5.0 SP2 CollApplType, tag 1043, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   CollApplType                                    { get; set; } = ValidateCollApplType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAdjustedQuantity, tag 1044, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingAdjustedQuantity                      { get; set; } = ValidateUnderlyingAdjustedQuantity;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFXRate, tag 1045, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingFXRate                                { get; set; } = ValidateUnderlyingFXRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFXRateCalc, tag 1046, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> UnderlyingFXRateCalc                            { get; set; } = ValidateUnderlyingFXRateCalc;

	/// <summary>Holds a FIX 5.0 SP2 AllocPositionEffect, tag 1047, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> AllocPositionEffect                             { get; set; } = ValidateAllocPositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 DealingCapacity, tag 1048, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DealingCapacity                                 { get; set; } = ValidateDealingCapacity;

	/// <summary>Holds a FIX 5.0 SP2 InstrmtAssignmentMethod, tag 1049, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> InstrmtAssignmentMethod                         { get; set; } = ValidateInstrmtAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyIDSource, tag 1050, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> InstrumentPartyIDSource                         { get; set; } = ValidateInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyRole, tag 1051, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   InstrumentPartyRole                             { get; set; } = ValidateInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrumentPartySubIDs, tag 1052, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoInstrumentPartySubIDs                         { get; set; } = ValidateNoInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartySubID, tag 1053, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      InstrumentPartySubID                            { get; set; } = ValidateInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartySubIDType, tag 1054, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   InstrumentPartySubIDType                        { get; set; } = ValidateInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 PositionCurrency, tag 1055, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PositionCurrency                                { get; set; } = ValidatePositionCurrency;

	/// <summary>Holds a FIX 5.0 SP2 CalculatedCcyLastQty, tag 1056, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CalculatedCcyLastQty                            { get; set; } = ValidateCalculatedCcyLastQty;

	/// <summary>Holds a FIX 5.0 SP2 AggressorIndicator, tag 1057, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   AggressorIndicator                              { get; set; } = ValidateAggressorIndicator;

	/// <summary>Holds a FIX 5.0 SP2 NoUndlyInstrumentParties, tag 1058, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUndlyInstrumentParties                        { get; set; } = ValidateNoUndlyInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyID, tag 1059, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingInstrumentPartyID                     { get; set; } = ValidateUnderlyingInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyIDSource, tag 1060, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> UnderlyingInstrumentPartyIDSource               { get; set; } = ValidateUnderlyingInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyRole, tag 1061, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingInstrumentPartyRole                   { get; set; } = ValidateUnderlyingInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoUndlyInstrumentPartySubIDs, tag 1062, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUndlyInstrumentPartySubIDs                    { get; set; } = ValidateNoUndlyInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartySubID, tag 1063, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingInstrumentPartySubID                  { get; set; } = ValidateUnderlyingInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartySubIDType, tag 1064, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingInstrumentPartySubIDType              { get; set; } = ValidateUnderlyingInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 BidSwapPoints, tag 1065, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   BidSwapPoints                                   { get; set; } = ValidateBidSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 OfferSwapPoints, tag 1066, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OfferSwapPoints                                 { get; set; } = ValidateOfferSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegBidForwardPoints, tag 1067, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegBidForwardPoints                             { get; set; } = ValidateLegBidForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegOfferForwardPoints, tag 1068, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegOfferForwardPoints                           { get; set; } = ValidateLegOfferForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 SwapPoints, tag 1069, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SwapPoints                                      { get; set; } = ValidateSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 MDQuoteType, tag 1070, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDQuoteType                                     { get; set; } = ValidateMDQuoteType;

	/// <summary>Holds a FIX 5.0 SP2 LastSwapPoints, tag 1071, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LastSwapPoints                                  { get; set; } = ValidateLastSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 SideGrossTradeAmt, tag 1072, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SideGrossTradeAmt                               { get; set; } = ValidateSideGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 LegLastForwardPoints, tag 1073, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegLastForwardPoints                            { get; set; } = ValidateLegLastForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegCalculatedCcyLastQty, tag 1074, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegCalculatedCcyLastQty                         { get; set; } = ValidateLegCalculatedCcyLastQty;

	/// <summary>Holds a FIX 5.0 SP2 LegGrossTradeAmt, tag 1075, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegGrossTradeAmt                                { get; set; } = ValidateLegGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 MaturityTime, tag 1079, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.ZonedTime> MaturityTime                                    { get; set; } = ValidateMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 RefOrderID, tag 1080, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefOrderID                                      { get; set; } = ValidateRefOrderID;

	/// <summary>Holds a FIX 5.0 SP2 RefOrderIDSource, tag 1081, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> RefOrderIDSource                                { get; set; } = ValidateRefOrderIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryDisplayQty, tag 1082, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SecondaryDisplayQty                             { get; set; } = ValidateSecondaryDisplayQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayWhen, tag 1083, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DisplayWhen                                     { get; set; } = ValidateDisplayWhen;

	/// <summary>Holds a FIX 5.0 SP2 DisplayMethod, tag 1084, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DisplayMethod                                   { get; set; } = ValidateDisplayMethod;

	/// <summary>Holds a FIX 5.0 SP2 DisplayLowQty, tag 1085, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DisplayLowQty                                   { get; set; } = ValidateDisplayLowQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayHighQty, tag 1086, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DisplayHighQty                                  { get; set; } = ValidateDisplayHighQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayMinIncr, tag 1087, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DisplayMinIncr                                  { get; set; } = ValidateDisplayMinIncr;

	/// <summary>Holds a FIX 5.0 SP2 RefreshQty, tag 1088, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RefreshQty                                      { get; set; } = ValidateRefreshQty;

	/// <summary>Holds a FIX 5.0 SP2 MatchIncrement, tag 1089, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MatchIncrement                                  { get; set; } = ValidateMatchIncrement;

	/// <summary>Holds a FIX 5.0 SP2 MaxPriceLevels, tag 1090, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MaxPriceLevels                                  { get; set; } = ValidateMaxPriceLevels;

	/// <summary>Holds a FIX 5.0 SP2 PreTradeAnonymity, tag 1091, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PreTradeAnonymity                               { get; set; } = ValidatePreTradeAnonymity;

	/// <summary>Holds a FIX 5.0 SP2 PriceProtectionScope, tag 1092, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> PriceProtectionScope                            { get; set; } = ValidatePriceProtectionScope;

	/// <summary>Holds a FIX 5.0 SP2 LotType, tag 1093, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> LotType                                         { get; set; } = ValidateLotType;

	/// <summary>Holds a FIX 5.0 SP2 PegPriceType, tag 1094, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PegPriceType                                    { get; set; } = ValidatePegPriceType;

	/// <summary>Holds a FIX 5.0 SP2 PeggedRefPrice, tag 1095, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PeggedRefPrice                                  { get; set; } = ValidatePeggedRefPrice;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityIDSource, tag 1096, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PegSecurityIDSource                             { get; set; } = ValidatePegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityID, tag 1097, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PegSecurityID                                   { get; set; } = ValidatePegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 PegSymbol, tag 1098, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PegSymbol                                       { get; set; } = ValidatePegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityDesc, tag 1099, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PegSecurityDesc                                 { get; set; } = ValidatePegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 TriggerType, tag 1100, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerType                                     { get; set; } = ValidateTriggerType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerAction, tag 1101, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerAction                                   { get; set; } = ValidateTriggerAction;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPrice, tag 1102, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TriggerPrice                                    { get; set; } = ValidateTriggerPrice;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSymbol, tag 1103, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerSymbol                                   { get; set; } = ValidateTriggerSymbol;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityID, tag 1104, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerSecurityID                               { get; set; } = ValidateTriggerSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityIDSource, tag 1105, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerSecurityIDSource                         { get; set; } = ValidateTriggerSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityDesc, tag 1106, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerSecurityDesc                             { get; set; } = ValidateTriggerSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceType, tag 1107, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerPriceType                                { get; set; } = ValidateTriggerPriceType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceTypeScope, tag 1108, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerPriceTypeScope                           { get; set; } = ValidateTriggerPriceTypeScope;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceDirection, tag 1109, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerPriceDirection                           { get; set; } = ValidateTriggerPriceDirection;

	/// <summary>Holds a FIX 5.0 SP2 TriggerNewPrice, tag 1110, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TriggerNewPrice                                 { get; set; } = ValidateTriggerNewPrice;

	/// <summary>Holds a FIX 5.0 SP2 TriggerOrderType, tag 1111, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TriggerOrderType                                { get; set; } = ValidateTriggerOrderType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerNewQty, tag 1112, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TriggerNewQty                                   { get; set; } = ValidateTriggerNewQty;

	/// <summary>Holds a FIX 5.0 SP2 TriggerTradingSessionID, tag 1113, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerTradingSessionID                         { get; set; } = ValidateTriggerTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 TriggerTradingSessionSubID, tag 1114, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TriggerTradingSessionSubID                      { get; set; } = ValidateTriggerTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 OrderCategory, tag 1115, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OrderCategory                                   { get; set; } = ValidateOrderCategory;

	/// <summary>Holds a FIX 5.0 SP2 NoRootPartyIDs, tag 1116, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRootPartyIDs                                  { get; set; } = ValidateNoRootPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyID, tag 1117, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RootPartyID                                     { get; set; } = ValidateRootPartyID;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyIDSource, tag 1118, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> RootPartyIDSource                               { get; set; } = ValidateRootPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyRole, tag 1119, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RootPartyRole                                   { get; set; } = ValidateRootPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoRootPartySubIDs, tag 1120, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRootPartySubIDs                               { get; set; } = ValidateNoRootPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 RootPartySubID, tag 1121, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RootPartySubID                                  { get; set; } = ValidateRootPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 RootPartySubIDType, tag 1122, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RootPartySubIDType                              { get; set; } = ValidateRootPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 TradeHandlingInstr, tag 1123, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TradeHandlingInstr                              { get; set; } = ValidateTradeHandlingInstr;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeHandlingInstr, tag 1124, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> OrigTradeHandlingInstr                          { get; set; } = ValidateOrigTradeHandlingInstr;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeDate, tag 1125, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      OrigTradeDate                                   { get; set; } = ValidateOrigTradeDate;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeID, tag 1126, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrigTradeID                                     { get; set; } = ValidateOrigTradeID;

	/// <summary>Holds a FIX 5.0 SP2 OrigSecondaryTradeID, tag 1127, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      OrigSecondaryTradeID                            { get; set; } = ValidateOrigSecondaryTradeID;

	/// <summary>Holds a FIX 5.0 SP2 ApplVerID, tag 1128, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ApplVerID                                       { get; set; } = ValidateApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 CstmApplVerID, tag 1129, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      CstmApplVerID                                   { get; set; } = ValidateCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 RefApplVerID, tag 1130, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefApplVerID                                    { get; set; } = ValidateRefApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 RefCstmApplVerID, tag 1131, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefCstmApplVerID                                { get; set; } = ValidateRefCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 TZTransactTime, tag 1132, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> TZTransactTime                                  { get; set; } = ValidateTZTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 ExDestinationIDSource, tag 1133, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExDestinationIDSource                           { get; set; } = ValidateExDestinationIDSource;

	/// <summary>Holds a FIX 5.0 SP2 ReportedPxDiff, tag 1134, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ReportedPxDiff                                  { get; set; } = ValidateReportedPxDiff;

	/// <summary>Holds a FIX 5.0 SP2 RptSys, tag 1135, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RptSys                                          { get; set; } = ValidateRptSys;

	/// <summary>Holds a FIX 5.0 SP2 AllocClearingFeeIndicator, tag 1136, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      AllocClearingFeeIndicator                       { get; set; } = ValidateAllocClearingFeeIndicator;

	/// <summary>Holds a FIX 5.0 SP2 DefaultApplVerID, tag 1137, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DefaultApplVerID                                { get; set; } = ValidateDefaultApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 DisplayQty, tag 1138, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DisplayQty                                      { get; set; } = ValidateDisplayQty;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeSpecialInstructions, tag 1139, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ExchangeSpecialInstructions                     { get; set; } = ValidateExchangeSpecialInstructions;

	/// <summary>Holds a FIX 5.0 SP2 MaxTradeVol, tag 1140, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MaxTradeVol                                     { get; set; } = ValidateMaxTradeVol;

	/// <summary>Holds a FIX 5.0 SP2 NoMDFeedTypes, tag 1141, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMDFeedTypes                                   { get; set; } = ValidateNoMDFeedTypes;

	/// <summary>Holds a FIX 5.0 SP2 MatchAlgorithm, tag 1142, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MatchAlgorithm                                  { get; set; } = ValidateMatchAlgorithm;

	/// <summary>Holds a FIX 5.0 SP2 MaxPriceVariation, tag 1143, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MaxPriceVariation                               { get; set; } = ValidateMaxPriceVariation;

	/// <summary>Holds a FIX 5.0 SP2 ImpliedMarketIndicator, tag 1144, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ImpliedMarketIndicator                          { get; set; } = ValidateImpliedMarketIndicator;

	/// <summary>Holds a FIX 5.0 SP2 EventTime, tag 1145, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> EventTime                                       { get; set; } = ValidateEventTime;

	/// <summary>Holds a FIX 5.0 SP2 MinPriceIncrementAmount, tag 1146, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinPriceIncrementAmount                         { get; set; } = ValidateMinPriceIncrementAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnitOfMeasureQty, tag 1147, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnitOfMeasureQty                                { get; set; } = ValidateUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 LowLimitPrice, tag 1148, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LowLimitPrice                                   { get; set; } = ValidateLowLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 HighLimitPrice, tag 1149, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   HighLimitPrice                                  { get; set; } = ValidateHighLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 TradingReferencePrice, tag 1150, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TradingReferencePrice                           { get; set; } = ValidateTradingReferencePrice;

	/// <summary>Holds a FIX 5.0 SP2 SecurityGroup, tag 1151, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityGroup                                   { get; set; } = ValidateSecurityGroup;

	/// <summary>Holds a FIX 5.0 SP2 LegNumber, tag 1152, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegNumber                                       { get; set; } = ValidateLegNumber;

	/// <summary>Holds a FIX 5.0 SP2 SettlementCycleNo, tag 1153, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlementCycleNo                               { get; set; } = ValidateSettlementCycleNo;

	/// <summary>Holds a FIX 5.0 SP2 SideCurrency, tag 1154, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideCurrency                                    { get; set; } = ValidateSideCurrency;

	/// <summary>Holds a FIX 5.0 SP2 SideSettlCurrency, tag 1155, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideSettlCurrency                               { get; set; } = ValidateSideSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 ApplExtID, tag 1156, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplExtID                                       { get; set; } = ValidateApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 CcyAmt, tag 1157, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CcyAmt                                          { get; set; } = ValidateCcyAmt;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlDetails, tag 1158, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSettlDetails                                  { get; set; } = ValidateNoSettlDetails;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligMode, tag 1159, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SettlObligMode                                  { get; set; } = ValidateSettlObligMode;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligMsgID, tag 1160, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlObligMsgID                                 { get; set; } = ValidateSettlObligMsgID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligID, tag 1161, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlObligID                                    { get; set; } = ValidateSettlObligID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligTransType, tag 1162, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlObligTransType                             { get; set; } = ValidateSettlObligTransType;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligRefID, tag 1163, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SettlObligRefID                                 { get; set; } = ValidateSettlObligRefID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligSource, tag 1164, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlObligSource                                { get; set; } = ValidateSettlObligSource;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlOblig, tag 1165, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoSettlOblig                                    { get; set; } = ValidateNoSettlOblig;

	/// <summary>Holds a FIX 5.0 SP2 QuoteMsgID, tag 1166, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      QuoteMsgID                                      { get; set; } = ValidateQuoteMsgID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryStatus, tag 1167, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   QuoteEntryStatus                                { get; set; } = ValidateQuoteEntryStatus;

	/// <summary>Holds a FIX 5.0 SP2 TotNoCxldQuotes, tag 1168, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoCxldQuotes                                 { get; set; } = ValidateTotNoCxldQuotes;

	/// <summary>Holds a FIX 5.0 SP2 TotNoAccQuotes, tag 1169, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoAccQuotes                                  { get; set; } = ValidateTotNoAccQuotes;

	/// <summary>Holds a FIX 5.0 SP2 TotNoRejQuotes, tag 1170, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoRejQuotes                                  { get; set; } = ValidateTotNoRejQuotes;

	/// <summary>Holds a FIX 5.0 SP2 PrivateQuote, tag 1171, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   PrivateQuote                                    { get; set; } = ValidatePrivateQuote;

	/// <summary>Holds a FIX 5.0 SP2 RespondentType, tag 1172, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RespondentType                                  { get; set; } = ValidateRespondentType;

	/// <summary>Holds a FIX 5.0 SP2 MDSubBookType, tag 1173, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDSubBookType                                   { get; set; } = ValidateMDSubBookType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTradingEvent, tag 1174, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityTradingEvent                            { get; set; } = ValidateSecurityTradingEvent;

	/// <summary>Holds a FIX 5.0 SP2 NoStatsIndicators, tag 1175, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoStatsIndicators                               { get; set; } = ValidateNoStatsIndicators;

	/// <summary>Holds a FIX 5.0 SP2 StatsType, tag 1176, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StatsType                                       { get; set; } = ValidateStatsType;

	/// <summary>Holds a FIX 5.0 SP2 NoOfSecSizes, tag 1177, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoOfSecSizes                                    { get; set; } = ValidateNoOfSecSizes;

	/// <summary>Holds a FIX 5.0 SP2 MDSecSizeType, tag 1178, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MDSecSizeType                                   { get; set; } = ValidateMDSecSizeType;

	/// <summary>Holds a FIX 5.0 SP2 MDSecSize, tag 1179, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MDSecSize                                       { get; set; } = ValidateMDSecSize;

	/// <summary>Holds a FIX 5.0 SP2 ApplID, tag 1180, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ApplID                                          { get; set; } = ValidateApplID;

	/// <summary>Holds a FIX 5.0 SP2 ApplSeqNum, tag 1181, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplSeqNum                                      { get; set; } = ValidateApplSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 ApplBegSeqNum, tag 1182, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplBegSeqNum                                   { get; set; } = ValidateApplBegSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 ApplEndSeqNum, tag 1183, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplEndSeqNum                                   { get; set; } = ValidateApplEndSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXMLLen, tag 1184, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityXMLLen                                  { get; set; } = ValidateSecurityXMLLen;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXML, tag 1185, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      SecurityXML                                     { get; set; } = ValidateSecurityXML;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXMLSchema, tag 1186, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityXMLSchema                               { get; set; } = ValidateSecurityXMLSchema;

	/// <summary>Holds a FIX 5.0 SP2 RefreshIndicator, tag 1187, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   RefreshIndicator                                { get; set; } = ValidateRefreshIndicator;

	/// <summary>Holds a FIX 5.0 SP2 Volatility, tag 1188, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   Volatility                                      { get; set; } = ValidateVolatility;

	/// <summary>Holds a FIX 5.0 SP2 TimeToExpiration, tag 1189, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TimeToExpiration                                { get; set; } = ValidateTimeToExpiration;

	/// <summary>Holds a FIX 5.0 SP2 RiskFreeRate, tag 1190, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   RiskFreeRate                                    { get; set; } = ValidateRiskFreeRate;

	/// <summary>Holds a FIX 5.0 SP2 PriceUnitOfMeasure, tag 1191, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PriceUnitOfMeasure                              { get; set; } = ValidatePriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 PriceUnitOfMeasureQty, tag 1192, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   PriceUnitOfMeasureQty                           { get; set; } = ValidatePriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 SettlMethod, tag 1193, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> SettlMethod                                     { get; set; } = ValidateSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 ExerciseStyle, tag 1194, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ExerciseStyle                                   { get; set; } = ValidateExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 OptPayoutAmount, tag 1195, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OptPayoutAmount                                 { get; set; } = ValidateOptPayoutAmount;

	/// <summary>Holds a FIX 5.0 SP2 PriceQuoteMethod, tag 1196, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      PriceQuoteMethod                                { get; set; } = ValidatePriceQuoteMethod;

	/// <summary>Holds a FIX 5.0 SP2 ValuationMethod, tag 1197, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ValuationMethod                                 { get; set; } = ValidateValuationMethod;

	/// <summary>Holds a FIX 5.0 SP2 ListMethod, tag 1198, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ListMethod                                      { get; set; } = ValidateListMethod;

	/// <summary>Holds a FIX 5.0 SP2 CapPrice, tag 1199, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CapPrice                                        { get; set; } = ValidateCapPrice;

	/// <summary>Holds a FIX 5.0 SP2 FloorPrice, tag 1200, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FloorPrice                                      { get; set; } = ValidateFloorPrice;

	/// <summary>Holds a FIX 5.0 SP2 NoStrikeRules, tag 1201, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoStrikeRules                                   { get; set; } = ValidateNoStrikeRules;

	/// <summary>Holds a FIX 5.0 SP2 StartStrikePxRange, tag 1202, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StartStrikePxRange                              { get; set; } = ValidateStartStrikePxRange;

	/// <summary>Holds a FIX 5.0 SP2 EndStrikePxRange, tag 1203, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EndStrikePxRange                                { get; set; } = ValidateEndStrikePxRange;

	/// <summary>Holds a FIX 5.0 SP2 StrikeIncrement, tag 1204, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StrikeIncrement                                 { get; set; } = ValidateStrikeIncrement;

	/// <summary>Holds a FIX 5.0 SP2 NoTickRules, tag 1205, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTickRules                                     { get; set; } = ValidateNoTickRules;

	/// <summary>Holds a FIX 5.0 SP2 StartTickPriceRange, tag 1206, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StartTickPriceRange                             { get; set; } = ValidateStartTickPriceRange;

	/// <summary>Holds a FIX 5.0 SP2 EndTickPriceRange, tag 1207, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   EndTickPriceRange                               { get; set; } = ValidateEndTickPriceRange;

	/// <summary>Holds a FIX 5.0 SP2 TickIncrement, tag 1208, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   TickIncrement                                   { get; set; } = ValidateTickIncrement;

	/// <summary>Holds a FIX 5.0 SP2 TickRuleType, tag 1209, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TickRuleType                                    { get; set; } = ValidateTickRuleType;

	/// <summary>Holds a FIX 5.0 SP2 NestedInstrAttribType, tag 1210, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NestedInstrAttribType                           { get; set; } = ValidateNestedInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 NestedInstrAttribValue, tag 1211, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NestedInstrAttribValue                          { get; set; } = ValidateNestedInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityTime, tag 1212, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.ZonedTime> LegMaturityTime                                 { get; set; } = ValidateLegMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityTime, tag 1213, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.ZonedTime> UnderlyingMaturityTime                          { get; set; } = ValidateUnderlyingMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSymbol, tag 1214, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSymbol                                { get; set; } = ValidateDerivativeSymbol;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSymbolSfx, tag 1215, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSymbolSfx                             { get; set; } = ValidateDerivativeSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityID, tag 1216, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityID                            { get; set; } = ValidateDerivativeSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityIDSource, tag 1217, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityIDSource                      { get; set; } = ValidateDerivativeSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeSecurityAltID, tag 1218, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDerivativeSecurityAltID                       { get; set; } = ValidateNoDerivativeSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityAltID, tag 1219, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityAltID                         { get; set; } = ValidateDerivativeSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityAltIDSource, tag 1220, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityAltIDSource                   { get; set; } = ValidateDerivativeSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryLowLimitPrice, tag 1221, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SecondaryLowLimitPrice                          { get; set; } = ValidateSecondaryLowLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 MaturityRuleID, tag 1222, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MaturityRuleID                                  { get; set; } = ValidateMaturityRuleID;

	/// <summary>Holds a FIX 5.0 SP2 StrikeRuleID, tag 1223, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StrikeRuleID                                    { get; set; } = ValidateStrikeRuleID;

	/// <summary>Holds a FIX 5.0 SP2 LegUnitOfMeasureQty, tag 1224, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegUnitOfMeasureQty                             { get; set; } = ValidateLegUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeOptPayAmount, tag 1225, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeOptPayAmount                          { get; set; } = ValidateDerivativeOptPayAmount;

	/// <summary>Holds a FIX 5.0 SP2 EndMaturityMonthYear, tag 1226, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> EndMaturityMonthYear                            { get; set; } = ValidateEndMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 ProductComplex, tag 1227, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ProductComplex                                  { get; set; } = ValidateProductComplex;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeProductComplex, tag 1228, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeProductComplex                        { get; set; } = ValidateDerivativeProductComplex;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearIncrement, tag 1229, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MaturityMonthYearIncrement                      { get; set; } = ValidateMaturityMonthYearIncrement;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryHighLimitPrice, tag 1230, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SecondaryHighLimitPrice                         { get; set; } = ValidateSecondaryHighLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 MinLotSize, tag 1231, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   MinLotSize                                      { get; set; } = ValidateMinLotSize;

	/// <summary>Holds a FIX 5.0 SP2 NoExecInstRules, tag 1232, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoExecInstRules                                 { get; set; } = ValidateNoExecInstRules;

	/// <summary>Holds a FIX 5.0 SP2 NoLotTypeRules, tag 1234, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoLotTypeRules                                  { get; set; } = ValidateNoLotTypeRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMatchRules, tag 1235, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMatchRules                                    { get; set; } = ValidateNoMatchRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMaturityRules, tag 1236, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMaturityRules                                 { get; set; } = ValidateNoMaturityRules;

	/// <summary>Holds a FIX 5.0 SP2 NoOrdTypeRules, tag 1237, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoOrdTypeRules                                  { get; set; } = ValidateNoOrdTypeRules;

	/// <summary>Holds a FIX 5.0 SP2 NoTimeInForceRules, tag 1239, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTimeInForceRules                              { get; set; } = ValidateNoTimeInForceRules;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradingReferencePrice, tag 1240, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   SecondaryTradingReferencePrice                  { get; set; } = ValidateSecondaryTradingReferencePrice;

	/// <summary>Holds a FIX 5.0 SP2 StartMaturityMonthYear, tag 1241, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> StartMaturityMonthYear                          { get; set; } = ValidateStartMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 FlexProductEligibilityIndicator, tag 1242, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   FlexProductEligibilityIndicator                 { get; set; } = ValidateFlexProductEligibilityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 DerivFlexProductEligibilityIndicator, tag 1243, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   DerivFlexProductEligibilityIndicator            { get; set; } = ValidateDerivFlexProductEligibilityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 FlexibleIndicator, tag 1244, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   FlexibleIndicator                               { get; set; } = ValidateFlexibleIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradingCurrency, tag 1245, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradingCurrency                                 { get; set; } = ValidateTradingCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeProduct, tag 1246, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeProduct                               { get; set; } = ValidateDerivativeProduct;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityGroup, tag 1247, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityGroup                         { get; set; } = ValidateDerivativeSecurityGroup;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCFICode, tag 1248, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeCFICode                               { get; set; } = ValidateDerivativeCFICode;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityType, tag 1249, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityType                          { get; set; } = ValidateDerivativeSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecuritySubType, tag 1250, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecuritySubType                       { get; set; } = ValidateDerivativeSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityMonthYear, tag 1251, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> DerivativeMaturityMonthYear                     { get; set; } = ValidateDerivativeMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityDate, tag 1252, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DerivativeMaturityDate                          { get; set; } = ValidateDerivativeMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityTime, tag 1253, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.ZonedTime> DerivativeMaturityTime                          { get; set; } = ValidateDerivativeMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSettleOnOpenFlag, tag 1254, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSettleOnOpenFlag                      { get; set; } = ValidateDerivativeSettleOnOpenFlag;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrmtAssignmentMethod, tag 1255, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DerivativeInstrmtAssignmentMethod               { get; set; } = ValidateDerivativeInstrmtAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityStatus, tag 1256, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityStatus                        { get; set; } = ValidateDerivativeSecurityStatus;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrRegistry, tag 1257, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeInstrRegistry                         { get; set; } = ValidateDerivativeInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCountryOfIssue, tag 1258, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeCountryOfIssue                        { get; set; } = ValidateDerivativeCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStateOrProvinceOfIssue, tag 1259, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeStateOrProvinceOfIssue                { get; set; } = ValidateDerivativeStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeLocaleOfIssue, tag 1260, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeLocaleOfIssue                         { get; set; } = ValidateDerivativeLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikePrice, tag 1261, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeStrikePrice                           { get; set; } = ValidateDerivativeStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeCurrency, tag 1262, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeStrikeCurrency                        { get; set; } = ValidateDerivativeStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeMultiplier, tag 1263, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeStrikeMultiplier                      { get; set; } = ValidateDerivativeStrikeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeValue, tag 1264, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeStrikeValue                           { get; set; } = ValidateDerivativeStrikeValue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeOptAttribute, tag 1265, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DerivativeOptAttribute                          { get; set; } = ValidateDerivativeOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractMultiplier, tag 1266, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeContractMultiplier                    { get; set; } = ValidateDerivativeContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMinPriceIncrement, tag 1267, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeMinPriceIncrement                     { get; set; } = ValidateDerivativeMinPriceIncrement;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMinPriceIncrementAmount, tag 1268, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeMinPriceIncrementAmount               { get; set; } = ValidateDerivativeMinPriceIncrementAmount;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeUnitOfMeasure, tag 1269, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeUnitOfMeasure                         { get; set; } = ValidateDerivativeUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeUnitOfMeasureQty, tag 1270, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeUnitOfMeasureQty                      { get; set; } = ValidateDerivativeUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeTimeUnit, tag 1271, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeTimeUnit                              { get; set; } = ValidateDerivativeTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityExchange, tag 1272, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityExchange                      { get; set; } = ValidateDerivativeSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePositionLimit, tag 1273, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativePositionLimit                         { get; set; } = ValidateDerivativePositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeNTPositionLimit, tag 1274, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeNTPositionLimit                       { get; set; } = ValidateDerivativeNTPositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeIssuer, tag 1275, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeIssuer                                { get; set; } = ValidateDerivativeIssuer;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeIssueDate, tag 1276, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DerivativeIssueDate                             { get; set; } = ValidateDerivativeIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedIssuerLen, tag 1277, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeEncodedIssuerLen                      { get; set; } = ValidateDerivativeEncodedIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedIssuer, tag 1278, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      DerivativeEncodedIssuer                         { get; set; } = ValidateDerivativeEncodedIssuer;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityDesc, tag 1279, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityDesc                          { get; set; } = ValidateDerivativeSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedSecurityDescLen, tag 1280, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeEncodedSecurityDescLen                { get; set; } = ValidateDerivativeEncodedSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedSecurityDesc, tag 1281, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      DerivativeEncodedSecurityDesc                   { get; set; } = ValidateDerivativeEncodedSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXMLLen, tag 1282, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeSecurityXMLLen                        { get; set; } = ValidateDerivativeSecurityXMLLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXML, tag 1283, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      DerivativeSecurityXML                           { get; set; } = ValidateDerivativeSecurityXML;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXMLSchema, tag 1284, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeSecurityXMLSchema                     { get; set; } = ValidateDerivativeSecurityXMLSchema;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractSettlMonth, tag 1285, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> DerivativeContractSettlMonth                    { get; set; } = ValidateDerivativeContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeEvents, tag 1286, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDerivativeEvents                              { get; set; } = ValidateNoDerivativeEvents;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventType, tag 1287, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeEventType                             { get; set; } = ValidateDerivativeEventType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventDate, tag 1288, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      DerivativeEventDate                             { get; set; } = ValidateDerivativeEventDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventTime, tag 1289, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> DerivativeEventTime                             { get; set; } = ValidateDerivativeEventTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventPx, tag 1290, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeEventPx                               { get; set; } = ValidateDerivativeEventPx;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventText, tag 1291, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeEventText                             { get; set; } = ValidateDerivativeEventText;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrumentParties, tag 1292, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDerivativeInstrumentParties                   { get; set; } = ValidateNoDerivativeInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyID, tag 1293, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeInstrumentPartyID                     { get; set; } = ValidateDerivativeInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyIDSource, tag 1294, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeInstrumentPartyIDSource               { get; set; } = ValidateDerivativeInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyRole, tag 1295, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeInstrumentPartyRole                   { get; set; } = ValidateDerivativeInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrumentPartySubIDs, tag 1296, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDerivativeInstrumentPartySubIDs               { get; set; } = ValidateNoDerivativeInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartySubID, tag 1297, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeInstrumentPartySubID                  { get; set; } = ValidateDerivativeInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartySubIDType, tag 1298, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeInstrumentPartySubIDType              { get; set; } = ValidateDerivativeInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeExerciseStyle, tag 1299, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DerivativeExerciseStyle                         { get; set; } = ValidateDerivativeExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 MarketSegmentID, tag 1300, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MarketSegmentID                                 { get; set; } = ValidateMarketSegmentID;

	/// <summary>Holds a FIX 5.0 SP2 MarketID, tag 1301, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MarketID                                        { get; set; } = ValidateMarketID;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearIncrementUnits, tag 1302, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MaturityMonthYearIncrementUnits                 { get; set; } = ValidateMaturityMonthYearIncrementUnits;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearFormat, tag 1303, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MaturityMonthYearFormat                         { get; set; } = ValidateMaturityMonthYearFormat;

	/// <summary>Holds a FIX 5.0 SP2 StrikeExerciseStyle, tag 1304, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StrikeExerciseStyle                             { get; set; } = ValidateStrikeExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryPriceLimitType, tag 1305, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecondaryPriceLimitType                         { get; set; } = ValidateSecondaryPriceLimitType;

	/// <summary>Holds a FIX 5.0 SP2 PriceLimitType, tag 1306, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   PriceLimitType                                  { get; set; } = ValidatePriceLimitType;

	/// <summary>Holds a FIX 5.0 SP2 ExecInstValue, tag 1308, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ExecInstValue                                   { get; set; } = ValidateExecInstValue;

	/// <summary>Holds a FIX 5.0 SP2 NoTradingSessionRules, tag 1309, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTradingSessionRules                           { get; set; } = ValidateNoTradingSessionRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMarketSegments, tag 1310, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoMarketSegments                                { get; set; } = ValidateNoMarketSegments;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrAttrib, tag 1311, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoDerivativeInstrAttrib                         { get; set; } = ValidateNoDerivativeInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedInstrAttrib, tag 1312, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNestedInstrAttrib                             { get; set; } = ValidateNoNestedInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrAttribType, tag 1313, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeInstrAttribType                       { get; set; } = ValidateDerivativeInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrAttribValue, tag 1314, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeInstrAttribValue                      { get; set; } = ValidateDerivativeInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceUnitOfMeasure, tag 1315, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativePriceUnitOfMeasure                    { get; set; } = ValidateDerivativePriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceUnitOfMeasureQty, tag 1316, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativePriceUnitOfMeasureQty                 { get; set; } = ValidateDerivativePriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSettlMethod, tag 1317, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> DerivativeSettlMethod                           { get; set; } = ValidateDerivativeSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceQuoteMethod, tag 1318, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativePriceQuoteMethod                      { get; set; } = ValidateDerivativePriceQuoteMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeValuationMethod, tag 1319, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DerivativeValuationMethod                       { get; set; } = ValidateDerivativeValuationMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeListMethod, tag 1320, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeListMethod                            { get; set; } = ValidateDerivativeListMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCapPrice, tag 1321, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeCapPrice                              { get; set; } = ValidateDerivativeCapPrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeFloorPrice, tag 1322, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DerivativeFloorPrice                            { get; set; } = ValidateDerivativeFloorPrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePutOrCall, tag 1323, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativePutOrCall                             { get; set; } = ValidateDerivativePutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 ListUpdateAction, tag 1324, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> ListUpdateAction                                { get; set; } = ValidateListUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 ParentMktSegmID, tag 1325, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ParentMktSegmID                                 { get; set; } = ValidateParentMktSegmID;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionDesc, tag 1326, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TradingSessionDesc                              { get; set; } = ValidateTradingSessionDesc;

	/// <summary>Holds a FIX 5.0 SP2 TradSesUpdateAction, tag 1327, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TradSesUpdateAction                             { get; set; } = ValidateTradSesUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 RejectText, tag 1328, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RejectText                                      { get; set; } = ValidateRejectText;

	/// <summary>Holds a FIX 5.0 SP2 FeeMultiplier, tag 1329, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FeeMultiplier                                   { get; set; } = ValidateFeeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSymbol, tag 1330, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSymbol                             { get; set; } = ValidateUnderlyingLegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSymbolSfx, tag 1331, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSymbolSfx                          { get; set; } = ValidateUnderlyingLegSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityID, tag 1332, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityID                         { get; set; } = ValidateUnderlyingLegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityIDSource, tag 1333, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityIDSource                   { get; set; } = ValidateUnderlyingLegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingLegSecurityAltID, tag 1334, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoUnderlyingLegSecurityAltID                    { get; set; } = ValidateNoUnderlyingLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityAltID, tag 1335, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityAltID                      { get; set; } = ValidateUnderlyingLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityAltIDSource, tag 1336, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityAltIDSource                { get; set; } = ValidateUnderlyingLegSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityType, tag 1337, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityType                       { get; set; } = ValidateUnderlyingLegSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecuritySubType, tag 1338, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecuritySubType                    { get; set; } = ValidateUnderlyingLegSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityMonthYear, tag 1339, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.MonthYear> UnderlyingLegMaturityMonthYear                  { get; set; } = ValidateUnderlyingLegMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegStrikePrice, tag 1340, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingLegStrikePrice                        { get; set; } = ValidateUnderlyingLegStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityExchange, tag 1341, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityExchange                   { get; set; } = ValidateUnderlyingLegSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 NoOfLegUnderlyings, tag 1342, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoOfLegUnderlyings                              { get; set; } = ValidateNoOfLegUnderlyings;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegPutOrCall, tag 1343, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingLegPutOrCall                          { get; set; } = ValidateUnderlyingLegPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegCFICode, tag 1344, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegCFICode                            { get; set; } = ValidateUnderlyingLegCFICode;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityDate, tag 1345, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Date>      UnderlyingLegMaturityDate                       { get; set; } = ValidateUnderlyingLegMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 ApplReqID, tag 1346, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ApplReqID                                       { get; set; } = ValidateApplReqID;

	/// <summary>Holds a FIX 5.0 SP2 ApplReqType, tag 1347, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplReqType                                     { get; set; } = ValidateApplReqType;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseType, tag 1348, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplResponseType                                { get; set; } = ValidateApplResponseType;

	/// <summary>Holds a FIX 5.0 SP2 ApplTotalMessageCount, tag 1349, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplTotalMessageCount                           { get; set; } = ValidateApplTotalMessageCount;

	/// <summary>Holds a FIX 5.0 SP2 ApplLastSeqNum, tag 1350, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplLastSeqNum                                  { get; set; } = ValidateApplLastSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 NoApplIDs, tag 1351, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoApplIDs                                       { get; set; } = ValidateNoApplIDs;

	/// <summary>Holds a FIX 5.0 SP2 ApplResendFlag, tag 1352, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   ApplResendFlag                                  { get; set; } = ValidateApplResendFlag;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseID, tag 1353, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ApplResponseID                                  { get; set; } = ValidateApplResponseID;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseError, tag 1354, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplResponseError                               { get; set; } = ValidateApplResponseError;

	/// <summary>Holds a FIX 5.0 SP2 RefApplID, tag 1355, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefApplID                                       { get; set; } = ValidateRefApplID;

	/// <summary>Holds a FIX 5.0 SP2 ApplReportID, tag 1356, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ApplReportID                                    { get; set; } = ValidateApplReportID;

	/// <summary>Holds a FIX 5.0 SP2 RefApplLastSeqNum, tag 1357, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RefApplLastSeqNum                               { get; set; } = ValidateRefApplLastSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 LegPutOrCall, tag 1358, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegPutOrCall                                    { get; set; } = ValidateLegPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 TotNoFills, tag 1361, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TotNoFills                                      { get; set; } = ValidateTotNoFills;

	/// <summary>Holds a FIX 5.0 SP2 NoFills, tag 1362, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoFills                                         { get; set; } = ValidateNoFills;

	/// <summary>Holds a FIX 5.0 SP2 FillExecID, tag 1363, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      FillExecID                                      { get; set; } = ValidateFillExecID;

	/// <summary>Holds a FIX 5.0 SP2 FillPx, tag 1364, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FillPx                                          { get; set; } = ValidateFillPx;

	/// <summary>Holds a FIX 5.0 SP2 FillQty, tag 1365, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   FillQty                                         { get; set; } = ValidateFillQty;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocID, tag 1366, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegAllocID                                      { get; set; } = ValidateLegAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocSettlCurrency, tag 1367, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegAllocSettlCurrency                           { get; set; } = ValidateLegAllocSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TradSesEvent, tag 1368, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradSesEvent                                    { get; set; } = ValidateTradSesEvent;

	/// <summary>Holds a FIX 5.0 SP2 MassActionReportID, tag 1369, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MassActionReportID                              { get; set; } = ValidateMassActionReportID;

	/// <summary>Holds a FIX 5.0 SP2 NoNotAffectedOrders, tag 1370, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNotAffectedOrders                             { get; set; } = ValidateNoNotAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 NotAffectedOrderID, tag 1371, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NotAffectedOrderID                              { get; set; } = ValidateNotAffectedOrderID;

	/// <summary>Holds a FIX 5.0 SP2 NotAffOrigClOrdID, tag 1372, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NotAffOrigClOrdID                               { get; set; } = ValidateNotAffOrigClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 MassActionType, tag 1373, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassActionType                                  { get; set; } = ValidateMassActionType;

	/// <summary>Holds a FIX 5.0 SP2 MassActionScope, tag 1374, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassActionScope                                 { get; set; } = ValidateMassActionScope;

	/// <summary>Holds a FIX 5.0 SP2 MassActionResponse, tag 1375, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassActionResponse                              { get; set; } = ValidateMassActionResponse;

	/// <summary>Holds a FIX 5.0 SP2 MassActionRejectReason, tag 1376, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MassActionRejectReason                          { get; set; } = ValidateMassActionRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 MultilegModel, tag 1377, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MultilegModel                                   { get; set; } = ValidateMultilegModel;

	/// <summary>Holds a FIX 5.0 SP2 MultilegPriceMethod, tag 1378, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   MultilegPriceMethod                             { get; set; } = ValidateMultilegPriceMethod;

	/// <summary>Holds a FIX 5.0 SP2 LegVolatility, tag 1379, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegVolatility                                   { get; set; } = ValidateLegVolatility;

	/// <summary>Holds a FIX 5.0 SP2 DividendYield, tag 1380, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DividendYield                                   { get; set; } = ValidateDividendYield;

	/// <summary>Holds a FIX 5.0 SP2 LegDividendYield, tag 1381, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegDividendYield                                { get; set; } = ValidateLegDividendYield;

	/// <summary>Holds a FIX 5.0 SP2 CurrencyRatio, tag 1382, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   CurrencyRatio                                   { get; set; } = ValidateCurrencyRatio;

	/// <summary>Holds a FIX 5.0 SP2 LegCurrencyRatio, tag 1383, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegCurrencyRatio                                { get; set; } = ValidateLegCurrencyRatio;

	/// <summary>Holds a FIX 5.0 SP2 LegExecInst, tag 1384, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Multiple>  LegExecInst                                     { get; set; } = ValidateLegExecInst;

	/// <summary>Holds a FIX 5.0 SP2 ContingencyType, tag 1385, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ContingencyType                                 { get; set; } = ValidateContingencyType;

	/// <summary>Holds a FIX 5.0 SP2 ListRejectReason, tag 1386, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ListRejectReason                                { get; set; } = ValidateListRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 NoTrdRepIndicators, tag 1387, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTrdRepIndicators                              { get; set; } = ValidateNoTrdRepIndicators;

	/// <summary>Holds a FIX 5.0 SP2 TrdRepPartyRole, tag 1388, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TrdRepPartyRole                                 { get; set; } = ValidateTrdRepPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 TrdRepIndicator, tag 1389, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   TrdRepIndicator                                 { get; set; } = ValidateTrdRepIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradePublishIndicator, tag 1390, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TradePublishIndicator                           { get; set; } = ValidateTradePublishIndicator;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegOptAttribute, tag 1391, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> UnderlyingLegOptAttribute                       { get; set; } = ValidateUnderlyingLegOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityDesc, tag 1392, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingLegSecurityDesc                       { get; set; } = ValidateUnderlyingLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 MarketReqID, tag 1393, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MarketReqID                                     { get; set; } = ValidateMarketReqID;

	/// <summary>Holds a FIX 5.0 SP2 MarketReportID, tag 1394, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MarketReportID                                  { get; set; } = ValidateMarketReportID;

	/// <summary>Holds a FIX 5.0 SP2 MarketUpdateAction, tag 1395, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> MarketUpdateAction                              { get; set; } = ValidateMarketUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 MarketSegmentDesc, tag 1396, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MarketSegmentDesc                               { get; set; } = ValidateMarketSegmentDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedMktSegmDescLen, tag 1397, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedMktSegmDescLen                           { get; set; } = ValidateEncodedMktSegmDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedMktSegmDesc, tag 1398, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedMktSegmDesc                              { get; set; } = ValidateEncodedMktSegmDesc;

	/// <summary>Holds a FIX 5.0 SP2 ApplNewSeqNum, tag 1399, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplNewSeqNum                                   { get; set; } = ValidateApplNewSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPasswordMethod, tag 1400, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncryptedPasswordMethod                         { get; set; } = ValidateEncryptedPasswordMethod;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPasswordLen, tag 1401, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncryptedPasswordLen                            { get; set; } = ValidateEncryptedPasswordLen;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPassword, tag 1402, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncryptedPassword                               { get; set; } = ValidateEncryptedPassword;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedNewPasswordLen, tag 1403, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncryptedNewPasswordLen                         { get; set; } = ValidateEncryptedNewPasswordLen;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedNewPassword, tag 1404, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncryptedNewPassword                            { get; set; } = ValidateEncryptedNewPassword;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityTime, tag 1405, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.ZonedTime> UnderlyingLegMaturityTime                       { get; set; } = ValidateUnderlyingLegMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 RefApplExtID, tag 1406, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RefApplExtID                                    { get; set; } = ValidateRefApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 DefaultApplExtID, tag 1407, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DefaultApplExtID                                { get; set; } = ValidateDefaultApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 DefaultCstmApplVerID, tag 1408, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      DefaultCstmApplVerID                            { get; set; } = ValidateDefaultCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 SessionStatus, tag 1409, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SessionStatus                                   { get; set; } = ValidateSessionStatus;

	/// <summary>Holds a FIX 5.0 SP2 DefaultVerIndicator, tag 1410, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Boolean>   DefaultVerIndicator                             { get; set; } = ValidateDefaultVerIndicator;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartySubIDType, tag 1411, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested4PartySubIDType                           { get; set; } = ValidateNested4PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartySubID, tag 1412, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested4PartySubID                               { get; set; } = ValidateNested4PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 NoNested4PartySubIDs, tag 1413, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested4PartySubIDs                            { get; set; } = ValidateNoNested4PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoNested4PartyIDs, tag 1414, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNested4PartyIDs                               { get; set; } = ValidateNoNested4PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyID, tag 1415, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Nested4PartyID                                  { get; set; } = ValidateNested4PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyIDSource, tag 1416, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> Nested4PartyIDSource                            { get; set; } = ValidateNested4PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyRole, tag 1417, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   Nested4PartyRole                                { get; set; } = ValidateNested4PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 LegLastQty, tag 1418, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegLastQty                                      { get; set; } = ValidateLegLastQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingExerciseStyle, tag 1419, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingExerciseStyle                         { get; set; } = ValidateUnderlyingExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 LegExerciseStyle, tag 1420, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegExerciseStyle                                { get; set; } = ValidateLegExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceUnitOfMeasure, tag 1421, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LegPriceUnitOfMeasure                           { get; set; } = ValidateLegPriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceUnitOfMeasureQty, tag 1422, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   LegPriceUnitOfMeasureQty                        { get; set; } = ValidateLegPriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingUnitOfMeasureQty, tag 1423, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingUnitOfMeasureQty                      { get; set; } = ValidateUnderlyingUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceUnitOfMeasure, tag 1424, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingPriceUnitOfMeasure                    { get; set; } = ValidateUnderlyingPriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceUnitOfMeasureQty, tag 1425, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingPriceUnitOfMeasureQty                 { get; set; } = ValidateUnderlyingPriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 ApplReportType, tag 1426, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ApplReportType                                  { get; set; } = ValidateApplReportType;

	/// <summary>Holds a FIX 5.0 SP2 SideExecID, tag 1427, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SideExecID                                      { get; set; } = ValidateSideExecID;

	/// <summary>Holds a FIX 5.0 SP2 OrderDelay, tag 1428, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OrderDelay                                      { get; set; } = ValidateOrderDelay;

	/// <summary>Holds a FIX 5.0 SP2 OrderDelayUnit, tag 1429, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OrderDelayUnit                                  { get; set; } = ValidateOrderDelayUnit;

	/// <summary>Holds a FIX 5.0 SP2 VenueType, tag 1430, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> VenueType                                       { get; set; } = ValidateVenueType;

	/// <summary>Holds a FIX 5.0 SP2 RefOrdIDReason, tag 1431, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RefOrdIDReason                                  { get; set; } = ValidateRefOrdIDReason;

	/// <summary>Holds a FIX 5.0 SP2 OrigCustOrderCapacity, tag 1432, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OrigCustOrderCapacity                           { get; set; } = ValidateOrigCustOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 RefApplReqID, tag 1433, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RefApplReqID                                    { get; set; } = ValidateRefApplReqID;

	/// <summary>Holds a FIX 5.0 SP2 ModelType, tag 1434, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ModelType                                       { get; set; } = ValidateModelType;

	/// <summary>Holds a FIX 5.0 SP2 ContractMultiplierUnit, tag 1435, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ContractMultiplierUnit                          { get; set; } = ValidateContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 LegContractMultiplierUnit, tag 1436, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegContractMultiplierUnit                       { get; set; } = ValidateLegContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingContractMultiplierUnit, tag 1437, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingContractMultiplierUnit                { get; set; } = ValidateUnderlyingContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractMultiplierUnit, tag 1438, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeContractMultiplierUnit                { get; set; } = ValidateDerivativeContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 FlowScheduleType, tag 1439, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   FlowScheduleType                                { get; set; } = ValidateFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 LegFlowScheduleType, tag 1440, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   LegFlowScheduleType                             { get; set; } = ValidateLegFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFlowScheduleType, tag 1441, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingFlowScheduleType                      { get; set; } = ValidateUnderlyingFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeFlowScheduleType, tag 1442, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   DerivativeFlowScheduleType                      { get; set; } = ValidateDerivativeFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 FillLiquidityInd, tag 1443, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   FillLiquidityInd                                { get; set; } = ValidateFillLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 SideLiquidityInd, tag 1444, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SideLiquidityInd                                { get; set; } = ValidateSideLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 NoRateSources, tag 1445, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoRateSources                                   { get; set; } = ValidateNoRateSources;

	/// <summary>Holds a FIX 5.0 SP2 RateSource, tag 1446, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RateSourceField                                 { get; set; } = ValidateRateSourceField;

	/// <summary>Holds a FIX 5.0 SP2 RateSourceType, tag 1447, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   RateSourceType                                  { get; set; } = ValidateRateSourceType;

	/// <summary>Holds a FIX 5.0 SP2 ReferencePage, tag 1448, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      ReferencePage                                   { get; set; } = ValidateReferencePage;

	/// <summary>Holds a FIX 5.0 SP2 RestructuringType, tag 1449, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      RestructuringType                               { get; set; } = ValidateRestructuringType;

	/// <summary>Holds a FIX 5.0 SP2 Seniority, tag 1450, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      Seniority                                       { get; set; } = ValidateSeniority;

	/// <summary>Holds a FIX 5.0 SP2 NotionalPercentageOutstanding, tag 1451, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   NotionalPercentageOutstanding                   { get; set; } = ValidateNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 OriginalNotionalPercentageOutstanding, tag 1452, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   OriginalNotionalPercentageOutstanding           { get; set; } = ValidateOriginalNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRestructuringType, tag 1453, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingRestructuringType                     { get; set; } = ValidateUnderlyingRestructuringType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSeniority, tag 1454, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      UnderlyingSeniority                             { get; set; } = ValidateUnderlyingSeniority;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingNotionalPercentageOutstanding, tag 1455, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingNotionalPercentageOutstanding         { get; set; } = ValidateUnderlyingNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingOriginalNotionalPercentageOutstanding, tag 1456, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingOriginalNotionalPercentageOutstanding { get; set; } = ValidateUnderlyingOriginalNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 AttachmentPoint, tag 1457, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   AttachmentPoint                                 { get; set; } = ValidateAttachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 DetachmentPoint, tag 1458, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   DetachmentPoint                                 { get; set; } = ValidateDetachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAttachmentPoint, tag 1459, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingAttachmentPoint                       { get; set; } = ValidateUnderlyingAttachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDetachmentPoint, tag 1460, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   UnderlyingDetachmentPoint                       { get; set; } = ValidateUnderlyingDetachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 NoTargetPartyIDs, tag 1461, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoTargetPartyIDs                                { get; set; } = ValidateNoTargetPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyID, tag 1462, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      TargetPartyID                                   { get; set; } = ValidateTargetPartyID;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyIDSource, tag 1463, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Character> TargetPartyIDSource                             { get; set; } = ValidateTargetPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyRole, tag 1464, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   TargetPartyRole                                 { get; set; } = ValidateTargetPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListID, tag 1465, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityListID                                  { get; set; } = ValidateSecurityListID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListRefID, tag 1466, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityListRefID                               { get; set; } = ValidateSecurityListRefID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListDesc, tag 1467, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      SecurityListDesc                                { get; set; } = ValidateSecurityListDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityListDescLen, tag 1468, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   EncodedSecurityListDescLen                      { get; set; } = ValidateEncodedSecurityListDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityListDesc, tag 1469, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Data>      EncodedSecurityListDesc                         { get; set; } = ValidateEncodedSecurityListDesc;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListType, tag 1470, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityListType                                { get; set; } = ValidateSecurityListType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListTypeSource, tag 1471, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   SecurityListTypeSource                          { get; set; } = ValidateSecurityListTypeSource;

	/// <summary>Holds a FIX 5.0 SP2 NewsID, tag 1472, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NewsID                                          { get; set; } = ValidateNewsID;

	/// <summary>Holds a FIX 5.0 SP2 NewsCategory, tag 1473, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NewsCategory                                    { get; set; } = ValidateNewsCategory;

	/// <summary>Holds a FIX 5.0 SP2 LanguageCode, tag 1474, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      LanguageCode                                    { get; set; } = ValidateLanguageCode;

	/// <summary>Holds a FIX 5.0 SP2 NoNewsRefIDs, tag 1475, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoNewsRefIDs                                    { get; set; } = ValidateNoNewsRefIDs;

	/// <summary>Holds a FIX 5.0 SP2 NewsRefID, tag 1476, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      NewsRefID                                       { get; set; } = ValidateNewsRefID;

	/// <summary>Holds a FIX 5.0 SP2 NewsRefType, tag 1477, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NewsRefType                                     { get; set; } = ValidateNewsRefType;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceDeterminationMethod, tag 1478, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StrikePriceDeterminationMethod                  { get; set; } = ValidateStrikePriceDeterminationMethod;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceBoundaryMethod, tag 1479, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StrikePriceBoundaryMethod                       { get; set; } = ValidateStrikePriceBoundaryMethod;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceBoundaryPrecision, tag 1480, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   StrikePriceBoundaryPrecision                    { get; set; } = ValidateStrikePriceBoundaryPrecision;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceDeterminationMethod, tag 1481, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   UnderlyingPriceDeterminationMethod              { get; set; } = ValidateUnderlyingPriceDeterminationMethod;

	/// <summary>Holds a FIX 5.0 SP2 OptPayoutType, tag 1482, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   OptPayoutType                                   { get; set; } = ValidateOptPayoutType;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEvents, tag 1483, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoComplexEvents                                 { get; set; } = ValidateNoComplexEvents;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventType, tag 1484, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ComplexEventType                                { get; set; } = ValidateComplexEventType;

	/// <summary>Holds a FIX 5.0 SP2 ComplexOptPayoutAmount, tag 1485, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ComplexOptPayoutAmount                          { get; set; } = ValidateComplexOptPayoutAmount;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPrice, tag 1486, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ComplexEventPrice                               { get; set; } = ValidateComplexEventPrice;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceBoundaryMethod, tag 1487, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ComplexEventPriceBoundaryMethod                 { get; set; } = ValidateComplexEventPriceBoundaryMethod;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceBoundaryPrecision, tag 1488, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Decimal>   ComplexEventPriceBoundaryPrecision              { get; set; } = ValidateComplexEventPriceBoundaryPrecision;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceTimeType, tag 1489, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ComplexEventPriceTimeType                       { get; set; } = ValidateComplexEventPriceTimeType;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventCondition, tag 1490, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   ComplexEventCondition                           { get; set; } = ValidateComplexEventCondition;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEventDates, tag 1491, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoComplexEventDates                             { get; set; } = ValidateNoComplexEventDates;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventStartDate, tag 1492, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ComplexEventStartDate                           { get; set; } = ValidateComplexEventStartDate;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventEndDate, tag 1493, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> ComplexEventEndDate                             { get; set; } = ValidateComplexEventEndDate;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEventTimes, tag 1494, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoComplexEventTimes                             { get; set; } = ValidateNoComplexEventTimes;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventStartTime, tag 1495, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Time>      ComplexEventStartTime                           { get; set; } = ValidateComplexEventStartTime;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventEndTime, tag 1496, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Time>      ComplexEventEndTime                             { get; set; } = ValidateComplexEventEndTime;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnReqID, tag 1497, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StreamAsgnReqID                                 { get; set; } = ValidateStreamAsgnReqID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnReqType, tag 1498, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StreamAsgnReqType                               { get; set; } = ValidateStreamAsgnReqType;

	/// <summary>Holds a FIX 5.0 SP2 NoAsgnReqs, tag 1499, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   NoAsgnReqs                                      { get; set; } = ValidateNoAsgnReqs;

	/// <summary>Holds a FIX 5.0 SP2 MDStreamID, tag 1500, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      MDStreamID                                      { get; set; } = ValidateMDStreamID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnRptID, tag 1501, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Text>      StreamAsgnRptID                                 { get; set; } = ValidateStreamAsgnRptID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnRejReason, tag 1502, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StreamAsgnRejReason                             { get; set; } = ValidateStreamAsgnRejReason;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnAckType, tag 1503, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StreamAsgnAckType                               { get; set; } = ValidateStreamAsgnAckType;

	/// <summary>Holds a FIX 5.0 SP2 RelSymTransactTime, tag 1504, to its type.</summary>
	public Action<Fix50Context, FixCheck, FixField.Timestamp> RelSymTransactTime                              { get; set; } = ValidateRelSymTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnType, tag 1617, to the values the specification lists.</summary>
	public Action<Fix50Context, FixCheck, FixField.Integer>   StreamAsgnType                                  { get; set; } = ValidateStreamAsgnType;

	static void ValidateAccount(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvId(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvSide(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(check, field);
	}

	static void ValidateAdvTransType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("C" or "N" or "R"))
			Invalid(check, field);
	}

	static void ValidateAvgPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginSeqNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginString(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBodyLength(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCheckSum(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClOrdID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommission(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateCumQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndSeqNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecInst(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
					"C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or
					"P" or "Q" or "R" or "S" or "T" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or
					"c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or
					"p" or "q" or "r" or "s" or "t"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateExecRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHandlInst(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or
			"A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M"))
			Invalid(check, field);
	}

	static void ValidateIOIID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIQltyInd(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('H' or 'L' or 'M'))
			Invalid(check, field);
	}

	static void ValidateIOIRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIQty(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("L" or "M" or "S" or "U"))
			Invalid(check, field);
	}

	static void ValidateIOITransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateLastCapacity(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateLastMkt(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLinesOfText(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or
			"9" or "A" or "AA" or "AB" or "AC" or "AD" or "AE" or "AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or
			"AL" or "AM" or "AN" or "AO" or "AP" or "AQ" or "AR" or "AS" or "AT" or "AU" or "AV" or "AW" or "AX" or
			"AY" or "AZ" or "B" or "BA" or "BB" or "BC" or "BD" or "BE" or "BF" or "BG" or "BH" or "BI" or "BJ" or
			"BK" or "BL" or "BM" or "BN" or "BO" or "BP" or "BQ" or "BR" or "BS" or "BT" or "BU" or "BV" or "BW" or
			"BX" or "BY" or "BZ" or "C" or "CA" or "CB" or "CC" or "CD" or "CE" or "D" or "E" or "F" or "G" or
			"H" or "J" or "K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or "T" or "V" or "W" or "X" or "Y" or
			"Z" or "a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or
			"o" or "p" or "q" or "r" or "s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
			Invalid(check, field);
	}

	static void ValidateNewSeqNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrdStatus(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C' or 'D' or 'E'))
			Invalid(check, field);
	}

	static void ValidateOrdType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or
			'A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'I' or 'J' or 'K' or 'L' or 'M' or 'P' or 'Q'))
			Invalid(check, field);
	}

	static void ValidateOrigClOrdID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossDupFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSendingTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuantity(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSide(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or
			'A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'G'))
			Invalid(check, field);
	}

	static void ValidateSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeInForce(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateTransactTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUrgency(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateValidUntilTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or
			"9" or "B" or "C"))
			Invalid(check, field);
	}

	static void ValidateSettlDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSymbolSfx(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("CD" or "WI"))
			Invalid(check, field);
	}

	static void ValidateListID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListSeqNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInst(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocTransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateRefAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgPxPrecision(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePositionEffect(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'D' or 'F' or 'N' or 'O' or 'R'))
			Invalid(check, field);
	}

	static void ValidateNoAllocs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccount(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProcessCode(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateNoRpts(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRptSeq(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDlvyInst(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateAllocRejCode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateSignature(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureDataLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureData(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSignatureLength(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEmailType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRawDataLength(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRawData(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossResend(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateStopPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDestination(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlRejReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 18 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 99))
			Invalid(check, field);
	}

	static void ValidateOrdRejReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 18 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateIOIQualifier(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'B' or 'C' or 'D' or 'I' or 'L' or 'M' or 'O' or 'P' or
			'Q' or 'R' or 'S' or 'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateIssuer(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeartBtInt(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxFloor(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTestReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReportToExch(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocateReqd(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetMoney(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateForexReq(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigSendingTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGapFillFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoExecs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExpireTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDKReason(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateDeliverToCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOINaturalFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMiscFees(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeCurr(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "10" or "11" or "12" or "13" or "14" or "2" or "3" or
			"4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(check, field);
	}

	static void ValidatePrevClosePx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateResetSeqNumFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderLocationID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetLocationID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfLocationID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToLocationID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRelatedSym(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubject(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeadline(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateURLLink(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or
			'B' or 'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'I' or 'J' or 'K' or 'L'))
			Invalid(check, field);
	}

	static void ValidateLeavesQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashOrderQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAvgPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocNetMoney(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRateCalc(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('D' or 'M'))
			Invalid(check, field);
	}

	static void ValidateNumDaysInterest(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstMode(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateAllocText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstTransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(check, field);
	}

	static void ValidateEmailThreadID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("?" or "ABS" or "AMENDED" or "AN" or "BA" or "BDN" or "BN" or
			"BOX" or "BRADY" or "BRIDGE" or "BUYSELL" or "CAMM" or "CAN" or "CASH" or "CB" or "CD" or "CDS" or
			"CL" or "CMB" or "CMBS" or "CMO" or "COFO" or "COFP" or "CORP" or "CP" or "CPP" or "CS" or "CTB" or
			"DEFLTED" or "DINP" or "DN" or "DUAL" or "EUCD" or "EUCORP" or "EUCP" or "EUFRN" or "EUSOV" or
			"EUSUPRA" or "FAC" or "FADN" or "FOR" or "FORWARD" or "FRN" or "FUT" or "FXFWD" or "FXNDF" or
			"FXSPOT" or "FXSWAP" or "GO" or "IET" or "IRS" or "LOFC" or "LQN" or "MATURED" or "MBS" or "MF" or
			"MIO" or "MLEG" or "MPO" or "MPP" or "MPT" or "MT" or "MTN" or "NONE" or "ONITE" or "OOC" or "OOF" or
			"OOP" or "OPT" or "PEF" or "PFAND" or "PN" or "PROV" or "PS" or "PZFJ" or "RAN" or "REPLACD" or
			"REPO" or "RETIRED" or "REV" or "RVLV" or "RVLVTRM" or "SECLOAN" or "SECPLEDGE" or "SLQN" or "SPCLA" or
			"SPCLO" or "SPCLT" or "STN" or "STRUCT" or "SUPRA" or "SWING" or "TAN" or "TAXA" or "TB" or "TBA" or
			"TBILL" or "TBOND" or "TCAL" or "TD" or "TECP" or "TERM" or "TINT" or "TIPS" or "TLQN" or "TMCP" or
			"TNOTE" or "TPRN" or "TRAN" or "UST" or "USTB" or "VRDN" or "WAR" or "WITHDRN" or "XCN" or "XLINKD" or
			"YANK" or "YCD"))
			Invalid(check, field);
	}

	static void ValidateEffectiveTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateStandInstDbName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDeliveryType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateBidSpotRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSpotRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDate2(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastSpotRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSecondaryOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoIOIQualifiers(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePutOrCall(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateStrikePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCoveredOrUncovered(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateOptAttribute(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityExchange(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNotifyBrokerOfCredit(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocHandlInst(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMaxShow(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegOffsetValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlDataLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlData(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRoutingIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRoutingType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateRoutingID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSpread(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurveCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurveName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("EONIA" or "EUREPO" or "Euribor" or "FutureSWAP" or "LIBID" or
			"LIBOR" or "MuniAAA" or "OTHER" or "Pfandbriefe" or "SONIA" or "SWAP" or "Treasury"))
			Invalid(check, field);
	}

	static void ValidateBenchmarkCurvePoint(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCouponRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCouponPaymentDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIssueDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepurchaseTerm(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepurchaseRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFactor(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeOriginationDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContractMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoStipulations(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStipulationType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("ABS" or "AMT" or "AUTOREINV" or "AVAILQTY" or "AVFICO" or
			"AVSIZE" or "BANKQUAL" or "BGNCON" or "BROKERCREDIT" or "COUPON" or "CPP" or "CPR" or "CPY" or
			"CURRENCY" or "CUSTOMDATE" or "DISCOUNT" or "GEOG" or "HAIRCUT" or "HEP" or "INSURED" or "INTERNALPX" or
			"INTERNALQTY" or "ISSUE" or "ISSUER" or "ISSUESIZE" or "LEAVEQTY" or "LOOKBACK" or "LOT" or "LOTVAR" or
			"MAT" or "MATURITY" or "MAXBAL" or "MAXORDQTY" or "MAXSUBS" or "MHP" or "MINDNOM" or "MININCR" or
			"MINQTY" or "MPR" or "ORDRINCR" or "PAYFREQ" or "PIECES" or "PMAX" or "POOL" or "PPC" or "PPL" or
			"PPM" or "PPT" or "PRICE" or "PRICEFREQ" or "PRIMARY" or "PROD" or "PROTECT" or "PSA" or "PURPOSE" or
			"PXSOURCE" or "RATING" or "REDEMPTION" or "REFINT" or "REFPRIN" or "REFTRADE" or "RESTRICTED" or
			"ROLLTYPE" or "SALESCREDITOVR" or "SECTOR" or "SECTYPE" or "SMM" or "STRUCT" or "SUBSFREQ" or
			"SUBSLEFT" or "TEXT" or "TRADERCREDIT" or "TRDVAR" or "WAC" or "WAL" or "WALA" or "WAM" or "WHOLE" or
			"YIELD" or "YTM"))
			Invalid(check, field);
	}

	static void ValidateStipulationValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("AFTERTAX" or "ANNUAL" or "ATISSUE" or "AVGMATURITY" or "BOOK" or
			"CALL" or "CHANGE" or "CLOSE" or "COMPOUND" or "CURRENT" or "GOVTEQUIV" or "GROSS" or "INFLATION" or
			"INVERSEFLOATER" or "LASTCLOSE" or "LASTMONTH" or "LASTQUARTER" or "LASTYEAR" or "LONGAVGLIFE" or
			"MARK" or "MATURITY" or "NEXTREFUND" or "OPENAVG" or "PREVCLOSE" or "PROCEEDS" or "PUT" or
			"SEMIANNUAL" or "SHORTAVGLIFE" or "SIMPLE" or "TAXEQUIV" or "TENDER" or "TRUE" or "VALUE1_32" or "WORST"))
			Invalid(check, field);
	}

	static void ValidateYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalTakedown(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConcession(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRepoCollateralSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRedemptionDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCouponPaymentDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIssueDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepoCollateralSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepurchaseTerm(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRepurchaseRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingFactor(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRedemptionDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCouponPaymentDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIssueDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepoCollateralSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepurchaseTerm(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRepurchaseRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegFactor(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRedemptionDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCreditRating(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCreditRating(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCreditRating(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradedFlatSwitch(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBasisFeatureDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBasisFeaturePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubscriptionRequestType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMarketDepth(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateAggregatedBook(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntryTypes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntries(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'J' or 'K' or 'L' or 'M' or 'N' or 'O' or
			'P' or 'Q' or 'R' or 'S' or 'T' or 'U' or 'V' or 'W' or 'X' or 'Y' or 'Z' or 'a'))
			Invalid(check, field);
	}

	static void ValidateMDEntryPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryTime(Fix50Context context, FixCheck check, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTickDirection(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateMDMkt(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteCondition(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "A" or "B" or "C" or "D" or
					"E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or "P" or "Q" or
					"R" or "S" or "T" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or
					"e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or "p" or "q" or
					"r" or "s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateTradeCondition(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("0" or "1" or "2" or "3" or "4" or "A" or "AA" or "AB" or "AC" or "AD" or "AE" or
					"AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or "AL" or "AM" or "AN" or "AO" or "AP" or "AQ" or
					"AR" or "AS" or "AT" or "AV" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or
					"K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or "T" or "U" or "V" or "W" or "X" or
					"Y" or "Z" or "a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or
					"l" or "m" or "n" or "o" or "p" or "q" or "r" or "s" or "t" or "u" or "v" or "w" or "x" or
					"y" or "z"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateMDEntryID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateMDEntryRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqRejReason(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C' or 'D'))
			Invalid(check, field);
	}

	static void ValidateMDEntryOriginator(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocationID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeskID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeleteReason(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateOpenCloseSettlFlag(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("0" or "1" or "2" or "3" or "4" or "5"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateSellerDays(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryBuyer(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySeller(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryPositionNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFinancialStatus(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("1" or "2" or "3"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateCorporateAction(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or
					"M" or "N" or "O" or "P" or "Q" or "R" or "S" or "T" or "U" or "V" or "W"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateDefBidSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefOfferSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteEntries(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteSets(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateQuoteCancelType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateQuoteEntryID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateQuoteResponseLevel(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateQuoteSetID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateTotNoQuoteEntries(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIssuer(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityExchange(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbolSfx(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPutOrCall(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStrikePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingOptAttribute(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateSecurityResponseID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityResponseType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateSecurityStatusReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnsolicitedIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityTradingStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 21 or
			22 or 23 or 24 or 25 or 26 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateHaltReasonInt(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateInViewOfCommon(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDueToRelated(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBuyVolume(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSellVolume(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHighPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLowPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdjustment(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "6"))
			Invalid(check, field);
	}

	static void ValidateContraTrader(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesMode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateTradSesStartTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesOpenTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesPreCloseTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesCloseTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesEndTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNumberOfOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMessageEncoding(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedIssuerLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedIssuer(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInstLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInst(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedTextLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedText(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubjectLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubject(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadlineLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadline(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocTextLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocText(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuerLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuer(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteSetValidUntilTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteEntryRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastMsgSeqNumProcessed(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefTagID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefMsgType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSessionRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or 3 or 4 or
			5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateBidRequestTransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N'))
			Invalid(check, field);
	}

	static void ValidateContraBroker(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplianceID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSolicitedFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecRestatementReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateBusinessRejectRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBusinessRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 18 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateGrossTradeAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoContraBrokers(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxMessageSize(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMsgTypes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgDirection(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('R' or 'S'))
			Invalid(check, field);
	}

	static void ValidateNoTradingSessions(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalVolumeTraded(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDiscretionInst(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(check, field);
	}

	static void ValidateDiscretionOffsetValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClientBidID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoRelatedSym(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNumTickets(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue1(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoBidDescriptors(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidDescriptorType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateBidDescriptor(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValueInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateLiquidityPctLow(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityPctHigh(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEFPTrackingError(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFairValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutsideIndexPct(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateValueOfFutures(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityIndType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateWtAverageLiquidity(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeForPhysical(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutMainCntryUIndex(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossPercent(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProgRptReqs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateProgPeriodInterval(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIncTaxInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNumBidders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidTradeType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(check, field);
	}

	static void ValidateBasisPxType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or
			'B' or 'C' or 'D' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateNoBidComponents(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCountry(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoStrikes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateDayOrderQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayCumQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayAvgPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGTBookingInst(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoStrikes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateNetGrossInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateListOrderStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateExpireDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInstType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateCxlRejResponseTo(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingCouponRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingContractMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityNumSecurities(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMultiLegReportingType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateStrikeTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusTextLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusText(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or
			'A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(check, field);
	}

	static void ValidatePartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetChgPrevDay(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or
			38 or 39 or 4 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 5 or 50 or 51 or 52 or
			53 or 54 or 55 or 56 or 57 or 58 or 59 or 6 or 60 or 61 or 62 or 63 or 64 or 65 or 66 or 67 or 68 or
			69 or 7 or 70 or 71 or 72 or 73 or 74 or 75 or 76 or 77 or 78 or 79 or 8 or 80 or 81 or 82 or 83 or
			84 or 85 or 9))
			Invalid(check, field);
	}

	static void ValidateNoPartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSecurityAltID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityAltID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityAltIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingSecurityAltID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityAltID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityAltIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProduct(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateCFICode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingProduct(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCFICode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTestMessageIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBookingRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIndividualAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRoundingDirection(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRoundingModulus(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCountryOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStateOrProvinceOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocaleOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRegistDtls(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMailingDtls(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInvestorCountryOfResidence(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentRef(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDistribPaymentMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateCashDistribCurr(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCancellationRights(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('M' or 'N' or 'O' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateMoneyLaunderingStatus(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or 'N' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateMailingInst(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTransBkdTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecPriceType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(check, field);
	}

	static void ValidateExecPriceAdjustment(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDateOfBirth(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportTransType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateCardHolderName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardNumber(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardExpDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardIssNum(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateRegistAcctType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDesignation(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTaxAdvantageType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 999))
			Invalid(check, field);
	}

	static void ValidateRegistRejReasonText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFundRenewWaiv(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('N' or 'Y'))
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentCode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentAcctNumber(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribPayRef(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashDistribAgentAcctName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCardStartDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePaymentRemitterID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistStatus(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'H' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateRegistRejReasonCode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateRegistRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistDtls(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDistribInsts(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistEmail(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDistribPercentage(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRegistTransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateExecValuationPoint(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderPercent(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOwnershipType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('2' or 'J' or 'T'))
			Invalid(check, field);
	}

	static void ValidateNoContAmts(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContAmtType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateContAmtValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContAmtCurr(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOwnerType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidatePartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryClOrdID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryExecID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderCapacity(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'G' or 'I' or 'P' or 'R' or 'W'))
			Invalid(check, field);
	}

	static void ValidateOrderRestrictions(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
					"D" or "E" or "F"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateMassCancelRequestType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or
			'A' or 'B' or 'C'))
			Invalid(check, field);
	}

	static void ValidateMassCancelResponse(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C'))
			Invalid(check, field);
	}

	static void ValidateMassCancelRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateTotalAffectedOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoAffectedOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAffectedOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAffectedSecondaryOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNestedPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNestedPartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalAccruedInterestAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrRegistry(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashMargin(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateNestedPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateScope(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("1" or "2" or "3"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateMDImplicitDelete(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCrossPrioritization(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateOrigCrossID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSides(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateUsername(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePassword(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoSecurityTypes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSecurityTypes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateSecurityRequestResult(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateRoundLot(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinTradeVol(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMultiLegRptTypeReq(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateLegPositionEffect(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCoveredOrUncovered(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesStatusRejReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 99))
			Invalid(check, field);
	}

	static void ValidateTradeRequestID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidatePreviouslyReported(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMatchStatus(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMatchType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "A1" or
			"A2" or "A3" or "A4" or "A5" or "AQ" or "M1" or "M2" or "M3" or "M4" or "M5" or "M6" or "MT" or "S1" or
			"S2" or "S3" or "S4" or "S5"))
			Invalid(check, field);
	}

	static void ValidateOddLot(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoClearingInstructions(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingInstruction(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateTradeInputSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeInputDevice(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDates(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccountType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateCustOrderCapacity(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateClOrdLinkID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMassStatusReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMassStatusReqType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateOrigOrdModTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayBookingInst(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateBookingUnit(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidatePreallocMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingCountryOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStateOrProvinceOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLocaleOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrRegistry(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCountryOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStateOrProvinceOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLocaleOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegInstrRegistry(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSymbolSfx(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegSecurityAltID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityAltID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityAltIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegProduct(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCFICode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegMaturityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStrikePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOptAttribute(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegContractMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCouponRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityExchange(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIssuer(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegIssuerLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegIssuer(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegSecurityDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedLegSecurityDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRatioQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSide(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7"))
			Invalid(check, field);
	}

	static void ValidateAllocType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateNoHops(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopSendingTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHopRefID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMidPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMidYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingFeeIndicator(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "9" or "B" or "C" or "E" or
			"F" or "H" or "I" or "L" or "M"))
			Invalid(check, field);
	}

	static void ValidateWorkingIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLastPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorityIndicator(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidatePriceImprovement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrice2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastForwardPoints2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidForwardPoints2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferForwardPoints2(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRFQReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMktBidPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMktOfferPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinBidSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinOfferSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteStatusReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegalConfirm(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLastPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLastQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraLegRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrBidFxRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrOfferFxRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRequestRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateSideComplianceID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAcctIDSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 99))
			Invalid(check, field);
	}

	static void ValidateAllocAcctIDSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateConfirmTransType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateContractSettlMonth(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryForm(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateLastParPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegAllocs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocAccount(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIndividualAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocAcctIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSettlCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurveCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurveName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkCurvePoint(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBenchmarkPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBidPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegIOIQty(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLegStipulations(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOfferPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOrderQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStipulationType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStipulationValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSwapType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidatePool(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuotePriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateQuoteRespID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRespType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateQuoteQualifier(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldRedemptionPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReversalIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateYieldCalcDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoPositions(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("ALC" or "AS" or "ASF" or "CAA" or "CEA" or "DLT" or "DLV" or
			"DN" or "EP" or "ETR" or "EX" or "FIN" or "IAS" or "IES" or "PA" or "PIT" or "PNTN" or "RCV" or "SEA" or
			"SOD" or "SPL" or "TA" or "TOT" or "TQ" or "TRF" or "TX" or "XM"))
			Invalid(check, field);
	}

	static void ValidateLongQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateShortQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosQtyStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidatePosAmtType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("ACPN" or "BANK" or "CASH" or "CMTM" or "COLAT" or "CPN" or
			"CRES" or "DLV" or "FMTM" or "IACPN" or "ICMTM" or "ICPN" or "IMTM" or "PREM" or "SETL" or "SMTM" or
			"TVAR" or "VADJ"))
			Invalid(check, field);
	}

	static void ValidatePosAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosTransType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidatePosReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyings(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintAction(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateOrigPosReqRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintRptRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingBusinessDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlSessID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("EOD" or "ETH" or "ITD" or "RTH"))
			Invalid(check, field);
	}

	static void ValidateSettlSessSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdjustmentType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateContraryInstructionIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorSpreadIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintRptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosMaintStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidatePosMaintResult(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 99))
			Invalid(check, field);
	}

	static void ValidatePosReqType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateResponseTransportType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateResponseDestination(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalNumPosReports(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePosReqResult(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(check, field);
	}

	static void ValidatePosReqStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateSettlPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriorSettlPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteQualifiers(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlCurrAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInterestAtMaturity(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegDatedDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPool(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocInterestAtMaturity(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccruedInterestAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAssignmentMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('P' or 'R'))
			Invalid(check, field);
	}

	static void ValidateAssignmentUnit(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOpenInterest(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExerciseMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'M'))
			Invalid(check, field);
	}

	static void ValidateTotNumTradeReports(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeRequestResult(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateTradeRequestStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTradeReportRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(check, field);
	}

	static void ValidateSideMultiLegReportingType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNoPosAmt(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAutoAcceptIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested2PartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmarkSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySubType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecuritySubType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegSecuritySubType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessPct(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllowableOneSidednessCurr(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTrdRegTimestamps(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestamp(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestampType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateTrdRegTimestampOrigin(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateConfirmRejReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateBookingType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateIndividualAllocRejCode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstMsgID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSettlInst(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastUpdateTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocSettlInstType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNoSettlPartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDlvyInstType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'S'))
			Invalid(check, field);
	}

	static void ValidateTerminationType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNextExpectedMsgSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrdStatusReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstReqRejCode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateSecondaryAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocReportType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (10 or 11 or 12 or 14 or 2 or 3 or 4 or 5 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateAllocReportRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocCancReplaceReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateCopyMsgIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccountType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateOrderAvgPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderBookingQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSettlPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(check, field);
	}

	static void ValidateNoNestedPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested2PartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested2PartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocIntermedReqType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateNoUsernames(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceDelta(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueMax(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueDepth(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplQueueResolution(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateApplQueueAction(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNoAltMDSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAltMDSourceID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradeReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgPxIndicator(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTradeLinkID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderInputDevice(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingTradingSessionID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingTradingSessionSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeLegRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeRule(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeAllocIndicator(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateExpirationCycle(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTrdType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or
			37 or 38 or 39 or 4 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 5 or 50 or 51 or
			52 or 53 or 54 or 55 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateTrdSubType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 21 or
			22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or
			38 or 39 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateTransferReason(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNumAssignmentReports(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAsgnRptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateThresholdAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegMoveType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidatePegOffsetType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidatePegLimitType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidatePegRoundDirection(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidatePeggedPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegScope(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateDiscretionMoveType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateDiscretionOffsetType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateDiscretionLimitType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateDiscretionRoundDirection(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateDiscretionPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDiscretionScope(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateTargetStrategy(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTargetStrategyParameters(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateParticipationRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetStrategyPerformance(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastLiquidityInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidatePublishTrdIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateShortSaleReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateQtyType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateSecondaryTrdType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeReportType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(check, field);
	}

	static void ValidateAllocNoOrdersType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSharedCommission(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateConfirmReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgParPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReportedPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoCapacities(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderCapacityQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoEvents(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 3 or 4 or
			5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateEventDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEventText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePctAtRisk(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoInstrAttrib(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrAttribType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateInstrAttribValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDatedDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInterestAccrualDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCPProgram(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateCPRegType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCPProgram(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCPRegType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdMatchID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradeReportRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingDirtyPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingEndPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStartValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCurrentValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingEndValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingStips(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStipType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStipValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityNetMoney(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeBasis(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateTotNoAllocs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastFragment(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateCollInquiryQualifier(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateNoTrades(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarginRatio(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarginExcess(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalNetValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashOutstanding(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnTransType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCollRespID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAsgnRespType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateCollAsgnRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 99))
			Invalid(check, field);
	}

	static void ValidateCollAsgnRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollRptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollInquiryID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateTotNumReports(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastRptRequested(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAgreementCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliveryType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateEndAccruedInterestAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartCash(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndCash(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserRequestID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNewPassword(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUserStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateUserStatusText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStatusValue(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateStatusText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefCompID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkResponseID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkRequestID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastNetworkResponseID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkRequestType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 4 or 8))
			Invalid(check, field);
	}

	static void ValidateNoCompIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetworkStatusResponseType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoCollInquiryQualifier(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRptStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 3))
			Invalid(check, field);
	}

	static void ValidateAffirmStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateUnderlyingStrikeCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegStrikeCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeBracket(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollAction(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateCollInquiryStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateCollInquiryResult(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateStrikeCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested3PartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested3PartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested3PartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegContractSettlMonth(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegInterestAccrualDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoStrategyParameters(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrategyParameterName(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrategyParameterType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateStrategyParameterValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHostCrossID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTimeInForce(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReportID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityReportID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityStatus(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2"))
			Invalid(check, field);
	}

	static void ValidateSettleOnOpenFlag(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrikeMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrikeValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinPriceIncrement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePositionLimit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNTPositionLimit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingAllocationPercent(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCashAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCashType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("DIFF" or "FIXED"))
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlementType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (2 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateQuantityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContIntRptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLateIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInputSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityUpdateAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'D' or 'M'))
			Invalid(check, field);
	}

	static void ValidateNoExpiration(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExpirationQtyType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateExpQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingAmounts(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPayAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCollectAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlementDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlementStatus(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryIndividualAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRndPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIndividualAllocType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateAllocCustomerCapacity(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTierCode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("Alw" or "Bbl" or "Bcf" or "Bu" or "Gal" or "MMBtu" or "MMbbl" or
			"MWh" or "USD" or "lbs" or "oz_tr" or "t" or "tn"))
			Invalid(check, field);
	}

	static void ValidateTimeUnit(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("D" or "H" or "Min" or "Mo" or "S" or "Wk" or "Yr"))
			Invalid(check, field);
	}

	static void ValidateUnderlyingUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingTimeUnit(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegTimeUnit(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTradeReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideFillStationCd(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideReasonCd(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTrdSubTyp(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideLastQty(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMessageEventSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTrdRegTimestamp(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTrdRegTimestampType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideTrdRegTimestampSrc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAsOfIndicator(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateNoSideTrdRegTS(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOptionRatio(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoInstrumentParties(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrumentPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeVolume(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDBookType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMDFeedType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDPriceLevel(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDOriginType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateFirstPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySpotRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateManualOrderIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCustDirectedOrder(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReceivedDeptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCustOrderHandlingInst(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("ADD" or "AON" or "CNH" or "DIR" or "E.W" or "FOK" or "IO" or "IOC" or "LOC" or
					"LOO" or "MAC" or "MAO" or "MOC" or "MOO" or "MQT" or "NH" or "OVD" or "PEG" or "RSV" or
					"S.W" or "SCL" or "TMO" or "TS" or "WRK"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateOrderHandlingInstSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1))
			Invalid(check, field);
	}

	static void ValidateDeskType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("A" or "AR" or "D" or "IN" or "IS" or "O" or "PF" or "PR" or
			"PT" or "S" or "T"))
			Invalid(check, field);
	}

	static void ValidateDeskTypeSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1))
			Invalid(check, field);
	}

	static void ValidateDeskOrderHandlingInst(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("ADD" or "AON" or "CNH" or "DIR" or "E.W" or "FOK" or "IO" or "IOC" or "LOC" or
					"LOO" or "MAC" or "MAO" or "MOC" or "MOO" or "MQT" or "NH" or "OVD" or "PEG" or "RSV" or
					"S.W" or "SCL" or "TMO" or "TS" or "WRK"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateExecAckStatus(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingDeliveryAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCapValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSettlMethod(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFirmTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryFirmTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCollApplType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateUnderlyingAdjustedQuantity(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingFXRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingFXRateCalc(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('D' or 'M'))
			Invalid(check, field);
	}

	static void ValidateAllocPositionEffect(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(check, field);
	}

	static void ValidateDealingCapacity(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'P' or 'R'))
			Invalid(check, field);
	}

	static void ValidateInstrmtAssignmentMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('P' or 'R'))
			Invalid(check, field);
	}

	static void ValidateInstrumentPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrumentPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrumentPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateInstrumentPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePositionCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCalculatedCcyLastQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAggressorIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUndlyInstrumentParties(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrumentPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrumentPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrumentPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUndlyInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrumentPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingInstrumentPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidSwapPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSwapPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegBidForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegOfferForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSwapPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDQuoteType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateLastSwapPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideGrossTradeAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLastForwardPoints(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCalculatedCcyLastQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegGrossTradeAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityTime(Fix50Context context, FixCheck check, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefOrderIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateSecondaryDisplayQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDisplayWhen(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateDisplayMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateDisplayLowQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDisplayHighQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDisplayMinIncr(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefreshQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMatchIncrement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxPriceLevels(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePreTradeAnonymity(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceProtectionScope(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateLotType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidatePegPriceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidatePeggedRefPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateTriggerAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateTriggerPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerPriceType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateTriggerPriceTypeScope(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateTriggerPriceDirection(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('D' or 'U'))
			Invalid(check, field);
	}

	static void ValidateTriggerNewPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerOrderType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateTriggerNewQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerTradingSessionID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTriggerTradingSessionSubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderCategory(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateNoRootPartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRootPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRootPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRootPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRootPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRootPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRootPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeHandlingInstr(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateOrigTradeHandlingInstr(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigTradeDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigSecondaryTradeID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(check, field);
	}

	static void ValidateCstmApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefCstmApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTZTransactTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDestinationIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('B' or 'C' or 'D' or 'E' or 'G'))
			Invalid(check, field);
	}

	static void ValidateReportedPxDiff(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRptSys(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocClearingFeeIndicator(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefaultApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDisplayQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeSpecialInstructions(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxTradeVol(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDFeedTypes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMatchAlgorithm(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxPriceVariation(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateImpliedMarketIndicator(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateEventTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinPriceIncrementAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLowLimitPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHighLimitPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingReferencePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityGroup(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegNumber(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlementCycleNo(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideSettlCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplExtID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCcyAmt(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoSettlDetails(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlObligMode(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateSettlObligMsgID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlObligID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlObligTransType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(check, field);
	}

	static void ValidateSettlObligRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlObligSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateNoSettlOblig(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteMsgID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteEntryStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 12 or 13 or 14 or 15 or 16 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateTotNoCxldQuotes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoAccQuotes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoRejQuotes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrivateQuote(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRespondentType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateMDSubBookType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityTradingEvent(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateNoStatsIndicators(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStatsType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNoOfSecSizes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDSecSizeType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1))
			Invalid(check, field);
	}

	static void ValidateMDSecSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplBegSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplEndSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityXMLLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityXML(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityXMLSchema(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefreshIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateVolatility(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeToExpiration(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRiskFreeRate(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'P'))
			Invalid(check, field);
	}

	static void ValidateExerciseStyle(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateOptPayoutAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceQuoteMethod(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("INT" or "INX" or "PCTPAR" or "STD"))
			Invalid(check, field);
	}

	static void ValidateValuationMethod(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("CDS" or "CDSD" or "EQTY" or "FUT" or "FUTDA"))
			Invalid(check, field);
	}

	static void ValidateListMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateCapPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFloorPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoStrikeRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartStrikePxRange(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndStrikePxRange(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrikeIncrement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTickRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartTickPriceRange(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndTickPriceRange(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTickIncrement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTickRuleType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateNestedInstrAttribType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNestedInstrAttribValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegMaturityTime(Fix50Context context, FixCheck check, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityTime(Fix50Context context, FixCheck check, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSymbolSfx(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDerivativeSecurityAltID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityAltID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityAltIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryLowLimitPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityRuleID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStrikeRuleID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeOptPayAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProductComplex(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeProductComplex(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYearIncrement(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryHighLimitPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinLotSize(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoExecInstRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoLotTypeRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMatchRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMaturityRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoOrdTypeRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTimeInForceRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryTradingReferencePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStartMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFlexProductEligibilityIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivFlexProductEligibilityIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFlexibleIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeProduct(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityGroup(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeCFICode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecuritySubType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeMaturityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeMaturityTime(Fix50Context context, FixCheck check, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSettleOnOpenFlag(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrmtAssignmentMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityStatus(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrRegistry(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeCountryOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeStateOrProvinceOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeLocaleOfIssue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeStrikePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeStrikeCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeStrikeMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeStrikeValue(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeOptAttribute(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeContractMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeMinPriceIncrement(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeMinPriceIncrementAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeTimeUnit(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityExchange(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativePositionLimit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeNTPositionLimit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeIssuer(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeIssueDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEncodedIssuerLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEncodedIssuer(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEncodedSecurityDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEncodedSecurityDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityXMLLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityXML(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSecurityXMLSchema(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeContractSettlMonth(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDerivativeEvents(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEventType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEventDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEventTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEventPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeEventText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDerivativeInstrumentParties(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrumentPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrumentPartyIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrumentPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDerivativeInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrumentPartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrumentPartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeExerciseStyle(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarketSegmentID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarketID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYearIncrementUnits(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYearFormat(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateStrikeExerciseStyle(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecondaryPriceLimitType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceLimitType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateExecInstValue(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTradingSessionRules(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMarketSegments(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDerivativeInstrAttrib(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNestedInstrAttrib(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrAttribType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeInstrAttribValue(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativePriceUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativePriceUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeSettlMethod(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativePriceQuoteMethod(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeValuationMethod(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeListMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeCapPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeFloorPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativePutOrCall(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListUpdateAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateParentMktSegmID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesUpdateAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRejectText(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFeeMultiplier(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSymbol(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSymbolSfx(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoUnderlyingLegSecurityAltID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityAltID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityAltIDSource(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecuritySubType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegMaturityMonthYear(Fix50Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegStrikePrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityExchange(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoOfLegUnderlyings(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegPutOrCall(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegCFICode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegMaturityDate(Fix50Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplReqType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateApplResponseType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateApplTotalMessageCount(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplLastSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoApplIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplResendFlag(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplResponseID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplResponseError(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateRefApplID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefApplLastSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPutOrCall(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoFills(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoFills(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFillExecID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFillPx(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFillQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegAllocSettlCurrency(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesEvent(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMassActionReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNotAffectedOrders(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNotAffectedOrderID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNotAffOrigClOrdID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMassActionType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMassActionScope(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateMassActionResponse(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateMassActionRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(check, field);
	}

	static void ValidateMultilegModel(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateMultilegPriceMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateLegVolatility(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDividendYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegDividendYield(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCurrencyRatio(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegCurrencyRatio(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegExecInst(Fix50Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContingencyType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateListRejectReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 11 or 2 or 4 or 5 or 6 or 99))
			Invalid(check, field);
	}

	static void ValidateNoTrdRepIndicators(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRepPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTrdRepIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradePublishIndicator(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegOptAttribute(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegSecurityDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarketReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarketReportID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMarketUpdateAction(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'D' or 'M'))
			Invalid(check, field);
	}

	static void ValidateMarketSegmentDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedMktSegmDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedMktSegmDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplNewSeqNum(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptedPasswordMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptedPasswordLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptedPassword(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptedNewPasswordLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptedNewPassword(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingLegMaturityTime(Fix50Context context, FixCheck check, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefApplExtID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefaultApplExtID(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefaultCstmApplVerID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSessionStatus(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateDefaultVerIndicator(Fix50Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested4PartySubIDType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested4PartySubID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested4PartySubIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNested4PartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested4PartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested4PartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNested4PartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegLastQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingExerciseStyle(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegExerciseStyle(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPriceUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLegPriceUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPriceUnitOfMeasure(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPriceUnitOfMeasureQty(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateApplReportType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateSideExecID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderDelay(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderDelayUnit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateVenueType(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('E' or 'P' or 'X'))
			Invalid(check, field);
	}

	static void ValidateRefOrdIDReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateOrigCustOrderCapacity(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateRefApplReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateModelType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateContractMultiplierUnit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateLegContractMultiplierUnit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingContractMultiplierUnit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeContractMultiplierUnit(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFlowScheduleType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateLegFlowScheduleType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingFlowScheduleType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDerivativeFlowScheduleType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFillLiquidityInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideLiquidityInd(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRateSources(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRateSourceField(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(check, field);
	}

	static void ValidateRateSourceType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateReferencePage(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRestructuringType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("FR" or "MM" or "MR" or "XR"))
			Invalid(check, field);
	}

	static void ValidateSeniority(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("SB" or "SD" or "SR"))
			Invalid(check, field);
	}

	static void ValidateNotionalPercentageOutstanding(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOriginalNotionalPercentageOutstanding(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingRestructuringType(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSeniority(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingNotionalPercentageOutstanding(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingOriginalNotionalPercentageOutstanding(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAttachmentPoint(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDetachmentPoint(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingAttachmentPoint(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingDetachmentPoint(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoTargetPartyIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetPartyID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetPartyIDSource(Fix50Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetPartyRole(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListDesc(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityListDescLen(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityListDesc(Fix50Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityListType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateSecurityListTypeSource(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNewsID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNewsCategory(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 99))
			Invalid(check, field);
	}

	static void ValidateLanguageCode(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoNewsRefIDs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNewsRefID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNewsRefType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateStrikePriceDeterminationMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateStrikePriceBoundaryMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateStrikePriceBoundaryPrecision(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPriceDeterminationMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateOptPayoutType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateNoComplexEvents(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateComplexOptPayoutAmount(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventPrice(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventPriceBoundaryMethod(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateComplexEventPriceBoundaryPrecision(Fix50Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventPriceTimeType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateComplexEventCondition(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoComplexEventDates(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventStartDate(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventEndDate(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoComplexEventTimes(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventStartTime(Fix50Context context, FixCheck check, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplexEventEndTime(Fix50Context context, FixCheck check, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStreamAsgnReqID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStreamAsgnReqType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoAsgnReqs(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDStreamID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStreamAsgnRptID(Fix50Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStreamAsgnRejReason(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 99))
			Invalid(check, field);
	}

	static void ValidateStreamAsgnAckType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateRelSymTransactTime(Fix50Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStreamAsgnType(Fix50Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}
}
