using System;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand.

/// <summary>The check of every field of FIX 4.2 against its type, and against the values the repository lists for it.</summary>
partial class FixValidator42
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 4.2 Account, tag 1, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Account                          { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 4.2 AdvId, tag 2, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AdvId                            { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 4.2 AdvRefID, tag 3, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AdvRefID                         { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 4.2 AdvSide, tag 4, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> AdvSide                          { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.2 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AdvTransType                     { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.2 AvgPx, tag 6, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AvgPx                            { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 4.2 BeginSeqNo, tag 7, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   BeginSeqNo                       { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 4.2 BeginString, tag 8, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      BeginString                      { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 4.2 BodyLength, tag 9, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   BodyLength                       { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 4.2 CheckSum, tag 10, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CheckSum                         { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 4.2 ClOrdID, tag 11, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ClOrdID                          { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 4.2 Commission, tag 12, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   Commission                       { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 4.2 CommType, tag 13, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> CommType                         { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.2 CumQty, tag 14, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   CumQty                           { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 4.2 Currency, tag 15, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Currency                         { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 4.2 EndSeqNo, tag 16, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EndSeqNo                         { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 4.2 ExecID, tag 17, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ExecID                           { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 4.2 ExecInst, tag 18, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Multiple, bool>  ExecInst                         { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.2 ExecRefID, tag 19, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ExecRefID                        { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 4.2 ExecTransType, tag 20, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> ExecTransType                    { get; set; } = ValidateExecTransType;

	/// <summary>Holds a FIX 4.2 HandlInst, tag 21, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> HandlInst                        { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.2 IDSource, tag 22, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      IDSource                         { get; set; } = ValidateIDSource;

	/// <summary>Holds a FIX 4.2 IOIid, tag 23, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      IOIid                            { get; set; } = ValidateIOIid;

	/// <summary>Holds a FIX 4.2 IOIOthSvc, tag 24, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> IOIOthSvc                        { get; set; } = ValidateIOIOthSvc;

	/// <summary>Holds a FIX 4.2 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> IOIQltyInd                       { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.2 IOIRefID, tag 26, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      IOIRefID                         { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 4.2 IOIShares, tag 27, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      IOIShares                        { get; set; } = ValidateIOIShares;

	/// <summary>Holds a FIX 4.2 IOITransType, tag 28, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> IOITransType                     { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.2 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> LastCapacity                     { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.2 LastMkt, tag 30, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      LastMkt                          { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 4.2 LastPx, tag 31, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LastPx                           { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 4.2 LastShares, tag 32, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LastShares                       { get; set; } = ValidateLastShares;

	/// <summary>Holds a FIX 4.2 LinesOfText, tag 33, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   LinesOfText                      { get; set; } = ValidateLinesOfText;

	/// <summary>Holds a FIX 4.2 MsgSeqNum, tag 34, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MsgSeqNum                        { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 4.2 MsgType, tag 35, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MsgType                          { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.2 NewSeqNo, tag 36, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NewSeqNo                         { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 4.2 OrderID, tag 37, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      OrderID                          { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 4.2 OrderQty, tag 38, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OrderQty                         { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 4.2 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> OrdStatus                        { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.2 OrdType, tag 40, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> OrdType                          { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.2 OrigClOrdID, tag 41, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      OrigClOrdID                      { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 4.2 OrigTime, tag 42, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> OrigTime                         { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 4.2 PossDupFlag, tag 43, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   PossDupFlag                      { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 4.2 Price, tag 44, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   Price                            { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 4.2 RefSeqNum, tag 45, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   RefSeqNum                        { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 4.2 RelatdSym, tag 46, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      RelatdSym                        { get; set; } = ValidateRelatdSym;

	/// <summary>Holds a FIX 4.2 Rule80A, tag 47, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> Rule80A                          { get; set; } = ValidateRule80A;

	/// <summary>Holds a FIX 4.2 SecurityID, tag 48, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityID                       { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 4.2 SenderCompID, tag 49, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SenderCompID                     { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 4.2 SenderSubID, tag 50, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SenderSubID                      { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 4.2 SendingDate, tag 51, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      SendingDate                      { get; set; } = ValidateSendingDate;

	/// <summary>Holds a FIX 4.2 SendingTime, tag 52, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> SendingTime                      { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 4.2 Shares, tag 53, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   Shares                           { get; set; } = ValidateShares;

	/// <summary>Holds a FIX 4.2 Side, tag 54, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> Side                             { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.2 Symbol, tag 55, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Symbol                           { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 4.2 TargetCompID, tag 56, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TargetCompID                     { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 4.2 TargetSubID, tag 57, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TargetSubID                      { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 4.2 Text, tag 58, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Text                             { get; set; } = ValidateText;

	/// <summary>Holds a FIX 4.2 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> TimeInForce                      { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.2 TransactTime, tag 60, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TransactTime                     { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 4.2 Urgency, tag 61, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> Urgency                          { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.2 ValidUntilTime, tag 62, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> ValidUntilTime                   { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 4.2 SettlmntTyp, tag 63, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SettlmntTyp                      { get; set; } = ValidateSettlmntTyp;

	/// <summary>Holds a FIX 4.2 FutSettDate, tag 64, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      FutSettDate                      { get; set; } = ValidateFutSettDate;

	/// <summary>Holds a FIX 4.2 SymbolSfx, tag 65, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SymbolSfx                        { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 4.2 ListID, tag 66, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ListID                           { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 4.2 ListSeqNo, tag 67, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ListSeqNo                        { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 4.2 TotNoOrders, tag 68, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TotNoOrders                      { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 4.2 ListExecInst, tag 69, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ListExecInst                     { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 4.2 AllocID, tag 70, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AllocID                          { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 4.2 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> AllocTransType                   { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.2 RefAllocID, tag 72, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      RefAllocID                       { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 4.2 NoOrders, tag 73, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoOrders                         { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 4.2 AvgPrxPrecision, tag 74, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   AvgPrxPrecision                  { get; set; } = ValidateAvgPrxPrecision;

	/// <summary>Holds a FIX 4.2 TradeDate, tag 75, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      TradeDate                        { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 4.2 ExecBroker, tag 76, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ExecBroker                       { get; set; } = ValidateExecBroker;

	/// <summary>Holds a FIX 4.2 OpenClose, tag 77, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> OpenClose                        { get; set; } = ValidateOpenClose;

	/// <summary>Holds a FIX 4.2 NoAllocs, tag 78, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoAllocs                         { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 4.2 AllocAccount, tag 79, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AllocAccount                     { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 4.2 AllocShares, tag 80, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AllocShares                      { get; set; } = ValidateAllocShares;

	/// <summary>Holds a FIX 4.2 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> ProcessCode                      { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.2 NoRpts, tag 82, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoRpts                           { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 4.2 RptSeq, tag 83, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   RptSeq                           { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 4.2 CxlQty, tag 84, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   CxlQty                           { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 4.2 NoDlvyInst, tag 85, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoDlvyInst                       { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 4.2 DlvyInst, tag 86, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      DlvyInst                         { get; set; } = ValidateDlvyInst;

	/// <summary>Holds a FIX 4.2 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   AllocStatus                      { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.2 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   AllocRejCode                     { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.2 Signature, tag 89, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      Signature                        { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 4.2 SecureDataLen, tag 90, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SecureDataLen                    { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 4.2 SecureData, tag 91, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      SecureData                       { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 4.2 BrokerOfCredit, tag 92, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      BrokerOfCredit                   { get; set; } = ValidateBrokerOfCredit;

	/// <summary>Holds a FIX 4.2 SignatureLength, tag 93, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SignatureLength                  { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 4.2 EmailType, tag 94, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> EmailType                        { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.2 RawDataLength, tag 95, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   RawDataLength                    { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 4.2 RawData, tag 96, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      RawData                          { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 4.2 PossResend, tag 97, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   PossResend                       { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 4.2 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncryptMethod                    { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.2 StopPx, tag 99, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   StopPx                           { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 4.2 ExDestination, tag 100, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ExDestination                    { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 4.2 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   CxlRejReason                     { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.2 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   OrdRejReason                     { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.2 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> IOIQualifier                     { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.2 WaveNo, tag 105, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      WaveNo                           { get; set; } = ValidateWaveNo;

	/// <summary>Holds a FIX 4.2 Issuer, tag 106, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Issuer                           { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 4.2 SecurityDesc, tag 107, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityDesc                     { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 4.2 HeartBtInt, tag 108, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   HeartBtInt                       { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 4.2 ClientID, tag 109, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ClientID                         { get; set; } = ValidateClientID;

	/// <summary>Holds a FIX 4.2 MinQty, tag 110, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MinQty                           { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 4.2 MaxFloor, tag 111, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MaxFloor                         { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 4.2 TestReqID, tag 112, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TestReqID                        { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 4.2 ReportToExch, tag 113, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   ReportToExch                     { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 4.2 LocateReqd, tag 114, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   LocateReqd                       { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 4.2 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      OnBehalfOfCompID                 { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 4.2 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      OnBehalfOfSubID                  { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 4.2 QuoteID, tag 117, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      QuoteID                          { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 4.2 NetMoney, tag 118, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   NetMoney                         { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 4.2 SettlCurrAmt, tag 119, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SettlCurrAmt                     { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 4.2 SettlCurrency, tag 120, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlCurrency                    { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 4.2 ForexReq, tag 121, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   ForexReq                         { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 4.2 OrigSendingTime, tag 122, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> OrigSendingTime                  { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 4.2 GapFillFlag, tag 123, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   GapFillFlag                      { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 4.2 NoExecs, tag 124, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoExecs                          { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 4.2 CxlType, tag 125, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> CxlType                          { get; set; } = ValidateCxlType;

	/// <summary>Holds a FIX 4.2 ExpireTime, tag 126, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> ExpireTime                       { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 4.2 DKReason, tag 127, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> DKReason                         { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.2 DeliverToCompID, tag 128, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      DeliverToCompID                  { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 4.2 DeliverToSubID, tag 129, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      DeliverToSubID                   { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 4.2 IOINaturalFlag, tag 130, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   IOINaturalFlag                   { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 4.2 QuoteReqID, tag 131, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      QuoteReqID                       { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 4.2 BidPx, tag 132, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   BidPx                            { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 4.2 OfferPx, tag 133, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OfferPx                          { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 4.2 BidSize, tag 134, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   BidSize                          { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 4.2 OfferSize, tag 135, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OfferSize                        { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 4.2 NoMiscFees, tag 136, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoMiscFees                       { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 4.2 MiscFeeAmt, tag 137, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MiscFeeAmt                       { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 4.2 MiscFeeCurr, tag 138, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MiscFeeCurr                      { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 4.2 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MiscFeeType                      { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.2 PrevClosePx, tag 140, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   PrevClosePx                      { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 4.2 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   ResetSeqNumFlag                  { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 4.2 SenderLocationID, tag 142, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SenderLocationID                 { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 4.2 TargetLocationID, tag 143, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TargetLocationID                 { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 4.2 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      OnBehalfOfLocationID             { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 4.2 DeliverToLocationID, tag 145, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      DeliverToLocationID              { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 4.2 NoRelatedSym, tag 146, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoRelatedSym                     { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 4.2 Subject, tag 147, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Subject                          { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 4.2 Headline, tag 148, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Headline                         { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 4.2 URLLink, tag 149, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      URLLink                          { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 4.2 ExecType, tag 150, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> ExecType                         { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.2 LeavesQty, tag 151, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LeavesQty                        { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 4.2 CashOrderQty, tag 152, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   CashOrderQty                     { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 4.2 AllocAvgPx, tag 153, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AllocAvgPx                       { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 4.2 AllocNetMoney, tag 154, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AllocNetMoney                    { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 4.2 SettlCurrFxRate, tag 155, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SettlCurrFxRate                  { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 4.2 SettlCurrFxRateCalc, tag 156, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SettlCurrFxRateCalc              { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.2 NumDaysInterest, tag 157, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NumDaysInterest                  { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 4.2 AccruedInterestRate, tag 158, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AccruedInterestRate              { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 4.2 AccruedInterestAmt, tag 159, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AccruedInterestAmt               { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 4.2 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SettlInstMode                    { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.2 AllocText, tag 161, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AllocText                        { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 4.2 SettlInstID, tag 162, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlInstID                      { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 4.2 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SettlInstTransType               { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.2 EmailThreadID, tag 164, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      EmailThreadID                    { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 4.2 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SettlInstSource                  { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.2 SettlLocation, tag 166, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlLocation                    { get; set; } = ValidateSettlLocation;

	/// <summary>Holds a FIX 4.2 SecurityType, tag 167, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityType                     { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.2 EffectiveTime, tag 168, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> EffectiveTime                    { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 4.2 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   StandInstDbType                  { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.2 StandInstDbName, tag 170, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      StandInstDbName                  { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 4.2 StandInstDbID, tag 171, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      StandInstDbID                    { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 4.2 SettlDeliveryType, tag 172, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SettlDeliveryType                { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.2 SettlDepositoryCode, tag 173, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlDepositoryCode              { get; set; } = ValidateSettlDepositoryCode;

	/// <summary>Holds a FIX 4.2 SettlBrkrCode, tag 174, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlBrkrCode                    { get; set; } = ValidateSettlBrkrCode;

	/// <summary>Holds a FIX 4.2 SettlInstCode, tag 175, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlInstCode                    { get; set; } = ValidateSettlInstCode;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentName, tag 176, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentName           { get; set; } = ValidateSecuritySettlAgentName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentCode, tag 177, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentCode           { get; set; } = ValidateSecuritySettlAgentCode;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentAcctNum, tag 178, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentAcctNum        { get; set; } = ValidateSecuritySettlAgentAcctNum;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentAcctName, tag 179, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentAcctName       { get; set; } = ValidateSecuritySettlAgentAcctName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentContactName, tag 180, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentContactName    { get; set; } = ValidateSecuritySettlAgentContactName;

	/// <summary>Holds a FIX 4.2 SecuritySettlAgentContactPhone, tag 181, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecuritySettlAgentContactPhone   { get; set; } = ValidateSecuritySettlAgentContactPhone;

	/// <summary>Holds a FIX 4.2 CashSettlAgentName, tag 182, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentName               { get; set; } = ValidateCashSettlAgentName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentCode, tag 183, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentCode               { get; set; } = ValidateCashSettlAgentCode;

	/// <summary>Holds a FIX 4.2 CashSettlAgentAcctNum, tag 184, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentAcctNum            { get; set; } = ValidateCashSettlAgentAcctNum;

	/// <summary>Holds a FIX 4.2 CashSettlAgentAcctName, tag 185, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentAcctName           { get; set; } = ValidateCashSettlAgentAcctName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentContactName, tag 186, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentContactName        { get; set; } = ValidateCashSettlAgentContactName;

	/// <summary>Holds a FIX 4.2 CashSettlAgentContactPhone, tag 187, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      CashSettlAgentContactPhone       { get; set; } = ValidateCashSettlAgentContactPhone;

	/// <summary>Holds a FIX 4.2 BidSpotRate, tag 188, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   BidSpotRate                      { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 4.2 BidForwardPoints, tag 189, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   BidForwardPoints                 { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 4.2 OfferSpotRate, tag 190, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OfferSpotRate                    { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 4.2 OfferForwardPoints, tag 191, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OfferForwardPoints               { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 4.2 OrderQty2, tag 192, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OrderQty2                        { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 4.2 FutSettDate2, tag 193, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      FutSettDate2                     { get; set; } = ValidateFutSettDate2;

	/// <summary>Holds a FIX 4.2 LastSpotRate, tag 194, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LastSpotRate                     { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 4.2 LastForwardPoints, tag 195, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LastForwardPoints                { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 4.2 AllocLinkID, tag 196, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      AllocLinkID                      { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 4.2 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   AllocLinkType                    { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.2 SecondaryOrderID, tag 198, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecondaryOrderID                 { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 4.2 NoIOIQualifiers, tag 199, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoIOIQualifiers                  { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 4.2 MaturityMonthYear, tag 200, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.MonthYear, bool> MaturityMonthYear                { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 4.2 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   PutOrCall                        { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.2 StrikePrice, tag 202, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   StrikePrice                      { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 4.2 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   CoveredOrUncovered               { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.2 CustomerOrFirm, tag 204, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   CustomerOrFirm                   { get; set; } = ValidateCustomerOrFirm;

	/// <summary>Holds a FIX 4.2 MaturityDay, tag 205, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MaturityDay                      { get; set; } = ValidateMaturityDay;

	/// <summary>Holds a FIX 4.2 OptAttribute, tag 206, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> OptAttribute                     { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 4.2 SecurityExchange, tag 207, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityExchange                 { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 4.2 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   NotifyBrokerOfCredit             { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 4.2 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   AllocHandlInst                   { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.2 MaxShow, tag 210, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MaxShow                          { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 4.2 PegDifference, tag 211, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   PegDifference                    { get; set; } = ValidatePegDifference;

	/// <summary>Holds a FIX 4.2 XmlDataLen, tag 212, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   XmlDataLen                       { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 4.2 XmlData, tag 213, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      XmlData                          { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 4.2 SettlInstRefID, tag 214, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SettlInstRefID                   { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 4.2 NoRoutingIDs, tag 215, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoRoutingIDs                     { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 4.2 RoutingType, tag 216, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   RoutingType                      { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.2 RoutingID, tag 217, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      RoutingID                        { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 4.2 SpreadToBenchmark, tag 218, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SpreadToBenchmark                { get; set; } = ValidateSpreadToBenchmark;

	/// <summary>Holds a FIX 4.2 Benchmark, tag 219, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> Benchmark                        { get; set; } = ValidateBenchmark;

	/// <summary>Holds a FIX 4.2 CouponRate, tag 223, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   CouponRate                       { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 4.2 ContractMultiplier, tag 231, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   ContractMultiplier               { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 4.2 MDReqID, tag 262, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDReqID                          { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 4.2 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> SubscriptionRequestType          { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.2 MarketDepth, tag 264, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MarketDepth                      { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 4.2 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MDUpdateType                     { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.2 AggregatedBook, tag 266, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   AggregatedBook                   { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 4.2 NoMDEntryTypes, tag 267, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoMDEntryTypes                   { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 4.2 NoMDEntries, tag 268, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoMDEntries                      { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 4.2 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MDEntryType                      { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.2 MDEntryPx, tag 270, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MDEntryPx                        { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 4.2 MDEntrySize, tag 271, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   MDEntrySize                      { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 4.2 MDEntryDate, tag 272, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      MDEntryDate                      { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 4.2 MDEntryTime, tag 273, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Time, bool>      MDEntryTime                      { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 4.2 TickDirection, tag 274, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> TickDirection                    { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.2 MDMkt, tag 275, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDMkt                            { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 4.2 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Multiple, bool>  QuoteCondition                   { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.2 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Multiple, bool>  TradeCondition                   { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.2 MDEntryID, tag 278, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDEntryID                        { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 4.2 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MDUpdateAction                   { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.2 MDEntryRefID, tag 280, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDEntryRefID                     { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 4.2 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MDReqRejReason                   { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.2 MDEntryOriginator, tag 282, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDEntryOriginator                { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 4.2 LocationID, tag 283, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      LocationID                       { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 4.2 DeskID, tag 284, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      DeskID                           { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 4.2 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> DeleteReason                     { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.2 OpenCloseSettleFlag, tag 286, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> OpenCloseSettleFlag              { get; set; } = ValidateOpenCloseSettleFlag;

	/// <summary>Holds a FIX 4.2 SellerDays, tag 287, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SellerDays                       { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 4.2 MDEntryBuyer, tag 288, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDEntryBuyer                     { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 4.2 MDEntrySeller, tag 289, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MDEntrySeller                    { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 4.2 MDEntryPositionNo, tag 290, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MDEntryPositionNo                { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 4.2 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> FinancialStatus                  { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.2 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> CorporateAction                  { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.2 DefBidSize, tag 293, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DefBidSize                       { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 4.2 DefOfferSize, tag 294, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DefOfferSize                     { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 4.2 NoQuoteEntries, tag 295, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoQuoteEntries                   { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 4.2 NoQuoteSets, tag 296, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoQuoteSets                      { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 4.2 QuoteAckStatus, tag 297, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteAckStatus                   { get; set; } = ValidateQuoteAckStatus;

	/// <summary>Holds a FIX 4.2 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteCancelType                  { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.2 QuoteEntryID, tag 299, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      QuoteEntryID                     { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 4.2 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteRejectReason                { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.2 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteResponseLevel               { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.2 QuoteSetID, tag 302, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      QuoteSetID                       { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 4.2 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteRequestType                 { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.2 TotQuoteEntries, tag 304, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TotQuoteEntries                  { get; set; } = ValidateTotQuoteEntries;

	/// <summary>Holds a FIX 4.2 UnderlyingIDSource, tag 305, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingIDSource               { get; set; } = ValidateUnderlyingIDSource;

	/// <summary>Holds a FIX 4.2 UnderlyingIssuer, tag 306, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingIssuer                 { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityDesc           { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityExchange       { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityID             { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 4.2 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSecurityType           { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 4.2 UnderlyingSymbol, tag 311, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSymbol                 { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 4.2 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingSymbolSfx              { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 4.2 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.MonthYear, bool> UnderlyingMaturityMonthYear      { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 4.2 UnderlyingMaturityDay, tag 314, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   UnderlyingMaturityDay            { get; set; } = ValidateUnderlyingMaturityDay;

	/// <summary>Holds a FIX 4.2 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   UnderlyingPutOrCall              { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 4.2 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   UnderlyingStrikePrice            { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 4.2 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> UnderlyingOptAttribute           { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 4.2 UnderlyingCurrency, tag 318, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      UnderlyingCurrency               { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 4.2 RatioQty, tag 319, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   RatioQty                         { get; set; } = ValidateRatioQty;

	/// <summary>Holds a FIX 4.2 SecurityReqID, tag 320, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityReqID                    { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 4.2 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SecurityRequestType              { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.2 SecurityResponseID, tag 322, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityResponseID               { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 4.2 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SecurityResponseType             { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.2 SecurityStatusReqID, tag 324, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      SecurityStatusReqID              { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 4.2 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   UnsolicitedIndicator             { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 4.2 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SecurityTradingStatus            { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.2 HaltReason, tag 327, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> HaltReason                       { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.2 InViewOfCommon, tag 328, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   InViewOfCommon                   { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 4.2 DueToRelated, tag 329, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   DueToRelated                     { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 4.2 BuyVolume, tag 330, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   BuyVolume                        { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 4.2 SellVolume, tag 331, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SellVolume                       { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 4.2 HighPx, tag 332, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   HighPx                           { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 4.2 LowPx, tag 333, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LowPx                            { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 4.2 Adjustment, tag 334, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   Adjustment                       { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.2 TradSesReqID, tag 335, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TradSesReqID                     { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 4.2 TradingSessionID, tag 336, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      TradingSessionID                 { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 4.2 ContraTrader, tag 337, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ContraTrader                     { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 4.2 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TradSesMethod                    { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.2 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TradSesMode                      { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.2 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TradSesStatus                    { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.2 TradSesStartTime, tag 341, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TradSesStartTime                 { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 4.2 TradSesOpenTime, tag 342, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TradSesOpenTime                  { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 4.2 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TradSesPreCloseTime              { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 4.2 TradSesCloseTime, tag 344, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TradSesCloseTime                 { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 4.2 TradSesEndTime, tag 345, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> TradSesEndTime                   { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 4.2 NumberOfOrders, tag 346, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NumberOfOrders                   { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 4.2 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      MessageEncoding                  { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.2 EncodedIssuerLen, tag 348, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedIssuerLen                 { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 4.2 EncodedIssuer, tag 349, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedIssuer                    { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 4.2 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedSecurityDescLen           { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 4.2 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedSecurityDesc              { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 4.2 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedListExecInstLen           { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 4.2 EncodedListExecInst, tag 353, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedListExecInst              { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 4.2 EncodedTextLen, tag 354, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedTextLen                   { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 4.2 EncodedText, tag 355, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedText                      { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 4.2 EncodedSubjectLen, tag 356, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedSubjectLen                { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 4.2 EncodedSubject, tag 357, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedSubject                   { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 4.2 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedHeadlineLen               { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 4.2 EncodedHeadline, tag 359, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedHeadline                  { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 4.2 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedAllocTextLen              { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 4.2 EncodedAllocText, tag 361, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedAllocText                 { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingIssuerLen       { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingIssuer          { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedUnderlyingSecurityDescLen { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 4.2 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedUnderlyingSecurityDesc    { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.2 AllocPrice, tag 366, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   AllocPrice                       { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 4.2 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> QuoteSetValidUntilTime           { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 4.2 QuoteEntryRejectReason, tag 368, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   QuoteEntryRejectReason           { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 4.2 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   LastMsgSeqNumProcessed           { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 4.2 OnBehalfOfSendingTime, tag 370, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> OnBehalfOfSendingTime            { get; set; } = ValidateOnBehalfOfSendingTime;

	/// <summary>Holds a FIX 4.2 RefTagID, tag 371, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   RefTagID                         { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 4.2 RefMsgType, tag 372, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      RefMsgType                       { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 4.2 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SessionRejectReason              { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.2 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> BidRequestTransType              { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.2 ContraBroker, tag 375, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ContraBroker                     { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 4.2 ComplianceID, tag 376, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ComplianceID                     { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 4.2 SolicitedFlag, tag 377, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   SolicitedFlag                    { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 4.2 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ExecRestatementReason            { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.2 BusinessRejectRefID, tag 379, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      BusinessRejectRefID              { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 4.2 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   BusinessRejectReason             { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.2 GrossTradeAmt, tag 381, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   GrossTradeAmt                    { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 4.2 NoContraBrokers, tag 382, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoContraBrokers                  { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 4.2 MaxMessageSize, tag 383, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   MaxMessageSize                   { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 4.2 NoMsgTypes, tag 384, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoMsgTypes                       { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 4.2 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MsgDirection                     { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.2 NoTradingSessions, tag 386, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoTradingSessions                { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 4.2 TotalVolumeTraded, tag 387, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   TotalVolumeTraded                { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 4.2 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> DiscretionInst                   { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.2 DiscretionOffset, tag 389, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DiscretionOffset                 { get; set; } = ValidateDiscretionOffset;

	/// <summary>Holds a FIX 4.2 BidID, tag 390, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      BidID                            { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 4.2 ClientBidID, tag 391, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ClientBidID                      { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 4.2 ListName, tag 392, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ListName                         { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 4.2 TotalNumSecurities, tag 393, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TotalNumSecurities               { get; set; } = ValidateTotalNumSecurities;

	/// <summary>Holds a FIX 4.2 BidType, tag 394, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   BidType                          { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.2 NumTickets, tag 395, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NumTickets                       { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 4.2 SideValue1, tag 396, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SideValue1                       { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 4.2 SideValue2, tag 397, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   SideValue2                       { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 4.2 NoBidDescriptors, tag 398, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoBidDescriptors                 { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 4.2 BidDescriptorType, tag 399, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   BidDescriptorType                { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.2 BidDescriptor, tag 400, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      BidDescriptor                    { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 4.2 SideValueInd, tag 401, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   SideValueInd                     { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.2 LiquidityPctLow, tag 402, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LiquidityPctLow                  { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 4.2 LiquidityPctHigh, tag 403, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LiquidityPctHigh                 { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 4.2 LiquidityValue, tag 404, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   LiquidityValue                   { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 4.2 EFPTrackingError, tag 405, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   EFPTrackingError                 { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 4.2 FairValue, tag 406, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   FairValue                        { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 4.2 OutsideIndexPct, tag 407, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OutsideIndexPct                  { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 4.2 ValueOfFutures, tag 408, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   ValueOfFutures                   { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 4.2 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   LiquidityIndType                 { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.2 WtAverageLiquidity, tag 410, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   WtAverageLiquidity               { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 4.2 ExchangeForPhysical, tag 411, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Boolean, bool>   ExchangeForPhysical              { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 4.2 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   OutMainCntryUIndex               { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 4.2 CrossPercent, tag 413, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   CrossPercent                     { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 4.2 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ProgRptReqs                      { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.2 ProgPeriodInterval, tag 415, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ProgPeriodInterval               { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 4.2 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   IncTaxInd                        { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.2 NumBidders, tag 417, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NumBidders                       { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 4.2 TradeType, tag 418, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> TradeType                        { get; set; } = ValidateTradeType;

	/// <summary>Holds a FIX 4.2 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> BasisPxType                      { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.2 NoBidComponents, tag 420, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoBidComponents                  { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 4.2 Country, tag 421, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      Country                          { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 4.2 TotNoStrikes, tag 422, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   TotNoStrikes                     { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 4.2 PriceType, tag 423, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   PriceType                        { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.2 DayOrderQty, tag 424, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DayOrderQty                      { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 4.2 DayCumQty, tag 425, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DayCumQty                        { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 4.2 DayAvgPx, tag 426, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   DayAvgPx                         { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 4.2 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   GTBookingInst                    { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.2 NoStrikes, tag 428, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NoStrikes                        { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 4.2 ListStatusType, tag 429, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ListStatusType                   { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.2 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   NetGrossInd                      { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.2 ListOrderStatus, tag 431, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   ListOrderStatus                  { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.2 ExpireDate, tag 432, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Date, bool>      ExpireDate                       { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 4.2 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> ListExecInstType                 { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.2 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> CxlRejResponseTo                 { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.2 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   UnderlyingCouponRate             { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 4.2 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   UnderlyingContractMultiplier     { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 4.2 ContraTradeQty, tag 437, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Decimal, bool>   ContraTradeQty                   { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 4.2 ContraTradeTime, tag 438, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> ContraTradeTime                  { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 4.2 ClearingFirm, tag 439, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ClearingFirm                     { get; set; } = ValidateClearingFirm;

	/// <summary>Holds a FIX 4.2 ClearingAccount, tag 440, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ClearingAccount                  { get; set; } = ValidateClearingAccount;

	/// <summary>Holds a FIX 4.2 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   LiquidityNumSecurities           { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 4.2 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Func<Fix42Context, FixMessage, FixField.Character, bool> MultiLegReportingType            { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.2 StrikeTime, tag 443, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Timestamp, bool> StrikeTime                       { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 4.2 ListStatusText, tag 444, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Text, bool>      ListStatusText                   { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 4.2 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Integer, bool>   EncodedListStatusTextLen         { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 4.2 EncodedListStatusText, tag 446, to its type.</summary>
	public Func<Fix42Context, FixMessage, FixField.Data, bool>      EncodedListStatusText            { get; set; } = ValidateEncodedListStatusText;

	static bool ValidateAccount(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvId(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvSide(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvTransType(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("C" or "N" or "R"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginSeqNo(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginString(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBodyLength(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCheckSum(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommission(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCumQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCurrency(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndSeqNo(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecInst(Fix42Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
				"C" or "D" or "E" or "F" or "G" or "I" or "L" or "M" or "N" or "O" or "P" or "R" or "S" or "T" or
				"U" or "V" or "W"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateExecRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecTransType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHandlInst(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIDSource(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIid(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIOthSvc(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQltyInd(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIShares(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("L" or "M" or "S"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOITransType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastCapacity(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMkt(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastShares(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLinesOfText(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgSeqNum(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgType(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or
			"C" or "D" or "E" or "F" or "G" or "H" or "J" or "K" or "L" or "M" or "N" or "P" or "Q" or "R" or "S" or
			"T" or "V" or "W" or "X" or "Y" or "Z" or "a" or "b" or "c" or "d" or "e" or "f" or "g" or "h" or "i" or
			"j" or "k" or "l" or "m"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewSeqNo(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatus(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C' or 'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I' or 'P'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigClOrdID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossDupFlag(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSeqNum(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRelatdSym(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRule80A(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'H' or 'I' or 'J' or 'K' or 'L' or 'M' or
			'N' or 'O' or 'P' or 'R' or 'S' or 'T' or 'U' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderCompID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderSubID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSendingDate(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSendingTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShares(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSide(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbol(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetCompID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetSubID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateText(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeInForce(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransactTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUrgency(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValidUntilTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlmntTyp(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFutSettDate(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbolSfx(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListSeqNo(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoOrders(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInst(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocTransType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefAllocID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOrders(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPrxPrecision(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeDate(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecBroker(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenClose(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'O'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAllocs(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccount(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocShares(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProcessCode(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRpts(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRptSeq(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDlvyInst(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDlvyInst(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocStatus(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocRejCode(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignature(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureDataLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureData(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBrokerOfCredit(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignatureLength(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawDataLength(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawData(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossResend(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptMethod(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStopPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDestination(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdRejReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQualifier(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'C' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or 'S' or 'T' or 'V' or
			'W' or 'X' or 'Y' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWaveNo(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssuer(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityDesc(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeartBtInt(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClientID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxFloor(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportToExch(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocateReqd(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfCompID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfSubID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetMoney(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrAmt(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrency(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateForexReq(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigSendingTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGapFillFlag(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExecs(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDKReason(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToCompID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToSubID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOINaturalFlag(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSize(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSize(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMiscFees(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeAmt(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeCurr(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrevClosePx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResetSeqNumFlag(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderLocationID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetLocationID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfLocationID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToLocationID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRelatedSym(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubject(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeadline(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateURLLink(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C' or 'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLeavesQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOrderQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAvgPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNetMoney(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRateCalc(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumDaysInterest(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestAmt(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMode(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocText(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstTransType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailThreadID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstSource(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlLocation(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("CED" or "DTC" or "EUR" or "FED" or "ISO Country Code" or "PNY" or "PTC"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityType(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("?" or "BA" or "CB" or "CD" or "CMO" or "CORP" or "CP" or "CPP" or "CS" or "FHA" or
			"FHL" or "FN" or "FOR" or "FUT" or "GN" or "GOVT" or "IET" or "MF" or "MIO" or "MPO" or "MPP" or
			"MPT" or "MUNI" or "NONE" or "OPT" or "PS" or "RP" or "RVRP" or "SL" or "TD" or "USTB" or "WAR" or "ZOO"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEffectiveTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDeliveryType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDepositoryCode(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlBrkrCode(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstCode(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentCode(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentAcctNum(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentAcctName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentContactName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySettlAgentContactPhone(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentCode(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentAcctNum(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentAcctName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentContactName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashSettlAgentContactPhone(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSpotRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSpotRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty2(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFutSettDate2(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastSpotRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryOrderID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoIOIQualifiers(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYear(Fix42Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePutOrCall(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePrice(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCoveredOrUncovered(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustomerOrFirm(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityDay(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptAttribute(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityExchange(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotifyBrokerOfCredit(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocHandlInst(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxShow(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegDifference(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlDataLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlData(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRoutingIDs(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSpreadToBenchmark(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmark(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractMultiplier(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubscriptionRequestType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketDepth(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAggregatedBook(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntryTypes(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntries(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySize(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryDate(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryTime(Fix42Context context, FixMessage message, FixField.Time field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickDirection(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDMkt(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCondition(Fix42Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateTradeCondition(Fix42Context context, FixMessage message, FixField.Multiple field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I" or "J" or "K" or "L" or
				"M" or "N"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDEntryID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateAction(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqRejReason(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryOriginator(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocationID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeleteReason(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenCloseSettleFlag(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSellerDays(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryBuyer(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySeller(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPositionNo(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFinancialStatus(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCorporateAction(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefBidSize(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefOfferSize(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteEntries(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteSets(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteAckStatus(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCancelType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRejectReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteResponseLevel(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotQuoteEntries(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIDSource(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssuer(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityDesc(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityExchange(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityType(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbol(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbolSfx(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityMonthYear(Fix42Context context, FixMessage message, FixField.MonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityDay(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPutOrCall(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikePrice(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingOptAttribute(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrency(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRatioQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnsolicitedIndicator(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingStatus(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or 20 or 3 or 4 or
			5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHaltReason(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInViewOfCommon(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDueToRelated(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBuyVolume(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSellVolume(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHighPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLowPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustment(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesReqID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTrader(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMethod(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMode(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatus(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStartTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesOpenTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesPreCloseTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesCloseTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesEndTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumberOfOrders(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEncoding(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuerLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuer(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDescLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDesc(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInstLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInst(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedTextLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedText(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubjectLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubject(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadlineLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadline(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocTextLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocText(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuerLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuer(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDescLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDesc(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocPrice(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetValidUntilTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryRejectReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMsgSeqNumProcessed(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfSendingTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefTagID(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefMsgType(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionRejectReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidRequestTransType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraBroker(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplianceID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSolicitedFlag(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecRestatementReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectRefID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectReason(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGrossTradeAmt(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContraBrokers(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxMessageSize(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMsgTypes(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgDirection(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTradingSessions(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalVolumeTraded(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionInst(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffset(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClientBidID(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListName(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNumSecurities(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumTickets(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue1(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue2(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidDescriptors(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptorType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptor(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValueInd(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctLow(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctHigh(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityValue(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEFPTrackingError(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFairValue(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutsideIndexPct(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValueOfFutures(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityIndType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWtAverageLiquidity(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeForPhysical(Fix42Context context, FixMessage message, FixField.Boolean field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutMainCntryUIndex(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPercent(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgRptReqs(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgPeriodInterval(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIncTaxInd(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumBidders(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisPxType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidComponents(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountry(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoStrikes(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayOrderQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayCumQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayAvgPx(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGTBookingInst(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrikes(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusType(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetGrossInd(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListOrderStatus(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireDate(Fix42Context context, FixMessage message, FixField.Date field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInstType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejResponseTo(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponRate(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingContractMultiplier(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeQty(Fix42Context context, FixMessage message, FixField.Decimal field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingFirm(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingAccount(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityNumSecurities(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegReportingType(Fix42Context context, FixMessage message, FixField.Character field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeTime(Fix42Context context, FixMessage message, FixField.Timestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusText(Fix42Context context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusTextLen(Fix42Context context, FixMessage message, FixField.Integer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusText(Fix42Context context, FixMessage message, FixField.Data field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}
}
