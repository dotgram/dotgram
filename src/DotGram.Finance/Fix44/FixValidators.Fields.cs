using System;

namespace DotGram.Finance.Fix44;

/// <summary>The check of every field of FIX 4.4 against its type, and against the values the repository lists for it.</summary>
partial class FixValidators
{
	// The fields, every one, so that a dictionary has a slot for whatever it limits.

	/// <summary>Holds a FIX 4.4 Account, tag 1, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Account, bool> Account                                 { get; set; } = ValidateAccount;

	/// <summary>Holds a FIX 4.4 AdvId, tag 2, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AdvId, bool> AdvId                                   { get; set; } = ValidateAdvId;

	/// <summary>Holds a FIX 4.4 AdvRefID, tag 3, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AdvRefID, bool> AdvRefID                                { get; set; } = ValidateAdvRefID;

	/// <summary>Holds a FIX 4.4 AdvSide, tag 4, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdvSide, bool> AdvSide                                 { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.4 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdvTransType, bool> AdvTransType                            { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.4 AvgPx, tag 6, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AvgPx, bool> AvgPx                                   { get; set; } = ValidateAvgPx;

	/// <summary>Holds a FIX 4.4 BeginSeqNo, tag 7, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BeginSeqNo, bool> BeginSeqNo                              { get; set; } = ValidateBeginSeqNo;

	/// <summary>Holds a FIX 4.4 BeginString, tag 8, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BeginString, bool> BeginString                             { get; set; } = ValidateBeginString;

	/// <summary>Holds a FIX 4.4 BodyLength, tag 9, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BodyLength, bool> BodyLength                              { get; set; } = ValidateBodyLength;

	/// <summary>Holds a FIX 4.4 CheckSum, tag 10, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CheckSum, bool> CheckSum                                { get; set; } = ValidateCheckSum;

	/// <summary>Holds a FIX 4.4 ClOrdID, tag 11, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ClOrdID, bool> ClOrdID                                 { get; set; } = ValidateClOrdID;

	/// <summary>Holds a FIX 4.4 Commission, tag 12, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Commission, bool> Commission                              { get; set; } = ValidateCommission;

	/// <summary>Holds a FIX 4.4 CommType, tag 13, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CommType, bool> CommType                                { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.4 CumQty, tag 14, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CumQty, bool> CumQty                                  { get; set; } = ValidateCumQty;

	/// <summary>Holds a FIX 4.4 Currency, tag 15, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Currency, bool> Currency                                { get; set; } = ValidateCurrency;

	/// <summary>Holds a FIX 4.4 EndSeqNo, tag 16, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EndSeqNo, bool> EndSeqNo                                { get; set; } = ValidateEndSeqNo;

	/// <summary>Holds a FIX 4.4 ExecID, tag 17, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExecID, bool> ExecID                                  { get; set; } = ValidateExecID;

	/// <summary>Holds a FIX 4.4 ExecInst, tag 18, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecInst, bool> ExecInst                                { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.4 ExecRefID, tag 19, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExecRefID, bool> ExecRefID                               { get; set; } = ValidateExecRefID;

	/// <summary>Holds a FIX 4.4 HandlInst, tag 21, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.HandlInst, bool> HandlInst                               { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.4 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityIDSource, bool> SecurityIDSource                        { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 4.4 IOIid, tag 23, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IOIid, bool> IOIid                                   { get; set; } = ValidateIOIid;

	/// <summary>Holds a FIX 4.4 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQltyInd, bool> IOIQltyInd                              { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.4 IOIRefID, tag 26, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IOIRefID, bool> IOIRefID                                { get; set; } = ValidateIOIRefID;

	/// <summary>Holds a FIX 4.4 IOIQty, tag 27, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQty, bool> IOIQty                                  { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 4.4 IOITransType, tag 28, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOITransType, bool> IOITransType                            { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.4 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LastCapacity, bool> LastCapacity                            { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.4 LastMkt, tag 30, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastMkt, bool> LastMkt                                 { get; set; } = ValidateLastMkt;

	/// <summary>Holds a FIX 4.4 LastPx, tag 31, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastPx, bool> LastPx                                  { get; set; } = ValidateLastPx;

	/// <summary>Holds a FIX 4.4 LastQty, tag 32, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastQty, bool> LastQty                                 { get; set; } = ValidateLastQty;

	/// <summary>Holds a FIX 4.4 LinesOfText, tag 33, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LinesOfText, bool> LinesOfText                           { get; set; } = ValidateLinesOfText;

	/// <summary>Holds a FIX 4.4 MsgSeqNum, tag 34, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MsgSeqNum, bool> MsgSeqNum                               { get; set; } = ValidateMsgSeqNum;

	/// <summary>Holds a FIX 4.4 MsgType, tag 35, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MsgType, bool> MsgType                                 { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.4 NewSeqNo, tag 36, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NewSeqNo, bool> NewSeqNo                                { get; set; } = ValidateNewSeqNo;

	/// <summary>Holds a FIX 4.4 OrderID, tag 37, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderID, bool> OrderID                                 { get; set; } = ValidateOrderID;

	/// <summary>Holds a FIX 4.4 OrderQty, tag 38, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderQty, bool> OrderQty                                { get; set; } = ValidateOrderQty;

	/// <summary>Holds a FIX 4.4 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdStatus, bool> OrdStatus                               { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.4 OrdType, tag 40, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdType, bool> OrdType                                 { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.4 OrigClOrdID, tag 41, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigClOrdID, bool> OrigClOrdID                             { get; set; } = ValidateOrigClOrdID;

	/// <summary>Holds a FIX 4.4 OrigTime, tag 42, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigTime, bool> OrigTime                                { get; set; } = ValidateOrigTime;

	/// <summary>Holds a FIX 4.4 PossDupFlag, tag 43, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PossDupFlag, bool> PossDupFlag                             { get; set; } = ValidatePossDupFlag;

	/// <summary>Holds a FIX 4.4 Price, tag 44, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Price, bool> Price                                   { get; set; } = ValidatePrice;

	/// <summary>Holds a FIX 4.4 RefSeqNum, tag 45, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefSeqNum, bool> RefSeqNum                               { get; set; } = ValidateRefSeqNum;

	/// <summary>Holds a FIX 4.4 SecurityID, tag 48, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityID, bool> SecurityID                              { get; set; } = ValidateSecurityID;

	/// <summary>Holds a FIX 4.4 SenderCompID, tag 49, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SenderCompID, bool> SenderCompID                            { get; set; } = ValidateSenderCompID;

	/// <summary>Holds a FIX 4.4 SenderSubID, tag 50, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SenderSubID, bool> SenderSubID                             { get; set; } = ValidateSenderSubID;

	/// <summary>Holds a FIX 4.4 SendingTime, tag 52, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SendingTime, bool> SendingTime                             { get; set; } = ValidateSendingTime;

	/// <summary>Holds a FIX 4.4 Quantity, tag 53, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Quantity, bool> Quantity                                { get; set; } = ValidateQuantity;

	/// <summary>Holds a FIX 4.4 Side, tag 54, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Side, bool> Side                                    { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.4 Symbol, tag 55, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Symbol, bool> Symbol                                  { get; set; } = ValidateSymbol;

	/// <summary>Holds a FIX 4.4 TargetCompID, tag 56, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TargetCompID, bool> TargetCompID                            { get; set; } = ValidateTargetCompID;

	/// <summary>Holds a FIX 4.4 TargetSubID, tag 57, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TargetSubID, bool> TargetSubID                             { get; set; } = ValidateTargetSubID;

	/// <summary>Holds a FIX 4.4 Text, tag 58, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Text, bool> Text                                    { get; set; } = ValidateText;

	/// <summary>Holds a FIX 4.4 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TimeInForce, bool> TimeInForce                             { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.4 TransactTime, tag 60, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TransactTime, bool> TransactTime                            { get; set; } = ValidateTransactTime;

	/// <summary>Holds a FIX 4.4 Urgency, tag 61, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Urgency, bool> Urgency                                 { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.4 ValidUntilTime, tag 62, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ValidUntilTime, bool> ValidUntilTime                          { get; set; } = ValidateValidUntilTime;

	/// <summary>Holds a FIX 4.4 SettlType, tag 63, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlType, bool> SettlType                               { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 4.4 SettlDate, tag 64, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlDate, bool> SettlDate                               { get; set; } = ValidateSettlDate;

	/// <summary>Holds a FIX 4.4 SymbolSfx, tag 65, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SymbolSfx, bool> SymbolSfx                               { get; set; } = ValidateSymbolSfx;

	/// <summary>Holds a FIX 4.4 ListID, tag 66, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ListID, bool> ListID                                  { get; set; } = ValidateListID;

	/// <summary>Holds a FIX 4.4 ListSeqNo, tag 67, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ListSeqNo, bool> ListSeqNo                               { get; set; } = ValidateListSeqNo;

	/// <summary>Holds a FIX 4.4 TotNoOrders, tag 68, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoOrders, bool> TotNoOrders                             { get; set; } = ValidateTotNoOrders;

	/// <summary>Holds a FIX 4.4 ListExecInst, tag 69, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ListExecInst, bool> ListExecInst                            { get; set; } = ValidateListExecInst;

	/// <summary>Holds a FIX 4.4 AllocID, tag 70, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocID, bool> AllocID                                 { get; set; } = ValidateAllocID;

	/// <summary>Holds a FIX 4.4 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocTransType, bool> AllocTransType                          { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.4 RefAllocID, tag 72, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefAllocID, bool> RefAllocID                              { get; set; } = ValidateRefAllocID;

	/// <summary>Holds a FIX 4.4 NoOrders, tag 73, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoOrders, bool> NoOrders                                { get; set; } = ValidateNoOrders;

	/// <summary>Holds a FIX 4.4 AvgPxPrecision, tag 74, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AvgPxPrecision, bool> AvgPxPrecision                          { get; set; } = ValidateAvgPxPrecision;

	/// <summary>Holds a FIX 4.4 TradeDate, tag 75, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeDate, bool> TradeDate                               { get; set; } = ValidateTradeDate;

	/// <summary>Holds a FIX 4.4 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PositionEffect, bool> PositionEffect                          { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 4.4 NoAllocs, tag 78, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoAllocs, bool> NoAllocs                                { get; set; } = ValidateNoAllocs;

	/// <summary>Holds a FIX 4.4 AllocAccount, tag 79, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAccount, bool> AllocAccount                            { get; set; } = ValidateAllocAccount;

	/// <summary>Holds a FIX 4.4 AllocQty, tag 80, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocQty, bool> AllocQty                                { get; set; } = ValidateAllocQty;

	/// <summary>Holds a FIX 4.4 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ProcessCode, bool> ProcessCode                             { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.4 NoRpts, tag 82, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoRpts, bool> NoRpts                                  { get; set; } = ValidateNoRpts;

	/// <summary>Holds a FIX 4.4 RptSeq, tag 83, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RptSeq, bool> RptSeq                                  { get; set; } = ValidateRptSeq;

	/// <summary>Holds a FIX 4.4 CxlQty, tag 84, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CxlQty, bool> CxlQty                                  { get; set; } = ValidateCxlQty;

	/// <summary>Holds a FIX 4.4 NoDlvyInst, tag 85, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoDlvyInst, bool> NoDlvyInst                              { get; set; } = ValidateNoDlvyInst;

	/// <summary>Holds a FIX 4.4 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocStatus, bool> AllocStatus                             { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.4 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocRejCode, bool> AllocRejCode                            { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.4 Signature, tag 89, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Signature, bool> Signature                               { get; set; } = ValidateSignature;

	/// <summary>Holds a FIX 4.4 SecureDataLen, tag 90, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecureDataLen, bool> SecureDataLen                           { get; set; } = ValidateSecureDataLen;

	/// <summary>Holds a FIX 4.4 SecureData, tag 91, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecureData, bool> SecureData                              { get; set; } = ValidateSecureData;

	/// <summary>Holds a FIX 4.4 SignatureLength, tag 93, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SignatureLength, bool> SignatureLength                         { get; set; } = ValidateSignatureLength;

	/// <summary>Holds a FIX 4.4 EmailType, tag 94, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EmailType, bool> EmailType                               { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.4 RawDataLength, tag 95, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RawDataLength, bool> RawDataLength                           { get; set; } = ValidateRawDataLength;

	/// <summary>Holds a FIX 4.4 RawData, tag 96, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RawData, bool> RawData                                 { get; set; } = ValidateRawData;

	/// <summary>Holds a FIX 4.4 PossResend, tag 97, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PossResend, bool> PossResend                              { get; set; } = ValidatePossResend;

	/// <summary>Holds a FIX 4.4 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EncryptMethod, bool> EncryptMethod                           { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.4 StopPx, tag 99, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StopPx, bool> StopPx                                  { get; set; } = ValidateStopPx;

	/// <summary>Holds a FIX 4.4 ExDestination, tag 100, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExDestination, bool> ExDestination                           { get; set; } = ValidateExDestination;

	/// <summary>Holds a FIX 4.4 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CxlRejReason, bool> CxlRejReason                            { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.4 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdRejReason, bool> OrdRejReason                            { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.4 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQualifier, bool> IOIQualifier                            { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.4 Issuer, tag 106, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Issuer, bool> Issuer                                  { get; set; } = ValidateIssuer;

	/// <summary>Holds a FIX 4.4 SecurityDesc, tag 107, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityDesc, bool> SecurityDesc                            { get; set; } = ValidateSecurityDesc;

	/// <summary>Holds a FIX 4.4 HeartBtInt, tag 108, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.HeartBtInt, bool> HeartBtInt                              { get; set; } = ValidateHeartBtInt;

	/// <summary>Holds a FIX 4.4 MinQty, tag 110, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MinQty, bool> MinQty                                  { get; set; } = ValidateMinQty;

	/// <summary>Holds a FIX 4.4 MaxFloor, tag 111, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaxFloor, bool> MaxFloor                                { get; set; } = ValidateMaxFloor;

	/// <summary>Holds a FIX 4.4 TestReqID, tag 112, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TestReqID, bool> TestReqID                               { get; set; } = ValidateTestReqID;

	/// <summary>Holds a FIX 4.4 ReportToExch, tag 113, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ReportToExch, bool> ReportToExch                            { get; set; } = ValidateReportToExch;

	/// <summary>Holds a FIX 4.4 LocateReqd, tag 114, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LocateReqd, bool> LocateReqd                              { get; set; } = ValidateLocateReqd;

	/// <summary>Holds a FIX 4.4 OnBehalfOfCompID, tag 115, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OnBehalfOfCompID, bool> OnBehalfOfCompID                        { get; set; } = ValidateOnBehalfOfCompID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfSubID, tag 116, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OnBehalfOfSubID, bool> OnBehalfOfSubID                         { get; set; } = ValidateOnBehalfOfSubID;

	/// <summary>Holds a FIX 4.4 QuoteID, tag 117, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteID, bool> QuoteID                                 { get; set; } = ValidateQuoteID;

	/// <summary>Holds a FIX 4.4 NetMoney, tag 118, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NetMoney, bool> NetMoney                                { get; set; } = ValidateNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrAmt, tag 119, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrAmt, bool> SettlCurrAmt                            { get; set; } = ValidateSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 SettlCurrency, tag 120, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrency, bool> SettlCurrency                           { get; set; } = ValidateSettlCurrency;

	/// <summary>Holds a FIX 4.4 ForexReq, tag 121, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ForexReq, bool> ForexReq                                { get; set; } = ValidateForexReq;

	/// <summary>Holds a FIX 4.4 OrigSendingTime, tag 122, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigSendingTime, bool> OrigSendingTime                         { get; set; } = ValidateOrigSendingTime;

	/// <summary>Holds a FIX 4.4 GapFillFlag, tag 123, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.GapFillFlag, bool> GapFillFlag                             { get; set; } = ValidateGapFillFlag;

	/// <summary>Holds a FIX 4.4 NoExecs, tag 124, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoExecs, bool> NoExecs                                 { get; set; } = ValidateNoExecs;

	/// <summary>Holds a FIX 4.4 ExpireTime, tag 126, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExpireTime, bool> ExpireTime                              { get; set; } = ValidateExpireTime;

	/// <summary>Holds a FIX 4.4 DKReason, tag 127, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DKReason, bool> DKReason                                { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.4 DeliverToCompID, tag 128, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DeliverToCompID, bool> DeliverToCompID                         { get; set; } = ValidateDeliverToCompID;

	/// <summary>Holds a FIX 4.4 DeliverToSubID, tag 129, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DeliverToSubID, bool> DeliverToSubID                          { get; set; } = ValidateDeliverToSubID;

	/// <summary>Holds a FIX 4.4 IOINaturalFlag, tag 130, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IOINaturalFlag, bool> IOINaturalFlag                          { get; set; } = ValidateIOINaturalFlag;

	/// <summary>Holds a FIX 4.4 QuoteReqID, tag 131, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteReqID, bool> QuoteReqID                              { get; set; } = ValidateQuoteReqID;

	/// <summary>Holds a FIX 4.4 BidPx, tag 132, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidPx, bool> BidPx                                   { get; set; } = ValidateBidPx;

	/// <summary>Holds a FIX 4.4 OfferPx, tag 133, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferPx, bool> OfferPx                                 { get; set; } = ValidateOfferPx;

	/// <summary>Holds a FIX 4.4 BidSize, tag 134, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidSize, bool> BidSize                                 { get; set; } = ValidateBidSize;

	/// <summary>Holds a FIX 4.4 OfferSize, tag 135, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferSize, bool> OfferSize                               { get; set; } = ValidateOfferSize;

	/// <summary>Holds a FIX 4.4 NoMiscFees, tag 136, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoMiscFees, bool> NoMiscFees                              { get; set; } = ValidateNoMiscFees;

	/// <summary>Holds a FIX 4.4 MiscFeeAmt, tag 137, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeAmt, bool> MiscFeeAmt                              { get; set; } = ValidateMiscFeeAmt;

	/// <summary>Holds a FIX 4.4 MiscFeeCurr, tag 138, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeCurr, bool> MiscFeeCurr                             { get; set; } = ValidateMiscFeeCurr;

	/// <summary>Holds a FIX 4.4 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeType, bool> MiscFeeType                             { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.4 PrevClosePx, tag 140, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PrevClosePx, bool> PrevClosePx                             { get; set; } = ValidatePrevClosePx;

	/// <summary>Holds a FIX 4.4 ResetSeqNumFlag, tag 141, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ResetSeqNumFlag, bool> ResetSeqNumFlag                         { get; set; } = ValidateResetSeqNumFlag;

	/// <summary>Holds a FIX 4.4 SenderLocationID, tag 142, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SenderLocationID, bool> SenderLocationID                        { get; set; } = ValidateSenderLocationID;

	/// <summary>Holds a FIX 4.4 TargetLocationID, tag 143, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TargetLocationID, bool> TargetLocationID                        { get; set; } = ValidateTargetLocationID;

	/// <summary>Holds a FIX 4.4 OnBehalfOfLocationID, tag 144, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OnBehalfOfLocationID, bool> OnBehalfOfLocationID                    { get; set; } = ValidateOnBehalfOfLocationID;

	/// <summary>Holds a FIX 4.4 DeliverToLocationID, tag 145, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DeliverToLocationID, bool> DeliverToLocationID                     { get; set; } = ValidateDeliverToLocationID;

	/// <summary>Holds a FIX 4.4 NoRelatedSym, tag 146, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoRelatedSym, bool> NoRelatedSym                            { get; set; } = ValidateNoRelatedSym;

	/// <summary>Holds a FIX 4.4 Subject, tag 147, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Subject, bool> Subject                                 { get; set; } = ValidateSubject;

	/// <summary>Holds a FIX 4.4 Headline, tag 148, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Headline, bool> Headline                                { get; set; } = ValidateHeadline;

	/// <summary>Holds a FIX 4.4 URLLink, tag 149, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.URLLink, bool> URLLink                                 { get; set; } = ValidateURLLink;

	/// <summary>Holds a FIX 4.4 ExecType, tag 150, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecType, bool> ExecType                                { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.4 LeavesQty, tag 151, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LeavesQty, bool> LeavesQty                               { get; set; } = ValidateLeavesQty;

	/// <summary>Holds a FIX 4.4 CashOrderQty, tag 152, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashOrderQty, bool> CashOrderQty                            { get; set; } = ValidateCashOrderQty;

	/// <summary>Holds a FIX 4.4 AllocAvgPx, tag 153, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAvgPx, bool> AllocAvgPx                              { get; set; } = ValidateAllocAvgPx;

	/// <summary>Holds a FIX 4.4 AllocNetMoney, tag 154, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocNetMoney, bool> AllocNetMoney                           { get; set; } = ValidateAllocNetMoney;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRate, tag 155, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrFxRate, bool> SettlCurrFxRate                         { get; set; } = ValidateSettlCurrFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrFxRateCalc, bool> SettlCurrFxRateCalc                     { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.4 NumDaysInterest, tag 157, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NumDaysInterest, bool> NumDaysInterest                         { get; set; } = ValidateNumDaysInterest;

	/// <summary>Holds a FIX 4.4 AccruedInterestRate, tag 158, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AccruedInterestRate, bool> AccruedInterestRate                     { get; set; } = ValidateAccruedInterestRate;

	/// <summary>Holds a FIX 4.4 AccruedInterestAmt, tag 159, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AccruedInterestAmt, bool> AccruedInterestAmt                      { get; set; } = ValidateAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstMode, bool> SettlInstMode                           { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.4 AllocText, tag 161, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocText, bool> AllocText                               { get; set; } = ValidateAllocText;

	/// <summary>Holds a FIX 4.4 SettlInstID, tag 162, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstID, bool> SettlInstID                             { get; set; } = ValidateSettlInstID;

	/// <summary>Holds a FIX 4.4 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstTransType, bool> SettlInstTransType                      { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.4 EmailThreadID, tag 164, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EmailThreadID, bool> EmailThreadID                           { get; set; } = ValidateEmailThreadID;

	/// <summary>Holds a FIX 4.4 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstSource, bool> SettlInstSource                         { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.4 SecurityType, tag 167, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityType, bool> SecurityType                            { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.4 EffectiveTime, tag 168, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EffectiveTime, bool> EffectiveTime                           { get; set; } = ValidateEffectiveTime;

	/// <summary>Holds a FIX 4.4 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StandInstDbType, bool> StandInstDbType                         { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.4 StandInstDbName, tag 170, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StandInstDbName, bool> StandInstDbName                         { get; set; } = ValidateStandInstDbName;

	/// <summary>Holds a FIX 4.4 StandInstDbID, tag 171, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StandInstDbID, bool> StandInstDbID                           { get; set; } = ValidateStandInstDbID;

	/// <summary>Holds a FIX 4.4 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlDeliveryType, bool> SettlDeliveryType                       { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.4 BidSpotRate, tag 188, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidSpotRate, bool> BidSpotRate                             { get; set; } = ValidateBidSpotRate;

	/// <summary>Holds a FIX 4.4 BidForwardPoints, tag 189, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidForwardPoints, bool> BidForwardPoints                        { get; set; } = ValidateBidForwardPoints;

	/// <summary>Holds a FIX 4.4 OfferSpotRate, tag 190, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferSpotRate, bool> OfferSpotRate                           { get; set; } = ValidateOfferSpotRate;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints, tag 191, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferForwardPoints, bool> OfferForwardPoints                      { get; set; } = ValidateOfferForwardPoints;

	/// <summary>Holds a FIX 4.4 OrderQty2, tag 192, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderQty2, bool> OrderQty2                               { get; set; } = ValidateOrderQty2;

	/// <summary>Holds a FIX 4.4 SettlDate2, tag 193, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlDate2, bool> SettlDate2                              { get; set; } = ValidateSettlDate2;

	/// <summary>Holds a FIX 4.4 LastSpotRate, tag 194, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastSpotRate, bool> LastSpotRate                            { get; set; } = ValidateLastSpotRate;

	/// <summary>Holds a FIX 4.4 LastForwardPoints, tag 195, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastForwardPoints, bool> LastForwardPoints                       { get; set; } = ValidateLastForwardPoints;

	/// <summary>Holds a FIX 4.4 AllocLinkID, tag 196, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocLinkID, bool> AllocLinkID                             { get; set; } = ValidateAllocLinkID;

	/// <summary>Holds a FIX 4.4 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocLinkType, bool> AllocLinkType                           { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.4 SecondaryOrderID, tag 198, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryOrderID, bool> SecondaryOrderID                        { get; set; } = ValidateSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 NoIOIQualifiers, tag 199, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoIOIQualifiers, bool> NoIOIQualifiers                         { get; set; } = ValidateNoIOIQualifiers;

	/// <summary>Holds a FIX 4.4 MaturityMonthYear, tag 200, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaturityMonthYear, bool> MaturityMonthYear                       { get; set; } = ValidateMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PutOrCall, bool> PutOrCall                               { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.4 StrikePrice, tag 202, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StrikePrice, bool> StrikePrice                             { get; set; } = ValidateStrikePrice;

	/// <summary>Holds a FIX 4.4 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CoveredOrUncovered, bool> CoveredOrUncovered                      { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 OptAttribute, tag 206, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OptAttribute, bool> OptAttribute                            { get; set; } = ValidateOptAttribute;

	/// <summary>Holds a FIX 4.4 SecurityExchange, tag 207, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityExchange, bool> SecurityExchange                        { get; set; } = ValidateSecurityExchange;

	/// <summary>Holds a FIX 4.4 NotifyBrokerOfCredit, tag 208, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NotifyBrokerOfCredit, bool> NotifyBrokerOfCredit                    { get; set; } = ValidateNotifyBrokerOfCredit;

	/// <summary>Holds a FIX 4.4 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocHandlInst, bool> AllocHandlInst                          { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.4 MaxShow, tag 210, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaxShow, bool> MaxShow                                 { get; set; } = ValidateMaxShow;

	/// <summary>Holds a FIX 4.4 PegOffsetValue, tag 211, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PegOffsetValue, bool> PegOffsetValue                          { get; set; } = ValidatePegOffsetValue;

	/// <summary>Holds a FIX 4.4 XmlDataLen, tag 212, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.XmlDataLen, bool> XmlDataLen                              { get; set; } = ValidateXmlDataLen;

	/// <summary>Holds a FIX 4.4 XmlData, tag 213, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.XmlData, bool> XmlData                                 { get; set; } = ValidateXmlData;

	/// <summary>Holds a FIX 4.4 SettlInstRefID, tag 214, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstRefID, bool> SettlInstRefID                          { get; set; } = ValidateSettlInstRefID;

