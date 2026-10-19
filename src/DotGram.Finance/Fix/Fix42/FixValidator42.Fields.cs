using System;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.cs from the FIX 4.2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>The check of every field of FIX 4.2 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator42
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 4.2 Account, tag 1, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Account                          { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 4.2 AdvId, tag 2, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AdvId                            { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 4.2 AdvRefID, tag 3, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AdvRefID                         { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 4.2 AdvSide, tag 4, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> AdvSide                          { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.2 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AdvTransType                     { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.2 AvgPx, tag 6, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AvgPx                            { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 4.2 BeginSeqNo, tag 7, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   BeginSeqNo                       { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 4.2 BeginString, tag 8, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      BeginString                      { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 4.2 BodyLength, tag 9, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   BodyLength                       { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 4.2 CheckSum, tag 10, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CheckSum                         { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 4.2 ClOrdID, tag 11, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ClOrdID                          { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 4.2 Commission, tag 12, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   Commission                       { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 4.2 CommType, tag 13, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> CommType                         { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.2 CumQty, tag 14, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   CumQty                           { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 4.2 Currency, tag 15, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Currency                         { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 4.2 EndSeqNo, tag 16, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EndSeqNo                         { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 4.2 ExecID, tag 17, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ExecID                           { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 4.2 ExecInst, tag 18, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Multiple>  ExecInst                         { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.2 ExecRefID, tag 19, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ExecRefID                        { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 4.2 ExecTransType, tag 20, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> ExecTransType                    { get; set; } = ValidateExecTransType;

	/// <summary>Holds a FIX 4.2 HandlInst, tag 21, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> HandlInst                        { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.2 IDSource, tag 22, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      IDSource                         { get; set; } = ValidateIDSource;

	/// <summary>Holds a FIX 4.2 IOIid, tag 23, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      IOIid                            { get; set; } = ValidateIOIid;

	/// <summary>Holds a FIX 4.2 IOIOthSvc, tag 24, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> IOIOthSvc                        { get; set; } = ValidateIOIOthSvc;

	/// <summary>Holds a FIX 4.2 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> IOIQltyInd                       { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.2 IOIRefID, tag 26, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      IOIRefID                         { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 4.2 IOIShares, tag 27, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      IOIShares                        { get; set; } = ValidateIOIShares;

	/// <summary>Holds a FIX 4.2 IOITransType, tag 28, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> IOITransType                     { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.2 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> LastCapacity                     { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.2 LastMkt, tag 30, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      LastMkt                          { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 4.2 LastPx, tag 31, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LastPx                           { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 4.2 LastShares, tag 32, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LastShares                       { get; set; } = ValidateLastShares;

	/// <summary>Holds a FIX 4.2 LinesOfText, tag 33, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   LinesOfText                      { get; set; } = ValidateLinesOfText;

	/// <summary>Holds a FIX 4.2 MsgSeqNum, tag 34, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MsgSeqNum                        { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 4.2 MsgType, tag 35, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MsgType                          { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.2 NewSeqNo, tag 36, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NewSeqNo                         { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 4.2 OrderID, tag 37, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      OrderID                          { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 4.2 OrderQty, tag 38, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OrderQty                         { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 4.2 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> OrdStatus                        { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.2 OrdType, tag 40, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> OrdType                          { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.2 OrigClOrdID, tag 41, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      OrigClOrdID                      { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 4.2 OrigTime, tag 42, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> OrigTime                         { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 4.2 PossDupFlag, tag 43, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   PossDupFlag                      { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 4.2 Price, tag 44, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   Price                            { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 4.2 RefSeqNum, tag 45, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   RefSeqNum                        { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 4.2 RelatdSym, tag 46, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      RelatdSym                        { get; set; } = ValidateRelatdSym;

	/// <summary>Holds a FIX 4.2 Rule80A, tag 47, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> Rule80A                          { get; set; } = ValidateRule80A;

	/// <summary>Holds a FIX 4.2 SecurityID, tag 48, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityID                       { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 4.2 SenderCompID, tag 49, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SenderCompID                     { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 4.2 SenderSubID, tag 50, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SenderSubID                      { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 4.2 SendingDate, tag 51, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      SendingDate                      { get; set; } = ValidateSendingDate;

	/// <summary>Holds a FIX 4.2 SendingTime, tag 52, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> SendingTime                      { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 4.2 Shares, tag 53, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   Shares                           { get; set; } = ValidateShares;

	/// <summary>Holds a FIX 4.2 Side, tag 54, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> Side                             { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.2 Symbol, tag 55, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Symbol                           { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 4.2 TargetCompID, tag 56, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TargetCompID                     { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 4.2 TargetSubID, tag 57, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TargetSubID                      { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 4.2 Text, tag 58, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Text                             { get; set; } = ValidateText;

	/// <summary>Holds a FIX 4.2 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> TimeInForce                      { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.2 TransactTime, tag 60, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TransactTime                     { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 4.2 Urgency, tag 61, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> Urgency                          { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.2 ValidUntilTime, tag 62, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> ValidUntilTime                   { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 4.2 SettlmntTyp, tag 63, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SettlmntTyp                      { get; set; } = ValidateSettlmntTyp;

	/// <summary>Holds a FIX 4.2 FutSettDate, tag 64, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      FutSettDate                      { get; set; } = ValidateFutSettDate;

	/// <summary>Holds a FIX 4.2 SymbolSfx, tag 65, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SymbolSfx                        { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 4.2 ListID, tag 66, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ListID                           { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 4.2 ListSeqNo, tag 67, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ListSeqNo                        { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 4.2 TotNoOrders, tag 68, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TotNoOrders                      { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 4.2 ListExecInst, tag 69, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ListExecInst                     { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 4.2 AllocID, tag 70, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AllocID                          { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 4.2 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> AllocTransType                   { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.2 RefAllocID, tag 72, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      RefAllocID                       { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 4.2 NoOrders, tag 73, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoOrders                         { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 4.2 AvgPrxPrecision, tag 74, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   AvgPrxPrecision                  { get; set; } = ValidateAvgPrxPrecision;

	/// <summary>Holds a FIX 4.2 TradeDate, tag 75, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      TradeDate                        { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 4.2 ExecBroker, tag 76, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ExecBroker                       { get; set; } = ValidateExecBroker;

	/// <summary>Holds a FIX 4.2 OpenClose, tag 77, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> OpenClose                        { get; set; } = ValidateOpenClose;

	/// <summary>Holds a FIX 4.2 NoAllocs, tag 78, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoAllocs                         { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 4.2 AllocAccount, tag 79, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AllocAccount                     { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 4.2 AllocShares, tag 80, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AllocShares                      { get; set; } = ValidateAllocShares;

	/// <summary>Holds a FIX 4.2 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> ProcessCode                      { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.2 NoRpts, tag 82, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoRpts                           { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 4.2 RptSeq, tag 83, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   RptSeq                           { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 4.2 CxlQty, tag 84, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   CxlQty                           { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 4.2 NoDlvyInst, tag 85, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoDlvyInst                       { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 4.2 DlvyInst, tag 86, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      DlvyInst                         { get; set; } = ValidateDlvyInst;

	/// <summary>Holds a FIX 4.2 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   AllocStatus                      { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.2 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   AllocRejCode                     { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.2 Signature, tag 89, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      Signature                        { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 4.2 SecureDataLen, tag 90, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SecureDataLen                    { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 4.2 SecureData, tag 91, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      SecureData                       { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 4.2 BrokerOfCredit, tag 92, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      BrokerOfCredit                   { get; set; } = ValidateBrokerOfCredit;

	/// <summary>Holds a FIX 4.2 SignatureLength, tag 93, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SignatureLength                  { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 4.2 EmailType, tag 94, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> EmailType                        { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.2 RawDataLength, tag 95, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   RawDataLength                    { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 4.2 RawData, tag 96, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      RawData                          { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 4.2 PossResend, tag 97, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   PossResend                       { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 4.2 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncryptMethod                    { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.2 StopPx, tag 99, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   StopPx                           { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 4.2 ExDestination, tag 100, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ExDestination                    { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 4.2 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   CxlRejReason                     { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.2 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   OrdRejReason                     { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.2 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> IOIQualifier                     { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.2 WaveNo, tag 105, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      WaveNo                           { get; set; } = ValidateWaveNo;

	/// <summary>Holds a FIX 4.2 Issuer, tag 106, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Issuer                           { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 4.2 SecurityDesc, tag 107, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityDesc                     { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 4.2 HeartBtInt, tag 108, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   HeartBtInt                       { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 4.2 ClientID, tag 109, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ClientID                         { get; set; } = ValidateClientID;

	/// <summary>Holds a FIX 4.2 MinQty, tag 110, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MinQty                           { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 4.2 MaxFloor, tag 111, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MaxFloor                         { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 4.2 TestReqID, tag 112, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TestReqID                        { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 4.2 ReportToExch, tag 113, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   ReportToExch                     { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 4.2 LocateReqd, tag 114, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   LocateReqd                       { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 4.2 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      OnBehalfOfCompID                 { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 4.2 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      OnBehalfOfSubID                  { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 4.2 QuoteID, tag 117, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      QuoteID                          { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 4.2 NetMoney, tag 118, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   NetMoney                         { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 4.2 SettlCurrAmt, tag 119, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SettlCurrAmt                     { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 4.2 SettlCurrency, tag 120, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlCurrency                    { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 4.2 ForexReq, tag 121, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   ForexReq                         { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 4.2 OrigSendingTime, tag 122, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> OrigSendingTime                  { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 4.2 GapFillFlag, tag 123, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   GapFillFlag                      { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 4.2 NoExecs, tag 124, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoExecs                          { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 4.2 CxlType, tag 125, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> CxlType                          { get; set; } = ValidateCxlType;

	/// <summary>Holds a FIX 4.2 ExpireTime, tag 126, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> ExpireTime                       { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 4.2 DKReason, tag 127, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> DKReason                         { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.2 DeliverToCompID, tag 128, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      DeliverToCompID                  { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 4.2 DeliverToSubID, tag 129, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      DeliverToSubID                   { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 4.2 IOINaturalFlag, tag 130, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   IOINaturalFlag                   { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 4.2 QuoteReqID, tag 131, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      QuoteReqID                       { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 4.2 BidPx, tag 132, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   BidPx                            { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 4.2 OfferPx, tag 133, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OfferPx                          { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 4.2 BidSize, tag 134, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   BidSize                          { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 4.2 OfferSize, tag 135, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OfferSize                        { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 4.2 NoMiscFees, tag 136, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoMiscFees                       { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 4.2 MiscFeeAmt, tag 137, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MiscFeeAmt                       { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 4.2 MiscFeeCurr, tag 138, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MiscFeeCurr                      { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 4.2 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MiscFeeType                      { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.2 PrevClosePx, tag 140, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   PrevClosePx                      { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 4.2 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   ResetSeqNumFlag                  { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 4.2 SenderLocationID, tag 142, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SenderLocationID                 { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 4.2 TargetLocationID, tag 143, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TargetLocationID                 { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 4.2 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      OnBehalfOfLocationID             { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 4.2 DeliverToLocationID, tag 145, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      DeliverToLocationID              { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 4.2 NoRelatedSym, tag 146, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoRelatedSym                     { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 4.2 Subject, tag 147, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Subject                          { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 4.2 Headline, tag 148, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Headline                         { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 4.2 URLLink, tag 149, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      URLLink                          { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 4.2 ExecType, tag 150, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> ExecType                         { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.2 LeavesQty, tag 151, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LeavesQty                        { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 4.2 CashOrderQty, tag 152, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   CashOrderQty                     { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 4.2 AllocAvgPx, tag 153, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AllocAvgPx                       { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 4.2 AllocNetMoney, tag 154, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AllocNetMoney                    { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 4.2 SettlCurrFxRate, tag 155, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SettlCurrFxRate                  { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 4.2 SettlCurrFxRateCalc, tag 156, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SettlCurrFxRateCalc              { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.2 NumDaysInterest, tag 157, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NumDaysInterest                  { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 4.2 AccruedInterestRate, tag 158, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AccruedInterestRate              { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 4.2 AccruedInterestAmt, tag 159, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AccruedInterestAmt               { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 4.2 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SettlInstMode                    { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.2 AllocText, tag 161, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AllocText                        { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 4.2 SettlInstID, tag 162, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlInstID                      { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 4.2 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SettlInstTransType               { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.2 EmailThreadID, tag 164, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      EmailThreadID                    { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 4.2 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SettlInstSource                  { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.2 SettlLocation, tag 166, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlLocation                    { get; set; } = ValidateSettlLocation;

	/// <summary>Holds a FIX 4.2 SecurityType, tag 167, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityType                     { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.2 EffectiveTime, tag 168, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> EffectiveTime                    { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 4.2 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   StandInstDbType                  { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.2 StandInstDbName, tag 170, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      StandInstDbName                  { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 4.2 StandInstDbID, tag 171, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      StandInstDbID                    { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 4.2 SettlDeliveryType, tag 172, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SettlDeliveryType                { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.2 SettlDepositoryCode, tag 173, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlDepositoryCode              { get; set; } = ValidateSettlDepositoryCode;

	/// <summary>Holds a FIX 4.2 SettlBrkrCode, tag 174, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlBrkrCode                    { get; set; } = ValidateSettlBrkrCode;

	/// <summary>Holds a FIX 4.2 SettlInstCode, tag 175, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlInstCode                    { get; set; } = ValidateSettlInstCode;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentName, tag 176, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentName           { get; set; } = ValidateSecuritySettlAgentName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentCode, tag 177, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentCode           { get; set; } = ValidateSecuritySettlAgentCode;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentAcctNum, tag 178, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentAcctNum        { get; set; } = ValidateSecuritySettlAgentAcctNum;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentAcctName, tag 179, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentAcctName       { get; set; } = ValidateSecuritySettlAgentAcctName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentContactName, tag 180, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentContactName    { get; set; } = ValidateSecuritySettlAgentContactName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentContactPhone, tag 181, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecuritySettlAgentContactPhone   { get; set; } = ValidateSecuritySettlAgentContactPhone;

	/// <summary>Holds a FIX 4.2 CashSettlAgentName, tag 182, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentName               { get; set; } = ValidateCashSettlAgentName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentCode, tag 183, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentCode               { get; set; } = ValidateCashSettlAgentCode;

	/// <summary>Holds a FIX 4.2 CashSettlAgentAcctNum, tag 184, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentAcctNum            { get; set; } = ValidateCashSettlAgentAcctNum;

	/// <summary>Holds a FIX 4.2 CashSettlAgentAcctName, tag 185, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentAcctName           { get; set; } = ValidateCashSettlAgentAcctName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentContactName, tag 186, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentContactName        { get; set; } = ValidateCashSettlAgentContactName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentContactPhone, tag 187, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      CashSettlAgentContactPhone       { get; set; } = ValidateCashSettlAgentContactPhone;

	/// <summary>Holds a FIX 4.2 BidSpotRate, tag 188, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   BidSpotRate                      { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 4.2 BidForwardPoints, tag 189, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   BidForwardPoints                 { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 4.2 OfferSpotRate, tag 190, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OfferSpotRate                    { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 4.2 OfferForwardPoints, tag 191, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OfferForwardPoints               { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 4.2 OrderQty2, tag 192, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OrderQty2                        { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 4.2 FutSettDate2, tag 193, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      FutSettDate2                     { get; set; } = ValidateFutSettDate2;

	/// <summary>Holds a FIX 4.2 LastSpotRate, tag 194, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LastSpotRate                     { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 4.2 LastForwardPoints, tag 195, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LastForwardPoints                { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 4.2 AllocLinkID, tag 196, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      AllocLinkID                      { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 4.2 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   AllocLinkType                    { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.2 SecondaryOrderID, tag 198, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecondaryOrderID                 { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 4.2 NoIOIQualifiers, tag 199, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoIOIQualifiers                  { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 4.2 MaturityMonthYear, tag 200, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.MonthYear> MaturityMonthYear                { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 4.2 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   PutOrCall                        { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.2 StrikePrice, tag 202, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   StrikePrice                      { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 4.2 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   CoveredOrUncovered               { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.2 CustomerOrFirm, tag 204, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   CustomerOrFirm                   { get; set; } = ValidateCustomerOrFirm;

	/// <summary>Holds a FIX 4.2 MaturityDay, tag 205, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MaturityDay                      { get; set; } = ValidateMaturityDay;

	/// <summary>Holds a FIX 4.2 OptAttribute, tag 206, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> OptAttribute                     { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 4.2 SecurityExchange, tag 207, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityExchange                 { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 4.2 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   NotifyBrokerOfCredit             { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 4.2 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   AllocHandlInst                   { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.2 MaxShow, tag 210, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MaxShow                          { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 4.2 PegDifference, tag 211, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   PegDifference                    { get; set; } = ValidatePegDifference;

	/// <summary>Holds a FIX 4.2 XmlDataLen, tag 212, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   XmlDataLen                       { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 4.2 XmlData, tag 213, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      XmlData                          { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 4.2 SettlInstRefID, tag 214, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SettlInstRefID                   { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 4.2 NoRoutingIDs, tag 215, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoRoutingIDs                     { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 4.2 RoutingType, tag 216, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   RoutingType                      { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.2 RoutingID, tag 217, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      RoutingID                        { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 4.2 SpreadToBenchmark, tag 218, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SpreadToBenchmark                { get; set; } = ValidateSpreadToBenchmark;

	/// <summary>Holds a FIX 4.2 Benchmark, tag 219, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> Benchmark                        { get; set; } = ValidateBenchmark;

	/// <summary>Holds a FIX 4.2 CouponRate, tag 223, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   CouponRate                       { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 4.2 ContractMultiplier, tag 231, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   ContractMultiplier               { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 4.2 MDReqID, tag 262, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDReqID                          { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 4.2 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> SubscriptionRequestType          { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.2 MarketDepth, tag 264, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MarketDepth                      { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 4.2 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MDUpdateType                     { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.2 AggregatedBook, tag 266, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   AggregatedBook                   { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 4.2 NoMDEntryTypes, tag 267, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoMDEntryTypes                   { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 4.2 NoMDEntries, tag 268, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoMDEntries                      { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 4.2 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MDEntryType                      { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.2 MDEntryPx, tag 270, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MDEntryPx                        { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 4.2 MDEntrySize, tag 271, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   MDEntrySize                      { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 4.2 MDEntryDate, tag 272, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      MDEntryDate                      { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 4.2 MDEntryTime, tag 273, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Time>      MDEntryTime                      { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 4.2 TickDirection, tag 274, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> TickDirection                    { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.2 MDMkt, tag 275, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDMkt                            { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 4.2 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Multiple>  QuoteCondition                   { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.2 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Multiple>  TradeCondition                   { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.2 MDEntryID, tag 278, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDEntryID                        { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 4.2 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MDUpdateAction                   { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.2 MDEntryRefID, tag 280, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDEntryRefID                     { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 4.2 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MDReqRejReason                   { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.2 MDEntryOriginator, tag 282, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDEntryOriginator                { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 4.2 LocationID, tag 283, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      LocationID                       { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 4.2 DeskID, tag 284, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      DeskID                           { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 4.2 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> DeleteReason                     { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.2 OpenCloseSettleFlag, tag 286, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> OpenCloseSettleFlag              { get; set; } = ValidateOpenCloseSettleFlag;

	/// <summary>Holds a FIX 4.2 SellerDays, tag 287, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SellerDays                       { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 4.2 MDEntryBuyer, tag 288, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDEntryBuyer                     { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 4.2 MDEntrySeller, tag 289, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MDEntrySeller                    { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 4.2 MDEntryPositionNo, tag 290, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MDEntryPositionNo                { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 4.2 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> FinancialStatus                  { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.2 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> CorporateAction                  { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.2 DefBidSize, tag 293, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DefBidSize                       { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 4.2 DefOfferSize, tag 294, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DefOfferSize                     { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 4.2 NoQuoteEntries, tag 295, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoQuoteEntries                   { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 4.2 NoQuoteSets, tag 296, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoQuoteSets                      { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 4.2 QuoteAckStatus, tag 297, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteAckStatus                   { get; set; } = ValidateQuoteAckStatus;

	/// <summary>Holds a FIX 4.2 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteCancelType                  { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.2 QuoteEntryID, tag 299, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      QuoteEntryID                     { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 4.2 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteRejectReason                { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.2 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteResponseLevel               { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.2 QuoteSetID, tag 302, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      QuoteSetID                       { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 4.2 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteRequestType                 { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.2 TotQuoteEntries, tag 304, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TotQuoteEntries                  { get; set; } = ValidateTotQuoteEntries;

	/// <summary>Holds a FIX 4.2 UnderlyingIDSource, tag 305, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingIDSource               { get; set; } = ValidateUnderlyingIDSource;

	/// <summary>Holds a FIX 4.2 UnderlyingIssuer, tag 306, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingIssuer                 { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSecurityDesc           { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSecurityExchange       { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSecurityID             { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSecurityType           { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 4.2 UnderlyingSymbol, tag 311, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSymbol                 { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 4.2 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingSymbolSfx              { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 4.2 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.MonthYear> UnderlyingMaturityMonthYear      { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 4.2 UnderlyingMaturityDay, tag 314, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   UnderlyingMaturityDay            { get; set; } = ValidateUnderlyingMaturityDay;

	/// <summary>Holds a FIX 4.2 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   UnderlyingPutOrCall              { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 4.2 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   UnderlyingStrikePrice            { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 4.2 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> UnderlyingOptAttribute           { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 4.2 UnderlyingCurrency, tag 318, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      UnderlyingCurrency               { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 4.2 RatioQty, tag 319, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   RatioQty                         { get; set; } = ValidateRatioQty;

	/// <summary>Holds a FIX 4.2 SecurityReqID, tag 320, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityReqID                    { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 4.2 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SecurityRequestType              { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.2 SecurityResponseID, tag 322, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityResponseID               { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 4.2 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SecurityResponseType             { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.2 SecurityStatusReqID, tag 324, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      SecurityStatusReqID              { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 4.2 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   UnsolicitedIndicator             { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 4.2 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SecurityTradingStatus            { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.2 HaltReason, tag 327, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> HaltReason                       { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.2 InViewOfCommon, tag 328, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   InViewOfCommon                   { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 4.2 DueToRelated, tag 329, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   DueToRelated                     { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 4.2 BuyVolume, tag 330, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   BuyVolume                        { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 4.2 SellVolume, tag 331, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SellVolume                       { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 4.2 HighPx, tag 332, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   HighPx                           { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 4.2 LowPx, tag 333, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LowPx                            { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 4.2 Adjustment, tag 334, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   Adjustment                       { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.2 TradSesReqID, tag 335, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TradSesReqID                     { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 4.2 TradingSessionID, tag 336, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      TradingSessionID                 { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 4.2 ContraTrader, tag 337, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ContraTrader                     { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 4.2 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TradSesMethod                    { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.2 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TradSesMode                      { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.2 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TradSesStatus                    { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.2 TradSesStartTime, tag 341, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TradSesStartTime                 { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 4.2 TradSesOpenTime, tag 342, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TradSesOpenTime                  { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 4.2 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TradSesPreCloseTime              { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 4.2 TradSesCloseTime, tag 344, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TradSesCloseTime                 { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 4.2 TradSesEndTime, tag 345, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> TradSesEndTime                   { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 4.2 NumberOfOrders, tag 346, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NumberOfOrders                   { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 4.2 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      MessageEncoding                  { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.2 EncodedIssuerLen, tag 348, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedIssuerLen                 { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 4.2 EncodedIssuer, tag 349, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedIssuer                    { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 4.2 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedSecurityDescLen           { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 4.2 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedSecurityDesc              { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 4.2 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedListExecInstLen           { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 4.2 EncodedListExecInst, tag 353, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedListExecInst              { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 4.2 EncodedTextLen, tag 354, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedTextLen                   { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 4.2 EncodedText, tag 355, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedText                      { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 4.2 EncodedSubjectLen, tag 356, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedSubjectLen                { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 4.2 EncodedSubject, tag 357, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedSubject                   { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 4.2 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedHeadlineLen               { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 4.2 EncodedHeadline, tag 359, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedHeadline                  { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 4.2 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedAllocTextLen              { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 4.2 EncodedAllocText, tag 361, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedAllocText                 { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedUnderlyingIssuerLen       { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedUnderlyingIssuer          { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedUnderlyingSecurityDescLen { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedUnderlyingSecurityDesc    { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.2 AllocPrice, tag 366, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   AllocPrice                       { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 4.2 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> QuoteSetValidUntilTime           { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 4.2 QuoteEntryRejectReason, tag 368, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   QuoteEntryRejectReason           { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 4.2 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   LastMsgSeqNumProcessed           { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 4.2 OnBehalfOfSendingTime, tag 370, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> OnBehalfOfSendingTime            { get; set; } = ValidateOnBehalfOfSendingTime;

	/// <summary>Holds a FIX 4.2 RefTagID, tag 371, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   RefTagID                         { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 4.2 RefMsgType, tag 372, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      RefMsgType                       { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 4.2 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SessionRejectReason              { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.2 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> BidRequestTransType              { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.2 ContraBroker, tag 375, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ContraBroker                     { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 4.2 ComplianceID, tag 376, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ComplianceID                     { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 4.2 SolicitedFlag, tag 377, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   SolicitedFlag                    { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 4.2 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ExecRestatementReason            { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.2 BusinessRejectRefID, tag 379, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      BusinessRejectRefID              { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 4.2 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   BusinessRejectReason             { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.2 GrossTradeAmt, tag 381, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   GrossTradeAmt                    { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 4.2 NoContraBrokers, tag 382, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoContraBrokers                  { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 4.2 MaxMessageSize, tag 383, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   MaxMessageSize                   { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 4.2 NoMsgTypes, tag 384, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoMsgTypes                       { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 4.2 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MsgDirection                     { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.2 NoTradingSessions, tag 386, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoTradingSessions                { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 4.2 TotalVolumeTraded, tag 387, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   TotalVolumeTraded                { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 4.2 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> DiscretionInst                   { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.2 DiscretionOffset, tag 389, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DiscretionOffset                 { get; set; } = ValidateDiscretionOffset;

	/// <summary>Holds a FIX 4.2 BidID, tag 390, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      BidID                            { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 4.2 ClientBidID, tag 391, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ClientBidID                      { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 4.2 ListName, tag 392, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ListName                         { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 4.2 TotalNumSecurities, tag 393, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TotalNumSecurities               { get; set; } = ValidateTotalNumSecurities;

	/// <summary>Holds a FIX 4.2 BidType, tag 394, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   BidType                          { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.2 NumTickets, tag 395, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NumTickets                       { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 4.2 SideValue1, tag 396, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SideValue1                       { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 4.2 SideValue2, tag 397, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   SideValue2                       { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 4.2 NoBidDescriptors, tag 398, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoBidDescriptors                 { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 4.2 BidDescriptorType, tag 399, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   BidDescriptorType                { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.2 BidDescriptor, tag 400, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      BidDescriptor                    { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 4.2 SideValueInd, tag 401, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   SideValueInd                     { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.2 LiquidityPctLow, tag 402, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LiquidityPctLow                  { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 4.2 LiquidityPctHigh, tag 403, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LiquidityPctHigh                 { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 4.2 LiquidityValue, tag 404, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   LiquidityValue                   { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 4.2 EFPTrackingError, tag 405, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   EFPTrackingError                 { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 4.2 FairValue, tag 406, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   FairValue                        { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 4.2 OutsideIndexPct, tag 407, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OutsideIndexPct                  { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 4.2 ValueOfFutures, tag 408, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   ValueOfFutures                   { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 4.2 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   LiquidityIndType                 { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.2 WtAverageLiquidity, tag 410, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   WtAverageLiquidity               { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 4.2 ExchangeForPhysical, tag 411, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Boolean>   ExchangeForPhysical              { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 4.2 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   OutMainCntryUIndex               { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 4.2 CrossPercent, tag 413, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   CrossPercent                     { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 4.2 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ProgRptReqs                      { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.2 ProgPeriodInterval, tag 415, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ProgPeriodInterval               { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 4.2 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   IncTaxInd                        { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.2 NumBidders, tag 417, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NumBidders                       { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 4.2 TradeType, tag 418, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> TradeType                        { get; set; } = ValidateTradeType;

	/// <summary>Holds a FIX 4.2 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> BasisPxType                      { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.2 NoBidComponents, tag 420, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoBidComponents                  { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 4.2 Country, tag 421, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      Country                          { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 4.2 TotNoStrikes, tag 422, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   TotNoStrikes                     { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 4.2 PriceType, tag 423, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   PriceType                        { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.2 DayOrderQty, tag 424, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DayOrderQty                      { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 4.2 DayCumQty, tag 425, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DayCumQty                        { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 4.2 DayAvgPx, tag 426, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   DayAvgPx                         { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 4.2 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   GTBookingInst                    { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.2 NoStrikes, tag 428, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NoStrikes                        { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 4.2 ListStatusType, tag 429, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ListStatusType                   { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.2 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   NetGrossInd                      { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.2 ListOrderStatus, tag 431, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   ListOrderStatus                  { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.2 ExpireDate, tag 432, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Date>      ExpireDate                       { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 4.2 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> ListExecInstType                 { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.2 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> CxlRejResponseTo                 { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.2 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   UnderlyingCouponRate             { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 4.2 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   UnderlyingContractMultiplier     { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 4.2 ContraTradeQty, tag 437, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Decimal>   ContraTradeQty                   { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 4.2 ContraTradeTime, tag 438, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> ContraTradeTime                  { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 4.2 ClearingFirm, tag 439, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ClearingFirm                     { get; set; } = ValidateClearingFirm;

	/// <summary>Holds a FIX 4.2 ClearingAccount, tag 440, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ClearingAccount                  { get; set; } = ValidateClearingAccount;

	/// <summary>Holds a FIX 4.2 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   LiquidityNumSecurities           { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 4.2 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Action<Fix42Context, FixCheck, FixField.Character> MultiLegReportingType            { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.2 StrikeTime, tag 443, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Timestamp> StrikeTime                       { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 4.2 ListStatusText, tag 444, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Text>      ListStatusText                   { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 4.2 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Integer>   EncodedListStatusTextLen         { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 4.2 EncodedListStatusText, tag 446, to its type.</summary>
	public Action<Fix42Context, FixCheck, FixField.Data>      EncodedListStatusText            { get; set; } = ValidateEncodedListStatusText;

	static void ValidateAccount(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvId(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdvSide(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(check, field);
	}

	static void ValidateAdvTransType(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("C" or "N" or "R"))
			Invalid(check, field);
	}

	static void ValidateAvgPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginSeqNo(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBeginString(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBodyLength(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCheckSum(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClOrdID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommission(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCommType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateCumQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCurrency(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEndSeqNo(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecInst(Fix42Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
					"C" or "D" or "E" or "F" or "G" or "I" or "L" or "M" or "N" or "O" or "P" or "R" or "S" or
					"T" or "U" or "V" or "W"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateExecRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecTransType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateHandlInst(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateIDSource(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(check, field);
	}

	static void ValidateIOIid(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIOthSvc(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIQltyInd(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('H' or 'L' or 'M'))
			Invalid(check, field);
	}

	static void ValidateIOIRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOIShares(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("L" or "M" or "S"))
			Invalid(check, field);
	}

	static void ValidateIOITransType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateLastCapacity(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(check, field);
	}

	static void ValidateLastMkt(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastShares(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLinesOfText(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgSeqNum(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgType(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or
			"9" or "A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "J" or "K" or "L" or "M" or "N" or "P" or
			"Q" or "R" or "S" or "T" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or "e" or "f" or
			"g" or "h" or "i" or "j" or "k" or "l" or "m"))
			Invalid(check, field);
	}

	static void ValidateNewSeqNo(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrdStatus(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C' or 'D' or 'E'))
			Invalid(check, field);
	}

	static void ValidateOrdType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or
			'A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'G' or 'H' or 'I' or 'P'))
			Invalid(check, field);
	}

	static void ValidateOrigClOrdID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossDupFlag(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePrice(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefSeqNum(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRelatdSym(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRule80A(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'H' or 'I' or 'J' or
			'K' or 'L' or 'M' or 'N' or 'O' or 'P' or 'R' or 'S' or 'T' or 'U' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateSecurityID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderCompID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderSubID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSendingDate(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSendingTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateShares(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSide(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateSymbol(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetCompID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetSubID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateText(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTimeInForce(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateTransactTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUrgency(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateValidUntilTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlmntTyp(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateFutSettDate(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSymbolSfx(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListSeqNo(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoOrders(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInst(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocTransType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateRefAllocID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoOrders(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAvgPrxPrecision(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeDate(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecBroker(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOpenClose(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'O'))
			Invalid(check, field);
	}

	static void ValidateNoAllocs(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAccount(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocShares(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProcessCode(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(check, field);
	}

	static void ValidateNoRpts(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRptSeq(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoDlvyInst(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDlvyInst(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocStatus(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateAllocRejCode(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(check, field);
	}

	static void ValidateSignature(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureDataLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecureData(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBrokerOfCredit(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSignatureLength(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEmailType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateRawDataLength(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRawData(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePossResend(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncryptMethod(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateStopPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExDestination(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlRejReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateOrdRejReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(check, field);
	}

	static void ValidateIOIQualifier(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'C' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or
			'S' or 'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateWaveNo(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIssuer(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityDesc(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeartBtInt(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClientID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMinQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxFloor(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTestReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateReportToExch(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocateReqd(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfCompID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfSubID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetMoney(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrAmt(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrency(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateForexReq(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrigSendingTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGapFillFlag(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoExecs(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCxlType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExpireTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDKReason(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateDeliverToCompID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToSubID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIOINaturalFlag(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidSize(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSize(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMiscFees(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeAmt(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeCurr(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMiscFeeType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidatePrevClosePx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateResetSeqNumFlag(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSenderLocationID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTargetLocationID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfLocationID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeliverToLocationID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRelatedSym(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubject(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHeadline(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateURLLink(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or
			'9' or 'A' or 'B' or 'C' or 'D' or 'E'))
			Invalid(check, field);
	}

	static void ValidateLeavesQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashOrderQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocAvgPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocNetMoney(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlCurrFxRateCalc(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNumDaysInterest(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAccruedInterestAmt(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstMode(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateAllocText(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstTransType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N' or 'R'))
			Invalid(check, field);
	}

	static void ValidateEmailThreadID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstSource(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateSettlLocation(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("CED" or "DTC" or "EUR" or "FED" or "ISO Country Code" or "PNY" or
			"PTC"))
			Invalid(check, field);
	}

	static void ValidateSecurityType(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("?" or "BA" or "CB" or "CD" or "CMO" or "CORP" or "CP" or "CPP" or
			"CS" or "FHA" or "FHL" or "FN" or "FOR" or "FUT" or "GN" or "GOVT" or "IET" or "MF" or "MIO" or "MPO" or
			"MPP" or "MPT" or "MUNI" or "NONE" or "OPT" or "PS" or "RP" or "RVRP" or "SL" or "TD" or "USTB" or
			"WAR" or "ZOO"))
			Invalid(check, field);
	}

	static void ValidateEffectiveTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateStandInstDbName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateStandInstDbID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDeliveryType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlDepositoryCode(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlBrkrCode(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstCode(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentCode(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentAcctNum(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentAcctName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentContactName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecuritySettlAgentContactPhone(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentCode(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentAcctNum(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentAcctName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentContactName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCashSettlAgentContactPhone(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidSpotRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidForwardPoints(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferSpotRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOfferForwardPoints(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOrderQty2(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFutSettDate2(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastSpotRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLastForwardPoints(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocLinkType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateSecondaryOrderID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoIOIQualifiers(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaturityMonthYear(Fix42Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePutOrCall(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateStrikePrice(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCoveredOrUncovered(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateCustomerOrFirm(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateMaturityDay(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOptAttribute(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityExchange(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNotifyBrokerOfCredit(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocHandlInst(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateMaxShow(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePegDifference(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlDataLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateXmlData(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSettlInstRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoRoutingIDs(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRoutingType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateRoutingID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSpreadToBenchmark(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBenchmark(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateCouponRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContractMultiplier(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSubscriptionRequestType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMarketDepth(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1))
			Invalid(check, field);
	}

	static void ValidateAggregatedBook(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntryTypes(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMDEntries(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(check, field);
	}

	static void ValidateMDEntryPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySize(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryDate(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryTime(Fix42Context context, FixCheck check, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTickDirection(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateMDMkt(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteCondition(Fix42Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateTradeCondition(Fix42Context context, FixCheck check, FixField.Multiple field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else
			foreach (var code in field.Value)
				if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or
					"M" or "N"))
				{
					Invalid(check, field);

					break;
				}
	}

	static void ValidateMDEntryID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDUpdateAction(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateMDEntryRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDReqRejReason(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8'))
			Invalid(check, field);
	}

	static void ValidateMDEntryOriginator(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLocationID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeskID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDeleteReason(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1'))
			Invalid(check, field);
	}

	static void ValidateOpenCloseSettleFlag(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2'))
			Invalid(check, field);
	}

	static void ValidateSellerDays(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryBuyer(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntrySeller(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMDEntryPositionNo(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFinancialStatus(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1'))
			Invalid(check, field);
	}

	static void ValidateCorporateAction(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'B' or 'C' or 'D' or 'E'))
			Invalid(check, field);
	}

	static void ValidateDefBidSize(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDefOfferSize(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteEntries(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoQuoteSets(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteAckStatus(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateQuoteCancelType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateQuoteEntryID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRejectReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateQuoteResponseLevel(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateQuoteSetID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteRequestType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateTotQuoteEntries(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIDSource(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingIssuer(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityDesc(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityExchange(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSecurityType(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbol(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingSymbolSfx(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityMonthYear(Fix42Context context, FixCheck check, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingMaturityDay(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingPutOrCall(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingStrikePrice(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingOptAttribute(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingCurrency(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRatioQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityRequestType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateSecurityResponseID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityResponseType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(check, field);
	}

	static void ValidateSecurityStatusReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnsolicitedIndicator(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSecurityTradingStatus(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 3 or 4 or
			5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateHaltReason(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(check, field);
	}

	static void ValidateInViewOfCommon(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDueToRelated(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBuyVolume(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSellVolume(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateHighPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLowPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAdjustment(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesReqID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradingSessionID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTrader(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesMethod(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesMode(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateTradSesStatus(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateTradSesStartTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesOpenTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesPreCloseTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesCloseTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradSesEndTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNumberOfOrders(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMessageEncoding(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid || field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(check, field);
	}

	static void ValidateEncodedIssuerLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedIssuer(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDescLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSecurityDesc(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInstLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListExecInst(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedTextLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedText(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubjectLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedSubject(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadlineLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedHeadline(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocTextLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedAllocText(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuerLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingIssuer(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDescLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedUnderlyingSecurityDesc(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateAllocPrice(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteSetValidUntilTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateQuoteEntryRejectReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateLastMsgSeqNumProcessed(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOnBehalfOfSendingTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefTagID(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateRefMsgType(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSessionRejectReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(check, field);
	}

	static void ValidateBidRequestTransType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('C' or 'N'))
			Invalid(check, field);
	}

	static void ValidateContraBroker(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateComplianceID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSolicitedFlag(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExecRestatementReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateBusinessRejectRefID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBusinessRejectReason(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(check, field);
	}

	static void ValidateGrossTradeAmt(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoContraBrokers(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMaxMessageSize(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoMsgTypes(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMsgDirection(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('R' or 'S'))
			Invalid(check, field);
	}

	static void ValidateNoTradingSessions(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalVolumeTraded(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDiscretionInst(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(check, field);
	}

	static void ValidateDiscretionOffset(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClientBidID(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListName(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotalNumSecurities(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNumTickets(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue1(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValue2(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNoBidDescriptors(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidDescriptorType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateBidDescriptor(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateSideValueInd(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityPctLow(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityPctHigh(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityValue(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEFPTrackingError(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateFairValue(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutsideIndexPct(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateValueOfFutures(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityIndType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(check, field);
	}

	static void ValidateWtAverageLiquidity(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExchangeForPhysical(Fix42Context context, FixCheck check, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateOutMainCntryUIndex(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCrossPercent(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateProgRptReqs(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateProgPeriodInterval(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateIncTaxInd(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateNumBidders(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTradeType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(check, field);
	}

	static void ValidateBasisPxType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or
			'B' or 'C' or 'D' or 'Z'))
			Invalid(check, field);
	}

	static void ValidateNoBidComponents(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateCountry(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateTotNoStrikes(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidatePriceType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(check, field);
	}

	static void ValidateDayOrderQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayCumQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateDayAvgPx(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateGTBookingInst(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(check, field);
	}

	static void ValidateNoStrikes(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusType(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateNetGrossInd(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
		else if (field.Value is not (1 or 2))
			Invalid(check, field);
	}

	static void ValidateListOrderStatus(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateExpireDate(Fix42Context context, FixCheck check, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListExecInstType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateCxlRejResponseTo(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2'))
			Invalid(check, field);
	}

	static void ValidateUnderlyingCouponRate(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateUnderlyingContractMultiplier(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeQty(Fix42Context context, FixCheck check, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateContraTradeTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingFirm(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateClearingAccount(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateLiquidityNumSecurities(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateMultiLegReportingType(Fix42Context context, FixCheck check, FixField.Character field)
	{
		if (!field.IsValid || field.Value is not ('1' or '2' or '3'))
			Invalid(check, field);
	}

	static void ValidateStrikeTime(Fix42Context context, FixCheck check, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateListStatusText(Fix42Context context, FixCheck check, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusTextLen(Fix42Context context, FixCheck check, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}

	static void ValidateEncodedListStatusText(Fix42Context context, FixCheck check, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(check, field);
	}
}
