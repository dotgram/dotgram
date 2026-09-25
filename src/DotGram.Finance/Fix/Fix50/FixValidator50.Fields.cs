using System;

namespace DotGram.Finance.Fix.Fix50;

// Written by generate.py from the FIX 5.0 SP2 repository; not edited by hand.

/// <summary>The check of every field of FIX 5.0 SP2 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator50
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 5.0 SP2 Account, tag 1, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Account                                         { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 5.0 SP2 AdvId, tag 2, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AdvId                                           { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 5.0 SP2 AdvRefID, tag 3, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AdvRefID                                        { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 5.0 SP2 AdvSide, tag 4, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> AdvSide                                         { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 5.0 SP2 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AdvTransType                                    { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 5.0 SP2 AvgPx, tag 6, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AvgPx                                           { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 BeginSeqNo, tag 7, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BeginSeqNo                                      { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 BeginString, tag 8, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BeginString                                     { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 5.0 SP2 BodyLength, tag 9, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BodyLength                                      { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 5.0 SP2 CheckSum, tag 10, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CheckSum                                        { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 5.0 SP2 ClOrdID, tag 11, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ClOrdID                                         { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 Commission, tag 12, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Commission                                      { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 5.0 SP2 CommType, tag 13, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> CommType                                        { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 5.0 SP2 CumQty, tag 14, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CumQty                                          { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 5.0 SP2 Currency, tag 15, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Currency                                        { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 5.0 SP2 EndSeqNo, tag 16, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EndSeqNo                                        { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 ExecID, tag 17, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ExecID                                          { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 5.0 SP2 ExecInst, tag 18, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  ExecInst                                        { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 5.0 SP2 ExecRefID, tag 19, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ExecRefID                                       { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 5.0 SP2 HandlInst, tag 21, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> HandlInst                                       { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 5.0 SP2 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityIDSource                                { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 IOIID, tag 23, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      IOIID                                           { get; set; } = ValidateIOIID;

	/// <summary>Holds a FIX 5.0 SP2 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> IOIQltyInd                                      { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 5.0 SP2 IOIRefID, tag 26, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      IOIRefID                                        { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 5.0 SP2 IOIQty, tag 27, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      IOIQty                                          { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 5.0 SP2 IOITransType, tag 28, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> IOITransType                                    { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 5.0 SP2 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LastCapacity                                    { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 5.0 SP2 LastMkt, tag 30, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LastMkt                                         { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 5.0 SP2 LastPx, tag 31, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastPx                                          { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 5.0 SP2 LastQty, tag 32, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastQty                                         { get; set; } = ValidateLastQty;

	/// <summary>Holds a FIX 5.0 SP2 NoLinesOfText, tag 33, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLinesOfText                                   { get; set; } = ValidateNoLinesOfText;

	/// <summary>Holds a FIX 5.0 SP2 MsgSeqNum, tag 34, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MsgSeqNum                                       { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 MsgType, tag 35, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MsgType                                         { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 5.0 SP2 NewSeqNo, tag 36, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NewSeqNo                                        { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 OrderID, tag 37, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrderID                                         { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 5.0 SP2 OrderQty, tag 38, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderQty                                        { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OrdStatus                                       { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 5.0 SP2 OrdType, tag 40, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OrdType                                         { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 5.0 SP2 OrigClOrdID, tag 41, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrigClOrdID                                     { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 OrigTime, tag 42, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> OrigTime                                        { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 5.0 SP2 PossDupFlag, tag 43, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PossDupFlag                                     { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 5.0 SP2 Price, tag 44, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Price                                           { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 5.0 SP2 RefSeqNum, tag 45, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RefSeqNum                                       { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 SecurityID, tag 48, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityID                                      { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 SenderCompID, tag 49, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SenderCompID                                    { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 5.0 SP2 SenderSubID, tag 50, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SenderSubID                                     { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 5.0 SP2 SendingTime, tag 52, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> SendingTime                                     { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 Quantity, tag 53, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Quantity                                        { get; set; } = ValidateQuantity;

	/// <summary>Holds a FIX 5.0 SP2 Side, tag 54, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> Side                                            { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 5.0 SP2 Symbol, tag 55, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Symbol                                          { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 5.0 SP2 TargetCompID, tag 56, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TargetCompID                                    { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 5.0 SP2 TargetSubID, tag 57, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TargetSubID                                     { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 5.0 SP2 Text, tag 58, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Text                                            { get; set; } = ValidateText;

	/// <summary>Holds a FIX 5.0 SP2 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TimeInForce                                     { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 5.0 SP2 TransactTime, tag 60, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TransactTime                                    { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 Urgency, tag 61, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> Urgency                                         { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 5.0 SP2 ValidUntilTime, tag 62, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ValidUntilTime                                  { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 5.0 SP2 SettlType, tag 63, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlType                                       { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 5.0 SP2 SettlDate, tag 64, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      SettlDate                                       { get; set; } = ValidateSettlDate;

	/// <summary>Holds a FIX 5.0 SP2 SymbolSfx, tag 65, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SymbolSfx                                       { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 ListID, tag 66, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ListID                                          { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 5.0 SP2 ListSeqNo, tag 67, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ListSeqNo                                       { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 5.0 SP2 TotNoOrders, tag 68, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoOrders                                     { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 5.0 SP2 ListExecInst, tag 69, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ListExecInst                                    { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 5.0 SP2 AllocID, tag 70, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocID                                         { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 5.0 SP2 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> AllocTransType                                  { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 5.0 SP2 RefAllocID, tag 72, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefAllocID                                      { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 5.0 SP2 NoOrders, tag 73, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoOrders                                        { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 5.0 SP2 AvgPxPrecision, tag 74, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AvgPxPrecision                                  { get; set; } = ValidateAvgPxPrecision;

	/// <summary>Holds a FIX 5.0 SP2 TradeDate, tag 75, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      TradeDate                                       { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 5.0 SP2 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> PositionEffect                                  { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 NoAllocs, tag 78, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoAllocs                                        { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccount, tag 79, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocAccount                                    { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 5.0 SP2 AllocQty, tag 80, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocQty                                        { get; set; } = ValidateAllocQty;

	/// <summary>Holds a FIX 5.0 SP2 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ProcessCode                                     { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 5.0 SP2 NoRpts, tag 82, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRpts                                          { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 5.0 SP2 RptSeq, tag 83, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RptSeq                                          { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 5.0 SP2 CxlQty, tag 84, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CxlQty                                          { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 5.0 SP2 NoDlvyInst, tag 85, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDlvyInst                                      { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 5.0 SP2 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocStatus                                     { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 5.0 SP2 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocRejCode                                    { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 5.0 SP2 Signature, tag 89, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      Signature                                       { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 5.0 SP2 SecureDataLen, tag 90, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecureDataLen                                   { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 5.0 SP2 SecureData, tag 91, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      SecureData                                      { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 5.0 SP2 SignatureLength, tag 93, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SignatureLength                                 { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 5.0 SP2 EmailType, tag 94, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> EmailType                                       { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 5.0 SP2 RawDataLength, tag 95, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RawDataLength                                   { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 5.0 SP2 RawData, tag 96, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      RawData                                         { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 5.0 SP2 PossResend, tag 97, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PossResend                                      { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 5.0 SP2 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncryptMethod                                   { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 5.0 SP2 StopPx, tag 99, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StopPx                                          { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 5.0 SP2 ExDestination, tag 100, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ExDestination                                   { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 5.0 SP2 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CxlRejReason                                    { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 5.0 SP2 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OrdRejReason                                    { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 5.0 SP2 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> IOIQualifier                                    { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 5.0 SP2 Issuer, tag 106, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Issuer                                          { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 5.0 SP2 SecurityDesc, tag 107, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityDesc                                    { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 HeartBtInt, tag 108, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   HeartBtInt                                      { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 5.0 SP2 MinQty, tag 110, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinQty                                          { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 5.0 SP2 MaxFloor, tag 111, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MaxFloor                                        { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 5.0 SP2 TestReqID, tag 112, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TestReqID                                       { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 5.0 SP2 ReportToExch, tag 113, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ReportToExch                                    { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 5.0 SP2 LocateReqd, tag 114, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   LocateReqd                                      { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OnBehalfOfCompID                                { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OnBehalfOfSubID                                 { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteID, tag 117, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteID                                         { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 5.0 SP2 NetMoney, tag 118, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   NetMoney                                        { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrAmt, tag 119, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SettlCurrAmt                                    { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrency, tag 120, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlCurrency                                   { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 ForexReq, tag 121, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ForexReq                                        { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 5.0 SP2 OrigSendingTime, tag 122, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> OrigSendingTime                                 { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 GapFillFlag, tag 123, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   GapFillFlag                                     { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 5.0 SP2 NoExecs, tag 124, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoExecs                                         { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 5.0 SP2 ExpireTime, tag 126, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ExpireTime                                      { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 5.0 SP2 DKReason, tag 127, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DKReason                                        { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToCompID, tag 128, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DeliverToCompID                                 { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToSubID, tag 129, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DeliverToSubID                                  { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 5.0 SP2 IOINaturalFlag, tag 130, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   IOINaturalFlag                                  { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 5.0 SP2 QuoteReqID, tag 131, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteReqID                                      { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 5.0 SP2 BidPx, tag 132, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidPx                                           { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 5.0 SP2 OfferPx, tag 133, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferPx                                         { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 BidSize, tag 134, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidSize                                         { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 5.0 SP2 OfferSize, tag 135, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferSize                                       { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 NoMiscFees, tag 136, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMiscFees                                      { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeAmt, tag 137, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MiscFeeAmt                                      { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeCurr, tag 138, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MiscFeeCurr                                     { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MiscFeeType                                     { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 5.0 SP2 PrevClosePx, tag 140, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PrevClosePx                                     { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 5.0 SP2 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ResetSeqNumFlag                                 { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 5.0 SP2 SenderLocationID, tag 142, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SenderLocationID                                { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 5.0 SP2 TargetLocationID, tag 143, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TargetLocationID                                { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 5.0 SP2 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OnBehalfOfLocationID                            { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 5.0 SP2 DeliverToLocationID, tag 145, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DeliverToLocationID                             { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 5.0 SP2 NoRelatedSym, tag 146, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRelatedSym                                    { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 5.0 SP2 Subject, tag 147, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Subject                                         { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 5.0 SP2 Headline, tag 148, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Headline                                        { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 5.0 SP2 URLLink, tag 149, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      URLLink                                         { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 5.0 SP2 ExecType, tag 150, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExecType                                        { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 5.0 SP2 LeavesQty, tag 151, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LeavesQty                                       { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 5.0 SP2 CashOrderQty, tag 152, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CashOrderQty                                    { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 AllocAvgPx, tag 153, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocAvgPx                                      { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 AllocNetMoney, tag 154, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocNetMoney                                   { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrFxRate, tag 155, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SettlCurrFxRate                                 { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlCurrFxRateCalc                             { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 5.0 SP2 NumDaysInterest, tag 157, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NumDaysInterest                                 { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 5.0 SP2 AccruedInterestRate, tag 158, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AccruedInterestRate                             { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 5.0 SP2 AccruedInterestAmt, tag 159, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AccruedInterestAmt                              { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlInstMode                                   { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 5.0 SP2 AllocText, tag 161, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocText                                       { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstID, tag 162, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlInstID                                     { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlInstTransType                              { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 5.0 SP2 EmailThreadID, tag 164, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      EmailThreadID                                   { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlInstSource                                 { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 5.0 SP2 SecurityType, tag 167, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityType                                    { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 EffectiveTime, tag 168, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> EffectiveTime                                   { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StandInstDbType                                 { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbName, tag 170, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StandInstDbName                                 { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 5.0 SP2 StandInstDbID, tag 171, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StandInstDbID                                   { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 5.0 SP2 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlDeliveryType                               { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 5.0 SP2 BidSpotRate, tag 188, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidSpotRate                                     { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 BidForwardPoints, tag 189, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidForwardPoints                                { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 OfferSpotRate, tag 190, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferSpotRate                                   { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 OfferForwardPoints, tag 191, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferForwardPoints                              { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 OrderQty2, tag 192, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderQty2                                       { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 5.0 SP2 SettlDate2, tag 193, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      SettlDate2                                      { get; set; } = ValidateSettlDate2;

	/// <summary>Holds a FIX 5.0 SP2 LastSpotRate, tag 194, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastSpotRate                                    { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 5.0 SP2 LastForwardPoints, tag 195, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastForwardPoints                               { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 AllocLinkID, tag 196, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocLinkID                                     { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 5.0 SP2 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocLinkType                                   { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryOrderID, tag 198, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryOrderID                                { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 5.0 SP2 NoIOIQualifiers, tag 199, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoIOIQualifiers                                 { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYear, tag 200, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> MaturityMonthYear                               { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PutOrCall                                       { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 StrikePrice, tag 202, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StrikePrice                                     { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CoveredOrUncovered                              { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 5.0 SP2 OptAttribute, tag 206, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OptAttribute                                    { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 SecurityExchange, tag 207, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityExchange                                { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   NotifyBrokerOfCredit                            { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 5.0 SP2 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocHandlInst                                  { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 5.0 SP2 MaxShow, tag 210, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MaxShow                                         { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 5.0 SP2 PegOffsetValue, tag 211, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PegOffsetValue                                  { get; set; } = ValidatePegOffsetValue;

	/// <summary>Holds a FIX 5.0 SP2 XmlDataLen, tag 212, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   XmlDataLen                                      { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 5.0 SP2 XmlData, tag 213, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      XmlData                                         { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstRefID, tag 214, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlInstRefID                                  { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 5.0 SP2 NoRoutingIDs, tag 215, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRoutingIDs                                    { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 5.0 SP2 RoutingType, tag 216, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RoutingType                                     { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 5.0 SP2 RoutingID, tag 217, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RoutingID                                       { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 5.0 SP2 Spread, tag 218, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Spread                                          { get; set; } = ValidateSpread;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurveCurrency, tag 220, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BenchmarkCurveCurrency                          { get; set; } = ValidateBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurveName, tag 221, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BenchmarkCurveName                              { get; set; } = ValidateBenchmarkCurveName;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkCurvePoint, tag 222, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BenchmarkCurvePoint                             { get; set; } = ValidateBenchmarkCurvePoint;

	/// <summary>Holds a FIX 5.0 SP2 CouponRate, tag 223, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CouponRate                                      { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 CouponPaymentDate, tag 224, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      CouponPaymentDate                               { get; set; } = ValidateCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 IssueDate, tag 225, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      IssueDate                                       { get; set; } = ValidateIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 RepurchaseTerm, tag 226, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RepurchaseTerm                                  { get; set; } = ValidateRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 RepurchaseRate, tag 227, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RepurchaseRate                                  { get; set; } = ValidateRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 Factor, tag 228, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Factor                                          { get; set; } = ValidateFactor;

	/// <summary>Holds a FIX 5.0 SP2 TradeOriginationDate, tag 229, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      TradeOriginationDate                            { get; set; } = ValidateTradeOriginationDate;

	/// <summary>Holds a FIX 5.0 SP2 ExDate, tag 230, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      ExDate                                          { get; set; } = ValidateExDate;

	/// <summary>Holds a FIX 5.0 SP2 ContractMultiplier, tag 231, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ContractMultiplier                              { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 NoStipulations, tag 232, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoStipulations                                  { get; set; } = ValidateNoStipulations;

	/// <summary>Holds a FIX 5.0 SP2 StipulationType, tag 233, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StipulationType                                 { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 5.0 SP2 StipulationValue, tag 234, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StipulationValue                                { get; set; } = ValidateStipulationValue;

	/// <summary>Holds a FIX 5.0 SP2 YieldType, tag 235, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      YieldType                                       { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 5.0 SP2 Yield, tag 236, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Yield                                           { get; set; } = ValidateYield;

	/// <summary>Holds a FIX 5.0 SP2 TotalTakedown, tag 237, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TotalTakedown                                   { get; set; } = ValidateTotalTakedown;

	/// <summary>Holds a FIX 5.0 SP2 Concession, tag 238, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Concession                                      { get; set; } = ValidateConcession;

	/// <summary>Holds a FIX 5.0 SP2 RepoCollateralSecurityType, tag 239, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RepoCollateralSecurityType                      { get; set; } = ValidateRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 RedemptionDate, tag 240, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      RedemptionDate                                  { get; set; } = ValidateRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCouponPaymentDate, tag 241, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingCouponPaymentDate                     { get; set; } = ValidateUnderlyingCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingIssueDate, tag 242, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingIssueDate                             { get; set; } = ValidateUnderlyingIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepoCollateralSecurityType, tag 243, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingRepoCollateralSecurityType            { get; set; } = ValidateUnderlyingRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepurchaseTerm, tag 244, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingRepurchaseTerm                        { get; set; } = ValidateUnderlyingRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRepurchaseRate, tag 245, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingRepurchaseRate                        { get; set; } = ValidateUnderlyingRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFactor, tag 246, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingFactor                                { get; set; } = ValidateUnderlyingFactor;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRedemptionDate, tag 247, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingRedemptionDate                        { get; set; } = ValidateUnderlyingRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 LegCouponPaymentDate, tag 248, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegCouponPaymentDate                            { get; set; } = ValidateLegCouponPaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 LegIssueDate, tag 249, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegIssueDate                                    { get; set; } = ValidateLegIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 LegRepoCollateralSecurityType, tag 250, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegRepoCollateralSecurityType                   { get; set; } = ValidateLegRepoCollateralSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 LegRepurchaseTerm, tag 251, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegRepurchaseTerm                               { get; set; } = ValidateLegRepurchaseTerm;

	/// <summary>Holds a FIX 5.0 SP2 LegRepurchaseRate, tag 252, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegRepurchaseRate                               { get; set; } = ValidateLegRepurchaseRate;

	/// <summary>Holds a FIX 5.0 SP2 LegFactor, tag 253, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegFactor                                       { get; set; } = ValidateLegFactor;

	/// <summary>Holds a FIX 5.0 SP2 LegRedemptionDate, tag 254, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegRedemptionDate                               { get; set; } = ValidateLegRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 CreditRating, tag 255, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CreditRating                                    { get; set; } = ValidateCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCreditRating, tag 256, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCreditRating                          { get; set; } = ValidateUnderlyingCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 LegCreditRating, tag 257, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegCreditRating                                 { get; set; } = ValidateLegCreditRating;

	/// <summary>Holds a FIX 5.0 SP2 TradedFlatSwitch, tag 258, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   TradedFlatSwitch                                { get; set; } = ValidateTradedFlatSwitch;

	/// <summary>Holds a FIX 5.0 SP2 BasisFeatureDate, tag 259, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      BasisFeatureDate                                { get; set; } = ValidateBasisFeatureDate;

	/// <summary>Holds a FIX 5.0 SP2 BasisFeaturePrice, tag 260, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BasisFeaturePrice                               { get; set; } = ValidateBasisFeaturePrice;

	/// <summary>Holds a FIX 5.0 SP2 MDReqID, tag 262, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDReqID                                         { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 5.0 SP2 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SubscriptionRequestType                         { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 5.0 SP2 MarketDepth, tag 264, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MarketDepth                                     { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 5.0 SP2 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDUpdateType                                    { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 5.0 SP2 AggregatedBook, tag 266, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   AggregatedBook                                  { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 5.0 SP2 NoMDEntryTypes, tag 267, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMDEntryTypes                                  { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 5.0 SP2 NoMDEntries, tag 268, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMDEntries                                     { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MDEntryType                                     { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryPx, tag 270, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MDEntryPx                                       { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySize, tag 271, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MDEntrySize                                     { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryDate, tag 272, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      MDEntryDate                                     { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryTime, tag 273, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Time, bool>      MDEntryTime                                     { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 5.0 SP2 TickDirection, tag 274, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TickDirection                                   { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 5.0 SP2 MDMkt, tag 275, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDMkt                                           { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 5.0 SP2 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  QuoteCondition                                  { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 5.0 SP2 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  TradeCondition                                  { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryID, tag 278, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDEntryID                                       { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 5.0 SP2 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MDUpdateAction                                  { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryRefID, tag 280, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDEntryRefID                                    { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 5.0 SP2 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MDReqRejReason                                  { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryOriginator, tag 282, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDEntryOriginator                               { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 5.0 SP2 LocationID, tag 283, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LocationID                                      { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 5.0 SP2 DeskID, tag 284, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DeskID                                          { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 5.0 SP2 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DeleteReason                                    { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 5.0 SP2 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  OpenCloseSettlFlag                              { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 5.0 SP2 SellerDays, tag 287, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SellerDays                                      { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryBuyer, tag 288, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDEntryBuyer                                    { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySeller, tag 289, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDEntrySeller                                   { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryPositionNo, tag 290, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDEntryPositionNo                               { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 5.0 SP2 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  FinancialStatus                                 { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 5.0 SP2 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  CorporateAction                                 { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 5.0 SP2 DefBidSize, tag 293, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DefBidSize                                      { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 5.0 SP2 DefOfferSize, tag 294, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DefOfferSize                                    { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteEntries, tag 295, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoQuoteEntries                                  { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteSets, tag 296, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoQuoteSets                                     { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteStatus                                     { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 5.0 SP2 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteCancelType                                 { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryID, tag 299, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteEntryID                                    { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteRejectReason                               { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteResponseLevel                              { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 5.0 SP2 QuoteSetID, tag 302, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteSetID                                      { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteRequestType                                { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 5.0 SP2 TotNoQuoteEntries, tag 304, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoQuoteEntries                               { get; set; } = ValidateTotNoQuoteEntries;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityIDSource, tag 305, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityIDSource                      { get; set; } = ValidateUnderlyingSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingIssuer, tag 306, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingIssuer                                { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityDesc                          { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityExchange                      { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityID                            { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityType                          { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSymbol, tag 311, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSymbol                                { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSymbolSfx                             { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> UnderlyingMaturityMonthYear                     { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingPutOrCall                             { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingStrikePrice                           { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> UnderlyingOptAttribute                          { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCurrency, tag 318, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCurrency                              { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 5.0 SP2 SecurityReqID, tag 320, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityReqID                                   { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityRequestType                             { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityResponseID, tag 322, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityResponseID                              { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityResponseType                            { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatusReqID, tag 324, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityStatusReqID                             { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   UnsolicitedIndicator                            { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityTradingStatus                           { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 5.0 SP2 HaltReasonInt, tag 327, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   HaltReasonInt                                   { get; set; } = ValidateHaltReasonInt;

	/// <summary>Holds a FIX 5.0 SP2 InViewOfCommon, tag 328, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   InViewOfCommon                                  { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 5.0 SP2 DueToRelated, tag 329, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   DueToRelated                                    { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 5.0 SP2 BuyVolume, tag 330, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BuyVolume                                       { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 5.0 SP2 SellVolume, tag 331, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SellVolume                                      { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 5.0 SP2 HighPx, tag 332, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   HighPx                                          { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 5.0 SP2 LowPx, tag 333, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LowPx                                           { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 5.0 SP2 Adjustment, tag 334, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Adjustment                                      { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 5.0 SP2 TradSesReqID, tag 335, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradSesReqID                                    { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionID, tag 336, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradingSessionID                                { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 ContraTrader, tag 337, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ContraTrader                                    { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 5.0 SP2 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradSesMethod                                   { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 5.0 SP2 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradSesMode                                     { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradSesStatus                                   { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStartTime, tag 341, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TradSesStartTime                                { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesOpenTime, tag 342, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TradSesOpenTime                                 { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TradSesPreCloseTime                             { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesCloseTime, tag 344, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TradSesCloseTime                                { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 5.0 SP2 TradSesEndTime, tag 345, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TradSesEndTime                                  { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 5.0 SP2 NumberOfOrders, tag 346, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NumberOfOrders                                  { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 5.0 SP2 MessageEncoding, tag 347, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MessageEncoding                                 { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 5.0 SP2 EncodedIssuerLen, tag 348, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedIssuerLen                                { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedIssuer, tag 349, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedIssuer                                   { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedSecurityDescLen                          { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedSecurityDesc                             { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedListExecInstLen                          { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListExecInst, tag 353, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedListExecInst                             { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 5.0 SP2 EncodedTextLen, tag 354, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedTextLen                                  { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedText, tag 355, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedText                                     { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSubjectLen, tag 356, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedSubjectLen                               { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSubject, tag 357, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedSubject                                  { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 5.0 SP2 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedHeadlineLen                              { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedHeadline, tag 359, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedHeadline                                 { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 5.0 SP2 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedAllocTextLen                             { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedAllocText, tag 361, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedAllocText                                { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingIssuerLen                      { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingIssuer                         { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingSecurityDescLen                { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingSecurityDesc                   { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 AllocPrice, tag 366, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocPrice                                      { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 5.0 SP2 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> QuoteSetValidUntilTime                          { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryRejectReason, tag 368, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteEntryRejectReason                          { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LastMsgSeqNumProcessed                          { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 5.0 SP2 RefTagID, tag 371, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RefTagID                                        { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 5.0 SP2 RefMsgType, tag 372, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefMsgType                                      { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 5.0 SP2 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SessionRejectReason                             { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> BidRequestTransType                             { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 5.0 SP2 ContraBroker, tag 375, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ContraBroker                                    { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 5.0 SP2 ComplianceID, tag 376, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ComplianceID                                    { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 5.0 SP2 SolicitedFlag, tag 377, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   SolicitedFlag                                   { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 5.0 SP2 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ExecRestatementReason                           { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 5.0 SP2 BusinessRejectRefID, tag 379, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BusinessRejectRefID                             { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 5.0 SP2 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BusinessRejectReason                            { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 GrossTradeAmt, tag 381, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   GrossTradeAmt                                   { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 NoContraBrokers, tag 382, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoContraBrokers                                 { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 5.0 SP2 MaxMessageSize, tag 383, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MaxMessageSize                                  { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 5.0 SP2 NoMsgTypes, tag 384, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMsgTypes                                      { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 5.0 SP2 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MsgDirection                                    { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 5.0 SP2 NoTradingSessions, tag 386, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTradingSessions                               { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 5.0 SP2 TotalVolumeTraded, tag 387, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TotalVolumeTraded                               { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DiscretionInst                                  { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionOffsetValue, tag 389, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DiscretionOffsetValue                           { get; set; } = ValidateDiscretionOffsetValue;

	/// <summary>Holds a FIX 5.0 SP2 BidID, tag 390, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BidID                                           { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 5.0 SP2 ClientBidID, tag 391, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ClientBidID                                     { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 5.0 SP2 ListName, tag 392, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ListName                                        { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 5.0 SP2 TotNoRelatedSym, tag 393, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoRelatedSym                                 { get; set; } = ValidateTotNoRelatedSym;

	/// <summary>Holds a FIX 5.0 SP2 BidType, tag 394, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BidType                                         { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 5.0 SP2 NumTickets, tag 395, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NumTickets                                      { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 5.0 SP2 SideValue1, tag 396, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SideValue1                                      { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 5.0 SP2 SideValue2, tag 397, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SideValue2                                      { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 5.0 SP2 NoBidDescriptors, tag 398, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoBidDescriptors                                { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 5.0 SP2 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BidDescriptorType                               { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 5.0 SP2 BidDescriptor, tag 400, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BidDescriptor                                   { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 5.0 SP2 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideValueInd                                    { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityPctLow, tag 402, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LiquidityPctLow                                 { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityPctHigh, tag 403, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LiquidityPctHigh                                { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityValue, tag 404, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LiquidityValue                                  { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 5.0 SP2 EFPTrackingError, tag 405, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EFPTrackingError                                { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 5.0 SP2 FairValue, tag 406, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FairValue                                       { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 5.0 SP2 OutsideIndexPct, tag 407, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OutsideIndexPct                                 { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 5.0 SP2 ValueOfFutures, tag 408, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ValueOfFutures                                  { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LiquidityIndType                                { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 5.0 SP2 WtAverageLiquidity, tag 410, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   WtAverageLiquidity                              { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeForPhysical, tag 411, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ExchangeForPhysical                             { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 5.0 SP2 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OutMainCntryUIndex                              { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 5.0 SP2 CrossPercent, tag 413, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CrossPercent                                    { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 5.0 SP2 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ProgRptReqs                                     { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 5.0 SP2 ProgPeriodInterval, tag 415, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ProgPeriodInterval                              { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 5.0 SP2 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   IncTaxInd                                       { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 5.0 SP2 NumBidders, tag 417, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NumBidders                                      { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 5.0 SP2 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> BidTradeType                                    { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 5.0 SP2 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> BasisPxType                                     { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 5.0 SP2 NoBidComponents, tag 420, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoBidComponents                                 { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 5.0 SP2 Country, tag 421, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Country                                         { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 5.0 SP2 TotNoStrikes, tag 422, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoStrikes                                    { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 5.0 SP2 PriceType, tag 423, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PriceType                                       { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 5.0 SP2 DayOrderQty, tag 424, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DayOrderQty                                     { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 DayCumQty, tag 425, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DayCumQty                                       { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 5.0 SP2 DayAvgPx, tag 426, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DayAvgPx                                        { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   GTBookingInst                                   { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 5.0 SP2 NoStrikes, tag 428, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoStrikes                                       { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 5.0 SP2 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ListStatusType                                  { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 5.0 SP2 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NetGrossInd                                     { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 5.0 SP2 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ListOrderStatus                                 { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 5.0 SP2 ExpireDate, tag 432, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      ExpireDate                                      { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 5.0 SP2 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ListExecInstType                                { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 5.0 SP2 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> CxlRejResponseTo                                { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingCouponRate                            { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingContractMultiplier                    { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 ContraTradeQty, tag 437, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ContraTradeQty                                  { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 5.0 SP2 ContraTradeTime, tag 438, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ContraTradeTime                                 { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 5.0 SP2 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LiquidityNumSecurities                          { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 5.0 SP2 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MultiLegReportingType                           { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 5.0 SP2 StrikeTime, tag 443, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> StrikeTime                                      { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 5.0 SP2 ListStatusText, tag 444, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ListStatusText                                  { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedListStatusTextLen                        { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedListStatusText, tag 446, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedListStatusText                           { get; set; } = ValidateEncodedListStatusText;

	/// <summary>Holds a FIX 5.0 SP2 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> PartyIDSource                                   { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 PartyID, tag 448, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PartyID                                         { get; set; } = ValidatePartyID;

	/// <summary>Holds a FIX 5.0 SP2 NetChgPrevDay, tag 451, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   NetChgPrevDay                                   { get; set; } = ValidateNetChgPrevDay;

	/// <summary>Holds a FIX 5.0 SP2 PartyRole, tag 452, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PartyRole                                       { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoPartyIDs, tag 453, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoPartyIDs                                      { get; set; } = ValidateNoPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoSecurityAltID, tag 454, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSecurityAltID                                 { get; set; } = ValidateNoSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityAltID, tag 455, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityAltID                                   { get; set; } = ValidateSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityAltIDSource, tag 456, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityAltIDSource                             { get; set; } = ValidateSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingSecurityAltID, tag 457, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUnderlyingSecurityAltID                       { get; set; } = ValidateNoUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityAltID, tag 458, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityAltID                         { get; set; } = ValidateUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecurityAltIDSource, tag 459, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityAltIDSource                   { get; set; } = ValidateUnderlyingSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Product, tag 460, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Product                                         { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 5.0 SP2 CFICode, tag 461, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CFICode                                         { get; set; } = ValidateCFICode;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingProduct, tag 462, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingProduct                               { get; set; } = ValidateUnderlyingProduct;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCFICode, tag 463, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCFICode                               { get; set; } = ValidateUnderlyingCFICode;

	/// <summary>Holds a FIX 5.0 SP2 TestMessageIndicator, tag 464, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   TestMessageIndicator                            { get; set; } = ValidateTestMessageIndicator;

	/// <summary>Holds a FIX 5.0 SP2 BookingRefID, tag 466, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BookingRefID                                    { get; set; } = ValidateBookingRefID;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocID, tag 467, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      IndividualAllocID                               { get; set; } = ValidateIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> RoundingDirection                               { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 5.0 SP2 RoundingModulus, tag 469, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RoundingModulus                                 { get; set; } = ValidateRoundingModulus;

	/// <summary>Holds a FIX 5.0 SP2 CountryOfIssue, tag 470, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CountryOfIssue                                  { get; set; } = ValidateCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 StateOrProvinceOfIssue, tag 471, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StateOrProvinceOfIssue                          { get; set; } = ValidateStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LocaleOfIssue, tag 472, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LocaleOfIssue                                   { get; set; } = ValidateLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 NoRegistDtls, tag 473, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRegistDtls                                    { get; set; } = ValidateNoRegistDtls;

	/// <summary>Holds a FIX 5.0 SP2 MailingDtls, tag 474, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MailingDtls                                     { get; set; } = ValidateMailingDtls;

	/// <summary>Holds a FIX 5.0 SP2 InvestorCountryOfResidence, tag 475, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InvestorCountryOfResidence                      { get; set; } = ValidateInvestorCountryOfResidence;

	/// <summary>Holds a FIX 5.0 SP2 PaymentRef, tag 476, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PaymentRef                                      { get; set; } = ValidatePaymentRef;

	/// <summary>Holds a FIX 5.0 SP2 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DistribPaymentMethod                            { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribCurr, tag 478, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribCurr                                 { get; set; } = ValidateCashDistribCurr;

	/// <summary>Holds a FIX 5.0 SP2 CommCurrency, tag 479, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CommCurrency                                    { get; set; } = ValidateCommCurrency;

	/// <summary>Holds a FIX 5.0 SP2 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> CancellationRights                              { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 5.0 SP2 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MoneyLaunderingStatus                           { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 5.0 SP2 MailingInst, tag 482, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MailingInst                                     { get; set; } = ValidateMailingInst;

	/// <summary>Holds a FIX 5.0 SP2 TransBkdTime, tag 483, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TransBkdTime                                    { get; set; } = ValidateTransBkdTime;

	/// <summary>Holds a FIX 5.0 SP2 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExecPriceType                                   { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 5.0 SP2 ExecPriceAdjustment, tag 485, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ExecPriceAdjustment                             { get; set; } = ValidateExecPriceAdjustment;

	/// <summary>Holds a FIX 5.0 SP2 DateOfBirth, tag 486, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DateOfBirth                                     { get; set; } = ValidateDateOfBirth;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportTransType, tag 487, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeReportTransType                            { get; set; } = ValidateTradeReportTransType;

	/// <summary>Holds a FIX 5.0 SP2 CardHolderName, tag 488, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CardHolderName                                  { get; set; } = ValidateCardHolderName;

	/// <summary>Holds a FIX 5.0 SP2 CardNumber, tag 489, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CardNumber                                      { get; set; } = ValidateCardNumber;

	/// <summary>Holds a FIX 5.0 SP2 CardExpDate, tag 490, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      CardExpDate                                     { get; set; } = ValidateCardExpDate;

	/// <summary>Holds a FIX 5.0 SP2 CardIssNum, tag 491, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CardIssNum                                      { get; set; } = ValidateCardIssNum;

	/// <summary>Holds a FIX 5.0 SP2 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PaymentMethod                                   { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 5.0 SP2 RegistAcctType, tag 493, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistAcctType                                  { get; set; } = ValidateRegistAcctType;

	/// <summary>Holds a FIX 5.0 SP2 Designation, tag 494, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Designation                                     { get; set; } = ValidateDesignation;

	/// <summary>Holds a FIX 5.0 SP2 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TaxAdvantageType                                { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 5.0 SP2 RegistRejReasonText, tag 496, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistRejReasonText                             { get; set; } = ValidateRegistRejReasonText;

	/// <summary>Holds a FIX 5.0 SP2 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> FundRenewWaiv                                   { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentName, tag 498, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribAgentName                            { get; set; } = ValidateCashDistribAgentName;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentCode, tag 499, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribAgentCode                            { get; set; } = ValidateCashDistribAgentCode;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentAcctNumber, tag 500, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribAgentAcctNumber                      { get; set; } = ValidateCashDistribAgentAcctNumber;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribPayRef, tag 501, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribPayRef                               { get; set; } = ValidateCashDistribPayRef;

	/// <summary>Holds a FIX 5.0 SP2 CashDistribAgentAcctName, tag 502, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CashDistribAgentAcctName                        { get; set; } = ValidateCashDistribAgentAcctName;

	/// <summary>Holds a FIX 5.0 SP2 CardStartDate, tag 503, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      CardStartDate                                   { get; set; } = ValidateCardStartDate;

	/// <summary>Holds a FIX 5.0 SP2 PaymentDate, tag 504, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      PaymentDate                                     { get; set; } = ValidatePaymentDate;

	/// <summary>Holds a FIX 5.0 SP2 PaymentRemitterID, tag 505, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PaymentRemitterID                               { get; set; } = ValidatePaymentRemitterID;

	/// <summary>Holds a FIX 5.0 SP2 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> RegistStatus                                    { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 5.0 SP2 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RegistRejReasonCode                             { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 5.0 SP2 RegistRefID, tag 508, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistRefID                                     { get; set; } = ValidateRegistRefID;

	/// <summary>Holds a FIX 5.0 SP2 RegistDtls, tag 509, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistDtls                                      { get; set; } = ValidateRegistDtls;

	/// <summary>Holds a FIX 5.0 SP2 NoDistribInsts, tag 510, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDistribInsts                                  { get; set; } = ValidateNoDistribInsts;

	/// <summary>Holds a FIX 5.0 SP2 RegistEmail, tag 511, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistEmail                                     { get; set; } = ValidateRegistEmail;

	/// <summary>Holds a FIX 5.0 SP2 DistribPercentage, tag 512, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DistribPercentage                               { get; set; } = ValidateDistribPercentage;

	/// <summary>Holds a FIX 5.0 SP2 RegistID, tag 513, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RegistID                                        { get; set; } = ValidateRegistID;

	/// <summary>Holds a FIX 5.0 SP2 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> RegistTransType                                 { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 5.0 SP2 ExecValuationPoint, tag 515, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ExecValuationPoint                              { get; set; } = ValidateExecValuationPoint;

	/// <summary>Holds a FIX 5.0 SP2 OrderPercent, tag 516, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderPercent                                    { get; set; } = ValidateOrderPercent;

	/// <summary>Holds a FIX 5.0 SP2 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OwnershipType                                   { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 5.0 SP2 NoContAmts, tag 518, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoContAmts                                      { get; set; } = ValidateNoContAmts;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ContAmtType                                     { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtValue, tag 520, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ContAmtValue                                    { get; set; } = ValidateContAmtValue;

	/// <summary>Holds a FIX 5.0 SP2 ContAmtCurr, tag 521, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ContAmtCurr                                     { get; set; } = ValidateContAmtCurr;

	/// <summary>Holds a FIX 5.0 SP2 OwnerType, tag 522, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OwnerType                                       { get; set; } = ValidateOwnerType;

	/// <summary>Holds a FIX 5.0 SP2 PartySubID, tag 523, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PartySubID                                      { get; set; } = ValidatePartySubID;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyID, tag 524, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NestedPartyID                                   { get; set; } = ValidateNestedPartyID;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyIDSource, tag 525, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> NestedPartyIDSource                             { get; set; } = ValidateNestedPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryClOrdID, tag 526, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryClOrdID                                { get; set; } = ValidateSecondaryClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryExecID, tag 527, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryExecID                                 { get; set; } = ValidateSecondaryExecID;

	/// <summary>Holds a FIX 5.0 SP2 OrderCapacity, tag 528, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OrderCapacity                                   { get; set; } = ValidateOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 OrderRestrictions, tag 529, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  OrderRestrictions                               { get; set; } = ValidateOrderRestrictions;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelRequestType, tag 530, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MassCancelRequestType                           { get; set; } = ValidateMassCancelRequestType;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelResponse, tag 531, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MassCancelResponse                              { get; set; } = ValidateMassCancelResponse;

	/// <summary>Holds a FIX 5.0 SP2 MassCancelRejectReason, tag 532, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassCancelRejectReason                          { get; set; } = ValidateMassCancelRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 TotalAffectedOrders, tag 533, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotalAffectedOrders                             { get; set; } = ValidateTotalAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 NoAffectedOrders, tag 534, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoAffectedOrders                                { get; set; } = ValidateNoAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 AffectedOrderID, tag 535, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AffectedOrderID                                 { get; set; } = ValidateAffectedOrderID;

	/// <summary>Holds a FIX 5.0 SP2 AffectedSecondaryOrderID, tag 536, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AffectedSecondaryOrderID                        { get; set; } = ValidateAffectedSecondaryOrderID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteType, tag 537, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteType                                       { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartyRole, tag 538, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NestedPartyRole                                 { get; set; } = ValidateNestedPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedPartyIDs, tag 539, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNestedPartyIDs                                { get; set; } = ValidateNoNestedPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 TotalAccruedInterestAmt, tag 540, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TotalAccruedInterestAmt                         { get; set; } = ValidateTotalAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 MaturityDate, tag 541, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      MaturityDate                                    { get; set; } = ValidateMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityDate, tag 542, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingMaturityDate                          { get; set; } = ValidateUnderlyingMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 InstrRegistry, tag 543, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InstrRegistry                                   { get; set; } = ValidateInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 CashMargin, tag 544, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> CashMargin                                      { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartySubID, tag 545, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NestedPartySubID                                { get; set; } = ValidateNestedPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 Scope, tag 546, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  Scope                                           { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 5.0 SP2 MDImplicitDelete, tag 547, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   MDImplicitDelete                                { get; set; } = ValidateMDImplicitDelete;

	/// <summary>Holds a FIX 5.0 SP2 CrossID, tag 548, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CrossID                                         { get; set; } = ValidateCrossID;

	/// <summary>Holds a FIX 5.0 SP2 CrossType, tag 549, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CrossType                                       { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 5.0 SP2 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CrossPrioritization                             { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 5.0 SP2 OrigCrossID, tag 551, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrigCrossID                                     { get; set; } = ValidateOrigCrossID;

	/// <summary>Holds a FIX 5.0 SP2 NoSides, tag 552, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSides                                         { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 5.0 SP2 Username, tag 553, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Username                                        { get; set; } = ValidateUsername;

	/// <summary>Holds a FIX 5.0 SP2 Password, tag 554, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Password                                        { get; set; } = ValidatePassword;

	/// <summary>Holds a FIX 5.0 SP2 NoLegs, tag 555, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLegs                                          { get; set; } = ValidateNoLegs;

	/// <summary>Holds a FIX 5.0 SP2 LegCurrency, tag 556, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegCurrency                                     { get; set; } = ValidateLegCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TotNoSecurityTypes, tag 557, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoSecurityTypes                              { get; set; } = ValidateTotNoSecurityTypes;

	/// <summary>Holds a FIX 5.0 SP2 NoSecurityTypes, tag 558, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSecurityTypes                                 { get; set; } = ValidateNoSecurityTypes;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityListRequestType                         { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityRequestResult                           { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 5.0 SP2 RoundLot, tag 561, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RoundLot                                        { get; set; } = ValidateRoundLot;

	/// <summary>Holds a FIX 5.0 SP2 MinTradeVol, tag 562, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinTradeVol                                     { get; set; } = ValidateMinTradeVol;

	/// <summary>Holds a FIX 5.0 SP2 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MultiLegRptTypeReq                              { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 5.0 SP2 LegPositionEffect, tag 564, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LegPositionEffect                               { get; set; } = ValidateLegPositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 LegCoveredOrUncovered, tag 565, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegCoveredOrUncovered                           { get; set; } = ValidateLegCoveredOrUncovered;

	/// <summary>Holds a FIX 5.0 SP2 LegPrice, tag 566, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegPrice                                        { get; set; } = ValidateLegPrice;

	/// <summary>Holds a FIX 5.0 SP2 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradSesStatusRejReason                          { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestID, tag 568, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeRequestID                                  { get; set; } = ValidateTradeRequestID;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeRequestType                                { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 5.0 SP2 PreviouslyReported, tag 570, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PreviouslyReported                              { get; set; } = ValidatePreviouslyReported;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportID, tag 571, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeReportID                                   { get; set; } = ValidateTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportRefID, tag 572, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeReportRefID                                { get; set; } = ValidateTradeReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MatchStatus                                     { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 5.0 SP2 MatchType, tag 574, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MatchType                                       { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 5.0 SP2 OddLot, tag 575, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   OddLot                                          { get; set; } = ValidateOddLot;

	/// <summary>Holds a FIX 5.0 SP2 NoClearingInstructions, tag 576, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoClearingInstructions                          { get; set; } = ValidateNoClearingInstructions;

	/// <summary>Holds a FIX 5.0 SP2 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ClearingInstruction                             { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 5.0 SP2 TradeInputSource, tag 578, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeInputSource                                { get; set; } = ValidateTradeInputSource;

	/// <summary>Holds a FIX 5.0 SP2 TradeInputDevice, tag 579, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeInputDevice                                { get; set; } = ValidateTradeInputDevice;

	/// <summary>Holds a FIX 5.0 SP2 NoDates, tag 580, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDates                                         { get; set; } = ValidateNoDates;

	/// <summary>Holds a FIX 5.0 SP2 AccountType, tag 581, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AccountType                                     { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 5.0 SP2 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CustOrderCapacity                               { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 ClOrdLinkID, tag 583, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ClOrdLinkID                                     { get; set; } = ValidateClOrdLinkID;

	/// <summary>Holds a FIX 5.0 SP2 MassStatusReqID, tag 584, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MassStatusReqID                                 { get; set; } = ValidateMassStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassStatusReqType                               { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 5.0 SP2 OrigOrdModTime, tag 586, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> OrigOrdModTime                                  { get; set; } = ValidateOrigOrdModTime;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlType, tag 587, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LegSettlType                                    { get; set; } = ValidateLegSettlType;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlDate, tag 588, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegSettlDate                                    { get; set; } = ValidateLegSettlDate;

	/// <summary>Holds a FIX 5.0 SP2 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DayBookingInst                                  { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 5.0 SP2 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> BookingUnit                                     { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 5.0 SP2 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> PreallocMethod                                  { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCountryOfIssue, tag 592, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCountryOfIssue                        { get; set; } = ValidateUnderlyingCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStateOrProvinceOfIssue, tag 593, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingStateOrProvinceOfIssue                { get; set; } = ValidateUnderlyingStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLocaleOfIssue, tag 594, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLocaleOfIssue                         { get; set; } = ValidateUnderlyingLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrRegistry, tag 595, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingInstrRegistry                         { get; set; } = ValidateUnderlyingInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 LegCountryOfIssue, tag 596, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegCountryOfIssue                               { get; set; } = ValidateLegCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegStateOrProvinceOfIssue, tag 597, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegStateOrProvinceOfIssue                       { get; set; } = ValidateLegStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegLocaleOfIssue, tag 598, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegLocaleOfIssue                                { get; set; } = ValidateLegLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 LegInstrRegistry, tag 599, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegInstrRegistry                                { get; set; } = ValidateLegInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 LegSymbol, tag 600, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSymbol                                       { get; set; } = ValidateLegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 LegSymbolSfx, tag 601, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSymbolSfx                                    { get; set; } = ValidateLegSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityID, tag 602, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityID                                   { get; set; } = ValidateLegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityIDSource, tag 603, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityIDSource                             { get; set; } = ValidateLegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoLegSecurityAltID, tag 604, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLegSecurityAltID                              { get; set; } = ValidateNoLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityAltID, tag 605, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityAltID                                { get; set; } = ValidateLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityAltIDSource, tag 606, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityAltIDSource                          { get; set; } = ValidateLegSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 LegProduct, tag 607, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegProduct                                      { get; set; } = ValidateLegProduct;

	/// <summary>Holds a FIX 5.0 SP2 LegCFICode, tag 608, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegCFICode                                      { get; set; } = ValidateLegCFICode;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityType, tag 609, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityType                                 { get; set; } = ValidateLegSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityMonthYear, tag 610, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> LegMaturityMonthYear                            { get; set; } = ValidateLegMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityDate, tag 611, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegMaturityDate                                 { get; set; } = ValidateLegMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 LegStrikePrice, tag 612, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegStrikePrice                                  { get; set; } = ValidateLegStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 LegOptAttribute, tag 613, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LegOptAttribute                                 { get; set; } = ValidateLegOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 LegContractMultiplier, tag 614, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegContractMultiplier                           { get; set; } = ValidateLegContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 LegCouponRate, tag 615, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegCouponRate                                   { get; set; } = ValidateLegCouponRate;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityExchange, tag 616, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityExchange                             { get; set; } = ValidateLegSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 LegIssuer, tag 617, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegIssuer                                       { get; set; } = ValidateLegIssuer;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegIssuerLen, tag 618, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedLegIssuerLen                             { get; set; } = ValidateEncodedLegIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegIssuer, tag 619, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedLegIssuer                                { get; set; } = ValidateEncodedLegIssuer;

	/// <summary>Holds a FIX 5.0 SP2 LegSecurityDesc, tag 620, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecurityDesc                                 { get; set; } = ValidateLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegSecurityDescLen, tag 621, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedLegSecurityDescLen                       { get; set; } = ValidateEncodedLegSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedLegSecurityDesc, tag 622, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedLegSecurityDesc                          { get; set; } = ValidateEncodedLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 LegRatioQty, tag 623, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegRatioQty                                     { get; set; } = ValidateLegRatioQty;

	/// <summary>Holds a FIX 5.0 SP2 LegSide, tag 624, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LegSide                                         { get; set; } = ValidateLegSide;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionSubID, tag 625, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradingSessionSubID                             { get; set; } = ValidateTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 AllocType, tag 626, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocType                                       { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 5.0 SP2 NoHops, tag 627, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoHops                                          { get; set; } = ValidateNoHops;

	/// <summary>Holds a FIX 5.0 SP2 HopCompID, tag 628, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      HopCompID                                       { get; set; } = ValidateHopCompID;

	/// <summary>Holds a FIX 5.0 SP2 HopSendingTime, tag 629, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> HopSendingTime                                  { get; set; } = ValidateHopSendingTime;

	/// <summary>Holds a FIX 5.0 SP2 HopRefID, tag 630, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   HopRefID                                        { get; set; } = ValidateHopRefID;

	/// <summary>Holds a FIX 5.0 SP2 MidPx, tag 631, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MidPx                                           { get; set; } = ValidateMidPx;

	/// <summary>Holds a FIX 5.0 SP2 BidYield, tag 632, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidYield                                        { get; set; } = ValidateBidYield;

	/// <summary>Holds a FIX 5.0 SP2 MidYield, tag 633, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MidYield                                        { get; set; } = ValidateMidYield;

	/// <summary>Holds a FIX 5.0 SP2 OfferYield, tag 634, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferYield                                      { get; set; } = ValidateOfferYield;

	/// <summary>Holds a FIX 5.0 SP2 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ClearingFeeIndicator                            { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 5.0 SP2 WorkingIndicator, tag 636, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   WorkingIndicator                                { get; set; } = ValidateWorkingIndicator;

	/// <summary>Holds a FIX 5.0 SP2 LegLastPx, tag 637, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegLastPx                                       { get; set; } = ValidateLegLastPx;

	/// <summary>Holds a FIX 5.0 SP2 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PriorityIndicator                               { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PriceImprovement, tag 639, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PriceImprovement                                { get; set; } = ValidatePriceImprovement;

	/// <summary>Holds a FIX 5.0 SP2 Price2, tag 640, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Price2                                          { get; set; } = ValidatePrice2;

	/// <summary>Holds a FIX 5.0 SP2 LastForwardPoints2, tag 641, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastForwardPoints2                              { get; set; } = ValidateLastForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 BidForwardPoints2, tag 642, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidForwardPoints2                               { get; set; } = ValidateBidForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 OfferForwardPoints2, tag 643, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferForwardPoints2                             { get; set; } = ValidateOfferForwardPoints2;

	/// <summary>Holds a FIX 5.0 SP2 RFQReqID, tag 644, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RFQReqID                                        { get; set; } = ValidateRFQReqID;

	/// <summary>Holds a FIX 5.0 SP2 MktBidPx, tag 645, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MktBidPx                                        { get; set; } = ValidateMktBidPx;

	/// <summary>Holds a FIX 5.0 SP2 MktOfferPx, tag 646, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MktOfferPx                                      { get; set; } = ValidateMktOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 MinBidSize, tag 647, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinBidSize                                      { get; set; } = ValidateMinBidSize;

	/// <summary>Holds a FIX 5.0 SP2 MinOfferSize, tag 648, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinOfferSize                                    { get; set; } = ValidateMinOfferSize;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatusReqID, tag 649, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteStatusReqID                                { get; set; } = ValidateQuoteStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 LegalConfirm, tag 650, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   LegalConfirm                                    { get; set; } = ValidateLegalConfirm;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLastPx, tag 651, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingLastPx                                { get; set; } = ValidateUnderlyingLastPx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLastQty, tag 652, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingLastQty                               { get; set; } = ValidateUnderlyingLastQty;

	/// <summary>Holds a FIX 5.0 SP2 LegRefID, tag 654, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegRefID                                        { get; set; } = ValidateLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 ContraLegRefID, tag 655, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ContraLegRefID                                  { get; set; } = ValidateContraLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrBidFxRate, tag 656, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SettlCurrBidFxRate                              { get; set; } = ValidateSettlCurrBidFxRate;

	/// <summary>Holds a FIX 5.0 SP2 SettlCurrOfferFxRate, tag 657, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SettlCurrOfferFxRate                            { get; set; } = ValidateSettlCurrOfferFxRate;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteRequestRejectReason                        { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 SideComplianceID, tag 659, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideComplianceID                                { get; set; } = ValidateSideComplianceID;

	/// <summary>Holds a FIX 5.0 SP2 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AcctIDSource                                    { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 AllocAcctIDSource, tag 661, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocAcctIDSource                               { get; set; } = ValidateAllocAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkPrice, tag 662, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BenchmarkPrice                                  { get; set; } = ValidateBenchmarkPrice;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkPriceType, tag 663, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BenchmarkPriceType                              { get; set; } = ValidateBenchmarkPriceType;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmID, tag 664, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ConfirmID                                       { get; set; } = ValidateConfirmID;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ConfirmStatus                                   { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ConfirmTransType                                { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 5.0 SP2 ContractSettlMonth, tag 667, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> ContractSettlMonth                              { get; set; } = ValidateContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DeliveryForm                                    { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 5.0 SP2 LastParPx, tag 669, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastParPx                                       { get; set; } = ValidateLastParPx;

	/// <summary>Holds a FIX 5.0 SP2 NoLegAllocs, tag 670, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLegAllocs                                     { get; set; } = ValidateNoLegAllocs;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocAccount, tag 671, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegAllocAccount                                 { get; set; } = ValidateLegAllocAccount;

	/// <summary>Holds a FIX 5.0 SP2 LegIndividualAllocID, tag 672, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegIndividualAllocID                            { get; set; } = ValidateLegIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocQty, tag 673, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegAllocQty                                     { get; set; } = ValidateLegAllocQty;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocAcctIDSource, tag 674, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegAllocAcctIDSource                            { get; set; } = ValidateLegAllocAcctIDSource;

	/// <summary>Holds a FIX 5.0 SP2 LegSettlCurrency, tag 675, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSettlCurrency                                { get; set; } = ValidateLegSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurveCurrency, tag 676, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurveCurrency                       { get; set; } = ValidateLegBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurveName, tag 677, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurveName                           { get; set; } = ValidateLegBenchmarkCurveName;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurvePoint, tag 678, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegBenchmarkCurvePoint                          { get; set; } = ValidateLegBenchmarkCurvePoint;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkPrice, tag 679, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegBenchmarkPrice                               { get; set; } = ValidateLegBenchmarkPrice;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkPriceType, tag 680, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegBenchmarkPriceType                           { get; set; } = ValidateLegBenchmarkPriceType;

	/// <summary>Holds a FIX 5.0 SP2 LegBidPx, tag 681, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegBidPx                                        { get; set; } = ValidateLegBidPx;

	/// <summary>Holds a FIX 5.0 SP2 LegIOIQty, tag 682, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegIOIQty                                       { get; set; } = ValidateLegIOIQty;

	/// <summary>Holds a FIX 5.0 SP2 NoLegStipulations, tag 683, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLegStipulations                               { get; set; } = ValidateNoLegStipulations;

	/// <summary>Holds a FIX 5.0 SP2 LegOfferPx, tag 684, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegOfferPx                                      { get; set; } = ValidateLegOfferPx;

	/// <summary>Holds a FIX 5.0 SP2 LegOrderQty, tag 685, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegOrderQty                                     { get; set; } = ValidateLegOrderQty;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceType, tag 686, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegPriceType                                    { get; set; } = ValidateLegPriceType;

	/// <summary>Holds a FIX 5.0 SP2 LegQty, tag 687, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegQty                                          { get; set; } = ValidateLegQty;

	/// <summary>Holds a FIX 5.0 SP2 LegStipulationType, tag 688, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegStipulationType                              { get; set; } = ValidateLegStipulationType;

	/// <summary>Holds a FIX 5.0 SP2 LegStipulationValue, tag 689, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegStipulationValue                             { get; set; } = ValidateLegStipulationValue;

	/// <summary>Holds a FIX 5.0 SP2 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegSwapType                                     { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 5.0 SP2 Pool, tag 691, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Pool                                            { get; set; } = ValidatePool;

	/// <summary>Holds a FIX 5.0 SP2 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuotePriceType                                  { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRespID, tag 693, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteRespID                                     { get; set; } = ValidateQuoteRespID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteRespType                                   { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 5.0 SP2 QuoteQualifier, tag 695, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> QuoteQualifier                                  { get; set; } = ValidateQuoteQualifier;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionDate, tag 696, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      YieldRedemptionDate                             { get; set; } = ValidateYieldRedemptionDate;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionPrice, tag 697, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   YieldRedemptionPrice                            { get; set; } = ValidateYieldRedemptionPrice;

	/// <summary>Holds a FIX 5.0 SP2 YieldRedemptionPriceType, tag 698, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   YieldRedemptionPriceType                        { get; set; } = ValidateYieldRedemptionPriceType;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkSecurityID, tag 699, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BenchmarkSecurityID                             { get; set; } = ValidateBenchmarkSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 ReversalIndicator, tag 700, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ReversalIndicator                               { get; set; } = ValidateReversalIndicator;

	/// <summary>Holds a FIX 5.0 SP2 YieldCalcDate, tag 701, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      YieldCalcDate                                   { get; set; } = ValidateYieldCalcDate;

	/// <summary>Holds a FIX 5.0 SP2 NoPositions, tag 702, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoPositions                                     { get; set; } = ValidateNoPositions;

	/// <summary>Holds a FIX 5.0 SP2 PosType, tag 703, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PosType                                         { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 5.0 SP2 LongQty, tag 704, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LongQty                                         { get; set; } = ValidateLongQty;

	/// <summary>Holds a FIX 5.0 SP2 ShortQty, tag 705, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ShortQty                                        { get; set; } = ValidateShortQty;

	/// <summary>Holds a FIX 5.0 SP2 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosQtyStatus                                    { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 5.0 SP2 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PosAmtType                                      { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 5.0 SP2 PosAmt, tag 708, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PosAmt                                          { get; set; } = ValidatePosAmt;

	/// <summary>Holds a FIX 5.0 SP2 PosTransType, tag 709, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosTransType                                    { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 5.0 SP2 PosReqID, tag 710, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PosReqID                                        { get; set; } = ValidatePosReqID;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyings, tag 711, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUnderlyings                                   { get; set; } = ValidateNoUnderlyings;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosMaintAction                                  { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 5.0 SP2 OrigPosReqRefID, tag 713, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrigPosReqRefID                                 { get; set; } = ValidateOrigPosReqRefID;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintRptRefID, tag 714, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PosMaintRptRefID                                { get; set; } = ValidatePosMaintRptRefID;

	/// <summary>Holds a FIX 5.0 SP2 ClearingBusinessDate, tag 715, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      ClearingBusinessDate                            { get; set; } = ValidateClearingBusinessDate;

	/// <summary>Holds a FIX 5.0 SP2 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlSessID                                     { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 5.0 SP2 SettlSessSubID, tag 717, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlSessSubID                                  { get; set; } = ValidateSettlSessSubID;

	/// <summary>Holds a FIX 5.0 SP2 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AdjustmentType                                  { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 5.0 SP2 ContraryInstructionIndicator, tag 719, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ContraryInstructionIndicator                    { get; set; } = ValidateContraryInstructionIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PriorSpreadIndicator, tag 720, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PriorSpreadIndicator                            { get; set; } = ValidatePriorSpreadIndicator;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintRptID, tag 721, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PosMaintRptID                                   { get; set; } = ValidatePosMaintRptID;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosMaintStatus                                  { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 5.0 SP2 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosMaintResult                                  { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 5.0 SP2 PosReqType, tag 724, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosReqType                                      { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 5.0 SP2 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ResponseTransportType                           { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 5.0 SP2 ResponseDestination, tag 726, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ResponseDestination                             { get; set; } = ValidateResponseDestination;

	/// <summary>Holds a FIX 5.0 SP2 TotalNumPosReports, tag 727, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotalNumPosReports                              { get; set; } = ValidateTotalNumPosReports;

	/// <summary>Holds a FIX 5.0 SP2 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosReqResult                                    { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 5.0 SP2 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PosReqStatus                                    { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 5.0 SP2 SettlPrice, tag 730, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SettlPrice                                      { get; set; } = ValidateSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlPriceType                                  { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlPrice, tag 732, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingSettlPrice                            { get; set; } = ValidateUnderlyingSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlPriceType, tag 733, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingSettlPriceType                        { get; set; } = ValidateUnderlyingSettlPriceType;

	/// <summary>Holds a FIX 5.0 SP2 PriorSettlPrice, tag 734, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PriorSettlPrice                                 { get; set; } = ValidatePriorSettlPrice;

	/// <summary>Holds a FIX 5.0 SP2 NoQuoteQualifiers, tag 735, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoQuoteQualifiers                               { get; set; } = ValidateNoQuoteQualifiers;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlCurrency, tag 736, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocSettlCurrency                              { get; set; } = ValidateAllocSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlCurrAmt, tag 737, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocSettlCurrAmt                               { get; set; } = ValidateAllocSettlCurrAmt;

	/// <summary>Holds a FIX 5.0 SP2 InterestAtMaturity, tag 738, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   InterestAtMaturity                              { get; set; } = ValidateInterestAtMaturity;

	/// <summary>Holds a FIX 5.0 SP2 LegDatedDate, tag 739, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegDatedDate                                    { get; set; } = ValidateLegDatedDate;

	/// <summary>Holds a FIX 5.0 SP2 LegPool, tag 740, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegPool                                         { get; set; } = ValidateLegPool;

	/// <summary>Holds a FIX 5.0 SP2 AllocInterestAtMaturity, tag 741, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocInterestAtMaturity                         { get; set; } = ValidateAllocInterestAtMaturity;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccruedInterestAmt, tag 742, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllocAccruedInterestAmt                         { get; set; } = ValidateAllocAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryDate, tag 743, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DeliveryDate                                    { get; set; } = ValidateDeliveryDate;

	/// <summary>Holds a FIX 5.0 SP2 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> AssignmentMethod                                { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 AssignmentUnit, tag 745, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AssignmentUnit                                  { get; set; } = ValidateAssignmentUnit;

	/// <summary>Holds a FIX 5.0 SP2 OpenInterest, tag 746, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OpenInterest                                    { get; set; } = ValidateOpenInterest;

	/// <summary>Holds a FIX 5.0 SP2 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExerciseMethod                                  { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 5.0 SP2 TotNumTradeReports, tag 748, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNumTradeReports                              { get; set; } = ValidateTotNumTradeReports;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeRequestResult                              { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 5.0 SP2 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeRequestStatus                              { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeReportRejectReason                         { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideMultiLegReportingType                       { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 5.0 SP2 NoPosAmt, tag 753, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoPosAmt                                        { get; set; } = ValidateNoPosAmt;

	/// <summary>Holds a FIX 5.0 SP2 AutoAcceptIndicator, tag 754, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   AutoAcceptIndicator                             { get; set; } = ValidateAutoAcceptIndicator;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportID, tag 755, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocReportID                                   { get; set; } = ValidateAllocReportID;

	/// <summary>Holds a FIX 5.0 SP2 NoNested2PartyIDs, tag 756, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested2PartyIDs                               { get; set; } = ValidateNoNested2PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyID, tag 757, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested2PartyID                                  { get; set; } = ValidateNested2PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyIDSource, tag 758, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> Nested2PartyIDSource                            { get; set; } = ValidateNested2PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartyRole, tag 759, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested2PartyRole                                { get; set; } = ValidateNested2PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartySubID, tag 760, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested2PartySubID                               { get; set; } = ValidateNested2PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 BenchmarkSecurityIDSource, tag 761, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      BenchmarkSecurityIDSource                       { get; set; } = ValidateBenchmarkSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecuritySubType, tag 762, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecuritySubType                                 { get; set; } = ValidateSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSecuritySubType, tag 763, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSecuritySubType                       { get; set; } = ValidateUnderlyingSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 LegSecuritySubType, tag 764, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegSecuritySubType                              { get; set; } = ValidateLegSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessPct, tag 765, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllowableOneSidednessPct                        { get; set; } = ValidateAllowableOneSidednessPct;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessValue, tag 766, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AllowableOneSidednessValue                      { get; set; } = ValidateAllowableOneSidednessValue;

	/// <summary>Holds a FIX 5.0 SP2 AllowableOneSidednessCurr, tag 767, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllowableOneSidednessCurr                       { get; set; } = ValidateAllowableOneSidednessCurr;

	/// <summary>Holds a FIX 5.0 SP2 NoTrdRegTimestamps, tag 768, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTrdRegTimestamps                              { get; set; } = ValidateNoTrdRegTimestamps;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestamp, tag 769, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TrdRegTimestamp                                 { get; set; } = ValidateTrdRegTimestamp;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TrdRegTimestampType                             { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestampOrigin, tag 771, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TrdRegTimestampOrigin                           { get; set; } = ValidateTrdRegTimestampOrigin;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmRefID, tag 772, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ConfirmRefID                                    { get; set; } = ValidateConfirmRefID;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ConfirmType                                     { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ConfirmRejReason                                { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 5.0 SP2 BookingType, tag 775, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   BookingType                                     { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocRejCode, tag 776, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   IndividualAllocRejCode                          { get; set; } = ValidateIndividualAllocRejCode;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstMsgID, tag 777, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlInstMsgID                                  { get; set; } = ValidateSettlInstMsgID;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlInst, tag 778, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSettlInst                                     { get; set; } = ValidateNoSettlInst;

	/// <summary>Holds a FIX 5.0 SP2 LastUpdateTime, tag 779, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> LastUpdateTime                                  { get; set; } = ValidateLastUpdateTime;

	/// <summary>Holds a FIX 5.0 SP2 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocSettlInstType                              { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlPartyIDs, tag 781, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSettlPartyIDs                                 { get; set; } = ValidateNoSettlPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyID, tag 782, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlPartyID                                    { get; set; } = ValidateSettlPartyID;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyIDSource, tag 783, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlPartyIDSource                              { get; set; } = ValidateSettlPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartyRole, tag 784, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlPartyRole                                  { get; set; } = ValidateSettlPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartySubID, tag 785, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlPartySubID                                 { get; set; } = ValidateSettlPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 SettlPartySubIDType, tag 786, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlPartySubIDType                             { get; set; } = ValidateSettlPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DlvyInstType                                    { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 5.0 SP2 TerminationType, tag 788, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TerminationType                                 { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 5.0 SP2 NextExpectedMsgSeqNum, tag 789, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NextExpectedMsgSeqNum                           { get; set; } = ValidateNextExpectedMsgSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 OrdStatusReqID, tag 790, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrdStatusReqID                                  { get; set; } = ValidateOrdStatusReqID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstReqID, tag 791, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlInstReqID                                  { get; set; } = ValidateSettlInstReqID;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlInstReqRejCode                             { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryAllocID, tag 793, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryAllocID                                { get; set; } = ValidateSecondaryAllocID;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocReportType                                 { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 5.0 SP2 AllocReportRefID, tag 795, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocReportRefID                                { get; set; } = ValidateAllocReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocCancReplaceReason                          { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 5.0 SP2 CopyMsgIndicator, tag 797, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   CopyMsgIndicator                                { get; set; } = ValidateCopyMsgIndicator;

	/// <summary>Holds a FIX 5.0 SP2 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocAccountType                                { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 5.0 SP2 OrderAvgPx, tag 799, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderAvgPx                                      { get; set; } = ValidateOrderAvgPx;

	/// <summary>Holds a FIX 5.0 SP2 OrderBookingQty, tag 800, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderBookingQty                                 { get; set; } = ValidateOrderBookingQty;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlPartySubIDs, tag 801, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSettlPartySubIDs                              { get; set; } = ValidateNoSettlPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoPartySubIDs, tag 802, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoPartySubIDs                                   { get; set; } = ValidateNoPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PartySubIDType                                  { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedPartySubIDs, tag 804, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNestedPartySubIDs                             { get; set; } = ValidateNoNestedPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NestedPartySubIDType, tag 805, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NestedPartySubIDType                            { get; set; } = ValidateNestedPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 NoNested2PartySubIDs, tag 806, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested2PartySubIDs                            { get; set; } = ValidateNoNested2PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested2PartySubIDType, tag 807, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested2PartySubIDType                           { get; set; } = ValidateNested2PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocIntermedReqType                            { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 5.0 SP2 NoUsernames, tag 809, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUsernames                                     { get; set; } = ValidateNoUsernames;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPx, tag 810, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingPx                                    { get; set; } = ValidateUnderlyingPx;

	/// <summary>Holds a FIX 5.0 SP2 PriceDelta, tag 811, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PriceDelta                                      { get; set; } = ValidatePriceDelta;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueMax, tag 812, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplQueueMax                                    { get; set; } = ValidateApplQueueMax;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueDepth, tag 813, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplQueueDepth                                  { get; set; } = ValidateApplQueueDepth;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplQueueResolution                             { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 5.0 SP2 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplQueueAction                                 { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 5.0 SP2 NoAltMDSource, tag 816, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoAltMDSource                                   { get; set; } = ValidateNoAltMDSource;

	/// <summary>Holds a FIX 5.0 SP2 AltMDSourceID, tag 817, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AltMDSourceID                                   { get; set; } = ValidateAltMDSourceID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeReportID, tag 818, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryTradeReportID                          { get; set; } = ValidateSecondaryTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AvgPxIndicator                                  { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradeLinkID, tag 820, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeLinkID                                     { get; set; } = ValidateTradeLinkID;

	/// <summary>Holds a FIX 5.0 SP2 OrderInputDevice, tag 821, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrderInputDevice                                { get; set; } = ValidateOrderInputDevice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTradingSessionID, tag 822, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingTradingSessionID                      { get; set; } = ValidateUnderlyingTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTradingSessionSubID, tag 823, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingTradingSessionSubID                   { get; set; } = ValidateUnderlyingTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 TradeLegRefID, tag 824, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeLegRefID                                   { get; set; } = ValidateTradeLegRefID;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeRule, tag 825, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ExchangeRule                                    { get; set; } = ValidateExchangeRule;

	/// <summary>Holds a FIX 5.0 SP2 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeAllocIndicator                             { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 5.0 SP2 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ExpirationCycle                                 { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 5.0 SP2 TrdType, tag 828, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TrdType                                         { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 5.0 SP2 TrdSubType, tag 829, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TrdSubType                                      { get; set; } = ValidateTrdSubType;

	/// <summary>Holds a FIX 5.0 SP2 TransferReason, tag 830, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TransferReason                                  { get; set; } = ValidateTransferReason;

	/// <summary>Holds a FIX 5.0 SP2 TotNumAssignmentReports, tag 832, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNumAssignmentReports                         { get; set; } = ValidateTotNumAssignmentReports;

	/// <summary>Holds a FIX 5.0 SP2 AsgnRptID, tag 833, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AsgnRptID                                       { get; set; } = ValidateAsgnRptID;

	/// <summary>Holds a FIX 5.0 SP2 ThresholdAmount, tag 834, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ThresholdAmount                                 { get; set; } = ValidateThresholdAmount;

	/// <summary>Holds a FIX 5.0 SP2 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegMoveType                                     { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 5.0 SP2 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegOffsetType                                   { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 5.0 SP2 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegLimitType                                    { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 5.0 SP2 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegRoundDirection                               { get; set; } = ValidatePegRoundDirection;

	/// <summary>Holds a FIX 5.0 SP2 PeggedPrice, tag 839, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PeggedPrice                                     { get; set; } = ValidatePeggedPrice;

	/// <summary>Holds a FIX 5.0 SP2 PegScope, tag 840, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegScope                                        { get; set; } = ValidatePegScope;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionMoveType, tag 841, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DiscretionMoveType                              { get; set; } = ValidateDiscretionMoveType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionOffsetType, tag 842, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DiscretionOffsetType                            { get; set; } = ValidateDiscretionOffsetType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionLimitType, tag 843, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DiscretionLimitType                             { get; set; } = ValidateDiscretionLimitType;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionRoundDirection, tag 844, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DiscretionRoundDirection                        { get; set; } = ValidateDiscretionRoundDirection;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionPrice, tag 845, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DiscretionPrice                                 { get; set; } = ValidateDiscretionPrice;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DiscretionScope                                 { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TargetStrategy                                  { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategyParameters, tag 848, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TargetStrategyParameters                        { get; set; } = ValidateTargetStrategyParameters;

	/// <summary>Holds a FIX 5.0 SP2 ParticipationRate, tag 849, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ParticipationRate                               { get; set; } = ValidateParticipationRate;

	/// <summary>Holds a FIX 5.0 SP2 TargetStrategyPerformance, tag 850, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TargetStrategyPerformance                       { get; set; } = ValidateTargetStrategyPerformance;

	/// <summary>Holds a FIX 5.0 SP2 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LastLiquidityInd                                { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 PublishTrdIndicator, tag 852, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PublishTrdIndicator                             { get; set; } = ValidatePublishTrdIndicator;

	/// <summary>Holds a FIX 5.0 SP2 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ShortSaleReason                                 { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 5.0 SP2 QtyType, tag 854, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QtyType                                         { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTrdType, tag 855, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecondaryTrdType                                { get; set; } = ValidateSecondaryTrdType;

	/// <summary>Holds a FIX 5.0 SP2 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradeReportType                                 { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 5.0 SP2 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocNoOrdersType                               { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 5.0 SP2 SharedCommission, tag 858, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SharedCommission                                { get; set; } = ValidateSharedCommission;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmReqID, tag 859, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ConfirmReqID                                    { get; set; } = ValidateConfirmReqID;

	/// <summary>Holds a FIX 5.0 SP2 AvgParPx, tag 860, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AvgParPx                                        { get; set; } = ValidateAvgParPx;

	/// <summary>Holds a FIX 5.0 SP2 ReportedPx, tag 861, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ReportedPx                                      { get; set; } = ValidateReportedPx;

	/// <summary>Holds a FIX 5.0 SP2 NoCapacities, tag 862, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoCapacities                                    { get; set; } = ValidateNoCapacities;

	/// <summary>Holds a FIX 5.0 SP2 OrderCapacityQty, tag 863, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OrderCapacityQty                                { get; set; } = ValidateOrderCapacityQty;

	/// <summary>Holds a FIX 5.0 SP2 NoEvents, tag 864, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoEvents                                        { get; set; } = ValidateNoEvents;

	/// <summary>Holds a FIX 5.0 SP2 EventType, tag 865, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EventType                                       { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 5.0 SP2 EventDate, tag 866, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      EventDate                                       { get; set; } = ValidateEventDate;

	/// <summary>Holds a FIX 5.0 SP2 EventPx, tag 867, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EventPx                                         { get; set; } = ValidateEventPx;

	/// <summary>Holds a FIX 5.0 SP2 EventText, tag 868, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      EventText                                       { get; set; } = ValidateEventText;

	/// <summary>Holds a FIX 5.0 SP2 PctAtRisk, tag 869, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PctAtRisk                                       { get; set; } = ValidatePctAtRisk;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrAttrib, tag 870, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoInstrAttrib                                   { get; set; } = ValidateNoInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   InstrAttribType                                 { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 InstrAttribValue, tag 872, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InstrAttribValue                                { get; set; } = ValidateInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 DatedDate, tag 873, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DatedDate                                       { get; set; } = ValidateDatedDate;

	/// <summary>Holds a FIX 5.0 SP2 InterestAccrualDate, tag 874, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      InterestAccrualDate                             { get; set; } = ValidateInterestAccrualDate;

	/// <summary>Holds a FIX 5.0 SP2 CPProgram, tag 875, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CPProgram                                       { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 5.0 SP2 CPRegType, tag 876, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CPRegType                                       { get; set; } = ValidateCPRegType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCPProgram, tag 877, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCPProgram                             { get; set; } = ValidateUnderlyingCPProgram;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCPRegType, tag 878, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCPRegType                             { get; set; } = ValidateUnderlyingCPRegType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingQty, tag 879, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingQty                                   { get; set; } = ValidateUnderlyingQty;

	/// <summary>Holds a FIX 5.0 SP2 TrdMatchID, tag 880, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TrdMatchID                                      { get; set; } = ValidateTrdMatchID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeReportRefID, tag 881, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryTradeReportRefID                       { get; set; } = ValidateSecondaryTradeReportRefID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDirtyPrice, tag 882, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingDirtyPrice                            { get; set; } = ValidateUnderlyingDirtyPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingEndPrice, tag 883, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingEndPrice                              { get; set; } = ValidateUnderlyingEndPrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStartValue, tag 884, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingStartValue                            { get; set; } = ValidateUnderlyingStartValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCurrentValue, tag 885, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingCurrentValue                          { get; set; } = ValidateUnderlyingCurrentValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingEndValue, tag 886, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingEndValue                              { get; set; } = ValidateUnderlyingEndValue;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingStips, tag 887, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUnderlyingStips                               { get; set; } = ValidateNoUnderlyingStips;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStipType, tag 888, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingStipType                              { get; set; } = ValidateUnderlyingStipType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStipValue, tag 889, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingStipValue                             { get; set; } = ValidateUnderlyingStipValue;

	/// <summary>Holds a FIX 5.0 SP2 MaturityNetMoney, tag 890, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MaturityNetMoney                                { get; set; } = ValidateMaturityNetMoney;

	/// <summary>Holds a FIX 5.0 SP2 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MiscFeeBasis                                    { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 5.0 SP2 TotNoAllocs, tag 892, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoAllocs                                     { get; set; } = ValidateTotNoAllocs;

	/// <summary>Holds a FIX 5.0 SP2 LastFragment, tag 893, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   LastFragment                                    { get; set; } = ValidateLastFragment;

	/// <summary>Holds a FIX 5.0 SP2 CollReqID, tag 894, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollReqID                                       { get; set; } = ValidateCollReqID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollAsgnReason                                  { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollInquiryQualifier                            { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 5.0 SP2 NoTrades, tag 897, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTrades                                        { get; set; } = ValidateNoTrades;

	/// <summary>Holds a FIX 5.0 SP2 MarginRatio, tag 898, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MarginRatio                                     { get; set; } = ValidateMarginRatio;

	/// <summary>Holds a FIX 5.0 SP2 MarginExcess, tag 899, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MarginExcess                                    { get; set; } = ValidateMarginExcess;

	/// <summary>Holds a FIX 5.0 SP2 TotalNetValue, tag 900, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TotalNetValue                                   { get; set; } = ValidateTotalNetValue;

	/// <summary>Holds a FIX 5.0 SP2 CashOutstanding, tag 901, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CashOutstanding                                 { get; set; } = ValidateCashOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnID, tag 902, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollAsgnID                                      { get; set; } = ValidateCollAsgnID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollAsgnTransType                               { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 5.0 SP2 CollRespID, tag 904, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollRespID                                      { get; set; } = ValidateCollRespID;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollAsgnRespType                                { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollAsgnRejectReason                            { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 CollAsgnRefID, tag 907, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollAsgnRefID                                   { get; set; } = ValidateCollAsgnRefID;

	/// <summary>Holds a FIX 5.0 SP2 CollRptID, tag 908, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollRptID                                       { get; set; } = ValidateCollRptID;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryID, tag 909, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CollInquiryID                                   { get; set; } = ValidateCollInquiryID;

	/// <summary>Holds a FIX 5.0 SP2 CollStatus, tag 910, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollStatus                                      { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 5.0 SP2 TotNumReports, tag 911, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNumReports                                   { get; set; } = ValidateTotNumReports;

	/// <summary>Holds a FIX 5.0 SP2 LastRptRequested, tag 912, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   LastRptRequested                                { get; set; } = ValidateLastRptRequested;

	/// <summary>Holds a FIX 5.0 SP2 AgreementDesc, tag 913, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AgreementDesc                                   { get; set; } = ValidateAgreementDesc;

	/// <summary>Holds a FIX 5.0 SP2 AgreementID, tag 914, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AgreementID                                     { get; set; } = ValidateAgreementID;

	/// <summary>Holds a FIX 5.0 SP2 AgreementDate, tag 915, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      AgreementDate                                   { get; set; } = ValidateAgreementDate;

	/// <summary>Holds a FIX 5.0 SP2 StartDate, tag 916, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      StartDate                                       { get; set; } = ValidateStartDate;

	/// <summary>Holds a FIX 5.0 SP2 EndDate, tag 917, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      EndDate                                         { get; set; } = ValidateEndDate;

	/// <summary>Holds a FIX 5.0 SP2 AgreementCurrency, tag 918, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AgreementCurrency                               { get; set; } = ValidateAgreementCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DeliveryType                                    { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 5.0 SP2 EndAccruedInterestAmt, tag 920, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EndAccruedInterestAmt                           { get; set; } = ValidateEndAccruedInterestAmt;

	/// <summary>Holds a FIX 5.0 SP2 StartCash, tag 921, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StartCash                                       { get; set; } = ValidateStartCash;

	/// <summary>Holds a FIX 5.0 SP2 EndCash, tag 922, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EndCash                                         { get; set; } = ValidateEndCash;

	/// <summary>Holds a FIX 5.0 SP2 UserRequestID, tag 923, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UserRequestID                                   { get; set; } = ValidateUserRequestID;

	/// <summary>Holds a FIX 5.0 SP2 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UserRequestType                                 { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 5.0 SP2 NewPassword, tag 925, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NewPassword                                     { get; set; } = ValidateNewPassword;

	/// <summary>Holds a FIX 5.0 SP2 UserStatus, tag 926, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UserStatus                                      { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 5.0 SP2 UserStatusText, tag 927, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UserStatusText                                  { get; set; } = ValidateUserStatusText;

	/// <summary>Holds a FIX 5.0 SP2 StatusValue, tag 928, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StatusValue                                     { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 5.0 SP2 StatusText, tag 929, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StatusText                                      { get; set; } = ValidateStatusText;

	/// <summary>Holds a FIX 5.0 SP2 RefCompID, tag 930, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefCompID                                       { get; set; } = ValidateRefCompID;

	/// <summary>Holds a FIX 5.0 SP2 RefSubID, tag 931, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefSubID                                        { get; set; } = ValidateRefSubID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkResponseID, tag 932, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NetworkResponseID                               { get; set; } = ValidateNetworkResponseID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkRequestID, tag 933, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NetworkRequestID                                { get; set; } = ValidateNetworkRequestID;

	/// <summary>Holds a FIX 5.0 SP2 LastNetworkResponseID, tag 934, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LastNetworkResponseID                           { get; set; } = ValidateLastNetworkResponseID;

	/// <summary>Holds a FIX 5.0 SP2 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NetworkRequestType                              { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 5.0 SP2 NoCompIDs, tag 936, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoCompIDs                                       { get; set; } = ValidateNoCompIDs;

	/// <summary>Holds a FIX 5.0 SP2 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NetworkStatusResponseType                       { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 5.0 SP2 NoCollInquiryQualifier, tag 938, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoCollInquiryQualifier                          { get; set; } = ValidateNoCollInquiryQualifier;

	/// <summary>Holds a FIX 5.0 SP2 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TrdRptStatus                                    { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 5.0 SP2 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AffirmStatus                                    { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStrikeCurrency, tag 941, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingStrikeCurrency                        { get; set; } = ValidateUnderlyingStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 LegStrikeCurrency, tag 942, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegStrikeCurrency                               { get; set; } = ValidateLegStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TimeBracket, tag 943, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TimeBracket                                     { get; set; } = ValidateTimeBracket;

	/// <summary>Holds a FIX 5.0 SP2 CollAction, tag 944, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollAction                                      { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollInquiryStatus                               { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 5.0 SP2 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollInquiryResult                               { get; set; } = ValidateCollInquiryResult;

	/// <summary>Holds a FIX 5.0 SP2 StrikeCurrency, tag 947, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StrikeCurrency                                  { get; set; } = ValidateStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 NoNested3PartyIDs, tag 948, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested3PartyIDs                               { get; set; } = ValidateNoNested3PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyID, tag 949, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested3PartyID                                  { get; set; } = ValidateNested3PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyIDSource, tag 950, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> Nested3PartyIDSource                            { get; set; } = ValidateNested3PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartyRole, tag 951, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested3PartyRole                                { get; set; } = ValidateNested3PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoNested3PartySubIDs, tag 952, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested3PartySubIDs                            { get; set; } = ValidateNoNested3PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartySubID, tag 953, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested3PartySubID                               { get; set; } = ValidateNested3PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 Nested3PartySubIDType, tag 954, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested3PartySubIDType                           { get; set; } = ValidateNested3PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 LegContractSettlMonth, tag 955, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> LegContractSettlMonth                           { get; set; } = ValidateLegContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 LegInterestAccrualDate, tag 956, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      LegInterestAccrualDate                          { get; set; } = ValidateLegInterestAccrualDate;

	/// <summary>Holds a FIX 5.0 SP2 NoStrategyParameters, tag 957, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoStrategyParameters                            { get; set; } = ValidateNoStrategyParameters;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterName, tag 958, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StrategyParameterName                           { get; set; } = ValidateStrategyParameterName;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterType, tag 959, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StrategyParameterType                           { get; set; } = ValidateStrategyParameterType;

	/// <summary>Holds a FIX 5.0 SP2 StrategyParameterValue, tag 960, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StrategyParameterValue                          { get; set; } = ValidateStrategyParameterValue;

	/// <summary>Holds a FIX 5.0 SP2 HostCrossID, tag 961, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      HostCrossID                                     { get; set; } = ValidateHostCrossID;

	/// <summary>Holds a FIX 5.0 SP2 SideTimeInForce, tag 962, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> SideTimeInForce                                 { get; set; } = ValidateSideTimeInForce;

	/// <summary>Holds a FIX 5.0 SP2 MDReportID, tag 963, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDReportID                                      { get; set; } = ValidateMDReportID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityReportID, tag 964, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityReportID                                { get; set; } = ValidateSecurityReportID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatus, tag 965, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityStatus                                  { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 5.0 SP2 SettleOnOpenFlag, tag 966, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettleOnOpenFlag                                { get; set; } = ValidateSettleOnOpenFlag;

	/// <summary>Holds a FIX 5.0 SP2 StrikeMultiplier, tag 967, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StrikeMultiplier                                { get; set; } = ValidateStrikeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 StrikeValue, tag 968, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StrikeValue                                     { get; set; } = ValidateStrikeValue;

	/// <summary>Holds a FIX 5.0 SP2 MinPriceIncrement, tag 969, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinPriceIncrement                               { get; set; } = ValidateMinPriceIncrement;

	/// <summary>Holds a FIX 5.0 SP2 PositionLimit, tag 970, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PositionLimit                                   { get; set; } = ValidatePositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 NTPositionLimit, tag 971, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NTPositionLimit                                 { get; set; } = ValidateNTPositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAllocationPercent, tag 972, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingAllocationPercent                     { get; set; } = ValidateUnderlyingAllocationPercent;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCashAmount, tag 973, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingCashAmount                            { get; set; } = ValidateUnderlyingCashAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCashType, tag 974, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingCashType                              { get; set; } = ValidateUnderlyingCashType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementType, tag 975, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingSettlementType                        { get; set; } = ValidateUnderlyingSettlementType;

	/// <summary>Holds a FIX 5.0 SP2 QuantityDate, tag 976, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      QuantityDate                                    { get; set; } = ValidateQuantityDate;

	/// <summary>Holds a FIX 5.0 SP2 ContIntRptID, tag 977, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ContIntRptID                                    { get; set; } = ValidateContIntRptID;

	/// <summary>Holds a FIX 5.0 SP2 LateIndicator, tag 978, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   LateIndicator                                   { get; set; } = ValidateLateIndicator;

	/// <summary>Holds a FIX 5.0 SP2 InputSource, tag 979, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InputSource                                     { get; set; } = ValidateInputSource;

	/// <summary>Holds a FIX 5.0 SP2 SecurityUpdateAction, tag 980, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SecurityUpdateAction                            { get; set; } = ValidateSecurityUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 NoExpiration, tag 981, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoExpiration                                    { get; set; } = ValidateNoExpiration;

	/// <summary>Holds a FIX 5.0 SP2 ExpirationQtyType, tag 982, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ExpirationQtyType                               { get; set; } = ValidateExpirationQtyType;

	/// <summary>Holds a FIX 5.0 SP2 ExpQty, tag 983, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ExpQty                                          { get; set; } = ValidateExpQty;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingAmounts, tag 984, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUnderlyingAmounts                             { get; set; } = ValidateNoUnderlyingAmounts;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPayAmount, tag 985, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingPayAmount                             { get; set; } = ValidateUnderlyingPayAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCollectAmount, tag 986, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingCollectAmount                         { get; set; } = ValidateUnderlyingCollectAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementDate, tag 987, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingSettlementDate                        { get; set; } = ValidateUnderlyingSettlementDate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlementStatus, tag 988, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSettlementStatus                      { get; set; } = ValidateUnderlyingSettlementStatus;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryIndividualAllocID, tag 989, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryIndividualAllocID                      { get; set; } = ValidateSecondaryIndividualAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegReportID, tag 990, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegReportID                                     { get; set; } = ValidateLegReportID;

	/// <summary>Holds a FIX 5.0 SP2 RndPx, tag 991, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RndPx                                           { get; set; } = ValidateRndPx;

	/// <summary>Holds a FIX 5.0 SP2 IndividualAllocType, tag 992, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   IndividualAllocType                             { get; set; } = ValidateIndividualAllocType;

	/// <summary>Holds a FIX 5.0 SP2 AllocCustomerCapacity, tag 993, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocCustomerCapacity                           { get; set; } = ValidateAllocCustomerCapacity;

	/// <summary>Holds a FIX 5.0 SP2 TierCode, tag 994, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TierCode                                        { get; set; } = ValidateTierCode;

	/// <summary>Holds a FIX 5.0 SP2 UnitOfMeasure, tag 996, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnitOfMeasure                                   { get; set; } = ValidateUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 TimeUnit, tag 997, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TimeUnit                                        { get; set; } = ValidateTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingUnitOfMeasure, tag 998, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingUnitOfMeasure                         { get; set; } = ValidateUnderlyingUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 LegUnitOfMeasure, tag 999, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegUnitOfMeasure                                { get; set; } = ValidateLegUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingTimeUnit, tag 1000, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingTimeUnit                              { get; set; } = ValidateUnderlyingTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 LegTimeUnit, tag 1001, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegTimeUnit                                     { get; set; } = ValidateLegTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 AllocMethod, tag 1002, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   AllocMethod                                     { get; set; } = ValidateAllocMethod;

	/// <summary>Holds a FIX 5.0 SP2 TradeID, tag 1003, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradeID                                         { get; set; } = ValidateTradeID;

	/// <summary>Holds a FIX 5.0 SP2 SideTradeReportID, tag 1005, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideTradeReportID                               { get; set; } = ValidateSideTradeReportID;

	/// <summary>Holds a FIX 5.0 SP2 SideFillStationCd, tag 1006, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideFillStationCd                               { get; set; } = ValidateSideFillStationCd;

	/// <summary>Holds a FIX 5.0 SP2 SideReasonCd, tag 1007, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideReasonCd                                    { get; set; } = ValidateSideReasonCd;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdSubTyp, tag 1008, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideTrdSubTyp                                   { get; set; } = ValidateSideTrdSubTyp;

	/// <summary>Holds a FIX 5.0 SP2 SideLastQty, tag 1009, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideLastQty                                     { get; set; } = ValidateSideLastQty;

	/// <summary>Holds a FIX 5.0 SP2 MessageEventSource, tag 1011, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MessageEventSource                              { get; set; } = ValidateMessageEventSource;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestamp, tag 1012, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> SideTrdRegTimestamp                             { get; set; } = ValidateSideTrdRegTimestamp;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestampType, tag 1013, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideTrdRegTimestampType                         { get; set; } = ValidateSideTrdRegTimestampType;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTimestampSrc, tag 1014, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideTrdRegTimestampSrc                          { get; set; } = ValidateSideTrdRegTimestampSrc;

	/// <summary>Holds a FIX 5.0 SP2 AsOfIndicator, tag 1015, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> AsOfIndicator                                   { get; set; } = ValidateAsOfIndicator;

	/// <summary>Holds a FIX 5.0 SP2 NoSideTrdRegTS, tag 1016, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSideTrdRegTS                                  { get; set; } = ValidateNoSideTrdRegTS;

	/// <summary>Holds a FIX 5.0 SP2 LegOptionRatio, tag 1017, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegOptionRatio                                  { get; set; } = ValidateLegOptionRatio;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrumentParties, tag 1018, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoInstrumentParties                             { get; set; } = ValidateNoInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyID, tag 1019, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InstrumentPartyID                               { get; set; } = ValidateInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 TradeVolume, tag 1020, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TradeVolume                                     { get; set; } = ValidateTradeVolume;

	/// <summary>Holds a FIX 5.0 SP2 MDBookType, tag 1021, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDBookType                                      { get; set; } = ValidateMDBookType;

	/// <summary>Holds a FIX 5.0 SP2 MDFeedType, tag 1022, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDFeedType                                      { get; set; } = ValidateMDFeedType;

	/// <summary>Holds a FIX 5.0 SP2 MDPriceLevel, tag 1023, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDPriceLevel                                    { get; set; } = ValidateMDPriceLevel;

	/// <summary>Holds a FIX 5.0 SP2 MDOriginType, tag 1024, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDOriginType                                    { get; set; } = ValidateMDOriginType;

	/// <summary>Holds a FIX 5.0 SP2 FirstPx, tag 1025, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FirstPx                                         { get; set; } = ValidateFirstPx;

	/// <summary>Holds a FIX 5.0 SP2 MDEntrySpotRate, tag 1026, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MDEntrySpotRate                                 { get; set; } = ValidateMDEntrySpotRate;

	/// <summary>Holds a FIX 5.0 SP2 MDEntryForwardPoints, tag 1027, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MDEntryForwardPoints                            { get; set; } = ValidateMDEntryForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 ManualOrderIndicator, tag 1028, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ManualOrderIndicator                            { get; set; } = ValidateManualOrderIndicator;

	/// <summary>Holds a FIX 5.0 SP2 CustDirectedOrder, tag 1029, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   CustDirectedOrder                               { get; set; } = ValidateCustDirectedOrder;

	/// <summary>Holds a FIX 5.0 SP2 ReceivedDeptID, tag 1030, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ReceivedDeptID                                  { get; set; } = ValidateReceivedDeptID;

	/// <summary>Holds a FIX 5.0 SP2 CustOrderHandlingInst, tag 1031, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  CustOrderHandlingInst                           { get; set; } = ValidateCustOrderHandlingInst;

	/// <summary>Holds a FIX 5.0 SP2 OrderHandlingInstSource, tag 1032, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OrderHandlingInstSource                         { get; set; } = ValidateOrderHandlingInstSource;

	/// <summary>Holds a FIX 5.0 SP2 DeskType, tag 1033, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DeskType                                        { get; set; } = ValidateDeskType;

	/// <summary>Holds a FIX 5.0 SP2 DeskTypeSource, tag 1034, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DeskTypeSource                                  { get; set; } = ValidateDeskTypeSource;

	/// <summary>Holds a FIX 5.0 SP2 DeskOrderHandlingInst, tag 1035, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  DeskOrderHandlingInst                           { get; set; } = ValidateDeskOrderHandlingInst;

	/// <summary>Holds a FIX 5.0 SP2 ExecAckStatus, tag 1036, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExecAckStatus                                   { get; set; } = ValidateExecAckStatus;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDeliveryAmount, tag 1037, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingDeliveryAmount                        { get; set; } = ValidateUnderlyingDeliveryAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingCapValue, tag 1038, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingCapValue                              { get; set; } = ValidateUnderlyingCapValue;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSettlMethod, tag 1039, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSettlMethod                           { get; set; } = ValidateUnderlyingSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradeID, tag 1040, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryTradeID                                { get; set; } = ValidateSecondaryTradeID;

	/// <summary>Holds a FIX 5.0 SP2 FirmTradeID, tag 1041, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      FirmTradeID                                     { get; set; } = ValidateFirmTradeID;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryFirmTradeID, tag 1042, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecondaryFirmTradeID                            { get; set; } = ValidateSecondaryFirmTradeID;

	/// <summary>Holds a FIX 5.0 SP2 CollApplType, tag 1043, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   CollApplType                                    { get; set; } = ValidateCollApplType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAdjustedQuantity, tag 1044, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingAdjustedQuantity                      { get; set; } = ValidateUnderlyingAdjustedQuantity;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFXRate, tag 1045, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingFXRate                                { get; set; } = ValidateUnderlyingFXRate;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFXRateCalc, tag 1046, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> UnderlyingFXRateCalc                            { get; set; } = ValidateUnderlyingFXRateCalc;

	/// <summary>Holds a FIX 5.0 SP2 AllocPositionEffect, tag 1047, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> AllocPositionEffect                             { get; set; } = ValidateAllocPositionEffect;

	/// <summary>Holds a FIX 5.0 SP2 DealingCapacity, tag 1048, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DealingCapacity                                 { get; set; } = ValidateDealingCapacity;

	/// <summary>Holds a FIX 5.0 SP2 InstrmtAssignmentMethod, tag 1049, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> InstrmtAssignmentMethod                         { get; set; } = ValidateInstrmtAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyIDSource, tag 1050, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> InstrumentPartyIDSource                         { get; set; } = ValidateInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartyRole, tag 1051, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   InstrumentPartyRole                             { get; set; } = ValidateInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoInstrumentPartySubIDs, tag 1052, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoInstrumentPartySubIDs                         { get; set; } = ValidateNoInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartySubID, tag 1053, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      InstrumentPartySubID                            { get; set; } = ValidateInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentPartySubIDType, tag 1054, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   InstrumentPartySubIDType                        { get; set; } = ValidateInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 PositionCurrency, tag 1055, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PositionCurrency                                { get; set; } = ValidatePositionCurrency;

	/// <summary>Holds a FIX 5.0 SP2 CalculatedCcyLastQty, tag 1056, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CalculatedCcyLastQty                            { get; set; } = ValidateCalculatedCcyLastQty;

	/// <summary>Holds a FIX 5.0 SP2 AggressorIndicator, tag 1057, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   AggressorIndicator                              { get; set; } = ValidateAggressorIndicator;

	/// <summary>Holds a FIX 5.0 SP2 NoUndlyInstrumentParties, tag 1058, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUndlyInstrumentParties                        { get; set; } = ValidateNoUndlyInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyID, tag 1059, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingInstrumentPartyID                     { get; set; } = ValidateUnderlyingInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyIDSource, tag 1060, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> UnderlyingInstrumentPartyIDSource               { get; set; } = ValidateUnderlyingInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartyRole, tag 1061, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingInstrumentPartyRole                   { get; set; } = ValidateUnderlyingInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoUndlyInstrumentPartySubIDs, tag 1062, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUndlyInstrumentPartySubIDs                    { get; set; } = ValidateNoUndlyInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartySubID, tag 1063, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingInstrumentPartySubID                  { get; set; } = ValidateUnderlyingInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrumentPartySubIDType, tag 1064, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingInstrumentPartySubIDType              { get; set; } = ValidateUnderlyingInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 BidSwapPoints, tag 1065, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   BidSwapPoints                                   { get; set; } = ValidateBidSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 OfferSwapPoints, tag 1066, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OfferSwapPoints                                 { get; set; } = ValidateOfferSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegBidForwardPoints, tag 1067, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegBidForwardPoints                             { get; set; } = ValidateLegBidForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegOfferForwardPoints, tag 1068, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegOfferForwardPoints                           { get; set; } = ValidateLegOfferForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 SwapPoints, tag 1069, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SwapPoints                                      { get; set; } = ValidateSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 MDQuoteType, tag 1070, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDQuoteType                                     { get; set; } = ValidateMDQuoteType;

	/// <summary>Holds a FIX 5.0 SP2 LastSwapPoints, tag 1071, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LastSwapPoints                                  { get; set; } = ValidateLastSwapPoints;

	/// <summary>Holds a FIX 5.0 SP2 SideGrossTradeAmt, tag 1072, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SideGrossTradeAmt                               { get; set; } = ValidateSideGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 LegLastForwardPoints, tag 1073, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegLastForwardPoints                            { get; set; } = ValidateLegLastForwardPoints;

	/// <summary>Holds a FIX 5.0 SP2 LegCalculatedCcyLastQty, tag 1074, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegCalculatedCcyLastQty                         { get; set; } = ValidateLegCalculatedCcyLastQty;

	/// <summary>Holds a FIX 5.0 SP2 LegGrossTradeAmt, tag 1075, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegGrossTradeAmt                                { get; set; } = ValidateLegGrossTradeAmt;

	/// <summary>Holds a FIX 5.0 SP2 MaturityTime, tag 1079, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.ZonedTime, bool> MaturityTime                                    { get; set; } = ValidateMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 RefOrderID, tag 1080, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefOrderID                                      { get; set; } = ValidateRefOrderID;

	/// <summary>Holds a FIX 5.0 SP2 RefOrderIDSource, tag 1081, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> RefOrderIDSource                                { get; set; } = ValidateRefOrderIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryDisplayQty, tag 1082, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SecondaryDisplayQty                             { get; set; } = ValidateSecondaryDisplayQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayWhen, tag 1083, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DisplayWhen                                     { get; set; } = ValidateDisplayWhen;

	/// <summary>Holds a FIX 5.0 SP2 DisplayMethod, tag 1084, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DisplayMethod                                   { get; set; } = ValidateDisplayMethod;

	/// <summary>Holds a FIX 5.0 SP2 DisplayLowQty, tag 1085, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DisplayLowQty                                   { get; set; } = ValidateDisplayLowQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayHighQty, tag 1086, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DisplayHighQty                                  { get; set; } = ValidateDisplayHighQty;

	/// <summary>Holds a FIX 5.0 SP2 DisplayMinIncr, tag 1087, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DisplayMinIncr                                  { get; set; } = ValidateDisplayMinIncr;

	/// <summary>Holds a FIX 5.0 SP2 RefreshQty, tag 1088, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RefreshQty                                      { get; set; } = ValidateRefreshQty;

	/// <summary>Holds a FIX 5.0 SP2 MatchIncrement, tag 1089, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MatchIncrement                                  { get; set; } = ValidateMatchIncrement;

	/// <summary>Holds a FIX 5.0 SP2 MaxPriceLevels, tag 1090, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MaxPriceLevels                                  { get; set; } = ValidateMaxPriceLevels;

	/// <summary>Holds a FIX 5.0 SP2 PreTradeAnonymity, tag 1091, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PreTradeAnonymity                               { get; set; } = ValidatePreTradeAnonymity;

	/// <summary>Holds a FIX 5.0 SP2 PriceProtectionScope, tag 1092, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> PriceProtectionScope                            { get; set; } = ValidatePriceProtectionScope;

	/// <summary>Holds a FIX 5.0 SP2 LotType, tag 1093, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> LotType                                         { get; set; } = ValidateLotType;

	/// <summary>Holds a FIX 5.0 SP2 PegPriceType, tag 1094, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PegPriceType                                    { get; set; } = ValidatePegPriceType;

	/// <summary>Holds a FIX 5.0 SP2 PeggedRefPrice, tag 1095, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PeggedRefPrice                                  { get; set; } = ValidatePeggedRefPrice;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityIDSource, tag 1096, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PegSecurityIDSource                             { get; set; } = ValidatePegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityID, tag 1097, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PegSecurityID                                   { get; set; } = ValidatePegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 PegSymbol, tag 1098, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PegSymbol                                       { get; set; } = ValidatePegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 PegSecurityDesc, tag 1099, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PegSecurityDesc                                 { get; set; } = ValidatePegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 TriggerType, tag 1100, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerType                                     { get; set; } = ValidateTriggerType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerAction, tag 1101, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerAction                                   { get; set; } = ValidateTriggerAction;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPrice, tag 1102, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TriggerPrice                                    { get; set; } = ValidateTriggerPrice;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSymbol, tag 1103, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerSymbol                                   { get; set; } = ValidateTriggerSymbol;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityID, tag 1104, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerSecurityID                               { get; set; } = ValidateTriggerSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityIDSource, tag 1105, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerSecurityIDSource                         { get; set; } = ValidateTriggerSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 TriggerSecurityDesc, tag 1106, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerSecurityDesc                             { get; set; } = ValidateTriggerSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceType, tag 1107, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerPriceType                                { get; set; } = ValidateTriggerPriceType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceTypeScope, tag 1108, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerPriceTypeScope                           { get; set; } = ValidateTriggerPriceTypeScope;

	/// <summary>Holds a FIX 5.0 SP2 TriggerPriceDirection, tag 1109, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerPriceDirection                           { get; set; } = ValidateTriggerPriceDirection;

	/// <summary>Holds a FIX 5.0 SP2 TriggerNewPrice, tag 1110, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TriggerNewPrice                                 { get; set; } = ValidateTriggerNewPrice;

	/// <summary>Holds a FIX 5.0 SP2 TriggerOrderType, tag 1111, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TriggerOrderType                                { get; set; } = ValidateTriggerOrderType;

	/// <summary>Holds a FIX 5.0 SP2 TriggerNewQty, tag 1112, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TriggerNewQty                                   { get; set; } = ValidateTriggerNewQty;

	/// <summary>Holds a FIX 5.0 SP2 TriggerTradingSessionID, tag 1113, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerTradingSessionID                         { get; set; } = ValidateTriggerTradingSessionID;

	/// <summary>Holds a FIX 5.0 SP2 TriggerTradingSessionSubID, tag 1114, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TriggerTradingSessionSubID                      { get; set; } = ValidateTriggerTradingSessionSubID;

	/// <summary>Holds a FIX 5.0 SP2 OrderCategory, tag 1115, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OrderCategory                                   { get; set; } = ValidateOrderCategory;

	/// <summary>Holds a FIX 5.0 SP2 NoRootPartyIDs, tag 1116, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRootPartyIDs                                  { get; set; } = ValidateNoRootPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyID, tag 1117, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RootPartyID                                     { get; set; } = ValidateRootPartyID;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyIDSource, tag 1118, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> RootPartyIDSource                               { get; set; } = ValidateRootPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 RootPartyRole, tag 1119, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RootPartyRole                                   { get; set; } = ValidateRootPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoRootPartySubIDs, tag 1120, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRootPartySubIDs                               { get; set; } = ValidateNoRootPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 RootPartySubID, tag 1121, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RootPartySubID                                  { get; set; } = ValidateRootPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 RootPartySubIDType, tag 1122, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RootPartySubIDType                              { get; set; } = ValidateRootPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 TradeHandlingInstr, tag 1123, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TradeHandlingInstr                              { get; set; } = ValidateTradeHandlingInstr;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeHandlingInstr, tag 1124, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> OrigTradeHandlingInstr                          { get; set; } = ValidateOrigTradeHandlingInstr;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeDate, tag 1125, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      OrigTradeDate                                   { get; set; } = ValidateOrigTradeDate;

	/// <summary>Holds a FIX 5.0 SP2 OrigTradeID, tag 1126, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrigTradeID                                     { get; set; } = ValidateOrigTradeID;

	/// <summary>Holds a FIX 5.0 SP2 OrigSecondaryTradeID, tag 1127, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      OrigSecondaryTradeID                            { get; set; } = ValidateOrigSecondaryTradeID;

	/// <summary>Holds a FIX 5.0 SP2 ApplVerID, tag 1128, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ApplVerID                                       { get; set; } = ValidateApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 CstmApplVerID, tag 1129, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      CstmApplVerID                                   { get; set; } = ValidateCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 RefApplVerID, tag 1130, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefApplVerID                                    { get; set; } = ValidateRefApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 RefCstmApplVerID, tag 1131, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefCstmApplVerID                                { get; set; } = ValidateRefCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 TZTransactTime, tag 1132, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> TZTransactTime                                  { get; set; } = ValidateTZTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 ExDestinationIDSource, tag 1133, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExDestinationIDSource                           { get; set; } = ValidateExDestinationIDSource;

	/// <summary>Holds a FIX 5.0 SP2 ReportedPxDiff, tag 1134, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ReportedPxDiff                                  { get; set; } = ValidateReportedPxDiff;

	/// <summary>Holds a FIX 5.0 SP2 RptSys, tag 1135, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RptSys                                          { get; set; } = ValidateRptSys;

	/// <summary>Holds a FIX 5.0 SP2 AllocClearingFeeIndicator, tag 1136, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      AllocClearingFeeIndicator                       { get; set; } = ValidateAllocClearingFeeIndicator;

	/// <summary>Holds a FIX 5.0 SP2 DefaultApplVerID, tag 1137, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DefaultApplVerID                                { get; set; } = ValidateDefaultApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 DisplayQty, tag 1138, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DisplayQty                                      { get; set; } = ValidateDisplayQty;

	/// <summary>Holds a FIX 5.0 SP2 ExchangeSpecialInstructions, tag 1139, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ExchangeSpecialInstructions                     { get; set; } = ValidateExchangeSpecialInstructions;

	/// <summary>Holds a FIX 5.0 SP2 MaxTradeVol, tag 1140, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MaxTradeVol                                     { get; set; } = ValidateMaxTradeVol;

	/// <summary>Holds a FIX 5.0 SP2 NoMDFeedTypes, tag 1141, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMDFeedTypes                                   { get; set; } = ValidateNoMDFeedTypes;

	/// <summary>Holds a FIX 5.0 SP2 MatchAlgorithm, tag 1142, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MatchAlgorithm                                  { get; set; } = ValidateMatchAlgorithm;

	/// <summary>Holds a FIX 5.0 SP2 MaxPriceVariation, tag 1143, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MaxPriceVariation                               { get; set; } = ValidateMaxPriceVariation;

	/// <summary>Holds a FIX 5.0 SP2 ImpliedMarketIndicator, tag 1144, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ImpliedMarketIndicator                          { get; set; } = ValidateImpliedMarketIndicator;

	/// <summary>Holds a FIX 5.0 SP2 EventTime, tag 1145, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> EventTime                                       { get; set; } = ValidateEventTime;

	/// <summary>Holds a FIX 5.0 SP2 MinPriceIncrementAmount, tag 1146, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinPriceIncrementAmount                         { get; set; } = ValidateMinPriceIncrementAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnitOfMeasureQty, tag 1147, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnitOfMeasureQty                                { get; set; } = ValidateUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 LowLimitPrice, tag 1148, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LowLimitPrice                                   { get; set; } = ValidateLowLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 HighLimitPrice, tag 1149, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   HighLimitPrice                                  { get; set; } = ValidateHighLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 TradingReferencePrice, tag 1150, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TradingReferencePrice                           { get; set; } = ValidateTradingReferencePrice;

	/// <summary>Holds a FIX 5.0 SP2 SecurityGroup, tag 1151, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityGroup                                   { get; set; } = ValidateSecurityGroup;

	/// <summary>Holds a FIX 5.0 SP2 LegNumber, tag 1152, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegNumber                                       { get; set; } = ValidateLegNumber;

	/// <summary>Holds a FIX 5.0 SP2 SettlementCycleNo, tag 1153, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlementCycleNo                               { get; set; } = ValidateSettlementCycleNo;

	/// <summary>Holds a FIX 5.0 SP2 SideCurrency, tag 1154, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideCurrency                                    { get; set; } = ValidateSideCurrency;

	/// <summary>Holds a FIX 5.0 SP2 SideSettlCurrency, tag 1155, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideSettlCurrency                               { get; set; } = ValidateSideSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 ApplExtID, tag 1156, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplExtID                                       { get; set; } = ValidateApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 CcyAmt, tag 1157, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CcyAmt                                          { get; set; } = ValidateCcyAmt;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlDetails, tag 1158, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSettlDetails                                  { get; set; } = ValidateNoSettlDetails;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligMode, tag 1159, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SettlObligMode                                  { get; set; } = ValidateSettlObligMode;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligMsgID, tag 1160, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlObligMsgID                                 { get; set; } = ValidateSettlObligMsgID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligID, tag 1161, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlObligID                                    { get; set; } = ValidateSettlObligID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligTransType, tag 1162, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlObligTransType                             { get; set; } = ValidateSettlObligTransType;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligRefID, tag 1163, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SettlObligRefID                                 { get; set; } = ValidateSettlObligRefID;

	/// <summary>Holds a FIX 5.0 SP2 SettlObligSource, tag 1164, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlObligSource                                { get; set; } = ValidateSettlObligSource;

	/// <summary>Holds a FIX 5.0 SP2 NoSettlOblig, tag 1165, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoSettlOblig                                    { get; set; } = ValidateNoSettlOblig;

	/// <summary>Holds a FIX 5.0 SP2 QuoteMsgID, tag 1166, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      QuoteMsgID                                      { get; set; } = ValidateQuoteMsgID;

	/// <summary>Holds a FIX 5.0 SP2 QuoteEntryStatus, tag 1167, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   QuoteEntryStatus                                { get; set; } = ValidateQuoteEntryStatus;

	/// <summary>Holds a FIX 5.0 SP2 TotNoCxldQuotes, tag 1168, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoCxldQuotes                                 { get; set; } = ValidateTotNoCxldQuotes;

	/// <summary>Holds a FIX 5.0 SP2 TotNoAccQuotes, tag 1169, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoAccQuotes                                  { get; set; } = ValidateTotNoAccQuotes;

	/// <summary>Holds a FIX 5.0 SP2 TotNoRejQuotes, tag 1170, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoRejQuotes                                  { get; set; } = ValidateTotNoRejQuotes;

	/// <summary>Holds a FIX 5.0 SP2 PrivateQuote, tag 1171, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   PrivateQuote                                    { get; set; } = ValidatePrivateQuote;

	/// <summary>Holds a FIX 5.0 SP2 RespondentType, tag 1172, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RespondentType                                  { get; set; } = ValidateRespondentType;

	/// <summary>Holds a FIX 5.0 SP2 MDSubBookType, tag 1173, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDSubBookType                                   { get; set; } = ValidateMDSubBookType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTradingEvent, tag 1174, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityTradingEvent                            { get; set; } = ValidateSecurityTradingEvent;

	/// <summary>Holds a FIX 5.0 SP2 NoStatsIndicators, tag 1175, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoStatsIndicators                               { get; set; } = ValidateNoStatsIndicators;

	/// <summary>Holds a FIX 5.0 SP2 StatsType, tag 1176, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StatsType                                       { get; set; } = ValidateStatsType;

	/// <summary>Holds a FIX 5.0 SP2 NoOfSecSizes, tag 1177, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoOfSecSizes                                    { get; set; } = ValidateNoOfSecSizes;

	/// <summary>Holds a FIX 5.0 SP2 MDSecSizeType, tag 1178, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MDSecSizeType                                   { get; set; } = ValidateMDSecSizeType;

	/// <summary>Holds a FIX 5.0 SP2 MDSecSize, tag 1179, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MDSecSize                                       { get; set; } = ValidateMDSecSize;

	/// <summary>Holds a FIX 5.0 SP2 ApplID, tag 1180, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ApplID                                          { get; set; } = ValidateApplID;

	/// <summary>Holds a FIX 5.0 SP2 ApplSeqNum, tag 1181, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplSeqNum                                      { get; set; } = ValidateApplSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 ApplBegSeqNum, tag 1182, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplBegSeqNum                                   { get; set; } = ValidateApplBegSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 ApplEndSeqNum, tag 1183, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplEndSeqNum                                   { get; set; } = ValidateApplEndSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXMLLen, tag 1184, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityXMLLen                                  { get; set; } = ValidateSecurityXMLLen;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXML, tag 1185, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      SecurityXML                                     { get; set; } = ValidateSecurityXML;

	/// <summary>Holds a FIX 5.0 SP2 SecurityXMLSchema, tag 1186, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityXMLSchema                               { get; set; } = ValidateSecurityXMLSchema;

	/// <summary>Holds a FIX 5.0 SP2 RefreshIndicator, tag 1187, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   RefreshIndicator                                { get; set; } = ValidateRefreshIndicator;

	/// <summary>Holds a FIX 5.0 SP2 Volatility, tag 1188, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   Volatility                                      { get; set; } = ValidateVolatility;

	/// <summary>Holds a FIX 5.0 SP2 TimeToExpiration, tag 1189, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TimeToExpiration                                { get; set; } = ValidateTimeToExpiration;

	/// <summary>Holds a FIX 5.0 SP2 RiskFreeRate, tag 1190, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   RiskFreeRate                                    { get; set; } = ValidateRiskFreeRate;

	/// <summary>Holds a FIX 5.0 SP2 PriceUnitOfMeasure, tag 1191, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PriceUnitOfMeasure                              { get; set; } = ValidatePriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 PriceUnitOfMeasureQty, tag 1192, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   PriceUnitOfMeasureQty                           { get; set; } = ValidatePriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 SettlMethod, tag 1193, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> SettlMethod                                     { get; set; } = ValidateSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 ExerciseStyle, tag 1194, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ExerciseStyle                                   { get; set; } = ValidateExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 OptPayoutAmount, tag 1195, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OptPayoutAmount                                 { get; set; } = ValidateOptPayoutAmount;

	/// <summary>Holds a FIX 5.0 SP2 PriceQuoteMethod, tag 1196, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      PriceQuoteMethod                                { get; set; } = ValidatePriceQuoteMethod;

	/// <summary>Holds a FIX 5.0 SP2 ValuationMethod, tag 1197, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ValuationMethod                                 { get; set; } = ValidateValuationMethod;

	/// <summary>Holds a FIX 5.0 SP2 ListMethod, tag 1198, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ListMethod                                      { get; set; } = ValidateListMethod;

	/// <summary>Holds a FIX 5.0 SP2 CapPrice, tag 1199, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CapPrice                                        { get; set; } = ValidateCapPrice;

	/// <summary>Holds a FIX 5.0 SP2 FloorPrice, tag 1200, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FloorPrice                                      { get; set; } = ValidateFloorPrice;

	/// <summary>Holds a FIX 5.0 SP2 NoStrikeRules, tag 1201, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoStrikeRules                                   { get; set; } = ValidateNoStrikeRules;

	/// <summary>Holds a FIX 5.0 SP2 StartStrikePxRange, tag 1202, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StartStrikePxRange                              { get; set; } = ValidateStartStrikePxRange;

	/// <summary>Holds a FIX 5.0 SP2 EndStrikePxRange, tag 1203, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EndStrikePxRange                                { get; set; } = ValidateEndStrikePxRange;

	/// <summary>Holds a FIX 5.0 SP2 StrikeIncrement, tag 1204, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StrikeIncrement                                 { get; set; } = ValidateStrikeIncrement;

	/// <summary>Holds a FIX 5.0 SP2 NoTickRules, tag 1205, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTickRules                                     { get; set; } = ValidateNoTickRules;

	/// <summary>Holds a FIX 5.0 SP2 StartTickPriceRange, tag 1206, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StartTickPriceRange                             { get; set; } = ValidateStartTickPriceRange;

	/// <summary>Holds a FIX 5.0 SP2 EndTickPriceRange, tag 1207, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   EndTickPriceRange                               { get; set; } = ValidateEndTickPriceRange;

	/// <summary>Holds a FIX 5.0 SP2 TickIncrement, tag 1208, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   TickIncrement                                   { get; set; } = ValidateTickIncrement;

	/// <summary>Holds a FIX 5.0 SP2 TickRuleType, tag 1209, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TickRuleType                                    { get; set; } = ValidateTickRuleType;

	/// <summary>Holds a FIX 5.0 SP2 NestedInstrAttribType, tag 1210, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NestedInstrAttribType                           { get; set; } = ValidateNestedInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 NestedInstrAttribValue, tag 1211, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NestedInstrAttribValue                          { get; set; } = ValidateNestedInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 LegMaturityTime, tag 1212, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.ZonedTime, bool> LegMaturityTime                                 { get; set; } = ValidateLegMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingMaturityTime, tag 1213, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.ZonedTime, bool> UnderlyingMaturityTime                          { get; set; } = ValidateUnderlyingMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSymbol, tag 1214, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSymbol                                { get; set; } = ValidateDerivativeSymbol;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSymbolSfx, tag 1215, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSymbolSfx                             { get; set; } = ValidateDerivativeSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityID, tag 1216, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityID                            { get; set; } = ValidateDerivativeSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityIDSource, tag 1217, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityIDSource                      { get; set; } = ValidateDerivativeSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeSecurityAltID, tag 1218, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDerivativeSecurityAltID                       { get; set; } = ValidateNoDerivativeSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityAltID, tag 1219, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityAltID                         { get; set; } = ValidateDerivativeSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityAltIDSource, tag 1220, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityAltIDSource                   { get; set; } = ValidateDerivativeSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryLowLimitPrice, tag 1221, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SecondaryLowLimitPrice                          { get; set; } = ValidateSecondaryLowLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 MaturityRuleID, tag 1222, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MaturityRuleID                                  { get; set; } = ValidateMaturityRuleID;

	/// <summary>Holds a FIX 5.0 SP2 StrikeRuleID, tag 1223, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StrikeRuleID                                    { get; set; } = ValidateStrikeRuleID;

	/// <summary>Holds a FIX 5.0 SP2 LegUnitOfMeasureQty, tag 1224, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegUnitOfMeasureQty                             { get; set; } = ValidateLegUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeOptPayAmount, tag 1225, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeOptPayAmount                          { get; set; } = ValidateDerivativeOptPayAmount;

	/// <summary>Holds a FIX 5.0 SP2 EndMaturityMonthYear, tag 1226, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> EndMaturityMonthYear                            { get; set; } = ValidateEndMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 ProductComplex, tag 1227, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ProductComplex                                  { get; set; } = ValidateProductComplex;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeProductComplex, tag 1228, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeProductComplex                        { get; set; } = ValidateDerivativeProductComplex;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearIncrement, tag 1229, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MaturityMonthYearIncrement                      { get; set; } = ValidateMaturityMonthYearIncrement;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryHighLimitPrice, tag 1230, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SecondaryHighLimitPrice                         { get; set; } = ValidateSecondaryHighLimitPrice;

	/// <summary>Holds a FIX 5.0 SP2 MinLotSize, tag 1231, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   MinLotSize                                      { get; set; } = ValidateMinLotSize;

	/// <summary>Holds a FIX 5.0 SP2 NoExecInstRules, tag 1232, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoExecInstRules                                 { get; set; } = ValidateNoExecInstRules;

	/// <summary>Holds a FIX 5.0 SP2 NoLotTypeRules, tag 1234, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoLotTypeRules                                  { get; set; } = ValidateNoLotTypeRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMatchRules, tag 1235, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMatchRules                                    { get; set; } = ValidateNoMatchRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMaturityRules, tag 1236, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMaturityRules                                 { get; set; } = ValidateNoMaturityRules;

	/// <summary>Holds a FIX 5.0 SP2 NoOrdTypeRules, tag 1237, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoOrdTypeRules                                  { get; set; } = ValidateNoOrdTypeRules;

	/// <summary>Holds a FIX 5.0 SP2 NoTimeInForceRules, tag 1239, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTimeInForceRules                              { get; set; } = ValidateNoTimeInForceRules;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryTradingReferencePrice, tag 1240, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   SecondaryTradingReferencePrice                  { get; set; } = ValidateSecondaryTradingReferencePrice;

	/// <summary>Holds a FIX 5.0 SP2 StartMaturityMonthYear, tag 1241, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> StartMaturityMonthYear                          { get; set; } = ValidateStartMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 FlexProductEligibilityIndicator, tag 1242, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   FlexProductEligibilityIndicator                 { get; set; } = ValidateFlexProductEligibilityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 DerivFlexProductEligibilityIndicator, tag 1243, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   DerivFlexProductEligibilityIndicator            { get; set; } = ValidateDerivFlexProductEligibilityIndicator;

	/// <summary>Holds a FIX 5.0 SP2 FlexibleIndicator, tag 1244, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   FlexibleIndicator                               { get; set; } = ValidateFlexibleIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradingCurrency, tag 1245, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradingCurrency                                 { get; set; } = ValidateTradingCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeProduct, tag 1246, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeProduct                               { get; set; } = ValidateDerivativeProduct;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityGroup, tag 1247, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityGroup                         { get; set; } = ValidateDerivativeSecurityGroup;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCFICode, tag 1248, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeCFICode                               { get; set; } = ValidateDerivativeCFICode;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityType, tag 1249, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityType                          { get; set; } = ValidateDerivativeSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecuritySubType, tag 1250, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecuritySubType                       { get; set; } = ValidateDerivativeSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityMonthYear, tag 1251, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> DerivativeMaturityMonthYear                     { get; set; } = ValidateDerivativeMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityDate, tag 1252, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DerivativeMaturityDate                          { get; set; } = ValidateDerivativeMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMaturityTime, tag 1253, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.ZonedTime, bool> DerivativeMaturityTime                          { get; set; } = ValidateDerivativeMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSettleOnOpenFlag, tag 1254, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSettleOnOpenFlag                      { get; set; } = ValidateDerivativeSettleOnOpenFlag;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrmtAssignmentMethod, tag 1255, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DerivativeInstrmtAssignmentMethod               { get; set; } = ValidateDerivativeInstrmtAssignmentMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityStatus, tag 1256, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityStatus                        { get; set; } = ValidateDerivativeSecurityStatus;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrRegistry, tag 1257, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeInstrRegistry                         { get; set; } = ValidateDerivativeInstrRegistry;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCountryOfIssue, tag 1258, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeCountryOfIssue                        { get; set; } = ValidateDerivativeCountryOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStateOrProvinceOfIssue, tag 1259, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeStateOrProvinceOfIssue                { get; set; } = ValidateDerivativeStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeLocaleOfIssue, tag 1260, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeLocaleOfIssue                         { get; set; } = ValidateDerivativeLocaleOfIssue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikePrice, tag 1261, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeStrikePrice                           { get; set; } = ValidateDerivativeStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeCurrency, tag 1262, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeStrikeCurrency                        { get; set; } = ValidateDerivativeStrikeCurrency;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeMultiplier, tag 1263, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeStrikeMultiplier                      { get; set; } = ValidateDerivativeStrikeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeStrikeValue, tag 1264, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeStrikeValue                           { get; set; } = ValidateDerivativeStrikeValue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeOptAttribute, tag 1265, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DerivativeOptAttribute                          { get; set; } = ValidateDerivativeOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractMultiplier, tag 1266, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeContractMultiplier                    { get; set; } = ValidateDerivativeContractMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMinPriceIncrement, tag 1267, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeMinPriceIncrement                     { get; set; } = ValidateDerivativeMinPriceIncrement;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeMinPriceIncrementAmount, tag 1268, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeMinPriceIncrementAmount               { get; set; } = ValidateDerivativeMinPriceIncrementAmount;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeUnitOfMeasure, tag 1269, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeUnitOfMeasure                         { get; set; } = ValidateDerivativeUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeUnitOfMeasureQty, tag 1270, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeUnitOfMeasureQty                      { get; set; } = ValidateDerivativeUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeTimeUnit, tag 1271, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeTimeUnit                              { get; set; } = ValidateDerivativeTimeUnit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityExchange, tag 1272, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityExchange                      { get; set; } = ValidateDerivativeSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePositionLimit, tag 1273, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativePositionLimit                         { get; set; } = ValidateDerivativePositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeNTPositionLimit, tag 1274, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeNTPositionLimit                       { get; set; } = ValidateDerivativeNTPositionLimit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeIssuer, tag 1275, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeIssuer                                { get; set; } = ValidateDerivativeIssuer;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeIssueDate, tag 1276, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DerivativeIssueDate                             { get; set; } = ValidateDerivativeIssueDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedIssuerLen, tag 1277, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeEncodedIssuerLen                      { get; set; } = ValidateDerivativeEncodedIssuerLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedIssuer, tag 1278, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      DerivativeEncodedIssuer                         { get; set; } = ValidateDerivativeEncodedIssuer;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityDesc, tag 1279, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityDesc                          { get; set; } = ValidateDerivativeSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedSecurityDescLen, tag 1280, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeEncodedSecurityDescLen                { get; set; } = ValidateDerivativeEncodedSecurityDescLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEncodedSecurityDesc, tag 1281, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      DerivativeEncodedSecurityDesc                   { get; set; } = ValidateDerivativeEncodedSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXMLLen, tag 1282, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeSecurityXMLLen                        { get; set; } = ValidateDerivativeSecurityXMLLen;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXML, tag 1283, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      DerivativeSecurityXML                           { get; set; } = ValidateDerivativeSecurityXML;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityXMLSchema, tag 1284, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeSecurityXMLSchema                     { get; set; } = ValidateDerivativeSecurityXMLSchema;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractSettlMonth, tag 1285, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> DerivativeContractSettlMonth                    { get; set; } = ValidateDerivativeContractSettlMonth;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeEvents, tag 1286, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDerivativeEvents                              { get; set; } = ValidateNoDerivativeEvents;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventType, tag 1287, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeEventType                             { get; set; } = ValidateDerivativeEventType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventDate, tag 1288, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      DerivativeEventDate                             { get; set; } = ValidateDerivativeEventDate;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventTime, tag 1289, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> DerivativeEventTime                             { get; set; } = ValidateDerivativeEventTime;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventPx, tag 1290, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeEventPx                               { get; set; } = ValidateDerivativeEventPx;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeEventText, tag 1291, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeEventText                             { get; set; } = ValidateDerivativeEventText;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrumentParties, tag 1292, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDerivativeInstrumentParties                   { get; set; } = ValidateNoDerivativeInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyID, tag 1293, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeInstrumentPartyID                     { get; set; } = ValidateDerivativeInstrumentPartyID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyIDSource, tag 1294, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeInstrumentPartyIDSource               { get; set; } = ValidateDerivativeInstrumentPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartyRole, tag 1295, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeInstrumentPartyRole                   { get; set; } = ValidateDerivativeInstrumentPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrumentPartySubIDs, tag 1296, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDerivativeInstrumentPartySubIDs               { get; set; } = ValidateNoDerivativeInstrumentPartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartySubID, tag 1297, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeInstrumentPartySubID                  { get; set; } = ValidateDerivativeInstrumentPartySubID;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrumentPartySubIDType, tag 1298, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeInstrumentPartySubIDType              { get; set; } = ValidateDerivativeInstrumentPartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeExerciseStyle, tag 1299, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DerivativeExerciseStyle                         { get; set; } = ValidateDerivativeExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 MarketSegmentID, tag 1300, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MarketSegmentID                                 { get; set; } = ValidateMarketSegmentID;

	/// <summary>Holds a FIX 5.0 SP2 MarketID, tag 1301, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MarketID                                        { get; set; } = ValidateMarketID;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearIncrementUnits, tag 1302, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MaturityMonthYearIncrementUnits                 { get; set; } = ValidateMaturityMonthYearIncrementUnits;

	/// <summary>Holds a FIX 5.0 SP2 MaturityMonthYearFormat, tag 1303, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MaturityMonthYearFormat                         { get; set; } = ValidateMaturityMonthYearFormat;

	/// <summary>Holds a FIX 5.0 SP2 StrikeExerciseStyle, tag 1304, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StrikeExerciseStyle                             { get; set; } = ValidateStrikeExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 SecondaryPriceLimitType, tag 1305, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecondaryPriceLimitType                         { get; set; } = ValidateSecondaryPriceLimitType;

	/// <summary>Holds a FIX 5.0 SP2 PriceLimitType, tag 1306, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   PriceLimitType                                  { get; set; } = ValidatePriceLimitType;

	/// <summary>Holds a FIX 5.0 SP2 ExecInstValue, tag 1308, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ExecInstValue                                   { get; set; } = ValidateExecInstValue;

	/// <summary>Holds a FIX 5.0 SP2 NoTradingSessionRules, tag 1309, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTradingSessionRules                           { get; set; } = ValidateNoTradingSessionRules;

	/// <summary>Holds a FIX 5.0 SP2 NoMarketSegments, tag 1310, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoMarketSegments                                { get; set; } = ValidateNoMarketSegments;

	/// <summary>Holds a FIX 5.0 SP2 NoDerivativeInstrAttrib, tag 1311, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoDerivativeInstrAttrib                         { get; set; } = ValidateNoDerivativeInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 NoNestedInstrAttrib, tag 1312, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNestedInstrAttrib                             { get; set; } = ValidateNoNestedInstrAttrib;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrAttribType, tag 1313, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeInstrAttribType                       { get; set; } = ValidateDerivativeInstrAttribType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeInstrAttribValue, tag 1314, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeInstrAttribValue                      { get; set; } = ValidateDerivativeInstrAttribValue;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceUnitOfMeasure, tag 1315, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativePriceUnitOfMeasure                    { get; set; } = ValidateDerivativePriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceUnitOfMeasureQty, tag 1316, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativePriceUnitOfMeasureQty                 { get; set; } = ValidateDerivativePriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSettlMethod, tag 1317, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> DerivativeSettlMethod                           { get; set; } = ValidateDerivativeSettlMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePriceQuoteMethod, tag 1318, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativePriceQuoteMethod                      { get; set; } = ValidateDerivativePriceQuoteMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeValuationMethod, tag 1319, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DerivativeValuationMethod                       { get; set; } = ValidateDerivativeValuationMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeListMethod, tag 1320, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeListMethod                            { get; set; } = ValidateDerivativeListMethod;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeCapPrice, tag 1321, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeCapPrice                              { get; set; } = ValidateDerivativeCapPrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeFloorPrice, tag 1322, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DerivativeFloorPrice                            { get; set; } = ValidateDerivativeFloorPrice;

	/// <summary>Holds a FIX 5.0 SP2 DerivativePutOrCall, tag 1323, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativePutOrCall                             { get; set; } = ValidateDerivativePutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 ListUpdateAction, tag 1324, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> ListUpdateAction                                { get; set; } = ValidateListUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 ParentMktSegmID, tag 1325, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ParentMktSegmID                                 { get; set; } = ValidateParentMktSegmID;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionDesc, tag 1326, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TradingSessionDesc                              { get; set; } = ValidateTradingSessionDesc;

	/// <summary>Holds a FIX 5.0 SP2 TradSesUpdateAction, tag 1327, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TradSesUpdateAction                             { get; set; } = ValidateTradSesUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 RejectText, tag 1328, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RejectText                                      { get; set; } = ValidateRejectText;

	/// <summary>Holds a FIX 5.0 SP2 FeeMultiplier, tag 1329, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FeeMultiplier                                   { get; set; } = ValidateFeeMultiplier;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSymbol, tag 1330, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSymbol                             { get; set; } = ValidateUnderlyingLegSymbol;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSymbolSfx, tag 1331, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSymbolSfx                          { get; set; } = ValidateUnderlyingLegSymbolSfx;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityID, tag 1332, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityID                         { get; set; } = ValidateUnderlyingLegSecurityID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityIDSource, tag 1333, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityIDSource                   { get; set; } = ValidateUnderlyingLegSecurityIDSource;

	/// <summary>Holds a FIX 5.0 SP2 NoUnderlyingLegSecurityAltID, tag 1334, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoUnderlyingLegSecurityAltID                    { get; set; } = ValidateNoUnderlyingLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityAltID, tag 1335, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityAltID                      { get; set; } = ValidateUnderlyingLegSecurityAltID;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityAltIDSource, tag 1336, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityAltIDSource                { get; set; } = ValidateUnderlyingLegSecurityAltIDSource;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityType, tag 1337, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityType                       { get; set; } = ValidateUnderlyingLegSecurityType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecuritySubType, tag 1338, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecuritySubType                    { get; set; } = ValidateUnderlyingLegSecuritySubType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityMonthYear, tag 1339, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.MonthYear, bool> UnderlyingLegMaturityMonthYear                  { get; set; } = ValidateUnderlyingLegMaturityMonthYear;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegStrikePrice, tag 1340, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingLegStrikePrice                        { get; set; } = ValidateUnderlyingLegStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityExchange, tag 1341, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityExchange                   { get; set; } = ValidateUnderlyingLegSecurityExchange;

	/// <summary>Holds a FIX 5.0 SP2 NoOfLegUnderlyings, tag 1342, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoOfLegUnderlyings                              { get; set; } = ValidateNoOfLegUnderlyings;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegPutOrCall, tag 1343, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingLegPutOrCall                          { get; set; } = ValidateUnderlyingLegPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegCFICode, tag 1344, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegCFICode                            { get; set; } = ValidateUnderlyingLegCFICode;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityDate, tag 1345, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Date, bool>      UnderlyingLegMaturityDate                       { get; set; } = ValidateUnderlyingLegMaturityDate;

	/// <summary>Holds a FIX 5.0 SP2 ApplReqID, tag 1346, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ApplReqID                                       { get; set; } = ValidateApplReqID;

	/// <summary>Holds a FIX 5.0 SP2 ApplReqType, tag 1347, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplReqType                                     { get; set; } = ValidateApplReqType;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseType, tag 1348, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplResponseType                                { get; set; } = ValidateApplResponseType;

	/// <summary>Holds a FIX 5.0 SP2 ApplTotalMessageCount, tag 1349, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplTotalMessageCount                           { get; set; } = ValidateApplTotalMessageCount;

	/// <summary>Holds a FIX 5.0 SP2 ApplLastSeqNum, tag 1350, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplLastSeqNum                                  { get; set; } = ValidateApplLastSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 NoApplIDs, tag 1351, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoApplIDs                                       { get; set; } = ValidateNoApplIDs;

	/// <summary>Holds a FIX 5.0 SP2 ApplResendFlag, tag 1352, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   ApplResendFlag                                  { get; set; } = ValidateApplResendFlag;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseID, tag 1353, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ApplResponseID                                  { get; set; } = ValidateApplResponseID;

	/// <summary>Holds a FIX 5.0 SP2 ApplResponseError, tag 1354, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplResponseError                               { get; set; } = ValidateApplResponseError;

	/// <summary>Holds a FIX 5.0 SP2 RefApplID, tag 1355, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefApplID                                       { get; set; } = ValidateRefApplID;

	/// <summary>Holds a FIX 5.0 SP2 ApplReportID, tag 1356, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ApplReportID                                    { get; set; } = ValidateApplReportID;

	/// <summary>Holds a FIX 5.0 SP2 RefApplLastSeqNum, tag 1357, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RefApplLastSeqNum                               { get; set; } = ValidateRefApplLastSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 LegPutOrCall, tag 1358, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegPutOrCall                                    { get; set; } = ValidateLegPutOrCall;

	/// <summary>Holds a FIX 5.0 SP2 TotNoFills, tag 1361, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TotNoFills                                      { get; set; } = ValidateTotNoFills;

	/// <summary>Holds a FIX 5.0 SP2 NoFills, tag 1362, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoFills                                         { get; set; } = ValidateNoFills;

	/// <summary>Holds a FIX 5.0 SP2 FillExecID, tag 1363, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      FillExecID                                      { get; set; } = ValidateFillExecID;

	/// <summary>Holds a FIX 5.0 SP2 FillPx, tag 1364, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FillPx                                          { get; set; } = ValidateFillPx;

	/// <summary>Holds a FIX 5.0 SP2 FillQty, tag 1365, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   FillQty                                         { get; set; } = ValidateFillQty;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocID, tag 1366, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegAllocID                                      { get; set; } = ValidateLegAllocID;

	/// <summary>Holds a FIX 5.0 SP2 LegAllocSettlCurrency, tag 1367, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegAllocSettlCurrency                           { get; set; } = ValidateLegAllocSettlCurrency;

	/// <summary>Holds a FIX 5.0 SP2 TradSesEvent, tag 1368, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradSesEvent                                    { get; set; } = ValidateTradSesEvent;

	/// <summary>Holds a FIX 5.0 SP2 MassActionReportID, tag 1369, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MassActionReportID                              { get; set; } = ValidateMassActionReportID;

	/// <summary>Holds a FIX 5.0 SP2 NoNotAffectedOrders, tag 1370, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNotAffectedOrders                             { get; set; } = ValidateNoNotAffectedOrders;

	/// <summary>Holds a FIX 5.0 SP2 NotAffectedOrderID, tag 1371, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NotAffectedOrderID                              { get; set; } = ValidateNotAffectedOrderID;

	/// <summary>Holds a FIX 5.0 SP2 NotAffOrigClOrdID, tag 1372, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NotAffOrigClOrdID                               { get; set; } = ValidateNotAffOrigClOrdID;

	/// <summary>Holds a FIX 5.0 SP2 MassActionType, tag 1373, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassActionType                                  { get; set; } = ValidateMassActionType;

	/// <summary>Holds a FIX 5.0 SP2 MassActionScope, tag 1374, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassActionScope                                 { get; set; } = ValidateMassActionScope;

	/// <summary>Holds a FIX 5.0 SP2 MassActionResponse, tag 1375, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassActionResponse                              { get; set; } = ValidateMassActionResponse;

	/// <summary>Holds a FIX 5.0 SP2 MassActionRejectReason, tag 1376, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MassActionRejectReason                          { get; set; } = ValidateMassActionRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 MultilegModel, tag 1377, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MultilegModel                                   { get; set; } = ValidateMultilegModel;

	/// <summary>Holds a FIX 5.0 SP2 MultilegPriceMethod, tag 1378, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   MultilegPriceMethod                             { get; set; } = ValidateMultilegPriceMethod;

	/// <summary>Holds a FIX 5.0 SP2 LegVolatility, tag 1379, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegVolatility                                   { get; set; } = ValidateLegVolatility;

	/// <summary>Holds a FIX 5.0 SP2 DividendYield, tag 1380, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DividendYield                                   { get; set; } = ValidateDividendYield;

	/// <summary>Holds a FIX 5.0 SP2 LegDividendYield, tag 1381, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegDividendYield                                { get; set; } = ValidateLegDividendYield;

	/// <summary>Holds a FIX 5.0 SP2 CurrencyRatio, tag 1382, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   CurrencyRatio                                   { get; set; } = ValidateCurrencyRatio;

	/// <summary>Holds a FIX 5.0 SP2 LegCurrencyRatio, tag 1383, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegCurrencyRatio                                { get; set; } = ValidateLegCurrencyRatio;

	/// <summary>Holds a FIX 5.0 SP2 LegExecInst, tag 1384, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Multiple, bool>  LegExecInst                                     { get; set; } = ValidateLegExecInst;

	/// <summary>Holds a FIX 5.0 SP2 ContingencyType, tag 1385, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ContingencyType                                 { get; set; } = ValidateContingencyType;

	/// <summary>Holds a FIX 5.0 SP2 ListRejectReason, tag 1386, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ListRejectReason                                { get; set; } = ValidateListRejectReason;

	/// <summary>Holds a FIX 5.0 SP2 NoTrdRepIndicators, tag 1387, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTrdRepIndicators                              { get; set; } = ValidateNoTrdRepIndicators;

	/// <summary>Holds a FIX 5.0 SP2 TrdRepPartyRole, tag 1388, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TrdRepPartyRole                                 { get; set; } = ValidateTrdRepPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 TrdRepIndicator, tag 1389, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   TrdRepIndicator                                 { get; set; } = ValidateTrdRepIndicator;

	/// <summary>Holds a FIX 5.0 SP2 TradePublishIndicator, tag 1390, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TradePublishIndicator                           { get; set; } = ValidateTradePublishIndicator;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegOptAttribute, tag 1391, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> UnderlyingLegOptAttribute                       { get; set; } = ValidateUnderlyingLegOptAttribute;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegSecurityDesc, tag 1392, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingLegSecurityDesc                       { get; set; } = ValidateUnderlyingLegSecurityDesc;

	/// <summary>Holds a FIX 5.0 SP2 MarketReqID, tag 1393, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MarketReqID                                     { get; set; } = ValidateMarketReqID;

	/// <summary>Holds a FIX 5.0 SP2 MarketReportID, tag 1394, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MarketReportID                                  { get; set; } = ValidateMarketReportID;

	/// <summary>Holds a FIX 5.0 SP2 MarketUpdateAction, tag 1395, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> MarketUpdateAction                              { get; set; } = ValidateMarketUpdateAction;

	/// <summary>Holds a FIX 5.0 SP2 MarketSegmentDesc, tag 1396, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MarketSegmentDesc                               { get; set; } = ValidateMarketSegmentDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedMktSegmDescLen, tag 1397, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedMktSegmDescLen                           { get; set; } = ValidateEncodedMktSegmDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedMktSegmDesc, tag 1398, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedMktSegmDesc                              { get; set; } = ValidateEncodedMktSegmDesc;

	/// <summary>Holds a FIX 5.0 SP2 ApplNewSeqNum, tag 1399, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplNewSeqNum                                   { get; set; } = ValidateApplNewSeqNum;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPasswordMethod, tag 1400, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncryptedPasswordMethod                         { get; set; } = ValidateEncryptedPasswordMethod;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPasswordLen, tag 1401, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncryptedPasswordLen                            { get; set; } = ValidateEncryptedPasswordLen;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedPassword, tag 1402, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncryptedPassword                               { get; set; } = ValidateEncryptedPassword;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedNewPasswordLen, tag 1403, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncryptedNewPasswordLen                         { get; set; } = ValidateEncryptedNewPasswordLen;

	/// <summary>Holds a FIX 5.0 SP2 EncryptedNewPassword, tag 1404, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncryptedNewPassword                            { get; set; } = ValidateEncryptedNewPassword;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingLegMaturityTime, tag 1405, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.ZonedTime, bool> UnderlyingLegMaturityTime                       { get; set; } = ValidateUnderlyingLegMaturityTime;

	/// <summary>Holds a FIX 5.0 SP2 RefApplExtID, tag 1406, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RefApplExtID                                    { get; set; } = ValidateRefApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 DefaultApplExtID, tag 1407, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DefaultApplExtID                                { get; set; } = ValidateDefaultApplExtID;

	/// <summary>Holds a FIX 5.0 SP2 DefaultCstmApplVerID, tag 1408, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      DefaultCstmApplVerID                            { get; set; } = ValidateDefaultCstmApplVerID;

	/// <summary>Holds a FIX 5.0 SP2 SessionStatus, tag 1409, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SessionStatus                                   { get; set; } = ValidateSessionStatus;

	/// <summary>Holds a FIX 5.0 SP2 DefaultVerIndicator, tag 1410, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Boolean, bool>   DefaultVerIndicator                             { get; set; } = ValidateDefaultVerIndicator;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartySubIDType, tag 1411, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested4PartySubIDType                           { get; set; } = ValidateNested4PartySubIDType;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartySubID, tag 1412, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested4PartySubID                               { get; set; } = ValidateNested4PartySubID;

	/// <summary>Holds a FIX 5.0 SP2 NoNested4PartySubIDs, tag 1413, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested4PartySubIDs                            { get; set; } = ValidateNoNested4PartySubIDs;

	/// <summary>Holds a FIX 5.0 SP2 NoNested4PartyIDs, tag 1414, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNested4PartyIDs                               { get; set; } = ValidateNoNested4PartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyID, tag 1415, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Nested4PartyID                                  { get; set; } = ValidateNested4PartyID;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyIDSource, tag 1416, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> Nested4PartyIDSource                            { get; set; } = ValidateNested4PartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 Nested4PartyRole, tag 1417, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   Nested4PartyRole                                { get; set; } = ValidateNested4PartyRole;

	/// <summary>Holds a FIX 5.0 SP2 LegLastQty, tag 1418, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegLastQty                                      { get; set; } = ValidateLegLastQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingExerciseStyle, tag 1419, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingExerciseStyle                         { get; set; } = ValidateUnderlyingExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 LegExerciseStyle, tag 1420, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegExerciseStyle                                { get; set; } = ValidateLegExerciseStyle;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceUnitOfMeasure, tag 1421, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LegPriceUnitOfMeasure                           { get; set; } = ValidateLegPriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 LegPriceUnitOfMeasureQty, tag 1422, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   LegPriceUnitOfMeasureQty                        { get; set; } = ValidateLegPriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingUnitOfMeasureQty, tag 1423, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingUnitOfMeasureQty                      { get; set; } = ValidateUnderlyingUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceUnitOfMeasure, tag 1424, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingPriceUnitOfMeasure                    { get; set; } = ValidateUnderlyingPriceUnitOfMeasure;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceUnitOfMeasureQty, tag 1425, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingPriceUnitOfMeasureQty                 { get; set; } = ValidateUnderlyingPriceUnitOfMeasureQty;

	/// <summary>Holds a FIX 5.0 SP2 ApplReportType, tag 1426, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ApplReportType                                  { get; set; } = ValidateApplReportType;

	/// <summary>Holds a FIX 5.0 SP2 SideExecID, tag 1427, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SideExecID                                      { get; set; } = ValidateSideExecID;

	/// <summary>Holds a FIX 5.0 SP2 OrderDelay, tag 1428, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OrderDelay                                      { get; set; } = ValidateOrderDelay;

	/// <summary>Holds a FIX 5.0 SP2 OrderDelayUnit, tag 1429, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OrderDelayUnit                                  { get; set; } = ValidateOrderDelayUnit;

	/// <summary>Holds a FIX 5.0 SP2 VenueType, tag 1430, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> VenueType                                       { get; set; } = ValidateVenueType;

	/// <summary>Holds a FIX 5.0 SP2 RefOrdIDReason, tag 1431, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RefOrdIDReason                                  { get; set; } = ValidateRefOrdIDReason;

	/// <summary>Holds a FIX 5.0 SP2 OrigCustOrderCapacity, tag 1432, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OrigCustOrderCapacity                           { get; set; } = ValidateOrigCustOrderCapacity;

	/// <summary>Holds a FIX 5.0 SP2 RefApplReqID, tag 1433, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RefApplReqID                                    { get; set; } = ValidateRefApplReqID;

	/// <summary>Holds a FIX 5.0 SP2 ModelType, tag 1434, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ModelType                                       { get; set; } = ValidateModelType;

	/// <summary>Holds a FIX 5.0 SP2 ContractMultiplierUnit, tag 1435, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ContractMultiplierUnit                          { get; set; } = ValidateContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 LegContractMultiplierUnit, tag 1436, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegContractMultiplierUnit                       { get; set; } = ValidateLegContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingContractMultiplierUnit, tag 1437, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingContractMultiplierUnit                { get; set; } = ValidateUnderlyingContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeContractMultiplierUnit, tag 1438, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeContractMultiplierUnit                { get; set; } = ValidateDerivativeContractMultiplierUnit;

	/// <summary>Holds a FIX 5.0 SP2 FlowScheduleType, tag 1439, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   FlowScheduleType                                { get; set; } = ValidateFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 LegFlowScheduleType, tag 1440, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   LegFlowScheduleType                             { get; set; } = ValidateLegFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingFlowScheduleType, tag 1441, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingFlowScheduleType                      { get; set; } = ValidateUnderlyingFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeFlowScheduleType, tag 1442, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   DerivativeFlowScheduleType                      { get; set; } = ValidateDerivativeFlowScheduleType;

	/// <summary>Holds a FIX 5.0 SP2 FillLiquidityInd, tag 1443, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   FillLiquidityInd                                { get; set; } = ValidateFillLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 SideLiquidityInd, tag 1444, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SideLiquidityInd                                { get; set; } = ValidateSideLiquidityInd;

	/// <summary>Holds a FIX 5.0 SP2 NoRateSources, tag 1445, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoRateSources                                   { get; set; } = ValidateNoRateSources;

	/// <summary>Holds a FIX 5.0 SP2 RateSource, tag 1446, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RateSourceField                                 { get; set; } = ValidateRateSourceField;

	/// <summary>Holds a FIX 5.0 SP2 RateSourceType, tag 1447, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   RateSourceType                                  { get; set; } = ValidateRateSourceType;

	/// <summary>Holds a FIX 5.0 SP2 ReferencePage, tag 1448, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      ReferencePage                                   { get; set; } = ValidateReferencePage;

	/// <summary>Holds a FIX 5.0 SP2 RestructuringType, tag 1449, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      RestructuringType                               { get; set; } = ValidateRestructuringType;

	/// <summary>Holds a FIX 5.0 SP2 Seniority, tag 1450, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      Seniority                                       { get; set; } = ValidateSeniority;

	/// <summary>Holds a FIX 5.0 SP2 NotionalPercentageOutstanding, tag 1451, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   NotionalPercentageOutstanding                   { get; set; } = ValidateNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 OriginalNotionalPercentageOutstanding, tag 1452, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   OriginalNotionalPercentageOutstanding           { get; set; } = ValidateOriginalNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingRestructuringType, tag 1453, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingRestructuringType                     { get; set; } = ValidateUnderlyingRestructuringType;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingSeniority, tag 1454, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      UnderlyingSeniority                             { get; set; } = ValidateUnderlyingSeniority;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingNotionalPercentageOutstanding, tag 1455, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingNotionalPercentageOutstanding         { get; set; } = ValidateUnderlyingNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingOriginalNotionalPercentageOutstanding, tag 1456, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingOriginalNotionalPercentageOutstanding { get; set; } = ValidateUnderlyingOriginalNotionalPercentageOutstanding;

	/// <summary>Holds a FIX 5.0 SP2 AttachmentPoint, tag 1457, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   AttachmentPoint                                 { get; set; } = ValidateAttachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 DetachmentPoint, tag 1458, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   DetachmentPoint                                 { get; set; } = ValidateDetachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAttachmentPoint, tag 1459, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingAttachmentPoint                       { get; set; } = ValidateUnderlyingAttachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingDetachmentPoint, tag 1460, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   UnderlyingDetachmentPoint                       { get; set; } = ValidateUnderlyingDetachmentPoint;

	/// <summary>Holds a FIX 5.0 SP2 NoTargetPartyIDs, tag 1461, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoTargetPartyIDs                                { get; set; } = ValidateNoTargetPartyIDs;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyID, tag 1462, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      TargetPartyID                                   { get; set; } = ValidateTargetPartyID;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyIDSource, tag 1463, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Character, bool> TargetPartyIDSource                             { get; set; } = ValidateTargetPartyIDSource;

	/// <summary>Holds a FIX 5.0 SP2 TargetPartyRole, tag 1464, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   TargetPartyRole                                 { get; set; } = ValidateTargetPartyRole;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListID, tag 1465, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityListID                                  { get; set; } = ValidateSecurityListID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListRefID, tag 1466, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityListRefID                               { get; set; } = ValidateSecurityListRefID;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListDesc, tag 1467, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      SecurityListDesc                                { get; set; } = ValidateSecurityListDesc;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityListDescLen, tag 1468, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   EncodedSecurityListDescLen                      { get; set; } = ValidateEncodedSecurityListDescLen;

	/// <summary>Holds a FIX 5.0 SP2 EncodedSecurityListDesc, tag 1469, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Data, bool>      EncodedSecurityListDesc                         { get; set; } = ValidateEncodedSecurityListDesc;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListType, tag 1470, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityListType                                { get; set; } = ValidateSecurityListType;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListTypeSource, tag 1471, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   SecurityListTypeSource                          { get; set; } = ValidateSecurityListTypeSource;

	/// <summary>Holds a FIX 5.0 SP2 NewsID, tag 1472, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NewsID                                          { get; set; } = ValidateNewsID;

	/// <summary>Holds a FIX 5.0 SP2 NewsCategory, tag 1473, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NewsCategory                                    { get; set; } = ValidateNewsCategory;

	/// <summary>Holds a FIX 5.0 SP2 LanguageCode, tag 1474, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      LanguageCode                                    { get; set; } = ValidateLanguageCode;

	/// <summary>Holds a FIX 5.0 SP2 NoNewsRefIDs, tag 1475, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoNewsRefIDs                                    { get; set; } = ValidateNoNewsRefIDs;

	/// <summary>Holds a FIX 5.0 SP2 NewsRefID, tag 1476, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      NewsRefID                                       { get; set; } = ValidateNewsRefID;

	/// <summary>Holds a FIX 5.0 SP2 NewsRefType, tag 1477, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NewsRefType                                     { get; set; } = ValidateNewsRefType;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceDeterminationMethod, tag 1478, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StrikePriceDeterminationMethod                  { get; set; } = ValidateStrikePriceDeterminationMethod;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceBoundaryMethod, tag 1479, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StrikePriceBoundaryMethod                       { get; set; } = ValidateStrikePriceBoundaryMethod;

	/// <summary>Holds a FIX 5.0 SP2 StrikePriceBoundaryPrecision, tag 1480, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   StrikePriceBoundaryPrecision                    { get; set; } = ValidateStrikePriceBoundaryPrecision;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingPriceDeterminationMethod, tag 1481, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   UnderlyingPriceDeterminationMethod              { get; set; } = ValidateUnderlyingPriceDeterminationMethod;

	/// <summary>Holds a FIX 5.0 SP2 OptPayoutType, tag 1482, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   OptPayoutType                                   { get; set; } = ValidateOptPayoutType;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEvents, tag 1483, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoComplexEvents                                 { get; set; } = ValidateNoComplexEvents;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventType, tag 1484, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ComplexEventType                                { get; set; } = ValidateComplexEventType;

	/// <summary>Holds a FIX 5.0 SP2 ComplexOptPayoutAmount, tag 1485, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ComplexOptPayoutAmount                          { get; set; } = ValidateComplexOptPayoutAmount;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPrice, tag 1486, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ComplexEventPrice                               { get; set; } = ValidateComplexEventPrice;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceBoundaryMethod, tag 1487, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ComplexEventPriceBoundaryMethod                 { get; set; } = ValidateComplexEventPriceBoundaryMethod;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceBoundaryPrecision, tag 1488, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Decimal, bool>   ComplexEventPriceBoundaryPrecision              { get; set; } = ValidateComplexEventPriceBoundaryPrecision;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventPriceTimeType, tag 1489, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ComplexEventPriceTimeType                       { get; set; } = ValidateComplexEventPriceTimeType;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventCondition, tag 1490, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   ComplexEventCondition                           { get; set; } = ValidateComplexEventCondition;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEventDates, tag 1491, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoComplexEventDates                             { get; set; } = ValidateNoComplexEventDates;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventStartDate, tag 1492, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ComplexEventStartDate                           { get; set; } = ValidateComplexEventStartDate;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventEndDate, tag 1493, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> ComplexEventEndDate                             { get; set; } = ValidateComplexEventEndDate;

	/// <summary>Holds a FIX 5.0 SP2 NoComplexEventTimes, tag 1494, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoComplexEventTimes                             { get; set; } = ValidateNoComplexEventTimes;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventStartTime, tag 1495, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Time, bool>      ComplexEventStartTime                           { get; set; } = ValidateComplexEventStartTime;

	/// <summary>Holds a FIX 5.0 SP2 ComplexEventEndTime, tag 1496, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Time, bool>      ComplexEventEndTime                             { get; set; } = ValidateComplexEventEndTime;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnReqID, tag 1497, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StreamAsgnReqID                                 { get; set; } = ValidateStreamAsgnReqID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnReqType, tag 1498, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StreamAsgnReqType                               { get; set; } = ValidateStreamAsgnReqType;

	/// <summary>Holds a FIX 5.0 SP2 NoAsgnReqs, tag 1499, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   NoAsgnReqs                                      { get; set; } = ValidateNoAsgnReqs;

	/// <summary>Holds a FIX 5.0 SP2 MDStreamID, tag 1500, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      MDStreamID                                      { get; set; } = ValidateMDStreamID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnRptID, tag 1501, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Text, bool>      StreamAsgnRptID                                 { get; set; } = ValidateStreamAsgnRptID;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnRejReason, tag 1502, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StreamAsgnRejReason                             { get; set; } = ValidateStreamAsgnRejReason;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnAckType, tag 1503, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StreamAsgnAckType                               { get; set; } = ValidateStreamAsgnAckType;

	/// <summary>Holds a FIX 5.0 SP2 RelSymTransactTime, tag 1504, to its type.</summary>
	public Func<Fix50Context, FixMessage, FixField.Timestamp, bool> RelSymTransactTime                              { get; set; } = ValidateRelSymTransactTime;

	/// <summary>Holds a FIX 5.0 SP2 StreamAsgnType, tag 1617, to the values the specification lists.</summary>
	public Func<Fix50Context, FixMessage, FixField.Integer, bool>   StreamAsgnType                                  { get; set; } = ValidateStreamAsgnType;

	static bool ValidateAccount(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvId(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvSide(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvTransType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("C" or "N" or "R"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginSeqNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginString(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBodyLength(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCheckSum(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommission(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCumQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndSeqNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecInst(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
				"C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or "P" or
				"Q" or "R" or "S" or "T" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or
				"e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or "p" or "q" or "r" or
				"s" or "t"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateExecRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHandlInst(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
			"D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQltyInd(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQty(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("L" or "M" or "S" or "U"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOITransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastCapacity(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMkt(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLinesOfText(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "AA" or
			"AB" or "AC" or "AD" or "AE" or "AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or "AL" or "AM" or "AN" or
			"AO" or "AP" or "AQ" or "AR" or "AS" or "AT" or "AU" or "AV" or "AW" or "AX" or "AY" or "AZ" or "B" or
			"BA" or "BB" or "BC" or "BD" or "BE" or "BF" or "BG" or "BH" or "BI" or "BJ" or "BK" or "BL" or "BM" or
			"BN" or "BO" or "BP" or "BQ" or "BR" or "BS" or "BT" or "BU" or "BV" or "BW" or "BX" or "BY" or "BZ" or
			"C" or "CA" or "CB" or "CC" or "CD" or "CE" or "D" or "E" or "F" or "G" or "H" or "J" or "K" or "L" or
			"M" or "N" or "P" or "Q" or "R" or "S" or "T" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or
			"d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or "p" or "q" or "r" or
			"s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewSeqNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatus(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C' or 'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I' or 'J' or 'K' or 'L' or 'M' or 'P' or 'Q'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigClOrdID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossDupFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSendingTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuantity(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSide(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeInForce(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransactTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUrgency(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValidUntilTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "B" or "C"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbolSfx(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("CD" or "WI"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListSeqNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInst(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocTransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxPrecision(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionEffect(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'D' or 'F' or 'N' or 'O' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAllocs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccount(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProcessCode(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRpts(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRptSeq(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDlvyInst(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocRejCode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignature(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureDataLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureData(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignatureLength(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawDataLength(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawData(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossResend(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStopPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDestination(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 18 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdRejReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 18 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQualifier(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or 'S' or
			'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssuer(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeartBtInt(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxFloor(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportToExch(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocateReqd(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetMoney(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateForexReq(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigSendingTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGapFillFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExecs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDKReason(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOINaturalFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMiscFees(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeCurr(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "10" or "11" or "12" or "13" or "14" or "2" or "3" or "4" or "5" or "6" or
			"7" or "8" or "9"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrevClosePx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResetSeqNumFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderLocationID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetLocationID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfLocationID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToLocationID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRelatedSym(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubject(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeadline(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateURLLink(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'E' or 'F' or 'G' or 'H' or 'I' or 'J' or 'K' or 'L'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLeavesQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOrderQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAvgPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNetMoney(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRateCalc(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumDaysInterest(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMode(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstTransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailThreadID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("?" or "ABS" or "AMENDED" or "AN" or "BA" or "BDN" or "BN" or "BOX" or "BRADY" or
			"BRIDGE" or "BUYSELL" or "CAMM" or "CAN" or "CASH" or "CB" or "CD" or "CDS" or "CL" or "CMB" or
			"CMBS" or "CMO" or "COFO" or "COFP" or "CORP" or "CP" or "CPP" or "CS" or "CTB" or "DEFLTED" or
			"DINP" or "DN" or "DUAL" or "EUCD" or "EUCORP" or "EUCP" or "EUFRN" or "EUSOV" or "EUSUPRA" or "FAC" or
			"FADN" or "FOR" or "FORWARD" or "FRN" or "FUT" or "FXFWD" or "FXNDF" or "FXSPOT" or "FXSWAP" or "GO" or
			"IET" or "IRS" or "LOFC" or "LQN" or "MATURED" or "MBS" or "MF" or "MIO" or "MLEG" or "MPO" or "MPP" or
			"MPT" or "MT" or "MTN" or "NONE" or "ONITE" or "OOC" or "OOF" or "OOP" or "OPT" or "PEF" or "PFAND" or
			"PN" or "PROV" or "PS" or "PZFJ" or "RAN" or "REPLACD" or "REPO" or "RETIRED" or "REV" or "RVLV" or
			"RVLVTRM" or "SECLOAN" or "SECPLEDGE" or "SLQN" or "SPCLA" or "SPCLO" or "SPCLT" or "STN" or "STRUCT" or
			"SUPRA" or "SWING" or "TAN" or "TAXA" or "TB" or "TBA" or "TBILL" or "TBOND" or "TCAL" or "TD" or
			"TECP" or "TERM" or "TINT" or "TIPS" or "TLQN" or "TMCP" or "TNOTE" or "TPRN" or "TRAN" or "UST" or
			"USTB" or "VRDN" or "WAR" or "WITHDRN" or "XCN" or "XLINKD" or "YANK" or "YCD"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEffectiveTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDeliveryType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSpotRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSpotRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate2(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastSpotRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoIOIQualifiers(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePutOrCall(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCoveredOrUncovered(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptAttribute(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityExchange(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotifyBrokerOfCredit(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocHandlInst(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxShow(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlDataLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlData(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRoutingIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSpread(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("EONIA" or "EUREPO" or "Euribor" or "FutureSWAP" or "LIBID" or "LIBOR" or
			"MuniAAA" or "OTHER" or "Pfandbriefe" or "SONIA" or "SWAP" or "Treasury"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurvePoint(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponPaymentDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssueDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseTerm(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFactor(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeOriginationDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStipulations(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStipulationType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ABS" or "AMT" or "AUTOREINV" or "AVAILQTY" or "AVFICO" or "AVSIZE" or "BANKQUAL" or
			"BGNCON" or "BROKERCREDIT" or "COUPON" or "CPP" or "CPR" or "CPY" or "CURRENCY" or "CUSTOMDATE" or
			"DISCOUNT" or "GEOG" or "HAIRCUT" or "HEP" or "INSURED" or "INTERNALPX" or "INTERNALQTY" or "ISSUE" or
			"ISSUER" or "ISSUESIZE" or "LEAVEQTY" or "LOOKBACK" or "LOT" or "LOTVAR" or "MAT" or "MATURITY" or
			"MAXBAL" or "MAXORDQTY" or "MAXSUBS" or "MHP" or "MINDNOM" or "MININCR" or "MINQTY" or "MPR" or
			"ORDRINCR" or "PAYFREQ" or "PIECES" or "PMAX" or "POOL" or "PPC" or "PPL" or "PPM" or "PPT" or
			"PRICE" or "PRICEFREQ" or "PRIMARY" or "PROD" or "PROTECT" or "PSA" or "PURPOSE" or "PXSOURCE" or
			"RATING" or "REDEMPTION" or "REFINT" or "REFPRIN" or "REFTRADE" or "RESTRICTED" or "ROLLTYPE" or
			"SALESCREDITOVR" or "SECTOR" or "SECTYPE" or "SMM" or "STRUCT" or "SUBSFREQ" or "SUBSLEFT" or "TEXT" or
			"TRADERCREDIT" or "TRDVAR" or "WAC" or "WAL" or "WALA" or "WAM" or "WHOLE" or "YIELD" or "YTM"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStipulationValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("AFTERTAX" or "ANNUAL" or "ATISSUE" or "AVGMATURITY" or "BOOK" or "CALL" or
			"CHANGE" or "CLOSE" or "COMPOUND" or "CURRENT" or "GOVTEQUIV" or "GROSS" or "INFLATION" or
			"INVERSEFLOATER" or "LASTCLOSE" or "LASTMONTH" or "LASTQUARTER" or "LASTYEAR" or "LONGAVGLIFE" or
			"MARK" or "MATURITY" or "NEXTREFUND" or "OPENAVG" or "PREVCLOSE" or "PROCEEDS" or "PUT" or
			"SEMIANNUAL" or "SHORTAVGLIFE" or "SIMPLE" or "TAXEQUIV" or "TENDER" or "TRUE" or "VALUE1_32" or "WORST"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalTakedown(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConcession(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepoCollateralSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRedemptionDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponPaymentDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssueDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepoCollateralSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseTerm(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFactor(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRedemptionDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponPaymentDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssueDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepoCollateralSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseTerm(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegFactor(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRedemptionDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCreditRating(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCreditRating(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCreditRating(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradedFlatSwitch(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeatureDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeaturePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubscriptionRequestType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketDepth(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAggregatedBook(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntryTypes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntries(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'J' or 'K' or 'L' or 'M' or 'N' or 'O' or 'P' or 'Q' or 'R' or
			'S' or 'T' or 'U' or 'V' or 'W' or 'X' or 'Y' or 'Z' or 'a'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryTime(Fix50Context context, FixMessage message, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickDirection(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDMkt(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCondition(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "A" or "B" or "C" or "D" or
				"E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or "M" or "N" or "O" or "P" or "Q" or "R" or
				"S" or "T" or "U" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or "e" or "f" or
				"g" or "h" or "i" or "j" or "k" or "l" or "m" or "n" or "o" or "p" or "q" or "r" or "s" or "t" or
				"u" or "v" or "w" or "x" or "y" or "z"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateTradeCondition(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "A" or "AA" or "AB" or "AC" or "AD" or "AE" or
				"AF" or "AG" or "AH" or "AI" or "AJ" or "AK" or "AL" or "AM" or "AN" or "AO" or "AP" or "AQ" or
				"AR" or "AS" or "AT" or "AV" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or
				"K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or "T" or "U" or "V" or "W" or "X" or "Y" or
				"Z" or "a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or "j" or "k" or "l" or "m" or
				"n" or "o" or "p" or "q" or "r" or "s" or "t" or "u" or "v" or "w" or "x" or "y" or "z"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDEntryID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqRejReason(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C' or 'D'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryOriginator(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocationID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeleteReason(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenCloseSettlFlag(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateSellerDays(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryBuyer(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySeller(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPositionNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFinancialStatus(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateCorporateAction(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or
				"M" or "N" or "O" or "P" or "Q" or "R" or "S" or "T" or "U" or "V" or "W"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateDefBidSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefOfferSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteEntries(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteSets(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCancelType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteResponseLevel(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoQuoteEntries(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssuer(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityExchange(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbolSfx(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPutOrCall(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingOptAttribute(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnsolicitedIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 21 or
			22 or 23 or 24 or 25 or 26 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHaltReasonInt(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInViewOfCommon(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDueToRelated(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBuyVolume(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSellVolume(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHighPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLowPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustment(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTrader(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStartTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesOpenTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesPreCloseTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesCloseTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesEndTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumberOfOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEncoding(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuerLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuer(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInstLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInst(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedTextLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedText(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubjectLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubject(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadlineLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadline(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocTextLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocText(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuerLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuer(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetValidUntilTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMsgSeqNumProcessed(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefTagID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefMsgType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or 3 or 4 or
			5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidRequestTransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraBroker(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplianceID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSolicitedFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecRestatementReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 18 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGrossTradeAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContraBrokers(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxMessageSize(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMsgTypes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgDirection(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTradingSessions(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalVolumeTraded(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionInst(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClientBidID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoRelatedSym(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumTickets(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue1(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidDescriptors(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptorType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptor(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValueInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctLow(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctHigh(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEFPTrackingError(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFairValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutsideIndexPct(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValueOfFutures(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityIndType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWtAverageLiquidity(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeForPhysical(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutMainCntryUIndex(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPercent(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgRptReqs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgPeriodInterval(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIncTaxInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumBidders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidTradeType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisPxType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidComponents(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountry(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoStrikes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayOrderQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayCumQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayAvgPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGTBookingInst(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrikes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetGrossInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListOrderStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInstType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejResponseTo(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingContractMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityNumSecurities(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegReportingType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusTextLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusText(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetChgPrevDay(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or
			38 or 39 or 4 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 5 or 50 or 51 or 52 or
			53 or 54 or 55 or 56 or 57 or 58 or 59 or 6 or 60 or 61 or 62 or 63 or 64 or 65 or 66 or 67 or 68 or
			69 or 7 or 70 or 71 or 72 or 73 or 74 or 75 or 76 or 77 or 78 or 79 or 8 or 80 or 81 or 82 or 83 or
			84 or 85 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityAltID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingSecurityAltID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProduct(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCFICode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingProduct(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCFICode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestMessageIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingDirection(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingModulus(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountryOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStateOrProvinceOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocaleOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRegistDtls(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingDtls(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInvestorCountryOfResidence(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRef(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPaymentMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribCurr(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCancellationRights(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('M' or 'N' or 'O' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMoneyLaunderingStatus(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or 'N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingInst(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransBkdTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceAdjustment(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDateOfBirth(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportTransType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardHolderName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardNumber(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardExpDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardIssNum(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistAcctType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDesignation(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTaxAdvantageType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 999))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFundRenewWaiv(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentCode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctNumber(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribPayRef(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardStartDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRemitterID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistStatus(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'H' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonCode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistDtls(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDistribInsts(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistEmail(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPercentage(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistTransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecValuationPoint(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderPercent(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnershipType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('2' or 'J' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContAmts(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtCurr(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnerType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryClOrdID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryExecID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacity(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'I' or 'P' or 'R' or 'W'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderRestrictions(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
				"D" or "E" or "F"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMassCancelRequestType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelResponse(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAffectedOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAffectedOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedSecondaryOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAccruedInterestAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrRegistry(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashMargin(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateScope(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDImplicitDelete(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPrioritization(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigCrossID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSides(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUsername(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePassword(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoSecurityTypes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityTypes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestResult(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundLot(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinTradeVol(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegRptTypeReq(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPositionEffect(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCoveredOrUncovered(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatusRejReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreviouslyReported(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchStatus(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "A1" or "A2" or "A3" or
			"A4" or "A5" or "AQ" or "M1" or "M2" or "M3" or "M4" or "M5" or "M6" or "MT" or "S1" or "S2" or "S3" or
			"S4" or "S5"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOddLot(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoClearingInstructions(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingInstruction(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputDevice(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDates(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccountType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustOrderCapacity(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdLinkID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigOrdModTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayBookingInst(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingUnit(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreallocMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCountryOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStateOrProvinceOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLocaleOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrRegistry(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCountryOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStateOrProvinceOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLocaleOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInstrRegistry(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbolSfx(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegSecurityAltID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegProduct(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCFICode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOptAttribute(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityExchange(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssuer(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuerLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuer(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRatioQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSide(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoHops(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopSendingTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopRefID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingFeeIndicator(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "9" or "B" or "C" or "E" or "F" or "H" or "I" or
			"L" or "M"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWorkingIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLastPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorityIndicator(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceImprovement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints2(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRFQReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktBidPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktOfferPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinBidSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinOfferSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegalConfirm(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraLegRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrBidFxRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrOfferFxRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideComplianceID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAcctIDSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAcctIDSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmTransType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractSettlMonth(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryForm(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastParPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegAllocs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAccount(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIndividualAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAcctIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurvePoint(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBidPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIOIQty(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegStipulations(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOfferPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOrderQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSwapType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePool(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuotePriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteQualifier(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReversalIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldCalcDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPositions(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ALC" or "AS" or "ASF" or "CAA" or "CEA" or "DLT" or "DLV" or "DN" or "EP" or
			"ETR" or "EX" or "FIN" or "IAS" or "IES" or "PA" or "PIT" or "PNTN" or "RCV" or "SEA" or "SOD" or
			"SPL" or "TA" or "TOT" or "TQ" or "TRF" or "TX" or "XM"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLongQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosQtyStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmtType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("ACPN" or "BANK" or "CASH" or "CMTM" or "COLAT" or "CPN" or "CRES" or "DLV" or
			"FMTM" or "IACPN" or "ICMTM" or "ICPN" or "IMTM" or "PREM" or "SETL" or "SMTM" or "TVAR" or "VADJ"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosTransType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyings(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintAction(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigPosReqRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingBusinessDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("EOD" or "ETH" or "ITD" or "RTH"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustmentType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraryInstructionIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSpreadIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintResult(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseTransportType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseDestination(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNumPosReports(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqResult(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSettlPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteQualifiers(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAtMaturity(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegDatedDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPool(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocInterestAtMaturity(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccruedInterestAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentUnit(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenInterest(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExerciseMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumTradeReports(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestResult(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideMultiLegReportingType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPosAmt(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAutoAcceptIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySubType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecuritySubType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecuritySubType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessPct(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessCurr(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrdRegTimestamps(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestamp(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampOrigin(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRejReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocRejCode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMsgID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlInst(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastUpdateTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlInstType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDlvyInstType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTerminationType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNextExpectedMsgSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatusReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqRejCode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (10 or 11 or 12 or 14 or 2 or 3 or 4 or 5 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocCancReplaceReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCopyMsgIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccountType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderAvgPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderBookingQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocIntermedReqType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUsernames(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceDelta(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueMax(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueDepth(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueResolution(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueAction(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAltMDSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAltMDSourceID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxIndicator(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLinkID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderInputDevice(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLegRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeRule(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeAllocIndicator(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpirationCycle(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or
			37 or 38 or 39 or 4 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 5 or 50 or 51 or
			52 or 53 or 54 or 55 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdSubType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 21 or
			22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or
			38 or 39 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransferReason(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumAssignmentReports(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAsgnRptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateThresholdAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegMoveType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegLimitType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegRoundDirection(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePeggedPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegScope(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionMoveType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionLimitType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionRoundDirection(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionScope(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategy(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyParameters(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateParticipationRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyPerformance(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastLiquidityInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePublishTrdIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortSaleReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQtyType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTrdType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNoOrdersType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSharedCommission(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgParPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportedPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCapacities(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacityQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoEvents(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 3 or 4 or
			5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePctAtRisk(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoInstrAttrib(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDatedDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAccrualDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPProgram(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPRegType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPProgram(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPRegType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdMatchID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingDirtyPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStartValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrentValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingStips(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityNetMoney(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeBasis(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoAllocs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastFragment(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryQualifier(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrades(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginRatio(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginExcess(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNetValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOutstanding(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnTransType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRespID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRespType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumReports(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastRptRequested(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndAccruedInterestAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartCash(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndCash(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewPassword(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatusText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusValue(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefCompID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkResponseID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastNetworkResponseID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCompIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponseType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCollInquiryQualifier(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRptStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffirmStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikeCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikeCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeBracket(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAction(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryResult(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractSettlMonth(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInterestAccrualDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrategyParameters(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrategyParameterName(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrategyParameterType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or
			21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrategyParameterValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHostCrossID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTimeInForce(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReportID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityReportID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettleOnOpenFlag(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinPriceIncrement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionLimit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNTPositionLimit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingAllocationPercent(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCashAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCashType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("DIFF" or "FIXED"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlementType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (2 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuantityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContIntRptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLateIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInputSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityUpdateAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExpiration(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpirationQtyType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingAmounts(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPayAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCollectAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlementDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlementStatus(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryIndividualAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRndPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocCustomerCapacity(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTierCode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("Alw" or "Bbl" or "Bcf" or "Bu" or "Gal" or "MMBtu" or "MMbbl" or "MWh" or "USD" or
			"lbs" or "oz_tr" or "t" or "tn"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeUnit(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("D" or "H" or "Min" or "Mo" or "S" or "Wk" or "Yr"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTimeUnit(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegTimeUnit(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTradeReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideFillStationCd(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideReasonCd(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTrdSubTyp(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideLastQty(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEventSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTrdRegTimestamp(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTrdRegTimestampType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideTrdRegTimestampSrc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAsOfIndicator(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSideTrdRegTS(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOptionRatio(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoInstrumentParties(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrumentPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeVolume(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDBookType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDFeedType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDPriceLevel(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDOriginType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFirstPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySpotRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateManualOrderIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustDirectedOrder(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReceivedDeptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustOrderHandlingInst(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("ADD" or "AON" or "CNH" or "DIR" or "E.W" or "FOK" or "IO" or "IOC" or "LOC" or
				"LOO" or "MAC" or "MAO" or "MOC" or "MOO" or "MQT" or "NH" or "OVD" or "PEG" or "RSV" or "S.W" or
				"SCL" or "TMO" or "TS" or "WRK"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateOrderHandlingInstSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("A" or "AR" or "D" or "IN" or "IS" or "O" or "PF" or "PR" or "PT" or "S" or "T"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskTypeSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskOrderHandlingInst(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("ADD" or "AON" or "CNH" or "DIR" or "E.W" or "FOK" or "IO" or "IOC" or "LOC" or
				"LOO" or "MAC" or "MAO" or "MOC" or "MOO" or "MQT" or "NH" or "OVD" or "PEG" or "RSV" or "S.W" or
				"SCL" or "TMO" or "TS" or "WRK"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateExecAckStatus(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingDeliveryAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCapValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlMethod(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFirmTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryFirmTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollApplType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingAdjustedQuantity(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFXRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFXRateCalc(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocPositionEffect(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDealingCapacity(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrmtAssignmentMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrumentPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrumentPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoInstrumentPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrumentPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrumentPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCalculatedCcyLastQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAggressorIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUndlyInstrumentParties(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrumentPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrumentPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrumentPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUndlyInstrumentPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrumentPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrumentPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSwapPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSwapPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBidForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOfferForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSwapPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDQuoteType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastSwapPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideGrossTradeAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLastForwardPoints(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCalculatedCcyLastQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegGrossTradeAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityTime(Fix50Context context, FixMessage message, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefOrderIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryDisplayQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayWhen(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayLowQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayHighQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayMinIncr(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefreshQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchIncrement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxPriceLevels(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreTradeAnonymity(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceProtectionScope(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLotType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegPriceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePeggedRefPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerPriceType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerPriceTypeScope(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerPriceDirection(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'U'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerNewPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerOrderType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerNewQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerTradingSessionID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTriggerTradingSessionSubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCategory(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRootPartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRootPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRootPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRootPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRootPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRootPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRootPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeHandlingInstr(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTradeHandlingInstr(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTradeDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigSecondaryTradeID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCstmApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefCstmApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTZTransactTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDestinationIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'G'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportedPxDiff(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRptSys(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocClearingFeeIndicator(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefaultApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDisplayQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeSpecialInstructions(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxTradeVol(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDFeedTypes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchAlgorithm(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxPriceVariation(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateImpliedMarketIndicator(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinPriceIncrementAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLowLimitPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHighLimitPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingReferencePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityGroup(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegNumber(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlementCycleNo(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideSettlCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplExtID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCcyAmt(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlDetails(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligMode(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligMsgID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligTransType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlObligSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlOblig(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteMsgID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 12 or 13 or 14 or 15 or 16 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoCxldQuotes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoAccQuotes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoRejQuotes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrivateQuote(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRespondentType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDSubBookType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingEvent(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStatsIndicators(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatsType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOfSecSizes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDSecSizeType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDSecSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplBegSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplEndSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityXMLLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityXML(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityXMLSchema(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefreshIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateVolatility(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeToExpiration(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRiskFreeRate(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'P'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExerciseStyle(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptPayoutAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceQuoteMethod(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("INT" or "INX" or "PCTPAR" or "STD"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValuationMethod(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("CDS" or "CDSD" or "EQTY" or "FUT" or "FUTDA"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCapPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFloorPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrikeRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartStrikePxRange(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndStrikePxRange(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeIncrement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTickRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartTickPriceRange(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndTickPriceRange(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickIncrement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickRuleType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedInstrAttribType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedInstrAttribValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityTime(Fix50Context context, FixMessage message, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityTime(Fix50Context context, FixMessage message, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSymbolSfx(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDerivativeSecurityAltID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityAltID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityAltIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryLowLimitPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityRuleID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeRuleID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeOptPayAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProductComplex(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeProductComplex(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYearIncrement(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryHighLimitPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinLotSize(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExecInstRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLotTypeRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMatchRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMaturityRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOrdTypeRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTimeInForceRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradingReferencePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFlexProductEligibilityIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivFlexProductEligibilityIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFlexibleIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeProduct(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityGroup(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeCFICode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecuritySubType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeMaturityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeMaturityTime(Fix50Context context, FixMessage message, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSettleOnOpenFlag(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrmtAssignmentMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityStatus(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrRegistry(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeCountryOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeStateOrProvinceOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeLocaleOfIssue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeStrikePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeStrikeCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeStrikeMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeStrikeValue(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeOptAttribute(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeContractMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeMinPriceIncrement(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeMinPriceIncrementAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeTimeUnit(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityExchange(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativePositionLimit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeNTPositionLimit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeIssuer(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeIssueDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEncodedIssuerLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEncodedIssuer(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEncodedSecurityDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEncodedSecurityDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityXMLLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityXML(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityXMLSchema(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeContractSettlMonth(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDerivativeEvents(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEventType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEventDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEventTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEventPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeEventText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDerivativeInstrumentParties(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrumentPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrumentPartyIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrumentPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDerivativeInstrumentPartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrumentPartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrumentPartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeExerciseStyle(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketSegmentID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYearIncrementUnits(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYearFormat(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeExerciseStyle(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryPriceLimitType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceLimitType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecInstValue(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTradingSessionRules(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMarketSegments(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDerivativeInstrAttrib(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedInstrAttrib(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrAttribType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeInstrAttribValue(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativePriceUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativePriceUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeSettlMethod(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativePriceQuoteMethod(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeValuationMethod(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeListMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeCapPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeFloorPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativePutOrCall(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListUpdateAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateParentMktSegmID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesUpdateAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRejectText(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFeeMultiplier(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSymbol(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSymbolSfx(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingLegSecurityAltID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityAltID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityAltIDSource(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecuritySubType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegMaturityMonthYear(Fix50Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegStrikePrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityExchange(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOfLegUnderlyings(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegPutOrCall(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegCFICode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegMaturityDate(Fix50Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplReqType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplResponseType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplTotalMessageCount(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplLastSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoApplIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplResendFlag(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplResponseID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplResponseError(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefApplID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefApplLastSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPutOrCall(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoFills(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoFills(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFillExecID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFillPx(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFillQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocSettlCurrency(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesEvent(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassActionReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNotAffectedOrders(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotAffectedOrderID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotAffOrigClOrdID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassActionType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassActionScope(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassActionResponse(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassActionRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultilegModel(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultilegPriceMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegVolatility(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDividendYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegDividendYield(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCurrencyRatio(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCurrencyRatio(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegExecInst(Fix50Context context, FixMessage message, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContingencyType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListRejectReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 11 or 2 or 4 or 5 or 6 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrdRepIndicators(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRepPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRepIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradePublishIndicator(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegOptAttribute(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegSecurityDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketReportID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketUpdateAction(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketSegmentDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedMktSegmDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedMktSegmDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplNewSeqNum(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptedPasswordMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptedPasswordLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptedPassword(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptedNewPasswordLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptedNewPassword(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLegMaturityTime(Fix50Context context, FixMessage message, FixField.ZonedTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefApplExtID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefaultApplExtID(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefaultCstmApplVerID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionStatus(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefaultVerIndicator(Fix50Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested4PartySubIDType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested4PartySubID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested4PartySubIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested4PartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested4PartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested4PartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested4PartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLastQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingExerciseStyle(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegExerciseStyle(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPriceUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPriceUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPriceUnitOfMeasure(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPriceUnitOfMeasureQty(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplReportType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideExecID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderDelay(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderDelayUnit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateVenueType(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('E' or 'P' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefOrdIDReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigCustOrderCapacity(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefApplReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateModelType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractMultiplierUnit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractMultiplierUnit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingContractMultiplierUnit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeContractMultiplierUnit(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFlowScheduleType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegFlowScheduleType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFlowScheduleType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDerivativeFlowScheduleType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFillLiquidityInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideLiquidityInd(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRateSources(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRateSourceField(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRateSourceType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReferencePage(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRestructuringType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("FR" or "MM" or "MR" or "XR"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSeniority(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("SB" or "SD" or "SR"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotionalPercentageOutstanding(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOriginalNotionalPercentageOutstanding(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRestructuringType(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSeniority(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingNotionalPercentageOutstanding(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingOriginalNotionalPercentageOutstanding(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAttachmentPoint(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDetachmentPoint(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingAttachmentPoint(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingDetachmentPoint(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTargetPartyIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetPartyID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetPartyIDSource(Fix50Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetPartyRole(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListDesc(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityListDescLen(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityListDesc(Fix50Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListTypeSource(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewsID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewsCategory(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLanguageCode(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNewsRefIDs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewsRefID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewsRefType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePriceDeterminationMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePriceBoundaryMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePriceBoundaryPrecision(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPriceDeterminationMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptPayoutType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoComplexEvents(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexOptPayoutAmount(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventPrice(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventPriceBoundaryMethod(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventPriceBoundaryPrecision(Fix50Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventPriceTimeType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventCondition(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoComplexEventDates(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventStartDate(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventEndDate(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoComplexEventTimes(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventStartTime(Fix50Context context, FixMessage message, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplexEventEndTime(Fix50Context context, FixMessage message, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnReqID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnReqType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAsgnReqs(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDStreamID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnRptID(Fix50Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnRejReason(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnAckType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRelSymTransactTime(Fix50Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStreamAsgnType(Fix50Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}
}