	/// <summary>Holds a FIX 4.4 NoRoutingIDs, tag 215, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoRoutingIDs, bool> NoRoutingIDs                            { get; set; } = ValidateNoRoutingIDs;

	/// <summary>Holds a FIX 4.4 RoutingType, tag 216, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RoutingType, bool> RoutingType                             { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.4 RoutingID, tag 217, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RoutingID, bool> RoutingID                               { get; set; } = ValidateRoutingID;

	/// <summary>Holds a FIX 4.4 Spread, tag 218, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Spread, bool> Spread                                  { get; set; } = ValidateSpread;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveCurrency, tag 220, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkCurveCurrency, bool> BenchmarkCurveCurrency                  { get; set; } = ValidateBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 BenchmarkCurveName, tag 221, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkCurveName, bool> BenchmarkCurveName                      { get; set; } = ValidateBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 BenchmarkCurvePoint, tag 222, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkCurvePoint, bool> BenchmarkCurvePoint                     { get; set; } = ValidateBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 CouponRate, tag 223, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CouponRate, bool> CouponRate                              { get; set; } = ValidateCouponRate;

	/// <summary>Holds a FIX 4.4 CouponPaymentDate, tag 224, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CouponPaymentDate, bool> CouponPaymentDate                       { get; set; } = ValidateCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 IssueDate, tag 225, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IssueDate, bool> IssueDate                               { get; set; } = ValidateIssueDate;

	/// <summary>Holds a FIX 4.4 RepurchaseTerm, tag 226, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RepurchaseTerm, bool> RepurchaseTerm                          { get; set; } = ValidateRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 RepurchaseRate, tag 227, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RepurchaseRate, bool> RepurchaseRate                          { get; set; } = ValidateRepurchaseRate;

	/// <summary>Holds a FIX 4.4 Factor, tag 228, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Factor, bool> Factor                                  { get; set; } = ValidateFactor;

	/// <summary>Holds a FIX 4.4 TradeOriginationDate, tag 229, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeOriginationDate, bool> TradeOriginationDate                    { get; set; } = ValidateTradeOriginationDate;

	/// <summary>Holds a FIX 4.4 ExDate, tag 230, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExDate, bool> ExDate                                  { get; set; } = ValidateExDate;

	/// <summary>Holds a FIX 4.4 ContractMultiplier, tag 231, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContractMultiplier, bool> ContractMultiplier                      { get; set; } = ValidateContractMultiplier;

	/// <summary>Holds a FIX 4.4 NoStipulations, tag 232, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoStipulations, bool> NoStipulations                          { get; set; } = ValidateNoStipulations;

	/// <summary>Holds a FIX 4.4 StipulationType, tag 233, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StipulationType, bool> StipulationType                         { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 4.4 StipulationValue, tag 234, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StipulationValue, bool> StipulationValue                        { get; set; } = ValidateStipulationValue;

	/// <summary>Holds a FIX 4.4 YieldType, tag 235, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.YieldType, bool> YieldType                               { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 4.4 Yield, tag 236, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Yield, bool> Yield                                   { get; set; } = ValidateYield;

	/// <summary>Holds a FIX 4.4 TotalTakedown, tag 237, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalTakedown, bool> TotalTakedown                           { get; set; } = ValidateTotalTakedown;

	/// <summary>Holds a FIX 4.4 Concession, tag 238, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Concession, bool> Concession                              { get; set; } = ValidateConcession;

	/// <summary>Holds a FIX 4.4 RepoCollateralSecurityType, tag 239, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RepoCollateralSecurityType, bool> RepoCollateralSecurityType              { get; set; } = ValidateRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 RedemptionDate, tag 240, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RedemptionDate, bool> RedemptionDate                          { get; set; } = ValidateRedemptionDate;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponPaymentDate, tag 241, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCouponPaymentDate, bool> UnderlyingCouponPaymentDate             { get; set; } = ValidateUnderlyingCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 UnderlyingIssueDate, tag 242, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingIssueDate, bool> UnderlyingIssueDate                     { get; set; } = ValidateUnderlyingIssueDate;

	/// <summary>Holds a FIX 4.4 UnderlyingRepoCollateralSecurityType, tag 243, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingRepoCollateralSecurityType, bool> UnderlyingRepoCollateralSecurityType    { get; set; } = ValidateUnderlyingRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseTerm, tag 244, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingRepurchaseTerm, bool> UnderlyingRepurchaseTerm                { get; set; } = ValidateUnderlyingRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 UnderlyingRepurchaseRate, tag 245, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingRepurchaseRate, bool> UnderlyingRepurchaseRate                { get; set; } = ValidateUnderlyingRepurchaseRate;

	/// <summary>Holds a FIX 4.4 UnderlyingFactor, tag 246, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingFactor, bool> UnderlyingFactor                        { get; set; } = ValidateUnderlyingFactor;

	/// <summary>Holds a FIX 4.4 UnderlyingRedemptionDate, tag 247, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingRedemptionDate, bool> UnderlyingRedemptionDate                { get; set; } = ValidateUnderlyingRedemptionDate;

	/// <summary>Holds a FIX 4.4 LegCouponPaymentDate, tag 248, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCouponPaymentDate, bool> LegCouponPaymentDate                    { get; set; } = ValidateLegCouponPaymentDate;

	/// <summary>Holds a FIX 4.4 LegIssueDate, tag 249, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegIssueDate, bool> LegIssueDate                            { get; set; } = ValidateLegIssueDate;

	/// <summary>Holds a FIX 4.4 LegRepoCollateralSecurityType, tag 250, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRepoCollateralSecurityType, bool> LegRepoCollateralSecurityType           { get; set; } = ValidateLegRepoCollateralSecurityType;

	/// <summary>Holds a FIX 4.4 LegRepurchaseTerm, tag 251, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRepurchaseTerm, bool> LegRepurchaseTerm                       { get; set; } = ValidateLegRepurchaseTerm;

	/// <summary>Holds a FIX 4.4 LegRepurchaseRate, tag 252, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRepurchaseRate, bool> LegRepurchaseRate                       { get; set; } = ValidateLegRepurchaseRate;

	/// <summary>Holds a FIX 4.4 LegFactor, tag 253, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegFactor, bool> LegFactor                               { get; set; } = ValidateLegFactor;

	/// <summary>Holds a FIX 4.4 LegRedemptionDate, tag 254, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRedemptionDate, bool> LegRedemptionDate                       { get; set; } = ValidateLegRedemptionDate;

	/// <summary>Holds a FIX 4.4 CreditRating, tag 255, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CreditRating, bool> CreditRating                            { get; set; } = ValidateCreditRating;

	/// <summary>Holds a FIX 4.4 UnderlyingCreditRating, tag 256, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCreditRating, bool> UnderlyingCreditRating                  { get; set; } = ValidateUnderlyingCreditRating;

	/// <summary>Holds a FIX 4.4 LegCreditRating, tag 257, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCreditRating, bool> LegCreditRating                         { get; set; } = ValidateLegCreditRating;

	/// <summary>Holds a FIX 4.4 TradedFlatSwitch, tag 258, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradedFlatSwitch, bool> TradedFlatSwitch                        { get; set; } = ValidateTradedFlatSwitch;

	/// <summary>Holds a FIX 4.4 BasisFeatureDate, tag 259, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BasisFeatureDate, bool> BasisFeatureDate                        { get; set; } = ValidateBasisFeatureDate;

	/// <summary>Holds a FIX 4.4 BasisFeaturePrice, tag 260, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BasisFeaturePrice, bool> BasisFeaturePrice                       { get; set; } = ValidateBasisFeaturePrice;

	/// <summary>Holds a FIX 4.4 MDReqID, tag 262, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDReqID, bool> MDReqID                                 { get; set; } = ValidateMDReqID;

	/// <summary>Holds a FIX 4.4 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SubscriptionRequestType, bool> SubscriptionRequestType                 { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.4 MarketDepth, tag 264, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MarketDepth, bool> MarketDepth                             { get; set; } = ValidateMarketDepth;

	/// <summary>Holds a FIX 4.4 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDUpdateType, bool> MDUpdateType                            { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.4 AggregatedBook, tag 266, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AggregatedBook, bool> AggregatedBook                          { get; set; } = ValidateAggregatedBook;

	/// <summary>Holds a FIX 4.4 NoMDEntryTypes, tag 267, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoMDEntryTypes, bool> NoMDEntryTypes                          { get; set; } = ValidateNoMDEntryTypes;

	/// <summary>Holds a FIX 4.4 NoMDEntries, tag 268, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoMDEntries, bool> NoMDEntries                             { get; set; } = ValidateNoMDEntries;

	/// <summary>Holds a FIX 4.4 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryType, bool> MDEntryType                             { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.4 MDEntryPx, tag 270, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryPx, bool> MDEntryPx                               { get; set; } = ValidateMDEntryPx;

	/// <summary>Holds a FIX 4.4 MDEntrySize, tag 271, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntrySize, bool> MDEntrySize                             { get; set; } = ValidateMDEntrySize;

	/// <summary>Holds a FIX 4.4 MDEntryDate, tag 272, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryDate, bool> MDEntryDate                             { get; set; } = ValidateMDEntryDate;

	/// <summary>Holds a FIX 4.4 MDEntryTime, tag 273, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryTime, bool> MDEntryTime                             { get; set; } = ValidateMDEntryTime;

	/// <summary>Holds a FIX 4.4 TickDirection, tag 274, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TickDirection, bool> TickDirection                           { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.4 MDMkt, tag 275, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDMkt, bool> MDMkt                                   { get; set; } = ValidateMDMkt;

	/// <summary>Holds a FIX 4.4 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteCondition, bool> QuoteCondition                          { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.4 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeCondition, bool> TradeCondition                          { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.4 MDEntryID, tag 278, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryID, bool> MDEntryID                               { get; set; } = ValidateMDEntryID;

	/// <summary>Holds a FIX 4.4 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDUpdateAction, bool> MDUpdateAction                          { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.4 MDEntryRefID, tag 280, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryRefID, bool> MDEntryRefID                            { get; set; } = ValidateMDEntryRefID;

	/// <summary>Holds a FIX 4.4 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDReqRejReason, bool> MDReqRejReason                          { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.4 MDEntryOriginator, tag 282, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryOriginator, bool> MDEntryOriginator                       { get; set; } = ValidateMDEntryOriginator;

	/// <summary>Holds a FIX 4.4 LocationID, tag 283, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LocationID, bool> LocationID                              { get; set; } = ValidateLocationID;

	/// <summary>Holds a FIX 4.4 DeskID, tag 284, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DeskID, bool> DeskID                                  { get; set; } = ValidateDeskID;

	/// <summary>Holds a FIX 4.4 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeleteReason, bool> DeleteReason                            { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.4 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OpenCloseSettlFlag, bool> OpenCloseSettlFlag                      { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 4.4 SellerDays, tag 287, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SellerDays, bool> SellerDays                              { get; set; } = ValidateSellerDays;

	/// <summary>Holds a FIX 4.4 MDEntryBuyer, tag 288, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryBuyer, bool> MDEntryBuyer                            { get; set; } = ValidateMDEntryBuyer;

	/// <summary>Holds a FIX 4.4 MDEntrySeller, tag 289, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntrySeller, bool> MDEntrySeller                           { get; set; } = ValidateMDEntrySeller;

	/// <summary>Holds a FIX 4.4 MDEntryPositionNo, tag 290, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryPositionNo, bool> MDEntryPositionNo                       { get; set; } = ValidateMDEntryPositionNo;

	/// <summary>Holds a FIX 4.4 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.FinancialStatus, bool> FinancialStatus                         { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.4 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CorporateAction, bool> CorporateAction                         { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.4 DefBidSize, tag 293, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DefBidSize, bool> DefBidSize                              { get; set; } = ValidateDefBidSize;

	/// <summary>Holds a FIX 4.4 DefOfferSize, tag 294, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DefOfferSize, bool> DefOfferSize                            { get; set; } = ValidateDefOfferSize;

	/// <summary>Holds a FIX 4.4 NoQuoteEntries, tag 295, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoQuoteEntries, bool> NoQuoteEntries                          { get; set; } = ValidateNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 NoQuoteSets, tag 296, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoQuoteSets, bool> NoQuoteSets                             { get; set; } = ValidateNoQuoteSets;

	/// <summary>Holds a FIX 4.4 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteStatus, bool> QuoteStatus                             { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 4.4 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteCancelType, bool> QuoteCancelType                         { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.4 QuoteEntryID, tag 299, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteEntryID, bool> QuoteEntryID                            { get; set; } = ValidateQuoteEntryID;

	/// <summary>Holds a FIX 4.4 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRejectReason, bool> QuoteRejectReason                       { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.4 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteResponseLevel, bool> QuoteResponseLevel                      { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.4 QuoteSetID, tag 302, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteSetID, bool> QuoteSetID                              { get; set; } = ValidateQuoteSetID;

	/// <summary>Holds a FIX 4.4 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRequestType, bool> QuoteRequestType                        { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.4 TotNoQuoteEntries, tag 304, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoQuoteEntries, bool> TotNoQuoteEntries                       { get; set; } = ValidateTotNoQuoteEntries;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityIDSource, tag 305, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityIDSource, bool> UnderlyingSecurityIDSource              { get; set; } = ValidateUnderlyingSecurityIDSource;

	/// <summary>Holds a FIX 4.4 UnderlyingIssuer, tag 306, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingIssuer, bool> UnderlyingIssuer                        { get; set; } = ValidateUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityDesc, tag 307, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityDesc, bool> UnderlyingSecurityDesc                  { get; set; } = ValidateUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityExchange, tag 308, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityExchange, bool> UnderlyingSecurityExchange              { get; set; } = ValidateUnderlyingSecurityExchange;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityID, tag 309, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityID, bool> UnderlyingSecurityID                    { get; set; } = ValidateUnderlyingSecurityID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityType, tag 310, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityType, bool> UnderlyingSecurityType                  { get; set; } = ValidateUnderlyingSecurityType;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbol, tag 311, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSymbol, bool> UnderlyingSymbol                        { get; set; } = ValidateUnderlyingSymbol;

	/// <summary>Holds a FIX 4.4 UnderlyingSymbolSfx, tag 312, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSymbolSfx, bool> UnderlyingSymbolSfx                     { get; set; } = ValidateUnderlyingSymbolSfx;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityMonthYear, tag 313, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingMaturityMonthYear, bool> UnderlyingMaturityMonthYear             { get; set; } = ValidateUnderlyingMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 UnderlyingPutOrCall, tag 315, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingPutOrCall, bool> UnderlyingPutOrCall                     { get; set; } = ValidateUnderlyingPutOrCall;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikePrice, tag 316, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStrikePrice, bool> UnderlyingStrikePrice                   { get; set; } = ValidateUnderlyingStrikePrice;

	/// <summary>Holds a FIX 4.4 UnderlyingOptAttribute, tag 317, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingOptAttribute, bool> UnderlyingOptAttribute                  { get; set; } = ValidateUnderlyingOptAttribute;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrency, tag 318, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCurrency, bool> UnderlyingCurrency                      { get; set; } = ValidateUnderlyingCurrency;

	/// <summary>Holds a FIX 4.4 SecurityReqID, tag 320, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityReqID, bool> SecurityReqID                           { get; set; } = ValidateSecurityReqID;

	/// <summary>Holds a FIX 4.4 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityRequestType, bool> SecurityRequestType                     { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.4 SecurityResponseID, tag 322, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityResponseID, bool> SecurityResponseID                      { get; set; } = ValidateSecurityResponseID;

	/// <summary>Holds a FIX 4.4 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityResponseType, bool> SecurityResponseType                    { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.4 SecurityStatusReqID, tag 324, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityStatusReqID, bool> SecurityStatusReqID                     { get; set; } = ValidateSecurityStatusReqID;

	/// <summary>Holds a FIX 4.4 UnsolicitedIndicator, tag 325, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnsolicitedIndicator, bool> UnsolicitedIndicator                    { get; set; } = ValidateUnsolicitedIndicator;

	/// <summary>Holds a FIX 4.4 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityTradingStatus, bool> SecurityTradingStatus                   { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.4 HaltReason, tag 327, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.HaltReason, bool> HaltReason                              { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.4 InViewOfCommon, tag 328, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InViewOfCommon, bool> InViewOfCommon                          { get; set; } = ValidateInViewOfCommon;

	/// <summary>Holds a FIX 4.4 DueToRelated, tag 329, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DueToRelated, bool> DueToRelated                            { get; set; } = ValidateDueToRelated;

	/// <summary>Holds a FIX 4.4 BuyVolume, tag 330, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BuyVolume, bool> BuyVolume                               { get; set; } = ValidateBuyVolume;

	/// <summary>Holds a FIX 4.4 SellVolume, tag 331, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SellVolume, bool> SellVolume                              { get; set; } = ValidateSellVolume;

	/// <summary>Holds a FIX 4.4 HighPx, tag 332, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.HighPx, bool> HighPx                                  { get; set; } = ValidateHighPx;

	/// <summary>Holds a FIX 4.4 LowPx, tag 333, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LowPx, bool> LowPx                                   { get; set; } = ValidateLowPx;

	/// <summary>Holds a FIX 4.4 Adjustment, tag 334, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Adjustment, bool> Adjustment                              { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.4 TradSesReqID, tag 335, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesReqID, bool> TradSesReqID                            { get; set; } = ValidateTradSesReqID;

	/// <summary>Holds a FIX 4.4 TradingSessionID, tag 336, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradingSessionID, bool> TradingSessionID                        { get; set; } = ValidateTradingSessionID;

	/// <summary>Holds a FIX 4.4 ContraTrader, tag 337, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraTrader, bool> ContraTrader                            { get; set; } = ValidateContraTrader;

	/// <summary>Holds a FIX 4.4 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesMethod, bool> TradSesMethod                           { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.4 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesMode, bool> TradSesMode                             { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.4 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesStatus, bool> TradSesStatus                           { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.4 TradSesStartTime, tag 341, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesStartTime, bool> TradSesStartTime                        { get; set; } = ValidateTradSesStartTime;

	/// <summary>Holds a FIX 4.4 TradSesOpenTime, tag 342, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesOpenTime, bool> TradSesOpenTime                         { get; set; } = ValidateTradSesOpenTime;

	/// <summary>Holds a FIX 4.4 TradSesPreCloseTime, tag 343, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesPreCloseTime, bool> TradSesPreCloseTime                     { get; set; } = ValidateTradSesPreCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesCloseTime, tag 344, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesCloseTime, bool> TradSesCloseTime                        { get; set; } = ValidateTradSesCloseTime;

	/// <summary>Holds a FIX 4.4 TradSesEndTime, tag 345, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesEndTime, bool> TradSesEndTime                          { get; set; } = ValidateTradSesEndTime;

	/// <summary>Holds a FIX 4.4 NumberOfOrders, tag 346, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NumberOfOrders, bool> NumberOfOrders                          { get; set; } = ValidateNumberOfOrders;

	/// <summary>Holds a FIX 4.4 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MessageEncoding, bool> MessageEncoding                         { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.4 EncodedIssuerLen, tag 348, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedIssuerLen, bool> EncodedIssuerLen                        { get; set; } = ValidateEncodedIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedIssuer, tag 349, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedIssuer, bool> EncodedIssuer                           { get; set; } = ValidateEncodedIssuer;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDescLen, tag 350, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedSecurityDescLen, bool> EncodedSecurityDescLen                  { get; set; } = ValidateEncodedSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedSecurityDesc, tag 351, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedSecurityDesc, bool> EncodedSecurityDesc                     { get; set; } = ValidateEncodedSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedListExecInstLen, tag 352, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedListExecInstLen, bool> EncodedListExecInstLen                  { get; set; } = ValidateEncodedListExecInstLen;

	/// <summary>Holds a FIX 4.4 EncodedListExecInst, tag 353, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedListExecInst, bool> EncodedListExecInst                     { get; set; } = ValidateEncodedListExecInst;

	/// <summary>Holds a FIX 4.4 EncodedTextLen, tag 354, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedTextLen, bool> EncodedTextLen                          { get; set; } = ValidateEncodedTextLen;

	/// <summary>Holds a FIX 4.4 EncodedText, tag 355, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedText, bool> EncodedText                             { get; set; } = ValidateEncodedText;

	/// <summary>Holds a FIX 4.4 EncodedSubjectLen, tag 356, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedSubjectLen, bool> EncodedSubjectLen                       { get; set; } = ValidateEncodedSubjectLen;

	/// <summary>Holds a FIX 4.4 EncodedSubject, tag 357, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedSubject, bool> EncodedSubject                          { get; set; } = ValidateEncodedSubject;

	/// <summary>Holds a FIX 4.4 EncodedHeadlineLen, tag 358, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedHeadlineLen, bool> EncodedHeadlineLen                      { get; set; } = ValidateEncodedHeadlineLen;

	/// <summary>Holds a FIX 4.4 EncodedHeadline, tag 359, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedHeadline, bool> EncodedHeadline                         { get; set; } = ValidateEncodedHeadline;

	/// <summary>Holds a FIX 4.4 EncodedAllocTextLen, tag 360, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedAllocTextLen, bool> EncodedAllocTextLen                     { get; set; } = ValidateEncodedAllocTextLen;

	/// <summary>Holds a FIX 4.4 EncodedAllocText, tag 361, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedAllocText, bool> EncodedAllocText                        { get; set; } = ValidateEncodedAllocText;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuerLen, tag 362, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedUnderlyingIssuerLen, bool> EncodedUnderlyingIssuerLen              { get; set; } = ValidateEncodedUnderlyingIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingIssuer, tag 363, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedUnderlyingIssuer, bool> EncodedUnderlyingIssuer                 { get; set; } = ValidateEncodedUnderlyingIssuer;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDescLen, tag 364, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedUnderlyingSecurityDescLen, bool> EncodedUnderlyingSecurityDescLen        { get; set; } = ValidateEncodedUnderlyingSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedUnderlyingSecurityDesc, tag 365, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedUnderlyingSecurityDesc, bool> EncodedUnderlyingSecurityDesc           { get; set; } = ValidateEncodedUnderlyingSecurityDesc;

	/// <summary>Holds a FIX 4.4 AllocPrice, tag 366, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocPrice, bool> AllocPrice                              { get; set; } = ValidateAllocPrice;

	/// <summary>Holds a FIX 4.4 QuoteSetValidUntilTime, tag 367, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteSetValidUntilTime, bool> QuoteSetValidUntilTime                  { get; set; } = ValidateQuoteSetValidUntilTime;

	/// <summary>Holds a FIX 4.4 QuoteEntryRejectReason, tag 368, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteEntryRejectReason, bool> QuoteEntryRejectReason                  { get; set; } = ValidateQuoteEntryRejectReason;

	/// <summary>Holds a FIX 4.4 LastMsgSeqNumProcessed, tag 369, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastMsgSeqNumProcessed, bool> LastMsgSeqNumProcessed                  { get; set; } = ValidateLastMsgSeqNumProcessed;

	/// <summary>Holds a FIX 4.4 RefTagID, tag 371, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefTagID, bool> RefTagID                                { get; set; } = ValidateRefTagID;

	/// <summary>Holds a FIX 4.4 RefMsgType, tag 372, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefMsgType, bool> RefMsgType                              { get; set; } = ValidateRefMsgType;

	/// <summary>Holds a FIX 4.4 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SessionRejectReason, bool> SessionRejectReason                     { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.4 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidRequestTransType, bool> BidRequestTransType                     { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.4 ContraBroker, tag 375, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraBroker, bool> ContraBroker                            { get; set; } = ValidateContraBroker;

	/// <summary>Holds a FIX 4.4 ComplianceID, tag 376, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ComplianceID, bool> ComplianceID                            { get; set; } = ValidateComplianceID;

	/// <summary>Holds a FIX 4.4 SolicitedFlag, tag 377, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SolicitedFlag, bool> SolicitedFlag                           { get; set; } = ValidateSolicitedFlag;

	/// <summary>Holds a FIX 4.4 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecRestatementReason, bool> ExecRestatementReason                   { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.4 BusinessRejectRefID, tag 379, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BusinessRejectRefID, bool> BusinessRejectRefID                     { get; set; } = ValidateBusinessRejectRefID;

	/// <summary>Holds a FIX 4.4 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BusinessRejectReason, bool> BusinessRejectReason                    { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.4 GrossTradeAmt, tag 381, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.GrossTradeAmt, bool> GrossTradeAmt                           { get; set; } = ValidateGrossTradeAmt;

	/// <summary>Holds a FIX 4.4 NoContraBrokers, tag 382, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoContraBrokers, bool> NoContraBrokers                         { get; set; } = ValidateNoContraBrokers;

	/// <summary>Holds a FIX 4.4 MaxMessageSize, tag 383, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaxMessageSize, bool> MaxMessageSize                          { get; set; } = ValidateMaxMessageSize;

	/// <summary>Holds a FIX 4.4 NoMsgTypes, tag 384, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoMsgTypes, bool> NoMsgTypes                              { get; set; } = ValidateNoMsgTypes;

	/// <summary>Holds a FIX 4.4 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MsgDirection, bool> MsgDirection                            { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.4 NoTradingSessions, tag 386, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoTradingSessions, bool> NoTradingSessions                       { get; set; } = ValidateNoTradingSessions;

	/// <summary>Holds a FIX 4.4 TotalVolumeTraded, tag 387, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalVolumeTraded, bool> TotalVolumeTraded                       { get; set; } = ValidateTotalVolumeTraded;

	/// <summary>Holds a FIX 4.4 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionInst, bool> DiscretionInst                          { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetValue, tag 389, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionOffsetValue, bool> DiscretionOffsetValue                   { get; set; } = ValidateDiscretionOffsetValue;

	/// <summary>Holds a FIX 4.4 BidID, tag 390, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidID, bool> BidID                                   { get; set; } = ValidateBidID;

	/// <summary>Holds a FIX 4.4 ClientBidID, tag 391, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ClientBidID, bool> ClientBidID                             { get; set; } = ValidateClientBidID;

	/// <summary>Holds a FIX 4.4 ListName, tag 392, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ListName, bool> ListName                                { get; set; } = ValidateListName;

	/// <summary>Holds a FIX 4.4 TotNoRelatedSym, tag 393, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoRelatedSym, bool> TotNoRelatedSym                         { get; set; } = ValidateTotNoRelatedSym;

	/// <summary>Holds a FIX 4.4 BidType, tag 394, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidType, bool> BidType                                 { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.4 NumTickets, tag 395, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NumTickets, bool> NumTickets                              { get; set; } = ValidateNumTickets;

