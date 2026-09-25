using System;

namespace DotGram.Finance.Fix.Fix44;

/// <summary>The check of every field of FIX 4.4 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator44
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 4.4 Account, tag 1, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Account                              { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 4.4 AdvId, tag 2, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AdvId                                { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 4.4 AdvRefID, tag 3, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AdvRefID                             { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 4.4 AdvSide, tag 4, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> AdvSide                              { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.4 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AdvTransType                         { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.4 AvgPx, tag 6, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AvgPx                                { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 4.4 BeginSeqNo, tag 7, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BeginSeqNo                           { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 4.4 BeginString, tag 8, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BeginString                          { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 4.4 BodyLength, tag 9, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BodyLength                           { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 4.4 CheckSum, tag 10, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CheckSum                             { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 4.4 ClOrdID, tag 11, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ClOrdID                              { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 4.4 Commission, tag 12, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Commission                           { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 4.4 CommType, tag 13, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> CommType                             { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.4 CumQty, tag 14, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CumQty                               { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 4.4 Currency, tag 15, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Currency                             { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 4.4 EndSeqNo, tag 16, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EndSeqNo                             { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 4.4 ExecID, tag 17, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ExecID                               { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 4.4 ExecInst, tag 18, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  ExecInst                             { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.4 ExecRefID, tag 19, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ExecRefID                            { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 4.4 HandlInst, tag 21, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> HandlInst                            { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.4 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityIDSource                     { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 4.4 IOIid, tag 23, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      IOIid                                { get; set; } = ValidateIOIid;

	/// <summary>Holds a FIX 4.4 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> IOIQltyInd                           { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.4 IOIRefID, tag 26, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      IOIRefID                             { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 4.4 IOIQty, tag 27, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      IOIQty                               { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 4.4 IOITransType, tag 28, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> IOITransType                         { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.4 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> LastCapacity                         { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.4 LastMkt, tag 30, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LastMkt                              { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 4.4 LastPx, tag 31, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastPx                               { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 4.4 LastQty, tag 32, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastQty                              { get; set; } = ValidateLastQty;

	/// <summary>Holds a FIX 4.4 LinesOfText, tag 33, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LinesOfText                          { get; set; } = ValidateLinesOfText;

	/// <summary>Holds a FIX 4.4 MsgSeqNum, tag 34, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MsgSeqNum                            { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 4.4 MsgType, tag 35, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MsgType                              { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.4 NewSeqNo, tag 36, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NewSeqNo                             { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 4.4 OrderID, tag 37, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrderID                              { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 4.4 OrderQty, tag 38, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderQty                             { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 4.4 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> OrdStatus                            { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.4 OrdType, tag 40, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> OrdType                              { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.4 OrigClOrdID, tag 41, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrigClOrdID                          { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 4.4 OrigTime, tag 42, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> OrigTime                             { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 4.4 PossDupFlag, tag 43, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   PossDupFlag                          { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 4.4 Price, tag 44, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Price                                { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 4.4 RefSeqNum, tag 45, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RefSeqNum                            { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 4.4 SecurityID, tag 48, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityID                           { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 4.4 SenderCompID, tag 49, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SenderCompID                         { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 4.4 SenderSubID, tag 50, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SenderSubID                          { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 4.4 SendingTime, tag 52, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> SendingTime                          { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 4.4 Quantity, tag 53, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Quantity                             { get; set; } = ValidateQuantity;

	/// <summary>Holds a FIX 4.4 Side, tag 54, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> Side                                 { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.4 Symbol, tag 55, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Symbol                               { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 4.4 TargetCompID, tag 56, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TargetCompID                         { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 4.4 TargetSubID, tag 57, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TargetSubID                          { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 4.4 Text, tag 58, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Text                                 { get; set; } = ValidateText;

	/// <summary>Holds a FIX 4.4 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> TimeInForce                          { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.4 TransactTime, tag 60, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TransactTime                         { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 4.4 Urgency, tag 61, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> Urgency                              { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.4 ValidUntilTime, tag 62, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> ValidUntilTime                       { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 4.4 SettlType, tag 63, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlType                            { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 4.4 SettlDate, tag 64, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      SettlDate                            { get; set; } = ValidateSettlDate;

	/// <summary>Holds a FIX 4.4 SymbolSfx, tag 65, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SymbolSfx                            { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 4.4 ListID, tag 66, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ListID                               { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 4.4 ListSeqNo, tag 67, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ListSeqNo                            { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 4.4 TotNoOrders, tag 68, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoOrders                          { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 4.4 ListExecInst, tag 69, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ListExecInst                         { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 4.4 AllocID, tag 70, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocID                              { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 4.4 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> AllocTransType                       { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.4 RefAllocID, tag 72, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RefAllocID                           { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 4.4 NoOrders, tag 73, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoOrders                             { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 4.4 AvgPxPrecision, tag 74, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AvgPxPrecision                       { get; set; } = ValidateAvgPxPrecision;

	/// <summary>Holds a FIX 4.4 TradeDate, tag 75, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      TradeDate                            { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 4.4 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> PositionEffect                       { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 4.4 NoAllocs, tag 78, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoAllocs                             { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 4.4 AllocAccount, tag 79, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocAccount                         { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 4.4 AllocQty, tag 80, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocQty                             { get; set; } = ValidateAllocQty;

	/// <summary>Holds a FIX 4.4 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> ProcessCode                          { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.4 NoRpts, tag 82, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoRpts                               { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 4.4 RptSeq, tag 83, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RptSeq                               { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 4.4 CxlQty, tag 84, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CxlQty                               { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 4.4 NoDlvyInst, tag 85, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoDlvyInst                           { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 4.4 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocStatus                          { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.4 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocRejCode                         { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.4 Signature, tag 89, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      Signature                            { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 4.4 SecureDataLen, tag 90, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecureDataLen                        { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 4.4 SecureData, tag 91, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      SecureData                           { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 4.4 SignatureLength, tag 93, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SignatureLength                      { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 4.4 EmailType, tag 94, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> EmailType                            { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.4 RawDataLength, tag 95, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RawDataLength                        { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 4.4 RawData, tag 96, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      RawData                              { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 4.4 PossResend, tag 97, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   PossResend                           { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 4.4 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncryptMethod                        { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.4 StopPx, tag 99, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   StopPx                               { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 4.4 ExDestination, tag 100, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ExDestination                        { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 4.4 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CxlRejReason                         { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.4 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   OrdRejReason                         { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.4 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> IOIQualifier                         { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.4 Issuer, tag 106, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Issuer                               { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 4.4 SecurityDesc, tag 107, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityDesc                         { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 4.4 HeartBtInt, tag 108, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   HeartBtInt                           { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 4.4 MinQty, tag 110, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MinQty                               { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 4.4 MaxFloor, tag 111, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MaxFloor                             { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 4.4 TestReqID, tag 112, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TestReqID                            { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 4.4 ReportToExch, tag 113, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ReportToExch                         { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 4.4 LocateReqd, tag 114, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   LocateReqd                           { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 4.4 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OnBehalfOfCompID                     { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OnBehalfOfSubID                      { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 4.4 QuoteID, tag 117, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteID                              { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 4.4 NetMoney, tag 118, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   NetMoney                             { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrAmt, tag 119, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SettlCurrAmt                         { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 SettlCurrency, tag 120, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlCurrency                        { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 4.4 ForexReq, tag 121, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ForexReq                             { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 4.4 OrigSendingTime, tag 122, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> OrigSendingTime                      { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 4.4 GapFillFlag, tag 123, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   GapFillFlag                          { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 4.4 NoExecs, tag 124, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoExecs                              { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 4.4 ExpireTime, tag 126, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> ExpireTime                           { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 4.4 DKReason, tag 127, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> DKReason                             { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.4 DeliverToCompID, tag 128, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      DeliverToCompID                      { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 4.4 DeliverToSubID, tag 129, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      DeliverToSubID                       { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 4.4 IOINaturalFlag, tag 130, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   IOINaturalFlag                       { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 4.4 QuoteReqID, tag 131, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteReqID                           { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 4.4 BidPx, tag 132, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidPx                                { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 4.4 OfferPx, tag 133, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferPx                              { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 4.4 BidSize, tag 134, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidSize                              { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 4.4 OfferSize, tag 135, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferSize                            { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 4.4 NoMiscFees, tag 136, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoMiscFees                           { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 4.4 MiscFeeAmt, tag 137, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MiscFeeAmt                           { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 4.4 MiscFeeCurr, tag 138, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MiscFeeCurr                          { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 4.4 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MiscFeeType                          { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.4 PrevClosePx, tag 140, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PrevClosePx                          { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 4.4 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ResetSeqNumFlag                      { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 4.4 SenderLocationID, tag 142, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SenderLocationID                     { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 4.4 TargetLocationID, tag 143, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TargetLocationID                     { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OnBehalfOfLocationID                 { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 4.4 DeliverToLocationID, tag 145, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      DeliverToLocationID                  { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 4.4 NoRelatedSym, tag 146, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoRelatedSym                         { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 4.4 Subject, tag 147, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Subject                              { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 4.4 Headline, tag 148, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Headline                             { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 4.4 URLLink, tag 149, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      URLLink                              { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 4.4 ExecType, tag 150, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> ExecType                             { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.4 LeavesQty, tag 151, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LeavesQty                            { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 4.4 CashOrderQty, tag 152, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CashOrderQty                         { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 4.4 AllocAvgPx, tag 153, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocAvgPx                           { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 4.4 AllocNetMoney, tag 154, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocNetMoney                        { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRate, tag 155, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SettlCurrFxRate                      { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlCurrFxRateCalc                  { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.4 NumDaysInterest, tag 157, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NumDaysInterest                      { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 4.4 AccruedInterestRate, tag 158, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AccruedInterestRate                  { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 4.4 AccruedInterestAmt, tag 159, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AccruedInterestAmt                   { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlInstMode                        { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.4 AllocText, tag 161, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocText                            { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 4.4 SettlInstID, tag 162, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlInstID                          { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 4.4 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlInstTransType                   { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.4 EmailThreadID, tag 164, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      EmailThreadID                        { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 4.4 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlInstSource                      { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.4 SecurityType, tag 167, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityType                         { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.4 EffectiveTime, tag 168, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> EffectiveTime                        { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 4.4 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   StandInstDbType                      { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.4 StandInstDbName, tag 170, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StandInstDbName                      { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 4.4 StandInstDbID, tag 171, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StandInstDbID                        { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 4.4 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SettlDeliveryType                    { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.4 BidSpotRate, tag 188, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidSpotRate                          { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 4.4 BidForwardPoints, tag 189, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidForwardPoints                     { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 4.4 OfferSpotRate, tag 190, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferSpotRate                        { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints, tag 191, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferForwardPoints                   { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 4.4 OrderQty2, tag 192, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderQty2                            { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 4.4 SettlDate2, tag 193, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      SettlDate2                           { get; set; } = ValidateSettlDate2;

	/// <summary>Holds a FIX 4.4 LastSpotRate, tag 194, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastSpotRate                         { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 4.4 LastForwardPoints, tag 195, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastForwardPoints                    { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 4.4 AllocLinkID, tag 196, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocLinkID                          { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 4.4 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocLinkType                        { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.4 SecondaryOrderID, tag 198, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryOrderID                     { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 NoIOIQualifiers, tag 199, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoIOIQualifiers                      { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 4.4 MaturityMonthYear, tag 200, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.MonthYear, bool> MaturityMonthYear                    { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PutOrCall                            { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.4 StrikePrice, tag 202, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   StrikePrice                          { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 4.4 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CoveredOrUncovered                   { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 OptAttribute, tag 206, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> OptAttribute                         { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 4.4 SecurityExchange, tag 207, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityExchange                     { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 4.4 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   NotifyBrokerOfCredit                 { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 4.4 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocHandlInst                       { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.4 MaxShow, tag 210, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MaxShow                              { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 4.4 PegOffsetValue, tag 211, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PegOffsetValue                       { get; set; } = ValidatePegOffsetValue;

	/// <summary>Holds a FIX 4.4 XmlDataLen, tag 212, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   XmlDataLen                           { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 4.4 XmlData, tag 213, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      XmlData                              { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 4.4 SettlInstRefID, tag 214, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlInstRefID                       { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 4.4 NoRoutingIDs, tag 215, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoRoutingIDs                         { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 4.4 RoutingType, tag 216, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RoutingType                          { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.4 RoutingID, tag 217, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RoutingID                            { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 4.4 Spread, tag 218, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Spread                               { get; set; } = ValidateSpread;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveCurrency, tag 220, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BenchmarkCurveCurrency               { get; set; } = ValidateBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveName, tag 221, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BenchmarkCurveName                   { get; set; } = ValidateBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 BenchmarkCurvePoint, tag 222, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BenchmarkCurvePoint                  { get; set; } = ValidateBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 CouponRate, tag 223, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CouponRate                           { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 4.4 CouponPaymentDate, tag 224, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      CouponPaymentDate                    { get; set; } = ValidateCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 IssueDate, tag 225, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      IssueDate                            { get; set; } = ValidateIssueDate;

	/// <summary>Holds a FIX 4.4 RepurchaseTerm, tag 226, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RepurchaseTerm                       { get; set; } = ValidateRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 RepurchaseRate, tag 227, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   RepurchaseRate                       { get; set; } = ValidateRepurchaseRate;

	/// <summary>Holds a FIX 4.4 Factor, tag 228, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Factor                               { get; set; } = ValidateFactor;

	/// <summary>Holds a FIX 4.4 TradeOriginationDate, tag 229, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      TradeOriginationDate                 { get; set; } = ValidateTradeOriginationDate;

	/// <summary>Holds a FIX 4.4 ExDate, tag 230, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      ExDate                               { get; set; } = ValidateExDate;

	/// <summary>Holds a FIX 4.4 ContractMultiplier, tag 231, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ContractMultiplier                   { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 4.4 NoStipulations, tag 232, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoStipulations                       { get; set; } = ValidateNoStipulations;

	/// <summary>Holds a FIX 4.4 StipulationType, tag 233, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StipulationType                      { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 4.4 StipulationValue, tag 234, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StipulationValue                     { get; set; } = ValidateStipulationValue;

	/// <summary>Holds a FIX 4.4 YieldType, tag 235, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      YieldType                            { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 4.4 Yield, tag 236, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Yield                                { get; set; } = ValidateYield;

	/// <summary>Holds a FIX 4.4 TotalTakedown, tag 237, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   TotalTakedown                        { get; set; } = ValidateTotalTakedown;

	/// <summary>Holds a FIX 4.4 Concession, tag 238, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Concession                           { get; set; } = ValidateConcession;

	/// <summary>Holds a FIX 4.4 RepoCollateralSecurityType, tag 239, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RepoCollateralSecurityType           { get; set; } = ValidateRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 RedemptionDate, tag 240, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      RedemptionDate                       { get; set; } = ValidateRedemptionDate;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponPaymentDate, tag 241, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      UnderlyingCouponPaymentDate          { get; set; } = ValidateUnderlyingCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 UnderlyingIssueDate, tag 242, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      UnderlyingIssueDate                  { get; set; } = ValidateUnderlyingIssueDate;

	/// <summary>Holds a FIX 4.4 UnderlyingRepoCollateralSecurityType, tag 243, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingRepoCollateralSecurityType { get; set; } = ValidateUnderlyingRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseTerm, tag 244, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UnderlyingRepurchaseTerm             { get; set; } = ValidateUnderlyingRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseRate, tag 245, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingRepurchaseRate             { get; set; } = ValidateUnderlyingRepurchaseRate;

	/// <summary>Holds a FIX 4.4 UnderlyingFactor, tag 246, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingFactor                     { get; set; } = ValidateUnderlyingFactor;

	/// <summary>Holds a FIX 4.4 UnderlyingRedemptionDate, tag 247, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      UnderlyingRedemptionDate             { get; set; } = ValidateUnderlyingRedemptionDate;

	/// <summary>Holds a FIX 4.4 LegCouponPaymentDate, tag 248, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegCouponPaymentDate                 { get; set; } = ValidateLegCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 LegIssueDate, tag 249, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegIssueDate                         { get; set; } = ValidateLegIssueDate;

	/// <summary>Holds a FIX 4.4 LegRepoCollateralSecurityType, tag 250, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegRepoCollateralSecurityType        { get; set; } = ValidateLegRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 LegRepurchaseTerm, tag 251, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegRepurchaseTerm                    { get; set; } = ValidateLegRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 LegRepurchaseRate, tag 252, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegRepurchaseRate                    { get; set; } = ValidateLegRepurchaseRate;

	/// <summary>Holds a FIX 4.4 LegFactor, tag 253, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegFactor                            { get; set; } = ValidateLegFactor;

	/// <summary>Holds a FIX 4.4 LegRedemptionDate, tag 254, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegRedemptionDate                    { get; set; } = ValidateLegRedemptionDate;

	/// <summary>Holds a FIX 4.4 CreditRating, tag 255, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CreditRating                         { get; set; } = ValidateCreditRating;

	/// <summary>Holds a FIX 4.4 UnderlyingCreditRating, tag 256, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCreditRating               { get; set; } = ValidateUnderlyingCreditRating;

	/// <summary>Holds a FIX 4.4 LegCreditRating, tag 257, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegCreditRating                      { get; set; } = ValidateLegCreditRating;

	/// <summary>Holds a FIX 4.4 TradedFlatSwitch, tag 258, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   TradedFlatSwitch                     { get; set; } = ValidateTradedFlatSwitch;

	/// <summary>Holds a FIX 4.4 BasisFeatureDate, tag 259, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      BasisFeatureDate                     { get; set; } = ValidateBasisFeatureDate;

	/// <summary>Holds a FIX 4.4 BasisFeaturePrice, tag 260, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BasisFeaturePrice                    { get; set; } = ValidateBasisFeaturePrice;

	/// <summary>Holds a FIX 4.4 MDReqID, tag 262, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDReqID                              { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 4.4 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SubscriptionRequestType              { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.4 MarketDepth, tag 264, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MarketDepth                          { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 4.4 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MDUpdateType                         { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.4 AggregatedBook, tag 266, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   AggregatedBook                       { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 4.4 NoMDEntryTypes, tag 267, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoMDEntryTypes                       { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 4.4 NoMDEntries, tag 268, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoMDEntries                          { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 4.4 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MDEntryType                          { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.4 MDEntryPx, tag 270, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MDEntryPx                            { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 4.4 MDEntrySize, tag 271, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MDEntrySize                          { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 4.4 MDEntryDate, tag 272, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      MDEntryDate                          { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 4.4 MDEntryTime, tag 273, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Time, bool>      MDEntryTime                          { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 4.4 TickDirection, tag 274, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> TickDirection                        { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.4 MDMkt, tag 275, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDMkt                                { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 4.4 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  QuoteCondition                       { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.4 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  TradeCondition                       { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.4 MDEntryID, tag 278, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDEntryID                            { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 4.4 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MDUpdateAction                       { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.4 MDEntryRefID, tag 280, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDEntryRefID                         { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 4.4 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MDReqRejReason                       { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.4 MDEntryOriginator, tag 282, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDEntryOriginator                    { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 4.4 LocationID, tag 283, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LocationID                           { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 4.4 DeskID, tag 284, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      DeskID                               { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 4.4 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> DeleteReason                         { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.4 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  OpenCloseSettlFlag                   { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 4.4 SellerDays, tag 287, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SellerDays                           { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 4.4 MDEntryBuyer, tag 288, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDEntryBuyer                         { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 4.4 MDEntrySeller, tag 289, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MDEntrySeller                        { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 4.4 MDEntryPositionNo, tag 290, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MDEntryPositionNo                    { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 4.4 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  FinancialStatus                      { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.4 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  CorporateAction                      { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.4 DefBidSize, tag 293, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DefBidSize                           { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 4.4 DefOfferSize, tag 294, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DefOfferSize                         { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 4.4 NoQuoteEntries, tag 295, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoQuoteEntries                       { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 NoQuoteSets, tag 296, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoQuoteSets                          { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 4.4 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteStatus                          { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 4.4 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteCancelType                      { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.4 QuoteEntryID, tag 299, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteEntryID                         { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 4.4 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteRejectReason                    { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.4 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteResponseLevel                   { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.4 QuoteSetID, tag 302, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteSetID                           { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 4.4 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteRequestType                     { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.4 TotNoQuoteEntries, tag 304, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoQuoteEntries                    { get; set; } = ValidateTotNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityIDSource, tag 305, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityIDSource           { get; set; } = ValidateUnderlyingSecurityIDSource;

	/// <summary>Holds a FIX 4.4 UnderlyingIssuer, tag 306, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingIssuer                     { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityDesc               { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityExchange           { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityID                 { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityType               { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbol, tag 311, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSymbol                     { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSymbolSfx                  { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.MonthYear, bool> UnderlyingMaturityMonthYear          { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UnderlyingPutOrCall                  { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingStrikePrice                { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 4.4 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> UnderlyingOptAttribute               { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrency, tag 318, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCurrency                   { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 4.4 SecurityReqID, tag 320, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityReqID                        { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 4.4 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecurityRequestType                  { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.4 SecurityResponseID, tag 322, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityResponseID                   { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 4.4 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecurityResponseType                 { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.4 SecurityStatusReqID, tag 324, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityStatusReqID                  { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 4.4 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   UnsolicitedIndicator                 { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 4.4 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecurityTradingStatus                { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.4 HaltReason, tag 327, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> HaltReason                           { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.4 InViewOfCommon, tag 328, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   InViewOfCommon                       { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 4.4 DueToRelated, tag 329, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   DueToRelated                         { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 4.4 BuyVolume, tag 330, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BuyVolume                            { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 4.4 SellVolume, tag 331, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SellVolume                           { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 4.4 HighPx, tag 332, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   HighPx                               { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 4.4 LowPx, tag 333, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LowPx                                { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 4.4 Adjustment, tag 334, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Adjustment                           { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.4 TradSesReqID, tag 335, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradSesReqID                         { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 4.4 TradingSessionID, tag 336, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradingSessionID                     { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 4.4 ContraTrader, tag 337, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ContraTrader                         { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 4.4 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradSesMethod                        { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.4 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradSesMode                          { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.4 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradSesStatus                        { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.4 TradSesStartTime, tag 341, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TradSesStartTime                     { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 4.4 TradSesOpenTime, tag 342, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TradSesOpenTime                      { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 4.4 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TradSesPreCloseTime                  { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesCloseTime, tag 344, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TradSesCloseTime                     { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesEndTime, tag 345, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TradSesEndTime                       { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 4.4 NumberOfOrders, tag 346, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NumberOfOrders                       { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 4.4 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MessageEncoding                      { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.4 EncodedIssuerLen, tag 348, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedIssuerLen                     { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedIssuer, tag 349, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedIssuer                        { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedSecurityDescLen               { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedSecurityDesc                  { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedListExecInstLen               { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 4.4 EncodedListExecInst, tag 353, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedListExecInst                  { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 4.4 EncodedTextLen, tag 354, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedTextLen                       { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 4.4 EncodedText, tag 355, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedText                          { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 4.4 EncodedSubjectLen, tag 356, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedSubjectLen                    { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 4.4 EncodedSubject, tag 357, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedSubject                       { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 4.4 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedHeadlineLen                   { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 4.4 EncodedHeadline, tag 359, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedHeadline                      { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 4.4 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedAllocTextLen                  { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 4.4 EncodedAllocText, tag 361, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedAllocText                     { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingIssuerLen           { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingIssuer              { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingSecurityDescLen     { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingSecurityDesc        { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 AllocPrice, tag 366, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocPrice                           { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 4.4 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> QuoteSetValidUntilTime               { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 4.4 QuoteEntryRejectReason, tag 368, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteEntryRejectReason               { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 4.4 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LastMsgSeqNumProcessed               { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 4.4 RefTagID, tag 371, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RefTagID                             { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 4.4 RefMsgType, tag 372, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RefMsgType                           { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 4.4 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SessionRejectReason                  { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.4 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> BidRequestTransType                  { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.4 ContraBroker, tag 375, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ContraBroker                         { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 4.4 ComplianceID, tag 376, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ComplianceID                         { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 4.4 SolicitedFlag, tag 377, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   SolicitedFlag                        { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 4.4 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ExecRestatementReason                { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.4 BusinessRejectRefID, tag 379, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BusinessRejectRefID                  { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 4.4 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BusinessRejectReason                 { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.4 GrossTradeAmt, tag 381, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   GrossTradeAmt                        { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 4.4 NoContraBrokers, tag 382, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoContraBrokers                      { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 4.4 MaxMessageSize, tag 383, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MaxMessageSize                       { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 4.4 NoMsgTypes, tag 384, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoMsgTypes                           { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 4.4 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MsgDirection                         { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.4 NoTradingSessions, tag 386, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoTradingSessions                    { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 4.4 TotalVolumeTraded, tag 387, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   TotalVolumeTraded                    { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 4.4 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> DiscretionInst                       { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetValue, tag 389, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DiscretionOffsetValue                { get; set; } = ValidateDiscretionOffsetValue;

	/// <summary>Holds a FIX 4.4 BidID, tag 390, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BidID                                { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 4.4 ClientBidID, tag 391, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ClientBidID                          { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 4.4 ListName, tag 392, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ListName                             { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 4.4 TotNoRelatedSym, tag 393, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoRelatedSym                      { get; set; } = ValidateTotNoRelatedSym;

	/// <summary>Holds a FIX 4.4 BidType, tag 394, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BidType                              { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.4 NumTickets, tag 395, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NumTickets                           { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 4.4 SideValue1, tag 396, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SideValue1                           { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 4.4 SideValue2, tag 397, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SideValue2                           { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 4.4 NoBidDescriptors, tag 398, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoBidDescriptors                     { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 4.4 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BidDescriptorType                    { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.4 BidDescriptor, tag 400, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BidDescriptor                        { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 4.4 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SideValueInd                         { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.4 LiquidityPctLow, tag 402, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LiquidityPctLow                      { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 4.4 LiquidityPctHigh, tag 403, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LiquidityPctHigh                     { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 4.4 LiquidityValue, tag 404, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LiquidityValue                       { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 4.4 EFPTrackingError, tag 405, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   EFPTrackingError                     { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 4.4 FairValue, tag 406, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   FairValue                            { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 4.4 OutsideIndexPct, tag 407, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OutsideIndexPct                      { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 4.4 ValueOfFutures, tag 408, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ValueOfFutures                       { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 4.4 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LiquidityIndType                     { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.4 WtAverageLiquidity, tag 410, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   WtAverageLiquidity                   { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 4.4 ExchangeForPhysical, tag 411, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ExchangeForPhysical                  { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 4.4 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OutMainCntryUIndex                   { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 4.4 CrossPercent, tag 413, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CrossPercent                         { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 4.4 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ProgRptReqs                          { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.4 ProgPeriodInterval, tag 415, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ProgPeriodInterval                   { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 4.4 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   IncTaxInd                            { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.4 NumBidders, tag 417, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NumBidders                           { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 4.4 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> BidTradeType                         { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 4.4 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> BasisPxType                          { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.4 NoBidComponents, tag 420, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoBidComponents                      { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 4.4 Country, tag 421, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Country                              { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 4.4 TotNoStrikes, tag 422, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoStrikes                         { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 4.4 PriceType, tag 423, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PriceType                            { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.4 DayOrderQty, tag 424, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DayOrderQty                          { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 4.4 DayCumQty, tag 425, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DayCumQty                            { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 4.4 DayAvgPx, tag 426, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DayAvgPx                             { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 4.4 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   GTBookingInst                        { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.4 NoStrikes, tag 428, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoStrikes                            { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 4.4 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ListStatusType                       { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.4 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NetGrossInd                          { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.4 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ListOrderStatus                      { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.4 ExpireDate, tag 432, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      ExpireDate                           { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 4.4 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> ListExecInstType                     { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.4 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> CxlRejResponseTo                     { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingCouponRate                 { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 4.4 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingContractMultiplier         { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 4.4 ContraTradeQty, tag 437, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ContraTradeQty                       { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 4.4 ContraTradeTime, tag 438, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> ContraTradeTime                      { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 4.4 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LiquidityNumSecurities               { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 4.4 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MultiLegReportingType                { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 StrikeTime, tag 443, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> StrikeTime                           { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 4.4 ListStatusText, tag 444, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ListStatusText                       { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 4.4 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedListStatusTextLen             { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 4.4 EncodedListStatusText, tag 446, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedListStatusText                { get; set; } = ValidateEncodedListStatusText;

	/// <summary>Holds a FIX 4.4 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> PartyIDSource                        { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 4.4 PartyID, tag 448, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PartyID                              { get; set; } = ValidatePartyID;

	/// <summary>Holds a FIX 4.4 NetChgPrevDay, tag 451, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   NetChgPrevDay                        { get; set; } = ValidateNetChgPrevDay;

	/// <summary>Holds a FIX 4.4 PartyRole, tag 452, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PartyRole                            { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 4.4 NoPartyIDs, tag 453, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoPartyIDs                           { get; set; } = ValidateNoPartyIDs;

	/// <summary>Holds a FIX 4.4 NoSecurityAltID, tag 454, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSecurityAltID                      { get; set; } = ValidateNoSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltID, tag 455, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityAltID                        { get; set; } = ValidateSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltIDSource, tag 456, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecurityAltIDSource                  { get; set; } = ValidateSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 NoUnderlyingSecurityAltID, tag 457, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoUnderlyingSecurityAltID            { get; set; } = ValidateNoUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltID, tag 458, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityAltID              { get; set; } = ValidateUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltIDSource, tag 459, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityAltIDSource        { get; set; } = ValidateUnderlyingSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 Product, tag 460, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Product                              { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 4.4 CFICode, tag 461, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CFICode                              { get; set; } = ValidateCFICode;

	/// <summary>Holds a FIX 4.4 UnderlyingProduct, tag 462, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UnderlyingProduct                    { get; set; } = ValidateUnderlyingProduct;

	/// <summary>Holds a FIX 4.4 UnderlyingCFICode, tag 463, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCFICode                    { get; set; } = ValidateUnderlyingCFICode;

	/// <summary>Holds a FIX 4.4 TestMessageIndicator, tag 464, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   TestMessageIndicator                 { get; set; } = ValidateTestMessageIndicator;

	/// <summary>Holds a FIX 4.4 BookingRefID, tag 466, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BookingRefID                         { get; set; } = ValidateBookingRefID;

	/// <summary>Holds a FIX 4.4 IndividualAllocID, tag 467, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      IndividualAllocID                    { get; set; } = ValidateIndividualAllocID;

	/// <summary>Holds a FIX 4.4 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> RoundingDirection                    { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 4.4 RoundingModulus, tag 469, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   RoundingModulus                      { get; set; } = ValidateRoundingModulus;

	/// <summary>Holds a FIX 4.4 CountryOfIssue, tag 470, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CountryOfIssue                       { get; set; } = ValidateCountryOfIssue;

	/// <summary>Holds a FIX 4.4 StateOrProvinceOfIssue, tag 471, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StateOrProvinceOfIssue               { get; set; } = ValidateStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LocaleOfIssue, tag 472, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LocaleOfIssue                        { get; set; } = ValidateLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 NoRegistDtls, tag 473, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoRegistDtls                         { get; set; } = ValidateNoRegistDtls;

	/// <summary>Holds a FIX 4.4 MailingDtls, tag 474, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MailingDtls                          { get; set; } = ValidateMailingDtls;

	/// <summary>Holds a FIX 4.4 InvestorCountryOfResidence, tag 475, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      InvestorCountryOfResidence           { get; set; } = ValidateInvestorCountryOfResidence;

	/// <summary>Holds a FIX 4.4 PaymentRef, tag 476, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PaymentRef                           { get; set; } = ValidatePaymentRef;

	/// <summary>Holds a FIX 4.4 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DistribPaymentMethod                 { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 4.4 CashDistribCurr, tag 478, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribCurr                      { get; set; } = ValidateCashDistribCurr;

	/// <summary>Holds a FIX 4.4 CommCurrency, tag 479, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CommCurrency                         { get; set; } = ValidateCommCurrency;

	/// <summary>Holds a FIX 4.4 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> CancellationRights                   { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 4.4 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MoneyLaunderingStatus                { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 4.4 MailingInst, tag 482, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MailingInst                          { get; set; } = ValidateMailingInst;

	/// <summary>Holds a FIX 4.4 TransBkdTime, tag 483, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TransBkdTime                         { get; set; } = ValidateTransBkdTime;

	/// <summary>Holds a FIX 4.4 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> ExecPriceType                        { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 4.4 ExecPriceAdjustment, tag 485, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ExecPriceAdjustment                  { get; set; } = ValidateExecPriceAdjustment;

	/// <summary>Holds a FIX 4.4 DateOfBirth, tag 486, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      DateOfBirth                          { get; set; } = ValidateDateOfBirth;

	/// <summary>Holds a FIX 4.4 TradeReportTransType, tag 487, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeReportTransType                 { get; set; } = ValidateTradeReportTransType;

	/// <summary>Holds a FIX 4.4 CardHolderName, tag 488, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CardHolderName                       { get; set; } = ValidateCardHolderName;

	/// <summary>Holds a FIX 4.4 CardNumber, tag 489, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CardNumber                           { get; set; } = ValidateCardNumber;

	/// <summary>Holds a FIX 4.4 CardExpDate, tag 490, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      CardExpDate                          { get; set; } = ValidateCardExpDate;

	/// <summary>Holds a FIX 4.4 CardIssNum, tag 491, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CardIssNum                           { get; set; } = ValidateCardIssNum;

	/// <summary>Holds a FIX 4.4 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PaymentMethod                        { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 4.4 RegistAcctType, tag 493, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistAcctType                       { get; set; } = ValidateRegistAcctType;

	/// <summary>Holds a FIX 4.4 Designation, tag 494, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Designation                          { get; set; } = ValidateDesignation;

	/// <summary>Holds a FIX 4.4 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TaxAdvantageType                     { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 4.4 RegistRejReasonText, tag 496, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistRejReasonText                  { get; set; } = ValidateRegistRejReasonText;

	/// <summary>Holds a FIX 4.4 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> FundRenewWaiv                        { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 4.4 CashDistribAgentName, tag 498, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribAgentName                 { get; set; } = ValidateCashDistribAgentName;

	/// <summary>Holds a FIX 4.4 CashDistribAgentCode, tag 499, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribAgentCode                 { get; set; } = ValidateCashDistribAgentCode;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctNumber, tag 500, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribAgentAcctNumber           { get; set; } = ValidateCashDistribAgentAcctNumber;

	/// <summary>Holds a FIX 4.4 CashDistribPayRef, tag 501, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribPayRef                    { get; set; } = ValidateCashDistribPayRef;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctName, tag 502, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CashDistribAgentAcctName             { get; set; } = ValidateCashDistribAgentAcctName;

	/// <summary>Holds a FIX 4.4 CardStartDate, tag 503, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      CardStartDate                        { get; set; } = ValidateCardStartDate;

	/// <summary>Holds a FIX 4.4 PaymentDate, tag 504, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      PaymentDate                          { get; set; } = ValidatePaymentDate;

	/// <summary>Holds a FIX 4.4 PaymentRemitterID, tag 505, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PaymentRemitterID                    { get; set; } = ValidatePaymentRemitterID;

	/// <summary>Holds a FIX 4.4 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> RegistStatus                         { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 4.4 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   RegistRejReasonCode                  { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 4.4 RegistRefID, tag 508, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistRefID                          { get; set; } = ValidateRegistRefID;

	/// <summary>Holds a FIX 4.4 RegistDtls, tag 509, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistDtls                           { get; set; } = ValidateRegistDtls;

	/// <summary>Holds a FIX 4.4 NoDistribInsts, tag 510, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoDistribInsts                       { get; set; } = ValidateNoDistribInsts;

	/// <summary>Holds a FIX 4.4 RegistEmail, tag 511, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistEmail                          { get; set; } = ValidateRegistEmail;

	/// <summary>Holds a FIX 4.4 DistribPercentage, tag 512, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DistribPercentage                    { get; set; } = ValidateDistribPercentage;

	/// <summary>Holds a FIX 4.4 RegistID, tag 513, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RegistID                             { get; set; } = ValidateRegistID;

	/// <summary>Holds a FIX 4.4 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> RegistTransType                      { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 4.4 ExecValuationPoint, tag 515, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> ExecValuationPoint                   { get; set; } = ValidateExecValuationPoint;

	/// <summary>Holds a FIX 4.4 OrderPercent, tag 516, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderPercent                         { get; set; } = ValidateOrderPercent;

	/// <summary>Holds a FIX 4.4 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> OwnershipType                        { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 4.4 NoContAmts, tag 518, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoContAmts                           { get; set; } = ValidateNoContAmts;

	/// <summary>Holds a FIX 4.4 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ContAmtType                          { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 4.4 ContAmtValue, tag 520, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ContAmtValue                         { get; set; } = ValidateContAmtValue;

	/// <summary>Holds a FIX 4.4 ContAmtCurr, tag 521, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ContAmtCurr                          { get; set; } = ValidateContAmtCurr;

	/// <summary>Holds a FIX 4.4 OwnerType, tag 522, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   OwnerType                            { get; set; } = ValidateOwnerType;

	/// <summary>Holds a FIX 4.4 PartySubID, tag 523, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PartySubID                           { get; set; } = ValidatePartySubID;

	/// <summary>Holds a FIX 4.4 NestedPartyID, tag 524, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      NestedPartyID                        { get; set; } = ValidateNestedPartyID;

	/// <summary>Holds a FIX 4.4 NestedPartyIDSource, tag 525, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> NestedPartyIDSource                  { get; set; } = ValidateNestedPartyIDSource;

	/// <summary>Holds a FIX 4.4 SecondaryClOrdID, tag 526, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryClOrdID                     { get; set; } = ValidateSecondaryClOrdID;

	/// <summary>Holds a FIX 4.4 SecondaryExecID, tag 527, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryExecID                      { get; set; } = ValidateSecondaryExecID;

	/// <summary>Holds a FIX 4.4 OrderCapacity, tag 528, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> OrderCapacity                        { get; set; } = ValidateOrderCapacity;

	/// <summary>Holds a FIX 4.4 OrderRestrictions, tag 529, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  OrderRestrictions                    { get; set; } = ValidateOrderRestrictions;

	/// <summary>Holds a FIX 4.4 MassCancelRequestType, tag 530, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MassCancelRequestType                { get; set; } = ValidateMassCancelRequestType;

	/// <summary>Holds a FIX 4.4 MassCancelResponse, tag 531, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MassCancelResponse                   { get; set; } = ValidateMassCancelResponse;

	/// <summary>Holds a FIX 4.4 MassCancelRejectReason, tag 532, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MassCancelRejectReason               { get; set; } = ValidateMassCancelRejectReason;

	/// <summary>Holds a FIX 4.4 TotalAffectedOrders, tag 533, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotalAffectedOrders                  { get; set; } = ValidateTotalAffectedOrders;

	/// <summary>Holds a FIX 4.4 NoAffectedOrders, tag 534, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoAffectedOrders                     { get; set; } = ValidateNoAffectedOrders;

	/// <summary>Holds a FIX 4.4 AffectedOrderID, tag 535, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AffectedOrderID                      { get; set; } = ValidateAffectedOrderID;

	/// <summary>Holds a FIX 4.4 AffectedSecondaryOrderID, tag 536, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AffectedSecondaryOrderID             { get; set; } = ValidateAffectedSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 QuoteType, tag 537, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteType                            { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 4.4 NestedPartyRole, tag 538, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NestedPartyRole                      { get; set; } = ValidateNestedPartyRole;

	/// <summary>Holds a FIX 4.4 NoNestedPartyIDs, tag 539, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNestedPartyIDs                     { get; set; } = ValidateNoNestedPartyIDs;

	/// <summary>Holds a FIX 4.4 TotalAccruedInterestAmt, tag 540, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   TotalAccruedInterestAmt              { get; set; } = ValidateTotalAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 MaturityDate, tag 541, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      MaturityDate                         { get; set; } = ValidateMaturityDate;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityDate, tag 542, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      UnderlyingMaturityDate               { get; set; } = ValidateUnderlyingMaturityDate;

	/// <summary>Holds a FIX 4.4 InstrRegistry, tag 543, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      InstrRegistry                        { get; set; } = ValidateInstrRegistry;

	/// <summary>Holds a FIX 4.4 CashMargin, tag 544, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> CashMargin                           { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 4.4 NestedPartySubID, tag 545, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      NestedPartySubID                     { get; set; } = ValidateNestedPartySubID;

	/// <summary>Holds a FIX 4.4 Scope, tag 546, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Multiple, bool>  Scope                                { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 4.4 MDImplicitDelete, tag 547, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   MDImplicitDelete                     { get; set; } = ValidateMDImplicitDelete;

	/// <summary>Holds a FIX 4.4 CrossID, tag 548, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CrossID                              { get; set; } = ValidateCrossID;

	/// <summary>Holds a FIX 4.4 CrossType, tag 549, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CrossType                            { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 4.4 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CrossPrioritization                  { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 4.4 OrigCrossID, tag 551, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrigCrossID                          { get; set; } = ValidateOrigCrossID;

	/// <summary>Holds a FIX 4.4 NoSides, tag 552, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSides                              { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 4.4 Username, tag 553, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Username                             { get; set; } = ValidateUsername;

	/// <summary>Holds a FIX 4.4 Password, tag 554, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Password                             { get; set; } = ValidatePassword;

	/// <summary>Holds a FIX 4.4 NoLegs, tag 555, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoLegs                               { get; set; } = ValidateNoLegs;

	/// <summary>Holds a FIX 4.4 LegCurrency, tag 556, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegCurrency                          { get; set; } = ValidateLegCurrency;

	/// <summary>Holds a FIX 4.4 TotNoSecurityTypes, tag 557, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoSecurityTypes                   { get; set; } = ValidateTotNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 NoSecurityTypes, tag 558, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSecurityTypes                      { get; set; } = ValidateNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecurityListRequestType              { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 4.4 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecurityRequestResult                { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 4.4 RoundLot, tag 561, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   RoundLot                             { get; set; } = ValidateRoundLot;

	/// <summary>Holds a FIX 4.4 MinTradeVol, tag 562, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MinTradeVol                          { get; set; } = ValidateMinTradeVol;

	/// <summary>Holds a FIX 4.4 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MultiLegRptTypeReq                   { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 4.4 LegPositionEffect, tag 564, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> LegPositionEffect                    { get; set; } = ValidateLegPositionEffect;

	/// <summary>Holds a FIX 4.4 LegCoveredOrUncovered, tag 565, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegCoveredOrUncovered                { get; set; } = ValidateLegCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 LegPrice, tag 566, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegPrice                             { get; set; } = ValidateLegPrice;

	/// <summary>Holds a FIX 4.4 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradSesStatusRejReason               { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 4.4 TradeRequestID, tag 568, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeRequestID                       { get; set; } = ValidateTradeRequestID;

	/// <summary>Holds a FIX 4.4 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeRequestType                     { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 4.4 PreviouslyReported, tag 570, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   PreviouslyReported                   { get; set; } = ValidatePreviouslyReported;

	/// <summary>Holds a FIX 4.4 TradeReportID, tag 571, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeReportID                        { get; set; } = ValidateTradeReportID;

	/// <summary>Holds a FIX 4.4 TradeReportRefID, tag 572, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeReportRefID                     { get; set; } = ValidateTradeReportRefID;

	/// <summary>Holds a FIX 4.4 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> MatchStatus                          { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 4.4 MatchType, tag 574, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MatchType                            { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 4.4 OddLot, tag 575, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   OddLot                               { get; set; } = ValidateOddLot;

	/// <summary>Holds a FIX 4.4 NoClearingInstructions, tag 576, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoClearingInstructions               { get; set; } = ValidateNoClearingInstructions;

	/// <summary>Holds a FIX 4.4 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ClearingInstruction                  { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 4.4 TradeInputSource, tag 578, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeInputSource                     { get; set; } = ValidateTradeInputSource;

	/// <summary>Holds a FIX 4.4 TradeInputDevice, tag 579, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeInputDevice                     { get; set; } = ValidateTradeInputDevice;

	/// <summary>Holds a FIX 4.4 NoDates, tag 580, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoDates                              { get; set; } = ValidateNoDates;

	/// <summary>Holds a FIX 4.4 AccountType, tag 581, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AccountType                          { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 4.4 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CustOrderCapacity                    { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 4.4 ClOrdLinkID, tag 583, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ClOrdLinkID                          { get; set; } = ValidateClOrdLinkID;

	/// <summary>Holds a FIX 4.4 MassStatusReqID, tag 584, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      MassStatusReqID                      { get; set; } = ValidateMassStatusReqID;

	/// <summary>Holds a FIX 4.4 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MassStatusReqType                    { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 4.4 OrigOrdModTime, tag 586, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> OrigOrdModTime                       { get; set; } = ValidateOrigOrdModTime;

	/// <summary>Holds a FIX 4.4 LegSettlType, tag 587, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> LegSettlType                         { get; set; } = ValidateLegSettlType;

	/// <summary>Holds a FIX 4.4 LegSettlDate, tag 588, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegSettlDate                         { get; set; } = ValidateLegSettlDate;

	/// <summary>Holds a FIX 4.4 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> DayBookingInst                       { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 4.4 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> BookingUnit                          { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 4.4 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> PreallocMethod                       { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 4.4 UnderlyingCountryOfIssue, tag 592, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCountryOfIssue             { get; set; } = ValidateUnderlyingCountryOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingStateOrProvinceOfIssue, tag 593, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingStateOrProvinceOfIssue     { get; set; } = ValidateUnderlyingStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingLocaleOfIssue, tag 594, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingLocaleOfIssue              { get; set; } = ValidateUnderlyingLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrRegistry, tag 595, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingInstrRegistry              { get; set; } = ValidateUnderlyingInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegCountryOfIssue, tag 596, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegCountryOfIssue                    { get; set; } = ValidateLegCountryOfIssue;

	/// <summary>Holds a FIX 4.4 LegStateOrProvinceOfIssue, tag 597, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegStateOrProvinceOfIssue            { get; set; } = ValidateLegStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LegLocaleOfIssue, tag 598, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegLocaleOfIssue                     { get; set; } = ValidateLegLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 LegInstrRegistry, tag 599, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegInstrRegistry                     { get; set; } = ValidateLegInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegSymbol, tag 600, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSymbol                            { get; set; } = ValidateLegSymbol;

	/// <summary>Holds a FIX 4.4 LegSymbolSfx, tag 601, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSymbolSfx                         { get; set; } = ValidateLegSymbolSfx;

	/// <summary>Holds a FIX 4.4 LegSecurityID, tag 602, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityID                        { get; set; } = ValidateLegSecurityID;

	/// <summary>Holds a FIX 4.4 LegSecurityIDSource, tag 603, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityIDSource                  { get; set; } = ValidateLegSecurityIDSource;

	/// <summary>Holds a FIX 4.4 NoLegSecurityAltID, tag 604, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoLegSecurityAltID                   { get; set; } = ValidateNoLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltID, tag 605, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityAltID                     { get; set; } = ValidateLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltIDSource, tag 606, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityAltIDSource               { get; set; } = ValidateLegSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 LegProduct, tag 607, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegProduct                           { get; set; } = ValidateLegProduct;

	/// <summary>Holds a FIX 4.4 LegCFICode, tag 608, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegCFICode                           { get; set; } = ValidateLegCFICode;

	/// <summary>Holds a FIX 4.4 LegSecurityType, tag 609, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityType                      { get; set; } = ValidateLegSecurityType;

	/// <summary>Holds a FIX 4.4 LegMaturityMonthYear, tag 610, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.MonthYear, bool> LegMaturityMonthYear                 { get; set; } = ValidateLegMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 LegMaturityDate, tag 611, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegMaturityDate                      { get; set; } = ValidateLegMaturityDate;

	/// <summary>Holds a FIX 4.4 LegStrikePrice, tag 612, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegStrikePrice                       { get; set; } = ValidateLegStrikePrice;

	/// <summary>Holds a FIX 4.4 LegOptAttribute, tag 613, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> LegOptAttribute                      { get; set; } = ValidateLegOptAttribute;

	/// <summary>Holds a FIX 4.4 LegContractMultiplier, tag 614, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegContractMultiplier                { get; set; } = ValidateLegContractMultiplier;

	/// <summary>Holds a FIX 4.4 LegCouponRate, tag 615, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegCouponRate                        { get; set; } = ValidateLegCouponRate;

	/// <summary>Holds a FIX 4.4 LegSecurityExchange, tag 616, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityExchange                  { get; set; } = ValidateLegSecurityExchange;

	/// <summary>Holds a FIX 4.4 LegIssuer, tag 617, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegIssuer                            { get; set; } = ValidateLegIssuer;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuerLen, tag 618, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedLegIssuerLen                  { get; set; } = ValidateEncodedLegIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuer, tag 619, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedLegIssuer                     { get; set; } = ValidateEncodedLegIssuer;

	/// <summary>Holds a FIX 4.4 LegSecurityDesc, tag 620, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecurityDesc                      { get; set; } = ValidateLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDescLen, tag 621, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EncodedLegSecurityDescLen            { get; set; } = ValidateEncodedLegSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDesc, tag 622, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Data, bool>      EncodedLegSecurityDesc               { get; set; } = ValidateEncodedLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 LegRatioQty, tag 623, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegRatioQty                          { get; set; } = ValidateLegRatioQty;

	/// <summary>Holds a FIX 4.4 LegSide, tag 624, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> LegSide                              { get; set; } = ValidateLegSide;

	/// <summary>Holds a FIX 4.4 TradingSessionSubID, tag 625, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradingSessionSubID                  { get; set; } = ValidateTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 AllocType, tag 626, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocType                            { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 4.4 NoHops, tag 627, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoHops                               { get; set; } = ValidateNoHops;

	/// <summary>Holds a FIX 4.4 HopCompID, tag 628, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      HopCompID                            { get; set; } = ValidateHopCompID;

	/// <summary>Holds a FIX 4.4 HopSendingTime, tag 629, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> HopSendingTime                       { get; set; } = ValidateHopSendingTime;

	/// <summary>Holds a FIX 4.4 HopRefID, tag 630, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   HopRefID                             { get; set; } = ValidateHopRefID;

	/// <summary>Holds a FIX 4.4 MidPx, tag 631, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MidPx                                { get; set; } = ValidateMidPx;

	/// <summary>Holds a FIX 4.4 BidYield, tag 632, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidYield                             { get; set; } = ValidateBidYield;

	/// <summary>Holds a FIX 4.4 MidYield, tag 633, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MidYield                             { get; set; } = ValidateMidYield;

	/// <summary>Holds a FIX 4.4 OfferYield, tag 634, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferYield                           { get; set; } = ValidateOfferYield;

	/// <summary>Holds a FIX 4.4 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ClearingFeeIndicator                 { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 4.4 WorkingIndicator, tag 636, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   WorkingIndicator                     { get; set; } = ValidateWorkingIndicator;

	/// <summary>Holds a FIX 4.4 LegLastPx, tag 637, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegLastPx                            { get; set; } = ValidateLegLastPx;

	/// <summary>Holds a FIX 4.4 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PriorityIndicator                    { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 4.4 PriceImprovement, tag 639, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PriceImprovement                     { get; set; } = ValidatePriceImprovement;

	/// <summary>Holds a FIX 4.4 Price2, tag 640, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   Price2                               { get; set; } = ValidatePrice2;

	/// <summary>Holds a FIX 4.4 LastForwardPoints2, tag 641, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastForwardPoints2                   { get; set; } = ValidateLastForwardPoints2;

	/// <summary>Holds a FIX 4.4 BidForwardPoints2, tag 642, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BidForwardPoints2                    { get; set; } = ValidateBidForwardPoints2;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints2, tag 643, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OfferForwardPoints2                  { get; set; } = ValidateOfferForwardPoints2;

	/// <summary>Holds a FIX 4.4 RFQReqID, tag 644, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RFQReqID                             { get; set; } = ValidateRFQReqID;

	/// <summary>Holds a FIX 4.4 MktBidPx, tag 645, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MktBidPx                             { get; set; } = ValidateMktBidPx;

	/// <summary>Holds a FIX 4.4 MktOfferPx, tag 646, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MktOfferPx                           { get; set; } = ValidateMktOfferPx;

	/// <summary>Holds a FIX 4.4 MinBidSize, tag 647, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MinBidSize                           { get; set; } = ValidateMinBidSize;

	/// <summary>Holds a FIX 4.4 MinOfferSize, tag 648, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MinOfferSize                         { get; set; } = ValidateMinOfferSize;

	/// <summary>Holds a FIX 4.4 QuoteStatusReqID, tag 649, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteStatusReqID                     { get; set; } = ValidateQuoteStatusReqID;

	/// <summary>Holds a FIX 4.4 LegalConfirm, tag 650, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   LegalConfirm                         { get; set; } = ValidateLegalConfirm;

	/// <summary>Holds a FIX 4.4 UnderlyingLastPx, tag 651, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingLastPx                     { get; set; } = ValidateUnderlyingLastPx;

	/// <summary>Holds a FIX 4.4 UnderlyingLastQty, tag 652, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingLastQty                    { get; set; } = ValidateUnderlyingLastQty;

	/// <summary>Holds a FIX 4.4 LegRefID, tag 654, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegRefID                             { get; set; } = ValidateLegRefID;

	/// <summary>Holds a FIX 4.4 ContraLegRefID, tag 655, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ContraLegRefID                       { get; set; } = ValidateContraLegRefID;

	/// <summary>Holds a FIX 4.4 SettlCurrBidFxRate, tag 656, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SettlCurrBidFxRate                   { get; set; } = ValidateSettlCurrBidFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrOfferFxRate, tag 657, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SettlCurrOfferFxRate                 { get; set; } = ValidateSettlCurrOfferFxRate;

	/// <summary>Holds a FIX 4.4 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteRequestRejectReason             { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 4.4 SideComplianceID, tag 659, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SideComplianceID                     { get; set; } = ValidateSideComplianceID;

	/// <summary>Holds a FIX 4.4 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AcctIDSource                         { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 4.4 AllocAcctIDSource, tag 661, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocAcctIDSource                    { get; set; } = ValidateAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 BenchmarkPrice, tag 662, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   BenchmarkPrice                       { get; set; } = ValidateBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 BenchmarkPriceType, tag 663, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BenchmarkPriceType                   { get; set; } = ValidateBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 ConfirmID, tag 664, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ConfirmID                            { get; set; } = ValidateConfirmID;

	/// <summary>Holds a FIX 4.4 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ConfirmStatus                        { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 4.4 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ConfirmTransType                     { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 4.4 ContractSettlMonth, tag 667, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.MonthYear, bool> ContractSettlMonth                   { get; set; } = ValidateContractSettlMonth;

	/// <summary>Holds a FIX 4.4 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DeliveryForm                         { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 4.4 LastParPx, tag 669, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LastParPx                            { get; set; } = ValidateLastParPx;

	/// <summary>Holds a FIX 4.4 NoLegAllocs, tag 670, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoLegAllocs                          { get; set; } = ValidateNoLegAllocs;

	/// <summary>Holds a FIX 4.4 LegAllocAccount, tag 671, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegAllocAccount                      { get; set; } = ValidateLegAllocAccount;

	/// <summary>Holds a FIX 4.4 LegIndividualAllocID, tag 672, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegIndividualAllocID                 { get; set; } = ValidateLegIndividualAllocID;

	/// <summary>Holds a FIX 4.4 LegAllocQty, tag 673, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegAllocQty                          { get; set; } = ValidateLegAllocQty;

	/// <summary>Holds a FIX 4.4 LegAllocAcctIDSource, tag 674, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegAllocAcctIDSource                 { get; set; } = ValidateLegAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 LegSettlCurrency, tag 675, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSettlCurrency                     { get; set; } = ValidateLegSettlCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveCurrency, tag 676, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurveCurrency            { get; set; } = ValidateLegBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveName, tag 677, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurveName                { get; set; } = ValidateLegBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurvePoint, tag 678, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurvePoint               { get; set; } = ValidateLegBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPrice, tag 679, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegBenchmarkPrice                    { get; set; } = ValidateLegBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPriceType, tag 680, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegBenchmarkPriceType                { get; set; } = ValidateLegBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 LegBidPx, tag 681, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegBidPx                             { get; set; } = ValidateLegBidPx;

	/// <summary>Holds a FIX 4.4 LegIOIQty, tag 682, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegIOIQty                            { get; set; } = ValidateLegIOIQty;

	/// <summary>Holds a FIX 4.4 NoLegStipulations, tag 683, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoLegStipulations                    { get; set; } = ValidateNoLegStipulations;

	/// <summary>Holds a FIX 4.4 LegOfferPx, tag 684, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegOfferPx                           { get; set; } = ValidateLegOfferPx;

	/// <summary>Holds a FIX 4.4 LegPriceType, tag 686, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegPriceType                         { get; set; } = ValidateLegPriceType;

	/// <summary>Holds a FIX 4.4 LegQty, tag 687, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LegQty                               { get; set; } = ValidateLegQty;

	/// <summary>Holds a FIX 4.4 LegStipulationType, tag 688, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegStipulationType                   { get; set; } = ValidateLegStipulationType;

	/// <summary>Holds a FIX 4.4 LegStipulationValue, tag 689, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegStipulationValue                  { get; set; } = ValidateLegStipulationValue;

	/// <summary>Holds a FIX 4.4 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LegSwapType                          { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 4.4 Pool, tag 691, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Pool                                 { get; set; } = ValidatePool;

	/// <summary>Holds a FIX 4.4 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuotePriceType                       { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 4.4 QuoteRespID, tag 693, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      QuoteRespID                          { get; set; } = ValidateQuoteRespID;

	/// <summary>Holds a FIX 4.4 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QuoteRespType                        { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 4.4 QuoteQualifier, tag 695, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> QuoteQualifier                       { get; set; } = ValidateQuoteQualifier;

	/// <summary>Holds a FIX 4.4 YieldRedemptionDate, tag 696, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      YieldRedemptionDate                  { get; set; } = ValidateYieldRedemptionDate;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPrice, tag 697, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   YieldRedemptionPrice                 { get; set; } = ValidateYieldRedemptionPrice;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPriceType, tag 698, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   YieldRedemptionPriceType             { get; set; } = ValidateYieldRedemptionPriceType;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityID, tag 699, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BenchmarkSecurityID                  { get; set; } = ValidateBenchmarkSecurityID;

	/// <summary>Holds a FIX 4.4 ReversalIndicator, tag 700, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ReversalIndicator                    { get; set; } = ValidateReversalIndicator;

	/// <summary>Holds a FIX 4.4 YieldCalcDate, tag 701, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      YieldCalcDate                        { get; set; } = ValidateYieldCalcDate;

	/// <summary>Holds a FIX 4.4 NoPositions, tag 702, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoPositions                          { get; set; } = ValidateNoPositions;

	/// <summary>Holds a FIX 4.4 PosType, tag 703, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PosType                              { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 4.4 LongQty, tag 704, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   LongQty                              { get; set; } = ValidateLongQty;

	/// <summary>Holds a FIX 4.4 ShortQty, tag 705, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ShortQty                             { get; set; } = ValidateShortQty;

	/// <summary>Holds a FIX 4.4 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosQtyStatus                         { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 4.4 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PosAmtType                           { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 4.4 PosAmt, tag 708, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PosAmt                               { get; set; } = ValidatePosAmt;

	/// <summary>Holds a FIX 4.4 PosTransType, tag 709, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosTransType                         { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 4.4 PosReqID, tag 710, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PosReqID                             { get; set; } = ValidatePosReqID;

	/// <summary>Holds a FIX 4.4 NoUnderlyings, tag 711, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoUnderlyings                        { get; set; } = ValidateNoUnderlyings;

	/// <summary>Holds a FIX 4.4 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosMaintAction                       { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 4.4 OrigPosReqRefID, tag 713, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrigPosReqRefID                      { get; set; } = ValidateOrigPosReqRefID;

	/// <summary>Holds a FIX 4.4 PosMaintRptRefID, tag 714, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PosMaintRptRefID                     { get; set; } = ValidatePosMaintRptRefID;

	/// <summary>Holds a FIX 4.4 ClearingBusinessDate, tag 715, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      ClearingBusinessDate                 { get; set; } = ValidateClearingBusinessDate;

	/// <summary>Holds a FIX 4.4 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlSessID                          { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 4.4 SettlSessSubID, tag 717, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlSessSubID                       { get; set; } = ValidateSettlSessSubID;

	/// <summary>Holds a FIX 4.4 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AdjustmentType                       { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 4.4 ContraryInstructionIndicator, tag 719, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   ContraryInstructionIndicator         { get; set; } = ValidateContraryInstructionIndicator;

	/// <summary>Holds a FIX 4.4 PriorSpreadIndicator, tag 720, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   PriorSpreadIndicator                 { get; set; } = ValidatePriorSpreadIndicator;

	/// <summary>Holds a FIX 4.4 PosMaintRptID, tag 721, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      PosMaintRptID                        { get; set; } = ValidatePosMaintRptID;

	/// <summary>Holds a FIX 4.4 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosMaintStatus                       { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 4.4 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosMaintResult                       { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 4.4 PosReqType, tag 724, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosReqType                           { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 4.4 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ResponseTransportType                { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 4.4 ResponseDestination, tag 726, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ResponseDestination                  { get; set; } = ValidateResponseDestination;

	/// <summary>Holds a FIX 4.4 TotalNumPosReports, tag 727, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotalNumPosReports                   { get; set; } = ValidateTotalNumPosReports;

	/// <summary>Holds a FIX 4.4 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosReqResult                         { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 4.4 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PosReqStatus                         { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 4.4 SettlPrice, tag 730, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SettlPrice                           { get; set; } = ValidateSettlPrice;

	/// <summary>Holds a FIX 4.4 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SettlPriceType                       { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPrice, tag 732, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingSettlPrice                 { get; set; } = ValidateUnderlyingSettlPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPriceType, tag 733, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UnderlyingSettlPriceType             { get; set; } = ValidateUnderlyingSettlPriceType;

	/// <summary>Holds a FIX 4.4 PriorSettlPrice, tag 734, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PriorSettlPrice                      { get; set; } = ValidatePriorSettlPrice;

	/// <summary>Holds a FIX 4.4 NoQuoteQualifiers, tag 735, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoQuoteQualifiers                    { get; set; } = ValidateNoQuoteQualifiers;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrency, tag 736, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocSettlCurrency                   { get; set; } = ValidateAllocSettlCurrency;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrAmt, tag 737, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocSettlCurrAmt                    { get; set; } = ValidateAllocSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 InterestAtMaturity, tag 738, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   InterestAtMaturity                   { get; set; } = ValidateInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 LegDatedDate, tag 739, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegDatedDate                         { get; set; } = ValidateLegDatedDate;

	/// <summary>Holds a FIX 4.4 LegPool, tag 740, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegPool                              { get; set; } = ValidateLegPool;

	/// <summary>Holds a FIX 4.4 AllocInterestAtMaturity, tag 741, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocInterestAtMaturity              { get; set; } = ValidateAllocInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 AllocAccruedInterestAmt, tag 742, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllocAccruedInterestAmt              { get; set; } = ValidateAllocAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 DeliveryDate, tag 743, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      DeliveryDate                         { get; set; } = ValidateDeliveryDate;

	/// <summary>Holds a FIX 4.4 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> AssignmentMethod                     { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 4.4 AssignmentUnit, tag 745, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AssignmentUnit                       { get; set; } = ValidateAssignmentUnit;

	/// <summary>Holds a FIX 4.4 OpenInterest, tag 746, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OpenInterest                         { get; set; } = ValidateOpenInterest;

	/// <summary>Holds a FIX 4.4 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> ExerciseMethod                       { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 4.4 TotNumTradeReports, tag 748, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNumTradeReports                   { get; set; } = ValidateTotNumTradeReports;

	/// <summary>Holds a FIX 4.4 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeRequestResult                   { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 4.4 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeRequestStatus                   { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 4.4 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeReportRejectReason              { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 4.4 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SideMultiLegReportingType            { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 NoPosAmt, tag 753, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoPosAmt                             { get; set; } = ValidateNoPosAmt;

	/// <summary>Holds a FIX 4.4 AutoAcceptIndicator, tag 754, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   AutoAcceptIndicator                  { get; set; } = ValidateAutoAcceptIndicator;

	/// <summary>Holds a FIX 4.4 AllocReportID, tag 755, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocReportID                        { get; set; } = ValidateAllocReportID;

	/// <summary>Holds a FIX 4.4 NoNested2PartyIDs, tag 756, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNested2PartyIDs                    { get; set; } = ValidateNoNested2PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartyID, tag 757, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Nested2PartyID                       { get; set; } = ValidateNested2PartyID;

	/// <summary>Holds a FIX 4.4 Nested2PartyIDSource, tag 758, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> Nested2PartyIDSource                 { get; set; } = ValidateNested2PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested2PartyRole, tag 759, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Nested2PartyRole                     { get; set; } = ValidateNested2PartyRole;

	/// <summary>Holds a FIX 4.4 Nested2PartySubID, tag 760, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Nested2PartySubID                    { get; set; } = ValidateNested2PartySubID;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityIDSource, tag 761, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      BenchmarkSecurityIDSource            { get; set; } = ValidateBenchmarkSecurityIDSource;

	/// <summary>Holds a FIX 4.4 SecuritySubType, tag 762, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecuritySubType                      { get; set; } = ValidateSecuritySubType;

	/// <summary>Holds a FIX 4.4 UnderlyingSecuritySubType, tag 763, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingSecuritySubType            { get; set; } = ValidateUnderlyingSecuritySubType;

	/// <summary>Holds a FIX 4.4 LegSecuritySubType, tag 764, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegSecuritySubType                   { get; set; } = ValidateLegSecuritySubType;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessPct, tag 765, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllowableOneSidednessPct             { get; set; } = ValidateAllowableOneSidednessPct;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessValue, tag 766, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AllowableOneSidednessValue           { get; set; } = ValidateAllowableOneSidednessValue;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessCurr, tag 767, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllowableOneSidednessCurr            { get; set; } = ValidateAllowableOneSidednessCurr;

	/// <summary>Holds a FIX 4.4 NoTrdRegTimestamps, tag 768, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoTrdRegTimestamps                   { get; set; } = ValidateNoTrdRegTimestamps;

	/// <summary>Holds a FIX 4.4 TrdRegTimestamp, tag 769, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> TrdRegTimestamp                      { get; set; } = ValidateTrdRegTimestamp;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TrdRegTimestampType                  { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampOrigin, tag 771, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TrdRegTimestampOrigin                { get; set; } = ValidateTrdRegTimestampOrigin;

	/// <summary>Holds a FIX 4.4 ConfirmRefID, tag 772, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ConfirmRefID                         { get; set; } = ValidateConfirmRefID;

	/// <summary>Holds a FIX 4.4 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ConfirmType                          { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 4.4 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ConfirmRejReason                     { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 4.4 BookingType, tag 775, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   BookingType                          { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 4.4 IndividualAllocRejCode, tag 776, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   IndividualAllocRejCode               { get; set; } = ValidateIndividualAllocRejCode;

	/// <summary>Holds a FIX 4.4 SettlInstMsgID, tag 777, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlInstMsgID                       { get; set; } = ValidateSettlInstMsgID;

	/// <summary>Holds a FIX 4.4 NoSettlInst, tag 778, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSettlInst                          { get; set; } = ValidateNoSettlInst;

	/// <summary>Holds a FIX 4.4 LastUpdateTime, tag 779, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Timestamp, bool> LastUpdateTime                       { get; set; } = ValidateLastUpdateTime;

	/// <summary>Holds a FIX 4.4 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocSettlInstType                   { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 4.4 NoSettlPartyIDs, tag 781, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSettlPartyIDs                      { get; set; } = ValidateNoSettlPartyIDs;

	/// <summary>Holds a FIX 4.4 SettlPartyID, tag 782, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlPartyID                         { get; set; } = ValidateSettlPartyID;

	/// <summary>Holds a FIX 4.4 SettlPartyIDSource, tag 783, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> SettlPartyIDSource                   { get; set; } = ValidateSettlPartyIDSource;

	/// <summary>Holds a FIX 4.4 SettlPartyRole, tag 784, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SettlPartyRole                       { get; set; } = ValidateSettlPartyRole;

	/// <summary>Holds a FIX 4.4 SettlPartySubID, tag 785, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlPartySubID                      { get; set; } = ValidateSettlPartySubID;

	/// <summary>Holds a FIX 4.4 SettlPartySubIDType, tag 786, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SettlPartySubIDType                  { get; set; } = ValidateSettlPartySubIDType;

	/// <summary>Holds a FIX 4.4 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> DlvyInstType                         { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 4.4 TerminationType, tag 788, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TerminationType                      { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 4.4 NextExpectedMsgSeqNum, tag 789, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NextExpectedMsgSeqNum                { get; set; } = ValidateNextExpectedMsgSeqNum;

	/// <summary>Holds a FIX 4.4 OrdStatusReqID, tag 790, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrdStatusReqID                       { get; set; } = ValidateOrdStatusReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqID, tag 791, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SettlInstReqID                       { get; set; } = ValidateSettlInstReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SettlInstReqRejCode                  { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 4.4 SecondaryAllocID, tag 793, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryAllocID                     { get; set; } = ValidateSecondaryAllocID;

	/// <summary>Holds a FIX 4.4 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocReportType                      { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 4.4 AllocReportRefID, tag 795, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AllocReportRefID                     { get; set; } = ValidateAllocReportRefID;

	/// <summary>Holds a FIX 4.4 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocCancReplaceReason               { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 4.4 CopyMsgIndicator, tag 797, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   CopyMsgIndicator                     { get; set; } = ValidateCopyMsgIndicator;

	/// <summary>Holds a FIX 4.4 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocAccountType                     { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 4.4 OrderAvgPx, tag 799, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderAvgPx                           { get; set; } = ValidateOrderAvgPx;

	/// <summary>Holds a FIX 4.4 OrderBookingQty, tag 800, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderBookingQty                      { get; set; } = ValidateOrderBookingQty;

	/// <summary>Holds a FIX 4.4 NoSettlPartySubIDs, tag 801, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoSettlPartySubIDs                   { get; set; } = ValidateNoSettlPartySubIDs;

	/// <summary>Holds a FIX 4.4 NoPartySubIDs, tag 802, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoPartySubIDs                        { get; set; } = ValidateNoPartySubIDs;

	/// <summary>Holds a FIX 4.4 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PartySubIDType                       { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNestedPartySubIDs, tag 804, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNestedPartySubIDs                  { get; set; } = ValidateNoNestedPartySubIDs;

	/// <summary>Holds a FIX 4.4 NestedPartySubIDType, tag 805, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NestedPartySubIDType                 { get; set; } = ValidateNestedPartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNested2PartySubIDs, tag 806, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNested2PartySubIDs                 { get; set; } = ValidateNoNested2PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartySubIDType, tag 807, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Nested2PartySubIDType                { get; set; } = ValidateNested2PartySubIDType;

	/// <summary>Holds a FIX 4.4 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocIntermedReqType                 { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 4.4 UnderlyingPx, tag 810, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingPx                         { get; set; } = ValidateUnderlyingPx;

	/// <summary>Holds a FIX 4.4 PriceDelta, tag 811, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PriceDelta                           { get; set; } = ValidatePriceDelta;

	/// <summary>Holds a FIX 4.4 ApplQueueMax, tag 812, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ApplQueueMax                         { get; set; } = ValidateApplQueueMax;

	/// <summary>Holds a FIX 4.4 ApplQueueDepth, tag 813, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ApplQueueDepth                       { get; set; } = ValidateApplQueueDepth;

	/// <summary>Holds a FIX 4.4 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ApplQueueResolution                  { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 4.4 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ApplQueueAction                      { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 4.4 NoAltMDSource, tag 816, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoAltMDSource                        { get; set; } = ValidateNoAltMDSource;

	/// <summary>Holds a FIX 4.4 AltMDSourceID, tag 817, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AltMDSourceID                        { get; set; } = ValidateAltMDSourceID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportID, tag 818, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryTradeReportID               { get; set; } = ValidateSecondaryTradeReportID;

	/// <summary>Holds a FIX 4.4 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AvgPxIndicator                       { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 4.4 TradeLinkID, tag 820, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeLinkID                          { get; set; } = ValidateTradeLinkID;

	/// <summary>Holds a FIX 4.4 OrderInputDevice, tag 821, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      OrderInputDevice                     { get; set; } = ValidateOrderInputDevice;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionID, tag 822, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingTradingSessionID           { get; set; } = ValidateUnderlyingTradingSessionID;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionSubID, tag 823, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingTradingSessionSubID        { get; set; } = ValidateUnderlyingTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 TradeLegRefID, tag 824, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TradeLegRefID                        { get; set; } = ValidateTradeLegRefID;

	/// <summary>Holds a FIX 4.4 ExchangeRule, tag 825, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ExchangeRule                         { get; set; } = ValidateExchangeRule;

	/// <summary>Holds a FIX 4.4 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeAllocIndicator                  { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 4.4 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ExpirationCycle                      { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 4.4 TrdType, tag 828, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TrdType                              { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 4.4 TrdSubType, tag 829, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TrdSubType                           { get; set; } = ValidateTrdSubType;

	/// <summary>Holds a FIX 4.4 TransferReason, tag 830, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TransferReason                       { get; set; } = ValidateTransferReason;

	/// <summary>Holds a FIX 4.4 TotNumAssignmentReports, tag 832, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNumAssignmentReports              { get; set; } = ValidateTotNumAssignmentReports;

	/// <summary>Holds a FIX 4.4 AsgnRptID, tag 833, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AsgnRptID                            { get; set; } = ValidateAsgnRptID;

	/// <summary>Holds a FIX 4.4 ThresholdAmount, tag 834, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ThresholdAmount                      { get; set; } = ValidateThresholdAmount;

	/// <summary>Holds a FIX 4.4 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PegMoveType                          { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 4.4 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PegOffsetType                        { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 4.4 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PegLimitType                         { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 4.4 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PegRoundDirection                    { get; set; } = ValidatePegRoundDirection;

	/// <summary>Holds a FIX 4.4 PeggedPrice, tag 839, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PeggedPrice                          { get; set; } = ValidatePeggedPrice;

	/// <summary>Holds a FIX 4.4 PegScope, tag 840, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   PegScope                             { get; set; } = ValidatePegScope;

	/// <summary>Holds a FIX 4.4 DiscretionMoveType, tag 841, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DiscretionMoveType                   { get; set; } = ValidateDiscretionMoveType;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetType, tag 842, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DiscretionOffsetType                 { get; set; } = ValidateDiscretionOffsetType;

	/// <summary>Holds a FIX 4.4 DiscretionLimitType, tag 843, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DiscretionLimitType                  { get; set; } = ValidateDiscretionLimitType;

	/// <summary>Holds a FIX 4.4 DiscretionRoundDirection, tag 844, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DiscretionRoundDirection             { get; set; } = ValidateDiscretionRoundDirection;

	/// <summary>Holds a FIX 4.4 DiscretionPrice, tag 845, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   DiscretionPrice                      { get; set; } = ValidateDiscretionPrice;

	/// <summary>Holds a FIX 4.4 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DiscretionScope                      { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 4.4 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TargetStrategy                       { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 4.4 TargetStrategyParameters, tag 848, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TargetStrategyParameters             { get; set; } = ValidateTargetStrategyParameters;

	/// <summary>Holds a FIX 4.4 ParticipationRate, tag 849, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ParticipationRate                    { get; set; } = ValidateParticipationRate;

	/// <summary>Holds a FIX 4.4 TargetStrategyPerformance, tag 850, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   TargetStrategyPerformance            { get; set; } = ValidateTargetStrategyPerformance;

	/// <summary>Holds a FIX 4.4 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   LastLiquidityInd                     { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 4.4 PublishTrdIndicator, tag 852, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   PublishTrdIndicator                  { get; set; } = ValidatePublishTrdIndicator;

	/// <summary>Holds a FIX 4.4 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   ShortSaleReason                      { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 4.4 QtyType, tag 854, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   QtyType                              { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 4.4 SecondaryTrdType, tag 855, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   SecondaryTrdType                     { get; set; } = ValidateSecondaryTrdType;

	/// <summary>Holds a FIX 4.4 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TradeReportType                      { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 4.4 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AllocNoOrdersType                    { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 4.4 SharedCommission, tag 858, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   SharedCommission                     { get; set; } = ValidateSharedCommission;

	/// <summary>Holds a FIX 4.4 ConfirmReqID, tag 859, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      ConfirmReqID                         { get; set; } = ValidateConfirmReqID;

	/// <summary>Holds a FIX 4.4 AvgParPx, tag 860, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   AvgParPx                             { get; set; } = ValidateAvgParPx;

	/// <summary>Holds a FIX 4.4 ReportedPx, tag 861, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   ReportedPx                           { get; set; } = ValidateReportedPx;

	/// <summary>Holds a FIX 4.4 NoCapacities, tag 862, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoCapacities                         { get; set; } = ValidateNoCapacities;

	/// <summary>Holds a FIX 4.4 OrderCapacityQty, tag 863, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   OrderCapacityQty                     { get; set; } = ValidateOrderCapacityQty;

	/// <summary>Holds a FIX 4.4 NoEvents, tag 864, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoEvents                             { get; set; } = ValidateNoEvents;

	/// <summary>Holds a FIX 4.4 EventType, tag 865, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   EventType                            { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 4.4 EventDate, tag 866, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      EventDate                            { get; set; } = ValidateEventDate;

	/// <summary>Holds a FIX 4.4 EventPx, tag 867, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   EventPx                              { get; set; } = ValidateEventPx;

	/// <summary>Holds a FIX 4.4 EventText, tag 868, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      EventText                            { get; set; } = ValidateEventText;

	/// <summary>Holds a FIX 4.4 PctAtRisk, tag 869, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   PctAtRisk                            { get; set; } = ValidatePctAtRisk;

	/// <summary>Holds a FIX 4.4 NoInstrAttrib, tag 870, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoInstrAttrib                        { get; set; } = ValidateNoInstrAttrib;

	/// <summary>Holds a FIX 4.4 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   InstrAttribType                      { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 4.4 InstrAttribValue, tag 872, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      InstrAttribValue                     { get; set; } = ValidateInstrAttribValue;

	/// <summary>Holds a FIX 4.4 DatedDate, tag 873, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      DatedDate                            { get; set; } = ValidateDatedDate;

	/// <summary>Holds a FIX 4.4 InterestAccrualDate, tag 874, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      InterestAccrualDate                  { get; set; } = ValidateInterestAccrualDate;

	/// <summary>Holds a FIX 4.4 CPProgram, tag 875, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CPProgram                            { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 4.4 CPRegType, tag 876, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CPRegType                            { get; set; } = ValidateCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingCPProgram, tag 877, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCPProgram                  { get; set; } = ValidateUnderlyingCPProgram;

	/// <summary>Holds a FIX 4.4 UnderlyingCPRegType, tag 878, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingCPRegType                  { get; set; } = ValidateUnderlyingCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingQty, tag 879, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingQty                        { get; set; } = ValidateUnderlyingQty;

	/// <summary>Holds a FIX 4.4 TrdMatchID, tag 880, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TrdMatchID                           { get; set; } = ValidateTrdMatchID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportRefID, tag 881, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      SecondaryTradeReportRefID            { get; set; } = ValidateSecondaryTradeReportRefID;

	/// <summary>Holds a FIX 4.4 UnderlyingDirtyPrice, tag 882, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingDirtyPrice                 { get; set; } = ValidateUnderlyingDirtyPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingEndPrice, tag 883, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingEndPrice                   { get; set; } = ValidateUnderlyingEndPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingStartValue, tag 884, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingStartValue                 { get; set; } = ValidateUnderlyingStartValue;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrentValue, tag 885, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingCurrentValue               { get; set; } = ValidateUnderlyingCurrentValue;

	/// <summary>Holds a FIX 4.4 UnderlyingEndValue, tag 886, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   UnderlyingEndValue                   { get; set; } = ValidateUnderlyingEndValue;

	/// <summary>Holds a FIX 4.4 NoUnderlyingStips, tag 887, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoUnderlyingStips                    { get; set; } = ValidateNoUnderlyingStips;

	/// <summary>Holds a FIX 4.4 UnderlyingStipType, tag 888, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingStipType                   { get; set; } = ValidateUnderlyingStipType;

	/// <summary>Holds a FIX 4.4 UnderlyingStipValue, tag 889, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingStipValue                  { get; set; } = ValidateUnderlyingStipValue;

	/// <summary>Holds a FIX 4.4 MaturityNetMoney, tag 890, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MaturityNetMoney                     { get; set; } = ValidateMaturityNetMoney;

	/// <summary>Holds a FIX 4.4 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   MiscFeeBasis                         { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 4.4 TotNoAllocs, tag 892, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNoAllocs                          { get; set; } = ValidateTotNoAllocs;

	/// <summary>Holds a FIX 4.4 LastFragment, tag 893, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   LastFragment                         { get; set; } = ValidateLastFragment;

	/// <summary>Holds a FIX 4.4 CollReqID, tag 894, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollReqID                            { get; set; } = ValidateCollReqID;

	/// <summary>Holds a FIX 4.4 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollAsgnReason                       { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 4.4 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollInquiryQualifier                 { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 NoTrades, tag 897, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoTrades                             { get; set; } = ValidateNoTrades;

	/// <summary>Holds a FIX 4.4 MarginRatio, tag 898, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MarginRatio                          { get; set; } = ValidateMarginRatio;

	/// <summary>Holds a FIX 4.4 MarginExcess, tag 899, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   MarginExcess                         { get; set; } = ValidateMarginExcess;

	/// <summary>Holds a FIX 4.4 TotalNetValue, tag 900, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   TotalNetValue                        { get; set; } = ValidateTotalNetValue;

	/// <summary>Holds a FIX 4.4 CashOutstanding, tag 901, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   CashOutstanding                      { get; set; } = ValidateCashOutstanding;

	/// <summary>Holds a FIX 4.4 CollAsgnID, tag 902, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollAsgnID                           { get; set; } = ValidateCollAsgnID;

	/// <summary>Holds a FIX 4.4 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollAsgnTransType                    { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 4.4 CollRespID, tag 904, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollRespID                           { get; set; } = ValidateCollRespID;

	/// <summary>Holds a FIX 4.4 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollAsgnRespType                     { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 4.4 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollAsgnRejectReason                 { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 4.4 CollAsgnRefID, tag 907, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollAsgnRefID                        { get; set; } = ValidateCollAsgnRefID;

	/// <summary>Holds a FIX 4.4 CollRptID, tag 908, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollRptID                            { get; set; } = ValidateCollRptID;

	/// <summary>Holds a FIX 4.4 CollInquiryID, tag 909, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      CollInquiryID                        { get; set; } = ValidateCollInquiryID;

	/// <summary>Holds a FIX 4.4 CollStatus, tag 910, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollStatus                           { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 4.4 TotNumReports, tag 911, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TotNumReports                        { get; set; } = ValidateTotNumReports;

	/// <summary>Holds a FIX 4.4 LastRptRequested, tag 912, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Boolean, bool>   LastRptRequested                     { get; set; } = ValidateLastRptRequested;

	/// <summary>Holds a FIX 4.4 AgreementDesc, tag 913, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AgreementDesc                        { get; set; } = ValidateAgreementDesc;

	/// <summary>Holds a FIX 4.4 AgreementID, tag 914, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AgreementID                          { get; set; } = ValidateAgreementID;

	/// <summary>Holds a FIX 4.4 AgreementDate, tag 915, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      AgreementDate                        { get; set; } = ValidateAgreementDate;

	/// <summary>Holds a FIX 4.4 StartDate, tag 916, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      StartDate                            { get; set; } = ValidateStartDate;

	/// <summary>Holds a FIX 4.4 EndDate, tag 917, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      EndDate                              { get; set; } = ValidateEndDate;

	/// <summary>Holds a FIX 4.4 AgreementCurrency, tag 918, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      AgreementCurrency                    { get; set; } = ValidateAgreementCurrency;

	/// <summary>Holds a FIX 4.4 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   DeliveryType                         { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 4.4 EndAccruedInterestAmt, tag 920, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   EndAccruedInterestAmt                { get; set; } = ValidateEndAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 StartCash, tag 921, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   StartCash                            { get; set; } = ValidateStartCash;

	/// <summary>Holds a FIX 4.4 EndCash, tag 922, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Decimal, bool>   EndCash                              { get; set; } = ValidateEndCash;

	/// <summary>Holds a FIX 4.4 UserRequestID, tag 923, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UserRequestID                        { get; set; } = ValidateUserRequestID;

	/// <summary>Holds a FIX 4.4 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UserRequestType                      { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 4.4 NewPassword, tag 925, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      NewPassword                          { get; set; } = ValidateNewPassword;

	/// <summary>Holds a FIX 4.4 UserStatus, tag 926, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   UserStatus                           { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 4.4 UserStatusText, tag 927, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UserStatusText                       { get; set; } = ValidateUserStatusText;

	/// <summary>Holds a FIX 4.4 StatusValue, tag 928, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   StatusValue                          { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 4.4 StatusText, tag 929, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StatusText                           { get; set; } = ValidateStatusText;

	/// <summary>Holds a FIX 4.4 RefCompID, tag 930, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RefCompID                            { get; set; } = ValidateRefCompID;

	/// <summary>Holds a FIX 4.4 RefSubID, tag 931, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      RefSubID                             { get; set; } = ValidateRefSubID;

	/// <summary>Holds a FIX 4.4 NetworkResponseID, tag 932, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      NetworkResponseID                    { get; set; } = ValidateNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestID, tag 933, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      NetworkRequestID                     { get; set; } = ValidateNetworkRequestID;

	/// <summary>Holds a FIX 4.4 LastNetworkResponseID, tag 934, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LastNetworkResponseID                { get; set; } = ValidateLastNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NetworkRequestType                   { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 4.4 NoCompIDs, tag 936, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoCompIDs                            { get; set; } = ValidateNoCompIDs;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NetworkStatusResponseType            { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 4.4 NoCollInquiryQualifier, tag 938, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoCollInquiryQualifier               { get; set; } = ValidateNoCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   TrdRptStatus                         { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 4.4 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   AffirmStatus                         { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikeCurrency, tag 941, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      UnderlyingStrikeCurrency             { get; set; } = ValidateUnderlyingStrikeCurrency;

	/// <summary>Holds a FIX 4.4 LegStrikeCurrency, tag 942, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      LegStrikeCurrency                    { get; set; } = ValidateLegStrikeCurrency;

	/// <summary>Holds a FIX 4.4 TimeBracket, tag 943, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      TimeBracket                          { get; set; } = ValidateTimeBracket;

	/// <summary>Holds a FIX 4.4 CollAction, tag 944, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollAction                           { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 4.4 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollInquiryStatus                    { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 4.4 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   CollInquiryResult                    { get; set; } = ValidateCollInquiryResult;

	/// <summary>Holds a FIX 4.4 StrikeCurrency, tag 947, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      StrikeCurrency                       { get; set; } = ValidateStrikeCurrency;

	/// <summary>Holds a FIX 4.4 NoNested3PartyIDs, tag 948, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNested3PartyIDs                    { get; set; } = ValidateNoNested3PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartyID, tag 949, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Nested3PartyID                       { get; set; } = ValidateNested3PartyID;

	/// <summary>Holds a FIX 4.4 Nested3PartyIDSource, tag 950, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Character, bool> Nested3PartyIDSource                 { get; set; } = ValidateNested3PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested3PartyRole, tag 951, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Nested3PartyRole                     { get; set; } = ValidateNested3PartyRole;

	/// <summary>Holds a FIX 4.4 NoNested3PartySubIDs, tag 952, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   NoNested3PartySubIDs                 { get; set; } = ValidateNoNested3PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartySubID, tag 953, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Text, bool>      Nested3PartySubID                    { get; set; } = ValidateNested3PartySubID;

	/// <summary>Holds a FIX 4.4 Nested3PartySubIDType, tag 954, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Integer, bool>   Nested3PartySubIDType                { get; set; } = ValidateNested3PartySubIDType;

	/// <summary>Holds a FIX 4.4 LegContractSettlMonth, tag 955, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.MonthYear, bool> LegContractSettlMonth                { get; set; } = ValidateLegContractSettlMonth;

	/// <summary>Holds a FIX 4.4 LegInterestAccrualDate, tag 956, to its type.</summary>
	public Func<Fix44Context, FixMessage, FixField.Date, bool>      LegInterestAccrualDate               { get; set; } = ValidateLegInterestAccrualDate;

	static bool ValidateAccount(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvId(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvSide(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvTransType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("C" or "N" or "R"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginSeqNo(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginString(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBodyLength(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCheckSum(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommission(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCumQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndSeqNo(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecInst(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
				"D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or "P" or "Q" or "R" or
				"S" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or "e"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateExecRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHandlInst(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
			"D" or "E" or "F" or "G" or "H" or "I" or "J"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIid(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQltyInd(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQty(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("L" or "M" or "S"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOITransType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastCapacity(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMkt(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLinesOfText(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgSeqNum(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "AA" or
			"AB" or "AC" or "AD" or "AE" or "AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or "AL" or "AM" or "AN" or
			"AO" or "AP" or "AQ" or "AR" or "AS" or "AT" or "AU" or "AV" or "AW" or "AX" or "AY" or "AZ" or "B" or
			"BA" or "BB" or "BC" or "BD" or "BE" or "BF" or "BG" or "BH" or "C" or "D" or "E" or "F" or "G" or "H" or
			"J" or "K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or "T" or "V" or "W" or "X" or "Y" or "Z" or
			"a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or
			"p" or "q" or "r" or "s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewSeqNo(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatus(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'D' or 'E' or 'G' or 'I' or
			'J' or 'K' or 'L' or 'M' or 'P'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigClOrdID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossDupFlag(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSeqNum(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSendingTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuantity(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSide(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbol(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeInForce(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransactTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUrgency(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValidUntilTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbolSfx(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListSeqNo(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoOrders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInst(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocTransType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefAllocID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOrders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxPrecision(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionEffect(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAllocs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccount(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProcessCode(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRpts(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRptSeq(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDlvyInst(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocRejCode(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignature(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureDataLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureData(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignatureLength(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawDataLength(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawData(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossResend(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptMethod(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStopPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDestination(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdRejReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQualifier(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or 'S' or
			'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssuer(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityDesc(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeartBtInt(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxFloor(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportToExch(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocateReqd(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetMoney(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateForexReq(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigSendingTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGapFillFlag(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExecs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDKReason(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOINaturalFlag(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMiscFees(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeCurr(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "10" or "11" or "12" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrevClosePx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResetSeqNumFlag(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderLocationID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetLocationID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfLocationID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToLocationID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRelatedSym(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubject(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeadline(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateURLLink(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLeavesQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOrderQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAvgPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNetMoney(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRateCalc(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumDaysInterest(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMode(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstTransType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailThreadID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ABS" or "AMENDED" or "AN" or "BA" or "BN" or "BOX" or "BRADY" or "BRIDGE" or
			"BUYSELL" or "CB" or "CD" or "CL" or "CMBS" or "CMO" or "COFO" or "COFP" or "CORP" or "CP" or "CPP" or
			"CS" or "DEFLTED" or "DINP" or "DN" or "DUAL" or "EUCD" or "EUCORP" or "EUCP" or "EUSOV" or "EUSUPRA" or
			"FAC" or "FADN" or "FOR" or "FORWARD" or "FUT" or "GO" or "IET" or "LOFC" or "LQN" or "MATURED" or "MBS" or
			"MF" or "MIO" or "MLEG" or "MPO" or "MPP" or "MPT" or "MT" or "MTN" or "NONE" or "ONITE" or "OPT" or
			"PEF" or "PFAND" or "PN" or "PS" or "PZFJ" or "RAN" or "REPLACD" or "REPO" or "RETIRED" or "REV" or
			"RVLV" or "RVLVTRM" or "SECLOAN" or "SECPLEDGE" or "SPCLA" or "SPCLO" or "SPCLT" or "STN" or "STRUCT" or
			"SUPRA" or "SWING" or "TAN" or "TAXA" or "TBA" or "TBILL" or "TBOND" or "TCAL" or "TD" or "TECP" or
			"TERM" or "TINT" or "TIPS" or "TNOTE" or "TPRN" or "TRAN" or "UST" or "USTB" or "VRDN" or "WAR" or
			"WITHDRN" or "XCN" or "XLINKD" or "YANK" or "YCD"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEffectiveTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDeliveryType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSpotRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSpotRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate2(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastSpotRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryOrderID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoIOIQualifiers(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYear(Fix44Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePutOrCall(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCoveredOrUncovered(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptAttribute(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityExchange(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotifyBrokerOfCredit(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocHandlInst(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxShow(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlDataLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlData(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRoutingIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSpread(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurvePoint(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponPaymentDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssueDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseTerm(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFactor(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeOriginationDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractMultiplier(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStipulations(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStipulationType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("AMT" or "AUTOREINV" or "BANKQUAL" or "BGNCON" or "COUPON" or "CURRENCY" or
			"CUSTOMDATE" or "GEOG" or "HAIRCUT" or "INSURED" or "ISSUE" or "ISSUER" or "ISSUESIZE" or "LOOKBACK" or
			"LOT" or "LOTVAR" or "MAT" or "MATURITY" or "MAXSUBS" or "MINDNOM" or "MININCR" or "MINQTY" or "PAYFREQ" or
			"PIECES" or "PMAX" or "PPL" or "PPM" or "PPT" or "PRICE" or "PRICEFREQ" or "PROD" or "PROTECT" or
			"PURPOSE" or "PXSOURCE" or "RATING" or "REDEMPTION" or "RESTRICTED" or "SECTOR" or "SECTYPE" or "STRUCT" or
			"SUBSFREQ" or "SUBSLEFT" or "TEXT" or "TRDVAR" or "WAC" or "WAL" or "WALA" or "WAM" or "WHOLE" or "YIELD"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStipulationValue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("AFTERTAX" or "ANNUAL" or "ATISSUE" or "AVGMATURITY" or "BOOK" or "CALL" or
			"CHANGE" or "CLOSE" or "COMPOUND" or "CURRENT" or "GOVTEQUIV" or "GROSS" or "INFLATION" or
			"INVERSEFLOATER" or "LASTCLOSE" or "LASTMONTH" or "LASTQUARTER" or "LASTYEAR" or "LONGAVGLIFE" or "MARK" or
			"MATURITY" or "NEXTREFUND" or "OPENAVG" or "PREVCLOSE" or "PROCEEDS" or "PUT" or "SEMIANNUAL" or
			"SHORTAVGLIFE" or "SIMPLE" or "TAXEQUIV" or "TENDER" or "TRUE" or "VALUE1/32" or "WORST"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYield(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalTakedown(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConcession(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepoCollateralSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRedemptionDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponPaymentDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssueDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepoCollateralSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseTerm(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFactor(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRedemptionDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponPaymentDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssueDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepoCollateralSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseTerm(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegFactor(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRedemptionDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCreditRating(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCreditRating(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCreditRating(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradedFlatSwitch(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeatureDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeaturePrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubscriptionRequestType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketDepth(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAggregatedBook(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntryTypes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntries(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryTime(Fix44Context context, FixMessage message, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickDirection(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDMkt(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCondition(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateTradeCondition(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or
				"N" or "P" or "Q" or "R"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDEntryID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateAction(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqRejReason(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryOriginator(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocationID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeleteReason(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenCloseSettlFlag(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateSellerDays(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryBuyer(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySeller(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPositionNo(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFinancialStatus(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateCorporateAction(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateDefBidSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefOfferSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteEntries(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteSets(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCancelType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteResponseLevel(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoQuoteEntries(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssuer(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityDesc(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityExchange(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbol(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbolSfx(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityMonthYear(Fix44Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPutOrCall(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikePrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingOptAttribute(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnsolicitedIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 21 or 22 or 23 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHaltReason(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInViewOfCommon(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDueToRelated(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBuyVolume(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSellVolume(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHighPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLowPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustment(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTrader(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMethod(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMode(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStartTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesOpenTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesPreCloseTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesCloseTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesEndTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumberOfOrders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEncoding(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuerLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuer(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDescLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDesc(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInstLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInst(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedTextLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedText(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubjectLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubject(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadlineLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadline(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocTextLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocText(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuerLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuer(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDescLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDesc(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetValidUntilTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMsgSeqNumProcessed(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefTagID(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefMsgType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 2 or
			3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidRequestTransType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraBroker(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplianceID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSolicitedFlag(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecRestatementReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGrossTradeAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContraBrokers(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxMessageSize(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMsgTypes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgDirection(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTradingSessions(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalVolumeTraded(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionInst(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClientBidID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoRelatedSym(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumTickets(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue1(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidDescriptors(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptorType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptor(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValueInd(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctLow(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctHigh(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEFPTrackingError(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFairValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutsideIndexPct(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValueOfFutures(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityIndType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWtAverageLiquidity(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeForPhysical(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutMainCntryUIndex(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPercent(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgRptReqs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgPeriodInterval(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIncTaxInd(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumBidders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidTradeType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisPxType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidComponents(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountry(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoStrikes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayOrderQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayCumQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayAvgPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGTBookingInst(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrikes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetGrossInd(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListOrderStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInstType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejResponseTo(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingContractMultiplier(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityNumSecurities(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegReportingType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusTextLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusText(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyIDSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetChgPrevDay(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyRole(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or
			34 or 35 or 36 or 37 or 38 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartyIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityAltID(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingSecurityAltID(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProduct(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or
			9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCFICode(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingProduct(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCFICode(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestMessageIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingDirection(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingModulus(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountryOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStateOrProvinceOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocaleOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRegistDtls(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingDtls(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInvestorCountryOfResidence(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRef(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPaymentMethod(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribCurr(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCancellationRights(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('M' or 'N' or 'O' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMoneyLaunderingStatus(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or 'N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingInst(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransBkdTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceAdjustment(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDateOfBirth(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportTransType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardHolderName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardNumber(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardExpDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardIssNum(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentMethod(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistAcctType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDesignation(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTaxAdvantageType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or
			19 or 2 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFundRenewWaiv(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentCode(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctNumber(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribPayRef(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardStartDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRemitterID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistStatus(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'H' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonCode(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or
			3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistDtls(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDistribInsts(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistEmail(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPercentage(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistTransType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecValuationPoint(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderPercent(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnershipType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('2' or 'J' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContAmts(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtCurr(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnerType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or
			9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyIDSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryClOrdID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryExecID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacity(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'I' or 'P' or 'R' or 'W'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderRestrictions(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMassCancelRequestType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelResponse(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelRejectReason(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "99"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAffectedOrders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAffectedOrders(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedOrderID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedSecondaryOrderID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyRole(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartyIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAccruedInterestAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrRegistry(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashMargin(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateScope(Fix44Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDImplicitDelete(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPrioritization(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigCrossID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSides(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUsername(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePassword(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoSecurityTypes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityTypes(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestResult(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundLot(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinTradeVol(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegRptTypeReq(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPositionEffect(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCoveredOrUncovered(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatusRejReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreviouslyReported(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchStatus(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("A1" or "A2" or "A3" or "A4" or "A5" or "AQ" or "M1" or "M2" or "M3" or "M4" or
			"M5" or "M6" or "MT" or "S1" or "S2" or "S3" or "S4" or "S5"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOddLot(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoClearingInstructions(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingInstruction(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputDevice(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDates(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccountType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustOrderCapacity(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdLinkID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigOrdModTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayBookingInst(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingUnit(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreallocMethod(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCountryOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStateOrProvinceOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLocaleOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrRegistry(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCountryOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStateOrProvinceOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLocaleOfIssue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInstrRegistry(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbol(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbolSfx(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegSecurityAltID(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegProduct(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCFICode(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityMonthYear(Fix44Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikePrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOptAttribute(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractMultiplier(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityExchange(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssuer(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuerLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuer(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityDesc(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDescLen(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDesc(Fix44Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRatioQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSide(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 5 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoHops(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopSendingTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopRefID(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidYield(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidYield(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferYield(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingFeeIndicator(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "9" or "B" or "C" or "E" or "F" or "H" or "I" or
			"L" or "M"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWorkingIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLastPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorityIndicator(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceImprovement(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints2(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRFQReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktBidPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktOfferPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinBidSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinOfferSize(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegalConfirm(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraLegRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrBidFxRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrOfferFxRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideComplianceID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAcctIDSource(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAcctIDSource(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmTransType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractSettlMonth(Fix44Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryForm(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastParPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegAllocs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAccount(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIndividualAllocID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAcctIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveName(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurvePoint(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBidPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIOIQty(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegStipulations(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOfferPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationValue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSwapType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePool(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuotePriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteQualifier(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReversalIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldCalcDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPositions(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ALC" or "AS" or "ASF" or "DLV" or "ETR" or "EX" or "FIN" or "IAS" or "IES" or
			"PA" or "PIT" or "SOD" or "SPL" or "TA" or "TOT" or "TQ" or "TRF" or "TX" or "XM"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLongQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosQtyStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmtType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("CASH" or "CRES" or "FMTM" or "IMTM" or "PREM" or "SMTM" or "TVAR" or "VADJ"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosTransType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyings(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintAction(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigPosReqRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingBusinessDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ETH" or "ITD" or "RTH"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustmentType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraryInstructionIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSpreadIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintResult(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseTransportType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseDestination(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNumPosReports(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqResult(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPriceType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSettlPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteQualifiers(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAtMaturity(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegDatedDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPool(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocInterestAtMaturity(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccruedInterestAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentMethod(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentUnit(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenInterest(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExerciseMethod(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumTradeReports(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestResult(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideMultiLegReportingType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPosAmt(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAutoAcceptIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartyIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyIDSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyRole(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityIDSource(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySubType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecuritySubType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecuritySubType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessPct(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessCurr(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrdRegTimestamps(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestamp(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampOrigin(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRejReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocRejCode(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMsgID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlInst(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastUpdateTime(Fix44Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlInstType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartyIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyIDSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyRole(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubIDType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDlvyInstType(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTerminationType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNextExpectedMsgSeqNum(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatusReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqRejCode(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryAllocID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (3 or 4 or 5 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocCancReplaceReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCopyMsgIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccountType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderAvgPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderBookingQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartySubIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartySubIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubIDType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartySubIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubIDType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartySubIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubIDType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocIntermedReqType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceDelta(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueMax(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueDepth(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueResolution(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueAction(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAltMDSource(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAltMDSourceID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxIndicator(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLinkID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderInputDevice(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLegRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeRule(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeAllocIndicator(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpirationCycle(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdSubType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransferReason(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumAssignmentReports(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAsgnRptID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateThresholdAmount(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegMoveType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegLimitType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegRoundDirection(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePeggedPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegScope(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionMoveType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionLimitType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionRoundDirection(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionScope(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategy(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyParameters(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateParticipationRate(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyPerformance(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastLiquidityInd(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePublishTrdIndicator(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortSaleReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQtyType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTrdType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNoOrdersType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSharedCommission(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgParPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportedPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCapacities(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacityQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoEvents(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventPx(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePctAtRisk(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoInstrAttrib(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribValue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDatedDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAccrualDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPProgram(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPRegType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPProgram(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPRegType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingQty(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdMatchID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingDirtyPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndPrice(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStartValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrentValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingStips(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipType(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipValue(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityNetMoney(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeBasis(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoAllocs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastFragment(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollReqID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryQualifier(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrades(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginRatio(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginExcess(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNetValue(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOutstanding(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnTransType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRespID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRespType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRejectReason(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRefID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRptID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumReports(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastRptRequested(Fix44Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDesc(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndAccruedInterestAmt(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartCash(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndCash(Fix44Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewPassword(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatusText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusValue(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusText(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefCompID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkResponseID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastNetworkResponseID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCompIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponseType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCollInquiryQualifier(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRptStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffirmStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikeCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikeCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeBracket(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAction(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryStatus(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryResult(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeCurrency(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartyIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyIDSource(Fix44Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyRole(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartySubIDs(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubID(Fix44Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubIDType(Fix44Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractSettlMonth(Fix44Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInterestAccrualDate(Fix44Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}
}
