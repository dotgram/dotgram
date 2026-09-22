using System;
using System.Collections.Generic;
using System.Numerics;

namespace DotGram.Finance.Fix;

/// <summary>The check of every FIX 4.4 message type, one method each.</summary>
/// <remarks>
/// <para>
/// Straight-line checks written against the fields of the class they are about, and nothing
/// else: no tables, no schema read at run time, no question asked of anything but the message.
/// What each one asks is what the published repository requires of that type — a field it marks
/// required, the counter of a repeating component it marks required, and a block it marks
/// required and the message left empty.
/// </para>
/// <para>
/// What a block requires is asked of the block, once, through the interface every carrier of it
/// implements. <c>Instrument</c> is required by twenty-eight of the ninety-three types; the
/// question is written here once and each of the twenty-eight asks it.
/// </para>
/// <para>
/// One instance a context, and a context takes <see cref="Default"/> unless it is given another.
/// The slots are properties rather than calls so that a dictionary loaded at run time can replace
/// the check of one message type and leave the other ninety-three, and each is typed on the class
/// it checks, so a check cannot be put in the wrong slot.
/// </para>
/// <para>
/// What is not here yet: what a group entry requires of itself, which is the same question one
/// level down and wants the same treatment.
/// </para>
/// </remarks>
class FixValidators
{
	/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>
	public static readonly FixValidators Default = new();