	/// <summary>Holds a FIX 4.4 SideValue1, tag 396, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SideValue1, bool> SideValue1                              { get; set; } = ValidateSideValue1;

	/// <summary>Holds a FIX 4.4 SideValue2, tag 397, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SideValue2, bool> SideValue2                              { get; set; } = ValidateSideValue2;

	/// <summary>Holds a FIX 4.4 NoBidDescriptors, tag 398, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoBidDescriptors, bool> NoBidDescriptors                        { get; set; } = ValidateNoBidDescriptors;

	/// <summary>Holds a FIX 4.4 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidDescriptorType, bool> BidDescriptorType                       { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.4 BidDescriptor, tag 400, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidDescriptor, bool> BidDescriptor                           { get; set; } = ValidateBidDescriptor;

	/// <summary>Holds a FIX 4.4 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SideValueInd, bool> SideValueInd                            { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.4 LiquidityPctLow, tag 402, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityPctLow, bool> LiquidityPctLow                         { get; set; } = ValidateLiquidityPctLow;

	/// <summary>Holds a FIX 4.4 LiquidityPctHigh, tag 403, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityPctHigh, bool> LiquidityPctHigh                        { get; set; } = ValidateLiquidityPctHigh;

	/// <summary>Holds a FIX 4.4 LiquidityValue, tag 404, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityValue, bool> LiquidityValue                          { get; set; } = ValidateLiquidityValue;

	/// <summary>Holds a FIX 4.4 EFPTrackingError, tag 405, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EFPTrackingError, bool> EFPTrackingError                        { get; set; } = ValidateEFPTrackingError;

	/// <summary>Holds a FIX 4.4 FairValue, tag 406, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.FairValue, bool> FairValue                               { get; set; } = ValidateFairValue;

	/// <summary>Holds a FIX 4.4 OutsideIndexPct, tag 407, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OutsideIndexPct, bool> OutsideIndexPct                         { get; set; } = ValidateOutsideIndexPct;

	/// <summary>Holds a FIX 4.4 ValueOfFutures, tag 408, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ValueOfFutures, bool> ValueOfFutures                          { get; set; } = ValidateValueOfFutures;

	/// <summary>Holds a FIX 4.4 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityIndType, bool> LiquidityIndType                        { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.4 WtAverageLiquidity, tag 410, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.WtAverageLiquidity, bool> WtAverageLiquidity                      { get; set; } = ValidateWtAverageLiquidity;

	/// <summary>Holds a FIX 4.4 ExchangeForPhysical, tag 411, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExchangeForPhysical, bool> ExchangeForPhysical                     { get; set; } = ValidateExchangeForPhysical;

	/// <summary>Holds a FIX 4.4 OutMainCntryUIndex, tag 412, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OutMainCntryUIndex, bool> OutMainCntryUIndex                      { get; set; } = ValidateOutMainCntryUIndex;

	/// <summary>Holds a FIX 4.4 CrossPercent, tag 413, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CrossPercent, bool> CrossPercent                            { get; set; } = ValidateCrossPercent;

	/// <summary>Holds a FIX 4.4 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ProgRptReqs, bool> ProgRptReqs                             { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.4 ProgPeriodInterval, tag 415, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ProgPeriodInterval, bool> ProgPeriodInterval                      { get; set; } = ValidateProgPeriodInterval;

	/// <summary>Holds a FIX 4.4 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IncTaxInd, bool> IncTaxInd                               { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.4 NumBidders, tag 417, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NumBidders, bool> NumBidders                              { get; set; } = ValidateNumBidders;

	/// <summary>Holds a FIX 4.4 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidTradeType, bool> BidTradeType                            { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 4.4 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BasisPxType, bool> BasisPxType                             { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.4 NoBidComponents, tag 420, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoBidComponents, bool> NoBidComponents                         { get; set; } = ValidateNoBidComponents;

	/// <summary>Holds a FIX 4.4 Country, tag 421, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Country, bool> Country                                 { get; set; } = ValidateCountry;

	/// <summary>Holds a FIX 4.4 TotNoStrikes, tag 422, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoStrikes, bool> TotNoStrikes                            { get; set; } = ValidateTotNoStrikes;

	/// <summary>Holds a FIX 4.4 PriceType, tag 423, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PriceType, bool> PriceType                               { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.4 DayOrderQty, tag 424, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DayOrderQty, bool> DayOrderQty                             { get; set; } = ValidateDayOrderQty;

	/// <summary>Holds a FIX 4.4 DayCumQty, tag 425, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DayCumQty, bool> DayCumQty                               { get; set; } = ValidateDayCumQty;

	/// <summary>Holds a FIX 4.4 DayAvgPx, tag 426, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DayAvgPx, bool> DayAvgPx                                { get; set; } = ValidateDayAvgPx;

	/// <summary>Holds a FIX 4.4 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.GTBookingInst, bool> GTBookingInst                           { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.4 NoStrikes, tag 428, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoStrikes, bool> NoStrikes                               { get; set; } = ValidateNoStrikes;

	/// <summary>Holds a FIX 4.4 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListStatusType, bool> ListStatusType                          { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.4 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetGrossInd, bool> NetGrossInd                             { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.4 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListOrderStatus, bool> ListOrderStatus                         { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.4 ExpireDate, tag 432, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExpireDate, bool> ExpireDate                              { get; set; } = ValidateExpireDate;

	/// <summary>Holds a FIX 4.4 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListExecInstType, bool> ListExecInstType                        { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.4 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CxlRejResponseTo, bool> CxlRejResponseTo                        { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.4 UnderlyingCouponRate, tag 435, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCouponRate, bool> UnderlyingCouponRate                    { get; set; } = ValidateUnderlyingCouponRate;

	/// <summary>Holds a FIX 4.4 UnderlyingContractMultiplier, tag 436, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingContractMultiplier, bool> UnderlyingContractMultiplier            { get; set; } = ValidateUnderlyingContractMultiplier;

	/// <summary>Holds a FIX 4.4 ContraTradeQty, tag 437, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraTradeQty, bool> ContraTradeQty                          { get; set; } = ValidateContraTradeQty;

	/// <summary>Holds a FIX 4.4 ContraTradeTime, tag 438, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraTradeTime, bool> ContraTradeTime                         { get; set; } = ValidateContraTradeTime;

	/// <summary>Holds a FIX 4.4 LiquidityNumSecurities, tag 441, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityNumSecurities, bool> LiquidityNumSecurities                  { get; set; } = ValidateLiquidityNumSecurities;

	/// <summary>Holds a FIX 4.4 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MultiLegReportingType, bool> MultiLegReportingType                   { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 StrikeTime, tag 443, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StrikeTime, bool> StrikeTime                              { get; set; } = ValidateStrikeTime;

	/// <summary>Holds a FIX 4.4 ListStatusText, tag 444, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ListStatusText, bool> ListStatusText                          { get; set; } = ValidateListStatusText;

	/// <summary>Holds a FIX 4.4 EncodedListStatusTextLen, tag 445, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedListStatusTextLen, bool> EncodedListStatusTextLen                { get; set; } = ValidateEncodedListStatusTextLen;

	/// <summary>Holds a FIX 4.4 EncodedListStatusText, tag 446, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedListStatusText, bool> EncodedListStatusText                   { get; set; } = ValidateEncodedListStatusText;

	/// <summary>Holds a FIX 4.4 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartyIDSource, bool> PartyIDSource                           { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 4.4 PartyID, tag 448, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PartyID, bool> PartyID                                 { get; set; } = ValidatePartyID;

	/// <summary>Holds a FIX 4.4 NetChgPrevDay, tag 451, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NetChgPrevDay, bool> NetChgPrevDay                           { get; set; } = ValidateNetChgPrevDay;

	/// <summary>Holds a FIX 4.4 PartyRole, tag 452, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartyRole, bool> PartyRole                               { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 4.4 NoPartyIDs, tag 453, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoPartyIDs, bool> NoPartyIDs                              { get; set; } = ValidateNoPartyIDs;

	/// <summary>Holds a FIX 4.4 NoSecurityAltID, tag 454, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoSecurityAltID, bool> NoSecurityAltID                         { get; set; } = ValidateNoSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltID, tag 455, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityAltID, bool> SecurityAltID                           { get; set; } = ValidateSecurityAltID;

	/// <summary>Holds a FIX 4.4 SecurityAltIDSource, tag 456, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityAltIDSource, bool> SecurityAltIDSource                     { get; set; } = ValidateSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 NoUnderlyingSecurityAltID, tag 457, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoUnderlyingSecurityAltID, bool> NoUnderlyingSecurityAltID               { get; set; } = ValidateNoUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltID, tag 458, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityAltID, bool> UnderlyingSecurityAltID                 { get; set; } = ValidateUnderlyingSecurityAltID;

	/// <summary>Holds a FIX 4.4 UnderlyingSecurityAltIDSource, tag 459, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecurityAltIDSource, bool> UnderlyingSecurityAltIDSource           { get; set; } = ValidateUnderlyingSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 Product, tag 460, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Product, bool> Product                                 { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 4.4 CFICode, tag 461, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CFICode, bool> CFICode                                 { get; set; } = ValidateCFICode;

	/// <summary>Holds a FIX 4.4 UnderlyingProduct, tag 462, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingProduct, bool> UnderlyingProduct                       { get; set; } = ValidateUnderlyingProduct;

	/// <summary>Holds a FIX 4.4 UnderlyingCFICode, tag 463, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCFICode, bool> UnderlyingCFICode                       { get; set; } = ValidateUnderlyingCFICode;

	/// <summary>Holds a FIX 4.4 TestMessageIndicator, tag 464, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TestMessageIndicator, bool> TestMessageIndicator                    { get; set; } = ValidateTestMessageIndicator;

	/// <summary>Holds a FIX 4.4 BookingRefID, tag 466, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BookingRefID, bool> BookingRefID                            { get; set; } = ValidateBookingRefID;

	/// <summary>Holds a FIX 4.4 IndividualAllocID, tag 467, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IndividualAllocID, bool> IndividualAllocID                       { get; set; } = ValidateIndividualAllocID;

	/// <summary>Holds a FIX 4.4 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RoundingDirection, bool> RoundingDirection                       { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 4.4 RoundingModulus, tag 469, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RoundingModulus, bool> RoundingModulus                         { get; set; } = ValidateRoundingModulus;

	/// <summary>Holds a FIX 4.4 CountryOfIssue, tag 470, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CountryOfIssue, bool> CountryOfIssue                          { get; set; } = ValidateCountryOfIssue;

	/// <summary>Holds a FIX 4.4 StateOrProvinceOfIssue, tag 471, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StateOrProvinceOfIssue, bool> StateOrProvinceOfIssue                  { get; set; } = ValidateStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LocaleOfIssue, tag 472, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LocaleOfIssue, bool> LocaleOfIssue                           { get; set; } = ValidateLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 NoRegistDtls, tag 473, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoRegistDtls, bool> NoRegistDtls                            { get; set; } = ValidateNoRegistDtls;

	/// <summary>Holds a FIX 4.4 MailingDtls, tag 474, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MailingDtls, bool> MailingDtls                             { get; set; } = ValidateMailingDtls;

	/// <summary>Holds a FIX 4.4 InvestorCountryOfResidence, tag 475, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InvestorCountryOfResidence, bool> InvestorCountryOfResidence              { get; set; } = ValidateInvestorCountryOfResidence;

	/// <summary>Holds a FIX 4.4 PaymentRef, tag 476, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PaymentRef, bool> PaymentRef                              { get; set; } = ValidatePaymentRef;

	/// <summary>Holds a FIX 4.4 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DistribPaymentMethod, bool> DistribPaymentMethod                    { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 4.4 CashDistribCurr, tag 478, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribCurr, bool> CashDistribCurr                         { get; set; } = ValidateCashDistribCurr;

	/// <summary>Holds a FIX 4.4 CommCurrency, tag 479, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CommCurrency, bool> CommCurrency                            { get; set; } = ValidateCommCurrency;

	/// <summary>Holds a FIX 4.4 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CancellationRights, bool> CancellationRights                      { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 4.4 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MoneyLaunderingStatus, bool> MoneyLaunderingStatus                   { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 4.4 MailingInst, tag 482, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MailingInst, bool> MailingInst                             { get; set; } = ValidateMailingInst;

	/// <summary>Holds a FIX 4.4 TransBkdTime, tag 483, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TransBkdTime, bool> TransBkdTime                            { get; set; } = ValidateTransBkdTime;

	/// <summary>Holds a FIX 4.4 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecPriceType, bool> ExecPriceType                           { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 4.4 ExecPriceAdjustment, tag 485, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExecPriceAdjustment, bool> ExecPriceAdjustment                     { get; set; } = ValidateExecPriceAdjustment;

	/// <summary>Holds a FIX 4.4 DateOfBirth, tag 486, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DateOfBirth, bool> DateOfBirth                             { get; set; } = ValidateDateOfBirth;

	/// <summary>Holds a FIX 4.4 TradeReportTransType, tag 487, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportTransType, bool> TradeReportTransType                    { get; set; } = ValidateTradeReportTransType;

	/// <summary>Holds a FIX 4.4 CardHolderName, tag 488, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CardHolderName, bool> CardHolderName                          { get; set; } = ValidateCardHolderName;

	/// <summary>Holds a FIX 4.4 CardNumber, tag 489, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CardNumber, bool> CardNumber                              { get; set; } = ValidateCardNumber;

	/// <summary>Holds a FIX 4.4 CardExpDate, tag 490, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CardExpDate, bool> CardExpDate                             { get; set; } = ValidateCardExpDate;

	/// <summary>Holds a FIX 4.4 CardIssNum, tag 491, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CardIssNum, bool> CardIssNum                              { get; set; } = ValidateCardIssNum;

	/// <summary>Holds a FIX 4.4 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PaymentMethod, bool> PaymentMethod                           { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 4.4 RegistAcctType, tag 493, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistAcctType, bool> RegistAcctType                          { get; set; } = ValidateRegistAcctType;

	/// <summary>Holds a FIX 4.4 Designation, tag 494, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Designation, bool> Designation                             { get; set; } = ValidateDesignation;

	/// <summary>Holds a FIX 4.4 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TaxAdvantageType, bool> TaxAdvantageType                        { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 4.4 RegistRejReasonText, tag 496, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistRejReasonText, bool> RegistRejReasonText                     { get; set; } = ValidateRegistRejReasonText;

	/// <summary>Holds a FIX 4.4 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.FundRenewWaiv, bool> FundRenewWaiv                           { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 4.4 CashDistribAgentName, tag 498, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribAgentName, bool> CashDistribAgentName                    { get; set; } = ValidateCashDistribAgentName;

	/// <summary>Holds a FIX 4.4 CashDistribAgentCode, tag 499, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribAgentCode, bool> CashDistribAgentCode                    { get; set; } = ValidateCashDistribAgentCode;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctNumber, tag 500, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribAgentAcctNumber, bool> CashDistribAgentAcctNumber              { get; set; } = ValidateCashDistribAgentAcctNumber;

	/// <summary>Holds a FIX 4.4 CashDistribPayRef, tag 501, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribPayRef, bool> CashDistribPayRef                       { get; set; } = ValidateCashDistribPayRef;

	/// <summary>Holds a FIX 4.4 CashDistribAgentAcctName, tag 502, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashDistribAgentAcctName, bool> CashDistribAgentAcctName                { get; set; } = ValidateCashDistribAgentAcctName;

	/// <summary>Holds a FIX 4.4 CardStartDate, tag 503, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CardStartDate, bool> CardStartDate                           { get; set; } = ValidateCardStartDate;

	/// <summary>Holds a FIX 4.4 PaymentDate, tag 504, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PaymentDate, bool> PaymentDate                             { get; set; } = ValidatePaymentDate;

	/// <summary>Holds a FIX 4.4 PaymentRemitterID, tag 505, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PaymentRemitterID, bool> PaymentRemitterID                       { get; set; } = ValidatePaymentRemitterID;

	/// <summary>Holds a FIX 4.4 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistStatus, bool> RegistStatus                            { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 4.4 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistRejReasonCode, bool> RegistRejReasonCode                     { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 4.4 RegistRefID, tag 508, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistRefID, bool> RegistRefID                             { get; set; } = ValidateRegistRefID;

	/// <summary>Holds a FIX 4.4 RegistDtls, tag 509, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistDtls, bool> RegistDtls                              { get; set; } = ValidateRegistDtls;

	/// <summary>Holds a FIX 4.4 NoDistribInsts, tag 510, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoDistribInsts, bool> NoDistribInsts                          { get; set; } = ValidateNoDistribInsts;

	/// <summary>Holds a FIX 4.4 RegistEmail, tag 511, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistEmail, bool> RegistEmail                             { get; set; } = ValidateRegistEmail;

	/// <summary>Holds a FIX 4.4 DistribPercentage, tag 512, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DistribPercentage, bool> DistribPercentage                       { get; set; } = ValidateDistribPercentage;

	/// <summary>Holds a FIX 4.4 RegistID, tag 513, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RegistID, bool> RegistID                                { get; set; } = ValidateRegistID;

	/// <summary>Holds a FIX 4.4 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistTransType, bool> RegistTransType                         { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 4.4 ExecValuationPoint, tag 515, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExecValuationPoint, bool> ExecValuationPoint                      { get; set; } = ValidateExecValuationPoint;

	/// <summary>Holds a FIX 4.4 OrderPercent, tag 516, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderPercent, bool> OrderPercent                            { get; set; } = ValidateOrderPercent;

	/// <summary>Holds a FIX 4.4 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OwnershipType, bool> OwnershipType                           { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 4.4 NoContAmts, tag 518, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoContAmts, bool> NoContAmts                              { get; set; } = ValidateNoContAmts;

	/// <summary>Holds a FIX 4.4 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ContAmtType, bool> ContAmtType                             { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 4.4 ContAmtValue, tag 520, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContAmtValue, bool> ContAmtValue                            { get; set; } = ValidateContAmtValue;

	/// <summary>Holds a FIX 4.4 ContAmtCurr, tag 521, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContAmtCurr, bool> ContAmtCurr                             { get; set; } = ValidateContAmtCurr;

	/// <summary>Holds a FIX 4.4 OwnerType, tag 522, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OwnerType, bool> OwnerType                               { get; set; } = ValidateOwnerType;

	/// <summary>Holds a FIX 4.4 PartySubID, tag 523, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PartySubID, bool> PartySubID                              { get; set; } = ValidatePartySubID;

	/// <summary>Holds a FIX 4.4 NestedPartyID, tag 524, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NestedPartyID, bool> NestedPartyID                           { get; set; } = ValidateNestedPartyID;

	/// <summary>Holds a FIX 4.4 NestedPartyIDSource, tag 525, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NestedPartyIDSource, bool> NestedPartyIDSource                     { get; set; } = ValidateNestedPartyIDSource;

	/// <summary>Holds a FIX 4.4 SecondaryClOrdID, tag 526, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryClOrdID, bool> SecondaryClOrdID                        { get; set; } = ValidateSecondaryClOrdID;

	/// <summary>Holds a FIX 4.4 SecondaryExecID, tag 527, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryExecID, bool> SecondaryExecID                         { get; set; } = ValidateSecondaryExecID;

	/// <summary>Holds a FIX 4.4 OrderCapacity, tag 528, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrderCapacity, bool> OrderCapacity                           { get; set; } = ValidateOrderCapacity;

	/// <summary>Holds a FIX 4.4 OrderRestrictions, tag 529, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrderRestrictions, bool> OrderRestrictions                       { get; set; } = ValidateOrderRestrictions;

	/// <summary>Holds a FIX 4.4 MassCancelRequestType, tag 530, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MassCancelRequestType, bool> MassCancelRequestType                   { get; set; } = ValidateMassCancelRequestType;

	/// <summary>Holds a FIX 4.4 MassCancelResponse, tag 531, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MassCancelResponse, bool> MassCancelResponse                      { get; set; } = ValidateMassCancelResponse;

	/// <summary>Holds a FIX 4.4 MassCancelRejectReason, tag 532, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MassCancelRejectReason, bool> MassCancelRejectReason                  { get; set; } = ValidateMassCancelRejectReason;

	/// <summary>Holds a FIX 4.4 TotalAffectedOrders, tag 533, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalAffectedOrders, bool> TotalAffectedOrders                     { get; set; } = ValidateTotalAffectedOrders;

	/// <summary>Holds a FIX 4.4 NoAffectedOrders, tag 534, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoAffectedOrders, bool> NoAffectedOrders                        { get; set; } = ValidateNoAffectedOrders;

	/// <summary>Holds a FIX 4.4 AffectedOrderID, tag 535, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AffectedOrderID, bool> AffectedOrderID                         { get; set; } = ValidateAffectedOrderID;

	/// <summary>Holds a FIX 4.4 AffectedSecondaryOrderID, tag 536, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AffectedSecondaryOrderID, bool> AffectedSecondaryOrderID                { get; set; } = ValidateAffectedSecondaryOrderID;

	/// <summary>Holds a FIX 4.4 QuoteType, tag 537, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteType, bool> QuoteType                               { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 4.4 NestedPartyRole, tag 538, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NestedPartyRole, bool> NestedPartyRole                         { get; set; } = ValidateNestedPartyRole;

	/// <summary>Holds a FIX 4.4 NoNestedPartyIDs, tag 539, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNestedPartyIDs, bool> NoNestedPartyIDs                        { get; set; } = ValidateNoNestedPartyIDs;

	/// <summary>Holds a FIX 4.4 TotalAccruedInterestAmt, tag 540, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalAccruedInterestAmt, bool> TotalAccruedInterestAmt                 { get; set; } = ValidateTotalAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 MaturityDate, tag 541, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaturityDate, bool> MaturityDate                            { get; set; } = ValidateMaturityDate;

	/// <summary>Holds a FIX 4.4 UnderlyingMaturityDate, tag 542, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingMaturityDate, bool> UnderlyingMaturityDate                  { get; set; } = ValidateUnderlyingMaturityDate;

	/// <summary>Holds a FIX 4.4 InstrRegistry, tag 543, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InstrRegistry, bool> InstrRegistry                           { get; set; } = ValidateInstrRegistry;

	/// <summary>Holds a FIX 4.4 CashMargin, tag 544, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CashMargin, bool> CashMargin                              { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 4.4 NestedPartySubID, tag 545, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NestedPartySubID, bool> NestedPartySubID                        { get; set; } = ValidateNestedPartySubID;

	/// <summary>Holds a FIX 4.4 Scope, tag 546, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Scope, bool> Scope                                   { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 4.4 MDImplicitDelete, tag 547, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MDImplicitDelete, bool> MDImplicitDelete                        { get; set; } = ValidateMDImplicitDelete;

	/// <summary>Holds a FIX 4.4 CrossID, tag 548, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CrossID, bool> CrossID                                 { get; set; } = ValidateCrossID;

	/// <summary>Holds a FIX 4.4 CrossType, tag 549, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CrossType, bool> CrossType                               { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 4.4 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CrossPrioritization, bool> CrossPrioritization                     { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 4.4 OrigCrossID, tag 551, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigCrossID, bool> OrigCrossID                             { get; set; } = ValidateOrigCrossID;

	/// <summary>Holds a FIX 4.4 NoSides, tag 552, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NoSides, bool> NoSides                                 { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 4.4 Username, tag 553, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Username, bool> Username                                { get; set; } = ValidateUsername;

	/// <summary>Holds a FIX 4.4 Password, tag 554, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Password, bool> Password                                { get; set; } = ValidatePassword;

	/// <summary>Holds a FIX 4.4 NoLegs, tag 555, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoLegs, bool> NoLegs                                  { get; set; } = ValidateNoLegs;

	/// <summary>Holds a FIX 4.4 LegCurrency, tag 556, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCurrency, bool> LegCurrency                             { get; set; } = ValidateLegCurrency;

	/// <summary>Holds a FIX 4.4 TotNoSecurityTypes, tag 557, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoSecurityTypes, bool> TotNoSecurityTypes                      { get; set; } = ValidateTotNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 NoSecurityTypes, tag 558, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoSecurityTypes, bool> NoSecurityTypes                         { get; set; } = ValidateNoSecurityTypes;

	/// <summary>Holds a FIX 4.4 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityListRequestType, bool> SecurityListRequestType                 { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 4.4 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityRequestResult, bool> SecurityRequestResult                   { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 4.4 RoundLot, tag 561, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RoundLot, bool> RoundLot                                { get; set; } = ValidateRoundLot;

	/// <summary>Holds a FIX 4.4 MinTradeVol, tag 562, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MinTradeVol, bool> MinTradeVol                             { get; set; } = ValidateMinTradeVol;

	/// <summary>Holds a FIX 4.4 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MultiLegRptTypeReq, bool> MultiLegRptTypeReq                      { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 4.4 LegPositionEffect, tag 564, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegPositionEffect, bool> LegPositionEffect                       { get; set; } = ValidateLegPositionEffect;

	/// <summary>Holds a FIX 4.4 LegCoveredOrUncovered, tag 565, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCoveredOrUncovered, bool> LegCoveredOrUncovered                   { get; set; } = ValidateLegCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 LegPrice, tag 566, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegPrice, bool> LegPrice                                { get; set; } = ValidateLegPrice;

	/// <summary>Holds a FIX 4.4 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesStatusRejReason, bool> TradSesStatusRejReason                  { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 4.4 TradeRequestID, tag 568, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestID, bool> TradeRequestID                          { get; set; } = ValidateTradeRequestID;

	/// <summary>Holds a FIX 4.4 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestType, bool> TradeRequestType                        { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 4.4 PreviouslyReported, tag 570, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PreviouslyReported, bool> PreviouslyReported                      { get; set; } = ValidatePreviouslyReported;

	/// <summary>Holds a FIX 4.4 TradeReportID, tag 571, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportID, bool> TradeReportID                           { get; set; } = ValidateTradeReportID;

	/// <summary>Holds a FIX 4.4 TradeReportRefID, tag 572, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportRefID, bool> TradeReportRefID                        { get; set; } = ValidateTradeReportRefID;

	/// <summary>Holds a FIX 4.4 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MatchStatus, bool> MatchStatus                             { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 4.4 MatchType, tag 574, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MatchType, bool> MatchType                               { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 4.4 OddLot, tag 575, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OddLot, bool> OddLot                                  { get; set; } = ValidateOddLot;

	/// <summary>Holds a FIX 4.4 NoClearingInstructions, tag 576, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoClearingInstructions, bool> NoClearingInstructions                  { get; set; } = ValidateNoClearingInstructions;

	/// <summary>Holds a FIX 4.4 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ClearingInstruction, bool> ClearingInstruction                     { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 4.4 TradeInputSource, tag 578, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeInputSource, bool> TradeInputSource                        { get; set; } = ValidateTradeInputSource;

	/// <summary>Holds a FIX 4.4 TradeInputDevice, tag 579, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeInputDevice, bool> TradeInputDevice                        { get; set; } = ValidateTradeInputDevice;

	/// <summary>Holds a FIX 4.4 NoDates, tag 580, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoDates, bool> NoDates                                 { get; set; } = ValidateNoDates;

	/// <summary>Holds a FIX 4.4 AccountType, tag 581, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AccountType, bool> AccountType                             { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 4.4 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CustOrderCapacity, bool> CustOrderCapacity                       { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 4.4 ClOrdLinkID, tag 583, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ClOrdLinkID, bool> ClOrdLinkID                             { get; set; } = ValidateClOrdLinkID;

	/// <summary>Holds a FIX 4.4 MassStatusReqID, tag 584, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MassStatusReqID, bool> MassStatusReqID                         { get; set; } = ValidateMassStatusReqID;

	/// <summary>Holds a FIX 4.4 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MassStatusReqType, bool> MassStatusReqType                       { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 4.4 OrigOrdModTime, tag 586, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigOrdModTime, bool> OrigOrdModTime                          { get; set; } = ValidateOrigOrdModTime;

	/// <summary>Holds a FIX 4.4 LegSettlType, tag 587, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSettlType, bool> LegSettlType                            { get; set; } = ValidateLegSettlType;

	/// <summary>Holds a FIX 4.4 LegSettlDate, tag 588, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSettlDate, bool> LegSettlDate                            { get; set; } = ValidateLegSettlDate;

	/// <summary>Holds a FIX 4.4 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DayBookingInst, bool> DayBookingInst                          { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 4.4 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BookingUnit, bool> BookingUnit                             { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 4.4 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PreallocMethod, bool> PreallocMethod                          { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 4.4 UnderlyingCountryOfIssue, tag 592, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCountryOfIssue, bool> UnderlyingCountryOfIssue                { get; set; } = ValidateUnderlyingCountryOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingStateOrProvinceOfIssue, tag 593, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStateOrProvinceOfIssue, bool> UnderlyingStateOrProvinceOfIssue        { get; set; } = ValidateUnderlyingStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingLocaleOfIssue, tag 594, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingLocaleOfIssue, bool> UnderlyingLocaleOfIssue                 { get; set; } = ValidateUnderlyingLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrRegistry, tag 595, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingInstrRegistry, bool> UnderlyingInstrRegistry                 { get; set; } = ValidateUnderlyingInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegCountryOfIssue, tag 596, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCountryOfIssue, bool> LegCountryOfIssue                       { get; set; } = ValidateLegCountryOfIssue;

	/// <summary>Holds a FIX 4.4 LegStateOrProvinceOfIssue, tag 597, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegStateOrProvinceOfIssue, bool> LegStateOrProvinceOfIssue               { get; set; } = ValidateLegStateOrProvinceOfIssue;

	/// <summary>Holds a FIX 4.4 LegLocaleOfIssue, tag 598, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegLocaleOfIssue, bool> LegLocaleOfIssue                        { get; set; } = ValidateLegLocaleOfIssue;

	/// <summary>Holds a FIX 4.4 LegInstrRegistry, tag 599, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegInstrRegistry, bool> LegInstrRegistry                        { get; set; } = ValidateLegInstrRegistry;

	/// <summary>Holds a FIX 4.4 LegSymbol, tag 600, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSymbol, bool> LegSymbol                               { get; set; } = ValidateLegSymbol;

	/// <summary>Holds a FIX 4.4 LegSymbolSfx, tag 601, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSymbolSfx, bool> LegSymbolSfx                            { get; set; } = ValidateLegSymbolSfx;

	/// <summary>Holds a FIX 4.4 LegSecurityID, tag 602, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityID, bool> LegSecurityID                           { get; set; } = ValidateLegSecurityID;

	/// <summary>Holds a FIX 4.4 LegSecurityIDSource, tag 603, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityIDSource, bool> LegSecurityIDSource                     { get; set; } = ValidateLegSecurityIDSource;

	/// <summary>Holds a FIX 4.4 NoLegSecurityAltID, tag 604, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoLegSecurityAltID, bool> NoLegSecurityAltID                      { get; set; } = ValidateNoLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltID, tag 605, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityAltID, bool> LegSecurityAltID                        { get; set; } = ValidateLegSecurityAltID;

	/// <summary>Holds a FIX 4.4 LegSecurityAltIDSource, tag 606, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityAltIDSource, bool> LegSecurityAltIDSource                  { get; set; } = ValidateLegSecurityAltIDSource;

	/// <summary>Holds a FIX 4.4 LegProduct, tag 607, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegProduct, bool> LegProduct                              { get; set; } = ValidateLegProduct;

	/// <summary>Holds a FIX 4.4 LegCFICode, tag 608, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCFICode, bool> LegCFICode                              { get; set; } = ValidateLegCFICode;

	/// <summary>Holds a FIX 4.4 LegSecurityType, tag 609, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityType, bool> LegSecurityType                         { get; set; } = ValidateLegSecurityType;

	/// <summary>Holds a FIX 4.4 LegMaturityMonthYear, tag 610, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegMaturityMonthYear, bool> LegMaturityMonthYear                    { get; set; } = ValidateLegMaturityMonthYear;

	/// <summary>Holds a FIX 4.4 LegMaturityDate, tag 611, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegMaturityDate, bool> LegMaturityDate                         { get; set; } = ValidateLegMaturityDate;

	/// <summary>Holds a FIX 4.4 LegStrikePrice, tag 612, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegStrikePrice, bool> LegStrikePrice                          { get; set; } = ValidateLegStrikePrice;

	/// <summary>Holds a FIX 4.4 LegOptAttribute, tag 613, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegOptAttribute, bool> LegOptAttribute                         { get; set; } = ValidateLegOptAttribute;

	/// <summary>Holds a FIX 4.4 LegContractMultiplier, tag 614, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegContractMultiplier, bool> LegContractMultiplier                   { get; set; } = ValidateLegContractMultiplier;

	/// <summary>Holds a FIX 4.4 LegCouponRate, tag 615, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegCouponRate, bool> LegCouponRate                           { get; set; } = ValidateLegCouponRate;

	/// <summary>Holds a FIX 4.4 LegSecurityExchange, tag 616, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityExchange, bool> LegSecurityExchange                     { get; set; } = ValidateLegSecurityExchange;

	/// <summary>Holds a FIX 4.4 LegIssuer, tag 617, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegIssuer, bool> LegIssuer                               { get; set; } = ValidateLegIssuer;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuerLen, tag 618, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedLegIssuerLen, bool> EncodedLegIssuerLen                     { get; set; } = ValidateEncodedLegIssuerLen;

	/// <summary>Holds a FIX 4.4 EncodedLegIssuer, tag 619, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedLegIssuer, bool> EncodedLegIssuer                        { get; set; } = ValidateEncodedLegIssuer;

	/// <summary>Holds a FIX 4.4 LegSecurityDesc, tag 620, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecurityDesc, bool> LegSecurityDesc                         { get; set; } = ValidateLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDescLen, tag 621, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedLegSecurityDescLen, bool> EncodedLegSecurityDescLen               { get; set; } = ValidateEncodedLegSecurityDescLen;

	/// <summary>Holds a FIX 4.4 EncodedLegSecurityDesc, tag 622, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EncodedLegSecurityDesc, bool> EncodedLegSecurityDesc                  { get; set; } = ValidateEncodedLegSecurityDesc;

	/// <summary>Holds a FIX 4.4 LegRatioQty, tag 623, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRatioQty, bool> LegRatioQty                             { get; set; } = ValidateLegRatioQty;

	/// <summary>Holds a FIX 4.4 LegSide, tag 624, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSide, bool> LegSide                                 { get; set; } = ValidateLegSide;

	/// <summary>Holds a FIX 4.4 TradingSessionSubID, tag 625, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradingSessionSubID, bool> TradingSessionSubID                     { get; set; } = ValidateTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 AllocType, tag 626, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocType, bool> AllocType                               { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 4.4 NoHops, tag 627, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoHops, bool> NoHops                                  { get; set; } = ValidateNoHops;

	/// <summary>Holds a FIX 4.4 HopCompID, tag 628, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.HopCompID, bool> HopCompID                               { get; set; } = ValidateHopCompID;

	/// <summary>Holds a FIX 4.4 HopSendingTime, tag 629, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.HopSendingTime, bool> HopSendingTime                          { get; set; } = ValidateHopSendingTime;

	/// <summary>Holds a FIX 4.4 HopRefID, tag 630, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.HopRefID, bool> HopRefID                                { get; set; } = ValidateHopRefID;

	/// <summary>Holds a FIX 4.4 MidPx, tag 631, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MidPx, bool> MidPx                                   { get; set; } = ValidateMidPx;

	/// <summary>Holds a FIX 4.4 BidYield, tag 632, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidYield, bool> BidYield                                { get; set; } = ValidateBidYield;

	/// <summary>Holds a FIX 4.4 MidYield, tag 633, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MidYield, bool> MidYield                                { get; set; } = ValidateMidYield;

	/// <summary>Holds a FIX 4.4 OfferYield, tag 634, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferYield, bool> OfferYield                              { get; set; } = ValidateOfferYield;

	/// <summary>Holds a FIX 4.4 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ClearingFeeIndicator, bool> ClearingFeeIndicator                    { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 4.4 WorkingIndicator, tag 636, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.WorkingIndicator, bool> WorkingIndicator                        { get; set; } = ValidateWorkingIndicator;

	/// <summary>Holds a FIX 4.4 LegLastPx, tag 637, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegLastPx, bool> LegLastPx                               { get; set; } = ValidateLegLastPx;

	/// <summary>Holds a FIX 4.4 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PriorityIndicator, bool> PriorityIndicator                       { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 4.4 PriceImprovement, tag 639, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PriceImprovement, bool> PriceImprovement                        { get; set; } = ValidatePriceImprovement;

	/// <summary>Holds a FIX 4.4 Price2, tag 640, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Price2, bool> Price2                                  { get; set; } = ValidatePrice2;

	/// <summary>Holds a FIX 4.4 LastForwardPoints2, tag 641, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastForwardPoints2, bool> LastForwardPoints2                      { get; set; } = ValidateLastForwardPoints2;

	/// <summary>Holds a FIX 4.4 BidForwardPoints2, tag 642, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BidForwardPoints2, bool> BidForwardPoints2                       { get; set; } = ValidateBidForwardPoints2;

	/// <summary>Holds a FIX 4.4 OfferForwardPoints2, tag 643, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OfferForwardPoints2, bool> OfferForwardPoints2                     { get; set; } = ValidateOfferForwardPoints2;

	/// <summary>Holds a FIX 4.4 RFQReqID, tag 644, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RFQReqID, bool> RFQReqID                                { get; set; } = ValidateRFQReqID;

	/// <summary>Holds a FIX 4.4 MktBidPx, tag 645, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MktBidPx, bool> MktBidPx                                { get; set; } = ValidateMktBidPx;

	/// <summary>Holds a FIX 4.4 MktOfferPx, tag 646, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MktOfferPx, bool> MktOfferPx                              { get; set; } = ValidateMktOfferPx;

	/// <summary>Holds a FIX 4.4 MinBidSize, tag 647, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MinBidSize, bool> MinBidSize                              { get; set; } = ValidateMinBidSize;

	/// <summary>Holds a FIX 4.4 MinOfferSize, tag 648, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MinOfferSize, bool> MinOfferSize                            { get; set; } = ValidateMinOfferSize;

	/// <summary>Holds a FIX 4.4 QuoteStatusReqID, tag 649, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteStatusReqID, bool> QuoteStatusReqID                        { get; set; } = ValidateQuoteStatusReqID;

	/// <summary>Holds a FIX 4.4 LegalConfirm, tag 650, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegalConfirm, bool> LegalConfirm                            { get; set; } = ValidateLegalConfirm;

	/// <summary>Holds a FIX 4.4 UnderlyingLastPx, tag 651, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingLastPx, bool> UnderlyingLastPx                        { get; set; } = ValidateUnderlyingLastPx;

	/// <summary>Holds a FIX 4.4 UnderlyingLastQty, tag 652, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingLastQty, bool> UnderlyingLastQty                       { get; set; } = ValidateUnderlyingLastQty;

	/// <summary>Holds a FIX 4.4 LegRefID, tag 654, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegRefID, bool> LegRefID                                { get; set; } = ValidateLegRefID;

	/// <summary>Holds a FIX 4.4 ContraLegRefID, tag 655, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraLegRefID, bool> ContraLegRefID                          { get; set; } = ValidateContraLegRefID;

	/// <summary>Holds a FIX 4.4 SettlCurrBidFxRate, tag 656, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrBidFxRate, bool> SettlCurrBidFxRate                      { get; set; } = ValidateSettlCurrBidFxRate;

	/// <summary>Holds a FIX 4.4 SettlCurrOfferFxRate, tag 657, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrOfferFxRate, bool> SettlCurrOfferFxRate                    { get; set; } = ValidateSettlCurrOfferFxRate;

	/// <summary>Holds a FIX 4.4 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRequestRejectReason, bool> QuoteRequestRejectReason                { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 4.4 SideComplianceID, tag 659, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SideComplianceID, bool> SideComplianceID                        { get; set; } = ValidateSideComplianceID;

	/// <summary>Holds a FIX 4.4 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AcctIDSource, bool> AcctIDSource                            { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 4.4 AllocAcctIDSource, tag 661, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAcctIDSource, bool> AllocAcctIDSource                       { get; set; } = ValidateAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 BenchmarkPrice, tag 662, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkPrice, bool> BenchmarkPrice                          { get; set; } = ValidateBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 BenchmarkPriceType, tag 663, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkPriceType, bool> BenchmarkPriceType                      { get; set; } = ValidateBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 ConfirmID, tag 664, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmID, bool> ConfirmID                               { get; set; } = ValidateConfirmID;

	/// <summary>Holds a FIX 4.4 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmStatus, bool> ConfirmStatus                           { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 4.4 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmTransType, bool> ConfirmTransType                        { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 4.4 ContractSettlMonth, tag 667, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContractSettlMonth, bool> ContractSettlMonth                      { get; set; } = ValidateContractSettlMonth;

	/// <summary>Holds a FIX 4.4 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeliveryForm, bool> DeliveryForm                            { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 4.4 LastParPx, tag 669, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastParPx, bool> LastParPx                               { get; set; } = ValidateLastParPx;

	/// <summary>Holds a FIX 4.4 NoLegAllocs, tag 670, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoLegAllocs, bool> NoLegAllocs                             { get; set; } = ValidateNoLegAllocs;

	/// <summary>Holds a FIX 4.4 LegAllocAccount, tag 671, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegAllocAccount, bool> LegAllocAccount                         { get; set; } = ValidateLegAllocAccount;

	/// <summary>Holds a FIX 4.4 LegIndividualAllocID, tag 672, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegIndividualAllocID, bool> LegIndividualAllocID                    { get; set; } = ValidateLegIndividualAllocID;

	/// <summary>Holds a FIX 4.4 LegAllocQty, tag 673, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegAllocQty, bool> LegAllocQty                             { get; set; } = ValidateLegAllocQty;

	/// <summary>Holds a FIX 4.4 LegAllocAcctIDSource, tag 674, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegAllocAcctIDSource, bool> LegAllocAcctIDSource                    { get; set; } = ValidateLegAllocAcctIDSource;

	/// <summary>Holds a FIX 4.4 LegSettlCurrency, tag 675, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSettlCurrency, bool> LegSettlCurrency                        { get; set; } = ValidateLegSettlCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveCurrency, tag 676, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBenchmarkCurveCurrency, bool> LegBenchmarkCurveCurrency               { get; set; } = ValidateLegBenchmarkCurveCurrency;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveName, tag 677, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBenchmarkCurveName, bool> LegBenchmarkCurveName                   { get; set; } = ValidateLegBenchmarkCurveName;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurvePoint, tag 678, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBenchmarkCurvePoint, bool> LegBenchmarkCurvePoint                  { get; set; } = ValidateLegBenchmarkCurvePoint;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPrice, tag 679, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBenchmarkPrice, bool> LegBenchmarkPrice                       { get; set; } = ValidateLegBenchmarkPrice;

	/// <summary>Holds a FIX 4.4 LegBenchmarkPriceType, tag 680, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBenchmarkPriceType, bool> LegBenchmarkPriceType                   { get; set; } = ValidateLegBenchmarkPriceType;

	/// <summary>Holds a FIX 4.4 LegBidPx, tag 681, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegBidPx, bool> LegBidPx                                { get; set; } = ValidateLegBidPx;

	/// <summary>Holds a FIX 4.4 LegIOIQty, tag 682, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegIOIQty, bool> LegIOIQty                               { get; set; } = ValidateLegIOIQty;

	/// <summary>Holds a FIX 4.4 NoLegStipulations, tag 683, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoLegStipulations, bool> NoLegStipulations                       { get; set; } = ValidateNoLegStipulations;

	/// <summary>Holds a FIX 4.4 LegOfferPx, tag 684, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegOfferPx, bool> LegOfferPx                              { get; set; } = ValidateLegOfferPx;

	/// <summary>Holds a FIX 4.4 LegPriceType, tag 686, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegPriceType, bool> LegPriceType                            { get; set; } = ValidateLegPriceType;

	/// <summary>Holds a FIX 4.4 LegQty, tag 687, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegQty, bool> LegQty                                  { get; set; } = ValidateLegQty;

	/// <summary>Holds a FIX 4.4 LegStipulationType, tag 688, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegStipulationType, bool> LegStipulationType                      { get; set; } = ValidateLegStipulationType;

	/// <summary>Holds a FIX 4.4 LegStipulationValue, tag 689, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegStipulationValue, bool> LegStipulationValue                     { get; set; } = ValidateLegStipulationValue;

	/// <summary>Holds a FIX 4.4 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LegSwapType, bool> LegSwapType                             { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 4.4 Pool, tag 691, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Pool, bool> Pool                                    { get; set; } = ValidatePool;

	/// <summary>Holds a FIX 4.4 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuotePriceType, bool> QuotePriceType                          { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 4.4 QuoteRespID, tag 693, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRespID, bool> QuoteRespID                             { get; set; } = ValidateQuoteRespID;

	/// <summary>Holds a FIX 4.4 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRespType, bool> QuoteRespType                           { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 4.4 QuoteQualifier, tag 695, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteQualifier, bool> QuoteQualifier                          { get; set; } = ValidateQuoteQualifier;

	/// <summary>Holds a FIX 4.4 YieldRedemptionDate, tag 696, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.YieldRedemptionDate, bool> YieldRedemptionDate                     { get; set; } = ValidateYieldRedemptionDate;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPrice, tag 697, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.YieldRedemptionPrice, bool> YieldRedemptionPrice                    { get; set; } = ValidateYieldRedemptionPrice;

	/// <summary>Holds a FIX 4.4 YieldRedemptionPriceType, tag 698, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.YieldRedemptionPriceType, bool> YieldRedemptionPriceType                { get; set; } = ValidateYieldRedemptionPriceType;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityID, tag 699, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkSecurityID, bool> BenchmarkSecurityID                     { get; set; } = ValidateBenchmarkSecurityID;

	/// <summary>Holds a FIX 4.4 ReversalIndicator, tag 700, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ReversalIndicator, bool> ReversalIndicator                       { get; set; } = ValidateReversalIndicator;

	/// <summary>Holds a FIX 4.4 YieldCalcDate, tag 701, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.YieldCalcDate, bool> YieldCalcDate                           { get; set; } = ValidateYieldCalcDate;

	/// <summary>Holds a FIX 4.4 NoPositions, tag 702, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoPositions, bool> NoPositions                             { get; set; } = ValidateNoPositions;

	/// <summary>Holds a FIX 4.4 PosType, tag 703, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosType, bool> PosType                                 { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 4.4 LongQty, tag 704, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LongQty, bool> LongQty                                 { get; set; } = ValidateLongQty;

	/// <summary>Holds a FIX 4.4 ShortQty, tag 705, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ShortQty, bool> ShortQty                                { get; set; } = ValidateShortQty;

	/// <summary>Holds a FIX 4.4 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosQtyStatus, bool> PosQtyStatus                            { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 4.4 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosAmtType, bool> PosAmtType                              { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 4.4 PosAmt, tag 708, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PosAmt, bool> PosAmt                                  { get; set; } = ValidatePosAmt;

	/// <summary>Holds a FIX 4.4 PosTransType, tag 709, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosTransType, bool> PosTransType                            { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 4.4 PosReqID, tag 710, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqID, bool> PosReqID                                { get; set; } = ValidatePosReqID;

	/// <summary>Holds a FIX 4.4 NoUnderlyings, tag 711, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoUnderlyings, bool> NoUnderlyings                           { get; set; } = ValidateNoUnderlyings;

	/// <summary>Holds a FIX 4.4 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintAction, bool> PosMaintAction                          { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 4.4 OrigPosReqRefID, tag 713, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrigPosReqRefID, bool> OrigPosReqRefID                         { get; set; } = ValidateOrigPosReqRefID;

	/// <summary>Holds a FIX 4.4 PosMaintRptRefID, tag 714, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintRptRefID, bool> PosMaintRptRefID                        { get; set; } = ValidatePosMaintRptRefID;

	/// <summary>Holds a FIX 4.4 ClearingBusinessDate, tag 715, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ClearingBusinessDate, bool> ClearingBusinessDate                    { get; set; } = ValidateClearingBusinessDate;

	/// <summary>Holds a FIX 4.4 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlSessID, bool> SettlSessID                             { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 4.4 SettlSessSubID, tag 717, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlSessSubID, bool> SettlSessSubID                          { get; set; } = ValidateSettlSessSubID;

	/// <summary>Holds a FIX 4.4 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdjustmentType, bool> AdjustmentType                          { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 4.4 ContraryInstructionIndicator, tag 719, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ContraryInstructionIndicator, bool> ContraryInstructionIndicator            { get; set; } = ValidateContraryInstructionIndicator;

	/// <summary>Holds a FIX 4.4 PriorSpreadIndicator, tag 720, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PriorSpreadIndicator, bool> PriorSpreadIndicator                    { get; set; } = ValidatePriorSpreadIndicator;

	/// <summary>Holds a FIX 4.4 PosMaintRptID, tag 721, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintRptID, bool> PosMaintRptID                           { get; set; } = ValidatePosMaintRptID;

	/// <summary>Holds a FIX 4.4 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintStatus, bool> PosMaintStatus                          { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 4.4 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintResult, bool> PosMaintResult                          { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 4.4 PosReqType, tag 724, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqType, bool> PosReqType                              { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 4.4 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ResponseTransportType, bool> ResponseTransportType                   { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 4.4 ResponseDestination, tag 726, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ResponseDestination, bool> ResponseDestination                     { get; set; } = ValidateResponseDestination;

	/// <summary>Holds a FIX 4.4 TotalNumPosReports, tag 727, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalNumPosReports, bool> TotalNumPosReports                      { get; set; } = ValidateTotalNumPosReports;

	/// <summary>Holds a FIX 4.4 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqResult, bool> PosReqResult                            { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 4.4 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqStatus, bool> PosReqStatus                            { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 4.4 SettlPrice, tag 730, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPrice, bool> SettlPrice                              { get; set; } = ValidateSettlPrice;

	/// <summary>Holds a FIX 4.4 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPriceType, bool> SettlPriceType                          { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPrice, tag 732, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSettlPrice, bool> UnderlyingSettlPrice                    { get; set; } = ValidateUnderlyingSettlPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingSettlPriceType, tag 733, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSettlPriceType, bool> UnderlyingSettlPriceType                { get; set; } = ValidateUnderlyingSettlPriceType;

	/// <summary>Holds a FIX 4.4 PriorSettlPrice, tag 734, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PriorSettlPrice, bool> PriorSettlPrice                         { get; set; } = ValidatePriorSettlPrice;

	/// <summary>Holds a FIX 4.4 NoQuoteQualifiers, tag 735, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoQuoteQualifiers, bool> NoQuoteQualifiers                       { get; set; } = ValidateNoQuoteQualifiers;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrency, tag 736, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocSettlCurrency, bool> AllocSettlCurrency                      { get; set; } = ValidateAllocSettlCurrency;

	/// <summary>Holds a FIX 4.4 AllocSettlCurrAmt, tag 737, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocSettlCurrAmt, bool> AllocSettlCurrAmt                       { get; set; } = ValidateAllocSettlCurrAmt;

	/// <summary>Holds a FIX 4.4 InterestAtMaturity, tag 738, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InterestAtMaturity, bool> InterestAtMaturity                      { get; set; } = ValidateInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 LegDatedDate, tag 739, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegDatedDate, bool> LegDatedDate                            { get; set; } = ValidateLegDatedDate;

	/// <summary>Holds a FIX 4.4 LegPool, tag 740, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegPool, bool> LegPool                                 { get; set; } = ValidateLegPool;

	/// <summary>Holds a FIX 4.4 AllocInterestAtMaturity, tag 741, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocInterestAtMaturity, bool> AllocInterestAtMaturity                 { get; set; } = ValidateAllocInterestAtMaturity;

	/// <summary>Holds a FIX 4.4 AllocAccruedInterestAmt, tag 742, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAccruedInterestAmt, bool> AllocAccruedInterestAmt                 { get; set; } = ValidateAllocAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 DeliveryDate, tag 743, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DeliveryDate, bool> DeliveryDate                            { get; set; } = ValidateDeliveryDate;

	/// <summary>Holds a FIX 4.4 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AssignmentMethod, bool> AssignmentMethod                        { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 4.4 AssignmentUnit, tag 745, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AssignmentUnit, bool> AssignmentUnit                          { get; set; } = ValidateAssignmentUnit;

	/// <summary>Holds a FIX 4.4 OpenInterest, tag 746, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OpenInterest, bool> OpenInterest                            { get; set; } = ValidateOpenInterest;

	/// <summary>Holds a FIX 4.4 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExerciseMethod, bool> ExerciseMethod                          { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 4.4 TotNumTradeReports, tag 748, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNumTradeReports, bool> TotNumTradeReports                      { get; set; } = ValidateTotNumTradeReports;

	/// <summary>Holds a FIX 4.4 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestResult, bool> TradeRequestResult                      { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 4.4 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestStatus, bool> TradeRequestStatus                      { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 4.4 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportRejectReason, bool> TradeReportRejectReason                 { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 4.4 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SideMultiLegReportingType, bool> SideMultiLegReportingType               { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 NoPosAmt, tag 753, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoPosAmt, bool> NoPosAmt                                { get; set; } = ValidateNoPosAmt;