	/// <summary>Holds a FIX 4.4 Advertisement to the schema.</summary>
	public Func<FixContext, FixMessage.Advertisement, bool> Advertisement                           { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 4.4 AllocationInstruction to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationInstruction, bool> AllocationInstruction                   { get; set; } = ValidateAllocationInstruction;

	/// <summary>Holds a FIX 4.4 AllocationInstructionAck to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationInstructionAck, bool> AllocationInstructionAck                { get; set; } = ValidateAllocationInstructionAck;

	/// <summary>Holds a FIX 4.4 AllocationReport to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationReport, bool> AllocationReport                        { get; set; } = ValidateAllocationReport;

	/// <summary>Holds a FIX 4.4 AllocationReportAck to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationReportAck, bool> AllocationReportAck                     { get; set; } = ValidateAllocationReportAck;

	/// <summary>Holds a FIX 4.4 AssignmentReport to the schema.</summary>
	public Func<FixContext, FixMessage.AssignmentReport, bool> AssignmentReport                        { get; set; } = ValidateAssignmentReport;

	/// <summary>Holds a FIX 4.4 BidRequest to the schema.</summary>
	public Func<FixContext, FixMessage.BidRequest, bool> BidRequest                              { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 4.4 BidResponse to the schema.</summary>
	public Func<FixContext, FixMessage.BidResponse, bool> BidResponse                             { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 4.4 BusinessMessageReject to the schema.</summary>
	public Func<FixContext, FixMessage.BusinessMessageReject, bool> BusinessMessageReject                   { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 4.4 CollateralAssignment to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralAssignment, bool> CollateralAssignment                    { get; set; } = ValidateCollateralAssignment;

	/// <summary>Holds a FIX 4.4 CollateralInquiry to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralInquiry, bool> CollateralInquiry                       { get; set; } = ValidateCollateralInquiry;

	/// <summary>Holds a FIX 4.4 CollateralInquiryAck to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralInquiryAck, bool> CollateralInquiryAck                    { get; set; } = ValidateCollateralInquiryAck;

	/// <summary>Holds a FIX 4.4 CollateralReport to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralReport, bool> CollateralReport                        { get; set; } = ValidateCollateralReport;

	/// <summary>Holds a FIX 4.4 CollateralRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralRequest, bool> CollateralRequest                       { get; set; } = ValidateCollateralRequest;

	/// <summary>Holds a FIX 4.4 CollateralResponse to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralResponse, bool> CollateralResponse                      { get; set; } = ValidateCollateralResponse;

	/// <summary>Holds a FIX 4.4 Confirmation to the schema.</summary>
	public Func<FixContext, FixMessage.Confirmation, bool> Confirmation                            { get; set; } = ValidateConfirmation;

	/// <summary>Holds a FIX 4.4 ConfirmationAck to the schema.</summary>
	public Func<FixContext, FixMessage.ConfirmationAck, bool> ConfirmationAck                         { get; set; } = ValidateConfirmationAck;

	/// <summary>Holds a FIX 4.4 ConfirmationRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ConfirmationRequest, bool> ConfirmationRequest                     { get; set; } = ValidateConfirmationRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelReplaceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CrossOrderCancelReplaceRequest, bool> CrossOrderCancelReplaceRequest          { get; set; } = ValidateCrossOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CrossOrderCancelRequest, bool> CrossOrderCancelRequest                 { get; set; } = ValidateCrossOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityList to the schema.</summary>
	public Func<FixContext, FixMessage.DerivativeSecurityList, bool> DerivativeSecurityList                  { get; set; } = ValidateDerivativeSecurityList;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityListRequest to the schema.</summary>
	public Func<FixContext, FixMessage.DerivativeSecurityListRequest, bool> DerivativeSecurityListRequest           { get; set; } = ValidateDerivativeSecurityListRequest;

	/// <summary>Holds a FIX 4.4 DontKnowTrade to the schema.</summary>
	public Func<FixContext, FixMessage.DontKnowTrade, bool> DontKnowTrade                           { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 4.4 Email to the schema.</summary>
	public Func<FixContext, FixMessage.Email, bool> Email                                   { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 4.4 ExecutionReport to the schema.</summary>
	public Func<FixContext, FixMessage.ExecutionReport, bool> ExecutionReport                         { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 4.4 Heartbeat to the schema.</summary>
	public Func<FixContext, FixMessage.Heartbeat, bool> Heartbeat                               { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 4.4 IOI to the schema.</summary>
	public Func<FixContext, FixMessage.IOI, bool> IOI                                     { get; set; } = ValidateIOI;

	/// <summary>Holds a FIX 4.4 ListCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ListCancelRequest, bool> ListCancelRequest                       { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 4.4 ListExecute to the schema.</summary>
	public Func<FixContext, FixMessage.ListExecute, bool> ListExecute                             { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 4.4 ListStatus to the schema.</summary>
	public Func<FixContext, FixMessage.ListStatus, bool> ListStatus                              { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 4.4 ListStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ListStatusRequest, bool> ListStatusRequest                       { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 4.4 ListStrikePrice to the schema.</summary>
	public Func<FixContext, FixMessage.ListStrikePrice, bool> ListStrikePrice                         { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 4.4 Logon to the schema.</summary>
	public Func<FixContext, FixMessage.Logon, bool> Logon                                   { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 4.4 Logout to the schema.</summary>
	public Func<FixContext, FixMessage.Logout, bool> Logout                                  { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 4.4 MarketDataIncrementalRefresh to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataIncrementalRefresh, bool> MarketDataIncrementalRefresh            { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 4.4 MarketDataRequest to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataRequest, bool> MarketDataRequest                       { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 4.4 MarketDataRequestReject to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataRequestReject, bool> MarketDataRequestReject                 { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 4.4 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataSnapshotFullRefresh, bool> MarketDataSnapshotFullRefresh           { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 4.4 MassQuote to the schema.</summary>
	public Func<FixContext, FixMessage.MassQuote, bool> MassQuote                               { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 4.4 MassQuoteAcknowledgement to the schema.</summary>
	public Func<FixContext, FixMessage.MassQuoteAcknowledgement, bool> MassQuoteAcknowledgement                { get; set; } = ValidateMassQuoteAcknowledgement;

	/// <summary>Holds a FIX 4.4 MultilegOrderCancelReplace to the schema.</summary>
	public Func<FixContext, FixMessage.MultilegOrderCancelReplace, bool> MultilegOrderCancelReplace              { get; set; } = ValidateMultilegOrderCancelReplace;

	/// <summary>Holds a FIX 4.4 NetworkCounterpartySystemStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.NetworkCounterpartySystemStatusRequest, bool> NetworkCounterpartySystemStatusRequest  { get; set; } = ValidateNetworkCounterpartySystemStatusRequest;

	/// <summary>Holds a FIX 4.4 NetworkCounterpartySystemStatusResponse to the schema.</summary>
	public Func<FixContext, FixMessage.NetworkCounterpartySystemStatusResponse, bool> NetworkCounterpartySystemStatusResponse { get; set; } = ValidateNetworkCounterpartySystemStatusResponse;

	/// <summary>Holds a FIX 4.4 NewOrderCross to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderCross, bool> NewOrderCross                           { get; set; } = ValidateNewOrderCross;

	/// <summary>Holds a FIX 4.4 NewOrderList to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderList, bool> NewOrderList                            { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 4.4 NewOrderMultileg to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderMultileg, bool> NewOrderMultileg                        { get; set; } = ValidateNewOrderMultileg;

	/// <summary>Holds a FIX 4.4 NewOrderSingle to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderSingle, bool> NewOrderSingle                          { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 4.4 News to the schema.</summary>
	public Func<FixContext, FixMessage.News, bool> News                                    { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 4.4 OrderCancelReject to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelReject, bool> OrderCancelReject                       { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 4.4 OrderCancelReplaceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelReplaceRequest, bool> OrderCancelReplaceRequest               { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 OrderCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelRequest, bool> OrderCancelRequest                      { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassCancelReport to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassCancelReport, bool> OrderMassCancelReport                   { get; set; } = ValidateOrderMassCancelReport;

	/// <summary>Holds a FIX 4.4 OrderMassCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassCancelRequest, bool> OrderMassCancelRequest                  { get; set; } = ValidateOrderMassCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassStatusRequest, bool> OrderMassStatusRequest                  { get; set; } = ValidateOrderMassStatusRequest;

	/// <summary>Holds a FIX 4.4 OrderStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderStatusRequest, bool> OrderStatusRequest                      { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceReport to the schema.</summary>
	public Func<FixContext, FixMessage.PositionMaintenanceReport, bool> PositionMaintenanceReport               { get; set; } = ValidatePositionMaintenanceReport;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.PositionMaintenanceRequest, bool> PositionMaintenanceRequest              { get; set; } = ValidatePositionMaintenanceRequest;

	/// <summary>Holds a FIX 4.4 PositionReport to the schema.</summary>
	public Func<FixContext, FixMessage.PositionReport, bool> PositionReport                          { get; set; } = ValidatePositionReport;

	/// <summary>Holds a FIX 4.4 Quote to the schema.</summary>
	public Func<FixContext, FixMessage.Quote, bool> Quote                                   { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 4.4 QuoteCancel to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteCancel, bool> QuoteCancel                             { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 4.4 QuoteRequest to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteRequest, bool> QuoteRequest                            { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 4.4 QuoteRequestReject to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteRequestReject, bool> QuoteRequestReject                      { get; set; } = ValidateQuoteRequestReject;

	/// <summary>Holds a FIX 4.4 QuoteResponse to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteResponse, bool> QuoteResponse                           { get; set; } = ValidateQuoteResponse;

	/// <summary>Holds a FIX 4.4 QuoteStatusReport to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteStatusReport, bool> QuoteStatusReport                       { get; set; } = ValidateQuoteStatusReport;

	/// <summary>Holds a FIX 4.4 QuoteStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteStatusRequest, bool> QuoteStatusRequest                      { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 4.4 RFQRequest to the schema.</summary>
	public Func<FixContext, FixMessage.RFQRequest, bool> RFQRequest                              { get; set; } = ValidateRFQRequest;

	/// <summary>Holds a FIX 4.4 RegistrationInstructions to the schema.</summary>
	public Func<FixContext, FixMessage.RegistrationInstructions, bool> RegistrationInstructions                { get; set; } = ValidateRegistrationInstructions;

	/// <summary>Holds a FIX 4.4 RegistrationInstructionsResponse to the schema.</summary>
	public Func<FixContext, FixMessage.RegistrationInstructionsResponse, bool> RegistrationInstructionsResponse        { get; set; } = ValidateRegistrationInstructionsResponse;

	/// <summary>Holds a FIX 4.4 Reject to the schema.</summary>
	public Func<FixContext, FixMessage.Reject, bool> Reject                                  { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 4.4 RequestForPositions to the schema.</summary>
	public Func<FixContext, FixMessage.RequestForPositions, bool> RequestForPositions                     { get; set; } = ValidateRequestForPositions;

	/// <summary>Holds a FIX 4.4 RequestForPositionsAck to the schema.</summary>
	public Func<FixContext, FixMessage.RequestForPositionsAck, bool> RequestForPositionsAck                  { get; set; } = ValidateRequestForPositionsAck;

	/// <summary>Holds a FIX 4.4 ResendRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ResendRequest, bool> ResendRequest                           { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 4.4 SecurityDefinition to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityDefinition, bool> SecurityDefinition                      { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 4.4 SecurityDefinitionRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityDefinitionRequest, bool> SecurityDefinitionRequest               { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 4.4 SecurityList to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityList, bool> SecurityList                            { get; set; } = ValidateSecurityList;

	/// <summary>Holds a FIX 4.4 SecurityListRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityListRequest, bool> SecurityListRequest                     { get; set; } = ValidateSecurityListRequest;

	/// <summary>Holds a FIX 4.4 SecurityStatus to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityStatus, bool> SecurityStatus                          { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 4.4 SecurityStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityStatusRequest, bool> SecurityStatusRequest                   { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypeRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityTypeRequest, bool> SecurityTypeRequest                     { get; set; } = ValidateSecurityTypeRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypes to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityTypes, bool> SecurityTypes                           { get; set; } = ValidateSecurityTypes;

	/// <summary>Holds a FIX 4.4 SequenceReset to the schema.</summary>
	public Func<FixContext, FixMessage.SequenceReset, bool> SequenceReset                           { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 4.4 SettlementInstructionRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SettlementInstructionRequest, bool> SettlementInstructionRequest            { get; set; } = ValidateSettlementInstructionRequest;

	/// <summary>Holds a FIX 4.4 SettlementInstructions to the schema.</summary>
	public Func<FixContext, FixMessage.SettlementInstructions, bool> SettlementInstructions                  { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 4.4 TestRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TestRequest, bool> TestRequest                             { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReport to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReport, bool> TradeCaptureReport                      { get; set; } = ValidateTradeCaptureReport;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportAck to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportAck, bool> TradeCaptureReportAck                   { get; set; } = ValidateTradeCaptureReportAck;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportRequest, bool> TradeCaptureReportRequest               { get; set; } = ValidateTradeCaptureReportRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequestAck to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportRequestAck, bool> TradeCaptureReportRequestAck            { get; set; } = ValidateTradeCaptureReportRequestAck;

	/// <summary>Holds a FIX 4.4 TradingSessionStatus to the schema.</summary>
	public Func<FixContext, FixMessage.TradingSessionStatus, bool> TradingSessionStatus                    { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 4.4 TradingSessionStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TradingSessionStatusRequest, bool> TradingSessionStatusRequest             { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds a FIX 4.4 UserRequest to the schema.</summary>
	public Func<FixContext, FixMessage.UserRequest, bool> UserRequest                             { get; set; } = ValidateUserRequest;

	/// <summary>Holds a FIX 4.4 UserResponse to the schema.</summary>
	public Func<FixContext, FixMessage.UserResponse, bool> UserResponse                            { get; set; } = ValidateUserResponse;

	/// <summary>Holds a FIX 4.4 XMLnonFIX to the schema.</summary>
	public Func<FixContext, FixMessage.XMLnonFIX, bool> XMLnonFIX                               { get; set; } = ValidateXMLnonFIX;

	/// <summary>Holds a message of a type FIX 4.4 does not describe to the schema.</summary>
	public Func<FixContext, FixMessage.Custom, bool> Custom                                  { get; set; } = ValidateCustom;

	static bool ValidateAdvertisement(FixContext context, FixMessage.Advertisement message)
	{
		if (message.AdvId        is null) Missing(message, 2);
		if (message.AdvSide      is null) Missing(message, 4);
		if (message.AdvTransType is null) Missing(message, 5);
		if (message.Quantity     is null) Missing(message, 53);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction(FixContext context, FixMessage.AllocationInstruction message)
	{
		if (message.AvgPx             is null) Missing(message, 6);
		if (message.Quantity          is null) Missing(message, 53);
		if (message.Side              is null) Missing(message, 54);
		if (message.AllocID           is null) Missing(message, 70);
		if (message.AllocTransType    is null) Missing(message, 71);
		if (message.TradeDate         is null) Missing(message, 75);
		if (message.AllocType         is null) Missing(message, 626);
		if (message.AllocNoOrdersType is null) Missing(message, 857);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoAllocs, message.AllocGrp);
		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecAllocGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoOrders, message.OrdAllocGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateAllocationInstructionAck(FixContext context, FixMessage.AllocationInstructionAck message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.AllocID      is null) Missing(message, 70);
		if (message.AllocStatus  is null) Missing(message, 87);

		Counted(message, message.NoAllocs, message.AllocAckGrp);
		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateAllocationReport(FixContext context, FixMessage.AllocationReport message)
	{
		if (message.AvgPx             is null) Missing(message, 6);
		if (message.Quantity          is null) Missing(message, 53);
		if (message.Side              is null) Missing(message, 54);
		if (message.AllocTransType    is null) Missing(message, 71);
		if (message.TradeDate         is null) Missing(message, 75);
		if (message.AllocStatus       is null) Missing(message, 87);
		if (message.AllocReportID     is null) Missing(message, 755);
		if (message.AllocReportType   is null) Missing(message, 794);
		if (message.AllocNoOrdersType is null) Missing(message, 857);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoAllocs, message.AllocGrp);
		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecAllocGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoOrders, message.OrdAllocGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateAllocationReportAck(FixContext context, FixMessage.AllocationReportAck message)
	{
		if (message.TransactTime  is null) Missing(message, 60);
		if (message.AllocID       is null) Missing(message, 70);
		if (message.AllocStatus   is null) Missing(message, 87);
		if (message.AllocReportID is null) Missing(message, 755);

		Counted(message, message.NoAllocs, message.AllocAckGrp);
		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateAssignmentReport(FixContext context, FixMessage.AssignmentReport message)
	{
		if (message.NoPartyIDs           is null) Missing(message, 453);
		if (message.AccountType          is null) Missing(message, 581);
		if (message.NoPositions          is null) Missing(message, 702);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		if (message.SettlSessID          is null) Missing(message, 716);
		if (message.SettlSessSubID       is null) Missing(message, 717);
		if (message.SettlPrice           is null) Missing(message, 730);
		if (message.SettlPriceType       is null) Missing(message, 731);
		if (message.UnderlyingSettlPrice is null) Missing(message, 732);
		if (message.AssignmentMethod     is null) Missing(message, 744);
		if (message.OpenInterest         is null) Missing(message, 746);
		if (message.ExerciseMethod       is null) Missing(message, 747);
		if (message.NoPosAmt             is null) Missing(message, 753);
		if (message.AsgnRptID            is null) Missing(message, 833);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoPosAmt, message.PositionAmountData);
		Counted(message, message.NoPositions, message.PositionQty);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateBidRequest(FixContext context, FixMessage.BidRequest message)
	{
		if (message.BidRequestTransType is null) Missing(message, 374);
		if (message.ClientBidID         is null) Missing(message, 391);
		if (message.TotNoRelatedSym     is null) Missing(message, 393);
		if (message.BidType             is null) Missing(message, 394);
		if (message.BidTradeType        is null) Missing(message, 418);
		if (message.BasisPxType         is null) Missing(message, 419);

		Counted(message, message.NoBidComponents, message.BidCompReqGrp);
		Counted(message, message.NoBidDescriptors, message.BidDescReqGrp);

		return message.IsValid;
	}

	static bool ValidateBidResponse(FixContext context, FixMessage.BidResponse message)
	{
		if (message.NoBidComponents is null) Missing(message, 420);

		Counted(message, message.NoBidComponents, message.BidCompRspGrp);

		return message.IsValid;
	}

	static bool ValidateBusinessMessageReject(FixContext context, FixMessage.BusinessMessageReject message)
	{
		if (message.RefMsgType           is null) Missing(message, 372);
		if (message.BusinessRejectReason is null) Missing(message, 380);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment(FixContext context, FixMessage.CollateralAssignment message)
	{
		if (message.TransactTime      is null) Missing(message, 60);
		if (message.CollAsgnReason    is null) Missing(message, 895);
		if (message.CollAsgnID        is null) Missing(message, 902);
		if (message.CollAsgnTransType is null) Missing(message, 903);

		Counted(message, message.NoDlvyInst, message.DlvyInstGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtCollGrp);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry(FixContext context, FixMessage.CollateralInquiry message)
	{
		Counted(message, message.NoCollInquiryQualifier, message.CollInqQualGrp);
		Counted(message, message.NoDlvyInst, message.DlvyInstGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck(FixContext context, FixMessage.CollateralInquiryAck message)
	{
		if (message.CollInquiryID     is null) Missing(message, 909);
		if (message.CollInquiryStatus is null) Missing(message, 945);

		Counted(message, message.NoCollInquiryQualifier, message.CollInqQualGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateCollateralReport(FixContext context, FixMessage.CollateralReport message)
	{
		if (message.CollRptID  is null) Missing(message, 908);
		if (message.CollStatus is null) Missing(message, 910);

		Counted(message, message.NoDlvyInst, message.DlvyInstGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest(FixContext context, FixMessage.CollateralRequest message)
	{
		if (message.TransactTime   is null) Missing(message, 60);
		if (message.CollReqID      is null) Missing(message, 894);
		if (message.CollAsgnReason is null) Missing(message, 895);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtCollGrp);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse(FixContext context, FixMessage.CollateralResponse message)
	{
		if (message.TransactTime     is null) Missing(message, 60);
		if (message.CollAsgnReason   is null) Missing(message, 895);
		if (message.CollAsgnID       is null) Missing(message, 902);
		if (message.CollRespID       is null) Missing(message, 904);
		if (message.CollAsgnRespType is null) Missing(message, 905);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoExecs, message.ExecCollGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrades, message.TrdCollGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtCollGrp);

		return message.IsValid;
	}

	static bool ValidateConfirmation(FixContext context, FixMessage.Confirmation message)
	{
		if (message.AvgPx            is null) Missing(message, 6);
		if (message.Side             is null) Missing(message, 54);
		if (message.TransactTime     is null) Missing(message, 60);
		if (message.TradeDate        is null) Missing(message, 75);
		if (message.AllocAccount     is null) Missing(message, 79);
		if (message.AllocQty         is null) Missing(message, 80);
		if (message.NetMoney         is null) Missing(message, 118);
		if (message.GrossTradeAmt    is null) Missing(message, 381);
		if (message.NoLegs           is null) Missing(message, 555);
		if (message.ConfirmID        is null) Missing(message, 664);
		if (message.ConfirmStatus    is null) Missing(message, 665);
		if (message.ConfirmTransType is null) Missing(message, 666);
		if (message.NoUnderlyings    is null) Missing(message, 711);
		if (message.ConfirmType      is null) Missing(message, 773);
		if (message.NoCapacities     is null) Missing(message, 862);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoCapacities, message.CpctyConfGrp);
		Counted(message, message.NoDlvyInst, message.DlvyInstGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoOrders, message.OrdAllocGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateConfirmationAck(FixContext context, FixMessage.ConfirmationAck message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.TradeDate    is null) Missing(message, 75);
		if (message.ConfirmID    is null) Missing(message, 664);
		if (message.AffirmStatus is null) Missing(message, 940);

		return message.IsValid;
	}

	static bool ValidateConfirmationRequest(FixContext context, FixMessage.ConfirmationRequest message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.ConfirmType  is null) Missing(message, 773);
		if (message.ConfirmReqID is null) Missing(message, 859);

		Counted(message, message.NoOrders, message.OrdAllocGrp);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest(FixContext context, FixMessage.CrossOrderCancelReplaceRequest message)
	{
		if (message.OrdType             is null) Missing(message, 40);
		if (message.TransactTime        is null) Missing(message, 60);
		if (message.CrossID             is null) Missing(message, 548);
		if (message.CrossType           is null) Missing(message, 549);
		if (message.CrossPrioritization is null) Missing(message, 550);
		if (message.OrigCrossID         is null) Missing(message, 551);
		if (message.NoSides             is null) Missing(message, 552);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoSides, message.SideCrossOrdModGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelRequest(FixContext context, FixMessage.CrossOrderCancelRequest message)
	{
		if (message.TransactTime        is null) Missing(message, 60);
		if (message.CrossID             is null) Missing(message, 548);
		if (message.CrossType           is null) Missing(message, 549);
		if (message.CrossPrioritization is null) Missing(message, 550);
		if (message.OrigCrossID         is null) Missing(message, 551);
		if (message.NoSides             is null) Missing(message, 552);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoSides, message.SideCrossOrdCxlGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityList(FixContext context, FixMessage.DerivativeSecurityList message)
	{
		if (message.SecurityReqID         is null) Missing(message, 320);
		if (message.SecurityResponseID    is null) Missing(message, 322);
		if (message.SecurityRequestResult is null) Missing(message, 560);

		Counted(message, message.NoRelatedSym, message.RelSymDerivSecGrp);
		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityListRequest(FixContext context, FixMessage.DerivativeSecurityListRequest message)
	{
		if (message.SecurityReqID           is null) Missing(message, 320);
		if (message.SecurityListRequestType is null) Missing(message, 559);

		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateDontKnowTrade(FixContext context, FixMessage.DontKnowTrade message)
	{
		if (message.ExecID   is null) Missing(message, 17);
		if (message.OrderID  is null) Missing(message, 37);
		if (message.Side     is null) Missing(message, 54);
		if (message.DKReason is null) Missing(message, 127);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateEmail(FixContext context, FixMessage.Email message)
	{
		if (message.NoLinesOfText is null) Missing(message, 33);
		if (message.EmailType     is null) Missing(message, 94);
		if (message.Subject       is null) Missing(message, 147);
		if (message.EmailThreadID is null) Missing(message, 164);

		Counted(message, message.NoRelatedSym, message.InstrmtGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoLinesOfText, message.LinesOfTextGrp);
		Counted(message, message.NoRoutingIDs, message.RoutingGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateExecutionReport(FixContext context, FixMessage.ExecutionReport message)
	{
		if (message.AvgPx     is null) Missing(message, 6);
		if (message.CumQty    is null) Missing(message, 14);
		if (message.ExecID    is null) Missing(message, 17);
		if (message.OrderID   is null) Missing(message, 37);
		if (message.OrdStatus is null) Missing(message, 39);
		if (message.Side      is null) Missing(message, 54);
		if (message.ExecType  is null) Missing(message, 150);
		if (message.LeavesQty is null) Missing(message, 151);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoContAmts, message.ContAmtGrp);
		Counted(message, message.NoContraBrokers, message.ContraGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegExecGrp);
		Counted(message, message.NoMiscFees, message.MiscFeesGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateHeartbeat(FixContext context, FixMessage.Heartbeat message)
	{
		// The repository requires nothing of this type beyond the standard header and trailer.

		return message.IsValid;
	}

	static bool ValidateIOI(FixContext context, FixMessage.IOI message)
	{
		if (message.IOIID        is null) Missing(message, 23);
		if (message.IOIQty       is null) Missing(message, 27);
		if (message.IOITransType is null) Missing(message, 28);
		if (message.Side         is null) Missing(message, 54);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoIOIQualifiers, message.IOIQualGrp);
		Counted(message, message.NoLegs, message.InstrmtLegIOIGrp);
		Counted(message, message.NoRoutingIDs, message.RoutingGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateListCancelRequest(FixContext context, FixMessage.ListCancelRequest message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.ListID       is null) Missing(message, 66);

		return message.IsValid;
	}

	static bool ValidateListExecute(FixContext context, FixMessage.ListExecute message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.ListID       is null) Missing(message, 66);

		return message.IsValid;
	}

	static bool ValidateListStatus(FixContext context, FixMessage.ListStatus message)
	{
		if (message.ListID          is null) Missing(message, 66);
		if (message.TotNoOrders     is null) Missing(message, 68);
		if (message.NoOrders        is null) Missing(message, 73);
		if (message.NoRpts          is null) Missing(message, 82);
		if (message.RptSeq          is null) Missing(message, 83);
		if (message.ListStatusType  is null) Missing(message, 429);
		if (message.ListOrderStatus is null) Missing(message, 431);

		Counted(message, message.NoOrders, message.OrdListStatGrp);

		return message.IsValid;
	}

	static bool ValidateListStatusRequest(FixContext context, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(message, 66);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice(FixContext context, FixMessage.ListStrikePrice message)
	{
		if (message.ListID       is null) Missing(message, 66);
		if (message.TotNoStrikes is null) Missing(message, 422);
		if (message.NoStrikes    is null) Missing(message, 428);

		Counted(message, message.NoStrikes, message.InstrmtStrkPxGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtStrkPxGrp);

		return message.IsValid;
	}

	static bool ValidateLogon(FixContext context, FixMessage.Logon message)
	{
		if (message.EncryptMethod is null) Missing(message, 98);
		if (message.HeartBtInt    is null) Missing(message, 108);

		Counted(message, message.NoMsgTypes, message.MsgTypeGrp);

		return message.IsValid;
	}

	static bool ValidateLogout(FixContext context, FixMessage.Logout message)
	{
		// The repository requires nothing of this type beyond the standard header and trailer.

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh(FixContext context, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (message.NoMDEntries is null) Missing(message, 268);

		Counted(message, message.NoMDEntries, message.MDIncGrp);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest(FixContext context, FixMessage.MarketDataRequest message)
	{
		if (message.NoRelatedSym            is null) Missing(message, 146);
		if (message.MDReqID                 is null) Missing(message, 262);
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		if (message.MarketDepth             is null) Missing(message, 264);
		if (message.NoMDEntryTypes          is null) Missing(message, 267);

		Counted(message, message.NoRelatedSym, message.InstrmtMDReqGrp);
		Counted(message, message.NoMDEntryTypes, message.MDReqGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequestReject(FixContext context, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(message, 262);

		Counted(message, message.NoAltMDSource, message.MDRjctGrp);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh(FixContext context, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.NoMDEntries is null) Missing(message, 268);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoMDEntries, message.MDFullGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateMassQuote(FixContext context, FixMessage.MassQuote message)
	{
		if (message.QuoteID     is null) Missing(message, 117);
		if (message.NoQuoteSets is null) Missing(message, 296);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteSets, message.QuotSetGrp);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement(FixContext context, FixMessage.MassQuoteAcknowledgement message)
	{
		if (message.QuoteStatus is null) Missing(message, 297);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteSets, message.QuotSetAckGrp);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplace(FixContext context, FixMessage.MultilegOrderCancelReplace message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrdType      is null) Missing(message, 40);
		if (message.OrigClOrdID  is null) Missing(message, 41);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);
		if (message.NoLegs       is null) Missing(message, 555);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.LegOrdGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoAllocs, message.PreAllocMlegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateNetworkCounterpartySystemStatusRequest(FixContext context, FixMessage.NetworkCounterpartySystemStatusRequest message)
	{
		if (message.NetworkRequestID   is null) Missing(message, 933);
		if (message.NetworkRequestType is null) Missing(message, 935);

		Counted(message, message.NoCompIDs, message.CompIDReqGrp);

		return message.IsValid;
	}

	static bool ValidateNetworkCounterpartySystemStatusResponse(FixContext context, FixMessage.NetworkCounterpartySystemStatusResponse message)
	{
		if (message.NetworkResponseID         is null) Missing(message, 932);
		if (message.NoCompIDs                 is null) Missing(message, 936);
		if (message.NetworkStatusResponseType is null) Missing(message, 937);

		Counted(message, message.NoCompIDs, message.CompIDStatGrp);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross(FixContext context, FixMessage.NewOrderCross message)
	{
		if (message.OrdType             is null) Missing(message, 40);
		if (message.TransactTime        is null) Missing(message, 60);
		if (message.CrossID             is null) Missing(message, 548);
		if (message.CrossType           is null) Missing(message, 549);
		if (message.CrossPrioritization is null) Missing(message, 550);
		if (message.NoSides             is null) Missing(message, 552);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoSides, message.SideCrossOrdModGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateNewOrderList(FixContext context, FixMessage.NewOrderList message)
	{
		if (message.ListID      is null) Missing(message, 66);
		if (message.TotNoOrders is null) Missing(message, 68);
		if (message.NoOrders    is null) Missing(message, 73);
		if (message.BidType     is null) Missing(message, 394);

		Counted(message, message.NoOrders, message.ListOrdGrp);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg(FixContext context, FixMessage.NewOrderMultileg message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrdType      is null) Missing(message, 40);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);
		if (message.NoLegs       is null) Missing(message, 555);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.LegOrdGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoAllocs, message.PreAllocMlegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle(FixContext context, FixMessage.NewOrderSingle message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrdType      is null) Missing(message, 40);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoAllocs, message.PreAllocGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateNews(FixContext context, FixMessage.News message)
	{
		if (message.NoLinesOfText is null) Missing(message, 33);
		if (message.Headline      is null) Missing(message, 148);

		Counted(message, message.NoRelatedSym, message.InstrmtGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoLinesOfText, message.LinesOfTextGrp);
		Counted(message, message.NoRoutingIDs, message.RoutingGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReject(FixContext context, FixMessage.OrderCancelReject message)
	{
		if (message.ClOrdID          is null) Missing(message, 11);
		if (message.OrderID          is null) Missing(message, 37);
		if (message.OrdStatus        is null) Missing(message, 39);
		if (message.OrigClOrdID      is null) Missing(message, 41);
		if (message.CxlRejResponseTo is null) Missing(message, 434);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest(FixContext context, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrdType      is null) Missing(message, 40);
		if (message.OrigClOrdID  is null) Missing(message, 41);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoAllocs, message.PreAllocGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateOrderCancelRequest(FixContext context, FixMessage.OrderCancelRequest message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrigClOrdID  is null) Missing(message, 41);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);

		if (Empty((IInstrument)message)) Absent(message, 55);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateOrderMassCancelReport(FixContext context, FixMessage.OrderMassCancelReport message)
	{
		if (message.OrderID               is null) Missing(message, 37);
		if (message.MassCancelRequestType is null) Missing(message, 530);
		if (message.MassCancelResponse    is null) Missing(message, 531);

		Counted(message, message.NoAffectedOrders, message.AffectedOrdGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateOrderMassCancelRequest(FixContext context, FixMessage.OrderMassCancelRequest message)
	{
		if (message.ClOrdID               is null) Missing(message, 11);
		if (message.TransactTime          is null) Missing(message, 60);
		if (message.MassCancelRequestType is null) Missing(message, 530);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateOrderMassStatusRequest(FixContext context, FixMessage.OrderMassStatusRequest message)
	{
		if (message.MassStatusReqID   is null) Missing(message, 584);
		if (message.MassStatusReqType is null) Missing(message, 585);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateOrderStatusRequest(FixContext context, FixMessage.OrderStatusRequest message)
	{
		if (message.ClOrdID is null) Missing(message, 11);
		if (message.Side    is null) Missing(message, 54);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceReport(FixContext context, FixMessage.PositionMaintenanceReport message)
	{
		if (message.Account              is null) Missing(message, 1);
		if (message.TransactTime         is null) Missing(message, 60);
		if (message.AccountType          is null) Missing(message, 581);
		if (message.NoPositions          is null) Missing(message, 702);
		if (message.PosTransType         is null) Missing(message, 709);
		if (message.PosMaintAction       is null) Missing(message, 712);
		if (message.OrigPosReqRefID      is null) Missing(message, 713);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		if (message.PosMaintRptID        is null) Missing(message, 721);
		if (message.PosMaintStatus       is null) Missing(message, 722);
		if (message.NoPosAmt             is null) Missing(message, 753);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoPosAmt, message.PositionAmountData);
		Counted(message, message.NoPositions, message.PositionQty);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceRequest(FixContext context, FixMessage.PositionMaintenanceRequest message)
	{
		if (message.Account              is null) Missing(message, 1);
		if (message.TransactTime         is null) Missing(message, 60);
		if (message.NoPartyIDs           is null) Missing(message, 453);
		if (message.AccountType          is null) Missing(message, 581);
		if (message.NoPositions          is null) Missing(message, 702);
		if (message.PosTransType         is null) Missing(message, 709);
		if (message.PosReqID             is null) Missing(message, 710);
		if (message.PosMaintAction       is null) Missing(message, 712);
		if (message.ClearingBusinessDate is null) Missing(message, 715);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoPositions, message.PositionQty);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidatePositionReport(FixContext context, FixMessage.PositionReport message)
	{
		if (message.Account              is null) Missing(message, 1);
		if (message.NoPartyIDs           is null) Missing(message, 453);
		if (message.AccountType          is null) Missing(message, 581);
		if (message.NoPositions          is null) Missing(message, 702);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		if (message.PosMaintRptID        is null) Missing(message, 721);
		if (message.PosReqResult         is null) Missing(message, 728);
		if (message.SettlPrice           is null) Missing(message, 730);
		if (message.SettlPriceType       is null) Missing(message, 731);
		if (message.PriorSettlPrice      is null) Missing(message, 734);
		if (message.NoPosAmt             is null) Missing(message, 753);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoUnderlyings, message.PosUndInstrmtGrp);
		Counted(message, message.NoPosAmt, message.PositionAmountData);
		Counted(message, message.NoPositions, message.PositionQty);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);

		return message.IsValid;
	}

	static bool ValidateQuote(FixContext context, FixMessage.Quote message)
	{
		if (message.QuoteID is null) Missing(message, 117);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.LegQuotGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteQualifiers, message.QuotQualGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel(FixContext context, FixMessage.QuoteCancel message)
	{
		if (message.QuoteID         is null) Missing(message, 117);
		if (message.QuoteCancelType is null) Missing(message, 298);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteEntries, message.QuotCxlEntriesGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest(FixContext context, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID   is null) Missing(message, 131);
		if (message.NoRelatedSym is null) Missing(message, 146);

		Counted(message, message.NoRelatedSym, message.QuotReqGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject(FixContext context, FixMessage.QuoteRequestReject message)
	{
		if (message.QuoteReqID               is null) Missing(message, 131);
		if (message.NoRelatedSym             is null) Missing(message, 146);
		if (message.QuoteRequestRejectReason is null) Missing(message, 658);

		Counted(message, message.NoRelatedSym, message.QuotReqRjctGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse(FixContext context, FixMessage.QuoteResponse message)
	{
		if (message.QuoteRespID   is null) Missing(message, 693);
		if (message.QuoteRespType is null) Missing(message, 694);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.LegQuotGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteQualifiers, message.QuotQualGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReport(FixContext context, FixMessage.QuoteStatusReport message)
	{
		if (message.QuoteID is null) Missing(message, 117);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.LegQuotStatGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteQualifiers, message.QuotQualGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoStipulations, message.Stipulations);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusRequest(FixContext context, FixMessage.QuoteStatusRequest message)
	{
		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateRFQRequest(FixContext context, FixMessage.RFQRequest message)
	{
		if (message.NoRelatedSym is null) Missing(message, 146);
		if (message.RFQReqID     is null) Missing(message, 644);

		Counted(message, message.NoRelatedSym, message.RFQReqGrp);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructions(FixContext context, FixMessage.RegistrationInstructions message)
	{
		if (message.RegistRefID     is null) Missing(message, 508);
		if (message.RegistID        is null) Missing(message, 513);
		if (message.RegistTransType is null) Missing(message, 514);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoDistribInsts, message.RgstDistInstGrp);
		Counted(message, message.NoRegistDtls, message.RgstDtlsGrp);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructionsResponse(FixContext context, FixMessage.RegistrationInstructionsResponse message)
	{
		if (message.RegistStatus    is null) Missing(message, 506);
		if (message.RegistRefID     is null) Missing(message, 508);
		if (message.RegistID        is null) Missing(message, 513);
		if (message.RegistTransType is null) Missing(message, 514);

		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateReject(FixContext context, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(message, 45);

		return message.IsValid;
	}

	static bool ValidateRequestForPositions(FixContext context, FixMessage.RequestForPositions message)
	{
		if (message.Account              is null) Missing(message, 1);
		if (message.TransactTime         is null) Missing(message, 60);
		if (message.NoPartyIDs           is null) Missing(message, 453);
		if (message.AccountType          is null) Missing(message, 581);
		if (message.PosReqID             is null) Missing(message, 710);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		if (message.PosReqType           is null) Missing(message, 724);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateRequestForPositionsAck(FixContext context, FixMessage.RequestForPositionsAck message)
	{
		if (message.Account       is null) Missing(message, 1);
		if (message.NoPartyIDs    is null) Missing(message, 453);
		if (message.AccountType   is null) Missing(message, 581);
		if (message.PosMaintRptID is null) Missing(message, 721);
		if (message.PosReqResult  is null) Missing(message, 728);
		if (message.PosReqStatus  is null) Missing(message, 729);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateResendRequest(FixContext context, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(message, 7);
		if (message.EndSeqNo   is null) Missing(message, 16);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition(FixContext context, FixMessage.SecurityDefinition message)
	{
		if (message.SecurityReqID        is null) Missing(message, 320);
		if (message.SecurityResponseID   is null) Missing(message, 322);
		if (message.SecurityResponseType is null) Missing(message, 323);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest(FixContext context, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID       is null) Missing(message, 320);
		if (message.SecurityRequestType is null) Missing(message, 321);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityList(FixContext context, FixMessage.SecurityList message)
	{
		if (message.SecurityReqID         is null) Missing(message, 320);
		if (message.SecurityResponseID    is null) Missing(message, 322);
		if (message.SecurityRequestResult is null) Missing(message, 560);

		Counted(message, message.NoRelatedSym, message.SecListGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequest(FixContext context, FixMessage.SecurityListRequest message)
	{
		if (message.SecurityReqID           is null) Missing(message, 320);
		if (message.SecurityListRequestType is null) Missing(message, 559);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus(FixContext context, FixMessage.SecurityStatus message)
	{
		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusRequest(FixContext context, FixMessage.SecurityStatusRequest message)
	{
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		if (message.SecurityStatusReqID     is null) Missing(message, 324);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityTypeRequest(FixContext context, FixMessage.SecurityTypeRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);

		return message.IsValid;
	}

	static bool ValidateSecurityTypes(FixContext context, FixMessage.SecurityTypes message)
	{
		if (message.SecurityReqID        is null) Missing(message, 320);
		if (message.SecurityResponseID   is null) Missing(message, 322);
		if (message.SecurityResponseType is null) Missing(message, 323);

		Counted(message, message.NoSecurityTypes, message.SecTypesGrp);

		return message.IsValid;
	}

	static bool ValidateSequenceReset(FixContext context, FixMessage.SequenceReset message)
	{
		if (message.NewSeqNo is null) Missing(message, 36);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructionRequest(FixContext context, FixMessage.SettlementInstructionRequest message)
	{
		if (message.TransactTime   is null) Missing(message, 60);
		if (message.SettlInstReqID is null) Missing(message, 791);

		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructions(FixContext context, FixMessage.SettlementInstructions message)
	{
		if (message.TransactTime   is null) Missing(message, 60);
		if (message.SettlInstMode  is null) Missing(message, 160);
		if (message.SettlInstMsgID is null) Missing(message, 777);

		Counted(message, message.NoSettlInst, message.SettlInstGrp);

		return message.IsValid;
	}

	static bool ValidateTestRequest(FixContext context, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(message, 112);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport(FixContext context, FixMessage.TradeCaptureReport message)
	{
		if (message.LastPx             is null) Missing(message, 31);
		if (message.LastQty            is null) Missing(message, 32);
		if (message.TransactTime       is null) Missing(message, 60);
		if (message.TradeDate          is null) Missing(message, 75);
		if (message.NoSides            is null) Missing(message, 552);
		if (message.PreviouslyReported is null) Missing(message, 570);
		if (message.TradeReportID      is null) Missing(message, 571);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoPosAmt, message.PositionAmountData);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoSides, message.TrdCapRptSideGrp);
		Counted(message, message.NoLegs, message.TrdInstrmtLegGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportAck(FixContext context, FixMessage.TradeCaptureReportAck message)
	{
		if (message.ExecType      is null) Missing(message, 150);
		if (message.TradeReportID is null) Missing(message, 571);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoAllocs, message.TrdAllocGrp);
		Counted(message, message.NoLegs, message.TrdInstrmtLegGrp);
		Counted(message, message.NoTrdRegTimestamps, message.TrdRegTimestamps);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequest(FixContext context, FixMessage.TradeCaptureReportRequest message)
	{
		if (message.TradeRequestID   is null) Missing(message, 568);
		if (message.TradeRequestType is null) Missing(message, 569);

		Counted(message, message.NoInstrAttrib, message.AttrbGrp);
		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoDates, message.TrdCapDtGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequestAck(FixContext context, FixMessage.TradeCaptureReportRequestAck message)
	{
		if (message.TradeRequestID     is null) Missing(message, 568);
		if (message.TradeRequestType   is null) Missing(message, 569);
		if (message.TradeRequestResult is null) Missing(message, 749);
		if (message.TradeRequestStatus is null) Missing(message, 750);

		if (Empty((IInstrument)message)) Absent(message, 55);

		Counted(message, message.NoEvents, message.EvntGrp);
		Counted(message, message.NoLegs, message.InstrmtLegGrp);
		Counted(message, message.NoSecurityAltID, message.SecAltIDGrp);
		Counted(message, message.NoUnderlyings, message.UndInstrmtGrp);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatus(FixContext context, FixMessage.TradingSessionStatus message)
	{
		if (message.TradingSessionID is null) Missing(message, 336);
		if (message.TradSesStatus    is null) Missing(message, 340);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatusRequest(FixContext context, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		if (message.TradSesReqID            is null) Missing(message, 335);

		return message.IsValid;
	}

	static bool ValidateUserRequest(FixContext context, FixMessage.UserRequest message)
	{
		if (message.Username        is null) Missing(message, 553);
		if (message.UserRequestID   is null) Missing(message, 923);
		if (message.UserRequestType is null) Missing(message, 924);

		return message.IsValid;
	}

	static bool ValidateUserResponse(FixContext context, FixMessage.UserResponse message)
	{
		if (message.Username      is null) Missing(message, 553);
		if (message.UserRequestID is null) Missing(message, 923);

		return message.IsValid;
	}

	static bool ValidateXMLnonFIX(FixContext context, FixMessage.XMLnonFIX message)
	{
		// The repository requires nothing of this type beyond the standard header and trailer.

		return message.IsValid;
	}

	static bool ValidateCustom(FixContext context, FixMessage.Custom message)
	{
		// The type is not one the schema describes, which is the whole of what can be said about it.
		message.AddFinding(new FixFinding(FixRule.UnknownMessageType, 35, message.MsgType?.Position ?? 0, message.MsgType, -1));

		return message.IsValid;
	}

	/// <summary>Whether a carrier of the FIX 4.4 CommissionData block has none of its fields.</summary>
	static bool Empty(ICommissionData block)
	{
		return block.Commission is null &&
			block.CommType is null &&
			block.CommCurrency is null &&
			block.FundRenewWaiv is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 DiscretionInstructions block has none of its fields.</summary>
	static bool Empty(IDiscretionInstructions block)
	{
		return block.DiscretionInst is null &&
			block.DiscretionOffsetValue is null &&
			block.DiscretionMoveType is null &&
			block.DiscretionOffsetType is null &&
			block.DiscretionLimitType is null &&
			block.DiscretionRoundDirection is null &&
			block.DiscretionScope is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 FinancingDetails block has none of its fields.</summary>
	static bool Empty(IFinancingDetails block)
	{
		return block.AgreementDesc is null &&
			block.AgreementID is null &&
			block.AgreementDate is null &&
			block.AgreementCurrency is null &&
			block.TerminationType is null &&
			block.StartDate is null &&
			block.EndDate is null &&
			block.DeliveryType is null &&
			block.MarginRatio is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 Instrument block has none of its fields.</summary>
	static bool Empty(IInstrument block)
	{
		return block.Symbol is null &&
			block.SymbolSfx is null &&
			block.SecurityID is null &&
			block.SecurityIDSource is null &&
			block.NoSecurityAltID is null &&
			block.SecAltIDGrp is null &&
			block.Product is null &&
			block.CFICode is null &&
			block.SecurityType is null &&
			block.SecuritySubType is null &&
			block.MaturityMonthYear is null &&
			block.MaturityDate is null &&
			block.PutOrCall is null &&
			block.CouponPaymentDate is null &&
			block.IssueDate is null &&
			block.RepoCollateralSecurityType is null &&
			block.RepurchaseTerm is null &&
			block.RepurchaseRate is null &&
			block.Factor is null &&
			block.CreditRating is null &&
			block.InstrRegistry is null &&
			block.CountryOfIssue is null &&
			block.StateOrProvinceOfIssue is null &&
			block.LocaleOfIssue is null &&
			block.RedemptionDate is null &&
			block.StrikePrice is null &&
			block.StrikeCurrency is null &&
			block.OptAttribute is null &&
			block.ContractMultiplier is null &&
			block.CouponRate is null &&
			block.SecurityExchange is null &&
			block.Issuer is null &&
			block.EncodedIssuerLen is null &&
			block.EncodedIssuer is null &&
			block.SecurityDesc is null &&
			block.EncodedSecurityDescLen is null &&
			block.EncodedSecurityDesc is null &&
			block.Pool is null &&
			block.ContractSettlMonth is null &&
			block.CPProgram is null &&
			block.CPRegType is null &&
			block.NoEvents is null &&
			block.EvntGrp is null &&
			block.DatedDate is null &&
			block.InterestAccrualDate is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 InstrumentExtension block has none of its fields.</summary>
	static bool Empty(IInstrumentExtension block)
	{
		return block.DeliveryForm is null &&
			block.PctAtRisk is null &&
			block.NoInstrAttrib is null &&
			block.AttrbGrp is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 InstrumentLeg block has none of its fields.</summary>
	static bool Empty(IInstrumentLeg block)
	{
		return block.LegSymbol is null &&
			block.LegSymbolSfx is null &&
			block.LegSecurityID is null &&
			block.LegSecurityIDSource is null &&
			block.NoLegSecurityAltID is null &&
			block.LegSecAltIDGrp is null &&
			block.LegProduct is null &&
			block.LegCFICode is null &&
			block.LegSecurityType is null &&
			block.LegSecuritySubType is null &&
			block.LegMaturityMonthYear is null &&
			block.LegMaturityDate is null &&
			block.LegCouponPaymentDate is null &&
			block.LegIssueDate is null &&
			block.LegRepoCollateralSecurityType is null &&
			block.LegRepurchaseTerm is null &&
			block.LegRepurchaseRate is null &&
			block.LegFactor is null &&
			block.LegCreditRating is null &&
			block.LegInstrRegistry is null &&
			block.LegCountryOfIssue is null &&
			block.LegStateOrProvinceOfIssue is null &&
			block.LegLocaleOfIssue is null &&
			block.LegRedemptionDate is null &&
			block.LegStrikePrice is null &&
			block.LegStrikeCurrency is null &&
			block.LegOptAttribute is null &&
			block.LegContractMultiplier is null &&
			block.LegCouponRate is null &&
			block.LegSecurityExchange is null &&
			block.LegIssuer is null &&
			block.EncodedLegIssuerLen is null &&
			block.EncodedLegIssuer is null &&
			block.LegSecurityDesc is null &&
			block.EncodedLegSecurityDescLen is null &&
			block.EncodedLegSecurityDesc is null &&
			block.LegRatioQty is null &&
			block.LegSide is null &&
			block.LegCurrency is null &&
			block.LegPool is null &&
			block.LegDatedDate is null &&
			block.LegContractSettlMonth is null &&
			block.LegInterestAccrualDate is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 LegBenchmarkCurveData block has none of its fields.</summary>
	static bool Empty(ILegBenchmarkCurveData block)
	{
		return block.LegBenchmarkCurveCurrency is null &&
			block.LegBenchmarkCurveName is null &&
			block.LegBenchmarkCurvePoint is null &&
			block.LegBenchmarkPrice is null &&
			block.LegBenchmarkPriceType is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 OrderQtyData block has none of its fields.</summary>
	static bool Empty(IOrderQtyData block)
	{
		return block.OrderQty is null &&
			block.CashOrderQty is null &&
			block.OrderPercent is null &&
			block.RoundingDirection is null &&
			block.RoundingModulus is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 PegInstructions block has none of its fields.</summary>
	static bool Empty(IPegInstructions block)
	{
		return block.PegOffsetValue is null &&
			block.PegMoveType is null &&
			block.PegOffsetType is null &&
			block.PegLimitType is null &&
			block.PegRoundDirection is null &&
			block.PegScope is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 SettlInstructionsData block has none of its fields.</summary>
	static bool Empty(ISettlInstructionsData block)
	{
		return block.SettlDeliveryType is null &&
			block.StandInstDbType is null &&
			block.StandInstDbName is null &&
			block.StandInstDbID is null &&
			block.NoDlvyInst is null &&
			block.DlvyInstGrp is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 SpreadOrBenchmarkCurveData block has none of its fields.</summary>
	static bool Empty(ISpreadOrBenchmarkCurveData block)
	{
		return block.Spread is null &&
			block.BenchmarkCurveCurrency is null &&
			block.BenchmarkCurveName is null &&
			block.BenchmarkCurvePoint is null &&
			block.BenchmarkPrice is null &&
			block.BenchmarkPriceType is null &&
			block.BenchmarkSecurityID is null &&
			block.BenchmarkSecurityIDSource is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 UnderlyingInstrument block has none of its fields.</summary>
	static bool Empty(IUnderlyingInstrument block)
	{
		return block.UnderlyingSymbol is null &&
			block.UnderlyingSymbolSfx is null &&
			block.UnderlyingSecurityID is null &&
			block.UnderlyingSecurityIDSource is null &&
			block.NoUnderlyingSecurityAltID is null &&
			block.UndSecAltIDGrp is null &&
			block.UnderlyingProduct is null &&
			block.UnderlyingCFICode is null &&
			block.UnderlyingSecurityType is null &&
			block.UnderlyingSecuritySubType is null &&
			block.UnderlyingMaturityMonthYear is null &&
			block.UnderlyingMaturityDate is null &&
			block.UnderlyingPutOrCall is null &&
			block.UnderlyingCouponPaymentDate is null &&
			block.UnderlyingIssueDate is null &&
			block.UnderlyingRepoCollateralSecurityType is null &&
			block.UnderlyingRepurchaseTerm is null &&
			block.UnderlyingRepurchaseRate is null &&
			block.UnderlyingFactor is null &&
			block.UnderlyingCreditRating is null &&
			block.UnderlyingInstrRegistry is null &&
			block.UnderlyingCountryOfIssue is null &&
			block.UnderlyingStateOrProvinceOfIssue is null &&
			block.UnderlyingLocaleOfIssue is null &&
			block.UnderlyingRedemptionDate is null &&
			block.UnderlyingStrikePrice is null &&
			block.UnderlyingStrikeCurrency is null &&
			block.UnderlyingOptAttribute is null &&
			block.UnderlyingContractMultiplier is null &&
			block.UnderlyingCouponRate is null &&
			block.UnderlyingSecurityExchange is null &&
			block.UnderlyingIssuer is null &&
			block.EncodedUnderlyingIssuerLen is null &&
			block.EncodedUnderlyingIssuer is null &&
			block.UnderlyingSecurityDesc is null &&
			block.EncodedUnderlyingSecurityDescLen is null &&
			block.EncodedUnderlyingSecurityDesc is null &&
			block.UnderlyingCPProgram is null &&
			block.UnderlyingCPRegType is null &&
			block.UnderlyingCurrency is null &&
			block.UnderlyingQty is null &&
			block.UnderlyingPx is null &&
			block.UnderlyingDirtyPrice is null &&
			block.UnderlyingEndPrice is null &&
			block.UnderlyingStartValue is null &&
			block.UnderlyingCurrentValue is null &&
			block.UnderlyingEndValue is null &&
			block.NoUnderlyingStips is null &&
			block.UnderlyingStipulations is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 YieldData block has none of its fields.</summary>
	static bool Empty(IYieldData block)
	{
		return block.YieldType is null &&
			block.Yield is null &&
			block.YieldCalcDate is null &&
			block.YieldRedemptionDate is null &&
			block.YieldRedemptionPrice is null &&
			block.YieldRedemptionPriceType is null;
	}

	/// <summary>A field the schema requires of this type, and the message does not have.</summary>
	static void Missing(FixMessage message, int tag)
	{
		message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, tag, 0, null, -1));
	}

	/// <summary>A block the schema requires of this type, and the message has no field of.</summary>
	/// <remarks>Named by the first tag it would have held, since a block has no tag of its own.</remarks>
	static void Absent(FixMessage message, int tag)
	{
		message.AddFinding(new FixFinding(FixRule.RequiredComponentMissing, tag, 0, null, -1));
	}

	/// <summary>A counter and the entries that follow it, which have to be the same number.</summary>
	/// <remarks>
	/// A counter absent while entries are present is a finding of the reading, made where the first
	/// entry was built, so there is nothing left to say about it here.
	/// </remarks>
	static void Counted<T>(FixMessage message, FixField.Typed<BigInteger>? counter, List<T>? entries)
	{
		if (counter is null)
			return;

		if (!counter.IsValid || counter.Value != (entries?.Count ?? 0))
			message.AddFinding(new FixFinding(FixRule.GroupCountMismatch, counter.Tag, counter.Position, counter, -1));
	}
}