	/// <summary>Holds a FIX 4.4 AutoAcceptIndicator, tag 754, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AutoAcceptIndicator, bool> AutoAcceptIndicator                     { get; set; } = ValidateAutoAcceptIndicator;

	/// <summary>Holds a FIX 4.4 AllocReportID, tag 755, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocReportID, bool> AllocReportID                           { get; set; } = ValidateAllocReportID;

	/// <summary>Holds a FIX 4.4 NoNested2PartyIDs, tag 756, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNested2PartyIDs, bool> NoNested2PartyIDs                       { get; set; } = ValidateNoNested2PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartyID, tag 757, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested2PartyID, bool> Nested2PartyID                          { get; set; } = ValidateNested2PartyID;

	/// <summary>Holds a FIX 4.4 Nested2PartyIDSource, tag 758, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested2PartyIDSource, bool> Nested2PartyIDSource                    { get; set; } = ValidateNested2PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested2PartyRole, tag 759, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested2PartyRole, bool> Nested2PartyRole                        { get; set; } = ValidateNested2PartyRole;

	/// <summary>Holds a FIX 4.4 Nested2PartySubID, tag 760, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested2PartySubID, bool> Nested2PartySubID                       { get; set; } = ValidateNested2PartySubID;

	/// <summary>Holds a FIX 4.4 BenchmarkSecurityIDSource, tag 761, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.BenchmarkSecurityIDSource, bool> BenchmarkSecurityIDSource               { get; set; } = ValidateBenchmarkSecurityIDSource;

	/// <summary>Holds a FIX 4.4 SecuritySubType, tag 762, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecuritySubType, bool> SecuritySubType                         { get; set; } = ValidateSecuritySubType;

	/// <summary>Holds a FIX 4.4 UnderlyingSecuritySubType, tag 763, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingSecuritySubType, bool> UnderlyingSecuritySubType               { get; set; } = ValidateUnderlyingSecuritySubType;

	/// <summary>Holds a FIX 4.4 LegSecuritySubType, tag 764, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegSecuritySubType, bool> LegSecuritySubType                      { get; set; } = ValidateLegSecuritySubType;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessPct, tag 765, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllowableOneSidednessPct, bool> AllowableOneSidednessPct                { get; set; } = ValidateAllowableOneSidednessPct;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessValue, tag 766, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllowableOneSidednessValue, bool> AllowableOneSidednessValue              { get; set; } = ValidateAllowableOneSidednessValue;

	/// <summary>Holds a FIX 4.4 AllowableOneSidednessCurr, tag 767, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllowableOneSidednessCurr, bool> AllowableOneSidednessCurr               { get; set; } = ValidateAllowableOneSidednessCurr;

	/// <summary>Holds a FIX 4.4 NoTrdRegTimestamps, tag 768, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoTrdRegTimestamps, bool> NoTrdRegTimestamps                      { get; set; } = ValidateNoTrdRegTimestamps;

	/// <summary>Holds a FIX 4.4 TrdRegTimestamp, tag 769, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRegTimestamp, bool> TrdRegTimestamp                         { get; set; } = ValidateTrdRegTimestamp;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRegTimestampType, bool> TrdRegTimestampType                     { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampOrigin, tag 771, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRegTimestampOrigin, bool> TrdRegTimestampOrigin                   { get; set; } = ValidateTrdRegTimestampOrigin;

	/// <summary>Holds a FIX 4.4 ConfirmRefID, tag 772, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmRefID, bool> ConfirmRefID                            { get; set; } = ValidateConfirmRefID;

	/// <summary>Holds a FIX 4.4 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmType, bool> ConfirmType                             { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 4.4 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmRejReason, bool> ConfirmRejReason                        { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 4.4 BookingType, tag 775, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BookingType, bool> BookingType                             { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 4.4 IndividualAllocRejCode, tag 776, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.IndividualAllocRejCode, bool> IndividualAllocRejCode                  { get; set; } = ValidateIndividualAllocRejCode;

	/// <summary>Holds a FIX 4.4 SettlInstMsgID, tag 777, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstMsgID, bool> SettlInstMsgID                          { get; set; } = ValidateSettlInstMsgID;

	/// <summary>Holds a FIX 4.4 NoSettlInst, tag 778, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoSettlInst, bool> NoSettlInst                             { get; set; } = ValidateNoSettlInst;

	/// <summary>Holds a FIX 4.4 LastUpdateTime, tag 779, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastUpdateTime, bool> LastUpdateTime                          { get; set; } = ValidateLastUpdateTime;

	/// <summary>Holds a FIX 4.4 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocSettlInstType, bool> AllocSettlInstType                      { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 4.4 NoSettlPartyIDs, tag 781, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoSettlPartyIDs, bool> NoSettlPartyIDs                         { get; set; } = ValidateNoSettlPartyIDs;

	/// <summary>Holds a FIX 4.4 SettlPartyID, tag 782, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPartyID, bool> SettlPartyID                            { get; set; } = ValidateSettlPartyID;

	/// <summary>Holds a FIX 4.4 SettlPartyIDSource, tag 783, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPartyIDSource, bool> SettlPartyIDSource                      { get; set; } = ValidateSettlPartyIDSource;

	/// <summary>Holds a FIX 4.4 SettlPartyRole, tag 784, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPartyRole, bool> SettlPartyRole                          { get; set; } = ValidateSettlPartyRole;

	/// <summary>Holds a FIX 4.4 SettlPartySubID, tag 785, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPartySubID, bool> SettlPartySubID                         { get; set; } = ValidateSettlPartySubID;

	/// <summary>Holds a FIX 4.4 SettlPartySubIDType, tag 786, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPartySubIDType, bool> SettlPartySubIDType                     { get; set; } = ValidateSettlPartySubIDType;

	/// <summary>Holds a FIX 4.4 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DlvyInstType, bool> DlvyInstType                            { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 4.4 TerminationType, tag 788, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TerminationType, bool> TerminationType                         { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 4.4 NextExpectedMsgSeqNum, tag 789, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NextExpectedMsgSeqNum, bool> NextExpectedMsgSeqNum                   { get; set; } = ValidateNextExpectedMsgSeqNum;

	/// <summary>Holds a FIX 4.4 OrdStatusReqID, tag 790, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrdStatusReqID, bool> OrdStatusReqID                          { get; set; } = ValidateOrdStatusReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqID, tag 791, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstReqID, bool> SettlInstReqID                          { get; set; } = ValidateSettlInstReqID;

	/// <summary>Holds a FIX 4.4 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstReqRejCode, bool> SettlInstReqRejCode                     { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 4.4 SecondaryAllocID, tag 793, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryAllocID, bool> SecondaryAllocID                        { get; set; } = ValidateSecondaryAllocID;

	/// <summary>Holds a FIX 4.4 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocReportType, bool> AllocReportType                         { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 4.4 AllocReportRefID, tag 795, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AllocReportRefID, bool> AllocReportRefID                        { get; set; } = ValidateAllocReportRefID;

	/// <summary>Holds a FIX 4.4 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocCancReplaceReason, bool> AllocCancReplaceReason                  { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 4.4 CopyMsgIndicator, tag 797, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CopyMsgIndicator, bool> CopyMsgIndicator                        { get; set; } = ValidateCopyMsgIndicator;

	/// <summary>Holds a FIX 4.4 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAccountType, bool> AllocAccountType                        { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 4.4 OrderAvgPx, tag 799, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderAvgPx, bool> OrderAvgPx                              { get; set; } = ValidateOrderAvgPx;

	/// <summary>Holds a FIX 4.4 OrderBookingQty, tag 800, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderBookingQty, bool> OrderBookingQty                         { get; set; } = ValidateOrderBookingQty;

	/// <summary>Holds a FIX 4.4 NoSettlPartySubIDs, tag 801, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoSettlPartySubIDs, bool> NoSettlPartySubIDs                      { get; set; } = ValidateNoSettlPartySubIDs;

	/// <summary>Holds a FIX 4.4 NoPartySubIDs, tag 802, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoPartySubIDs, bool> NoPartySubIDs                           { get; set; } = ValidateNoPartySubIDs;

	/// <summary>Holds a FIX 4.4 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartySubIDType, bool> PartySubIDType                          { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNestedPartySubIDs, tag 804, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNestedPartySubIDs, bool> NoNestedPartySubIDs                     { get; set; } = ValidateNoNestedPartySubIDs;

	/// <summary>Holds a FIX 4.4 NestedPartySubIDType, tag 805, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NestedPartySubIDType, bool> NestedPartySubIDType                    { get; set; } = ValidateNestedPartySubIDType;

	/// <summary>Holds a FIX 4.4 NoNested2PartySubIDs, tag 806, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNested2PartySubIDs, bool> NoNested2PartySubIDs                    { get; set; } = ValidateNoNested2PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested2PartySubIDType, tag 807, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested2PartySubIDType, bool> Nested2PartySubIDType                   { get; set; } = ValidateNested2PartySubIDType;

	/// <summary>Holds a FIX 4.4 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocIntermedReqType, bool> AllocIntermedReqType                    { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 4.4 UnderlyingPx, tag 810, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingPx, bool> UnderlyingPx                            { get; set; } = ValidateUnderlyingPx;

	/// <summary>Holds a FIX 4.4 PriceDelta, tag 811, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PriceDelta, bool> PriceDelta                              { get; set; } = ValidatePriceDelta;

	/// <summary>Holds a FIX 4.4 ApplQueueMax, tag 812, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueMax, bool> ApplQueueMax                            { get; set; } = ValidateApplQueueMax;

	/// <summary>Holds a FIX 4.4 ApplQueueDepth, tag 813, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueDepth, bool> ApplQueueDepth                          { get; set; } = ValidateApplQueueDepth;

	/// <summary>Holds a FIX 4.4 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueResolution, bool> ApplQueueResolution                     { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 4.4 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueAction, bool> ApplQueueAction                         { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 4.4 NoAltMDSource, tag 816, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoAltMDSource, bool> NoAltMDSource                           { get; set; } = ValidateNoAltMDSource;

	/// <summary>Holds a FIX 4.4 AltMDSourceID, tag 817, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AltMDSourceID, bool> AltMDSourceID                           { get; set; } = ValidateAltMDSourceID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportID, tag 818, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryTradeReportID, bool> SecondaryTradeReportID                  { get; set; } = ValidateSecondaryTradeReportID;

	/// <summary>Holds a FIX 4.4 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AvgPxIndicator, bool> AvgPxIndicator                          { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 4.4 TradeLinkID, tag 820, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeLinkID, bool> TradeLinkID                             { get; set; } = ValidateTradeLinkID;

	/// <summary>Holds a FIX 4.4 OrderInputDevice, tag 821, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderInputDevice, bool> OrderInputDevice                        { get; set; } = ValidateOrderInputDevice;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionID, tag 822, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingTradingSessionID, bool> UnderlyingTradingSessionID              { get; set; } = ValidateUnderlyingTradingSessionID;

	/// <summary>Holds a FIX 4.4 UnderlyingTradingSessionSubID, tag 823, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingTradingSessionSubID, bool> UnderlyingTradingSessionSubID           { get; set; } = ValidateUnderlyingTradingSessionSubID;

	/// <summary>Holds a FIX 4.4 TradeLegRefID, tag 824, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TradeLegRefID, bool> TradeLegRefID                           { get; set; } = ValidateTradeLegRefID;

	/// <summary>Holds a FIX 4.4 ExchangeRule, tag 825, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ExchangeRule, bool> ExchangeRule                            { get; set; } = ValidateExchangeRule;

	/// <summary>Holds a FIX 4.4 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeAllocIndicator, bool> TradeAllocIndicator                     { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 4.4 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExpirationCycle, bool> ExpirationCycle                         { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 4.4 TrdType, tag 828, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdType, bool> TrdType                                 { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 4.4 TrdSubType, tag 829, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TrdSubType, bool> TrdSubType                              { get; set; } = ValidateTrdSubType;

	/// <summary>Holds a FIX 4.4 TransferReason, tag 830, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TransferReason, bool> TransferReason                          { get; set; } = ValidateTransferReason;

	/// <summary>Holds a FIX 4.4 TotNumAssignmentReports, tag 832, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNumAssignmentReports, bool> TotNumAssignmentReports                 { get; set; } = ValidateTotNumAssignmentReports;

	/// <summary>Holds a FIX 4.4 AsgnRptID, tag 833, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AsgnRptID, bool> AsgnRptID                               { get; set; } = ValidateAsgnRptID;

	/// <summary>Holds a FIX 4.4 ThresholdAmount, tag 834, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ThresholdAmount, bool> ThresholdAmount                         { get; set; } = ValidateThresholdAmount;

	/// <summary>Holds a FIX 4.4 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegMoveType, bool> PegMoveType                             { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 4.4 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegOffsetType, bool> PegOffsetType                           { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 4.4 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegLimitType, bool> PegLimitType                            { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 4.4 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegRoundDirection, bool> PegRoundDirection                       { get; set; } = ValidatePegRoundDirection;

	/// <summary>Holds a FIX 4.4 PeggedPrice, tag 839, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PeggedPrice, bool> PeggedPrice                             { get; set; } = ValidatePeggedPrice;

	/// <summary>Holds a FIX 4.4 PegScope, tag 840, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegScope, bool> PegScope                                { get; set; } = ValidatePegScope;

	/// <summary>Holds a FIX 4.4 DiscretionMoveType, tag 841, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionMoveType, bool> DiscretionMoveType                      { get; set; } = ValidateDiscretionMoveType;

	/// <summary>Holds a FIX 4.4 DiscretionOffsetType, tag 842, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionOffsetType, bool> DiscretionOffsetType                    { get; set; } = ValidateDiscretionOffsetType;

	/// <summary>Holds a FIX 4.4 DiscretionLimitType, tag 843, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionLimitType, bool> DiscretionLimitType                     { get; set; } = ValidateDiscretionLimitType;

	/// <summary>Holds a FIX 4.4 DiscretionRoundDirection, tag 844, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionRoundDirection, bool> DiscretionRoundDirection                { get; set; } = ValidateDiscretionRoundDirection;

	/// <summary>Holds a FIX 4.4 DiscretionPrice, tag 845, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionPrice, bool> DiscretionPrice                         { get; set; } = ValidateDiscretionPrice;

	/// <summary>Holds a FIX 4.4 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionScope, bool> DiscretionScope                         { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 4.4 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TargetStrategy, bool> TargetStrategy                          { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 4.4 TargetStrategyParameters, tag 848, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TargetStrategyParameters, bool> TargetStrategyParameters                { get; set; } = ValidateTargetStrategyParameters;

	/// <summary>Holds a FIX 4.4 ParticipationRate, tag 849, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ParticipationRate, bool> ParticipationRate                       { get; set; } = ValidateParticipationRate;

	/// <summary>Holds a FIX 4.4 TargetStrategyPerformance, tag 850, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TargetStrategyPerformance, bool> TargetStrategyPerformance               { get; set; } = ValidateTargetStrategyPerformance;

	/// <summary>Holds a FIX 4.4 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LastLiquidityInd, bool> LastLiquidityInd                        { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 4.4 PublishTrdIndicator, tag 852, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PublishTrdIndicator, bool> PublishTrdIndicator                     { get; set; } = ValidatePublishTrdIndicator;

	/// <summary>Holds a FIX 4.4 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ShortSaleReason, bool> ShortSaleReason                         { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 4.4 QtyType, tag 854, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QtyType, bool> QtyType                                 { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 4.4 SecondaryTrdType, tag 855, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryTrdType, bool> SecondaryTrdType                        { get; set; } = ValidateSecondaryTrdType;

	/// <summary>Holds a FIX 4.4 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportType, bool> TradeReportType                         { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 4.4 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocNoOrdersType, bool> AllocNoOrdersType                       { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 4.4 SharedCommission, tag 858, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SharedCommission, bool> SharedCommission                        { get; set; } = ValidateSharedCommission;

	/// <summary>Holds a FIX 4.4 ConfirmReqID, tag 859, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmReqID, bool> ConfirmReqID                            { get; set; } = ValidateConfirmReqID;

	/// <summary>Holds a FIX 4.4 AvgParPx, tag 860, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AvgParPx, bool> AvgParPx                                { get; set; } = ValidateAvgParPx;

	/// <summary>Holds a FIX 4.4 ReportedPx, tag 861, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.ReportedPx, bool> ReportedPx                              { get; set; } = ValidateReportedPx;

	/// <summary>Holds a FIX 4.4 NoCapacities, tag 862, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoCapacities, bool> NoCapacities                            { get; set; } = ValidateNoCapacities;

	/// <summary>Holds a FIX 4.4 OrderCapacityQty, tag 863, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.OrderCapacityQty, bool> OrderCapacityQty                        { get; set; } = ValidateOrderCapacityQty;

	/// <summary>Holds a FIX 4.4 NoEvents, tag 864, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoEvents, bool> NoEvents                                { get; set; } = ValidateNoEvents;

	/// <summary>Holds a FIX 4.4 EventType, tag 865, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EventType, bool> EventType                               { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 4.4 EventDate, tag 866, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EventDate, bool> EventDate                               { get; set; } = ValidateEventDate;

	/// <summary>Holds a FIX 4.4 EventPx, tag 867, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EventPx, bool> EventPx                                 { get; set; } = ValidateEventPx;

	/// <summary>Holds a FIX 4.4 EventText, tag 868, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EventText, bool> EventText                               { get; set; } = ValidateEventText;

	/// <summary>Holds a FIX 4.4 PctAtRisk, tag 869, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.PctAtRisk, bool> PctAtRisk                               { get; set; } = ValidatePctAtRisk;

	/// <summary>Holds a FIX 4.4 NoInstrAttrib, tag 870, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoInstrAttrib, bool> NoInstrAttrib                           { get; set; } = ValidateNoInstrAttrib;

	/// <summary>Holds a FIX 4.4 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.InstrAttribType, bool> InstrAttribType                         { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 4.4 InstrAttribValue, tag 872, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InstrAttribValue, bool> InstrAttribValue                        { get; set; } = ValidateInstrAttribValue;

	/// <summary>Holds a FIX 4.4 DatedDate, tag 873, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.DatedDate, bool> DatedDate                               { get; set; } = ValidateDatedDate;

	/// <summary>Holds a FIX 4.4 InterestAccrualDate, tag 874, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.InterestAccrualDate, bool> InterestAccrualDate                     { get; set; } = ValidateInterestAccrualDate;

	/// <summary>Holds a FIX 4.4 CPProgram, tag 875, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CPProgram, bool> CPProgram                               { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 4.4 CPRegType, tag 876, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CPRegType, bool> CPRegType                               { get; set; } = ValidateCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingCPProgram, tag 877, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCPProgram, bool> UnderlyingCPProgram                     { get; set; } = ValidateUnderlyingCPProgram;

	/// <summary>Holds a FIX 4.4 UnderlyingCPRegType, tag 878, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCPRegType, bool> UnderlyingCPRegType                     { get; set; } = ValidateUnderlyingCPRegType;

	/// <summary>Holds a FIX 4.4 UnderlyingQty, tag 879, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingQty, bool> UnderlyingQty                           { get; set; } = ValidateUnderlyingQty;

	/// <summary>Holds a FIX 4.4 TrdMatchID, tag 880, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TrdMatchID, bool> TrdMatchID                              { get; set; } = ValidateTrdMatchID;

	/// <summary>Holds a FIX 4.4 SecondaryTradeReportRefID, tag 881, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.SecondaryTradeReportRefID, bool> SecondaryTradeReportRefID               { get; set; } = ValidateSecondaryTradeReportRefID;

	/// <summary>Holds a FIX 4.4 UnderlyingDirtyPrice, tag 882, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingDirtyPrice, bool> UnderlyingDirtyPrice                    { get; set; } = ValidateUnderlyingDirtyPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingEndPrice, tag 883, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingEndPrice, bool> UnderlyingEndPrice                      { get; set; } = ValidateUnderlyingEndPrice;

	/// <summary>Holds a FIX 4.4 UnderlyingStartValue, tag 884, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStartValue, bool> UnderlyingStartValue                    { get; set; } = ValidateUnderlyingStartValue;

	/// <summary>Holds a FIX 4.4 UnderlyingCurrentValue, tag 885, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingCurrentValue, bool> UnderlyingCurrentValue                  { get; set; } = ValidateUnderlyingCurrentValue;

	/// <summary>Holds a FIX 4.4 UnderlyingEndValue, tag 886, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingEndValue, bool> UnderlyingEndValue                      { get; set; } = ValidateUnderlyingEndValue;

	/// <summary>Holds a FIX 4.4 NoUnderlyingStips, tag 887, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoUnderlyingStips, bool> NoUnderlyingStips                       { get; set; } = ValidateNoUnderlyingStips;

	/// <summary>Holds a FIX 4.4 UnderlyingStipType, tag 888, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStipType, bool> UnderlyingStipType                      { get; set; } = ValidateUnderlyingStipType;

	/// <summary>Holds a FIX 4.4 UnderlyingStipValue, tag 889, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStipValue, bool> UnderlyingStipValue                     { get; set; } = ValidateUnderlyingStipValue;

	/// <summary>Holds a FIX 4.4 MaturityNetMoney, tag 890, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MaturityNetMoney, bool> MaturityNetMoney                        { get; set; } = ValidateMaturityNetMoney;

	/// <summary>Holds a FIX 4.4 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeBasis, bool> MiscFeeBasis                            { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 4.4 TotNoAllocs, tag 892, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNoAllocs, bool> TotNoAllocs                             { get; set; } = ValidateTotNoAllocs;

	/// <summary>Holds a FIX 4.4 LastFragment, tag 893, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastFragment, bool> LastFragment                            { get; set; } = ValidateLastFragment;

	/// <summary>Holds a FIX 4.4 CollReqID, tag 894, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollReqID, bool> CollReqID                               { get; set; } = ValidateCollReqID;

	/// <summary>Holds a FIX 4.4 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnReason, bool> CollAsgnReason                          { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 4.4 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryQualifier, bool> CollInquiryQualifier                    { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 NoTrades, tag 897, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoTrades, bool> NoTrades                                { get; set; } = ValidateNoTrades;

	/// <summary>Holds a FIX 4.4 MarginRatio, tag 898, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MarginRatio, bool> MarginRatio                             { get; set; } = ValidateMarginRatio;

	/// <summary>Holds a FIX 4.4 MarginExcess, tag 899, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.MarginExcess, bool> MarginExcess                            { get; set; } = ValidateMarginExcess;

	/// <summary>Holds a FIX 4.4 TotalNetValue, tag 900, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotalNetValue, bool> TotalNetValue                           { get; set; } = ValidateTotalNetValue;

	/// <summary>Holds a FIX 4.4 CashOutstanding, tag 901, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CashOutstanding, bool> CashOutstanding                         { get; set; } = ValidateCashOutstanding;

	/// <summary>Holds a FIX 4.4 CollAsgnID, tag 902, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnID, bool> CollAsgnID                              { get; set; } = ValidateCollAsgnID;

	/// <summary>Holds a FIX 4.4 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnTransType, bool> CollAsgnTransType                       { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 4.4 CollRespID, tag 904, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollRespID, bool> CollRespID                              { get; set; } = ValidateCollRespID;

	/// <summary>Holds a FIX 4.4 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnRespType, bool> CollAsgnRespType                        { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 4.4 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnRejectReason, bool> CollAsgnRejectReason                    { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 4.4 CollAsgnRefID, tag 907, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnRefID, bool> CollAsgnRefID                           { get; set; } = ValidateCollAsgnRefID;

	/// <summary>Holds a FIX 4.4 CollRptID, tag 908, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollRptID, bool> CollRptID                               { get; set; } = ValidateCollRptID;

	/// <summary>Holds a FIX 4.4 CollInquiryID, tag 909, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryID, bool> CollInquiryID                           { get; set; } = ValidateCollInquiryID;

	/// <summary>Holds a FIX 4.4 CollStatus, tag 910, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollStatus, bool> CollStatus                              { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 4.4 TotNumReports, tag 911, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TotNumReports, bool> TotNumReports                           { get; set; } = ValidateTotNumReports;

	/// <summary>Holds a FIX 4.4 LastRptRequested, tag 912, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastRptRequested, bool> LastRptRequested                        { get; set; } = ValidateLastRptRequested;

	/// <summary>Holds a FIX 4.4 AgreementDesc, tag 913, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AgreementDesc, bool> AgreementDesc                           { get; set; } = ValidateAgreementDesc;

	/// <summary>Holds a FIX 4.4 AgreementID, tag 914, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AgreementID, bool> AgreementID                             { get; set; } = ValidateAgreementID;

	/// <summary>Holds a FIX 4.4 AgreementDate, tag 915, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AgreementDate, bool> AgreementDate                           { get; set; } = ValidateAgreementDate;

	/// <summary>Holds a FIX 4.4 StartDate, tag 916, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StartDate, bool> StartDate                               { get; set; } = ValidateStartDate;

	/// <summary>Holds a FIX 4.4 EndDate, tag 917, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EndDate, bool> EndDate                                 { get; set; } = ValidateEndDate;

	/// <summary>Holds a FIX 4.4 AgreementCurrency, tag 918, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.AgreementCurrency, bool> AgreementCurrency                       { get; set; } = ValidateAgreementCurrency;

	/// <summary>Holds a FIX 4.4 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeliveryType, bool> DeliveryType                            { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 4.4 EndAccruedInterestAmt, tag 920, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EndAccruedInterestAmt, bool> EndAccruedInterestAmt                   { get; set; } = ValidateEndAccruedInterestAmt;

	/// <summary>Holds a FIX 4.4 StartCash, tag 921, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StartCash, bool> StartCash                               { get; set; } = ValidateStartCash;

	/// <summary>Holds a FIX 4.4 EndCash, tag 922, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.EndCash, bool> EndCash                                 { get; set; } = ValidateEndCash;

	/// <summary>Holds a FIX 4.4 UserRequestID, tag 923, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UserRequestID, bool> UserRequestID                           { get; set; } = ValidateUserRequestID;

	/// <summary>Holds a FIX 4.4 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.UserRequestType, bool> UserRequestType                         { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 4.4 NewPassword, tag 925, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NewPassword, bool> NewPassword                             { get; set; } = ValidateNewPassword;

	/// <summary>Holds a FIX 4.4 UserStatus, tag 926, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.UserStatus, bool> UserStatus                              { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 4.4 UserStatusText, tag 927, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UserStatusText, bool> UserStatusText                          { get; set; } = ValidateUserStatusText;

	/// <summary>Holds a FIX 4.4 StatusValue, tag 928, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StatusValue, bool> StatusValue                             { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 4.4 StatusText, tag 929, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StatusText, bool> StatusText                              { get; set; } = ValidateStatusText;

	/// <summary>Holds a FIX 4.4 RefCompID, tag 930, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefCompID, bool> RefCompID                               { get; set; } = ValidateRefCompID;

	/// <summary>Holds a FIX 4.4 RefSubID, tag 931, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.RefSubID, bool> RefSubID                                { get; set; } = ValidateRefSubID;

	/// <summary>Holds a FIX 4.4 NetworkResponseID, tag 932, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkResponseID, bool> NetworkResponseID                       { get; set; } = ValidateNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestID, tag 933, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkRequestID, bool> NetworkRequestID                        { get; set; } = ValidateNetworkRequestID;

	/// <summary>Holds a FIX 4.4 LastNetworkResponseID, tag 934, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LastNetworkResponseID, bool> LastNetworkResponseID                   { get; set; } = ValidateLastNetworkResponseID;

	/// <summary>Holds a FIX 4.4 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkRequestType, bool> NetworkRequestType                      { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 4.4 NoCompIDs, tag 936, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoCompIDs, bool> NoCompIDs                               { get; set; } = ValidateNoCompIDs;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkStatusResponseType, bool> NetworkStatusResponseType               { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 4.4 NoCollInquiryQualifier, tag 938, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoCollInquiryQualifier, bool> NoCollInquiryQualifier                  { get; set; } = ValidateNoCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRptStatus, bool> TrdRptStatus                            { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 4.4 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AffirmStatus, bool> AffirmStatus                            { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 4.4 UnderlyingStrikeCurrency, tag 941, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.UnderlyingStrikeCurrency, bool> UnderlyingStrikeCurrency                { get; set; } = ValidateUnderlyingStrikeCurrency;

	/// <summary>Holds a FIX 4.4 LegStrikeCurrency, tag 942, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegStrikeCurrency, bool> LegStrikeCurrency                       { get; set; } = ValidateLegStrikeCurrency;

	/// <summary>Holds a FIX 4.4 TimeBracket, tag 943, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.TimeBracket, bool> TimeBracket                             { get; set; } = ValidateTimeBracket;

	/// <summary>Holds a FIX 4.4 CollAction, tag 944, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAction, bool> CollAction                              { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 4.4 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryStatus, bool> CollInquiryStatus                       { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 4.4 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryResult, bool> CollInquiryResult                       { get; set; } = ValidateCollInquiryResult;

	/// <summary>Holds a FIX 4.4 StrikeCurrency, tag 947, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.StrikeCurrency, bool> StrikeCurrency                          { get; set; } = ValidateStrikeCurrency;

	/// <summary>Holds a FIX 4.4 NoNested3PartyIDs, tag 948, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNested3PartyIDs, bool> NoNested3PartyIDs                       { get; set; } = ValidateNoNested3PartyIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartyID, tag 949, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested3PartyID, bool> Nested3PartyID                          { get; set; } = ValidateNested3PartyID;

	/// <summary>Holds a FIX 4.4 Nested3PartyIDSource, tag 950, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested3PartyIDSource, bool> Nested3PartyIDSource                    { get; set; } = ValidateNested3PartyIDSource;

	/// <summary>Holds a FIX 4.4 Nested3PartyRole, tag 951, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested3PartyRole, bool> Nested3PartyRole                        { get; set; } = ValidateNested3PartyRole;

	/// <summary>Holds a FIX 4.4 NoNested3PartySubIDs, tag 952, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.NoNested3PartySubIDs, bool> NoNested3PartySubIDs                    { get; set; } = ValidateNoNested3PartySubIDs;

	/// <summary>Holds a FIX 4.4 Nested3PartySubID, tag 953, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested3PartySubID, bool> Nested3PartySubID                       { get; set; } = ValidateNested3PartySubID;

	/// <summary>Holds a FIX 4.4 Nested3PartySubIDType, tag 954, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.Nested3PartySubIDType, bool> Nested3PartySubIDType                   { get; set; } = ValidateNested3PartySubIDType;

	/// <summary>Holds a FIX 4.4 LegContractSettlMonth, tag 955, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegContractSettlMonth, bool> LegContractSettlMonth                   { get; set; } = ValidateLegContractSettlMonth;

	/// <summary>Holds a FIX 4.4 LegInterestAccrualDate, tag 956, to its type.</summary>
	public Func<FixContext, FixMessage, FixField.LegInterestAccrualDate, bool> LegInterestAccrualDate                  { get; set; } = ValidateLegInterestAccrualDate;

	static bool ValidateAccount(FixContext context, FixMessage message, FixField.Account field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvId(FixContext context, FixMessage message, FixField.AdvId field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvRefID(FixContext context, FixMessage message, FixField.AdvRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvSide(FixContext context, FixMessage message, FixField.AdvSide field)
	{
		if (field.Value is not ('B' or 'S' or 'T' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdvTransType(FixContext context, FixMessage message, FixField.AdvTransType field)
	{
		if (field.Value is not ("C" or "N" or "R"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPx(FixContext context, FixMessage message, FixField.AvgPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginSeqNo(FixContext context, FixMessage message, FixField.BeginSeqNo field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBeginString(FixContext context, FixMessage message, FixField.BeginString field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBodyLength(FixContext context, FixMessage message, FixField.BodyLength field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCheckSum(FixContext context, FixMessage message, FixField.CheckSum field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdID(FixContext context, FixMessage message, FixField.ClOrdID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommission(FixContext context, FixMessage message, FixField.Commission field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommType(FixContext context, FixMessage message, FixField.CommType field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCumQty(FixContext context, FixMessage message, FixField.CumQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCurrency(FixContext context, FixMessage message, FixField.Currency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndSeqNo(FixContext context, FixMessage message, FixField.EndSeqNo field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecID(FixContext context, FixMessage message, FixField.ExecID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecInst(FixContext context, FixMessage message, FixField.ExecInst field)
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

	static bool ValidateExecRefID(FixContext context, FixMessage message, FixField.ExecRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHandlInst(FixContext context, FixMessage message, FixField.HandlInst field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityIDSource(FixContext context, FixMessage message, FixField.SecurityIDSource field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A" or "B" or "C" or
			"D" or "E" or "F" or "G" or "H" or "I" or "J"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIid(FixContext context, FixMessage message, FixField.IOIid field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQltyInd(FixContext context, FixMessage message, FixField.IOIQltyInd field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIRefID(FixContext context, FixMessage message, FixField.IOIRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQty(FixContext context, FixMessage message, FixField.IOIQty field)
	{
		if (field.Value is not ("L" or "M" or "S"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOITransType(FixContext context, FixMessage message, FixField.IOITransType field)
	{
		if (field.Value is not ('C' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastCapacity(FixContext context, FixMessage message, FixField.LastCapacity field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMkt(FixContext context, FixMessage message, FixField.LastMkt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastPx(FixContext context, FixMessage message, FixField.LastPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastQty(FixContext context, FixMessage message, FixField.LastQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLinesOfText(FixContext context, FixMessage message, FixField.LinesOfText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgSeqNum(FixContext context, FixMessage message, FixField.MsgSeqNum field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgType(FixContext context, FixMessage message, FixField.MsgType field)
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

	static bool ValidateNewSeqNo(FixContext context, FixMessage message, FixField.NewSeqNo field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderID(FixContext context, FixMessage message, FixField.OrderID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty(FixContext context, FixMessage message, FixField.OrderQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatus(FixContext context, FixMessage message, FixField.OrdStatus field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdType(FixContext context, FixMessage message, FixField.OrdType field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '6' or '7' or '8' or '9' or 'D' or 'E' or 'G' or 'I' or
			'J' or 'K' or 'L' or 'M' or 'P'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigClOrdID(FixContext context, FixMessage message, FixField.OrigClOrdID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigTime(FixContext context, FixMessage message, FixField.OrigTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossDupFlag(FixContext context, FixMessage message, FixField.PossDupFlag field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice(FixContext context, FixMessage message, FixField.Price field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSeqNum(FixContext context, FixMessage message, FixField.RefSeqNum field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityID(FixContext context, FixMessage message, FixField.SecurityID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderCompID(FixContext context, FixMessage message, FixField.SenderCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderSubID(FixContext context, FixMessage message, FixField.SenderSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSendingTime(FixContext context, FixMessage message, FixField.SendingTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuantity(FixContext context, FixMessage message, FixField.Quantity field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSide(FixContext context, FixMessage message, FixField.Side field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbol(FixContext context, FixMessage message, FixField.Symbol field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetCompID(FixContext context, FixMessage message, FixField.TargetCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetSubID(FixContext context, FixMessage message, FixField.TargetSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateText(FixContext context, FixMessage message, FixField.Text field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeInForce(FixContext context, FixMessage message, FixField.TimeInForce field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransactTime(FixContext context, FixMessage message, FixField.TransactTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUrgency(FixContext context, FixMessage message, FixField.Urgency field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValidUntilTime(FixContext context, FixMessage message, FixField.ValidUntilTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlType(FixContext context, FixMessage message, FixField.SettlType field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate(FixContext context, FixMessage message, FixField.SettlDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSymbolSfx(FixContext context, FixMessage message, FixField.SymbolSfx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListID(FixContext context, FixMessage message, FixField.ListID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListSeqNo(FixContext context, FixMessage message, FixField.ListSeqNo field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoOrders(FixContext context, FixMessage message, FixField.TotNoOrders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInst(FixContext context, FixMessage message, FixField.ListExecInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocID(FixContext context, FixMessage message, FixField.AllocID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocTransType(FixContext context, FixMessage message, FixField.AllocTransType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefAllocID(FixContext context, FixMessage message, FixField.RefAllocID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoOrders(FixContext context, FixMessage message, FixField.NoOrders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxPrecision(FixContext context, FixMessage message, FixField.AvgPxPrecision field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeDate(FixContext context, FixMessage message, FixField.TradeDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionEffect(FixContext context, FixMessage message, FixField.PositionEffect field)
	{
		if (field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAllocs(FixContext context, FixMessage message, FixField.NoAllocs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccount(FixContext context, FixMessage message, FixField.AllocAccount field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocQty(FixContext context, FixMessage message, FixField.AllocQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProcessCode(FixContext context, FixMessage message, FixField.ProcessCode field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRpts(FixContext context, FixMessage message, FixField.NoRpts field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRptSeq(FixContext context, FixMessage message, FixField.RptSeq field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlQty(FixContext context, FixMessage message, FixField.CxlQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDlvyInst(FixContext context, FixMessage message, FixField.NoDlvyInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocStatus(FixContext context, FixMessage message, FixField.AllocStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocRejCode(FixContext context, FixMessage message, FixField.AllocRejCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignature(FixContext context, FixMessage message, FixField.Signature field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureDataLen(FixContext context, FixMessage message, FixField.SecureDataLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecureData(FixContext context, FixMessage message, FixField.SecureData field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSignatureLength(FixContext context, FixMessage message, FixField.SignatureLength field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailType(FixContext context, FixMessage message, FixField.EmailType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawDataLength(FixContext context, FixMessage message, FixField.RawDataLength field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRawData(FixContext context, FixMessage message, FixField.RawData field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePossResend(FixContext context, FixMessage message, FixField.PossResend field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptMethod(FixContext context, FixMessage message, FixField.EncryptMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStopPx(FixContext context, FixMessage message, FixField.StopPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDestination(FixContext context, FixMessage message, FixField.ExDestination field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejReason(FixContext context, FixMessage message, FixField.CxlRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdRejReason(FixContext context, FixMessage message, FixField.OrdRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOIQualifier(FixContext context, FixMessage message, FixField.IOIQualifier field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'I' or 'L' or 'M' or 'O' or 'P' or 'Q' or 'R' or 'S' or
			'T' or 'V' or 'W' or 'X' or 'Y' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssuer(FixContext context, FixMessage message, FixField.Issuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityDesc(FixContext context, FixMessage message, FixField.SecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeartBtInt(FixContext context, FixMessage message, FixField.HeartBtInt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinQty(FixContext context, FixMessage message, FixField.MinQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxFloor(FixContext context, FixMessage message, FixField.MaxFloor field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestReqID(FixContext context, FixMessage message, FixField.TestReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportToExch(FixContext context, FixMessage message, FixField.ReportToExch field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocateReqd(FixContext context, FixMessage message, FixField.LocateReqd field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfCompID(FixContext context, FixMessage message, FixField.OnBehalfOfCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfSubID(FixContext context, FixMessage message, FixField.OnBehalfOfSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteID(FixContext context, FixMessage message, FixField.QuoteID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetMoney(FixContext context, FixMessage message, FixField.NetMoney field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrAmt(FixContext context, FixMessage message, FixField.SettlCurrAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrency(FixContext context, FixMessage message, FixField.SettlCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateForexReq(FixContext context, FixMessage message, FixField.ForexReq field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigSendingTime(FixContext context, FixMessage message, FixField.OrigSendingTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGapFillFlag(FixContext context, FixMessage message, FixField.GapFillFlag field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoExecs(FixContext context, FixMessage message, FixField.NoExecs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireTime(FixContext context, FixMessage message, FixField.ExpireTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDKReason(FixContext context, FixMessage message, FixField.DKReason field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToCompID(FixContext context, FixMessage message, FixField.DeliverToCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToSubID(FixContext context, FixMessage message, FixField.DeliverToSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIOINaturalFlag(FixContext context, FixMessage message, FixField.IOINaturalFlag field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteReqID(FixContext context, FixMessage message, FixField.QuoteReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidPx(FixContext context, FixMessage message, FixField.BidPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferPx(FixContext context, FixMessage message, FixField.OfferPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSize(FixContext context, FixMessage message, FixField.BidSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSize(FixContext context, FixMessage message, FixField.OfferSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMiscFees(FixContext context, FixMessage message, FixField.NoMiscFees field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeAmt(FixContext context, FixMessage message, FixField.MiscFeeAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeCurr(FixContext context, FixMessage message, FixField.MiscFeeCurr field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeType(FixContext context, FixMessage message, FixField.MiscFeeType field)
	{
		if (field.Value is not ("1" or "10" or "11" or "12" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrevClosePx(FixContext context, FixMessage message, FixField.PrevClosePx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResetSeqNumFlag(FixContext context, FixMessage message, FixField.ResetSeqNumFlag field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSenderLocationID(FixContext context, FixMessage message, FixField.SenderLocationID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetLocationID(FixContext context, FixMessage message, FixField.TargetLocationID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOnBehalfOfLocationID(FixContext context, FixMessage message, FixField.OnBehalfOfLocationID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliverToLocationID(FixContext context, FixMessage message, FixField.DeliverToLocationID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRelatedSym(FixContext context, FixMessage message, FixField.NoRelatedSym field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubject(FixContext context, FixMessage message, FixField.Subject field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHeadline(FixContext context, FixMessage message, FixField.Headline field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateURLLink(FixContext context, FixMessage message, FixField.URLLink field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecType(FixContext context, FixMessage message, FixField.ExecType field)
	{
		if (field.Value is not ('0' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLeavesQty(FixContext context, FixMessage message, FixField.LeavesQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOrderQty(FixContext context, FixMessage message, FixField.CashOrderQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAvgPx(FixContext context, FixMessage message, FixField.AllocAvgPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNetMoney(FixContext context, FixMessage message, FixField.AllocNetMoney field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRate(FixContext context, FixMessage message, FixField.SettlCurrFxRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrFxRateCalc(FixContext context, FixMessage message, FixField.SettlCurrFxRateCalc field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumDaysInterest(FixContext context, FixMessage message, FixField.NumDaysInterest field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestRate(FixContext context, FixMessage message, FixField.AccruedInterestRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccruedInterestAmt(FixContext context, FixMessage message, FixField.AccruedInterestAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMode(FixContext context, FixMessage message, FixField.SettlInstMode field)
	{
		if (field.Value is not ('1' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocText(FixContext context, FixMessage message, FixField.AllocText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstID(FixContext context, FixMessage message, FixField.SettlInstID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstTransType(FixContext context, FixMessage message, FixField.SettlInstTransType field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailThreadID(FixContext context, FixMessage message, FixField.EmailThreadID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstSource(FixContext context, FixMessage message, FixField.SettlInstSource field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityType(FixContext context, FixMessage message, FixField.SecurityType field)
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

	static bool ValidateEffectiveTime(FixContext context, FixMessage message, FixField.EffectiveTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbType(FixContext context, FixMessage message, FixField.StandInstDbType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbName(FixContext context, FixMessage message, FixField.StandInstDbName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStandInstDbID(FixContext context, FixMessage message, FixField.StandInstDbID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDeliveryType(FixContext context, FixMessage message, FixField.SettlDeliveryType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidSpotRate(FixContext context, FixMessage message, FixField.BidSpotRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints(FixContext context, FixMessage message, FixField.BidForwardPoints field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferSpotRate(FixContext context, FixMessage message, FixField.OfferSpotRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints(FixContext context, FixMessage message, FixField.OfferForwardPoints field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderQty2(FixContext context, FixMessage message, FixField.OrderQty2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDate2(FixContext context, FixMessage message, FixField.SettlDate2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastSpotRate(FixContext context, FixMessage message, FixField.LastSpotRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints(FixContext context, FixMessage message, FixField.LastForwardPoints field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkID(FixContext context, FixMessage message, FixField.AllocLinkID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkType(FixContext context, FixMessage message, FixField.AllocLinkType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryOrderID(FixContext context, FixMessage message, FixField.SecondaryOrderID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoIOIQualifiers(FixContext context, FixMessage message, FixField.NoIOIQualifiers field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityMonthYear(FixContext context, FixMessage message, FixField.MaturityMonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePutOrCall(FixContext context, FixMessage message, FixField.PutOrCall field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikePrice(FixContext context, FixMessage message, FixField.StrikePrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCoveredOrUncovered(FixContext context, FixMessage message, FixField.CoveredOrUncovered field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOptAttribute(FixContext context, FixMessage message, FixField.OptAttribute field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityExchange(FixContext context, FixMessage message, FixField.SecurityExchange field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNotifyBrokerOfCredit(FixContext context, FixMessage message, FixField.NotifyBrokerOfCredit field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocHandlInst(FixContext context, FixMessage message, FixField.AllocHandlInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxShow(FixContext context, FixMessage message, FixField.MaxShow field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetValue(FixContext context, FixMessage message, FixField.PegOffsetValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlDataLen(FixContext context, FixMessage message, FixField.XmlDataLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateXmlData(FixContext context, FixMessage message, FixField.XmlData field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstRefID(FixContext context, FixMessage message, FixField.SettlInstRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRoutingIDs(FixContext context, FixMessage message, FixField.NoRoutingIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingType(FixContext context, FixMessage message, FixField.RoutingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingID(FixContext context, FixMessage message, FixField.RoutingID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSpread(FixContext context, FixMessage message, FixField.Spread field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveCurrency(FixContext context, FixMessage message, FixField.BenchmarkCurveCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurveName(FixContext context, FixMessage message, FixField.BenchmarkCurveName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkCurvePoint(FixContext context, FixMessage message, FixField.BenchmarkCurvePoint field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponRate(FixContext context, FixMessage message, FixField.CouponRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCouponPaymentDate(FixContext context, FixMessage message, FixField.CouponPaymentDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIssueDate(FixContext context, FixMessage message, FixField.IssueDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseTerm(FixContext context, FixMessage message, FixField.RepurchaseTerm field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepurchaseRate(FixContext context, FixMessage message, FixField.RepurchaseRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFactor(FixContext context, FixMessage message, FixField.Factor field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeOriginationDate(FixContext context, FixMessage message, FixField.TradeOriginationDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExDate(FixContext context, FixMessage message, FixField.ExDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractMultiplier(FixContext context, FixMessage message, FixField.ContractMultiplier field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStipulations(FixContext context, FixMessage message, FixField.NoStipulations field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStipulationType(FixContext context, FixMessage message, FixField.StipulationType field)
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

	static bool ValidateStipulationValue(FixContext context, FixMessage message, FixField.StipulationValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldType(FixContext context, FixMessage message, FixField.YieldType field)
	{
		if (field.Value is not ("AFTERTAX" or "ANNUAL" or "ATISSUE" or "AVGMATURITY" or "BOOK" or "CALL" or
			"CHANGE" or "CLOSE" or "COMPOUND" or "CURRENT" or "GOVTEQUIV" or "GROSS" or "INFLATION" or
			"INVERSEFLOATER" or "LASTCLOSE" or "LASTMONTH" or "LASTQUARTER" or "LASTYEAR" or "LONGAVGLIFE" or "MARK" or
			"MATURITY" or "NEXTREFUND" or "OPENAVG" or "PREVCLOSE" or "PROCEEDS" or "PUT" or "SEMIANNUAL" or
			"SHORTAVGLIFE" or "SIMPLE" or "TAXEQUIV" or "TENDER" or "TRUE" or "VALUE1/32" or "WORST"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYield(FixContext context, FixMessage message, FixField.Yield field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalTakedown(FixContext context, FixMessage message, FixField.TotalTakedown field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConcession(FixContext context, FixMessage message, FixField.Concession field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRepoCollateralSecurityType(FixContext context, FixMessage message, FixField.RepoCollateralSecurityType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRedemptionDate(FixContext context, FixMessage message, FixField.RedemptionDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponPaymentDate(FixContext context, FixMessage message, FixField.UnderlyingCouponPaymentDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssueDate(FixContext context, FixMessage message, FixField.UnderlyingIssueDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepoCollateralSecurityType(FixContext context, FixMessage message, FixField.UnderlyingRepoCollateralSecurityType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseTerm(FixContext context, FixMessage message, FixField.UnderlyingRepurchaseTerm field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRepurchaseRate(FixContext context, FixMessage message, FixField.UnderlyingRepurchaseRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingFactor(FixContext context, FixMessage message, FixField.UnderlyingFactor field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingRedemptionDate(FixContext context, FixMessage message, FixField.UnderlyingRedemptionDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponPaymentDate(FixContext context, FixMessage message, FixField.LegCouponPaymentDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssueDate(FixContext context, FixMessage message, FixField.LegIssueDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepoCollateralSecurityType(FixContext context, FixMessage message, FixField.LegRepoCollateralSecurityType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseTerm(FixContext context, FixMessage message, FixField.LegRepurchaseTerm field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRepurchaseRate(FixContext context, FixMessage message, FixField.LegRepurchaseRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegFactor(FixContext context, FixMessage message, FixField.LegFactor field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRedemptionDate(FixContext context, FixMessage message, FixField.LegRedemptionDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCreditRating(FixContext context, FixMessage message, FixField.CreditRating field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCreditRating(FixContext context, FixMessage message, FixField.UnderlyingCreditRating field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCreditRating(FixContext context, FixMessage message, FixField.LegCreditRating field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradedFlatSwitch(FixContext context, FixMessage message, FixField.TradedFlatSwitch field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeatureDate(FixContext context, FixMessage message, FixField.BasisFeatureDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisFeaturePrice(FixContext context, FixMessage message, FixField.BasisFeaturePrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqID(FixContext context, FixMessage message, FixField.MDReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSubscriptionRequestType(FixContext context, FixMessage message, FixField.SubscriptionRequestType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarketDepth(FixContext context, FixMessage message, FixField.MarketDepth field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateType(FixContext context, FixMessage message, FixField.MDUpdateType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAggregatedBook(FixContext context, FixMessage message, FixField.AggregatedBook field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntryTypes(FixContext context, FixMessage message, FixField.NoMDEntryTypes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMDEntries(FixContext context, FixMessage message, FixField.NoMDEntries field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryType(FixContext context, FixMessage message, FixField.MDEntryType field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPx(FixContext context, FixMessage message, FixField.MDEntryPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySize(FixContext context, FixMessage message, FixField.MDEntrySize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryDate(FixContext context, FixMessage message, FixField.MDEntryDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryTime(FixContext context, FixMessage message, FixField.MDEntryTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTickDirection(FixContext context, FixMessage message, FixField.TickDirection field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDMkt(FixContext context, FixMessage message, FixField.MDMkt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCondition(FixContext context, FixMessage message, FixField.QuoteCondition field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E" or "F" or "G" or "H" or "I"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateTradeCondition(FixContext context, FixMessage message, FixField.TradeCondition field)
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

	static bool ValidateMDEntryID(FixContext context, FixMessage message, FixField.MDEntryID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateAction(FixContext context, FixMessage message, FixField.MDUpdateAction field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryRefID(FixContext context, FixMessage message, FixField.MDEntryRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDReqRejReason(FixContext context, FixMessage message, FixField.MDReqRejReason field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or
			'C'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryOriginator(FixContext context, FixMessage message, FixField.MDEntryOriginator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocationID(FixContext context, FixMessage message, FixField.LocationID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeskID(FixContext context, FixMessage message, FixField.DeskID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeleteReason(FixContext context, FixMessage message, FixField.DeleteReason field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenCloseSettlFlag(FixContext context, FixMessage message, FixField.OpenCloseSettlFlag field)
	{
		foreach (var code in field.Value)
			if (code is not ("0" or "1" or "2" or "3" or "4" or "5"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateSellerDays(FixContext context, FixMessage message, FixField.SellerDays field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryBuyer(FixContext context, FixMessage message, FixField.MDEntryBuyer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntrySeller(FixContext context, FixMessage message, FixField.MDEntrySeller field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDEntryPositionNo(FixContext context, FixMessage message, FixField.MDEntryPositionNo field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFinancialStatus(FixContext context, FixMessage message, FixField.FinancialStatus field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateCorporateAction(FixContext context, FixMessage message, FixField.CorporateAction field)
	{
		foreach (var code in field.Value)
			if (code is not ("A" or "B" or "C" or "D" or "E"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateDefBidSize(FixContext context, FixMessage message, FixField.DefBidSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDefOfferSize(FixContext context, FixMessage message, FixField.DefOfferSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteEntries(FixContext context, FixMessage message, FixField.NoQuoteEntries field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteSets(FixContext context, FixMessage message, FixField.NoQuoteSets field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatus(FixContext context, FixMessage message, FixField.QuoteStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or
			6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCancelType(FixContext context, FixMessage message, FixField.QuoteCancelType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryID(FixContext context, FixMessage message, FixField.QuoteEntryID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRejectReason(FixContext context, FixMessage message, FixField.QuoteRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteResponseLevel(FixContext context, FixMessage message, FixField.QuoteResponseLevel field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetID(FixContext context, FixMessage message, FixField.QuoteSetID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestType(FixContext context, FixMessage message, FixField.QuoteRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoQuoteEntries(FixContext context, FixMessage message, FixField.TotNoQuoteEntries field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityIDSource(FixContext context, FixMessage message, FixField.UnderlyingSecurityIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingIssuer(FixContext context, FixMessage message, FixField.UnderlyingIssuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityDesc(FixContext context, FixMessage message, FixField.UnderlyingSecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityExchange(FixContext context, FixMessage message, FixField.UnderlyingSecurityExchange field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityID(FixContext context, FixMessage message, FixField.UnderlyingSecurityID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityType(FixContext context, FixMessage message, FixField.UnderlyingSecurityType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbol(FixContext context, FixMessage message, FixField.UnderlyingSymbol field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSymbolSfx(FixContext context, FixMessage message, FixField.UnderlyingSymbolSfx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityMonthYear(FixContext context, FixMessage message, FixField.UnderlyingMaturityMonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPutOrCall(FixContext context, FixMessage message, FixField.UnderlyingPutOrCall field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikePrice(FixContext context, FixMessage message, FixField.UnderlyingStrikePrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingOptAttribute(FixContext context, FixMessage message, FixField.UnderlyingOptAttribute field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrency(FixContext context, FixMessage message, FixField.UnderlyingCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityReqID(FixContext context, FixMessage message, FixField.SecurityReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestType(FixContext context, FixMessage message, FixField.SecurityRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseID(FixContext context, FixMessage message, FixField.SecurityResponseID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseType(FixContext context, FixMessage message, FixField.SecurityResponseType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusReqID(FixContext context, FixMessage message, FixField.SecurityStatusReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnsolicitedIndicator(FixContext context, FixMessage message, FixField.UnsolicitedIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingStatus(FixContext context, FixMessage message, FixField.SecurityTradingStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 2 or
			20 or 21 or 22 or 23 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHaltReason(FixContext context, FixMessage message, FixField.HaltReason field)
	{
		if (field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInViewOfCommon(FixContext context, FixMessage message, FixField.InViewOfCommon field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDueToRelated(FixContext context, FixMessage message, FixField.DueToRelated field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBuyVolume(FixContext context, FixMessage message, FixField.BuyVolume field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSellVolume(FixContext context, FixMessage message, FixField.SellVolume field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHighPx(FixContext context, FixMessage message, FixField.HighPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLowPx(FixContext context, FixMessage message, FixField.LowPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustment(FixContext context, FixMessage message, FixField.Adjustment field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesReqID(FixContext context, FixMessage message, FixField.TradSesReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionID(FixContext context, FixMessage message, FixField.TradingSessionID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTrader(FixContext context, FixMessage message, FixField.ContraTrader field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMethod(FixContext context, FixMessage message, FixField.TradSesMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMode(FixContext context, FixMessage message, FixField.TradSesMode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatus(FixContext context, FixMessage message, FixField.TradSesStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStartTime(FixContext context, FixMessage message, FixField.TradSesStartTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesOpenTime(FixContext context, FixMessage message, FixField.TradSesOpenTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesPreCloseTime(FixContext context, FixMessage message, FixField.TradSesPreCloseTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesCloseTime(FixContext context, FixMessage message, FixField.TradSesCloseTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesEndTime(FixContext context, FixMessage message, FixField.TradSesEndTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumberOfOrders(FixContext context, FixMessage message, FixField.NumberOfOrders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEncoding(FixContext context, FixMessage message, FixField.MessageEncoding field)
	{
		if (field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuerLen(FixContext context, FixMessage message, FixField.EncodedIssuerLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedIssuer(FixContext context, FixMessage message, FixField.EncodedIssuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDescLen(FixContext context, FixMessage message, FixField.EncodedSecurityDescLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSecurityDesc(FixContext context, FixMessage message, FixField.EncodedSecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInstLen(FixContext context, FixMessage message, FixField.EncodedListExecInstLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListExecInst(FixContext context, FixMessage message, FixField.EncodedListExecInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedTextLen(FixContext context, FixMessage message, FixField.EncodedTextLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedText(FixContext context, FixMessage message, FixField.EncodedText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubjectLen(FixContext context, FixMessage message, FixField.EncodedSubjectLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedSubject(FixContext context, FixMessage message, FixField.EncodedSubject field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadlineLen(FixContext context, FixMessage message, FixField.EncodedHeadlineLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedHeadline(FixContext context, FixMessage message, FixField.EncodedHeadline field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocTextLen(FixContext context, FixMessage message, FixField.EncodedAllocTextLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedAllocText(FixContext context, FixMessage message, FixField.EncodedAllocText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuerLen(FixContext context, FixMessage message, FixField.EncodedUnderlyingIssuerLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingIssuer(FixContext context, FixMessage message, FixField.EncodedUnderlyingIssuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDescLen(FixContext context, FixMessage message, FixField.EncodedUnderlyingSecurityDescLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedUnderlyingSecurityDesc(FixContext context, FixMessage message, FixField.EncodedUnderlyingSecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocPrice(FixContext context, FixMessage message, FixField.AllocPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteSetValidUntilTime(FixContext context, FixMessage message, FixField.QuoteSetValidUntilTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteEntryRejectReason(FixContext context, FixMessage message, FixField.QuoteEntryRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastMsgSeqNumProcessed(FixContext context, FixMessage message, FixField.LastMsgSeqNumProcessed field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefTagID(FixContext context, FixMessage message, FixField.RefTagID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefMsgType(FixContext context, FixMessage message, FixField.RefMsgType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionRejectReason(FixContext context, FixMessage message, FixField.SessionRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 2 or
			3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidRequestTransType(FixContext context, FixMessage message, FixField.BidRequestTransType field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraBroker(FixContext context, FixMessage message, FixField.ContraBroker field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateComplianceID(FixContext context, FixMessage message, FixField.ComplianceID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSolicitedFlag(FixContext context, FixMessage message, FixField.SolicitedFlag field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecRestatementReason(FixContext context, FixMessage message, FixField.ExecRestatementReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectRefID(FixContext context, FixMessage message, FixField.BusinessRejectRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectReason(FixContext context, FixMessage message, FixField.BusinessRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGrossTradeAmt(FixContext context, FixMessage message, FixField.GrossTradeAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContraBrokers(FixContext context, FixMessage message, FixField.NoContraBrokers field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaxMessageSize(FixContext context, FixMessage message, FixField.MaxMessageSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoMsgTypes(FixContext context, FixMessage message, FixField.NoMsgTypes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgDirection(FixContext context, FixMessage message, FixField.MsgDirection field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTradingSessions(FixContext context, FixMessage message, FixField.NoTradingSessions field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalVolumeTraded(FixContext context, FixMessage message, FixField.TotalVolumeTraded field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionInst(FixContext context, FixMessage message, FixField.DiscretionInst field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetValue(FixContext context, FixMessage message, FixField.DiscretionOffsetValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidID(FixContext context, FixMessage message, FixField.BidID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClientBidID(FixContext context, FixMessage message, FixField.ClientBidID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListName(FixContext context, FixMessage message, FixField.ListName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoRelatedSym(FixContext context, FixMessage message, FixField.TotNoRelatedSym field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidType(FixContext context, FixMessage message, FixField.BidType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumTickets(FixContext context, FixMessage message, FixField.NumTickets field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue1(FixContext context, FixMessage message, FixField.SideValue1 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValue2(FixContext context, FixMessage message, FixField.SideValue2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidDescriptors(FixContext context, FixMessage message, FixField.NoBidDescriptors field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptorType(FixContext context, FixMessage message, FixField.BidDescriptorType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptor(FixContext context, FixMessage message, FixField.BidDescriptor field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValueInd(FixContext context, FixMessage message, FixField.SideValueInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctLow(FixContext context, FixMessage message, FixField.LiquidityPctLow field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityPctHigh(FixContext context, FixMessage message, FixField.LiquidityPctHigh field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityValue(FixContext context, FixMessage message, FixField.LiquidityValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEFPTrackingError(FixContext context, FixMessage message, FixField.EFPTrackingError field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFairValue(FixContext context, FixMessage message, FixField.FairValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutsideIndexPct(FixContext context, FixMessage message, FixField.OutsideIndexPct field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateValueOfFutures(FixContext context, FixMessage message, FixField.ValueOfFutures field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityIndType(FixContext context, FixMessage message, FixField.LiquidityIndType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWtAverageLiquidity(FixContext context, FixMessage message, FixField.WtAverageLiquidity field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeForPhysical(FixContext context, FixMessage message, FixField.ExchangeForPhysical field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOutMainCntryUIndex(FixContext context, FixMessage message, FixField.OutMainCntryUIndex field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPercent(FixContext context, FixMessage message, FixField.CrossPercent field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgRptReqs(FixContext context, FixMessage message, FixField.ProgRptReqs field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgPeriodInterval(FixContext context, FixMessage message, FixField.ProgPeriodInterval field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIncTaxInd(FixContext context, FixMessage message, FixField.IncTaxInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNumBidders(FixContext context, FixMessage message, FixField.NumBidders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidTradeType(FixContext context, FixMessage message, FixField.BidTradeType field)
	{
		if (field.Value is not ('A' or 'G' or 'J' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBasisPxType(FixContext context, FixMessage message, FixField.BasisPxType field)
	{
		if (field.Value is not ('2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or 'D' or
			'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoBidComponents(FixContext context, FixMessage message, FixField.NoBidComponents field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountry(FixContext context, FixMessage message, FixField.Country field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoStrikes(FixContext context, FixMessage message, FixField.TotNoStrikes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceType(FixContext context, FixMessage message, FixField.PriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayOrderQty(FixContext context, FixMessage message, FixField.DayOrderQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayCumQty(FixContext context, FixMessage message, FixField.DayCumQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayAvgPx(FixContext context, FixMessage message, FixField.DayAvgPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGTBookingInst(FixContext context, FixMessage message, FixField.GTBookingInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoStrikes(FixContext context, FixMessage message, FixField.NoStrikes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusType(FixContext context, FixMessage message, FixField.ListStatusType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetGrossInd(FixContext context, FixMessage message, FixField.NetGrossInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListOrderStatus(FixContext context, FixMessage message, FixField.ListOrderStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpireDate(FixContext context, FixMessage message, FixField.ExpireDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListExecInstType(FixContext context, FixMessage message, FixField.ListExecInstType field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejResponseTo(FixContext context, FixMessage message, FixField.CxlRejResponseTo field)
	{
		if (field.Value is not ('1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCouponRate(FixContext context, FixMessage message, FixField.UnderlyingCouponRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingContractMultiplier(FixContext context, FixMessage message, FixField.UnderlyingContractMultiplier field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeQty(FixContext context, FixMessage message, FixField.ContraTradeQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraTradeTime(FixContext context, FixMessage message, FixField.ContraTradeTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityNumSecurities(FixContext context, FixMessage message, FixField.LiquidityNumSecurities field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegReportingType(FixContext context, FixMessage message, FixField.MultiLegReportingType field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeTime(FixContext context, FixMessage message, FixField.StrikeTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusText(FixContext context, FixMessage message, FixField.ListStatusText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusTextLen(FixContext context, FixMessage message, FixField.EncodedListStatusTextLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedListStatusText(FixContext context, FixMessage message, FixField.EncodedListStatusText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyIDSource(FixContext context, FixMessage message, FixField.PartyIDSource field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G' or 'H' or 'I'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyID(FixContext context, FixMessage message, FixField.PartyID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetChgPrevDay(FixContext context, FixMessage message, FixField.NetChgPrevDay field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartyRole(FixContext context, FixMessage message, FixField.PartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 30 or 31 or 32 or 33 or
			34 or 35 or 36 or 37 or 38 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartyIDs(FixContext context, FixMessage message, FixField.NoPartyIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityAltID(FixContext context, FixMessage message, FixField.NoSecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltID(FixContext context, FixMessage message, FixField.SecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityAltIDSource(FixContext context, FixMessage message, FixField.SecurityAltIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingSecurityAltID(FixContext context, FixMessage message, FixField.NoUnderlyingSecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltID(FixContext context, FixMessage message, FixField.UnderlyingSecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecurityAltIDSource(FixContext context, FixMessage message, FixField.UnderlyingSecurityAltIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProduct(FixContext context, FixMessage message, FixField.Product field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or
			9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCFICode(FixContext context, FixMessage message, FixField.CFICode field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingProduct(FixContext context, FixMessage message, FixField.UnderlyingProduct field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCFICode(FixContext context, FixMessage message, FixField.UnderlyingCFICode field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTestMessageIndicator(FixContext context, FixMessage message, FixField.TestMessageIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingRefID(FixContext context, FixMessage message, FixField.BookingRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocID(FixContext context, FixMessage message, FixField.IndividualAllocID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingDirection(FixContext context, FixMessage message, FixField.RoundingDirection field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingModulus(FixContext context, FixMessage message, FixField.RoundingModulus field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCountryOfIssue(FixContext context, FixMessage message, FixField.CountryOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStateOrProvinceOfIssue(FixContext context, FixMessage message, FixField.StateOrProvinceOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLocaleOfIssue(FixContext context, FixMessage message, FixField.LocaleOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoRegistDtls(FixContext context, FixMessage message, FixField.NoRegistDtls field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingDtls(FixContext context, FixMessage message, FixField.MailingDtls field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInvestorCountryOfResidence(FixContext context, FixMessage message, FixField.InvestorCountryOfResidence field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRef(FixContext context, FixMessage message, FixField.PaymentRef field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPaymentMethod(FixContext context, FixMessage message, FixField.DistribPaymentMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribCurr(FixContext context, FixMessage message, FixField.CashDistribCurr field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCommCurrency(FixContext context, FixMessage message, FixField.CommCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCancellationRights(FixContext context, FixMessage message, FixField.CancellationRights field)
	{
		if (field.Value is not ('M' or 'N' or 'O' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMoneyLaunderingStatus(FixContext context, FixMessage message, FixField.MoneyLaunderingStatus field)
	{
		if (field.Value is not ('1' or '2' or '3' or 'N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMailingInst(FixContext context, FixMessage message, FixField.MailingInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransBkdTime(FixContext context, FixMessage message, FixField.TransBkdTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceType(FixContext context, FixMessage message, FixField.ExecPriceType field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecPriceAdjustment(FixContext context, FixMessage message, FixField.ExecPriceAdjustment field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDateOfBirth(FixContext context, FixMessage message, FixField.DateOfBirth field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportTransType(FixContext context, FixMessage message, FixField.TradeReportTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardHolderName(FixContext context, FixMessage message, FixField.CardHolderName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardNumber(FixContext context, FixMessage message, FixField.CardNumber field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardExpDate(FixContext context, FixMessage message, FixField.CardExpDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardIssNum(FixContext context, FixMessage message, FixField.CardIssNum field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentMethod(FixContext context, FixMessage message, FixField.PaymentMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistAcctType(FixContext context, FixMessage message, FixField.RegistAcctType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDesignation(FixContext context, FixMessage message, FixField.Designation field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTaxAdvantageType(FixContext context, FixMessage message, FixField.TaxAdvantageType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or
			19 or 2 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonText(FixContext context, FixMessage message, FixField.RegistRejReasonText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFundRenewWaiv(FixContext context, FixMessage message, FixField.FundRenewWaiv field)
	{
		if (field.Value is not ('N' or 'Y'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentName(FixContext context, FixMessage message, FixField.CashDistribAgentName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentCode(FixContext context, FixMessage message, FixField.CashDistribAgentCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctNumber(FixContext context, FixMessage message, FixField.CashDistribAgentAcctNumber field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribPayRef(FixContext context, FixMessage message, FixField.CashDistribPayRef field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashDistribAgentAcctName(FixContext context, FixMessage message, FixField.CashDistribAgentAcctName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCardStartDate(FixContext context, FixMessage message, FixField.CardStartDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentDate(FixContext context, FixMessage message, FixField.PaymentDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentRemitterID(FixContext context, FixMessage message, FixField.PaymentRemitterID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistStatus(FixContext context, FixMessage message, FixField.RegistStatus field)
	{
		if (field.Value is not ('A' or 'H' or 'N' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRejReasonCode(FixContext context, FixMessage message, FixField.RegistRejReasonCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 2 or
			3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistRefID(FixContext context, FixMessage message, FixField.RegistRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistDtls(FixContext context, FixMessage message, FixField.RegistDtls field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDistribInsts(FixContext context, FixMessage message, FixField.NoDistribInsts field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistEmail(FixContext context, FixMessage message, FixField.RegistEmail field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPercentage(FixContext context, FixMessage message, FixField.DistribPercentage field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistID(FixContext context, FixMessage message, FixField.RegistID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistTransType(FixContext context, FixMessage message, FixField.RegistTransType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecValuationPoint(FixContext context, FixMessage message, FixField.ExecValuationPoint field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderPercent(FixContext context, FixMessage message, FixField.OrderPercent field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnershipType(FixContext context, FixMessage message, FixField.OwnershipType field)
	{
		if (field.Value is not ('2' or 'J' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoContAmts(FixContext context, FixMessage message, FixField.NoContAmts field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtType(FixContext context, FixMessage message, FixField.ContAmtType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 2 or 3 or 4 or 5 or 6 or
			7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtValue(FixContext context, FixMessage message, FixField.ContAmtValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtCurr(FixContext context, FixMessage message, FixField.ContAmtCurr field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnerType(FixContext context, FixMessage message, FixField.OwnerType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or
			9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubID(FixContext context, FixMessage message, FixField.PartySubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyID(FixContext context, FixMessage message, FixField.NestedPartyID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyIDSource(FixContext context, FixMessage message, FixField.NestedPartyIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryClOrdID(FixContext context, FixMessage message, FixField.SecondaryClOrdID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryExecID(FixContext context, FixMessage message, FixField.SecondaryExecID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacity(FixContext context, FixMessage message, FixField.OrderCapacity field)
	{
		if (field.Value is not ('A' or 'G' or 'I' or 'P' or 'R' or 'W'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderRestrictions(FixContext context, FixMessage message, FixField.OrderRestrictions field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "A"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMassCancelRequestType(FixContext context, FixMessage message, FixField.MassCancelRequestType field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelResponse(FixContext context, FixMessage message, FixField.MassCancelResponse field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassCancelRejectReason(FixContext context, FixMessage message, FixField.MassCancelRejectReason field)
	{
		if (field.Value is not ("0" or "1" or "2" or "3" or "4" or "5" or "6" or "99"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAffectedOrders(FixContext context, FixMessage message, FixField.TotalAffectedOrders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAffectedOrders(FixContext context, FixMessage message, FixField.NoAffectedOrders field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedOrderID(FixContext context, FixMessage message, FixField.AffectedOrderID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffectedSecondaryOrderID(FixContext context, FixMessage message, FixField.AffectedSecondaryOrderID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteType(FixContext context, FixMessage message, FixField.QuoteType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartyRole(FixContext context, FixMessage message, FixField.NestedPartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartyIDs(FixContext context, FixMessage message, FixField.NoNestedPartyIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalAccruedInterestAmt(FixContext context, FixMessage message, FixField.TotalAccruedInterestAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityDate(FixContext context, FixMessage message, FixField.MaturityDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingMaturityDate(FixContext context, FixMessage message, FixField.UnderlyingMaturityDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrRegistry(FixContext context, FixMessage message, FixField.InstrRegistry field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashMargin(FixContext context, FixMessage message, FixField.CashMargin field)
	{
		if (field.Value is not ('1' or '2' or '3'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubID(FixContext context, FixMessage message, FixField.NestedPartySubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateScope(FixContext context, FixMessage message, FixField.Scope field)
	{
		foreach (var code in field.Value)
			if (code is not ("1" or "2" or "3"))
			{
				Invalid(message, field);

				break;
			}

		return message.IsValid;
	}

	static bool ValidateMDImplicitDelete(FixContext context, FixMessage message, FixField.MDImplicitDelete field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossID(FixContext context, FixMessage message, FixField.CrossID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossType(FixContext context, FixMessage message, FixField.CrossType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPrioritization(FixContext context, FixMessage message, FixField.CrossPrioritization field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigCrossID(FixContext context, FixMessage message, FixField.OrigCrossID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSides(FixContext context, FixMessage message, FixField.NoSides field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUsername(FixContext context, FixMessage message, FixField.Username field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePassword(FixContext context, FixMessage message, FixField.Password field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegs(FixContext context, FixMessage message, FixField.NoLegs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCurrency(FixContext context, FixMessage message, FixField.LegCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoSecurityTypes(FixContext context, FixMessage message, FixField.TotNoSecurityTypes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSecurityTypes(FixContext context, FixMessage message, FixField.NoSecurityTypes field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequestType(FixContext context, FixMessage message, FixField.SecurityListRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestResult(FixContext context, FixMessage message, FixField.SecurityRequestResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundLot(FixContext context, FixMessage message, FixField.RoundLot field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinTradeVol(FixContext context, FixMessage message, FixField.MinTradeVol field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegRptTypeReq(FixContext context, FixMessage message, FixField.MultiLegRptTypeReq field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPositionEffect(FixContext context, FixMessage message, FixField.LegPositionEffect field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCoveredOrUncovered(FixContext context, FixMessage message, FixField.LegCoveredOrUncovered field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPrice(FixContext context, FixMessage message, FixField.LegPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatusRejReason(FixContext context, FixMessage message, FixField.TradSesStatusRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestID(FixContext context, FixMessage message, FixField.TradeRequestID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestType(FixContext context, FixMessage message, FixField.TradeRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreviouslyReported(FixContext context, FixMessage message, FixField.PreviouslyReported field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportID(FixContext context, FixMessage message, FixField.TradeReportID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRefID(FixContext context, FixMessage message, FixField.TradeReportRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchStatus(FixContext context, FixMessage message, FixField.MatchStatus field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMatchType(FixContext context, FixMessage message, FixField.MatchType field)
	{
		if (field.Value is not ("A1" or "A2" or "A3" or "A4" or "A5" or "AQ" or "M1" or "M2" or "M3" or "M4" or
			"M5" or "M6" or "MT" or "S1" or "S2" or "S3" or "S4" or "S5"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOddLot(FixContext context, FixMessage message, FixField.OddLot field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoClearingInstructions(FixContext context, FixMessage message, FixField.NoClearingInstructions field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingInstruction(FixContext context, FixMessage message, FixField.ClearingInstruction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 11 or 12 or 13 or 2 or 3 or 4 or 5 or 6 or 7 or
			8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputSource(FixContext context, FixMessage message, FixField.TradeInputSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeInputDevice(FixContext context, FixMessage message, FixField.TradeInputDevice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoDates(FixContext context, FixMessage message, FixField.NoDates field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccountType(FixContext context, FixMessage message, FixField.AccountType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustOrderCapacity(FixContext context, FixMessage message, FixField.CustOrderCapacity field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClOrdLinkID(FixContext context, FixMessage message, FixField.ClOrdLinkID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqID(FixContext context, FixMessage message, FixField.MassStatusReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqType(FixContext context, FixMessage message, FixField.MassStatusReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigOrdModTime(FixContext context, FixMessage message, FixField.OrigOrdModTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlType(FixContext context, FixMessage message, FixField.LegSettlType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlDate(FixContext context, FixMessage message, FixField.LegSettlDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDayBookingInst(FixContext context, FixMessage message, FixField.DayBookingInst field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingUnit(FixContext context, FixMessage message, FixField.BookingUnit field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePreallocMethod(FixContext context, FixMessage message, FixField.PreallocMethod field)
	{
		if (field.Value is not ('0' or '1'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCountryOfIssue(FixContext context, FixMessage message, FixField.UnderlyingCountryOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStateOrProvinceOfIssue(FixContext context, FixMessage message, FixField.UnderlyingStateOrProvinceOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLocaleOfIssue(FixContext context, FixMessage message, FixField.UnderlyingLocaleOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrRegistry(FixContext context, FixMessage message, FixField.UnderlyingInstrRegistry field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCountryOfIssue(FixContext context, FixMessage message, FixField.LegCountryOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStateOrProvinceOfIssue(FixContext context, FixMessage message, FixField.LegStateOrProvinceOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLocaleOfIssue(FixContext context, FixMessage message, FixField.LegLocaleOfIssue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInstrRegistry(FixContext context, FixMessage message, FixField.LegInstrRegistry field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbol(FixContext context, FixMessage message, FixField.LegSymbol field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSymbolSfx(FixContext context, FixMessage message, FixField.LegSymbolSfx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityID(FixContext context, FixMessage message, FixField.LegSecurityID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityIDSource(FixContext context, FixMessage message, FixField.LegSecurityIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegSecurityAltID(FixContext context, FixMessage message, FixField.NoLegSecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltID(FixContext context, FixMessage message, FixField.LegSecurityAltID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityAltIDSource(FixContext context, FixMessage message, FixField.LegSecurityAltIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegProduct(FixContext context, FixMessage message, FixField.LegProduct field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCFICode(FixContext context, FixMessage message, FixField.LegCFICode field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityType(FixContext context, FixMessage message, FixField.LegSecurityType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityMonthYear(FixContext context, FixMessage message, FixField.LegMaturityMonthYear field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegMaturityDate(FixContext context, FixMessage message, FixField.LegMaturityDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikePrice(FixContext context, FixMessage message, FixField.LegStrikePrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOptAttribute(FixContext context, FixMessage message, FixField.LegOptAttribute field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractMultiplier(FixContext context, FixMessage message, FixField.LegContractMultiplier field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegCouponRate(FixContext context, FixMessage message, FixField.LegCouponRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityExchange(FixContext context, FixMessage message, FixField.LegSecurityExchange field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIssuer(FixContext context, FixMessage message, FixField.LegIssuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuerLen(FixContext context, FixMessage message, FixField.EncodedLegIssuerLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegIssuer(FixContext context, FixMessage message, FixField.EncodedLegIssuer field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecurityDesc(FixContext context, FixMessage message, FixField.LegSecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDescLen(FixContext context, FixMessage message, FixField.EncodedLegSecurityDescLen field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncodedLegSecurityDesc(FixContext context, FixMessage message, FixField.EncodedLegSecurityDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRatioQty(FixContext context, FixMessage message, FixField.LegRatioQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSide(FixContext context, FixMessage message, FixField.LegSide field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradingSessionSubID(FixContext context, FixMessage message, FixField.TradingSessionSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocType(FixContext context, FixMessage message, FixField.AllocType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 5 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoHops(FixContext context, FixMessage message, FixField.NoHops field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopCompID(FixContext context, FixMessage message, FixField.HopCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopSendingTime(FixContext context, FixMessage message, FixField.HopSendingTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHopRefID(FixContext context, FixMessage message, FixField.HopRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidPx(FixContext context, FixMessage message, FixField.MidPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidYield(FixContext context, FixMessage message, FixField.BidYield field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMidYield(FixContext context, FixMessage message, FixField.MidYield field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferYield(FixContext context, FixMessage message, FixField.OfferYield field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingFeeIndicator(FixContext context, FixMessage message, FixField.ClearingFeeIndicator field)
	{
		if (field.Value is not ("1" or "2" or "3" or "4" or "5" or "9" or "B" or "C" or "E" or "F" or "H" or "I" or
			"L" or "M"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateWorkingIndicator(FixContext context, FixMessage message, FixField.WorkingIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegLastPx(FixContext context, FixMessage message, FixField.LegLastPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorityIndicator(FixContext context, FixMessage message, FixField.PriorityIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceImprovement(FixContext context, FixMessage message, FixField.PriceImprovement field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePrice2(FixContext context, FixMessage message, FixField.Price2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastForwardPoints2(FixContext context, FixMessage message, FixField.LastForwardPoints2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidForwardPoints2(FixContext context, FixMessage message, FixField.BidForwardPoints2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOfferForwardPoints2(FixContext context, FixMessage message, FixField.OfferForwardPoints2 field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRFQReqID(FixContext context, FixMessage message, FixField.RFQReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktBidPx(FixContext context, FixMessage message, FixField.MktBidPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMktOfferPx(FixContext context, FixMessage message, FixField.MktOfferPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinBidSize(FixContext context, FixMessage message, FixField.MinBidSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMinOfferSize(FixContext context, FixMessage message, FixField.MinOfferSize field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReqID(FixContext context, FixMessage message, FixField.QuoteStatusReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegalConfirm(FixContext context, FixMessage message, FixField.LegalConfirm field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastPx(FixContext context, FixMessage message, FixField.UnderlyingLastPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingLastQty(FixContext context, FixMessage message, FixField.UnderlyingLastQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegRefID(FixContext context, FixMessage message, FixField.LegRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraLegRefID(FixContext context, FixMessage message, FixField.ContraLegRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrBidFxRate(FixContext context, FixMessage message, FixField.SettlCurrBidFxRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlCurrOfferFxRate(FixContext context, FixMessage message, FixField.SettlCurrOfferFxRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestRejectReason(FixContext context, FixMessage message, FixField.QuoteRequestRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideComplianceID(FixContext context, FixMessage message, FixField.SideComplianceID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAcctIDSource(FixContext context, FixMessage message, FixField.AcctIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAcctIDSource(FixContext context, FixMessage message, FixField.AllocAcctIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPrice(FixContext context, FixMessage message, FixField.BenchmarkPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkPriceType(FixContext context, FixMessage message, FixField.BenchmarkPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmID(FixContext context, FixMessage message, FixField.ConfirmID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmStatus(FixContext context, FixMessage message, FixField.ConfirmStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmTransType(FixContext context, FixMessage message, FixField.ConfirmTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContractSettlMonth(FixContext context, FixMessage message, FixField.ContractSettlMonth field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryForm(FixContext context, FixMessage message, FixField.DeliveryForm field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastParPx(FixContext context, FixMessage message, FixField.LastParPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegAllocs(FixContext context, FixMessage message, FixField.NoLegAllocs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAccount(FixContext context, FixMessage message, FixField.LegAllocAccount field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIndividualAllocID(FixContext context, FixMessage message, FixField.LegIndividualAllocID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocQty(FixContext context, FixMessage message, FixField.LegAllocQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegAllocAcctIDSource(FixContext context, FixMessage message, FixField.LegAllocAcctIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSettlCurrency(FixContext context, FixMessage message, FixField.LegSettlCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveCurrency(FixContext context, FixMessage message, FixField.LegBenchmarkCurveCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveName(FixContext context, FixMessage message, FixField.LegBenchmarkCurveName field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurvePoint(FixContext context, FixMessage message, FixField.LegBenchmarkCurvePoint field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPrice(FixContext context, FixMessage message, FixField.LegBenchmarkPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkPriceType(FixContext context, FixMessage message, FixField.LegBenchmarkPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegBidPx(FixContext context, FixMessage message, FixField.LegBidPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegIOIQty(FixContext context, FixMessage message, FixField.LegIOIQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoLegStipulations(FixContext context, FixMessage message, FixField.NoLegStipulations field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegOfferPx(FixContext context, FixMessage message, FixField.LegOfferPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPriceType(FixContext context, FixMessage message, FixField.LegPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegQty(FixContext context, FixMessage message, FixField.LegQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationType(FixContext context, FixMessage message, FixField.LegStipulationType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStipulationValue(FixContext context, FixMessage message, FixField.LegStipulationValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSwapType(FixContext context, FixMessage message, FixField.LegSwapType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePool(FixContext context, FixMessage message, FixField.Pool field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuotePriceType(FixContext context, FixMessage message, FixField.QuotePriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespID(FixContext context, FixMessage message, FixField.QuoteRespID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespType(FixContext context, FixMessage message, FixField.QuoteRespType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteQualifier(FixContext context, FixMessage message, FixField.QuoteQualifier field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionDate(FixContext context, FixMessage message, FixField.YieldRedemptionDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPrice(FixContext context, FixMessage message, FixField.YieldRedemptionPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldRedemptionPriceType(FixContext context, FixMessage message, FixField.YieldRedemptionPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityID(FixContext context, FixMessage message, FixField.BenchmarkSecurityID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReversalIndicator(FixContext context, FixMessage message, FixField.ReversalIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateYieldCalcDate(FixContext context, FixMessage message, FixField.YieldCalcDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPositions(FixContext context, FixMessage message, FixField.NoPositions field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosType(FixContext context, FixMessage message, FixField.PosType field)
	{
		if (field.Value is not ("ALC" or "AS" or "ASF" or "DLV" or "ETR" or "EX" or "FIN" or "IAS" or "IES" or
			"PA" or "PIT" or "SOD" or "SPL" or "TA" or "TOT" or "TQ" or "TRF" or "TX" or "XM"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLongQty(FixContext context, FixMessage message, FixField.LongQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortQty(FixContext context, FixMessage message, FixField.ShortQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosQtyStatus(FixContext context, FixMessage message, FixField.PosQtyStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmtType(FixContext context, FixMessage message, FixField.PosAmtType field)
	{
		if (field.Value is not ("CASH" or "CRES" or "FMTM" or "IMTM" or "PREM" or "SMTM" or "TVAR" or "VADJ"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmt(FixContext context, FixMessage message, FixField.PosAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosTransType(FixContext context, FixMessage message, FixField.PosTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqID(FixContext context, FixMessage message, FixField.PosReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyings(FixContext context, FixMessage message, FixField.NoUnderlyings field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintAction(FixContext context, FixMessage message, FixField.PosMaintAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrigPosReqRefID(FixContext context, FixMessage message, FixField.OrigPosReqRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptRefID(FixContext context, FixMessage message, FixField.PosMaintRptRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateClearingBusinessDate(FixContext context, FixMessage message, FixField.ClearingBusinessDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessID(FixContext context, FixMessage message, FixField.SettlSessID field)
	{
		if (field.Value is not ("ETH" or "ITD" or "RTH"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessSubID(FixContext context, FixMessage message, FixField.SettlSessSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustmentType(FixContext context, FixMessage message, FixField.AdjustmentType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContraryInstructionIndicator(FixContext context, FixMessage message, FixField.ContraryInstructionIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSpreadIndicator(FixContext context, FixMessage message, FixField.PriorSpreadIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintRptID(FixContext context, FixMessage message, FixField.PosMaintRptID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintStatus(FixContext context, FixMessage message, FixField.PosMaintStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintResult(FixContext context, FixMessage message, FixField.PosMaintResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqType(FixContext context, FixMessage message, FixField.PosReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseTransportType(FixContext context, FixMessage message, FixField.ResponseTransportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseDestination(FixContext context, FixMessage message, FixField.ResponseDestination field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNumPosReports(FixContext context, FixMessage message, FixField.TotalNumPosReports field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqResult(FixContext context, FixMessage message, FixField.PosReqResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqStatus(FixContext context, FixMessage message, FixField.PosReqStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPrice(FixContext context, FixMessage message, FixField.SettlPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPriceType(FixContext context, FixMessage message, FixField.SettlPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPrice(FixContext context, FixMessage message, FixField.UnderlyingSettlPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSettlPriceType(FixContext context, FixMessage message, FixField.UnderlyingSettlPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriorSettlPrice(FixContext context, FixMessage message, FixField.PriorSettlPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoQuoteQualifiers(FixContext context, FixMessage message, FixField.NoQuoteQualifiers field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrency(FixContext context, FixMessage message, FixField.AllocSettlCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlCurrAmt(FixContext context, FixMessage message, FixField.AllocSettlCurrAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAtMaturity(FixContext context, FixMessage message, FixField.InterestAtMaturity field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegDatedDate(FixContext context, FixMessage message, FixField.LegDatedDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegPool(FixContext context, FixMessage message, FixField.LegPool field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocInterestAtMaturity(FixContext context, FixMessage message, FixField.AllocInterestAtMaturity field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccruedInterestAmt(FixContext context, FixMessage message, FixField.AllocAccruedInterestAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryDate(FixContext context, FixMessage message, FixField.DeliveryDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentMethod(FixContext context, FixMessage message, FixField.AssignmentMethod field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentUnit(FixContext context, FixMessage message, FixField.AssignmentUnit field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOpenInterest(FixContext context, FixMessage message, FixField.OpenInterest field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExerciseMethod(FixContext context, FixMessage message, FixField.ExerciseMethod field)
	{
		if (field.Value is not ('A' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumTradeReports(FixContext context, FixMessage message, FixField.TotNumTradeReports field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestResult(FixContext context, FixMessage message, FixField.TradeRequestResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestStatus(FixContext context, FixMessage message, FixField.TradeRequestStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRejectReason(FixContext context, FixMessage message, FixField.TradeReportRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideMultiLegReportingType(FixContext context, FixMessage message, FixField.SideMultiLegReportingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPosAmt(FixContext context, FixMessage message, FixField.NoPosAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAutoAcceptIndicator(FixContext context, FixMessage message, FixField.AutoAcceptIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportID(FixContext context, FixMessage message, FixField.AllocReportID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartyIDs(FixContext context, FixMessage message, FixField.NoNested2PartyIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyID(FixContext context, FixMessage message, FixField.Nested2PartyID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyIDSource(FixContext context, FixMessage message, FixField.Nested2PartyIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartyRole(FixContext context, FixMessage message, FixField.Nested2PartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubID(FixContext context, FixMessage message, FixField.Nested2PartySubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBenchmarkSecurityIDSource(FixContext context, FixMessage message, FixField.BenchmarkSecurityIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecuritySubType(FixContext context, FixMessage message, FixField.SecuritySubType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingSecuritySubType(FixContext context, FixMessage message, FixField.UnderlyingSecuritySubType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSecuritySubType(FixContext context, FixMessage message, FixField.LegSecuritySubType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessPct(FixContext context, FixMessage message, FixField.AllowableOneSidednessPct field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessValue(FixContext context, FixMessage message, FixField.AllowableOneSidednessValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllowableOneSidednessCurr(FixContext context, FixMessage message, FixField.AllowableOneSidednessCurr field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrdRegTimestamps(FixContext context, FixMessage message, FixField.NoTrdRegTimestamps field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestamp(FixContext context, FixMessage message, FixField.TrdRegTimestamp field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampType(FixContext context, FixMessage message, FixField.TrdRegTimestampType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampOrigin(FixContext context, FixMessage message, FixField.TrdRegTimestampOrigin field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRefID(FixContext context, FixMessage message, FixField.ConfirmRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmType(FixContext context, FixMessage message, FixField.ConfirmType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRejReason(FixContext context, FixMessage message, FixField.ConfirmRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingType(FixContext context, FixMessage message, FixField.BookingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIndividualAllocRejCode(FixContext context, FixMessage message, FixField.IndividualAllocRejCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMsgID(FixContext context, FixMessage message, FixField.SettlInstMsgID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlInst(FixContext context, FixMessage message, FixField.NoSettlInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastUpdateTime(FixContext context, FixMessage message, FixField.LastUpdateTime field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlInstType(FixContext context, FixMessage message, FixField.AllocSettlInstType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartyIDs(FixContext context, FixMessage message, FixField.NoSettlPartyIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyID(FixContext context, FixMessage message, FixField.SettlPartyID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyIDSource(FixContext context, FixMessage message, FixField.SettlPartyIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartyRole(FixContext context, FixMessage message, FixField.SettlPartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubID(FixContext context, FixMessage message, FixField.SettlPartySubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPartySubIDType(FixContext context, FixMessage message, FixField.SettlPartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDlvyInstType(FixContext context, FixMessage message, FixField.DlvyInstType field)
	{
		if (field.Value is not ('C' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTerminationType(FixContext context, FixMessage message, FixField.TerminationType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNextExpectedMsgSeqNum(FixContext context, FixMessage message, FixField.NextExpectedMsgSeqNum field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdStatusReqID(FixContext context, FixMessage message, FixField.OrdStatusReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqID(FixContext context, FixMessage message, FixField.SettlInstReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqRejCode(FixContext context, FixMessage message, FixField.SettlInstReqRejCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryAllocID(FixContext context, FixMessage message, FixField.SecondaryAllocID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportType(FixContext context, FixMessage message, FixField.AllocReportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (3 or 4 or 5 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportRefID(FixContext context, FixMessage message, FixField.AllocReportRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocCancReplaceReason(FixContext context, FixMessage message, FixField.AllocCancReplaceReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCopyMsgIndicator(FixContext context, FixMessage message, FixField.CopyMsgIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccountType(FixContext context, FixMessage message, FixField.AllocAccountType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 6 or 7 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderAvgPx(FixContext context, FixMessage message, FixField.OrderAvgPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderBookingQty(FixContext context, FixMessage message, FixField.OrderBookingQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSettlPartySubIDs(FixContext context, FixMessage message, FixField.NoSettlPartySubIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoPartySubIDs(FixContext context, FixMessage message, FixField.NoPartySubIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubIDType(FixContext context, FixMessage message, FixField.PartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNestedPartySubIDs(FixContext context, FixMessage message, FixField.NoNestedPartySubIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNestedPartySubIDType(FixContext context, FixMessage message, FixField.NestedPartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested2PartySubIDs(FixContext context, FixMessage message, FixField.NoNested2PartySubIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested2PartySubIDType(FixContext context, FixMessage message, FixField.Nested2PartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocIntermedReqType(FixContext context, FixMessage message, FixField.AllocIntermedReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingPx(FixContext context, FixMessage message, FixField.UnderlyingPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePriceDelta(FixContext context, FixMessage message, FixField.PriceDelta field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueMax(FixContext context, FixMessage message, FixField.ApplQueueMax field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueDepth(FixContext context, FixMessage message, FixField.ApplQueueDepth field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueResolution(FixContext context, FixMessage message, FixField.ApplQueueResolution field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueAction(FixContext context, FixMessage message, FixField.ApplQueueAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoAltMDSource(FixContext context, FixMessage message, FixField.NoAltMDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAltMDSourceID(FixContext context, FixMessage message, FixField.AltMDSourceID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportID(FixContext context, FixMessage message, FixField.SecondaryTradeReportID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxIndicator(FixContext context, FixMessage message, FixField.AvgPxIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLinkID(FixContext context, FixMessage message, FixField.TradeLinkID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderInputDevice(FixContext context, FixMessage message, FixField.OrderInputDevice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionID(FixContext context, FixMessage message, FixField.UnderlyingTradingSessionID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingTradingSessionSubID(FixContext context, FixMessage message, FixField.UnderlyingTradingSessionSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeLegRefID(FixContext context, FixMessage message, FixField.TradeLegRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExchangeRule(FixContext context, FixMessage message, FixField.ExchangeRule field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeAllocIndicator(FixContext context, FixMessage message, FixField.TradeAllocIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpirationCycle(FixContext context, FixMessage message, FixField.ExpirationCycle field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdType(FixContext context, FixMessage message, FixField.TrdType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 10 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdSubType(FixContext context, FixMessage message, FixField.TrdSubType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTransferReason(FixContext context, FixMessage message, FixField.TransferReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumAssignmentReports(FixContext context, FixMessage message, FixField.TotNumAssignmentReports field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAsgnRptID(FixContext context, FixMessage message, FixField.AsgnRptID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateThresholdAmount(FixContext context, FixMessage message, FixField.ThresholdAmount field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegMoveType(FixContext context, FixMessage message, FixField.PegMoveType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetType(FixContext context, FixMessage message, FixField.PegOffsetType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegLimitType(FixContext context, FixMessage message, FixField.PegLimitType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegRoundDirection(FixContext context, FixMessage message, FixField.PegRoundDirection field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePeggedPrice(FixContext context, FixMessage message, FixField.PeggedPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegScope(FixContext context, FixMessage message, FixField.PegScope field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionMoveType(FixContext context, FixMessage message, FixField.DiscretionMoveType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetType(FixContext context, FixMessage message, FixField.DiscretionOffsetType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionLimitType(FixContext context, FixMessage message, FixField.DiscretionLimitType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionRoundDirection(FixContext context, FixMessage message, FixField.DiscretionRoundDirection field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionPrice(FixContext context, FixMessage message, FixField.DiscretionPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionScope(FixContext context, FixMessage message, FixField.DiscretionScope field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategy(FixContext context, FixMessage message, FixField.TargetStrategy field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyParameters(FixContext context, FixMessage message, FixField.TargetStrategyParameters field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateParticipationRate(FixContext context, FixMessage message, FixField.ParticipationRate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategyPerformance(FixContext context, FixMessage message, FixField.TargetStrategyPerformance field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastLiquidityInd(FixContext context, FixMessage message, FixField.LastLiquidityInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePublishTrdIndicator(FixContext context, FixMessage message, FixField.PublishTrdIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortSaleReason(FixContext context, FixMessage message, FixField.ShortSaleReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQtyType(FixContext context, FixMessage message, FixField.QtyType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTrdType(FixContext context, FixMessage message, FixField.SecondaryTrdType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportType(FixContext context, FixMessage message, FixField.TradeReportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNoOrdersType(FixContext context, FixMessage message, FixField.AllocNoOrdersType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSharedCommission(FixContext context, FixMessage message, FixField.SharedCommission field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmReqID(FixContext context, FixMessage message, FixField.ConfirmReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgParPx(FixContext context, FixMessage message, FixField.AvgParPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateReportedPx(FixContext context, FixMessage message, FixField.ReportedPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCapacities(FixContext context, FixMessage message, FixField.NoCapacities field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrderCapacityQty(FixContext context, FixMessage message, FixField.OrderCapacityQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoEvents(FixContext context, FixMessage message, FixField.NoEvents field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventType(FixContext context, FixMessage message, FixField.EventType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventDate(FixContext context, FixMessage message, FixField.EventDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventPx(FixContext context, FixMessage message, FixField.EventPx field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventText(FixContext context, FixMessage message, FixField.EventText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePctAtRisk(FixContext context, FixMessage message, FixField.PctAtRisk field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoInstrAttrib(FixContext context, FixMessage message, FixField.NoInstrAttrib field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribType(FixContext context, FixMessage message, FixField.InstrAttribType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or
			2 or 20 or 21 or 22 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribValue(FixContext context, FixMessage message, FixField.InstrAttribValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDatedDate(FixContext context, FixMessage message, FixField.DatedDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInterestAccrualDate(FixContext context, FixMessage message, FixField.InterestAccrualDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPProgram(FixContext context, FixMessage message, FixField.CPProgram field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPRegType(FixContext context, FixMessage message, FixField.CPRegType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPProgram(FixContext context, FixMessage message, FixField.UnderlyingCPProgram field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCPRegType(FixContext context, FixMessage message, FixField.UnderlyingCPRegType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingQty(FixContext context, FixMessage message, FixField.UnderlyingQty field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdMatchID(FixContext context, FixMessage message, FixField.TrdMatchID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecondaryTradeReportRefID(FixContext context, FixMessage message, FixField.SecondaryTradeReportRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingDirtyPrice(FixContext context, FixMessage message, FixField.UnderlyingDirtyPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndPrice(FixContext context, FixMessage message, FixField.UnderlyingEndPrice field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStartValue(FixContext context, FixMessage message, FixField.UnderlyingStartValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingCurrentValue(FixContext context, FixMessage message, FixField.UnderlyingCurrentValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingEndValue(FixContext context, FixMessage message, FixField.UnderlyingEndValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoUnderlyingStips(FixContext context, FixMessage message, FixField.NoUnderlyingStips field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipType(FixContext context, FixMessage message, FixField.UnderlyingStipType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipValue(FixContext context, FixMessage message, FixField.UnderlyingStipValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMaturityNetMoney(FixContext context, FixMessage message, FixField.MaturityNetMoney field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeBasis(FixContext context, FixMessage message, FixField.MiscFeeBasis field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNoAllocs(FixContext context, FixMessage message, FixField.TotNoAllocs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastFragment(FixContext context, FixMessage message, FixField.LastFragment field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollReqID(FixContext context, FixMessage message, FixField.CollReqID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnReason(FixContext context, FixMessage message, FixField.CollAsgnReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryQualifier(FixContext context, FixMessage message, FixField.CollInquiryQualifier field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoTrades(FixContext context, FixMessage message, FixField.NoTrades field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginRatio(FixContext context, FixMessage message, FixField.MarginRatio field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMarginExcess(FixContext context, FixMessage message, FixField.MarginExcess field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotalNetValue(FixContext context, FixMessage message, FixField.TotalNetValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashOutstanding(FixContext context, FixMessage message, FixField.CashOutstanding field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnID(FixContext context, FixMessage message, FixField.CollAsgnID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnTransType(FixContext context, FixMessage message, FixField.CollAsgnTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRespID(FixContext context, FixMessage message, FixField.CollRespID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRespType(FixContext context, FixMessage message, FixField.CollAsgnRespType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRejectReason(FixContext context, FixMessage message, FixField.CollAsgnRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRefID(FixContext context, FixMessage message, FixField.CollAsgnRefID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollRptID(FixContext context, FixMessage message, FixField.CollRptID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryID(FixContext context, FixMessage message, FixField.CollInquiryID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollStatus(FixContext context, FixMessage message, FixField.CollStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTotNumReports(FixContext context, FixMessage message, FixField.TotNumReports field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastRptRequested(FixContext context, FixMessage message, FixField.LastRptRequested field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDesc(FixContext context, FixMessage message, FixField.AgreementDesc field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementID(FixContext context, FixMessage message, FixField.AgreementID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementDate(FixContext context, FixMessage message, FixField.AgreementDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartDate(FixContext context, FixMessage message, FixField.StartDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndDate(FixContext context, FixMessage message, FixField.EndDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAgreementCurrency(FixContext context, FixMessage message, FixField.AgreementCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryType(FixContext context, FixMessage message, FixField.DeliveryType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndAccruedInterestAmt(FixContext context, FixMessage message, FixField.EndAccruedInterestAmt field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStartCash(FixContext context, FixMessage message, FixField.StartCash field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEndCash(FixContext context, FixMessage message, FixField.EndCash field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestID(FixContext context, FixMessage message, FixField.UserRequestID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestType(FixContext context, FixMessage message, FixField.UserRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNewPassword(FixContext context, FixMessage message, FixField.NewPassword field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatus(FixContext context, FixMessage message, FixField.UserStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4 or 5 or 6))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatusText(FixContext context, FixMessage message, FixField.UserStatusText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusValue(FixContext context, FixMessage message, FixField.StatusValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusText(FixContext context, FixMessage message, FixField.StatusText field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefCompID(FixContext context, FixMessage message, FixField.RefCompID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRefSubID(FixContext context, FixMessage message, FixField.RefSubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkResponseID(FixContext context, FixMessage message, FixField.NetworkResponseID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestID(FixContext context, FixMessage message, FixField.NetworkRequestID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastNetworkResponseID(FixContext context, FixMessage message, FixField.LastNetworkResponseID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestType(FixContext context, FixMessage message, FixField.NetworkRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 4 or 8))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCompIDs(FixContext context, FixMessage message, FixField.NoCompIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponseType(FixContext context, FixMessage message, FixField.NetworkStatusResponseType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoCollInquiryQualifier(FixContext context, FixMessage message, FixField.NoCollInquiryQualifier field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRptStatus(FixContext context, FixMessage message, FixField.TrdRptStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffirmStatus(FixContext context, FixMessage message, FixField.AffirmStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (1 or 2 or 3))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStrikeCurrency(FixContext context, FixMessage message, FixField.UnderlyingStrikeCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegStrikeCurrency(FixContext context, FixMessage message, FixField.LegStrikeCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeBracket(FixContext context, FixMessage message, FixField.TimeBracket field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAction(FixContext context, FixMessage message, FixField.CollAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryStatus(FixContext context, FixMessage message, FixField.CollInquiryStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryResult(FixContext context, FixMessage message, FixField.CollInquiryResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (field.Value is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 99))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStrikeCurrency(FixContext context, FixMessage message, FixField.StrikeCurrency field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartyIDs(FixContext context, FixMessage message, FixField.NoNested3PartyIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyID(FixContext context, FixMessage message, FixField.Nested3PartyID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyIDSource(FixContext context, FixMessage message, FixField.Nested3PartyIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartyRole(FixContext context, FixMessage message, FixField.Nested3PartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoNested3PartySubIDs(FixContext context, FixMessage message, FixField.NoNested3PartySubIDs field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubID(FixContext context, FixMessage message, FixField.Nested3PartySubID field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNested3PartySubIDType(FixContext context, FixMessage message, FixField.Nested3PartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegContractSettlMonth(FixContext context, FixMessage message, FixField.LegContractSettlMonth field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegInterestAccrualDate(FixContext context, FixMessage message, FixField.LegInterestAccrualDate field)
	{
		if (!field.IsValid)
			Invalid(message, field);

		return message.IsValid;
	}
}
